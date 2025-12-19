using DeepSightEvent;
using DeepSightDisplay;
using DeepSightModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeepSightDB;
using DeepSightTool;

namespace DeepSightWorkLib.Services
{
    public class DefectProcessor
    {
        private readonly DefectClass _defect;
        private readonly ImageDisplayService _display;
        private readonly ImageLoaderService _imageLoader;
        private readonly ConcurrentQueue<VBModel> _aviQueue;
        private readonly ConcurrentQueue<Tuple<string, string, string, RootAIResult, DsCenterInfo>> _aiResultQueue;
        private readonly ConcurrentQueue<InferenceResultModel> _inferencePostProcessQueue;

        public DefectProcessor(DefectClass defect,
            ImageDisplayService display,
            ImageLoaderService imageLoader,
            ConcurrentQueue<VBModel> aviQueue,
            ConcurrentQueue<Tuple<string, string, string, RootAIResult, DsCenterInfo>> aiResultQueue,
            ConcurrentQueue<InferenceResultModel> inferencePostProcessQueue)
        {
            _defect = defect ?? throw new ArgumentNullException(nameof(defect));
            _display = display;
            _imageLoader = imageLoader;
            _aviQueue = aviQueue;
            _aiResultQueue = aiResultQueue;
            _inferencePostProcessQueue = inferencePostProcessQueue;
        }

        /// <summary>
        /// 调用 DefectClass.DefectMethodWithImages 并将返回结果解析为与原 BusinessClass.DefectMethod 相同的输出
        /// </summary>
        public bool DefectMethod(VBModel vBModel,int maxCount, out List<string> resList, out List<string> detailsList, out PcsResult pcsResult, out string vbJson)
        {
            resList = new List<string>();
            pcsResult = new PcsResult();
            detailsList = new List<string>();
            vbJson = string.Empty;

            if (vBModel.Mats == null || vBModel.Mats.Count == 0)
            {
                EnqueuePostProcess(vBModel, "", false);
                SystemEvent.SendTaskMsg(vBModel.SN, $"{vBModel.Side}面缺陷数为0，跳过AI检测");
                return true;
            }

            if (vBModel.Mats.Count > maxCount )
            {
                // To keep minimal change, do not enforce max here. Caller can handle.
                EnqueuePostProcess(vBModel, "", false);
                SystemEvent.SendTaskMsg(vBModel.SN, $"{vBModel.Side}面图片数量超过限制，跳过AI检测");
                return true;
            }

            RootVBInfo info = vBModel.VbInfo;
            RootPanelInfo panelInfo = vBModel.panelInfo;
            bool result;
            try
            {
                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                string infoJson = JsonConvert.SerializeObject(info, Formatting.None, jsonSetting);
                LogTextHelper.Info($"{vBModel.SN} {vBModel.Side}  准备调用算法,参数为：" + infoJson);

                // 计算超时时间：图片数量 * 2秒
                int timeoutSeconds = vBModel.Mats.Count * 2;
                string msg = null;
                bool timedOut = false;

                // 使用Task包装算法调用以实现超时控制
                var task = Task.Run(() =>
                {
                    _defect.DefectMethodWithImages(info, vBModel.Mats, out string outMsg);
                    return outMsg;
                });

                if (task.Wait(TimeSpan.FromSeconds(timeoutSeconds)))
                {
                    msg = task.Result;
                }
                else
                {
                    timedOut = true;
                    LogTextHelper.Warn($"{vBModel.SN} {vBModel.Side} 算法调用超时 (超过 {timeoutSeconds}秒)");
                }

                if (timedOut)
                {
                    EnqueuePostProcess(vBModel, "", false);
                    SystemEvent.SendTaskMsg(vBModel.SN, $"{vBModel.Side}面算法调用超时，跳过AI检测");
                    return true;
                }

                LogTextHelper.Info($"{vBModel.SN} {vBModel.Side} 算法返回原始结果: {msg}");

                if (string.IsNullOrEmpty(msg))
                {
                    LogTextHelper.Error($"算法返回结果为空 for Side {panelInfo.SideIndex}");
                    return false;
                }

                EnqueuePostProcess(vBModel, msg, true);

                var obj = JsonConvert.DeserializeObject<RootVBOutInfo>(msg);
                if (obj == null)
                {
                    LogTextHelper.Error($"算法返回结果反序列化失败 for Side {panelInfo.SideIndex}，原始消息: {msg}");
                    SystemEvent.SendTaskMsg(vBModel.SN, $"{vBModel.Side}面算法返回结果反序列化失败");
                    return false;
                }
                string code = obj.Code.ToString();
                string message = obj.Message.ToString();

                if (code == "200")
                {
                    JObject root = JObject.Parse(msg);
                    var dataToken = root["data"];
                    var inferWholeData = (dataToken as JObject)?["infer_whole_data"];
                    var inferResultsToken = (inferWholeData as JObject)?["infer_results"];

                    if (inferResultsToken is JArray inferResults)
                    {
                        foreach (var result1 in inferResults)
                        {
                            var inferDetails = (result1 as JObject)?["infer_details"];
                            if ((inferDetails as JObject)?["node_details"] is JObject nodeDetails)
                            {
                                string nodeDetailsJson = nodeDetails.ToString();
                                detailsList.Add(nodeDetailsJson);
                            }
                        }
                    }

                    pcsResult.vb_List = new List<VBRcvInfp>();
                    for (int i = 0; i < obj.Data.InferWholeData.InferResults.Count; i++)
                    {
                        VBRcvInfp vBRcv = new VBRcvInfp
                        {
                            bbox = new List<List<double>>()
                        };

                        if (obj.Data.InferWholeData.InferResults[i].Infer_Result != "OK")
                        {
                            for (int j = 0; j < obj.Data.InferWholeData.InferResults[i].inferDetails.Location.Count; j++)
                            {
                                string sub_defectName = obj.Data.InferWholeData.InferResults[i].Defect_name;

                                int subX = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].X);
                                int subY = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Y);
                                int subH = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Height);
                                int subW = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Width);

                                vBRcv.raw_bbox = new List<double>
                                {
                                    subX,
                                    subY,
                                    subW,
                                    subH
                                };
                                vBRcv.bbox.Add(vBRcv.raw_bbox);
                                vBRcv.sub_DefectNames.Add(sub_defectName);
                            }
                        }

                        if (vBModel.isByPass)
                        {
                            resList.Add("2");
                        }
                        else
                        {
                            resList.Add(obj.Data.InferWholeData.InferResults[i].Infer_Result == "NG" ? "1" : "0");
                        }
                        pcsResult.vb_List.Add(vBRcv);
                    }

                    vbJson = msg;
                    result = true;
                }
                else if (code == "600")
                {
                    LogTextHelper.Info($"{vBModel.SN} 算法返回码600: {message}");
                    SystemEvent.SendTaskMsg(vBModel.SN, $"{vBModel.Side}面算法返回(Code:600, {message})");
                    result = true;
                }
                else
                {
                    LogTextHelper.Warn($"算法调用失败 for Side {panelInfo.SideIndex}，返回码: {code}，返回信息：{message}");
                    SystemEvent.SendTaskMsg(vBModel.SN, $"{vBModel.Side}面算法调用失败(Code:{code}, {message})");
                    result = false;
                }
            }
            catch (Exception ex)
            {
                vbJson = string.Empty;
                resList = null;
                detailsList = null;
                result = false;
                pcsResult = null;
                SystemEvent.SendAlarmMsg("算法处理异常" + ex.ToString());
            }
            return result;
        }

        public void EnqueuePostProcess(VBModel vBModel, string rawJsonResult, bool needsProcessing)
        {
            var resultModel = new InferenceResultModel
            {
                RawJsonResult = rawJsonResult,
                VBModel = vBModel,
                InferenceCompletedTime = DateTime.Now,
                NeedsProcessing = needsProcessing
            };

            _inferencePostProcessQueue.Enqueue(resultModel);
            LogTextHelper.Info($"推理结果已加入后处理队列: SN={vBModel.SN}, QueueCount={_inferencePostProcessQueue.Count}");
        }

        public void EnqueueAIResult(VBModel info, List<string> msg)
        {
            SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面正在回写结果");
            RootAIResult data = new RootAIResult
            {
                DbName = "filter_time_to_airesults",
                Operation = "put",
                OpMode = info.Side == "A" ? "all_ow" : "ap",
                Key = info.Key,
            };

            List<ResultInfo> results = new List<ResultInfo>();
            if (msg == null || msg.Count == 0)
            {
                msg = Enumerable.Repeat("1", info.DefectIndex.Count).ToList();
            }
            for (int i = 0; i < info.DefectIndex.Count; i++)
            {
                ResultInfo res = new ResultInfo
                {
                    ResultInfos = $"{info.Side}_{info.PcsIndex[i]}_{info.DefectIndex[i]}_{msg[i]}",
                    Details = new Details()
                };
                results.Add(res);
            }
            WriteBackData writeBackData = new WriteBackData()
            {
                ResultInfos = results,
                SerialNumber = info.SN,
                PanelJsonPath = info.panelInfo.LocalDescribePath,
            };

            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            data.Value = JsonConvert.SerializeObject(writeBackData, Formatting.None, jsonSetting);

            DsCenterInfo dsinfo = null;
            var dbTub = Tuple.Create(info.Key, info.SN, info.Side, data, dsinfo);
            _aiResultQueue.Enqueue(dbTub);
        }
    }
}

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
        private readonly ConcurrentQueue<Tuple<List< AIDetailResultItem>, RootAIResult>> _aiResultQueue;
        private readonly ConcurrentQueue<InferenceResultModel> _inferencePostProcessQueue;

        public DefectProcessor(DefectClass defect,
            ConcurrentQueue<Tuple<List<AIDetailResultItem>, RootAIResult>> aiResultQueue,
            ConcurrentQueue<InferenceResultModel> inferencePostProcessQueue)
        {
            _defect = defect ?? throw new ArgumentNullException(nameof(defect));
            _aiResultQueue = aiResultQueue;
            _inferencePostProcessQueue = inferencePostProcessQueue;
        }

        /// <summary>
        /// 调用 DefectClass.DefectMethodWithImages 进行推理，结果处理与原 BusinessClass.DefectMethod 相同的逻辑
        /// </summary>
        public bool DefectMethod(VBModel vBModel,int maxCount, out List<string> resList, int timeoutSeconds = 10)
        {
            resList = new List<string>();
            if (vBModel.Mats == null || vBModel.Mats.Count == 0)
            {
                EnqueuePostProcess(vBModel, "", false);
                TaskStatusSender.SendSkipped(vBModel.SN, vBModel.Side, "缺陷数为0");
                return true;
            }

            if (vBModel.Mats.Count > maxCount )
            {
                EnqueuePostProcess(vBModel, "", false);
                TaskStatusSender.SendSkipped(vBModel.SN, vBModel.Side, $"图片数量超过最大值({vBModel.Mats.Count}>{maxCount})");
                return true;
            }

            if (vBModel.Mats_Temp == null || vBModel.Mats_Temp.Count == 0)
            {
                EnqueuePostProcess(vBModel, "", false);
                TaskStatusSender.SendSkipped(vBModel.SN, vBModel.Side, "模板图为空");
                return true;
            }

            RootVBInfo info = vBModel.VbInfo;
            bool result;
            try
            {
                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                string infoJson = JsonConvert.SerializeObject(info, Formatting.None, jsonSetting);
                LogTextHelper.Info($"{vBModel.SN} {vBModel.Side}  准备调用算法,参数为：" + infoJson);

                string msg = null;
                bool timedOut = false;

                // 使用Task封装算法调用实现超时机制
                var task = Task.Run(() =>
                {
                    _defect.DefectMethodWithImages2(info, vBModel.Mats,vBModel.Mats_Temp, out string outMsg);

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
                    TaskStatusSender.SendFailed(vBModel.SN, vBModel.Side, $"算法调用超时(>{timeoutSeconds}秒)");
                    return true;
                }

                LogTextHelper.Info($"{vBModel.SN} {vBModel.Side} 算法返回原始结果: {msg}");

                // 存储推理返回JSON到调试缓存
                try
                {
                    var debugInfo = SnDebugInfoCache.GetOrCreate(vBModel.SN, vBModel.Side);
                    debugInfo.InferenceReturnJson = msg;
                }
                catch { }

                if (string.IsNullOrEmpty(msg))
                {
                    LogTextHelper.Error($"算法返回结果为空 for Side {vBModel.Side}");
                    return false;
                }

                EnqueuePostProcess(vBModel, msg, true);

                var obj = JsonConvert.DeserializeObject<RootVBOutInfo>(msg);
                if (obj == null)
                {
                    LogTextHelper.Error($"算法返回结果反序列化失败 for Side {vBModel.Side}，原始消息: {msg}");
                    TaskStatusSender.SendFailed(vBModel.SN, vBModel.Side, "算法返回结果反序列化失败");
                    return false;
                }
                string code = obj.Code.ToString();
                string message = obj.Message.ToString();

                if (code == "200")
                {
                    for (int i = 0; i < obj.Data.InferWholeData.InferResults.Count; i++)
                    {
                        if (vBModel.isByPass)
                        {
                            resList.Add("2");
                        }
                        else
                        {
                            resList.Add(obj.Data.InferWholeData.InferResults[i].Infer_Result == "NG" ? "1" : "0");
                        }
                    }

                    result = true;
                }
                else if (code == "600")
                {
                    LogTextHelper.Info($"{vBModel.SN} 算法返回码600: {message}");
                    TaskStatusSender.SendSkipped(vBModel.SN, vBModel.Side, $"Code:600, {message}");
                    result = true;
                }
                else
                {
                    LogTextHelper.Warn($"算法处理失败 for Side {vBModel.Side}，错误码: {code}，错误信息：{message}");
                    TaskStatusSender.SendFailed(vBModel.SN, vBModel.Side, $"Code:{code}, {message}");
                    result = false;
                }
            }
            catch (Exception ex)
            {
                resList = null;
                result = false;
                SystemEvent.SendAlarmMsg("算法调用异常" + ex.ToString());
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
            // 使用新的状态发送方式
            TaskStatusSender.SendWritingResults(info.SN, info.Side);
            
            // 使用 VBModel 中携带的源数据库信息，回写到对应的DB
            var writeBackDbName = !string.IsNullOrEmpty(info.SourceWriteBackDbName)
                ? info.SourceWriteBackDbName
                : "filter_time_to_airesults";
            var targetUrl = info.SourceDbUrl; // 可能为 null，ResultWriterService 会使用默认 URL

            RootAIResult data = new RootAIResult
            {
                SN=info.SN,
                Side=info.Side,
                DbName = writeBackDbName,
                Operation = "put",
                OpMode = info.Side == "A" ? "all_ow" : "ap",
                Key = info.Key,
                TargetUrl = targetUrl,
            };
            LogTextHelper.Info($"SN:{info.SN} Side:{info.Side} 回写目标DB:{writeBackDbName}, URL:{targetUrl}");

            List<AIDetailResultItem> aIDetailResults=new List<AIDetailResultItem>();
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

                aIDetailResults.Add(new AIDetailResultItem()
                {
                    Index = i,
                    PcsIndex=i,
                    AiLabel = msg[i] == "0" ? "OK" : "NG",
                    AiClsType="",
                    AiFlag="Standard",
                    InferDetail=new Dictionary<string, object>(),

                });
            }

            // 追加直报缺陷的结果（标记为bypass "2"）
            if (info.DirectReportDefectIndices != null && info.DirectReportDefectIndices.Count > 0)
            {
                for (int i = 0; i < info.DirectReportDefectIndices.Count; i++)
                {
                    ResultInfo res = new ResultInfo
                    {
                        ResultInfos = $"{info.Side}_{info.DirectReportPcsIndices[i]}_{info.DirectReportDefectIndices[i]}_2",
                        Details = new Details()
                    };
                    results.Add(res);

                    aIDetailResults.Add(new AIDetailResultItem()
                    {
                        Index = info.DefectIndex.Count + i,
                        PcsIndex = info.DefectIndex.Count + i,
                        AiLabel = "NG",
                        AiClsType = "",
                        AiFlag = "DirectReport",
                        InferDetail = new Dictionary<string, object>(),
                    });
                }
                LogTextHelper.Info($"SN:{info.SN} 追加 {info.DirectReportDefectIndices.Count} 个直报缺陷结果");
            }
            WriteBackData writeBackData = new WriteBackData()
            {
                ResultInfos = results,
                SerialNumber = info.SN,
                PanelJsonPath = info.panelInfo.LocalDescribePath,
            };

            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            data.Value = JsonConvert.SerializeObject(writeBackData, Formatting.None, jsonSetting);



            var dbTub = Tuple.Create(aIDetailResults, data);
            _aiResultQueue.Enqueue(dbTub);
        }
    }
}

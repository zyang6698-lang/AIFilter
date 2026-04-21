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
using System.Threading;
using System.Threading.Tasks;
using DeepSightDB;
using DeepSightTool;

namespace DeepSightWorkLib.Services
{
    public class DefectProcessor
    {
        private readonly DefectClass _defect;
        private readonly ConcurrentQueue< RootAIResult> _aiResultQueue;
        private readonly ConcurrentQueue<InferenceResultModel> _inferencePostProcessQueue;

        public DefectProcessor(DefectClass defect,
            ConcurrentQueue< RootAIResult> aiResultQueue,
            ConcurrentQueue<InferenceResultModel> inferencePostProcessQueue)
        {
            _defect = defect ?? throw new ArgumentNullException(nameof(defect));
            _aiResultQueue = aiResultQueue;
            _inferencePostProcessQueue = inferencePostProcessQueue;
        }

        #region Build 方法（Pipeline 友好，纯构建不入队）

        /// <summary>
        /// 构建后处理模型（不入队）
        /// </summary>
        public static InferenceResultModel BuildPostProcessModel(VBModel vBModel, string rawJsonResult, bool needsProcessing)
        {
            return new InferenceResultModel
            {
                RawJsonResult = rawJsonResult,
                VBModel = vBModel,
                InferenceCompletedTime = DateTime.Now,
                NeedsProcessing = needsProcessing
            };
        }

        /// <summary>
        /// 构建 AI 回写结果（不入队）
        /// </summary>
        public static RootAIResult BuildAIResult(VBModel info, List<string> msg)
        {
            var writeBackDbName = !string.IsNullOrEmpty(info.SourceWriteBackDbName)
                ? info.SourceWriteBackDbName
                : "filter_time_to_airesults";
            var vrsWriteBackDbName = !string.IsNullOrEmpty(info.SourceVRSWriteBackDbName)
                ? info.SourceVRSWriteBackDbName
                : "ai_detail_results_tovrs";
            var targetUrl = info.SourceDbUrl;

            RootAIResult data = new RootAIResult
            {
                SN = info.SN,
                Side = info.Side,
                VRSDbKey=info.panelInfo.SerialNumber,
                AVIDbName = writeBackDbName,
                Operation = "put",
                OpMode = info.Side == "A" ? "all_ow" : "ap",
                Key = info.Key,
                TargetUrl = targetUrl,
                VRSTargetUrl = targetUrl,
                VRSDbName = vrsWriteBackDbName
            };
            LogTextHelper.Info($"SN:{info.SN} Side:{info.Side} 回写AVI DB:{writeBackDbName}, VRS DB:{vrsWriteBackDbName}, URL:{targetUrl}");

            List<AIDetailResultItem> aIDetailResults = new List<AIDetailResultItem>();
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
                    Index = info.DefectIndex[i],
                    PcsIndex = info.PcsIndex[i],
                    AiLabel = msg[i] == "0" ? "ok" : "ng",
                    AiClsType = "",
                    AiFlag = "experiment",
                    InferDetail = new Dictionary<string, object>(),
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
                        Index = info.DefectIndex[i],
                        PcsIndex = info.PcsIndex[i],
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
            data.AIDetailResultItems = aIDetailResults;

            return data;
        }

        #endregion

        #region Pipeline 友好的推理入口

        /// <summary>
        /// Pipeline 友好的推理方法 - 不入队，所有结果通过返回值传出
        /// </summary>
        /// <param name="vBModel">推理模型</param>
        /// <param name="maxCount">最大缺陷数</param>
        /// <param name="timeoutSeconds">超时秒数</param>
        /// <param name="cancellationToken">取消令牌（Pipeline 停止时可通知协作取消）</param>
        /// <returns>推理阶段结果</returns>
        public InferenceStageResult Infer(VBModel vBModel, int maxCount, int timeoutSeconds = 10, CancellationToken cancellationToken = default)
        {
            var stageResult = new InferenceStageResult();

            if (!_defect.IsInitialized)
            {
                LogTextHelper.Warn($"[{vBModel.SN} {vBModel.Side}] AI 引擎未初始化，跳过推理（Pipeline）");
                stageResult.PostProcessModel = BuildPostProcessModel(vBModel, "", false);
                TaskStatusSender.SendSkipped(vBModel.SN, vBModel.Side, "AI引擎未初始化");
                stageResult.Success = true;
                return stageResult;
            }

            if (vBModel.Mats == null || vBModel.Mats.Count == 0)
            {
                stageResult.PostProcessModel = BuildPostProcessModel(vBModel, "", false);
                TaskStatusSender.SendSkipped(vBModel.SN, vBModel.Side, "缺陷数为0");
                stageResult.Success = true;
                return stageResult;
            }

            if (vBModel.Mats.Count > maxCount)
            {
                stageResult.PostProcessModel = BuildPostProcessModel(vBModel, "", false);
                TaskStatusSender.SendSkipped(vBModel.SN, vBModel.Side, $"图片数量超过最大值({vBModel.Mats.Count}>{maxCount})");
                stageResult.Success = true;
                return stageResult;
            }

            if (vBModel.Mats_Temp == null || vBModel.Mats_Temp.Count == 0)
            {
                stageResult.PostProcessModel = BuildPostProcessModel(vBModel, "", false);
                TaskStatusSender.SendSkipped(vBModel.SN, vBModel.Side, "模板图为空");
                stageResult.Success = true;
                return stageResult;
            }

            RootVBInfo info = vBModel.VbInfo;
            try
            {
                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                string infoJson = JsonConvert.SerializeObject(info, Formatting.None, jsonSetting);
                LogTextHelper.Info($"{vBModel.SN} {vBModel.Side}  准备调用算法,参数为：" + infoJson);

                string msg = null;
                bool timedOut = false;

                // 注意：算法本身（DefectMethodWithImages2）为非托管调用，不支持 CancellationToken 取消，
                // 这里仅用 CancellationToken 实现协作式的等待取消，超时或取消后算法仍可能在后台运行直到结束。
                var task = Task.Run(() =>
                {
                    _defect.DefectMethodWithImages2(info, vBModel.Mats, vBModel.Mats_Temp, out string outMsg);
                    return outMsg;
                }, cancellationToken);

                try
                {
                    if (task.Wait(timeoutSeconds * 1000, cancellationToken))
                    {
                        msg = task.Result;
                    }
                    else
                    {
                        timedOut = true;
                        LogTextHelper.Warn($"{vBModel.SN} {vBModel.Side} 算法调用超时 (超过 {timeoutSeconds}秒)");
                    }
                }
                catch (OperationCanceledException)
                {
                    LogTextHelper.Warn($"{vBModel.SN} {vBModel.Side} 推理等待被取消");
                    stageResult.Success = false;
                    return stageResult;
                }

                if (timedOut)
                {
                    stageResult.PostProcessModel = BuildPostProcessModel(vBModel, "", false);
                    TaskStatusSender.SendFailed(vBModel.SN, vBModel.Side, $"算法调用超时(>{timeoutSeconds}秒)");
                    stageResult.Success = true;
                    return stageResult;
                }

                LogTextHelper.Info($"{vBModel.SN} {vBModel.Side} 算法返回原始结果: {msg}");

                // 存储推理返回JSON到调试缓存
                try
                {
                    var debugInfo = SnDebugInfoCache.GetOrCreate(vBModel.SN, vBModel.Side);
                    debugInfo.InferenceReturnJson = msg;
                    var prev = debugInfo.JudgmentSummary ?? "";
                    debugInfo.JudgmentSummary = prev + " → AI推理完成";
                }
                catch { }

                if (string.IsNullOrEmpty(msg))
                {
                    LogTextHelper.Error($"算法返回结果为空 for Side {vBModel.Side}");
                    stageResult.Success = false;
                    return stageResult;
                }

                stageResult.PostProcessModel = BuildPostProcessModel(vBModel, msg, true);

                var obj = JsonConvert.DeserializeObject<RootVBOutInfo>(msg);
                if (obj == null)
                {
                    LogTextHelper.Error($"算法返回结果反序列化失败 for Side {vBModel.Side}，原始消息: {msg}");
                    TaskStatusSender.SendFailed(vBModel.SN, vBModel.Side, "算法返回结果反序列化失败");
                    stageResult.Success = false;
                    return stageResult;
                }
                string code = obj.Code.ToString();
                string message = obj.Message.ToString();

                if (code == "200")
                {
                    for (int i = 0; i < obj.Data.InferWholeData.InferResults.Count; i++)
                    {
                        stageResult.Messages.Add(obj.Data.InferWholeData.InferResults[i].Infer_Result == "NG" ? "1" : "0");
                    }
                    stageResult.Success = true;
                }
                else if (code == "600")
                {
                    LogTextHelper.Info($"{vBModel.SN} 算法返回码600: {message}");
                    TaskStatusSender.SendSkipped(vBModel.SN, vBModel.Side, $"Code:600, {message}");
                    stageResult.Success = true;
                }
                else
                {
                    LogTextHelper.Warn($"算法处理失败 for Side {vBModel.Side}，错误码: {code}，错误信息：{message}");
                    TaskStatusSender.SendFailed(vBModel.SN, vBModel.Side, $"Code:{code}, {message}");
                    stageResult.Success = false;
                }
            }
            catch (Exception ex)
            {
                stageResult.Success = false;
                SystemEvent.SendAlarmMsg("算法调用异常" + ex.ToString());
            }
            return stageResult;
        }

        #endregion
    }

    /// <summary>
    /// 推理阶段的输出结果（Pipeline 友好，不含入队操作）
    /// </summary>
    public class InferenceStageResult
    {
        /// <summary>推理是否成功</summary>
        public bool Success { get; set; }

        /// <summary>推理结果消息列表（每个缺陷的 OK/NG 标记）</summary>
        public List<string> Messages { get; set; } = new List<string>();

        /// <summary>后处理模型（始终会构建，用于 PostProcess 阶段）</summary>
        public InferenceResultModel PostProcessModel { get; set; }
    }
}

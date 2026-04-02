using DeepSightCommunication;
using DeepSightDB;
using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DeepSightWorkLib.Services
{
    public class ResultWriterService
    {
        private readonly HttpClass _httpDb;
        private readonly ConcurrentDictionary<string, DateTime> _processingSnSet;

        public ResultWriterService(HttpClass httpDb, ConcurrentDictionary<string, DateTime> processingSnSet)
        {
            _httpDb = httpDb ?? throw new ArgumentNullException(nameof(httpDb));
            _processingSnSet = processingSnSet ?? throw new ArgumentNullException(nameof(processingSnSet));
        }

        public bool ReturnAVIVRS( RootAIResult aiResult)
        {
            try
            {
                bool vrsSuccess = false;
                bool aviSuccess = false;

                // === 1. 发送 VRS 数据 ===
                var vrsTargetUrl = !string.IsNullOrEmpty(aiResult?.VRSTargetUrl)
                    ? aiResult.VRSTargetUrl
                    : (!string.IsNullOrEmpty(aiResult?.TargetUrl) ? aiResult.TargetUrl : "");

                if (aiResult.AIDetailResultItems != null && aiResult.AIDetailResultItems.Count > 0
                    && !string.IsNullOrEmpty(aiResult.VRSDbName))
                {
                    var vrsDbInfo = new RootDbInfo
                    {
                        uniqueKey = Guid.NewGuid().ToString(),
                        db_name = aiResult.VRSDbName,
                        operation = "put",
                        op_mode = "all_ow",
                        key = $"{aiResult.SN}_{aiResult.Side}",
                        value = JsonConvert.SerializeObject(aiResult.AIDetailResultItems),
                    };

                    LogTextHelper.Info($"SN:{aiResult.SN} Side:{aiResult.Side} 回写VRS到 URL:{vrsTargetUrl}, DB:{aiResult.VRSDbName}");
                    if (_httpDb.HttpPostMethod(vrsTargetUrl, vrsDbInfo, 1, out string vrsResult))
                    {
                        vrsSuccess = true;
                        LogTextHelper.Info($"SN:{aiResult.SN} Side:{aiResult.Side} VRS回写成功");
                    }
                    else
                    {
                        LogTextHelper.Warn($"SN:{aiResult.SN} Side:{aiResult.Side} VRS回写失败");
                    }
                }

                // === 2. 发送 AVI 数据 ===
                var aviTargetUrl = !string.IsNullOrEmpty(aiResult?.TargetUrl) ? aiResult.TargetUrl : "";

                TaskStatusSender.SendWritingResults(aiResult.SN, aiResult.Side);
                LogTextHelper.Info($"SN:{aiResult.SN} Side:{aiResult.Side} 回写AVI到 URL:{aviTargetUrl}, DB:{aiResult?.AVIDbName}");
                if (_httpDb.HttpPostMethod(aviTargetUrl, aiResult, 1, out string result))
                {
                    aviSuccess = true;
                    TaskStatusSender.SendCompleted(aiResult.SN, aiResult.Side);

                    string snKey = $"{aiResult.SN}_{aiResult.Side}";
                    if (_processingSnSet.TryRemove(snKey, out DateTime addTime))
                    {
                        var duration = DateTime.Now - addTime;
                        LogTextHelper.Info($"SN:{aiResult.SN} Side:{aiResult.Side} 处理完成，耗时：{duration.TotalSeconds:F2}秒，已从处理集合中移除");
                    }
                    else
                    {
                        LogTextHelper.Warn($"SN:{aiResult.SN} Side:{aiResult.Side} 未在处理集合中找到，可能已被清理或未正确添加");
                    }
                }
                else
                {
                    LogTextHelper.Warn($"SN:{aiResult.SN} Side:{aiResult.Side} AVI回写失败");
                }

                return aviSuccess;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                return false;
            }
        }
    }
}

using DeepSightCommunication;
using DeepSightDB;
using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepSightWorkLib.Services
{
    public class ResultWriterService
    {
        private readonly HttpClass _httpDb;

        public ResultWriterService(HttpClass httpDb)
        {
            _httpDb = httpDb ?? throw new ArgumentNullException(nameof(httpDb));
        }

        public bool ReturnAVIVRS( RootAIResult aiResult)
        {
            try
            {
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
                        key = $"{aiResult.VRSDbKey}_{aiResult.Side}",
                        value = JsonConvert.SerializeObject(aiResult.AIDetailResultItems),
                    };

                    // 存储VRS回写JSON到调试缓存
                    try
                    {
                        var vrsDebugInfo = SnDebugInfoCache.GetOrCreate(aiResult.SN, aiResult.Side);
                        vrsDebugInfo.VrsWriteBackJson = JsonConvert.SerializeObject(vrsDbInfo, Formatting.Indented);
                    }
                    catch { /* 调试信息存储失败不影响业务 */ }

                    LogTextHelper.Info($"SN:{aiResult.SN} Side:{aiResult.Side} 回写VRS到 URL:{vrsTargetUrl}, DB:{aiResult.VRSDbName}");
                    if (_httpDb.HttpPostMethod(vrsTargetUrl, vrsDbInfo, 1, out string vrsResult))
                    {
                        LogTextHelper.Info($"SN:{aiResult.SN} Side:{aiResult.Side} VRS回写成功");
                    }
                    else
                    {
                        LogTextHelper.Warn($"SN:{aiResult.SN} Side:{aiResult.Side} VRS回写失败");
                    }
                }

                // === 3. 发送 VRS V1.0 数据（回写数据库V1.0）===
                if (aiResult.AIDetailResultItems != null && aiResult.AIDetailResultItems.Count > 0
                    && !string.IsNullOrEmpty(aiResult.VRSTargetUrl))
                {
                    // 从配置中查找 VRSWriteBackDbNameV1（默认 "ai_inference_result"）
                    string vrsDbNameV1 = "ai_inference_result";
                    try
                    {
                        var config = LevelDbConfigManager.Instance.Databases
                            .FirstOrDefault(db => db.IsEnabled);
                        if (config != null)
                            vrsDbNameV1 = config.VRSWriteBackDbNameV1;
                    }
                    catch { /* 配置读取失败不影响主流程 */ }

                    // 构建扁平化的 value 数组：side_pcsindex_defectindex_状态码
                    var valueList = new List<string>();
                    foreach (var item in aiResult.AIDetailResultItems)
                    {
                        string statusCode = item.AiLabel == "OK" ? "0"
                                          : item.AiLabel == "NG" ? "1"
                                          : "2";
                        valueList.Add($"{aiResult.Side}_{item.PcsIndex}_{item.Index}_{statusCode}");
                    }

                    var vrsV1DbInfo = new RootDbInfo
                    {
                        uniqueKey = Guid.NewGuid().ToString(),
                        db_name = vrsDbNameV1,
                        operation = "put",
                        op_mode = "all_ow",
                        key = $"{aiResult.SN}_{aiResult.Side}",
                        value = JsonConvert.SerializeObject(valueList),
                    };

                    // 存储VRS V1.0回写JSON到调试缓存
                    try
                    {
                        var vrsV1DebugInfo = SnDebugInfoCache.GetOrCreate(aiResult.SN, aiResult.Side);
                        vrsV1DebugInfo.VrsV1WriteBackJson = JsonConvert.SerializeObject(vrsV1DbInfo, Formatting.Indented);
                    }
                    catch { /* 调试信息存储失败不影响业务 */ }

                    LogTextHelper.Info($"SN:{aiResult.SN} Side:{aiResult.Side} 回写VRS V1.0到 URL:{aiResult.VRSTargetUrl}, DB:{vrsDbNameV1}");
                    if (_httpDb.HttpPostMethod(aiResult.VRSTargetUrl, vrsV1DbInfo, 1, out string vrsV1Result))
                    {
                        LogTextHelper.Info($"SN:{aiResult.SN} Side:{aiResult.Side} VRS V1.0回写成功");
                    }
                    else
                    {
                        LogTextHelper.Warn($"SN:{aiResult.SN} Side:{aiResult.Side} VRS V1.0回写失败");
                    }
                }

                // === 2. 发送 AVI 数据 ===
                var aviTargetUrl = !string.IsNullOrEmpty(aiResult?.TargetUrl) ? aiResult.TargetUrl : "";

                // 存储AVI回写JSON到调试缓存
                try
                {
                    var aviDebugInfo = SnDebugInfoCache.GetOrCreate(aiResult.SN, aiResult.Side);
                    aviDebugInfo.AviWriteBackJson = JsonConvert.SerializeObject(aiResult, Formatting.Indented);
                }
                catch { /* 调试信息存储失败不影响业务 */ }

                TaskStatusSender.SendWritingResults(aiResult.SN, aiResult.Side);
                LogTextHelper.Info($"SN:{aiResult.SN} Side:{aiResult.Side} 回写AVI到 URL:{aviTargetUrl}, DB:{aiResult?.AVIDbName}");
                if (_httpDb.HttpPostMethod(aviTargetUrl, aiResult, 1, out string result))
                {
                    aviSuccess = true;
                    TaskStatusSender.SendCompleted(aiResult.SN, aiResult.Side);
                    LogTextHelper.Info($"SN:{aiResult.SN} Side:{aiResult.Side} AVI回写成功");
                }
                else
                {
                    LogTextHelper.Warn($"SN:{aiResult.SN} Side:{aiResult.Side} AVI回写失败");
                    TaskStatusSender.SendFailed(aiResult.SN, aiResult.Side, "AVI回写失败");
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

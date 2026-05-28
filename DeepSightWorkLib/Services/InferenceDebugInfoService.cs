using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepSightWorkLib.Services
{
    public static class InferenceDebugInfoService
    {
        public static void RecordOfflineStep(VBModel vbModel, string modeName, string lotNumber, IEnumerable<DetectInfo> defectPoints,
            object sourceData, int loadedDefectImageCount = -1, int loadedReferenceImageCount = -1, string stage = null)
        {
            if (vbModel == null) return;

            try
            {
                var debugInfo = SnDebugInfoCache.GetOrCreate(vbModel.SN, vbModel.Side);
                debugInfo.SourceDbName = modeName;
                debugInfo.ProductSerial = string.IsNullOrEmpty(debugInfo.ProductSerial) ? vbModel.ProductSerial : debugInfo.ProductSerial;
                debugInfo.LotNumber = string.IsNullOrEmpty(debugInfo.LotNumber) ? lotNumber : debugInfo.LotNumber;
                debugInfo.DefectCount = defectPoints?.Count() ?? vbModel.AllDefectInfos?.Count ?? debugInfo.DefectCount;
                debugInfo.PcsCount = vbModel.PcsIndex?.Distinct().Count() ?? debugInfo.PcsCount;
                debugInfo.ImageCount = vbModel.ImageKeys?.Count ?? debugInfo.ImageCount;
                debugInfo.MinioPath = string.IsNullOrEmpty(debugInfo.MinioPath) ? vbModel.ImageKeys?.FirstOrDefault() : debugInfo.MinioPath;

                if (sourceData != null)
                    debugInfo.PanelInfoJson = SerializeDebugJson(sourceData);

                if (vbModel.VbInfo != null)
                    debugInfo.VbInferenceJson = SerializeDebugJson(vbModel.VbInfo);

                var summary = new StringBuilder();
                summary.Append($"{DateTime.Now:HH:mm:ss.fff} {modeName}");
                if (!string.IsNullOrEmpty(stage)) summary.Append($" | {stage}");
                summary.Append($" | TaskId={vbModel.TestTaskId}");
                summary.Append($" | 推理点={vbModel.ImageKeys?.Count ?? 0}");
                summary.Append($" | 总缺陷={debugInfo.DefectCount}");
                if (loadedDefectImageCount >= 0) summary.Append($" | 原图加载={loadedDefectImageCount}");
                if (loadedReferenceImageCount >= 0) summary.Append($" | 参考图加载={loadedReferenceImageCount}");
                AppendSummary(debugInfo, summary.ToString());

                SnDebugInfoCache.Cleanup();
            }
            catch (Exception ex)
            {
                LogTextHelper.Warn($"记录推理调试信息异常: {vbModel.SN}_{vbModel.Side}, {ex.Message}");
            }
        }

        public static void RecordFailure(VBModel vbModel, string stage, string message)
        {
            if (vbModel == null) return;

            try
            {
                var debugInfo = SnDebugInfoCache.GetOrCreate(vbModel.SN, vbModel.Side);
                debugInfo.ProductSerial = string.IsNullOrEmpty(debugInfo.ProductSerial) ? vbModel.ProductSerial : debugInfo.ProductSerial;
                debugInfo.HasError = true;
                debugInfo.ErrorStep = stage;
                debugInfo.ErrorMessage = message;
                debugInfo.ErrorTime = DateTime.Now;
                AppendSummary(debugInfo, $"[离线任务异常] {stage}: {message}");
            }
            catch (Exception ex)
            {
                LogTextHelper.Warn($"记录推理异常调试信息失败: {vbModel.SN}_{vbModel.Side}, {ex.Message}");
            }
        }

        public static string RecordInferenceReturn(VBModel vbModel, string rawJson)
        {
            string safeJson = RedactDebugJson(rawJson, Formatting.None);
            if (vbModel == null) return safeJson;

            try
            {
                var debugInfo = SnDebugInfoCache.GetOrCreate(vbModel.SN, vbModel.Side);
                debugInfo.InferenceReturnJson = safeJson;
                AppendSummary(debugInfo, "AI推理完成");
            }
            catch (Exception ex)
            {
                LogTextHelper.Warn($"记录推理返回调试信息失败: {vbModel.SN}_{vbModel.Side}, {ex.Message}");
            }

            return safeJson;
        }

        public static string SerializeDebugJson(object value)
        {
            if (value == null) return null;
            string json = JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return RedactDebugJson(json, Formatting.Indented);
        }

        public static string RedactDebugJson(string json, Formatting formatting = Formatting.Indented)
        {
            if (string.IsNullOrWhiteSpace(json)) return json;
            try
            {
                var token = JToken.Parse(json);
                RedactDebugToken(token);
                return token.ToString(formatting);
            }
            catch
            {
                return json;
            }
        }

        public static void AppendSummary(SnDebugInfo debugInfo, string message)
        {
            if (debugInfo == null || string.IsNullOrWhiteSpace(message)) return;
            debugInfo.JudgmentSummary = string.IsNullOrWhiteSpace(debugInfo.JudgmentSummary)
                ? message
                : debugInfo.JudgmentSummary + Environment.NewLine + message;
        }

        private static void RedactDebugToken(JToken token)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties().ToList())
                {
                    if (IsSensitiveDebugField(property.Name)) property.Value = "[REDACTED]";
                    else RedactDebugToken(property.Value);
                }
            }
            else if (token is JArray array)
            {
                foreach (var item in array) RedactDebugToken(item);
            }
        }

        private static bool IsSensitiveDebugField(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            string lower = name.ToLowerInvariant();
            return lower.Contains("secret") || lower.Contains("password") || lower.Contains("token") || lower.Contains("access_key");
        }
    }
}

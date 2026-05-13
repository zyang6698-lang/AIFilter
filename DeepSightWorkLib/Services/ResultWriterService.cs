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
        // VRS V1.0 状态码：与 AiLabel 的映射
        private const string StatusCodeOk = "0";
        private const string StatusCodeNg = "1";
        private const string StatusCodeOther = "2";

        private readonly LevelDbHttpClient _httpDb;

        public ResultWriterService(LevelDbHttpClient httpDb)
        {
            _httpDb = httpDb ?? throw new ArgumentNullException(nameof(httpDb));
        }

        /// <summary>
        /// 根据 AVI 侧 URL 在配置中查找对应的 LevelDbConfig
        /// （按 Url 精确匹配，匹配不到时回退到第一个启用项）
        /// </summary>
        internal static LevelDbConfig FindConfigByUrl(string url)
            => FindConfig(url, byVrs: false);

        /// <summary>
        /// 根据 VRS 侧 URL 在配置中查找对应的 LevelDbConfig
        /// （按 VRSUrl 精确匹配，匹配不到时回退到第一个启用项）
        /// </summary>
        internal static LevelDbConfig FindConfigByVrsUrl(string url)
            => FindConfig(url, byVrs: true);

        private static LevelDbConfig FindConfig(string url, bool byVrs)
        {
            try
            {
                var dbs = LevelDbConfigManager.Instance.Databases;
                if (dbs == null || dbs.Count == 0) return null;
                var matched = dbs.FirstOrDefault(db => string.Equals(byVrs ? db.VRSUrl : db.Url,
                    url, StringComparison.OrdinalIgnoreCase));
                return matched ?? dbs.FirstOrDefault(db => db.IsEnabled);
            }
            catch (Exception ex)
            {
                LogTextHelper.Warn($"查找 LevelDB 配置失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 将 AiLabel 映射为 VRS V1.0 状态码（OK=0, NG=1, 其他=2，大小写不敏感）
        /// </summary>
        internal static string MapAiLabelToStatusCode(string aiLabel)
        {
            if (string.Equals(aiLabel, "OK", StringComparison.OrdinalIgnoreCase)) return StatusCodeOk;
            if (string.Equals(aiLabel, "NG", StringComparison.OrdinalIgnoreCase)) return StatusCodeNg;
            return StatusCodeOther;
        }

        /// <summary>
        /// 依次执行 VRS 回写、VRS V1.0 回写、AVI 回写。返回 AVI 是否成功（用于上层判断）。
        /// </summary>
        public bool ReturnAVIVRS(RootAIResult aiResult)
        {
            if (aiResult == null) return false;
            try
            {
                WriteVrs(aiResult);
                WriteVrsV1(aiResult);
                return WriteAvi(aiResult);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// VRS 回写（普通）：写入 AIDetailResultItems 到 VRS 侧 DB
        /// </summary>
        private void WriteVrs(RootAIResult r)
        {
            if (r.AIDetailResultItems == null || r.AIDetailResultItems.Count == 0) return;
            if (string.IsNullOrEmpty(r.VRSDbName)) return;

            var url = !string.IsNullOrEmpty(r.VRSTargetUrl) ? r.VRSTargetUrl : r.TargetUrl;
            if (string.IsNullOrEmpty(url)) return;

            var dbInfo = new RootDbInfo
            {
                uniqueKey = Guid.NewGuid().ToString(),
                db_name = r.VRSDbName,
                operation = "put",
                op_mode = "all_ow",
                key = $"{r.VRSDbKey}_{r.Side}",
                value = JsonConvert.SerializeObject(r.AIDetailResultItems),
            };

            CacheDebugJson(r.SN, r.Side, d => d.VrsWriteBackJson = JsonConvert.SerializeObject(dbInfo, Formatting.Indented));

            LogTextHelper.Info($"SN:{r.SN} Side:{r.Side} 回写VRS到 URL:{url}, DB:{r.VRSDbName}");
            bool ok = _httpDb.PostJson(url, dbInfo, LevelDbOperation.Write, out _, "VRS回写");
            LogTextHelper.Info($"SN:{r.SN} Side:{r.Side} VRS回写{(ok ? "成功" : "失败")}");
        }

        /// <summary>
        /// VRS V1.0 回写：根据 VRSTargetUrl 匹配配置，按开关写入扁平化 value 列表
        /// </summary>
        private void WriteVrsV1(RootAIResult r)
        {
            if (r.AIDetailResultItems == null || r.AIDetailResultItems.Count == 0) return;
            if (string.IsNullOrEmpty(r.VRSTargetUrl)) return;

            var cfg = FindConfigByVrsUrl(r.VRSTargetUrl);
            bool enabled = cfg != null && cfg.EnableVRSWriteBackV1;
            if (!enabled)
            {
                LogTextHelper.Info($"SN:{r.SN} Side:{r.Side} VRS V1.0回写未启用，跳过");
                return;
            }

            string dbNameV1 = !string.IsNullOrEmpty(cfg.VRSWriteBackDbNameV1)
                ? cfg.VRSWriteBackDbNameV1
                : "ai_inference_result";

            var valueList = new List<string>();
            foreach (var item in r.AIDetailResultItems)
            {
                string statusCode = MapAiLabelToStatusCode(item.AiLabel);
                valueList.Add($"{r.Side}_{item.PcsIndex}_{item.Index}_{statusCode}");
            }

            var dbInfo = new RootDbInfo
            {
                uniqueKey = Guid.NewGuid().ToString(),
                db_name = dbNameV1,
                operation = "put",
                op_mode = "all_ow",
                key = $"{r.SN}_{r.Side}",
                value = JsonConvert.SerializeObject(valueList),
            };

            CacheDebugJson(r.SN, r.Side, d => d.VrsV1WriteBackJson = JsonConvert.SerializeObject(dbInfo, Formatting.Indented));

            LogTextHelper.Info($"SN:{r.SN} Side:{r.Side} 回写VRS V1.0到 URL:{r.VRSTargetUrl}, DB:{dbNameV1}");
            bool ok = _httpDb.PostJson(r.VRSTargetUrl, dbInfo, LevelDbOperation.Write, out _, "VRS V1.0回写");
            LogTextHelper.Info($"SN:{r.SN} Side:{r.Side} VRS V1.0回写{(ok ? "成功" : "失败")}");
        }

        /// <summary>
        /// AVI 回写：将整个 RootAIResult 写入 AVI 侧 DB；同步发送 UI 状态事件
        /// </summary>
        private bool WriteAvi(RootAIResult r)
        {
            var url = !string.IsNullOrEmpty(r.TargetUrl) ? r.TargetUrl : "";

            CacheDebugJson(r.SN, r.Side, d => d.AviWriteBackJson = JsonConvert.SerializeObject(r, Formatting.Indented));

            TaskStatusSender.SendWritingResults(r.SN, r.Side);
            LogTextHelper.Info($"SN:{r.SN} Side:{r.Side} 回写AVI到 URL:{url}, DB:{r.AVIDbName}");

            if (_httpDb.PostJson(url, r, LevelDbOperation.Write, out _, "AVI回写"))
            {
                TaskStatusSender.SendCompleted(r.SN, r.Side);
                LogTextHelper.Info($"SN:{r.SN} Side:{r.Side} AVI回写成功");
                return true;
            }

            LogTextHelper.Warn($"SN:{r.SN} Side:{r.Side} AVI回写失败");
            TaskStatusSender.SendFailed(r.SN, r.Side, "AVI回写失败");
            return false;
        }

        /// <summary>
        /// 写入调试缓存（不影响业务流程）
        /// </summary>
        private static void CacheDebugJson(string sn, string side, Action<SnDebugInfo> setter)
        {
            try
            {
                var info = SnDebugInfoCache.GetOrCreate(sn, side);
                setter(info);
            }
            catch (Exception ex)
            {
                LogTextHelper.Warn($"SN:{sn} Side:{side} 调试信息存储失败: {ex.Message}");
            }
        }
    }
}

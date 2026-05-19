using DeepSightCommunication;
using DeepSightDB;
using DeepSightEvent;
using DeepSightModel;
using DeepSightModel.Alarm;
using DeepSightTool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 从 LevelDB 的 VRS 历史结果表按 SN 读取并解析 VRS 历史结果。
    /// 遍历所有启用的 LevelDbConfig，命中即返回。
    /// </summary>
    public class VrsHistoryService
    {
        /// <summary>
        /// 当配置中未指定时使用的兜底库名
        /// </summary>
        public const string DefaultDbName = "vrs_history_result";

        private readonly LevelDbHttpClient _httpDb;

        public VrsHistoryService(LevelDbHttpClient httpDb)
        {
            _httpDb = httpDb ?? throw new ArgumentNullException(nameof(httpDb));
        }

        /// <summary>
        /// 按 SN 查询 vrs_history_result
        /// </summary>
        public bool TryGetBySn(string sn, out VrsHistoryResult result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(sn))
                return false;

            var configs = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled)
                .ToList();

            if (configs.Count == 0)
            {
                const string msg = "没有启用的 LevelDB 配置";
                LogTextHelper.Warn($"VrsHistoryService: {msg}");
                try { AlarmService.Instance.RaiseAlarm(AlarmLevel.Warning, AlarmCategory.Communication, "VrsHistoryService", msg); }
                catch { }
                return false;
            }

            foreach (var config in configs)
            {
                var dbName = string.IsNullOrWhiteSpace(config.VrsHistoryDbName)
                    ? DefaultDbName
                    : config.VrsHistoryDbName;
                try
                {
                    if (TryQuery(config.VRSUrl, dbName, sn, out string rawValue, out _))
                    {
                        var parsed = Parse(sn, rawValue);
                        if (parsed != null)
                        {
                            result = parsed;
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"VrsHistoryService 查询 SN={sn} DB={dbName} 异常: {ex}");
                    try { AlarmService.Instance.RaiseAlarm(AlarmLevel.Error, AlarmCategory.Communication, "VrsHistoryService.Query", $"VrsHistoryService 查询异常", $"SN={sn}, DB={dbName}, Error={ex.Message}", sn); }
                    catch { }
                }
            }

            return false;
        }

        /// <summary>
        /// 向指定 URL 的 LevelDB 发起 get 请求并返回 value 字符串。供测试入口使用。
        /// </summary>
        public bool TryQuery(string url, string dbName, string sn, out string rawValue, out string error)
        {
            rawValue = null;
            error = null;

            if (string.IsNullOrWhiteSpace(url))
            {
                error = "URL 为空";
                return false;
            }
            if (string.IsNullOrWhiteSpace(dbName))
            {
                error = "db_name 为空";
                return false;
            }
            if (string.IsNullOrWhiteSpace(sn))
            {
                error = "SN 为空";
                return false;
            }

            var req = new RootDbInfo
            {
                uniqueKey = Guid.NewGuid().ToString(),
                db_name = dbName,
                operation = "get",
                op_mode = "last",
                key = sn
            };

            if (!_httpDb.PostJson(url, req, LevelDbOperation.Read, out string response, "VRS历史查询"))
            {
                error = "HTTP 请求失败";
                return false;
            }
            if (string.IsNullOrEmpty(response))
            {
                error = "响应为空";
                return false;
            }

            // 错误响应预检（参考 AviReaderService.DoAviJsonTyped）
            try
            {
                var preCheck = JObject.Parse(response);
                var resultToken = preCheck["result"];
                if (resultToken != null && resultToken.Type == JTokenType.String)
                {
                    var s = resultToken.Value<string>();
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("err"))
                    {
                        LogTextHelper.Warn($"VrsHistoryService: LevelDB 返回错误响应 {s}, SN={sn}");
                        try { AlarmService.Instance.RaiseAlarm(AlarmLevel.Warning, AlarmCategory.Communication, "VrsHistoryService.LevelDB", $"LevelDB 返回错误响应 {s}", $"SN={sn}", sn); }
                        catch { }
                        error = $"LevelDB 返回错误：{s}";
                        return false;
                    }
                }
            }
            catch { /* 预检失败继续尝试解析 */ }

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var resp = JsonConvert.DeserializeObject<VRSResultResponse>(response, settings);
            rawValue = resp?.Value;
            if (string.IsNullOrEmpty(rawValue))
            {
                error = "未在响应中获取到 value 字段";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 解析 vrs_history_result 的 value 字段。
        /// value 可能是单个 JSON，也可能是分号(;)分隔的多段 JSON（多次 VRS 写入的累积快照），
        /// 每段格式：{"AsideInfo":["A_0_1_0_ok", ...],"BsideInfo":["B_1_1_0_ok", ...]}
        /// 取最后一个非空数据块（最新 VRS 快照）作为结果。
        /// </summary>
        public static VrsHistoryResult Parse(string sn, string valueJson)
        {
            if (string.IsNullOrWhiteSpace(valueJson))
                return null;

            try
            {
                var blocks = InferenceResultParser.ParseValueBlocks(valueJson);
                if (blocks == null || blocks.Count == 0)
                {
                    LogTextHelper.Warn($"VrsHistoryService.Parse 未解析到任何数据块 SN={sn}");
                    try { AlarmService.Instance.RaiseAlarm(AlarmLevel.Warning, AlarmCategory.Communication, "VrsHistoryService.Parse", "VrsHistoryService.Parse 未解析到任何数据块", $"SN={sn}", sn); }
                    catch { }
                    return null;
                }

                var latest = blocks[blocks.Count - 1];
                var result = new VrsHistoryResult { Sn = sn };

                if (latest.AsideInfo != null)
                {
                    foreach (var s in latest.AsideInfo)
                        if (!string.IsNullOrEmpty(s)) result.AsideInfo.Add(s);
                }
                if (latest.BsideInfo != null)
                {
                    foreach (var s in latest.BsideInfo)
                        if (!string.IsNullOrEmpty(s)) result.BsideInfo.Add(s);
                }

                foreach (var token in result.AsideInfo)
                {
                    if (TryParseEntry(token, out var entry))
                        result.Entries.Add(entry);
                }
                foreach (var token in result.BsideInfo)
                {
                    if (TryParseEntry(token, out var entry))
                        result.Entries.Add(entry);
                }

                return result;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"VrsHistoryService.Parse 解析异常 SN={sn}: {ex}");
                try { AlarmService.Instance.RaiseAlarm(AlarmLevel.Error, AlarmCategory.Communication, "VrsHistoryService.Parse", "VrsHistoryService.Parse 解析异常", $"SN={sn}, Error={ex.Message}", sn); }
                catch { }
                return null;
            }
        }

        /// <summary>
        /// 解析单条记录 "A_2_5_1_6257"
        /// 格式：Side_pcsIndex_defectIndex_vrs结果_扩展信息
        /// </summary>
        public static bool TryParseEntry(string token, out VrsEntry entry)
        {
            entry = null;
            if (string.IsNullOrWhiteSpace(token)) return false;

            var parts = token.Split('_');
            if (parts.Length < 4) return false;

            if (!int.TryParse(parts[1], out int pcsIndex)) return false;
            if (!int.TryParse(parts[2], out int defectIndex)) return false;
            if (!int.TryParse(parts[3], out int resultCode)) return false;

            entry = new VrsEntry
            {
                Raw = token,
                Side = parts[0],
                PcsIndex = pcsIndex,
                DefectIndex = defectIndex,
                ResultCode = resultCode,
                Result = ToResultCode(resultCode),
                Extra = parts.Length > 4 ? string.Join("_", parts.Skip(4)) : null
            };
            return true;
        }

        private static VrsResultCode ToResultCode(int code)
        {
            switch (code)
            {
                case 0: return VrsResultCode.Ok;
                case 1: return VrsResultCode.Ng;
                case 2: return VrsResultCode.Ignore;
                case 3: return VrsResultCode.NoResult;
                case 4: return VrsResultCode.NotAcceptNg;
                default: return VrsResultCode.NoResult;
            }
        }

        /// <summary>
        /// 将 VRS 原始结果码（0-4）映射为 DetectInfo.VrsState 编码。
        /// 约定：0=未判定, 1=OK, 2=NG, 3=忽略, 4=无结果, 5=NG不接收。
        /// </summary>
        public static int MapResultCodeToVrsState(int resultCode)
        {
            switch (resultCode)
            {
                case 0: return 1; // ok
                case 1: return 2; // ng
                case 2: return 3; // ignore
                case 3: return 4; // noresult
                case 4: return 5; // notAcceptNg
                default: return 0;
            }
        }
    }
}

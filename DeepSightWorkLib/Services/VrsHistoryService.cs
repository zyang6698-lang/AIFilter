using DeepSightCommunication;
using DeepSightDB;
using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 从 LevelDB 的 vrs_history_result 表按 SN 读取并解析 VRS 历史结果。
    /// 遍历所有启用的 LevelDbConfig，命中即返回。
    /// </summary>
    public class VrsHistoryService
    {
        private const string DbName = "vrs_history_result";

        private readonly HttpClass _httpDb;

        public VrsHistoryService(HttpClass httpDb)
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
                LogTextHelper.Warn("VrsHistoryService: 没有启用的 LevelDB 配置");
                return false;
            }

            foreach (var config in configs)
            {
                try
                {
                    if (TryQuery(config.Url, sn, out string rawValue))
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
                    LogTextHelper.Error($"VrsHistoryService 查询 SN={sn} DB={config.DbName} 异常: {ex}");
                }
            }

            return false;
        }

        /// <summary>
        /// 向指定 URL 的 LevelDB 发起 get 请求，解析响应并返回 value 字符串
        /// </summary>
        private bool TryQuery(string url, string sn, out string rawValue)
        {
            rawValue = null;

            var req = new RootDbInfo
            {
                uniqueKey = Guid.NewGuid().ToString(),
                db_name = DbName,
                operation = "get",
                is_select_range = "false",
                op_mode = "all",
                key = sn
            };

            if (!_httpDb.HttpPostMethod(url, req, 0, out string response))
                return false;
            if (string.IsNullOrEmpty(response))
                return false;

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

            var resp = JsonConvert.DeserializeObject<AviResponse>(response, settings);
            if (resp?.DataList == null || resp.DataList.Count == 0)
                return false;

            foreach (var item in resp.DataList)
            {
                string valueStr = null;
                if (item is string str)
                {
                    if (str == "end_range_send") continue;
                    try
                    {
                        var di = JsonConvert.DeserializeObject<AviDataItem>(str, settings);
                        valueStr = di?.Value;
                    }
                    catch { }
                }
                else if (item is JObject jo)
                {
                    valueStr = jo["value"]?.ToString();
                }

                if (!string.IsNullOrEmpty(valueStr))
                {
                    rawValue = valueStr;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 解析 vrs_history_result 的 value JSON。
        /// 示例：{"AsideInfo":["A_0_1_0_ok", ...],"BsideInfo":["B_1_1_0_ok"]}
        /// </summary>
        public static VrsHistoryResult Parse(string sn, string valueJson)
        {
            if (string.IsNullOrWhiteSpace(valueJson))
                return null;

            try
            {
                var jo = JObject.Parse(valueJson);
                var result = new VrsHistoryResult { Sn = sn };

                CollectTokens(jo["AsideInfo"], result.AsideInfo);
                CollectTokens(jo["BsideInfo"], result.BsideInfo);

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
                return null;
            }
        }

        private static void CollectTokens(JToken arr, List<string> target)
        {
            if (arr == null || arr.Type != JTokenType.Array) return;
            foreach (var t in arr)
            {
                if (t.Type == JTokenType.String)
                {
                    var s = t.Value<string>();
                    if (!string.IsNullOrEmpty(s)) target.Add(s);
                }
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

using DeepSightCommunication;
using DeepSightDB;
using DeepSightModel;
using DeepSightModel.Configuration;
using DeepSightTool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 推理请求生成服务：封装两种生成方式的核心业务逻辑
    /// 1) 按 MinIO 目录扫描 panel.json，按 SN 分组 A/B 面，写入 ai_merged_results
    /// 2) 按 Lot 批次：读取 lot_panel 建立 lot→SN 映射，遍历 SN 从 AVI_results_db 读取
    /// </summary>
    public class InferenceRequestService
    {
        private readonly HttpClass _http;

        public InferenceRequestService(HttpClass http = null)
        {
            _http = http ?? new HttpClass();
        }

        #region 按 MinIO 目录方式

        /// <summary>
        /// 扫描 MinIO 指定前缀下的 *-panel.json，解析后按 SN 分组（{ SN -> { "A": path, "B": path } }）
        /// </summary>
        public async Task<Dictionary<string, Dictionary<string, string>>> ScanAndGroupPanelsAsync(
            MinioClass minio, string bucket, string prefix, string ip,
            Action<int, int> onParseProgress = null)
        {
            if (minio == null) throw new ArgumentNullException(nameof(minio));
            var snGroups = new Dictionary<string, Dictionary<string, string>>();

            var allKeys = await minio.ListAllObjectKeysAsync(bucket, prefix, ip);
            var panelFiles = allKeys
                .Where(k => !string.IsNullOrEmpty(k) && k.EndsWith("-panel.json", StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (panelFiles.Count == 0) return snGroups;

            int parsed = 0;
            foreach (var objectKey in panelFiles)
            {
                try
                {
                    string json = minio.ReadJsonSync(bucket, objectKey, ip);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var panelInfo = JsonConvert.DeserializeObject<RootPanelInfo>(json);
                        if (panelInfo != null && !string.IsNullOrEmpty(panelInfo.SerialNumber))
                        {
                            string sn = panelInfo.SerialNumber;
                            string side = panelInfo.SideIndex?.ToUpper() ?? "";
                            if (side == "0") side = "A";
                            else if (side == "1") side = "B";
                            if (side == "A" || side == "B")
                            {
                                if (!snGroups.ContainsKey(sn))
                                    snGroups[sn] = new Dictionary<string, string>();
                                snGroups[sn][side] = $"{bucket}/{objectKey}";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Warn($"解析 panel.json 失败: {objectKey}, 错误: {ex.Message}");
                }
                parsed++;
                onParseProgress?.Invoke(parsed, panelFiles.Count);
            }
            return snGroups;
        }

        /// <summary>
        /// 基于已分组的 SN→A/B 面字典，向所有启用 LevelDB 配置发送 put 推理请求（ai_merged_results）
        /// </summary>
        public async Task<(int success, int failed)> SendMergedInferenceRequestsAsync(
            IList<LevelDbConfig> dbConfigs,
            Dictionary<string, Dictionary<string, string>> snGroups,
            string selectedIp, string minioPort, int delayMsBetween = 500,
            Action<int, int, int, int> onProgress = null)
        {
            int sent = 0, success = 0, failed = 0;
            int total = snGroups?.Count ?? 0;
            if (total == 0 || dbConfigs == null || dbConfigs.Count == 0)
                return (success, failed);

            foreach (var entry in snGroups)
            {
                string sn = entry.Key;
                bool hasA = entry.Value.ContainsKey("A");
                bool hasB = entry.Value.ContainsKey("B");
                foreach (var dbConfig in dbConfigs)
                {
                    try
                    {
                        var resultsInfo = new List<object>();
                        if (hasA)
                            resultsInfo.Add(new { side = "A", minio_ip = selectedIp, minio_port = minioPort, result_path = entry.Value["A"] });
                        if (hasB)
                            resultsInfo.Add(new { side = "B", minio_ip = selectedIp, minio_port = minioPort, result_path = entry.Value["B"] });

                        var valueObj = new { serial_number = sn, results_info = resultsInfo };
                        string valueStr = JsonConvert.SerializeObject(valueObj);
                        string timeKey = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        var dbInfo = new RootDbInfo
                        {
                            uniqueKey = Guid.NewGuid().ToString(),
                            db_name = "ai_merged_results",
                            operation = "put",
                            op_mode = "all_ow",
                            key = timeKey,
                            value = valueStr
                        };

                        if (_http.HttpPostMethod(dbConfig.Url, dbInfo, 1, out string _))
                        {
                            success++;
                            LogTextHelper.Info($"推理请求发送成功: SN={sn}, DB={dbConfig.DisplayName}");
                        }
                        else
                        {
                            failed++;
                            LogTextHelper.Warn($"推理请求发送失败: SN={sn}, DB={dbConfig.DisplayName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        LogTextHelper.Error($"推理请求发送异常: SN={sn}, 错误: {ex.Message}");
                    }
                    if (delayMsBetween > 0) await Task.Delay(delayMsBetween);
                }
                sent++;
                onProgress?.Invoke(sent, total, success, failed);
            }
            return (success, failed);
        }

        #endregion

        #region 按 Lot 批次方式

        /// <summary>
        /// 读取 lot_panel 全部 lot 列表（op_mode=prefix_keys，仅返回 key），
        /// 响应中 value 字段为形如 "[{\"key\":\"xxx\",\"value\":\"\"}]" 的 JSON 字符串，
        /// 解析后以 key 作为 lot 名称，返回 lot→空SN列表 的映射
        /// </summary>
        public bool TryReadLotToSnMapping(LevelDbConfig config,
            out Dictionary<string, List<string>> lotToSns, out string errorMessage)
        {
            lotToSns = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            errorMessage = null;
            if (config == null) { errorMessage = "config 为空"; return false; }

            var req = new RootDbInfo
            {
                uniqueKey = Guid.NewGuid().ToString(),
                db_name = "lot_panel",
                operation = "list",
                op_mode = "prefix_keys"
            };
            if (!_http.HttpPostMethod(config.Url, req, 0, out string rawResp) || string.IsNullOrWhiteSpace(rawResp))
            {
                errorMessage = "读取 lot_panel 数据库失败";
                return false;
            }

            try
            {
                var root = JObject.Parse(rawResp);
                var valueToken = root["value"];
                if (valueToken == null || valueToken.Type == JTokenType.Null)
                    return true;

                string valueStr = valueToken.Type == JTokenType.String
                    ? (string)valueToken
                    : valueToken.ToString(Formatting.None);
                if (string.IsNullOrWhiteSpace(valueStr)) return true;

                var items = JsonConvert.DeserializeObject<List<AviDataItem>>(valueStr);
                if (items == null) return true;

                foreach (var it in items)
                {
                    if (it == null || string.IsNullOrEmpty(it.Key)) continue;
                    string lot = it.Key.Trim();
                    if (string.IsNullOrEmpty(lot)) continue;
                    if (!lotToSns.ContainsKey(lot))
                        lotToSns[lot] = new List<string>();
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"解析 lot_panel 响应失败: {ex.Message}";
                LogTextHelper.Warn(errorMessage);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 按 lot 读取该 lot 下的 SN 列表（operation=get, key=lot）。
        /// 响应中 value 字段优先按 JSON 数组解析（每项形如 {"key":..,"value":"SN;timestamp;lot"}），
        /// 否则按整体字符串/逐行的 "SN;timestamp;lot" 兜底解析。
        /// </summary>
        public async Task<List<string>> FetchSnsByLotAsync(LevelDbConfig config, string lot)
        {
            var sns = new List<string>();
            if (config == null || string.IsNullOrEmpty(lot)) return sns;

            var req = new RootDbInfo
            {
                uniqueKey = Guid.NewGuid().ToString(),
                db_name = "lot_panel",
                operation = "get",
                op_mode = "all",
                key = lot
            };

            string rawResp = string.Empty;
            bool ok = await Task.Run(() => _http.HttpPostMethod(config.Url, req, 0, out rawResp));
            if (!ok || string.IsNullOrWhiteSpace(rawResp))
            {
                LogTextHelper.Warn($"按 lot 读取 SN 失败: lot={lot}");
                return sns;
            }

            try
            {
                var root = JObject.Parse(rawResp);
                var valueToken = root["value"];
                if (valueToken == null || valueToken.Type == JTokenType.Null) return sns;

                string valueStr = valueToken.Type == JTokenType.String
                    ? (string)valueToken
                    : valueToken.ToString(Formatting.None);
                if (string.IsNullOrWhiteSpace(valueStr)) return sns;

                if (valueStr.TrimStart().StartsWith("["))
                {
                    try
                    {
                        var items = JsonConvert.DeserializeObject<List<AviDataItem>>(valueStr);
                        if (items != null)
                        {
                            foreach (var it in items) ExtractAndAddSn(it?.Value, sns);
                            return sns;
                        }
                    }
                    catch { }
                }

                var lines = valueStr.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0) ExtractAndAddSn(valueStr, sns);
                else foreach (var line in lines) ExtractAndAddSn(line, sns);
            }
            catch (Exception ex)
            {
                LogTextHelper.Warn($"解析 lot SN 响应失败: lot={lot}, 错误: {ex.Message}");
            }
            return sns;
        }

        private static void ExtractAndAddSn(string record, List<string> sns)
        {
            if (string.IsNullOrEmpty(record)) return;
            var parts = record.Split(';');
            if (parts.Length == 0) return;
            string sn = parts[0].Trim();
            if (string.IsNullOrEmpty(sn)) return;
            if (!sns.Contains(sn)) sns.Add(sn);
        }

        /// <summary>
        /// 遍历 SN，从 AVI_results_db 按 key 读取对应 value，返回 SN→原始响应字符串
        /// </summary>
        public async Task<Dictionary<string, string>> FetchAviResultsBySnsAsync(
            LevelDbConfig config, IList<string> sns,
            Action<int, int, int, int> onProgress = null)
        {
            var snValues = new Dictionary<string, string>(StringComparer.Ordinal);
            if (config == null || sns == null || sns.Count == 0) return snValues;

            int idx = 0, succ = 0, fail = 0;
            foreach (var sn in sns)
            {
                try
                {
                    var req = new RootDbInfo
                    {
                        uniqueKey = Guid.NewGuid().ToString(),
                        db_name = "AVI_results_db",
                        operation = "get",
                        key = sn
                    };
                    string snResp = string.Empty;
                    bool ok = await Task.Run(() =>
                    {
                        bool r = _http.HttpPostMethod(config.Url, req, 0, out string resp);
                        snResp = resp;
                        return r;
                    });
                    if (ok && !string.IsNullOrEmpty(snResp))
                    {
                        snValues[sn] = snResp;
                        succ++;
                        LogTextHelper.Info($"AVI_results_db 读取成功: SN={sn}");
                    }
                    else
                    {
                        fail++;
                        LogTextHelper.Warn($"AVI_results_db 读取失败: SN={sn}");
                    }
                }
                catch (Exception ex)
                {
                    fail++;
                    LogTextHelper.Error($"AVI_results_db 读取异常 SN={sn}: {ex.Message}");
                }
                idx++;
                onProgress?.Invoke(idx, sns.Count, succ, fail);
            }
            return snValues;
        }

        #endregion
    }
}


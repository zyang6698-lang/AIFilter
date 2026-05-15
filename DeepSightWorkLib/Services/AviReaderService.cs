using DeepSightCommunication;
using DeepSightDB;
using DeepSightEvent;
using DeepSightModel;
using DeepSightModel.Configuration;
using DeepSightTool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 负责从 LevelDB 获取 AVI 消息并解析为强类型 AviDataItem 的服务（聚合/解析层）
    /// 支持多个 LevelDB 数据库，每个数据库维护独立的 fetchTime。
    /// </summary>
    public class AviReaderService
    {
        private readonly LevelDbHttpClient _httpDb;
        private const string FixedTimeFormat = "yyyyMMddHHmmssfff";
        private readonly ConcurrentDictionary<string, DateTime> _processingSnSet;
        // delegate to call ReadJsonByMinio implemented elsewhere (BusinessClass)
        private readonly Action<AviProcessingContext> _readJsonByMinio;

        /// <summary>
        /// 每个数据库源的运行时 fetchTime（key 为 Url|DbName，value 为上次获取的时间）
        /// 避免多个现场使用相同 DbName 时互相覆盖读取游标。
        /// </summary>
        private readonly ConcurrentDictionary<string, DateTime> _fetchTimeByDb = new ConcurrentDictionary<string, DateTime>();

        public AviReaderService(LevelDbHttpClient httpDb, ConcurrentDictionary<string, DateTime> processingSnSet, Action<AviProcessingContext> readJsonByMinio)
        {
            _httpDb = httpDb ?? throw new ArgumentNullException(nameof(httpDb));
            _processingSnSet = processingSnSet ?? throw new ArgumentNullException(nameof(processingSnSet));
            _readJsonByMinio = readJsonByMinio ?? throw new ArgumentNullException(nameof(readJsonByMinio));

            // 启动时从 LevelDbConfig 加载持久化的 fetchTime
            LoadFetchTimesFromLevelDbConfig();
        }

        private static string BuildFetchTimeKey(LevelDbConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return $"{config.Url}|{config.DbName}";
        }

        /// <summary>
        /// 获取指定数据库源的 fetchTime
        /// </summary>
        public DateTime GetFetchTime(LevelDbConfig config)
        {
            return _fetchTimeByDb.GetOrAdd(BuildFetchTimeKey(config), DateTime.MinValue);
        }

        /// <summary>
        /// 设置指定数据库源的 fetchTime，同步更新内存和 LevelDbConfig 持久化
        /// </summary>
        public void SetFetchTime(LevelDbConfig config, DateTime time)
        {
            _fetchTimeByDb[BuildFetchTimeKey(config)] = time;
            config.LastFetchTime = time;
            SaveLevelDbConfig();
        }

        /// <summary>
        /// 重置所有数据库的 fetchTime 为当前时间，并持久化到 LevelDbConfig
        /// </summary>
        public void ResetAllFetchTimes()
        {
            var now = DateTime.Now;
            var configs = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled)
                .ToList();

            foreach (var config in configs)
            {
                _fetchTimeByDb[BuildFetchTimeKey(config)] = now;
                config.LastFetchTime = now;
            }

            SaveLevelDbConfig();
            LogTextHelper.Info($"已重置 {configs.Count} 个数据库源的 fetchTime 为: {now:yyyy-MM-dd HH:mm:ss.fff}");
        }

        /// <summary>
        /// 从 LevelDbConfig 加载持久化的 fetchTime（不重置为当前时间），用于启动时恢复上次游标
        /// </summary>
        public void LoadPersistedFetchTimes()
        {
            LoadFetchTimesFromLevelDbConfig();

            var configs = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled)
                .ToList();

            foreach (var config in configs)
            {
                var time = GetFetchTime(config);
                LogTextHelper.Info($"数据库 {config.DbName} 加载持久化 fetchTime: {time:yyyy-MM-dd HH:mm:ss.fff}");
            }
        }

        /// <summary>
        /// 获取所有启用数据库中最早的 fetchTime（用于判断是否有未处理的时间段）
        /// </summary>
        public DateTime GetEarliestFetchTime()
        {
            var configs = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled)
                .ToList();

            if (configs.Count == 0) return DateTime.Now;

            DateTime earliest = DateTime.MaxValue;
            foreach (var config in configs)
            {
                var time = GetFetchTime(config);
                if (time < earliest) earliest = time;
            }

            return earliest == DateTime.MaxValue ? DateTime.Now : earliest;
        }

        #region FetchTime 持久化（存储在 LevelDbConfig 中）

        /// <summary>
        /// 从 LevelDbConfig 的 LastFetchTime 字段加载 fetchTime 到内存字典
        /// </summary>
        private void LoadFetchTimesFromLevelDbConfig()
        {
            try
            {
                var configs = LevelDbConfigManager.Instance.Databases;
                int loaded = 0;
                foreach (var config in configs)
                {
                    if (config.LastFetchTime.HasValue)
                    {
                        _fetchTimeByDb[BuildFetchTimeKey(config)] = config.LastFetchTime.Value;
                        loaded++;
                    }
                }

                if (loaded > 0)
                    LogTextHelper.Info($"从 LevelDB 配置加载了 {loaded} 个 fetchTime 记录");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"加载 fetchTime 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 将 LevelDbConfig 保存到文件（持久化 LastFetchTime）
        /// </summary>
        private static void SaveLevelDbConfig()
        {
            try
            {
                LevelDbConfigManager.Save(LevelDbConfigManager.Instance);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"保存 LevelDB 配置失败: {ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// 检查所有启用数据库中 fetchTime 到当前时间之间是否存在待处理的推理请求。
        /// 返回各数据库的待处理条数汇总。
        /// </summary>
        public int CheckPendingDataCount()
        {
            var configs = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled)
                .ToList();

            if (configs.Count == 0) return 0;

            int totalCount = 0;
            foreach (var config in configs)
            {
                try
                {
                    if (ReadAVI(config, out string result))
                    {
                        int count = CountAviDataItems(result);
                        if (count > 0)
                        {
                            LogTextHelper.Info($"数据库 {config.DbName} 有 {count} 条待处理数据 (fetchTime={GetFetchTime(config):yyyy-MM-dd HH:mm:ss})");
                            totalCount += count;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"检查数据库 {config.DbName} 待处理数据失败: {ex.Message}");
                }
            }

            return totalCount;
        }

        /// <summary>
        /// 检查所有启用数据库中 fetchTime 到当前时间之间是否存在待处理的推理请求。
        /// 返回每个数据库的详细信息（数据库显示名、fetchTime、待处理条数）。
        /// </summary>
        public List<PendingDataInfo> CheckPendingDataCountPerDb()
        {
            var result = new List<PendingDataInfo>();
            var configs = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled)
                .ToList();

            if (configs.Count == 0) return result;

            foreach (var config in configs)
            {
                try
                {
                    var fetchTime = GetFetchTime(config);
                    if (fetchTime <= DateTime.MinValue) continue;

                    if (ReadAVI(config, out string data))
                    {
                        int count = CountAviDataItems(data);
                        if (count > 0)
                        {
                            LogTextHelper.Info($"数据库 {config.DbName} 有 {count} 条待处理数据 (fetchTime={fetchTime:yyyy-MM-dd HH:mm:ss})");
                            result.Add(new PendingDataInfo
                            {
                                DisplayName = config.DisplayName,
                                FetchTime = fetchTime,
                                PendingCount = count
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"检查数据库 {config.DbName} 待处理数据失败: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// 从所有启用数据库读取待处理数据，解析并构建 AviProcessingContext，
        /// 但不调用 _readJsonByMinio（即不发送推理请求）。
        /// 同时更新 fetchTime，防止后续 WorkerReadAVI 重复读取这批数据。
        /// </summary>
        /// <returns>各数据库详情 + 暂存的推理请求上下文列表</returns>
        public PreparedPendingData ReadAndHoldAllAVI()
        {
            var prepared = new PreparedPendingData();
            var configs = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled)
                .ToList();

            if (configs.Count == 0) return prepared;

            foreach (var config in configs)
            {
                try
                {
                    var fetchTime = GetFetchTime(config);
                    if (fetchTime <= DateTime.MinValue) continue;

                    if (ReadAVI(config, out string data))
                    {
                        var holdList = new List<AviProcessingContext>();
                        DoAviJsonTyped(data, config, holdList);

                        if (holdList.Count > 0)
                        {
                            LogTextHelper.Info($"数据库 {config.DbName} 读取到 {holdList.Count} 条待处理数据 (fetchTime={fetchTime:yyyy-MM-dd HH:mm:ss})");
                            prepared.DbInfos.Add(new PendingDataInfo
                            {
                                DisplayName = config.DisplayName,
                                FetchTime = fetchTime,
                                PendingCount = holdList.Count
                            });
                            prepared.HeldContexts.AddRange(holdList);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"读取数据库 {config.DbName} 待处理数据失败: {ex.Message}");
                }
            }

            return prepared;
        }

        /// <summary>
        /// 将暂存的推理请求上下文逐条发送（调用 _readJsonByMinio 委托）
        /// </summary>
        public void ReleaseHeldContext(AviProcessingContext ctx)
        {
            string snKey = $"{ctx.SerialNumber}_{ctx.Side}";
            // 标记为处理中
            if (!_processingSnSet.TryAdd(snKey, DateTime.Now))
            {
                LogTextHelper.Warn($"SN:{ctx.SerialNumber} Side:{ctx.Side} 已在处理中，跳过");
                return;
            }
            try
            {
                _readJsonByMinio(ctx);
                LogTextHelper.Info($"SN:{ctx.SerialNumber} Side:{ctx.Side} 历史数据已释放到处理管线");
            }
            catch (Exception ex)
            {
                _processingSnSet.TryRemove(snKey, out _);
                LogTextHelper.Error($"SN:{ctx.SerialNumber} Side:{ctx.Side} 释放历史数据异常: {ex}");
            }
        }

        /// <summary>
        /// 统计 AVI JSON 响应中的数据条目数（不做实际处理）
        /// </summary>
        private int CountAviDataItems(string jsonInfo)
        {
            if (string.IsNullOrEmpty(jsonInfo)) return 0;

            try
            {
                // 前置检查：错误响应
                try
                {
                    var preCheck = JObject.Parse(jsonInfo);
                    var resultToken = preCheck["result"];
                    if (resultToken != null && resultToken.Type == JTokenType.String)
                    {
                        var resultStr = resultToken.Value<string>();
                        if (!string.IsNullOrEmpty(resultStr) && resultStr.StartsWith("err"))
                            return 0;
                    }
                }
                catch { }

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var response = JsonConvert.DeserializeObject<AviResponse>(jsonInfo, settings);
                if (response?.DataList == null || response.DataList.Count <= 0)
                    return 0;

                int count = 0;
                foreach (var item in response.DataList)
                {
                    if (item is string strItem && strItem == "end_range_send")
                        continue;
                    count++;
                }
                return count;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"统计AVI数据条目异常: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 从所有配置的 LevelDB 数据库读取 AVI 数据
        /// </summary>
        /// <returns>是否至少有一个数据库读取成功</returns>
        public bool ReadAllAVI()
        {
            var configs = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled)
                .ToList();

            if (configs.Count == 0)
            {
                LogTextHelper.Warn("没有配置启用的 LevelDB 数据库");
                return false;
            }

            bool anySuccess = false;
            foreach (var config in configs)
            {
                try
                {
                    if (ReadAVI(config, out string result))
                    {
                        DoAviJsonTyped(result, config);
                        anySuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"读取数据库 {config.DbName} 失败: {ex}");
                }
            }

            return anySuccess;
        }

        /// <summary>
        /// 从指定配置的 LevelDB 读取 AVI JSON
        /// </summary>
        public bool ReadAVI(LevelDbConfig config, out string result)
        {
            var dbFetchTime = GetFetchTime(config);
            string rangeStart = dbFetchTime.ToString(FixedTimeFormat);
            string rangeEnd = DateTime.Now.Date.AddDays(1).AddTicks(-1).ToString(FixedTimeFormat);
            RootDbInfo getInfo = new RootDbInfo
            {
                uniqueKey = Guid.NewGuid().ToString(),
                db_name = config.DbName,
                operation = "get",
                is_select_range = "true",
                op_mode = "all",
                range_start = rangeStart,
                range_end = rangeEnd,
            };
            bool success = _httpDb.PostJson(config.Url, getInfo, LevelDbOperation.Read, out result, "AVI读取");
            if (!success)
            {
                LogTextHelper.Warn($"AVI读取失败: DbName={config.DbName}, Url={config.Url}, RangeStart={rangeStart}, RangeEnd={rangeEnd}");
            }
            return success;
        }

        /// <summary>
        /// 将传入的 AVI JSON 解析为 AviDataItem，并对每个数据项调用回调处理。
        /// 该方法不做后续业务处理，仅负责解析与类型转换。
        /// </summary>
        /// <param name="jsonInfo">AVI JSON 字符串</param>
        /// <param name="config">当前正在处理的 LevelDB 配置</param>
        /// <param name="holdList">若非 null，则不调用 _readJsonByMinio，而是将上下文收集到此列表（暂存模式）</param>
        public void DoAviJsonTyped(string jsonInfo, LevelDbConfig config, List<AviProcessingContext> holdList = null)
        {
            if (string.IsNullOrEmpty(jsonInfo) )
                return;

            try
            {
                // 前置检查：如果响应为错误结果（如 err_key_found），直接跳过，避免反序列化异常
                try
                {
                    var preCheck = JObject.Parse(jsonInfo);
                    var resultToken = preCheck["result"];
                    if (resultToken != null && resultToken.Type == JTokenType.String)
                    {
                        var resultStr = resultToken.Value<string>();
                        if (!string.IsNullOrEmpty(resultStr) && resultStr.StartsWith("err"))
                        {
                            LogTextHelper.Warn($"LevelDB返回错误响应: {resultStr}，跳过处理");
                            return;
                        }
                    }
                }
                catch { /* 预检失败不影响后续正常解析 */ }

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var response = JsonConvert.DeserializeObject<AviResponse>(jsonInfo, settings);
                if (response?.DataList == null || response.DataList.Count <= 0)
                {
                    return;
                }

                foreach (var item in response.DataList)
                {
                    try
                    {
                        if (item is string strItem)
                        {
                            if (strItem == "end_range_send")
                                continue;

                            var dataItem = JsonConvert.DeserializeObject<AviDataItem>(strItem, settings);
                            if (dataItem == null) continue;
                            ProcessAviDataItem(dataItem, settings, config, holdList);
                        }
                        else if (item is JObject jObj)
                        {
                            var dataItem = jObj.ToObject<AviDataItem>();
                            if (dataItem == null) continue;
                            ProcessAviDataItem(dataItem, settings, config, holdList);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"处理单个AVI数据项异常: {ex}");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"处理AVI_JSON异常: {ex}");
            }
        }

        /// <summary>
        /// 解析 Minio 路径（与原 BusinessClass.ParseMinioPath 相同），返回 bucket/object 与 path两个部分
        /// </summary>
        public static void ParseMinioPath(string fullPath, out string path, out string result)
        {
            try
            {
                string normalizedPath = fullPath.Replace('\\', '/');
                string[] parts = normalizedPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                int minioIndex = Array.IndexOf(parts, "deepiresults");

                if (minioIndex == -1)
                {
                    throw new ArgumentException("路径中未包含 'minio' 目录");
                }

                if (parts.Length < minioIndex + 3)
                {
                    throw new ArgumentException("路径不完整，缺少存储桶或对象键");
                }
                string bucketName = parts[minioIndex + 1];
                string objectKey = string.Join("/", parts, minioIndex + 2, parts.Length - minioIndex - 2);

                string str = string.Join("/", parts, minioIndex + 1, parts.Length - minioIndex - 2);
                path = $"{bucketName}/{objectKey}";
                result = str;
            }
            catch (Exception ex)
            {
                path = string.Empty;
                result = string.Empty;
                LogTextHelper.Error(ex.ToString());
            }
        }




        /// <summary>
        /// 处理单个 AVI 数据项。
        /// holdList 为 null 时：正常模式，标记 _processingSnSet 并调用 _readJsonByMinio。
        /// holdList 非 null 时：暂存模式，只解析构建 AviProcessingContext 并加入 holdList，不标记、不发送。
        /// </summary>
        private void ProcessAviDataItem(AviDataItem dataItem, JsonSerializerSettings settings, LevelDbConfig config, List<AviProcessingContext> holdList = null)
        {
            LogTextHelper.Info($"开始处理AVI数据项");
            if (string.IsNullOrEmpty(dataItem.Key) || string.IsNullOrEmpty(dataItem.Value))
                return;

            if (DateTime.TryParseExact(
             dataItem.Key,
            format: FixedTimeFormat,
            provider: CultureInfo.InvariantCulture,
            style: DateTimeStyles.None,
            result: out DateTime dt))
            {
                // 更新当前数据库的 fetchTime
                SetFetchTime(config, dt.AddMilliseconds(1));
            }
            // 第二层：反序列化 value 字符串
            var valueData = JsonConvert.DeserializeObject<AviValueData>(dataItem.Value, settings);
            if (valueData?.ResultsInfo == null || valueData.ResultsInfo.Count == 0)
                return;

            string serialNumber = valueData.SerialNumber;
            if (string.IsNullOrEmpty(serialNumber))
                return;

            LogTextHelper.Info($"获取到{serialNumber}的数据");

            // 缓存原始LevelDB JSON数据用于调试显示
            string rawLevelDbJson = dataItem.Value;

            // 遍历 results_info
            foreach (var resultInfo in valueData.ResultsInfo)
            {
                if (resultInfo == null) continue;

                string side = resultInfo.Side;
                string snKey = $"{serialNumber}_{side}";

                // ── 正常模式：检查/标记 _processingSnSet ──
                if (holdList == null)
                {
                    if (_processingSnSet.ContainsKey(snKey))
                    {
                        LogTextHelper.Warn($"SN:{serialNumber} Side:{side} 正在处理中，跳过重复请求");
                        continue;
                    }
                    if (!_processingSnSet.TryAdd(snKey, DateTime.Now))
                    {
                        LogTextHelper.Warn($"SN:{serialNumber} Side:{side} 添加到处理集合失败，可能已被其他线程处理");
                        continue;
                    }
                    LogTextHelper.Info($"SN:{serialNumber} Side:{side} 已标记为处理中，当前处理集合大小：{_processingSnSet.Count}");
                }

                // ── 解析 MinIO IP / Port ──
                string minioIp;
                string minioPort;
                if (string.Equals(side, "A", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(config.MinioIpA))
                {
                    minioIp = config.MinioIpA;
                    minioPort = MinioSettings.Instance.DefaultPort;
                }
                else if (string.Equals(side, "B", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(config.MinioIpB))
                {
                    minioIp = config.MinioIpB;
                    minioPort = MinioSettings.Instance.DefaultPort;
                }
                else
                {
                    minioIp = resultInfo.MinioIp;
                    minioPort = resultInfo.MinioPort.ToString();
                }

                if (string.IsNullOrEmpty(minioIp) || string.IsNullOrEmpty(minioPort) || minioPort == "0")
                {
                    if (holdList == null) _processingSnSet.TryRemove(snKey, out _);
                    Thread.Sleep(500);
                    TaskStatusSender.SendFailed(serialNumber, side, "Minio格式错误");
                    SystemEvent.SendAlarmMsg($"SN:{serialNumber} {side}面 Minio格式错误;具体信息 MinioIP:{minioIp} MinioPort:{minioPort}");
                    continue;
                }

                string resultPath = resultInfo.ResultPath;
                if (string.IsNullOrEmpty(resultPath))
                {
                    if (holdList == null) _processingSnSet.TryRemove(snKey, out _);
                    continue;
                }

                try
                {
                    ParseMinioPath(resultPath, out string path, out string result);
                    var writeBackDbName = config.WriteBackDbName ?? "filter_time_to_airesults";
                    var vrsWriteBackDbName = config.VRSWriteBackDbName ?? "ai_detail_results_tovrs";
                    var dbUrl = config.Url ?? "";
                    var vrsDbUrl = config.VRSUrl ?? dbUrl;

                    // 存储原始LevelDB JSON到调试缓存
                    var debugInfo = SnDebugInfoCache.GetOrCreate(serialNumber, side);
                    debugInfo.RawLevelDbJson = rawLevelDbJson;
                    debugInfo.SourceDbName = config.DbName;
                    debugInfo.SourceDbUrl = dbUrl;
                    debugInfo.SourceVRSDbUrl = vrsDbUrl;

                    var processingContext = new AviProcessingContext
                    {
                        MinioIp = minioIp,
                        MinioPort = minioPort,
                        Key = dataItem.Key,
                        Head = result,
                        SerialNumber = serialNumber,
                        Side = side,
                        MinioPath = path,
                        WriteBackDbName = writeBackDbName,
                        DbUrl = dbUrl,
                        VrsWriteBackDbName = vrsWriteBackDbName,
                        VrsDbUrl = vrsDbUrl
                    };

                    if (holdList != null)
                    {
                        // 暂存模式：只收集，不发送
                        holdList.Add(processingContext);
                    }
                    else
                    {
                        // 正常模式：立即发送
                        _readJsonByMinio(processingContext);
                        LogTextHelper.Info($"SN:{serialNumber} Side:{side} 通过Minio读取Json完成, AVI回写DB:{writeBackDbName}@{dbUrl}, VRS回写DB:{vrsWriteBackDbName}@{vrsDbUrl}");
                    }
                }
                catch (Exception ex)
                {
                    if (holdList == null) _processingSnSet.TryRemove(snKey, out _);
                    LogTextHelper.Error($"SN:{serialNumber} Side:{side} 处理异常: {ex}");
                }
            }
        }

    }

    /// <summary>
    /// 单个数据库的待处理数据信息
    /// </summary>
    public class PendingDataInfo
    {
        /// <summary>数据库显示名称</summary>
        public string DisplayName { get; set; }

        /// <summary>上次读取时间</summary>
        public DateTime FetchTime { get; set; }

        /// <summary>待处理条数</summary>
        public int PendingCount { get; set; }
    }

    /// <summary>
    /// ReadAndHoldAllAVI 的返回结果：各数据库统计 + 暂存的推理请求上下文
    /// </summary>
    public class PreparedPendingData
    {
        /// <summary>各数据库的待处理信息</summary>
        public List<PendingDataInfo> DbInfos { get; set; } = new List<PendingDataInfo>();

        /// <summary>已解析但未发送的推理请求上下文</summary>
        public List<AviProcessingContext> HeldContexts { get; set; } = new List<AviProcessingContext>();

        /// <summary>是否有暂存数据</summary>
        public bool HasData => HeldContexts.Count > 0;
    }
}

using DeepSightCommunication;
using DeepSightDB;
using DeepSightEvent;
using DeepSightModel;
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
        private readonly HttpClass _httpDb;
        private const string FixedTimeFormat = "yyyyMMddHHmmssfff";
        private readonly ConcurrentDictionary<string, DateTime> _processingSnSet;
        // delegate to call ReadJsonByMinio implemented elsewhere (BusinessClass)
        private readonly Action<AviProcessingContext> _readJsonByMinio;

        /// <summary>
        /// 每个数据库的 fetchTime（key 为 DbName，value 为上次获取的时间）
        /// </summary>
        private readonly ConcurrentDictionary<string, DateTime> _fetchTimeByDb = new ConcurrentDictionary<string, DateTime>();

        public AviReaderService(HttpClass httpDb, ConcurrentDictionary<string, DateTime> processingSnSet, Action<AviProcessingContext> readJsonByMinio)
        {
            _httpDb = httpDb ?? throw new ArgumentNullException(nameof(httpDb));
            _processingSnSet = processingSnSet ?? throw new ArgumentNullException(nameof(processingSnSet));
            _readJsonByMinio = readJsonByMinio ?? throw new ArgumentNullException(nameof(readJsonByMinio));
        }

        /// <summary>
        /// 获取指定数据库的 fetchTime
        /// </summary>
        public DateTime GetFetchTime(string dbName)
        {
            return _fetchTimeByDb.GetOrAdd(dbName, DateTime.MinValue);
        }

        /// <summary>
        /// 设置指定数据库的 fetchTime
        /// </summary>
        public void SetFetchTime(string dbName, DateTime time)
        {
            _fetchTimeByDb[dbName] = time;
        }

        /// <summary>
        /// 重置所有数据库的 fetchTime 为当前时间
        /// </summary>
        public void ResetAllFetchTimes()
        {
            var now = DateTime.Now;
            var configs = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled)
                .ToList();

            foreach (var config in configs)
            {
                _fetchTimeByDb[config.DbName] = now;
            }

            LogTextHelper.Info($"已重置 {configs.Count} 个数据库的 fetchTime 为: {now}");
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
            var dbFetchTime = GetFetchTime(config.DbName);
            RootDbInfo getInfo = new RootDbInfo
            {
                uniqueKey = Guid.NewGuid().ToString(),
                db_name = config.DbName,
                operation = "get",
                is_select_range = "true",
                op_mode = "all",
                range_start = dbFetchTime.ToString(FixedTimeFormat),
                range_end = DateTime.Now.Date.AddDays(1).AddTicks(-1).ToString(FixedTimeFormat),
            };
            return _httpDb.HttpPostMethod(config.Url, getInfo, 0, out result);
        }

        /// <summary>
        /// 将传入的 AVI JSON 解析为 AviDataItem，并对每个数据项调用回调处理。
        /// 该方法不做后续业务处理，仅负责解析与类型转换。
        /// </summary>
        /// <param name="jsonInfo">AVI JSON 字符串</param>
        /// <param name="config">当前正在处理的 LevelDB 配置</param>
        public void DoAviJsonTyped(string jsonInfo, LevelDbConfig config)
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
                            ProcessAviDataItem(dataItem, settings, config);
                        }
                        else if (item is JObject jObj)
                        {
                            var dataItem = jObj.ToObject<AviDataItem>();
                            if (dataItem == null) continue;
                            ProcessAviDataItem(dataItem, settings, config);
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




        private void ProcessAviDataItem(AviDataItem dataItem, JsonSerializerSettings settings, LevelDbConfig config)
        {
            LogTextHelper.Info($"开始处理AVI数据项");
            if (string.IsNullOrEmpty(dataItem.Key) || string.IsNullOrEmpty(dataItem.Value))
                return;

            if (DateTime.TryParseExact(
             dataItem.Key,
            format: FixedTimeFormat,  // 直接用固定格式
            provider: CultureInfo.InvariantCulture,
            style: DateTimeStyles.None,
            result: out DateTime dt))
            {
                // 更新当前数据库的 fetchTime
                SetFetchTime(config.DbName, dt.AddMilliseconds(1));
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

                // ⭐ 关键修改点1：检查是否已在处理中
                if (_processingSnSet.ContainsKey(snKey))
                {
                    LogTextHelper.Warn($"SN:{serialNumber} Side:{side} 正在处理中，跳过重复请求");
                    continue;
                }

                // ⭐ 关键修改点2：标记为处理中
                if (!_processingSnSet.TryAdd(snKey, DateTime.Now))
                {
                    LogTextHelper.Warn($"SN:{serialNumber} Side:{side} 添加到处理集合失败，可能已被其他线程处理");
                    continue;
                }

                LogTextHelper.Info($"SN:{serialNumber} Side:{side} 已标记为处理中，当前处理集合大小：{_processingSnSet.Count}");

                // 根据 side 信息从当前 LevelDB 配置中获取对应的 MinIO IP
                // A面使用 MinioIpA，B面使用 MinioIpB，未配置时回退到 LevelDB 数据中的 MinioIp
                string minioIp;
                if (string.Equals(side, "A", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(config.MinioIpA))
                {
                    minioIp = config.MinioIpA;
                    LogTextHelper.Info($"SN:{serialNumber} Side:{side} 使用配置的A面MinIO IP: {minioIp} (来源DB:{config.DbName})");
                }
                else if (string.Equals(side, "B", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(config.MinioIpB))
                {
                    minioIp = config.MinioIpB;
                    LogTextHelper.Info($"SN:{serialNumber} Side:{side} 使用配置的B面MinIO IP: {minioIp} (来源DB:{config.DbName})");
                }
                else
                {
                    minioIp = resultInfo.MinioIp;
                    LogTextHelper.Info($"SN:{serialNumber} Side:{side} 使用LevelDB数据中的MinIO IP: {minioIp}");
                }

                string minioPort = resultInfo.MinioPort.ToString();

                if (string.IsNullOrEmpty(minioIp) || resultInfo.MinioPort == 0)
                {
                    // ⭐ 关键修改点3：异常情况需要移除标记
                    _processingSnSet.TryRemove(snKey, out _);

                    Thread.Sleep(500);
                    TaskStatusSender.SendFailed(serialNumber, side, "Minio格式错误");
                    SystemEvent.SendAlarmMsg($"SN:{serialNumber} {side}面 Minio格式错误;具体信息 MinioIP:{minioIp} MinioPort:{minioPort}");
                    continue;
                }

                string resultPath = resultInfo.ResultPath;
                if (string.IsNullOrEmpty(resultPath))
                {
                    // ⭐ 关键修改点4：异常情况需要移除标记
                    _processingSnSet.TryRemove(snKey, out _);
                    continue;
                }

                try
                {
                    ParseMinioPath(resultPath, out string path, out string result);
                    LogTextHelper.Info($"SN:{serialNumber} Side:{side} 解析Minio路径完成");
                    // call injected ReadJsonByMinio delegate (含源DB回写信息)
                    var writeBackDbName = config.WriteBackDbName ?? "filter_time_to_airesults";
                    var vrsWriteBackDbName = config.VRSWriteBackDbName ?? "ai_detail_results_tovrs";
                    var dbUrl = config.Url ?? "";
                    // 存储原始LevelDB JSON到调试缓存
                    var debugInfo = SnDebugInfoCache.GetOrCreate(serialNumber, side);
                    debugInfo.RawLevelDbJson = rawLevelDbJson;
                    debugInfo.SourceDbName = config.DbName;
                    debugInfo.SourceDbUrl = config.Url;

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
                        VrsWriteBackDbName = vrsWriteBackDbName
                    };
                    _readJsonByMinio(processingContext);
                    LogTextHelper.Info($"SN:{serialNumber} Side:{side} 通过Minio读取Json完成, 回写DB:{writeBackDbName}, VRS回写DB:{vrsWriteBackDbName}, URL:{dbUrl}");
                }
                catch (Exception ex)
                {
                    // ⭐ 关键修改点5：异常时移除标记
                    _processingSnSet.TryRemove(snKey, out _);
                    LogTextHelper.Error($"SN:{serialNumber} Side:{side} 处理异常: {ex}");
                }
            }
        }

    }
}

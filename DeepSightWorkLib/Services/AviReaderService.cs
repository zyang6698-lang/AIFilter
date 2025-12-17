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
using System.Threading;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 负责从 LevelDB 获取 AVI 消息并解析为强类型 AviDataItem 的服务（聚合/解析层）
    /// 目前仅实现解析与读取逻辑，后续会把更多职责从 BusinessClass 按需迁移到此处。
    /// </summary>
    public class AviReaderService
    {
        private readonly HttpClass _httpDb;
        private const string FixedTimeFormat = "yyyyMMddHHmmssfff";
        private readonly ConcurrentDictionary<string, DateTime> _processingSnSet;
        // delegate to call ReadJsonByMinio implemented elsewhere (BusinessClass)
        private readonly Action<string, string, string, string, string, string, string> _readJsonByMinio;
        public DateTime fetchTime = DateTime.MinValue;

        // Updated constructor to accept delegate for ReadJsonByMinio
        public AviReaderService(HttpClass httpDb, ConcurrentDictionary<string, DateTime> processingSnSet, Action<string, string, string, string, string, string, string> readJsonByMinio)
        {
            _httpDb = httpDb ?? throw new ArgumentNullException(nameof(httpDb));
            _processingSnSet = processingSnSet ?? throw new ArgumentNullException(nameof(processingSnSet));
            _readJsonByMinio = readJsonByMinio ?? throw new ArgumentNullException(nameof(readJsonByMinio));
        }

        /// <summary>
        /// 从 LevelDB 读取 AVI JSON（封装 BusinessClass.ReadAVI 的逻辑），使用外部提供的 fetchTime
        /// </summary>
        public bool ReadAVI(string url,  out string result)
        {
            RootDbInfo getInfo = new RootDbInfo
            {
                uniqueKey = Guid.NewGuid().ToString(),
                db_name = "ai_merged_results",
                operation = "get",
                is_select_range = "true",
                op_mode = "all",
                range_start = fetchTime.ToString(FixedTimeFormat),
                range_end = DateTime.Now.Date.AddDays(1).AddTicks(-1).ToString(FixedTimeFormat),
            };
            return _httpDb.HttpPostMethod(url, getInfo, 0, out result);
        }

        /// <summary>
        /// 将传入的 AVI JSON 解析为 AviDataItem，并对每个数据项调用回调处理。
        /// 该方法不做后续业务处理，仅负责解析与类型转换。
        /// </summary>
        public void DoAviJsonTyped(string jsonInfo)
        {
            if (string.IsNullOrEmpty(jsonInfo) )
                return;

            try
            {
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
                            ProcessAviDataItem(dataItem,settings);
                        }
                        else if (item is JObject jObj)
                        {
                            var dataItem = jObj.ToObject<AviDataItem>();
                            if (dataItem == null) continue;
                            ProcessAviDataItem(dataItem, settings);
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




        private void ProcessAviDataItem(AviDataItem dataItem, JsonSerializerSettings settings)
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
                fetchTime = dt.AddMilliseconds(1);
            }
            // 第二层：反序列化 value 字符串
            var valueData = JsonConvert.DeserializeObject<AviValueData>(dataItem.Value, settings);
            if (valueData?.ResultsInfo == null || valueData.ResultsInfo.Count == 0)
                return;

            string serialNumber = valueData.SerialNumber;
            if (string.IsNullOrEmpty(serialNumber))
                return;

            LogTextHelper.Info($"获取到{serialNumber}的数据");

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

                string minioIp = resultInfo.MinioIp;
                string minioPort = resultInfo.MinioPort.ToString();

                if (string.IsNullOrEmpty(minioIp) || resultInfo.MinioPort == 0)
                {
                    // ⭐ 关键修改点3：异常情况需要移除标记
                    _processingSnSet.TryRemove(snKey, out _);

                    Thread.Sleep(500);
                    SystemEvent.SendTaskMsg(serialNumber, $"{side}面Minio格式错误");
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
                    // call injected ReadJsonByMinio delegate
                    _readJsonByMinio(minioIp, minioPort, dataItem.Key, result, serialNumber, side, path);
                    LogTextHelper.Info($"SN:{serialNumber} Side:{side} 通过Minio读取Json完成");
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

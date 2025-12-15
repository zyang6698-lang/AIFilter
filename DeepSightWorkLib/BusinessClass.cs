using DeepSightCommunication;
using DeepSightDB;
using DeepSightDisplay;
using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using Minio;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace DeepSightWorkLib
{
    //后续考虑是否设计为抽象，支持传统及AI调用
    public class BusinessClass : IDisposable
    {
        #region 私有字段

        private readonly DatabaseHelper _databaseHelper;

        /// <summary>
        /// 读取 AVI 存储对象队列
        /// </summary>
        private readonly ConcurrentQueue<VBModel> _aviQueue = new ConcurrentQueue<VBModel>();

        /// <summary>
        /// 算法处理结果存储对象队列 (Key, SN, Side, RootAIResult, DsCenterInfo)
        /// </summary>
        private readonly ConcurrentQueue<Tuple<string, string, string, RootAIResult, DsCenterInfo>> _aiResultQueue = new ConcurrentQueue<Tuple<string, string, string, RootAIResult, DsCenterInfo>>();

        /// <summary>
        /// 图片加载队列（解耦图片读取和推理）
        /// </summary>
        private readonly ConcurrentQueue<ImageLoadModel> _imageLoadQueue = new ConcurrentQueue<ImageLoadModel>();

        /// <summary>
        /// 推理后处理队列（异步处理推理结果）
        /// </summary>
        private readonly ConcurrentQueue<InferenceResultModel> _inferencePostProcessQueue = new ConcurrentQueue<InferenceResultModel>();

        /// <summary>
        /// 奥特斯项目上传中台数据字典
        /// </summary>
        private readonly ConcurrentDictionary<string, DsCenterInfo> _dsCenterInfoDict = new ConcurrentDictionary<string, DsCenterInfo>();

        /// <summary>
        /// 正在处理的SN+Side集合（防止重复检测）
        /// Key格式："{SerialNumber}_{Side}"
        /// Value：入队时间戳
        /// </summary>
        private readonly ConcurrentDictionary<string, DateTime> _processingSnSet = new ConcurrentDictionary<string, DateTime>();

        /// <summary>
        /// 缓存过期时间（分钟），超过此时间自动清理，防止内存泄漏
        /// </summary>
        private const int CACHE_EXPIRE_MINUTES = 30;

        /// <summary>
        /// 开始/停止作业标志（内部字段）
        /// </summary>
        private volatile bool _isStart = false;

        private DateTime fetchTime = DateTime.Now;
        private const string FixedTimeFormat = "yyyyMMddHHmmssfff";
        private Stopwatch AIStopwatch;


        // Task-based management（线程管理）
        private CancellationTokenSource _cancellationTokenSource;
        private Task _readAviTask;
        private Task _defectTask;
        private Task _returnAviTask;
        private Task _imageLoadTask;
        private Task _postProcessTask;
        private Task _cleanupTask;

        #endregion

        #region 公共属性

        /// <summary>
        /// 算法检测对象
        /// </summary>
        public DefectClass Defect { get; private set; }

        /// <summary>
        /// LevelDB 交互服务
        /// </summary>
        public HttpClass HttpDb { get; private set; }

        /// <summary>
        /// Minio 对象存储服务
        /// </summary>
        public MinioClass Minio { get; private set; }

        /// <summary>
        /// 图像显示服务
        /// </summary>
        public ImageDisplayService ImageDisplay { get; private set; }


        /// <summary>
        /// 开始/停止作业标志
        /// </summary>
        public bool IsStart
        {
            get => _isStart;
            set
            {
                bool previousValue = _isStart;
                _isStart = value;

                // 当从 true 变为 false 时，清空所有正在处理的队列
                if (previousValue && !value)
                {
                    ClearAllProcessingQueues();
                }
                // 当从 false 变为 true 时，更新推理请求开始时间
                if (!previousValue && value)
                {
                    fetchTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// 是否显示检测框（同步到 ImageDisplayService）
        /// </summary>
        public bool IsShowBox
        {
            get => ImageDisplay?.IsShowBox ?? false;
            set
            {
                if (ImageDisplay != null)
                {
                    ImageDisplay.IsShowBox = value;
                }
            }
        }

        /// <summary>
        /// 是否允许处理
        /// </summary>
        public bool IsAllow { get; set; } = true;

        /// <summary>
        /// 是否测试模式 (true: 测试模式)
        /// </summary>
        public bool TestFlag { get; set; } = false;

        /// <summary>
        /// LevelDB 服务 URL
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// 产品料号
        /// </summary>
        public string ProductSerial { get; set; } = "";

        /// <summary>
        /// 当前方案名称
        /// </summary>
        public string Solution { get; set; } = "";

        /// <summary>
        /// 当前流程名称
        /// </summary>
        public string Flow { get; set; } = "";

        /// <summary>
        /// 是否切换模式
        /// </summary>
        public bool IsSwitch { get; set; } = false;

        /// <summary>
        /// 方案配置
        /// </summary>
        public SolutionConfig SolConfig { get; set; } = new SolutionConfig();

        /// <summary>
        /// AVI 配置
        /// </summary>
        public AVIConfig AviConfig { get; set; } = new AVIConfig();

        /// <summary>
        /// 系统配置
        /// </summary>
        public ConfigurationClass SysConfig { get; set; } = new ConfigurationClass();

        #endregion

        #region 兼容性属性 (已弃用，保持向后兼容)

        [Obsolete("请使用 Defect 属性")]
        public DefectClass defect { get => Defect; set => Defect = value; }

        [Obsolete("请使用 IsStart 属性")]
        public bool isStart { get => IsStart; set => IsStart = value; }

        [Obsolete("请使用 IsShowBox 属性")]
        public bool isShowBox { get => IsShowBox; set => IsShowBox = value; }

        [Obsolete("请使用 SolConfig 属性")]
        public SolutionConfig solconfig { get => SolConfig; set => SolConfig = value; }

        [Obsolete("请使用 AviConfig 属性")]
        public AVIConfig aviconfig { get => AviConfig; set => AviConfig = value; }

        [Obsolete("请使用 SysConfig 属性")]
        public ConfigurationClass sysConfig { get => SysConfig; set => SysConfig = value; }

        /// <summary>
        /// 小图显示集合
        /// </summary>
        public List<CvDisplay> DisplaysList { get; set; }

        /// <summary>
        /// 小图显示集合2
        /// </summary>
        public List<CvDisplay> DisplaysList2 { get; set; }

        #endregion

        #region 构造函数和初始化

        public BusinessClass()
        {
            Defect = new DefectClass();
            HttpDb = new HttpClass();
            Minio = new MinioClass();
            ImageDisplay = new ImageDisplayService(Minio);
            // 先初始化数据库（确保数据库和表存在），然后再创建 DatabaseHelper 实例
            DatabaseHelper.InitializeDatabase();
            _databaseHelper = new DatabaseHelper();
        }

        /// <summary>
        /// 初始化工作线程
        /// </summary>
        /// <param name="url">LevelDB 服务 URL</param>
        /// <param name="index">游标索引</param>
        public void InitWork(string url)
        {
            this.URL = url;
            this.IsStart = false;
            // 工作线程 -> 使用 Task 管理
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            _readAviTask = Task.Run(() => ThreadReadAVI(token), token);
            _imageLoadTask = Task.Run(() => ThreadImageLoad(token), token);
            _defectTask = Task.Run(() => ThreadDefect(token), token);
            _returnAviTask = Task.Run(() => ThreadReturnAVI(token), token);
            _postProcessTask = Task.Run(() => ThreadPostProcess(token), token);
            AIStopwatch = new Stopwatch();
            // ⭐ 关键修改点7：启动清理线程
            _cleanupTask = Task.Factory.StartNew(() => ThreadCleanupProcessingCache(token),
                token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            LogTextHelper.Info("BusinessClass 初始化完成，所有线程已启动");
        }

        /// <summary>
        /// 设置显示窗口列表（委托给 ImageDisplayService）
        /// </summary>
        public void SetHWindow(List<CvDisplay> displaysList)
        {
            ImageDisplay.SetDisplayList(displaysList);
            DisplaysList = ImageDisplay.DisplaysList;
        }

        /// <summary>
        /// 设置显示窗口列表2（委托给 ImageDisplayService）
        /// </summary>
        public void SetHWindow2(List<CvDisplay> displaysList)
        {
            ImageDisplay.SetDisplayList2(displaysList);
            DisplaysList2 = ImageDisplay.DisplaysList2;
        }

        #region 兼容性方法（已弃用）

        [Obsolete("请使用 SetHWindow 方法")]
        public void setHWindow(List<CvDisplay> displaysList) => SetHWindow(displaysList);

        [Obsolete("请使用 ImageDisplay.ShowImage 方法")]
        public void showImage(string path, int index, string result = "", VBRcvInfp box = null) => ShowImage(path, index, result, box);

        /// <summary>
        /// 显示图片（委托给 ImageDisplayService）
        /// </summary>
        public void ShowImage(string path, int index, string result = "", VBRcvInfp box = null)
        {
            ImageDisplay.ShowImage(path, index, result, box);
        }

        #endregion

        #endregion

        #region 辅助方法

        #endregion

        #region 线程管理


        /// <summary>
        /// 线程处理 (Task循环)
        /// </summary>
        private void ThreadReadAVI(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 使用 Thread.Sleep 替代 Task.Delay().Wait()，避免阻塞线程池线程
                    Thread.Sleep(500);
                    if (token.IsCancellationRequested) break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    if (!IsStart || TestFlag || !IsAllow)
                    {
                        continue;
                    }
                    // 读取 AVI 数据
                    if (ReadAVI(URL, out string result))
                    {
                        DoAviJsonTyped(result);
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error(ex.ToString());
                }
            }
        }

        #endregion

        #region AVI 数据读取与处理

        /// <summary>
        /// 处理 AVI JSON 数据（强类型反序列化版本 - 性能最优）
        /// 使用强类型模型直接反序列化，避免多次字符串解析
        /// 注意：需要先确保 AviResponseModel.cs 已添加到 DeepSightModel 项目
        /// </summary>
        public void DoAviJsonTyped(string jsonInfo)
        {
            try
            {
                // 使用 JsonConvert 的设置来优化性能
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                // 第一层：反序列化外层响应
                var response = JsonConvert.DeserializeObject<AviResponse>(jsonInfo, settings);

                if (response?.DataList == null || response.DataList.Count <= 1)
                {
                    return;
                }

                // 遍历 data_list
                foreach (var item in response.DataList)
                {
                    try
                    {
                        // 跳过字符串类型的标记（如 "end_range_send"）
                        if (item is string strItem)
                        {
                            if (strItem == "end_range_send")
                                continue;

                            // 如果是JSON字符串，尝试解析
                            var dataItem = JsonConvert.DeserializeObject<AviDataItem>(strItem, settings);
                            if (dataItem == null) continue;

                            ProcessAviDataItem(dataItem, settings);
                        }
                        else if (item is JObject jObj)
                        {
                            // 如果已经是JObject，直接转换
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
        /// 处理单个AVI数据项（辅助方法）
        /// </summary>
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
                    ReadJsonByMinio(minioIp, minioPort, dataItem.Key, result, serialNumber, side, path);
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

        /// <summary>
        /// 解析Minio路径
        /// </summary>
        public void ParseMinioPath(string fullPath, out string path, out string result)
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
                path = "";
                result = "";
                LogTextHelper.Error(ex.ToString());
            }
        }


        public bool ReadAVI(string url, out string Result)
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
            return HttpDb.HttpPostMethod(url, getInfo, 0, out Result);
        }

        #endregion

        #region 算法检测处理

        /// <summary>
        /// 算法检测线程处理 (Task循环)
        /// </summary>
        private void ThreadDefect(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 使用 Thread.Sleep 替代 Task.Delay().Wait()，避免阻塞线程池线程
                    Thread.Sleep(15);
                    if (token.IsCancellationRequested) break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (!IsStart)
                {
                    continue;
                }
                if (_aviQueue.Count > 0)
                {
                    if (_aviQueue.TryDequeue(out VBModel info))
                    {
                        try
                        {
                            // 检查点1：出队后立即检查是否应该中止
                            if (!IsStart)
                            {
                                LogTextHelper.Info($"AI检测任务被暂停中止（出队后），SN:{info?.SN}");
                                SystemEvent.SendTaskMsg(info?.SN, "暂停-AI检测已中止");
                                continue; // finally 会释放资源
                            }

                            SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面开始AI检测(缺陷数:{info.Mats.Count})");
                            AIStopwatch.Restart();

                            //调用算法处理
                            LogTextHelper.Info($"准备DefectMethod，SN:{info.SN}，图片数量:{info.Mats.Count}");
                            if (DefectMethod(info, out List<string> msg, out List<string> details, out PcsResult pcsResult, out string vbJson))
                            {
                                // 检查点2：推理完成后检查是否应该中止（不再回写结果）
                                if (!IsStart)
                                {
                                    LogTextHelper.Info($"AI检测任务被暂停中止（推理完成后），SN:{info.SN}");
                                    SystemEvent.SendTaskMsg(info.SN, "暂停-AI检测已中止（结果未回写）");
                                    continue; // finally 会释放资源
                                }

                                SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面AI检测完成");
                                SystemEvent.SendResultInfo(info.SN, msg, details, pcsResult);
                                if (TestFlag)
                                {
                                    SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面已完成");
                                    LogTextHelper.Info($"{info.SN}  算法返回结果：{string.Join(",", msg)}");
                                }
                                else
                                {
                                    SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面正在回写结果");
                                    RootAIResult data = new RootAIResult
                                    {
                                        DbName = "filter_time_to_airesults",
                                        Operation = "put",
                                        OpMode = info.Side == "A" ? "all_ow" : "ap",
                                        Key = info.Key,
                                    };


                                    List<ResultInfo> results = new List<ResultInfo>();
                                    if (msg.Count == 0)
                                    {
                                        msg = Enumerable.Repeat("1", info.DefectIndex.Count).ToList();
                                    }
                                    for (int i = 0; i < info.DefectIndex.Count; i++)
                                    {
                                        
                                        ResultInfo res = new ResultInfo
                                        {
                                            ResultInfos = $"{info.Side}_{info.PcsIndex[i]}_{info.DefectIndex[i]}_{msg[i]}",
                                            Details = new Details()
                                            {

                                            }
                                        };
                                        results.Add(res);
                                    }
                                    WriteBackData writeBackData = new WriteBackData()
                                    {
                                        ResultInfos = results,
                                        SerialNumber = info.SN,
                                        PanelJsonPath = info.panelInfo.LocalDescribePath,
                                    };

                                    JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };//去掉空值NULL
                                    data.Value = JsonConvert.SerializeObject(writeBackData, Formatting.None, jsonSetting);

                                    // 在 B 面时获取中台数据，一起放入队列处理
                                    DsCenterInfo dsinfo = null;
                                    if (info.Side == "B")
                                    {
                                        _dsCenterInfoDict.TryRemove($"{info.panelInfo.LotId}_{info.panelInfo.SerialNumber}", out dsinfo);
                                    }

                                    Tuple<string, string, string, RootAIResult, DsCenterInfo> dbTub = Tuple.Create(info.Key, info.SN, info.Side, data, dsinfo);
                                    // 存储算法处理的结果
                                    _aiResultQueue.Enqueue(dbTub);
                                }
                            }
                            else
                            {
                                LogTextHelper.Warn($"KEY:{info.Key} SN:{info.SN}检测失败！");
                                SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面已完成");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogTextHelper.Error(ex.ToString());
                            SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面已完成");
                        }
                        finally
                        {
                            AIStopwatch.Stop();
                            LogTextHelper.Info($"{info.SN} {info.Side} AI花费时间{AIStopwatch.ElapsedMilliseconds}ms");
                            // 释放 Mat 资源，防止内存泄漏
                            if (info?.Mats != null)
                            {
                                foreach (var mat in info.Mats)
                                {
                                    mat?.Dispose();
                                }
                                info.Mats.Clear();
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Minio 操作与数据转换

        /// <summary>
        /// 通过Minio读取Json文件
        /// </summary>
        public void ReadJsonByMinio(string ip, string port, string key, string head, string sn, string side, string path)
        {
            try
            {
                LogTextHelper.Info($"{sn} 准备ReadJsonByMinio");
                SystemEvent.SendTaskMsg(sn, $"{side}面正在读取数据");
                Minio.BuildClient(ip, port);
                string json = Minio.ReadJsonSync("deepiresults", path, ip);
                var obj = JsonConvert.DeserializeObject<RootPanelInfo>(json);
                List<int> defectIndex = new List<int>();
                List<int> pcsList = new List<int>();
                if (TestFlag)
                {
                    side = obj.SideIndex;
                }
                if (side == "A")
                {
                    SystemEvent.SendTaskMsg(sn);
                }
                LogTextHelper.Info($"{sn} {side} 开始将json转为vbinfo");
                RootVBInfo vbInfo = PanelJsonToVBInfo(ip, port, head, obj, ref defectIndex, ref pcsList, out bool isByPass);

                RootPanelInfoWithIP rootobj = new RootPanelInfoWithIP()
                {
                    IP = ip,
                    rootInfo = obj,
                };

                // 解耦：先获取图片Key列表，入图片加载队列，非阻塞
                var imageKeys = GetAllMinioImageKeys(rootobj);

                //考虑用Model方式
                VBModel model = new VBModel
                {
                    Key = key,
                    SN = sn,
                    Side = side,
                    DefectIndex = defectIndex,
                    PcsIndex = pcsList,
                    VbInfo = vbInfo,
                    minioPath = head,
                    panelInfo = obj,
                    isByPass = isByPass,
                    ImageKeys=imageKeys
                };

                var loadModel = new ImageLoadModel
                {
                    Model = model,
                    RootPanelInfo = rootobj
                };
                _imageLoadQueue.Enqueue(loadModel);

                LogTextHelper.Info($"{sn} {side} ReadJsonByMinio完成,入队列_imageLoadQueue成功,待加载图片数量:{imageKeys.Count}");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常" + ex.ToString());
            }
        }

        public RootVBInfo PanelJsonToVBInfo(string minioip, string minioport, string head, RootPanelInfo info, ref List<int> defectList, ref List<int> pcsList, out bool isByPass)
        {
            try
            {
                isByPass = false;
                LogTextHelper.Info($"{info.SerialNumber} {info.SideIndex}  ProcuctSerial:" + info.ProductSerial);
                var solutionFlow = SolConfig.solus.FirstOrDefault(o => o.ProductSerial == info.ProductSerial);
                if (solutionFlow != null)
                {
                    if (info.SideIndex == "A")
                    {
                        Solution = solutionFlow.Asolution;
                        Flow = solutionFlow.Aflow;
                    }
                    else
                    {
                        Solution = solutionFlow.Bsolution;
                        Flow = solutionFlow.Bflow;
                    }
                    IsSwitch = solutionFlow.IsSwitch;
                }
                else
                {
                    isByPass = true;
                    LogTextHelper.Warn($"料号 {info.ProductSerial} 未配置，使用默认方案，标记为ByPass");
                    var defaultSolutionFlow = SolConfig.solus.FirstOrDefault(o => o.ProductSerial.ToUpper() == "DEFAULT");
                    if (defaultSolutionFlow != null)
                    {
                        if (info.SideIndex == "A")
                        {
                            Solution = defaultSolutionFlow.Asolution;
                            Flow = defaultSolutionFlow.Aflow;
                        }
                        else
                        {
                            Solution = defaultSolutionFlow.Bsolution;
                            Flow = defaultSolutionFlow.Bflow;
                        }
                        IsSwitch = defaultSolutionFlow.IsSwitch;
                    }
                    else
                    {
                        throw new Exception("找不到default的算法流程");
                    }
                }

                DsCenterInfo dsInfo = new DsCenterInfo();
                PanelData panelData = new PanelData
                {
                    Project = SysConfig.ProjectName
                };
                if (info.SideIndex == "A")
                {
                    panelData.Product = info.ProductSerial;
                    panelData.Lot = info.LotId;
                    panelData.Sn = info.SerialNumber;
                    panelData.Avi = info.StationName;
                    panelData.CustomTags = "";
                }
                else
                {
                    _dsCenterInfoDict.TryGetValue($"{info.LotId}_{info.SerialNumber}", out dsInfo);
                    JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                    string infoJson = JsonConvert.SerializeObject(dsInfo, Formatting.None, jsonSetting);
                }

                LogTextHelper.Info($"当前产品:{info.SerialNumber},{info.SideIndex}面,所属料号:{info.ProductSerial},切换参数-->方案:{Solution},flow:{Flow}");
                RootVBInfo vBInfo = new RootVBInfo
                {
                    MessageType = "visionbuilder_inference",
                    paramsData = new ParamsData()
                };
                vBInfo.paramsData.InferResUuid = Guid.NewGuid().ToString();
                vBInfo.paramsData.InferWholeData = new InferWholeData
                {
                    ImageInferParams = new ImageInferParams
                    {
                        PipelineName = Solution,
                        NodeParams = new List<NodeParam>
                        {
                            new NodeParam()
                            {
                                NodeName = Flow,
                                height = 200,
                                width = 200,
                            }
                        }
                    },
                    ImageData = new ImageData()
                    {
                        DataType = "minio",
                        DataValue = new DataValue
                        {
                            InferImageGroup = new List<InferImageGroup>()
                        }
                    }
                };

                for (int i = 0; i < info.PcsInfo.Count; i++)
                {
                    ContentItem item = new ContentItem();
                    if (info.SideIndex == "A")
                    {
                        item.MachineName = info.StationName;
                        item.ProductSerial = info.ProductSerial;
                        item.SerialNumber = info.SerialNumber;
                        item.ProcessTimeA = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
                        item.LotId = info.LotId;
                        item.Lot = info.LotId;
                        item.Product = info.ProductSerial;
                        item.OperateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        item.ExpansionAndContraction = null;
                        item.PieceIndex = (i + 1).ToString();
                        item.PieceVesIndex = (i + 1).ToString();
                    }
                    else
                    {
                        JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };//去掉空值NULL
                        string infoJson = JsonConvert.SerializeObject(dsInfo, Formatting.None, jsonSetting);
                        dsInfo?.Data[0].Content.TryGetValue((i + 1).ToString(), out item);
                        item.ProcessTimeB = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
                    }


                    // info的PcsInfo在ATS只有一条，做其他项目时要注意
                    if (info.PcsInfo.TryGetValue((i + 1).ToString(), out PcsInfo pcsInfo))
                    {
                        LogTextHelper.Info($"SN:{info.SerialNumber}_{info.SideIndex}面报点数据为:{pcsInfo.DefectInfo.Count}");

                        for (int j = 0; j < pcsInfo.DefectInfo.Count; j++)
                        {
                            //奥特斯
                            DsCenterDefectInfo dsDefectinfo = new DsCenterDefectInfo
                            {
                                SideType = info.SideIndex,
                                SideType2 = info.SideIndex,
                                PcsIndex = i + 1,
                                PcsVesIndex = (i + 1).ToString(),
                                DefectRoi = pcsInfo.DefectInfo[j].DefectRoi,

                                DefectOriginRoi = pcsInfo.DefectInfo[j].DefectOriginRoi,
                                DefectsRoi = new List<int>()
                                {
                                    pcsInfo.DefectInfo[j].DefectRoi.X,
                                    pcsInfo.DefectInfo[j].DefectRoi.Y,
                                    pcsInfo.DefectInfo[j].DefectRoi.Height,
                                    pcsInfo.DefectInfo[j].DefectRoi.Width
                                }
                            };
                            InferImageGroup group = new InferImageGroup
                            {
                                MachineTemplateInfo = new MachineTemplateInfo()
                                {
                                    MachineName = info.StationName,
                                    product = info.ProductSerial,
                                    Side = info.SideIndex,
                                },
                                GroupUuid = Guid.NewGuid().ToString(),
                                GroupInfos = new List<GroupInfo>(),
                                DefectCode = "",
                                TempImgPath = Path.Combine(SolConfig.PartNumberImagesLoc, $"{info.ProductSerial}\\{info.ProductSerial}[{info.SideIndex}].jpg"),
                                ImgROI = new List<int>
                                {
                                    pcsInfo.DefectInfo[j].DefectRoi.X,
                                    pcsInfo.DefectInfo[j].DefectRoi.Y,
                                    pcsInfo.DefectInfo[j].DefectRoi.Width,
                                    pcsInfo.DefectInfo[j].DefectRoi.Height
                                }
                            };
                            if (IsSwitch)
                            {
                                group.DefectCode = pcsInfo.DefectInfo[j].DefectCode;
                            }
                            WatchPathConfig config = AviConfig.WatchPaths.FirstOrDefault(o => o.AviName == info.StationName);
                            if (config != null)
                            {
                                void AddGroupInfo(List<string> images, string imageType, Action<string> addUrlAction = null)
                                {
                                    if (images != null && images.Count > 0)
                                    {
                                        var imagePath = $"{head}/{images[0]}";
                                        group.GroupInfos.Add(new GroupInfo()
                                        {
                                            ImagePath = imagePath,
                                            ImageUuid = Guid.NewGuid().ToString(),
                                            ImageType = imageType,
                                        });
                                        addUrlAction?.Invoke($"http://{config.MinioConfig}/deepiresults/{imagePath}");
                                    }
                                }
                                AddGroupInfo(pcsInfo.DefectInfo[j].DefectVrsImages, "defect", url => dsDefectinfo.DefectImages.Add(url));
                                AddGroupInfo(pcsInfo.DefectInfo[j].DefectVrsOkImages, "template");
                                AddGroupInfo(pcsInfo.DefectInfo[j].DefectVrsGerberImages, "gerber", url => dsDefectinfo.DefectGerberImages.Add(url));
                            }
                            else
                            {
                                LogTextHelper.Error($"{pcsInfo.PcsSerialNumber}:panel的machineID:{info.StationName} 未找到对应机台的machineID");
                            }

                            vBInfo.paramsData.InferWholeData.ImageData.DataValue.InferImageGroup.Add(group);
                            group.inspectDetails = new InspectDetails
                            {
                                InferRois = new List<InferRoi>() { }
                            };
                            defectList.Add(j);
                            pcsList.Add(pcsInfo.DefectInfo[j].PcsIndex);
                            item.DefectsInfo.Add(dsDefectinfo);
                        }
                    }
                    if (info.SideIndex == "A")
                    {
                        panelData.Content.Add((i + 1).ToString(), item);
                    }
                }
                vBInfo.paramsData.InferWholeData.OtherInfos = new Others();
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio = new ImageMminio();
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.access_key_id = "deepiobjectdata";
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.bucket = "deepiresults";
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.endpoint_url = minioip;
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.secret_key = "deepiobject2019";
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.secret_port = minioport;

                if (info.SideIndex == "A")
                {
                    dsInfo.Data.Add(panelData);
                    _dsCenterInfoDict.TryAdd($"{info.LotId}_{info.SerialNumber}", dsInfo);
                    LogTextHelper.Info($"{info.LotId}_{info.SerialNumber}_在A面创建dsinfo成功");
                }
                else
                {
                    _dsCenterInfoDict[$"{info.LotId}_{info.SerialNumber}"] = dsInfo;
                    LogTextHelper.Info($"{info.LotId}_{info.SerialNumber}_在B面取得dsinfo成功");
                }
                return vBInfo;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                throw;
            }
        }

        #endregion

        #region 算法调用与结果处理

        public bool DefectMethod(VBModel vBModel, out List<string> resList, out List<string> detailsList, out PcsResult pcsResult, out string vbJson)
        {
            resList = new List<string>();
            pcsResult = new PcsResult();
            detailsList = new List<string>();
            vbJson = string.Empty;

            if (vBModel.Mats.Count == 0)
            {
                // 异步保存到数据库
                EnqueuePostProcess(vBModel, "", false);
                LogTextHelper.Info($"{vBModel.SN} {vBModel.Side},图片数量为0，跳过vb检测流程");
                SystemEvent.SendTaskMsg(vBModel.SN, $"{vBModel.Side}面缺陷数为0，跳过AI检测");
                return true;
            }

            if (vBModel.Mats.Count > SysConfig.MaxDefectCount)
            {
                // 异步保存到数据库
                EnqueuePostProcess(vBModel, "", false);
                LogTextHelper.Info($"{vBModel.SN} {vBModel.Side},图片数量大于{SysConfig.MaxDefectCount}，跳过vb检测流程");
                SystemEvent.SendTaskMsg(vBModel.SN, $"{vBModel.Side}面缺陷数({vBModel.Mats.Count})>{SysConfig.MaxDefectCount}，跳过AI检测");
                return true;
            }

            RootVBInfo info = vBModel.VbInfo;
            RootPanelInfo panelInfo = vBModel.panelInfo;
            bool result;
            try
            {

                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };//去掉空值NULL
                string infoJson = JsonConvert.SerializeObject(info, Formatting.None, jsonSetting);
                LogTextHelper.Info($"{vBModel.SN} {vBModel.Side}  准备调用算法,参数为：" + infoJson);

                // ============ 核心推理调用（同步） ============
                Defect.DefectMethodWithImages(info, vBModel.Mats, out string msg);
                // ============================================

                LogTextHelper.Info($"{vBModel.SN} {vBModel.Side} 算法返回原始结果 for Side {panelInfo.SideIndex}: {msg}");

                //将RootVBOutInfo结果msg处理
                if (string.IsNullOrEmpty(msg))
                {
                    LogTextHelper.Error($"算法返回结果为空 for Side {panelInfo.SideIndex}");
                    return false;
                }

                // ============ 将后处理任务加入队列（异步执行） ============
                EnqueuePostProcess(vBModel, msg, true);
                // ======================================================

                // 快速解析返回码用于立即返回结果
                var obj = JsonConvert.DeserializeObject<RootVBOutInfo>(msg);
                if (obj == null)
                {
                    LogTextHelper.Error($"算法返回结果反序列化失败 for Side {panelInfo.SideIndex}，原始消息: {msg}");
                    SystemEvent.SendTaskMsg(vBModel.SN, $"{vBModel.Side}面算法返回结果反序列化失败");
                    return false;
                }
                string code = obj.Code.ToString();
                string message = obj.Message.ToString();

                LogTextHelper.Info($"{vBModel.SN} {vBModel.Side} 算法返回码: {code}, 消息: {message}");

                // 快速构建返回结果（用于界面显示）
                if (code == "200")
                {
                    JObject root = JObject.Parse(msg);

                    // 安全访问嵌套属性，避免 JValue 类型导致的异常
                    var dataToken = root["data"];
                    var inferWholeData = (dataToken as JObject)?["infer_whole_data"];
                    var inferResultsToken = (inferWholeData as JObject)?["infer_results"];

                    if (inferResultsToken is JArray inferResults)
                    {
                        foreach (var result1 in inferResults)
                        {
                            var inferDetails = (result1 as JObject)?["infer_details"];

                            if ((inferDetails as JObject)?["node_details"] is JObject nodeDetails)
                            {
                                string nodeDetailsJson = nodeDetails.ToString();
                                detailsList.Add(nodeDetailsJson);
                            }
                        }
                    }

                    // 处理 resList、pcsResult 等（用于界面显示）
                    pcsResult.vb_List = new List<VBRcvInfp>();
                    for (int i = 0; i < obj.Data.InferWholeData.InferResults.Count; i++)
                    {
                        VBRcvInfp vBRcv = new VBRcvInfp
                        {
                            bbox = new List<List<double>>()
                        };

                        if (obj.Data.InferWholeData.InferResults[i].Infer_Result != "OK")
                        {
                            for (int j = 0; j < obj.Data.InferWholeData.InferResults[i].inferDetails.Location.Count; j++)
                            {
                                string sub_defectName = obj.Data.InferWholeData.InferResults[i].Defect_name;

                                int subX = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].X);
                                int subY = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Y);
                                int subH = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Height);
                                int subW = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Width);

                                vBRcv.raw_bbox = new List<double>
                                {
                                    subX,
                                    subY,
                                    subW,
                                    subH
                                };
                                vBRcv.bbox.Add(vBRcv.raw_bbox);
                                vBRcv.sub_DefectNames.Add(sub_defectName);
                            }
                        }

                        // 0为OK 1为NG 2为bypass
                        if (vBModel.isByPass)
                        {
                            resList.Add("2");
                        }
                        else
                        {
                            resList.Add(obj.Data.InferWholeData.InferResults[i].Infer_Result == "NG" ? "1" : "0");
                        }
                        pcsResult.vb_List.Add(vBRcv);
                    }

                    // 如果是ByPass，添加状态消息
                    if (vBModel.isByPass)
                    {
                        LogTextHelper.Info($"{vBModel.SN} {vBModel.Side}面使用默认方案，结果标记为ByPass");
                    }

                    vbJson = msg;
                    result = true;
                }
                else if (code == "600")
                {
                    LogTextHelper.Info($"{vBModel.SN} 算法返回码600: {message}");
                    SystemEvent.SendTaskMsg(vBModel.SN, $"{vBModel.Side}面算法返回(Code:600, {message})");
                    result = true;
                }
                else
                {
                    LogTextHelper.Warn($"算法调用失败 for Side {panelInfo.SideIndex}，返回码: {code}，返回信息：{message}");
                    SystemEvent.SendTaskMsg(vBModel.SN, $"{vBModel.Side}面算法调用失败(Code:{code}, {message})");
                    result = false;
                }
            }
            catch (Exception ex)
            {
                vbJson = string.Empty;
                resList = null;
                detailsList = null;
                result = false;
                pcsResult = null;
                SystemEvent.SendAlarmMsg("算法处理异常" + ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 更新中台数据（奥特斯项目）- 纯中台数据更新，不涉及HeatPoint等其他逻辑
        /// </summary>
        /// <param name="panelInfo">面板信息</param>
        /// <param name="obj">算法返回结果</param>
        private void UpdateDsCenterInfo(RootPanelInfo panelInfo, RootVBOutInfo obj)
        {
            if (!_dsCenterInfoDict.TryGetValue($"{panelInfo.LotId}_{panelInfo.SerialNumber}", out DsCenterInfo dsCenterInfo))
            {
                return;
            }
            if (dsCenterInfo == null)
            {
                return;
            }
            LogTextHelper.Info($"取出{panelInfo.LotId}_{panelInfo.SerialNumber}的中台数据，准备更新...");

            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                for (int i = 0; i < obj.Data.InferWholeData.InferResults.Count; i++)
                {
                    dsCenterInfo.Data[0].Content["1"].DefectsCount++;

                    try
                    {
                        //更新中台数据
                        if (panelInfo.SideIndex == "A")
                        {
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].AiResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].ManualResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].ManualDefectCode = obj.Data.InferWholeData.InferResults[i].Defect_name;
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].DefectCode = obj.Data.InferWholeData.InferResults[i].Defect_name;
                        }
                        else
                        {
                            dsCenterInfo.Data[0].Content["1"].EndTime = time;
                            panelInfo.EndTime = time;
                            //因为上传中台数据A/B面的一个pcs信息在一个包，但A/B面处理是分开的；
                            //如果是B的话，先计算A面的报点数
                            int Bcount = panelInfo.PcsInfo["1"].DefectInfo.Count;
                            int ALLcount = dsCenterInfo.Data[0].Content["1"].DefectsInfo.Count;
                            int index = ALLcount - Bcount;
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].AiResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].ManualResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].ManualDefectCode = obj.Data.InferWholeData.InferResults[i].Defect_name;
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].DefectCode = obj.Data.InferWholeData.InferResults[i].Defect_name;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error("更新中台数据异常" + ex.ToString());
                    }

                    // 更新子缺陷信息到中台数据
                    for (int j = 0; j < obj.Data.InferWholeData.InferResults[i].inferDetails.Location.Count; j++)
                    {
                        DsCenterSubDefectInfo subDefectInfo = new DsCenterSubDefectInfo();
                        subDefectInfo.SubDefectArea = Convert.ToDouble(obj.Data.InferWholeData.InferResults[i].inferDetails.DefectArea);
                        string sub_defectName = obj.Data.InferWholeData.InferResults[i].Defect_name;
                        subDefectInfo.SubDefectCode = sub_defectName;

                        int defectX = 0;
                        int defectY = 0;
                        if (panelInfo.SideIndex == "A")
                        {
                            defectX = dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].DefectRoi.X;
                            defectY = dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].DefectRoi.Y;
                        }
                        else
                        {
                            int Bcount = panelInfo.PcsInfo["1"].DefectInfo.Count;
                            int ALLcount = dsCenterInfo.Data[0].Content["1"].DefectsInfo.Count;
                            int index = ALLcount - Bcount;
                            defectX = dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].DefectRoi.X;
                            defectY = dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].DefectRoi.Y;
                        }

                        int subX = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].X);
                        int subY = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Y);
                        int subH = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Height);
                        int subW = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Width);

                        subDefectInfo.SubDefectHeight = subH;
                        subDefectInfo.SubDefectWidth = subW;
                        subDefectInfo.SubDefectIndex = j;
                        subDefectInfo.SubDefectRoi.Add(subX);
                        subDefectInfo.SubDefectRoi.Add(subY);
                        subDefectInfo.SubDefectRoi.Add(subW);
                        subDefectInfo.SubDefectRoi.Add(subH);
                        //中心点参数
                        int CenterPointX = defectX + subX / 2 + subW / 4;
                        int CenterPointY = defectY + subY / 2 + subH / 4;
                        subDefectInfo.CenterPoint.Add(CenterPointX);
                        subDefectInfo.CenterPoint.Add(CenterPointY);

                        //更新中台数据
                        if (panelInfo.SideIndex == "A")
                        {
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].SubDefectsInfo.Add(subDefectInfo);
                        }
                        else
                        {
                            int Bcount = panelInfo.PcsInfo["1"].DefectInfo.Count;
                            int ALLcount = dsCenterInfo.Data[0].Content["1"].DefectsInfo.Count;
                            int index = ALLcount - Bcount;
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].SubDefectsInfo.Add(subDefectInfo);
                        }
                    }
                }
                //B面做完判断总结果
                if (panelInfo.SideIndex == "B")
                {
                    if (dsCenterInfo.Data[0].Content["1"].DefectsInfo.Exists(o => o.AiResult.ToLower() == "ng"))
                    {
                        dsCenterInfo.Data[0].Content["1"].ConfirmResult = "ng";
                    }
                    else
                    {
                        dsCenterInfo.Data[0].Content["1"].ConfirmResult = "ok";
                    }
                    dsCenterInfo.Data[0].EndTime = time;
                    dsCenterInfo.Data[0].Content["1"].EndTime = time;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("中台数据处理异常" + ex.ToString());
            }
        }

        /// <summary>
        /// 根据 RootPanelInfoWithIP 拼装并返回该 Panel 所有相关图像在 Minio 中的键列表。
        /// 返回的每个字符串均为 showImage/showImage2 可直接使用的自定义格式："endpoint:objectKey"。
        /// 规则：
        /// - endpoint 使用参数 info.IP（Minio 的 endpoint）
        /// - objectKey 使用 RootPanelInfo.LocalDescribeDir 中的 "deepiresults" 之后的相对路径作为 head，再拼接各缺陷的相对图像路径
        /// - 包含所有缺陷的 defect/template/gerber 图像（若存在）
        /// 注意：本方法不访问 Minio，仅组装路径，便于后续显示或下载。
        /// </summary>
        /// <param name="info">包含 Minio IP 与 Panel 详细信息的对象</param>
        /// <returns>所有图像键的列表，元素格式为 "endpoint:objectKey"</returns>
        public List<string> GetAllMinioImageKeys(RootPanelInfoWithIP info)
        {
            var results = new List<string>();
            try
            {
                if (info == null || info.rootInfo == null || string.IsNullOrWhiteSpace(info.IP))
                {
                    LogTextHelper.Warn("GetAllMinioImageKeys: 参数为空或 IP 缺失。");
                    return results;
                }

                var panel = info.rootInfo;

                // 从 LocalDescribeDir 解析出相对 head（即 deepiresults 之后的对象前缀）
                // 例如：D:\minio\deepiresults\20250813\2i02j0aaanc00\A\20250813235326770521
                // 提取为：20250813/2i02j0aaanc00/A/20250813235326770521
                string head = ExtractHeadFromLocalDescribeDir(panel.LocalDescribeDir);
                if (string.IsNullOrWhiteSpace(head))
                {
                    LogTextHelper.Warn($"GetAllMinioImageKeys: 无法从 LocalDescribeDir 解析 head。LocalDescribeDir={panel.LocalDescribeDir}");
                    return results;
                }

                // 遍历 pcs -> defects，收集所有图片（defect/template/gerber）
                if (panel.PcsInfo == null || panel.PcsInfo.Count == 0)
                {
                    LogTextHelper.Info($"GetAllMinioImageKeys: PcsInfo 为空。SN={panel.SerialNumber}");
                    return results;
                }

                foreach (var kvp in panel.PcsInfo)
                {
                    var pcs = kvp.Value;
                    if (pcs == null || pcs.DefectInfo == null || pcs.DefectInfo.Count == 0)
                    {
                        continue;
                    }

                    for (int j = 0; j < pcs.DefectInfo.Count; j++)
                    {
                        var defect = pcs.DefectInfo[j];
                        if (defect == null)
                            continue;

                        // 三类图片集合：DefectVrsImages、DefectVrsOkImages、DefectVrsGerberImages
                        AddImages(defect.DefectVrsImages, info.IP, head, results);
                        //AddImages(defect.DefectVrsOkImages, info.IP, head, results);
                        //AddImages(defect.DefectVrsGerberImages, info.IP, head, results);
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("GetAllMinioImageKeys 异常：" + ex);
            }

            return results;

            // 将相对图片路径拼接为 endpoint:key 并加入结果
            void AddImages(List<string> images, string endpoint, string prefix, List<string> output)
            {
                if (images == null || images.Count == 0) return;
                foreach (var rel in images)
                {
                    if (string.IsNullOrWhiteSpace(rel)) continue;
                    // 统一分隔符并去除可能的前导斜杠
                    var normalizedRel = rel.Replace('\\', '/').TrimStart('/');
                    var objectKey = $"{prefix}/{normalizedRel}";
                    output.Add($"{endpoint}:{objectKey}");
                }
            }
        }

        /// <summary>
        /// 从 RootPanelInfo.LocalDescribeDir 提取 deepiresults 之后的对象前缀（以 / 分隔）。
        /// 示例输入：D:\minio\deepiresults\20250813\2i02j0aaanc00\A\20250813235326770521
        /// 输出：    20250813/2i02j0aaanc00/A/20250813235326770521
        /// </summary>
        private string ExtractHeadFromLocalDescribeDir(string localDescribeDir)
        {
            if (string.IsNullOrWhiteSpace(localDescribeDir)) return string.Empty;

            try
            {
                var normalized = localDescribeDir.Replace('\\', '/');
                var idx = normalized.IndexOf("deepiresults", StringComparison.OrdinalIgnoreCase);
                if (idx < 0) return string.Empty;

                // 取 deepiresults 之后的部分
                var after = normalized.Substring(idx + "deepiresults".Length).Trim('/');
                return after;
            }
            catch
            {
                return string.Empty;
            }
        }



        /// <summary>
        /// 核心：根据自定义的“endpoint:key”格式从 Minio 读取图片并解码为 Mat
        /// 传入参数 path 示例：  "127.0.0.1:port子路径/对象键"（当前调用代码中为 path.Split(':') 后两段）
        /// 实际格式为 showImage/showImage2 里使用的 path，形如 "192.168.1.10:folder1/folder2/image.jpg"
        /// </summary>
        /// <param name="path">自定义的冒号分隔路径（前半为 endpoint/标识，后半为对象键）</param>
        /// <returns>成功返回 Mat，失败返回 null</returns>
        private Mat LoadMinioImage(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            try
            {
                var parts = path.Split(':');
                if (parts.Length < 2)
                {
                    LogTextHelper.Warn("图片路径格式错误(需包含冒号)：" + path);
                    return null;
                }
                using (var stream = Minio.GetImageStreamSync("deepiresults", parts[1], parts[0]))
                {
                    if (stream == null || stream.Length == 0)
                    {
                        LogTextHelper.Warn("Minio返回空图片数据：" + path);
                        return null;
                    }
                    return Cv2.ImDecode(stream.ToArray(), ImreadModes.Color);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("加载 Minio 图片异常：" + ex);
                return null;
            }
        }

        #endregion

        #region 图像显示（委托给 ImageDisplayService）

        /// <summary>
        /// 显示图片（支持多图拼接）- 委托给 ImageDisplayService
        /// </summary>
        public void ShowImage2(int index, List<string> paths, string result = "", VBRcvInfp box = null)
        {
            ImageDisplay.ShowImage2(index, paths, result, box);
        }

        #endregion

        #region 数据库操作

        public void GenerateVRSTestData() => DatabaseHelper.GenerateEmployeeReportTestData(5000);

        public Task<(string SerialNumber, string LotNumber, string ProductSerial, string PathIndex)> GetLatestPanelInfoByMachineId(string machineId) =>
            _databaseHelper.GetLatestPanelInfoByMachineId(machineId);

        public void SaveEmployeeReport(EmployeeReport report) =>
            _databaseHelper.SaveEmployeeReport(report);

        /// <summary>
        /// 清空数据库所有表的数据
        /// </summary>
        public Task<bool> ClearAllDatabaseData() =>
            _databaseHelper.ClearAllData();

        /// <summary>
        /// 保存/更新 PanelSide 数据到数据库（支持覆盖现有数据）
        /// </summary>
        /// <param name="record">PanelSideRecord 记录</param>
        public void SavePanelSide(PanelSideRecord record) =>
            _databaseHelper.SavePanelSide(record);

        /// <summary>
        /// 保存 PanelSide 数据到数据库（从 RootPanelInfo 构建记录）
        /// </summary>
        /// <param name="panelInfo">面板信息</param>
        /// <param name="detectPoints">缺陷点列表</param>
        /// <param name="aviState">AVI 状态 (1: OK, 2: NG)</param>
        /// <param name="aiState">AI 状态 (1: OK, 2: NG, 3: Bypass)</param>
        private void SavePanelSideToDatabase(RootPanelInfo panelInfo, List<DetectInfo> detectPoints, int aviState, int aiState)
        {
            if (!DateTime.TryParse(panelInfo.AviCreateTime, out DateTime aviCreationTime))
            {
                aviCreationTime = DateTime.Now;
                LogTextHelper.Info($"无法解析 AviCreateTime '{panelInfo.AviCreateTime}'。将使用当前时间 '{aviCreationTime}' 作为备用。");
            }

            LogTextHelper.Info($"存储 SN={panelInfo.SerialNumber}, Side={panelInfo.SideIndex}, AviState={aviState}, AiState={aiState}, DefectCount={detectPoints?.Count ?? 0} 到数据库...");
            _databaseHelper.SavePanelSide(new PanelSideRecord()
            {
                Data = new SideData()
                {
                    Side = panelInfo.SideIndex,
                    DetectPoints = detectPoints,
                    AviState = aviState,
                    AiState = aiState,
                    VvsState = 0,
                    VrsState = 0,
                    FinalState = 0
                },
                ProductSerial = panelInfo.ProductSerial,
                DetectionDate = DateTime.Now,
                AviCreationTime = aviCreationTime,
                LotNumber = panelInfo.LotId,
                SerialNumber = panelInfo.SerialNumber,
                MachineId = panelInfo.StationName,
                Side = panelInfo.SideIndex,
                PathIndex = panelInfo.PathIndex
            });
        }
        public Task<List<PanelDataRecord>> GetPanelsData(DateTime start, DateTime end, string partnumber = null) =>
            _databaseHelper.GetPanelsData(start, end, partnumber);

        public Task<(string LotNumber, string ProductSerial)> GetLatestLotAndProductSerial(string machineId) =>
            _databaseHelper.GetLatestLotAndProductSerial(machineId);

        public Task<List<PanelDataRecord>> GetPanelsDataByMachineAndLot(string machineId, string lotNumber) =>
            _databaseHelper.GetPanelsDataByMachineAndLot(machineId, lotNumber);


        #endregion

        #region 后台线程处理

        /// <summary>
        /// 图片加载线程处理 (解耦图片读取和推理)
        /// </summary>
        private void ThreadImageLoad(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 使用 Thread.Sleep 替代 Task.Delay().Wait()，避免阻塞线程池线程
                    Thread.Sleep(15);
                    if (token.IsCancellationRequested) break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (!IsStart)
                {
                    continue;
                }

                if (_imageLoadQueue.TryDequeue(out ImageLoadModel loadModel))
                {
                    try
                    {
                        // 检查点1：出队后立即检查是否应该中止
                        if (!IsStart)
                        {
                            LogTextHelper.Info($"图片加载任务被暂停中止，SN:{loadModel.Model?.SN}");
                            SystemEvent.SendTaskMsg(loadModel.Model?.SN, "暂停-图片加载已中止");
                            // 清理已加载的资源
                            if (loadModel.Model?.Mats != null)
                            {
                                foreach (var mat in loadModel.Model.Mats)
                                {
                                    mat?.Dispose();
                                }
                                loadModel.Model.Mats.Clear();
                            }
                            continue;
                        }

                        LogTextHelper.Info($"开始加载图片，SN:{loadModel.Model.SN}，数量：{loadModel.Model.ImageKeys.Count}");
                        SystemEvent.SendTaskMsg(loadModel.Model.SN, $"{loadModel.Model.Side}面正在加载图片");

                        // 并行加载图片提高效率
                        loadModel.Model.Mats = loadModel.Model.ImageKeys
                            .AsParallel()
                            .AsOrdered()
                            .Select(t => LoadMinioImage(t))
                            .Where(m => m != null)
                            .ToList();

                        // 检查点2：图片加载完成后检查是否应该中止
                        if (!IsStart)
                        {
                            LogTextHelper.Info($"图片加载后任务被暂停中止，SN:{loadModel.Model.SN}");
                            SystemEvent.SendTaskMsg(loadModel.Model.SN, "暂停-图片加载后已中止");
                            // 释放已加载的图片资源
                            if (loadModel.Model.Mats != null)
                            {
                                foreach (var mat in loadModel.Model.Mats)
                                {
                                    mat?.Dispose();
                                }
                                loadModel.Model.Mats.Clear();
                            }
                            continue;
                        }

                        // 检查加载结果
                        int expectedCount = loadModel.Model.ImageKeys.Count;
                        int actualCount = loadModel.Model.Mats.Count;
                        if (expectedCount == 0)
                        {
                            // 本身就没有报点数据，不是错误
                            LogTextHelper.Info($"SN:{loadModel.Model.SN} 无报点数据，无需加载图片");
                        }
                        else if (actualCount == 0)
                        {
                            LogTextHelper.Error($"图片加载失败，SN:{loadModel.Model.SN}，期望{expectedCount}张图片，实际加载0张！");
                        }
                        else if (actualCount < expectedCount)
                        {
                            LogTextHelper.Warn($"图片部分加载失败，SN:{loadModel.Model.SN}，期望{expectedCount}张，实际{actualCount}张");
                        }

                        // 图片加载完成，入推理队列
                        _aviQueue.Enqueue(loadModel.Model);

                        // 发送 PanelInfo 事件
                        SystemEvent.SendPanelInfo(loadModel.Model.SN, loadModel.RootPanelInfo);

                        SystemEvent.SendTaskMsg(loadModel.Model.SN, $"{loadModel.Model.Side}面图片加载完成");
                        LogTextHelper.Info($"图片加载完成，SN:{loadModel.Model.SN}，实际加载:{loadModel.Model.Mats.Count}张，入队列_aviQueue成功");
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"图片加载异常，SN:{loadModel.Model?.SN}：" + ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 线程处理 (Task循环)
        /// </summary>
        private void ThreadReturnAVI(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 使用 Thread.Sleep 替代 Task.Delay().Wait()，避免阻塞线程池线程
                    Thread.Sleep(15);
                    if (token.IsCancellationRequested) break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (!IsStart)
                {
                    continue;
                }

                try
                {
                    if (_aiResultQueue.Count > 0)
                    {
                        if (_aiResultQueue.TryDequeue(out Tuple<string, string, string, RootAIResult, DsCenterInfo> info))
                        {
                            // 检查点：出队后检查是否应该中止
                            if (!IsStart)
                            {
                                LogTextHelper.Info($"结果回写任务被暂停中止，SN:{info?.Item2}");
                                SystemEvent.SendTaskMsg(info?.Item2, "暂停-结果回写已中止");
                                continue;
                            }
                            // 回写处理
                            ReturnAVI(info);

                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error(ex.ToString());
                }
            }
        }

        public bool ReturnAVI(Tuple<string, string, string, RootAIResult, DsCenterInfo> info)
        {
            try
            {
                SystemEvent.SendTaskMsg(info.Item2, $"{info.Item3}面已完成");
                if (HttpDb.HttpPostMethod(URL, info.Item4, 1, out string result))
                {
                    //返回更新界面信息
                    SystemEvent.SendTaskMsg(info.Item2, $"{info.Item3}面已完成");


                    string snKey = $"{info.Item2}_{info.Item3}";
                    // ⭐ 关键修改点6：无论成功失败，都要移除标记
                    if (_processingSnSet.TryRemove(snKey, out DateTime addTime))
                    {
                        var duration = DateTime.Now - addTime;
                        LogTextHelper.Info($"SN:{info.Item2} Side:{info.Item3} 处理完成，耗时：{duration.TotalSeconds:F2}秒，已从处理集合移除");
                    }
                    else
                    {
                        LogTextHelper.Warn($"SN:{info.Item2} Side:{info.Item3} 未在处理集合中找到，可能已被清理或未正确添加");
                    }

                    // 处理中台数据推送（B面时 Item5 不为空）
                    if (info.Item5 != null)
                    {
                        HttpDb.HttpPostMethod2(SysConfig.DsCenterUrl, info.Item5, 0, out string outInfo);
                        LogTextHelper.Info($"sn:{info.Item2}_中台数据返回信息:{outInfo}");
                    }

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 推理后处理线程（异步处理推理结果）
        /// </summary>
        private void ThreadPostProcess(CancellationToken token)
        {
            LogTextHelper.Info("推理后处理线程已启动");

            while (!token.IsCancellationRequested)
            {
                if (!IsStart)
                {
                    continue;
                }
                try
                {
                    if (_inferencePostProcessQueue.TryDequeue(out InferenceResultModel resultModel))
                    {
                        try
                        {
                            // 检查点：出队后检查是否应该中止
                            if (!IsStart)
                            {
                                LogTextHelper.Info($"后处理任务被暂停中止，SN:{resultModel.VBModel?.SN}");
                                SystemEvent.SendTaskMsg(resultModel.VBModel?.SN, "暂停-后处理已中止");
                                continue;
                            }
                            ProcessInferenceResult(resultModel);
                        }
                        catch (Exception ex)
                        {
                            LogTextHelper.Error($"后处理异常: SN={resultModel.VBModel?.SN}, 错误={ex}");
                        }
                    }
                    else
                    {
                        Thread.Sleep(10); // 队列为空时短暂休眠
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"后处理线程异常: {ex}");
                }
            }

            LogTextHelper.Info("推理后处理线程已停止");
        }

        /// <summary>
        /// 定期清理过期的处理标记（防止内存泄漏）
        /// </summary>
        private void ThreadCleanupProcessingCache(CancellationToken token)
        {
            LogTextHelper.Info("处理缓存清理线程已启动");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(60000); // 每分钟清理一次

                    if (token.IsCancellationRequested) break;

                    var now = DateTime.Now;
                    var expiredKeys = _processingSnSet
                        .Where(kvp => (now - kvp.Value).TotalMinutes > CACHE_EXPIRE_MINUTES)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in expiredKeys)
                    {
                        if (_processingSnSet.TryRemove(key, out DateTime addTime))
                        {
                            var duration = now - addTime;
                            LogTextHelper.Warn($"清理过期处理标记：{key}，已超时 {duration.TotalMinutes:F1} 分钟");
                        }
                    }

                    if (expiredKeys.Count > 0)
                    {
                        LogTextHelper.Info($"清理了 {expiredKeys.Count} 个过期处理标记，当前集合大小：{_processingSnSet.Count}");
                    }

                    // 输出统计信息
                    LogProcessingStatistics();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"清理处理缓存异常: {ex}");
                }
            }

            LogTextHelper.Info("处理缓存清理线程已停止");
        }

        /// <summary>
        /// 定期输出处理统计信息
        /// </summary>
        private void LogProcessingStatistics()
        {
            try
            {
                LogTextHelper.Info($"处理统计 - 处理中:{_processingSnSet.Count}, " +
                                  $"图片加载队列:{_imageLoadQueue.Count}, " +
                                  $"推理队列:{_aviQueue.Count}, " +
                                  $"后处理队列:{_inferencePostProcessQueue.Count}, " +
                                  $"结果队列:{_aiResultQueue.Count}"+
                                  $"数据库队列{_databaseHelper.GetQueueLength()}");

                // 告警：如果处理集合持续增长超过阈值
                if (_processingSnSet.Count > 100)
                {
                    LogTextHelper.Warn($"⚠️ 处理集合过大({_processingSnSet.Count})，可能存在处理阻塞或标记未清理！");

                    // 输出前10个最老的处理项
                    var oldestItems = _processingSnSet
                        .OrderBy(kvp => kvp.Value)
                        .Take(10)
                        .ToList();

                    foreach (var item in oldestItems)
                    {
                        var duration = DateTime.Now - item.Value;
                        LogTextHelper.Warn($"  - {item.Key}: 已处理 {duration.TotalMinutes:F1} 分钟");
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"输出统计信息异常: {ex}");
            }
        }

        /// <summary>
        /// 处理推理结果（从 DefectMethod 中提取的后处理逻辑）
        /// </summary>
        private void ProcessInferenceResult(InferenceResultModel resultModel)
        {
            VBModel vBModel = resultModel.VBModel;
            string msg = resultModel.RawJsonResult;
            RootPanelInfo panelInfo = vBModel.panelInfo;

            string snKey = $"{vBModel.SN}_{vBModel.Side}";

            try
            {
                // 如果不需要处理（例如图片数量为0或超过最大值）
                if (!resultModel.NeedsProcessing)
                {
                    if (vBModel.Mats.Count == 0)
                    {
                        SavePanelSideToDatabase(panelInfo, new List<DetectInfo>(), 1, 1);
                        LogTextHelper.Info($"后处理完成(无图片): SN={vBModel.SN}");
                    }
                    else if (vBModel.Mats.Count > SysConfig.MaxDefectCount)
                    {
                        var imageKeys=vBModel.ImageKeys.Select(t=>new DetectInfo() { ImagePath=t, AIStatus=3}).ToList();
                        SavePanelSideToDatabase(panelInfo, imageKeys, 2, 3);
                        LogTextHelper.Info($"后处理完成(图片超限): SN={vBModel.SN}");
                    }
                    return;
                }

                LogTextHelper.Info($"开始后处理: SN={vBModel.SN}, Side={panelInfo.SideIndex}");

                // 解析推理结果
                var obj = JsonConvert.DeserializeObject<RootVBOutInfo>(msg);
                if (obj == null)
                {
                    LogTextHelper.Error($"算法返回结果反序列化失败 for Side {panelInfo.SideIndex}，原始消息: {msg}");
                    return;
                }

                string code = obj.Code.ToString();
                string message = obj.Message.ToString();

                if (code == "200")
                {
                    List<DetectInfo> avi_HeatInfo = new List<DetectInfo>();

                    // 更新中台数据
                    UpdateDsCenterInfo(panelInfo, obj);

                    // 处理 HeatPoint 等信息
                    for (int i = 0; i < obj.Data.InferWholeData.InferResults.Count; i++)
                    {
                        DetectInfo heatInfo = new DetectInfo();
                        int index = panelInfo.LocalDescribeDir.IndexOf("deepiresults", StringComparison.OrdinalIgnoreCase);
                        if (index != -1)
                        {
                            string basePath = panelInfo.LocalDescribeDir.Substring(0, index + "deepiresults".Length);
                            string relativePath = obj.Data.InferWholeData.InferResults[i].GroupInfos[0].ImagePath.Replace('/', '\\');
                            string mergedPath = Path.Combine(basePath, relativePath);
                            heatInfo.ImagePath = mergedPath;
                        }

                        if (obj.Data.InferWholeData.InferResults[i].Infer_Result == "OK")
                        {
                            heatInfo.AIStatus = 1;
                        }
                        else
                        {
                            heatInfo.AIStatus = 2;
                            for (int j = 0; j < obj.Data.InferWholeData.InferResults[i].inferDetails.Location.Count; j++)
                            {
                                string sub_defectName = obj.Data.InferWholeData.InferResults[i].Defect_name;
                                int subX = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].X);
                                int subY = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Y);
                                int subH = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Height);
                                int subW = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Width);

                                if (j == 0)
                                {
                                    heatInfo.DefectName = sub_defectName;
                                    heatInfo.RoiX = subX / 2 + subW / 4;
                                    heatInfo.RoiY = subY / 2 + subH / 4;

                                    if (sub_defectName == "AU10" || sub_defectName == "CU10" || sub_defectName == "CU41"
                                        || sub_defectName == "HO01" || sub_defectName == "SM10")
                                    {
                                        heatInfo.DefectShape = "dot";
                                    }
                                    else
                                    {
                                        heatInfo.DefectShape = "line";
                                    }
                                }
                            }
                        }

                        if (vBModel.isByPass)
                        {
                            heatInfo.AIStatus = 3;
                        }

                        avi_HeatInfo.Add(heatInfo);
                    }

                    // 存数据到数据库
                    int aviState = avi_HeatInfo.Count == 0 ? 1 : 2;
                    int aiState = avi_HeatInfo.Any(h => h.AIStatus == 3) ? 3 : avi_HeatInfo.Any(h => h.AIStatus == 2) ? 2 : 1;
                    SavePanelSideToDatabase(panelInfo, avi_HeatInfo, aviState, aiState);

                    LogTextHelper.Info($"后处理完成: SN={vBModel.SN}, Side={panelInfo.SideIndex}, AviState={aviState}, AiState={aiState}");
                }
                else if (code == "600")
                {
                    // 无缺陷，AVI OK, AI OK
                    SavePanelSideToDatabase(panelInfo, new List<DetectInfo>(), 1, 1);
                    LogTextHelper.Info($"后处理完成(无缺陷): SN={vBModel.SN}, Side={panelInfo.SideIndex}");
                }
                else
                {
                    LogTextHelper.Warn($"算法调用失败 for Side {panelInfo.SideIndex}，返回码: {code}，返回信息：{message}");
                }
            }
            finally
            {

            }
        }

        /// <summary>
        /// 将推理结果加入后处理队列
        /// </summary>
        private void EnqueuePostProcess(VBModel vBModel, string rawJsonResult, bool needsProcessing)
        {
            var resultModel = new InferenceResultModel
            {
                RawJsonResult = rawJsonResult,
                VBModel = vBModel,
                InferenceCompletedTime = DateTime.Now,
                NeedsProcessing = needsProcessing
            };

            _inferencePostProcessQueue.Enqueue(resultModel);
            LogTextHelper.Info($"推理结果已加入后处理队列: SN={vBModel.SN}, QueueCount={_inferencePostProcessQueue.Count}");
        }

        /// <summary>
        /// 清空所有正在处理的队列（当 IsStart 设置为 false 时调用）
        /// </summary>
        private void ClearAllProcessingQueues()
        {
            try
            {
                LogTextHelper.Info("IsStart 设置为 false，开始清空所有处理队列...");

                // 用于收集被清除的 SN 列表
                HashSet<string> clearedSnSet = new HashSet<string>();

                int aviQueueCount = 0;
                int imageLoadQueueCount = 0;
                int aiResultQueueCount = 0;
                int postProcessQueueCount = 0;

                // 清空 _aviQueue 并释放 Mat 资源
                while (_aviQueue.TryDequeue(out var vbModel))
                {
                    if (vbModel != null)
                    {
                        if (!string.IsNullOrEmpty(vbModel.SN))
                        {
                            clearedSnSet.Add(vbModel.SN);
                        }
                        if (vbModel.Mats != null)
                        {
                            foreach (var mat in vbModel.Mats)
                            {
                                mat?.Dispose();
                            }
                            vbModel.Mats.Clear();
                        }
                    }
                    aviQueueCount++;
                }

                // 清空 _imageLoadQueue 并释放 Mat 资源
                while (_imageLoadQueue.TryDequeue(out var loadModel))
                {
                    if (loadModel?.Model != null)
                    {
                        if (!string.IsNullOrEmpty(loadModel.Model.SN))
                        {
                            clearedSnSet.Add(loadModel.Model.SN);
                        }
                        if (loadModel.Model.Mats != null)
                        {
                            foreach (var mat in loadModel.Model.Mats)
                            {
                                mat?.Dispose();
                            }
                            loadModel.Model.Mats.Clear();
                        }
                    }
                    imageLoadQueueCount++;
                }

                // 清空 _aiResultQueue
                while (_aiResultQueue.TryDequeue(out var aiResult))
                {
                    if (aiResult != null && !string.IsNullOrEmpty(aiResult.Item2))
                    {
                        clearedSnSet.Add(aiResult.Item2); // Item2 是 SN
                    }
                    aiResultQueueCount++;
                }

                // 清空 _inferencePostProcessQueue
                while (_inferencePostProcessQueue.TryDequeue(out var postProcess))
                {
                    if (postProcess?.VBModel != null && !string.IsNullOrEmpty(postProcess.VBModel.SN))
                    {
                        clearedSnSet.Add(postProcess.VBModel.SN);
                    }
                    postProcessQueueCount++;
                }

                // 清空 _processingSnSet
                int processingSnCount = _processingSnSet.Count;
                _processingSnSet.Clear();

                // 清空 _dsCenterInfoDict
                int dsCenterInfoCount = _dsCenterInfoDict.Count;
                _dsCenterInfoDict.Clear();

                LogTextHelper.Info($"所有处理队列已清空 - " +
                    $"AVI队列:{aviQueueCount}, " +
                    $"图片加载队列:{imageLoadQueueCount}, " +
                    $"AI结果队列:{aiResultQueueCount}, " +
                    $"后处理队列:{postProcessQueueCount}, " +
                    $"处理中集合:{processingSnCount}, " +
                    $"中台数据字典:{dsCenterInfoCount}");

                // 通知界面被清除的 SN
                foreach (var sn in clearedSnSet)
                {
                    SystemEvent.SendTaskMsg(sn, "暂停-已从队列移除");
                }

                if (clearedSnSet.Count > 0)
                {
                    LogTextHelper.Info($"已通知界面 {clearedSnSet.Count} 个SN被暂停移除：{string.Join(", ", clearedSnSet)}");
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"清空处理队列时发生异常: {ex}");
            }
        }

        #endregion

        #region 释放资源
        public void Dispose()
        {
            try
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                    try
                    {
                        // ⭐ 关键修改点8：等待所有任务完成，包括清理线程
                        Task.WaitAll(new[] {
                            _readAviTask,
                            _imageLoadTask,
                            _defectTask,
                            _returnAviTask,
                            _postProcessTask,
                            _cleanupTask  // 新增
                        }, 5000);
                    }
                    catch (AggregateException)
                    {
                        // 忽略因取消导致的任务异常
                    }
                    _cancellationTokenSource.Dispose();
                }

                // 清理队列中残留的 Mat 资源
                while (_aviQueue.TryDequeue(out var vbModel))
                {
                    if (vbModel?.Mats != null)
                    {
                        foreach (var mat in vbModel.Mats)
                        {
                            mat?.Dispose();
                        }
                        vbModel.Mats.Clear();
                    }
                }

                // 清理图片加载队列中残留的资源
                while (_imageLoadQueue.TryDequeue(out var loadModel))
                {
                    if (loadModel?.Model?.Mats != null)
                    {
                        foreach (var mat in loadModel.Model.Mats)
                        {
                            mat?.Dispose();
                        }
                        loadModel.Model.Mats.Clear();
                    }
                }

                // 清理后处理队列
                while (_inferencePostProcessQueue.TryDequeue(out var _))
                {
                    // 仅清空队列，不需要释放资源
                }
                LogTextHelper.Info("后处理队列已清空");

                // ⭐ 关键修改点9：清理处理集合
                _processingSnSet.Clear();
                LogTextHelper.Info($"已清理处理集合，释放资源完成");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Dispose an exception occurred during task cancellation:" + ex.ToString());
            }

        }
        #endregion
    }
}

using DeepSightCommunication;
using DeepSightCommunication.Interfaces;
using DeepSightDB;
using DeepSightDB.Interfaces;
using DeepSightDisplay;
using DeepSightEvent;
using DeepSightModel;
using DeepSightModel.Configuration;
using DeepSightTool;
using DeepSightWorkLib.Interfaces;
using DeepSightWorkLib.Services;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DeepSightWorkLib
{
    /// <summary>
    /// 业务处理类（Facade 模式）- 组合调用各个服务
    /// 职责：作为门面类协调各个服务，不直接处理业务逻辑
    /// </summary>
    public class BusinessClass : IDisposable
    {
        #region 私有字段 - 核心服务

        private readonly IDatabaseService _databaseHelper;

        /// <summary>
        /// 队列管理器 - 统一管理所有处理队列
        /// </summary>
        private readonly QueueManager _queueManager;

        /// <summary>
        /// Panel 数据转换服务
        /// </summary>
        private readonly IPanelDataConverter _panelDataConverter;

        /// <summary>
        /// 工作线程管理器
        /// </summary>
        private readonly WorkerThreadManager _workerManager;

        #endregion

        #region 私有字段 - 业务服务

        private AviReaderService _aviReaderService;
        private ImageLoaderService _imageLoaderService;
        private DefectProcessor _defectProcessor;
        private ResultWriterService _resultWriterService;
        private PostProcessService _postProcessService;

        /// <summary>
        /// 模型验证测试服务
        /// </summary>
        private ModelValidationTestService _validationTestService;

        #endregion

        #region 私有字段 - 数据库批量写入

        /// <summary>
        /// Panel 批量写入大小（来自配置）
        /// </summary>
        private static int PanelSideBatchSize => DefaultValues.PanelSideBatchSize;
        private readonly object _panelRecordLock = new object();
        private readonly List<PanelSideRecord> _pendingPanelSideRecords = new List<PanelSideRecord>();

        #endregion

        #region 私有字段 - 运行状态

        /// <summary>
        /// 缓存过期时间（分钟）- 来自配置
        /// </summary>
        private static int CACHE_EXPIRE_MINUTES => DefaultValues.CacheExpireMinutes;

        /// <summary>
        /// 开始/停止作业标志
        /// </summary>
        private volatile bool _isStart = false;

        /// <summary>
        /// AI 检测耗时计时器
        /// </summary>
        private Stopwatch AIStopwatch;

        #endregion

        #region 公共属性

        /// <summary>
        /// 算法检测服务（接口类型，支持依赖注入）
        /// </summary>
        public IDefectService DefectService { get; private set; }

        /// <summary>
        /// HTTP 服务（接口类型，支持依赖注入）
        /// </summary>
        public IHttpService HttpService { get; private set; }

        /// <summary>
        /// Minio 服务（接口类型，支持依赖注入）
        /// </summary>
        public IMinioService MinioService { get; private set; }

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

                // 当从 false 变为 true 时，重置所有数据库的 fetchTime 为当前时间
                if (!previousValue && value)
                {
                    _aviReaderService.ResetAllFetchTimes();
                }
                // 停止后自动将暂存数据库信息写入
                if (previousValue&& !value)
                {
                    FlushPendingPanelSideRecords();
                }
            }
        }

        /// <summary>
        /// 是否允许处理
        /// </summary>
        public bool IsAllow { get; set; } = true;

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

        /// <summary>
        /// 模型验证测试服务（只读）
        /// </summary>
        public ModelValidationTestService ValidationTestService => _validationTestService;

        #endregion

        #region 构造函数和初始化

        /// <summary>
        /// 默认构造函数（使用具体实现）
        /// </summary>
        public BusinessClass() : this(
            new DefectClass(),
            new HttpClass(),
            new MinioClass(),
            null,
            null) // DatabaseHelper 和 PanelDataConverter 使用默认实现
        {
        }

        /// <summary>
        /// 依赖注入构造函数（支持测试和自定义实现）
        /// </summary>
        /// <param name="defectService">缺陷检测服务</param>
        /// <param name="httpService">HTTP 服务</param>
        /// <param name="minioService">Minio 服务</param>
        /// <param name="databaseService">数据库服务（可选，为 null 时使用默认实现）</param>
        /// <param name="panelDataConverter">Panel数据转换服务（可选，为 null 时使用默认实现）</param>
        public BusinessClass(
            IDefectService defectService,
            IHttpService httpService,
            IMinioService minioService,
            IDatabaseService databaseService,
            IPanelDataConverter panelDataConverter = null)
        {
            DefectService = defectService ?? throw new ArgumentNullException(nameof(defectService));
            HttpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
            MinioService = minioService ?? throw new ArgumentNullException(nameof(minioService));

            // 初始化核心管理器
            _queueManager = new QueueManager();
            _workerManager = new WorkerThreadManager();
            _panelDataConverter = panelDataConverter ?? new PanelDataConverter();

            // 需要具体的 MinioClass 实例来创建 ImageDisplayService
            var minioInstance = minioService as MinioClass ?? new MinioClass();

            // 数据库初始化
            if (databaseService == null)
            {
                DatabaseHelper.InitializeDatabase();
                _databaseHelper = new DatabaseHelper();
            }
            else
            {
                _databaseHelper = databaseService;
            }

            // 需要具体类型的实例来初始化服务
            var httpInstance = httpService as HttpClass ?? new HttpClass();
            var defectInstance = defectService as DefectClass ?? new DefectClass();

            // 初始化拆分后的服务（使用 QueueManager 中的队列）
            _aviReaderService = new AviReaderService(httpInstance, _queueManager.ProcessingSnSet, ReadJsonByMinio);
            _imageLoaderService = new ImageLoaderService(minioInstance);
            _defectProcessor = new DefectProcessor(defectInstance, _queueManager.AIResultQueue, _queueManager.PostProcessQueue);
            _postProcessService = new PostProcessService( SysConfig, SavePanelSideToDatabase);
        }

        /// <summary>
        /// 初始化工作线程
        /// </summary>
        public void InitWork()
        {
            this.IsStart = false;

            var httpInstance = HttpService as HttpClass ?? new HttpClass();
            _resultWriterService = new ResultWriterService(httpInstance, _queueManager.ProcessingSnSet);

            // 初始化模型验证测试服务
            var dbHelper = _databaseHelper as DatabaseHelper ?? new DatabaseHelper();
            _validationTestService = new ModelValidationTestService(
                dbHelper,
                _imageLoaderService,
                _queueManager,
                SolConfig,
                AviConfig);

            // 将验证测试服务注入到后处理服务
            _postProcessService.SetValidationTestService(_validationTestService);

            // 使用 WorkerThreadManager 管理线程
            _workerManager.Start();
            AIStopwatch = new Stopwatch();

            // 注册各个工作线程（轮询间隔来自 DefaultValues 配置）
            _workerManager.RegisterPollingWorker(() => WorkerReadAVI(),
                new WorkerConfig { Name = "ReadAVI", PollIntervalMs = DefaultValues.ReadAviPollIntervalMs });

            _workerManager.RegisterPollingWorker(() => WorkerImageLoad(),
                new WorkerConfig { Name = "ImageLoad", PollIntervalMs = DefaultValues.ImageLoadPollIntervalMs });

            _workerManager.RegisterPollingWorker(() => WorkerDefect(),
                new WorkerConfig { Name = "Defect", PollIntervalMs = DefaultValues.DefectPollIntervalMs });

            _workerManager.RegisterPollingWorker(() => WorkerReturnAVI(),
                new WorkerConfig { Name = "ReturnAVI", PollIntervalMs = DefaultValues.ReturnAviPollIntervalMs });

            _workerManager.RegisterPollingWorker(() => WorkerPostProcess(),
                new WorkerConfig { Name = "PostProcess", PollIntervalMs = DefaultValues.PostProcessPollIntervalMs });

            _workerManager.RegisterPollingWorker(() => WorkerCleanupCache(),
                new WorkerConfig { Name = "CleanupCache", PollIntervalMs = DefaultValues.CleanupCachePollIntervalMs, IsLongRunning = true });

            LogTextHelper.Info("BusinessClass 初始化完成，所有线程已启动");
        }


        #endregion

        #region Worker 方法（由 WorkerThreadManager 调度）

        /// <summary>
        /// 读取 AVI 数据工作单元 - 从所有配置的 LevelDB 数据库读取
        /// </summary>
        private bool WorkerReadAVI()
        {
            if (!IsStart || !IsAllow) return false;

            try
            {
                // 使用新的多数据库读取方法
                return _aviReaderService.ReadAllAVI();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"WorkerReadAVI 异常: {ex}");
            }
            return false;
        }

        /// <summary>
        /// 图片加载工作单元
        /// </summary>
        private bool WorkerImageLoad()
        {
            if (_queueManager.ImageLoadQueue.TryDequeue(out ImageLoadModel loadModel))
            {
                try
                {
                    LogTextHelper.Info($"开始加载图片，SN:{loadModel.Model.SN}，数量：{loadModel.Model.ImageKeys.Count}");
                    TaskStatusSender.SendLoadingImages(loadModel.Model.SN, loadModel.Model.Side);

                    loadModel.Model.Mats = _imageLoaderService.LoadImages(loadModel.Model.ImageKeys);
                    loadModel.Model.Mats_Temp = _imageLoaderService.LoadImages(loadModel.Model.ImageKeys_Temp);
                    LogImageLoadResult(loadModel);

                    _queueManager.AviQueue.Enqueue(loadModel.Model);
                    SystemEvent.SendPanelInfo(loadModel.Model.SN, loadModel.Model.Side, loadModel.RootPanelInfo);
                    TaskStatusSender.SendImagesLoaded(loadModel.Model.SN, loadModel.Model.Side, loadModel.Model.Mats.Count);

                    LogTextHelper.Info($"图片加载完成，SN:{loadModel.Model.SN}，实际加载:{loadModel.Model.Mats.Count}张");
                    return true;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"图片加载异常，SN:{loadModel.Model?.SN}：{ex}");
                    HandleProductError(loadModel.Model?.SN, loadModel.Model?.Side, "图片加载", ex.Message);
                }
            }
            return false;
        }

        /// <summary>
        /// AI 检测工作单元
        /// </summary>
        private bool WorkerDefect()
        {
            // 检查队列是否有任务（先 Peek 而不是 Dequeue）
            if (!_queueManager.AviQueue.TryPeek(out _))
                return false;

            if (_queueManager.AviQueue.TryDequeue(out VBModel info))
            {
                try
                {
                    // 验证测试任务使用不同的日志前缀
                    string taskPrefix = info.IsValidationTest ? "[验证测试]" : "";
                    TaskStatusSender.SendAIDetecting(info.SN, info.Side);
                    AIStopwatch.Restart();

                    if (_defectProcessor.DefectMethod(info, SysConfig.MaxDefectCount,
                        out List<string> msg,
                        AviConfig.GetInferResultTimeout))
                    {
                        TaskStatusSender.SendAICompleted(info.SN, info.Side);
                        if (!info.IsValidationTest)
                        {
                            SystemEvent.SendResultInfo(info.SN, info.Side, msg);
                        }
                    }
                    else
                    {
                        LogTextHelper.Error($"KEY:{info.Key} SN:{info.SN}检测失败！");
                        TaskStatusSender.SendFailed(info.SN, info.Side, "检测失败");
                    }

                    // 验证测试任务不需要入队 AIResultQueue（不需要回写到LDB）
                    if (!info.IsValidationTest)
                    {
                        _defectProcessor.EnqueueAIResult(info, msg);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"AI检测异常：{ex}");
                    HandleProductError(info?.SN, info?.Side, "AI检测", ex.Message);
                }
                finally
                {
                    AIStopwatch.Stop();
                    var elapsedMs = AIStopwatch.ElapsedMilliseconds;
                    if (elapsedMs > 20)
                    {
                        TaskStatusSender.SendAICompleted(info?.SN, info?.Side, elapsedMs);
                    }
                    CleanupMats(info?.Mats);
                }
            }
            return false;
        }

        /// <summary>
        /// 结果回写工作单元
        /// </summary>
        private bool WorkerReturnAVI()
        {
            if (_queueManager.AIResultQueue.TryDequeue(out var info))
            {
                try
                {
                    _resultWriterService?.ReturnAVI(info);
                    return true;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"结果回写异常：{ex}");
                    HandleProductError(info?.Item2.SN, null, "结果回写", ex.Message);
                }
            }
            return false;
        }

        /// <summary>
        /// 后处理工作单元
        /// </summary>
        private bool WorkerPostProcess()
        {
            // 检查队列是否有任务
            if (!_queueManager.PostProcessQueue.TryPeek(out _))
                return false;

            if (_queueManager.PostProcessQueue.TryDequeue(out InferenceResultModel resultModel))
            {
                try
                {
                    _postProcessService.ProcessInferenceResult(resultModel);
                    return true;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"后处理异常: SN={resultModel.VBModel?.SN}, 错误={ex}");
                    HandleProductError(resultModel.VBModel?.SN, resultModel.VBModel?.Side, "后处理", ex.Message);
                }
            }
            return false;
        }

        /// <summary>
        /// 缓存清理工作单元
        /// </summary>
        private bool WorkerCleanupCache()
        {
            try
            {
                int cleanedCount = _queueManager.CleanupExpiredProcessing(CACHE_EXPIRE_MINUTES);
                if (cleanedCount > 0)
                {
                    LogTextHelper.Info($"清理了 {cleanedCount} 个过期处理标记");
                }
                LogProcessingStatistics();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"清理缓存异常: {ex}");
            }
            return false; // 始终返回 false，保持定时轮询
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 清理 Mat 资源
        /// </summary>
        private void CleanupMats(List<Mat> mats)
        {
            if (mats == null) return;
            foreach (var mat in mats)
            {
                mat?.Dispose();
            }
            mats.Clear();
        }

        /// <summary>
        /// 记录图片加载结果
        /// </summary>
        private void LogImageLoadResult(ImageLoadModel loadModel)
        {
            int expectedCount = loadModel.Model.ImageKeys.Count;
            int actualCount = loadModel.Model.Mats.Count;

            if (expectedCount == 0)
            {
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
        }

        #endregion

        #region Minio 操作与数据转换

        /// <summary>
        /// 通过Minio读取Json文件
        /// </summary>
        /// <param name="writeBackDbName">回写目标数据库名称</param>
        /// <param name="dbUrl">源数据库服务器 URL</param>
        public void ReadJsonByMinio(string ip, string port, string key, string head, string sn, string side, string path, string writeBackDbName, string dbUrl)
        {
            try
            {
                TaskStatusSender.SendQueued(sn, side);
                string json = MinioService.ReadJsonSync("deepiresults", path, ip);

                if (string.IsNullOrWhiteSpace(json))
                {
                    string errMsg = $"SN={sn}, Side={side}: 从Minio读取的JSON为空，路径={path}, IP={ip}";
                    LogTextHelper.Error(errMsg);
                    HandleProductError(sn, side, "通过Minio读取Json文件", "读取的JSON内容为空");
                    return;
                }

                var obj = JsonConvert.DeserializeObject<RootPanelInfo>(json);

                if (obj == null)
                {
                    string errMsg = $"SN={sn}, Side={side}: JSON反序列化结果为null，路径={path}";
                    LogTextHelper.Error(errMsg);
                    HandleProductError(sn, side, "通过Minio读取Json文件", "JSON反序列化为RootPanelInfo失败，结果为null");
                    return;
                }

                LogTextHelper.Info($"{sn} {side} 开始将json转为vbinfo");

                // 使用 PanelDataConverter 进行转换
                var context = new PanelConvertContext
                {
                    MinioIP = ip,
                    MinioPort = port,
                    Head = head,
                    SolutionConfig = SolConfig,
                    AviConfig = AviConfig,
                    OnSolutionConfigChanged = SaveSolutionConfig,
                };
                var convertResult = _panelDataConverter.Convert(obj, context);

                RootPanelInfoWithIP rootobj = new RootPanelInfoWithIP()
                {
                    IP = ip,
                    Head=head,
                    RootInfo = obj,
                };

                // 解耦：先获取图片Key列表，入图片加载队列，非阻塞
                var imageKeys = _imageLoaderService.GetAllMinioImageKeys(rootobj);
                var imageKeys_Gerber = _imageLoaderService.GetAllMinioGerberImageKeys(rootobj);
                var imageKeys_Temp = _imageLoaderService.GetAllMinioTemplateImageKeys(rootobj);

                //考虑用Model方式
                VBModel model = new VBModel
                {
                    Key = key,
                    SN = sn,
                    Side = side,
                    DefectIndex = convertResult.DefectIndexList,
                    PcsIndex = convertResult.PcsIndexList,
                    VbInfo = convertResult.VBInfo,
                    minioPath = head,
                    panelInfo = obj,
                    ImageKeys = imageKeys,
                    ImageKeys_Gerber = imageKeys_Gerber,
                    ImageKeys_Temp = imageKeys_Temp,
                    SourceDbUrl = dbUrl,
                    SourceWriteBackDbName = writeBackDbName,
                    DirectReportDefectIndices = convertResult.DirectReportDefectIndices,
                    DirectReportPcsIndices = convertResult.DirectReportPcsIndices
                };

                // 存储调试信息到缓存
                try
                {
                    var debugInfo = SnDebugInfoCache.GetOrCreate(sn, side);
                    debugInfo.PanelInfoJson = json;
                    debugInfo.VbInferenceJson = JsonConvert.SerializeObject(convertResult.VBInfo, Formatting.Indented);
                    debugInfo.DefectCount = convertResult.DefectIndexList?.Count ?? 0;
                    debugInfo.PcsCount = convertResult.PcsIndexList?.Count ?? 0;
                    debugInfo.ImageCount = imageKeys?.Count ?? 0;
                    debugInfo.MinioPath = head;
                    debugInfo.ProductSerial = obj.ProductSerial;
                    debugInfo.LotNumber = obj.LotId ?? obj.LotBatch;

                    // 构建判断过程摘要
                    var defectCodes = new System.Collections.Generic.List<string>();
                    if (obj.PcsInfo != null)
                    {
                        foreach (var pcs in obj.PcsInfo.Values)
                        {
                            if (pcs?.DefectInfo != null)
                            {
                                foreach (var d in pcs.DefectInfo)
                                {
                                    if (!string.IsNullOrEmpty(d.DefectCode))
                                        defectCodes.Add(d.DefectCode);
                                }
                            }
                        }
                    }
                    var summary = new System.Text.StringBuilder();
                    if (defectCodes.Count > 0)
                        summary.Append($"AVI报点{defectCodes.Count}个: {string.Join(",", defectCodes.Distinct())}");
                    else
                        summary.Append("AVI无报点");
                    if (convertResult.DirectReportDefectIndices?.Count > 0)
                        summary.Append($" | 直报{convertResult.DirectReportDefectIndices.Count}个");
                    debugInfo.JudgmentSummary = summary.ToString();

                    SnDebugInfoCache.Cleanup();
                }
                catch (Exception debugEx)
                {
                    LogTextHelper.Warn($"存储SN调试信息异常: {debugEx.Message}");
                }

                var loadModel = new ImageLoadModel
                {
                    Model = model,
                    RootPanelInfo = rootobj
                };
                _queueManager.ImageLoadQueue.Enqueue(loadModel);

                LogTextHelper.Info($"{sn} {side} ReadJsonByMinio完成,入队列成功,待加载图片数量:{imageKeys.Count}");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常" + ex.ToString());
                HandleProductError(sn, side, "通过Minio读取Json文件", ex.Message);
            }
        }

        #endregion

        #region 算法调用与结果处理

        public bool DefectMethod(VBModel vBModel, out List<string> resList) => _defectProcessor.DefectMethod(vBModel, SysConfig.MaxDefectCount, out resList);

        #endregion

        #region 配置保存

        /// <summary>
        /// 保存方案配置到文件（用于料号自动新增后持久化）
        /// </summary>
        private void SaveSolutionConfig(SolutionConfig config)
        {
            var solClass = new DeepSight_Solution();
            solClass.Save(config);
        }

        #endregion

        #region 数据库操作

        public void GenerateVRSTestData() => DatabaseHelper.GenerateEmployeeReportTestData(5000);

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
        /// 获取数据库服务实例（用于 CSV 数据导入等场景）
        /// </summary>
        public IDatabaseService GetDatabaseService() => _databaseHelper;

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

            // 聚合所有 DetectInfo 的 DrawInfo 到 SideData 级别
            string drawInfoJson = null;
            if (detectPoints != null)
            {
                var drawInfoList = detectPoints
                    .Where(d => !string.IsNullOrEmpty(d.DrawInfo))
                    .Select(d => d.DrawInfo)
                    .Distinct()
                    .ToList();
                if (drawInfoList.Count > 0)
                {
                    drawInfoJson = JsonConvert.SerializeObject(drawInfoList);
                }
            }

            LogTextHelper.Info($"存储 SN={panelInfo.SerialNumber}, Side={panelInfo.SideIndex}, AviState={aviState}, AiState={aiState}, DefectCount={detectPoints?.Count ?? 0} 到数据库...");
            var record = new PanelSideRecord()
            {
                Data = new SideData()
                {
                    Side = panelInfo.SideIndex,
                    DetectPoints = detectPoints,
                    AviState = aviState,
                    AiState = aiState,
                    VvsState = 0,
                    VrsState = 0,
                    FinalState = 0,
                    DrawInfo = drawInfoJson
                },
                ProductSerial = panelInfo.ProductSerial,
                DetectionDate = DateTime.Now,
                AviCreationTime = aviCreationTime,
                LotNumber = panelInfo.LotId,
                SerialNumber = panelInfo.SerialNumber,
                MachineId = panelInfo.MachineName,
                Side = panelInfo.SideIndex,
            };
            BoardStatCache.Update(record);

            // 将推理后的ROI信息发送到UI
            if (detectPoints != null && detectPoints.Count > 0)
            {
                var rois = detectPoints.Select(dp => new Roi
                {
                    X = dp.RoiX,
                    Y = dp.RoiY,
                    Width = dp.Width,
                    Height = dp.Height
                }).ToList();
                SystemEvent.SendRoiInfo(panelInfo.SerialNumber, panelInfo.SideIndex, rois);
            }

            // 将DetectInfo信息发送到UI（用于图片放大和单图测试）
            SystemEvent.SendDetectInfo(panelInfo.SerialNumber, panelInfo.SideIndex, detectPoints ?? new List<DetectInfo>());

            List<PanelSideRecord> batchToFlush = null;
            lock (_panelRecordLock)
            {
                _pendingPanelSideRecords.Add(record);
                if (_pendingPanelSideRecords.Count >= PanelSideBatchSize)
                {
                    batchToFlush = new List<PanelSideRecord>(_pendingPanelSideRecords);
                    _pendingPanelSideRecords.Clear();
                }
            }

            if (batchToFlush != null)
            {
                _databaseHelper.SavePanelSidesBatch(batchToFlush);
            }
        }

        private void FlushPendingPanelSideRecords()
        {
            List<PanelSideRecord> snapshot = null;
            lock (_panelRecordLock)
            {
                if (_pendingPanelSideRecords.Count > 0)
                {
                    snapshot = new List<PanelSideRecord>(_pendingPanelSideRecords);
                    _pendingPanelSideRecords.Clear();
                }
            }

            if (snapshot != null)
            {
                _databaseHelper.SavePanelSidesBatch(snapshot);
            }
        }

        public Task<List<PanelDataRecord>> GetPanelsData(DateTime start, DateTime end, string partnumber = null) =>
            _databaseHelper.GetPanelsData(start, end, partnumber);

        public Task<List<PanelDataRecord>> GetPanelsDataByMachineAndLot(string machineId, string lotNumber) =>
            _databaseHelper.GetPanelsDataByMachineAndLot(machineId, lotNumber);

        public Task<List<string>> GetRecentLotNumbers(int page, int pageSize) =>
            _databaseHelper.GetRecentLotNumbers(page, pageSize);

        public Task<int> GetTotalLotCount() =>
            _databaseHelper.GetTotalLotCount();
        #endregion

        #region 统计信息

        /// <summary>
        /// 定期输出处理统计信息
        /// </summary>
        private void LogProcessingStatistics()
        {
            try
            {
                var stats = _queueManager.GetStatistics();
                LogTextHelper.Info($"处理统计 - {stats}, 数据库队列:{_databaseHelper.GetQueueLength()}");

                // 告警：如果处理集合持续增长超过阈值
                if (stats.ProcessingSnCount > 100)
                {
                    LogTextHelper.Warn($"⚠️ 处理集合过大({stats.ProcessingSnCount})，可能存在处理阻塞或标记未清理！");

                    // 输出前10个最老的处理项
                    var oldestItems = _queueManager.ProcessingSnSet
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

        #endregion

        #region 释放资源

        public void Dispose()
        {
            try
            {
                // 停止所有工作线程
                _workerManager?.Stop();

                // 刷新待写入的数据库记录
                FlushPendingPanelSideRecords();

                // 清空所有队列并释放资源
                _queueManager?.ClearAllQueues();

                // 释放管理器资源
                _workerManager?.Dispose();
                _queueManager?.Dispose();

                LogTextHelper.Info("BusinessClass 资源已释放");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"Dispose 异常: {ex}");
            }
        }

        #endregion

        #region 异常处理

        /// <summary>
        /// 统一的产品异常处理方法
        /// 1. 从所有队列中移除该SN的数据
        /// 2. 在SnDebugInfo中记录错误信息
        /// 3. 向UI发送失败状态
        /// </summary>
        /// <param name="sn">产品序列号</param>
        /// <param name="side">面别（A/B），可为null</param>
        /// <param name="errorStep">出错步骤（如：图片加载、AI检测、结果回写、后处理）</param>
        /// <param name="errorMessage">错误原因描述</param>
        private void HandleProductError(string sn, string side, string errorStep, string errorMessage)
        {
            try
            {
                if (string.IsNullOrEmpty(sn)) return;

                // 1. 从所有队列中移除该SN的数据
                int removedCount = _queueManager.RemoveSnFromAllQueues(sn);
                LogTextHelper.Error($"产品异常处理: SN={sn}, 步骤={errorStep}, 原因={errorMessage}, 队列移除={removedCount}项");

                // 2. 在SnDebugInfo中记录错误信息
                string effectiveSide = string.IsNullOrEmpty(side) ? "A" : side;
                var debugInfo = SnDebugInfoCache.GetOrCreate(sn, effectiveSide);
                debugInfo.HasError = true;
                debugInfo.ErrorStep = errorStep;
                debugInfo.ErrorMessage = errorMessage;
                debugInfo.ErrorTime = DateTime.Now;
                // 更新判断摘要为错误信息
                debugInfo.JudgmentSummary = $"[异常] {errorStep}: {errorMessage}";

                // 3. 向UI发送失败状态（显示红色报错状态）
                TaskStatusSender.SendFailed(sn, effectiveSide, $"[{errorStep}] {errorMessage}");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"HandleProductError 自身异常: SN={sn}, {ex}");
            }
        }

        #endregion

        #region 推理结果处理相关

        /// <summary>
        /// 清空所有正在处理的队列（当 IsStart 设置为 false 时调用）
        /// </summary>
        private void ClearAllProcessingQueues()
        {
            try
            {
                LogTextHelper.Info("IsStart 设置为 false，开始清空所有处理队列...");

                // 使用 QueueManager 清空所有队列
                var clearedSnSet = _queueManager.ClearAllQueues();

                // 通知界面被清除的 SN
                foreach (var sn in clearedSnSet)
                {
                    TaskStatusSender.SendSkipped(sn, "", "暂停-已从队列移除");
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
    }
}

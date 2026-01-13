using DeepSightCommunication;
using DeepSightCommunication.Interfaces;
using DeepSightDB;
using DeepSightDB.Interfaces;
using DeepSightDisplay;
using DeepSightEvent;
using DeepSightModel;
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

        private ImageDisplayService ImageDisplay { get; set; }
        private AviReaderService _aviReaderService;
        private ImageLoaderService _imageLoaderService;
        private DefectProcessor _defectProcessor;
        private ResultWriterService _resultWriterService;
        private PostProcessService _postProcessService;

        #endregion

        #region 私有字段 - 数据库批量写入

        private const int PanelSideBatchSize = 100;
        private readonly object _panelRecordLock = new object();
        private readonly List<PanelSideRecord> _pendingPanelSideRecords = new List<PanelSideRecord>();

        #endregion

        #region 私有字段 - 运行状态

        /// <summary>
        /// 缓存过期时间（分钟）
        /// </summary>
        private const int CACHE_EXPIRE_MINUTES = 30;

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

                // 当从 true 变为 false 时，清空所有正在处理的队列
                if (previousValue && !value)
                {
                    ClearAllProcessingQueues();
                    FlushPendingPanelSideRecords();
                }
                // 当从 false 变为 true 时，更新推理请求开始时间
                if (!previousValue && value)
                {
                    _aviReaderService.fetchTime = DateTime.Now;
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
            ImageDisplay = new ImageDisplayService(minioInstance);

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
            _defectProcessor = new DefectProcessor(defectInstance, ImageDisplay, _imageLoaderService,
                _queueManager.AviQueue, _queueManager.AIResultQueue, _queueManager.PostProcessQueue);
            _postProcessService = new PostProcessService(_panelDataConverter, SysConfig, SavePanelSideToDatabase);
        }

        /// <summary>
        /// 初始化工作线程
        /// </summary>
        public void InitWork()
        {
            this.IsStart = false;

            var httpInstance = HttpService as HttpClass ?? new HttpClass();
            var ldbUrl = $"{SysConfig.ServerIP}:{SysConfig.ServerPort}";
            _resultWriterService = new ResultWriterService(httpInstance, _queueManager.ProcessingSnSet, ldbUrl, SysConfig.DsCenterUrl);

            // 使用 WorkerThreadManager 管理线程
            _workerManager.Start();
            AIStopwatch = new Stopwatch();

            // 注册各个工作线程
            _workerManager.RegisterPollingWorker(() => WorkerReadAVI(),
                new WorkerConfig { Name = "ReadAVI", PollIntervalMs = 100 });

            _workerManager.RegisterPollingWorker(() => WorkerImageLoad(),
                new WorkerConfig { Name = "ImageLoad", PollIntervalMs = 15 });

            _workerManager.RegisterPollingWorker(() => WorkerDefect(),
                new WorkerConfig { Name = "Defect", PollIntervalMs = 15 });

            _workerManager.RegisterPollingWorker(() => WorkerReturnAVI(),
                new WorkerConfig { Name = "ReturnAVI", PollIntervalMs = 15 });

            _workerManager.RegisterPollingWorker(() => WorkerPostProcess(),
                new WorkerConfig { Name = "PostProcess", PollIntervalMs = 10 });

            _workerManager.RegisterPollingWorker(() => WorkerCleanupCache(),
                new WorkerConfig { Name = "CleanupCache", PollIntervalMs = 60000, IsLongRunning = true });

            LogTextHelper.Info("BusinessClass 初始化完成，所有线程已启动");
        }

        /// <summary>
        /// 设置显示窗口列表（委托给 ImageDisplayService）
        /// </summary>
        public void SetHWindow(List<CvDisplay> displaysList)
        {
            ImageDisplay.SetDisplayList(displaysList);
        }

        #region 兼容性方法（已弃用）
        /// <summary>
        /// 显示图片（委托给 ImageDisplayService）
        /// </summary>
        public void ShowImage(string path, int index, string result = "", VBRcvInfp box = null)
        {
            ImageDisplay.ShowImage(path, index, result, box);
        }

        #endregion

        #endregion

        #region Worker 方法（由 WorkerThreadManager 调度）

        /// <summary>
        /// 读取 AVI 数据工作单元
        /// </summary>
        private bool WorkerReadAVI()
        {
            if (!IsStart || !IsAllow) return false;

            try
            {
                var ldbUrl = $"{SysConfig.ServerIP}:{SysConfig.ServerPort}";
                if (_aviReaderService.ReadAVI(ldbUrl, out string result))
                {
                    _aviReaderService.DoAviJsonTyped(result);
                    return true;
                }
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
            if (!IsStart) return false;

            if (_queueManager.ImageLoadQueue.TryDequeue(out ImageLoadModel loadModel))
            {
                try
                {
                    if (!IsStart)
                    {
                        LogTextHelper.Info($"图片加载任务被暂停中止，SN:{loadModel.Model?.SN}");
                        TaskStatusSender.SendSkipped(loadModel.Model?.SN, loadModel.Model?.Side, "任务已暂停");
                        CleanupMats(loadModel.Model?.Mats);
                        return false;
                    }

                    LogTextHelper.Info($"开始加载图片，SN:{loadModel.Model.SN}，数量：{loadModel.Model.ImageKeys.Count}");
                    TaskStatusSender.SendLoadingImages(loadModel.Model.SN, loadModel.Model.Side);

                    loadModel.Model.Mats = _imageLoaderService.LoadImages(loadModel.Model.ImageKeys);

                    if (!IsStart)
                    {
                        LogTextHelper.Info($"图片加载后任务被暂停中止，SN:{loadModel.Model.SN}");
                        TaskStatusSender.SendSkipped(loadModel.Model.SN, loadModel.Model.Side, "任务已暂停");
                        CleanupMats(loadModel.Model.Mats);
                        return false;
                    }

                    LogImageLoadResult(loadModel);

                    _queueManager.AviQueue.Enqueue(loadModel.Model);
                    SystemEvent.SendPanelInfo(loadModel.Model.SN, loadModel.RootPanelInfo);
                    TaskStatusSender.SendImagesLoaded(loadModel.Model.SN, loadModel.Model.Side, loadModel.Model.Mats.Count);

                    LogTextHelper.Info($"图片加载完成，SN:{loadModel.Model.SN}，实际加载:{loadModel.Model.Mats.Count}张");
                    return true;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"图片加载异常，SN:{loadModel.Model?.SN}：{ex}");
                }
            }
            return false;
        }

        /// <summary>
        /// AI 检测工作单元
        /// </summary>
        private bool WorkerDefect()
        {
            if (!IsStart) return false;

            if (_queueManager.AviQueue.TryDequeue(out VBModel info))
            {
                try
                {
                    if (!IsStart)
                    {
                        LogTextHelper.Info($"AI检测任务被暂停中止，SN:{info?.SN}");
                        SystemEvent.SendTaskMsg(info?.SN, "暂停-AI检测已中止");
                        return false;
                    }

                    SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面开始AI检测(缺陷数:{info.Mats.Count})");
                    AIStopwatch.Restart();

                    if (_defectProcessor.DefectMethod(info, SysConfig.MaxDefectCount,
                        out List<string> msg, out List<string> details, out PcsResult pcsResult, out string vbJson,
                        AviConfig.GetInferResultTimeout))
                    {
                        if (!IsStart)
                        {
                            LogTextHelper.Info($"AI检测任务被暂停中止（推理完成后），SN:{info.SN}");
                            SystemEvent.SendTaskMsg(info.SN, "暂停-AI检测已中止（结果未回写）");
                            return false;
                        }
                        SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面AI检测完成");
                        SystemEvent.SendResultInfo(info.SN, msg, details, pcsResult);
                    }
                    else
                    {
                        LogTextHelper.Warn($"KEY:{info.Key} SN:{info.SN}检测失败！");
                        SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面已完成");
                    }
                    _defectProcessor.EnqueueAIResult(info, msg);
                    return true;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"AI检测异常：{ex}");
                    SystemEvent.SendTaskMsg(info?.SN, $"{info?.Side}面已完成");
                }
                finally
                {
                    AIStopwatch.Stop();
                    var elapsedMs = AIStopwatch.ElapsedMilliseconds;
                    if (elapsedMs > 20)
                    {
                        SystemEvent.SendTaskMsg(info?.SN, $"{info?.Side}面AI耗时:{elapsedMs}ms", elapsedMs);
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
            if (!IsStart) return false;

            if (_queueManager.AIResultQueue.TryDequeue(out var info))
            {
                try
                {
                    if (!IsStart)
                    {
                        LogTextHelper.Info($"结果回写任务被暂停中止，SN:{info?.Item2}");
                        TaskStatusSender.SendSkipped(info?.Item2, info?.Item3, "任务已暂停");
                        return false;
                    }
                    _resultWriterService?.ReturnAVI(info);
                    return true;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"结果回写异常：{ex}");
                }
            }
            return false;
        }

        /// <summary>
        /// 后处理工作单元
        /// </summary>
        private bool WorkerPostProcess()
        {
            if (!IsStart) return false;

            if (_queueManager.PostProcessQueue.TryDequeue(out InferenceResultModel resultModel))
            {
                try
                {
                    if (!IsStart)
                    {
                        LogTextHelper.Info($"后处理任务被暂停中止，SN:{resultModel.VBModel?.SN}");
                        TaskStatusSender.SendSkipped(resultModel.VBModel?.SN, resultModel.VBModel?.panelInfo?.SideIndex, "任务已暂停");
                        return false;
                    }
                    _postProcessService.ProcessInferenceResult(resultModel);
                    return true;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"后处理异常: SN={resultModel.VBModel?.SN}, 错误={ex}");
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

        #region AVI 数据读取与处理

        /// <summary>
        /// 解析Minio路径
        /// </summary>
        public void ParseMinioPath(string fullPath, out string path, out string result) => AviReaderService.ParseMinioPath(fullPath, out path, out result);

        #endregion

        #region Minio 操作与数据转换

        /// <summary>
        /// 通过Minio读取Json文件
        /// </summary>
        public void ReadJsonByMinio(string ip, string port, string key, string head, string sn, string side, string path)
        {
            try
            {
                SystemEvent.SendTaskMsg(sn, $"{side}面正在读取Minio数据");
                MinioService.BuildClient(ip, port);
                string json = MinioService.ReadJsonSync("deepiresults", path, ip);
                var obj = JsonConvert.DeserializeObject<RootPanelInfo>(json);

                if (side == "A")
                {
                    TaskStatusSender.SendQueued(sn);
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
                    ProjectName = SysConfig.ProjectName
                };
                var convertResult = _panelDataConverter.Convert(obj, context);

                RootPanelInfoWithIP rootobj = new RootPanelInfoWithIP()
                {
                    IP = ip,
                    rootInfo = obj,
                };

                // 解耦：先获取图片Key列表，入图片加载队列，非阻塞
                var imageKeys = _imageLoaderService.GetAllMinioImageKeys(rootobj);

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
                    isByPass = convertResult.IsByPass,
                    ImageKeys = imageKeys
                };

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
            }
        }

        #endregion

        #region 算法调用与结果处理

        public bool DefectMethod(VBModel vBModel, out List<string> resList, out List<string> detailsList, out PcsResult pcsResult, out string vbJson) => _defectProcessor.DefectMethod(vBModel, SysConfig.MaxDefectCount, out resList, out detailsList, out pcsResult, out vbJson);

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
        /// 获取 DatabaseHelper 实例（兼容旧代码）
        /// </summary>
        [Obsolete("请使用 GetDatabaseService() 方法")]
        public DatabaseHelper GetDatabaseHelper() => _databaseHelper as DatabaseHelper;

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
            };
            BoardStatCache.Update(record);
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
    }
}

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
using DeepSightWorkLib.Services.Pipeline;
using DeepSightWorkLib.Services.Pipeline.Stages;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        /// TPL Dataflow 处理 Pipeline
        /// </summary>
        private ProcessingPipeline _pipeline;

        /// <summary>
        /// 获取 Pipeline 统计信息（供 UI 监控使用）
        /// </summary>
        public PipelineStatistics GetPipelineStatistics()
        {
            return _pipeline?.GetStatistics() ?? new PipelineStatistics();
        }

        /// <summary>
        /// Pipeline 是否正在运行
        /// </summary>
        public bool IsPipelineRunning => _pipeline?.IsRunning ?? false;

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
                    BoardStatCache.Flush();
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
            _aviReaderService = new AviReaderService(httpInstance, _queueManager.ProcessingSnSet, PostToPipeline);
            _imageLoaderService = new ImageLoaderService(minioInstance);
            _defectProcessor = new DefectProcessor(defectInstance, _queueManager.AIResultQueue, _queueManager.PostProcessQueue);
            _postProcessService = new PostProcessService( SysConfig, SavePanelSideToDatabase);
        }

        /// <summary>
        /// 初始化工作线程和 Pipeline
        /// </summary>
        public void InitWork()
        {
            this.IsStart = false;

            var httpInstance = HttpService as HttpClass ?? new HttpClass();
            _resultWriterService = new ResultWriterService(httpInstance, _queueManager.ProcessingSnSet);

            // 初始化模型验证测试服务
            // 注意：_pipeline 在 InitPipeline() 中初始化，lambda 惰性求值，执行时 _pipeline 已就绪
            var dbHelper = _databaseHelper as DatabaseHelper ?? new DatabaseHelper();
            _validationTestService = new ModelValidationTestService(
                dbHelper,
                _imageLoaderService,
                ctx => _pipeline?.Post(ctx, out _),
                SolConfig,
                AviConfig,
                () => SysConfig.UseGerberImage);

            // 将验证测试服务注入到后处理服务
            _postProcessService.SetValidationTestService(_validationTestService);

            // ---- 构建 TPL Dataflow Pipeline ----
            InitPipeline();

            // 使用 WorkerThreadManager 管理仍需轮询的线程
            _workerManager.Start();

            // 仅保留 ReadAVI 轮询（作为 Pipeline 数据源）和缓存清理
            _workerManager.RegisterPollingWorker(() => WorkerReadAVI(),
                new WorkerConfig { Name = "ReadAVI", PollIntervalMs = DefaultValues.ReadAviPollIntervalMs });

            _workerManager.RegisterPollingWorker(() => WorkerCleanupCache(),
                new WorkerConfig { Name = "CleanupCache", PollIntervalMs = DefaultValues.CleanupCachePollIntervalMs, IsLongRunning = true });

            LogTextHelper.Info("BusinessClass 初始化完成，Pipeline 已启动");
        }

        /// <summary>
        /// 初始化 TPL Dataflow Pipeline — 组装各阶段
        /// </summary>
        private void InitPipeline()
        {
            // 阶段1: JSON 解析
            var jsonParseStage = new JsonParseStage(
                MinioService,
                _imageLoaderService,
                _panelDataConverter,
                () => new PanelConvertContext
                {
                    SolutionConfig = SolConfig,
                    AviConfig = AviConfig,
                    OnSolutionConfigChanged = SaveSolutionConfig
                });

            // 阶段2: 图片加载（根据配置决定使用 Gerber 图还是 Template 图）
            var imageLoadStage = new ImageLoadStage(_imageLoaderService, () => SysConfig.UseGerberImage);

            // 阶段3: 推理
            var inferenceStage = new InferenceStage(
                _defectProcessor,
                () => SysConfig.MaxDefectCount,
                () => SysConfig.MaxWaitTime);

            // 阶段4a: 结果回写
            var resultWriteStage = new ResultWriteStage(_resultWriterService);

            // 阶段4b: 后处理
            var postProcessStage = new PostProcessStage(_postProcessService);

            // 组装 Pipeline
            _pipeline = new ProcessingPipeline()
                .WithJsonParseStage(jsonParseStage.Execute)
                .WithImageLoadStage(imageLoadStage.Execute)
                .WithInferenceStage(inferenceStage.Execute)
                .WithResultWriteStage(resultWriteStage.Execute)
                .WithPostProcessStage(postProcessStage.Execute)
                .WithErrorHandler(HandlePipelineError)
                .WithCompletionHandler(HandlePipelineCompleted);

            _pipeline.Start(maxDegreeOfParallelism: 1);
            LogTextHelper.Info("ProcessingPipeline 已初始化并启动");
        }

        /// <summary>
        /// Pipeline 全局错误处理器
        /// </summary>
        private void HandlePipelineError(PipelineContext ctx)
        {
            try
            {
                string sn = ctx.SN;
                string side = ctx.Side;
                if (string.IsNullOrEmpty(sn)) return;

                // 在 SnDebugInfo 中记录错误信息
                string effectiveSide = string.IsNullOrEmpty(side) ? "A" : side;
                var debugInfo = SnDebugInfoCache.GetOrCreate(sn, effectiveSide);
                debugInfo.HasError = true;
                debugInfo.ErrorStep = ctx.ErrorStage;
                debugInfo.ErrorMessage = ctx.ErrorMessage;
                debugInfo.ErrorTime = DateTime.Now;
                debugInfo.JudgmentSummary = $"[异常] {ctx.ErrorStage}: {ctx.ErrorMessage}";

                // 向 UI 发送失败状态
                TaskStatusSender.SendFailed(sn, effectiveSide, $"[{ctx.ErrorStage}] {ctx.ErrorMessage}");

                // 清除 ProcessingSnSet 标记
                string snKey = $"{sn}_{effectiveSide}";
                _queueManager.ProcessingSnSet.TryRemove(snKey, out _);

                LogTextHelper.Error($"Pipeline 错误处理: SN={sn}, 阶段={ctx.ErrorStage}, 原因={ctx.ErrorMessage}");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"HandlePipelineError 自身异常: {ex}");
            }
        }

        /// <summary>
        /// Pipeline 完成回调 — 所有下游阶段（ResultWrite + PostProcess）都完成后触发
        /// 用于统计全流程耗时和清理 ProcessingSnSet
        /// </summary>
        private void HandlePipelineCompleted(PipelineContext ctx)
        {
            try
            {
                ctx.Stopwatch.Stop();
                string sn = ctx.SN;
                string side = ctx.Side;

                LogTextHelper.Info($"SN:{sn} {side} Pipeline 全流程完成，总耗时: {ctx.Stopwatch.ElapsedMilliseconds}ms" +
                    (ctx.InferenceElapsedMs > 0 ? $"（推理: {ctx.InferenceElapsedMs}ms）" : ""));
            }
            catch (Exception ex)
            {
                LogTextHelper.Warn($"HandlePipelineCompleted 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 向 Pipeline 投递一个 AviProcessingContext（作为 AviReaderService 回调）
        /// </summary>
        private void PostToPipeline(AviProcessingContext aviCtx)
        {
            var ctx = new PipelineContext { AviContext = aviCtx };

            if (!_pipeline.Post(ctx, out string failureReason))
            {
                LogTextHelper.Error($"Pipeline 投递失败: SN={aviCtx.SerialNumber}, Side={aviCtx.Side}, 原因={failureReason}");
                // 投递失败时清理 ProcessingSnSet 标记
                string snKey = $"{aviCtx.SerialNumber}_{aviCtx.Side}";
                _queueManager.ProcessingSnSet.TryRemove(snKey, out _);
                // 通知 UI
                TaskStatusSender.SendFailed(aviCtx.SerialNumber, aviCtx.Side, $"Pipeline投递失败: {failureReason}");
            }
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

                // Pipeline 统计
                string pipelineInfo = "";
                if (_pipeline != null && _pipeline.IsRunning)
                {
                    var pipelineStats = _pipeline.GetStatistics();
                    pipelineInfo = $", Pipeline: [{pipelineStats}]";
                }

                LogTextHelper.Info($"处理统计 - {stats}, 数据库队列:{_databaseHelper.GetQueueLength()}{pipelineInfo}");

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
                // 停止 Pipeline（等待在途数据处理完成）
                _pipeline?.Dispose();

                // 停止所有工作线程
                _workerManager?.Stop();

                // 刷新待写入的数据库记录
                FlushPendingPanelSideRecords();
                BoardStatCache.Flush();

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
        /// 清空所有正在处理的队列（重置软件或暂停时调用）
        /// </summary>
        public void ClearAllProcessingQueues()
        {
            try
            {
                LogTextHelper.Info("IsStart 设置为 false，开始清空所有处理队列...");

                // 释放旧 Pipeline（等待在途数据处理完成后重新初始化）
                if (_pipeline != null)
                {
                    _pipeline.Dispose();
                    _pipeline = null;
                    LogTextHelper.Info("Pipeline 已释放");
                }

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

                // 重新初始化并启动 Pipeline
                InitPipeline();
                LogTextHelper.Info("Pipeline 已重新初始化并启动");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"清空处理队列时发生异常: {ex}");
            }
        }

        #endregion
    }
}

using DeepSightDB;
using DeepSightModel;
using DeepSightModel.Configuration;
using DeepSightTool;
using DeepSightWorkLib.Services.Pipeline;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 模型推理测试服务 - 统一处理一致性测试、二次推理、单图测试
    /// </summary>
    public class ModelValidationTestService
    {
        private readonly DatabaseHelper _databaseHelper;
        private readonly Action<PipelineContext> _postToPipeline;
        private readonly VBModelBuilder _vbModelBuilder;
        private readonly ImageLoaderService _imageLoaderService;
        private readonly Func<bool> _useGerberImageProvider;

        // 统一任务管理
        private readonly ConcurrentDictionary<string, InferenceTask> _activeTasks = new ConcurrentDictionary<string, InferenceTask>();

        // 单图测试等待结果
        private readonly ConcurrentDictionary<string, TaskCompletionSource<SingleImageTestResult>> _singleImageTestResults
            = new ConcurrentDictionary<string, TaskCompletionSource<SingleImageTestResult>>();

        public ModelValidationTestService(
            DatabaseHelper databaseHelper,
            ImageLoaderService imageLoaderService,
            Action<PipelineContext> postToPipeline,
            SolutionConfig solutionConfig,
            Func<bool> useGerberImageProvider = null)
        {
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _imageLoaderService = imageLoaderService ?? throw new ArgumentNullException(nameof(imageLoaderService));
            _postToPipeline = postToPipeline ?? throw new ArgumentNullException(nameof(postToPipeline));
            _vbModelBuilder = new VBModelBuilder(solutionConfig);
            _useGerberImageProvider = useGerberImageProvider ?? (() => false);
        }

        #region 统一任务创建接口

        /// <summary>
        /// 创建并启动推理任务（统一入口）
        /// </summary>
        public async Task<InferenceTask> CreateTaskAsync(InferenceTaskRequest request, CancellationToken cancellationToken = default)
        {
            var task = new InferenceTask
            {
                Mode = request.Mode,
                Description = request.Description ?? GetDefaultDescription(request.Mode),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                LotNumber = request.LotNumber,
                ProductSerial = request.ProductSerial,
                MaxRecords = request.MaxRecords
            };

            _activeTasks[task.TaskId] = task;
            LogTextHelper.Info($"创建{GetModeName(request.Mode)}任务: {task.TaskId}, 描述: {task.Description}");

            // 异步执行任务
            _ = Task.Run(() => ExecuteTaskAsync(task, cancellationToken), cancellationToken);

            return task;
        }

        /// <summary>
        /// 兼容旧接口：创建一致性测试任务
        /// </summary>
        public async Task<InferenceTask> CreateTestTaskAsync(ValidationTestRequest request, CancellationToken cancellationToken = default)
        {
            return await CreateTaskAsync(new InferenceTaskRequest
            {
                Mode = InferenceMode.ConsistencyTest,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                LotNumber = request.LotNumber,
                ProductSerial = request.ProductSerial,
                MaxRecords = request.MaxRecords,
                Description = request.Description
            }, cancellationToken);
        }

        /// <summary>
        /// 兼容旧接口：创建二次推理任务
        /// </summary>
        public async Task<InferenceTask> CreateSecondaryInferenceTaskAsync(SecondaryInferenceRequest request, CancellationToken cancellationToken = default)
        {
            return await CreateTaskAsync(new InferenceTaskRequest
            {
                Mode = InferenceMode.SecondaryInference,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                LotNumber = request.LotNumber,
                ProductSerial = request.ProductSerial,
                MaxRecords = request.MaxRecords,
                Description = request.Description
            }, cancellationToken);
        }

        private string GetDefaultDescription(InferenceMode mode)
        {
            switch (mode)
            {
                case InferenceMode.ConsistencyTest: return $"一致性测试 {DateTime.Now:yyyy-MM-dd HH:mm}";
                case InferenceMode.SecondaryInference: return $"二次推理 {DateTime.Now:yyyy-MM-dd HH:mm}";
                case InferenceMode.SingleImageTest: return $"单图测试 {DateTime.Now:yyyy-MM-dd HH:mm}";
                default: return $"推理任务 {DateTime.Now:yyyy-MM-dd HH:mm}";
            }
        }

        private string GetModeName(InferenceMode mode)
        {
            switch (mode)
            {
                case InferenceMode.ConsistencyTest: return "一致性测试";
                case InferenceMode.SecondaryInference: return "二次推理";
                case InferenceMode.SingleImageTest: return "单图测试";
                default: return "推理";
            }
        }

        #endregion

        #region 统一任务执行

        /// <summary>
        /// 执行推理任务
        /// </summary>
        private async Task ExecuteTaskAsync(InferenceTask task, CancellationToken cancellationToken)
        {
            try
            {
                task.State = InferenceTaskState.Running;
                task.StartTime = DateTime.Now;

                // 获取数据
                var records = await GetTaskDataAsync(task);
                task.TotalRecords = records.Count;
                LogTextHelper.Info($"{GetModeName(task.Mode)}任务 {task.TaskId}: 获取到 {records.Count} 条数据");

                if (records.Count == 0)
                {
                    task.State = InferenceTaskState.Completed;
                    task.EndTime = DateTime.Now;
                    return;
                }

                // 处理每条记录
                foreach (var record in records)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        task.State = InferenceTaskState.Cancelled;
                        break;
                    }

                    await ProcessRecordAsync(task, record);
                }

                LogTextHelper.Info($"{GetModeName(task.Mode)}任务 {task.TaskId} 入队完成: 总数={task.TotalRecords}, 已入队={task.EnqueuedRecords}, 跳过(直报)={task.SkippedRecords}, 错误={task.ErrorRecords}");

                if (task.State == InferenceTaskState.Running && task.IsReallyCompleted)
                {
                    task.State = InferenceTaskState.Completed;
                    task.EndTime = DateTime.Now;
                    LogTextHelper.Info($"{GetModeName(task.Mode)}任务 {task.TaskId} 已完成: 已处理={task.ProcessedRecords}, 跳过={task.SkippedRecords}, 错误={task.ErrorRecords}");
                }
            }
            catch (Exception ex)
            {
                task.State = InferenceTaskState.Failed;
                task.EndTime = DateTime.Now;
                LogTextHelper.Error($"{GetModeName(task.Mode)}任务 {task.TaskId} 执行失败: {ex}");
            }
        }

        /// <summary>
        /// 获取任务数据
        /// </summary>
        private async Task<List<(PanelDataRecord Panel, SideData Side, List<DetectInfo> DefectPoints)>> GetTaskDataAsync(InferenceTask task)
        {
            var results = new List<(PanelDataRecord, SideData, List<DetectInfo>)>();

            List<PanelDataRecord> panels;
            if (!string.IsNullOrEmpty(task.LotNumber))
            {
                panels = await _databaseHelper.GetPanelsDataByMachineAndLot(null, task.LotNumber);
            }
            else if (task.StartDate.HasValue && task.EndDate.HasValue)
            {
                panels = await _databaseHelper.GetPanelsData(task.StartDate.Value, task.EndDate.Value, task.ProductSerial);
            }
            else
            {
                panels = await _databaseHelper.GetPanelsData(DateTime.Now.AddDays(-7), DateTime.Now);
            }

            foreach (var panel in panels)
            {
                if (panel.Sides == null) continue;
                foreach (var side in panel.Sides)
                {
                    if (side.DetectPoints == null || side.DetectPoints.Count == 0) continue;

                    // 根据模式筛选缺陷点
                    List<DetectInfo> defectPoints;

                    //选择所有已完成AI推理的点
                    if (side.AiState <= 0) continue;
                    defectPoints = side.DetectPoints;


                    if (defectPoints.Count > 0)
                    {
                        results.Add((panel, side, defectPoints));
                        if (task.MaxRecords.HasValue && results.Count >= task.MaxRecords.Value)
                            return results;
                    }
                }
            }
        
            return results;
        }

        /// <summary>
        /// 处理单条记录
        /// </summary>
        private async Task ProcessRecordAsync(InferenceTask task, (PanelDataRecord Panel, SideData Side, List<DetectInfo> DefectPoints) record)
        {
            var (panel, side, defectPoints) = record;

            try
            {
                if (task.Mode == InferenceMode.ConsistencyTest)
                {
                    await UpdateTestStateAsync(panel.SerialNumber, side.Side, ValidationTestState.Testing);
                }

                // 使用统一的 VBModelBuilder 构建 VBModel
                var context = new VBModelBuildContext
                {
                    Mode = task.Mode,
                    TaskId = task.TaskId,
                    SerialNumber = panel.SerialNumber,
                    SideName = side.Side,
                    ProductSerial = panel.ProductSerial,
                    MachineId = panel.MachineId,
                    Side = side,
                    DefectPoints = defectPoints
                };

                var vbModel = _vbModelBuilder.Build(context);

                if (vbModel == null)
                {
                    // 真无数据（DefectPoints 为空）
                    task.ErrorRecords++;
                    if (task.Mode == InferenceMode.ConsistencyTest)
                    {
                        await UpdateTestStateAsync(panel.SerialNumber, side.Side, ValidationTestState.TestError);
                    }
                    LogTextHelper.Warn($"{GetModeName(task.Mode)}记录 {panel.SerialNumber}_{side.Side} 无有效图片");
                    return;
                }

                if (vbModel.ImageKeys == null || vbModel.ImageKeys.Count == 0)
                {
                    // 区分"全部直报跳过"和"真无有效图片"
                    bool allDirectReport = vbModel.DirectReportFlags != null && vbModel.DirectReportFlags.Any(f => f);
                    if (allDirectReport)
                    {
                        // 该面所有缺陷均为直报，业务上跳过推理，不计为错误
                        task.SkippedRecords++;
                        if (task.Mode == InferenceMode.ConsistencyTest)
                        {
                            await UpdateTestStateAsync(panel.SerialNumber, side.Side, ValidationTestState.NotTested);
                        }
                        LogTextHelper.Info($"{GetModeName(task.Mode)}记录 {panel.SerialNumber}_{side.Side} 所有缺陷均为直报，跳过推理");
                    }
                    else
                    {
                        // 真正的无有效图片（图片路径为空等异常）
                        task.ErrorRecords++;
                        if (task.Mode == InferenceMode.ConsistencyTest)
                        {
                            await UpdateTestStateAsync(panel.SerialNumber, side.Side, ValidationTestState.TestError);
                        }
                        LogTextHelper.Warn($"{GetModeName(task.Mode)}记录 {panel.SerialNumber}_{side.Side} 无有效图片");
                    }
                    return;
                }

                // 加载图片（仅非直报缺陷，与 ImageKeys 对齐，根据配置判断直报）
                var nonDirectReportDefects = defectPoints
                    .Where(d => !KeyDefectConfigManager.Instance.IsDirectReportByProduct(d.DefectName, panel.ProductSerial))
                    .ToList();

                // 使用 ImageLoaderService 从 MinIO 加载缺陷图和模板/Gerber图
                var defectImagePaths = nonDirectReportDefects
                    .Select(d => d.ImagePath)
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList();

                vbModel.Mats = _imageLoaderService.LoadImages(defectImagePaths);

                bool useGerber = _useGerberImageProvider();
                if (useGerber)
                {
                    // 使用 Gerber 图
                    var gerberImagePaths = nonDirectReportDefects
                        .Select(d => d.GerberImagePath)
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToList();
                    vbModel.Mats_Gerber = _imageLoaderService.LoadImages(gerberImagePaths);
                    vbModel.Mats_Temp = vbModel.Mats_Gerber;
                }
                else
                {
                    // 使用 Template 图（默认）
                    var tempImagePaths = nonDirectReportDefects
                        .Select(d => d.TempImagePath)
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToList();
                    vbModel.Mats_Temp = _imageLoaderService.LoadImages(tempImagePaths);
                }

                // 构建 PipelineContext 并投递到 Pipeline（跳过JSON解析和图片加载阶段，图片已预加载）
                var pipelineCtx = new PipelineContext
                {
                    LoadModel = new ImageLoadModel { Model = vbModel }
                };
                _postToPipeline(pipelineCtx);
                LogTextHelper.Info($"{GetModeName(task.Mode)}任务投递Pipeline: {panel.SerialNumber}_{side.Side}, 总缺陷数: {defectPoints.Count}, 推理缺陷数: {nonDirectReportDefects.Count}");

                task.EnqueuedRecords++;
            }
            catch (Exception ex)
            {
                task.ErrorRecords++;
                if (task.Mode == InferenceMode.ConsistencyTest)
                {
                    await UpdateTestStateAsync(panel.SerialNumber, side.Side, ValidationTestState.TestError);
                }
                LogTextHelper.Error($"处理{GetModeName(task.Mode)}记录异常 {panel.SerialNumber}_{side.Side}: {ex}");
            }
        }

        #endregion

        /// <summary>
        /// 更新数据库测试状态
        /// </summary>
        private async Task UpdateTestStateAsync(string serialNumber, string side, ValidationTestState state)
        {
            try
            {
                await _databaseHelper.UpdateTestStateAsync(serialNumber, side, (int)state, DateTime.Now);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"更新测试状态失败: {serialNumber}_{side}, {ex.Message}");
            }
        }

        private static int ParseInferenceStatus(string inferResult)
        {
            if (!int.TryParse(inferResult, out int parsed))
            {
                return 0;
            }

            switch (parsed)
            {
                case 0: return 1;
                case 1: return 2;
                case 2: return 3;
                default: return 3;
            }
        }

        private static int AggregateSideStatus(IEnumerable<int> statuses)
        {
            if (statuses == null)
            {
                return 0;
            }

            var statusList = statuses.ToList();
            if (statusList.Count == 0)
            {
                return 0;
            }

            if (statusList.Any(s => s == 3)) return 3;
            if (statusList.Any(s => s == 2)) return 2;
            if (statusList.Any(s => s == 1)) return 1;
            return 0;
        }

        private static string GetStatusText(int status)
        {
            switch (status)
            {
                case 0: return "未检测";
                case 1: return "OK";
                case 2: return "NG";
                case 3: return "异常";
                default: return status.ToString();
            }
        }

        #region 测试结果处理（由 PostProcessService 调用）

        /// <summary>
        /// 处理验证测试的推理结果（供 PostProcessService 调用）
        /// </summary>
        public void ProcessValidationTestResult(VBModel vbModel, List<string> inferResults)
        {
            if (!vbModel.IsValidationTest || string.IsNullOrEmpty(vbModel.TestTaskId))
                return;

            try
            {
                // 比对结果
                var sideResult = CompareResults(vbModel, inferResults);
                sideResult.TestTime = DateTime.Now;

                // 更新任务统计
                if (_activeTasks.TryGetValue(vbModel.TestTaskId, out var task))
                {
                    lock (task)
                    {
                        task.ConsistencyResults.Add(sideResult);
                        task.ProcessedRecords++;  // 在推理结果返回时增加处理计数

                        if (sideResult.State == ValidationTestState.Consistent)
                            task.ConsistentRecords++;
                        else if (sideResult.State == ValidationTestState.Inconsistent)
                            task.InconsistentRecords++;
                        else
                            task.ErrorRecords++;

                        // 更新VVS相关统计
                        if (sideResult.HasVVSData)
                        {
                            task.VVSRecords++;
                            task.TotalMissCount += sideResult.MissCount;
                            task.TotalOverKillCount += sideResult.OverKillCount;
                        }

                        // 检查任务是否真正完成（所有入队的记录都已返回结果）
                        if (task.IsReallyCompleted && task.State == InferenceTaskState.Running)
                        {
                            task.State = InferenceTaskState.Completed;
                            task.EndTime = DateTime.Now;
                            LogTextHelper.Info($"测试任务 {task.TaskId} 全部完成: 一致={task.ConsistentRecords}, 不一致={task.InconsistentRecords}, 错误={task.ErrorRecords}, VVS记录={task.VVSRecords}, 漏失={task.TotalMissCount}, 误报={task.TotalOverKillCount}");
                        }
                    }
                }

                // 更新数据库状态
                _ = UpdateTestStateAsync(vbModel.SN, vbModel.Side, sideResult.State);

                LogTextHelper.Info($"测试结果: {vbModel.SN}_{vbModel.Side}, 状态={sideResult.State}, " +
                    $"一致率={sideResult.ConsistencyRate:F1}%");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"处理验证测试结果异常: {vbModel.SN}_{vbModel.Side}, {ex}");
            }
        }

        /// <summary>
        /// 比对新旧推理结果
        /// </summary>
        private SideTestResult CompareResults(VBModel vbModel, List<string> inferResults)
        {
            var result = new SideTestResult
            {
                SerialNumber = vbModel.SN,
                Side = vbModel.Side,
                TotalDefects = vbModel.DefectIndex?.Count ?? 0,
                DataSource = vbModel.HasVVSData ? OriginalDataSourceType.VVS : OriginalDataSourceType.AI
            };

            if (vbModel.OriginalAIResults == null || inferResults == null)
            {
                result.State = ValidationTestState.TestError;
                result.ErrorMessage = "原始结果或新结果为空";
                result.OriginalSideResult = "-";
                result.NewSideResult = "-";
                return result;
            }

            var originalStatuses = new List<int>();
            var newStatuses = new List<int>();

            // 比对每个缺陷的结果
            for (int i = 0; i < (vbModel.DefectIndex?.Count ?? 0); i++)
            {
                var defectIdx = vbModel.DefectIndex[i];
                var originalAIStatus = vbModel.OriginalAIResults.ContainsKey(defectIdx)
                    ? vbModel.OriginalAIResults[defectIdx] : 0;
                var originalVVSStatus = vbModel.OriginalVVSResults != null && vbModel.OriginalVVSResults.ContainsKey(defectIdx)
                    ? vbModel.OriginalVVSResults[defectIdx] : 0;

                int newStatus = i < inferResults.Count ? ParseInferenceStatus(inferResults[i]) : 3;

                // 确定用于比对的原始状态（优先使用VVS）
                int effectiveOriginalStatus = vbModel.HasVVSData && originalVVSStatus > 0 ? originalVVSStatus : originalAIStatus;
                originalStatuses.Add(effectiveOriginalStatus);
                newStatuses.Add(newStatus);

                var defectResult = new DefectTestResult
                {
                    DefectIndex = defectIdx,
                    ImagePath = i < vbModel.ImageKeys.Count ? vbModel.ImageKeys[i] : null,
                    OriginalAIStatus = originalAIStatus,
                    OriginalVVSStatus = originalVVSStatus,
                    NewAIStatus = newStatus,
                    DataSource = vbModel.HasVVSData && originalVVSStatus > 0 ? OriginalDataSourceType.VVS : OriginalDataSourceType.AI
                };

                result.DefectResults.Add(defectResult);

                if (defectResult.IsConsistent)
                    result.ConsistentCount++;
                else
                    result.InconsistentCount++;

                // 统计漏失和误报（仅VVS数据有效）
                if (defectResult.IsMiss)
                    result.MissCount++;
                if (defectResult.IsOverKill)
                    result.OverKillCount++;
            }

            result.OriginalSideResult = GetStatusText(AggregateSideStatus(originalStatuses));
            result.NewSideResult = GetStatusText(AggregateSideStatus(newStatuses));

            // 判定整体状态
            result.State = result.InconsistentCount == 0
                ? ValidationTestState.Consistent
                : ValidationTestState.Inconsistent;

            return result;
        }

        #endregion

        #region 单图测试

        /// <summary>
        /// 运行单图测试（同步等待结果）
        /// </summary>
        public async Task<SingleImageTestResult> RunSingleImageTestAsync(
            DetectInfo detectInfo,
            string productSerial,
            string machineId,
            string side,
            int timeout = 30)
        {
            if (detectInfo == null || string.IsNullOrEmpty(detectInfo.ImagePath))
            {
                return new SingleImageTestResult
                {
                    Success = false,
                    ErrorMessage = "缺陷信息或图片路径为空"
                };
            }

            var testKey = $"SINGLE_{Guid.NewGuid():N}";

            try
            {
                var tcs = new TaskCompletionSource<SingleImageTestResult>();
                _singleImageTestResults[testKey] = tcs;

                // 使用 VBModelBuilder 构建 VBModel
                var context = new VBModelBuildContext
                {
                    Mode = InferenceMode.SingleImageTest,
                    TaskId = testKey,
                    SerialNumber = testKey,
                    SideName = side,
                    ProductSerial = productSerial,
                    MachineId = machineId,
                    DefectPoints = new List<DetectInfo> { detectInfo }
                };

                var vbModel = _vbModelBuilder.Build(context);
                vbModel.OriginalAIResults = new Dictionary<int, int> { { 0, detectInfo.AIStatus } };

                // 通过Minio加载缺陷图
                var mat = _imageLoaderService.LoadMinioImage(detectInfo.ImagePath);
                if (mat == null || mat.Empty())
                {
                    return new SingleImageTestResult
                    {
                        Success = false,
                        ErrorMessage = "加载缺陷图片失败",
                        ImagePath = detectInfo.ImagePath
                    };
                }

                vbModel.Mats = new List<Mat> { mat };

                // 根据配置决定加载模板图还是Gerber图
                bool useGerber = _useGerberImageProvider();
                string refImagePath = useGerber ? detectInfo.GerberImagePath : detectInfo.TempImagePath;
                string imageTypeName = useGerber ? "Gerber" : "模板";

                if (!string.IsNullOrEmpty(refImagePath))
                {
                    var refMat = _imageLoaderService.LoadMinioImage(refImagePath);
                    if (refMat == null || refMat.Empty())
                    {
                        LogTextHelper.Warn($"单图测试加载{imageTypeName}图失败: {refImagePath}，将使用空列表");
                        vbModel.Mats_Temp = new List<Mat>();
                    }
                    else
                    {
                        vbModel.Mats_Temp = new List<Mat> { refMat };
                        if (useGerber) vbModel.Mats_Gerber = new List<Mat> { refMat };
                    }
                }
                else
                {
                    LogTextHelper.Warn($"单图测试{imageTypeName}图路径为空");
                    vbModel.Mats_Temp = new List<Mat>();
                }

                // 构建 PipelineContext 并投递到 Pipeline（跳过JSON解析和图片加载阶段，图片已预加载）
                var pipelineCtx = new PipelineContext
                {
                    LoadModel = new ImageLoadModel { Model = vbModel }
                };
                _postToPipeline(pipelineCtx);
                LogTextHelper.Info($"单图测试投递Pipeline: {testKey}, 图片: {detectInfo.ImagePath}");

                // 等待结果
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeout));
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    return new SingleImageTestResult
                    {
                        Success = false,
                        ErrorMessage = "测试超时",
                        ImagePath = detectInfo.ImagePath,
                        OriginalAIStatus = detectInfo.AIStatus
                    };
                }

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"单图测试异常: {testKey}, {ex}");
                return new SingleImageTestResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ImagePath = detectInfo.ImagePath
                };
            }
            finally
            {
                _singleImageTestResults.TryRemove(testKey, out _);
            }
        }

        /// <summary>
        /// 处理二次推理的推理结果（供 PostProcessService 调用）
        /// </summary>
        public async Task ProcessSecondaryInferenceResultAsync(VBModel vbModel, List<string> inferResults)
        {
            if (!vbModel.IsSecondaryInference || string.IsNullOrEmpty(vbModel.TestTaskId))
                return;

            try
            {
                var sideResult = new SecondaryInferenceResult
                {
                    SerialNumber = vbModel.SN,
                    Side = vbModel.Side,
                    InferenceTime = DateTime.Now
                };

                // 获取原始 DetectInfo 列表用于更新
                var updatedDetectInfos = new List<DetectInfo>();
                int changedToOkCount = 0;
                int finalOkCount = 0;
                int finalNgCount = 0;
                int finalBypassCount = 0;
                int finalUndetectedCount = 0;
                int originalOkCount = 0;
                int originalNgCount = 0;
                int originalBypassCount = 0;
                int originalUndetectedCount = 0;

                if (vbModel.OriginalDetectInfos != null && inferResults != null)
                {
                    for (int i = 0; i < Math.Min(vbModel.DefectIndex.Count, inferResults.Count); i++)
                    {
                        var defectIdx = vbModel.DefectIndex[i];
                        if (vbModel.OriginalDetectInfos.TryGetValue(defectIdx, out var detectInfo) && detectInfo != null)
                        {
                            var originalStatus = detectInfo.AIStatus;
                            int newStatus = ParseInferenceStatus(inferResults[i]);

                            // 统计原始四种状态
                            switch (originalStatus)
                            {
                                case 0: originalUndetectedCount++; break;
                                case 1: originalOkCount++; break;
                                case 2: originalNgCount++; break;
                                case 3: originalBypassCount++; break;
                                default: originalBypassCount++; break;
                            }

                            // 记录点结果
                            sideResult.PointResults.Add(new SecondaryInferencePointResult
                            {
                                DefectIndex = defectIdx,
                                ImagePath = detectInfo.ImagePath,
                                OriginalAIStatus = originalStatus,
                                NewAIStatus = newStatus
                            });

                            // 更新 DetectInfo 的 AIStatus
                            detectInfo.AIStatus = newStatus;
                            updatedDetectInfos.Add(detectInfo);

                            // 统计四种状态
                            switch (newStatus)
                            {
                                case 0: finalUndetectedCount++; break;
                                case 1: finalOkCount++; break;
                                case 2: finalNgCount++; break;
                                case 3: finalBypassCount++; break;
                                default: finalBypassCount++; break;
                            }

                            // 统计从NG变为OK的数量
                            if (originalStatus == 2 && newStatus == 1)
                            {
                                changedToOkCount++;
                            }
                        }
                    }
                }

                sideResult.ChangedToOkCount = changedToOkCount;
                sideResult.OriginalNgCount = originalNgCount;
                sideResult.OriginalOkCount = originalOkCount;
                sideResult.OriginalBypassCount = originalBypassCount;
                sideResult.OriginalUndetectedCount = originalUndetectedCount;
                sideResult.FinalOkCount = finalOkCount;
                sideResult.FinalNgCount = finalNgCount;
                sideResult.FinalBypassCount = finalBypassCount;
                sideResult.FinalUndetectedCount = finalUndetectedCount;
                sideResult.State = SecondaryInferenceResultState.Completed;

                // 更新数据库
                if (updatedDetectInfos.Count > 0)
                {
                    // 计算新的 AI 状态：如果所有点都是 OK(1)，则面状态为 OK(1)，否则为 NG(2)
                    int newAiState = updatedDetectInfos.All(d => d.AIStatus == 1) ? 1 : 2;
                    await _databaseHelper.UpdatePanelSideAiStateAsync(vbModel.SN, vbModel.Side, updatedDetectInfos, newAiState);
                    LogTextHelper.Info($"二次推理数据库更新完成: {vbModel.SN}_{vbModel.Side}, 更新点数={updatedDetectInfos.Count}, 转OK数={changedToOkCount}");
                }

                // 更新任务统计
                if (_activeTasks.TryGetValue(vbModel.TestTaskId, out var task))
                {
                    lock (task)
                    {
                        task.SecondaryResults.Add(sideResult);
                        task.ProcessedRecords++;

                        if (changedToOkCount > 0)
                            task.OkRecords += changedToOkCount;
                        task.NgRecords += sideResult.OriginalNgCount - changedToOkCount;

                        // 检查任务是否真正完成
                        if (task.IsReallyCompleted && task.State == InferenceTaskState.Running)
                        {
                            task.State = InferenceTaskState.Completed;
                            task.EndTime = DateTime.Now;
                            LogTextHelper.Info($"二次推理任务 {task.TaskId} 全部完成: 总点数={task.OkRecords + task.NgRecords}, 转OK={task.OkRecords}");
                        }
                    }
                }

                sideResult.FinalNgCount = sideResult.OriginalNgCount - changedToOkCount;
                LogTextHelper.Info($"二次推理结果: {vbModel.SN}_{vbModel.Side}, 原NG数={sideResult.OriginalNgCount}, 转OK={changedToOkCount}, 最终NG={sideResult.FinalNgCount}");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"处理二次推理结果异常: {vbModel.SN}_{vbModel.Side}, {ex}");
            }
        }

        /// <summary>
        /// 处理单图测试的推理结果（供 PostProcessService 调用）
        /// </summary>
        public void ProcessSingleImageTestResult(VBModel vbModel, List<string> inferResults, string rawJsonResult = null)
        {
            if (!vbModel.IsSingleImageTest || string.IsNullOrEmpty(vbModel.TestTaskId))
                return;

            var testKey = vbModel.TestTaskId;

            if (!_singleImageTestResults.TryGetValue(testKey, out var tcs))
            {
                LogTextHelper.Warn($"单图测试结果无对应等待任务: {testKey}");
                return;
            }

            try
            {
                var originalStatus = vbModel.OriginalAIResults?.ContainsKey(0) == true
                    ? vbModel.OriginalAIResults[0] : 0;

                int newStatus = 0;
                if (inferResults != null && inferResults.Count > 0)
                {
                    newStatus = ParseInferenceStatus(inferResults[0]);
                }

                var result = new SingleImageTestResult
                {
                    Success = true,
                    OriginalAIStatus = originalStatus,
                    NewAIStatus = newStatus,
                    ImagePath = vbModel.ImageKeys?.FirstOrDefault()
                };

                // 从原始 JSON 中提取复判详情
                ExtractInferDetail(result, rawJsonResult);

                tcs.TrySetResult(result);
                LogTextHelper.Info($"单图测试完成: {testKey}, 原状态={originalStatus}, 新状态={newStatus}, 一致={result.IsConsistent}, 缺陷名={result.DefectName ?? "-"}");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"处理单图测试结果异常: {testKey}, {ex}");
                tcs.TrySetResult(new SingleImageTestResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// 从原始推理 JSON 中提取复判详情并填充到 SingleImageTestResult
        /// </summary>
        private static void ExtractInferDetail(SingleImageTestResult result, string rawJsonResult)
        {
            if (string.IsNullOrEmpty(rawJsonResult))
                return;

            try
            {
                var obj = JsonConvert.DeserializeObject<RootVBOutInfo>(rawJsonResult);
                if (obj?.Code?.ToString() != "200" || obj.Data?.InferWholeData?.InferResults == null)
                    return;

                var inferResults = obj.Data.InferWholeData.InferResults;
                if (inferResults.Count == 0)
                    return;

                var first = inferResults[0];
                result.DefectName = first.Defect_name;
                result.DefectCode = first.Defect_code;

                if (first.InferDetails != null)
                {
                    result.DefectArea = first.InferDetails.DefectArea;

                    // 序列化 DrawInfo 以便详情弹窗使用
                    if (first.InferDetails.DrawInfoList != null)
                    {
                        result.DrawInfo = JsonConvert.SerializeObject(first.InferDetails.DrawInfoList);
                    }

                    // 构建可读的复判详情摘要
                    result.InferDetailText = BuildInferDetailText(first);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Warn($"提取单图测试复判详情失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 从 InferResult 构建可读的复判详情文本
        /// </summary>
        private static string BuildInferDetailText(InferResult inferResult)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(inferResult.Defect_name))
                sb.AppendLine($"缺陷名称: {inferResult.Defect_name}");

            if (inferResult.InferDetails != null)
            {
                if (!string.IsNullOrEmpty(inferResult.InferDetails.DefectArea))
                    sb.AppendLine($"缺陷面积: {inferResult.InferDetails.DefectArea}");

                // 从 DrawInfo 提取判别条件
                var drawInfoList = inferResult.InferDetails.DrawInfoList;
                if (drawInfoList != null)
                {
                    foreach (var drawInfo in drawInfoList)
                    {
                        if (!string.IsNullOrEmpty(drawInfo.InspectName))
                            sb.AppendLine($"检测项: {drawInfo.InspectName} ({drawInfo.InspectLabel})");

                        if (drawInfo.Conditions != null)
                        {
                            foreach (var cond in drawInfo.Conditions)
                            {
                                string thresholdText = "";
                                if (cond.Threshold != null)
                                {
                                    var parts = new List<string>();
                                    if (cond.Threshold.Min.HasValue) parts.Add($"min={cond.Threshold.Min.Value}");
                                    if (cond.Threshold.Max.HasValue) parts.Add($"max={cond.Threshold.Max.Value}");
                                    thresholdText = string.Join(", ", parts);
                                }
                                sb.AppendLine($"  {cond.Name}: {cond.Value} {cond.Unit} [{thresholdText}]");
                            }
                        }
                    }
                }
            }

            return sb.Length > 0 ? sb.ToString().TrimEnd() : null;
        }

        #endregion

        #region 任务查询接口

        /// <summary>
        /// 获取测试任务状态
        /// </summary>
        public InferenceTask GetTaskStatus(string taskId)
        {
            _activeTasks.TryGetValue(taskId, out var task);
            return task;
        }

        /// <summary>
        /// 获取所有活跃任务
        /// </summary>
        public List<InferenceTask> GetActiveTasks()
        {
            return _activeTasks.Values.ToList();
        }

        /// <summary>
        /// 取消测试任务
        /// </summary>
        public bool CancelTask(string taskId)
        {
            if (_activeTasks.TryGetValue(taskId, out var task))
            {
                task.State = InferenceTaskState.Cancelled;
                task.EndTime = DateTime.Now;
                return true;
            }
            return false;
        }

        #endregion
    }
}

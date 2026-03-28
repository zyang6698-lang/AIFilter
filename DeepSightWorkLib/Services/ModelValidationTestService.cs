using DeepSightDB;
using DeepSightModel;
using DeepSightTool;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly QueueManager _queueManager;
        private readonly VBModelBuilder _vbModelBuilder;
        private readonly ImageLoaderService _imageLoaderService;

        // 统一任务管理
        private readonly ConcurrentDictionary<string, InferenceTask> _activeTasks = new ConcurrentDictionary<string, InferenceTask>();

        // 单图测试等待结果
        private readonly ConcurrentDictionary<string, TaskCompletionSource<SingleImageTestResult>> _singleImageTestResults
            = new ConcurrentDictionary<string, TaskCompletionSource<SingleImageTestResult>>();

        public ModelValidationTestService(
            DatabaseHelper databaseHelper,
            ImageLoaderService imageLoaderService,
            QueueManager queueManager,
            SolutionConfig solutionConfig,
            AVIConfig aviConfig)
        {
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _imageLoaderService = imageLoaderService ?? throw new ArgumentNullException(nameof(imageLoaderService));
            _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
            _vbModelBuilder = new VBModelBuilder(solutionConfig);
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

                LogTextHelper.Info($"{GetModeName(task.Mode)}任务 {task.TaskId} 入队完成: 总数={task.TotalRecords}, 已入队={task.EnqueuedRecords}, 错误={task.ErrorRecords}");
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
                    if (task.Mode == InferenceMode.SecondaryInference)
                    {
                        // 二次推理：只选择AI状态为NG的点
                        defectPoints = side.DetectPoints.Where(p => p.AIStatus == 2).ToList();
                    }
                    else
                    {
                        // 一致性测试：选择所有已完成AI推理的点
                        if (side.AiState <= 0) continue;
                        defectPoints = side.DetectPoints;
                    }

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

                if (vbModel == null || vbModel.ImageKeys == null || vbModel.ImageKeys.Count == 0)
                {
                    task.ErrorRecords++;
                    if (task.Mode == InferenceMode.ConsistencyTest)
                    {
                        await UpdateTestStateAsync(panel.SerialNumber, side.Side, ValidationTestState.TestError);
                    }
                    LogTextHelper.Warn($"{GetModeName(task.Mode)}记录 {panel.SerialNumber}_{side.Side} 无有效图片");
                    return;
                }

                // 加载图片
                vbModel.Mats = _vbModelBuilder.LoadImages(defectPoints);

                // 入队到推理队列
                _queueManager.AviQueue.Enqueue(vbModel);
                LogTextHelper.Info($"{GetModeName(task.Mode)}任务入队: {panel.SerialNumber}_{side.Side}, 缺陷数: {defectPoints.Count}");

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
                TotalDefects = vbModel.OriginalAIResults?.Count ?? 0,
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

            bool hasOriginalNG = false;
            bool hasNewNG = false;

            // 比对每个缺陷的结果
            for (int i = 0; i < Math.Min(vbModel.DefectIndex.Count, inferResults.Count); i++)
            {
                var defectIdx = vbModel.DefectIndex[i];
                var originalAIStatus = vbModel.OriginalAIResults.ContainsKey(defectIdx)
                    ? vbModel.OriginalAIResults[defectIdx] : 0;
                var originalVVSStatus = vbModel.OriginalVVSResults != null && vbModel.OriginalVVSResults.ContainsKey(defectIdx)
                    ? vbModel.OriginalVVSResults[defectIdx] : 0;

                // 新结果: "0"=OK, "1"=NG, "2"=ByPass
                int newStatus = 0;
                if (int.TryParse(inferResults[i], out int parsed))
                {
                    newStatus = parsed == 0 ? 1 : 2;  // 转换为 AIStatus 格式: 1=OK, 2=NG
                }

                // 确定用于比对的原始状态（优先使用VVS）
                int effectiveOriginalStatus = vbModel.HasVVSData && originalVVSStatus > 0 ? originalVVSStatus : originalAIStatus;

                // 判断面级别是否有NG (Status: 1=OK, 2=NG)
                if (effectiveOriginalStatus == 2) hasOriginalNG = true;
                if (newStatus == 2) hasNewNG = true;

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

            // 设置面级别判定结果 (任一缺陷为NG则面为NG)
            result.OriginalSideResult = hasOriginalNG ? "NG" : "OK";
            result.NewSideResult = hasNewNG ? "NG" : "OK";

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

                // 通过Minio加载图片
                var mat = _imageLoaderService.LoadMinioImage(detectInfo.ImagePath);
                if (mat == null || mat.Empty())
                {
                    return new SingleImageTestResult
                    {
                        Success = false,
                        ErrorMessage = "加载图片失败",
                        ImagePath = detectInfo.ImagePath
                    };
                }

                vbModel.Mats = new List<Mat> { mat };

                _queueManager.AviQueue.Enqueue(vbModel);
                LogTextHelper.Info($"单图测试入队: {testKey}, 图片: {detectInfo.ImagePath}");

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
                    InferenceTime = DateTime.Now,
                    OriginalNgCount = vbModel.DefectIndex?.Count ?? 0
                };

                // 获取原始 DetectInfo 列表用于更新
                var updatedDetectInfos = new List<DetectInfo>();
                int changedToOkCount = 0;

                if (vbModel.OriginalDetectInfos != null && inferResults != null)
                {
                    for (int i = 0; i < Math.Min(vbModel.DefectIndex.Count, inferResults.Count); i++)
                    {
                        var defectIdx = vbModel.DefectIndex[i];
                        if (vbModel.OriginalDetectInfos.TryGetValue(defectIdx, out var detectInfoObj) && detectInfoObj is DetectInfo detectInfo)
                        {
                            var originalStatus = detectInfo.AIStatus;
                            // 解析新的推理结果: "0" = OK(1), "1" = NG(2)
                            int newStatus = 0;
                            if (int.TryParse(inferResults[i], out int parsed))
                            {
                                newStatus = parsed == 0 ? 1 : 2;
                            }

                            // 记录点结果
                            sideResult.PointResults.Add(new SecondaryInferencePointResult
                            {
                                DefectIndex = defectIdx,
                                OriginalAIStatus = originalStatus,
                                NewAIStatus = newStatus
                            });

                            // 更新 DetectInfo 的 AIStatus
                            detectInfo.AIStatus = newStatus;
                            updatedDetectInfos.Add(detectInfo);

                            // 统计变化
                            if (originalStatus == 2 && newStatus == 1)
                            {
                                changedToOkCount++;
                            }
                        }
                    }
                }

                sideResult.ChangedToOkCount = changedToOkCount;
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
        public void ProcessSingleImageTestResult(VBModel vbModel, List<string> inferResults)
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
                    if (int.TryParse(inferResults[0], out int parsed))
                    {
                        newStatus = parsed == 0 ? 1 : 2;
                    }
                }

                var result = new SingleImageTestResult
                {
                    Success = true,
                    OriginalAIStatus = originalStatus,
                    NewAIStatus = newStatus,
                    ImagePath = vbModel.ImageKeys?.FirstOrDefault()
                };

                tcs.TrySetResult(result);
                LogTextHelper.Info($"单图测试完成: {testKey}, 原状态={originalStatus}, 新状态={newStatus}, 一致={result.IsConsistent}");
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

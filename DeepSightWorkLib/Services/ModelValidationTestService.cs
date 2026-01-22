using DeepSightDB;
using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 模型验证测试服务 - 用于验证模型一致性
    /// 深度集成现有推理通道，复用 QueueManager 和 DefectProcessor
    /// </summary>
    public class ModelValidationTestService
    {
        private readonly DatabaseHelper _databaseHelper;
        private readonly ImageLoaderService _imageLoaderService;
        private readonly QueueManager _queueManager;
        private readonly SolutionConfig _solutionConfig;
        private readonly AVIConfig _aviConfig;
        private readonly string _minioIP;
        private readonly string _minioPort;

        // 测试任务管理
        private readonly ConcurrentDictionary<string, ValidationTestTask> _activeTasks = new ConcurrentDictionary<string, ValidationTestTask>();

        // 等待测试完成的结果收集
        private readonly ConcurrentDictionary<string, SideTestResult> _pendingResults = new ConcurrentDictionary<string, SideTestResult>();

        public ModelValidationTestService(
            DatabaseHelper databaseHelper,
            ImageLoaderService imageLoaderService,
            QueueManager queueManager,
            SolutionConfig solutionConfig,
            AVIConfig aviConfig,
            string minioIP,
            string minioPort)
        {
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _imageLoaderService = imageLoaderService ?? throw new ArgumentNullException(nameof(imageLoaderService));
            _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
            _solutionConfig = solutionConfig;
            _aviConfig = aviConfig;
            _minioIP = minioIP;
            _minioPort = minioPort;
        }

        /// <summary>
        /// 创建并启动验证测试任务
        /// </summary>
        public async Task<ValidationTestTask> CreateTestTaskAsync(ValidationTestRequest request, CancellationToken cancellationToken = default)
        {
            var task = new ValidationTestTask
            {
                Description = request.Description ?? $"模型验证测试 {DateTime.Now:yyyy-MM-dd HH:mm}",
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                LotNumber = request.LotNumber,
                ProductSerial = request.ProductSerial,
                MaxRecords = request.MaxRecords
            };

            _activeTasks[task.TaskId] = task;
            LogTextHelper.Info($"创建模型验证测试任务: {task.TaskId}, 描述: {task.Description}");

            // 异步执行测试
            _ = Task.Run(() => ExecuteTestTaskAsync(task, cancellationToken), cancellationToken);

            return task;
        }

        /// <summary>
        /// 执行测试任务
        /// </summary>
        private async Task ExecuteTestTaskAsync(ValidationTestTask task, CancellationToken cancellationToken)
        {
            try
            {
                task.State = ValidationTestTaskState.Running;
                task.StartTime = DateTime.Now;

                // 1. 从数据库获取历史数据
                var records = await GetTestDataAsync(task);
                task.TotalRecords = records.Count;
                LogTextHelper.Info($"测试任务 {task.TaskId}: 获取到 {records.Count} 条待测试数据");

                if (records.Count == 0)
                {
                    task.State = ValidationTestTaskState.Completed;
                    task.EndTime = DateTime.Now;
                    return;
                }

                // 2. 构建测试任务并入队
                foreach (var record in records)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        task.State = ValidationTestTaskState.Cancelled;
                        break;
                    }

                    await ProcessSingleRecordAsync(task, record, cancellationToken);
                }

                // 注意：这里只是入队完成，不能标记为 Completed
                // 任务状态保持 Running，等待所有推理结果返回后在 ProcessValidationTestResult 中判断是否完成
                LogTextHelper.Info($"测试任务 {task.TaskId} 入队完成: 总数={task.TotalRecords}, 已入队={task.EnqueuedRecords}, 错误={task.ErrorRecords}");
            }
            catch (Exception ex)
            {
                task.State = ValidationTestTaskState.Failed;
                task.EndTime = DateTime.Now;
                LogTextHelper.Error($"测试任务 {task.TaskId} 执行失败: {ex}");
            }
        }

        /// <summary>
        /// 从数据库获取测试数据
        /// </summary>
        private async Task<List<(PanelDataRecord Panel, SideData Side)>> GetTestDataAsync(ValidationTestTask task)
        {
            var results = new List<(PanelDataRecord, SideData)>();

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
                // 默认获取最近7天数据
                panels = await _databaseHelper.GetPanelsData(DateTime.Now.AddDays(-7), DateTime.Now);
            }
            // 筛选有缺陷数据的记录
            foreach (var panel in panels)
            {
                if (panel.Sides == null) continue;
                foreach (var side in panel.Sides)
                {
                    // 只测试有缺陷且已完成AI推理的记录
                    if (side.DetectPoints != null && side.DetectPoints.Count > 0 && side.AiState > 0)
                    {
                        results.Add((panel, side));
                        if (task.MaxRecords.HasValue && results.Count >= task.MaxRecords.Value)
                            return results;
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 处理单条记录的测试
        /// </summary>
        private async Task ProcessSingleRecordAsync(ValidationTestTask task, (PanelDataRecord Panel, SideData Side) record, CancellationToken cancellationToken)
        {
            var (panel, side) = record;
            var testKey = $"{task.TaskId}_{panel.SerialNumber}_{side.Side}";

            try
            {
                // 更新数据库状态为"测试中"
                await UpdateTestStateAsync(panel.SerialNumber, side.Side, ValidationTestState.Testing);

                // 构建测试用的 VBModel
                var vbModel = BuildValidationVBModel(task, panel, side);

                if (vbModel == null || vbModel.ImageKeys == null || vbModel.ImageKeys.Count == 0)
                {
                    task.ErrorRecords++;
                    await UpdateTestStateAsync(panel.SerialNumber, side.Side, ValidationTestState.TestError);
                    LogTextHelper.Warn($"测试记录 {panel.SerialNumber}_{side.Side} 无有效图片");
                    return;
                }

                // 创建待处理结果
                var pendingResult = new SideTestResult
                {
                    SerialNumber = panel.SerialNumber,
                    Side = side.Side,
                    TotalDefects = side.DetectPoints.Count,
                    State = ValidationTestState.Testing
                };
                _pendingResults[testKey] = pendingResult;

                // 加载图片并入队到现有推理通道
                var loadModel = new ImageLoadModel
                {
                    Model = vbModel,
                    RootPanelInfo = null  // 测试任务不需要完整的 PanelInfo
                };

                // 直接加载图片
                var mats = LoadImagesForTest(side.DetectPoints);
                vbModel.Mats = mats;

                // 入队到 AviQueue（复用现有推理通道）
                _queueManager.AviQueue.Enqueue(vbModel);
                LogTextHelper.Info($"测试任务入队: {panel.SerialNumber}_{side.Side}, 缺陷数: {mats.Count}");

                // 注意：这里只增加入队计数，ProcessedRecords 在推理结果返回后由 ProcessValidationTestResult 增加
                task.EnqueuedRecords++;
            }
            catch (Exception ex)
            {
                task.ErrorRecords++;
                await UpdateTestStateAsync(panel.SerialNumber, side.Side, ValidationTestState.TestError);
                LogTextHelper.Error($"处理测试记录异常 {panel.SerialNumber}_{side.Side}: {ex}");
            }
        }

        /// <summary>
        /// 构建验证测试用的 VBModel
        /// </summary>
        private VBModel BuildValidationVBModel(ValidationTestTask task, PanelDataRecord panel, SideData side)
        {
            // 构建原始AI结果字典和VVS结果字典
            var originalResults = new Dictionary<int, int>();
            var originalVVSResults = new Dictionary<int, int>();
            var imageKeys = new List<string>();
            var imageDefects = new List<DetectInfo>();
            var defectIndexList = new List<int>();
            var pcsIndexList = new List<int>();

            // 检查是否有VVS数据（任一缺陷有VVS状态）
            bool hasVVSData = side.VvsState > 0 || side.DetectPoints.Any(d => d.VVSStatus > 0);

            for (int i = 0; i < side.DetectPoints.Count; i++)
            {
                var defect = side.DetectPoints[i];
                originalResults[i] = defect.AIStatus;
                originalVVSResults[i] = defect.VVSStatus;

                // 从 ImagePath 提取 Minio 路径
                if (!string.IsNullOrEmpty(defect.ImagePath))
                {
                    imageKeys.Add(defect.ImagePath);
                    imageDefects.Add(defect);
                    defectIndexList.Add(i);
                    pcsIndexList.Add(i);  // 简化处理
                }
            }

            if (imageKeys.Count == 0) return null;

            // 构建 VBInfo
            var vbInfo = CreateValidationVBInfo(panel, side, imageKeys, imageDefects);

            return new VBModel
            {
                Key = $"TEST_{task.TaskId}_{panel.SerialNumber}_{side.Side}",
                SN = panel.SerialNumber,
                Side = side.Side,
                DefectIndex = defectIndexList,
                PcsIndex = pcsIndexList,
                ImageKeys = imageKeys,
                VbInfo = vbInfo,
                IsValidationTest = true,
                OriginalAIResults = originalResults,
                OriginalVVSResults = originalVVSResults,
                HasVVSData = hasVVSData,
                TestTaskId = task.TaskId
            };
        }

        /// <summary>
        /// 创建验证测试用的 VBInfo
        /// </summary>
        private RootVBInfo CreateValidationVBInfo(PanelDataRecord panel, SideData side, List<string> imageKeys, List<DetectInfo> imageDefects)
        {
            // 从配置中查找料号对应的方案和流程
            var solutionFlow = _solutionConfig?.solus?.FirstOrDefault(o => o.ProductSerial == panel.ProductSerial)
                ?? _solutionConfig?.solus?.FirstOrDefault(o => o.ProductSerial?.ToUpper() == "DEFAULT");

            string solution = side.Side == "A" ? (solutionFlow?.Asolution ?? "default") : (solutionFlow?.Bsolution ?? "default");
            string flow = side.Side == "A" ? (solutionFlow?.Aflow ?? "1") : (solutionFlow?.Bflow ?? "1");

            var vbInfo = new RootVBInfo
            {
                MessageType = "visionbuilder_inference",
                paramsData = new ParamsData
                {
                    InferResUuid = Guid.NewGuid().ToString(),
                    InferWholeData = new InferWholeData
                    {
                        ImageInferParams = new ImageInferParams
                        {
                            PipelineName = solution,
                            NodeParams = new List<NodeParam>
                            {
                                new NodeParam { NodeName = flow, height = 200, width = 200 }
                            }
                        },
                        ImageData = new ImageData
                        {
                            DataType = "minio",
                            DataValue = new DataValue { InferImageGroup = new List<InferImageGroup>() }
                        },
                        OtherInfos = new Others
                        {
                            imageminio = new ImageMminio
                            {
                                access_key_id = "deepiobjectdata",
                                bucket = "deepiresults",
                                endpoint_url = _minioIP,
                                secret_key = "deepiobject2019",
                                secret_port = _minioPort
                            }
                        }
                    }
                }
            };

            // 构造模板图片路径 (格式: TemplateImages/{ProductSerial}/{MachineName}/{ProductSerial}[{Side}].jpg)
            string tempImgPath = BuildTemplateImagePath(panel.ProductSerial, panel.MachineId, side.Side);

            // 添加图片信息
            for (int i = 0; i < imageKeys.Count; i++)
            {
                var imagePath = imageKeys[i];
                var defect = (imageDefects != null && i < imageDefects.Count) ? imageDefects[i] : null;

                // 修复1: img_roi - 优先使用 RoiX/RoiY/Width/Height，如果为0则使用 Origin 版本
                int roiX = 0;
                int roiY = 0;
                int roiW = 0;
                int roiH = 0;

                if (defect != null)
                {
                    // 优先使用非Origin的ROI值（这是实际的绝对坐标）
                    if (defect.Width > 0 && defect.Height > 0)
                    {
                        roiX = defect.RoiX;
                        roiY = defect.RoiY;
                        roiW = defect.Width;
                        roiH = defect.Height;
                    }
                    // 如果没有，则使用Origin版本
                    else if (defect.OriginWidth > 0 && defect.OriginHeight > 0)
                    {
                        roiX = defect.OriginRoiX;
                        roiY = defect.OriginRoiY;
                        roiW = defect.OriginWidth;
                        roiH = defect.OriginHeight;
                    }
                }

                // 提取缺陷图片的minio路径
                string defectMinioPath = ExtractMinioPath(imagePath);
                
                // 修复2: group_infos - 构造template图片路径（在文件名后加[E]）
                string templateMinioPath = BuildTemplateMinioPath(defectMinioPath);

                // 修复3: defect_code - 使用缺陷类型
                string defectCode = defect?.DefectType ?? "";

                var group = new InferImageGroup
                {
                    GroupUuid = Guid.NewGuid().ToString(),
                    GroupInfos = new List<GroupInfo>
                    {
                        // defect 图片
                        new GroupInfo
                        {
                            ImagePath = defectMinioPath,
                            ImageUuid = Guid.NewGuid().ToString(),
                            ImageType = "defect"
                        },
                        // template 图片
                        new GroupInfo
                        {
                            ImagePath = templateMinioPath,
                            ImageUuid = Guid.NewGuid().ToString(),
                            ImageType = "template"
                        }
                    },
                    MachineTemplateInfo = new MachineTemplateInfo
                    {
                        MachineName = panel.MachineId,
                        product = panel.ProductSerial,
                        Side = side.Side
                    },
                    // 修复1: 使用正确的ROI值
                    ImgROI = new List<int> { roiX, roiY, roiW, roiH },
                    // 修复3: 添加 defect_code
                    DefectCode = defectCode,
                    // 修复4: 添加 tempImgPath
                    TempImgPath = tempImgPath,
                    inspectDetails = new InspectDetails { InferRois = new List<InferRoi>() }
                };
                vbInfo.paramsData.InferWholeData.ImageData.DataValue.InferImageGroup.Add(group);
            }

            return vbInfo;
        }

        /// <summary>
        /// 构造模板图片本地路径
        /// </summary>
        private string BuildTemplateImagePath(string productSerial, string machineName, string side)
        {
            if (string.IsNullOrEmpty(productSerial) || string.IsNullOrEmpty(machineName))
                return "";

            // 格式: TemplateImages/{ProductSerial}/{MachineName}/{ProductSerial}[{Side}].jpg
            // BaseDirectory 是 Bin 目录，需要取其父目录
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string parentDir = System.IO.Directory.GetParent(baseDir.TrimEnd('\\')).FullName;
            string templatePath = System.IO.Path.Combine(parentDir, "TemplateImages", productSerial, machineName, $"{productSerial}[{side}].jpg");
            return templatePath;
        }

        /// <summary>
        /// 从defect图片路径构造template图片路径（在文件名后添加[E]）
        /// 例如: xxx/[001][00138,00084][00192x00208].jpg -> xxx/[001][00138,00084][00192x00208][E].jpg
        /// </summary>
        private string BuildTemplateMinioPath(string defectPath)
        {
            if (string.IsNullOrEmpty(defectPath))
                return "";

            // 在扩展名前插入[E]
            int lastDotIndex = defectPath.LastIndexOf('.');
            if (lastDotIndex > 0)
            {
                return defectPath.Substring(0, lastDotIndex) + "[E]" + defectPath.Substring(lastDotIndex);
            }
            
            // 如果没有扩展名，直接在末尾添加[E]
            return defectPath + "[E]";
        }

        /// <summary>
        /// 从完整路径提取 Minio 相对路径
        /// </summary>
        private string ExtractMinioPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return fullPath;

            // 路径格式可能是: "deepiresults\xxx\xxx.jpg" 或 "ip:xxx/xxx.jpg"
            int index = fullPath.IndexOf("deepiresults", StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                var relativePath = fullPath.Substring(index + "deepiresults".Length).TrimStart('\\', '/');
                return relativePath.Replace('\\', '/');
            }

            // 如果是 ip:path 格式
            if (fullPath.Contains(":"))
            {
                var parts = fullPath.Split(':');
                if (parts.Length >= 2)
                    return parts[1];
            }

            return fullPath;
        }

        /// <summary>
        /// 加载测试用图片
        /// </summary>
        private List<Mat> LoadImagesForTest(List<DetectInfo> defects)
        {
            var mats = new List<Mat>();
            foreach (var defect in defects)
            {
                if (string.IsNullOrEmpty(defect.ImagePath)) continue;

                try
                {
                    var mat = Cv2.ImRead(defect.ImagePath);
                    if (mat != null)
                    {
                        // 需要保持 Mat 存活到推理调用结束
                        mats.Add(mat);
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Warn($"加载测试图片失败: {defect.ImagePath}, {ex.Message}");
                }
            }
            return mats;
        }

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

            var testKey = $"{vbModel.TestTaskId}_{vbModel.SN}_{vbModel.Side}";

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
                        task.Results.Add(sideResult);
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
                        if (task.IsReallyCompleted && task.State == ValidationTestTaskState.Running)
                        {
                            task.State = ValidationTestTaskState.Completed;
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

        #region 任务查询接口

        /// <summary>
        /// 获取测试任务状态
        /// </summary>
        public ValidationTestTask GetTaskStatus(string taskId)
        {
            _activeTasks.TryGetValue(taskId, out var task);
            return task;
        }

        /// <summary>
        /// 获取所有活跃任务
        /// </summary>
        public List<ValidationTestTask> GetActiveTasks()
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
                task.State = ValidationTestTaskState.Cancelled;
                task.EndTime = DateTime.Now;
                return true;
            }
            return false;
        }

        #endregion
    }
}

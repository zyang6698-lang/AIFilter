using DeepSightModel;
using DeepSightTool;
using DeepSightWorkLib.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// 模型验证测试结果显示控件
    /// </summary>
    public partial class UcValidationTestResult : UserControl
    {
        #region Fields

        private InferenceTask _currentTask;
        private List<SideTestResultDisplay> _allResults = new List<SideTestResultDisplay>();
        private List<SideTestResultDisplay> _filteredResults = new List<SideTestResultDisplay>();
        private Timer _refreshTimer;
        private SplitContainer _splitContainerResults;
        private UcAiResultComparison _comparisonControl;
        private bool _comparisonVisible;
        private bool _isRefreshingGrid;

        #endregion

        #region Constructor

        public UcValidationTestResult()
        {
            InitializeComponent();
            InitializeInlineComparisonLayout();
            InitializeStyles();
            InitializeEvents();
        }

        private void InitializeInlineComparisonLayout()
        {
            _splitContainerResults = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                BackColor = Color.FromArgb(30, 30, 30),
                Panel2Collapsed = true,
                SplitterWidth = 5
            };

            _comparisonControl = new UcAiResultComparison { Dock = DockStyle.Fill };
            _comparisonControl.SingleImageTestRequested += ComparisonControl_SingleImageTestRequested;

            splitContainer_Main.Panel2.Controls.Remove(panel_Filter);
            splitContainer_Main.Panel2.Controls.Remove(dataGridView_Results);
            dataGridView_Results.Dock = DockStyle.Fill;
            _splitContainerResults.Panel1.Controls.Add(dataGridView_Results);
            _splitContainerResults.Panel2.Controls.Add(_comparisonControl);
            splitContainer_Main.Panel2.Controls.Add(_splitContainerResults);
            splitContainer_Main.Panel2.Controls.Add(panel_Filter);
        }

        private void InitializeStyles()
        {
            dataGridView_Results.ApplyDarkTheme();

            // ComboBox 默认选项
            comboBox_Filter.SelectedIndex = 0;
        }

        private void InitializeEvents()
        {
            comboBox_Filter.SelectedIndexChanged += ComboBox_Filter_SelectedIndexChanged;
            btn_ExportResult.Click += Btn_ExportResult_Click;
            btn_CompareResults.Click += Btn_CompareResults_Click;
            dataGridView_Results.CellDoubleClick += DataGridView_Results_CellDoubleClick;
            dataGridView_Results.SelectionChanged += DataGridView_Results_SelectionChanged;

            // 定时刷新进度
            _refreshTimer = new Timer { Interval = 500 };
            _refreshTimer.Tick += RefreshTimer_Tick;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 开始监控测试任务
        /// </summary>
        public void StartMonitoring(InferenceTask task)
        {
            _currentTask = task;
            _isSecondaryInferenceMode = false;
            _allResults.Clear();
            _filteredResults.Clear();
            btn_CompareResults.Enabled = false;
            SetComparisonVisible(false);

            label_Title.Text = $"模型一致性测试 - {task.Description ?? task.TaskId}";
            UpdateProgress();
            _refreshTimer.Start();
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        public void StopMonitoring()
        {
            _refreshTimer.Stop();
        }

        #region 二次推理监控

        private bool _isSecondaryInferenceMode = false;

        /// <summary>
        /// 开始监控二次推理任务
        /// </summary>
        public void StartMonitoringSecondaryInference(InferenceTask task)
        {
            _currentTask = task;
            _isSecondaryInferenceMode = true;
            _allResults.Clear();
            _filteredResults.Clear();
            btn_CompareResults.Enabled = false;
            SetComparisonVisible(false);

            label_Title.Text = $"二次推理 - {task.Description ?? task.TaskId}";
            UpdateSecondaryInferenceProgress();
            _refreshTimer.Start();
        }

        /// <summary>
        /// 刷新二次推理进度
        /// </summary>
        private void UpdateSecondaryInferenceProgress()
        {
            if (_currentTask == null) return;

            int progress = (int)_currentTask.Progress;
            progressBar_Test.Value = Math.Min(progress, 100);

            string progressText = BuildProgressText();
            if (_currentTask.EnqueuedRecords != _currentTask.ProcessedRecords)
            {
                progressText += $" | 入队: {_currentTask.EnqueuedRecords}";
            }
            label_Progress.Text = progressText;

            // 更新统计信息 - 二次推理显示四种AI状态
            int total = _currentTask.SecondaryResults.Count;
            int totalOk = _currentTask.SecondaryResults.Sum(r => r.FinalOkCount);
            int totalNg = _currentTask.SecondaryResults.Sum(r => r.FinalNgCount);
            int totalBypass = _currentTask.SecondaryResults.Sum(r => r.FinalBypassCount);
            int totalUndetected = _currentTask.SecondaryResults.Sum(r => r.FinalUndetectedCount);
            int changedToOk = _currentTask.SecondaryResults.Sum(r => r.ChangedToOkCount);

            string summaryText = $"总处理面数: {total} | " +
                $"OK: {totalOk} | NG: {totalNg} | 异常: {totalBypass} | 未检测: {totalUndetected} | " +
                $"NG→OK: {changedToOk}";

            label_Summary.Text = summaryText;
        }

        /// <summary>
        /// 刷新二次推理结果列表
        /// </summary>
        private void RefreshSecondaryInferenceResults()
        {
            if (_currentTask == null) return;

            _allResults.Clear();
            foreach (var result in _currentTask.SecondaryResults)
            {
                // 原判定：显示推理前四种AI状态
                string originalText = BuildStatusText(result.OriginalOkCount, result.OriginalNgCount, result.OriginalBypassCount, result.OriginalUndetectedCount);
                // 新判定：显示推理后四种AI状态
                string newText = BuildStatusText(result.FinalOkCount, result.FinalNgCount, result.FinalBypassCount, result.FinalUndetectedCount);

                // 变化描述
                string changeText;
                if (result.ChangedToOkCount > 0)
                {
                    var changes = new List<string>();
                    changes.Add($"NG→OK: {result.ChangedToOkCount}");
                    int totalChanged = result.PointResults.Count(p => p.IsChanged);
                    if (totalChanged > result.ChangedToOkCount)
                        changes.Add($"其他变化: {totalChanged - result.ChangedToOkCount}");
                    changeText = string.Join(", ", changes);
                }
                else
                {
                    int totalChanged = result.PointResults.Count(p => p.IsChanged);
                    changeText = totalChanged > 0 ? $"变化: {totalChanged}" : "无变化";
                }

                _allResults.Add(new SideTestResultDisplay
                {
                    SerialNumber = result.SerialNumber,
                    Side = result.Side,
                    DataSourceText = "二次推理",
                    HasVVSData = false,
                    OriginalResult = originalText,
                    NewResult = newText,
                    IsConsistent = changeText,
                    DefectCount = result.OriginalNgCount + result.OriginalOkCount + result.OriginalBypassCount + result.OriginalUndetectedCount,
                    ConsistentCount = result.FinalOkCount,
                    InconsistentCount = result.FinalNgCount,
                    MissCount = result.FinalBypassCount,
                    OverKillCount = result.FinalUndetectedCount,
                    IsConsistentBool = result.ChangedToOkCount > 0,
                    Details = null
                });
            }

            ApplyFilter();
        }

        /// <summary>
        /// 构建四种AI状态的显示文本，仅显示数量大于0的状态
        /// </summary>
        private string BuildStatusText(int okCount, int ngCount, int bypassCount, int undetectedCount)
        {
            var parts = new List<string>();
            if (okCount > 0) parts.Add($"OK:{okCount}");
            if (ngCount > 0) parts.Add($"NG:{ngCount}");
            if (bypassCount > 0) parts.Add($"异常:{bypassCount}");
            if (undetectedCount > 0) parts.Add($"未检测:{undetectedCount}");
            return parts.Count > 0 ? string.Join(" | ", parts) : "无缺陷点";
        }

        #endregion

        /// <summary>
        /// 显示测试结果
        /// </summary>
        public void DisplayResults(InferenceTask task)
        {
            _currentTask = task;
            _refreshTimer.Stop();

            _allResults.Clear();
            foreach (var result in task.ConsistencyResults)
            {
                // 根据一致/不一致数量判断整体结果
                bool isConsistent = result.InconsistentCount == 0 && result.State != ValidationTestState.TestError;
                _allResults.Add(new SideTestResultDisplay
                {
                    SerialNumber = result.SerialNumber,
                    Side = result.Side,
                    DataSourceText = result.HasVVSData ? "VVS" : "AI",
                    HasVVSData = result.HasVVSData,
                    OriginalResult = result.OriginalSideResult ?? "-",
                    NewResult = result.NewSideResult ?? "-",
                    IsConsistent = isConsistent ? "✓ 一致" : "✗ 不一致",
                    DefectCount = result.TotalDefects,
                    ConsistentCount = result.ConsistentCount,
                    InconsistentCount = result.InconsistentCount,
                    MissCount = result.MissCount,
                    OverKillCount = result.OverKillCount,
                    IsConsistentBool = isConsistent,
                    Details = result.DefectResults
                });
            }

            UpdateProgress();
            ApplyFilter();
            UpdateCompareButtonState();
        }

        #endregion

        #region Private Methods

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            // 二次推理模式
            if (_isSecondaryInferenceMode)
            {
                RefreshSecondaryInferenceTimer();
                return;
            }

            // 一致性测试模式
            if (_currentTask == null) return;

            // 获取最新的任务状态
            var service = Machine.master?.ValidationTestService;
            if (service == null) return;

            var latestTask = service.GetTaskStatus(_currentTask.TaskId);
            if (latestTask != null)
            {
                _currentTask = latestTask;
                // 使用内部刷新方法，不停止定时器
                RefreshResultsInternal(latestTask);
            }

            // 只有在任务真正完成（所有结果都返回）后才停止刷新
            // 使用 IsReallyCompleted 而不是仅依赖 State
            if (_currentTask.State == InferenceTaskState.Failed ||
                _currentTask.State == InferenceTaskState.Cancelled ||
                (_currentTask.State == InferenceTaskState.Completed && _currentTask.IsReallyCompleted))
            {
                _refreshTimer.Stop();
                UpdateCompareButtonState();
            }
        }

        /// <summary>
        /// 二次推理模式的定时刷新
        /// </summary>
        private void RefreshSecondaryInferenceTimer()
        {
            if (_currentTask == null) return;

            var service = Machine.master?.ValidationTestService;
            if (service == null) return;

            var latestTask = service.GetTaskStatus(_currentTask.TaskId);
            if (latestTask != null)
            {
                _currentTask = latestTask;
                UpdateSecondaryInferenceProgress();
                RefreshSecondaryInferenceResults();
            }

            // 检查任务是否完成
            if (_currentTask.State == InferenceTaskState.Failed ||
                _currentTask.State == InferenceTaskState.Cancelled ||
                (_currentTask.State == InferenceTaskState.Completed && _currentTask.IsReallyCompleted))
            {
                _refreshTimer.Stop();
                _isSecondaryInferenceMode = false;
                UpdateCompareButtonState();
            }
        }

        /// <summary>
        /// 内部刷新方法，不停止定时器
        /// </summary>
        private void RefreshResultsInternal(InferenceTask task)
        {
            _allResults.Clear();
            foreach (var result in task.ConsistencyResults)
            {
                // 根据一致/不一致数量判断整体结果
                bool isConsistent = result.InconsistentCount == 0 && result.State != ValidationTestState.TestError;
                _allResults.Add(new SideTestResultDisplay
                {
                    SerialNumber = result.SerialNumber,
                    Side = result.Side,
                    DataSourceText = result.HasVVSData ? "VVS" : "AI",
                    HasVVSData = result.HasVVSData,
                    OriginalResult = result.OriginalSideResult ?? "-",
                    NewResult = result.NewSideResult ?? "-",
                    IsConsistent = isConsistent ? "✓ 一致" : "✗ 不一致",
                    DefectCount = result.TotalDefects,
                    ConsistentCount = result.ConsistentCount,
                    InconsistentCount = result.InconsistentCount,
                    MissCount = result.MissCount,
                    OverKillCount = result.OverKillCount,
                    IsConsistentBool = isConsistent,
                    Details = result.DefectResults
                });
            }

            UpdateProgress();
            ApplyFilter();
            UpdateCompareButtonState();
        }

        private void UpdateProgress()
        {
            if (_currentTask == null) return;

            // 显示实际处理进度（推理结果返回的进度）
            int progress = (int)_currentTask.Progress;
            progressBar_Test.Value = Math.Min(progress, 100);

            // 显示入队进度和处理进度
            string progressText = BuildProgressText();
            if (_currentTask.EnqueuedRecords != _currentTask.ProcessedRecords)
            {
                // 如果入队数和处理数不同，额外显示入队进度
                progressText += $" | 入队: {_currentTask.EnqueuedRecords}";
            }
            label_Progress.Text = progressText;

            // 更新统计信息
            int total = _allResults.Count;
            int consistent = _allResults.Count(r => r.IsConsistentBool);
            int inconsistent = total - consistent;
            double rate = total > 0 ? (double)consistent / total * 100 : 0;

            // 统计VVS数据的漏失和误报
            int vvsCount = _allResults.Count(r => r.HasVVSData);
            int totalMiss = _allResults.Where(r => r.HasVVSData).Sum(r => r.MissCount);
            int totalOverKill = _allResults.Where(r => r.HasVVSData).Sum(r => r.OverKillCount);

            string summaryText = $"总测试数: {total} | 一致: {consistent} | 不一致: {inconsistent} | " +
                $"一致率: {rate:F1}% | 总体一致率: {_currentTask.OverallConsistencyRate:F1}%";

            // 如果有VVS数据，显示漏失和误报指标
            if (vvsCount > 0)
            {
                summaryText += $"\nVVS数据: {vvsCount} | 漏失: {totalMiss} | 误报: {totalOverKill}";
            }

            label_Summary.Text = summaryText;
        }

        private string BuildProgressText()
        {
            if (_currentTask == null)
            {
                return string.Empty;
            }

            int progress = (int)_currentTask.Progress;
            return $"处理进度: {progress}% (处理:{_currentTask.ProcessedRecords} / 跳过:{_currentTask.SkippedRecords} / 错误:{_currentTask.ErrorRecords} / 总数:{_currentTask.TotalRecords})";
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

        private static string GetStatusText(int status, string source)
        {
            var statusText = GetStatusText(status);
            return string.IsNullOrWhiteSpace(source) ? statusText : $"{statusText}({source})";
        }

        private void ApplyFilter()
        {
            string filter = comboBox_Filter.SelectedItem?.ToString() ?? "全部";

            switch (filter)
            {
                case "一致":
                    _filteredResults = _allResults.Where(r => r.IsConsistentBool).ToList();
                    break;
                case "不一致":
                    _filteredResults = _allResults.Where(r => !r.IsConsistentBool).ToList();
                    break;
                default:
                    _filteredResults = _allResults.ToList();
                    break;
            }

            RefreshDataGrid();
        }

        private void RefreshDataGrid()
        {
            _isRefreshingGrid = true;
            try
            {
                dataGridView_Results.Rows.Clear();
                foreach (var result in _filteredResults)
                {
                    // 漏失和误报仅对VVS数据显示数值，AI数据显示"-"
                    string missDisplay = result.HasVVSData ? result.MissCount.ToString() : "-";
                    string overKillDisplay = result.HasVVSData ? result.OverKillCount.ToString() : "-";

                    int rowIndex = dataGridView_Results.Rows.Add(
                        result.SerialNumber,
                        result.Side,
                        result.DataSourceText,
                        result.OriginalResult,
                        result.NewResult,
                        result.IsConsistent,
                        result.DefectCount,
                        result.ConsistentCount,
                        result.InconsistentCount,
                        missDisplay,
                        overKillDisplay);

                    // 设置不一致行的颜色
                    if (!result.IsConsistentBool)
                    {
                        dataGridView_Results.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(120, 50, 50);
                    }

                    // VVS数据来源的行用不同颜色标识
                    if (result.HasVVSData)
                    {
                        dataGridView_Results.Rows[rowIndex].Cells["col_DataSource"].Style.ForeColor = Color.Cyan;
                    }
                }

                if (dataGridView_Results.Rows.Count > 0)
                {
                    dataGridView_Results.ClearSelection();
                    dataGridView_Results.Rows[0].Selected = true;
                    dataGridView_Results.SafeSetCurrentCell(dataGridView_Results.Rows[0].Cells[0]);
                }
            }
            finally
            {
                _isRefreshingGrid = false;
            }

            UpdateComparisonForSelectedRow();
        }

        private void ComboBox_Filter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void UpdateCompareButtonState()
        {
            bool canCompare = _currentTask != null
                && _currentTask.State == InferenceTaskState.Completed
                && _currentTask.IsReallyCompleted
                && HasComparisonData(_currentTask);

            btn_CompareResults.Enabled = canCompare;
            if (!canCompare)
            {
                SetComparisonVisible(false);
            }
        }

        private bool HasComparisonData(InferenceTask task)
        {
            if (task == null) return false;
            if (task.Mode == InferenceMode.SecondaryInference)
            {
                return task.SecondaryResults.Any(r => r.PointResults != null && r.PointResults.Any(p => p.DetectInfo != null));
            }
            return task.ConsistencyResults.Any(r => r.DefectResults != null && r.DefectResults.Any(d => d.DetectInfo != null));
        }

        private void Btn_CompareResults_Click(object sender, EventArgs e)
        {
            if (_currentTask == null || !HasComparisonData(_currentTask))
            {
                MessageBox.Show("当前任务没有可对比的AI结果。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetComparisonVisible(!_comparisonVisible);
            if (_comparisonVisible)
            {
                UpdateComparisonForSelectedRow();
            }
        }

        private void SetComparisonVisible(bool visible)
        {
            _comparisonVisible = visible;
            if (_splitContainerResults != null)
            {
                _splitContainerResults.Panel2Collapsed = !visible;
                if (visible)
                {
                    int availableHeight = _splitContainerResults.Height;
                    if (availableHeight > 260)
                    {
                        _splitContainerResults.SplitterDistance = Math.Max(120, availableHeight / 3);
                    }
                }
            }

            btn_CompareResults.Text = visible ? "隐藏对比详情" : "对比前后结果";
            if (!visible)
            {
                _comparisonControl?.ClearComparison();
            }
        }

        private void DataGridView_Results_SelectionChanged(object sender, EventArgs e)
        {
            if (_isRefreshingGrid) return;
            UpdateComparisonForSelectedRow();
        }

        private void UpdateComparisonForSelectedRow()
        {
            if (!_comparisonVisible || _comparisonControl == null || _currentTask == null)
            {
                return;
            }

            var selectedResult = GetSelectedResult();
            if (selectedResult == null)
            {
                _comparisonControl.ClearComparison();
                return;
            }

            _comparisonControl.DisplayTask(_currentTask, selectedResult.SerialNumber, selectedResult.Side);
        }

        private SideTestResultDisplay GetSelectedResult()
        {
            if (dataGridView_Results.CurrentRow == null)
            {
                return null;
            }

            int rowIndex = dataGridView_Results.CurrentRow.Index;
            if (rowIndex < 0 || rowIndex >= _filteredResults.Count)
            {
                return null;
            }

            return _filteredResults[rowIndex];
        }

        private void DataGridView_Results_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _filteredResults.Count) return;

            var result = _filteredResults[e.RowIndex];
            if (result.Details != null && result.Details.Count > 0)
            {
                ShowDefectDetails(result);
            }
        }

        private async void ComparisonControl_SingleImageTestRequested(object sender, SingleImageTestEventArgs e)
        {
            if (e?.HeatPoint == null)
                return;

            if (string.IsNullOrWhiteSpace(e.ProductSerial) || string.IsNullOrWhiteSpace(e.MachineId) || string.IsNullOrWhiteSpace(e.Side))
            {
                MessageBox.Show("缺少料号、机台或面别信息，无法运行单图测试。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var service = Machine.master?.ValidationTestService;
            if (service == null)
            {
                MessageBox.Show("模型验证测试服务未初始化。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Enabled = false;
                Cursor = Cursors.WaitCursor;

                var result = await service.RunSingleImageTestAsync(
                    e.HeatPoint,
                    e.ProductSerial,
                    e.MachineId,
                    e.Side,
                    timeout: 30);

                if (result.Success)
                {
                    string consistentText = result.IsConsistent ? "✓ 一致" : "✗ 不一致";
                    string message = $"单图测试完成！\n\n" +
                        $"原始AI结果: {GetStatusText(result.OriginalAIStatus)}\n" +
                        $"新AI结果: {GetStatusText(result.NewAIStatus)}\n" +
                        $"比对结果: {consistentText}";

                    if (!string.IsNullOrEmpty(result.InferDetailText))
                    {
                        message += $"\n\n--- 复判详情 ---\n{result.InferDetailText}";
                    }
                    else if (!string.IsNullOrEmpty(result.DefectName))
                    {
                        message += $"\n\n缺陷名称: {result.DefectName}";
                        if (!string.IsNullOrEmpty(result.DefectArea))
                            message += $"\n缺陷面积: {result.DefectArea}";
                    }

                    MessageBox.Show(message, "单图测试结果", MessageBoxButtons.OK,
                        result.IsConsistent ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"单图测试失败: {result.ErrorMessage}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"对比详情单图测试异常: {ex}");
                MessageBox.Show($"单图测试异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void ShowDefectDetails(SideTestResultDisplay result)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"序列号: {result.SerialNumber}  面别: {result.Side}  数据来源: {result.DataSourceText}");
            sb.AppendLine($"原判定: {result.OriginalResult}  新判定: {result.NewResult}");
            if (result.HasVVSData)
            {
                sb.AppendLine($"漏失: {result.MissCount}  误报: {result.OverKillCount}");
            }
            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"缺陷对比详情 (共 {result.DefectCount} 个):");
            sb.AppendLine();

            int index = 1;
            foreach (var defect in result.Details)
            {
                string orig;
                if (defect.DataSource == OriginalDataSourceType.VVS)
                {
                    orig = GetStatusText(defect.OriginalVVSStatus, "VVS");
                }
                else
                {
                    orig = GetStatusText(defect.OriginalAIStatus, "AI");
                }
                string newR = GetStatusText(defect.NewAIStatus, "AI");
                string status = defect.IsConsistent ? "✓" : "✗";

                // 标记漏失和误报
                string extraInfo = "";
                if (defect.IsMiss) extraInfo = " [漏失]";
                else if (defect.IsOverKill) extraInfo = " [误报]";

                sb.AppendLine($"  {index}. 缺陷{defect.DefectIndex}: 原={orig} 新={newR} {status}{extraInfo}");
                index++;
            }

            MessageBox.Show(sb.ToString(), "缺陷对比详情", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Btn_ExportResult_Click(object sender, EventArgs e)
        {
            if (_currentTask == null || _allResults.Count == 0)
            {
                MessageBox.Show("没有可导出的测试结果。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV文件|*.csv|文本文件|*.txt";
                dialog.FileName = $"模型一致性测试报告_{_currentTask.TaskId}_{DateTime.Now:yyyyMMddHHmmss}";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ExportReport(dialog.FileName);
                    MessageBox.Show("报告导出成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{dialog.FileName}\"");
                }
            }
        }

        private void ExportReport(string filePath)
        {
            var lines = new List<string>();

            // 报告头
            lines.Add("模型一致性测试报告");
            lines.Add($"任务ID,{_currentTask.TaskId}");
            lines.Add($"描述,{_currentTask.Description}");
            lines.Add($"创建时间,{_currentTask.CreateTime:yyyy-MM-dd HH:mm:ss}");
            lines.Add($"完成时间,{_currentTask.EndTime:yyyy-MM-dd HH:mm:ss}");
            lines.Add($"总测试数,{_currentTask.TotalRecords}");
            lines.Add($"一致数,{_currentTask.ConsistentRecords}");
            lines.Add($"不一致数,{_currentTask.InconsistentRecords}");
            lines.Add($"一致率,{_currentTask.OverallConsistencyRate:F2}%");

            // VVS相关统计
            if (_currentTask.VVSRecords > 0)
            {
                lines.Add($"VVS数据记录数,{_currentTask.VVSRecords}");
                lines.Add($"总漏失数,{_currentTask.TotalMissCount}");
                lines.Add($"总误报数,{_currentTask.TotalOverKillCount}");
            }
            lines.Add("");

            // 详细结果表头
            lines.Add("序列号,面别,数据来源,原判定,新判定,是否一致,缺陷总数,一致数,不一致数,漏失数,误报数");

            // 详细结果
            foreach (var result in _allResults)
            {
                string missDisplay = result.HasVVSData ? result.MissCount.ToString() : "-";
                string overKillDisplay = result.HasVVSData ? result.OverKillCount.ToString() : "-";

                lines.Add($"{result.SerialNumber},{result.Side},{result.DataSourceText},{result.OriginalResult}," +
                    $"{result.NewResult},{(result.IsConsistentBool ? "一致" : "不一致")}," +
                    $"{result.DefectCount},{result.ConsistentCount},{result.InconsistentCount},{missDisplay},{overKillDisplay}");
            }

            File.WriteAllLines(filePath, lines, Encoding.UTF8);
            LogTextHelper.Info($"测试报告已导出: {filePath}");
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// 面测试结果显示模型
    /// </summary>
    public class SideTestResultDisplay
    {
        public string SerialNumber { get; set; }
        public string Side { get; set; }
        /// <summary>
        /// 数据来源显示文本（AI/VVS）
        /// </summary>
        public string DataSourceText { get; set; }
        /// <summary>
        /// 是否包含VVS数据
        /// </summary>
        public bool HasVVSData { get; set; }
        public string OriginalResult { get; set; }
        public string NewResult { get; set; }
        public string IsConsistent { get; set; }
        public int DefectCount { get; set; }
        public int ConsistentCount { get; set; }
        public int InconsistentCount { get; set; }
        /// <summary>
        /// 漏失数（仅VVS数据有效）
        /// </summary>
        public int MissCount { get; set; }
        /// <summary>
        /// 误报数（仅VVS数据有效）
        /// </summary>
        public int OverKillCount { get; set; }
        public bool IsConsistentBool { get; set; }
        public List<DefectTestResult> Details { get; set; }
    }

    #endregion
}


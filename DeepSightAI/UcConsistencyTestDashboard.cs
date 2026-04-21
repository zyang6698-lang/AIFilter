using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using DeepSightModel;
using DeepSightTool;
using DeepSightWorkLib.Services;

namespace DeepSightAI
{
    /// <summary>
    /// 一致性测试看板 - 数据集管理 + 历史记录 + 趋势图表
    /// </summary>
    public partial class UcConsistencyTestDashboard : UserControl
    {
        #region Fields

        private readonly ConsistencyTestDataStore _dataStore = new ConsistencyTestDataStore();

        // 数据
        private List<ConsistencyTestDataset> _datasets = new List<ConsistencyTestDataset>();
        private ConsistencyTestDataset _selectedDataset;
        private ConsistencyTestHistory _selectedHistory;

        // 测试执行
        private bool _isTestRunning;
        private CancellationTokenSource _testCts;
        private System.Windows.Forms.Timer _testRefreshTimer;

        #endregion

        #region Constructor

        public UcConsistencyTestDashboard()
        {
            InitializeComponent();
            InitializeGridColumns();
            InitializeChartConfig();
            SubscribeEvents();
            LoadDatasets();
        }

        #endregion

        #region Post-Init Setup

        private void InitializeGridColumns()
        {
            dataGridView_Rounds.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "RoundNumber", HeaderText = "轮次", FillWeight = 40 },
                new DataGridViewTextBoxColumn { Name = "ModelName", HeaderText = "模型", FillWeight = 80 },
                new DataGridViewTextBoxColumn { Name = "StartTime", HeaderText = "开始时间", FillWeight = 100 },
                new DataGridViewTextBoxColumn { Name = "State", HeaderText = "状态", FillWeight = 50 },
                new DataGridViewTextBoxColumn { Name = "TotalRecords", HeaderText = "总记录", FillWeight = 50 },
                new DataGridViewTextBoxColumn { Name = "ConsistencyRate", HeaderText = "一致率%", FillWeight = 60 },
                new DataGridViewTextBoxColumn { Name = "MissRate", HeaderText = "漏失率%", FillWeight = 60 },
                new DataGridViewTextBoxColumn { Name = "OverKillRate", HeaderText = "误报率%", FillWeight = 60 },
            });
        }

        private void InitializeChartConfig()
        {
            var chartArea = new ChartArea("MainArea")
            {
                BackColor = Color.FromArgb(35, 35, 38),
                AxisX = {
                    Title = "轮次",
                    TitleForeColor = Color.FromArgb(200, 200, 200),
                    LabelStyle = { ForeColor = Color.FromArgb(180, 180, 180) },
                    LineColor = Color.FromArgb(80, 80, 80),
                    MajorGrid = { LineColor = Color.FromArgb(50, 50, 50) },
                    Interval = 1
                },
                AxisY = {
                    Title = "百分比 (%)",
                    TitleForeColor = Color.FromArgb(200, 200, 200),
                    LabelStyle = { ForeColor = Color.FromArgb(180, 180, 180) },
                    LineColor = Color.FromArgb(80, 80, 80),
                    MajorGrid = { LineColor = Color.FromArgb(50, 50, 50) },
                    Minimum = 0,
                    Maximum = 100
                }
            };
            chart_Trend.ChartAreas.Add(chartArea);

            var legend = new Legend
            {
                BackColor = Color.FromArgb(35, 35, 38),
                ForeColor = Color.FromArgb(200, 200, 200),
                Docking = Docking.Top
            };
            chart_Trend.Legends.Add(legend);

            chart_Trend.Series.Add(new Series("漏失率")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(255, 99, 71),
                BorderWidth = 2,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 6
            });

            chart_Trend.Series.Add(new Series("误报率")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(65, 160, 255),
                BorderWidth = 2,
                MarkerStyle = MarkerStyle.Square,
                MarkerSize = 6
            });

            chart_Trend.Series.Add(new Series("一致率")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(50, 205, 50),
                BorderWidth = 2,
                MarkerStyle = MarkerStyle.Triangle,
                MarkerSize = 6
            });
        }

        private void SubscribeEvents()
        {
            listBox_Datasets.SelectedIndexChanged += ListBox_Datasets_SelectedIndexChanged;
            btn_RefreshDatasets.Click += (s, e) => LoadDatasets();
            btn_DeleteDataset.Click += Btn_DeleteDataset_Click;
            btn_StartTest.Click += Btn_StartTest_Click;
        }

        #endregion

        #region Data Loading

        public void LoadDatasets()
        {
            try
            {
                _datasets = _dataStore.LoadDatasets();
                listBox_Datasets.Items.Clear();
                foreach (var ds in _datasets)
                {
                    listBox_Datasets.Items.Add($"{ds.Name}  ({ds.LotEntries.Count} Lots, {ds.TotalRecords} 条)");
                }
                if (_datasets.Count > 0) listBox_Datasets.SelectedIndex = 0;
                else ClearDisplay();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"加载数据集失败: {ex}");
            }
        }

        private void LoadRounds(ConsistencyTestHistory history)
        {
            dataGridView_Rounds.Rows.Clear();
            if (history == null || history.Rounds.Count == 0) return;

            foreach (var round in history.Rounds.OrderBy(r => r.RoundNumber))
            {
                string stateText;
                switch (round.State)
                {
                    case ConsistencyTestRoundState.Completed: stateText = "已完成"; break;
                    case ConsistencyTestRoundState.Running: stateText = "运行中"; break;
                    case ConsistencyTestRoundState.Failed: stateText = "失败"; break;
                    case ConsistencyTestRoundState.Cancelled: stateText = "已取消"; break;
                    default: stateText = "待执行"; break;
                }

                dataGridView_Rounds.Rows.Add(
                    round.RoundNumber,
                    string.IsNullOrEmpty(round.ModelName) ? "(默认)" : round.ModelName,
                    round.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    stateText,
                    round.TotalRecords,
                    round.ConsistencyRate.ToString("F1"),
                    round.MissRate.ToString("F1"),
                    round.OverKillRate.ToString("F1")
                );
            }
        }

        private void UpdateChart(ConsistencyTestHistory history)
        {
            foreach (var series in chart_Trend.Series)
                series.Points.Clear();

            if (history == null || history.Rounds.Count == 0) return;

            var completedRounds = history.Rounds
                .Where(r => r.State == ConsistencyTestRoundState.Completed)
                .OrderBy(r => r.RoundNumber)
                .ToList();

            foreach (var round in completedRounds)
            {
                chart_Trend.Series["漏失率"].Points.AddXY(round.RoundNumber, round.MissRate);
                chart_Trend.Series["误报率"].Points.AddXY(round.RoundNumber, round.OverKillRate);
                chart_Trend.Series["一致率"].Points.AddXY(round.RoundNumber, round.ConsistencyRate);
            }

            // 动态调Y轴范围
            if (completedRounds.Count > 0)
            {
                var maxVal = completedRounds.Max(r => Math.Max(r.MissRate, Math.Max(r.OverKillRate, r.ConsistencyRate)));
                chart_Trend.ChartAreas[0].AxisY.Maximum = Math.Min(100, Math.Ceiling(maxVal / 10) * 10 + 10);
            }
        }

        private void ClearDisplay()
        {
            dataGridView_Rounds.Rows.Clear();
            foreach (var series in chart_Trend.Series)
                series.Points.Clear();
            label_RoundsTitle.Text = "测试轮次记录";
        }

        #endregion

        #region Event Handlers

        private void ListBox_Datasets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Datasets.SelectedIndex < 0 || listBox_Datasets.SelectedIndex >= _datasets.Count)
            {
                ClearDisplay();
                return;
            }

            _selectedDataset = _datasets[listBox_Datasets.SelectedIndex];
            _selectedHistory = _dataStore.LoadHistory(_selectedDataset.Id);
            _selectedHistory.DatasetName = _selectedDataset.Name;

            label_RoundsTitle.Text = $"测试轮次记录 - {_selectedDataset.Name} ({_selectedHistory.Rounds.Count} 轮)";
            LoadRounds(_selectedHistory);
            UpdateChart(_selectedHistory);
        }

        private void Btn_DeleteDataset_Click(object sender, EventArgs e)
        {
            if (listBox_Datasets.SelectedIndex < 0) return;
            var ds = _datasets[listBox_Datasets.SelectedIndex];
            var result = MessageBox.Show(
                $"确定删除数据集 \"{ds.Name}\" 及其所有历史记录？\n此操作不可恢复。",
                "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            _dataStore.DeleteDataset(ds.Id);
            _dataStore.DeleteHistory(ds.Id);
            LoadDatasets();
        }

        #endregion

        #region Start Test

        /// <summary>
        /// 开始测试按钮点击
        /// </summary>
        private async void Btn_StartTest_Click(object sender, EventArgs e)
        {
            if (_isTestRunning)
            {
                _testCts?.Cancel();
                return;
            }

            if (_selectedDataset == null || _selectedDataset.LotEntries.Count == 0)
            {
                MessageBox.Show("请先选择一个包含Lot的数据集。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var service = Machine.master?.ValidationTestService;
            if (service == null)
            {
                MessageBox.Show("模型验证测试服务未初始化，请确认系统已完成启动。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int totalRounds = (int)numericUpDown_Rounds.Value;
            var confirmResult = MessageBox.Show(
                $"将对数据集 \"{_selectedDataset.Name}\" 中的 {_selectedDataset.LotEntries.Count} 个Lot执行 {totalRounds} 轮一致性测试。\n\n" +
                "测试过程中请勿关闭程序。\n确定开始？",
                "确认开始测试", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult != DialogResult.Yes) return;

            _isTestRunning = true;
            _testCts = new CancellationTokenSource();
            btn_StartTest.Text = "取消测试";
            btn_DeleteDataset.Enabled = false;
            btn_RefreshDatasets.Enabled = false;
            listBox_Datasets.Enabled = false;
            numericUpDown_Rounds.Enabled = false;
            panel_TestProgress.Visible = true;

            try
            {
                for (int r = 0; r < totalRounds; r++)
                {
                    if (_testCts.IsCancellationRequested) break;
                    label_TestProgress.Text = $"第 {r + 1}/{totalRounds} 轮...";
                    await RunConsistencyTestAsync(service);
                    if (_testCts.IsCancellationRequested) break;
                }
            }
            finally
            {
                _isTestRunning = false;
                btn_StartTest.Text = "开始测试";
                btn_DeleteDataset.Enabled = true;
                btn_RefreshDatasets.Enabled = true;
                listBox_Datasets.Enabled = true;
                numericUpDown_Rounds.Enabled = true;
                panel_TestProgress.Visible = false;
            }
        }

        /// <summary>
        /// 执行一致性测试 - 对数据集中每个Lot创建测试任务，汇总结果
        /// </summary>
        private async Task RunConsistencyTestAsync(ModelValidationTestService service)
        {
            var dataset = _selectedDataset;
            var lotEntries = dataset.LotEntries.ToList();

            var round = new ConsistencyTestRound
            {
                DatasetId = dataset.Id,
                DatasetName = dataset.Name,
                StartTime = DateTime.Now,
                State = ConsistencyTestRoundState.Running,
                ModelName = ""
            };

            _dataStore.AddRound(dataset.Id, round);

            var allTasks = new List<InferenceTask>();
            int totalLots = lotEntries.Count;

            try
            {
                for (int i = 0; i < totalLots; i++)
                {
                    if (_testCts.Token.IsCancellationRequested) break;

                    var lot = lotEntries[i];
                    label_TestProgress.Text = $"正在启动 Lot {i + 1}/{totalLots}: {lot.LotNumber}...";
                    progressBar_Test.Value = (int)((double)i / totalLots * 30);

                    var request = new ValidationTestRequest
                    {
                        LotNumber = lot.LotNumber,
                        StartDate = lot.StartDate.AddMinutes(-1),
                        EndDate = lot.EndDate.AddMinutes(1),
                        MaxRecords = lot.RecordCount * 2,
                        Description = $"一致性测试 - {dataset.Name} - {lot.LotNumber}"
                    };

                    var task = await service.CreateTestTaskAsync(request, _testCts.Token);
                    if (task != null)
                        allTasks.Add(task);
                }

                await WaitForAllTasksAsync(service, allTasks);

                if (_testCts.Token.IsCancellationRequested)
                {
                    round.State = ConsistencyTestRoundState.Cancelled;
                    round.EndTime = DateTime.Now;
                }
                else
                {
                    AggregateResults(round, allTasks);
                    round.State = ConsistencyTestRoundState.Completed;
                    round.EndTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"一致性测试执行失败: {ex}");
                round.State = ConsistencyTestRoundState.Failed;
                round.EndTime = DateTime.Now;
                MessageBox.Show($"测试执行失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UpdateRoundInHistory(dataset.Id, round);

                _selectedHistory = _dataStore.LoadHistory(dataset.Id);
                _selectedHistory.DatasetName = dataset.Name;
                label_RoundsTitle.Text = $"测试轮次记录 - {dataset.Name} ({_selectedHistory.Rounds.Count} 轮)";
                LoadRounds(_selectedHistory);
                UpdateChart(_selectedHistory);
            }
        }

        private async Task WaitForAllTasksAsync(ModelValidationTestService service, List<InferenceTask> tasks)
        {
            while (!_testCts.Token.IsCancellationRequested)
            {
                int completedCount = 0;
                int totalProcessed = 0;
                int totalRecords = 0;

                foreach (var t in tasks)
                {
                    var latest = service.GetTaskStatus(t.TaskId);
                    if (latest != null)
                    {
                        totalProcessed += latest.ProcessedRecords;
                        totalRecords += latest.TotalRecords;

                        if ((latest.State == InferenceTaskState.Completed && latest.IsReallyCompleted) ||
                            latest.State == InferenceTaskState.Failed ||
                            latest.State == InferenceTaskState.Cancelled)
                        {
                            completedCount++;
                        }
                    }
                }

                double taskProgress = totalRecords > 0 ? (double)totalProcessed / totalRecords : 0;
                int overallProgress = 30 + (int)(taskProgress * 70);
                progressBar_Test.Value = Math.Min(100, overallProgress);
                label_TestProgress.Text = $"测试进度: {completedCount}/{tasks.Count} Lots, {totalProcessed}/{totalRecords} 条记录";

                if (completedCount >= tasks.Count)
                    break;

                await Task.Delay(1000, _testCts.Token).ContinueWith(_ => { });
            }
        }

        private void AggregateResults(ConsistencyTestRound round, List<InferenceTask> tasks)
        {
            foreach (var t in tasks)
            {
                round.TotalRecords += t.ProcessedRecords;
                round.ConsistentRecords += t.ConsistentRecords;
                round.InconsistentRecords += t.InconsistentRecords;
                round.VVSRecords += t.VVSRecords;
                round.TotalMissCount += t.TotalMissCount;
                round.TotalOverKillCount += t.TotalOverKillCount;
                if (t.ConsistencyResults != null)
                {
                    round.TotalDefects += t.ConsistencyResults.Sum(r => r.TotalDefects);
                }
            }
        }

        private void UpdateRoundInHistory(string datasetId, ConsistencyTestRound round)
        {
            var history = _dataStore.LoadHistory(datasetId);
            var idx = history.Rounds.FindIndex(r => r.RoundId == round.RoundId);
            if (idx >= 0)
                history.Rounds[idx] = round;
            else
                history.Rounds.Add(round);
            _dataStore.SaveHistory(history);
        }

        #endregion
    }
}


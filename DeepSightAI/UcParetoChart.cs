using DeepSightModel;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DeepSightAI
{
    /// <summary>
    /// 帕累托图控件：以 Lot 为基准，展示缺陷按发生频次排序的柱状图与累计百分比折线
    /// 支持多数据源合并展示：AI结果 / VVS结果 / VRS结果（可多选1~3个）
    /// 当前 Lot 由父窗体通过 ShowLot 注入（与 UcDefectQuery.cmb_Lot 联动）。
    /// </summary>
    public partial class UcParetoChart : UserControl
    {
        private string _currentLot;
        private List<DefectReviewItem> _currentItems;

        /// <summary>
        /// 数据来源定义
        /// </summary>
        private struct SourceDef
        {
            public string Name;           // 显示名称
            public Color BarColor;        // 柱状图颜色
            public Color LineColor;       // 折线颜色
            public string SeriesPrefix;   // Series 名称前缀
            public Func<DetectInfo, bool> HasData;   // 是否有数据
            public Func<DetectInfo, bool> IsNg;      // 是否为NG
        }

        private static readonly SourceDef[] AllSources = new[]
        {
            new SourceDef
            {
                Name = "AI",
                BarColor = Color.FromArgb(65, 160, 255),
                LineColor = Color.FromArgb(100, 190, 255),
                SeriesPrefix = "AI",
                HasData = hp => hp.AIStatus != 0,
                IsNg = hp => hp.AIStatus == 2
            },
            new SourceDef
            {
                Name = "VVS",
                BarColor = Color.FromArgb(255, 107, 107),
                LineColor = Color.FromArgb(255, 150, 150),
                SeriesPrefix = "VVS",
                HasData = hp => hp.VVSStatus != 0,
                IsNg = hp => hp.VVSStatus == 2
            },
            new SourceDef
            {
                Name = "VRS",
                BarColor = Color.FromArgb(81, 207, 102),
                LineColor = Color.FromArgb(120, 230, 140),
                SeriesPrefix = "VRS",
                HasData = hp => hp.VrsState != 0,
                IsNg = hp => hp.VrsState == 2 || hp.VrsState == 5
            }
        };

        public UcParetoChart()
        {
            InitializeComponent();
            InitializeChartConfig();
            WireEvents();
        }

        private void WireEvents()
        {
            chk_AI.CheckedChanged += (s, e) => RefreshChart();
            chk_VVS.CheckedChanged += (s, e) => RefreshChart();
            chk_VRS.CheckedChanged += (s, e) => RefreshChart();
            btn_Refresh.Click += (s, e) => RefreshChart();
        }

        private void InitializeChartConfig()
        {
            chart_Pareto.ChartAreas.Clear();
            chart_Pareto.Series.Clear();
            chart_Pareto.Legends.Clear();

            var area = new ChartArea("MainArea")
            {
                BackColor = Color.FromArgb(35, 35, 38),
                AxisX = {
                    Title = "缺陷名称",
                    TitleForeColor = Color.FromArgb(200, 200, 200),
                    LabelStyle = { ForeColor = Color.FromArgb(180, 180, 180), Angle = -30 },
                    LineColor = Color.FromArgb(80, 80, 80),
                    MajorGrid = { LineColor = Color.FromArgb(50, 50, 50), Enabled = false },
                    Interval = 1
                },
                AxisY = {
                    Title = "频次",
                    TitleForeColor = Color.FromArgb(200, 200, 200),
                    LabelStyle = { ForeColor = Color.FromArgb(180, 180, 180) },
                    LineColor = Color.FromArgb(80, 80, 80),
                    MajorGrid = { LineColor = Color.FromArgb(50, 50, 50) },
                    Minimum = 0
                },
                AxisY2 = {
                    Title = "累计百分比 (%)",
                    TitleForeColor = Color.FromArgb(200, 200, 200),
                    LabelStyle = { ForeColor = Color.FromArgb(180, 180, 180) },
                    LineColor = Color.FromArgb(80, 80, 80),
                    MajorGrid = { Enabled = false },
                    Minimum = 0,
                    Maximum = 100,
                    Enabled = AxisEnabled.True
                }
            };
            chart_Pareto.ChartAreas.Add(area);

            var legend = new Legend
            {
                BackColor = Color.FromArgb(35, 35, 38),
                ForeColor = Color.FromArgb(200, 200, 200),
                Docking = Docking.Top
            };
            chart_Pareto.Legends.Add(legend);
        }

        /// <summary>
        /// 由父窗体在切换 Lot 时调用，展示指定 Lot 的帕累托图。
        /// </summary>
        public void ShowLot(string lotNumber, List<DefectReviewItem> items)
        {
            _currentLot = lotNumber;
            _currentItems = items;
            RefreshChart();
        }

        /// <summary>
        /// 清空图表显示，用于切换查询条件或两段式加载第一阶段。
        /// </summary>
        public void Clear(string hint = "无数据，请先执行查询")
        {
            _currentLot = null;
            _currentItems = null;
            ClearChart(hint);
        }

        private void ClearChart(string message)
        {
            foreach (var s in chart_Pareto.Series) s.Points.Clear();
            label_Status.Text = message ?? string.Empty;
            lbl_AIStatus.Text = "● AI: 无数据";
            lbl_VVSStatus.Text = "● VVS: 无数据";
            lbl_VRSStatus.Text = "● VRS: 无数据";
            lbl_AIStatus.ForeColor = Color.FromArgb(200, 200, 200);
            lbl_VVSStatus.ForeColor = Color.FromArgb(200, 200, 200);
            lbl_VRSStatus.ForeColor = Color.FromArgb(200, 200, 200);
        }

        /// <summary>
        /// 获取当前选中的来源列表（按用户 CheckBox 勾选状态）
        /// </summary>
        private List<SourceDef> GetSelectedSources()
        {
            var selected = new List<SourceDef>();
            if (chk_AI.Checked) selected.Add(AllSources[0]);
            if (chk_VVS.Checked) selected.Add(AllSources[1]);
            if (chk_VRS.Checked) selected.Add(AllSources[2]);
            return selected;
        }

        /// <summary>
        /// 统计单个来源的缺陷分组结果
        /// </summary>
        private class SourceStatistics
        {
            public SourceDef Source;
            public List<IGrouping<string, DetectInfo>> GroupedPoints;
            public int TotalPoints;
            public int NgPoints;
            public bool HasData;
        }

        /// <summary>
        /// 根据当前选中的 Lot 与勾选的数据来源刷新帕累托图
        /// 支持多来源合并展示（簇状柱形图 + 各自累计百分比折线）
        /// 过滤 AiState == 0 的 DefectReviewItem（无AI结果不参与统计）
        /// </summary>
        private void RefreshChart()
        {
            try
            {
                string lot = _currentLot;
                var items = _currentItems;
                if (string.IsNullOrEmpty(lot) || items == null)
                {
                    ClearChart("请选择 Lot");
                    return;
                }

                // 过滤 OriginalAiState == 0 的项（无AI结果不参与统计）
                var validItems = items
                    .Where(it => it != null && it.HeatPoints != null && it.OriginalAiState != 0)
                    .ToList();

                if (validItems.Count == 0)
                {
                    ClearChart($"Lot: {lot} 无有效AI结果数据（OriginalAiState全为0）");
                    return;
                }

                var selectedSources = GetSelectedSources();
                if (selectedSources.Count == 0)
                {
                    ClearChart("请至少选择一个数据来源");
                    return;
                }

                // 对每个选中来源分别统计
                var statsList = new List<SourceStatistics>();
                foreach (var src in selectedSources)
                {
                    var points = validItems
                        .SelectMany(it => it.HeatPoints)
                        .Where(hp => hp != null && src.HasData(hp))
                        .ToList();

                    var grouped = points
                        .GroupBy(hp => string.IsNullOrEmpty(hp.DefectName) ? "(未命名)" : hp.DefectName)
                        .ToList();

                    statsList.Add(new SourceStatistics
                    {
                        Source = src,
                        GroupedPoints = grouped,
                        TotalPoints = points.Count,
                        NgPoints = points.Count(p => src.IsNg(p)),
                        HasData = points.Count > 0
                    });
                }

                // 检查是否所有来源都无数据
                if (statsList.All(s => !s.HasData))
                {
                    var sourceNames = string.Join("/", statsList.Select(s => s.Source.Name));
                    ClearChart($"Lot: {lot} 无 {sourceNames} 结果数据");
                    UpdateTopStatusLabels(statsList);
                    return;
                }

                // 收集所有缺陷名称的并集作为 X 轴分类
                var allDefectNames = new HashSet<string>();
                foreach (var st in statsList)
                {
                    if (st.HasData)
                    {
                        foreach (var g in st.GroupedPoints)
                            allDefectNames.Add(g.Key);
                    }
                }

                // 按总频次排序（取所有来源中该缺陷的最大频次来排序，保持一致性）
                var nameOrder = allDefectNames
                    .OrderByDescending(name =>
                        statsList.Where(s => s.HasData)
                            .Select(s => s.GroupedPoints.FirstOrDefault(g => g.Key == name)?.Count() ?? 0)
                            .DefaultIfEmpty(0)
                            .Max())
                    .ToList();

                // 清除旧 Series 并动态创建
                chart_Pareto.Series.Clear();

                // 对各来源数据归一化为 nameOrder 顺序，缺失值填 0
                var normalizedData = new Dictionary<string, List<int>>();
                foreach (var st in statsList)
                {
                    var data = new List<int>();
                    var countDict = st.HasData
                        ? st.GroupedPoints.ToDictionary(g => g.Key, g => g.Count())
                        : new Dictionary<string, int>();
                    foreach (var name in nameOrder)
                    {
                        data.Add(countDict.TryGetValue(name, out int c) ? c : 0);
                    }
                    normalizedData[st.Source.SeriesPrefix] = data;
                }

                // 计算各来源的累计值（用于折线，每个来源独立计算）
                var cumulativeData = new Dictionary<string, List<double>>();
                foreach (var st in statsList)
                {
                    var data = normalizedData[st.Source.SeriesPrefix];
                    int total = st.TotalPoints;
                    var cumList = new List<double>();
                    int running = 0;
                    for (int i = 0; i < nameOrder.Count; i++)
                    {
                        running += data[i];
                        double pct = total > 0 ? running * 100.0 / total : 0;
                        cumList.Add(pct);
                    }
                    cumulativeData[st.Source.SeriesPrefix] = cumList;
                }

                // 创建柱状图 Series（每个来源一个）
                foreach (var st in statsList)
                {
                    var barSeries = new Series($"{st.Source.SeriesPrefix}_频次")
                    {
                        ChartType = SeriesChartType.Column,
                        Color = st.Source.BarColor,
                        BorderColor = Color.FromArgb(
                            Math.Max(0, st.Source.BarColor.R - 30),
                            Math.Max(0, st.Source.BarColor.G - 60),
                            Math.Max(0, st.Source.BarColor.B - 50)),
                        IsValueShownAsLabel = true,
                        LabelForeColor = Color.White,
                        ["PixelPointWidth"] = Math.Max(8, 40 / selectedSources.Count).ToString()
                    };
                    chart_Pareto.Series.Add(barSeries);

                    var data = normalizedData[st.Source.SeriesPrefix];
                    for (int i = 0; i < nameOrder.Count; i++)
                    {
                        barSeries.Points.AddXY(nameOrder[i], data[i]);
                    }
                }

                // 创建累计百分比折线 Series（每个来源一个）
                foreach (var st in statsList)
                {
                    var lineSeries = new Series($"{st.Source.SeriesPrefix}_累计%")
                    {
                        ChartType = SeriesChartType.Line,
                        Color = st.Source.LineColor,
                        BorderWidth = 2,
                        MarkerStyle = MarkerStyle.Circle,
                        MarkerSize = 6,
                        MarkerColor = st.Source.LineColor,
                        YAxisType = AxisType.Secondary,
                        IsValueShownAsLabel = true,
                        LabelForeColor = st.Source.LineColor,
                        LabelFormat = "F1"
                    };
                    chart_Pareto.Series.Add(lineSeries);

                    var cumData = cumulativeData[st.Source.SeriesPrefix];
                    for (int i = 0; i < nameOrder.Count; i++)
                    {
                        int idx = lineSeries.Points.AddXY(nameOrder[i], cumData[i]);
                        lineSeries.Points[idx].Label = cumData[i].ToString("F1") + "%";
                    }
                }

                chart_Pareto.ChartAreas[0].RecalculateAxesScale();

                // 更新顶部状态标签
                UpdateTopStatusLabels(statsList);

                // 更新底部状态栏
                var statusParts = new List<string> { $"Lot: {lot}" };
                foreach (var st in statsList)
                {
                    string sourceTag = st.Source.Name;
                    if (st.HasData)
                    {
                        statusParts.Add($"{sourceTag}: {st.TotalPoints}点(NG:{st.NgPoints})");
                    }
                    else
                    {
                        statusParts.Add($"{sourceTag}: ⚠ 无数据");
                    }
                }
                statusParts.Add($"缺陷类别: {nameOrder.Count}");
                label_Status.Text = string.Join("  |  ", statusParts);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"刷新帕累托图异常: {ex}");
                ClearChart("刷新失败，请查看日志");
            }
        }

        /// <summary>
        /// 更新顶部各数据源的状态标签（显示颜色圆点 + 数据量/缺失状态）
        /// </summary>
        private void UpdateTopStatusLabels(List<SourceStatistics> statsList)
        {
            foreach (var st in statsList)
            {
                Label lbl;
                switch (st.Source.Name)
                {
                    case "AI": lbl = lbl_AIStatus; break;
                    case "VVS": lbl = lbl_VVSStatus; break;
                    case "VRS": lbl = lbl_VRSStatus; break;
                    default: continue;
                }

                if (st.HasData)
                {
                    lbl.Text = $"● {st.Source.Name}: {st.TotalPoints}点";
                    lbl.ForeColor = st.Source.BarColor;
                }
                else
                {
                    lbl.Text = $"○ {st.Source.Name}: 无数据";
                    lbl.ForeColor = Color.FromArgb(140, 140, 140);
                }
            }
        }
    }
}
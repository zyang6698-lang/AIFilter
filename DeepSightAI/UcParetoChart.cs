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
    /// 数据来源可切换：VRS 结果（VrsState != 0）或 AI 结果（AIStatus != 0）
    /// 当前 Lot 由父窗体通过 ShowLot 注入（与 UcDefectQuery.cmb_Lot 联动）。
    /// </summary>
    public partial class UcParetoChart : UserControl
    {
        private string _currentLot;
        private List<DefectReviewItem> _currentItems;

        private const string DataSourceVrs = "VRS结果";
        private const string DataSourceAi = "AI结果";

        public UcParetoChart()
        {
            InitializeComponent();
            InitializeChartConfig();
            InitializeDataSourceCombo();
            cmb_DataSource.SelectedIndexChanged += (s, e) => RefreshChart();
            btn_Refresh.Click += (s, e) => RefreshChart();
        }

        private void InitializeDataSourceCombo()
        {
            cmb_DataSource.Items.Clear();
            cmb_DataSource.Items.Add(DataSourceVrs);
            cmb_DataSource.Items.Add(DataSourceAi);
            cmb_DataSource.SelectedIndex = 0;
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

            chart_Pareto.Series.Add(new Series("缺陷频次")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(65, 160, 255),
                BorderColor = Color.FromArgb(30, 90, 150),
                IsValueShownAsLabel = true,
                LabelForeColor = Color.White
            });

            chart_Pareto.Series.Add(new Series("累计百分比")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(255, 165, 0),
                BorderWidth = 2,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 7,
                MarkerColor = Color.FromArgb(255, 165, 0),
                YAxisType = AxisType.Secondary,
                IsValueShownAsLabel = true,
                LabelForeColor = Color.FromArgb(255, 200, 100),
                LabelFormat = "F1"
            });
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
        }

        /// <summary>
        /// 根据当前选中的 Lot 与数据来源刷新帕累托图
        /// VRS：仅统计 VrsState != 0 的报点；AI：仅统计 AIStatus != 0 的报点
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

                bool useAi = (cmb_DataSource.SelectedItem as string) == DataSourceAi;
                string sourceTag = useAi ? "AI" : "VRS";

                var points = items
                    .Where(it => it?.HeatPoints != null)
                    .SelectMany(it => it.HeatPoints)
                    .Where(hp => hp != null && (useAi ? hp.AIStatus != 0 : hp.VrsState != 0))
                    .ToList();

                if (points.Count == 0)
                {
                    ClearChart($"Lot: {lot} 无 {sourceTag} 结果数据");
                    return;
                }

                var grouped = points
                    .GroupBy(hp => string.IsNullOrEmpty(hp.DefectName) ? "(未命名)" : hp.DefectName)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                int total = grouped.Sum(x => x.Count);
                var barSeries = chart_Pareto.Series["缺陷频次"];
                var lineSeries = chart_Pareto.Series["累计百分比"];
                barSeries.Points.Clear();
                lineSeries.Points.Clear();

                int cumulative = 0;
                int ngPoints = useAi
                    ? points.Count(p => p.AIStatus == 2)
                    : points.Count(p => p.VrsState == 2 || p.VrsState == 5);
                for (int i = 0; i < grouped.Count; i++)
                {
                    var item = grouped[i];
                    cumulative += item.Count;
                    double cumulativePct = total > 0 ? cumulative * 100.0 / total : 0;
                    barSeries.Points.AddXY(item.Name, item.Count);
                    int li = lineSeries.Points.AddXY(item.Name, cumulativePct);
                    lineSeries.Points[li].Label = cumulativePct.ToString("F1") + "%";
                }

                chart_Pareto.ChartAreas[0].RecalculateAxesScale();
                label_Status.Text = $"Lot: {lot}  数据来源: {sourceTag}  缺陷类别: {grouped.Count}  总报点: {total}  {sourceTag} NG: {ngPoints}";
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"刷新帕累托图异常: {ex}");
                ClearChart("刷新失败，请查看日志");
            }
        }
    }
}

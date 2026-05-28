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
    public partial class UcParetoChart : UserControl
    {
        private string _currentLot;
        private List<DefectReviewItem> _currentItems;

        private struct SourceDef
        {
            public string Name;
            public Color BarColor;
            public Color LineColor;
            public Func<DetectInfo, bool> HasData;
            public Func<DetectInfo, bool> IsNg;
        }

        private static readonly SourceDef SrcAI = new SourceDef
        {
            Name = "AI",
            BarColor = Color.FromArgb(65, 160, 255),
            LineColor = Color.FromArgb(100, 190, 255),
            HasData = hp => hp.AIStatus != 0,
            IsNg = hp => hp.AIStatus == 2
        };

        private static readonly SourceDef SrcVVS = new SourceDef
        {
            Name = "VVS",
            BarColor = Color.FromArgb(255, 107, 107),
            LineColor = Color.FromArgb(255, 150, 150),
            HasData = hp => hp.VVSStatus != 0,
            IsNg = hp => hp.VVSStatus == 2
        };

        private static readonly SourceDef SrcVRS = new SourceDef
        {
            Name = "VRS",
            BarColor = Color.FromArgb(81, 207, 102),
            LineColor = Color.FromArgb(120, 230, 140),
            HasData = hp => hp.VrsState != 0,
            IsNg = hp => hp.VrsState == 2 || hp.VrsState == 5
        };

        private static readonly string[] SourceNames = { "AI", "VVS", "VRS" };

        public UcParetoChart()
        {
            InitializeComponent();
            InitComboBoxes();
            InitAllCharts();
            WireEvents();
        }

        private void InitComboBoxes()
        {
            cmb_TopLeft.Items.AddRange(SourceNames);
            cmb_TopLeft.SelectedIndex = 0;
            cmb_TopRight.Items.AddRange(SourceNames);
            cmb_TopRight.SelectedIndex = 1;
        }

        private void WireEvents()
        {
            cmb_TopLeft.SelectedIndexChanged += (s, e) => RefreshAllCharts();
            cmb_TopRight.SelectedIndexChanged += (s, e) => RefreshAllCharts();
            rdo_VVS.CheckedChanged += (s, e) => RefreshAllCharts();
            chk_OriginName.CheckedChanged += (s, e) => RefreshAllCharts();
            btn_Refresh.Click += (s, e) => RefreshAllCharts();
        }

        private void InitAllCharts()
        {
            ConfigureChart(chart_TopLeft, "");
            ConfigureChart(chart_TopRight, "");
            ConfigureChart(chart_BottomLeft, "AI OK & ?NG");
            ConfigureChart(chart_BottomRight, "AI NG & ?NG");
        }

        private void ConfigureChart(Chart chart, string defaultTitle)
        {
            chart.ChartAreas.Clear();
            chart.Series.Clear();
            chart.Legends.Clear();

            var area = new ChartArea("MainArea")
            {
                BackColor = Color.FromArgb(35, 35, 38),
                AxisX = {
                    LabelStyle = { ForeColor = Color.FromArgb(180, 180, 180), Angle = -30, Font = new Font("微软雅黑", 7F) },
                    LineColor = Color.FromArgb(80, 80, 80),
                    MajorGrid = { Enabled = false },
                    Interval = 1
                },
                AxisY = {
                    Title = "频次",
                    TitleForeColor = Color.FromArgb(200, 200, 200),
                    TitleFont = new Font("微软雅黑", 7F),
                    LabelStyle = { ForeColor = Color.FromArgb(180, 180, 180), Font = new Font("微软雅黑", 7F) },
                    LineColor = Color.FromArgb(80, 80, 80),
                    MajorGrid = { LineColor = Color.FromArgb(50, 50, 50) },
                    Minimum = 0
                },
                AxisY2 = {
                    Title = "累计%",
                    TitleForeColor = Color.FromArgb(200, 200, 200),
                    TitleFont = new Font("微软雅黑", 7F),
                    LabelStyle = { ForeColor = Color.FromArgb(180, 180, 180), Font = new Font("微软雅黑", 7F) },
                    LineColor = Color.FromArgb(80, 80, 80),
                    MajorGrid = { Enabled = false },
                    Minimum = 0,
                    Maximum = 100,
                    Enabled = AxisEnabled.True
                }
            };
            chart.ChartAreas.Add(area);

            if (!string.IsNullOrEmpty(defaultTitle))
            {
                chart.Titles.Add(new Title(defaultTitle)
                {
                    ForeColor = Color.FromArgb(200, 200, 200),
                    Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                    Docking = Docking.Top
                });
            }
        }

        public void ShowLot(string lotNumber, List<DefectReviewItem> items)
        {
            _currentLot = lotNumber;
            _currentItems = items;
            RefreshAllCharts();
        }

        public void Clear(string hint = "无数据，请先执行查询")
        {
            _currentLot = null;
            _currentItems = null;
            ClearAllCharts(hint);
        }

        private void ClearAllCharts(string message)
        {
            ClearSingleChart(chart_TopLeft);
            ClearSingleChart(chart_TopRight);
            ClearSingleChart(chart_BottomLeft);
            ClearSingleChart(chart_BottomRight);
            label_Status.Text = message ?? string.Empty;
        }

        private void ClearSingleChart(Chart chart)
        {
            chart.Series.Clear();
        }

        private string GetDefectNameKey(DetectInfo hp)
        {
            if (chk_OriginName.Checked)
            {
                return string.IsNullOrEmpty(hp.OriginDefectName) ? "(未命名)" : hp.OriginDefectName;
            }
            return string.IsNullOrEmpty(hp.DefectName) ? "(未命名)" : hp.DefectName;
        }

        private SourceDef GetSourceByName(string name)
        {
            switch (name)
            {
                case "VVS": return SrcVVS;
                case "VRS": return SrcVRS;
                default: return SrcAI;
            }
        }

        private void RefreshAllCharts()
        {
            try
            {
                string lot = _currentLot;
                var items = _currentItems;
                if (string.IsNullOrEmpty(lot) || items == null)
                {
                    ClearAllCharts("请选择 Lot");
                    return;
                }

                var validItems = items
                    .Where(it => it != null && it.HeatPoints != null && it.OriginalAiState != 0)
                    .ToList();

                if (validItems.Count == 0)
                {
                    ClearAllCharts($"Lot: {lot} 无有效AI结果数据");
                    return;
                }

                var allPoints = validItems.SelectMany(it => it.HeatPoints).Where(hp => hp != null).ToList();

                var srcTopLeft = GetSourceByName(cmb_TopLeft.SelectedItem?.ToString());
                var srcTopRight = GetSourceByName(cmb_TopRight.SelectedItem?.ToString());
                bool useVVS = rdo_VVS.Checked;
                var srcBottom = useVVS ? SrcVVS : SrcVRS;
                string bottomLabel = useVVS ? "VVS" : "VRS";

                RefreshSingleSourceChart(chart_TopLeft, allPoints, srcTopLeft,
                    $"{srcTopLeft.Name} 缺陷分布");

                RefreshSingleSourceChart(chart_TopRight, allPoints, srcTopRight,
                    $"{srcTopRight.Name} 缺陷分布");

                var aiOkPoints = allPoints.Where(hp => hp.AIStatus == 1).ToList();
                RefreshFilteredChart(chart_BottomLeft, aiOkPoints, srcBottom,
                    $"AI OK & {bottomLabel} NG 缺陷分布");

                var aiNgPoints = allPoints.Where(hp => hp.AIStatus == 2).ToList();
                RefreshFilteredOkChart(chart_BottomRight, aiNgPoints, srcBottom,
                    $"AI NG & {bottomLabel} OK 缺陷分布");

                var statusParts = new List<string> { $"Lot: {lot}" };
                statusParts.Add($"总点数: {allPoints.Count}");
                statusParts.Add($"AI OK: {aiOkPoints.Count}");
                statusParts.Add($"AI NG: {aiNgPoints.Count}");
                label_Status.Text = string.Join("  |  ", statusParts);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"刷新帕累托图异常: {ex}");
                ClearAllCharts("刷新失败，请查看日志");
            }
        }

        private void RefreshSingleSourceChart(Chart chart, List<DetectInfo> allPoints, SourceDef src, string title)
        {
            chart.Series.Clear();
            UpdateChartTitle(chart, title);

            var points = allPoints.Where(hp => src.HasData(hp)).ToList();
            if (points.Count == 0)
            {
                UpdateChartTitle(chart, title + " (无数据)");
                return;
            }

            var grouped = points
                .GroupBy(hp => GetDefectNameKey(hp))
                .OrderByDescending(g => g.Count())
                .ToList();

            FillChart(chart, grouped, points.Count, src.BarColor, src.LineColor, src.Name);
        }

        private void RefreshFilteredChart(Chart chart, List<DetectInfo> preFiltered, SourceDef src, string title)
        {
            chart.Series.Clear();
            UpdateChartTitle(chart, title);

            var ngPoints = preFiltered.Where(hp => src.HasData(hp) && src.IsNg(hp)).ToList();
            if (ngPoints.Count == 0)
            {
                UpdateChartTitle(chart, title + " (无数据)");
                return;
            }

            var grouped = ngPoints
                .GroupBy(hp => GetDefectNameKey(hp))
                .OrderByDescending(g => g.Count())
                .ToList();

            FillChart(chart, grouped, ngPoints.Count, src.BarColor, src.LineColor, src.Name);
        }

        private void RefreshFilteredOkChart(Chart chart, List<DetectInfo> preFiltered, SourceDef src, string title)
        {
            chart.Series.Clear();
            UpdateChartTitle(chart, title);

            var okPoints = preFiltered.Where(hp => src.HasData(hp) && !src.IsNg(hp)).ToList();
            if (okPoints.Count == 0)
            {
                UpdateChartTitle(chart, title + " (无数据)");
                return;
            }

            var grouped = okPoints
                .GroupBy(hp => GetDefectNameKey(hp))
                .OrderByDescending(g => g.Count())
                .ToList();

            FillChart(chart, grouped, okPoints.Count, src.BarColor, src.LineColor, src.Name);
        }

        private void UpdateChartTitle(Chart chart, string title)
        {
            chart.Titles.Clear();
            chart.Titles.Add(new Title(title)
            {
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                Docking = Docking.Top
            });
        }

        private void FillChart(Chart chart, List<IGrouping<string, DetectInfo>> grouped, int total,
            Color barColor, Color lineColor, string prefix)
        {
            var barSeries = new Series($"{prefix}_频次")
            {
                ChartType = SeriesChartType.Column,
                Color = barColor,
                BorderColor = Color.FromArgb(
                    Math.Max(0, barColor.R - 30),
                    Math.Max(0, barColor.G - 60),
                    Math.Max(0, barColor.B - 50)),
                IsValueShownAsLabel = true,
                LabelForeColor = Color.White,
                Font = new Font("微软雅黑", 7F),
                ["PixelPointWidth"] = "30"
            };
            chart.Series.Add(barSeries);

            var lineSeries = new Series($"{prefix}_累计%")
            {
                ChartType = SeriesChartType.Line,
                Color = lineColor,
                BorderWidth = 2,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 5,
                MarkerColor = lineColor,
                YAxisType = AxisType.Secondary,
                IsValueShownAsLabel = true,
                LabelForeColor = lineColor,
                Font = new Font("微软雅黑", 7F)
            };
            chart.Series.Add(lineSeries);

            int running = 0;
            foreach (var g in grouped)
            {
                int count = g.Count();
                barSeries.Points.AddXY(g.Key, count);
                running += count;
                double pct = total > 0 ? running * 100.0 / total : 0;
                int idx = lineSeries.Points.AddXY(g.Key, pct);
                lineSeries.Points[idx].Label = pct.ToString("F1") + "%";
            }

            chart.ChartAreas[0].RecalculateAxesScale();
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeepSightDB;
using DeepSightTool;

namespace DeepSightAI
{
    public partial class FrSearch : Form
    {
        #region 字段

        private const int MAX_LOT_COUNT = 100; // 最多加载的Lot数量
        private const int MAX_PANELS_PER_LOT = 100; // 每个Lot最多加载的Panel数量
        private bool _isLoading = false;

        // 当前页Lot汇总数据
        private DataTable _lotSummaryTable;
        // 当前选中Lot的Panel详情
        private DataTable _panelDetailTable;
        // 缓存当前页每个Lot的Panel数据
        private Dictionary<string, List<PanelDataRecord>> _lotDataCache = new Dictionary<string, List<PanelDataRecord>>();

        #endregion

        #region 构造与单例

        public FrSearch()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            InitDataTables();
            ApplyGridStyles();
        }

        private static FrSearch _instance;

        public static FrSearch Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrSearch();
                }
                return _instance;
            }
        }

        #endregion

        #region 初始化

        private void InitDataTables()
        {
            // Lot汇总表
            _lotSummaryTable = new DataTable();
            _lotSummaryTable.Columns.Add("Lot号", typeof(string));
            _lotSummaryTable.Columns.Add("料号", typeof(string));
            _lotSummaryTable.Columns.Add("机台", typeof(string));
            _lotSummaryTable.Columns.Add("板数", typeof(int));
            _lotSummaryTable.Columns.Add("最早检测", typeof(string));
            _lotSummaryTable.Columns.Add("最新检测", typeof(string));
            _lotSummaryTable.Columns.Add("AVI OK率", typeof(string));
            _lotSummaryTable.Columns.Add("AI OK率", typeof(string));
            _lotSummaryTable.Columns.Add("总报点", typeof(int));
            _lotSummaryTable.Columns.Add("AI过滤OK", typeof(int));

            dgv_Lots.DataSource = _lotSummaryTable;

            // Panel详情表
            _panelDetailTable = new DataTable();
            _panelDetailTable.Columns.Add("序列号", typeof(string));
            _panelDetailTable.Columns.Add("料号", typeof(string));
            _panelDetailTable.Columns.Add("机台", typeof(string));
            _panelDetailTable.Columns.Add("检测时间", typeof(string));
            _panelDetailTable.Columns.Add("A面AVI", typeof(string));
            _panelDetailTable.Columns.Add("A面AI", typeof(string));
            _panelDetailTable.Columns.Add("A面报点", typeof(int));
            _panelDetailTable.Columns.Add("B面AVI", typeof(string));
            _panelDetailTable.Columns.Add("B面AI", typeof(string));
            _panelDetailTable.Columns.Add("B面报点", typeof(int));

            dgv_Panels.DataSource = _panelDetailTable;
        }

        private void ApplyGridStyles()
        {
            var headerStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(44, 62, 80),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9.5F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            var cellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(34, 55, 70),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(52, 101, 164),
                SelectionForeColor = Color.White,
                Font = new Font("微软雅黑", 9F),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            dgv_Lots.ColumnHeadersDefaultCellStyle = headerStyle;
            dgv_Lots.DefaultCellStyle = cellStyle;
            dgv_Panels.ColumnHeadersDefaultCellStyle = headerStyle;
            dgv_Panels.DefaultCellStyle = cellStyle;
        }

        private async void FrSearch_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        #endregion

        #region 数据加载

        private async Task LoadDataAsync()
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                SetLoadingState(true, "正在加载...");

                // 获取最近的Lot列表
                var lotNumbers = await Machine.master.GetRecentLotNumbers(1, MAX_LOT_COUNT);

                // 清空缓存和表格
                _lotDataCache.Clear();
                _lotSummaryTable.Rows.Clear();
                _panelDetailTable.Rows.Clear();
                label_DetailTitle.Text = "请选择一个Lot查看详情";

                // 逐个加载Lot数据并填充汇总信息
                SetLoadingState(true, $"正在加载 {lotNumbers.Count} 个Lot的数据...");

                foreach (var lotNumber in lotNumbers)
                {
                    var allPanels = await Machine.master.GetPanelsDataByMachineAndLot(null, lotNumber);
                    var panels = allPanels.Take(MAX_PANELS_PER_LOT).ToList();
                    _lotDataCache[lotNumber] = panels;
                    AddLotSummaryRow(lotNumber, panels, allPanels.Count);
                }

                SetLoadingState(false, "");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"加载生产事件数据失败: {ex}");
                MessageBox.Show("加载数据失败，请检查数据库连接。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetLoadingState(false, "");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void AddLotSummaryRow(string lotNumber, List<PanelDataRecord> panels, int totalCount = -1)
        {
            if (panels == null || panels.Count == 0)
            {
                _lotSummaryTable.Rows.Add(lotNumber, "-", "-", 0, "-", "-", "-", "-", 0, 0);
                return;
            }

            int displayCount = totalCount > 0 ? totalCount : panels.Count;
            var productSerials = panels.Select(p => p.ProductSerial).Where(s => !string.IsNullOrEmpty(s)).Distinct();
            var machineIds = panels.Select(p => p.MachineId).Where(s => !string.IsNullOrEmpty(s)).Distinct();
            var minDate = panels.Min(p => p.DetectionDate);
            var maxDate = panels.Max(p => p.DetectionDate);

            // 统计
            var stat = PanelDataRecord.GetBoardStat(panels);
            string aviOkRate = stat.AviPanelCount > 0
                ? $"{(double)stat.AviPanelOKCount / stat.AviPanelCount * 100:F1}%"
                : "-";
            string aiOkRate = stat.AviPanelCount > 0
                ? $"{(double)stat.AiPanelOKCount / stat.AviPanelCount * 100:F1}%"
                : "-";

            _lotSummaryTable.Rows.Add(
                lotNumber,
                string.Join(",", productSerials),
                string.Join(",", machineIds),
                displayCount,
                minDate.ToString("MM-dd HH:mm"),
                maxDate.ToString("MM-dd HH:mm"),
                aviOkRate,
                aiOkRate,
                stat.AiFilterCount,
                stat.AiFilterOKCount
            );
        }

        #endregion

        #region Lot选择 → 加载Panel详情

        private void dgv_Lots_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv_Lots.SelectedRows.Count == 0) return;

            var row = dgv_Lots.SelectedRows[0];
            var lotNumber = row.Cells["Lot号"]?.Value?.ToString();
            if (string.IsNullOrEmpty(lotNumber)) return;

            LoadPanelDetails(lotNumber);
        }

        private void LoadPanelDetails(string lotNumber)
        {
            _panelDetailTable.Rows.Clear();

            if (!_lotDataCache.TryGetValue(lotNumber, out var panels) || panels.Count == 0)
            {
                label_DetailTitle.Text = $"Lot: {lotNumber} - 无数据";
                return;
            }

            var displayPanels = panels.Take(MAX_PANELS_PER_LOT).ToList();
            string countText = panels.Count > MAX_PANELS_PER_LOT
                ? $"共 {panels.Count} 块板 (显示前{MAX_PANELS_PER_LOT}条)"
                : $"共 {panels.Count} 块板";
            label_DetailTitle.Text = $"Lot: {lotNumber} - {countText}";

            foreach (var panel in displayPanels)
            {
                var sideA = panel.Sides?.FirstOrDefault(s => s.Side == "A");
                var sideB = panel.Sides?.FirstOrDefault(s => s.Side == "B");

                _panelDetailTable.Rows.Add(
                    panel.SerialNumber,
                    panel.ProductSerial ?? "-",
                    panel.MachineId ?? "-",
                    panel.DetectionDate.ToString("MM-dd HH:mm:ss"),
                    GetStateText(sideA?.AviState ?? 0),
                    GetStateText(sideA?.AiState ?? 0),
                    sideA?.DetectPoints?.Count ?? 0,
                    GetStateText(sideB?.AviState ?? 0),
                    GetStateText(sideB?.AiState ?? 0),
                    sideB?.DetectPoints?.Count ?? 0
                );
            }
        }

        private static string GetStateText(int state)
        {
            switch (state)
            {
                case 0: return "未运行";
                case 1: return "OK";
                case 2: return "NG";
                case 3: return "异常";
                default: return state.ToString();
            }
        }

        #endregion

        private async void btn_Refresh_Click(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        #region UI辅助

        private void SetLoadingState(bool loading, string message)
        {
            label_Loading.Text = message;
            label_Loading.Visible = loading;
            btn_Refresh.Enabled = !loading;
        }

        #endregion
    }
}


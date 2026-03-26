﻿using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DeepSightModel;
using DeepSightTool;

namespace DeepSightAI
{
    /// <summary>
    /// 最近生产信息查看器 - 上方Lot列表，下方选中Lot的SN列表（含判断过程）
    /// </summary>
    public partial class FrSearch : Form
    {
        #region 字段

        private DataTable _lotTable;      // 上方：Lot汇总
        private DataTable _snTable;       // 下方：SN详情
        private SnDebugInfo[] _currentSnItems; // 当前选中Lot下的SN快照

        #endregion

        #region 构造与单例

        public FrSearch()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);

            InitTables();
            ApplyGridStyles();
            InitContextMenu();
        }

        private static FrSearch _instance;

        public static FrSearch Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FrSearch();
                return _instance;
            }
        }

        #endregion

        #region 初始化

        private void InitTables()
        {
            // Lot汇总表
            _lotTable = new DataTable();
            _lotTable.Columns.Add("Lot号", typeof(string));
            _lotTable.Columns.Add("料号", typeof(string));
            _lotTable.Columns.Add("板数", typeof(int));
            _lotTable.Columns.Add("最新时间", typeof(string));

            dgv_Lots.DataSource = _lotTable;

            // SN详情表
            _snTable = new DataTable();
            _snTable.Columns.Add("SN", typeof(string));
            _snTable.Columns.Add("面别", typeof(string));
            _snTable.Columns.Add("料号", typeof(string));
            _snTable.Columns.Add("缺陷数", typeof(int));
            _snTable.Columns.Add("PCS数", typeof(int));
            _snTable.Columns.Add("图片数", typeof(int));
            _snTable.Columns.Add("判断过程", typeof(string));
            _snTable.Columns.Add("数据源", typeof(string));
            _snTable.Columns.Add("时间", typeof(string));

            dgv_Panels.DataSource = _snTable;
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

            foreach (var dgv in new[] { dgv_Lots, dgv_Panels })
            {
                dgv.ColumnHeadersDefaultCellStyle = headerStyle;
                dgv.DefaultCellStyle = cellStyle;
            }
        }

        private void InitContextMenu()
        {
            // SN表右键菜单
            var contextMenu = new ContextMenuStrip();
            var menuItem = new ToolStripMenuItem("查看调试详情 (JSON)");
            menuItem.Click += ShowDebugInfo_Click;
            contextMenu.Items.Add(menuItem);
            dgv_Panels.ContextMenuStrip = contextMenu;

            // 右键时自动选中行
            dgv_Panels.CellMouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
                {
                    dgv_Panels.ClearSelection();
                    dgv_Panels.Rows[e.RowIndex].Selected = true;
                    dgv_Panels.CurrentCell = dgv_Panels.Rows[e.RowIndex].Cells[0];
                }
            };
        }

        private void FrSearch_Load(object sender, EventArgs e)
        {
            RefreshData();
        }

        #endregion

        #region 数据刷新

        private void RefreshData()
        {
            try
            {
                // 刷新Lot汇总
                var lotSummary = SnDebugInfoCache.GetLotSummary();
                _lotTable.Rows.Clear();

                foreach (var lot in lotSummary)
                {
                    _lotTable.Rows.Add(
                        lot.LotNumber,
                        lot.ProductSerial ?? "-",
                        lot.Count,
                        lot.LatestTime.ToString("HH:mm:ss")
                    );
                }

                int totalSnCount = SnDebugInfoCache.GetAll().Length;
                label_Loading.Text = $"共 {lotSummary.Count} 个Lot，{totalSnCount} 条SN记录";
                label_Loading.Visible = true;

                // 自动选中第一行
                if (dgv_Lots.Rows.Count > 0)
                {
                    dgv_Lots.ClearSelection();
                    dgv_Lots.Rows[0].Selected = true;
                }
                else
                {
                    _snTable.Rows.Clear();
                    _currentSnItems = null;
                    label_DetailTitle.Text = "暂无数据";
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"刷新最近生产信息异常: {ex}");
            }
        }

        #endregion

        #region 选中Lot → 加载SN列表

        private void dgv_Lots_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv_Lots.SelectedRows.Count == 0) return;

            var row = dgv_Lots.SelectedRows[0];
            string lotNumber = row.Cells["Lot号"].Value?.ToString();
            if (string.IsNullOrEmpty(lotNumber)) return;

            LoadSnForLot(lotNumber);
        }

        private void LoadSnForLot(string lotNumber)
        {
            _currentSnItems = SnDebugInfoCache.GetByLot(lotNumber);
            _snTable.Rows.Clear();

            foreach (var info in _currentSnItems)
            {
                _snTable.Rows.Add(
                    info.SerialNumber,
                    info.Side,
                    info.ProductSerial ?? "-",
                    info.DefectCount,
                    info.PcsCount,
                    info.ImageCount,
                    info.JudgmentSummary ?? "-",
                    info.SourceDbName ?? "-",
                    info.CreateTime.ToString("HH:mm:ss.fff")
                );
            }

            // 行颜色标注
            for (int i = 0; i < dgv_Panels.Rows.Count && i < _currentSnItems.Length; i++)
            {
                var dgvRow = dgv_Panels.Rows[i];
                var info = _currentSnItems[i];
                if (info.HasError)
                    dgvRow.DefaultCellStyle.ForeColor = Color.FromArgb(255, 100, 100);
                else if (info.IsByPass)
                    dgvRow.DefaultCellStyle.ForeColor = Color.Yellow;
                else if (info.InferenceReturnJson != null)
                    dgvRow.DefaultCellStyle.ForeColor = Color.FromArgb(100, 255, 100);
            }

            label_DetailTitle.Text = $"Lot: {lotNumber} — 共 {_currentSnItems.Length} 条SN";
        }

        #endregion

        #region 右键 → 打开调试窗口

        private void ShowDebugInfo_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv_Panels.CurrentRow == null || _currentSnItems == null) return;

                int idx = dgv_Panels.CurrentRow.Index;
                if (idx < 0 || idx >= _currentSnItems.Length) return;

                var info = _currentSnItems[idx];
                var form = new FrSnDebugInfo(info.SerialNumber, info);
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"打开SN调试详情异常: {ex}");
            }
        }

        #endregion

        private void btn_Refresh_Click(object sender, EventArgs e)
        {
            RefreshData();
        }
    }
}


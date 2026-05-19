using DeepSightModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// 缺陷列表面板：包含缺陷数据表格（DGV）和操作工具栏（SN搜索、操作按钮）。
    /// 负责管理表格数据绑定、SN搜索过滤及行选择导航。
    /// </summary>
    public partial class UcDefectListPanel : UserControl
    {
        #region Fields

        // 当前 Lot 的全量数据（用于 SN 搜索时恢复）
        private List<DefectReviewItem> _sourceItems = new List<DefectReviewItem>();
        private SortableBindingList<DefectReviewItem> _bindingList;
        private string _dgvPlaceholderText = "请先执行查询";

        #endregion

        #region Events

        /// <summary>双击某行时触发（用于跳转到缺陷详情页）</summary>
        public event Action<DefectReviewItem> ItemDoubleClicked;

        /// <summary>点击"模型一致性测试"菜单项时触发</summary>
        public event EventHandler RunTestClicked;

        /// <summary>点击"运行二次推理"菜单项时触发</summary>
        public event EventHandler SecondaryInferenceClicked;

        /// <summary>点击"添加到数据集"菜单项时触发</summary>
        public event EventHandler AddToDatasetClicked;

        /// <summary>点击"图片详情"按钮时触发</summary>
        public event EventHandler ImageDetailClicked;

        #endregion

        #region Constructor

        public UcDefectListPanel()
        {
            InitializeComponent();
            _bindingList = new SortableBindingList<DefectReviewItem>(_sourceItems);
            dataGridView_Defects.DataSource = _bindingList;
            dataGridView_Defects.ApplyDarkTheme();

            // DGV 事件
            dataGridView_Defects.CellDoubleClick += OnCellDoubleClick;
            dataGridView_Defects.CellValueChanged += OnCellValueChanged;
            dataGridView_Defects.Paint += OnDgvPaint;

            // 工具栏按钮事件
            btn_SnSearch.Click += (s, e) => FilterBySn();
            txt_SnFilter.KeyDown += OnSnFilterKeyDown;

            // 图片详情按钮
            btn_ImageDetail.Click += (s, e) => ImageDetailClicked?.Invoke(this, EventArgs.Empty);

            // 更多操作下拉菜单
            btn_MoreActions.Click += (s, e) =>
            {
                contextMenu_Actions.Show(btn_MoreActions, 0, btn_MoreActions.Height);
            };
            menuItem_RunTest.Click += (s, e) => RunTestClicked?.Invoke(this, EventArgs.Empty);
            menuItem_SecondaryInference.Click += (s, e) => SecondaryInferenceClicked?.Invoke(this, EventArgs.Empty);
            menuItem_AddToDataset.Click += (s, e) => AddToDatasetClicked?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Public API

        /// <summary>当前 DGV 选中行的 DefectReviewItem（无选中时为 null）</summary>
        public DefectReviewItem CurrentItem
        {
            get
            {
                var cell = dataGridView_Defects.CurrentCell;
                if (cell == null || cell.RowIndex < 0) return null;
                return dataGridView_Defects.Rows[cell.RowIndex].DataBoundItem as DefectReviewItem;
            }
        }

        /// <summary>
        /// 加载指定列表到 DGV 并刷新显示。
        /// </summary>
        /// <param name="items">要显示的数据，传 null 表示清空</param>
        /// <param name="placeholderText">DGV 无数据时显示的占位提示，null 表示保持现有提示</param>
        public void LoadItems(List<DefectReviewItem> items, string placeholderText = null)
        {
            _sourceItems.Clear();
            if (items != null)
                _sourceItems.AddRange(items);
            if (placeholderText != null)
                _dgvPlaceholderText = placeholderText;
            RefreshDisplay(_sourceItems);
        }

        /// <summary>清空 DGV，并可选设置占位提示文本</summary>
        public void ClearItems(string placeholderText = "请先执行查询")
        {
            LoadItems(null, placeholderText);
        }

        /// <summary>强制刷新 DGV 数据绑定（外部修改了 Item 属性后调用）</summary>
        public void ResetBindings() => _bindingList?.ResetBindings();

        /// <summary>
        /// 将 DGV 选中行移至下一行，并返回新选中的 DefectReviewItem；
        /// 到末尾时循环回首行。无数据时返回 null。
        /// </summary>
        public DefectReviewItem SelectNextRow()
        {
            if (dataGridView_Defects.Rows.Count == 0) return null;
            int cur = dataGridView_Defects.CurrentCell?.RowIndex ?? -1;
            int next = (cur + 1) % dataGridView_Defects.Rows.Count;
            dataGridView_Defects.ClearSelection();
            dataGridView_Defects.Rows[next].Selected = true;
            dataGridView_Defects.SafeSetCurrentCell(dataGridView_Defects.Rows[next].Cells[0]);
            return dataGridView_Defects.Rows[next].DataBoundItem as DefectReviewItem;
        }

        #endregion

        #region Private Methods

        private void RefreshDisplay(List<DefectReviewItem> items)
        {
            _bindingList = new SortableBindingList<DefectReviewItem>(items);
            dataGridView_Defects.DataSource = _bindingList;
            dataGridView_Defects.Invalidate();
        }

        private void FilterBySn()
        {
            string filter = txt_SnFilter.Text.Trim();
            if (string.IsNullOrEmpty(filter))
            {
                // 清空搜索时恢复当前 Lot 的全量数据
                RefreshDisplay(_sourceItems);
                return;
            }

            var filtered = _sourceItems
                .Where(x => x.SerialNumber != null &&
                            x.SerialNumber.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            RefreshDisplay(filtered);

            if (filtered.Count == 1)
            {
                dataGridView_Defects.ClearSelection();
                dataGridView_Defects.Rows[0].Selected = true;
                dataGridView_Defects.SafeSetCurrentCell(dataGridView_Defects.Rows[0].Cells[0]);
            }
            else if (filtered.Count == 0)
            {
                MessageBox.Show($"未找到包含 \"{filter}\" 的序列号。",
                    "搜索结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var item = dataGridView_Defects.Rows[e.RowIndex].DataBoundItem as DefectReviewItem;
            if (item != null) ItemDoubleClicked?.Invoke(item);
        }

        private void OnCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != col_ManualStatus.Index) return;
            var item = dataGridView_Defects.Rows[e.RowIndex].DataBoundItem as DefectReviewItem;
            if (item != null)
                item.IsModified = true;
        }

        private void OnDgvPaint(object sender, PaintEventArgs e)
        {
            if (dataGridView_Defects.Rows.Count > 0 || string.IsNullOrEmpty(_dgvPlaceholderText)) return;
            using (var brush = new SolidBrush(Color.FromArgb(160, 200, 200, 200)))
            using (var font = new Font("微软雅黑", 14F, FontStyle.Bold))
            {
                var size = e.Graphics.MeasureString(_dgvPlaceholderText, font);
                var rect = dataGridView_Defects.ClientRectangle;
                e.Graphics.DrawString(_dgvPlaceholderText, font, brush,
                    (rect.Width - size.Width) / 2f, (rect.Height - size.Height) / 2f);
            }
        }

        private void OnSnFilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            FilterBySn();
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        #endregion
    }
}

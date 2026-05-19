using System.Drawing;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// DataGridView 深色主题样式统一入口
    /// 通过扩展方法一次性应用主题色、字体、行高等配置
    /// </summary>
    public static class DataGridViewStyleHelper
    {
        #region 主题色值

        public static readonly Color HeaderBack = Color.FromArgb(0, 64, 82);
        public static readonly Color HeaderFore = Color.White;

        // 与项目主窗体 BackColor (29,48,60) 保持一致
        public static readonly Color CellBack = Color.FromArgb(29, 48, 60);
        public static readonly Color CellAltBack = Color.FromArgb(35, 55, 70);
        public static readonly Color CellFore = Color.White;

        public static readonly Color GridLine = Color.FromArgb(60, 80, 95);

        public static readonly Color SelectionBack = Color.FromArgb(0, 122, 204);
        public static readonly Color SelectionFore = Color.White;

        private static readonly Font HeaderFont = new Font("微软雅黑", 9F, FontStyle.Bold);
        private static readonly Font CellFont = new Font("微软雅黑", 9F);

        #endregion

        /// <summary>
        /// 应用统一的深色主题样式
        /// </summary>
        /// <param name="dgv">目标 DataGridView（兼容 Sunny.UI.UIDataGridView）</param>
        /// <param name="rowHeight">行高，默认 32</param>
        /// <param name="highlightSelection">是否在选中行显示蓝色高亮；编辑型表格建议传 false</param>
        public static void ApplyDarkTheme(this DataGridView dgv, int rowHeight = 32, bool highlightSelection = true)
        {
            if (dgv == null) return;

            dgv.EnableHeadersVisualStyles = false;
            dgv.BorderStyle = BorderStyle.None;
            dgv.BackgroundColor = CellBack;
            dgv.GridColor = GridLine;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = HeaderBack;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = HeaderFore;
            dgv.ColumnHeadersDefaultCellStyle.Font = HeaderFont;

            var selBack = highlightSelection ? SelectionBack : CellBack;
            var selAltBack = highlightSelection ? SelectionBack : CellAltBack;

            dgv.DefaultCellStyle.BackColor = CellBack;
            dgv.DefaultCellStyle.ForeColor = CellFore;
            dgv.DefaultCellStyle.SelectionBackColor = selBack;
            dgv.DefaultCellStyle.SelectionForeColor = SelectionFore;
            dgv.DefaultCellStyle.Font = CellFont;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = CellAltBack;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = CellFore;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = selAltBack;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = SelectionFore;

            dgv.RowTemplate.Height = rowHeight;

            // Sunny.UI.UIDataGridView 额外的条纹色属性
            if (dgv is Sunny.UI.UIDataGridView uidgv)
            {
                uidgv.StripeEvenColor = CellBack;
                uidgv.StripeOddColor = CellAltBack;
            }
        }

        /// <summary>
        /// 安全设置 CurrentCell，避免"不能将当前单元格设置为不可见的单元格"异常。
        /// 在设置前检查目标单元格、其所在行和列是否均可见。
        /// </summary>
        /// <param name="dgv">目标 DataGridView</param>
        /// <param name="cell">要设置为当前单元格的单元格</param>
        /// <returns>是否成功设置</returns>
        public static bool SafeSetCurrentCell(this DataGridView dgv, DataGridViewCell cell)
        {
            if (cell?.OwningRow != null && cell.OwningRow.Visible &&
                cell.OwningColumn != null && cell.OwningColumn.Visible &&
                cell.Visible)
            {
                dgv.CurrentCell = cell;
                return true;
            }
            return false;
        }
    }
}
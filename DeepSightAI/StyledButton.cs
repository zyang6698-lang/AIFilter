using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// 统一样式的自定义按钮控件
    /// </summary>
    public class StyledButton : Button
    {
        private static readonly Color NormalBackColor = Color.FromArgb(0, 64, 82);
        private static readonly Color HoverBackColor = Color.FromArgb(0, 86, 110);
        private static readonly Color PressedBackColor = Color.FromArgb(0, 50, 64);
        private static readonly Color NormalForeColor = Color.FromArgb(216, 219, 188);

        public StyledButton()
        {
            ApplyStyle();
        }

        private void ApplyStyle()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.FlatAppearance.MouseOverBackColor = HoverBackColor;
            this.FlatAppearance.MouseDownBackColor = PressedBackColor;
            this.BackColor = NormalBackColor;
            this.ForeColor = NormalForeColor;
            this.Font = new Font("微软雅黑", 9F, FontStyle.Regular);
            this.Cursor = Cursors.Hand;
            this.UseVisualStyleBackColor = false;
        }
    }
}


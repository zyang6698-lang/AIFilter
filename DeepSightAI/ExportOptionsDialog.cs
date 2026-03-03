using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeepSightAI
{
    public class ExportOptionsDialog : Form
    {
        private CheckBox chkOriginalImage;
        private CheckBox chkTemplateImage;
        private Button btnOk;
        private Button btnCancel;
        private Label lblHint;

        /// <summary>
        /// 是否导出原图
        /// </summary>
        public bool ExportOriginalImage => chkOriginalImage.Checked;

        /// <summary>
        /// 是否导出模板图
        /// </summary>
        public bool ExportTemplateImage => chkTemplateImage.Checked;

        public ExportOptionsDialog()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "导出选项";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(300, 180);

            var lblTitle = new Label
            {
                Text = "请选择要导出的图片类型（至少选择一项）：",
                Location = new Point(20, 20),
                AutoSize = true
            };

            chkOriginalImage = new CheckBox
            {
                Text = "原图",
                Location = new Point(40, 55),
                AutoSize = true,
                Checked = true
            };

            chkTemplateImage = new CheckBox
            {
                Text = "模板图",
                Location = new Point(40, 85),
                AutoSize = true,
                Checked = true
            };

            lblHint = new Label
            {
                Text = "",
                Location = new Point(20, 115),
                AutoSize = true,
                ForeColor = Color.Red
            };

            btnOk = new Button
            {
                Text = "确定",
                Size = new Size(80, 30),
                Location = new Point(110, 140)
            };
            btnOk.Click += BtnOk_Click;

            btnCancel = new Button
            {
                Text = "取消",
                Size = new Size(80, 30),
                Location = new Point(200, 140),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(chkOriginalImage);
            this.Controls.Add(chkTemplateImage);
            this.Controls.Add(lblHint);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (!chkOriginalImage.Checked && !chkTemplateImage.Checked)
            {
                lblHint.Text = "请至少选择一项！";
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}


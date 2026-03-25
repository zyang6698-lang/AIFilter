using System;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class ExportOptionsDialog : Form
    {
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
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (!chkOriginalImage.Checked && !chkTemplateImage.Checked)
            {
                lblHint.Text = "请至少选择一项！";
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}


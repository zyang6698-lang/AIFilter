using System;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class DlgExportOptions : Form
    {
        /// <summary>
        /// 是否导出原图
        /// </summary>
        public bool ExportOriginalImage => chkOriginalImage.Checked;

        /// <summary>
        /// 是否导出模板图
        /// </summary>
        public bool ExportTemplateImage => chkTemplateImage.Checked;

        /// <summary>
        /// 是否导出 Gerber 图
        /// </summary>
        public bool ExportGerberImage => chkGerberImage.Checked;

        public DlgExportOptions()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}


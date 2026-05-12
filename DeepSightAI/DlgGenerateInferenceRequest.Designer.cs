namespace DeepSightAI
{
    partial class DlgGenerateInferenceRequest
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabMain = new Sunny.UI.UITabControl();
            this.tabPageByMinio = new System.Windows.Forms.TabPage();
            this.lblMinioStatus = new Sunny.UI.UILabel();
            this.progressBarMinio = new System.Windows.Forms.ProgressBar();
            this.btnRunByMinio = new Sunny.UI.UISymbolButton();
            this.lblMinioDesc = new Sunny.UI.UILabel();
            this.tabPageByLot = new System.Windows.Forms.TabPage();
            this.lblLotStatus = new Sunny.UI.UILabel();
            this.progressBarLot = new System.Windows.Forms.ProgressBar();
            this.btnRunByLot = new Sunny.UI.UISymbolButton();
            this.btnLoadLots = new Sunny.UI.UISymbolButton();
            this.cmbDb = new Sunny.UI.UIComboBox();
            this.cmbLot = new Sunny.UI.UIComboBox();
            this.lblDb = new Sunny.UI.UILabel();
            this.lblLot = new Sunny.UI.UILabel();
            this.lblLotDesc = new Sunny.UI.UILabel();
            this.btnClose = new Sunny.UI.UISymbolButton();
            this.tabMain.SuspendLayout();
            this.tabPageByMinio.SuspendLayout();
            this.tabPageByLot.SuspendLayout();
            this.SuspendLayout();
            //
            // tabMain
            //
            this.tabMain.Controls.Add(this.tabPageByMinio);
            this.tabMain.Controls.Add(this.tabPageByLot);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabMain.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tabMain.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabMain.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.tabMain.ItemSize = new System.Drawing.Size(140, 32);
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.MainPage = "";
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(620, 380);
            this.tabMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabMain.Style = Sunny.UI.UIStyle.Custom;
            this.tabMain.TabBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            this.tabMain.TabIndex = 0;
            this.tabMain.TabSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.tabMain.TabSelectedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.tabMain.TabSelectedHighColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            //
            // tabPageByMinio
            //
            this.tabPageByMinio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tabPageByMinio.Controls.Add(this.lblMinioStatus);
            this.tabPageByMinio.Controls.Add(this.progressBarMinio);
            this.tabPageByMinio.Controls.Add(this.btnRunByMinio);
            this.tabPageByMinio.Controls.Add(this.lblMinioDesc);
            this.tabPageByMinio.Location = new System.Drawing.Point(0, 36);
            this.tabPageByMinio.Name = "tabPageByMinio";
            this.tabPageByMinio.Padding = new System.Windows.Forms.Padding(16);
            this.tabPageByMinio.Size = new System.Drawing.Size(620, 344);
            this.tabPageByMinio.TabIndex = 0;
            this.tabPageByMinio.Text = "按 MinIO 目录";
            //
            // lblMinioDesc
            //
            this.lblMinioDesc.BackColor = System.Drawing.Color.Transparent;
            this.lblMinioDesc.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMinioDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblMinioDesc.Location = new System.Drawing.Point(20, 20);
            this.lblMinioDesc.Name = "lblMinioDesc";
            this.lblMinioDesc.Size = new System.Drawing.Size(580, 64);
            this.lblMinioDesc.Style = Sunny.UI.UIStyle.Custom;
            this.lblMinioDesc.TabIndex = 0;
            this.lblMinioDesc.Text = "通过浏览 MinIO 目录选择 panel.json 所在前缀，按 SN 分组 A/B 面后向 ai_merged_results 数据库写入推理请求。";
            //
            // btnRunByMinio
            //
            this.btnRunByMinio.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRunByMinio.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnRunByMinio.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(86)))), ((int)(((byte)(110)))));
            this.btnRunByMinio.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnRunByMinio.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRunByMinio.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnRunByMinio.Location = new System.Drawing.Point(20, 110);
            this.btnRunByMinio.Name = "btnRunByMinio";
            this.btnRunByMinio.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.btnRunByMinio.Size = new System.Drawing.Size(220, 36);
            this.btnRunByMinio.Style = Sunny.UI.UIStyle.Custom;
            this.btnRunByMinio.Symbol = 61462;
            this.btnRunByMinio.TabIndex = 1;
            this.btnRunByMinio.Text = "选择 MinIO 目录并生成";
            this.btnRunByMinio.Click += new System.EventHandler(this.btnRunByMinio_Click);
            //
            // progressBarMinio
            //
            this.progressBarMinio.Location = new System.Drawing.Point(20, 170);
            this.progressBarMinio.Name = "progressBarMinio";
            this.progressBarMinio.Size = new System.Drawing.Size(580, 22);
            this.progressBarMinio.TabIndex = 2;
            //
            // lblMinioStatus
            //
            this.lblMinioStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblMinioStatus.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMinioStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblMinioStatus.Location = new System.Drawing.Point(20, 200);
            this.lblMinioStatus.Name = "lblMinioStatus";
            this.lblMinioStatus.Size = new System.Drawing.Size(580, 60);
            this.lblMinioStatus.Style = Sunny.UI.UIStyle.Custom;
            this.lblMinioStatus.TabIndex = 3;
            this.lblMinioStatus.Text = "就绪";

            //
            // tabPageByLot
            //
            this.tabPageByLot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tabPageByLot.Controls.Add(this.lblLotStatus);
            this.tabPageByLot.Controls.Add(this.progressBarLot);
            this.tabPageByLot.Controls.Add(this.btnRunByLot);
            this.tabPageByLot.Controls.Add(this.btnLoadLots);
            this.tabPageByLot.Controls.Add(this.cmbDb);
            this.tabPageByLot.Controls.Add(this.cmbLot);
            this.tabPageByLot.Controls.Add(this.lblDb);
            this.tabPageByLot.Controls.Add(this.lblLot);
            this.tabPageByLot.Controls.Add(this.lblLotDesc);
            this.tabPageByLot.Location = new System.Drawing.Point(0, 36);
            this.tabPageByLot.Name = "tabPageByLot";
            this.tabPageByLot.Padding = new System.Windows.Forms.Padding(16);
            this.tabPageByLot.Size = new System.Drawing.Size(620, 344);
            this.tabPageByLot.TabIndex = 1;
            this.tabPageByLot.Text = "按 Lot 批次";
            //
            // lblLotDesc
            //
            this.lblLotDesc.BackColor = System.Drawing.Color.Transparent;
            this.lblLotDesc.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblLotDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblLotDesc.Location = new System.Drawing.Point(20, 16);
            this.lblLotDesc.Name = "lblLotDesc";
            this.lblLotDesc.Size = new System.Drawing.Size(580, 44);
            this.lblLotDesc.Style = Sunny.UI.UIStyle.Custom;
            this.lblLotDesc.TabIndex = 0;
            this.lblLotDesc.Text = "选择源 LevelDB 后点击\"加载 Lot 列表\"，再从下拉框选定 Lot，最后点击\"按选定 Lot 生成\"。";
            //
            // lblDb
            //
            this.lblDb.BackColor = System.Drawing.Color.Transparent;
            this.lblDb.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblDb.Location = new System.Drawing.Point(20, 78);
            this.lblDb.Name = "lblDb";
            this.lblDb.Size = new System.Drawing.Size(96, 32);
            this.lblDb.Style = Sunny.UI.UIStyle.Custom;
            this.lblDb.TabIndex = 1;
            this.lblDb.Text = "源 LevelDB:";
            this.lblDb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // cmbDb
            //
            this.cmbDb.DataSource = null;
            this.cmbDb.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbDb.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(19)))), ((int)(((byte)(31)))));
            this.cmbDb.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cmbDb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.cmbDb.ItemHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.cmbDb.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.cmbDb.Location = new System.Drawing.Point(120, 78);
            this.cmbDb.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbDb.Name = "cmbDb";
            this.cmbDb.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cmbDb.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.cmbDb.Size = new System.Drawing.Size(340, 32);
            this.cmbDb.Style = Sunny.UI.UIStyle.Custom;
            this.cmbDb.SymbolSize = 22;
            this.cmbDb.TabIndex = 2;
            this.cmbDb.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmbDb.Watermark = "";
            //
            // btnLoadLots
            //
            this.btnLoadLots.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLoadLots.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnLoadLots.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(86)))), ((int)(((byte)(110)))));
            this.btnLoadLots.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnLoadLots.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnLoadLots.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnLoadLots.Location = new System.Drawing.Point(470, 78);
            this.btnLoadLots.Name = "btnLoadLots";
            this.btnLoadLots.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.btnLoadLots.Size = new System.Drawing.Size(130, 32);
            this.btnLoadLots.Style = Sunny.UI.UIStyle.Custom;
            this.btnLoadLots.Symbol = 61473;
            this.btnLoadLots.TabIndex = 3;
            this.btnLoadLots.Text = "加载 Lot 列表";
            this.btnLoadLots.Click += new System.EventHandler(this.btnLoadLots_Click);
            //
            // lblLot
            //
            this.lblLot.BackColor = System.Drawing.Color.Transparent;
            this.lblLot.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblLot.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblLot.Location = new System.Drawing.Point(20, 126);
            this.lblLot.Name = "lblLot";
            this.lblLot.Size = new System.Drawing.Size(96, 32);
            this.lblLot.Style = Sunny.UI.UIStyle.Custom;
            this.lblLot.TabIndex = 4;
            this.lblLot.Text = "Lot:";
            this.lblLot.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // cmbLot
            //
            this.cmbLot.DataSource = null;
            this.cmbLot.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbLot.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(19)))), ((int)(((byte)(31)))));
            this.cmbLot.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cmbLot.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.cmbLot.ItemHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.cmbLot.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.cmbLot.Location = new System.Drawing.Point(120, 126);
            this.cmbLot.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbLot.Name = "cmbLot";
            this.cmbLot.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cmbLot.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.cmbLot.Size = new System.Drawing.Size(480, 32);
            this.cmbLot.Style = Sunny.UI.UIStyle.Custom;
            this.cmbLot.SymbolSize = 22;
            this.cmbLot.TabIndex = 5;
            this.cmbLot.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmbLot.Watermark = "";

            //
            // btnRunByLot
            //
            this.btnRunByLot.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRunByLot.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnRunByLot.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(86)))), ((int)(((byte)(110)))));
            this.btnRunByLot.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnRunByLot.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRunByLot.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnRunByLot.Location = new System.Drawing.Point(20, 176);
            this.btnRunByLot.Name = "btnRunByLot";
            this.btnRunByLot.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.btnRunByLot.Size = new System.Drawing.Size(220, 36);
            this.btnRunByLot.Style = Sunny.UI.UIStyle.Custom;
            this.btnRunByLot.Symbol = 61462;
            this.btnRunByLot.TabIndex = 6;
            this.btnRunByLot.Text = "按选定 Lot 生成";
            this.btnRunByLot.Click += new System.EventHandler(this.btnRunByLot_Click);
            //
            // progressBarLot
            //
            this.progressBarLot.Location = new System.Drawing.Point(20, 230);
            this.progressBarLot.Name = "progressBarLot";
            this.progressBarLot.Size = new System.Drawing.Size(580, 22);
            this.progressBarLot.TabIndex = 7;
            //
            // lblLotStatus
            //
            this.lblLotStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblLotStatus.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblLotStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblLotStatus.Location = new System.Drawing.Point(20, 262);
            this.lblLotStatus.Name = "lblLotStatus";
            this.lblLotStatus.Size = new System.Drawing.Size(580, 60);
            this.lblLotStatus.Style = Sunny.UI.UIStyle.Custom;
            this.lblLotStatus.TabIndex = 8;
            this.lblLotStatus.Text = "就绪";
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnClose.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(86)))), ((int)(((byte)(110)))));
            this.btnClose.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnClose.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnClose.Location = new System.Drawing.Point(518, 392);
            this.btnClose.Name = "btnClose";
            this.btnClose.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.btnClose.Size = new System.Drawing.Size(90, 32);
            this.btnClose.Style = Sunny.UI.UIStyle.Custom;
            this.btnClose.Symbol = 61453;
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "关闭";
            //
            // DlgGenerateInferenceRequest
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(620, 436);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.tabMain);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DlgGenerateInferenceRequest";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "生成推理请求";
            this.tabMain.ResumeLayout(false);
            this.tabPageByMinio.ResumeLayout(false);
            this.tabPageByLot.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITabControl tabMain;
        private System.Windows.Forms.TabPage tabPageByMinio;
        private Sunny.UI.UILabel lblMinioDesc;
        private Sunny.UI.UISymbolButton btnRunByMinio;
        private System.Windows.Forms.ProgressBar progressBarMinio;
        private Sunny.UI.UILabel lblMinioStatus;
        private System.Windows.Forms.TabPage tabPageByLot;
        private Sunny.UI.UILabel lblLotDesc;
        private Sunny.UI.UILabel lblDb;
        private Sunny.UI.UIComboBox cmbDb;
        private Sunny.UI.UISymbolButton btnLoadLots;
        private Sunny.UI.UILabel lblLot;
        private Sunny.UI.UIComboBox cmbLot;
        private Sunny.UI.UISymbolButton btnRunByLot;
        private System.Windows.Forms.ProgressBar progressBarLot;
        private Sunny.UI.UILabel lblLotStatus;
        private Sunny.UI.UISymbolButton btnClose;
    }
}

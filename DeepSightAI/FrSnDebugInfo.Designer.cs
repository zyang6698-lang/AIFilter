namespace DeepSightAI
{
    partial class FrSnDebugInfo
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabLevelDb = new System.Windows.Forms.TabPage();
            this.txtLevelDbJson = new System.Windows.Forms.RichTextBox();
            this.tabPanelInfo = new System.Windows.Forms.TabPage();
            this.txtPanelInfoJson = new System.Windows.Forms.RichTextBox();
            this.tabVbJson = new System.Windows.Forms.TabPage();
            this.txtVbInferenceJson = new System.Windows.Forms.RichTextBox();
            this.tabInferReturn = new System.Windows.Forms.TabPage();
            this.txtInferenceReturnJson = new System.Windows.Forms.RichTextBox();
            this.tabOther = new System.Windows.Forms.TabPage();
            this.txtOtherInfo = new System.Windows.Forms.RichTextBox();
            this.btnPanel = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.searchPanel = new System.Windows.Forms.Panel();
            this.btnCloseSearch = new System.Windows.Forms.Button();
            this.btnFind = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.tabLevelDb.SuspendLayout();
            this.tabPanelInfo.SuspendLayout();
            this.tabVbJson.SuspendLayout();
            this.tabInferReturn.SuspendLayout();
            this.tabOther.SuspendLayout();
            this.btnPanel.SuspendLayout();
            this.searchPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // tabControl
            //
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1184, 815);
            this.tabControl.TabIndex = 0;
            this.tabControl.TabPages.AddRange(new System.Windows.Forms.TabPage[] {
                this.tabLevelDb,
                this.tabPanelInfo,
                this.tabVbJson,
                this.tabInferReturn,
                this.tabOther});
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
            //
            // tabLevelDb
            //
            this.tabLevelDb.Controls.Add(this.txtLevelDbJson);
            this.tabLevelDb.Location = new System.Drawing.Point(4, 34);
            this.tabLevelDb.Name = "tabLevelDb";
            this.tabLevelDb.Size = new System.Drawing.Size(1176, 777);
            this.tabLevelDb.TabIndex = 0;
            this.tabLevelDb.Text = "LevelDB推理请求";
            //
            // txtLevelDbJson
            //
            this.txtLevelDbJson.BackColor = System.Drawing.Color.FromArgb(20, 35, 45);
            this.txtLevelDbJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLevelDbJson.Font = new System.Drawing.Font("Consolas", 12F);
            this.txtLevelDbJson.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.txtLevelDbJson.Location = new System.Drawing.Point(0, 0);
            this.txtLevelDbJson.Name = "txtLevelDbJson";
            this.txtLevelDbJson.ReadOnly = true;
            this.txtLevelDbJson.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.txtLevelDbJson.Size = new System.Drawing.Size(1176, 777);
            this.txtLevelDbJson.TabIndex = 0;
            this.txtLevelDbJson.Text = "";
            this.txtLevelDbJson.WordWrap = false;
            //
            // tabPanelInfo
            //
            this.tabPanelInfo.Controls.Add(this.txtPanelInfoJson);
            this.tabPanelInfo.Location = new System.Drawing.Point(4, 34);
            this.tabPanelInfo.Name = "tabPanelInfo";
            this.tabPanelInfo.Size = new System.Drawing.Size(1176, 777);
            this.tabPanelInfo.TabIndex = 1;
            this.tabPanelInfo.Text = "PanelInfo JSON";
            //
            // txtPanelInfoJson
            //
            this.txtPanelInfoJson.BackColor = System.Drawing.Color.FromArgb(20, 35, 45);
            this.txtPanelInfoJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPanelInfoJson.Font = new System.Drawing.Font("Consolas", 12F);
            this.txtPanelInfoJson.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.txtPanelInfoJson.Location = new System.Drawing.Point(0, 0);
            this.txtPanelInfoJson.Name = "txtPanelInfoJson";
            this.txtPanelInfoJson.ReadOnly = true;
            this.txtPanelInfoJson.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.txtPanelInfoJson.Size = new System.Drawing.Size(1176, 777);
            this.txtPanelInfoJson.TabIndex = 0;
            this.txtPanelInfoJson.Text = "";
            this.txtPanelInfoJson.WordWrap = false;
            //
            // tabVbJson
            //
            this.tabVbJson.Controls.Add(this.txtVbInferenceJson);
            this.tabVbJson.Location = new System.Drawing.Point(4, 34);
            this.tabVbJson.Name = "tabVbJson";
            this.tabVbJson.Size = new System.Drawing.Size(1176, 777);
            this.tabVbJson.TabIndex = 2;
            this.tabVbJson.Text = "VB推理JSON";
            //
            // txtVbInferenceJson
            //
            this.txtVbInferenceJson.BackColor = System.Drawing.Color.FromArgb(20, 35, 45);
            this.txtVbInferenceJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtVbInferenceJson.Font = new System.Drawing.Font("Consolas", 12F);
            this.txtVbInferenceJson.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.txtVbInferenceJson.Location = new System.Drawing.Point(0, 0);
            this.txtVbInferenceJson.Name = "txtVbInferenceJson";
            this.txtVbInferenceJson.ReadOnly = true;
            this.txtVbInferenceJson.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.txtVbInferenceJson.Size = new System.Drawing.Size(1176, 777);
            this.txtVbInferenceJson.TabIndex = 0;
            this.txtVbInferenceJson.Text = "";
            this.txtVbInferenceJson.WordWrap = false;
            //
            // tabInferReturn
            //
            this.tabInferReturn.Controls.Add(this.txtInferenceReturnJson);
            this.tabInferReturn.Location = new System.Drawing.Point(4, 34);
            this.tabInferReturn.Name = "tabInferReturn";
            this.tabInferReturn.Size = new System.Drawing.Size(1176, 777);
            this.tabInferReturn.TabIndex = 3;
            this.tabInferReturn.Text = "推理返回JSON";
            //
            // txtInferenceReturnJson
            //
            this.txtInferenceReturnJson.BackColor = System.Drawing.Color.FromArgb(20, 35, 45);
            this.txtInferenceReturnJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInferenceReturnJson.Font = new System.Drawing.Font("Consolas", 12F);
            this.txtInferenceReturnJson.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.txtInferenceReturnJson.Location = new System.Drawing.Point(0, 0);
            this.txtInferenceReturnJson.Name = "txtInferenceReturnJson";
            this.txtInferenceReturnJson.ReadOnly = true;
            this.txtInferenceReturnJson.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.txtInferenceReturnJson.Size = new System.Drawing.Size(1176, 777);
            this.txtInferenceReturnJson.TabIndex = 0;
            this.txtInferenceReturnJson.Text = "";
            this.txtInferenceReturnJson.WordWrap = false;
            //
            // tabOther
            //
            this.tabOther.Controls.Add(this.txtOtherInfo);
            this.tabOther.Location = new System.Drawing.Point(4, 34);
            this.tabOther.Name = "tabOther";
            this.tabOther.Size = new System.Drawing.Size(1176, 777);
            this.tabOther.TabIndex = 4;
            this.tabOther.Text = "其他信息";
            //
            // txtOtherInfo
            //
            this.txtOtherInfo.BackColor = System.Drawing.Color.FromArgb(20, 35, 45);
            this.txtOtherInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOtherInfo.Font = new System.Drawing.Font("Consolas", 12F);
            this.txtOtherInfo.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.txtOtherInfo.Location = new System.Drawing.Point(0, 0);
            this.txtOtherInfo.Name = "txtOtherInfo";
            this.txtOtherInfo.ReadOnly = true;
            this.txtOtherInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.txtOtherInfo.Size = new System.Drawing.Size(1176, 777);
            this.txtOtherInfo.TabIndex = 0;
            this.txtOtherInfo.Text = "";
            this.txtOtherInfo.WordWrap = false;
            //
            // btnPanel
            //
            this.btnPanel.Controls.Add(this.btnClose);
            this.btnPanel.Controls.Add(this.btnCopy);
            this.btnPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnPanel.BackColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.btnPanel.Height = 45;
            this.btnPanel.Location = new System.Drawing.Point(0, 855);
            this.btnPanel.Name = "btnPanel";
            this.btnPanel.Size = new System.Drawing.Size(1184, 45);
            this.btnPanel.TabIndex = 1;
            //
            // btnCopy
            //
            this.btnCopy.BackColor = System.Drawing.Color.FromArgb(50, 80, 100);
            this.btnCopy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopy.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.btnCopy.ForeColor = System.Drawing.Color.White;
            this.btnCopy.Location = new System.Drawing.Point(10, 5);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(180, 35);
            this.btnCopy.TabIndex = 0;
            this.btnCopy.Text = "复制当前页内容";
            this.btnCopy.Click += new System.EventHandler(this.BtnCopy_Click);
            //
            // btnClose
            //
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(50, 80, 100);
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(200, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 35);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "关闭";
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // searchPanel
            //
            this.searchPanel.Controls.Add(this.btnCloseSearch);
            this.searchPanel.Controls.Add(this.btnFind);
            this.searchPanel.Controls.Add(this.txtSearch);
            this.searchPanel.Controls.Add(this.lblSearch);
            this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchPanel.BackColor = System.Drawing.Color.FromArgb(40, 60, 75);
            this.searchPanel.Height = 40;
            this.searchPanel.Location = new System.Drawing.Point(0, 0);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Size = new System.Drawing.Size(1184, 40);
            this.searchPanel.TabIndex = 2;
            this.searchPanel.Visible = false;
            //
            // lblSearch
            //
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.lblSearch.ForeColor = System.Drawing.Color.White;
            this.lblSearch.Location = new System.Drawing.Point(10, 10);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(50, 20);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "搜索:";
            //
            // txtSearch
            //
            this.txtSearch.BackColor = System.Drawing.Color.FromArgb(20, 35, 45);
            this.txtSearch.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.txtSearch.ForeColor = System.Drawing.Color.White;
            this.txtSearch.Location = new System.Drawing.Point(70, 7);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(350, 28);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtSearch_KeyDown);
            //
            // btnFind
            //
            this.btnFind.BackColor = System.Drawing.Color.FromArgb(50, 80, 100);
            this.btnFind.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFind.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnFind.ForeColor = System.Drawing.Color.White;
            this.btnFind.Location = new System.Drawing.Point(430, 5);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(120, 30);
            this.btnFind.TabIndex = 2;
            this.btnFind.Text = "查找下一个";
            this.btnFind.Click += new System.EventHandler(this.BtnFind_Click);
            //
            // btnCloseSearch
            //
            this.btnCloseSearch.BackColor = System.Drawing.Color.FromArgb(50, 80, 100);
            this.btnCloseSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCloseSearch.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnCloseSearch.ForeColor = System.Drawing.Color.White;
            this.btnCloseSearch.Location = new System.Drawing.Point(560, 5);
            this.btnCloseSearch.Name = "btnCloseSearch";
            this.btnCloseSearch.Size = new System.Drawing.Size(30, 30);
            this.btnCloseSearch.TabIndex = 3;
            this.btnCloseSearch.Text = "✕";
            this.btnCloseSearch.Click += new System.EventHandler(this.BtnCloseSearch_Click);
            //
            // FrSnDebugInfo
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.ClientSize = new System.Drawing.Size(1184, 900);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.searchPanel);
            this.Controls.Add(this.btnPanel);
            this.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.KeyPreview = true;
            this.MaximizeBox = true;
            this.MinimizeBox = false;
            this.Name = "FrSnDebugInfo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SN调试信息";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrSnDebugInfo_KeyDown);
            this.tabControl.ResumeLayout(false);
            this.tabLevelDb.ResumeLayout(false);
            this.tabPanelInfo.ResumeLayout(false);
            this.tabVbJson.ResumeLayout(false);
            this.tabInferReturn.ResumeLayout(false);
            this.tabOther.ResumeLayout(false);
            this.btnPanel.ResumeLayout(false);
            this.searchPanel.ResumeLayout(false);
            this.searchPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabLevelDb;
        private System.Windows.Forms.RichTextBox txtLevelDbJson;
        private System.Windows.Forms.TabPage tabPanelInfo;
        private System.Windows.Forms.RichTextBox txtPanelInfoJson;
        private System.Windows.Forms.TabPage tabVbJson;
        private System.Windows.Forms.RichTextBox txtVbInferenceJson;
        private System.Windows.Forms.TabPage tabInferReturn;
        private System.Windows.Forms.RichTextBox txtInferenceReturnJson;
        private System.Windows.Forms.TabPage tabOther;
        private System.Windows.Forms.RichTextBox txtOtherInfo;
        private System.Windows.Forms.Panel btnPanel;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Panel searchPanel;
        private System.Windows.Forms.Button btnCloseSearch;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
    }
}

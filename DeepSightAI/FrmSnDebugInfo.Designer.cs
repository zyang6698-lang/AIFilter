namespace DeepSightAI
{
    partial class FrmSnDebugInfo
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
            this.tvLevelDbJson = new System.Windows.Forms.TreeView();
            this.tabPanelInfo = new System.Windows.Forms.TabPage();
            this.tvPanelInfoJson = new System.Windows.Forms.TreeView();
            this.tabVbJson = new System.Windows.Forms.TabPage();
            this.tvVbInferenceJson = new System.Windows.Forms.TreeView();
            this.tabInferReturn = new System.Windows.Forms.TabPage();
            this.tvInferenceReturnJson = new System.Windows.Forms.TreeView();
            this.tabInferAnalysis = new System.Windows.Forms.TabPage();
            this.tvInferAnalysis = new System.Windows.Forms.TreeView();
            this.tabAviWriteBack = new System.Windows.Forms.TabPage();
            this.tvAviWriteBackJson = new System.Windows.Forms.TreeView();
            this.tabVrsWriteBack = new System.Windows.Forms.TabPage();
            this.tvVrsWriteBackJson = new System.Windows.Forms.TreeView();
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
            this.tabInferAnalysis.SuspendLayout();
            this.tabAviWriteBack.SuspendLayout();
            this.tabVrsWriteBack.SuspendLayout();
            this.tabOther.SuspendLayout();
            this.btnPanel.SuspendLayout();
            this.searchPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // tabControl
            //
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl.ItemSize = new System.Drawing.Size(146, 36);
            this.tabControl.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1184, 815);
            this.tabControl.TabIndex = 0;
            this.tabControl.Padding = new System.Drawing.Point(0, 0);
            this.tabControl.TabPages.AddRange(new System.Windows.Forms.TabPage[] {
                this.tabLevelDb,
                this.tabPanelInfo,
                this.tabVbJson,
                this.tabInferReturn,
                this.tabInferAnalysis,
                this.tabAviWriteBack,
                this.tabVrsWriteBack,
                this.tabOther});
            this.tabControl.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.TabControl_DrawItem);
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
            //
            // tabLevelDb
            //
            this.tabLevelDb.Controls.Add(this.tvLevelDbJson);
            this.tabLevelDb.Location = new System.Drawing.Point(4, 34);
            this.tabLevelDb.Name = "tabLevelDb";
            this.tabLevelDb.Size = new System.Drawing.Size(1176, 777);
            this.tabLevelDb.TabIndex = 0;
            this.tabLevelDb.Text = "LevelDB推理请求";
            //
            // tvLevelDbJson
            //
            this.tvLevelDbJson.BackColor = System.Drawing.Color.FromArgb(20, 35, 45);
            this.tvLevelDbJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvLevelDbJson.Font = new System.Drawing.Font("Consolas", 11F);
            this.tvLevelDbJson.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.tvLevelDbJson.Location = new System.Drawing.Point(0, 0);
            this.tvLevelDbJson.Name = "tvLevelDbJson";
            this.tvLevelDbJson.Size = new System.Drawing.Size(1176, 777);
            this.tvLevelDbJson.TabIndex = 0;
            this.tvLevelDbJson.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvLevelDbJson.FullRowSelect = true;
            this.tvLevelDbJson.ItemHeight = 24;
            //
            // tabPanelInfo
            //
            this.tabPanelInfo.Controls.Add(this.tvPanelInfoJson);
            this.tabPanelInfo.Location = new System.Drawing.Point(4, 34);
            this.tabPanelInfo.Name = "tabPanelInfo";
            this.tabPanelInfo.Size = new System.Drawing.Size(1176, 777);
            this.tabPanelInfo.TabIndex = 1;
            this.tabPanelInfo.Text = "PanelInfo JSON";
            //
            // tvPanelInfoJson
            //
            this.tvPanelInfoJson.BackColor = System.Drawing.Color.FromArgb(20, 35, 45);
            this.tvPanelInfoJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvPanelInfoJson.Font = new System.Drawing.Font("Consolas", 11F);
            this.tvPanelInfoJson.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.tvPanelInfoJson.Location = new System.Drawing.Point(0, 0);
            this.tvPanelInfoJson.Name = "tvPanelInfoJson";
            this.tvPanelInfoJson.Size = new System.Drawing.Size(1176, 777);
            this.tvPanelInfoJson.TabIndex = 0;
            this.tvPanelInfoJson.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvPanelInfoJson.FullRowSelect = true;
            this.tvPanelInfoJson.ItemHeight = 24;
            //
            // tabVbJson
            //
            this.tabVbJson.Controls.Add(this.tvVbInferenceJson);
            this.tabVbJson.Location = new System.Drawing.Point(4, 34);
            this.tabVbJson.Name = "tabVbJson";
            this.tabVbJson.Size = new System.Drawing.Size(1176, 777);
            this.tabVbJson.TabIndex = 2;
            this.tabVbJson.Text = "VB推理JSON";
            //
            // tvVbInferenceJson
            //
            this.tvVbInferenceJson.BackColor = System.Drawing.Color.FromArgb(20, 35, 45);
            this.tvVbInferenceJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvVbInferenceJson.Font = new System.Drawing.Font("Consolas", 11F);
            this.tvVbInferenceJson.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.tvVbInferenceJson.Location = new System.Drawing.Point(0, 0);
            this.tvVbInferenceJson.Name = "tvVbInferenceJson";
            this.tvVbInferenceJson.Size = new System.Drawing.Size(1176, 777);
            this.tvVbInferenceJson.TabIndex = 0;
            this.tvVbInferenceJson.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvVbInferenceJson.FullRowSelect = true;
            this.tvVbInferenceJson.ItemHeight = 24;
            //
            // tabInferReturn
            //
            this.tabInferReturn.Controls.Add(this.tvInferenceReturnJson);
            this.tabInferReturn.Location = new System.Drawing.Point(4, 34);
            this.tabInferReturn.Name = "tabInferReturn";
            this.tabInferReturn.Size = new System.Drawing.Size(1176, 777);
            this.tabInferReturn.TabIndex = 3;
            this.tabInferReturn.Text = "推理返回JSON";
            //
            // tvInferenceReturnJson
            //
            this.tvInferenceReturnJson.BackColor = System.Drawing.Color.FromArgb(20, 35, 45);
            this.tvInferenceReturnJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvInferenceReturnJson.Font = new System.Drawing.Font("Consolas", 11F);
            this.tvInferenceReturnJson.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.tvInferenceReturnJson.Location = new System.Drawing.Point(0, 0);
            this.tvInferenceReturnJson.Name = "tvInferenceReturnJson";
            this.tvInferenceReturnJson.Size = new System.Drawing.Size(1176, 777);
            this.tvInferenceReturnJson.TabIndex = 0;
            this.tvInferenceReturnJson.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvInferenceReturnJson.FullRowSelect = true;
            this.tvInferenceReturnJson.ItemHeight = 24;
            //
            // tabInferAnalysis
            //
            this.tabInferAnalysis.Controls.Add(this.tvInferAnalysis);
            this.tabInferAnalysis.Location = new System.Drawing.Point(4, 34);
            this.tabInferAnalysis.Name = "tabInferAnalysis";
            this.tabInferAnalysis.Size = new System.Drawing.Size(1176, 777);
            this.tabInferAnalysis.TabIndex = 8;
            this.tabInferAnalysis.Text = "推理结果解析";
            //
            // tvInferAnalysis
            //
            this.tvInferAnalysis.BackColor = System.Drawing.Color.FromArgb(20, 35, 45);
            this.tvInferAnalysis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvInferAnalysis.Font = new System.Drawing.Font("Consolas", 11F);
            this.tvInferAnalysis.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.tvInferAnalysis.Location = new System.Drawing.Point(0, 0);
            this.tvInferAnalysis.Name = "tvInferAnalysis";
            this.tvInferAnalysis.Size = new System.Drawing.Size(1176, 777);
            this.tvInferAnalysis.TabIndex = 0;
            this.tvInferAnalysis.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvInferAnalysis.FullRowSelect = true;
            this.tvInferAnalysis.ItemHeight = 24;
            //
            // tabAviWriteBack
            //
            this.tabAviWriteBack.Controls.Add(this.tvAviWriteBackJson);
            this.tabAviWriteBack.Location = new System.Drawing.Point(4, 34);
            this.tabAviWriteBack.Name = "tabAviWriteBack";
            this.tabAviWriteBack.Size = new System.Drawing.Size(1176, 777);
            this.tabAviWriteBack.TabIndex = 5;
            this.tabAviWriteBack.Text = "回写AVI JSON";
            //
            // tvAviWriteBackJson
            //
            this.tvAviWriteBackJson.BackColor = System.Drawing.Color.FromArgb(20, 35, 45);
            this.tvAviWriteBackJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvAviWriteBackJson.Font = new System.Drawing.Font("Consolas", 11F);
            this.tvAviWriteBackJson.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.tvAviWriteBackJson.Location = new System.Drawing.Point(0, 0);
            this.tvAviWriteBackJson.Name = "tvAviWriteBackJson";
            this.tvAviWriteBackJson.Size = new System.Drawing.Size(1176, 777);
            this.tvAviWriteBackJson.TabIndex = 0;
            this.tvAviWriteBackJson.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvAviWriteBackJson.FullRowSelect = true;
            this.tvAviWriteBackJson.ItemHeight = 24;
            //
            // tabVrsWriteBack
            //
            this.tabVrsWriteBack.Controls.Add(this.tvVrsWriteBackJson);
            this.tabVrsWriteBack.Location = new System.Drawing.Point(4, 34);
            this.tabVrsWriteBack.Name = "tabVrsWriteBack";
            this.tabVrsWriteBack.Size = new System.Drawing.Size(1176, 777);
            this.tabVrsWriteBack.TabIndex = 6;
            this.tabVrsWriteBack.Text = "回写VRS JSON";
            //
            // tvVrsWriteBackJson
            //
            this.tvVrsWriteBackJson.BackColor = System.Drawing.Color.FromArgb(20, 35, 45);
            this.tvVrsWriteBackJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvVrsWriteBackJson.Font = new System.Drawing.Font("Consolas", 11F);
            this.tvVrsWriteBackJson.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.tvVrsWriteBackJson.Location = new System.Drawing.Point(0, 0);
            this.tvVrsWriteBackJson.Name = "tvVrsWriteBackJson";
            this.tvVrsWriteBackJson.Size = new System.Drawing.Size(1176, 777);
            this.tvVrsWriteBackJson.TabIndex = 0;
            this.tvVrsWriteBackJson.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvVrsWriteBackJson.FullRowSelect = true;
            this.tvVrsWriteBackJson.ItemHeight = 24;
            //
            // tabOther
            //
            this.tabOther.Controls.Add(this.txtOtherInfo);
            this.tabOther.Location = new System.Drawing.Point(4, 34);
            this.tabOther.Name = "tabOther";
            this.tabOther.Size = new System.Drawing.Size(1176, 777);
            this.tabOther.TabIndex = 7;
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
            // FrmSnDebugInfo
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
            this.Name = "FrmSnDebugInfo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SN调试信息";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrSnDebugInfo_KeyDown);
            this.tabControl.ResumeLayout(false);
            this.tabLevelDb.ResumeLayout(false);
            this.tabPanelInfo.ResumeLayout(false);
            this.tabVbJson.ResumeLayout(false);
            this.tabInferReturn.ResumeLayout(false);
            this.tabInferAnalysis.ResumeLayout(false);
            this.tabAviWriteBack.ResumeLayout(false);
            this.tabVrsWriteBack.ResumeLayout(false);
            this.tabOther.ResumeLayout(false);
            this.btnPanel.ResumeLayout(false);
            this.searchPanel.ResumeLayout(false);
            this.searchPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabLevelDb;
        private System.Windows.Forms.TreeView tvLevelDbJson;
        private System.Windows.Forms.TabPage tabPanelInfo;
        private System.Windows.Forms.TreeView tvPanelInfoJson;
        private System.Windows.Forms.TabPage tabVbJson;
        private System.Windows.Forms.TreeView tvVbInferenceJson;
        private System.Windows.Forms.TabPage tabInferReturn;
        private System.Windows.Forms.TreeView tvInferenceReturnJson;
        private System.Windows.Forms.TabPage tabInferAnalysis;
        private System.Windows.Forms.TreeView tvInferAnalysis;
        private System.Windows.Forms.TabPage tabAviWriteBack;
        private System.Windows.Forms.TreeView tvAviWriteBackJson;
        private System.Windows.Forms.TabPage tabVrsWriteBack;
        private System.Windows.Forms.TreeView tvVrsWriteBackJson;
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

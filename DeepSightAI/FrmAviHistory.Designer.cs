namespace DeepSightAI
{
    partial class FrmAviHistory
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
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.tlpToolbar = new System.Windows.Forms.TableLayoutPanel();
            this.lblStart = new System.Windows.Forms.Label();
            this.dtpStart = new Sunny.UI.UIDatetimePicker();
            this.lblEnd = new System.Windows.Forms.Label();
            this.dtpEnd = new Sunny.UI.UIDatetimePicker();
            this.lblDb = new System.Windows.Forms.Label();
            this.cmbDbConfig = new System.Windows.Forms.ComboBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.dgvHistory = new System.Windows.Forms.DataGridView();
            this.btnQuery = new DeepSightAI.StyledButton();
            this.btnResend = new DeepSightAI.StyledButton();
            this.tlpMain.SuspendLayout();
            this.tlpToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).BeginInit();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.lblTitle, 0, 0);
            this.tlpMain.Controls.Add(this.tlpToolbar, 0, 1);
            this.tlpMain.Controls.Add(this.dgvHistory, 0, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(1100, 600);
            this.tlpMain.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(3, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(1094, 45);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "AVI历史数据";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tlpToolbar
            // 
            this.tlpToolbar.ColumnCount = 7;
            this.tlpToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 400F));
            this.tlpToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 400F));
            this.tlpToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpToolbar.Controls.Add(this.lblStart, 0, 0);
            this.tlpToolbar.Controls.Add(this.dtpStart, 1, 0);
            this.tlpToolbar.Controls.Add(this.lblEnd, 3, 0);
            this.tlpToolbar.Controls.Add(this.dtpEnd, 4, 0);
            this.tlpToolbar.Controls.Add(this.lblDb, 0, 1);
            this.tlpToolbar.Controls.Add(this.cmbDbConfig, 1, 1);
            this.tlpToolbar.Controls.Add(this.btnQuery, 3, 1);
            this.tlpToolbar.Controls.Add(this.btnResend, 4, 1);
            this.tlpToolbar.Controls.Add(this.lblStatus, 6, 0);
            this.tlpToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpToolbar.Location = new System.Drawing.Point(3, 48);
            this.tlpToolbar.Name = "tlpToolbar";
            this.tlpToolbar.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            this.tlpToolbar.RowCount = 2;
            this.tlpToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpToolbar.Size = new System.Drawing.Size(1094, 104);
            this.tlpToolbar.TabIndex = 1;
            // 
            // lblStart
            // 
            this.lblStart.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblStart.AutoSize = true;
            this.lblStart.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.lblStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblStart.Location = new System.Drawing.Point(11, 14);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(93, 25);
            this.lblStart.TabIndex = 0;
            this.lblStart.Text = "起始时间:";
            // 
            // dtpStart
            // 
            this.dtpStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpStart.DateCultureInfo = new System.Globalization.CultureInfo("zh-CN");
            this.dtpStart.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dtpStart.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.dtpStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.dtpStart.Location = new System.Drawing.Point(111, 9);
            this.dtpStart.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dtpStart.MaxLength = 19;
            this.dtpStart.MinimumSize = new System.Drawing.Size(63, 0);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.dtpStart.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(120)))));
            this.dtpStart.Size = new System.Drawing.Size(392, 36);
            this.dtpStart.Style = Sunny.UI.UIStyle.Custom;
            this.dtpStart.StyleCustomMode = true;
            this.dtpStart.SymbolDropDown = 61555;
            this.dtpStart.SymbolNormal = 61555;
            this.dtpStart.SymbolSize = 24;
            this.dtpStart.TabIndex = 1;
            this.dtpStart.Text = "2026-05-15 16:35:59";
            this.dtpStart.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.dtpStart.Value = new System.DateTime(2026, 5, 15, 16, 35, 59, 675);
            this.dtpStart.Watermark = "";
            // 
            // lblEnd
            // 
            this.lblEnd.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblEnd.AutoSize = true;
            this.lblEnd.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.lblEnd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblEnd.Location = new System.Drawing.Point(530, 14);
            this.lblEnd.Name = "lblEnd";
            this.lblEnd.Size = new System.Drawing.Size(93, 25);
            this.lblEnd.TabIndex = 2;
            this.lblEnd.Text = "截止时间:";
            // 
            // dtpEnd
            // 
            this.dtpEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpEnd.DateCultureInfo = new System.Globalization.CultureInfo("zh-CN");
            this.dtpEnd.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dtpEnd.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.dtpEnd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.dtpEnd.Location = new System.Drawing.Point(637, 9);
            this.dtpEnd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dtpEnd.MaxLength = 19;
            this.dtpEnd.MinimumSize = new System.Drawing.Size(63, 0);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.dtpEnd.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(120)))));
            this.dtpEnd.Size = new System.Drawing.Size(392, 36);
            this.dtpEnd.Style = Sunny.UI.UIStyle.Custom;
            this.dtpEnd.StyleCustomMode = true;
            this.dtpEnd.SymbolDropDown = 61555;
            this.dtpEnd.SymbolNormal = 61555;
            this.dtpEnd.SymbolSize = 24;
            this.dtpEnd.TabIndex = 3;
            this.dtpEnd.Text = "2026-05-15 16:35:59";
            this.dtpEnd.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.dtpEnd.Value = new System.DateTime(2026, 5, 15, 16, 35, 59, 720);
            this.dtpEnd.Watermark = "";
            // 
            // lblDb
            // 
            this.lblDb.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDb.AutoSize = true;
            this.lblDb.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.lblDb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblDb.Location = new System.Drawing.Point(11, 64);
            this.lblDb.Name = "lblDb";
            this.lblDb.Size = new System.Drawing.Size(74, 25);
            this.lblDb.TabIndex = 4;
            this.lblDb.Text = "数据源:";
            // 
            // cmbDbConfig
            // 
            this.cmbDbConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbDbConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDbConfig.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.cmbDbConfig.FormattingEnabled = true;
            this.cmbDbConfig.Location = new System.Drawing.Point(110, 60);
            this.cmbDbConfig.Name = "cmbDbConfig";
            this.cmbDbConfig.Size = new System.Drawing.Size(328, 32);
            this.cmbDbConfig.TabIndex = 5;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.lblStatus.ForeColor = System.Drawing.Color.Yellow;
            this.lblStatus.Location = new System.Drawing.Point(1056, 39);
            this.lblStatus.Name = "lblStatus";
            this.tlpToolbar.SetRowSpan(this.lblStatus, 2);
            this.lblStatus.Size = new System.Drawing.Size(0, 25);
            this.lblStatus.TabIndex = 8;
            // 
            // dgvHistory
            // 
            this.dgvHistory.AllowUserToAddRows = false;
            this.dgvHistory.AllowUserToDeleteRows = false;
            this.dgvHistory.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvHistory.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgvHistory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvHistory.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvHistory.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvHistory.EnableHeadersVisualStyles = false;
            this.dgvHistory.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(70)))), ((int)(((byte)(90)))));
            this.dgvHistory.Location = new System.Drawing.Point(3, 158);
            this.dgvHistory.Name = "dgvHistory";
            this.dgvHistory.ReadOnly = true;
            this.dgvHistory.RowHeadersVisible = false;
            this.dgvHistory.RowHeadersWidth = 51;
            this.dgvHistory.RowTemplate.Height = 28;
            this.dgvHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvHistory.Size = new System.Drawing.Size(1094, 439);
            this.dgvHistory.TabIndex = 2;
            // 
            // btnQuery
            // 
            this.btnQuery.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnQuery.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnQuery.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnQuery.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuery.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.btnQuery.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnQuery.Location = new System.Drawing.Point(530, 59);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(100, 36);
            this.btnQuery.TabIndex = 6;
            this.btnQuery.Text = "查询";
            this.btnQuery.UseVisualStyleBackColor = false;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // btnResend
            // 
            this.btnResend.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnResend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnResend.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnResend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResend.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.btnResend.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnResend.Location = new System.Drawing.Point(636, 59);
            this.btnResend.Name = "btnResend";
            this.btnResend.Size = new System.Drawing.Size(140, 36);
            this.btnResend.TabIndex = 7;
            this.btnResend.Text = "重发选中项";
            this.btnResend.UseVisualStyleBackColor = false;
            this.btnResend.Click += new System.EventHandler(this.btnResend_Click);
            // 
            // FrmAviHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.ClientSize = new System.Drawing.Size(1100, 600);
            this.Controls.Add(this.tlpMain);
            this.MinimizeBox = false;
            this.Name = "FrmAviHistory";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AVI历史数据查看";
            this.Load += new System.EventHandler(this.FrmAviHistory_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpToolbar.ResumeLayout(false);
            this.tlpToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tlpToolbar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblStart;
        private Sunny.UI.UIDatetimePicker dtpStart;
        private System.Windows.Forms.Label lblEnd;
        private Sunny.UI.UIDatetimePicker dtpEnd;
        private System.Windows.Forms.Label lblDb;
        private System.Windows.Forms.ComboBox cmbDbConfig;
        private DeepSightAI.StyledButton btnQuery;
        private DeepSightAI.StyledButton btnResend;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView dgvHistory;
    }
}

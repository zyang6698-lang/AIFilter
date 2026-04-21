namespace DeepSightAI
{
    partial class FrmAlarm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panelStats = new System.Windows.Forms.Panel();
            this.flpStats = new System.Windows.Forms.FlowLayoutPanel();
            this.lblCriticalCount = new System.Windows.Forms.Label();
            this.lblErrorCount = new System.Windows.Forms.Label();
            this.lblWarningCount = new System.Windows.Forms.Label();
            this.lblInfoCount = new System.Windows.Forms.Label();
            this.flpActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnTestAlarm = new DeepSightAI.StyledButton();
            this.btnExport = new DeepSightAI.StyledButton();
            this.cboLevelFilter = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dataGridViewData = new System.Windows.Forms.DataGridView();
            this.ColLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CreateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColSource = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DefectName1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel.SuspendLayout();
            this.panelStats.SuspendLayout();
            this.flpStats.SuspendLayout();
            this.flpActions.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewData)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.panelStats, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.panel2, 0, 1);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(643, 401);
            this.tableLayoutPanel.TabIndex = 1;
            // 
            // panelStats
            // 
            this.panelStats.Controls.Add(this.flpStats);
            this.panelStats.Controls.Add(this.flpActions);
            this.panelStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelStats.Location = new System.Drawing.Point(2, 2);
            this.panelStats.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panelStats.Name = "panelStats";
            this.panelStats.Size = new System.Drawing.Size(639, 36);
            this.panelStats.TabIndex = 2;
            // 
            // flpStats
            // 
            this.flpStats.Controls.Add(this.lblCriticalCount);
            this.flpStats.Controls.Add(this.lblErrorCount);
            this.flpStats.Controls.Add(this.lblWarningCount);
            this.flpStats.Controls.Add(this.lblInfoCount);
            this.flpStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpStats.Location = new System.Drawing.Point(0, 0);
            this.flpStats.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.flpStats.Name = "flpStats";
            this.flpStats.Padding = new System.Windows.Forms.Padding(3, 11, 0, 0);
            this.flpStats.Size = new System.Drawing.Size(426, 36);
            this.flpStats.TabIndex = 0;
            this.flpStats.WrapContents = false;
            // 
            // lblCriticalCount
            // 
            this.lblCriticalCount.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.lblCriticalCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(79)))));
            this.lblCriticalCount.Location = new System.Drawing.Point(3, 14);
            this.lblCriticalCount.Margin = new System.Windows.Forms.Padding(0, 3, 11, 0);
            this.lblCriticalCount.Name = "lblCriticalCount";
            this.lblCriticalCount.Size = new System.Drawing.Size(87, 15);
            this.lblCriticalCount.TabIndex = 0;
            this.lblCriticalCount.Text = "🔴 严重: 0";
            this.lblCriticalCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblErrorCount
            // 
            this.lblErrorCount.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.lblErrorCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(173)))), ((int)(((byte)(20)))));
            this.lblErrorCount.Location = new System.Drawing.Point(101, 14);
            this.lblErrorCount.Margin = new System.Windows.Forms.Padding(0, 3, 11, 0);
            this.lblErrorCount.Name = "lblErrorCount";
            this.lblErrorCount.Size = new System.Drawing.Size(87, 15);
            this.lblErrorCount.TabIndex = 1;
            this.lblErrorCount.Text = "🟠 错误: 0";
            this.lblErrorCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblWarningCount
            // 
            this.lblWarningCount.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.lblWarningCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(219)))), ((int)(((byte)(20)))));
            this.lblWarningCount.Location = new System.Drawing.Point(199, 14);
            this.lblWarningCount.Margin = new System.Windows.Forms.Padding(0, 3, 11, 0);
            this.lblWarningCount.Name = "lblWarningCount";
            this.lblWarningCount.Size = new System.Drawing.Size(87, 15);
            this.lblWarningCount.TabIndex = 2;
            this.lblWarningCount.Text = "🟡 警告: 0";
            this.lblWarningCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInfoCount
            // 
            this.lblInfoCount.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.lblInfoCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(144)))), ((int)(((byte)(255)))));
            this.lblInfoCount.Location = new System.Drawing.Point(297, 14);
            this.lblInfoCount.Margin = new System.Windows.Forms.Padding(0, 3, 11, 0);
            this.lblInfoCount.Name = "lblInfoCount";
            this.lblInfoCount.Size = new System.Drawing.Size(87, 15);
            this.lblInfoCount.TabIndex = 3;
            this.lblInfoCount.Text = "🔵 提示: 0";
            this.lblInfoCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flpActions
            // 
            this.flpActions.Controls.Add(this.btnTestAlarm);
            this.flpActions.Controls.Add(this.btnExport);
            this.flpActions.Controls.Add(this.cboLevelFilter);
            this.flpActions.Dock = System.Windows.Forms.DockStyle.Right;
            this.flpActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpActions.Location = new System.Drawing.Point(426, 0);
            this.flpActions.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.flpActions.Name = "flpActions";
            this.flpActions.Padding = new System.Windows.Forms.Padding(0, 7, 5, 7);
            this.flpActions.Size = new System.Drawing.Size(213, 36);
            this.flpActions.TabIndex = 1;
            this.flpActions.WrapContents = false;
            // 
            // btnTestAlarm
            // 
            this.btnTestAlarm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnTestAlarm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTestAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTestAlarm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTestAlarm.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnTestAlarm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnTestAlarm.Location = new System.Drawing.Point(136, 8);
            this.btnTestAlarm.Margin = new System.Windows.Forms.Padding(5, 1, 0, 0);
            this.btnTestAlarm.Name = "btnTestAlarm";
            this.btnTestAlarm.Size = new System.Drawing.Size(72, 25);
            this.btnTestAlarm.TabIndex = 5;
            this.btnTestAlarm.Text = "🧪 测试告警";
            this.btnTestAlarm.UseVisualStyleBackColor = false;
            this.btnTestAlarm.Click += new System.EventHandler(this.btnTestAlarm_Click);
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnExport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnExport.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnExport.Location = new System.Drawing.Point(83, 8);
            this.btnExport.Margin = new System.Windows.Forms.Padding(5, 1, 0, 0);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(48, 25);
            this.btnExport.TabIndex = 4;
            this.btnExport.Text = "导出";
            this.btnExport.UseVisualStyleBackColor = false;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // cboLevelFilter
            // 
            this.cboLevelFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(65)))), ((int)(((byte)(80)))));
            this.cboLevelFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLevelFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboLevelFilter.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.cboLevelFilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.cboLevelFilter.Items.AddRange(new object[] {
            "全部",
            "提示",
            "警告",
            "错误",
            "严重"});
            this.cboLevelFilter.Location = new System.Drawing.Point(13, 8);
            this.cboLevelFilter.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.cboLevelFilter.Name = "cboLevelFilter";
            this.cboLevelFilter.Size = new System.Drawing.Size(65, 25);
            this.cboLevelFilter.TabIndex = 3;
            this.cboLevelFilter.SelectedIndexChanged += new System.EventHandler(this.cboLevelFilter_SelectedIndexChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dataGridViewData);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(2, 42);
            this.panel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(639, 357);
            this.panel2.TabIndex = 1;
            // 
            // dataGridViewData
            // 
            this.dataGridViewData.AllowUserToAddRows = false;
            this.dataGridViewData.AllowUserToDeleteRows = false;
            this.dataGridViewData.AllowUserToResizeColumns = false;
            this.dataGridViewData.AllowUserToResizeRows = false;
            this.dataGridViewData.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColLevel,
            this.CreateTime,
            this.ColCategory,
            this.ColSource,
            this.DefectName1,
            this.ColCount});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(249)))), ((int)(((byte)(235)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.InactiveCaption;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewData.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewData.EnableHeadersVisualStyles = false;
            this.dataGridViewData.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewData.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dataGridViewData.MultiSelect = false;
            this.dataGridViewData.Name = "dataGridViewData";
            this.dataGridViewData.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewData.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewData.RowHeadersVisible = false;
            this.dataGridViewData.RowHeadersWidth = 51;
            this.dataGridViewData.RowTemplate.Height = 27;
            this.dataGridViewData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewData.ShowCellErrors = false;
            this.dataGridViewData.ShowCellToolTips = false;
            this.dataGridViewData.ShowEditingIcon = false;
            this.dataGridViewData.ShowRowErrors = false;
            this.dataGridViewData.Size = new System.Drawing.Size(639, 357);
            this.dataGridViewData.TabIndex = 1;
            // 
            // ColLevel
            // 
            this.ColLevel.HeaderText = "等级";
            this.ColLevel.MinimumWidth = 6;
            this.ColLevel.Name = "ColLevel";
            this.ColLevel.ReadOnly = true;
            this.ColLevel.Width = 60;
            // 
            // CreateTime
            // 
            this.CreateTime.DataPropertyName = "CreateTime";
            this.CreateTime.FillWeight = 120F;
            this.CreateTime.HeaderText = "时间";
            this.CreateTime.MinimumWidth = 6;
            this.CreateTime.Name = "CreateTime";
            this.CreateTime.ReadOnly = true;
            this.CreateTime.Width = 150;
            // 
            // ColCategory
            // 
            this.ColCategory.HeaderText = "分类";
            this.ColCategory.MinimumWidth = 6;
            this.ColCategory.Name = "ColCategory";
            this.ColCategory.ReadOnly = true;
            this.ColCategory.Width = 80;
            // 
            // ColSource
            // 
            this.ColSource.HeaderText = "来源";
            this.ColSource.MinimumWidth = 6;
            this.ColSource.Name = "ColSource";
            this.ColSource.ReadOnly = true;
            this.ColSource.Width = 110;
            // 
            // DefectName1
            // 
            this.DefectName1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.DefectName1.DataPropertyName = "DefectName1";
            this.DefectName1.HeaderText = "告警信息";
            this.DefectName1.MinimumWidth = 6;
            this.DefectName1.Name = "DefectName1";
            this.DefectName1.ReadOnly = true;
            // 
            // ColCount
            // 
            this.ColCount.HeaderText = "次数";
            this.ColCount.MinimumWidth = 6;
            this.ColCount.Name = "ColCount";
            this.ColCount.ReadOnly = true;
            this.ColCount.Width = 55;
            // 
            // FrmAlarm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.ClientSize = new System.Drawing.Size(643, 401);
            this.Controls.Add(this.tableLayoutPanel);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FrmAlarm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmAlarm";
            this.Load += new System.EventHandler(this.FrAlarm_Load);
            this.tableLayoutPanel.ResumeLayout(false);
            this.panelStats.ResumeLayout(false);
            this.flpStats.ResumeLayout(false);
            this.flpActions.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Panel panelStats;
        private System.Windows.Forms.Label lblCriticalCount;
        private System.Windows.Forms.Label lblErrorCount;
        private System.Windows.Forms.Label lblWarningCount;
        private System.Windows.Forms.Label lblInfoCount;
        private System.Windows.Forms.ComboBox cboLevelFilter;
        private DeepSightAI.StyledButton btnExport;
        private DeepSightAI.StyledButton btnTestAlarm;
        private System.Windows.Forms.FlowLayoutPanel flpActions;
        private System.Windows.Forms.FlowLayoutPanel flpStats;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView dataGridViewData;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColLevel;
        private System.Windows.Forms.DataGridViewTextBoxColumn CreateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColCategory;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn DefectName1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColCount;
    }
}
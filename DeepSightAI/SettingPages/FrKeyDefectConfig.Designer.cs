namespace DeepSightAI.SettingPages
{
    partial class FrKeyDefectConfig
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlProfile = new System.Windows.Forms.Panel();
            this.lblProfile = new System.Windows.Forms.Label();
            this.cboProfile = new System.Windows.Forms.ComboBox();
            this.btnNewProfile = new System.Windows.Forms.Button();
            this.btnDeleteProfile = new System.Windows.Forms.Button();
            this.dgvDefects = new System.Windows.Forms.DataGridView();
            this.colDefectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIsKey = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colIsDirectReport = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colAutoDiscovered = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlActions = new System.Windows.Forms.Panel();
            this.lblNewName = new System.Windows.Forms.Label();
            this.txtNewDefectName = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnToggleAll = new System.Windows.Forms.Button();
            this.grpAlarm = new System.Windows.Forms.GroupBox();
            this.chkAlarmEnabled = new System.Windows.Forms.CheckBox();
            this.lblRatio = new System.Windows.Forms.Label();
            this.nudRatioThreshold = new System.Windows.Forms.NumericUpDown();
            this.lblCount = new System.Windows.Forms.Label();
            this.nudCountThreshold = new System.Windows.Forms.NumericUpDown();
            this.lblCooldown = new System.Windows.Forms.Label();
            this.nudCooldown = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel1.SuspendLayout();
            this.pnlProfile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDefects)).BeginInit();
            this.pnlActions.SuspendLayout();
            this.grpAlarm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRatioThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCountThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCooldown)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.pnlProfile, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dgvDefects, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.pnlActions, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.grpAlarm, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(8);
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(779, 750);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pnlProfile
            // 
            this.pnlProfile.Controls.Add(this.lblProfile);
            this.pnlProfile.Controls.Add(this.cboProfile);
            this.pnlProfile.Controls.Add(this.btnNewProfile);
            this.pnlProfile.Controls.Add(this.btnDeleteProfile);
            this.pnlProfile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlProfile.Location = new System.Drawing.Point(11, 11);
            this.pnlProfile.Name = "pnlProfile";
            this.pnlProfile.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.pnlProfile.Size = new System.Drawing.Size(757, 40);
            this.pnlProfile.TabIndex = 0;
            // 
            // lblProfile
            // 
            this.lblProfile.AutoSize = true;
            this.lblProfile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblProfile.Location = new System.Drawing.Point(0, 10);
            this.lblProfile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblProfile.Name = "lblProfile";
            this.lblProfile.Size = new System.Drawing.Size(75, 15);
            this.lblProfile.TabIndex = 0;
            this.lblProfile.Text = "缺陷配置:";
            // 
            // cboProfile
            // 
            this.cboProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.cboProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboProfile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.cboProfile.Location = new System.Drawing.Point(80, 6);
            this.cboProfile.Margin = new System.Windows.Forms.Padding(4);
            this.cboProfile.Name = "cboProfile";
            this.cboProfile.Size = new System.Drawing.Size(265, 23);
            this.cboProfile.TabIndex = 10;
            this.cboProfile.SelectedIndexChanged += new System.EventHandler(this.cboProfile_SelectedIndexChanged);
            // 
            // btnNewProfile
            // 
            this.btnNewProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnNewProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewProfile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnNewProfile.Location = new System.Drawing.Point(360, 4);
            this.btnNewProfile.Margin = new System.Windows.Forms.Padding(4);
            this.btnNewProfile.Name = "btnNewProfile";
            this.btnNewProfile.Size = new System.Drawing.Size(100, 31);
            this.btnNewProfile.TabIndex = 11;
            this.btnNewProfile.Text = "新建配置";
            this.btnNewProfile.UseVisualStyleBackColor = false;
            this.btnNewProfile.Click += new System.EventHandler(this.btnNewProfile_Click);
            // 
            // btnDeleteProfile
            // 
            this.btnDeleteProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnDeleteProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteProfile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnDeleteProfile.Location = new System.Drawing.Point(470, 4);
            this.btnDeleteProfile.Margin = new System.Windows.Forms.Padding(4);
            this.btnDeleteProfile.Name = "btnDeleteProfile";
            this.btnDeleteProfile.Size = new System.Drawing.Size(100, 31);
            this.btnDeleteProfile.TabIndex = 12;
            this.btnDeleteProfile.Text = "删除配置";
            this.btnDeleteProfile.UseVisualStyleBackColor = false;
            this.btnDeleteProfile.Click += new System.EventHandler(this.btnDeleteProfile_Click);
            // 
            // dgvDefects
            // 
            this.dgvDefects.AllowUserToAddRows = false;
            this.dgvDefects.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(38)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.dgvDefects.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDefects.ColumnHeadersHeight = 29;
            this.dgvDefects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDefectName,
            this.colIsKey,
            this.colIsDirectReport,
            this.colAutoDiscovered});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDefects.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDefects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDefects.EnableHeadersVisualStyles = false;
            this.dgvDefects.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(70)))), ((int)(((byte)(85)))));
            this.dgvDefects.Location = new System.Drawing.Point(12, 58);
            this.dgvDefects.Margin = new System.Windows.Forms.Padding(4);
            this.dgvDefects.Name = "dgvDefects";
            this.dgvDefects.RowHeadersVisible = false;
            this.dgvDefects.RowHeadersWidth = 51;
            this.dgvDefects.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDefects.Size = new System.Drawing.Size(755, 526);
            this.dgvDefects.TabIndex = 0;
            this.dgvDefects.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgvDefects_CurrentCellDirtyStateChanged);
            // 
            // colDefectName
            // 
            this.colDefectName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDefectName.HeaderText = "缺陷名称";
            this.colDefectName.MinimumWidth = 120;
            this.colDefectName.Name = "colDefectName";
            // 
            // colIsKey
            // 
            this.colIsKey.HeaderText = "重点缺陷";
            this.colIsKey.MinimumWidth = 6;
            this.colIsKey.Name = "colIsKey";
            this.colIsKey.Width = 80;
            // 
            // colIsDirectReport
            // 
            this.colIsDirectReport.HeaderText = "直报";
            this.colIsDirectReport.MinimumWidth = 6;
            this.colIsDirectReport.Name = "colIsDirectReport";
            this.colIsDirectReport.Width = 80;
            // 
            // colAutoDiscovered
            // 
            this.colAutoDiscovered.HeaderText = "来源";
            this.colAutoDiscovered.MinimumWidth = 6;
            this.colAutoDiscovered.Name = "colAutoDiscovered";
            this.colAutoDiscovered.ReadOnly = true;
            this.colAutoDiscovered.Width = 125;
            // 
            // pnlActions
            // 
            this.pnlActions.Controls.Add(this.lblNewName);
            this.pnlActions.Controls.Add(this.txtNewDefectName);
            this.pnlActions.Controls.Add(this.btnAdd);
            this.pnlActions.Controls.Add(this.btnDelete);
            this.pnlActions.Controls.Add(this.btnToggleAll);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlActions.Location = new System.Drawing.Point(11, 591);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.pnlActions.Size = new System.Drawing.Size(757, 40);
            this.pnlActions.TabIndex = 1;
            // 
            // lblNewName
            // 
            this.lblNewName.AutoSize = true;
            this.lblNewName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblNewName.Location = new System.Drawing.Point(0, 10);
            this.lblNewName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNewName.Name = "lblNewName";
            this.lblNewName.Size = new System.Drawing.Size(75, 15);
            this.lblNewName.TabIndex = 13;
            this.lblNewName.Text = "缺陷名称:";
            // 
            // txtNewDefectName
            // 
            this.txtNewDefectName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.txtNewDefectName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtNewDefectName.Location = new System.Drawing.Point(94, 6);
            this.txtNewDefectName.Margin = new System.Windows.Forms.Padding(4);
            this.txtNewDefectName.Name = "txtNewDefectName";
            this.txtNewDefectName.Size = new System.Drawing.Size(199, 25);
            this.txtNewDefectName.TabIndex = 1;
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnAdd.Location = new System.Drawing.Point(309, 4);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(4);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(100, 31);
            this.btnAdd.TabIndex = 14;
            this.btnAdd.Text = "添加";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnDelete.Location = new System.Drawing.Point(419, 4);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(100, 31);
            this.btnDelete.TabIndex = 15;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnToggleAll
            // 
            this.btnToggleAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnToggleAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnToggleAll.Location = new System.Drawing.Point(529, 4);
            this.btnToggleAll.Margin = new System.Windows.Forms.Padding(4);
            this.btnToggleAll.Name = "btnToggleAll";
            this.btnToggleAll.Size = new System.Drawing.Size(113, 31);
            this.btnToggleAll.TabIndex = 16;
            this.btnToggleAll.Text = "全选/取消";
            this.btnToggleAll.UseVisualStyleBackColor = false;
            this.btnToggleAll.Click += new System.EventHandler(this.btnToggleAll_Click);
            // 
            // grpAlarm
            // 
            this.grpAlarm.Controls.Add(this.chkAlarmEnabled);
            this.grpAlarm.Controls.Add(this.lblRatio);
            this.grpAlarm.Controls.Add(this.nudRatioThreshold);
            this.grpAlarm.Controls.Add(this.lblCount);
            this.grpAlarm.Controls.Add(this.nudCountThreshold);
            this.grpAlarm.Controls.Add(this.lblCooldown);
            this.grpAlarm.Controls.Add(this.nudCooldown);
            this.grpAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpAlarm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.grpAlarm.Location = new System.Drawing.Point(12, 638);
            this.grpAlarm.Margin = new System.Windows.Forms.Padding(4);
            this.grpAlarm.Name = "grpAlarm";
            this.grpAlarm.Padding = new System.Windows.Forms.Padding(4);
            this.grpAlarm.Size = new System.Drawing.Size(755, 100);
            this.grpAlarm.TabIndex = 5;
            this.grpAlarm.TabStop = false;
            this.grpAlarm.Text = "报警配置";
            // 
            // chkAlarmEnabled
            // 
            this.chkAlarmEnabled.AutoSize = true;
            this.chkAlarmEnabled.Checked = true;
            this.chkAlarmEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAlarmEnabled.Location = new System.Drawing.Point(20, 28);
            this.chkAlarmEnabled.Margin = new System.Windows.Forms.Padding(4);
            this.chkAlarmEnabled.Name = "chkAlarmEnabled";
            this.chkAlarmEnabled.Size = new System.Drawing.Size(89, 19);
            this.chkAlarmEnabled.TabIndex = 0;
            this.chkAlarmEnabled.Text = "启用报警";
            // 
            // lblRatio
            // 
            this.lblRatio.AutoSize = true;
            this.lblRatio.Location = new System.Drawing.Point(20, 62);
            this.lblRatio.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRatio.Name = "lblRatio";
            this.lblRatio.Size = new System.Drawing.Size(99, 15);
            this.lblRatio.TabIndex = 1;
            this.lblRatio.Text = "比例阈值(%):";
            // 
            // nudRatioThreshold
            // 
            this.nudRatioThreshold.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.nudRatioThreshold.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.nudRatioThreshold.Location = new System.Drawing.Point(133, 60);
            this.nudRatioThreshold.Margin = new System.Windows.Forms.Padding(4);
            this.nudRatioThreshold.Name = "nudRatioThreshold";
            this.nudRatioThreshold.Size = new System.Drawing.Size(80, 25);
            this.nudRatioThreshold.TabIndex = 2;
            this.nudRatioThreshold.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(233, 62);
            this.lblCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(75, 15);
            this.lblCount.TabIndex = 3;
            this.lblCount.Text = "数量阈值:";
            // 
            // nudCountThreshold
            // 
            this.nudCountThreshold.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.nudCountThreshold.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.nudCountThreshold.Location = new System.Drawing.Point(327, 60);
            this.nudCountThreshold.Margin = new System.Windows.Forms.Padding(4);
            this.nudCountThreshold.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.nudCountThreshold.Name = "nudCountThreshold";
            this.nudCountThreshold.Size = new System.Drawing.Size(93, 25);
            this.nudCountThreshold.TabIndex = 4;
            // 
            // lblCooldown
            // 
            this.lblCooldown.AutoSize = true;
            this.lblCooldown.Location = new System.Drawing.Point(440, 62);
            this.lblCooldown.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCooldown.Name = "lblCooldown";
            this.lblCooldown.Size = new System.Drawing.Size(76, 15);
            this.lblCooldown.TabIndex = 5;
            this.lblCooldown.Text = "冷却(秒):";
            // 
            // nudCooldown
            // 
            this.nudCooldown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.nudCooldown.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.nudCooldown.Location = new System.Drawing.Point(533, 60);
            this.nudCooldown.Margin = new System.Windows.Forms.Padding(4);
            this.nudCooldown.Maximum = new decimal(new int[] {
            86400,
            0,
            0,
            0});
            this.nudCooldown.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudCooldown.Name = "nudCooldown";
            this.nudCooldown.Size = new System.Drawing.Size(93, 25);
            this.nudCooldown.TabIndex = 6;
            this.nudCooldown.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // FrKeyDefectConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(38)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(779, 750);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FrKeyDefectConfig";
            this.Text = "重点缺陷管理";
            this.Load += new System.EventHandler(this.FrKeyDefectConfig_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pnlProfile.ResumeLayout(false);
            this.pnlProfile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDefects)).EndInit();
            this.pnlActions.ResumeLayout(false);
            this.pnlActions.PerformLayout();
            this.grpAlarm.ResumeLayout(false);
            this.grpAlarm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRatioThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCountThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCooldown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel pnlProfile;
        private System.Windows.Forms.Panel pnlActions;
        private System.Windows.Forms.DataGridView dgvDefects;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDefectName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsKey;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsDirectReport;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAutoDiscovered;
        private System.Windows.Forms.TextBox txtNewDefectName;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnToggleAll;
        private System.Windows.Forms.GroupBox grpAlarm;
        private System.Windows.Forms.CheckBox chkAlarmEnabled;
        private System.Windows.Forms.Label lblRatio;
        private System.Windows.Forms.NumericUpDown nudRatioThreshold;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.NumericUpDown nudCountThreshold;
        private System.Windows.Forms.Label lblCooldown;
        private System.Windows.Forms.NumericUpDown nudCooldown;
        private System.Windows.Forms.Label lblNewName;
        private System.Windows.Forms.Label lblProfile;
        private System.Windows.Forms.ComboBox cboProfile;
        private System.Windows.Forms.Button btnNewProfile;
        private System.Windows.Forms.Button btnDeleteProfile;

    }
}

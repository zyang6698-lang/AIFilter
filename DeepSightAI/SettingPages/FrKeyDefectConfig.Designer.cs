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
            this.dgvDefects = new System.Windows.Forms.DataGridView();
            this.colDefectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIsKey = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colIsDirectReport = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colAutoDiscovered = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.lblNewName = new System.Windows.Forms.Label();
            this.lblProfile = new System.Windows.Forms.Label();
            this.cboProfile = new System.Windows.Forms.ComboBox();
            this.btnNewProfile = new System.Windows.Forms.Button();
            this.btnDeleteProfile = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDefects)).BeginInit();
            this.grpAlarm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRatioThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCountThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCooldown)).BeginInit();
            this.SuspendLayout();
            //
            // lblProfile
            //
            this.lblProfile.AutoSize = true;
            this.lblProfile.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.lblProfile.Location = new System.Drawing.Point(12, 16);
            this.lblProfile.Name = "lblProfile";
            this.lblProfile.Size = new System.Drawing.Size(65, 12);
            this.lblProfile.Text = "缺陷配置:";
            //
            // cboProfile
            //
            this.cboProfile.BackColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.cboProfile.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.cboProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboProfile.Location = new System.Drawing.Point(83, 12);
            this.cboProfile.Name = "cboProfile";
            this.cboProfile.Size = new System.Drawing.Size(200, 20);
            this.cboProfile.TabIndex = 10;
            this.cboProfile.SelectedIndexChanged += new System.EventHandler(this.cboProfile_SelectedIndexChanged);
            //
            // btnNewProfile
            //
            this.btnNewProfile.BackColor = System.Drawing.Color.FromArgb(0, 64, 82);
            this.btnNewProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewProfile.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.btnNewProfile.Location = new System.Drawing.Point(295, 10);
            this.btnNewProfile.Name = "btnNewProfile";
            this.btnNewProfile.Size = new System.Drawing.Size(75, 25);
            this.btnNewProfile.Text = "新建配置";
            this.btnNewProfile.UseVisualStyleBackColor = false;
            this.btnNewProfile.Click += new System.EventHandler(this.btnNewProfile_Click);
            //
            // btnDeleteProfile
            //
            this.btnDeleteProfile.BackColor = System.Drawing.Color.FromArgb(0, 64, 82);
            this.btnDeleteProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteProfile.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.btnDeleteProfile.Location = new System.Drawing.Point(380, 10);
            this.btnDeleteProfile.Name = "btnDeleteProfile";
            this.btnDeleteProfile.Size = new System.Drawing.Size(75, 25);
            this.btnDeleteProfile.Text = "删除配置";
            this.btnDeleteProfile.UseVisualStyleBackColor = false;
            this.btnDeleteProfile.Click += new System.EventHandler(this.btnDeleteProfile_Click);
            //
            // dgvDefects
            //
            this.dgvDefects.AllowUserToAddRows = false;
            this.dgvDefects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvDefects.BackgroundColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.dgvDefects.ColumnHeadersDefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(20, 38, 48),
                ForeColor = System.Drawing.Color.FromArgb(216, 219, 188),
                Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold),
                Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
            };
            this.dgvDefects.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(29, 48, 60),
                ForeColor = System.Drawing.Color.FromArgb(216, 219, 188),
                SelectionBackColor = System.Drawing.Color.FromArgb(0, 64, 82),
                SelectionForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("微软雅黑", 9F)
            };
            this.dgvDefects.EnableHeadersVisualStyles = false;
            this.dgvDefects.GridColor = System.Drawing.Color.FromArgb(50, 70, 85);
            this.dgvDefects.RowHeadersVisible = false;
            this.dgvDefects.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDefects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colDefectName, this.colIsKey, this.colIsDirectReport, this.colAutoDiscovered});
            this.dgvDefects.Location = new System.Drawing.Point(12, 42);
            this.dgvDefects.Name = "dgvDefects";
            this.dgvDefects.Size = new System.Drawing.Size(560, 250);
            this.dgvDefects.TabIndex = 0;
            this.dgvDefects.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgvDefects_CurrentCellDirtyStateChanged);
            //
            // colDefectName
            //
            this.colDefectName.HeaderText = "缺陷名称";
            this.colDefectName.Name = "colDefectName";
            this.colDefectName.Width = 200;
            //
            // colIsKey
            //
            this.colIsKey.HeaderText = "重点缺陷";
            this.colIsKey.Name = "colIsKey";
            this.colIsKey.Width = 80;
            //
            // colIsDirectReport
            //
            this.colIsDirectReport.HeaderText = "直报";
            this.colIsDirectReport.Name = "colIsDirectReport";
            this.colIsDirectReport.Width = 80;
            //
            // colAutoDiscovered
            //
            this.colAutoDiscovered.HeaderText = "来源";
            this.colAutoDiscovered.Name = "colAutoDiscovered";
            this.colAutoDiscovered.ReadOnly = true;
            this.colAutoDiscovered.Width = 100;
            //
            // lblNewName
            //
            this.lblNewName.AutoSize = true;
            this.lblNewName.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.lblNewName.Location = new System.Drawing.Point(12, 300);
            this.lblNewName.Name = "lblNewName";
            this.lblNewName.Size = new System.Drawing.Size(65, 12);
            this.lblNewName.Text = "缺陷名称:";
            //
            // txtNewDefectName
            //
            this.txtNewDefectName.BackColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.txtNewDefectName.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.txtNewDefectName.Location = new System.Drawing.Point(83, 297);
            this.txtNewDefectName.Name = "txtNewDefectName";
            this.txtNewDefectName.Size = new System.Drawing.Size(150, 21);
            this.txtNewDefectName.TabIndex = 1;
            //
            // btnAdd
            //
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(0, 64, 82);
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.btnAdd.Location = new System.Drawing.Point(245, 295);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 25);
            this.btnAdd.Text = "添加";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            //
            // btnDelete
            //
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(0, 64, 82);
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.btnDelete.Location = new System.Drawing.Point(330, 295);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 25);
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            //
            // btnToggleAll
            //
            this.btnToggleAll.BackColor = System.Drawing.Color.FromArgb(0, 64, 82);
            this.btnToggleAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleAll.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.btnToggleAll.Location = new System.Drawing.Point(415, 295);
            this.btnToggleAll.Name = "btnToggleAll";
            this.btnToggleAll.Size = new System.Drawing.Size(85, 25);
            this.btnToggleAll.Text = "全选/取消";
            this.btnToggleAll.UseVisualStyleBackColor = false;
            this.btnToggleAll.Click += new System.EventHandler(this.btnToggleAll_Click);
            //
            // grpAlarm
            //
            this.grpAlarm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpAlarm.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.grpAlarm.Location = new System.Drawing.Point(12, 328);
            this.grpAlarm.Name = "grpAlarm";
            this.grpAlarm.Size = new System.Drawing.Size(560, 95);
            this.grpAlarm.TabIndex = 5;
            this.grpAlarm.TabStop = false;
            this.grpAlarm.Text = "报警配置";
            this.grpAlarm.Controls.Add(this.chkAlarmEnabled);
            this.grpAlarm.Controls.Add(this.lblRatio);
            this.grpAlarm.Controls.Add(this.nudRatioThreshold);
            this.grpAlarm.Controls.Add(this.lblCount);
            this.grpAlarm.Controls.Add(this.nudCountThreshold);
            this.grpAlarm.Controls.Add(this.lblCooldown);
            this.grpAlarm.Controls.Add(this.nudCooldown);
            //
            // chkAlarmEnabled
            //
            this.chkAlarmEnabled.AutoSize = true;
            this.chkAlarmEnabled.Checked = true;
            this.chkAlarmEnabled.Location = new System.Drawing.Point(15, 22);
            this.chkAlarmEnabled.Name = "chkAlarmEnabled";
            this.chkAlarmEnabled.Size = new System.Drawing.Size(96, 16);
            this.chkAlarmEnabled.Text = "启用报警";
            //
            // lblRatio
            //
            this.lblRatio.AutoSize = true;
            this.lblRatio.Location = new System.Drawing.Point(15, 50);
            this.lblRatio.Name = "lblRatio";
            this.lblRatio.Text = "比例阈值(%):";
            //
            // nudRatioThreshold
            //
            this.nudRatioThreshold.BackColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.nudRatioThreshold.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.nudRatioThreshold.Location = new System.Drawing.Point(100, 48);
            this.nudRatioThreshold.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudRatioThreshold.Name = "nudRatioThreshold";
            this.nudRatioThreshold.Size = new System.Drawing.Size(60, 21);
            this.nudRatioThreshold.Value = new decimal(new int[] { 30, 0, 0, 0 });
            //
            // lblCount
            //
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(175, 50);
            this.lblCount.Name = "lblCount";
            this.lblCount.Text = "数量阈值:";
            //
            // nudCountThreshold
            //
            this.nudCountThreshold.BackColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.nudCountThreshold.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.nudCountThreshold.Location = new System.Drawing.Point(245, 48);
            this.nudCountThreshold.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            this.nudCountThreshold.Name = "nudCountThreshold";
            this.nudCountThreshold.Size = new System.Drawing.Size(70, 21);
            //
            // lblCooldown
            //
            this.lblCooldown.AutoSize = true;
            this.lblCooldown.Location = new System.Drawing.Point(330, 50);
            this.lblCooldown.Name = "lblCooldown";
            this.lblCooldown.Text = "冷却(秒):";
            //
            // nudCooldown
            //
            this.nudCooldown.BackColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.nudCooldown.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.nudCooldown.Location = new System.Drawing.Point(400, 48);
            this.nudCooldown.Maximum = new decimal(new int[] { 86400, 0, 0, 0 });
            this.nudCooldown.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this.nudCooldown.Name = "nudCooldown";
            this.nudCooldown.Size = new System.Drawing.Size(70, 21);
            this.nudCooldown.Value = new decimal(new int[] { 300, 0, 0, 0 });
            //
            // FrKeyDefectConfig
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(20, 38, 48);
            this.ClientSize = new System.Drawing.Size(584, 600);
            this.Controls.Add(this.lblProfile);
            this.Controls.Add(this.cboProfile);
            this.Controls.Add(this.btnNewProfile);
            this.Controls.Add(this.btnDeleteProfile);
            this.Controls.Add(this.dgvDefects);
            this.Controls.Add(this.lblNewName);
            this.Controls.Add(this.txtNewDefectName);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnToggleAll);
            this.Controls.Add(this.grpAlarm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrKeyDefectConfig";
            this.Text = "重点缺陷管理";
            this.Load += new System.EventHandler(this.FrKeyDefectConfig_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDefects)).EndInit();
            this.grpAlarm.ResumeLayout(false);
            this.grpAlarm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRatioThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCountThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCooldown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

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

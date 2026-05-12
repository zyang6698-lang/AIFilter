namespace DeepSightAI.SettingPages
{
    partial class PgSettingKeyDefect
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlProfile = new System.Windows.Forms.TableLayoutPanel();
            this.lblProfile = new System.Windows.Forms.Label();
            this.cboProfile = new System.Windows.Forms.ComboBox();
            this.btnNewProfile = new System.Windows.Forms.Button();
            this.btnDeleteProfile = new System.Windows.Forms.Button();
            this.dgvDefects = new Sunny.UI.UIDataGridView();
            this.colDefectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIsKey = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colIsDirectReport = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colAutoDiscovered = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlActions = new System.Windows.Forms.TableLayoutPanel();
            this.lblNewName = new System.Windows.Forms.Label();
            this.txtNewDefectName = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnToggleAll = new System.Windows.Forms.Button();
            this.grpAlarm = new System.Windows.Forms.GroupBox();
            this.tlpAlarm = new System.Windows.Forms.TableLayoutPanel();
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
            this.tlpAlarm.SuspendLayout();
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
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(779, 750);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pnlProfile
            // 
            this.pnlProfile.ColumnCount = 4;
            this.pnlProfile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlProfile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlProfile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlProfile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlProfile.Controls.Add(this.lblProfile, 0, 0);
            this.pnlProfile.Controls.Add(this.cboProfile, 1, 0);
            this.pnlProfile.Controls.Add(this.btnNewProfile, 2, 0);
            this.pnlProfile.Controls.Add(this.btnDeleteProfile, 3, 0);
            this.pnlProfile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlProfile.Location = new System.Drawing.Point(11, 11);
            this.pnlProfile.Name = "pnlProfile";
            this.pnlProfile.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.pnlProfile.RowCount = 1;
            this.pnlProfile.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlProfile.Size = new System.Drawing.Size(757, 44);
            this.pnlProfile.TabIndex = 0;
            // 
            // lblProfile
            // 
            this.lblProfile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblProfile.AutoSize = true;
            this.lblProfile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblProfile.Location = new System.Drawing.Point(4, 12);
            this.lblProfile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblProfile.Name = "lblProfile";
            this.lblProfile.Size = new System.Drawing.Size(99, 20);
            this.lblProfile.TabIndex = 0;
            this.lblProfile.Text = "缺陷配置:";
            // 
            // cboProfile
            // 
            this.cboProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.cboProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboProfile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.cboProfile.Location = new System.Drawing.Point(111, 10);
            this.cboProfile.Margin = new System.Windows.Forms.Padding(4);
            this.cboProfile.Name = "cboProfile";
            this.cboProfile.Size = new System.Drawing.Size(488, 28);
            this.cboProfile.TabIndex = 10;
            this.cboProfile.SelectedIndexChanged += new System.EventHandler(this.cboProfile_SelectedIndexChanged);
            // 
            // btnNewProfile
            // 
            this.btnNewProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnNewProfile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNewProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewProfile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnNewProfile.Location = new System.Drawing.Point(607, 8);
            this.btnNewProfile.Margin = new System.Windows.Forms.Padding(4);
            this.btnNewProfile.Name = "btnNewProfile";
            this.btnNewProfile.Size = new System.Drawing.Size(69, 28);
            this.btnNewProfile.TabIndex = 11;
            this.btnNewProfile.Text = "新建配置";
            this.btnNewProfile.UseVisualStyleBackColor = false;
            this.btnNewProfile.Click += new System.EventHandler(this.btnNewProfile_Click);
            // 
            // btnDeleteProfile
            // 
            this.btnDeleteProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnDeleteProfile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDeleteProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteProfile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnDeleteProfile.Location = new System.Drawing.Point(684, 8);
            this.btnDeleteProfile.Margin = new System.Windows.Forms.Padding(4);
            this.btnDeleteProfile.Name = "btnDeleteProfile";
            this.btnDeleteProfile.Size = new System.Drawing.Size(69, 28);
            this.btnDeleteProfile.TabIndex = 12;
            this.btnDeleteProfile.Text = "删除配置";
            this.btnDeleteProfile.UseVisualStyleBackColor = false;
            this.btnDeleteProfile.Click += new System.EventHandler(this.btnDeleteProfile_Click);
            // 
            // dgvDefects
            // 
            this.dgvDefects.AllowUserToAddRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(58)))));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            this.dgvDefects.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDefects.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgvDefects.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvDefects.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            this.dgvDefects.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDefects.ColumnHeadersHeight = 32;
            this.dgvDefects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDefectName,
            this.colIsKey,
            this.colIsDirectReport,
            this.colAutoDiscovered});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDefects.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvDefects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDefects.EnableHeadersVisualStyles = false;
            this.dgvDefects.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dgvDefects.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.dgvDefects.Location = new System.Drawing.Point(12, 62);
            this.dgvDefects.Margin = new System.Windows.Forms.Padding(4);
            this.dgvDefects.Name = "dgvDefects";
            this.dgvDefects.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDefects.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvDefects.RowHeadersVisible = false;
            this.dgvDefects.RowHeadersWidth = 51;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dgvDefects.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvDefects.ScrollBarBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgvDefects.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.dgvDefects.ScrollBarRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgvDefects.ScrollBarStyleInherited = false;
            this.dgvDefects.SelectedIndex = -1;
            this.dgvDefects.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDefects.Size = new System.Drawing.Size(755, 518);
            this.dgvDefects.StripeEvenColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.dgvDefects.StripeOddColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(58)))));
            this.dgvDefects.Style = Sunny.UI.UIStyle.Custom;
            this.dgvDefects.StyleCustomMode = true;
            this.dgvDefects.TabIndex = 0;
            this.dgvDefects.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgvDefects_CurrentCellDirtyStateChanged);
            // 
            // colDefectName
            // 
            this.colDefectName.HeaderText = "缺陷名称";
            this.colDefectName.MinimumWidth = 120;
            this.colDefectName.Name = "colDefectName";
            this.colDefectName.Width = 469;
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
            this.pnlActions.ColumnCount = 5;
            this.pnlActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlActions.Controls.Add(this.lblNewName, 0, 0);
            this.pnlActions.Controls.Add(this.txtNewDefectName, 1, 0);
            this.pnlActions.Controls.Add(this.btnAdd, 2, 0);
            this.pnlActions.Controls.Add(this.btnDelete, 3, 0);
            this.pnlActions.Controls.Add(this.btnToggleAll, 4, 0);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlActions.Location = new System.Drawing.Point(11, 587);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.pnlActions.RowCount = 1;
            this.pnlActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlActions.Size = new System.Drawing.Size(757, 44);
            this.pnlActions.TabIndex = 1;
            // 
            // lblNewName
            // 
            this.lblNewName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblNewName.AutoSize = true;
            this.lblNewName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblNewName.Location = new System.Drawing.Point(4, 12);
            this.lblNewName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNewName.Name = "lblNewName";
            this.lblNewName.Size = new System.Drawing.Size(99, 20);
            this.lblNewName.TabIndex = 13;
            this.lblNewName.Text = "缺陷名称:";
            // 
            // txtNewDefectName
            // 
            this.txtNewDefectName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNewDefectName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.txtNewDefectName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtNewDefectName.Location = new System.Drawing.Point(111, 8);
            this.txtNewDefectName.Margin = new System.Windows.Forms.Padding(4);
            this.txtNewDefectName.Name = "txtNewDefectName";
            this.txtNewDefectName.Size = new System.Drawing.Size(355, 30);
            this.txtNewDefectName.TabIndex = 1;
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnAdd.Location = new System.Drawing.Point(474, 8);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(4);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(69, 28);
            this.btnAdd.TabIndex = 14;
            this.btnAdd.Text = "添加";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnDelete.Location = new System.Drawing.Point(551, 8);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(69, 28);
            this.btnDelete.TabIndex = 15;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnToggleAll
            // 
            this.btnToggleAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnToggleAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnToggleAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnToggleAll.Location = new System.Drawing.Point(628, 8);
            this.btnToggleAll.Margin = new System.Windows.Forms.Padding(4);
            this.btnToggleAll.Name = "btnToggleAll";
            this.btnToggleAll.Size = new System.Drawing.Size(125, 28);
            this.btnToggleAll.TabIndex = 16;
            this.btnToggleAll.Text = "全选/取消";
            this.btnToggleAll.UseVisualStyleBackColor = false;
            this.btnToggleAll.Click += new System.EventHandler(this.btnToggleAll_Click);
            // 
            // grpAlarm
            // 
            this.grpAlarm.Controls.Add(this.tlpAlarm);
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
            // tlpAlarm
            // 
            this.tlpAlarm.ColumnCount = 7;
            this.tlpAlarm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAlarm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAlarm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAlarm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAlarm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAlarm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAlarm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAlarm.Controls.Add(this.chkAlarmEnabled, 0, 0);
            this.tlpAlarm.Controls.Add(this.lblRatio, 0, 1);
            this.tlpAlarm.Controls.Add(this.nudRatioThreshold, 1, 1);
            this.tlpAlarm.Controls.Add(this.lblCount, 2, 1);
            this.tlpAlarm.Controls.Add(this.nudCountThreshold, 3, 1);
            this.tlpAlarm.Controls.Add(this.lblCooldown, 4, 1);
            this.tlpAlarm.Controls.Add(this.nudCooldown, 5, 1);
            this.tlpAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpAlarm.Location = new System.Drawing.Point(4, 27);
            this.tlpAlarm.Name = "tlpAlarm";
            this.tlpAlarm.RowCount = 2;
            this.tlpAlarm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpAlarm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpAlarm.Size = new System.Drawing.Size(747, 69);
            this.tlpAlarm.TabIndex = 0;
            // 
            // chkAlarmEnabled
            // 
            this.chkAlarmEnabled.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAlarmEnabled.AutoSize = true;
            this.chkAlarmEnabled.Checked = true;
            this.chkAlarmEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tlpAlarm.SetColumnSpan(this.chkAlarmEnabled, 7);
            this.chkAlarmEnabled.Location = new System.Drawing.Point(4, 5);
            this.chkAlarmEnabled.Margin = new System.Windows.Forms.Padding(4);
            this.chkAlarmEnabled.Name = "chkAlarmEnabled";
            this.chkAlarmEnabled.Size = new System.Drawing.Size(111, 24);
            this.chkAlarmEnabled.TabIndex = 0;
            this.chkAlarmEnabled.Text = "启用报警";
            // 
            // lblRatio
            // 
            this.lblRatio.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRatio.AutoSize = true;
            this.lblRatio.Location = new System.Drawing.Point(4, 41);
            this.lblRatio.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRatio.Name = "lblRatio";
            this.lblRatio.Size = new System.Drawing.Size(129, 20);
            this.lblRatio.TabIndex = 1;
            this.lblRatio.Text = "比例阈值(%):";
            // 
            // nudRatioThreshold
            // 
            this.nudRatioThreshold.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudRatioThreshold.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.nudRatioThreshold.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.nudRatioThreshold.Location = new System.Drawing.Point(141, 38);
            this.nudRatioThreshold.Margin = new System.Windows.Forms.Padding(4);
            this.nudRatioThreshold.Name = "nudRatioThreshold";
            this.nudRatioThreshold.Size = new System.Drawing.Size(80, 30);
            this.nudRatioThreshold.TabIndex = 2;
            this.nudRatioThreshold.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // lblCount
            // 
            this.lblCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(229, 41);
            this.lblCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(99, 20);
            this.lblCount.TabIndex = 3;
            this.lblCount.Text = "数量阈值:";
            // 
            // nudCountThreshold
            // 
            this.nudCountThreshold.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudCountThreshold.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.nudCountThreshold.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.nudCountThreshold.Location = new System.Drawing.Point(336, 38);
            this.nudCountThreshold.Margin = new System.Windows.Forms.Padding(4);
            this.nudCountThreshold.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.nudCountThreshold.Name = "nudCountThreshold";
            this.nudCountThreshold.Size = new System.Drawing.Size(93, 30);
            this.nudCountThreshold.TabIndex = 4;
            // 
            // lblCooldown
            // 
            this.lblCooldown.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCooldown.AutoSize = true;
            this.lblCooldown.Location = new System.Drawing.Point(437, 41);
            this.lblCooldown.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCooldown.Name = "lblCooldown";
            this.lblCooldown.Size = new System.Drawing.Size(99, 20);
            this.lblCooldown.TabIndex = 5;
            this.lblCooldown.Text = "冷却(秒):";
            // 
            // nudCooldown
            // 
            this.nudCooldown.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudCooldown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.nudCooldown.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.nudCooldown.Location = new System.Drawing.Point(544, 38);
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
            this.nudCooldown.Size = new System.Drawing.Size(93, 30);
            this.nudCooldown.TabIndex = 6;
            this.nudCooldown.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // PgSettingKeyDefect
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(38)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(779, 750);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "PgSettingKeyDefect";
            this.Text = "重点缺陷管理";
            this.Load += new System.EventHandler(this.FrKeyDefectConfig_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pnlProfile.ResumeLayout(false);
            this.pnlProfile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDefects)).EndInit();
            this.pnlActions.ResumeLayout(false);
            this.pnlActions.PerformLayout();
            this.grpAlarm.ResumeLayout(false);
            this.tlpAlarm.ResumeLayout(false);
            this.tlpAlarm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRatioThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCountThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCooldown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel pnlProfile;
        private System.Windows.Forms.TableLayoutPanel pnlActions;
        private System.Windows.Forms.TableLayoutPanel tlpAlarm;
        private Sunny.UI.UIDataGridView dgvDefects;
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
        private System.Windows.Forms.DataGridViewTextBoxColumn colDefectName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsKey;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsDirectReport;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAutoDiscovered;
    }
}

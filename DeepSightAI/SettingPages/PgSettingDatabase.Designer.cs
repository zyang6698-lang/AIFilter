namespace DeepSightAI.SettingPages
{
    partial class PgSettingDatabase
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.dgvDatabases = new System.Windows.Forms.DataGridView();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnAdd = new DeepSightAI.StyledButton();
            this.btnDelete = new DeepSightAI.StyledButton();
            this.btnTestAll = new DeepSightAI.StyledButton();
            this.colDbName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPort = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWriteBackDbName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVRSWriteBackDbName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVRSWriteBackDbNameV1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colConnectionStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTestConnection = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colIsEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colMinioIpA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMinioStatusA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMinioTestA = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colMinioIpB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMinioStatusB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMinioTestB = new System.Windows.Forms.DataGridViewButtonColumn();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatabases)).BeginInit();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel1.Controls.Add(this.dgvDatabases);
            this.panel1.Controls.Add(this.panelButtons);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(10);
            this.panel1.Size = new System.Drawing.Size(800, 500);
            this.panel1.TabIndex = 0;
            // 
            // dgvDatabases
            // 
            this.dgvDatabases.AllowUserToAddRows = false;
            this.dgvDatabases.AllowUserToDeleteRows = false;
            this.dgvDatabases.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgvDatabases.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(38)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.dgvDatabases.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDatabases.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDatabases.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDbName,
            this.colIP,
            this.colPort,
            this.colWriteBackDbName,
            this.colVRSWriteBackDbName,
            this.colVRSWriteBackDbNameV1,
            this.colConnectionStatus,
            this.colTestConnection,
            this.colIsEnabled,
            this.colMinioIpA,
            this.colMinioStatusA,
            this.colMinioTestA,
            this.colMinioIpB,
            this.colMinioStatusB,
            this.colMinioTestB});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDatabases.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDatabases.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDatabases.EnableHeadersVisualStyles = false;
            this.dgvDatabases.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.dgvDatabases.Location = new System.Drawing.Point(10, 55);
            this.dgvDatabases.MultiSelect = false;
            this.dgvDatabases.Name = "dgvDatabases";
            this.dgvDatabases.RowHeadersVisible = false;
            this.dgvDatabases.RowHeadersWidth = 51;
            this.dgvDatabases.RowTemplate.Height = 30;
            this.dgvDatabases.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDatabases.Size = new System.Drawing.Size(780, 435);
            this.dgvDatabases.TabIndex = 1;
            this.dgvDatabases.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDatabases_CellContentClick);
            this.dgvDatabases.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDatabases_CellValueChanged);
            this.dgvDatabases.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgvDatabases_CurrentCellDirtyStateChanged);
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.btnAdd);
            this.panelButtons.Controls.Add(this.btnDelete);
            this.panelButtons.Controls.Add(this.btnTestAll);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelButtons.Location = new System.Drawing.Point(10, 10);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(780, 45);
            this.panelButtons.TabIndex = 0;
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnAdd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnAdd.Location = new System.Drawing.Point(5, 8);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(100, 30);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "添加数据库";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnDelete.Location = new System.Drawing.Point(115, 8);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(100, 30);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "删除选中";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnTestAll
            // 
            this.btnTestAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnTestAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTestAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTestAll.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnTestAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnTestAll.Location = new System.Drawing.Point(225, 8);
            this.btnTestAll.Name = "btnTestAll";
            this.btnTestAll.Size = new System.Drawing.Size(100, 30);
            this.btnTestAll.TabIndex = 2;
            this.btnTestAll.Text = "全部测试";
            this.btnTestAll.UseVisualStyleBackColor = false;
            this.btnTestAll.Click += new System.EventHandler(this.btnTestAll_Click);
            // 
            // colDbName
            // 
            this.colDbName.HeaderText = "数据库名称";
            this.colDbName.MinimumWidth = 6;
            this.colDbName.Name = "colDbName";
            this.colDbName.Width = 200;
            // 
            // colIP
            // 
            this.colIP.HeaderText = "IP地址";
            this.colIP.MinimumWidth = 6;
            this.colIP.Name = "colIP";
            this.colIP.Width = 200;
            // 
            // colPort
            // 
            this.colPort.HeaderText = "端口";
            this.colPort.MinimumWidth = 6;
            this.colPort.Name = "colPort";
            // 
            // colWriteBackDbName
            // 
            this.colWriteBackDbName.HeaderText = "回写DB名称";
            this.colWriteBackDbName.MinimumWidth = 6;
            this.colWriteBackDbName.Name = "colWriteBackDbName";
            this.colWriteBackDbName.Width = 150;
            // 
            // colVRSWriteBackDbName
            // 
            this.colVRSWriteBackDbName.HeaderText = "VRS回写DB名称";
            this.colVRSWriteBackDbName.MinimumWidth = 6;
            this.colVRSWriteBackDbName.Name = "colVRSWriteBackDbName";
            this.colVRSWriteBackDbName.Width = 150;
            // 
            // colVRSWriteBackDbNameV1
            // 
            this.colVRSWriteBackDbNameV1.HeaderText = "VRS回写数据库V1.0";
            this.colVRSWriteBackDbNameV1.MinimumWidth = 6;
            this.colVRSWriteBackDbNameV1.Name = "colVRSWriteBackDbNameV1";
            this.colVRSWriteBackDbNameV1.Width = 150;
            // 
            // colConnectionStatus
            // 
            this.colConnectionStatus.HeaderText = "连接状态";
            this.colConnectionStatus.MinimumWidth = 6;
            this.colConnectionStatus.Name = "colConnectionStatus";
            this.colConnectionStatus.ReadOnly = true;
            this.colConnectionStatus.Width = 90;
            // 
            // colTestConnection
            // 
            this.colTestConnection.HeaderText = "测试";
            this.colTestConnection.MinimumWidth = 6;
            this.colTestConnection.Name = "colTestConnection";
            this.colTestConnection.Text = "测试连接";
            this.colTestConnection.UseColumnTextForButtonValue = true;
            this.colTestConnection.Width = 80;
            // 
            // colIsEnabled
            // 
            this.colIsEnabled.HeaderText = "启用";
            this.colIsEnabled.MinimumWidth = 6;
            this.colIsEnabled.Name = "colIsEnabled";
            this.colIsEnabled.Width = 60;
            // 
            // colMinioIpA
            // 
            this.colMinioIpA.HeaderText = "A面MinIO IP";
            this.colMinioIpA.MinimumWidth = 6;
            this.colMinioIpA.Name = "colMinioIpA";
            this.colMinioIpA.Width = 150;
            // 
            // colMinioStatusA
            // 
            this.colMinioStatusA.HeaderText = "A面MinIO状态";
            this.colMinioStatusA.MinimumWidth = 6;
            this.colMinioStatusA.Name = "colMinioStatusA";
            this.colMinioStatusA.ReadOnly = true;
            // 
            // colMinioTestA
            // 
            this.colMinioTestA.HeaderText = "A面测试";
            this.colMinioTestA.MinimumWidth = 6;
            this.colMinioTestA.Name = "colMinioTestA";
            this.colMinioTestA.Text = "测试";
            this.colMinioTestA.UseColumnTextForButtonValue = true;
            this.colMinioTestA.Width = 80;
            // 
            // colMinioIpB
            // 
            this.colMinioIpB.HeaderText = "B面MinIO IP";
            this.colMinioIpB.MinimumWidth = 6;
            this.colMinioIpB.Name = "colMinioIpB";
            this.colMinioIpB.Width = 150;
            // 
            // colMinioStatusB
            // 
            this.colMinioStatusB.HeaderText = "B面MinIO状态";
            this.colMinioStatusB.MinimumWidth = 6;
            this.colMinioStatusB.Name = "colMinioStatusB";
            this.colMinioStatusB.ReadOnly = true;
            // 
            // colMinioTestB
            // 
            this.colMinioTestB.HeaderText = "B面测试";
            this.colMinioTestB.MinimumWidth = 6;
            this.colMinioTestB.Name = "colMinioTestB";
            this.colMinioTestB.Text = "测试";
            this.colMinioTestB.UseColumnTextForButtonValue = true;
            this.colMinioTestB.Width = 80;
            // 
            // PgSettingDatabase
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Name = "PgSettingDatabase";
            this.Load += new System.EventHandler(this.FrLevelDbConfig_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatabases)).EndInit();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dgvDatabases;
        private System.Windows.Forms.Panel panelButtons;
        private DeepSightAI.StyledButton btnAdd;
        private DeepSightAI.StyledButton btnDelete;
        private DeepSightAI.StyledButton btnTestAll;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDbName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIP;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPort;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWriteBackDbName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVRSWriteBackDbName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVRSWriteBackDbNameV1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colConnectionStatus;
        private System.Windows.Forms.DataGridViewButtonColumn colTestConnection;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMinioIpA;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMinioStatusA;
        private System.Windows.Forms.DataGridViewButtonColumn colMinioTestA;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMinioIpB;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMinioStatusB;
        private System.Windows.Forms.DataGridViewButtonColumn colMinioTestB;
    }
}


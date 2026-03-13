namespace DeepSightAI
{
    partial class AnalyticsControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnShowAnalytics = new DeepSightAI.StyledButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnClearDatabase = new DeepSightAI.StyledButton();
            this.btnZipPic = new DeepSightAI.StyledButton();
            this.lblImportStatus = new System.Windows.Forms.Label();
            this.progressBarImport = new System.Windows.Forms.ProgressBar();
            this.btnTest = new DeepSightAI.StyledButton();
            this.btnReadEmployeeData = new DeepSightAI.StyledButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnShowAnalytics
            // 
            this.btnShowAnalytics.Location = new System.Drawing.Point(46, 42);
            this.btnShowAnalytics.Name = "btnShowAnalytics";
            this.btnShowAnalytics.Size = new System.Drawing.Size(110, 33);
            this.btnShowAnalytics.TabIndex = 119;
            this.btnShowAnalytics.Text = "图表展示";
            this.btnShowAnalytics.Click += new System.EventHandler(this.btnShowAnalytics_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnClearDatabase);
            this.splitContainer1.Panel1.Controls.Add(this.btnZipPic);
            this.splitContainer1.Panel1.Controls.Add(this.lblImportStatus);
            this.splitContainer1.Panel1.Controls.Add(this.progressBarImport);
            this.splitContainer1.Panel1.Controls.Add(this.btnTest);
            this.splitContainer1.Panel1.Controls.Add(this.btnReadEmployeeData);
            this.splitContainer1.Panel1.Controls.Add(this.btnShowAnalytics);
            this.splitContainer1.Size = new System.Drawing.Size(726, 493);
            this.splitContainer1.SplitterDistance = 202;
            this.splitContainer1.TabIndex = 120;
            // 
            // btnClearDatabase
            // 
            this.btnClearDatabase.Location = new System.Drawing.Point(46, 272);
            this.btnClearDatabase.Name = "btnClearDatabase";
            this.btnClearDatabase.Size = new System.Drawing.Size(110, 33);
            this.btnClearDatabase.TabIndex = 126;
            this.btnClearDatabase.Text = "清空数据库";
            this.btnClearDatabase.Click += new System.EventHandler(this.btnClearDatabase_Click);
            // 
            // btnZipPic
            // 
            this.btnZipPic.Location = new System.Drawing.Point(46, 218);
            this.btnZipPic.Name = "btnZipPic";
            this.btnZipPic.Size = new System.Drawing.Size(110, 33);
            this.btnZipPic.TabIndex = 125;
            this.btnZipPic.Text = "图片压缩";
            this.btnZipPic.Click += new System.EventHandler(this.btnZipPic_Click);
            // 
            // lblImportStatus
            // 
            this.lblImportStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblImportStatus.Location = new System.Drawing.Point(16, 414);
            this.lblImportStatus.Name = "lblImportStatus";
            this.lblImportStatus.Size = new System.Drawing.Size(174, 20);
            this.lblImportStatus.TabIndex = 124;
            this.lblImportStatus.Text = "就绪";
            // 
            // progressBarImport
            // 
            this.progressBarImport.Location = new System.Drawing.Point(16, 384);
            this.progressBarImport.Name = "progressBarImport";
            this.progressBarImport.Size = new System.Drawing.Size(174, 23);
            this.progressBarImport.TabIndex = 123;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(46, 161);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(110, 33);
            this.btnTest.TabIndex = 121;
            this.btnTest.Text = "导入CSV数据";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnReadEmployeeData
            // 
            this.btnReadEmployeeData.Location = new System.Drawing.Point(46, 104);
            this.btnReadEmployeeData.Name = "btnReadEmployeeData";
            this.btnReadEmployeeData.Size = new System.Drawing.Size(110, 33);
            this.btnReadEmployeeData.TabIndex = 120;
            this.btnReadEmployeeData.Text = "读取员工数据";
            this.btnReadEmployeeData.Click += new System.EventHandler(this.btnReadEmployeeData_Click);
            // 
            // AnalyticsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.Controls.Add(this.splitContainer1);
            this.Name = "AnalyticsControl";
            this.Size = new System.Drawing.Size(726, 493);
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DeepSightAI.StyledButton btnShowAnalytics;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private DeepSightAI.StyledButton btnReadEmployeeData;
        private DeepSightAI.StyledButton btnTest;
        private System.Windows.Forms.ProgressBar progressBarImport;
        private System.Windows.Forms.Label lblImportStatus;
        private DeepSightAI.StyledButton btnZipPic;
        private DeepSightAI.StyledButton btnClearDatabase;
    }
}

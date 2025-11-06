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
            this.btnShowAnalytics = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnReadEmployeeData = new System.Windows.Forms.Button();
            this.btnTestDB = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnShowAnalytics
            // 
            this.btnShowAnalytics.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnShowAnalytics.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnShowAnalytics.Location = new System.Drawing.Point(46, 42);
            this.btnShowAnalytics.Name = "btnShowAnalytics";
            this.btnShowAnalytics.Size = new System.Drawing.Size(110, 33);
            this.btnShowAnalytics.TabIndex = 119;
            this.btnShowAnalytics.Text = "图表展示";
            this.btnShowAnalytics.UseVisualStyleBackColor = false;
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
            this.splitContainer1.Panel1.Controls.Add(this.btnTestDB);
            this.splitContainer1.Panel1.Controls.Add(this.btnReadEmployeeData);
            this.splitContainer1.Panel1.Controls.Add(this.btnShowAnalytics);
            this.splitContainer1.Size = new System.Drawing.Size(726, 493);
            this.splitContainer1.SplitterDistance = 202;
            this.splitContainer1.TabIndex = 120;
            // 
            // btnReadEmployeeData
            // 
            this.btnReadEmployeeData.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnReadEmployeeData.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnReadEmployeeData.Location = new System.Drawing.Point(46, 104);
            this.btnReadEmployeeData.Name = "btnReadEmployeeData";
            this.btnReadEmployeeData.Size = new System.Drawing.Size(110, 33);
            this.btnReadEmployeeData.TabIndex = 120;
            this.btnReadEmployeeData.Text = "读取员工数据";
            this.btnReadEmployeeData.UseVisualStyleBackColor = false;
            this.btnReadEmployeeData.Click += new System.EventHandler(this.btnReadEmployeeData_Click);
            // 
            // btnTestDB
            // 
            this.btnTestDB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnTestDB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnTestDB.Location = new System.Drawing.Point(46, 161);
            this.btnTestDB.Name = "btnTestDB";
            this.btnTestDB.Size = new System.Drawing.Size(110, 33);
            this.btnTestDB.TabIndex = 121;
            this.btnTestDB.Text = "测试sqlite";
            this.btnTestDB.UseVisualStyleBackColor = false;
            this.btnTestDB.Click += new System.EventHandler(this.btnTestDB_Click);
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

        private System.Windows.Forms.Button btnShowAnalytics;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnReadEmployeeData;
        private System.Windows.Forms.Button btnTestDB;
    }
}

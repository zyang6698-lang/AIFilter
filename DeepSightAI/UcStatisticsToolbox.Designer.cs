namespace DeepSightAI
{
    partial class UcStatisticsToolbox
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
            this.mainTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.grpDataTools = new System.Windows.Forms.GroupBox();
            this.flowDataTools = new System.Windows.Forms.FlowLayoutPanel();
            this.btnReadEmployeeData = new DeepSightAI.StyledButton();
            this.btnTest = new DeepSightAI.StyledButton();
            this.btnClearDatabase = new DeepSightAI.StyledButton();
            this.btnGenerateInference = new DeepSightAI.StyledButton();
            this.grpFileTools = new System.Windows.Forms.GroupBox();
            this.flowFileTools = new System.Windows.Forms.FlowLayoutPanel();
            this.btnZipPic = new DeepSightAI.StyledButton();
            this.btnExportLogs = new DeepSightAI.StyledButton();
            this.grpStatus = new System.Windows.Forms.GroupBox();
            this.tableStatus = new System.Windows.Forms.TableLayoutPanel();
            this.progressBarImport = new System.Windows.Forms.ProgressBar();
            this.lblImportStatus = new System.Windows.Forms.Label();
            this.mainTableLayout.SuspendLayout();
            this.grpDataTools.SuspendLayout();
            this.flowDataTools.SuspendLayout();
            this.grpFileTools.SuspendLayout();
            this.flowFileTools.SuspendLayout();
            this.grpStatus.SuspendLayout();
            this.tableStatus.SuspendLayout();
            this.SuspendLayout();
            //
            // mainTableLayout
            //
            this.mainTableLayout.ColumnCount = 1;
            this.mainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayout.Controls.Add(this.grpDataTools, 0, 0);
            this.mainTableLayout.Controls.Add(this.grpFileTools, 0, 1);
            this.mainTableLayout.Controls.Add(this.grpStatus, 0, 2);
            this.mainTableLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.mainTableLayout.Location = new System.Drawing.Point(12, 12);
            this.mainTableLayout.Name = "mainTableLayout";
            this.mainTableLayout.Padding = new System.Windows.Forms.Padding(4);
            this.mainTableLayout.RowCount = 3;
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.mainTableLayout.Size = new System.Drawing.Size(400, 380);
            this.mainTableLayout.TabIndex = 0;
            //
            // grpDataTools
            //
            this.grpDataTools.Controls.Add(this.flowDataTools);
            this.grpDataTools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDataTools.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Bold);
            this.grpDataTools.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(210)))));
            this.grpDataTools.Location = new System.Drawing.Point(7, 7);
            this.grpDataTools.Name = "grpDataTools";
            this.grpDataTools.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.grpDataTools.Size = new System.Drawing.Size(386, 100);
            this.grpDataTools.TabIndex = 0;
            this.grpDataTools.TabStop = false;
            this.grpDataTools.Text = "📊 数据工具";
            //
            // flowDataTools
            //
            this.flowDataTools.Controls.Add(this.btnReadEmployeeData);
            this.flowDataTools.Controls.Add(this.btnTest);
            this.flowDataTools.Controls.Add(this.btnClearDatabase);
            this.flowDataTools.Controls.Add(this.btnGenerateInference);
            this.flowDataTools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowDataTools.Location = new System.Drawing.Point(10, 24);
            this.flowDataTools.Name = "flowDataTools";
            this.flowDataTools.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.flowDataTools.Size = new System.Drawing.Size(366, 70);
            this.flowDataTools.TabIndex = 0;
            //
            // btnReadEmployeeData
            //
            this.btnReadEmployeeData.Margin = new System.Windows.Forms.Padding(4);
            this.btnReadEmployeeData.Name = "btnReadEmployeeData";
            this.btnReadEmployeeData.Size = new System.Drawing.Size(130, 36);
            this.btnReadEmployeeData.TabIndex = 0;
            this.btnReadEmployeeData.Text = "读取员工数据";
            this.btnReadEmployeeData.Click += new System.EventHandler(this.btnReadEmployeeData_Click);
            //
            // btnTest
            //
            this.btnTest.Margin = new System.Windows.Forms.Padding(4);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(130, 36);
            this.btnTest.TabIndex = 1;
            this.btnTest.Text = "导入CSV数据";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            //
            // btnClearDatabase
            //
            this.btnClearDatabase.Margin = new System.Windows.Forms.Padding(4);
            this.btnClearDatabase.Name = "btnClearDatabase";
            this.btnClearDatabase.Size = new System.Drawing.Size(130, 36);
            this.btnClearDatabase.TabIndex = 2;
            this.btnClearDatabase.Text = "清空数据库";
            this.btnClearDatabase.Click += new System.EventHandler(this.btnClearDatabase_Click);
            //
            // btnGenerateInference
            //
            this.btnGenerateInference.Margin = new System.Windows.Forms.Padding(4);
            this.btnGenerateInference.Name = "btnGenerateInference";
            this.btnGenerateInference.Size = new System.Drawing.Size(130, 36);
            this.btnGenerateInference.TabIndex = 3;
            this.btnGenerateInference.Text = "生成推理请求";
            this.btnGenerateInference.Click += new System.EventHandler(this.btnGenerateInference_Click);
            //
            // grpFileTools
            //
            this.grpFileTools.Controls.Add(this.flowFileTools);
            this.grpFileTools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpFileTools.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Bold);
            this.grpFileTools.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(210)))));
            this.grpFileTools.Location = new System.Drawing.Point(7, 113);
            this.grpFileTools.Name = "grpFileTools";
            this.grpFileTools.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.grpFileTools.Size = new System.Drawing.Size(386, 100);
            this.grpFileTools.TabIndex = 1;
            this.grpFileTools.TabStop = false;
            this.grpFileTools.Text = "📁 文件工具";
            //
            // flowFileTools
            //
            this.flowFileTools.Controls.Add(this.btnZipPic);
            this.flowFileTools.Controls.Add(this.btnExportLogs);
            this.flowFileTools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowFileTools.Location = new System.Drawing.Point(10, 24);
            this.flowFileTools.Name = "flowFileTools";
            this.flowFileTools.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.flowFileTools.Size = new System.Drawing.Size(366, 70);
            this.flowFileTools.TabIndex = 0;
            //
            // btnZipPic
            //
            this.btnZipPic.Margin = new System.Windows.Forms.Padding(4);
            this.btnZipPic.Name = "btnZipPic";
            this.btnZipPic.Size = new System.Drawing.Size(130, 36);
            this.btnZipPic.TabIndex = 0;
            this.btnZipPic.Text = "图片压缩";
            this.btnZipPic.Click += new System.EventHandler(this.btnZipPic_Click);
            //
            // btnExportLogs
            //
            this.btnExportLogs.Margin = new System.Windows.Forms.Padding(4);
            this.btnExportLogs.Name = "btnExportLogs";
            this.btnExportLogs.Size = new System.Drawing.Size(130, 36);
            this.btnExportLogs.TabIndex = 1;
            this.btnExportLogs.Text = "导出今日日志";
            this.btnExportLogs.Click += new System.EventHandler(this.btnExportLogs_Click);
            //
            // grpStatus
            //
            this.grpStatus.Controls.Add(this.tableStatus);
            this.grpStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpStatus.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Bold);
            this.grpStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(210)))));
            this.grpStatus.Location = new System.Drawing.Point(7, 219);
            this.grpStatus.Name = "grpStatus";
            this.grpStatus.Padding = new System.Windows.Forms.Padding(10, 6, 10, 10);
            this.grpStatus.Size = new System.Drawing.Size(386, 100);
            this.grpStatus.TabIndex = 2;
            this.grpStatus.TabStop = false;
            this.grpStatus.Text = "⏳ 状态";
            //
            // tableStatus
            //
            this.tableStatus.ColumnCount = 1;
            this.tableStatus.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableStatus.Controls.Add(this.progressBarImport, 0, 0);
            this.tableStatus.Controls.Add(this.lblImportStatus, 0, 1);
            this.tableStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableStatus.Location = new System.Drawing.Point(10, 24);
            this.tableStatus.Name = "tableStatus";
            this.tableStatus.RowCount = 2;
            this.tableStatus.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableStatus.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableStatus.Size = new System.Drawing.Size(366, 66);
            this.tableStatus.TabIndex = 0;
            //
            // progressBarImport
            //
            this.progressBarImport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBarImport.Location = new System.Drawing.Point(3, 3);
            this.progressBarImport.Name = "progressBarImport";
            this.progressBarImport.Size = new System.Drawing.Size(360, 24);
            this.progressBarImport.TabIndex = 0;
            //
            // lblImportStatus
            //
            this.lblImportStatus.AutoSize = true;
            this.lblImportStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblImportStatus.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblImportStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblImportStatus.Location = new System.Drawing.Point(3, 30);
            this.lblImportStatus.Name = "lblImportStatus";
            this.lblImportStatus.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblImportStatus.Size = new System.Drawing.Size(360, 24);
            this.lblImportStatus.TabIndex = 1;
            this.lblImportStatus.Text = "就绪";
            //
            // UcStatisticsToolbox
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.Controls.Add(this.mainTableLayout);
            this.Name = "UcStatisticsToolbox";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.Size = new System.Drawing.Size(726, 493);
            this.mainTableLayout.ResumeLayout(false);
            this.grpDataTools.ResumeLayout(false);
            this.flowDataTools.ResumeLayout(false);
            this.grpFileTools.ResumeLayout(false);
            this.flowFileTools.ResumeLayout(false);
            this.grpStatus.ResumeLayout(false);
            this.tableStatus.ResumeLayout(false);
            this.tableStatus.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel mainTableLayout;
        private System.Windows.Forms.GroupBox grpDataTools;
        private System.Windows.Forms.FlowLayoutPanel flowDataTools;
        private DeepSightAI.StyledButton btnReadEmployeeData;
        private DeepSightAI.StyledButton btnTest;
        private DeepSightAI.StyledButton btnClearDatabase;
        private DeepSightAI.StyledButton btnGenerateInference;
        private System.Windows.Forms.GroupBox grpFileTools;
        private System.Windows.Forms.FlowLayoutPanel flowFileTools;
        private DeepSightAI.StyledButton btnZipPic;
        private DeepSightAI.StyledButton btnExportLogs;
        private System.Windows.Forms.GroupBox grpStatus;
        private System.Windows.Forms.TableLayoutPanel tableStatus;
        private System.Windows.Forms.ProgressBar progressBarImport;
        private System.Windows.Forms.Label lblImportStatus;
    }
}

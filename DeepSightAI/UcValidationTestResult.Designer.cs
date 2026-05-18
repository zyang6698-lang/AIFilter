namespace DeepSightAI
{
    partial class UcValidationTestResult
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.splitContainer_Main = new System.Windows.Forms.SplitContainer();
            this.panel_Summary = new System.Windows.Forms.Panel();
            this.label_Summary = new System.Windows.Forms.Label();
            this.progressBar_Test = new System.Windows.Forms.ProgressBar();
            this.label_Progress = new System.Windows.Forms.Label();
            this.label_Title = new System.Windows.Forms.Label();
            this.dataGridView_Results = new System.Windows.Forms.DataGridView();
            this.col_SerialNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Side = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_DataSource = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_OriginalResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_NewResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_IsConsistent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_DefectCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_ConsistentCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_InconsistentCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_MissCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_OverKillCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel_Filter = new System.Windows.Forms.Panel();
            this.btn_CompareResults = new DeepSightAI.StyledButton();
            this.btn_ExportResult = new DeepSightAI.StyledButton();
            this.comboBox_Filter = new System.Windows.Forms.ComboBox();
            this.label_Filter = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).BeginInit();
            this.splitContainer_Main.Panel1.SuspendLayout();
            this.splitContainer_Main.Panel2.SuspendLayout();
            this.splitContainer_Main.SuspendLayout();
            this.panel_Summary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Results)).BeginInit();
            this.panel_Filter.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_Main
            // 
            this.splitContainer_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_Main.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer_Main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_Main.Name = "splitContainer_Main";
            this.splitContainer_Main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_Main.Panel1
            // 
            this.splitContainer_Main.Panel1.Controls.Add(this.panel_Summary);
            // 
            // splitContainer_Main.Panel2
            // 
            this.splitContainer_Main.Panel2.Controls.Add(this.dataGridView_Results);
            this.splitContainer_Main.Panel2.Controls.Add(this.panel_Filter);
            this.splitContainer_Main.Size = new System.Drawing.Size(800, 600);
            this.splitContainer_Main.SplitterDistance = 150;
            this.splitContainer_Main.TabIndex = 0;
            // 
            // panel_Summary
            // 
            this.panel_Summary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(38)))));
            this.panel_Summary.Controls.Add(this.label_Summary);
            this.panel_Summary.Controls.Add(this.progressBar_Test);
            this.panel_Summary.Controls.Add(this.label_Progress);
            this.panel_Summary.Controls.Add(this.label_Title);
            this.panel_Summary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Summary.Location = new System.Drawing.Point(0, 0);
            this.panel_Summary.Name = "panel_Summary";
            this.panel_Summary.Padding = new System.Windows.Forms.Padding(10);
            this.panel_Summary.Size = new System.Drawing.Size(800, 150);
            this.panel_Summary.TabIndex = 0;
            // 
            // label_Title
            // 
            this.label_Title.AutoSize = true;
            this.label_Title.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.label_Title.ForeColor = System.Drawing.Color.White;
            this.label_Title.Location = new System.Drawing.Point(10, 10);
            this.label_Title.Name = "label_Title";
            this.label_Title.Size = new System.Drawing.Size(180, 26);
            this.label_Title.TabIndex = 0;
            this.label_Title.Text = "模型一致性测试结果";
            // 
            // label_Progress
            // 
            this.label_Progress.AutoSize = true;
            this.label_Progress.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.label_Progress.ForeColor = System.Drawing.Color.LightGray;
            this.label_Progress.Location = new System.Drawing.Point(12, 45);
            this.label_Progress.Name = "label_Progress";
            this.label_Progress.Size = new System.Drawing.Size(100, 20);
            this.label_Progress.TabIndex = 1;
            this.label_Progress.Text = "测试进度: 0%";
            // 
            // progressBar_Test
            // 
            this.progressBar_Test.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top) | (System.Windows.Forms.AnchorStyles.Left) | (System.Windows.Forms.AnchorStyles.Right))));
            this.progressBar_Test.Location = new System.Drawing.Point(13, 70);
            this.progressBar_Test.Name = "progressBar_Test";
            this.progressBar_Test.Size = new System.Drawing.Size(774, 23);
            this.progressBar_Test.TabIndex = 2;
            // 
            // label_Summary
            // 
            this.label_Summary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top) | (System.Windows.Forms.AnchorStyles.Left) | (System.Windows.Forms.AnchorStyles.Right))));
            this.label_Summary.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label_Summary.ForeColor = System.Drawing.Color.LightGray;
            this.label_Summary.Location = new System.Drawing.Point(13, 100);
            this.label_Summary.Name = "label_Summary";
            this.label_Summary.Size = new System.Drawing.Size(774, 45);
            this.label_Summary.TabIndex = 3;
            this.label_Summary.Text = "总测试数: 0 | 一致: 0 | 不一致: 0 | 一致率: 0%";
            // 
            // panel_Filter
            // 
            this.panel_Filter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panel_Filter.Controls.Add(this.btn_CompareResults);
            this.panel_Filter.Controls.Add(this.btn_ExportResult);
            this.panel_Filter.Controls.Add(this.comboBox_Filter);
            this.panel_Filter.Controls.Add(this.label_Filter);
            this.panel_Filter.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_Filter.Location = new System.Drawing.Point(0, 0);
            this.panel_Filter.Name = "panel_Filter";
            this.panel_Filter.Size = new System.Drawing.Size(800, 35);
            this.panel_Filter.TabIndex = 0;
            // 
            // label_Filter
            // 
            this.label_Filter.AutoSize = true;
            this.label_Filter.ForeColor = System.Drawing.Color.White;
            this.label_Filter.Location = new System.Drawing.Point(10, 9);
            this.label_Filter.Name = "label_Filter";
            this.label_Filter.Size = new System.Drawing.Size(44, 17);
            this.label_Filter.TabIndex = 0;
            this.label_Filter.Text = "筛选:";
            // 
            // comboBox_Filter
            // 
            this.comboBox_Filter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Filter.Items.AddRange(new object[] { "全部", "一致", "不一致" });
            this.comboBox_Filter.Location = new System.Drawing.Point(60, 6);
            this.comboBox_Filter.Name = "comboBox_Filter";
            this.comboBox_Filter.Size = new System.Drawing.Size(100, 25);
            this.comboBox_Filter.TabIndex = 1;
            //
            // btn_CompareResults
            //
            this.btn_CompareResults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_CompareResults.Enabled = false;
            this.btn_CompareResults.Location = new System.Drawing.Point(581, 5);
            this.btn_CompareResults.Name = "btn_CompareResults";
            this.btn_CompareResults.Size = new System.Drawing.Size(110, 25);
            this.btn_CompareResults.TabIndex = 3;
            this.btn_CompareResults.Text = "对比前后结果";
            //
            // btn_ExportResult
            //
            this.btn_ExportResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ExportResult.Location = new System.Drawing.Point(697, 5);
            this.btn_ExportResult.Name = "btn_ExportResult";
            this.btn_ExportResult.Size = new System.Drawing.Size(90, 25);
            this.btn_ExportResult.TabIndex = 2;
            this.btn_ExportResult.Text = "导出报告";
            //
            // dataGridView_Results
            //
            this.dataGridView_Results.AllowUserToAddRows = false;
            this.dataGridView_Results.AllowUserToDeleteRows = false;
            this.dataGridView_Results.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView_Results.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.dataGridView_Results.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Results.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_SerialNumber,
            this.col_Side,
            this.col_DataSource,
            this.col_OriginalResult,
            this.col_NewResult,
            this.col_IsConsistent,
            this.col_DefectCount,
            this.col_ConsistentCount,
            this.col_InconsistentCount,
            this.col_MissCount,
            this.col_OverKillCount});
            this.dataGridView_Results.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_Results.Location = new System.Drawing.Point(0, 35);
            this.dataGridView_Results.Name = "dataGridView_Results";
            this.dataGridView_Results.ReadOnly = true;
            this.dataGridView_Results.RowTemplate.Height = 25;
            this.dataGridView_Results.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_Results.Size = new System.Drawing.Size(800, 411);
            this.dataGridView_Results.TabIndex = 1;
            //
            // col_SerialNumber
            //
            this.col_SerialNumber.HeaderText = "序列号";
            this.col_SerialNumber.Name = "col_SerialNumber";
            this.col_SerialNumber.ReadOnly = true;
            //
            // col_Side
            //
            this.col_Side.HeaderText = "面别";
            this.col_Side.Name = "col_Side";
            this.col_Side.ReadOnly = true;
            this.col_Side.FillWeight = 40;
            //
            // col_DataSource
            //
            this.col_DataSource.HeaderText = "数据来源";
            this.col_DataSource.Name = "col_DataSource";
            this.col_DataSource.ReadOnly = true;
            this.col_DataSource.FillWeight = 60;
            //
            // col_OriginalResult
            //
            this.col_OriginalResult.HeaderText = "原判定";
            this.col_OriginalResult.Name = "col_OriginalResult";
            this.col_OriginalResult.ReadOnly = true;
            this.col_OriginalResult.FillWeight = 60;
            //
            // col_NewResult
            //
            this.col_NewResult.HeaderText = "新判定";
            this.col_NewResult.Name = "col_NewResult";
            this.col_NewResult.ReadOnly = true;
            this.col_NewResult.FillWeight = 60;
            //
            // col_IsConsistent
            //
            this.col_IsConsistent.HeaderText = "是否一致";
            this.col_IsConsistent.Name = "col_IsConsistent";
            this.col_IsConsistent.ReadOnly = true;
            this.col_IsConsistent.FillWeight = 60;
            //
            // col_DefectCount
            //
            this.col_DefectCount.HeaderText = "缺陷数";
            this.col_DefectCount.Name = "col_DefectCount";
            this.col_DefectCount.ReadOnly = true;
            this.col_DefectCount.FillWeight = 50;
            //
            // col_ConsistentCount
            //
            this.col_ConsistentCount.HeaderText = "一致数";
            this.col_ConsistentCount.Name = "col_ConsistentCount";
            this.col_ConsistentCount.ReadOnly = true;
            this.col_ConsistentCount.FillWeight = 50;
            //
            // col_InconsistentCount
            //
            this.col_InconsistentCount.HeaderText = "不一致数";
            this.col_InconsistentCount.Name = "col_InconsistentCount";
            this.col_InconsistentCount.ReadOnly = true;
            this.col_InconsistentCount.FillWeight = 50;
            //
            // col_MissCount
            //
            this.col_MissCount.HeaderText = "漏失数";
            this.col_MissCount.Name = "col_MissCount";
            this.col_MissCount.ReadOnly = true;
            this.col_MissCount.FillWeight = 50;
            //
            // col_OverKillCount
            //
            this.col_OverKillCount.HeaderText = "误报数";
            this.col_OverKillCount.Name = "col_OverKillCount";
            this.col_OverKillCount.ReadOnly = true;
            this.col_OverKillCount.FillWeight = 50;
            //
            // UcValidationTestResult
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.Controls.Add(this.splitContainer_Main);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Name = "UcValidationTestResult";
            this.Size = new System.Drawing.Size(800, 600);
            this.splitContainer_Main.Panel1.ResumeLayout(false);
            this.splitContainer_Main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).EndInit();
            this.splitContainer_Main.ResumeLayout(false);
            this.panel_Summary.ResumeLayout(false);
            this.panel_Summary.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Results)).EndInit();
            this.panel_Filter.ResumeLayout(false);
            this.panel_Filter.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_Main;
        private System.Windows.Forms.Panel panel_Summary;
        private System.Windows.Forms.Label label_Title;
        private System.Windows.Forms.Label label_Progress;
        private System.Windows.Forms.ProgressBar progressBar_Test;
        private System.Windows.Forms.Label label_Summary;
        private System.Windows.Forms.DataGridView dataGridView_Results;
        private System.Windows.Forms.Panel panel_Filter;
        private System.Windows.Forms.Label label_Filter;
        private System.Windows.Forms.ComboBox comboBox_Filter;
        private DeepSightAI.StyledButton btn_CompareResults;
        private DeepSightAI.StyledButton btn_ExportResult;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_SerialNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Side;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_DataSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_OriginalResult;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_NewResult;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_IsConsistent;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_DefectCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_ConsistentCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_InconsistentCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_MissCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_OverKillCount;
    }
}


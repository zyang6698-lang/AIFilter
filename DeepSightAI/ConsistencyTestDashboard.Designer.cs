namespace DeepSightAI
{
    partial class ConsistencyTestDashboard
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
            this.panel_Left = new System.Windows.Forms.Panel();
            this.listBox_Datasets = new System.Windows.Forms.ListBox();
            this.panel_DatasetButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btn_RefreshDatasets = new DeepSightAI.StyledButton();
            this.btn_DeleteDataset = new DeepSightAI.StyledButton();
            this.btn_StartTest = new DeepSightAI.StyledButton();
            this.label_Rounds = new System.Windows.Forms.Label();
            this.numericUpDown_Rounds = new System.Windows.Forms.NumericUpDown();
            this.label_DatasetTitle = new System.Windows.Forms.Label();
            this.panel_Right = new System.Windows.Forms.Panel();
            this.splitContainer_Right = new System.Windows.Forms.SplitContainer();
            this.panel_RoundsTop = new System.Windows.Forms.Panel();
            this.dataGridView_Rounds = new System.Windows.Forms.DataGridView();
            this.panel_TestProgress = new System.Windows.Forms.Panel();
            this.progressBar_Test = new System.Windows.Forms.ProgressBar();
            this.label_TestProgress = new System.Windows.Forms.Label();
            this.label_RoundsTitle = new System.Windows.Forms.Label();
            this.panel_ChartBottom = new System.Windows.Forms.Panel();
            this.chart_Trend = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label_ChartTitle = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).BeginInit();
            this.splitContainer_Main.Panel1.SuspendLayout();
            this.splitContainer_Main.Panel2.SuspendLayout();
            this.splitContainer_Main.SuspendLayout();
            this.panel_Left.SuspendLayout();
            this.panel_DatasetButtons.SuspendLayout();
            this.panel_Right.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Right)).BeginInit();
            this.splitContainer_Right.Panel1.SuspendLayout();
            this.splitContainer_Right.Panel2.SuspendLayout();
            this.splitContainer_Right.SuspendLayout();
            this.panel_RoundsTop.SuspendLayout();
            this.panel_TestProgress.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Rounds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Rounds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart_Trend)).BeginInit();
            this.panel_ChartBottom.SuspendLayout();
            this.SuspendLayout();
            //
            // splitContainer_Main
            //
            this.splitContainer_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_Main.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer_Main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_Main.Name = "splitContainer_Main";
            this.splitContainer_Main.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.splitContainer_Main.Panel1.Controls.Add(this.panel_Left);
            this.splitContainer_Main.Panel2.Controls.Add(this.panel_Right);
            this.splitContainer_Main.Size = new System.Drawing.Size(1000, 600);
            this.splitContainer_Main.SplitterDistance = 250;
            this.splitContainer_Main.Panel1MinSize = 180;
            this.splitContainer_Main.Panel2MinSize = 200;
            this.splitContainer_Main.TabIndex = 0;
            //
            // panel_Left
            //
            this.panel_Left.BackColor = System.Drawing.Color.FromArgb(35, 35, 38);
            this.panel_Left.Controls.Add(this.listBox_Datasets);
            this.panel_Left.Controls.Add(this.panel_DatasetButtons);
            this.panel_Left.Controls.Add(this.label_DatasetTitle);
            this.panel_Left.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Left.Name = "panel_Left";
            //
            // label_DatasetTitle
            //
            this.label_DatasetTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.label_DatasetTitle.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.label_DatasetTitle.ForeColor = System.Drawing.Color.FromArgb(100, 200, 200);
            this.label_DatasetTitle.Height = 35;
            this.label_DatasetTitle.Name = "label_DatasetTitle";
            this.label_DatasetTitle.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.label_DatasetTitle.Text = "数据集列表";
            this.label_DatasetTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // listBox_Datasets
            //
            this.listBox_Datasets.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            this.listBox_Datasets.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBox_Datasets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox_Datasets.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.listBox_Datasets.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.listBox_Datasets.IntegralHeight = false;
            this.listBox_Datasets.Name = "listBox_Datasets";
            //
            // panel_DatasetButtons
            //
            this.panel_DatasetButtons.BackColor = System.Drawing.Color.FromArgb(35, 35, 38);
            this.panel_DatasetButtons.ColumnCount = 3;
            this.panel_DatasetButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.panel_DatasetButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.panel_DatasetButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.panel_DatasetButtons.RowCount = 2;
            this.panel_DatasetButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.panel_DatasetButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.panel_DatasetButtons.Controls.Add(this.btn_RefreshDatasets, 0, 0);
            this.panel_DatasetButtons.Controls.Add(this.btn_DeleteDataset, 1, 0);
            this.panel_DatasetButtons.Controls.Add(this.btn_StartTest, 0, 1);
            this.panel_DatasetButtons.Controls.Add(this.label_Rounds, 1, 1);
            this.panel_DatasetButtons.Controls.Add(this.numericUpDown_Rounds, 2, 1);
            this.panel_DatasetButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_DatasetButtons.Height = 75;
            this.panel_DatasetButtons.Name = "panel_DatasetButtons";
            this.panel_DatasetButtons.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
            //
            // btn_RefreshDatasets
            //
            this.btn_RefreshDatasets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_RefreshDatasets.Name = "btn_RefreshDatasets";
            this.btn_RefreshDatasets.Text = "刷新";
            //
            // btn_DeleteDataset
            //
            this.btn_DeleteDataset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_DeleteDataset.Name = "btn_DeleteDataset";
            this.btn_DeleteDataset.Text = "删除";
            //
            // btn_StartTest
            //
            this.btn_StartTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_StartTest.Name = "btn_StartTest";
            this.btn_StartTest.Text = "开始测试";
            //
            // label_Rounds
            //
            this.label_Rounds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_Rounds.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            this.label_Rounds.Name = "label_Rounds";
            this.label_Rounds.Text = "轮次:";
            this.label_Rounds.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // numericUpDown_Rounds
            //
            this.numericUpDown_Rounds.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numericUpDown_Rounds.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            this.numericUpDown_Rounds.ForeColor = System.Drawing.Color.White;
            this.numericUpDown_Rounds.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numericUpDown_Rounds.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numericUpDown_Rounds.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.numericUpDown_Rounds.Name = "numericUpDown_Rounds";
            this.numericUpDown_Rounds.Size = new System.Drawing.Size(50, 23);
            //
            // panel_Right
            //
            this.panel_Right.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.panel_Right.Controls.Add(this.splitContainer_Right);
            this.panel_Right.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Right.Name = "panel_Right";
            //
            // splitContainer_Right
            //
            this.splitContainer_Right.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.splitContainer_Right.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_Right.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer_Right.Name = "splitContainer_Right";
            this.splitContainer_Right.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainer_Right.Panel1.Controls.Add(this.panel_RoundsTop);
            this.splitContainer_Right.Panel2.Controls.Add(this.panel_ChartBottom);
            this.splitContainer_Right.Size = new System.Drawing.Size(746, 600);
            this.splitContainer_Right.SplitterDistance = 280;
            this.splitContainer_Right.Panel1MinSize = 100;
            this.splitContainer_Right.Panel2MinSize = 100;
            this.splitContainer_Right.TabIndex = 0;
            //
            // panel_RoundsTop
            //
            this.panel_RoundsTop.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.panel_RoundsTop.Controls.Add(this.dataGridView_Rounds);
            this.panel_RoundsTop.Controls.Add(this.panel_TestProgress);
            this.panel_RoundsTop.Controls.Add(this.label_RoundsTitle);
            this.panel_RoundsTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_RoundsTop.Name = "panel_RoundsTop";
            //
            // label_RoundsTitle
            //
            this.label_RoundsTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.label_RoundsTitle.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.label_RoundsTitle.ForeColor = System.Drawing.Color.FromArgb(100, 200, 200);
            this.label_RoundsTitle.Height = 30;
            this.label_RoundsTitle.Name = "label_RoundsTitle";
            this.label_RoundsTitle.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.label_RoundsTitle.Text = "测试轮次记录";
            this.label_RoundsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // panel_TestProgress
            //
            this.panel_TestProgress.BackColor = System.Drawing.Color.FromArgb(35, 35, 38);
            this.panel_TestProgress.Controls.Add(this.progressBar_Test);
            this.panel_TestProgress.Controls.Add(this.label_TestProgress);
            this.panel_TestProgress.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_TestProgress.Height = 30;
            this.panel_TestProgress.Name = "panel_TestProgress";
            this.panel_TestProgress.Visible = false;
            //
            // label_TestProgress
            //
            this.label_TestProgress.AutoSize = true;
            this.label_TestProgress.ForeColor = System.Drawing.Color.LightGray;
            this.label_TestProgress.Location = new System.Drawing.Point(5, 6);
            this.label_TestProgress.Name = "label_TestProgress";
            this.label_TestProgress.Text = "测试进度: 0%";
            //
            // progressBar_Test
            //
            this.progressBar_Test.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top) | (System.Windows.Forms.AnchorStyles.Left) | (System.Windows.Forms.AnchorStyles.Right))));
            this.progressBar_Test.Location = new System.Drawing.Point(120, 4);
            this.progressBar_Test.Name = "progressBar_Test";
            this.progressBar_Test.Size = new System.Drawing.Size(400, 20);
            //
            // dataGridView_Rounds
            //
            this.dataGridView_Rounds.AllowUserToAddRows = false;
            this.dataGridView_Rounds.AllowUserToDeleteRows = false;
            this.dataGridView_Rounds.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView_Rounds.BackgroundColor = System.Drawing.Color.FromArgb(45, 45, 48);
            this.dataGridView_Rounds.BorderStyle = System.Windows.Forms.BorderStyle.None;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.dataGridView_Rounds.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            this.dataGridView_Rounds.DefaultCellStyle = dataGridViewCellStyle2;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(58)))));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            this.dataGridView_Rounds.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView_Rounds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_Rounds.EnableHeadersVisualStyles = false;
            this.dataGridView_Rounds.GridColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.dataGridView_Rounds.Name = "dataGridView_Rounds";
            this.dataGridView_Rounds.ReadOnly = true;
            this.dataGridView_Rounds.RowHeadersVisible = false;
            this.dataGridView_Rounds.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //
            // panel_ChartBottom
            //
            this.panel_ChartBottom.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.panel_ChartBottom.Controls.Add(this.chart_Trend);
            this.panel_ChartBottom.Controls.Add(this.label_ChartTitle);
            this.panel_ChartBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_ChartBottom.Name = "panel_ChartBottom";
            //
            // label_ChartTitle
            //
            this.label_ChartTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.label_ChartTitle.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.label_ChartTitle.ForeColor = System.Drawing.Color.FromArgb(100, 200, 200);
            this.label_ChartTitle.Height = 30;
            this.label_ChartTitle.Name = "label_ChartTitle";
            this.label_ChartTitle.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.label_ChartTitle.Text = "趋势图表";
            this.label_ChartTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // chart_Trend
            //
            this.chart_Trend.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.chart_Trend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart_Trend.Name = "chart_Trend";
            //
            // ConsistencyTestDashboard
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.Controls.Add(this.splitContainer_Main);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.Name = "ConsistencyTestDashboard";
            this.Size = new System.Drawing.Size(1000, 600);
            this.splitContainer_Main.Panel1.ResumeLayout(false);
            this.splitContainer_Main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).EndInit();
            this.splitContainer_Main.ResumeLayout(false);
            this.panel_Left.ResumeLayout(false);
            this.panel_DatasetButtons.ResumeLayout(false);
            this.panel_Right.ResumeLayout(false);
            this.splitContainer_Right.Panel1.ResumeLayout(false);
            this.splitContainer_Right.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Right)).EndInit();
            this.splitContainer_Right.ResumeLayout(false);
            this.panel_RoundsTop.ResumeLayout(false);
            this.panel_TestProgress.ResumeLayout(false);
            this.panel_TestProgress.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Rounds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Rounds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart_Trend)).EndInit();
            this.panel_ChartBottom.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_Main;
        private System.Windows.Forms.Panel panel_Left;
        private System.Windows.Forms.Label label_DatasetTitle;
        private System.Windows.Forms.ListBox listBox_Datasets;
        private System.Windows.Forms.TableLayoutPanel panel_DatasetButtons;
        private DeepSightAI.StyledButton btn_RefreshDatasets;
        private DeepSightAI.StyledButton btn_DeleteDataset;
        private DeepSightAI.StyledButton btn_StartTest;
        private System.Windows.Forms.Label label_Rounds;
        private System.Windows.Forms.NumericUpDown numericUpDown_Rounds;
        private System.Windows.Forms.Panel panel_Right;
        private System.Windows.Forms.SplitContainer splitContainer_Right;
        private System.Windows.Forms.Panel panel_RoundsTop;
        private System.Windows.Forms.Label label_RoundsTitle;
        private System.Windows.Forms.Panel panel_TestProgress;
        private System.Windows.Forms.Label label_TestProgress;
        private System.Windows.Forms.ProgressBar progressBar_Test;
        private System.Windows.Forms.DataGridView dataGridView_Rounds;
        private System.Windows.Forms.Panel panel_ChartBottom;
        private System.Windows.Forms.Label label_ChartTitle;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_Trend;
    }
}


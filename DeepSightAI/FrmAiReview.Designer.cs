namespace DeepSightAI
{
    partial class FrmAiReview
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
            this.components = new System.ComponentModel.Container();
            this.panel_Main = new System.Windows.Forms.Panel();
            this.splitContainer_Main = new Sunny.UI.UISplitContainer();
            this.tableLayoutPanel_Left = new System.Windows.Forms.TableLayoutPanel();
            this.UcDefectQuery = new DeepSightAI.UcDefectQuery();
            this.panel_ReviewDetail = new System.Windows.Forms.Panel();
            this.label_ReviewDetail = new Sunny.UI.UIRichTextBox();
            this.tabControl_Main = new Sunny.UI.UITabControl();
            this.tabPage_Grid = new System.Windows.Forms.TabPage();
            this.ucDefectListPanel = new DeepSightAI.UcDefectListPanel();
            this.tabPage_Details = new System.Windows.Forms.TabPage();
            this.defectDetailControl1 = new DeepSightAI.UcDefectDetail();
            this.tabPage_Pareto = new System.Windows.Forms.TabPage();
            this.paretoChart1 = new DeepSightAI.UcParetoChart();
            this.tabPage_ValidationTest = new System.Windows.Forms.TabPage();
            this.validationTestResultControl1 = new DeepSightAI.UcValidationTestResult();
            this.tabPage_ConsistencyDashboard = new System.Windows.Forms.TabPage();
            this.consistencyTestDashboard1 = new DeepSightAI.UcConsistencyTestDashboard();
            this.tabPage_HeatMap = new System.Windows.Forms.TabPage();
            this.heatMapControl1 = new DeepSightAI.UcHeatMap();
            this.panel_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).BeginInit();
            this.splitContainer_Main.Panel1.SuspendLayout();
            this.splitContainer_Main.Panel2.SuspendLayout();
            this.splitContainer_Main.SuspendLayout();
            this.tableLayoutPanel_Left.SuspendLayout();
            this.panel_ReviewDetail.SuspendLayout();
            this.tabControl_Main.SuspendLayout();
            this.tabPage_Grid.SuspendLayout();
            this.tabPage_Details.SuspendLayout();
            this.tabPage_Pareto.SuspendLayout();
            this.tabPage_ValidationTest.SuspendLayout();
            this.tabPage_ConsistencyDashboard.SuspendLayout();
            this.tabPage_HeatMap.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_Main
            // 
            this.panel_Main.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel_Main.Controls.Add(this.splitContainer_Main);
            this.panel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Main.Location = new System.Drawing.Point(0, 0);
            this.panel_Main.Margin = new System.Windows.Forms.Padding(0);
            this.panel_Main.Name = "panel_Main";
            this.panel_Main.Size = new System.Drawing.Size(1924, 1088);
            this.panel_Main.TabIndex = 0;
            // 
            // splitContainer_Main
            // 
            this.splitContainer_Main.ArrowColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(190)))), ((int)(((byte)(200)))));
            this.splitContainer_Main.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer_Main.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_Main.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer_Main.HandleColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.splitContainer_Main.HandleHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.splitContainer_Main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_Main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_Main.MinimumSize = new System.Drawing.Size(20, 20);
            this.splitContainer_Main.Name = "splitContainer_Main";
            // 
            // splitContainer_Main.Panel1
            // 
            this.splitContainer_Main.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer_Main.Panel1.Controls.Add(this.tableLayoutPanel_Left);
            // 
            // splitContainer_Main.Panel2
            // 
            this.splitContainer_Main.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer_Main.Panel2.Controls.Add(this.tabControl_Main);
            this.splitContainer_Main.Size = new System.Drawing.Size(1924, 1088);
            this.splitContainer_Main.SplitterDistance = 363;
            this.splitContainer_Main.SplitterWidth = 11;
            this.splitContainer_Main.Style = Sunny.UI.UIStyle.Custom;
            this.splitContainer_Main.StyleCustomMode = true;
            this.splitContainer_Main.TabIndex = 3;
            // 
            // tableLayoutPanel_Left
            // 
            this.tableLayoutPanel_Left.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tableLayoutPanel_Left.ColumnCount = 1;
            this.tableLayoutPanel_Left.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_Left.Controls.Add(this.UcDefectQuery, 0, 0);
            this.tableLayoutPanel_Left.Controls.Add(this.panel_ReviewDetail, 0, 1);
            this.tableLayoutPanel_Left.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_Left.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_Left.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_Left.Name = "tableLayoutPanel_Left";
            this.tableLayoutPanel_Left.RowCount = 2;
            this.tableLayoutPanel_Left.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 360F));
            this.tableLayoutPanel_Left.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_Left.Size = new System.Drawing.Size(363, 1088);
            this.tableLayoutPanel_Left.TabIndex = 1;
            // 
            // UcDefectQuery
            // 
            this.UcDefectQuery.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.UcDefectQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UcDefectQuery.IsDateChecked = true;
            this.UcDefectQuery.Location = new System.Drawing.Point(0, 0);
            this.UcDefectQuery.LotNumber = "";
            this.UcDefectQuery.MachineID = "";
            this.UcDefectQuery.Margin = new System.Windows.Forms.Padding(0);
            this.UcDefectQuery.Name = "UcDefectQuery";
            this.UcDefectQuery.OnlyAviNg = false;
            this.UcDefectQuery.PartNumber = "";
            this.UcDefectQuery.SelectedDefectName = "";
            this.UcDefectQuery.SelectedSide = "";
            this.UcDefectQuery.Size = new System.Drawing.Size(363, 360);
            this.UcDefectQuery.TabIndex = 0;
            // 
            // panel_ReviewDetail
            // 
            this.panel_ReviewDetail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel_ReviewDetail.Controls.Add(this.label_ReviewDetail);
            this.panel_ReviewDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_ReviewDetail.Location = new System.Drawing.Point(0, 360);
            this.panel_ReviewDetail.Margin = new System.Windows.Forms.Padding(0);
            this.panel_ReviewDetail.Name = "panel_ReviewDetail";
            this.panel_ReviewDetail.Size = new System.Drawing.Size(363, 728);
            this.panel_ReviewDetail.TabIndex = 0;
            // 
            // label_ReviewDetail
            // 
            this.label_ReviewDetail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.label_ReviewDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_ReviewDetail.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.label_ReviewDetail.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.label_ReviewDetail.ForeColor = System.Drawing.Color.White;
            this.label_ReviewDetail.Location = new System.Drawing.Point(0, 0);
            this.label_ReviewDetail.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_ReviewDetail.MinimumSize = new System.Drawing.Size(1, 1);
            this.label_ReviewDetail.Name = "label_ReviewDetail";
            this.label_ReviewDetail.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.label_ReviewDetail.ReadOnly = true;
            this.label_ReviewDetail.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.label_ReviewDetail.ScrollBarBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.label_ReviewDetail.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.label_ReviewDetail.ScrollBarStyleInherited = false;
            this.label_ReviewDetail.ShowText = false;
            this.label_ReviewDetail.Size = new System.Drawing.Size(363, 728);
            this.label_ReviewDetail.Style = Sunny.UI.UIStyle.Custom;
            this.label_ReviewDetail.StyleCustomMode = true;
            this.label_ReviewDetail.TabIndex = 0;
            this.label_ReviewDetail.Text = "复判详情区";
            this.label_ReviewDetail.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabControl_Main
            // 
            this.tabControl_Main.Controls.Add(this.tabPage_Grid);
            this.tabControl_Main.Controls.Add(this.tabPage_Details);
            this.tabControl_Main.Controls.Add(this.tabPage_Pareto);
            this.tabControl_Main.Controls.Add(this.tabPage_ValidationTest);
            this.tabControl_Main.Controls.Add(this.tabPage_ConsistencyDashboard);
            this.tabControl_Main.Controls.Add(this.tabPage_HeatMap);
            this.tabControl_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_Main.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl_Main.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tabControl_Main.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.tabControl_Main.ItemSize = new System.Drawing.Size(120, 30);
            this.tabControl_Main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_Main.MainPage = "";
            this.tabControl_Main.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl_Main.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this.tabControl_Main.Name = "tabControl_Main";
            this.tabControl_Main.SelectedIndex = 0;
            this.tabControl_Main.Size = new System.Drawing.Size(1550, 1088);
            this.tabControl_Main.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl_Main.Style = Sunny.UI.UIStyle.Custom;
            this.tabControl_Main.StyleCustomMode = true;
            this.tabControl_Main.TabBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tabControl_Main.TabIndex = 2;
            this.tabControl_Main.TabSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.tabControl_Main.TabSelectedForeColor = System.Drawing.Color.White;
            this.tabControl_Main.TabSelectedHighColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.tabControl_Main.TabUnSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tabControl_Main.TabUnSelectedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.tabControl_Main.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            //
            // tabPage_Grid
            //
            this.tabPage_Grid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tabPage_Grid.Controls.Add(this.ucDefectListPanel);
            this.tabPage_Grid.Location = new System.Drawing.Point(0, 30);
            this.tabPage_Grid.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_Grid.Name = "tabPage_Grid";
            this.tabPage_Grid.Size = new System.Drawing.Size(1550, 1058);
            this.tabPage_Grid.TabIndex = 0;
            this.tabPage_Grid.Text = "缺陷列表";
            //
            // ucDefectListPanel
            //
            this.ucDefectListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucDefectListPanel.Location = new System.Drawing.Point(0, 0);
            this.ucDefectListPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ucDefectListPanel.Name = "ucDefectListPanel";
            this.ucDefectListPanel.Size = new System.Drawing.Size(1550, 1058);
            this.ucDefectListPanel.TabIndex = 0;
            //
            // tabPage_Details
            // 
            this.tabPage_Details.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tabPage_Details.Controls.Add(this.defectDetailControl1);
            this.tabPage_Details.Location = new System.Drawing.Point(0, 30);
            this.tabPage_Details.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_Details.Name = "tabPage_Details";
            this.tabPage_Details.Size = new System.Drawing.Size(1550, 1058);
            this.tabPage_Details.TabIndex = 1;
            this.tabPage_Details.Text = "缺陷详情";
            // 
            // defectDetailControl1
            // 
            this.defectDetailControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.defectDetailControl1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.defectDetailControl1.Location = new System.Drawing.Point(0, 0);
            this.defectDetailControl1.Margin = new System.Windows.Forms.Padding(0);
            this.defectDetailControl1.Name = "defectDetailControl1";
            this.defectDetailControl1.Size = new System.Drawing.Size(1550, 1058);
            this.defectDetailControl1.TabIndex = 0;
            // 
            // tabPage_Pareto
            // 
            this.tabPage_Pareto.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tabPage_Pareto.Controls.Add(this.paretoChart1);
            this.tabPage_Pareto.Location = new System.Drawing.Point(0, 30);
            this.tabPage_Pareto.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_Pareto.Name = "tabPage_Pareto";
            this.tabPage_Pareto.Size = new System.Drawing.Size(1550, 1058);
            this.tabPage_Pareto.TabIndex = 5;
            this.tabPage_Pareto.Text = "帕累托图";
            // 
            // paretoChart1
            // 
            this.paretoChart1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.paretoChart1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.paretoChart1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.paretoChart1.Location = new System.Drawing.Point(0, 0);
            this.paretoChart1.Margin = new System.Windows.Forms.Padding(0);
            this.paretoChart1.Name = "paretoChart1";
            this.paretoChart1.Size = new System.Drawing.Size(1550, 1058);
            this.paretoChart1.TabIndex = 0;
            // 
            // tabPage_ValidationTest
            // 
            this.tabPage_ValidationTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tabPage_ValidationTest.Controls.Add(this.validationTestResultControl1);
            this.tabPage_ValidationTest.Location = new System.Drawing.Point(0, 30);
            this.tabPage_ValidationTest.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_ValidationTest.Name = "tabPage_ValidationTest";
            this.tabPage_ValidationTest.Size = new System.Drawing.Size(1550, 1058);
            this.tabPage_ValidationTest.TabIndex = 2;
            this.tabPage_ValidationTest.Text = "模型一致性测试";
            // 
            // validationTestResultControl1
            // 
            this.validationTestResultControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.validationTestResultControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.validationTestResultControl1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.validationTestResultControl1.Location = new System.Drawing.Point(0, 0);
            this.validationTestResultControl1.Margin = new System.Windows.Forms.Padding(0);
            this.validationTestResultControl1.Name = "validationTestResultControl1";
            this.validationTestResultControl1.Size = new System.Drawing.Size(1550, 1058);
            this.validationTestResultControl1.TabIndex = 0;
            // 
            // tabPage_ConsistencyDashboard
            // 
            this.tabPage_ConsistencyDashboard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tabPage_ConsistencyDashboard.Controls.Add(this.consistencyTestDashboard1);
            this.tabPage_ConsistencyDashboard.Location = new System.Drawing.Point(0, 30);
            this.tabPage_ConsistencyDashboard.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_ConsistencyDashboard.Name = "tabPage_ConsistencyDashboard";
            this.tabPage_ConsistencyDashboard.Size = new System.Drawing.Size(1550, 1058);
            this.tabPage_ConsistencyDashboard.TabIndex = 3;
            this.tabPage_ConsistencyDashboard.Text = "一致性测试看板";
            // 
            // consistencyTestDashboard1
            // 
            this.consistencyTestDashboard1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.consistencyTestDashboard1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consistencyTestDashboard1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.consistencyTestDashboard1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.consistencyTestDashboard1.Location = new System.Drawing.Point(0, 0);
            this.consistencyTestDashboard1.Margin = new System.Windows.Forms.Padding(0);
            this.consistencyTestDashboard1.Name = "consistencyTestDashboard1";
            this.consistencyTestDashboard1.Size = new System.Drawing.Size(1550, 1058);
            this.consistencyTestDashboard1.TabIndex = 0;
            // 
            // tabPage_HeatMap
            // 
            this.tabPage_HeatMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tabPage_HeatMap.Controls.Add(this.heatMapControl1);
            this.tabPage_HeatMap.Location = new System.Drawing.Point(0, 30);
            this.tabPage_HeatMap.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_HeatMap.Name = "tabPage_HeatMap";
            this.tabPage_HeatMap.Size = new System.Drawing.Size(1550, 1058);
            this.tabPage_HeatMap.TabIndex = 4;
            this.tabPage_HeatMap.Text = "热力图";
            // 
            // heatMapControl1
            // 
            this.heatMapControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heatMapControl1.Location = new System.Drawing.Point(0, 0);
            this.heatMapControl1.Margin = new System.Windows.Forms.Padding(0);
            this.heatMapControl1.Name = "heatMapControl1";
            this.heatMapControl1.offsetX = 0;
            this.heatMapControl1.offsetY = 0;
            this.heatMapControl1.Size = new System.Drawing.Size(1550, 1058);
            this.heatMapControl1.TabIndex = 0;
            //
            // FrmAiReview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.ClientSize = new System.Drawing.Size(1924, 1088);
            this.Controls.Add(this.panel_Main);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "FrmAiReview";
            this.Text = "UcAiReview";
            this.panel_Main.ResumeLayout(false);
            this.splitContainer_Main.Panel1.ResumeLayout(false);
            this.splitContainer_Main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).EndInit();
            this.splitContainer_Main.ResumeLayout(false);
            this.tableLayoutPanel_Left.ResumeLayout(false);
            this.panel_ReviewDetail.ResumeLayout(false);
            this.tabControl_Main.ResumeLayout(false);
            this.tabPage_Grid.ResumeLayout(false);
            this.tabPage_Details.ResumeLayout(false);
            this.tabPage_Pareto.ResumeLayout(false);
            this.tabPage_ValidationTest.ResumeLayout(false);
            this.tabPage_ConsistencyDashboard.ResumeLayout(false);
            this.tabPage_HeatMap.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_Main;
        private UcDefectQuery UcDefectQuery;
        private UcDefectDetail defectDetailControl1;
        private Sunny.UI.UITabControl tabControl_Main;
        private System.Windows.Forms.TabPage tabPage_Grid;
        private System.Windows.Forms.TabPage tabPage_Details;
        private Sunny.UI.UISplitContainer splitContainer_Main;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Left;
        private System.Windows.Forms.Panel panel_ReviewDetail;
        private Sunny.UI.UIRichTextBox label_ReviewDetail;
        private System.Windows.Forms.TabPage tabPage_ValidationTest;
        private UcValidationTestResult validationTestResultControl1;
        private System.Windows.Forms.TabPage tabPage_ConsistencyDashboard;
        private UcConsistencyTestDashboard consistencyTestDashboard1;
        private System.Windows.Forms.TabPage tabPage_HeatMap;
        private UcHeatMap heatMapControl1;
        private System.Windows.Forms.TabPage tabPage_Pareto;
        private UcParetoChart paretoChart1;
        private UcDefectListPanel ucDefectListPanel;
    }
}

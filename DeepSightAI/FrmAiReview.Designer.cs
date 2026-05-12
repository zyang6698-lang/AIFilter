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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel_Main = new System.Windows.Forms.Panel();
            this.splitContainer_Main = new Sunny.UI.UISplitContainer();
            this.tableLayoutPanel_Left = new System.Windows.Forms.TableLayoutPanel();
            this.UcDefectQuery = new DeepSightAI.UcDefectQuery();
            this.panel_ReviewDetail = new System.Windows.Forms.Panel();
            this.label_ReviewDetail = new Sunny.UI.UIRichTextBox();
            this.tabControl_Main = new Sunny.UI.UITabControl();
            this.tabPage_Grid = new System.Windows.Forms.TabPage();
            this.panel_Grid = new System.Windows.Forms.Panel();
            this.dataGridView_Defects = new Sunny.UI.UIDataGridView();
            this.col_SN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Lot = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_MachineId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_ProductSerial = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Side = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_AiStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_ManualStatus = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.col_VrsStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_DefectCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_PathIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_DetectionDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitter_LotGrid = new System.Windows.Forms.Splitter();
            this.panel_LotList = new System.Windows.Forms.Panel();
            this.treeView_Lots = new System.Windows.Forms.TreeView();
            this.chk_OnlyAviNg = new System.Windows.Forms.CheckBox();
            this.panel_SnSearch = new System.Windows.Forms.Panel();
            this.btn_SnSearch = new DeepSightAI.StyledButton();
            this.txt_SnFilter = new System.Windows.Forms.TextBox();
            this.label_LotTitle = new System.Windows.Forms.Label();
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
            this.contextMenuStrip_Lot = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_AddToDataset = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_RunTest = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_SecondaryInference = new System.Windows.Forms.ToolStripMenuItem();
            this.panel_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).BeginInit();
            this.splitContainer_Main.Panel1.SuspendLayout();
            this.splitContainer_Main.Panel2.SuspendLayout();
            this.splitContainer_Main.SuspendLayout();
            this.tableLayoutPanel_Left.SuspendLayout();
            this.panel_ReviewDetail.SuspendLayout();
            this.tabControl_Main.SuspendLayout();
            this.tabPage_Grid.SuspendLayout();
            this.panel_Grid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Defects)).BeginInit();
            this.panel_LotList.SuspendLayout();
            this.panel_SnSearch.SuspendLayout();
            this.tabPage_Details.SuspendLayout();
            this.tabPage_Pareto.SuspendLayout();
            this.tabPage_ValidationTest.SuspendLayout();
            this.tabPage_ConsistencyDashboard.SuspendLayout();
            this.tabPage_HeatMap.SuspendLayout();
            this.contextMenuStrip_Lot.SuspendLayout();
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
            this.tableLayoutPanel_Left.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 315F));
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
            this.UcDefectQuery.PartNumber = "";
            this.UcDefectQuery.SelectedDefectName = "";
            this.UcDefectQuery.SelectedSide = "";
            this.UcDefectQuery.Size = new System.Drawing.Size(363, 315);
            this.UcDefectQuery.TabIndex = 0;
            // 
            // panel_ReviewDetail
            // 
            this.panel_ReviewDetail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel_ReviewDetail.Controls.Add(this.label_ReviewDetail);
            this.panel_ReviewDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_ReviewDetail.Location = new System.Drawing.Point(0, 315);
            this.panel_ReviewDetail.Margin = new System.Windows.Forms.Padding(0);
            this.panel_ReviewDetail.Name = "panel_ReviewDetail";
            this.panel_ReviewDetail.Size = new System.Drawing.Size(363, 773);
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
            this.label_ReviewDetail.Size = new System.Drawing.Size(363, 773);
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
            this.tabPage_Grid.Controls.Add(this.panel_Grid);
            this.tabPage_Grid.Location = new System.Drawing.Point(0, 30);
            this.tabPage_Grid.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_Grid.Name = "tabPage_Grid";
            this.tabPage_Grid.Size = new System.Drawing.Size(1550, 1058);
            this.tabPage_Grid.TabIndex = 0;
            this.tabPage_Grid.Text = "缺陷列表";
            // 
            // panel_Grid
            // 
            this.panel_Grid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel_Grid.Controls.Add(this.dataGridView_Defects);
            this.panel_Grid.Controls.Add(this.splitter_LotGrid);
            this.panel_Grid.Controls.Add(this.panel_LotList);
            this.panel_Grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Grid.Location = new System.Drawing.Point(0, 0);
            this.panel_Grid.Margin = new System.Windows.Forms.Padding(0);
            this.panel_Grid.Name = "panel_Grid";
            this.panel_Grid.Size = new System.Drawing.Size(1550, 1058);
            this.panel_Grid.TabIndex = 1;
            // 
            // dataGridView_Defects
            // 
            this.dataGridView_Defects.AllowUserToAddRows = false;
            this.dataGridView_Defects.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            this.dataGridView_Defects.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView_Defects.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView_Defects.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dataGridView_Defects.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView_Defects.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_Defects.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView_Defects.ColumnHeadersHeight = 32;
            this.dataGridView_Defects.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView_Defects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_SN,
            this.col_Lot,
            this.col_MachineId,
            this.col_ProductSerial,
            this.col_Side,
            this.col_AiStatus,
            this.col_ManualStatus,
            this.col_VrsStatus,
            this.col_DefectCount,
            this.col_PathIndex,
            this.col_DetectionDate});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView_Defects.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView_Defects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_Defects.EnableHeadersVisualStyles = false;
            this.dataGridView_Defects.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dataGridView_Defects.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.dataGridView_Defects.Location = new System.Drawing.Point(272, 0);
            this.dataGridView_Defects.Margin = new System.Windows.Forms.Padding(0);
            this.dataGridView_Defects.MultiSelect = false;
            this.dataGridView_Defects.Name = "dataGridView_Defects";
            this.dataGridView_Defects.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_Defects.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridView_Defects.RowHeadersVisible = false;
            this.dataGridView_Defects.RowHeadersWidth = 51;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dataGridView_Defects.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridView_Defects.RowTemplate.Height = 23;
            this.dataGridView_Defects.ScrollBarBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dataGridView_Defects.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.dataGridView_Defects.ScrollBarRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dataGridView_Defects.ScrollBarStyleInherited = false;
            this.dataGridView_Defects.SelectedIndex = -1;
            this.dataGridView_Defects.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView_Defects.Size = new System.Drawing.Size(1278, 1058);
            this.dataGridView_Defects.StripeEvenColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dataGridView_Defects.StripeOddColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            this.dataGridView_Defects.Style = Sunny.UI.UIStyle.Custom;
            this.dataGridView_Defects.StyleCustomMode = true;
            this.dataGridView_Defects.TabIndex = 0;
            // 
            // col_SN
            // 
            this.col_SN.DataPropertyName = "SerialNumber";
            this.col_SN.HeaderText = "序列号";
            this.col_SN.MinimumWidth = 6;
            this.col_SN.Name = "col_SN";
            this.col_SN.ReadOnly = true;
            // 
            // col_Lot
            // 
            this.col_Lot.DataPropertyName = "LotNumber";
            this.col_Lot.HeaderText = "Lot号";
            this.col_Lot.MinimumWidth = 6;
            this.col_Lot.Name = "col_Lot";
            this.col_Lot.ReadOnly = true;
            // 
            // col_MachineId
            // 
            this.col_MachineId.DataPropertyName = "MachineId";
            this.col_MachineId.HeaderText = "机台号";
            this.col_MachineId.MinimumWidth = 6;
            this.col_MachineId.Name = "col_MachineId";
            this.col_MachineId.ReadOnly = true;
            // 
            // col_ProductSerial
            // 
            this.col_ProductSerial.DataPropertyName = "ProductSerial";
            this.col_ProductSerial.HeaderText = "料号";
            this.col_ProductSerial.MinimumWidth = 6;
            this.col_ProductSerial.Name = "col_ProductSerial";
            this.col_ProductSerial.ReadOnly = true;
            // 
            // col_Side
            // 
            this.col_Side.DataPropertyName = "Side";
            this.col_Side.HeaderText = "面次";
            this.col_Side.MinimumWidth = 6;
            this.col_Side.Name = "col_Side";
            this.col_Side.ReadOnly = true;
            // 
            // col_AiStatus
            // 
            this.col_AiStatus.DataPropertyName = "AiStatus";
            this.col_AiStatus.HeaderText = "AI状态";
            this.col_AiStatus.MinimumWidth = 6;
            this.col_AiStatus.Name = "col_AiStatus";
            this.col_AiStatus.ReadOnly = true;
            // 
            // col_ManualStatus
            // 
            this.col_ManualStatus.DataPropertyName = "ManualStatus";
            this.col_ManualStatus.HeaderText = "人工判定";
            this.col_ManualStatus.Items.AddRange(new object[] {
            "未判定",
            "OK",
            "NG"});
            this.col_ManualStatus.MinimumWidth = 6;
            this.col_ManualStatus.Name = "col_ManualStatus";
            this.col_ManualStatus.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.col_ManualStatus.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // col_VrsStatus
            // 
            this.col_VrsStatus.DataPropertyName = "VrsStatus";
            this.col_VrsStatus.HeaderText = "VRS状态";
            this.col_VrsStatus.MinimumWidth = 6;
            this.col_VrsStatus.Name = "col_VrsStatus";
            this.col_VrsStatus.ReadOnly = true;
            // 
            // col_DefectCount
            // 
            this.col_DefectCount.DataPropertyName = "DefectCount";
            this.col_DefectCount.HeaderText = "缺陷数";
            this.col_DefectCount.MinimumWidth = 6;
            this.col_DefectCount.Name = "col_DefectCount";
            this.col_DefectCount.ReadOnly = true;
            // 
            // col_PathIndex
            // 
            this.col_PathIndex.DataPropertyName = "PathIndex";
            this.col_PathIndex.HeaderText = "路径索引";
            this.col_PathIndex.MinimumWidth = 6;
            this.col_PathIndex.Name = "col_PathIndex";
            this.col_PathIndex.ReadOnly = true;
            this.col_PathIndex.Visible = false;
            // 
            // col_DetectionDate
            // 
            this.col_DetectionDate.DataPropertyName = "DetectionDate";
            this.col_DetectionDate.HeaderText = "检测日期";
            this.col_DetectionDate.MinimumWidth = 6;
            this.col_DetectionDate.Name = "col_DetectionDate";
            this.col_DetectionDate.ReadOnly = true;
            // 
            // splitter_LotGrid
            // 
            this.splitter_LotGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitter_LotGrid.Location = new System.Drawing.Point(267, 0);
            this.splitter_LotGrid.Margin = new System.Windows.Forms.Padding(4);
            this.splitter_LotGrid.Name = "splitter_LotGrid";
            this.splitter_LotGrid.Size = new System.Drawing.Size(5, 1058);
            this.splitter_LotGrid.TabIndex = 3;
            this.splitter_LotGrid.TabStop = false;
            // 
            // panel_LotList
            // 
            this.panel_LotList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel_LotList.Controls.Add(this.treeView_Lots);
            this.panel_LotList.Controls.Add(this.chk_OnlyAviNg);
            this.panel_LotList.Controls.Add(this.panel_SnSearch);
            this.panel_LotList.Controls.Add(this.label_LotTitle);
            this.panel_LotList.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel_LotList.Location = new System.Drawing.Point(0, 0);
            this.panel_LotList.Margin = new System.Windows.Forms.Padding(4);
            this.panel_LotList.Name = "panel_LotList";
            this.panel_LotList.Size = new System.Drawing.Size(267, 1058);
            this.panel_LotList.TabIndex = 2;
            // 
            // treeView_Lots
            // 
            this.treeView_Lots.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.treeView_Lots.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView_Lots.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView_Lots.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.treeView_Lots.ForeColor = System.Drawing.Color.White;
            this.treeView_Lots.FullRowSelect = true;
            this.treeView_Lots.HideSelection = false;
            this.treeView_Lots.Location = new System.Drawing.Point(0, 97);
            this.treeView_Lots.Margin = new System.Windows.Forms.Padding(4);
            this.treeView_Lots.Name = "treeView_Lots";
            this.treeView_Lots.Size = new System.Drawing.Size(267, 961);
            this.treeView_Lots.TabIndex = 1;
            // 
            // chk_OnlyAviNg
            // 
            this.chk_OnlyAviNg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.chk_OnlyAviNg.Dock = System.Windows.Forms.DockStyle.Top;
            this.chk_OnlyAviNg.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.chk_OnlyAviNg.ForeColor = System.Drawing.Color.White;
            this.chk_OnlyAviNg.Location = new System.Drawing.Point(0, 69);
            this.chk_OnlyAviNg.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chk_OnlyAviNg.Name = "chk_OnlyAviNg";
            this.chk_OnlyAviNg.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.chk_OnlyAviNg.Size = new System.Drawing.Size(267, 28);
            this.chk_OnlyAviNg.TabIndex = 3;
            this.chk_OnlyAviNg.Text = "仅显示 AVI NG 数据";
            this.chk_OnlyAviNg.UseVisualStyleBackColor = false;
            // 
            // panel_SnSearch
            // 
            this.panel_SnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel_SnSearch.Controls.Add(this.btn_SnSearch);
            this.panel_SnSearch.Controls.Add(this.txt_SnFilter);
            this.panel_SnSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_SnSearch.Location = new System.Drawing.Point(0, 31);
            this.panel_SnSearch.Margin = new System.Windows.Forms.Padding(4);
            this.panel_SnSearch.Name = "panel_SnSearch";
            this.panel_SnSearch.Padding = new System.Windows.Forms.Padding(4);
            this.panel_SnSearch.Size = new System.Drawing.Size(267, 38);
            this.panel_SnSearch.TabIndex = 2;
            // 
            // btn_SnSearch
            // 
            this.btn_SnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btn_SnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_SnSearch.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_SnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_SnSearch.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btn_SnSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btn_SnSearch.Location = new System.Drawing.Point(212, 4);
            this.btn_SnSearch.Margin = new System.Windows.Forms.Padding(4);
            this.btn_SnSearch.Name = "btn_SnSearch";
            this.btn_SnSearch.Size = new System.Drawing.Size(51, 30);
            this.btn_SnSearch.TabIndex = 1;
            this.btn_SnSearch.Text = "搜索";
            this.btn_SnSearch.UseVisualStyleBackColor = false;
            // 
            // txt_SnFilter
            // 
            this.txt_SnFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            this.txt_SnFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txt_SnFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_SnFilter.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txt_SnFilter.ForeColor = System.Drawing.Color.White;
            this.txt_SnFilter.Location = new System.Drawing.Point(4, 4);
            this.txt_SnFilter.Margin = new System.Windows.Forms.Padding(4);
            this.txt_SnFilter.Name = "txt_SnFilter";
            this.txt_SnFilter.Size = new System.Drawing.Size(259, 27);
            this.txt_SnFilter.TabIndex = 0;
            // 
            // label_LotTitle
            // 
            this.label_LotTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.label_LotTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.label_LotTitle.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.label_LotTitle.ForeColor = System.Drawing.Color.White;
            this.label_LotTitle.Location = new System.Drawing.Point(0, 0);
            this.label_LotTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_LotTitle.Name = "label_LotTitle";
            this.label_LotTitle.Size = new System.Drawing.Size(267, 31);
            this.label_LotTitle.TabIndex = 0;
            this.label_LotTitle.Text = "Lot 分组 (点击展开)";
            this.label_LotTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            // contextMenuStrip_Lot
            // 
            this.contextMenuStrip_Lot.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip_Lot.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_AddToDataset,
            this.toolStripMenuItem_RunTest,
            this.toolStripMenuItem_SecondaryInference});
            this.contextMenuStrip_Lot.Name = "contextMenuStrip_Lot";
            this.contextMenuStrip_Lot.Size = new System.Drawing.Size(244, 76);
            // 
            // toolStripMenuItem_AddToDataset
            // 
            this.toolStripMenuItem_AddToDataset.Name = "toolStripMenuItem_AddToDataset";
            this.toolStripMenuItem_AddToDataset.Size = new System.Drawing.Size(243, 24);
            this.toolStripMenuItem_AddToDataset.Text = "添加到一致性测试数据集";
            // 
            // toolStripMenuItem_RunTest
            // 
            this.toolStripMenuItem_RunTest.Name = "toolStripMenuItem_RunTest";
            this.toolStripMenuItem_RunTest.Size = new System.Drawing.Size(243, 24);
            this.toolStripMenuItem_RunTest.Text = "运行模型一致性测试";
            // 
            // toolStripMenuItem_SecondaryInference
            // 
            this.toolStripMenuItem_SecondaryInference.Name = "toolStripMenuItem_SecondaryInference";
            this.toolStripMenuItem_SecondaryInference.Size = new System.Drawing.Size(243, 24);
            this.toolStripMenuItem_SecondaryInference.Text = "运行二次推理";
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
            this.panel_Grid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Defects)).EndInit();
            this.panel_LotList.ResumeLayout(false);
            this.panel_SnSearch.ResumeLayout(false);
            this.panel_SnSearch.PerformLayout();
            this.tabPage_Details.ResumeLayout(false);
            this.tabPage_Pareto.ResumeLayout(false);
            this.tabPage_ValidationTest.ResumeLayout(false);
            this.tabPage_ConsistencyDashboard.ResumeLayout(false);
            this.tabPage_HeatMap.ResumeLayout(false);
            this.contextMenuStrip_Lot.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_Main;
        private System.Windows.Forms.Panel panel_Grid;
        private Sunny.UI.UIDataGridView dataGridView_Defects;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_SN;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Lot;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_MachineId;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_ProductSerial;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Side;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_AiStatus;
        private System.Windows.Forms.DataGridViewComboBoxColumn col_ManualStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_VrsStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_DefectCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_PathIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_DetectionDate;
        private UcDefectQuery UcDefectQuery;
        private UcDefectDetail defectDetailControl1;
        private Sunny.UI.UITabControl tabControl_Main;
        private System.Windows.Forms.TabPage tabPage_Grid;
        private System.Windows.Forms.TabPage tabPage_Details;
        private Sunny.UI.UISplitContainer splitContainer_Main;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Left;
        private System.Windows.Forms.Panel panel_LotList;
        private System.Windows.Forms.Label label_LotTitle;
        private System.Windows.Forms.TreeView treeView_Lots;
        private System.Windows.Forms.Splitter splitter_LotGrid;
        private System.Windows.Forms.Panel panel_SnSearch;
        private System.Windows.Forms.TextBox txt_SnFilter;
        private DeepSightAI.StyledButton btn_SnSearch;
        private System.Windows.Forms.Panel panel_ReviewDetail;
        private Sunny.UI.UIRichTextBox label_ReviewDetail;
        private System.Windows.Forms.TabPage tabPage_ValidationTest;
        private UcValidationTestResult validationTestResultControl1;
        private System.Windows.Forms.TabPage tabPage_ConsistencyDashboard;
        private UcConsistencyTestDashboard consistencyTestDashboard1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_Lot;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_RunTest;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_SecondaryInference;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_AddToDataset;
        private System.Windows.Forms.CheckBox chk_OnlyAviNg;
        private System.Windows.Forms.TabPage tabPage_HeatMap;
        private UcHeatMap heatMapControl1;
        private System.Windows.Forms.TabPage tabPage_Pareto;
        private UcParetoChart paretoChart1;
    }
}

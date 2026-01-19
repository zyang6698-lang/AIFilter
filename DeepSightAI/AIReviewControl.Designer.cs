namespace DeepSightAI
{
    partial class AIReviewControl
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
            this.panel_Main = new System.Windows.Forms.Panel();
            this.splitContainer_Main = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.QueryControl = new DeepSightAI.QueryControl();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.btn_Save = new System.Windows.Forms.Button();
            this.btn_LoadImages = new System.Windows.Forms.Button();
            this.btn_Export = new System.Windows.Forms.Button();
            this.panel_ReviewDetail = new System.Windows.Forms.Panel();
            this.label_ReviewDetail = new System.Windows.Forms.Label();
            this.tabControl_Main = new System.Windows.Forms.TabControl();
            this.tabPage_Grid = new System.Windows.Forms.TabPage();
            this.panel_Grid = new System.Windows.Forms.Panel();
            this.dataGridView_Defects = new System.Windows.Forms.DataGridView();
            this.col_SN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Lot = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_MachineId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_ProductSerial = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Side = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_AiStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_ManualStatus = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.col_DefectCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_PathIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_DetectionDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitter_LotGrid = new System.Windows.Forms.Splitter();
            this.panel_LotList = new System.Windows.Forms.Panel();
            this.treeView_Lots = new System.Windows.Forms.TreeView();
            this.panel_SnSearch = new System.Windows.Forms.Panel();
            this.btn_SnSearch = new System.Windows.Forms.Button();
            this.txt_SnFilter = new System.Windows.Forms.TextBox();
            this.label_LotTitle = new System.Windows.Forms.Label();
            this.tabPage_Details = new System.Windows.Forms.TabPage();
            this.defectDetailControl1 = new DeepSightAI.DefectDetailControl();
            this.tabPage_ValidationTest = new System.Windows.Forms.TabPage();
            this.validationTestResultControl1 = new DeepSightAI.ValidationTestResultControl();
            this.contextMenuStrip_Lot = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_RunTest = new System.Windows.Forms.ToolStripMenuItem();
            this.panel_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).BeginInit();
            this.splitContainer_Main.Panel1.SuspendLayout();
            this.splitContainer_Main.Panel2.SuspendLayout();
            this.splitContainer_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panel_ReviewDetail.SuspendLayout();
            this.tabControl_Main.SuspendLayout();
            this.tabPage_Grid.SuspendLayout();
            this.panel_Grid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Defects)).BeginInit();
            this.panel_LotList.SuspendLayout();
            this.panel_SnSearch.SuspendLayout();
            this.tabPage_Details.SuspendLayout();
            this.tabPage_ValidationTest.SuspendLayout();
            this.contextMenuStrip_Lot.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_Main
            // 
            this.panel_Main.Controls.Add(this.splitContainer_Main);
            this.panel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Main.Location = new System.Drawing.Point(0, 0);
            this.panel_Main.Margin = new System.Windows.Forms.Padding(5);
            this.panel_Main.Name = "panel_Main";
            this.panel_Main.Size = new System.Drawing.Size(1924, 1087);
            this.panel_Main.TabIndex = 0;
            // 
            // splitContainer_Main
            // 
            this.splitContainer_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_Main.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer_Main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_Main.Margin = new System.Windows.Forms.Padding(5);
            this.splitContainer_Main.Name = "splitContainer_Main";
            // 
            // splitContainer_Main.Panel1
            // 
            this.splitContainer_Main.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer_Main.Panel2
            // 
            this.splitContainer_Main.Panel2.Controls.Add(this.tabControl_Main);
            this.splitContainer_Main.Size = new System.Drawing.Size(1924, 1087);
            this.splitContainer_Main.SplitterDistance = 363;
            this.splitContainer_Main.SplitterWidth = 7;
            this.splitContainer_Main.TabIndex = 3;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.QueryControl);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(363, 1087);
            this.splitContainer1.SplitterDistance = 504;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 1;
            // 
            // QueryControl
            // 
            this.QueryControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.QueryControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.QueryControl.IsDateChecked = true;
            this.QueryControl.Location = new System.Drawing.Point(0, 0);
            this.QueryControl.LotNumber = "";
            this.QueryControl.MachineID = "";
            this.QueryControl.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.QueryControl.Name = "QueryControl";
            this.QueryControl.PartNumber = "";
            this.QueryControl.SelectedSide = "A";
            this.QueryControl.Size = new System.Drawing.Size(363, 504);
            this.QueryControl.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.btn_Save);
            this.splitContainer2.Panel1.Controls.Add(this.btn_LoadImages);
            this.splitContainer2.Panel1.Controls.Add(this.btn_Export);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panel_ReviewDetail);
            this.splitContainer2.Size = new System.Drawing.Size(363, 578);
            this.splitContainer2.SplitterDistance = 73;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 0;
            // 
            // btn_Save
            // 
            this.btn_Save.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btn_Save.FlatAppearance.BorderSize = 0;
            this.btn_Save.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Save.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btn_Save.Location = new System.Drawing.Point(25, 11);
            this.btn_Save.Margin = new System.Windows.Forms.Padding(5);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(107, 38);
            this.btn_Save.TabIndex = 0;
            this.btn_Save.Text = "保存";
            this.btn_Save.UseVisualStyleBackColor = false;
            this.btn_Save.Click += new System.EventHandler(this.Btn_Save_Click);
            // 
            // btn_LoadImages
            // 
            this.btn_LoadImages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btn_LoadImages.FlatAppearance.BorderSize = 0;
            this.btn_LoadImages.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_LoadImages.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btn_LoadImages.Location = new System.Drawing.Point(140, 11);
            this.btn_LoadImages.Margin = new System.Windows.Forms.Padding(4);
            this.btn_LoadImages.Name = "btn_LoadImages";
            this.btn_LoadImages.Size = new System.Drawing.Size(107, 38);
            this.btn_LoadImages.TabIndex = 2;
            this.btn_LoadImages.Text = "读图";
            this.btn_LoadImages.UseVisualStyleBackColor = false;
            this.btn_LoadImages.Click += new System.EventHandler(this.Btn_LoadImages_Click);
            // 
            // btn_Export
            // 
            this.btn_Export.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btn_Export.FlatAppearance.BorderSize = 0;
            this.btn_Export.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Export.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btn_Export.Location = new System.Drawing.Point(256, 11);
            this.btn_Export.Margin = new System.Windows.Forms.Padding(5);
            this.btn_Export.Name = "btn_Export";
            this.btn_Export.Size = new System.Drawing.Size(107, 38);
            this.btn_Export.TabIndex = 1;
            this.btn_Export.Text = "导出";
            this.btn_Export.UseVisualStyleBackColor = false;
            this.btn_Export.Click += new System.EventHandler(this.Btn_Export_Click);
            // 
            // panel_ReviewDetail
            // 
            this.panel_ReviewDetail.Controls.Add(this.label_ReviewDetail);
            this.panel_ReviewDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_ReviewDetail.Location = new System.Drawing.Point(0, 0);
            this.panel_ReviewDetail.Margin = new System.Windows.Forms.Padding(4);
            this.panel_ReviewDetail.Name = "panel_ReviewDetail";
            this.panel_ReviewDetail.Size = new System.Drawing.Size(363, 500);
            this.panel_ReviewDetail.TabIndex = 0;
            // 
            // label_ReviewDetail
            // 
            this.label_ReviewDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_ReviewDetail.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.label_ReviewDetail.ForeColor = System.Drawing.Color.White;
            this.label_ReviewDetail.Location = new System.Drawing.Point(0, 0);
            this.label_ReviewDetail.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_ReviewDetail.Name = "label_ReviewDetail";
            this.label_ReviewDetail.Size = new System.Drawing.Size(363, 500);
            this.label_ReviewDetail.TabIndex = 0;
            this.label_ReviewDetail.Text = "复判详情区";
            this.label_ReviewDetail.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabControl_Main
            // 
            this.tabControl_Main.Controls.Add(this.tabPage_Grid);
            this.tabControl_Main.Controls.Add(this.tabPage_Details);
            this.tabControl_Main.Controls.Add(this.tabPage_ValidationTest);
            this.tabControl_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_Main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_Main.Margin = new System.Windows.Forms.Padding(5);
            this.tabControl_Main.Name = "tabControl_Main";
            this.tabControl_Main.SelectedIndex = 0;
            this.tabControl_Main.Size = new System.Drawing.Size(1554, 1087);
            this.tabControl_Main.TabIndex = 2;
            // 
            // tabPage_Grid
            // 
            this.tabPage_Grid.Controls.Add(this.panel_Grid);
            this.tabPage_Grid.Location = new System.Drawing.Point(4, 25);
            this.tabPage_Grid.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_Grid.Name = "tabPage_Grid";
            this.tabPage_Grid.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage_Grid.Size = new System.Drawing.Size(1546, 1058);
            this.tabPage_Grid.TabIndex = 0;
            this.tabPage_Grid.Text = "缺陷列表";
            this.tabPage_Grid.UseVisualStyleBackColor = true;
            // 
            // panel_Grid
            // 
            this.panel_Grid.Controls.Add(this.dataGridView_Defects);
            this.panel_Grid.Controls.Add(this.splitter_LotGrid);
            this.panel_Grid.Controls.Add(this.panel_LotList);
            this.panel_Grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Grid.Location = new System.Drawing.Point(5, 5);
            this.panel_Grid.Margin = new System.Windows.Forms.Padding(5);
            this.panel_Grid.Name = "panel_Grid";
            this.panel_Grid.Size = new System.Drawing.Size(1536, 1048);
            this.panel_Grid.TabIndex = 1;
            // 
            // dataGridView_Defects
            // 
            this.dataGridView_Defects.AllowUserToAddRows = false;
            this.dataGridView_Defects.AllowUserToDeleteRows = false;
            this.dataGridView_Defects.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView_Defects.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.dataGridView_Defects.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView_Defects.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Defects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_SN,
            this.col_Lot,
            this.col_MachineId,
            this.col_ProductSerial,
            this.col_Side,
            this.col_AiStatus,
            this.col_ManualStatus,
            this.col_DefectCount,
            this.col_PathIndex,
            this.col_DetectionDate});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView_Defects.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView_Defects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_Defects.Location = new System.Drawing.Point(272, 0);
            this.dataGridView_Defects.Margin = new System.Windows.Forms.Padding(5);
            this.dataGridView_Defects.MultiSelect = false;
            this.dataGridView_Defects.Name = "dataGridView_Defects";
            this.dataGridView_Defects.RowHeadersVisible = false;
            this.dataGridView_Defects.RowHeadersWidth = 51;
            this.dataGridView_Defects.RowTemplate.Height = 23;
            this.dataGridView_Defects.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView_Defects.Size = new System.Drawing.Size(1264, 1048);
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
            this.splitter_LotGrid.Location = new System.Drawing.Point(267, 0);
            this.splitter_LotGrid.Margin = new System.Windows.Forms.Padding(4);
            this.splitter_LotGrid.Name = "splitter_LotGrid";
            this.splitter_LotGrid.Size = new System.Drawing.Size(5, 1048);
            this.splitter_LotGrid.TabIndex = 3;
            this.splitter_LotGrid.TabStop = false;
            // 
            // panel_LotList
            // 
            this.panel_LotList.Controls.Add(this.treeView_Lots);
            this.panel_LotList.Controls.Add(this.panel_SnSearch);
            this.panel_LotList.Controls.Add(this.label_LotTitle);
            this.panel_LotList.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel_LotList.Location = new System.Drawing.Point(0, 0);
            this.panel_LotList.Margin = new System.Windows.Forms.Padding(4);
            this.panel_LotList.Name = "panel_LotList";
            this.panel_LotList.Size = new System.Drawing.Size(267, 1048);
            this.panel_LotList.TabIndex = 2;
            // 
            // treeView_Lots
            // 
            this.treeView_Lots.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.treeView_Lots.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView_Lots.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView_Lots.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.treeView_Lots.ForeColor = System.Drawing.Color.White;
            this.treeView_Lots.FullRowSelect = true;
            this.treeView_Lots.HideSelection = false;
            this.treeView_Lots.Location = new System.Drawing.Point(0, 69);
            this.treeView_Lots.Margin = new System.Windows.Forms.Padding(4);
            this.treeView_Lots.Name = "treeView_Lots";
            this.treeView_Lots.Size = new System.Drawing.Size(267, 979);
            this.treeView_Lots.TabIndex = 1;
            // 
            // panel_SnSearch
            // 
            this.panel_SnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(38)))));
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
            this.btn_SnSearch.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_SnSearch.FlatAppearance.BorderSize = 0;
            this.btn_SnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_SnSearch.Font = new System.Drawing.Font("微软雅黑", 8F);
            this.btn_SnSearch.ForeColor = System.Drawing.Color.White;
            this.btn_SnSearch.Location = new System.Drawing.Point(210, 4);
            this.btn_SnSearch.Margin = new System.Windows.Forms.Padding(4);
            this.btn_SnSearch.Name = "btn_SnSearch";
            this.btn_SnSearch.Size = new System.Drawing.Size(53, 30);
            this.btn_SnSearch.TabIndex = 1;
            this.btn_SnSearch.Text = "搜索";
            this.btn_SnSearch.UseVisualStyleBackColor = false;
            // 
            // txt_SnFilter
            // 
            this.txt_SnFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
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
            this.tabPage_Details.Controls.Add(this.defectDetailControl1);
            this.tabPage_Details.Location = new System.Drawing.Point(4, 25);
            this.tabPage_Details.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_Details.Name = "tabPage_Details";
            this.tabPage_Details.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage_Details.Size = new System.Drawing.Size(1546, 1058);
            this.tabPage_Details.TabIndex = 1;
            this.tabPage_Details.Text = "缺陷详情";
            this.tabPage_Details.UseVisualStyleBackColor = true;
            // 
            // defectDetailControl1
            // 
            this.defectDetailControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.defectDetailControl1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.defectDetailControl1.Location = new System.Drawing.Point(5, 5);
            this.defectDetailControl1.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.defectDetailControl1.Name = "defectDetailControl1";
            this.defectDetailControl1.Size = new System.Drawing.Size(1536, 1048);
            this.defectDetailControl1.TabIndex = 0;
            // 
            // tabPage_ValidationTest
            // 
            this.tabPage_ValidationTest.Controls.Add(this.validationTestResultControl1);
            this.tabPage_ValidationTest.Location = new System.Drawing.Point(4, 25);
            this.tabPage_ValidationTest.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_ValidationTest.Name = "tabPage_ValidationTest";
            this.tabPage_ValidationTest.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage_ValidationTest.Size = new System.Drawing.Size(1546, 1058);
            this.tabPage_ValidationTest.TabIndex = 2;
            this.tabPage_ValidationTest.Text = "模型一致性测试";
            this.tabPage_ValidationTest.UseVisualStyleBackColor = true;
            // 
            // validationTestResultControl1
            // 
            this.validationTestResultControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.validationTestResultControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.validationTestResultControl1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.validationTestResultControl1.Location = new System.Drawing.Point(5, 5);
            this.validationTestResultControl1.Margin = new System.Windows.Forms.Padding(5);
            this.validationTestResultControl1.Name = "validationTestResultControl1";
            this.validationTestResultControl1.Size = new System.Drawing.Size(1536, 1048);
            this.validationTestResultControl1.TabIndex = 0;
            // 
            // contextMenuStrip_Lot
            // 
            this.contextMenuStrip_Lot.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip_Lot.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_RunTest});
            this.contextMenuStrip_Lot.Name = "contextMenuStrip_Lot";
            this.contextMenuStrip_Lot.Size = new System.Drawing.Size(214, 28);
            // 
            // toolStripMenuItem_RunTest
            // 
            this.toolStripMenuItem_RunTest.Name = "toolStripMenuItem_RunTest";
            this.toolStripMenuItem_RunTest.Size = new System.Drawing.Size(213, 24);
            this.toolStripMenuItem_RunTest.Text = "运行模型一致性测试";
            // 
            // AIReviewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel_Main);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "AIReviewControl";
            this.Size = new System.Drawing.Size(1924, 1087);
            this.panel_Main.ResumeLayout(false);
            this.splitContainer_Main.Panel1.ResumeLayout(false);
            this.splitContainer_Main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).EndInit();
            this.splitContainer_Main.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panel_ReviewDetail.ResumeLayout(false);
            this.tabControl_Main.ResumeLayout(false);
            this.tabPage_Grid.ResumeLayout(false);
            this.panel_Grid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Defects)).EndInit();
            this.panel_LotList.ResumeLayout(false);
            this.panel_SnSearch.ResumeLayout(false);
            this.panel_SnSearch.PerformLayout();
            this.tabPage_Details.ResumeLayout(false);
            this.tabPage_ValidationTest.ResumeLayout(false);
            this.contextMenuStrip_Lot.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_Main;
        private System.Windows.Forms.Panel panel_Grid;
        private System.Windows.Forms.DataGridView dataGridView_Defects;
        private System.Windows.Forms.Button btn_LoadImages;
        private System.Windows.Forms.Button btn_Export;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_SN;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Lot;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_MachineId;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_ProductSerial;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Side;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_AiStatus;
        private System.Windows.Forms.DataGridViewComboBoxColumn col_ManualStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_DefectCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_PathIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_DetectionDate;
        private QueryControl QueryControl;
        private DefectDetailControl defectDetailControl1;
        private System.Windows.Forms.TabControl tabControl_Main;
        private System.Windows.Forms.TabPage tabPage_Grid;
        private System.Windows.Forms.TabPage tabPage_Details;
        private System.Windows.Forms.SplitContainer splitContainer_Main;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Panel panel_LotList;
        private System.Windows.Forms.Label label_LotTitle;
        private System.Windows.Forms.TreeView treeView_Lots;
        private System.Windows.Forms.Splitter splitter_LotGrid;
        private System.Windows.Forms.Panel panel_SnSearch;
        private System.Windows.Forms.TextBox txt_SnFilter;
        private System.Windows.Forms.Button btn_SnSearch;
        private System.Windows.Forms.Panel panel_ReviewDetail;
        private System.Windows.Forms.Label label_ReviewDetail;
        private System.Windows.Forms.TabPage tabPage_ValidationTest;
        private ValidationTestResultControl validationTestResultControl1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_Lot;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_RunTest;
    }
}

namespace DeepSightAI
{
    partial class FrmHome
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.table_main = new System.Windows.Forms.TableLayoutPanel();
            this.panel_show = new System.Windows.Forms.Panel();
            this.splitContainer1 = new Sunny.UI.UISplitContainer();
            this.splitContainer4 = new Sunny.UI.UISplitContainer();
            this.dataGridViewData = new Sunny.UI.UIDataGridView();
            this.Code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Side = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AVI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStripData = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnShowDebugInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.rich_log = new Sunny.UI.UIRichTextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnClearLog = new System.Windows.Forms.ToolStripMenuItem();
            this.btnShowLog = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer2 = new Sunny.UI.UISplitContainer();
            this.avi_panel = new System.Windows.Forms.Panel();
            this.machineStatusPanel = new DeepSightAI.SettingPages.UcMachineStatusPanel();
            this.splitContainer3 = new Sunny.UI.UISplitContainer();
            this.splitContainer5 = new Sunny.UI.UISplitContainer();
            this.table_Small = new System.Windows.Forms.TableLayoutPanel();
            this.paginationPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnPrevious = new DeepSightAI.StyledButton();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.btnNext = new DeepSightAI.StyledButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.statsGridPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_SnTotalCount = new System.Windows.Forms.Label();
            this.lbl_totalDefectCount = new System.Windows.Forms.Label();
            this.lbl_CountPerPanel = new System.Windows.Forms.Label();
            this.lbl_AiAllCount = new System.Windows.Forms.Label();
            this.lbl_aiFilterOKCount = new System.Windows.Forms.Label();
            this.lbl_filteredOkCount = new System.Windows.Forms.Label();
            this.lbl_aviPassRateCount = new System.Windows.Forms.Label();
            this.lbl_boardAiPassRate = new System.Windows.Forms.Label();
            this.lbl_utilizationRate = new System.Windows.Forms.Label();
            this.table_main.SuspendLayout();
            this.panel_show.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewData)).BeginInit();
            this.contextMenuStripData.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.avi_panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.paginationPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.statsGridPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // table_main
            // 
            this.table_main.ColumnCount = 1;
            this.table_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.table_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.table_main.Controls.Add(this.panel_show, 0, 0);
            this.table_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.table_main.Location = new System.Drawing.Point(0, 0);
            this.table_main.Margin = new System.Windows.Forms.Padding(0);
            this.table_main.Name = "table_main";
            this.table_main.RowCount = 1;
            this.table_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.table_main.Size = new System.Drawing.Size(1761, 851);
            this.table_main.TabIndex = 0;
            // 
            // panel_show
            // 
            this.panel_show.Controls.Add(this.splitContainer1);
            this.panel_show.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_show.Location = new System.Drawing.Point(3, 3);
            this.panel_show.Name = "panel_show";
            this.panel_show.Size = new System.Drawing.Size(1755, 845);
            this.panel_show.TabIndex = 2;
            // 
            // splitContainer1
            // 
            this.splitContainer1.ArrowColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(190)))), ((int)(((byte)(200)))));
            this.splitContainer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer1.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.HandleColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.splitContainer1.HandleHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.MinimumSize = new System.Drawing.Size(15, 16);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.Black;
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer4);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1755, 845);
            this.splitContainer1.SplitterDistance = 494;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.Style = Sunny.UI.UIStyle.Custom;
            this.splitContainer1.StyleCustomMode = true;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer4
            // 
            this.splitContainer4.ArrowColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(190)))), ((int)(((byte)(200)))));
            this.splitContainer4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer4.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer4.CollapsePanel = Sunny.UI.UISplitContainer.UICollapsePanel.Panel2;
            this.splitContainer4.Cursor = System.Windows.Forms.Cursors.Default;
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.HandleColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.splitContainer4.HandleHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.MinimumSize = new System.Drawing.Size(15, 16);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.dataGridViewData);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.rich_log);
            this.splitContainer4.Size = new System.Drawing.Size(494, 845);
            this.splitContainer4.SplitterDistance = 550;
            this.splitContainer4.SplitterWidth = 3;
            this.splitContainer4.Style = Sunny.UI.UIStyle.Custom;
            this.splitContainer4.StyleCustomMode = true;
            this.splitContainer4.TabIndex = 1;
            // 
            // dataGridViewData
            // 
            this.dataGridViewData.AllowUserToAddRows = false;
            this.dataGridViewData.AllowUserToDeleteRows = false;
            this.dataGridViewData.AllowUserToResizeColumns = false;
            this.dataGridViewData.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            this.dataGridViewData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewData.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dataGridViewData.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewData.ColumnHeadersHeight = 32;
            this.dataGridViewData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridViewData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Code,
            this.Side,
            this.AVI,
            this.AI,
            this.Time,
            this.Status});
            this.dataGridViewData.ContextMenuStrip = this.contextMenuStripData;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(249)))), ((int)(((byte)(235)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewData.EnableHeadersVisualStyles = false;
            this.dataGridViewData.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dataGridViewData.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.dataGridViewData.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewData.MultiSelect = false;
            this.dataGridViewData.Name = "dataGridViewData";
            this.dataGridViewData.ReadOnly = true;
            this.dataGridViewData.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewData.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridViewData.RowHeadersVisible = false;
            this.dataGridViewData.RowHeadersWidth = 51;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dataGridViewData.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridViewData.RowTemplate.Height = 27;
            this.dataGridViewData.ScrollBarBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dataGridViewData.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.dataGridViewData.ScrollBarRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dataGridViewData.ScrollBarStyleInherited = false;
            this.dataGridViewData.SelectedIndex = -1;
            this.dataGridViewData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewData.ShowCellErrors = false;
            this.dataGridViewData.ShowCellToolTips = false;
            this.dataGridViewData.ShowEditingIcon = false;
            this.dataGridViewData.ShowRowErrors = false;
            this.dataGridViewData.Size = new System.Drawing.Size(494, 550);
            this.dataGridViewData.StripeEvenColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dataGridViewData.StripeOddColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            this.dataGridViewData.Style = Sunny.UI.UIStyle.Custom;
            this.dataGridViewData.StyleCustomMode = true;
            this.dataGridViewData.TabIndex = 2;
            this.dataGridViewData.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewData_CellClick);
            // 
            // Code
            // 
            this.Code.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Code.DataPropertyName = "Code";
            this.Code.FillWeight = 180F;
            this.Code.HeaderText = "SN任务队列";
            this.Code.MinimumWidth = 6;
            this.Code.Name = "Code";
            this.Code.ReadOnly = true;
            this.Code.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Side
            // 
            this.Side.HeaderText = "面";
            this.Side.MinimumWidth = 6;
            this.Side.Name = "Side";
            this.Side.ReadOnly = true;
            this.Side.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Side.Width = 35;
            // 
            // AVI
            // 
            this.AVI.HeaderText = "AVI";
            this.AVI.MinimumWidth = 6;
            this.AVI.Name = "AVI";
            this.AVI.ReadOnly = true;
            this.AVI.Width = 50;
            // 
            // AI
            // 
            this.AI.HeaderText = "AI";
            this.AI.MinimumWidth = 6;
            this.AI.Name = "AI";
            this.AI.ReadOnly = true;
            this.AI.Width = 50;
            // 
            // Time
            // 
            this.Time.HeaderText = "时间(ms)";
            this.Time.MinimumWidth = 6;
            this.Time.Name = "Time";
            this.Time.ReadOnly = true;
            this.Time.Width = 125;
            // 
            // Status
            // 
            this.Status.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Status.FillWeight = 150F;
            this.Status.HeaderText = "状态";
            this.Status.MinimumWidth = 8;
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // contextMenuStripData
            // 
            this.contextMenuStripData.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripData.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnShowDebugInfo});
            this.contextMenuStripData.Name = "contextMenuStripData";
            this.contextMenuStripData.Size = new System.Drawing.Size(139, 28);
            // 
            // btnShowDebugInfo
            // 
            this.btnShowDebugInfo.Name = "btnShowDebugInfo";
            this.btnShowDebugInfo.Size = new System.Drawing.Size(138, 24);
            this.btnShowDebugInfo.Text = "显示详情";
            this.btnShowDebugInfo.Click += new System.EventHandler(this.btnShowDebugInfo_Click);
            // 
            // rich_log
            // 
            this.rich_log.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.rich_log.ContextMenuStrip = this.contextMenuStrip1;
            this.rich_log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rich_log.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.rich_log.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rich_log.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.rich_log.Location = new System.Drawing.Point(0, 0);
            this.rich_log.Margin = new System.Windows.Forms.Padding(6);
            this.rich_log.MinimumSize = new System.Drawing.Size(1, 1);
            this.rich_log.Name = "rich_log";
            this.rich_log.Padding = new System.Windows.Forms.Padding(2);
            this.rich_log.ReadOnly = true;
            this.rich_log.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.rich_log.ScrollBarBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.rich_log.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.rich_log.ScrollBarStyleInherited = false;
            this.rich_log.ShowText = false;
            this.rich_log.Size = new System.Drawing.Size(494, 292);
            this.rich_log.Style = Sunny.UI.UIStyle.Custom;
            this.rich_log.StyleCustomMode = true;
            this.rich_log.TabIndex = 12;
            this.rich_log.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnClearLog,
            this.btnShowLog});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(139, 52);
            // 
            // btnClearLog
            // 
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(138, 24);
            this.btnClearLog.Text = "清空日志";
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // btnShowLog
            // 
            this.btnShowLog.Name = "btnShowLog";
            this.btnShowLog.Size = new System.Drawing.Size(138, 24);
            this.btnShowLog.Text = "查看日志";
            this.btnShowLog.Click += new System.EventHandler(this.btnShowLog_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.ArrowColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(190)))), ((int)(((byte)(200)))));
            this.splitContainer2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer2.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer2.Cursor = System.Windows.Forms.Cursors.Default;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.HandleColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.splitContainer2.HandleHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.MinimumSize = new System.Drawing.Size(15, 16);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.avi_panel);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(1258, 845);
            this.splitContainer2.SplitterDistance = 402;
            this.splitContainer2.SplitterWidth = 3;
            this.splitContainer2.Style = Sunny.UI.UIStyle.Custom;
            this.splitContainer2.StyleCustomMode = true;
            this.splitContainer2.TabIndex = 0;
            // 
            // avi_panel
            // 
            this.avi_panel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.avi_panel.Controls.Add(this.machineStatusPanel);
            this.avi_panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.avi_panel.Location = new System.Drawing.Point(0, 0);
            this.avi_panel.Name = "avi_panel";
            this.avi_panel.Size = new System.Drawing.Size(1258, 402);
            this.avi_panel.TabIndex = 0;
            // 
            // machineStatusPanel
            // 
            this.machineStatusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.machineStatusPanel.Location = new System.Drawing.Point(0, 0);
            this.machineStatusPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.machineStatusPanel.Name = "machineStatusPanel";
            this.machineStatusPanel.ShowDeleteButtons = true;
            this.machineStatusPanel.Size = new System.Drawing.Size(1258, 402);
            this.machineStatusPanel.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.ArrowColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(190)))), ((int)(((byte)(200)))));
            this.splitContainer3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer3.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.HandleColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.splitContainer3.HandleHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.MinimumSize = new System.Drawing.Size(15, 16);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer5);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.panel1);
            this.splitContainer3.Size = new System.Drawing.Size(1258, 440);
            this.splitContainer3.SplitterDistance = 831;
            this.splitContainer3.SplitterWidth = 3;
            this.splitContainer3.Style = Sunny.UI.UIStyle.Custom;
            this.splitContainer3.StyleCustomMode = true;
            this.splitContainer3.TabIndex = 0;
            // 
            // splitContainer5
            // 
            this.splitContainer5.ArrowColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(190)))), ((int)(((byte)(200)))));
            this.splitContainer5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer5.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer5.CollapsePanel = Sunny.UI.UISplitContainer.UICollapsePanel.Panel2;
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.HandleColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.splitContainer5.HandleHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.MinimumSize = new System.Drawing.Size(15, 16);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.table_Small);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer5.Panel2.Controls.Add(this.paginationPanel);
            this.splitContainer5.Size = new System.Drawing.Size(831, 440);
            this.splitContainer5.SplitterDistance = 374;
            this.splitContainer5.SplitterWidth = 3;
            this.splitContainer5.Style = Sunny.UI.UIStyle.Custom;
            this.splitContainer5.StyleCustomMode = true;
            this.splitContainer5.TabIndex = 0;
            // 
            // table_Small
            // 
            this.table_Small.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.table_Small.ColumnCount = 1;
            this.table_Small.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.table_Small.Dock = System.Windows.Forms.DockStyle.Fill;
            this.table_Small.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.table_Small.Location = new System.Drawing.Point(0, 0);
            this.table_Small.Margin = new System.Windows.Forms.Padding(1);
            this.table_Small.Name = "table_Small";
            this.table_Small.RowCount = 1;
            this.table_Small.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 766F));
            this.table_Small.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 766F));
            this.table_Small.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 766F));
            this.table_Small.Size = new System.Drawing.Size(831, 374);
            this.table_Small.TabIndex = 11;
            // 
            // paginationPanel
            // 
            this.paginationPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.paginationPanel.ColumnCount = 3;
            this.paginationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95F));
            this.paginationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.paginationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95F));
            this.paginationPanel.Controls.Add(this.btnPrevious, 0, 0);
            this.paginationPanel.Controls.Add(this.lblPageInfo, 1, 0);
            this.paginationPanel.Controls.Add(this.btnNext, 2, 0);
            this.paginationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.paginationPanel.Location = new System.Drawing.Point(0, 0);
            this.paginationPanel.Name = "paginationPanel";
            this.paginationPanel.RowCount = 1;
            this.paginationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.paginationPanel.Size = new System.Drawing.Size(831, 63);
            this.paginationPanel.TabIndex = 0;
            // 
            // btnPrevious
            // 
            this.btnPrevious.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnPrevious.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrevious.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevious.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnPrevious.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnPrevious.Location = new System.Drawing.Point(10, 5);
            this.btnPrevious.Margin = new System.Windows.Forms.Padding(10, 5, 5, 5);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(80, 53);
            this.btnPrevious.TabIndex = 45;
            this.btnPrevious.Text = "上一页  ◀";
            this.btnPrevious.UseVisualStyleBackColor = false;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.lblPageInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPageInfo.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPageInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblPageInfo.Location = new System.Drawing.Point(98, 0);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(635, 63);
            this.lblPageInfo.TabIndex = 46;
            this.lblPageInfo.Text = "第 1 页 / 共 1 页";
            this.lblPageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnNext
            // 
            this.btnNext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnNext.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnNext.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnNext.Location = new System.Drawing.Point(741, 5);
            this.btnNext.Margin = new System.Windows.Forms.Padding(5, 5, 10, 5);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(80, 53);
            this.btnNext.TabIndex = 44;
            this.btnNext.Text = "下一页  ▶";
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel1.Controls.Add(this.statsGridPanel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(424, 440);
            this.panel1.TabIndex = 0;
            // 
            // statsGridPanel
            // 
            this.statsGridPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.statsGridPanel.ColumnCount = 3;
            this.statsGridPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.statsGridPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.statsGridPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.statsGridPanel.Controls.Add(this.lbl_SnTotalCount, 0, 0);
            this.statsGridPanel.Controls.Add(this.lbl_totalDefectCount, 1, 0);
            this.statsGridPanel.Controls.Add(this.lbl_CountPerPanel, 2, 0);
            this.statsGridPanel.Controls.Add(this.lbl_AiAllCount, 0, 1);
            this.statsGridPanel.Controls.Add(this.lbl_aiFilterOKCount, 1, 1);
            this.statsGridPanel.Controls.Add(this.lbl_filteredOkCount, 2, 1);
            this.statsGridPanel.Controls.Add(this.lbl_aviPassRateCount, 0, 2);
            this.statsGridPanel.Controls.Add(this.lbl_boardAiPassRate, 1, 2);
            this.statsGridPanel.Controls.Add(this.lbl_utilizationRate, 2, 2);
            this.statsGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statsGridPanel.Location = new System.Drawing.Point(0, 0);
            this.statsGridPanel.Name = "statsGridPanel";
            this.statsGridPanel.Padding = new System.Windows.Forms.Padding(5);
            this.statsGridPanel.RowCount = 3;
            this.statsGridPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.statsGridPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.statsGridPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.statsGridPanel.Size = new System.Drawing.Size(424, 440);
            this.statsGridPanel.TabIndex = 0;
            // 
            // lbl_SnTotalCount
            // 
            this.lbl_SnTotalCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_SnTotalCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_SnTotalCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_SnTotalCount.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_SnTotalCount.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_SnTotalCount.Location = new System.Drawing.Point(8, 8);
            this.lbl_SnTotalCount.Margin = new System.Windows.Forms.Padding(3);
            this.lbl_SnTotalCount.Name = "lbl_SnTotalCount";
            this.lbl_SnTotalCount.Size = new System.Drawing.Size(131, 137);
            this.lbl_SnTotalCount.TabIndex = 5;
            this.lbl_SnTotalCount.Text = "-";
            this.lbl_SnTotalCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_totalDefectCount
            // 
            this.lbl_totalDefectCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_totalDefectCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_totalDefectCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_totalDefectCount.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_totalDefectCount.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_totalDefectCount.Location = new System.Drawing.Point(145, 8);
            this.lbl_totalDefectCount.Margin = new System.Windows.Forms.Padding(3);
            this.lbl_totalDefectCount.Name = "lbl_totalDefectCount";
            this.lbl_totalDefectCount.Size = new System.Drawing.Size(132, 137);
            this.lbl_totalDefectCount.TabIndex = 0;
            this.lbl_totalDefectCount.Text = "-";
            this.lbl_totalDefectCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_CountPerPanel
            // 
            this.lbl_CountPerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_CountPerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_CountPerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_CountPerPanel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_CountPerPanel.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_CountPerPanel.Location = new System.Drawing.Point(283, 8);
            this.lbl_CountPerPanel.Margin = new System.Windows.Forms.Padding(3);
            this.lbl_CountPerPanel.Name = "lbl_CountPerPanel";
            this.lbl_CountPerPanel.Size = new System.Drawing.Size(133, 137);
            this.lbl_CountPerPanel.TabIndex = 9;
            this.lbl_CountPerPanel.Text = "-";
            this.lbl_CountPerPanel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_AiAllCount
            // 
            this.lbl_AiAllCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_AiAllCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_AiAllCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_AiAllCount.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_AiAllCount.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_AiAllCount.Location = new System.Drawing.Point(8, 151);
            this.lbl_AiAllCount.Margin = new System.Windows.Forms.Padding(3);
            this.lbl_AiAllCount.Name = "lbl_AiAllCount";
            this.lbl_AiAllCount.Size = new System.Drawing.Size(131, 137);
            this.lbl_AiAllCount.TabIndex = 1;
            this.lbl_AiAllCount.Text = "-";
            this.lbl_AiAllCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_aiFilterOKCount
            // 
            this.lbl_aiFilterOKCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_aiFilterOKCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_aiFilterOKCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_aiFilterOKCount.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_aiFilterOKCount.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_aiFilterOKCount.Location = new System.Drawing.Point(145, 151);
            this.lbl_aiFilterOKCount.Margin = new System.Windows.Forms.Padding(3);
            this.lbl_aiFilterOKCount.Name = "lbl_aiFilterOKCount";
            this.lbl_aiFilterOKCount.Size = new System.Drawing.Size(132, 137);
            this.lbl_aiFilterOKCount.TabIndex = 2;
            this.lbl_aiFilterOKCount.Text = "-";
            this.lbl_aiFilterOKCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_filteredOkCount
            // 
            this.lbl_filteredOkCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_filteredOkCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_filteredOkCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_filteredOkCount.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_filteredOkCount.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_filteredOkCount.Location = new System.Drawing.Point(283, 151);
            this.lbl_filteredOkCount.Margin = new System.Windows.Forms.Padding(3);
            this.lbl_filteredOkCount.Name = "lbl_filteredOkCount";
            this.lbl_filteredOkCount.Size = new System.Drawing.Size(133, 137);
            this.lbl_filteredOkCount.TabIndex = 4;
            this.lbl_filteredOkCount.Text = "-";
            this.lbl_filteredOkCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_aviPassRateCount
            // 
            this.lbl_aviPassRateCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_aviPassRateCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_aviPassRateCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_aviPassRateCount.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_aviPassRateCount.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_aviPassRateCount.Location = new System.Drawing.Point(8, 294);
            this.lbl_aviPassRateCount.Margin = new System.Windows.Forms.Padding(3);
            this.lbl_aviPassRateCount.Name = "lbl_aviPassRateCount";
            this.lbl_aviPassRateCount.Size = new System.Drawing.Size(131, 138);
            this.lbl_aviPassRateCount.TabIndex = 3;
            this.lbl_aviPassRateCount.Text = "-";
            this.lbl_aviPassRateCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_boardAiPassRate
            // 
            this.lbl_boardAiPassRate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_boardAiPassRate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_boardAiPassRate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_boardAiPassRate.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_boardAiPassRate.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_boardAiPassRate.Location = new System.Drawing.Point(145, 294);
            this.lbl_boardAiPassRate.Margin = new System.Windows.Forms.Padding(3);
            this.lbl_boardAiPassRate.Name = "lbl_boardAiPassRate";
            this.lbl_boardAiPassRate.Size = new System.Drawing.Size(132, 138);
            this.lbl_boardAiPassRate.TabIndex = 7;
            this.lbl_boardAiPassRate.Text = "-";
            this.lbl_boardAiPassRate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_utilizationRate
            // 
            this.lbl_utilizationRate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_utilizationRate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_utilizationRate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_utilizationRate.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_utilizationRate.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_utilizationRate.Location = new System.Drawing.Point(283, 294);
            this.lbl_utilizationRate.Margin = new System.Windows.Forms.Padding(3);
            this.lbl_utilizationRate.Name = "lbl_utilizationRate";
            this.lbl_utilizationRate.Size = new System.Drawing.Size(133, 138);
            this.lbl_utilizationRate.TabIndex = 8;
            this.lbl_utilizationRate.Text = "-";
            this.lbl_utilizationRate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FrmHome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1761, 851);
            this.Controls.Add(this.table_main);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmHome";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.table_main.ResumeLayout(false);
            this.panel_show.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewData)).EndInit();
            this.contextMenuStripData.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.avi_panel.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.paginationPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.statsGridPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel_show;
        public System.Windows.Forms.TableLayoutPanel table_main;
        private Sunny.UI.UISplitContainer splitContainer1;
        private Sunny.UI.UISplitContainer splitContainer4;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem btnClearLog;
        private System.Windows.Forms.ToolStripMenuItem btnShowLog;
        public Sunny.UI.UIRichTextBox rich_log;
        private Sunny.UI.UISplitContainer splitContainer2;
        private Sunny.UI.UISplitContainer splitContainer3;
        private Sunny.UI.UISplitContainer splitContainer5;
        public System.Windows.Forms.TableLayoutPanel table_Small;
        private System.Windows.Forms.Label lblPageInfo;
        private DeepSightAI.StyledButton btnPrevious;
        private DeepSightAI.StyledButton btnNext;
        public Sunny.UI.UIDataGridView dataGridViewData;
        private System.Windows.Forms.Panel avi_panel;
        private System.Windows.Forms.Panel panel1;
        private SettingPages.UcMachineStatusPanel machineStatusPanel;
        private System.Windows.Forms.Label lbl_totalDefectCount;
        private System.Windows.Forms.Label lbl_AiAllCount;
        private System.Windows.Forms.Label lbl_aiFilterOKCount;
        private System.Windows.Forms.Label lbl_aviPassRateCount;
        private System.Windows.Forms.Label lbl_filteredOkCount;
        private System.Windows.Forms.Label lbl_SnTotalCount;
        private System.Windows.Forms.Label lbl_boardAiPassRate;
        private System.Windows.Forms.Label lbl_utilizationRate;
        private System.Windows.Forms.Label lbl_CountPerPanel;
        private System.Windows.Forms.DataGridViewTextBoxColumn Code;
        private System.Windows.Forms.DataGridViewTextBoxColumn Side;
        private System.Windows.Forms.DataGridViewTextBoxColumn AVI;
        private System.Windows.Forms.DataGridViewTextBoxColumn AI;
        private System.Windows.Forms.DataGridViewTextBoxColumn Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripData;
        private System.Windows.Forms.ToolStripMenuItem btnShowDebugInfo;
        private System.Windows.Forms.TableLayoutPanel paginationPanel;
        private System.Windows.Forms.TableLayoutPanel statsGridPanel;
    }
}
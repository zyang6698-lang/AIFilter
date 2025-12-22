namespace DeepSightAI
{
    partial class FrHome
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
            this.table_main = new System.Windows.Forms.TableLayoutPanel();
            this.panel_show = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.dataGridViewData = new System.Windows.Forms.DataGridView();
            this.Code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AVI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rich_log = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnClearLog = new System.Windows.Forms.ToolStripMenuItem();
            this.btnShowLog = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.avi_panel = new System.Windows.Forms.Panel();
            this.aviCtr2Container = new DeepSightAI.SettingPages.AviCtr2Container();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.table_Small = new System.Windows.Forms.TableLayoutPanel();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbl_CountPerPanel = new System.Windows.Forms.Label();
            this.lbl_utilizationRate = new System.Windows.Forms.Label();
            this.lbl_boardAiPassRate = new System.Windows.Forms.Label();
            this.lbl_SnTotalCount = new System.Windows.Forms.Label();
            this.lbl_totalDefectCount = new System.Windows.Forms.Label();
            this.lbl_AiAllCount = new System.Windows.Forms.Label();
            this.lbl_aiFilterOKCount = new System.Windows.Forms.Label();
            this.lbl_aviPassRateCount = new System.Windows.Forms.Label();
            this.lbl_filteredOkCount = new System.Windows.Forms.Label();
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
            this.panel1.SuspendLayout();
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
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainer1.SplitterDistance = 420;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainer4.Size = new System.Drawing.Size(420, 845);
            this.splitContainer4.SplitterDistance = 550;
            this.splitContainer4.TabIndex = 1;
            // 
            // dataGridViewData
            // 
            this.dataGridViewData.AllowUserToAddRows = false;
            this.dataGridViewData.AllowUserToDeleteRows = false;
            this.dataGridViewData.AllowUserToResizeColumns = false;
            this.dataGridViewData.AllowUserToResizeRows = false;
            this.dataGridViewData.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Code,
            this.AVI,
            this.AI,
            this.Time,
            this.Status});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(249)))), ((int)(((byte)(235)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.InactiveCaption;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewData.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewData.EnableHeadersVisualStyles = false;
            this.dataGridViewData.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewData.MultiSelect = false;
            this.dataGridViewData.Name = "dataGridViewData";
            this.dataGridViewData.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewData.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewData.RowHeadersVisible = false;
            this.dataGridViewData.RowHeadersWidth = 51;
            this.dataGridViewData.RowTemplate.Height = 27;
            this.dataGridViewData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewData.ShowCellErrors = false;
            this.dataGridViewData.ShowCellToolTips = false;
            this.dataGridViewData.ShowEditingIcon = false;
            this.dataGridViewData.ShowRowErrors = false;
            this.dataGridViewData.Size = new System.Drawing.Size(420, 550);
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
            this.Time.Width = 120;
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
            // rich_log
            // 
            this.rich_log.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.rich_log.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rich_log.ContextMenuStrip = this.contextMenuStrip1;
            this.rich_log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rich_log.Location = new System.Drawing.Point(0, 0);
            this.rich_log.Margin = new System.Windows.Forms.Padding(6);
            this.rich_log.Name = "rich_log";
            this.rich_log.ReadOnly = true;
            this.rich_log.Size = new System.Drawing.Size(420, 291);
            this.rich_log.TabIndex = 12;
            this.rich_log.Text = "";
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
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.avi_panel);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(1331, 845);
            this.splitContainer2.SplitterDistance = 564;
            this.splitContainer2.TabIndex = 0;
            // 
            // avi_panel
            // 
            this.avi_panel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.avi_panel.Controls.Add(this.aviCtr2Container);
            this.avi_panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.avi_panel.Location = new System.Drawing.Point(0, 0);
            this.avi_panel.Name = "avi_panel";
            this.avi_panel.Size = new System.Drawing.Size(1331, 564);
            this.avi_panel.TabIndex = 0;
            // 
            // aviCtr2Container
            // 
            this.aviCtr2Container.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aviCtr2Container.Location = new System.Drawing.Point(0, 0);
            this.aviCtr2Container.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.aviCtr2Container.Name = "aviCtr2Container";
            this.aviCtr2Container.ShowDeleteButtons = true;
            this.aviCtr2Container.Size = new System.Drawing.Size(1331, 564);
            this.aviCtr2Container.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer5);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.panel1);
            this.splitContainer3.Size = new System.Drawing.Size(1331, 277);
            this.splitContainer3.SplitterDistance = 578;
            this.splitContainer3.TabIndex = 0;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainer5.Panel2.Controls.Add(this.lblPageInfo);
            this.splitContainer5.Panel2.Controls.Add(this.btnPrevious);
            this.splitContainer5.Panel2.Controls.Add(this.btnNext);
            this.splitContainer5.Size = new System.Drawing.Size(578, 277);
            this.splitContainer5.SplitterDistance = 236;
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
            this.table_Small.Size = new System.Drawing.Size(578, 236);
            this.table_Small.TabIndex = 11;
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.lblPageInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblPageInfo.Location = new System.Drawing.Point(223, 11);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(117, 20);
            this.lblPageInfo.TabIndex = 46;
            this.lblPageInfo.Text = "第 1 页 / 共 1 页";
            this.lblPageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnPrevious
            // 
            this.btnPrevious.Location = new System.Drawing.Point(17, 11);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(75, 35);
            this.btnPrevious.TabIndex = 45;
            this.btnPrevious.Text = "上一页";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnNext
            // 
            this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNext.Location = new System.Drawing.Point(490, 12);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 32);
            this.btnNext.TabIndex = 44;
            this.btnNext.Text = "下一页";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel1.Controls.Add(this.lbl_CountPerPanel);
            this.panel1.Controls.Add(this.lbl_utilizationRate);
            this.panel1.Controls.Add(this.lbl_boardAiPassRate);
            this.panel1.Controls.Add(this.lbl_SnTotalCount);
            this.panel1.Controls.Add(this.lbl_totalDefectCount);
            this.panel1.Controls.Add(this.lbl_AiAllCount);
            this.panel1.Controls.Add(this.lbl_aiFilterOKCount);
            this.panel1.Controls.Add(this.lbl_aviPassRateCount);
            this.panel1.Controls.Add(this.lbl_filteredOkCount);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(749, 277);
            this.panel1.TabIndex = 0;
            // 
            // lbl_CountPerPanel
            // 
            this.lbl_CountPerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_CountPerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_CountPerPanel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_CountPerPanel.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_CountPerPanel.Location = new System.Drawing.Point(469, 16);
            this.lbl_CountPerPanel.Name = "lbl_CountPerPanel";
            this.lbl_CountPerPanel.Size = new System.Drawing.Size(218, 77);
            this.lbl_CountPerPanel.TabIndex = 9;
            this.lbl_CountPerPanel.Text = "-";
            this.lbl_CountPerPanel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_utilizationRate
            // 
            this.lbl_utilizationRate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_utilizationRate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_utilizationRate.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_utilizationRate.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_utilizationRate.Location = new System.Drawing.Point(469, 197);
            this.lbl_utilizationRate.Name = "lbl_utilizationRate";
            this.lbl_utilizationRate.Size = new System.Drawing.Size(218, 77);
            this.lbl_utilizationRate.TabIndex = 8;
            this.lbl_utilizationRate.Text = "-";
            this.lbl_utilizationRate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_boardAiPassRate
            // 
            this.lbl_boardAiPassRate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_boardAiPassRate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_boardAiPassRate.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_boardAiPassRate.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_boardAiPassRate.Location = new System.Drawing.Point(242, 197);
            this.lbl_boardAiPassRate.Name = "lbl_boardAiPassRate";
            this.lbl_boardAiPassRate.Size = new System.Drawing.Size(218, 77);
            this.lbl_boardAiPassRate.TabIndex = 7;
            this.lbl_boardAiPassRate.Text = "-";
            this.lbl_boardAiPassRate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_SnTotalCount
            // 
            this.lbl_SnTotalCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_SnTotalCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_SnTotalCount.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_SnTotalCount.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_SnTotalCount.Location = new System.Drawing.Point(17, 16);
            this.lbl_SnTotalCount.Name = "lbl_SnTotalCount";
            this.lbl_SnTotalCount.Size = new System.Drawing.Size(218, 77);
            this.lbl_SnTotalCount.TabIndex = 5;
            this.lbl_SnTotalCount.Text = "-";
            this.lbl_SnTotalCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_totalDefectCount
            // 
            this.lbl_totalDefectCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_totalDefectCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_totalDefectCount.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_totalDefectCount.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_totalDefectCount.Location = new System.Drawing.Point(242, 16);
            this.lbl_totalDefectCount.Name = "lbl_totalDefectCount";
            this.lbl_totalDefectCount.Size = new System.Drawing.Size(218, 77);
            this.lbl_totalDefectCount.TabIndex = 0;
            this.lbl_totalDefectCount.Text = "-";
            this.lbl_totalDefectCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_AiAllCount
            // 
            this.lbl_AiAllCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_AiAllCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_AiAllCount.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_AiAllCount.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_AiAllCount.Location = new System.Drawing.Point(17, 107);
            this.lbl_AiAllCount.Name = "lbl_AiAllCount";
            this.lbl_AiAllCount.Size = new System.Drawing.Size(218, 77);
            this.lbl_AiAllCount.TabIndex = 1;
            this.lbl_AiAllCount.Text = "-";
            this.lbl_AiAllCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_aiFilterOKCount
            // 
            this.lbl_aiFilterOKCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_aiFilterOKCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_aiFilterOKCount.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_aiFilterOKCount.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_aiFilterOKCount.Location = new System.Drawing.Point(242, 107);
            this.lbl_aiFilterOKCount.Name = "lbl_aiFilterOKCount";
            this.lbl_aiFilterOKCount.Size = new System.Drawing.Size(218, 77);
            this.lbl_aiFilterOKCount.TabIndex = 2;
            this.lbl_aiFilterOKCount.Text = "-";
            this.lbl_aiFilterOKCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_aviPassRateCount
            // 
            this.lbl_aviPassRateCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_aviPassRateCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_aviPassRateCount.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_aviPassRateCount.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_aviPassRateCount.Location = new System.Drawing.Point(17, 197);
            this.lbl_aviPassRateCount.Name = "lbl_aviPassRateCount";
            this.lbl_aviPassRateCount.Size = new System.Drawing.Size(218, 77);
            this.lbl_aviPassRateCount.TabIndex = 3;
            this.lbl_aviPassRateCount.Text = "-";
            this.lbl_aviPassRateCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_filteredOkCount
            // 
            this.lbl_filteredOkCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.lbl_filteredOkCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_filteredOkCount.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.lbl_filteredOkCount.ForeColor = System.Drawing.Color.FloralWhite;
            this.lbl_filteredOkCount.Location = new System.Drawing.Point(469, 107);
            this.lbl_filteredOkCount.Name = "lbl_filteredOkCount";
            this.lbl_filteredOkCount.Size = new System.Drawing.Size(218, 77);
            this.lbl_filteredOkCount.TabIndex = 4;
            this.lbl_filteredOkCount.Text = "-";
            this.lbl_filteredOkCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FrHome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1761, 851);
            this.Controls.Add(this.table_main);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrHome";
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
            this.splitContainer5.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel_show;
        public System.Windows.Forms.TableLayoutPanel table_main;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem btnClearLog;
        private System.Windows.Forms.ToolStripMenuItem btnShowLog;
        public System.Windows.Forms.RichTextBox rich_log;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer5;
        public System.Windows.Forms.TableLayoutPanel table_Small;
        private System.Windows.Forms.Label lblPageInfo;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnNext;
        public System.Windows.Forms.DataGridView dataGridViewData;
        private System.Windows.Forms.Panel avi_panel;
        private System.Windows.Forms.Panel panel1;
        private SettingPages.AviCtr2Container aviCtr2Container;
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
        private System.Windows.Forms.DataGridViewTextBoxColumn AVI;
        private System.Windows.Forms.DataGridViewTextBoxColumn AI;
        private System.Windows.Forms.DataGridViewTextBoxColumn Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
    }
}
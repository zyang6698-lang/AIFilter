namespace DeepSightAI.SettingPages
{
    partial class PgSettingDatabase
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
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlLeft = new Sunny.UI.UIPanel();
            this.lstDatabases = new Sunny.UI.UIListBox();
            this.tlpListButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btnAdd = new Sunny.UI.UIButton();
            this.btnDelete = new Sunny.UI.UIButton();
            this.btnTestAll = new Sunny.UI.UIButton();
            this.lblListTitle = new Sunny.UI.UILabel();
            this.tlpRight = new System.Windows.Forms.TableLayoutPanel();
            this.grpAvi = new Sunny.UI.UIGroupBox();
            this.tlpAvi = new System.Windows.Forms.TableLayoutPanel();
            this.lblIp = new Sunny.UI.UILabel();
            this.txtIp = new Sunny.UI.UITextBox();
            this.lblPort = new Sunny.UI.UILabel();
            this.txtPort = new Sunny.UI.UITextBox();
            this.lblAviStatus = new Sunny.UI.UILabel();
            this.btnAviTest = new Sunny.UI.UIButton();
            this.lblDbName = new Sunny.UI.UILabel();
            this.txtDbName = new Sunny.UI.UITextBox();
            this.chkIsEnabled = new Sunny.UI.UISwitch();
            this.lblWriteBackDbName = new Sunny.UI.UILabel();
            this.txtWriteBackDbName = new Sunny.UI.UITextBox();
            this.grpVrs = new Sunny.UI.UIGroupBox();
            this.tlpVrs = new System.Windows.Forms.TableLayoutPanel();
            this.lblVrsIp = new Sunny.UI.UILabel();
            this.txtVrsIp = new Sunny.UI.UITextBox();
            this.lblVrsPort = new Sunny.UI.UILabel();
            this.txtVrsPort = new Sunny.UI.UITextBox();
            this.lblVrsStatus = new Sunny.UI.UILabel();
            this.btnVrsTest = new Sunny.UI.UIButton();
            this.lblVrsWriteBackDbName = new Sunny.UI.UILabel();
            this.txtVrsWriteBackDbName = new Sunny.UI.UITextBox();
            this.lblVrsWriteBackDbNameV1 = new Sunny.UI.UILabel();
            this.txtVrsWriteBackDbNameV1 = new Sunny.UI.UITextBox();
            this.lblVrsHistoryDbName = new Sunny.UI.UILabel();
            this.txtVrsHistoryDbName = new Sunny.UI.UITextBox();
            this.lblVrsTestSn = new Sunny.UI.UILabel();
            this.txtVrsTestSn = new Sunny.UI.UITextBox();
            this.grpMinio = new Sunny.UI.UIGroupBox();
            this.tlpMinio = new System.Windows.Forms.TableLayoutPanel();
            this.lblMinioA = new Sunny.UI.UILabel();
            this.txtMinioIpA = new Sunny.UI.UITextBox();
            this.lblMinioStatusA = new Sunny.UI.UILabel();
            this.btnMinioTestA = new Sunny.UI.UIButton();
            this.lblMinioB = new Sunny.UI.UILabel();
            this.txtMinioIpB = new Sunny.UI.UITextBox();
            this.lblMinioStatusB = new Sunny.UI.UILabel();
            this.btnMinioTestB = new Sunny.UI.UIButton();
            this.tlpMain.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.tlpListButtons.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.grpAvi.SuspendLayout();
            this.tlpAvi.SuspendLayout();
            this.grpVrs.SuspendLayout();
            this.tlpVrs.SuspendLayout();
            this.grpMinio.SuspendLayout();
            this.tlpMinio.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 260F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.pnlLeft, 0, 0);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.Padding = new System.Windows.Forms.Padding(8);
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(1263, 831);
            this.tlpMain.TabIndex = 0;
            // 
            // pnlLeft
            // 
            this.pnlLeft.Controls.Add(this.lstDatabases);
            this.pnlLeft.Controls.Add(this.tlpListButtons);
            this.pnlLeft.Controls.Add(this.lblListTitle);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeft.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.pnlLeft.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pnlLeft.Location = new System.Drawing.Point(8, 8);
            this.pnlLeft.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.pnlLeft.MinimumSize = new System.Drawing.Size(1, 1);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.pnlLeft.Size = new System.Drawing.Size(254, 815);
            this.pnlLeft.Style = Sunny.UI.UIStyle.Custom;
            this.pnlLeft.StyleCustomMode = true;
            this.pnlLeft.TabIndex = 0;
            this.pnlLeft.Text = null;
            this.pnlLeft.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lstDatabases
            // 
            this.lstDatabases.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstDatabases.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.lstDatabases.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lstDatabases.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lstDatabases.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.lstDatabases.ItemHeight = 30;
            this.lstDatabases.ItemSelectBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.lstDatabases.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lstDatabases.Location = new System.Drawing.Point(0, 32);
            this.lstDatabases.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lstDatabases.MinimumSize = new System.Drawing.Size(1, 1);
            this.lstDatabases.Name = "lstDatabases";
            this.lstDatabases.Padding = new System.Windows.Forms.Padding(2);
            this.lstDatabases.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.lstDatabases.ShowText = false;
            this.lstDatabases.Size = new System.Drawing.Size(254, 739);
            this.lstDatabases.Style = Sunny.UI.UIStyle.Custom;
            this.lstDatabases.StyleCustomMode = true;
            this.lstDatabases.TabIndex = 1;
            this.lstDatabases.Text = "lstDatabases";
            this.lstDatabases.SelectedIndexChanged += new System.EventHandler(this.lstDatabases_SelectedIndexChanged);
            // 
            // tlpListButtons
            // 
            this.tlpListButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tlpListButtons.ColumnCount = 3;
            this.tlpListButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.tlpListButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tlpListButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tlpListButtons.Controls.Add(this.btnAdd, 0, 0);
            this.tlpListButtons.Controls.Add(this.btnDelete, 1, 0);
            this.tlpListButtons.Controls.Add(this.btnTestAll, 2, 0);
            this.tlpListButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tlpListButtons.Location = new System.Drawing.Point(0, 771);
            this.tlpListButtons.Name = "tlpListButtons";
            this.tlpListButtons.RowCount = 1;
            this.tlpListButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpListButtons.Size = new System.Drawing.Size(254, 44);
            this.tlpListButtons.TabIndex = 2;
            // 
            // btnAdd
            // 
            this.btnAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAdd.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnAdd.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(115)))));
            this.btnAdd.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(70)))));
            this.btnAdd.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnAdd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnAdd.Location = new System.Drawing.Point(4, 6);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAdd.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnAdd.Size = new System.Drawing.Size(76, 32);
            this.btnAdd.Style = Sunny.UI.UIStyle.Custom;
            this.btnAdd.StyleCustomMode = true;
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "添加";
            this.btnAdd.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDelete.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnDelete.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(115)))));
            this.btnDelete.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(70)))));
            this.btnDelete.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnDelete.Location = new System.Drawing.Point(88, 6);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnDelete.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnDelete.Size = new System.Drawing.Size(76, 32);
            this.btnDelete.Style = Sunny.UI.UIStyle.Custom;
            this.btnDelete.StyleCustomMode = true;
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "删除";
            this.btnDelete.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnTestAll
            // 
            this.btnTestAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTestAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTestAll.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnTestAll.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(115)))));
            this.btnTestAll.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(70)))));
            this.btnTestAll.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnTestAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnTestAll.Location = new System.Drawing.Point(172, 6);
            this.btnTestAll.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnTestAll.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnTestAll.Name = "btnTestAll";
            this.btnTestAll.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnTestAll.Size = new System.Drawing.Size(78, 32);
            this.btnTestAll.Style = Sunny.UI.UIStyle.Custom;
            this.btnTestAll.StyleCustomMode = true;
            this.btnTestAll.TabIndex = 2;
            this.btnTestAll.Text = "全部测试";
            this.btnTestAll.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTestAll.Click += new System.EventHandler(this.btnTestAll_Click);
            // 
            // lblListTitle
            // 
            this.lblListTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.lblListTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListTitle.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblListTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblListTitle.Location = new System.Drawing.Point(0, 0);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Size = new System.Drawing.Size(254, 32);
            this.lblListTitle.Style = Sunny.UI.UIStyle.Custom;
            this.lblListTitle.StyleCustomMode = true;
            this.lblListTitle.TabIndex = 0;
            this.lblListTitle.Text = "  数据库列表";
            this.lblListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tlpRight
            // 
            this.tlpRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tlpRight.ColumnCount = 1;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Controls.Add(this.grpAvi, 0, 0);
            this.tlpRight.Controls.Add(this.grpVrs, 0, 1);
            this.tlpRight.Controls.Add(this.grpMinio, 0, 2);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(274, 8);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.tlpRight.MaximumSize = new System.Drawing.Size(900, 0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 3;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 230F));
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 275F));
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Size = new System.Drawing.Size(900, 815);
            this.tlpRight.TabIndex = 1;
            // 
            // grpAvi
            // 
            this.grpAvi.Controls.Add(this.tlpAvi);
            this.grpAvi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpAvi.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.grpAvi.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.grpAvi.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.grpAvi.Location = new System.Drawing.Point(0, 0);
            this.grpAvi.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.grpAvi.MinimumSize = new System.Drawing.Size(1, 1);
            this.grpAvi.Name = "grpAvi";
            this.grpAvi.Padding = new System.Windows.Forms.Padding(8, 32, 8, 6);
            this.grpAvi.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.grpAvi.Size = new System.Drawing.Size(900, 224);
            this.grpAvi.Style = Sunny.UI.UIStyle.Custom;
            this.grpAvi.StyleCustomMode = true;
            this.grpAvi.TabIndex = 1;
            this.grpAvi.Text = "AVI（源数据库 + 回写数据库）";
            this.grpAvi.TextAlignment = System.Drawing.ContentAlignment.TopLeft;
            // 
            // tlpAvi
            // 
            this.tlpAvi.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tlpAvi.ColumnCount = 6;
            this.tlpAvi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tlpAvi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
            this.tlpAvi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tlpAvi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tlpAvi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAvi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tlpAvi.Controls.Add(this.lblIp, 0, 0);
            this.tlpAvi.Controls.Add(this.txtIp, 1, 0);
            this.tlpAvi.Controls.Add(this.lblPort, 2, 0);
            this.tlpAvi.Controls.Add(this.txtPort, 3, 0);
            this.tlpAvi.Controls.Add(this.lblAviStatus, 4, 0);
            this.tlpAvi.Controls.Add(this.btnAviTest, 5, 0);
            this.tlpAvi.Controls.Add(this.lblDbName, 0, 1);
            this.tlpAvi.Controls.Add(this.txtDbName, 1, 1);
            this.tlpAvi.Controls.Add(this.lblWriteBackDbName, 0, 2);
            this.tlpAvi.Controls.Add(this.txtWriteBackDbName, 1, 2);
            this.tlpAvi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpAvi.Location = new System.Drawing.Point(8, 32);
            this.tlpAvi.Name = "tlpAvi";
            this.tlpAvi.RowCount = 3;
            this.tlpAvi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tlpAvi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tlpAvi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAvi.Size = new System.Drawing.Size(884, 186);
            this.tlpAvi.TabIndex = 0;
            // 
            // lblIp
            // 
            this.lblIp.BackColor = System.Drawing.Color.Transparent;
            this.lblIp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblIp.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblIp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblIp.Location = new System.Drawing.Point(3, 0);
            this.lblIp.Name = "lblIp";
            this.lblIp.Size = new System.Drawing.Size(114, 55);
            this.lblIp.Style = Sunny.UI.UIStyle.Custom;
            this.lblIp.StyleCustomMode = true;
            this.lblIp.TabIndex = 0;
            this.lblIp.Text = "IP：";
            this.lblIp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtIp
            // 
            this.txtIp.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtIp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtIp.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.txtIp.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtIp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtIp.Location = new System.Drawing.Point(123, 6);
            this.txtIp.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtIp.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtIp.Name = "txtIp";
            this.txtIp.Padding = new System.Windows.Forms.Padding(5);
            this.txtIp.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.txtIp.ShowText = false;
            this.txtIp.Size = new System.Drawing.Size(184, 43);
            this.txtIp.Style = Sunny.UI.UIStyle.Custom;
            this.txtIp.StyleCustomMode = true;
            this.txtIp.TabIndex = 1;
            this.txtIp.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtIp.Watermark = "127.0.0.1";
            // 
            // lblPort
            // 
            this.lblPort.BackColor = System.Drawing.Color.Transparent;
            this.lblPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPort.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblPort.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblPort.Location = new System.Drawing.Point(313, 0);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(64, 55);
            this.lblPort.Style = Sunny.UI.UIStyle.Custom;
            this.lblPort.StyleCustomMode = true;
            this.lblPort.TabIndex = 2;
            this.lblPort.Text = "端口：";
            this.lblPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPort
            // 
            this.txtPort.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPort.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.txtPort.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtPort.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtPort.Location = new System.Drawing.Point(383, 6);
            this.txtPort.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtPort.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtPort.Name = "txtPort";
            this.txtPort.Padding = new System.Windows.Forms.Padding(5);
            this.txtPort.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.txtPort.ShowText = false;
            this.txtPort.Size = new System.Drawing.Size(114, 43);
            this.txtPort.Style = Sunny.UI.UIStyle.Custom;
            this.txtPort.StyleCustomMode = true;
            this.txtPort.TabIndex = 3;
            this.txtPort.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtPort.Watermark = "9877";
            // 
            // lblAviStatus
            // 
            this.lblAviStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblAviStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAviStatus.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblAviStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblAviStatus.Location = new System.Drawing.Point(503, 0);
            this.lblAviStatus.Name = "lblAviStatus";
            this.lblAviStatus.Size = new System.Drawing.Size(268, 55);
            this.lblAviStatus.Style = Sunny.UI.UIStyle.Custom;
            this.lblAviStatus.StyleCustomMode = true;
            this.lblAviStatus.TabIndex = 4;
            this.lblAviStatus.Text = "状态：未测试";
            this.lblAviStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnAviTest
            // 
            this.btnAviTest.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAviTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAviTest.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnAviTest.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(115)))));
            this.btnAviTest.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(70)))));
            this.btnAviTest.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAviTest.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnAviTest.Location = new System.Drawing.Point(778, 6);
            this.btnAviTest.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAviTest.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnAviTest.Name = "btnAviTest";
            this.btnAviTest.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnAviTest.Size = new System.Drawing.Size(102, 43);
            this.btnAviTest.Style = Sunny.UI.UIStyle.Custom;
            this.btnAviTest.StyleCustomMode = true;
            this.btnAviTest.TabIndex = 5;
            this.btnAviTest.Text = "测试连接";
            this.btnAviTest.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAviTest.Click += new System.EventHandler(this.btnAviTest_Click);
            // 
            // lblDbName
            // 
            this.lblDbName.BackColor = System.Drawing.Color.Transparent;
            this.lblDbName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDbName.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblDbName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblDbName.Location = new System.Drawing.Point(3, 55);
            this.lblDbName.Name = "lblDbName";
            this.lblDbName.Size = new System.Drawing.Size(114, 55);
            this.lblDbName.Style = Sunny.UI.UIStyle.Custom;
            this.lblDbName.StyleCustomMode = true;
            this.lblDbName.TabIndex = 0;
            this.lblDbName.Text = "数据库名称：";
            this.lblDbName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDbName
            // 
            this.tlpAvi.SetColumnSpan(this.txtDbName, 3);
            this.txtDbName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtDbName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDbName.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.txtDbName.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtDbName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtDbName.Location = new System.Drawing.Point(123, 61);
            this.txtDbName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtDbName.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtDbName.Name = "txtDbName";
            this.txtDbName.Padding = new System.Windows.Forms.Padding(5);
            this.txtDbName.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.txtDbName.ShowText = false;
            this.txtDbName.Size = new System.Drawing.Size(374, 43);
            this.txtDbName.Style = Sunny.UI.UIStyle.Custom;
            this.txtDbName.StyleCustomMode = true;
            this.txtDbName.TabIndex = 1;
            this.txtDbName.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtDbName.Watermark = "ai_merged_results";
            // 
            // chkIsEnabled
            // 
            this.chkIsEnabled.ActiveColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(160)))));
            this.chkIsEnabled.ActiveText = "启用";
            this.chkIsEnabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkIsEnabled.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.chkIsEnabled.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.chkIsEnabled.InActiveText = "禁用";
            this.chkIsEnabled.Location = new System.Drawing.Point(799, 243);
            this.chkIsEnabled.Margin = new System.Windows.Forms.Padding(12, 11, 0, 0);
            this.chkIsEnabled.MinimumSize = new System.Drawing.Size(1, 1);
            this.chkIsEnabled.Name = "chkIsEnabled";
            this.chkIsEnabled.Size = new System.Drawing.Size(85, 29);
            this.chkIsEnabled.Style = Sunny.UI.UIStyle.Custom;
            this.chkIsEnabled.StyleCustomMode = true;
            this.chkIsEnabled.TabIndex = 8;
            // 
            // lblWriteBackDbName
            // 
            this.lblWriteBackDbName.BackColor = System.Drawing.Color.Transparent;
            this.lblWriteBackDbName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWriteBackDbName.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblWriteBackDbName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblWriteBackDbName.Location = new System.Drawing.Point(3, 110);
            this.lblWriteBackDbName.Name = "lblWriteBackDbName";
            this.lblWriteBackDbName.Size = new System.Drawing.Size(114, 76);
            this.lblWriteBackDbName.Style = Sunny.UI.UIStyle.Custom;
            this.lblWriteBackDbName.StyleCustomMode = true;
            this.lblWriteBackDbName.TabIndex = 6;
            this.lblWriteBackDbName.Text = "回写DB：";
            this.lblWriteBackDbName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtWriteBackDbName
            // 
            this.txtWriteBackDbName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtWriteBackDbName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtWriteBackDbName.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.txtWriteBackDbName.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtWriteBackDbName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtWriteBackDbName.Location = new System.Drawing.Point(123, 116);
            this.txtWriteBackDbName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtWriteBackDbName.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtWriteBackDbName.Name = "txtWriteBackDbName";
            this.txtWriteBackDbName.Padding = new System.Windows.Forms.Padding(5);
            this.txtWriteBackDbName.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.txtWriteBackDbName.ShowText = false;
            this.txtWriteBackDbName.Size = new System.Drawing.Size(184, 64);
            this.txtWriteBackDbName.Style = Sunny.UI.UIStyle.Custom;
            this.txtWriteBackDbName.StyleCustomMode = true;
            this.txtWriteBackDbName.TabIndex = 7;
            this.txtWriteBackDbName.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtWriteBackDbName.Watermark = "filter_time_to_airesults";
            // 
            // grpVrs
            // 
            this.grpVrs.Controls.Add(this.tlpVrs);
            this.grpVrs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpVrs.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.grpVrs.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.grpVrs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.grpVrs.Location = new System.Drawing.Point(0, 230);
            this.grpVrs.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.grpVrs.MinimumSize = new System.Drawing.Size(1, 1);
            this.grpVrs.Name = "grpVrs";
            this.grpVrs.Padding = new System.Windows.Forms.Padding(8, 32, 8, 6);
            this.grpVrs.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.grpVrs.Size = new System.Drawing.Size(900, 269);
            this.grpVrs.Style = Sunny.UI.UIStyle.Custom;
            this.grpVrs.StyleCustomMode = true;
            this.grpVrs.TabIndex = 2;
            this.grpVrs.Text = "VRS（VRS 回写 + VRS V1.0 回写）";
            this.grpVrs.TextAlignment = System.Drawing.ContentAlignment.TopLeft;
            // 
            // tlpVrs
            // 
            this.tlpVrs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tlpVrs.ColumnCount = 6;
            this.tlpVrs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tlpVrs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
            this.tlpVrs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpVrs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tlpVrs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpVrs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tlpVrs.Controls.Add(this.lblVrsIp, 0, 0);
            this.tlpVrs.Controls.Add(this.txtVrsIp, 1, 0);
            this.tlpVrs.Controls.Add(this.lblVrsPort, 2, 0);
            this.tlpVrs.Controls.Add(this.txtVrsPort, 3, 0);
            this.tlpVrs.Controls.Add(this.lblVrsStatus, 4, 0);
            this.tlpVrs.Controls.Add(this.btnVrsTest, 5, 0);
            this.tlpVrs.Controls.Add(this.lblVrsWriteBackDbName, 0, 1);
            this.tlpVrs.Controls.Add(this.txtVrsWriteBackDbName, 1, 1);
            this.tlpVrs.Controls.Add(this.lblVrsWriteBackDbNameV1, 0, 2);
            this.tlpVrs.Controls.Add(this.txtVrsWriteBackDbNameV1, 1, 2);
            this.tlpVrs.Controls.Add(this.lblVrsHistoryDbName, 0, 3);
            this.tlpVrs.Controls.Add(this.txtVrsHistoryDbName, 1, 3);
            this.tlpVrs.Controls.Add(this.lblVrsTestSn, 2, 3);
            this.tlpVrs.Controls.Add(this.txtVrsTestSn, 3, 3);
            this.tlpVrs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpVrs.Location = new System.Drawing.Point(8, 32);
            this.tlpVrs.Name = "tlpVrs";
            this.tlpVrs.RowCount = 4;
            this.tlpVrs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tlpVrs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tlpVrs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tlpVrs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tlpVrs.Size = new System.Drawing.Size(884, 231);
            this.tlpVrs.TabIndex = 0;
            // 
            // lblVrsIp
            // 
            this.lblVrsIp.BackColor = System.Drawing.Color.Transparent;
            this.lblVrsIp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVrsIp.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblVrsIp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblVrsIp.Location = new System.Drawing.Point(3, 0);
            this.lblVrsIp.Name = "lblVrsIp";
            this.lblVrsIp.Size = new System.Drawing.Size(144, 55);
            this.lblVrsIp.Style = Sunny.UI.UIStyle.Custom;
            this.lblVrsIp.StyleCustomMode = true;
            this.lblVrsIp.TabIndex = 0;
            this.lblVrsIp.Text = "VRS IP：";
            this.lblVrsIp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtVrsIp
            // 
            this.txtVrsIp.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtVrsIp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtVrsIp.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.txtVrsIp.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtVrsIp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtVrsIp.Location = new System.Drawing.Point(153, 6);
            this.txtVrsIp.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtVrsIp.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtVrsIp.Name = "txtVrsIp";
            this.txtVrsIp.Padding = new System.Windows.Forms.Padding(5);
            this.txtVrsIp.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.txtVrsIp.ShowText = false;
            this.txtVrsIp.Size = new System.Drawing.Size(184, 43);
            this.txtVrsIp.Style = Sunny.UI.UIStyle.Custom;
            this.txtVrsIp.StyleCustomMode = true;
            this.txtVrsIp.TabIndex = 1;
            this.txtVrsIp.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtVrsIp.Watermark = "与AVI一致";
            // 
            // lblVrsPort
            // 
            this.lblVrsPort.BackColor = System.Drawing.Color.Transparent;
            this.lblVrsPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVrsPort.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblVrsPort.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblVrsPort.Location = new System.Drawing.Point(343, 0);
            this.lblVrsPort.Name = "lblVrsPort";
            this.lblVrsPort.Size = new System.Drawing.Size(94, 55);
            this.lblVrsPort.Style = Sunny.UI.UIStyle.Custom;
            this.lblVrsPort.StyleCustomMode = true;
            this.lblVrsPort.TabIndex = 2;
            this.lblVrsPort.Text = "端口：";
            this.lblVrsPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtVrsPort
            // 
            this.txtVrsPort.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtVrsPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtVrsPort.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.txtVrsPort.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtVrsPort.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtVrsPort.Location = new System.Drawing.Point(443, 6);
            this.txtVrsPort.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtVrsPort.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtVrsPort.Name = "txtVrsPort";
            this.txtVrsPort.Padding = new System.Windows.Forms.Padding(5);
            this.txtVrsPort.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.txtVrsPort.ShowText = false;
            this.txtVrsPort.Size = new System.Drawing.Size(144, 43);
            this.txtVrsPort.Style = Sunny.UI.UIStyle.Custom;
            this.txtVrsPort.StyleCustomMode = true;
            this.txtVrsPort.TabIndex = 3;
            this.txtVrsPort.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtVrsPort.Watermark = "与AVI一致";
            // 
            // lblVrsStatus
            // 
            this.lblVrsStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblVrsStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVrsStatus.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblVrsStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblVrsStatus.Location = new System.Drawing.Point(593, 0);
            this.lblVrsStatus.Name = "lblVrsStatus";
            this.lblVrsStatus.Size = new System.Drawing.Size(178, 55);
            this.lblVrsStatus.Style = Sunny.UI.UIStyle.Custom;
            this.lblVrsStatus.StyleCustomMode = true;
            this.lblVrsStatus.TabIndex = 4;
            this.lblVrsStatus.Text = "状态：未测试";
            this.lblVrsStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnVrsTest
            // 
            this.btnVrsTest.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnVrsTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnVrsTest.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnVrsTest.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(115)))));
            this.btnVrsTest.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(70)))));
            this.btnVrsTest.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnVrsTest.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnVrsTest.Location = new System.Drawing.Point(778, 6);
            this.btnVrsTest.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnVrsTest.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnVrsTest.Name = "btnVrsTest";
            this.btnVrsTest.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnVrsTest.Size = new System.Drawing.Size(102, 43);
            this.btnVrsTest.Style = Sunny.UI.UIStyle.Custom;
            this.btnVrsTest.StyleCustomMode = true;
            this.btnVrsTest.TabIndex = 5;
            this.btnVrsTest.Text = "测试连接";
            this.btnVrsTest.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnVrsTest.Click += new System.EventHandler(this.btnVrsTest_Click);
            // 
            // lblVrsWriteBackDbName
            // 
            this.lblVrsWriteBackDbName.BackColor = System.Drawing.Color.Transparent;
            this.lblVrsWriteBackDbName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVrsWriteBackDbName.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblVrsWriteBackDbName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblVrsWriteBackDbName.Location = new System.Drawing.Point(3, 55);
            this.lblVrsWriteBackDbName.Name = "lblVrsWriteBackDbName";
            this.lblVrsWriteBackDbName.Size = new System.Drawing.Size(144, 55);
            this.lblVrsWriteBackDbName.Style = Sunny.UI.UIStyle.Custom;
            this.lblVrsWriteBackDbName.StyleCustomMode = true;
            this.lblVrsWriteBackDbName.TabIndex = 6;
            this.lblVrsWriteBackDbName.Text = "VRS回写DB：";
            this.lblVrsWriteBackDbName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtVrsWriteBackDbName
            // 
            this.txtVrsWriteBackDbName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtVrsWriteBackDbName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtVrsWriteBackDbName.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.txtVrsWriteBackDbName.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtVrsWriteBackDbName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtVrsWriteBackDbName.Location = new System.Drawing.Point(153, 61);
            this.txtVrsWriteBackDbName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtVrsWriteBackDbName.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtVrsWriteBackDbName.Name = "txtVrsWriteBackDbName";
            this.txtVrsWriteBackDbName.Padding = new System.Windows.Forms.Padding(5);
            this.txtVrsWriteBackDbName.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.txtVrsWriteBackDbName.ShowText = false;
            this.txtVrsWriteBackDbName.Size = new System.Drawing.Size(184, 43);
            this.txtVrsWriteBackDbName.Style = Sunny.UI.UIStyle.Custom;
            this.txtVrsWriteBackDbName.StyleCustomMode = true;
            this.txtVrsWriteBackDbName.TabIndex = 7;
            this.txtVrsWriteBackDbName.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtVrsWriteBackDbName.Watermark = "ai_detail_results_tovrs";
            // 
            // lblVrsWriteBackDbNameV1
            // 
            this.lblVrsWriteBackDbNameV1.BackColor = System.Drawing.Color.Transparent;
            this.lblVrsWriteBackDbNameV1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVrsWriteBackDbNameV1.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblVrsWriteBackDbNameV1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblVrsWriteBackDbNameV1.Location = new System.Drawing.Point(3, 110);
            this.lblVrsWriteBackDbNameV1.Name = "lblVrsWriteBackDbNameV1";
            this.lblVrsWriteBackDbNameV1.Size = new System.Drawing.Size(144, 55);
            this.lblVrsWriteBackDbNameV1.Style = Sunny.UI.UIStyle.Custom;
            this.lblVrsWriteBackDbNameV1.StyleCustomMode = true;
            this.lblVrsWriteBackDbNameV1.TabIndex = 8;
            this.lblVrsWriteBackDbNameV1.Text = "V1.0回写DB：";
            this.lblVrsWriteBackDbNameV1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtVrsWriteBackDbNameV1
            // 
            this.txtVrsWriteBackDbNameV1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtVrsWriteBackDbNameV1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtVrsWriteBackDbNameV1.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.txtVrsWriteBackDbNameV1.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtVrsWriteBackDbNameV1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtVrsWriteBackDbNameV1.Location = new System.Drawing.Point(153, 116);
            this.txtVrsWriteBackDbNameV1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtVrsWriteBackDbNameV1.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtVrsWriteBackDbNameV1.Name = "txtVrsWriteBackDbNameV1";
            this.txtVrsWriteBackDbNameV1.Padding = new System.Windows.Forms.Padding(5);
            this.txtVrsWriteBackDbNameV1.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.txtVrsWriteBackDbNameV1.ShowText = false;
            this.txtVrsWriteBackDbNameV1.Size = new System.Drawing.Size(184, 43);
            this.txtVrsWriteBackDbNameV1.Style = Sunny.UI.UIStyle.Custom;
            this.txtVrsWriteBackDbNameV1.StyleCustomMode = true;
            this.txtVrsWriteBackDbNameV1.TabIndex = 9;
            this.txtVrsWriteBackDbNameV1.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtVrsWriteBackDbNameV1.Watermark = "ai_inference_result";
            // 
            // lblVrsHistoryDbName
            // 
            this.lblVrsHistoryDbName.BackColor = System.Drawing.Color.Transparent;
            this.lblVrsHistoryDbName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVrsHistoryDbName.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblVrsHistoryDbName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblVrsHistoryDbName.Location = new System.Drawing.Point(3, 165);
            this.lblVrsHistoryDbName.Name = "lblVrsHistoryDbName";
            this.lblVrsHistoryDbName.Size = new System.Drawing.Size(144, 66);
            this.lblVrsHistoryDbName.Style = Sunny.UI.UIStyle.Custom;
            this.lblVrsHistoryDbName.StyleCustomMode = true;
            this.lblVrsHistoryDbName.TabIndex = 10;
            this.lblVrsHistoryDbName.Text = "VRS历史DB：";
            this.lblVrsHistoryDbName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtVrsHistoryDbName
            // 
            this.txtVrsHistoryDbName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtVrsHistoryDbName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtVrsHistoryDbName.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.txtVrsHistoryDbName.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtVrsHistoryDbName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtVrsHistoryDbName.Location = new System.Drawing.Point(153, 171);
            this.txtVrsHistoryDbName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtVrsHistoryDbName.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtVrsHistoryDbName.Name = "txtVrsHistoryDbName";
            this.txtVrsHistoryDbName.Padding = new System.Windows.Forms.Padding(5);
            this.txtVrsHistoryDbName.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.txtVrsHistoryDbName.ShowText = false;
            this.txtVrsHistoryDbName.Size = new System.Drawing.Size(184, 54);
            this.txtVrsHistoryDbName.Style = Sunny.UI.UIStyle.Custom;
            this.txtVrsHistoryDbName.StyleCustomMode = true;
            this.txtVrsHistoryDbName.TabIndex = 11;
            this.txtVrsHistoryDbName.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtVrsHistoryDbName.Watermark = "vrs_history_result";
            // 
            // lblVrsTestSn
            // 
            this.lblVrsTestSn.BackColor = System.Drawing.Color.Transparent;
            this.lblVrsTestSn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVrsTestSn.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblVrsTestSn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblVrsTestSn.Location = new System.Drawing.Point(343, 165);
            this.lblVrsTestSn.Name = "lblVrsTestSn";
            this.lblVrsTestSn.Size = new System.Drawing.Size(94, 66);
            this.lblVrsTestSn.Style = Sunny.UI.UIStyle.Custom;
            this.lblVrsTestSn.StyleCustomMode = true;
            this.lblVrsTestSn.TabIndex = 12;
            this.lblVrsTestSn.Text = "测试SN：";
            this.lblVrsTestSn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtVrsTestSn
            // 
            this.txtVrsTestSn.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtVrsTestSn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtVrsTestSn.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.txtVrsTestSn.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtVrsTestSn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtVrsTestSn.Location = new System.Drawing.Point(443, 171);
            this.txtVrsTestSn.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtVrsTestSn.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtVrsTestSn.Name = "txtVrsTestSn";
            this.txtVrsTestSn.Padding = new System.Windows.Forms.Padding(5);
            this.txtVrsTestSn.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.txtVrsTestSn.ShowText = false;
            this.txtVrsTestSn.Size = new System.Drawing.Size(144, 54);
            this.txtVrsTestSn.Style = Sunny.UI.UIStyle.Custom;
            this.txtVrsTestSn.StyleCustomMode = true;
            this.txtVrsTestSn.TabIndex = 13;
            this.txtVrsTestSn.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtVrsTestSn.Watermark = "留空仅测连接";
            // 
            // grpMinio
            // 
            this.grpMinio.Controls.Add(this.tlpMinio);
            this.grpMinio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMinio.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.grpMinio.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.grpMinio.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.grpMinio.Location = new System.Drawing.Point(0, 505);
            this.grpMinio.Margin = new System.Windows.Forms.Padding(0);
            this.grpMinio.MinimumSize = new System.Drawing.Size(1, 1);
            this.grpMinio.Name = "grpMinio";
            this.grpMinio.Padding = new System.Windows.Forms.Padding(8, 32, 8, 6);
            this.grpMinio.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.grpMinio.Size = new System.Drawing.Size(900, 310);
            this.grpMinio.Style = Sunny.UI.UIStyle.Custom;
            this.grpMinio.StyleCustomMode = true;
            this.grpMinio.TabIndex = 3;
            this.grpMinio.Text = "MinIO";
            this.grpMinio.TextAlignment = System.Drawing.ContentAlignment.TopLeft;
            // 
            // tlpMinio
            // 
            this.tlpMinio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tlpMinio.ColumnCount = 4;
            this.tlpMinio.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpMinio.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.tlpMinio.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMinio.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tlpMinio.Controls.Add(this.lblMinioA, 0, 0);
            this.tlpMinio.Controls.Add(this.txtMinioIpA, 1, 0);
            this.tlpMinio.Controls.Add(this.lblMinioStatusA, 2, 0);
            this.tlpMinio.Controls.Add(this.btnMinioTestA, 3, 0);
            this.tlpMinio.Controls.Add(this.lblMinioB, 0, 1);
            this.tlpMinio.Controls.Add(this.txtMinioIpB, 1, 1);
            this.tlpMinio.Controls.Add(this.lblMinioStatusB, 2, 1);
            this.tlpMinio.Controls.Add(this.btnMinioTestB, 3, 1);
            this.tlpMinio.Controls.Add(this.chkIsEnabled, 3, 2);
            this.tlpMinio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMinio.Location = new System.Drawing.Point(8, 32);
            this.tlpMinio.Name = "tlpMinio";
            this.tlpMinio.RowCount = 3;
            this.tlpMinio.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tlpMinio.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tlpMinio.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMinio.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMinio.Size = new System.Drawing.Size(884, 272);
            this.tlpMinio.TabIndex = 0;
            // 
            // lblMinioA
            // 
            this.lblMinioA.BackColor = System.Drawing.Color.Transparent;
            this.lblMinioA.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMinioA.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblMinioA.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblMinioA.Location = new System.Drawing.Point(3, 0);
            this.lblMinioA.Name = "lblMinioA";
            this.lblMinioA.Size = new System.Drawing.Size(94, 55);
            this.lblMinioA.Style = Sunny.UI.UIStyle.Custom;
            this.lblMinioA.StyleCustomMode = true;
            this.lblMinioA.TabIndex = 0;
            this.lblMinioA.Text = "A 面 IP：";
            this.lblMinioA.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtMinioIpA
            // 
            this.txtMinioIpA.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMinioIpA.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMinioIpA.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.txtMinioIpA.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtMinioIpA.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtMinioIpA.Location = new System.Drawing.Point(103, 6);
            this.txtMinioIpA.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtMinioIpA.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtMinioIpA.Name = "txtMinioIpA";
            this.txtMinioIpA.Padding = new System.Windows.Forms.Padding(5);
            this.txtMinioIpA.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.txtMinioIpA.ShowText = false;
            this.txtMinioIpA.Size = new System.Drawing.Size(244, 43);
            this.txtMinioIpA.Style = Sunny.UI.UIStyle.Custom;
            this.txtMinioIpA.StyleCustomMode = true;
            this.txtMinioIpA.TabIndex = 1;
            this.txtMinioIpA.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtMinioIpA.Watermark = "127.0.0.1";
            // 
            // lblMinioStatusA
            // 
            this.lblMinioStatusA.BackColor = System.Drawing.Color.Transparent;
            this.lblMinioStatusA.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMinioStatusA.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblMinioStatusA.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblMinioStatusA.Location = new System.Drawing.Point(353, 0);
            this.lblMinioStatusA.Name = "lblMinioStatusA";
            this.lblMinioStatusA.Size = new System.Drawing.Size(418, 55);
            this.lblMinioStatusA.Style = Sunny.UI.UIStyle.Custom;
            this.lblMinioStatusA.StyleCustomMode = true;
            this.lblMinioStatusA.TabIndex = 2;
            this.lblMinioStatusA.Text = "状态：未测试";
            this.lblMinioStatusA.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnMinioTestA
            // 
            this.btnMinioTestA.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMinioTestA.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMinioTestA.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnMinioTestA.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(115)))));
            this.btnMinioTestA.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(70)))));
            this.btnMinioTestA.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnMinioTestA.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnMinioTestA.Location = new System.Drawing.Point(778, 6);
            this.btnMinioTestA.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnMinioTestA.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnMinioTestA.Name = "btnMinioTestA";
            this.btnMinioTestA.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnMinioTestA.Size = new System.Drawing.Size(102, 43);
            this.btnMinioTestA.Style = Sunny.UI.UIStyle.Custom;
            this.btnMinioTestA.StyleCustomMode = true;
            this.btnMinioTestA.TabIndex = 3;
            this.btnMinioTestA.Text = "测试 A 面";
            this.btnMinioTestA.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnMinioTestA.Click += new System.EventHandler(this.btnMinioTestA_Click);
            // 
            // lblMinioB
            // 
            this.lblMinioB.BackColor = System.Drawing.Color.Transparent;
            this.lblMinioB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMinioB.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblMinioB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblMinioB.Location = new System.Drawing.Point(3, 55);
            this.lblMinioB.Name = "lblMinioB";
            this.lblMinioB.Size = new System.Drawing.Size(94, 55);
            this.lblMinioB.Style = Sunny.UI.UIStyle.Custom;
            this.lblMinioB.StyleCustomMode = true;
            this.lblMinioB.TabIndex = 4;
            this.lblMinioB.Text = "B 面 IP：";
            this.lblMinioB.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtMinioIpB
            // 
            this.txtMinioIpB.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMinioIpB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMinioIpB.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.txtMinioIpB.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtMinioIpB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.txtMinioIpB.Location = new System.Drawing.Point(103, 61);
            this.txtMinioIpB.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.txtMinioIpB.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtMinioIpB.Name = "txtMinioIpB";
            this.txtMinioIpB.Padding = new System.Windows.Forms.Padding(5);
            this.txtMinioIpB.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.txtMinioIpB.ShowText = false;
            this.txtMinioIpB.Size = new System.Drawing.Size(244, 43);
            this.txtMinioIpB.Style = Sunny.UI.UIStyle.Custom;
            this.txtMinioIpB.StyleCustomMode = true;
            this.txtMinioIpB.TabIndex = 5;
            this.txtMinioIpB.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtMinioIpB.Watermark = "127.0.0.1";
            // 
            // lblMinioStatusB
            // 
            this.lblMinioStatusB.BackColor = System.Drawing.Color.Transparent;
            this.lblMinioStatusB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMinioStatusB.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblMinioStatusB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblMinioStatusB.Location = new System.Drawing.Point(353, 55);
            this.lblMinioStatusB.Name = "lblMinioStatusB";
            this.lblMinioStatusB.Size = new System.Drawing.Size(418, 55);
            this.lblMinioStatusB.Style = Sunny.UI.UIStyle.Custom;
            this.lblMinioStatusB.StyleCustomMode = true;
            this.lblMinioStatusB.TabIndex = 6;
            this.lblMinioStatusB.Text = "状态：未测试";
            this.lblMinioStatusB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnMinioTestB
            // 
            this.btnMinioTestB.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMinioTestB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMinioTestB.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnMinioTestB.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(115)))));
            this.btnMinioTestB.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(70)))));
            this.btnMinioTestB.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnMinioTestB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnMinioTestB.Location = new System.Drawing.Point(778, 61);
            this.btnMinioTestB.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnMinioTestB.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnMinioTestB.Name = "btnMinioTestB";
            this.btnMinioTestB.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnMinioTestB.Size = new System.Drawing.Size(102, 43);
            this.btnMinioTestB.Style = Sunny.UI.UIStyle.Custom;
            this.btnMinioTestB.StyleCustomMode = true;
            this.btnMinioTestB.TabIndex = 7;
            this.btnMinioTestB.Text = "测试 B 面";
            this.btnMinioTestB.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnMinioTestB.Click += new System.EventHandler(this.btnMinioTestB_Click);
            // 
            // PgSettingDatabase
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.ClientSize = new System.Drawing.Size(1263, 831);
            this.Controls.Add(this.tlpMain);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Name = "PgSettingDatabase";
            this.Load += new System.EventHandler(this.FrLevelDbConfig_Load);
            this.tlpMain.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.tlpListButtons.ResumeLayout(false);
            this.tlpRight.ResumeLayout(false);
            this.grpAvi.ResumeLayout(false);
            this.tlpAvi.ResumeLayout(false);
            this.grpVrs.ResumeLayout(false);
            this.tlpVrs.ResumeLayout(false);
            this.grpMinio.ResumeLayout(false);
            this.tlpMinio.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private Sunny.UI.UIPanel pnlLeft;
        private Sunny.UI.UILabel lblListTitle;
        private Sunny.UI.UIListBox lstDatabases;
        private System.Windows.Forms.TableLayoutPanel tlpListButtons;
        private Sunny.UI.UIButton btnAdd;
        private Sunny.UI.UIButton btnDelete;
        private Sunny.UI.UIButton btnTestAll;
        private System.Windows.Forms.TableLayoutPanel tlpRight;
        private Sunny.UI.UILabel lblDbName;
        private Sunny.UI.UITextBox txtDbName;
        private Sunny.UI.UISwitch chkIsEnabled;
        private Sunny.UI.UIGroupBox grpAvi;
        private System.Windows.Forms.TableLayoutPanel tlpAvi;
        private Sunny.UI.UILabel lblIp;
        private Sunny.UI.UITextBox txtIp;
        private Sunny.UI.UILabel lblPort;
        private Sunny.UI.UITextBox txtPort;
        private Sunny.UI.UILabel lblWriteBackDbName;
        private Sunny.UI.UITextBox txtWriteBackDbName;
        private Sunny.UI.UILabel lblAviStatus;
        private Sunny.UI.UIButton btnAviTest;
        private Sunny.UI.UIGroupBox grpVrs;
        private System.Windows.Forms.TableLayoutPanel tlpVrs;
        private Sunny.UI.UILabel lblVrsIp;
        private Sunny.UI.UITextBox txtVrsIp;
        private Sunny.UI.UILabel lblVrsPort;
        private Sunny.UI.UITextBox txtVrsPort;
        private Sunny.UI.UILabel lblVrsWriteBackDbName;
        private Sunny.UI.UITextBox txtVrsWriteBackDbName;
        private Sunny.UI.UILabel lblVrsWriteBackDbNameV1;
        private Sunny.UI.UITextBox txtVrsWriteBackDbNameV1;
        private Sunny.UI.UILabel lblVrsHistoryDbName;
        private Sunny.UI.UITextBox txtVrsHistoryDbName;
        private Sunny.UI.UILabel lblVrsTestSn;
        private Sunny.UI.UITextBox txtVrsTestSn;
        private Sunny.UI.UILabel lblVrsStatus;
        private Sunny.UI.UIButton btnVrsTest;
        private Sunny.UI.UIGroupBox grpMinio;
        private System.Windows.Forms.TableLayoutPanel tlpMinio;
        private Sunny.UI.UILabel lblMinioA;
        private Sunny.UI.UITextBox txtMinioIpA;
        private Sunny.UI.UILabel lblMinioStatusA;
        private Sunny.UI.UIButton btnMinioTestA;
        private Sunny.UI.UILabel lblMinioB;
        private Sunny.UI.UITextBox txtMinioIpB;
        private Sunny.UI.UILabel lblMinioStatusB;
        private Sunny.UI.UIButton btnMinioTestB;
    }
}



namespace DeepSightAI
{
    partial class DlgMinioFolderPicker
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
            this.panelTop = new Sunny.UI.UIPanel();
            this.uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            this.cmbIp = new Sunny.UI.UIComboBox();
            this.lblIp = new Sunny.UI.UILabel();
            this.tree = new Sunny.UI.UITreeView();
            this.panelBottom = new Sunny.UI.UIPanel();
            this.lblStatus = new Sunny.UI.UILabel();
            this.btnCancel = new Sunny.UI.UISymbolButton();
            this.btnOk = new Sunny.UI.UISymbolButton();
            this.panelTop.SuspendLayout();
            this.uiTableLayoutPanel1.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.uiTableLayoutPanel1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panelTop.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelTop.MinimumSize = new System.Drawing.Size(1, 1);
            this.panelTop.Name = "panelTop";
            this.panelTop.RectSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.None;
            this.panelTop.Size = new System.Drawing.Size(480, 48);
            this.panelTop.Style = Sunny.UI.UIStyle.Custom;
            this.panelTop.TabIndex = 0;
            this.panelTop.Text = null;
            this.panelTop.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel1
            // 
            this.uiTableLayoutPanel1.ColumnCount = 2;
            this.uiTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28.33333F));
            this.uiTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 71.66666F));
            this.uiTableLayoutPanel1.Controls.Add(this.cmbIp, 1, 0);
            this.uiTableLayoutPanel1.Controls.Add(this.lblIp, 0, 0);
            this.uiTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiTableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            this.uiTableLayoutPanel1.RowCount = 1;
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.uiTableLayoutPanel1.Size = new System.Drawing.Size(480, 48);
            this.uiTableLayoutPanel1.TabIndex = 0;
            this.uiTableLayoutPanel1.TagString = null;
            // 
            // cmbIp
            // 
            this.cmbIp.DataSource = null;
            this.cmbIp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbIp.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbIp.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(19)))), ((int)(((byte)(31)))));
            this.cmbIp.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cmbIp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.cmbIp.ItemHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.cmbIp.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.cmbIp.Location = new System.Drawing.Point(140, 5);
            this.cmbIp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbIp.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbIp.Name = "cmbIp";
            this.cmbIp.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cmbIp.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.cmbIp.Size = new System.Drawing.Size(336, 38);
            this.cmbIp.Style = Sunny.UI.UIStyle.Custom;
            this.cmbIp.SymbolSize = 24;
            this.cmbIp.TabIndex = 1;
            this.cmbIp.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmbIp.Watermark = "";
            // 
            // lblIp
            // 
            this.lblIp.AutoSize = true;
            this.lblIp.BackColor = System.Drawing.Color.Transparent;
            this.lblIp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblIp.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblIp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblIp.Location = new System.Drawing.Point(3, 0);
            this.lblIp.Name = "lblIp";
            this.lblIp.Size = new System.Drawing.Size(108, 40);
            this.lblIp.Style = Sunny.UI.UIStyle.Custom;
            this.lblIp.TabIndex = 0;
            this.lblIp.Text = "MinIO IP:";
            this.lblIp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tree
            // 
            this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tree.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tree.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tree.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.tree.HideSelection = false;
            this.tree.Location = new System.Drawing.Point(0, 48);
            this.tree.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tree.MinimumSize = new System.Drawing.Size(1, 1);
            this.tree.Name = "tree";
            this.tree.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.tree.ScrollBarStyleInherited = false;
            this.tree.ShowText = false;
            this.tree.Size = new System.Drawing.Size(480, 416);
            this.tree.Style = Sunny.UI.UIStyle.Custom;
            this.tree.TabIndex = 1;
            this.tree.Text = null;
            this.tree.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.lblStatus);
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Controls.Add(this.btnOk);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panelBottom.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.panelBottom.Location = new System.Drawing.Point(0, 464);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelBottom.MinimumSize = new System.Drawing.Size(1, 1);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.RectSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.None;
            this.panelBottom.Size = new System.Drawing.Size(480, 56);
            this.panelBottom.Style = Sunny.UI.UIStyle.Custom;
            this.panelBottom.TabIndex = 2;
            this.panelBottom.Text = null;
            this.panelBottom.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblStatus.Location = new System.Drawing.Point(12, 20);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(278, 20);
            this.lblStatus.Style = Sunny.UI.UIStyle.Custom;
            this.lblStatus.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnCancel.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(86)))), ((int)(((byte)(110)))));
            this.btnCancel.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnCancel.Location = new System.Drawing.Point(390, 14);
            this.btnCancel.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.btnCancel.Size = new System.Drawing.Size(78, 30);
            this.btnCancel.Style = Sunny.UI.UIStyle.Custom;
            this.btnCancel.Symbol = 61453;
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "取消";
            this.btnCancel.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnOk.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(86)))), ((int)(((byte)(110)))));
            this.btnOk.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnOk.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnOk.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnOk.Location = new System.Drawing.Point(306, 14);
            this.btnOk.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnOk.Name = "btnOk";
            this.btnOk.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.btnOk.Size = new System.Drawing.Size(78, 30);
            this.btnOk.Style = Sunny.UI.UIStyle.Custom;
            this.btnOk.Symbol = 61452;
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "确定";
            this.btnOk.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // DlgMinioFolderPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            this.ClientSize = new System.Drawing.Size(480, 520);
            this.Controls.Add(this.tree);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(360, 360);
            this.Name = "DlgMinioFolderPicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "选择 MinIO 目录";
            this.panelTop.ResumeLayout(false);
            this.uiTableLayoutPanel1.ResumeLayout(false);
            this.uiTableLayoutPanel1.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UIPanel panelTop;
        private Sunny.UI.UILabel lblIp;
        private Sunny.UI.UIComboBox cmbIp;
        private Sunny.UI.UITreeView tree;
        private Sunny.UI.UIPanel panelBottom;
        private Sunny.UI.UILabel lblStatus;
        private Sunny.UI.UISymbolButton btnOk;
        private Sunny.UI.UISymbolButton btnCancel;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
    }
}

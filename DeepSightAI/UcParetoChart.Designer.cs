namespace DeepSightAI
{
    partial class UcParetoChart
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
            System.Drawing.Color themeBg = System.Drawing.Color.FromArgb(29, 48, 60);
            System.Drawing.Color themeFg = System.Drawing.Color.FromArgb(200, 200, 200);
            System.Drawing.Font themeFont = new System.Drawing.Font("微软雅黑", 9F);
            System.Drawing.Font themeFontBold = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);

            this.topToolbar = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_TopLeft = new System.Windows.Forms.Label();
            this.cmb_TopLeft = new System.Windows.Forms.ComboBox();
            this.lbl_TopRight = new System.Windows.Forms.Label();
            this.cmb_TopRight = new System.Windows.Forms.ComboBox();
            this.lbl_BottomSource = new System.Windows.Forms.Label();
            this.rdo_VVS = new System.Windows.Forms.RadioButton();
            this.rdo_VRS = new System.Windows.Forms.RadioButton();
            this.chk_OriginName = new System.Windows.Forms.CheckBox();
            this.btn_Refresh = new DeepSightAI.StyledButton();
            this.panel_Bottom = new System.Windows.Forms.Panel();
            this.label_Status = new System.Windows.Forms.Label();
            this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.chart_TopLeft = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart_TopRight = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart_BottomLeft = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart_BottomRight = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.topToolbar.SuspendLayout();
            this.panel_Bottom.SuspendLayout();
            this.tableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart_TopLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart_TopRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart_BottomLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart_BottomRight)).BeginInit();
            this.SuspendLayout();
            //
            // topToolbar (TableLayoutPanel: 1 row x 9 columns)
            //
            this.topToolbar.BackColor = themeBg;
            this.topToolbar.ColumnCount = 9;
            this.topToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize)); // 左上:
            this.topToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95F)); // cmb_TopLeft
            this.topToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize)); // 右上:
            this.topToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95F)); // cmb_TopRight
            this.topToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize)); // 下方对比:
            this.topToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize)); // VVS
            this.topToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize)); // VRS
            this.topToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize)); // 原始缺陷名
            this.topToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize)); // 刷新
            this.topToolbar.RowCount = 1;
            this.topToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.topToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.topToolbar.Location = new System.Drawing.Point(0, 0);
            this.topToolbar.Name = "topToolbar";
            this.topToolbar.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.topToolbar.Size = new System.Drawing.Size(1000, 38);
            this.topToolbar.TabIndex = 0;
            this.topToolbar.Controls.Add(this.lbl_TopLeft, 0, 0);
            this.topToolbar.Controls.Add(this.cmb_TopLeft, 1, 0);
            this.topToolbar.Controls.Add(this.lbl_TopRight, 2, 0);
            this.topToolbar.Controls.Add(this.cmb_TopRight, 3, 0);
            this.topToolbar.Controls.Add(this.lbl_BottomSource, 4, 0);
            this.topToolbar.Controls.Add(this.rdo_VVS, 5, 0);
            this.topToolbar.Controls.Add(this.rdo_VRS, 6, 0);
            this.topToolbar.Controls.Add(this.chk_OriginName, 7, 0);
            this.topToolbar.Controls.Add(this.btn_Refresh, 8, 0);
            //
            // lbl_TopLeft
            //
            this.lbl_TopLeft.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbl_TopLeft.AutoSize = true;
            this.lbl_TopLeft.Font = themeFontBold;
            this.lbl_TopLeft.ForeColor = themeFg;
            this.lbl_TopLeft.Name = "lbl_TopLeft";
            this.lbl_TopLeft.Text = "左上:";
            this.lbl_TopLeft.Margin = new System.Windows.Forms.Padding(4, 0, 2, 0);
            //
            // cmb_TopLeft
            //
            this.cmb_TopLeft.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmb_TopLeft.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_TopLeft.Font = themeFont;
            this.cmb_TopLeft.Name = "cmb_TopLeft";
            this.cmb_TopLeft.Size = new System.Drawing.Size(88, 25);
            this.cmb_TopLeft.TabIndex = 1;
            this.cmb_TopLeft.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            //
            // lbl_TopRight
            //
            this.lbl_TopRight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbl_TopRight.AutoSize = true;
            this.lbl_TopRight.Font = themeFontBold;
            this.lbl_TopRight.ForeColor = themeFg;
            this.lbl_TopRight.Name = "lbl_TopRight";
            this.lbl_TopRight.Text = "右上:";
            this.lbl_TopRight.Margin = new System.Windows.Forms.Padding(4, 0, 2, 0);
            //
            // cmb_TopRight
            //
            this.cmb_TopRight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmb_TopRight.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_TopRight.Font = themeFont;
            this.cmb_TopRight.Name = "cmb_TopRight";
            this.cmb_TopRight.Size = new System.Drawing.Size(88, 25);
            this.cmb_TopRight.TabIndex = 2;
            this.cmb_TopRight.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            //
            // lbl_BottomSource
            //
            this.lbl_BottomSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbl_BottomSource.AutoSize = true;
            this.lbl_BottomSource.Font = themeFontBold;
            this.lbl_BottomSource.ForeColor = themeFg;
            this.lbl_BottomSource.Name = "lbl_BottomSource";
            this.lbl_BottomSource.Text = "下方对比:";
            this.lbl_BottomSource.Margin = new System.Windows.Forms.Padding(8, 0, 2, 0);
            //
            // rdo_VVS
            //
            this.rdo_VVS.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.rdo_VVS.AutoSize = true;
            this.rdo_VVS.Checked = true;
            this.rdo_VVS.Font = themeFontBold;
            this.rdo_VVS.ForeColor = System.Drawing.Color.FromArgb(255, 107, 107);
            this.rdo_VVS.Name = "rdo_VVS";
            this.rdo_VVS.TabIndex = 3;
            this.rdo_VVS.TabStop = true;
            this.rdo_VVS.Text = "VVS";
            this.rdo_VVS.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            //
            // rdo_VRS
            //
            this.rdo_VRS.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.rdo_VRS.AutoSize = true;
            this.rdo_VRS.Font = themeFontBold;
            this.rdo_VRS.ForeColor = System.Drawing.Color.FromArgb(81, 207, 102);
            this.rdo_VRS.Name = "rdo_VRS";
            this.rdo_VRS.TabIndex = 4;
            this.rdo_VRS.Text = "VRS";
            this.rdo_VRS.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            //
            // chk_OriginName
            //
            this.chk_OriginName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chk_OriginName.AutoSize = true;
            this.chk_OriginName.Font = themeFontBold;
            this.chk_OriginName.ForeColor = themeFg;
            this.chk_OriginName.Name = "chk_OriginName";
            this.chk_OriginName.TabIndex = 5;
            this.chk_OriginName.Text = "原始缺陷名";
            this.chk_OriginName.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            //
            // btn_Refresh
            //
            this.btn_Refresh.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.Size = new System.Drawing.Size(68, 28);
            this.btn_Refresh.TabIndex = 6;
            this.btn_Refresh.Text = "刷新";
            this.btn_Refresh.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            //
            // tableLayout
            //
            this.tableLayout.ColumnCount = 2;
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout.RowCount = 2;
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayout.Location = new System.Drawing.Point(0, 40);
            this.tableLayout.Name = "tableLayout";
            this.tableLayout.BackColor = themeBg;
            this.tableLayout.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayout.Controls.Add(this.chart_TopLeft, 0, 0);
            this.tableLayout.Controls.Add(this.chart_TopRight, 1, 0);
            this.tableLayout.Controls.Add(this.chart_BottomLeft, 0, 1);
            this.tableLayout.Controls.Add(this.chart_BottomRight, 1, 1);
            this.tableLayout.Size = new System.Drawing.Size(1000, 530);
            this.tableLayout.TabIndex = 1;
            //
            // chart_TopLeft
            //
            this.chart_TopLeft.BackColor = themeBg;
            this.chart_TopLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart_TopLeft.Name = "chart_TopLeft";
            this.chart_TopLeft.TabIndex = 0;
            //
            // chart_TopRight
            //
            this.chart_TopRight.BackColor = themeBg;
            this.chart_TopRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart_TopRight.Name = "chart_TopRight";
            this.chart_TopRight.TabIndex = 1;
            //
            // chart_BottomLeft
            //
            this.chart_BottomLeft.BackColor = themeBg;
            this.chart_BottomLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart_BottomLeft.Name = "chart_BottomLeft";
            this.chart_BottomLeft.TabIndex = 2;
            //
            // chart_BottomRight
            //
            this.chart_BottomRight.BackColor = themeBg;
            this.chart_BottomRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart_BottomRight.Name = "chart_BottomRight";
            this.chart_BottomRight.TabIndex = 3;
            //
            // panel_Bottom
            //
            this.panel_Bottom.BackColor = themeBg;
            this.panel_Bottom.Controls.Add(this.label_Status);
            this.panel_Bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_Bottom.Location = new System.Drawing.Point(0, 570);
            this.panel_Bottom.Name = "panel_Bottom";
            this.panel_Bottom.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.panel_Bottom.Size = new System.Drawing.Size(1000, 30);
            this.panel_Bottom.TabIndex = 2;
            //
            // label_Status
            //
            this.label_Status.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_Status.Font = themeFont;
            this.label_Status.ForeColor = themeFg;
            this.label_Status.Location = new System.Drawing.Point(10, 4);
            this.label_Status.Name = "label_Status";
            this.label_Status.Size = new System.Drawing.Size(980, 22);
            this.label_Status.TabIndex = 0;
            this.label_Status.Text = "无数据，请先执行查询";
            this.label_Status.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // UcParetoChart
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = themeBg;
            this.Controls.Add(this.tableLayout);
            this.Controls.Add(this.panel_Bottom);
            this.Controls.Add(this.topToolbar);
            this.Font = themeFont;
            this.Name = "UcParetoChart";
            this.Size = new System.Drawing.Size(1000, 600);
            this.topToolbar.ResumeLayout(false);
            this.topToolbar.PerformLayout();
            this.panel_Bottom.ResumeLayout(false);
            this.tableLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart_TopLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart_TopRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart_BottomLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart_BottomRight)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel topToolbar;
        private System.Windows.Forms.Label lbl_TopLeft;
        private System.Windows.Forms.ComboBox cmb_TopLeft;
        private System.Windows.Forms.Label lbl_TopRight;
        private System.Windows.Forms.ComboBox cmb_TopRight;
        private System.Windows.Forms.Label lbl_BottomSource;
        private System.Windows.Forms.RadioButton rdo_VVS;
        private System.Windows.Forms.RadioButton rdo_VRS;
        private DeepSightAI.StyledButton btn_Refresh;
        private System.Windows.Forms.CheckBox chk_OriginName;
        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_TopLeft;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_TopRight;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_BottomLeft;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_BottomRight;
        private System.Windows.Forms.Panel panel_Bottom;
        private System.Windows.Forms.Label label_Status;
    }
}
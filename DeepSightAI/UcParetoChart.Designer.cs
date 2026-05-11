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
            this.panel_Top = new System.Windows.Forms.Panel();
            this.btn_Refresh = new DeepSightAI.StyledButton();
            this.cmb_Lot = new System.Windows.Forms.ComboBox();
            this.label_LotTitle = new System.Windows.Forms.Label();
            this.panel_Bottom = new System.Windows.Forms.Panel();
            this.label_Status = new System.Windows.Forms.Label();
            this.chart_Pareto = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panel_Top.SuspendLayout();
            this.panel_Bottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart_Pareto)).BeginInit();
            this.SuspendLayout();
            //
            // panel_Top
            //
            this.panel_Top.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel_Top.Controls.Add(this.btn_Refresh);
            this.panel_Top.Controls.Add(this.cmb_Lot);
            this.panel_Top.Controls.Add(this.label_LotTitle);
            this.panel_Top.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_Top.Location = new System.Drawing.Point(0, 0);
            this.panel_Top.Name = "panel_Top";
            this.panel_Top.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.panel_Top.Size = new System.Drawing.Size(1000, 44);
            this.panel_Top.TabIndex = 0;
            //
            // label_LotTitle
            //
            this.label_LotTitle.AutoSize = true;
            this.label_LotTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.label_LotTitle.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.label_LotTitle.ForeColor = System.Drawing.Color.White;
            this.label_LotTitle.Location = new System.Drawing.Point(10, 6);
            this.label_LotTitle.Name = "label_LotTitle";
            this.label_LotTitle.Padding = new System.Windows.Forms.Padding(0, 6, 8, 0);
            this.label_LotTitle.Text = "选择 Lot:";
            //
            // cmb_Lot
            //
            this.cmb_Lot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            this.cmb_Lot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Lot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmb_Lot.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmb_Lot.ForeColor = System.Drawing.Color.White;
            this.cmb_Lot.Location = new System.Drawing.Point(95, 9);
            this.cmb_Lot.Name = "cmb_Lot";
            this.cmb_Lot.Size = new System.Drawing.Size(320, 26);
            this.cmb_Lot.TabIndex = 0;
            //
            // btn_Refresh
            //
            this.btn_Refresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btn_Refresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Refresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Refresh.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btn_Refresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btn_Refresh.Location = new System.Drawing.Point(425, 8);
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.Size = new System.Drawing.Size(80, 28);
            this.btn_Refresh.TabIndex = 1;
            this.btn_Refresh.Text = "刷新";
            this.btn_Refresh.UseVisualStyleBackColor = false;
            //
            // panel_Bottom
            //
            this.panel_Bottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel_Bottom.Controls.Add(this.label_Status);
            this.panel_Bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_Bottom.Location = new System.Drawing.Point(0, 570);
            this.panel_Bottom.Name = "panel_Bottom";
            this.panel_Bottom.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.panel_Bottom.Size = new System.Drawing.Size(1000, 30);
            this.panel_Bottom.TabIndex = 1;
            //
            // label_Status
            //
            this.label_Status.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_Status.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label_Status.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.label_Status.Location = new System.Drawing.Point(10, 4);
            this.label_Status.Name = "label_Status";
            this.label_Status.Size = new System.Drawing.Size(980, 22);
            this.label_Status.TabIndex = 0;
            this.label_Status.Text = "无数据，请先执行查询";
            this.label_Status.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // chart_Pareto
            //
            this.chart_Pareto.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.chart_Pareto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart_Pareto.Location = new System.Drawing.Point(0, 44);
            this.chart_Pareto.Name = "chart_Pareto";
            this.chart_Pareto.Size = new System.Drawing.Size(1000, 526);
            this.chart_Pareto.TabIndex = 2;
            //
            // UcParetoChart
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.Controls.Add(this.chart_Pareto);
            this.Controls.Add(this.panel_Bottom);
            this.Controls.Add(this.panel_Top);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Name = "UcParetoChart";
            this.Size = new System.Drawing.Size(1000, 600);
            this.panel_Top.ResumeLayout(false);
            this.panel_Top.PerformLayout();
            this.panel_Bottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart_Pareto)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel_Top;
        private System.Windows.Forms.Label label_LotTitle;
        private System.Windows.Forms.ComboBox cmb_Lot;
        private DeepSightAI.StyledButton btn_Refresh;
        private System.Windows.Forms.Panel panel_Bottom;
        private System.Windows.Forms.Label label_Status;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_Pareto;
    }
}

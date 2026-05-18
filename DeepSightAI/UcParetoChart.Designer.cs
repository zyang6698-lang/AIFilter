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
            this.chk_AI = new System.Windows.Forms.CheckBox();
            this.chk_VVS = new System.Windows.Forms.CheckBox();
            this.chk_VRS = new System.Windows.Forms.CheckBox();
            this.btn_Refresh = new DeepSightAI.StyledButton();
            this.lbl_AIStatus = new System.Windows.Forms.Label();
            this.lbl_VVSStatus = new System.Windows.Forms.Label();
            this.lbl_VRSStatus = new System.Windows.Forms.Label();
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
            this.panel_Top.Controls.Add(this.lbl_VRSStatus);
            this.panel_Top.Controls.Add(this.lbl_VVSStatus);
            this.panel_Top.Controls.Add(this.lbl_AIStatus);
            this.panel_Top.Controls.Add(this.btn_Refresh);
            this.panel_Top.Controls.Add(this.chk_VRS);
            this.panel_Top.Controls.Add(this.chk_VVS);
            this.panel_Top.Controls.Add(this.chk_AI);
            this.panel_Top.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_Top.Location = new System.Drawing.Point(0, 0);
            this.panel_Top.Name = "panel_Top";
            this.panel_Top.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.panel_Top.Size = new System.Drawing.Size(1000, 44);
            this.panel_Top.TabIndex = 0;
            //
            // chk_AI
            //
            this.chk_AI.AutoSize = true;
            this.chk_AI.Checked = true;
            this.chk_AI.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_AI.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.chk_AI.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.chk_AI.Location = new System.Drawing.Point(18, 10);
            this.chk_AI.Name = "chk_AI";
            this.chk_AI.Size = new System.Drawing.Size(75, 23);
            this.chk_AI.TabIndex = 2;
            this.chk_AI.Text = "AI结果";
            this.chk_AI.UseVisualStyleBackColor = true;
            //
            // chk_VVS
            //
            this.chk_VVS.AutoSize = true;
            this.chk_VVS.Checked = true;
            this.chk_VVS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_VVS.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.chk_VVS.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(107)))), ((int)(((byte)(107)))));
            this.chk_VVS.Location = new System.Drawing.Point(120, 10);
            this.chk_VVS.Name = "chk_VVS";
            this.chk_VVS.Size = new System.Drawing.Size(85, 23);
            this.chk_VVS.TabIndex = 3;
            this.chk_VVS.Text = "VVS结果";
            this.chk_VVS.UseVisualStyleBackColor = true;
            //
            // chk_VRS
            //
            this.chk_VRS.AutoSize = true;
            this.chk_VRS.Checked = true;
            this.chk_VRS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_VRS.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.chk_VRS.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(207)))), ((int)(((byte)(102)))));
            this.chk_VRS.Location = new System.Drawing.Point(230, 10);
            this.chk_VRS.Name = "chk_VRS";
            this.chk_VRS.Size = new System.Drawing.Size(85, 23);
            this.chk_VRS.TabIndex = 4;
            this.chk_VRS.Text = "VRS结果";
            this.chk_VRS.UseVisualStyleBackColor = true;
            //
            // btn_Refresh
            //
            this.btn_Refresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btn_Refresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Refresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Refresh.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btn_Refresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btn_Refresh.Location = new System.Drawing.Point(345, 8);
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.Size = new System.Drawing.Size(80, 28);
            this.btn_Refresh.TabIndex = 1;
            this.btn_Refresh.Text = "刷新";
            this.btn_Refresh.UseVisualStyleBackColor = false;
            //
            // lbl_AIStatus
            //
            this.lbl_AIStatus.AutoSize = true;
            this.lbl_AIStatus.Font = new System.Drawing.Font("微软雅黑", 8F);
            this.lbl_AIStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lbl_AIStatus.Location = new System.Drawing.Point(440, 13);
            this.lbl_AIStatus.Name = "lbl_AIStatus";
            this.lbl_AIStatus.Size = new System.Drawing.Size(80, 17);
            this.lbl_AIStatus.TabIndex = 5;
            this.lbl_AIStatus.Text = "● AI: 无数据";
            //
            // lbl_VVSStatus
            //
            this.lbl_VVSStatus.AutoSize = true;
            this.lbl_VVSStatus.Font = new System.Drawing.Font("微软雅黑", 8F);
            this.lbl_VVSStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lbl_VVSStatus.Location = new System.Drawing.Point(540, 13);
            this.lbl_VVSStatus.Name = "lbl_VVSStatus";
            this.lbl_VVSStatus.Size = new System.Drawing.Size(85, 17);
            this.lbl_VVSStatus.TabIndex = 6;
            this.lbl_VVSStatus.Text = "● VVS: 无数据";
            //
            // lbl_VRSStatus
            //
            this.lbl_VRSStatus.AutoSize = true;
            this.lbl_VRSStatus.Font = new System.Drawing.Font("微软雅黑", 8F);
            this.lbl_VRSStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lbl_VRSStatus.Location = new System.Drawing.Point(645, 13);
            this.lbl_VRSStatus.Name = "lbl_VRSStatus";
            this.lbl_VRSStatus.Size = new System.Drawing.Size(85, 17);
            this.lbl_VRSStatus.TabIndex = 7;
            this.lbl_VRSStatus.Text = "● VRS: 无数据";
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
        private DeepSightAI.StyledButton btn_Refresh;
        private System.Windows.Forms.CheckBox chk_AI;
        private System.Windows.Forms.CheckBox chk_VVS;
        private System.Windows.Forms.CheckBox chk_VRS;
        private System.Windows.Forms.Label lbl_AIStatus;
        private System.Windows.Forms.Label lbl_VVSStatus;
        private System.Windows.Forms.Label lbl_VRSStatus;
        private System.Windows.Forms.Panel panel_Bottom;
        private System.Windows.Forms.Label label_Status;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_Pareto;
    }
}
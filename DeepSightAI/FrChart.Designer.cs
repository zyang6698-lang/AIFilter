namespace DeepSightAI
{
    partial class FrChart
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
            this.tabPage9 = new System.Windows.Forms.TabPage();
            this.analyticsControl1 = new DeepSightAI.ToolboxControl();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.heatMapControl21 = new DeepSightAI.HeatMapControl2();
            this.tabPage10 = new System.Windows.Forms.TabPage();
            this.aiReviewControl1 = new DeepSightAI.AIReviewControl();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage9.SuspendLayout();
            this.tabPage7.SuspendLayout();
            this.tabPage10.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage9
            // 
            this.tabPage9.Controls.Add(this.analyticsControl1);
            this.tabPage9.Location = new System.Drawing.Point(4, 29);
            this.tabPage9.Name = "tabPage9";
            this.tabPage9.Size = new System.Drawing.Size(1373, 792);
            this.tabPage9.TabIndex = 5;
            this.tabPage9.Text = "工具箱";
            this.tabPage9.UseVisualStyleBackColor = true;
            // 
            // analyticsControl1
            // 
            this.analyticsControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.analyticsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.analyticsControl1.Location = new System.Drawing.Point(0, 0);
            this.analyticsControl1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.analyticsControl1.Name = "analyticsControl1";
            this.analyticsControl1.Size = new System.Drawing.Size(1373, 792);
            this.analyticsControl1.TabIndex = 0;
            // 
            // tabPage7
            // 
            this.tabPage7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tabPage7.Controls.Add(this.heatMapControl21);
            this.tabPage7.Location = new System.Drawing.Point(4, 29);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage7.Size = new System.Drawing.Size(1373, 792);
            this.tabPage7.TabIndex = 3;
            this.tabPage7.Text = "热力图";
            // 
            // heatMapControl21
            // 
            this.heatMapControl21.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heatMapControl21.Location = new System.Drawing.Point(3, 3);
            this.heatMapControl21.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.heatMapControl21.Name = "heatMapControl21";
            this.heatMapControl21.offsetX = 0;
            this.heatMapControl21.offsetY = 0;
            this.heatMapControl21.Size = new System.Drawing.Size(1367, 786);
            this.heatMapControl21.TabIndex = 0;
            // 
            // tabPage10
            // 
            this.tabPage10.Controls.Add(this.aiReviewControl1);
            this.tabPage10.Location = new System.Drawing.Point(4, 29);
            this.tabPage10.Name = "tabPage10";
            this.tabPage10.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage10.Size = new System.Drawing.Size(1373, 792);
            this.tabPage10.TabIndex = 6;
            this.tabPage10.Text = "AIReview";
            this.tabPage10.UseVisualStyleBackColor = true;
            // 
            // aiReviewControl1
            // 
            this.aiReviewControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aiReviewControl1.Location = new System.Drawing.Point(3, 3);
            this.aiReviewControl1.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.aiReviewControl1.Name = "aiReviewControl1";
            this.aiReviewControl1.Size = new System.Drawing.Size(1367, 786);
            this.aiReviewControl1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage10);
            this.tabControl1.Controls.Add(this.tabPage7);
            this.tabControl1.Controls.Add(this.tabPage9);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1381, 825);
            this.tabControl1.TabIndex = 0;
            // 
            // FrChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.ClientSize = new System.Drawing.Size(1381, 825);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrChart";
            this.Text = "FrChart";
            this.tabPage9.ResumeLayout(false);
            this.tabPage7.ResumeLayout(false);
            this.tabPage10.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridViewTextBoxColumn sn;
        private System.Windows.Forms.TabPage tabPage9;
        internal ToolboxControl analyticsControl1;
        private System.Windows.Forms.TabPage tabPage7;
        private HeatMapControl2 heatMapControl21;
        private System.Windows.Forms.TabPage tabPage10;
        private AIReviewControl aiReviewControl1;
        private System.Windows.Forms.TabControl tabControl1;
    }
}
namespace DeepSightDisplay
{
    partial class frFullScreen
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
            this.label1 = new System.Windows.Forms.Label();
            this.cvDisplay1 = new DeepSightDisplay.CvDisplay();
            ((System.ComponentModel.ISupportInitialize)(this.cvDisplay1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F);
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "ESC退出全屏";
            // 
            // cvDisplay1
            // 
            this.cvDisplay1.AutoDisplay = DeepSightDisplay.CvDisplay.AutoDisplayMode.Fit;
            this.cvDisplay1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cvDisplay1.DrawModel = false;
            this.cvDisplay1.Image = null;
            this.cvDisplay1.Location = new System.Drawing.Point(0, 0);
            this.cvDisplay1.Name = "cvDisplay1";
            this.cvDisplay1.Size = new System.Drawing.Size(800, 548);
            this.cvDisplay1.TabIndex = 0;
            this.cvDisplay1.TabStop = false;
            // 
            // frFullScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 548);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cvDisplay1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frFullScreen";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "frFullScreen";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frFullScreen_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.cvDisplay1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        internal CvDisplay cvDisplay1;
    }
}
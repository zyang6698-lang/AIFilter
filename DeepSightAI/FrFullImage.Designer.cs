namespace DeepSightAI
{
    partial class FrFullImage
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
            this.cvDisplay1 = new DeepSightDisplay.CvDisplay();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.cvDisplay1)).BeginInit();
            this.SuspendLayout();
            // 
            // cvDisplay1
            // 
            this.cvDisplay1.AutoDisplay = DeepSightDisplay.CvDisplay.AutoDisplayMode.Original;
            this.cvDisplay1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cvDisplay1.DrawModel = false;
            this.cvDisplay1.Image = null;
            this.cvDisplay1.Location = new System.Drawing.Point(0, 0);
            this.cvDisplay1.Name = "cvDisplay1";
            this.cvDisplay1.OCR = null;
            this.cvDisplay1.ProductId = null;
            this.cvDisplay1.Size = new System.Drawing.Size(795, 621);
            this.cvDisplay1.stationIndex = 0;
            this.cvDisplay1.stationName = null;
            this.cvDisplay1.TabIndex = 0;
            this.cvDisplay1.TabStop = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F);
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "ESC退出全屏";
            // 
            // FrFullImage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(795, 621);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cvDisplay1);
            this.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrFullImage";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "FrFullImage";
            this.Load += new System.EventHandler(this.FrFullImage_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrFullImage_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.cvDisplay1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal DeepSightDisplay.CvDisplay cvDisplay1;
        private System.Windows.Forms.Label label1;
    }
}
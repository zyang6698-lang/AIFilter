namespace DeepSightAI
{
    partial class FrWelcome
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrWelcome));
            this.lbl_version = new System.Windows.Forms.Label();
            this.lbl_step = new System.Windows.Forms.Label();
            this.bar_step = new System.Windows.Forms.ProgressBar();
            this.btn_exit = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_version
            // 
            this.lbl_version.AutoSize = true;
            this.lbl_version.BackColor = System.Drawing.Color.Transparent;
            this.lbl_version.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_version.ForeColor = System.Drawing.Color.White;
            this.lbl_version.Location = new System.Drawing.Point(521, 311);
            this.lbl_version.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_version.Name = "lbl_version";
            this.lbl_version.Size = new System.Drawing.Size(84, 25);
            this.lbl_version.TabIndex = 1;
            this.lbl_version.Text = "V1.0.0.0";
            // 
            // lbl_step
            // 
            this.lbl_step.AutoSize = true;
            this.lbl_step.BackColor = System.Drawing.Color.Transparent;
            this.lbl_step.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_step.ForeColor = System.Drawing.Color.White;
            this.lbl_step.Location = new System.Drawing.Point(19, 316);
            this.lbl_step.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_step.Name = "lbl_step";
            this.lbl_step.Size = new System.Drawing.Size(112, 21);
            this.lbl_step.TabIndex = 2;
            this.lbl_step.Text = "Initializing......";
            // 
            // bar_step
            // 
            this.bar_step.ForeColor = System.Drawing.Color.Red;
            this.bar_step.Location = new System.Drawing.Point(-2, 347);
            this.bar_step.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.bar_step.Name = "bar_step";
            this.bar_step.Size = new System.Drawing.Size(614, 13);
            this.bar_step.TabIndex = 4;
            // 
            // btn_exit
            // 
            this.btn_exit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(57)))), ((int)(((byte)(103)))));
            this.btn_exit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_exit.FlatAppearance.BorderSize = 0;
            this.btn_exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_exit.Font = new System.Drawing.Font("新宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_exit.ForeColor = System.Drawing.Color.White;
            this.btn_exit.Location = new System.Drawing.Point(588, -1);
            this.btn_exit.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.btn_exit.Name = "btn_exit";
            this.btn_exit.Size = new System.Drawing.Size(24, 24);
            this.btn_exit.TabIndex = 10;
            this.btn_exit.TabStop = false;
            this.btn_exit.Text = "×";
            this.btn_exit.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btn_exit.UseVisualStyleBackColor = false;
            this.btn_exit.Click += new System.EventHandler(this.btn_exit_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::DeepSightAI.Properties.Resources._258E8E82_945C_49b9_9353_77868CDD1140;
            this.pictureBox1.Location = new System.Drawing.Point(87, 101);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(449, 139);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // FrWelcome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(57)))), ((int)(((byte)(103)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(611, 360);
            this.Controls.Add(this.btn_exit);
            this.Controls.Add(this.lbl_step);
            this.Controls.Add(this.bar_step);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lbl_version);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "FrWelcome";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AOI & Bohr";
            this.Load += new System.EventHandler(this.Frm_Welcome_Load);
            this.Shown += new System.EventHandler(this.FrWelcome_Shown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.setForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.setForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.setForm_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label lbl_step;
        internal System.Windows.Forms.Label lbl_version;
        internal System.Windows.Forms.ProgressBar bar_step;
        private System.Windows.Forms.Button btn_exit;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
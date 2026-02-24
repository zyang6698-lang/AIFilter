namespace DeepSightAI.SettingPages
{
    partial class FrBaseConfig
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrBaseConfig));
            this.panel1 = new System.Windows.Forms.Panel();
            this.txt_GetInferResultTimeout = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.txt_AgentShutdownTimeout = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.txt_MaxDefectCount = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.txt_Minioport = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.txt_endpoint_address = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.txt_severPort = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txt_severIP = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            //
            // panel1
            //
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel1.Controls.Add(this.txt_GetInferResultTimeout);
            this.panel1.Controls.Add(this.label23);
            this.panel1.Controls.Add(this.txt_AgentShutdownTimeout);
            this.panel1.Controls.Add(this.label22);
            this.panel1.Controls.Add(this.txt_MaxDefectCount);
            this.panel1.Controls.Add(this.label21);
            this.panel1.Controls.Add(this.txt_Minioport);
            this.panel1.Controls.Add(this.label18);
            this.panel1.Controls.Add(this.txt_endpoint_address);
            this.panel1.Controls.Add(this.label17);
            this.panel1.Controls.Add(this.txt_severPort);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.txt_severIP);
            this.panel1.Controls.Add(this.label8);
            this.panel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.panel1.Name = "panel1";
            //
            // txt_GetInferResultTimeout
            //
            this.txt_GetInferResultTimeout.Location = new System.Drawing.Point(220, 254);
            this.txt_GetInferResultTimeout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txt_GetInferResultTimeout.Name = "txt_GetInferResultTimeout";
            this.txt_GetInferResultTimeout.Size = new System.Drawing.Size(153, 27);
            this.txt_GetInferResultTimeout.TabIndex = 241;
            this.txt_GetInferResultTimeout.Text = "60";
            //
            // label23
            //
            this.label23.AutoSize = true;
            this.label23.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label23.Location = new System.Drawing.Point(36, 256);
            this.label23.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(178, 20);
            this.label23.TabIndex = 240;
            this.label23.Text = "推理结果最大等待时间(s)：";
            this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // txt_AgentShutdownTimeout
            //
            resources.ApplyResources(this.txt_AgentShutdownTimeout, "txt_AgentShutdownTimeout");
            this.txt_AgentShutdownTimeout.Name = "txt_AgentShutdownTimeout";
            //
            // label22
            //
            resources.ApplyResources(this.label22, "label22");
            this.label22.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label22.Name = "label22";
            // 
            // txt_MaxDefectCount
            // 
            resources.ApplyResources(this.txt_MaxDefectCount, "txt_MaxDefectCount");
            this.txt_MaxDefectCount.Name = "txt_MaxDefectCount";
            // 
            // label21
            // 
            resources.ApplyResources(this.label21, "label21");
            this.label21.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label21.Name = "label21";
            // 
            // txt_Minioport
            // 
            resources.ApplyResources(this.txt_Minioport, "txt_Minioport");
            this.txt_Minioport.Name = "txt_Minioport";
            // 
            // label18
            // 
            resources.ApplyResources(this.label18, "label18");
            this.label18.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label18.Name = "label18";
            // 
            // txt_endpoint_address
            // 
            resources.ApplyResources(this.txt_endpoint_address, "txt_endpoint_address");
            this.txt_endpoint_address.Name = "txt_endpoint_address";
            // 
            // label17
            // 
            resources.ApplyResources(this.label17, "label17");
            this.label17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label17.Name = "label17";
            // 
            // txt_severPort
            // 
            resources.ApplyResources(this.txt_severPort, "txt_severPort");
            this.txt_severPort.Name = "txt_severPort";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label7.Name = "label7";
            // 
            // txt_severIP
            // 
            resources.ApplyResources(this.txt_severIP, "txt_severIP");
            this.txt_severIP.Name = "txt_severIP";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label8.Name = "label8";
            // 
            // FrBaseConfig
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrBaseConfig";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrBaseConfig_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        internal System.Windows.Forms.TextBox txt_severPort;
        internal System.Windows.Forms.Label label7;
        internal System.Windows.Forms.TextBox txt_severIP;
        internal System.Windows.Forms.Label label8;
        internal System.Windows.Forms.TextBox txt_Minioport;
        internal System.Windows.Forms.Label label18;
        internal System.Windows.Forms.TextBox txt_endpoint_address;
        internal System.Windows.Forms.Label label17;
        internal System.Windows.Forms.TextBox txt_MaxDefectCount;
        internal System.Windows.Forms.Label label21;
        internal System.Windows.Forms.TextBox txt_AgentShutdownTimeout;
        internal System.Windows.Forms.Label label22;
        internal System.Windows.Forms.TextBox txt_GetInferResultTimeout;
        internal System.Windows.Forms.Label label23;
    }
}
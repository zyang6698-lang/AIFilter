namespace DeepSightAI.SettingPages
{
    partial class AviCtr2
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.labelCurrentWorkOrder = new System.Windows.Forms.Label();
            this.labelCurrentPartNumber = new System.Windows.Forms.Label();
            this.labelLineName = new System.Windows.Forms.Label();
            this.labelCurrentPartNumberValue = new System.Windows.Forms.Label();
            this.labelLotValue = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblOperatingRate = new System.Windows.Forms.Label();
            this.lblAiPassRate = new System.Windows.Forms.Label();
            this.lblAviPassRate = new System.Windows.Forms.Label();
            this.pictureBoxStatus = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // labelCurrentWorkOrder
            // 
            this.labelCurrentWorkOrder.AutoSize = true;
            this.labelCurrentWorkOrder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(199)))), ((int)(((byte)(220)))));
            this.labelCurrentWorkOrder.Location = new System.Drawing.Point(14, 101);
            this.labelCurrentWorkOrder.Name = "labelCurrentWorkOrder";
            this.labelCurrentWorkOrder.Size = new System.Drawing.Size(47, 15);
            this.labelCurrentWorkOrder.TabIndex = 4;
            this.labelCurrentWorkOrder.Text = "Lot :";
            // 
            // labelCurrentPartNumber
            // 
            this.labelCurrentPartNumber.AutoSize = true;
            this.labelCurrentPartNumber.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(199)))), ((int)(((byte)(220)))));
            this.labelCurrentPartNumber.Location = new System.Drawing.Point(14, 73);
            this.labelCurrentPartNumber.Name = "labelCurrentPartNumber";
            this.labelCurrentPartNumber.Size = new System.Drawing.Size(53, 15);
            this.labelCurrentPartNumber.TabIndex = 5;
            this.labelCurrentPartNumber.Text = "料号 :";
            // 
            // labelLineName
            // 
            this.labelLineName.AutoSize = true;
            this.labelLineName.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Bold);
            this.labelLineName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.labelLineName.Location = new System.Drawing.Point(60, 15);
            this.labelLineName.Name = "labelLineName";
            this.labelLineName.Size = new System.Drawing.Size(173, 30);
            this.labelLineName.TabIndex = 6;
            this.labelLineName.Text = "SMT-LINE-A";
            // 
            // labelCurrentPartNumberValue
            // 
            this.labelCurrentPartNumberValue.AutoSize = true;
            this.labelCurrentPartNumberValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(199)))), ((int)(((byte)(220)))));
            this.labelCurrentPartNumberValue.Location = new System.Drawing.Point(52, 73);
            this.labelCurrentPartNumberValue.Name = "labelCurrentPartNumberValue";
            this.labelCurrentPartNumberValue.Size = new System.Drawing.Size(15, 15);
            this.labelCurrentPartNumberValue.TabIndex = 13;
            this.labelCurrentPartNumberValue.Text = "-";
            // 
            // labelLotValue
            // 
            this.labelLotValue.AutoSize = true;
            this.labelLotValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(199)))), ((int)(((byte)(220)))));
            this.labelLotValue.Location = new System.Drawing.Point(54, 101);
            this.labelLotValue.Name = "labelLotValue";
            this.labelLotValue.Size = new System.Drawing.Size(15, 15);
            this.labelLotValue.TabIndex = 14;
            this.labelLotValue.Text = "-";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(260, 148);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 15);
            this.label1.TabIndex = 15;
            this.label1.Text = "稼动率:";
            this.label1.Visible = false;
            // 
            // lblOperatingRate
            // 
            this.lblOperatingRate.AutoSize = true;
            this.lblOperatingRate.ForeColor = System.Drawing.Color.LightGray;
            this.lblOperatingRate.Location = new System.Drawing.Point(318, 148);
            this.lblOperatingRate.Name = "lblOperatingRate";
            this.lblOperatingRate.Size = new System.Drawing.Size(31, 15);
            this.lblOperatingRate.TabIndex = 17;
            this.lblOperatingRate.Text = "98%";
            this.lblOperatingRate.Visible = false;
            // 
            // lblAiPassRate
            // 
            this.lblAiPassRate.AutoSize = true;
            this.lblAiPassRate.Font = new System.Drawing.Font("宋体", 9F);
            this.lblAiPassRate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(199)))), ((int)(((byte)(220)))));
            this.lblAiPassRate.Location = new System.Drawing.Point(147, 101);
            this.lblAiPassRate.Name = "lblAiPassRate";
            this.lblAiPassRate.Size = new System.Drawing.Size(111, 15);
            this.lblAiPassRate.TabIndex = 18;
            this.lblAiPassRate.Text = "AI Pass Rate:";
            // 
            // lblAviPassRate
            // 
            this.lblAviPassRate.AutoSize = true;
            this.lblAviPassRate.Font = new System.Drawing.Font("宋体", 9F);
            this.lblAviPassRate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(199)))), ((int)(((byte)(220)))));
            this.lblAviPassRate.Location = new System.Drawing.Point(147, 68);
            this.lblAviPassRate.Name = "lblAviPassRate";
            this.lblAviPassRate.Size = new System.Drawing.Size(111, 15);
            this.lblAviPassRate.TabIndex = 20;
            this.lblAviPassRate.Text = "AVI Pass Rate";
            // 
            // pictureBoxStatus
            // 
            this.pictureBoxStatus.Location = new System.Drawing.Point(20, 15);
            this.pictureBoxStatus.Name = "pictureBoxStatus";
            this.pictureBoxStatus.Size = new System.Drawing.Size(30, 30);
            this.pictureBoxStatus.TabIndex = 8;
            this.pictureBoxStatus.TabStop = false;
            // 
            // AviCtr2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.Controls.Add(this.lblAviPassRate);
            this.Controls.Add(this.lblAiPassRate);
            this.Controls.Add(this.lblOperatingRate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelLotValue);
            this.Controls.Add(this.labelCurrentPartNumberValue);
            this.Controls.Add(this.pictureBoxStatus);
            this.Controls.Add(this.labelLineName);
            this.Controls.Add(this.labelCurrentPartNumber);
            this.Controls.Add(this.labelCurrentWorkOrder);
            this.Name = "AviCtr2";
            this.Size = new System.Drawing.Size(296, 145);
            this.DoubleClick += new System.EventHandler(this.AviCtr2_DoubleClick);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelCurrentWorkOrder;
        private System.Windows.Forms.Label labelCurrentPartNumber;
        private System.Windows.Forms.Label labelLineName;
        private System.Windows.Forms.PictureBox pictureBoxStatus;
        private System.Windows.Forms.Label labelCurrentPartNumberValue;
        private System.Windows.Forms.Label labelLotValue;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblOperatingRate;
        private System.Windows.Forms.Label lblAiPassRate;
        private System.Windows.Forms.Label lblAviPassRate;
    }
}

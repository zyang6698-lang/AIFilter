namespace DeepSightAI.SettingPages
{
    partial class MachineStatusCard
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
               // 释放缓存的GDI对象
               _borderPen?.Dispose();
               foreach (var bmp in _statusBitmaps.Values)
               {
                   bmp?.Dispose();
               }
               _statusBitmaps.Clear();
               toolTip?.Dispose();
               components?.Dispose();
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
            this.labelLineName = new System.Windows.Forms.Label();
            this.labelCurrentPartNumberValue = new System.Windows.Forms.Label();
            this.labelLotValue = new System.Windows.Forms.Label();
            this.lblOperatingRate = new System.Windows.Forms.Label();
            this.lblAiPassRate = new System.Windows.Forms.Label();
            this.lblAviPassRate = new System.Windows.Forms.Label();
            this.pictureBoxStatus = new System.Windows.Forms.PictureBox();
            this.btnDelete = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDelete)).BeginInit();
            this.SuspendLayout();
            // 
            // labelLineName
            // 
            this.labelLineName.AutoSize = true;
            this.labelLineName.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Bold);
            this.labelLineName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.labelLineName.Location = new System.Drawing.Point(38, 12);
            this.labelLineName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelLineName.Name = "labelLineName";
            this.labelLineName.Size = new System.Drawing.Size(88, 24);
            this.labelLineName.TabIndex = 6;
            this.labelLineName.Text = "AVI112";
            //
            // labelCurrentPartNumberValue (title only)
            //
            this.labelCurrentPartNumberValue.AutoSize = true;
            this.labelCurrentPartNumberValue.Font = new System.Drawing.Font("Calibri", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCurrentPartNumberValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(140)))), ((int)(((byte)(165)))));
            this.labelCurrentPartNumberValue.Location = new System.Drawing.Point(10, 45);
            this.labelCurrentPartNumberValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelCurrentPartNumberValue.Name = "labelCurrentPartNumberValue";
            this.labelCurrentPartNumberValue.Size = new System.Drawing.Size(43, 13);
            this.labelCurrentPartNumberValue.TabIndex = 13;
            this.labelCurrentPartNumberValue.Text = "Part No.";
            //
            // labelLotValue (title only)
            //
            this.labelLotValue.AutoSize = true;
            this.labelLotValue.Font = new System.Drawing.Font("Calibri", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLotValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(140)))), ((int)(((byte)(165)))));
            this.labelLotValue.Location = new System.Drawing.Point(10, 67);
            this.labelLotValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelLotValue.Name = "labelLotValue";
            this.labelLotValue.Size = new System.Drawing.Size(16, 13);
            this.labelLotValue.TabIndex = 14;
            this.labelLotValue.Text = "Lot";
            // 
            // lblOperatingRate
            // 
            this.lblOperatingRate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblOperatingRate.AutoSize = true;
            this.lblOperatingRate.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOperatingRate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(199)))), ((int)(((byte)(220)))));
            this.lblOperatingRate.Location = new System.Drawing.Point(163, 58);
            this.lblOperatingRate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblOperatingRate.Name = "lblOperatingRate";
            this.lblOperatingRate.Size = new System.Drawing.Size(66, 14);
            this.lblOperatingRate.TabIndex = 15;
            this.lblOperatingRate.Text = "Utilization:";
            this.lblOperatingRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblOperatingRate.Visible = false;
            //
            // lblAiPassRate (title only)
            //
            this.lblAiPassRate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAiPassRate.AutoSize = true;
            this.lblAiPassRate.Font = new System.Drawing.Font("Calibri", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAiPassRate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(140)))), ((int)(((byte)(165)))));
            this.lblAiPassRate.Location = new System.Drawing.Point(148, 69);
            this.lblAiPassRate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAiPassRate.Name = "lblAiPassRate";
            this.lblAiPassRate.Size = new System.Drawing.Size(65, 13);
            this.lblAiPassRate.TabIndex = 18;
            this.lblAiPassRate.Text = "AI Pass Rate";
            this.lblAiPassRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblAviPassRate (title only)
            //
            this.lblAviPassRate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAviPassRate.AutoSize = true;
            this.lblAviPassRate.Font = new System.Drawing.Font("Calibri", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAviPassRate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(140)))), ((int)(((byte)(165)))));
            this.lblAviPassRate.Location = new System.Drawing.Point(148, 47);
            this.lblAviPassRate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAviPassRate.Name = "lblAviPassRate";
            this.lblAviPassRate.Size = new System.Drawing.Size(71, 13);
            this.lblAviPassRate.TabIndex = 20;
            this.lblAviPassRate.Text = "AVI Pass Rate";
            this.lblAviPassRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pictureBoxStatus
            // 
            this.pictureBoxStatus.Location = new System.Drawing.Point(10, 12);
            this.pictureBoxStatus.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBoxStatus.Name = "pictureBoxStatus";
            this.pictureBoxStatus.Size = new System.Drawing.Size(22, 24);
            this.pictureBoxStatus.TabIndex = 8;
            this.pictureBoxStatus.TabStop = false;
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.Location = new System.Drawing.Point(252, 60);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(2);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(20, 20);
            this.btnDelete.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnDelete.TabIndex = 21;
            this.btnDelete.TabStop = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // MachineStatusCard
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.lblAviPassRate);
            this.Controls.Add(this.lblAiPassRate);
            this.Controls.Add(this.lblOperatingRate);
            this.Controls.Add(this.labelLotValue);
            this.Controls.Add(this.labelCurrentPartNumberValue);
            this.Controls.Add(this.pictureBoxStatus);
            this.Controls.Add(this.labelLineName);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MachineStatusCard";
            this.Size = new System.Drawing.Size(278, 87);
            this.DoubleClick += new System.EventHandler(this.MachineStatusCard_DoubleClick);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDelete)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelLineName;
        private System.Windows.Forms.PictureBox pictureBoxStatus;
        private System.Windows.Forms.Label labelCurrentPartNumberValue;
        private System.Windows.Forms.Label labelLotValue;
        private System.Windows.Forms.Label lblOperatingRate;
        private System.Windows.Forms.Label lblAiPassRate;
        private System.Windows.Forms.Label lblAviPassRate;
        private System.Windows.Forms.PictureBox btnDelete;
    }
}

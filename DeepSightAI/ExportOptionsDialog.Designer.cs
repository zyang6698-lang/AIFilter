namespace DeepSightAI
{
    partial class ExportOptionsDialog
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.chkOriginalImage = new System.Windows.Forms.CheckBox();
            this.chkTemplateImage = new System.Windows.Forms.CheckBox();
            this.lblHint = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(0, 12);
            this.lblTitle.Text = "请选择要导出的图片类型（至少选择一项）：";
            // 
            // chkOriginalImage
            // 
            this.chkOriginalImage.AutoSize = true;
            this.chkOriginalImage.Checked = true;
            this.chkOriginalImage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOriginalImage.Location = new System.Drawing.Point(40, 55);
            this.chkOriginalImage.Name = "chkOriginalImage";
            this.chkOriginalImage.Size = new System.Drawing.Size(48, 16);
            this.chkOriginalImage.TabIndex = 0;
            this.chkOriginalImage.Text = "原图";
            this.chkOriginalImage.UseVisualStyleBackColor = true;
            // 
            // chkTemplateImage
            // 
            this.chkTemplateImage.AutoSize = true;
            this.chkTemplateImage.Checked = true;
            this.chkTemplateImage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTemplateImage.Location = new System.Drawing.Point(40, 85);
            this.chkTemplateImage.Name = "chkTemplateImage";
            this.chkTemplateImage.Size = new System.Drawing.Size(60, 16);
            this.chkTemplateImage.TabIndex = 1;
            this.chkTemplateImage.Text = "模板图";
            this.chkTemplateImage.UseVisualStyleBackColor = true;
            // 
            // lblHint
            // 
            this.lblHint.AutoSize = true;
            this.lblHint.ForeColor = System.Drawing.Color.Red;
            this.lblHint.Location = new System.Drawing.Point(20, 115);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(0, 12);
            this.lblHint.Text = "";
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(110, 140);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(80, 30);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "确定";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(200, 140);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // ExportOptionsDialog
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(300, 180);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.chkTemplateImage);
            this.Controls.Add(this.chkOriginalImage);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportOptionsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "导出选项";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.CheckBox chkOriginalImage;
        private System.Windows.Forms.CheckBox chkTemplateImage;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}


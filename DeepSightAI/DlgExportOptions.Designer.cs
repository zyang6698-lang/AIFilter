namespace DeepSightAI
{
    partial class DlgExportOptions
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
            this.lblSubTitle = new System.Windows.Forms.Label();
            this.grpImageTypes = new System.Windows.Forms.GroupBox();
            this.chkOriginalImage = new System.Windows.Forms.CheckBox();
            this.lblOriginalDesc = new System.Windows.Forms.Label();
            this.chkTemplateImage = new System.Windows.Forms.CheckBox();
            this.lblTemplateDesc = new System.Windows.Forms.Label();
            this.chkGerberImage = new System.Windows.Forms.CheckBox();
            this.lblGerberDesc = new System.Windows.Forms.Label();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpImageTypes.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.lblTitle.Location = new System.Drawing.Point(20, 16);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "导出选项";
            //
            // lblSubTitle
            //
            this.lblSubTitle.AutoSize = true;
            this.lblSubTitle.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(110, 110, 110);
            this.lblSubTitle.Location = new System.Drawing.Point(22, 42);
            this.lblSubTitle.Name = "lblSubTitle";
            this.lblSubTitle.Text = "请选择要导出的图片类型，可任意勾选一项或多项";
            //
            // grpImageTypes
            //
            this.grpImageTypes.Controls.Add(this.chkOriginalImage);
            this.grpImageTypes.Controls.Add(this.lblOriginalDesc);
            this.grpImageTypes.Controls.Add(this.chkTemplateImage);
            this.grpImageTypes.Controls.Add(this.lblTemplateDesc);
            this.grpImageTypes.Controls.Add(this.chkGerberImage);
            this.grpImageTypes.Controls.Add(this.lblGerberDesc);
            this.grpImageTypes.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.grpImageTypes.Location = new System.Drawing.Point(20, 70);
            this.grpImageTypes.Name = "grpImageTypes";
            this.grpImageTypes.Size = new System.Drawing.Size(360, 180);
            this.grpImageTypes.TabStop = false;
            this.grpImageTypes.Text = " 图片类型 ";
            //
            // chkOriginalImage
            //
            this.chkOriginalImage.AutoSize = true;
            this.chkOriginalImage.Checked = true;
            this.chkOriginalImage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOriginalImage.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.chkOriginalImage.Location = new System.Drawing.Point(20, 28);
            this.chkOriginalImage.Name = "chkOriginalImage";
            this.chkOriginalImage.TabIndex = 0;
            this.chkOriginalImage.Text = "原图";
            this.chkOriginalImage.UseVisualStyleBackColor = true;
            //
            // lblOriginalDesc
            //
            this.lblOriginalDesc.AutoSize = true;
            this.lblOriginalDesc.Font = new System.Drawing.Font("微软雅黑", 8.25F);
            this.lblOriginalDesc.ForeColor = System.Drawing.Color.FromArgb(130, 130, 130);
            this.lblOriginalDesc.Location = new System.Drawing.Point(40, 50);
            this.lblOriginalDesc.Name = "lblOriginalDesc";
            this.lblOriginalDesc.Text = "AOI 拍摄的缺陷区域原始图像（_0.png）";
            //
            // chkTemplateImage
            //
            this.chkTemplateImage.AutoSize = true;
            this.chkTemplateImage.Checked = true;
            this.chkTemplateImage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTemplateImage.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.chkTemplateImage.Location = new System.Drawing.Point(20, 76);
            this.chkTemplateImage.Name = "chkTemplateImage";
            this.chkTemplateImage.TabIndex = 1;
            this.chkTemplateImage.Text = "模板图 (Template)";
            this.chkTemplateImage.UseVisualStyleBackColor = true;
            //
            // lblTemplateDesc
            //
            this.lblTemplateDesc.AutoSize = true;
            this.lblTemplateDesc.Font = new System.Drawing.Font("微软雅黑", 8.25F);
            this.lblTemplateDesc.ForeColor = System.Drawing.Color.FromArgb(130, 130, 130);
            this.lblTemplateDesc.Location = new System.Drawing.Point(40, 98);
            this.lblTemplateDesc.Name = "lblTemplateDesc";
            this.lblTemplateDesc.Text = "同区域的良品模板图像（_1.png）";
            //
            // chkGerberImage
            //
            this.chkGerberImage.AutoSize = true;
            this.chkGerberImage.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.chkGerberImage.Location = new System.Drawing.Point(20, 124);
            this.chkGerberImage.Name = "chkGerberImage";
            this.chkGerberImage.TabIndex = 2;
            this.chkGerberImage.Text = "Gerber 图";
            this.chkGerberImage.UseVisualStyleBackColor = true;
            //
            // lblGerberDesc
            //
            this.lblGerberDesc.AutoSize = true;
            this.lblGerberDesc.Font = new System.Drawing.Font("微软雅黑", 8.25F);
            this.lblGerberDesc.ForeColor = System.Drawing.Color.FromArgb(130, 130, 130);
            this.lblGerberDesc.Location = new System.Drawing.Point(40, 146);
            this.lblGerberDesc.Name = "lblGerberDesc";
            this.lblGerberDesc.Text = "由 Gerber 数据渲染的参考图像（_2.png）";
            //
            // pnlButtons
            //
            this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
            this.pnlButtons.Controls.Add(this.btnOk);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 290);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(400, 56);
            //
            // btnOk
            //
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOk.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnOk.Location = new System.Drawing.Point(204, 12);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(86, 32);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "确定导出";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnCancel.Location = new System.Drawing.Point(298, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(86, 32);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            //
            // DlgExportOptions
            //
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(400, 346);
            this.Controls.Add(this.grpImageTypes);
            this.Controls.Add(this.lblSubTitle);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pnlButtons);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DlgExportOptions";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "导出选项";
            this.grpImageTypes.ResumeLayout(false);
            this.grpImageTypes.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubTitle;
        private System.Windows.Forms.GroupBox grpImageTypes;
        private System.Windows.Forms.CheckBox chkOriginalImage;
        private System.Windows.Forms.Label lblOriginalDesc;
        private System.Windows.Forms.CheckBox chkTemplateImage;
        private System.Windows.Forms.Label lblTemplateDesc;
        private System.Windows.Forms.CheckBox chkGerberImage;
        private System.Windows.Forms.Label lblGerberDesc;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}


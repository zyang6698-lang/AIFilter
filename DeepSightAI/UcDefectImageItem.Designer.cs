namespace DeepSightAI
{
    partial class UcDefectImageItem
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// ToolTip用于显示按钮提示
        /// </summary>
        private System.Windows.Forms.ToolTip _toolTip;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip?.Dispose();
                ClearLoadedImages();
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
            this.panel_Header = new System.Windows.Forms.Panel();
            this.label_SN = new System.Windows.Forms.Label();
            this.button_Run = new System.Windows.Forms.Button();
            this.tableImages = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox_OriginalImage = new System.Windows.Forms.PictureBox();
            this.pictureBox_TemplateImage = new System.Windows.Forms.PictureBox();
            this.pictureBox_AviImage = new System.Windows.Forms.PictureBox();
            this.panel_Status = new System.Windows.Forms.Panel();
            this.label_Status = new System.Windows.Forms.Label();
            this.label_Index = new System.Windows.Forms.Label();
            this.panel_Header.SuspendLayout();
            this.tableImages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_OriginalImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_TemplateImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_AviImage)).BeginInit();
            this.panel_Status.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_Header
            // 
            this.panel_Header.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel_Header.Controls.Add(this.label_SN);
            this.panel_Header.Controls.Add(this.button_Run);
            this.panel_Header.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_Header.Location = new System.Drawing.Point(0, 0);
            this.panel_Header.Name = "panel_Header";
            this.panel_Header.Size = new System.Drawing.Size(300, 28);
            this.panel_Header.TabIndex = 0;
            // 
            // label_SN
            // 
            this.label_SN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_SN.Font = new System.Drawing.Font("微软雅黑", 8.5F);
            this.label_SN.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label_SN.Location = new System.Drawing.Point(0, 0);
            this.label_SN.Name = "label_SN";
            this.label_SN.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.label_SN.Size = new System.Drawing.Size(272, 28);
            this.label_SN.TabIndex = 0;
            this.label_SN.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button_Run
            // 
            this.button_Run.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.button_Run.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_Run.Dock = System.Windows.Forms.DockStyle.Right;
            this.button_Run.FlatAppearance.BorderSize = 0;
            this.button_Run.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Run.Font = new System.Drawing.Font("Segoe UI Symbol", 10F);
            this.button_Run.ForeColor = System.Drawing.Color.LightGreen;
            this.button_Run.Location = new System.Drawing.Point(272, 0);
            this.button_Run.Name = "button_Run";
            this.button_Run.Size = new System.Drawing.Size(28, 28);
            this.button_Run.TabIndex = 1;
            this.button_Run.TabStop = false;
            this.button_Run.Text = "▶";
            this.button_Run.UseVisualStyleBackColor = false;
            this.button_Run.Click += new System.EventHandler(this.Button_Run_Click);
            // 
            // tableImages
            // 
            this.tableImages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tableImages.ColumnCount = 1;
            this.tableImages.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableImages.Controls.Add(this.pictureBox_OriginalImage, 0, 0);
            this.tableImages.Controls.Add(this.pictureBox_TemplateImage, 0, 1);
            this.tableImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableImages.Location = new System.Drawing.Point(0, 28);
            this.tableImages.Margin = new System.Windows.Forms.Padding(0);
            this.tableImages.Name = "tableImages";
            this.tableImages.RowCount = 2;
            this.tableImages.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableImages.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableImages.Size = new System.Drawing.Size(300, 400);
            this.tableImages.TabIndex = 4;
            // 
            // pictureBox_OriginalImage
            // 
            this.pictureBox_OriginalImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.pictureBox_OriginalImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_OriginalImage.Location = new System.Drawing.Point(0, 0);
            this.pictureBox_OriginalImage.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.pictureBox_OriginalImage.Name = "pictureBox_OriginalImage";
            this.pictureBox_OriginalImage.Size = new System.Drawing.Size(300, 199);
            this.pictureBox_OriginalImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_OriginalImage.TabIndex = 1;
            this.pictureBox_OriginalImage.TabStop = false;
            // 
            // pictureBox_TemplateImage
            // 
            this.pictureBox_TemplateImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.pictureBox_TemplateImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_TemplateImage.Location = new System.Drawing.Point(0, 201);
            this.pictureBox_TemplateImage.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.pictureBox_TemplateImage.Name = "pictureBox_TemplateImage";
            this.pictureBox_TemplateImage.Size = new System.Drawing.Size(300, 199);
            this.pictureBox_TemplateImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_TemplateImage.TabIndex = 2;
            this.pictureBox_TemplateImage.TabStop = false;
            //
            // pictureBox_AviImage
            //
            this.pictureBox_AviImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_AviImage.BackColor = System.Drawing.Color.Black;
            this.pictureBox_AviImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox_AviImage.Location = new System.Drawing.Point(214, 34);
            this.pictureBox_AviImage.Name = "pictureBox_AviImage";
            this.pictureBox_AviImage.Size = new System.Drawing.Size(78, 58);
            this.pictureBox_AviImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_AviImage.TabIndex = 5;
            this.pictureBox_AviImage.TabStop = false;
            this.pictureBox_AviImage.Visible = false;
            //
            // panel_Status
            //
            this.panel_Status.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel_Status.Controls.Add(this.label_Status);
            this.panel_Status.Controls.Add(this.label_Index);
            this.panel_Status.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_Status.Location = new System.Drawing.Point(0, 428);
            this.panel_Status.Name = "panel_Status";
            this.panel_Status.Size = new System.Drawing.Size(300, 44);
            this.panel_Status.TabIndex = 3;
            //
            // label_Status
            //
            this.label_Status.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_Status.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label_Status.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label_Status.Location = new System.Drawing.Point(0, 0);
            this.label_Status.Name = "label_Status";
            this.label_Status.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label_Status.Size = new System.Drawing.Size(300, 24);
            this.label_Status.TabIndex = 0;
            this.label_Status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label_Index
            //
            this.label_Index.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label_Index.Font = new System.Drawing.Font("微软雅黑", 8F);
            this.label_Index.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label_Index.Location = new System.Drawing.Point(0, 24);
            this.label_Index.Name = "label_Index";
            this.label_Index.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label_Index.Size = new System.Drawing.Size(300, 20);
            this.label_Index.TabIndex = 1;
            this.label_Index.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // UcDefectImageItem
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.Controls.Add(this.tableImages);
            this.Controls.Add(this.pictureBox_AviImage);
            this.Controls.Add(this.panel_Status);
            this.Controls.Add(this.panel_Header);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Name = "UcDefectImageItem";
            this.Size = new System.Drawing.Size(300, 472);
            this.panel_Header.ResumeLayout(false);
            this.tableImages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_OriginalImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_TemplateImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_AviImage)).EndInit();
            this.panel_Status.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_Header;
        private System.Windows.Forms.Label label_SN;
        private System.Windows.Forms.Button button_Run;
        private System.Windows.Forms.TableLayoutPanel tableImages;
        private System.Windows.Forms.PictureBox pictureBox_OriginalImage;
        private System.Windows.Forms.PictureBox pictureBox_TemplateImage;
        private System.Windows.Forms.PictureBox pictureBox_AviImage;
        private System.Windows.Forms.Panel panel_Status;
        private System.Windows.Forms.Label label_Status;
        private System.Windows.Forms.Label label_Index;
    }
}


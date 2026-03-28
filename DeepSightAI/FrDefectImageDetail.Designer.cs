namespace DeepSightAI
{
    partial class FrDefectImageDetail
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer_Main = new System.Windows.Forms.SplitContainer();
            this.pictureBox_Image = new System.Windows.Forms.PictureBox();
            this.panel_ImageToolbar = new System.Windows.Forms.Panel();
            this.button_OriginalImage = new System.Windows.Forms.Button();
            this.button_TemplateImage = new System.Windows.Forms.Button();
            this.button_DefectBoxImage = new System.Windows.Forms.Button();
            this.panel_Info = new System.Windows.Forms.Panel();
            this.label_Title = new System.Windows.Forms.Label();
            this.dataGridView_Info = new System.Windows.Forms.DataGridView();
            this.Column_Property = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).BeginInit();
            this.splitContainer_Main.Panel1.SuspendLayout();
            this.splitContainer_Main.Panel2.SuspendLayout();
            this.splitContainer_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Image)).BeginInit();
            this.panel_ImageToolbar.SuspendLayout();
            this.panel_Info.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Info)).BeginInit();
            this.SuspendLayout();
            // splitContainer_Main
            this.splitContainer_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_Main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_Main.Name = "splitContainer_Main";
            this.splitContainer_Main.Size = new System.Drawing.Size(1000, 650);
            this.splitContainer_Main.SplitterDistance = 650;
            this.splitContainer_Main.TabIndex = 0;
            // splitContainer_Main.Panel1 - Image area
            this.splitContainer_Main.Panel1.Controls.Add(this.pictureBox_Image);
            this.splitContainer_Main.Panel1.Controls.Add(this.panel_ImageToolbar);
            this.splitContainer_Main.Panel1.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            // splitContainer_Main.Panel2 - Info area
            this.splitContainer_Main.Panel2.Controls.Add(this.panel_Info);
            this.splitContainer_Main.Panel2.BackColor = System.Drawing.Color.FromArgb(37, 37, 38);
            // panel_ImageToolbar
            this.panel_ImageToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_ImageToolbar.Height = 36;
            this.panel_ImageToolbar.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            this.panel_ImageToolbar.Controls.Add(this.button_DefectBoxImage);
            this.panel_ImageToolbar.Controls.Add(this.button_TemplateImage);
            this.panel_ImageToolbar.Controls.Add(this.button_OriginalImage);
            this.panel_ImageToolbar.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            // button_OriginalImage
            this.button_OriginalImage.Dock = System.Windows.Forms.DockStyle.Left;
            this.button_OriginalImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_OriginalImage.FlatAppearance.BorderSize = 0;
            this.button_OriginalImage.ForeColor = System.Drawing.Color.White;
            this.button_OriginalImage.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.button_OriginalImage.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.button_OriginalImage.Text = "原图";
            this.button_OriginalImage.Size = new System.Drawing.Size(80, 28);
            this.button_OriginalImage.Cursor = System.Windows.Forms.Cursors.Hand;
            // button_TemplateImage
            this.button_TemplateImage.Dock = System.Windows.Forms.DockStyle.Left;
            this.button_TemplateImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_TemplateImage.FlatAppearance.BorderSize = 0;
            this.button_TemplateImage.ForeColor = System.Drawing.Color.Silver;
            this.button_TemplateImage.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
            this.button_TemplateImage.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.button_TemplateImage.Text = "模板图";
            this.button_TemplateImage.Size = new System.Drawing.Size(80, 28);
            this.button_TemplateImage.Cursor = System.Windows.Forms.Cursors.Hand;
            // button_DefectBoxImage
            this.button_DefectBoxImage.Dock = System.Windows.Forms.DockStyle.Left;
            this.button_DefectBoxImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_DefectBoxImage.FlatAppearance.BorderSize = 0;
            this.button_DefectBoxImage.ForeColor = System.Drawing.Color.Silver;
            this.button_DefectBoxImage.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
            this.button_DefectBoxImage.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.button_DefectBoxImage.Text = "缺陷框图";
            this.button_DefectBoxImage.Size = new System.Drawing.Size(90, 28);
            this.button_DefectBoxImage.Cursor = System.Windows.Forms.Cursors.Hand;
            // pictureBox_Image
            this.pictureBox_Image.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_Image.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.pictureBox_Image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_Image.TabStop = false;
            // label_Title
            this.label_Title.Dock = System.Windows.Forms.DockStyle.Top;
            this.label_Title.Height = 40;
            this.label_Title.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Bold);
            this.label_Title.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.label_Title.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            this.label_Title.Text = "  缺陷详细信息";
            this.label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // panel_Info
            this.panel_Info.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Info.Controls.Add(this.dataGridView_Info);
            this.panel_Info.Controls.Add(this.label_Title);
            // Column_Property
            this.Column_Property.HeaderText = "属性";
            this.Column_Property.Name = "Column_Property";
            this.Column_Property.ReadOnly = true;
            this.Column_Property.Width = 120;
            // Column_Value
            this.Column_Value.HeaderText = "值";
            this.Column_Value.Name = "Column_Value";
            this.Column_Value.ReadOnly = true;
            this.Column_Value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            // dataGridView_Info
            this.dataGridView_Info.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_Info.AllowUserToAddRows = false;
            this.dataGridView_Info.AllowUserToDeleteRows = false;
            this.dataGridView_Info.AllowUserToResizeRows = false;
            this.dataGridView_Info.ReadOnly = true;
            this.dataGridView_Info.RowHeadersVisible = false;
            this.dataGridView_Info.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_Info.BackgroundColor = System.Drawing.Color.FromArgb(37, 37, 38);
            this.dataGridView_Info.GridColor = System.Drawing.Color.FromArgb(60, 60, 65);
            this.dataGridView_Info.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            this.dataGridView_Info.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(37, 37, 38);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(51, 51, 52);
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.dataGridView_Info.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView_Info.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Info.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.Column_Property, this.Column_Value });
            this.dataGridView_Info.EnableHeadersVisualStyles = false;
            // FrDefectImageDetail
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 650);
            this.Controls.Add(this.splitContainer_Main);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.Name = "FrDefectImageDetail";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ShowInTaskbar = false;
            this.KeyPreview = true;
            this.splitContainer_Main.Panel1.ResumeLayout(false);
            this.splitContainer_Main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).EndInit();
            this.splitContainer_Main.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Image)).EndInit();
            this.panel_ImageToolbar.ResumeLayout(false);
            this.panel_Info.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Info)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_Main;
        private System.Windows.Forms.PictureBox pictureBox_Image;
        private System.Windows.Forms.Panel panel_ImageToolbar;
        private System.Windows.Forms.Button button_OriginalImage;
        private System.Windows.Forms.Button button_TemplateImage;
        private System.Windows.Forms.Button button_DefectBoxImage;
        private System.Windows.Forms.Panel panel_Info;
        private System.Windows.Forms.Label label_Title;
        private System.Windows.Forms.DataGridView dataGridView_Info;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Property;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Value;
    }
}


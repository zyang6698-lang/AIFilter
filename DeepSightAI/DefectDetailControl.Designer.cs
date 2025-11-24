namespace DeepSightAI
{
    partial class DefectDetailControl
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
            this.flowLayoutPanel_DefectImages = new System.Windows.Forms.FlowLayoutPanel();
            this.panel_Top = new System.Windows.Forms.Panel();
            this.label_DetailTitle = new System.Windows.Forms.Label();
            this.panel_Pagination = new System.Windows.Forms.Panel();
            this.flowLayoutPanel_Pagination = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.panel_Filter = new System.Windows.Forms.Panel();
            this.label_FilterVVS = new System.Windows.Forms.Label();
            this.label_FilterAI = new System.Windows.Forms.Label();
            this.comboBox_FilterVVS = new System.Windows.Forms.ComboBox();
            this.comboBox_FilterAI = new System.Windows.Forms.ComboBox();
            this.panel_Top.SuspendLayout();
            this.panel_Pagination.SuspendLayout();
            this.flowLayoutPanel_Pagination.SuspendLayout();
            this.panel_Filter.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel_DefectImages
            // 
            this.flowLayoutPanel_DefectImages.AutoScroll = true;
            this.flowLayoutPanel_DefectImages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.flowLayoutPanel_DefectImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel_DefectImages.Location = new System.Drawing.Point(0, 88);
            this.flowLayoutPanel_DefectImages.Name = "flowLayoutPanel_DefectImages";
            this.flowLayoutPanel_DefectImages.Padding = new System.Windows.Forms.Padding(10);
            this.flowLayoutPanel_DefectImages.Size = new System.Drawing.Size(1085, 507);
            this.flowLayoutPanel_DefectImages.TabIndex = 1;
            // 
            // panel_Top
            // 
            this.panel_Top.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.panel_Top.Controls.Add(this.label_DetailTitle);
            this.panel_Top.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_Top.Location = new System.Drawing.Point(0, 0);
            this.panel_Top.Name = "panel_Top";
            this.panel_Top.Size = new System.Drawing.Size(1085, 40);
            this.panel_Top.TabIndex = 2;
            // 
            // label_DetailTitle
            // 
            this.label_DetailTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_DetailTitle.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.label_DetailTitle.ForeColor = System.Drawing.Color.White;
            this.label_DetailTitle.Location = new System.Drawing.Point(0, 0);
            this.label_DetailTitle.Name = "label_DetailTitle";
            this.label_DetailTitle.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.label_DetailTitle.Size = new System.Drawing.Size(1085, 40);
            this.label_DetailTitle.TabIndex = 0;
            this.label_DetailTitle.Text = "Defect Details";
            this.label_DetailTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel_Pagination
            // 
            this.panel_Pagination.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.panel_Pagination.Controls.Add(this.flowLayoutPanel_Pagination);
            this.panel_Pagination.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_Pagination.Location = new System.Drawing.Point(0, 595);
            this.panel_Pagination.Name = "panel_Pagination";
            this.panel_Pagination.Size = new System.Drawing.Size(1085, 40);
            this.panel_Pagination.TabIndex = 3;
            // 
            // flowLayoutPanel_Pagination
            // 
            this.flowLayoutPanel_Pagination.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.flowLayoutPanel_Pagination.AutoSize = true;
            this.flowLayoutPanel_Pagination.Controls.Add(this.btnPrevPage);
            this.flowLayoutPanel_Pagination.Controls.Add(this.lblPageInfo);
            this.flowLayoutPanel_Pagination.Controls.Add(this.btnNextPage);
            this.flowLayoutPanel_Pagination.Location = new System.Drawing.Point(458, 4);
            this.flowLayoutPanel_Pagination.Name = "flowLayoutPanel_Pagination";
            this.flowLayoutPanel_Pagination.Size = new System.Drawing.Size(226, 36);
            this.flowLayoutPanel_Pagination.TabIndex = 3;
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.AutoSize = true;
            this.btnPrevPage.FlatAppearance.BorderSize = 0;
            this.btnPrevPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevPage.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnPrevPage.ForeColor = System.Drawing.Color.White;
            this.btnPrevPage.Location = new System.Drawing.Point(3, 3);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(66, 30);
            this.btnPrevPage.TabIndex = 0;
            this.btnPrevPage.Text = "< Prev";
            this.btnPrevPage.UseVisualStyleBackColor = true;
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblPageInfo.ForeColor = System.Drawing.Color.White;
            this.lblPageInfo.Location = new System.Drawing.Point(75, 8);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(73, 20);
            this.lblPageInfo.TabIndex = 1;
            this.lblPageInfo.Text = "Page 1/1";
            // 
            // btnNextPage
            // 
            this.btnNextPage.AutoSize = true;
            this.btnNextPage.FlatAppearance.BorderSize = 0;
            this.btnNextPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextPage.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnNextPage.ForeColor = System.Drawing.Color.White;
            this.btnNextPage.Location = new System.Drawing.Point(154, 3);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(69, 30);
            this.btnNextPage.TabIndex = 2;
            this.btnNextPage.Text = "Next >";
            this.btnNextPage.UseVisualStyleBackColor = true;
            // 
            // panel_Filter
            // 
            this.panel_Filter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.panel_Filter.Controls.Add(this.label_FilterVVS);
            this.panel_Filter.Controls.Add(this.label_FilterAI);
            this.panel_Filter.Controls.Add(this.comboBox_FilterVVS);
            this.panel_Filter.Controls.Add(this.comboBox_FilterAI);
            this.panel_Filter.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_Filter.Location = new System.Drawing.Point(0, 40);
            this.panel_Filter.Name = "panel_Filter";
            this.panel_Filter.Size = new System.Drawing.Size(1085, 48);
            this.panel_Filter.TabIndex = 4;
            // 
            // label_FilterVVS
            // 
            this.label_FilterVVS.AutoSize = true;
            this.label_FilterVVS.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label_FilterVVS.ForeColor = System.Drawing.Color.White;
            this.label_FilterVVS.Location = new System.Drawing.Point(250, 16);
            this.label_FilterVVS.Name = "label_FilterVVS";
            this.label_FilterVVS.Size = new System.Drawing.Size(83, 20);
            this.label_FilterVVS.TabIndex = 3;
            this.label_FilterVVS.Text = "VVS Filter:";
            // 
            // label_FilterAI
            // 
            this.label_FilterAI.AutoSize = true;
            this.label_FilterAI.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label_FilterAI.ForeColor = System.Drawing.Color.White;
            this.label_FilterAI.Location = new System.Drawing.Point(20, 16);
            this.label_FilterAI.Name = "label_FilterAI";
            this.label_FilterAI.Size = new System.Drawing.Size(69, 20);
            this.label_FilterAI.TabIndex = 2;
            this.label_FilterAI.Text = "AI Filter:";
            // 
            // comboBox_FilterVVS
            // 
            this.comboBox_FilterVVS.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_FilterVVS.FormattingEnabled = true;
            this.comboBox_FilterVVS.Location = new System.Drawing.Point(339, 14);
            this.comboBox_FilterVVS.Name = "comboBox_FilterVVS";
            this.comboBox_FilterVVS.Size = new System.Drawing.Size(121, 28);
            this.comboBox_FilterVVS.TabIndex = 1;
            // 
            // comboBox_FilterAI
            // 
            this.comboBox_FilterAI.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_FilterAI.FormattingEnabled = true;
            this.comboBox_FilterAI.Location = new System.Drawing.Point(95, 14);
            this.comboBox_FilterAI.Name = "comboBox_FilterAI";
            this.comboBox_FilterAI.Size = new System.Drawing.Size(121, 28);
            this.comboBox_FilterAI.TabIndex = 0;
            // 
            // DefectDetailControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowLayoutPanel_DefectImages);
            this.Controls.Add(this.panel_Filter);
            this.Controls.Add(this.panel_Pagination);
            this.Controls.Add(this.panel_Top);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "DefectDetailControl";
            this.Size = new System.Drawing.Size(1085, 635);
            this.panel_Top.ResumeLayout(false);
            this.panel_Pagination.ResumeLayout(false);
            this.panel_Pagination.PerformLayout();
            this.flowLayoutPanel_Pagination.ResumeLayout(false);
            this.flowLayoutPanel_Pagination.PerformLayout();
            this.panel_Filter.ResumeLayout(false);
            this.panel_Filter.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_DefectImages;
        private System.Windows.Forms.Panel panel_Top;
        private System.Windows.Forms.Label label_DetailTitle;
        private System.Windows.Forms.Panel panel_Pagination;
        private System.Windows.Forms.Label lblPageInfo;
        private System.Windows.Forms.Button btnNextPage;
        private System.Windows.Forms.Button btnPrevPage;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_Pagination;
        private System.Windows.Forms.Panel panel_Filter;
        private System.Windows.Forms.ComboBox comboBox_FilterVVS;
        private System.Windows.Forms.ComboBox comboBox_FilterAI;
        private System.Windows.Forms.Label label_FilterVVS;
        private System.Windows.Forms.Label label_FilterAI;
    }
}

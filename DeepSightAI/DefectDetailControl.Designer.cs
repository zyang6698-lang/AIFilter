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
            this.panel_DefectDetail = new System.Windows.Forms.Panel();
            this.flowLayoutPanel_DefectImages = new System.Windows.Forms.FlowLayoutPanel();
            this.panel_Pagination = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.panel_DetailTitle = new System.Windows.Forms.Panel();
            this.label_DetailTitle = new System.Windows.Forms.Label();
            this.panel_DefectDetail.SuspendLayout();
            this.panel_Pagination.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel_DetailTitle.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_DefectDetail
            // 
            this.panel_DefectDetail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.panel_DefectDetail.Controls.Add(this.flowLayoutPanel_DefectImages);
            this.panel_DefectDetail.Controls.Add(this.panel_Pagination);
            this.panel_DefectDetail.Controls.Add(this.panel_DetailTitle);
            this.panel_DefectDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_DefectDetail.Location = new System.Drawing.Point(0, 0);
            this.panel_DefectDetail.Name = "panel_DefectDetail";
            this.panel_DefectDetail.Size = new System.Drawing.Size(800, 600);
            this.panel_DefectDetail.TabIndex = 2;
            // 
            // flowLayoutPanel_DefectImages
            // 
            this.flowLayoutPanel_DefectImages.AutoScroll = true;
            this.flowLayoutPanel_DefectImages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.flowLayoutPanel_DefectImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel_DefectImages.Location = new System.Drawing.Point(0, 40);
            this.flowLayoutPanel_DefectImages.Name = "flowLayoutPanel_DefectImages";
            this.flowLayoutPanel_DefectImages.Size = new System.Drawing.Size(800, 520);
            this.flowLayoutPanel_DefectImages.TabIndex = 1;
            // 
            // panel_Pagination
            // 
            this.panel_Pagination.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));

            this.panel_Pagination.Controls.Add(this.flowLayoutPanel1);
            this.panel_Pagination.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_Pagination.Location = new System.Drawing.Point(0, 560);
            this.panel_Pagination.Name = "panel_Pagination";
            this.panel_Pagination.Size = new System.Drawing.Size(800, 40);
            this.panel_Pagination.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanel1.Controls.Add(this.btnPrevPage);
            this.flowLayoutPanel1.Controls.Add(this.lblPageInfo);
            this.flowLayoutPanel1.Controls.Add(this.btnNextPage);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(236, 6);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(328, 28);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));

            this.btnPrevPage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrevPage.Enabled = false;
            this.btnPrevPage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnPrevPage.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnPrevPage.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.btnPrevPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevPage.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.btnPrevPage.ForeColor = System.Drawing.Color.White;
            this.btnPrevPage.Location = new System.Drawing.Point(0, 0);
            this.btnPrevPage.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(100, 28);
            this.btnPrevPage.TabIndex = 0;
            this.btnPrevPage.Text = "? 上一页";
            this.btnPrevPage.UseVisualStyleBackColor = false;
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblPageInfo.ForeColor = System.Drawing.Color.Gainsboro;
            this.lblPageInfo.Location = new System.Drawing.Point(110, 0);
            this.lblPageInfo.Margin = new System.Windows.Forms.Padding(0);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.lblPageInfo.Size = new System.Drawing.Size(108, 24);
            this.lblPageInfo.TabIndex = 1;
            this.lblPageInfo.Text = "第 999/999 页";
            this.lblPageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnNextPage
            // 
            this.btnNextPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));

            this.btnNextPage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNextPage.Enabled = false;
            this.btnNextPage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnNextPage.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnNextPage.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.btnNextPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextPage.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.btnNextPage.ForeColor = System.Drawing.Color.White;
            this.btnNextPage.Location = new System.Drawing.Point(228, 0);
            this.btnNextPage.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(100, 28);
            this.btnNextPage.TabIndex = 2;
            this.btnNextPage.Text = "下一页 ?";
            this.btnNextPage.UseVisualStyleBackColor = false;
            // 
            // panel_DetailTitle
            // 
            this.panel_DetailTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.panel_DetailTitle.Controls.Add(this.label_DetailTitle);
            this.panel_DetailTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_DetailTitle.Location = new System.Drawing.Point(0, 0);
            this.panel_DetailTitle.Name = "panel_DetailTitle";
            this.panel_DetailTitle.Size = new System.Drawing.Size(800, 40);
            this.panel_DetailTitle.TabIndex = 0;
            // 
            // label_DetailTitle
            // 
            this.label_DetailTitle.AutoSize = true;
            this.label_DetailTitle.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_DetailTitle.ForeColor = System.Drawing.Color.White;
            this.label_DetailTitle.Location = new System.Drawing.Point(10, 9);
            this.label_DetailTitle.Name = "label_DetailTitle";
            this.label_DetailTitle.Size = new System.Drawing.Size(74, 22);
            this.label_DetailTitle.TabIndex = 0;
            this.label_DetailTitle.Text = "缺陷详情";
            // 
            // DefectDetailControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel_DefectDetail);
            this.Name = "DefectDetailControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.panel_DefectDetail.ResumeLayout(false);
            this.panel_Pagination.ResumeLayout(false);
            this.panel_Pagination.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panel_DetailTitle.ResumeLayout(false);
            this.panel_DetailTitle.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_DefectDetail;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_DefectImages;
        private System.Windows.Forms.Panel panel_DetailTitle;
        private System.Windows.Forms.Label label_DetailTitle;
        private System.Windows.Forms.Panel panel_Pagination;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnPrevPage;
        private System.Windows.Forms.Label lblPageInfo;
        private System.Windows.Forms.Button btnNextPage;
    }
}

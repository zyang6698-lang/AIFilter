﻿namespace DeepSightAI
{
    partial class FrSearch
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
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panel_Title = new System.Windows.Forms.Panel();
            this.label_Title = new System.Windows.Forms.Label();
            this.label_Loading = new System.Windows.Forms.Label();
            this.btn_Refresh = new System.Windows.Forms.Button();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.dgv_Lots = new System.Windows.Forms.DataGridView();
            this.panel_Detail = new System.Windows.Forms.Panel();
            this.dgv_Panels = new System.Windows.Forms.DataGridView();
            this.label_DetailTitle = new System.Windows.Forms.Label();
            this.panel_Paging = new System.Windows.Forms.Panel();
            this.btn_PrevPage = new System.Windows.Forms.Button();
            this.label_PageInfo = new System.Windows.Forms.Label();
            this.btn_NextPage = new System.Windows.Forms.Button();
            this.tableLayoutPanel.SuspendLayout();
            this.panel_Title.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Lots)).BeginInit();
            this.panel_Detail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Panels)).BeginInit();
            this.panel_Paging.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.panel_Title, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.splitContainer, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.panel_Paging, 0, 2);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 3;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 71F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(857, 501);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // panel_Title
            // 
            this.panel_Title.Controls.Add(this.label_Title);
            this.panel_Title.Controls.Add(this.label_Loading);
            this.panel_Title.Controls.Add(this.btn_Refresh);
            this.panel_Title.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Title.Location = new System.Drawing.Point(3, 2);
            this.panel_Title.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel_Title.Name = "panel_Title";
            this.panel_Title.Size = new System.Drawing.Size(851, 34);
            this.panel_Title.TabIndex = 0;
            // 
            // label_Title
            // 
            this.label_Title.AutoSize = true;
            this.label_Title.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.label_Title.ForeColor = System.Drawing.Color.White;
            this.label_Title.Location = new System.Drawing.Point(9, 4);
            this.label_Title.Name = "label_Title";
            this.label_Title.Size = new System.Drawing.Size(158, 31);
            this.label_Title.TabIndex = 0;
            this.label_Title.Text = "生产事件查看";
            // 
            // label_Loading
            // 
            this.label_Loading.AutoSize = true;
            this.label_Loading.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.label_Loading.ForeColor = System.Drawing.Color.Yellow;
            this.label_Loading.Location = new System.Drawing.Point(178, 8);
            this.label_Loading.Name = "label_Loading";
            this.label_Loading.Size = new System.Drawing.Size(0, 23);
            this.label_Loading.TabIndex = 1;
            this.label_Loading.Visible = false;
            // 
            // btn_Refresh
            // 
            this.btn_Refresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Refresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btn_Refresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(117)))), ((int)(((byte)(142)))));
            this.btn_Refresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Refresh.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btn_Refresh.ForeColor = System.Drawing.Color.White;
            this.btn_Refresh.Location = new System.Drawing.Point(775, 4);
            this.btn_Refresh.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.Size = new System.Drawing.Size(67, 25);
            this.btn_Refresh.TabIndex = 2;
            this.btn_Refresh.Text = "刷新";
            this.btn_Refresh.UseVisualStyleBackColor = false;
            this.btn_Refresh.Click += new System.EventHandler(this.btn_Refresh_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(3, 40);
            this.splitContainer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.dgv_Lots);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.panel_Detail);
            this.splitContainer.Size = new System.Drawing.Size(851, 388);
            this.splitContainer.SplitterDistance = 192;
            this.splitContainer.SplitterWidth = 3;
            this.splitContainer.TabIndex = 1;
            // 
            // dgv_Lots
            // 
            this.dgv_Lots.AllowUserToAddRows = false;
            this.dgv_Lots.AllowUserToDeleteRows = false;
            this.dgv_Lots.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgv_Lots.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            this.dgv_Lots.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgv_Lots.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgv_Lots.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgv_Lots.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Lots.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_Lots.EnableHeadersVisualStyles = false;
            this.dgv_Lots.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(70)))), ((int)(((byte)(90)))));
            this.dgv_Lots.Location = new System.Drawing.Point(0, 0);
            this.dgv_Lots.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgv_Lots.MultiSelect = false;
            this.dgv_Lots.Name = "dgv_Lots";
            this.dgv_Lots.ReadOnly = true;
            this.dgv_Lots.RowHeadersVisible = false;
            this.dgv_Lots.RowHeadersWidth = 51;
            this.dgv_Lots.RowTemplate.Height = 32;
            this.dgv_Lots.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_Lots.Size = new System.Drawing.Size(851, 192);
            this.dgv_Lots.TabIndex = 0;
            this.dgv_Lots.SelectionChanged += new System.EventHandler(this.dgv_Lots_SelectionChanged);
            // 
            // panel_Detail
            // 
            this.panel_Detail.Controls.Add(this.dgv_Panels);
            this.panel_Detail.Controls.Add(this.label_DetailTitle);
            this.panel_Detail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Detail.Location = new System.Drawing.Point(0, 0);
            this.panel_Detail.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel_Detail.Name = "panel_Detail";
            this.panel_Detail.Size = new System.Drawing.Size(851, 193);
            this.panel_Detail.TabIndex = 0;
            // 
            // dgv_Panels
            // 
            this.dgv_Panels.AllowUserToAddRows = false;
            this.dgv_Panels.AllowUserToDeleteRows = false;
            this.dgv_Panels.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgv_Panels.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgv_Panels.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgv_Panels.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgv_Panels.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgv_Panels.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Panels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_Panels.EnableHeadersVisualStyles = false;
            this.dgv_Panels.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(70)))), ((int)(((byte)(90)))));
            this.dgv_Panels.Location = new System.Drawing.Point(0, 23);
            this.dgv_Panels.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgv_Panels.Name = "dgv_Panels";
            this.dgv_Panels.ReadOnly = true;
            this.dgv_Panels.RowHeadersVisible = false;
            this.dgv_Panels.RowHeadersWidth = 51;
            this.dgv_Panels.RowTemplate.Height = 28;
            this.dgv_Panels.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_Panels.Size = new System.Drawing.Size(851, 170);
            this.dgv_Panels.TabIndex = 1;
            // 
            // label_DetailTitle
            // 
            this.label_DetailTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.label_DetailTitle.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.label_DetailTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(210)))), ((int)(((byte)(240)))));
            this.label_DetailTitle.Location = new System.Drawing.Point(0, 0);
            this.label_DetailTitle.Name = "label_DetailTitle";
            this.label_DetailTitle.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.label_DetailTitle.Size = new System.Drawing.Size(851, 23);
            this.label_DetailTitle.TabIndex = 0;
            this.label_DetailTitle.Text = "请选择一个Lot查看详情";
            this.label_DetailTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel_Paging
            // 
            this.panel_Paging.Controls.Add(this.btn_PrevPage);
            this.panel_Paging.Controls.Add(this.label_PageInfo);
            this.panel_Paging.Controls.Add(this.btn_NextPage);
            this.panel_Paging.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Paging.Location = new System.Drawing.Point(3, 432);
            this.panel_Paging.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel_Paging.Name = "panel_Paging";
            this.panel_Paging.Size = new System.Drawing.Size(851, 67);
            this.panel_Paging.TabIndex = 2;
            // 
            // btn_PrevPage
            // 
            this.btn_PrevPage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_PrevPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btn_PrevPage.Enabled = false;
            this.btn_PrevPage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(117)))), ((int)(((byte)(142)))));
            this.btn_PrevPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_PrevPage.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btn_PrevPage.ForeColor = System.Drawing.Color.White;
            this.btn_PrevPage.Location = new System.Drawing.Point(291, 21);
            this.btn_PrevPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_PrevPage.Name = "btn_PrevPage";
            this.btn_PrevPage.Size = new System.Drawing.Size(90, 37);
            this.btn_PrevPage.TabIndex = 0;
            this.btn_PrevPage.Text = "◀ 上一页";
            this.btn_PrevPage.UseVisualStyleBackColor = false;
            this.btn_PrevPage.Click += new System.EventHandler(this.btn_PrevPage_Click);
            // 
            // label_PageInfo
            // 
            this.label_PageInfo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label_PageInfo.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.label_PageInfo.ForeColor = System.Drawing.Color.White;
            this.label_PageInfo.Location = new System.Drawing.Point(395, 20);
            this.label_PageInfo.Name = "label_PageInfo";
            this.label_PageInfo.Size = new System.Drawing.Size(108, 37);
            this.label_PageInfo.TabIndex = 1;
            this.label_PageInfo.Text = "第 1 页";
            this.label_PageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_NextPage
            // 
            this.btn_NextPage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_NextPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btn_NextPage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(117)))), ((int)(((byte)(142)))));
            this.btn_NextPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_NextPage.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btn_NextPage.ForeColor = System.Drawing.Color.White;
            this.btn_NextPage.Location = new System.Drawing.Point(509, 21);
            this.btn_NextPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_NextPage.Name = "btn_NextPage";
            this.btn_NextPage.Size = new System.Drawing.Size(90, 37);
            this.btn_NextPage.TabIndex = 2;
            this.btn_NextPage.Text = "下一页 ▶";
            this.btn_NextPage.UseVisualStyleBackColor = false;
            this.btn_NextPage.Click += new System.EventHandler(this.btn_NextPage_Click);
            // 
            // FrSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.ClientSize = new System.Drawing.Size(857, 501);
            this.Controls.Add(this.tableLayoutPanel);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FrSearch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrSearch";
            this.Load += new System.EventHandler(this.FrSearch_Load);
            this.tableLayoutPanel.ResumeLayout(false);
            this.panel_Title.ResumeLayout(false);
            this.panel_Title.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Lots)).EndInit();
            this.panel_Detail.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Panels)).EndInit();
            this.panel_Paging.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Panel panel_Title;
        private System.Windows.Forms.Label label_Title;
        private System.Windows.Forms.Label label_Loading;
        private System.Windows.Forms.Button btn_Refresh;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.DataGridView dgv_Lots;
        private System.Windows.Forms.Panel panel_Detail;
        private System.Windows.Forms.Label label_DetailTitle;
        private System.Windows.Forms.DataGridView dgv_Panels;
        private System.Windows.Forms.Panel panel_Paging;
        private System.Windows.Forms.Button btn_PrevPage;
        private System.Windows.Forms.Label label_PageInfo;
        private System.Windows.Forms.Button btn_NextPage;
    }
}


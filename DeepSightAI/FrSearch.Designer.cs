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
            this.btn_Refresh = new DeepSightAI.StyledButton();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.dgv_Lots = new System.Windows.Forms.DataGridView();
            this.panel_Detail = new System.Windows.Forms.Panel();
            this.dgv_Panels = new System.Windows.Forms.DataGridView();
            this.label_DetailTitle = new System.Windows.Forms.Label();
            this.tableLayoutPanel.SuspendLayout();
            this.panel_Title.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Lots)).BeginInit();
            this.panel_Detail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Panels)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.panel_Title, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.splitContainer, 0, 1);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
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
            this.panel_Title.Size = new System.Drawing.Size(851, 46);
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
            this.btn_Refresh.Location = new System.Drawing.Point(745, 4);
            this.btn_Refresh.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.Size = new System.Drawing.Size(100, 40);
            this.btn_Refresh.TabIndex = 2;
            this.btn_Refresh.Text = "刷新";
            this.btn_Refresh.Click += new System.EventHandler(this.btn_Refresh_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(3, 52);
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
            this.splitContainer.Size = new System.Drawing.Size(851, 376);
            this.splitContainer.SplitterDistance = 186;
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
            this.dgv_Lots.Size = new System.Drawing.Size(851, 186);
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
            this.panel_Detail.Size = new System.Drawing.Size(851, 187);
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
            this.dgv_Panels.Size = new System.Drawing.Size(851, 164);
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
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Panel panel_Title;
        private System.Windows.Forms.Label label_Title;
        private System.Windows.Forms.Label label_Loading;
        private DeepSightAI.StyledButton btn_Refresh;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.DataGridView dgv_Lots;
        private System.Windows.Forms.Panel panel_Detail;
        private System.Windows.Forms.Label label_DetailTitle;
        private System.Windows.Forms.DataGridView dgv_Panels;

    }
}


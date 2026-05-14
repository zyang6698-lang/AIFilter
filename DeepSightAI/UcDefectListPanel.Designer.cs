namespace DeepSightAI
{
    partial class UcDefectListPanel
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle cs1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle cs2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle cs3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle cs4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle cs5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView_Defects = new Sunny.UI.UIDataGridView();
            this.col_SN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Lot = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_MachineId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_ProductSerial = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Side = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_AiStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_ManualStatus = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.col_VrsStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_DefectCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_PathIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_DetectionDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel_GridTop = new System.Windows.Forms.TableLayoutPanel();
            this.txt_SnFilter = new System.Windows.Forms.TextBox();
            this.btn_SnSearch = new DeepSightAI.StyledButton();
            this.btn_ImageDetail = new DeepSightAI.StyledButton();
            this.btn_MoreActions = new DeepSightAI.StyledButton();
            this.contextMenu_Actions = new System.Windows.Forms.ContextMenuStrip();
            this.menuItem_AddToDataset = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_RunTest = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_SecondaryInference = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Defects)).BeginInit();
            this.tableLayoutPanel_GridTop.SuspendLayout();
            this.contextMenu_Actions.SuspendLayout();
            this.SuspendLayout();
            //
            // dataGridView_Defects
            //
            this.dataGridView_Defects.AllowUserToAddRows = false;
            this.dataGridView_Defects.AllowUserToDeleteRows = false;
            cs1.BackColor = System.Drawing.Color.FromArgb(35, 55, 70);
            cs1.ForeColor = System.Drawing.Color.White;
            this.dataGridView_Defects.AlternatingRowsDefaultCellStyle = cs1;
            this.dataGridView_Defects.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView_Defects.BackgroundColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.dataGridView_Defects.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView_Defects.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            cs2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            cs2.BackColor = System.Drawing.Color.FromArgb(44, 62, 80);
            cs2.Font = new System.Drawing.Font("宋体", 12F);
            cs2.ForeColor = System.Drawing.Color.White;
            cs2.SelectionBackColor = System.Drawing.Color.FromArgb(44, 62, 80);
            cs2.SelectionForeColor = System.Drawing.Color.White;
            cs2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_Defects.ColumnHeadersDefaultCellStyle = cs2;
            this.dataGridView_Defects.ColumnHeadersHeight = 32;
            this.dataGridView_Defects.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView_Defects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.col_SN, this.col_Lot, this.col_MachineId, this.col_ProductSerial,
                this.col_Side, this.col_AiStatus, this.col_ManualStatus, this.col_VrsStatus,
                this.col_DefectCount, this.col_PathIndex, this.col_DetectionDate });
            cs3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            cs3.BackColor = System.Drawing.Color.FromArgb(29, 48, 60);
            cs3.Font = new System.Drawing.Font("宋体", 9F);
            cs3.ForeColor = System.Drawing.Color.White;
            cs3.SelectionBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            cs3.SelectionForeColor = System.Drawing.Color.White;
            this.dataGridView_Defects.DefaultCellStyle = cs3;
            this.dataGridView_Defects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_Defects.EnableHeadersVisualStyles = false;
            this.dataGridView_Defects.Font = new System.Drawing.Font("宋体", 12F);
            this.dataGridView_Defects.GridColor = System.Drawing.Color.FromArgb(60, 80, 95);
            this.dataGridView_Defects.Margin = new System.Windows.Forms.Padding(0);
            this.dataGridView_Defects.MultiSelect = false;
            this.dataGridView_Defects.Name = "dataGridView_Defects";
            this.dataGridView_Defects.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            cs4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            cs4.BackColor = System.Drawing.Color.FromArgb(29, 48, 60);
            cs4.Font = new System.Drawing.Font("宋体", 12F);
            cs4.ForeColor = System.Drawing.Color.White;
            cs4.SelectionBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            cs4.SelectionForeColor = System.Drawing.Color.White;
            cs4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_Defects.RowHeadersDefaultCellStyle = cs4;
            this.dataGridView_Defects.RowHeadersVisible = false;
            this.dataGridView_Defects.RowHeadersWidth = 51;
            cs5.BackColor = System.Drawing.Color.FromArgb(29, 48, 60);
            cs5.Font = new System.Drawing.Font("宋体", 12F);
            this.dataGridView_Defects.RowsDefaultCellStyle = cs5;
            this.dataGridView_Defects.RowTemplate.Height = 23;
            this.dataGridView_Defects.ScrollBarBackColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.dataGridView_Defects.ScrollBarColor = System.Drawing.Color.FromArgb(60, 80, 95);
            this.dataGridView_Defects.ScrollBarRectColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.dataGridView_Defects.ScrollBarStyleInherited = false;
            this.dataGridView_Defects.SelectedIndex = -1;
            this.dataGridView_Defects.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView_Defects.StripeEvenColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.dataGridView_Defects.StripeOddColor = System.Drawing.Color.FromArgb(35, 55, 70);
            this.dataGridView_Defects.Style = Sunny.UI.UIStyle.Custom;
            this.dataGridView_Defects.StyleCustomMode = true;
            this.dataGridView_Defects.TabIndex = 0;
            //
            // Columns
            //
            this.col_SN.DataPropertyName = "SerialNumber"; this.col_SN.HeaderText = "序列号"; this.col_SN.MinimumWidth = 6; this.col_SN.Name = "col_SN"; this.col_SN.ReadOnly = true;
            this.col_Lot.DataPropertyName = "LotNumber"; this.col_Lot.HeaderText = "Lot号"; this.col_Lot.MinimumWidth = 6; this.col_Lot.Name = "col_Lot"; this.col_Lot.ReadOnly = true;
            this.col_MachineId.DataPropertyName = "MachineId"; this.col_MachineId.HeaderText = "机台号"; this.col_MachineId.MinimumWidth = 6; this.col_MachineId.Name = "col_MachineId"; this.col_MachineId.ReadOnly = true;
            this.col_ProductSerial.DataPropertyName = "ProductSerial"; this.col_ProductSerial.HeaderText = "料号"; this.col_ProductSerial.MinimumWidth = 6; this.col_ProductSerial.Name = "col_ProductSerial"; this.col_ProductSerial.ReadOnly = true;
            this.col_Side.DataPropertyName = "Side"; this.col_Side.HeaderText = "面次"; this.col_Side.MinimumWidth = 6; this.col_Side.Name = "col_Side"; this.col_Side.ReadOnly = true;
            this.col_AiStatus.DataPropertyName = "AiStatus"; this.col_AiStatus.HeaderText = "AI状态"; this.col_AiStatus.MinimumWidth = 6; this.col_AiStatus.Name = "col_AiStatus"; this.col_AiStatus.ReadOnly = true;
            this.col_ManualStatus.DataPropertyName = "ManualStatus"; this.col_ManualStatus.HeaderText = "人工判定"; this.col_ManualStatus.Items.AddRange(new object[] { "未判定", "OK", "NG" }); this.col_ManualStatus.MinimumWidth = 6; this.col_ManualStatus.Name = "col_ManualStatus"; this.col_ManualStatus.Resizable = System.Windows.Forms.DataGridViewTriState.True; this.col_ManualStatus.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.col_VrsStatus.DataPropertyName = "VrsStatus"; this.col_VrsStatus.HeaderText = "VRS状态"; this.col_VrsStatus.MinimumWidth = 6; this.col_VrsStatus.Name = "col_VrsStatus"; this.col_VrsStatus.ReadOnly = true;
            this.col_DefectCount.DataPropertyName = "DefectCount"; this.col_DefectCount.HeaderText = "缺陷数"; this.col_DefectCount.MinimumWidth = 6; this.col_DefectCount.Name = "col_DefectCount"; this.col_DefectCount.ReadOnly = true;
            this.col_PathIndex.DataPropertyName = "PathIndex"; this.col_PathIndex.HeaderText = "路径索引"; this.col_PathIndex.MinimumWidth = 6; this.col_PathIndex.Name = "col_PathIndex"; this.col_PathIndex.ReadOnly = true; this.col_PathIndex.Visible = false;
            this.col_DetectionDate.DataPropertyName = "DetectionDate"; this.col_DetectionDate.HeaderText = "检测日期"; this.col_DetectionDate.MinimumWidth = 6; this.col_DetectionDate.Name = "col_DetectionDate"; this.col_DetectionDate.ReadOnly = true;
            //
            // tableLayoutPanel_GridTop
            //
            this.tableLayoutPanel_GridTop.BackColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.tableLayoutPanel_GridTop.ColumnCount = 5;
            this.tableLayoutPanel_GridTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 240F));
            this.tableLayoutPanel_GridTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel_GridTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_GridTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableLayoutPanel_GridTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel_GridTop.Controls.Add(this.txt_SnFilter, 0, 0);
            this.tableLayoutPanel_GridTop.Controls.Add(this.btn_SnSearch, 1, 0);
            this.tableLayoutPanel_GridTop.Controls.Add(this.btn_ImageDetail, 3, 0);
            this.tableLayoutPanel_GridTop.Controls.Add(this.btn_MoreActions, 4, 0);
            this.tableLayoutPanel_GridTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel_GridTop.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_GridTop.Name = "tableLayoutPanel_GridTop";
            this.tableLayoutPanel_GridTop.Padding = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel_GridTop.RowCount = 1;
            this.tableLayoutPanel_GridTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_GridTop.Size = new System.Drawing.Size(1550, 42);
            this.tableLayoutPanel_GridTop.TabIndex = 4;
            //
            // txt_SnFilter
            //
            this.txt_SnFilter.BackColor = System.Drawing.Color.FromArgb(35, 55, 70);
            this.txt_SnFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txt_SnFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_SnFilter.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txt_SnFilter.ForeColor = System.Drawing.Color.White;
            this.txt_SnFilter.Margin = new System.Windows.Forms.Padding(2, 6, 2, 6);
            this.txt_SnFilter.Name = "txt_SnFilter";
            this.txt_SnFilter.TabIndex = 0;
            //
            // btn_SnSearch
            //
            this.btn_SnSearch.BackColor = System.Drawing.Color.FromArgb(0, 64, 82);
            this.btn_SnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_SnSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_SnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_SnSearch.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btn_SnSearch.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.btn_SnSearch.Margin = new System.Windows.Forms.Padding(2, 2, 4, 2);
            this.btn_SnSearch.Name = "btn_SnSearch";
            this.btn_SnSearch.TabIndex = 1;
            this.btn_SnSearch.Text = "搜索";
            this.btn_SnSearch.UseVisualStyleBackColor = false;
            //
            // contextMenu_Actions（深色主题菜单）
            //
            this.contextMenu_Actions.BackColor = System.Drawing.Color.FromArgb(35, 55, 70);
            this.contextMenu_Actions.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.contextMenu_Actions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuItem_AddToDataset,
                this.menuItem_RunTest,
                this.menuItem_SecondaryInference });
            this.contextMenu_Actions.Name = "contextMenu_Actions";
            this.contextMenu_Actions.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            //
            // menuItem_AddToDataset
            //
            this.menuItem_AddToDataset.BackColor = System.Drawing.Color.FromArgb(35, 55, 70);
            this.menuItem_AddToDataset.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.menuItem_AddToDataset.Name = "menuItem_AddToDataset";
            this.menuItem_AddToDataset.Text = "添加到一致性数据集";
            //
            // menuItem_RunTest
            //
            this.menuItem_RunTest.BackColor = System.Drawing.Color.FromArgb(35, 55, 70);
            this.menuItem_RunTest.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.menuItem_RunTest.Name = "menuItem_RunTest";
            this.menuItem_RunTest.Text = "模型一致性测试";
            //
            // menuItem_SecondaryInference
            //
            this.menuItem_SecondaryInference.BackColor = System.Drawing.Color.FromArgb(35, 55, 70);
            this.menuItem_SecondaryInference.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.menuItem_SecondaryInference.Name = "menuItem_SecondaryInference";
            this.menuItem_SecondaryInference.Text = "运行二次推理";
            //
            // btn_ImageDetail
            //
            this.btn_ImageDetail.BackColor = System.Drawing.Color.FromArgb(0, 100, 120);
            this.btn_ImageDetail.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_ImageDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_ImageDetail.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_ImageDetail.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btn_ImageDetail.ForeColor = System.Drawing.Color.FromArgb(180, 230, 240);
            this.btn_ImageDetail.Margin = new System.Windows.Forms.Padding(2);
            this.btn_ImageDetail.Name = "btn_ImageDetail";
            this.btn_ImageDetail.TabIndex = 2;
            this.btn_ImageDetail.Text = "🖼 图片详情";
            this.btn_ImageDetail.UseVisualStyleBackColor = false;
            //
            // btn_MoreActions
            //
            this.btn_MoreActions.BackColor = System.Drawing.Color.FromArgb(0, 64, 82);
            this.btn_MoreActions.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_MoreActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_MoreActions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_MoreActions.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btn_MoreActions.ForeColor = System.Drawing.Color.FromArgb(216, 219, 188);
            this.btn_MoreActions.Margin = new System.Windows.Forms.Padding(2);
            this.btn_MoreActions.Name = "btn_MoreActions";
            this.btn_MoreActions.TabIndex = 3;
            this.btn_MoreActions.Text = "更多操作 ▾";
            this.btn_MoreActions.UseVisualStyleBackColor = false;
            //
            // UcDefectListPanel
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(29, 48, 60);
            this.Controls.Add(this.dataGridView_Defects);
            this.Controls.Add(this.tableLayoutPanel_GridTop);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UcDefectListPanel";
            this.Size = new System.Drawing.Size(1550, 1058);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Defects)).EndInit();
            this.tableLayoutPanel_GridTop.ResumeLayout(false);
            this.tableLayoutPanel_GridTop.PerformLayout();
            this.contextMenu_Actions.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UIDataGridView dataGridView_Defects;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_SN;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Lot;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_MachineId;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_ProductSerial;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Side;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_AiStatus;
        private System.Windows.Forms.DataGridViewComboBoxColumn col_ManualStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_VrsStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_DefectCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_PathIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_DetectionDate;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_GridTop;
        private System.Windows.Forms.TextBox txt_SnFilter;
        private DeepSightAI.StyledButton btn_SnSearch;
        private DeepSightAI.StyledButton btn_ImageDetail;
        private DeepSightAI.StyledButton btn_MoreActions;
        private System.Windows.Forms.ContextMenuStrip contextMenu_Actions;
        private System.Windows.Forms.ToolStripMenuItem menuItem_AddToDataset;
        private System.Windows.Forms.ToolStripMenuItem menuItem_RunTest;
        private System.Windows.Forms.ToolStripMenuItem menuItem_SecondaryInference;
    }
}

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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this.contextMenu_Actions = new System.Windows.Forms.ContextMenuStrip(this.components);
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
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            this.dataGridView_Defects.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView_Defects.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView_Defects.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dataGridView_Defects.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView_Defects.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 12F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_Defects.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView_Defects.ColumnHeadersHeight = 32;
            this.dataGridView_Defects.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView_Defects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_SN,
            this.col_Lot,
            this.col_MachineId,
            this.col_ProductSerial,
            this.col_Side,
            this.col_AiStatus,
            this.col_ManualStatus,
            this.col_VrsStatus,
            this.col_DefectCount,
            this.col_PathIndex,
            this.col_DetectionDate});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("宋体", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView_Defects.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView_Defects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_Defects.EnableHeadersVisualStyles = false;
            this.dataGridView_Defects.Font = new System.Drawing.Font("宋体", 12F);
            this.dataGridView_Defects.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.dataGridView_Defects.Location = new System.Drawing.Point(0, 42);
            this.dataGridView_Defects.Margin = new System.Windows.Forms.Padding(0);
            this.dataGridView_Defects.MultiSelect = false;
            this.dataGridView_Defects.Name = "dataGridView_Defects";
            this.dataGridView_Defects.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("宋体", 12F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_Defects.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridView_Defects.RowHeadersVisible = false;
            this.dataGridView_Defects.RowHeadersWidth = 51;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("宋体", 12F);
            this.dataGridView_Defects.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridView_Defects.RowTemplate.Height = 23;
            this.dataGridView_Defects.ScrollBarBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dataGridView_Defects.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.dataGridView_Defects.ScrollBarRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dataGridView_Defects.ScrollBarStyleInherited = false;
            this.dataGridView_Defects.SelectedIndex = -1;
            this.dataGridView_Defects.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView_Defects.Size = new System.Drawing.Size(1550, 1016);
            this.dataGridView_Defects.StripeEvenColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dataGridView_Defects.StripeOddColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            this.dataGridView_Defects.Style = Sunny.UI.UIStyle.Custom;
            this.dataGridView_Defects.StyleCustomMode = true;
            this.dataGridView_Defects.TabIndex = 0;
            // 
            // col_SN
            // 
            this.col_SN.DataPropertyName = "SerialNumber";
            this.col_SN.HeaderText = "序列号";
            this.col_SN.MinimumWidth = 6;
            this.col_SN.Name = "col_SN";
            this.col_SN.ReadOnly = true;
            // 
            // col_Lot
            // 
            this.col_Lot.DataPropertyName = "LotNumber";
            this.col_Lot.HeaderText = "Lot号";
            this.col_Lot.MinimumWidth = 6;
            this.col_Lot.Name = "col_Lot";
            this.col_Lot.ReadOnly = true;
            // 
            // col_MachineId
            // 
            this.col_MachineId.DataPropertyName = "MachineId";
            this.col_MachineId.HeaderText = "机台号";
            this.col_MachineId.MinimumWidth = 6;
            this.col_MachineId.Name = "col_MachineId";
            this.col_MachineId.ReadOnly = true;
            // 
            // col_ProductSerial
            // 
            this.col_ProductSerial.DataPropertyName = "ProductSerial";
            this.col_ProductSerial.HeaderText = "料号";
            this.col_ProductSerial.MinimumWidth = 6;
            this.col_ProductSerial.Name = "col_ProductSerial";
            this.col_ProductSerial.ReadOnly = true;
            // 
            // col_Side
            // 
            this.col_Side.DataPropertyName = "Side";
            this.col_Side.HeaderText = "面次";
            this.col_Side.MinimumWidth = 6;
            this.col_Side.Name = "col_Side";
            this.col_Side.ReadOnly = true;
            // 
            // col_AiStatus
            // 
            this.col_AiStatus.DataPropertyName = "AiStatus";
            this.col_AiStatus.HeaderText = "AI状态";
            this.col_AiStatus.MinimumWidth = 6;
            this.col_AiStatus.Name = "col_AiStatus";
            this.col_AiStatus.ReadOnly = true;
            // 
            // col_ManualStatus
            // 
            this.col_ManualStatus.DataPropertyName = "ManualStatus";
            this.col_ManualStatus.HeaderText = "人工判定";
            this.col_ManualStatus.Items.AddRange(new object[] {
            "未判定",
            "OK",
            "NG"});
            this.col_ManualStatus.MinimumWidth = 6;
            this.col_ManualStatus.Name = "col_ManualStatus";
            this.col_ManualStatus.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.col_ManualStatus.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // col_VrsStatus
            // 
            this.col_VrsStatus.DataPropertyName = "VrsStatus";
            this.col_VrsStatus.HeaderText = "VRS状态";
            this.col_VrsStatus.MinimumWidth = 6;
            this.col_VrsStatus.Name = "col_VrsStatus";
            this.col_VrsStatus.ReadOnly = true;
            // 
            // col_DefectCount
            // 
            this.col_DefectCount.DataPropertyName = "DefectCount";
            this.col_DefectCount.HeaderText = "缺陷数";
            this.col_DefectCount.MinimumWidth = 6;
            this.col_DefectCount.Name = "col_DefectCount";
            this.col_DefectCount.ReadOnly = true;
            // 
            // col_PathIndex
            // 
            this.col_PathIndex.DataPropertyName = "PathIndex";
            this.col_PathIndex.HeaderText = "路径索引";
            this.col_PathIndex.MinimumWidth = 6;
            this.col_PathIndex.Name = "col_PathIndex";
            this.col_PathIndex.ReadOnly = true;
            this.col_PathIndex.Visible = false;
            // 
            // col_DetectionDate
            // 
            this.col_DetectionDate.DataPropertyName = "DetectionDate";
            this.col_DetectionDate.HeaderText = "检测日期";
            this.col_DetectionDate.MinimumWidth = 6;
            this.col_DetectionDate.Name = "col_DetectionDate";
            this.col_DetectionDate.ReadOnly = true;
            // 
            // tableLayoutPanel_GridTop
            // 
            this.tableLayoutPanel_GridTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
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
            this.tableLayoutPanel_GridTop.Location = new System.Drawing.Point(0, 0);
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
            this.txt_SnFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            this.txt_SnFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txt_SnFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_SnFilter.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txt_SnFilter.ForeColor = System.Drawing.Color.White;
            this.txt_SnFilter.Location = new System.Drawing.Point(6, 10);
            this.txt_SnFilter.Margin = new System.Windows.Forms.Padding(2, 6, 2, 6);
            this.txt_SnFilter.Name = "txt_SnFilter";
            this.txt_SnFilter.Size = new System.Drawing.Size(236, 27);
            this.txt_SnFilter.TabIndex = 0;
            // 
            // btn_SnSearch
            // 
            this.btn_SnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btn_SnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_SnSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_SnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_SnSearch.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btn_SnSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btn_SnSearch.Location = new System.Drawing.Point(246, 6);
            this.btn_SnSearch.Margin = new System.Windows.Forms.Padding(2, 2, 4, 2);
            this.btn_SnSearch.Name = "btn_SnSearch";
            this.btn_SnSearch.Size = new System.Drawing.Size(64, 30);
            this.btn_SnSearch.TabIndex = 1;
            this.btn_SnSearch.Text = "搜索";
            this.btn_SnSearch.UseVisualStyleBackColor = false;
            // 
            // btn_ImageDetail
            // 
            this.btn_ImageDetail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(120)))));
            this.btn_ImageDetail.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_ImageDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_ImageDetail.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_ImageDetail.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btn_ImageDetail.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
            this.btn_ImageDetail.Location = new System.Drawing.Point(1318, 6);
            this.btn_ImageDetail.Margin = new System.Windows.Forms.Padding(2);
            this.btn_ImageDetail.Name = "btn_ImageDetail";
            this.btn_ImageDetail.Size = new System.Drawing.Size(106, 30);
            this.btn_ImageDetail.TabIndex = 2;
            this.btn_ImageDetail.Text = "🖼 图片详情";
            this.btn_ImageDetail.UseVisualStyleBackColor = false;
            // 
            // btn_MoreActions
            // 
            this.btn_MoreActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btn_MoreActions.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_MoreActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_MoreActions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_MoreActions.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btn_MoreActions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btn_MoreActions.Location = new System.Drawing.Point(1428, 6);
            this.btn_MoreActions.Margin = new System.Windows.Forms.Padding(2);
            this.btn_MoreActions.Name = "btn_MoreActions";
            this.btn_MoreActions.Size = new System.Drawing.Size(116, 30);
            this.btn_MoreActions.TabIndex = 3;
            this.btn_MoreActions.Text = "更多操作 ▾";
            this.btn_MoreActions.UseVisualStyleBackColor = false;
            // 
            // contextMenu_Actions
            // 
            this.contextMenu_Actions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            this.contextMenu_Actions.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.contextMenu_Actions.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenu_Actions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_AddToDataset,
            this.menuItem_RunTest,
            this.menuItem_SecondaryInference});
            this.contextMenu_Actions.Name = "contextMenu_Actions";
            this.contextMenu_Actions.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.contextMenu_Actions.Size = new System.Drawing.Size(214, 76);
            // 
            // menuItem_AddToDataset
            // 
            this.menuItem_AddToDataset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            this.menuItem_AddToDataset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.menuItem_AddToDataset.Name = "menuItem_AddToDataset";
            this.menuItem_AddToDataset.Size = new System.Drawing.Size(213, 24);
            this.menuItem_AddToDataset.Text = "添加到一致性数据集";
            // 
            // menuItem_RunTest
            // 
            this.menuItem_RunTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            this.menuItem_RunTest.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.menuItem_RunTest.Name = "menuItem_RunTest";
            this.menuItem_RunTest.Size = new System.Drawing.Size(213, 24);
            this.menuItem_RunTest.Text = "模型一致性测试";
            // 
            // menuItem_SecondaryInference
            // 
            this.menuItem_SecondaryInference.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            this.menuItem_SecondaryInference.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.menuItem_SecondaryInference.Name = "menuItem_SecondaryInference";
            this.menuItem_SecondaryInference.Size = new System.Drawing.Size(213, 24);
            this.menuItem_SecondaryInference.Text = "运行二次推理";
            // 
            // UcDefectListPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
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

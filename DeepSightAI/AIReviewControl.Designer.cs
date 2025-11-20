namespace DeepSightAI
{
    partial class AIReviewControl
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
            this.splitContainer_Main = new System.Windows.Forms.SplitContainer();
            this.heatMapQueryControl = new DeepSightAI.QueryControl();
            this.splitContainer_Right = new System.Windows.Forms.SplitContainer();
            this.dataGridView_Defects = new System.Windows.Forms.DataGridView();
            this.col_SerialNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_LotNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Side = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_AviStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_AiStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_ManualStatus = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.col_DefectCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_PathIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel_Buttons = new System.Windows.Forms.Panel();
            this.btn_Export = new System.Windows.Forms.Button();
            this.btn_Save = new System.Windows.Forms.Button();
            this.panel_Details = new System.Windows.Forms.Panel();
            this.tableLayoutPanel_Details = new System.Windows.Forms.TableLayoutPanel();
            this.label_DetailTitle = new System.Windows.Forms.Label();
            this.flowLayoutPanel_DefectImages = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).BeginInit();
            this.splitContainer_Main.Panel1.SuspendLayout();
            this.splitContainer_Main.Panel2.SuspendLayout();
            this.splitContainer_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Right)).BeginInit();
            this.splitContainer_Right.Panel1.SuspendLayout();
            this.splitContainer_Right.Panel2.SuspendLayout();
            this.splitContainer_Right.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Defects)).BeginInit();
            this.panel_Buttons.SuspendLayout();
            this.panel_Details.SuspendLayout();
            this.tableLayoutPanel_Details.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_Main
            // 
            this.splitContainer_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_Main.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer_Main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_Main.Name = "splitContainer_Main";
            // 
            // splitContainer_Main.Panel1
            // 
            this.splitContainer_Main.Panel1.Controls.Add(this.heatMapQueryControl);
            // 
            // splitContainer_Main.Panel2
            // 
            this.splitContainer_Main.Panel2.Controls.Add(this.splitContainer_Right);
            this.splitContainer_Main.Size = new System.Drawing.Size(1200, 800);
            this.splitContainer_Main.SplitterDistance = 350;
            this.splitContainer_Main.TabIndex = 0;
            // 
            // heatMapQueryControl
            // 
            this.heatMapQueryControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.heatMapQueryControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heatMapQueryControl.IsDateChecked = false;
            this.heatMapQueryControl.Location = new System.Drawing.Point(0, 0);
            this.heatMapQueryControl.LotNumber = "";
            this.heatMapQueryControl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.heatMapQueryControl.Name = "heatMapQueryControl";
            this.heatMapQueryControl.PartNumber = "";
            this.heatMapQueryControl.SelectedDate = new System.DateTime(2025, 11, 19, 15, 27, 2, 219);
            this.heatMapQueryControl.SelectedSide = "A";
            this.heatMapQueryControl.Size = new System.Drawing.Size(350, 800);
            this.heatMapQueryControl.TabIndex = 0;
            // 
            // splitContainer_Right
            // 
            this.splitContainer_Right.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_Right.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_Right.Name = "splitContainer_Right";
            this.splitContainer_Right.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_Right.Panel1
            // 
            this.splitContainer_Right.Panel1.Controls.Add(this.dataGridView_Defects);
            this.splitContainer_Right.Panel1.Controls.Add(this.panel_Buttons);
            // 
            // splitContainer_Right.Panel2
            // 
            this.splitContainer_Right.Panel2.Controls.Add(this.panel_Details);
            this.splitContainer_Right.Size = new System.Drawing.Size(846, 800);
            this.splitContainer_Right.SplitterDistance = 400;
            this.splitContainer_Right.TabIndex = 0;
            // 
            // dataGridView_Defects
            // 
            this.dataGridView_Defects.AllowUserToAddRows = false;
            this.dataGridView_Defects.AllowUserToDeleteRows = false;
            this.dataGridView_Defects.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.dataGridView_Defects.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Defects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_SerialNumber,
            this.col_LotNumber,
            this.col_Side,
            this.col_AviStatus,
            this.col_AiStatus,
            this.col_ManualStatus,
            this.col_DefectCount,
            this.col_PathIndex});
            this.dataGridView_Defects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_Defects.Location = new System.Drawing.Point(0, 0);
            this.dataGridView_Defects.Name = "dataGridView_Defects";
            this.dataGridView_Defects.RowHeadersWidth = 51;
            this.dataGridView_Defects.RowTemplate.Height = 27;
            this.dataGridView_Defects.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_Defects.Size = new System.Drawing.Size(846, 360);
            this.dataGridView_Defects.TabIndex = 0;
            // 
            // col_SerialNumber
            // 
            this.col_SerialNumber.DataPropertyName = "SerialNumber";
            this.col_SerialNumber.HeaderText = "序列号";
            this.col_SerialNumber.MinimumWidth = 6;
            this.col_SerialNumber.Name = "col_SerialNumber";
            this.col_SerialNumber.ReadOnly = true;
            this.col_SerialNumber.Width = 120;
            // 
            // col_LotNumber
            // 
            this.col_LotNumber.DataPropertyName = "LotNumber";
            this.col_LotNumber.HeaderText = "Lot号";
            this.col_LotNumber.MinimumWidth = 6;
            this.col_LotNumber.Name = "col_LotNumber";
            this.col_LotNumber.ReadOnly = true;
            // 
            // col_Side
            // 
            this.col_Side.DataPropertyName = "Side";
            this.col_Side.HeaderText = "面次";
            this.col_Side.MinimumWidth = 6;
            this.col_Side.Name = "col_Side";
            this.col_Side.ReadOnly = true;
            this.col_Side.Width = 60;
            // 
            // col_AviStatus
            // 
            this.col_AviStatus.DataPropertyName = "AviStatus";
            this.col_AviStatus.HeaderText = "AVI状态";
            this.col_AviStatus.MinimumWidth = 6;
            this.col_AviStatus.Name = "col_AviStatus";
            this.col_AviStatus.ReadOnly = true;
            this.col_AviStatus.Width = 80;
            // 
            // col_AiStatus
            // 
            this.col_AiStatus.DataPropertyName = "AiStatus";
            this.col_AiStatus.HeaderText = "AI状态";
            this.col_AiStatus.MinimumWidth = 6;
            this.col_AiStatus.Name = "col_AiStatus";
            this.col_AiStatus.ReadOnly = true;
            this.col_AiStatus.Width = 80;
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
            this.col_ManualStatus.Width = 90;
            // 
            // col_DefectCount
            // 
            this.col_DefectCount.DataPropertyName = "DefectCount";
            this.col_DefectCount.HeaderText = "缺陷数";
            this.col_DefectCount.MinimumWidth = 6;
            this.col_DefectCount.Name = "col_DefectCount";
            this.col_DefectCount.ReadOnly = true;
            this.col_DefectCount.Width = 80;
            // 
            // col_PathIndex
            // 
            this.col_PathIndex.DataPropertyName = "PathIndex";
            this.col_PathIndex.HeaderText = "PathIndex";
            this.col_PathIndex.MinimumWidth = 6;
            this.col_PathIndex.Name = "col_PathIndex";
            this.col_PathIndex.ReadOnly = true;
            this.col_PathIndex.Visible = false;
            this.col_PathIndex.Width = 125;
            // 
            // panel_Buttons
            // 
            this.panel_Buttons.Controls.Add(this.btn_Export);
            this.panel_Buttons.Controls.Add(this.btn_Save);
            this.panel_Buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_Buttons.Location = new System.Drawing.Point(0, 360);
            this.panel_Buttons.Name = "panel_Buttons";
            this.panel_Buttons.Size = new System.Drawing.Size(846, 40);
            this.panel_Buttons.TabIndex = 1;
            // 
            // btn_Export
            // 
            this.btn_Export.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Export.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btn_Export.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Export.ForeColor = System.Drawing.Color.White;
            this.btn_Export.Location = new System.Drawing.Point(736, 5);
            this.btn_Export.Name = "btn_Export";
            this.btn_Export.Size = new System.Drawing.Size(100, 30);
            this.btn_Export.TabIndex = 1;
            this.btn_Export.Text = "导出";
            this.btn_Export.UseVisualStyleBackColor = false;
            // 
            // btn_Save
            // 
            this.btn_Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Save.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btn_Save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Save.ForeColor = System.Drawing.Color.White;
            this.btn_Save.Location = new System.Drawing.Point(630, 5);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(100, 30);
            this.btn_Save.TabIndex = 0;
            this.btn_Save.Text = "保存";
            this.btn_Save.UseVisualStyleBackColor = false;
            // 
            // panel_Details
            // 
            this.panel_Details.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.panel_Details.Controls.Add(this.tableLayoutPanel_Details);
            this.panel_Details.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Details.Location = new System.Drawing.Point(0, 0);
            this.panel_Details.Name = "panel_Details";
            this.panel_Details.Size = new System.Drawing.Size(846, 396);
            this.panel_Details.TabIndex = 0;
            // 
            // tableLayoutPanel_Details
            // 
            this.tableLayoutPanel_Details.ColumnCount = 1;
            this.tableLayoutPanel_Details.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_Details.Controls.Add(this.label_DetailTitle, 0, 0);
            this.tableLayoutPanel_Details.Controls.Add(this.flowLayoutPanel_DefectImages, 0, 1);
            this.tableLayoutPanel_Details.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_Details.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_Details.Name = "tableLayoutPanel_Details";
            this.tableLayoutPanel_Details.RowCount = 2;
            this.tableLayoutPanel_Details.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel_Details.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_Details.Size = new System.Drawing.Size(846, 396);
            this.tableLayoutPanel_Details.TabIndex = 0;
            // 
            // label_DetailTitle
            // 
            this.label_DetailTitle.AutoSize = true;
            this.label_DetailTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_DetailTitle.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_DetailTitle.ForeColor = System.Drawing.Color.White;
            this.label_DetailTitle.Location = new System.Drawing.Point(3, 0);
            this.label_DetailTitle.Name = "label_DetailTitle";
            this.label_DetailTitle.Size = new System.Drawing.Size(840, 40);
            this.label_DetailTitle.TabIndex = 0;
            this.label_DetailTitle.Text = "缺陷详情";
            this.label_DetailTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanel_DefectImages
            // 
            this.flowLayoutPanel_DefectImages.AutoScroll = true;
            this.flowLayoutPanel_DefectImages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.flowLayoutPanel_DefectImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel_DefectImages.Location = new System.Drawing.Point(3, 43);
            this.flowLayoutPanel_DefectImages.Name = "flowLayoutPanel_DefectImages";
            this.flowLayoutPanel_DefectImages.Size = new System.Drawing.Size(840, 350);
            this.flowLayoutPanel_DefectImages.TabIndex = 1;
            // 
            // AIReviewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.Controls.Add(this.splitContainer_Main);
            this.Name = "AIReviewControl";
            this.Size = new System.Drawing.Size(1200, 800);
            this.splitContainer_Main.Panel1.ResumeLayout(false);
            this.splitContainer_Main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).EndInit();
            this.splitContainer_Main.ResumeLayout(false);
            this.splitContainer_Right.Panel1.ResumeLayout(false);
            this.splitContainer_Right.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Right)).EndInit();
            this.splitContainer_Right.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Defects)).EndInit();
            this.panel_Buttons.ResumeLayout(false);
            this.panel_Details.ResumeLayout(false);
            this.tableLayoutPanel_Details.ResumeLayout(false);
            this.tableLayoutPanel_Details.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_Main;
        private QueryControl heatMapQueryControl;
        private System.Windows.Forms.SplitContainer splitContainer_Right;
        private System.Windows.Forms.DataGridView dataGridView_Defects;
        private System.Windows.Forms.Panel panel_Details;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Details;
        private System.Windows.Forms.Label label_DetailTitle;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_DefectImages;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_SerialNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_LotNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Side;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_AviStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_AiStatus;
        private System.Windows.Forms.DataGridViewComboBoxColumn col_ManualStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_DefectCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_PathIndex;
        private System.Windows.Forms.Panel panel_Buttons;
        private System.Windows.Forms.Button btn_Export;
        private System.Windows.Forms.Button btn_Save;
    }
}

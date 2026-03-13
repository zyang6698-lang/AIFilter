using System;

namespace DeepSightAI
{
    partial class HeatMapControl2
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
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel_Details = new System.Windows.Forms.FlowLayoutPanel();
            this.table_HeatMap = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_loadArryImage = new DeepSightAI.StyledButton();
            this.btn_Select = new DeepSightAI.StyledButton();
            this.btnClip = new DeepSightAI.StyledButton();
            this.queryControl = new DeepSightAI.QueryControl();
            this.flowLayoutPanel_Defects = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tableLayoutPanel6.ColumnCount = 3;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 309F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 213F));
            this.tableLayoutPanel6.Controls.Add(this.flowLayoutPanel_Details, 2, 0);
            this.tableLayoutPanel6.Controls.Add(this.table_HeatMap, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(1310, 724);
            this.tableLayoutPanel6.TabIndex = 1;
            // 
            // flowLayoutPanel_Details
            // 
            this.flowLayoutPanel_Details.AutoScroll = true;
            this.flowLayoutPanel_Details.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.flowLayoutPanel_Details.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel_Details.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel_Details.Location = new System.Drawing.Point(1145, 2);
            this.flowLayoutPanel_Details.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.flowLayoutPanel_Details.Name = "flowLayoutPanel_Details";
            this.flowLayoutPanel_Details.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.flowLayoutPanel_Details.Size = new System.Drawing.Size(209, 720);
            this.flowLayoutPanel_Details.TabIndex = 141;
            this.flowLayoutPanel_Details.WrapContents = false;
            // 
            // table_HeatMap
            // 
            this.table_HeatMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.table_HeatMap.ColumnCount = 1;
            this.table_HeatMap.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.table_HeatMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.table_HeatMap.Location = new System.Drawing.Point(311, 2);
            this.table_HeatMap.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.table_HeatMap.Name = "table_HeatMap";
            this.table_HeatMap.RowCount = 1;
            this.table_HeatMap.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.table_HeatMap.Size = new System.Drawing.Size(830, 720);
            this.table_HeatMap.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tableLayoutPanel1.Controls.Add(this.btn_loadArryImage, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btn_Select, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.queryControl, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel_Defects, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnClip, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 252F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(305, 720);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // btn_loadArryImage
            // 
            this.btn_loadArryImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_loadArryImage.Location = new System.Drawing.Point(2, 2);
            this.btn_loadArryImage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btn_loadArryImage.Name = "btn_loadArryImage";
            this.btn_loadArryImage.Size = new System.Drawing.Size(97, 28);
            this.btn_loadArryImage.TabIndex = 120;
            this.btn_loadArryImage.Text = "加载Array图像";
            this.btn_loadArryImage.Click += new System.EventHandler(this.btn_loadArryImage_Click);
            // 
            // btn_Select
            // 
            this.btn_Select.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Select.Location = new System.Drawing.Point(204, 2);
            this.btn_Select.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btn_Select.Name = "btn_Select";
            this.btn_Select.Size = new System.Drawing.Size(99, 28);
            this.btn_Select.TabIndex = 139;
            this.btn_Select.Text = "选择";
            this.btn_Select.Click += new System.EventHandler(this.btn_Select_Click);
            // 
            // btnClip
            // 
            this.btnClip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClip.Location = new System.Drawing.Point(103, 2);
            this.btnClip.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnClip.Name = "btnClip";
            this.btnClip.Size = new System.Drawing.Size(97, 28);
            this.btnClip.TabIndex = 140;
            this.btnClip.Text = "裁剪";
            this.btnClip.Click += new System.EventHandler(this.btnClip_Click);
            // 
            // queryControl
            // 
            this.queryControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tableLayoutPanel1.SetColumnSpan(this.queryControl, 3);
            this.queryControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.queryControl.IsDateChecked = true;
            this.queryControl.Location = new System.Drawing.Point(2, 34);
            this.queryControl.LotNumber = "";
            this.queryControl.MachineID = "";
            this.queryControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.queryControl.Name = "queryControl";
            this.queryControl.PartNumber = "";
            this.queryControl.SelectedDefectName = "";
            this.queryControl.SelectedSide = "";
            this.queryControl.Size = new System.Drawing.Size(301, 248);
            this.queryControl.TabIndex = 1;
            // 
            // flowLayoutPanel_Defects
            // 
            this.flowLayoutPanel_Defects.AutoScroll = true;
            this.flowLayoutPanel_Defects.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel_Defects, 3);
            this.flowLayoutPanel_Defects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel_Defects.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel_Defects.Location = new System.Drawing.Point(2, 286);
            this.flowLayoutPanel_Defects.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.flowLayoutPanel_Defects.Name = "flowLayoutPanel_Defects";
            this.flowLayoutPanel_Defects.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.flowLayoutPanel_Defects.Size = new System.Drawing.Size(301, 432);
            this.flowLayoutPanel_Defects.TabIndex = 140;
            this.flowLayoutPanel_Defects.WrapContents = false;
            // 
            // HeatMapControl2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel6);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "HeatMapControl2";
            this.Size = new System.Drawing.Size(1310, 724);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void btn_setPanel_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.TableLayoutPanel table_HeatMap;
        private DeepSightAI.StyledButton btn_loadArryImage;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_Defects;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DeepSightAI.StyledButton btn_Select;
        private DeepSightAI.StyledButton btnClip;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_Details;
        private QueryControl queryControl;
    }
}

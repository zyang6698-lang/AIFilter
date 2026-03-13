namespace DeepSightAI
{
    partial class QueryControl
    {
        /// <summary> 
        /// ����������������
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// ������������ʹ�õ���Դ��
        /// </summary>
        /// <param name="disposing">���Ӧ�ͷ��й���Դ��Ϊ true������Ϊ false��</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region �����������ɵĴ���

        /// <summary> 
        /// �����֧������ķ��� - ��Ҫ�޸�
        /// ʹ�ô���༭���޸Ĵ˷��������ݡ�
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel_Main = new System.Windows.Forms.TableLayoutPanel();
            this.label79 = new System.Windows.Forms.Label();
            this.label82 = new System.Windows.Forms.Label();
            this.timePicker = new System.Windows.Forms.DateTimePicker();
            this.labelEndDate = new System.Windows.Forms.Label();
            this.timePickerEnd = new System.Windows.Forms.DateTimePicker();
            this.label81 = new System.Windows.Forms.Label();
            this.cmb_PartNumber = new System.Windows.Forms.ComboBox();
            this.label80 = new System.Windows.Forms.Label();
            this.txt_Lot = new System.Windows.Forms.TextBox();
            this.label_MachineID = new System.Windows.Forms.Label();
            this.cmb_MachineID = new System.Windows.Forms.ComboBox();
            this.label83 = new System.Windows.Forms.Label();
            this.flowLayoutPanel_Side = new System.Windows.Forms.FlowLayoutPanel();
            this.rbn_All = new System.Windows.Forms.RadioButton();
            this.rbn_Front = new System.Windows.Forms.RadioButton();
            this.rbn_Back = new System.Windows.Forms.RadioButton();
            this.label_DefectName = new System.Windows.Forms.Label();
            this.cmb_DefectName = new System.Windows.Forms.ComboBox();
            this.btn_queryHeatPoint = new DeepSightAI.StyledButton();
            this.tableLayoutPanel_Main.SuspendLayout();
            this.flowLayoutPanel_Side.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_Main
            // 
            this.tableLayoutPanel_Main.ColumnCount = 2;
            this.tableLayoutPanel_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_Main.Controls.Add(this.label79, 0, 0);
            this.tableLayoutPanel_Main.Controls.Add(this.label82, 0, 1);
            this.tableLayoutPanel_Main.Controls.Add(this.timePicker, 1, 1);
            this.tableLayoutPanel_Main.Controls.Add(this.labelEndDate, 0, 2);
            this.tableLayoutPanel_Main.Controls.Add(this.timePickerEnd, 1, 2);
            this.tableLayoutPanel_Main.Controls.Add(this.label81, 0, 3);
            this.tableLayoutPanel_Main.Controls.Add(this.cmb_PartNumber, 1, 3);
            this.tableLayoutPanel_Main.Controls.Add(this.label80, 0, 4);
            this.tableLayoutPanel_Main.Controls.Add(this.txt_Lot, 1, 4);
            this.tableLayoutPanel_Main.Controls.Add(this.label_MachineID, 0, 5);
            this.tableLayoutPanel_Main.Controls.Add(this.cmb_MachineID, 1, 5);
            this.tableLayoutPanel_Main.Controls.Add(this.label83, 0, 6);
            this.tableLayoutPanel_Main.Controls.Add(this.flowLayoutPanel_Side, 1, 6);
            this.tableLayoutPanel_Main.Controls.Add(this.label_DefectName, 0, 7);
            this.tableLayoutPanel_Main.Controls.Add(this.cmb_DefectName, 1, 7);
            this.tableLayoutPanel_Main.Controls.Add(this.btn_queryHeatPoint, 1, 8);
            this.tableLayoutPanel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_Main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_Main.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel_Main.Name = "tableLayoutPanel_Main";
            this.tableLayoutPanel_Main.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.tableLayoutPanel_Main.RowCount = 9;
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_Main.Size = new System.Drawing.Size(262, 252);
            this.tableLayoutPanel_Main.TabIndex = 0;
            // 
            // label79
            // 
            this.label79.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label79.AutoSize = true;
            this.tableLayoutPanel_Main.SetColumnSpan(this.label79, 2);
            this.label79.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label79.Location = new System.Drawing.Point(8, 12);
            this.label79.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.label79.Name = "label79";
            this.label79.Size = new System.Drawing.Size(65, 12);
            this.label79.TabIndex = 0;
            this.label79.Text = "查询信息：";
            // 
            // label82
            // 
            this.label82.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label82.AutoSize = true;
            this.label82.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label82.Location = new System.Drawing.Point(8, 36);
            this.label82.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.label82.Name = "label82";
            this.label82.Size = new System.Drawing.Size(65, 12);
            this.label82.TabIndex = 1;
            this.label82.Text = "起始日期：";
            // 
            // timePicker
            // 
            this.timePicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timePicker.Location = new System.Drawing.Point(77, 33);
            this.timePicker.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.timePicker.Name = "timePicker";
            this.timePicker.Size = new System.Drawing.Size(177, 21);
            this.timePicker.TabIndex = 2;
            // 
            // labelEndDate
            // 
            this.labelEndDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelEndDate.AutoSize = true;
            this.labelEndDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.labelEndDate.Location = new System.Drawing.Point(8, 60);
            this.labelEndDate.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.labelEndDate.Name = "labelEndDate";
            this.labelEndDate.Size = new System.Drawing.Size(65, 12);
            this.labelEndDate.TabIndex = 12;
            this.labelEndDate.Text = "结束日期：";
            // 
            // timePickerEnd
            // 
            this.timePickerEnd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timePickerEnd.Location = new System.Drawing.Point(77, 57);
            this.timePickerEnd.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.timePickerEnd.Name = "timePickerEnd";
            this.timePickerEnd.Size = new System.Drawing.Size(177, 21);
            this.timePickerEnd.TabIndex = 13;
            // 
            // label81
            // 
            this.label81.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label81.AutoSize = true;
            this.label81.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label81.Location = new System.Drawing.Point(8, 84);
            this.label81.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.label81.Name = "label81";
            this.label81.Size = new System.Drawing.Size(41, 12);
            this.label81.TabIndex = 3;
            this.label81.Text = "料号：";
            // 
            // cmb_PartNumber
            // 
            this.cmb_PartNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_PartNumber.FormattingEnabled = true;
            this.cmb_PartNumber.Location = new System.Drawing.Point(77, 81);
            this.cmb_PartNumber.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cmb_PartNumber.Name = "cmb_PartNumber";
            this.cmb_PartNumber.Size = new System.Drawing.Size(177, 20);
            this.cmb_PartNumber.TabIndex = 4;
            // 
            // label80
            // 
            this.label80.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label80.AutoSize = true;
            this.label80.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label80.Location = new System.Drawing.Point(8, 108);
            this.label80.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.label80.Name = "label80";
            this.label80.Size = new System.Drawing.Size(35, 12);
            this.label80.TabIndex = 6;
            this.label80.Text = "Lot：";
            // 
            // txt_Lot
            // 
            this.txt_Lot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_Lot.Location = new System.Drawing.Point(77, 105);
            this.txt_Lot.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txt_Lot.Name = "txt_Lot";
            this.txt_Lot.Size = new System.Drawing.Size(177, 21);
            this.txt_Lot.TabIndex = 7;
            // 
            // label_MachineID
            // 
            this.label_MachineID.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label_MachineID.AutoSize = true;
            this.label_MachineID.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label_MachineID.Location = new System.Drawing.Point(8, 132);
            this.label_MachineID.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.label_MachineID.Name = "label_MachineID";
            this.label_MachineID.Size = new System.Drawing.Size(53, 12);
            this.label_MachineID.TabIndex = 14;
            this.label_MachineID.Text = "机台号：";
            // 
            // cmb_MachineID
            // 
            this.cmb_MachineID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_MachineID.FormattingEnabled = true;
            this.cmb_MachineID.Location = new System.Drawing.Point(77, 129);
            this.cmb_MachineID.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cmb_MachineID.Name = "cmb_MachineID";
            this.cmb_MachineID.Size = new System.Drawing.Size(177, 20);
            this.cmb_MachineID.TabIndex = 15;
            // 
            // label83
            // 
            this.label83.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label83.AutoSize = true;
            this.label83.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label83.Location = new System.Drawing.Point(8, 156);
            this.label83.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.label83.Name = "label83";
            this.label83.Size = new System.Drawing.Size(53, 12);
            this.label83.TabIndex = 8;
            this.label83.Text = "正反面：";
            // 
            // flowLayoutPanel_Side
            // 
            this.flowLayoutPanel_Side.AutoSize = true;
            this.flowLayoutPanel_Side.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel_Side.Controls.Add(this.rbn_All);
            this.flowLayoutPanel_Side.Controls.Add(this.rbn_Front);
            this.flowLayoutPanel_Side.Controls.Add(this.rbn_Back);
            this.flowLayoutPanel_Side.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel_Side.Location = new System.Drawing.Point(75, 150);
            this.flowLayoutPanel_Side.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel_Side.Name = "flowLayoutPanel_Side";
            this.flowLayoutPanel_Side.Size = new System.Drawing.Size(181, 24);
            this.flowLayoutPanel_Side.TabIndex = 0;
            // 
            // rbn_All
            // 
            this.rbn_All.AutoSize = true;
            this.rbn_All.Checked = true;
            this.rbn_All.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.rbn_All.Location = new System.Drawing.Point(2, 2);
            this.rbn_All.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbn_All.Name = "rbn_All";
            this.rbn_All.Size = new System.Drawing.Size(47, 16);
            this.rbn_All.TabIndex = 9;
            this.rbn_All.TabStop = true;
            this.rbn_All.Text = "全选";
            this.rbn_All.UseVisualStyleBackColor = true;
            // 
            // rbn_Front
            // 
            this.rbn_Front.AutoSize = true;
            this.rbn_Front.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.rbn_Front.Location = new System.Drawing.Point(53, 2);
            this.rbn_Front.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbn_Front.Name = "rbn_Front";
            this.rbn_Front.Size = new System.Drawing.Size(47, 16);
            this.rbn_Front.TabIndex = 10;
            this.rbn_Front.Text = "正面";
            this.rbn_Front.UseVisualStyleBackColor = true;
            // 
            // rbn_Back
            // 
            this.rbn_Back.AutoSize = true;
            this.rbn_Back.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.rbn_Back.Location = new System.Drawing.Point(104, 2);
            this.rbn_Back.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbn_Back.Name = "rbn_Back";
            this.rbn_Back.Size = new System.Drawing.Size(47, 16);
            this.rbn_Back.TabIndex = 13;
            this.rbn_Back.Text = "反面";
            this.rbn_Back.UseVisualStyleBackColor = true;
            // 
            // label_DefectName
            // 
            this.label_DefectName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label_DefectName.AutoSize = true;
            this.label_DefectName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label_DefectName.Location = new System.Drawing.Point(8, 180);
            this.label_DefectName.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.label_DefectName.Name = "label_DefectName";
            this.label_DefectName.Size = new System.Drawing.Size(53, 12);
            this.label_DefectName.TabIndex = 16;
            this.label_DefectName.Text = "缺陷名：";
            // 
            // cmb_DefectName
            // 
            this.cmb_DefectName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_DefectName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_DefectName.FormattingEnabled = true;
            this.cmb_DefectName.Location = new System.Drawing.Point(77, 177);
            this.cmb_DefectName.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cmb_DefectName.Name = "cmb_DefectName";
            this.cmb_DefectName.Size = new System.Drawing.Size(177, 20);
            this.cmb_DefectName.TabIndex = 17;
            // 
            // btn_queryHeatPoint
            // 
            this.btn_queryHeatPoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_queryHeatPoint.Location = new System.Drawing.Point(148, 203);
            this.btn_queryHeatPoint.Margin = new System.Windows.Forms.Padding(2, 5, 2, 2);
            this.btn_queryHeatPoint.Name = "btn_queryHeatPoint";
            this.btn_queryHeatPoint.Size = new System.Drawing.Size(106, 26);
            this.btn_queryHeatPoint.TabIndex = 11;
            this.btn_queryHeatPoint.Text = "查询";
            this.btn_queryHeatPoint.Click += new System.EventHandler(this.Btn_queryHeatPoint_Click);
            // 
            // QueryControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.Controls.Add(this.tableLayoutPanel_Main);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "QueryControl";
            this.Size = new System.Drawing.Size(262, 252);
            this.tableLayoutPanel_Main.ResumeLayout(false);
            this.tableLayoutPanel_Main.PerformLayout();
            this.flowLayoutPanel_Side.ResumeLayout(false);
            this.flowLayoutPanel_Side.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Main;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_Side;
        private System.Windows.Forms.Label label79;
        private System.Windows.Forms.Label label82;
        private System.Windows.Forms.DateTimePicker timePicker;
        private System.Windows.Forms.Label labelEndDate;
        private System.Windows.Forms.DateTimePicker timePickerEnd;
        private System.Windows.Forms.Label label81;
        private System.Windows.Forms.ComboBox cmb_PartNumber;
        private System.Windows.Forms.Label label80;
        private System.Windows.Forms.TextBox txt_Lot;
        private System.Windows.Forms.Label label83;
        private System.Windows.Forms.RadioButton rbn_Front;
        private System.Windows.Forms.RadioButton rbn_Back;
        private System.Windows.Forms.RadioButton rbn_All;
        private System.Windows.Forms.Label label_MachineID;
        private System.Windows.Forms.ComboBox cmb_MachineID;
        private System.Windows.Forms.Label label_DefectName;
        private System.Windows.Forms.ComboBox cmb_DefectName;
        private DeepSightAI.StyledButton btn_queryHeatPoint;
    }
}

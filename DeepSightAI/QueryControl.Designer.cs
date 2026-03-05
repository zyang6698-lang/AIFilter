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
            this.label79 = new System.Windows.Forms.Label();
            this.label82 = new System.Windows.Forms.Label();
            this.timePicker = new System.Windows.Forms.DateTimePicker();
            this.labelEndDate = new System.Windows.Forms.Label();
            this.timePickerEnd = new System.Windows.Forms.DateTimePicker();
            this.label81 = new System.Windows.Forms.Label();
            this.cmb_PartNumber = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label80 = new System.Windows.Forms.Label();
            this.txt_Lot = new System.Windows.Forms.TextBox();
            this.label83 = new System.Windows.Forms.Label();
            this.rbn_Front = new System.Windows.Forms.RadioButton();
            this.rbn_Back = new System.Windows.Forms.RadioButton();
            this.rbn_All = new System.Windows.Forms.RadioButton();
            this.label_MachineID = new System.Windows.Forms.Label();
            this.cmb_MachineID = new System.Windows.Forms.ComboBox();
            this.label_DefectName = new System.Windows.Forms.Label();
            this.cmb_DefectName = new System.Windows.Forms.ComboBox();
            this.btn_queryHeatPoint = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label79
            // 
            this.label79.AutoSize = true;
            this.label79.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label79.Location = new System.Drawing.Point(15, 15);
            this.label79.Name = "label79";
            this.label79.Size = new System.Drawing.Size(82, 15);
            this.label79.TabIndex = 0;
            this.label79.Text = "查询信息：";
            // 
            // label82
            // 
            this.label82.AutoSize = true;
            this.label82.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label82.Location = new System.Drawing.Point(11, 46);
            this.label82.Name = "label82";
            this.label82.Size = new System.Drawing.Size(82, 15);
            this.label82.TabIndex = 1;
            this.label82.Text = "起始日期：";
            // 
            // timePicker
            // 
            this.timePicker.Location = new System.Drawing.Point(89, 41);
            this.timePicker.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.timePicker.Name = "timePicker";
            this.timePicker.Size = new System.Drawing.Size(241, 25);
            this.timePicker.TabIndex = 2;
            // 
            // labelEndDate
            // 
            this.labelEndDate.AutoSize = true;
            this.labelEndDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.labelEndDate.Location = new System.Drawing.Point(11, 88);
            this.labelEndDate.Name = "labelEndDate";
            this.labelEndDate.Size = new System.Drawing.Size(82, 15);
            this.labelEndDate.TabIndex = 12;
            this.labelEndDate.Text = "结束日期：";
            // 
            // timePickerEnd
            // 
            this.timePickerEnd.Location = new System.Drawing.Point(89, 82);
            this.timePickerEnd.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.timePickerEnd.Name = "timePickerEnd";
            this.timePickerEnd.Size = new System.Drawing.Size(241, 25);
            this.timePickerEnd.TabIndex = 13;
            // 
            // label81
            // 
            this.label81.AutoSize = true;
            this.label81.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label81.Location = new System.Drawing.Point(11, 128);
            this.label81.Name = "label81";
            this.label81.Size = new System.Drawing.Size(52, 15);
            this.label81.TabIndex = 3;
            this.label81.Text = "料号：";
            //
            // cmb_PartNumber
            //
            this.cmb_PartNumber.FormattingEnabled = true;
            this.cmb_PartNumber.Location = new System.Drawing.Point(89, 124);
            this.cmb_PartNumber.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmb_PartNumber.Name = "cmb_PartNumber";
            this.cmb_PartNumber.Size = new System.Drawing.Size(241, 23);
            this.cmb_PartNumber.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Snow;
            this.label2.Location = new System.Drawing.Point(15, 158);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(319, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "---------------------------------------";
            // 
            // label80
            // 
            this.label80.AutoSize = true;
            this.label80.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label80.Location = new System.Drawing.Point(15, 192);
            this.label80.Name = "label80";
            this.label80.Size = new System.Drawing.Size(46, 15);
            this.label80.TabIndex = 6;
            this.label80.Text = "Lot：";
            // 
            // txt_Lot
            // 
            this.txt_Lot.Location = new System.Drawing.Point(89, 189);
            this.txt_Lot.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txt_Lot.Name = "txt_Lot";
            this.txt_Lot.Size = new System.Drawing.Size(241, 25);
            this.txt_Lot.TabIndex = 7;
            // 
            // label83
            // 
            this.label83.AutoSize = true;
            this.label83.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label83.Location = new System.Drawing.Point(11, 269);
            this.label83.Name = "label83";
            this.label83.Size = new System.Drawing.Size(67, 15);
            this.label83.TabIndex = 8;
            this.label83.Text = "正反面：";
            // 
            // rbn_Front
            // 
            this.rbn_Front.AutoSize = true;
            this.rbn_Front.Checked = true;
            this.rbn_Front.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.rbn_Front.Location = new System.Drawing.Point(87, 269);
            this.rbn_Front.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rbn_Front.Name = "rbn_Front";
            this.rbn_Front.Size = new System.Drawing.Size(58, 19);
            this.rbn_Front.TabIndex = 9;
            this.rbn_Front.TabStop = true;
            this.rbn_Front.Text = "正面";
            this.rbn_Front.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rbn_Front.UseVisualStyleBackColor = true;
            // 
            // rbn_Back
            // 
            this.rbn_Back.AutoSize = true;
            this.rbn_Back.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.rbn_Back.Location = new System.Drawing.Point(155, 269);
            this.rbn_Back.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rbn_Back.Name = "rbn_Back";
            this.rbn_Back.Size = new System.Drawing.Size(58, 19);
            this.rbn_Back.TabIndex = 10;
            this.rbn_Back.Text = "反面";
            this.rbn_Back.UseVisualStyleBackColor = true;
            // 
            // rbn_All
            // 
            this.rbn_All.AutoSize = true;
            this.rbn_All.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.rbn_All.Location = new System.Drawing.Point(223, 269);
            this.rbn_All.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rbn_All.Name = "rbn_All";
            this.rbn_All.Size = new System.Drawing.Size(58, 19);
            this.rbn_All.TabIndex = 13;
            this.rbn_All.Text = "全选";
            this.rbn_All.UseVisualStyleBackColor = true;
            // 
            // label_MachineID
            // 
            this.label_MachineID.AutoSize = true;
            this.label_MachineID.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label_MachineID.Location = new System.Drawing.Point(11, 235);
            this.label_MachineID.Name = "label_MachineID";
            this.label_MachineID.Size = new System.Drawing.Size(67, 15);
            this.label_MachineID.TabIndex = 14;
            this.label_MachineID.Text = "机台号：";
            // 
            // cmb_MachineID
            // 
            this.cmb_MachineID.FormattingEnabled = true;
            this.cmb_MachineID.Location = new System.Drawing.Point(89, 231);
            this.cmb_MachineID.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmb_MachineID.Name = "cmb_MachineID";
            this.cmb_MachineID.Size = new System.Drawing.Size(241, 23);
            this.cmb_MachineID.TabIndex = 15;
            //
            // label_DefectName
            //
            this.label_DefectName.AutoSize = true;
            this.label_DefectName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label_DefectName.Location = new System.Drawing.Point(11, 306);
            this.label_DefectName.Name = "label_DefectName";
            this.label_DefectName.Size = new System.Drawing.Size(67, 15);
            this.label_DefectName.TabIndex = 16;
            this.label_DefectName.Text = "缺陷名：";
            //
            // cmb_DefectName
            //
            this.cmb_DefectName.FormattingEnabled = true;
            this.cmb_DefectName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_DefectName.Location = new System.Drawing.Point(89, 302);
            this.cmb_DefectName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmb_DefectName.Name = "cmb_DefectName";
            this.cmb_DefectName.Size = new System.Drawing.Size(241, 23);
            this.cmb_DefectName.TabIndex = 17;
            //
            // btn_queryHeatPoint
            //
            this.btn_queryHeatPoint.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btn_queryHeatPoint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btn_queryHeatPoint.Location = new System.Drawing.Point(189, 342);
            this.btn_queryHeatPoint.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_queryHeatPoint.Name = "btn_queryHeatPoint";
            this.btn_queryHeatPoint.Size = new System.Drawing.Size(141, 32);
            this.btn_queryHeatPoint.TabIndex = 11;
            this.btn_queryHeatPoint.Text = "查询";
            this.btn_queryHeatPoint.UseVisualStyleBackColor = false;
            this.btn_queryHeatPoint.Click += new System.EventHandler(this.Btn_queryHeatPoint_Click);
            // 
            // QueryControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.Controls.Add(this.cmb_DefectName);
            this.Controls.Add(this.label_DefectName);
            this.Controls.Add(this.cmb_MachineID);
            this.Controls.Add(this.label_MachineID);
            this.Controls.Add(this.timePickerEnd);
            this.Controls.Add(this.labelEndDate);
            this.Controls.Add(this.btn_queryHeatPoint);
            this.Controls.Add(this.rbn_All);
            this.Controls.Add(this.rbn_Back);
            this.Controls.Add(this.rbn_Front);
            this.Controls.Add(this.label83);
            this.Controls.Add(this.txt_Lot);
            this.Controls.Add(this.label80);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmb_PartNumber);
            this.Controls.Add(this.label81);
            this.Controls.Add(this.timePicker);
            this.Controls.Add(this.label82);
            this.Controls.Add(this.label79);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "QueryControl";
            this.Size = new System.Drawing.Size(349, 392);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label79;
        private System.Windows.Forms.Label label82;
        private System.Windows.Forms.DateTimePicker timePicker;
        private System.Windows.Forms.Label labelEndDate;
        private System.Windows.Forms.DateTimePicker timePickerEnd;
        private System.Windows.Forms.Label label81;
        private System.Windows.Forms.ComboBox cmb_PartNumber;
        private System.Windows.Forms.Label label2;
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
        private System.Windows.Forms.Button btn_queryHeatPoint;
    }
}

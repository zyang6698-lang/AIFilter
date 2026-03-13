
namespace DeepSightAI.SettingPages
{
    partial class FrUserManagement
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btn_Select = new DeepSightAI.StyledButton();
            this.chk_AlarmCtrl = new System.Windows.Forms.CheckBox();
            this.chk_DefectReportCtrl = new System.Windows.Forms.CheckBox();
            this.chk_ProSet = new System.Windows.Forms.CheckBox();
            this.chk_AISet = new System.Windows.Forms.CheckBox();
            this.chk_FunctionSet = new System.Windows.Forms.CheckBox();
            this.chk_GeneralSet = new System.Windows.Forms.CheckBox();
            this.chk_UserManage = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_Pwd = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_User = new System.Windows.Forms.TextBox();
            this.dgv_User = new System.Windows.Forms.DataGridView();
            this.LoginName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_CurrentUser = new System.Windows.Forms.TextBox();
            this.btn_Add = new DeepSightAI.StyledButton();
            this.btn_Save = new DeepSightAI.StyledButton();
            this.btn_Delete = new DeepSightAI.StyledButton();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_User)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btn_Select);
            this.groupBox2.Controls.Add(this.chk_AlarmCtrl);
            this.groupBox2.Controls.Add(this.chk_DefectReportCtrl);
            this.groupBox2.Controls.Add(this.chk_ProSet);
            this.groupBox2.Controls.Add(this.chk_AISet);
            this.groupBox2.Controls.Add(this.chk_FunctionSet);
            this.groupBox2.Controls.Add(this.chk_GeneralSet);
            this.groupBox2.Controls.Add(this.chk_UserManage);
            this.groupBox2.Font = new System.Drawing.Font("黑体", 11F);
            this.groupBox2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.groupBox2.Location = new System.Drawing.Point(322, 146);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(474, 282);
            this.groupBox2.TabIndex = 47;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "用户权限";
            // 
            // btn_Select
            // 
            this.btn_Select.Location = new System.Drawing.Point(300, 222);
            this.btn_Select.Name = "btn_Select";
            this.btn_Select.Size = new System.Drawing.Size(138, 44);
            this.btn_Select.TabIndex = 117;
            this.btn_Select.Text = "全选";
            this.btn_Select.Click += new System.EventHandler(this.btn_Select_Click);
            // 
            // chk_AlarmCtrl
            // 
            this.chk_AlarmCtrl.AutoSize = true;
            this.chk_AlarmCtrl.Font = new System.Drawing.Font("黑体", 11F);
            this.chk_AlarmCtrl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.chk_AlarmCtrl.Location = new System.Drawing.Point(87, 165);
            this.chk_AlarmCtrl.Name = "chk_AlarmCtrl";
            this.chk_AlarmCtrl.Size = new System.Drawing.Size(124, 26);
            this.chk_AlarmCtrl.TabIndex = 0;
            this.chk_AlarmCtrl.Text = "运动控制";
            this.chk_AlarmCtrl.UseVisualStyleBackColor = true;
            // 
            // chk_DefectReportCtrl
            // 
            this.chk_DefectReportCtrl.AutoSize = true;
            this.chk_DefectReportCtrl.Font = new System.Drawing.Font("黑体", 11F);
            this.chk_DefectReportCtrl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.chk_DefectReportCtrl.Location = new System.Drawing.Point(300, 102);
            this.chk_DefectReportCtrl.Name = "chk_DefectReportCtrl";
            this.chk_DefectReportCtrl.Size = new System.Drawing.Size(124, 26);
            this.chk_DefectReportCtrl.TabIndex = 0;
            this.chk_DefectReportCtrl.Text = "相机配置";
            this.chk_DefectReportCtrl.UseVisualStyleBackColor = true;
            // 
            // chk_ProSet
            // 
            this.chk_ProSet.AutoSize = true;
            this.chk_ProSet.Font = new System.Drawing.Font("黑体", 11F);
            this.chk_ProSet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.chk_ProSet.Location = new System.Drawing.Point(87, 102);
            this.chk_ProSet.Name = "chk_ProSet";
            this.chk_ProSet.Size = new System.Drawing.Size(124, 26);
            this.chk_ProSet.TabIndex = 0;
            this.chk_ProSet.Text = "硬件设置";
            this.chk_ProSet.UseVisualStyleBackColor = true;
            // 
            // chk_AISet
            // 
            this.chk_AISet.AutoSize = true;
            this.chk_AISet.Font = new System.Drawing.Font("黑体", 11F);
            this.chk_AISet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.chk_AISet.Location = new System.Drawing.Point(300, 42);
            this.chk_AISet.Name = "chk_AISet";
            this.chk_AISet.Size = new System.Drawing.Size(124, 26);
            this.chk_AISet.TabIndex = 0;
            this.chk_AISet.Text = "算法配置";
            this.chk_AISet.UseVisualStyleBackColor = true;
            // 
            // chk_FunctionSet
            // 
            this.chk_FunctionSet.AutoSize = true;
            this.chk_FunctionSet.Font = new System.Drawing.Font("黑体", 11F);
            this.chk_FunctionSet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.chk_FunctionSet.Location = new System.Drawing.Point(300, 165);
            this.chk_FunctionSet.Name = "chk_FunctionSet";
            this.chk_FunctionSet.Size = new System.Drawing.Size(102, 26);
            this.chk_FunctionSet.TabIndex = 0;
            this.chk_FunctionSet.Text = "IO监控";
            this.chk_FunctionSet.UseVisualStyleBackColor = true;
            // 
            // chk_GeneralSet
            // 
            this.chk_GeneralSet.AutoSize = true;
            this.chk_GeneralSet.Font = new System.Drawing.Font("黑体", 11F);
            this.chk_GeneralSet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.chk_GeneralSet.Location = new System.Drawing.Point(87, 42);
            this.chk_GeneralSet.Name = "chk_GeneralSet";
            this.chk_GeneralSet.Size = new System.Drawing.Size(124, 26);
            this.chk_GeneralSet.TabIndex = 0;
            this.chk_GeneralSet.Text = "常规设置";
            this.chk_GeneralSet.UseVisualStyleBackColor = true;
            // 
            // chk_UserManage
            // 
            this.chk_UserManage.AutoSize = true;
            this.chk_UserManage.Font = new System.Drawing.Font("黑体", 11F);
            this.chk_UserManage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.chk_UserManage.Location = new System.Drawing.Point(87, 230);
            this.chk_UserManage.Name = "chk_UserManage";
            this.chk_UserManage.Size = new System.Drawing.Size(124, 26);
            this.chk_UserManage.TabIndex = 0;
            this.chk_UserManage.Text = "用户权限";
            this.chk_UserManage.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txt_Pwd);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txt_User);
            this.groupBox1.Font = new System.Drawing.Font("黑体", 11F);
            this.groupBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.groupBox1.Location = new System.Drawing.Point(322, 39);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(474, 84);
            this.groupBox1.TabIndex = 46;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "用户信息";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("黑体", 11F);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label3.Location = new System.Drawing.Point(258, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 22);
            this.label3.TabIndex = 12;
            this.label3.Text = "密码：";
            // 
            // txt_Pwd
            // 
            this.txt_Pwd.Font = new System.Drawing.Font("黑体", 11F);
            this.txt_Pwd.Location = new System.Drawing.Point(340, 33);
            this.txt_Pwd.Name = "txt_Pwd";
            this.txt_Pwd.PasswordChar = '*';
            this.txt_Pwd.Size = new System.Drawing.Size(115, 33);
            this.txt_Pwd.TabIndex = 34;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("黑体", 11F);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label2.Location = new System.Drawing.Point(6, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 22);
            this.label2.TabIndex = 12;
            this.label2.Text = "用户名：";
            // 
            // txt_User
            // 
            this.txt_User.Font = new System.Drawing.Font("黑体", 11F);
            this.txt_User.Location = new System.Drawing.Point(117, 30);
            this.txt_User.Name = "txt_User";
            this.txt_User.Size = new System.Drawing.Size(118, 33);
            this.txt_User.TabIndex = 34;
            // 
            // dgv_User
            // 
            this.dgv_User.AllowUserToAddRows = false;
            this.dgv_User.AllowUserToDeleteRows = false;
            this.dgv_User.AllowUserToResizeColumns = false;
            this.dgv_User.AllowUserToResizeRows = false;
            this.dgv_User.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            this.dgv_User.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("黑体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_User.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_User.ColumnHeadersHeight = 35;
            this.dgv_User.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgv_User.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.LoginName});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("黑体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_User.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgv_User.EnableHeadersVisualStyles = false;
            this.dgv_User.Location = new System.Drawing.Point(39, 39);
            this.dgv_User.Name = "dgv_User";
            this.dgv_User.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("黑体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_User.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgv_User.RowHeadersWidth = 25;
            this.dgv_User.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("黑体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dgv_User.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgv_User.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgv_User.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dgv_User.RowTemplate.Height = 30;
            this.dgv_User.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_User.Size = new System.Drawing.Size(261, 388);
            this.dgv_User.TabIndex = 45;
            this.dgv_User.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_User_CellClick);
            // 
            // LoginName
            // 
            this.LoginName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.LoginName.DataPropertyName = "LoginName";
            dataGridViewCellStyle2.Font = new System.Drawing.Font("黑体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LoginName.DefaultCellStyle = dataGridViewCellStyle2;
            this.LoginName.FillWeight = 27.11618F;
            this.LoginName.HeaderText = "用户名";
            this.LoginName.MinimumWidth = 6;
            this.LoginName.Name = "LoginName";
            this.LoginName.ReadOnly = true;
            this.LoginName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("黑体", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label1.Location = new System.Drawing.Point(33, 465);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 23);
            this.label1.TabIndex = 48;
            this.label1.Text = "当前用户：";
            // 
            // txt_CurrentUser
            // 
            this.txt_CurrentUser.BackColor = System.Drawing.Color.White;
            this.txt_CurrentUser.Font = new System.Drawing.Font("黑体", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txt_CurrentUser.Location = new System.Drawing.Point(172, 459);
            this.txt_CurrentUser.Name = "txt_CurrentUser";
            this.txt_CurrentUser.ReadOnly = true;
            this.txt_CurrentUser.Size = new System.Drawing.Size(127, 33);
            this.txt_CurrentUser.TabIndex = 49;
            this.txt_CurrentUser.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btn_Add
            // 
            this.btn_Add.Location = new System.Drawing.Point(333, 456);
            this.btn_Add.Name = "btn_Add";
            this.btn_Add.Size = new System.Drawing.Size(138, 44);
            this.btn_Add.TabIndex = 118;
            this.btn_Add.Text = "增加用户";
            this.btn_Add.Click += new System.EventHandler(this.btn_Add_Click);
            //
            // btn_Save
            //
            this.btn_Save.Location = new System.Drawing.Point(658, 456);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(138, 44);
            this.btn_Save.TabIndex = 119;
            this.btn_Save.Text = "保存修改";
            //
            // btn_Delete
            //
            this.btn_Delete.Location = new System.Drawing.Point(496, 456);
            this.btn_Delete.Name = "btn_Delete";
            this.btn_Delete.Size = new System.Drawing.Size(138, 44);
            this.btn_Delete.TabIndex = 120;
            this.btn_Delete.Text = "删除用户";
            this.btn_Delete.Click += new System.EventHandler(this.btn_Delete_Click);
            // 
            // FrUserManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            this.ClientSize = new System.Drawing.Size(860, 542);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.btn_Delete);
            this.Controls.Add(this.btn_Add);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_CurrentUser);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dgv_User);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrUserManagement";
            this.Text = "FrUserManagement";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_User)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chk_AlarmCtrl;
        private System.Windows.Forms.CheckBox chk_DefectReportCtrl;
        private System.Windows.Forms.CheckBox chk_ProSet;
        private System.Windows.Forms.CheckBox chk_AISet;
        private System.Windows.Forms.CheckBox chk_GeneralSet;
        private System.Windows.Forms.CheckBox chk_UserManage;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txt_Pwd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_User;
        private System.Windows.Forms.DataGridView dgv_User;
        private System.Windows.Forms.DataGridViewTextBoxColumn LoginName;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox txt_CurrentUser;
        private DeepSightAI.StyledButton btn_Select;
        private DeepSightAI.StyledButton btn_Add;
        private DeepSightAI.StyledButton btn_Save;
        private DeepSightAI.StyledButton btn_Delete;
        private System.Windows.Forms.CheckBox chk_FunctionSet;
    }
}
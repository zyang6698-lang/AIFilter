namespace DeepSightAI.SettingPages
{
    partial class FrAIConfig
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrAIConfig));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnAutoAdd = new System.Windows.Forms.Button();
            this.btn_Delete = new System.Windows.Forms.Button();
            this.btn_Add = new System.Windows.Forms.Button();
            this.btn_setSolution = new System.Windows.Forms.Button();
            this.btn_GetAgain = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dataPost = new System.Windows.Forms.DataGridView();
            this.Index = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.liaohao = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.A_solution = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.A_flow = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.B_solution = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.B_flow = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.isSwitch = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Mode = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataPost)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.panel1.Name = "panel1";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panel2.Controls.Add(this.btnAutoAdd);
            this.panel2.Controls.Add(this.btn_Delete);
            this.panel2.Controls.Add(this.btn_Add);
            this.panel2.Controls.Add(this.btn_setSolution);
            this.panel2.Controls.Add(this.btn_GetAgain);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            //
            // btnAutoAdd
            //
            this.btnAutoAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            resources.ApplyResources(this.btnAutoAdd, "btnAutoAdd");
            this.btnAutoAdd.FlatAppearance.BorderSize = 0;
            this.btnAutoAdd.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnAutoAdd.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnAutoAdd.ForeColor = System.Drawing.Color.White;
            this.btnAutoAdd.Name = "btnAutoAdd";
            this.btnAutoAdd.UseVisualStyleBackColor = false;
            this.btnAutoAdd.Click += new System.EventHandler(this.btnAutoAdd_Click);
            //
            // btn_Delete
            //
            this.btn_Delete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            resources.ApplyResources(this.btn_Delete, "btn_Delete");
            this.btn_Delete.FlatAppearance.BorderSize = 0;
            this.btn_Delete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_Delete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_Delete.ForeColor = System.Drawing.Color.White;
            this.btn_Delete.Name = "btn_Delete";
            this.btn_Delete.UseVisualStyleBackColor = false;
            this.btn_Delete.Click += new System.EventHandler(this.btn_Delete_Click);
            //
            // btn_Add
            //
            this.btn_Add.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            resources.ApplyResources(this.btn_Add, "btn_Add");
            this.btn_Add.FlatAppearance.BorderSize = 0;
            this.btn_Add.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_Add.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_Add.ForeColor = System.Drawing.Color.White;
            this.btn_Add.Name = "btn_Add";
            this.btn_Add.UseVisualStyleBackColor = false;
            this.btn_Add.Click += new System.EventHandler(this.btn_Add_Click);
            //
            // btn_setSolution
            //
            this.btn_setSolution.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            resources.ApplyResources(this.btn_setSolution, "btn_setSolution");
            this.btn_setSolution.FlatAppearance.BorderSize = 0;
            this.btn_setSolution.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_setSolution.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_setSolution.ForeColor = System.Drawing.Color.White;
            this.btn_setSolution.Name = "btn_setSolution";
            this.btn_setSolution.UseVisualStyleBackColor = false;
            this.btn_setSolution.Click += new System.EventHandler(this.btn_setSolution_Click);
            //
            // btn_GetAgain
            //
            this.btn_GetAgain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            resources.ApplyResources(this.btn_GetAgain, "btn_GetAgain");
            this.btn_GetAgain.FlatAppearance.BorderSize = 0;
            this.btn_GetAgain.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_GetAgain.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_GetAgain.ForeColor = System.Drawing.Color.White;
            this.btn_GetAgain.Name = "btn_GetAgain";
            this.btn_GetAgain.UseVisualStyleBackColor = false;
            this.btn_GetAgain.Click += new System.EventHandler(this.btn_GetAgain_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.dataPost);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // dataPost
            // 
            this.dataPost.AllowUserToAddRows = false;
            this.dataPost.AllowUserToDeleteRows = false;
            this.dataPost.AllowUserToResizeColumns = false;
            this.dataPost.AllowUserToResizeRows = false;
            this.dataPost.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataPost.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataPost.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataPost.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Index,
            this.liaohao,
            this.A_solution,
            this.A_flow,
            this.B_solution,
            this.B_flow,
            this.isSwitch,
            this.Mode});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.InactiveCaption;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataPost.DefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.dataPost, "dataPost");
            this.dataPost.EnableHeadersVisualStyles = false;
            this.dataPost.MultiSelect = false;
            this.dataPost.Name = "dataPost";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataPost.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataPost.RowHeadersVisible = false;
            this.dataPost.RowTemplate.Height = 23;
            this.dataPost.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataPost_CellClick);
            this.dataPost.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataPost_CellValueChanged);
            // 
            // Index
            // 
            resources.ApplyResources(this.Index, "Index");
            this.Index.Name = "Index";
            this.Index.ReadOnly = true;
            // 
            // liaohao
            // 
            resources.ApplyResources(this.liaohao, "liaohao");
            this.liaohao.Name = "liaohao";
            this.liaohao.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // A_solution
            // 
            this.A_solution.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            resources.ApplyResources(this.A_solution, "A_solution");
            this.A_solution.Name = "A_solution";
            // 
            // A_flow
            // 
            this.A_flow.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            resources.ApplyResources(this.A_flow, "A_flow");
            this.A_flow.Name = "A_flow";
            this.A_flow.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // B_solution
            // 
            this.B_solution.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            resources.ApplyResources(this.B_solution, "B_solution");
            this.B_solution.Name = "B_solution";
            this.B_solution.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.B_solution.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // B_flow
            // 
            this.B_flow.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            resources.ApplyResources(this.B_flow, "B_flow");
            this.B_flow.Name = "B_flow";
            this.B_flow.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.B_flow.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // isSwitch
            // 
            resources.ApplyResources(this.isSwitch, "isSwitch");
            this.isSwitch.Name = "isSwitch";
            // 
            // Mode
            // 
            this.Mode.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            resources.ApplyResources(this.Mode, "Mode");
            this.Mode.Items.AddRange(new object[] {
            "by_machine",
            "copy",
            "cut"});
            this.Mode.Name = "Mode";
            this.Mode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // FrAIConfig
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrAIConfig";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataPost)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridViewTextBoxColumn Index;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridViewComboBoxColumn Code;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        internal System.Windows.Forms.Button btn_Delete;
        internal System.Windows.Forms.Button btn_Add;
        internal System.Windows.Forms.Button btn_setSolution;
        internal System.Windows.Forms.Button btn_GetAgain;
        private System.Windows.Forms.Panel panel3;
        public System.Windows.Forms.DataGridView dataPost;
        private System.Windows.Forms.DataGridViewTextBoxColumn liaohao;
        private System.Windows.Forms.DataGridViewComboBoxColumn A_solution;
        private System.Windows.Forms.DataGridViewComboBoxColumn A_flow;
        private System.Windows.Forms.DataGridViewComboBoxColumn B_solution;
        private System.Windows.Forms.DataGridViewComboBoxColumn B_flow;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isSwitch;
        private System.Windows.Forms.DataGridViewComboBoxColumn Mode;
        internal System.Windows.Forms.Button btnAutoAdd;
    }
}
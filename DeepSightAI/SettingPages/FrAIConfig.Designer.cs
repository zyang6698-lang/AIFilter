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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dgvPipeline = new System.Windows.Forms.DataGridView();
            this.panelLeftHeader = new System.Windows.Forms.Panel();
            this.btn_GetAgain = new DeepSightAI.StyledButton();
            this.btnDeleteConfig = new DeepSightAI.StyledButton();
            this.btnAddConfig = new DeepSightAI.StyledButton();
            this.lblLeftTitle = new System.Windows.Forms.Label();
            this.dgvProducts = new System.Windows.Forms.DataGridView();
            this.colIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProductSerial = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMode = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.panelRightHeader = new System.Windows.Forms.Panel();
            this.btnAutoAdd = new DeepSightAI.StyledButton();
            this.btnDeleteProduct = new DeepSightAI.StyledButton();
            this.btnAddProduct = new DeepSightAI.StyledButton();
            this.lblRightTitle = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.colConfigName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colASolution = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colAFlow = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colBSolution = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colBFlow = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colIsSwitch = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colProductCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPipeline)).BeginInit();
            this.panelLeftHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).BeginInit();
            this.panelRightHeader.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer1.Panel1.Controls.Add(this.dgvPipeline);
            this.splitContainer1.Panel1.Controls.Add(this.panelLeftHeader);
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.splitContainer1.Panel2.Controls.Add(this.dgvProducts);
            this.splitContainer1.Panel2.Controls.Add(this.panelRightHeader);
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            // 
            // dgvPipeline
            // 
            this.dgvPipeline.AllowUserToAddRows = false;
            this.dgvPipeline.AllowUserToDeleteRows = false;
            this.dgvPipeline.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgvPipeline.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(106)))), ((int)(((byte)(122)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
            this.dgvPipeline.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvPipeline.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPipeline.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colConfigName,
            this.colASolution,
            this.colAFlow,
            this.colBSolution,
            this.colBFlow,
            this.colIsSwitch,
            this.colProductCount});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(106)))), ((int)(((byte)(122)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvPipeline.DefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.dgvPipeline, "dgvPipeline");
            this.dgvPipeline.EnableHeadersVisualStyles = false;
            this.dgvPipeline.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            this.dgvPipeline.MultiSelect = false;
            this.dgvPipeline.Name = "dgvPipeline";
            this.dgvPipeline.RowHeadersVisible = false;
            this.dgvPipeline.RowTemplate.Height = 28;
            this.dgvPipeline.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPipeline.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPipeline_CellValueChanged);
            this.dgvPipeline.SelectionChanged += new System.EventHandler(this.dgvPipeline_SelectionChanged);
            // 
            // panelLeftHeader
            // 
            this.panelLeftHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panelLeftHeader.Controls.Add(this.btn_GetAgain);
            this.panelLeftHeader.Controls.Add(this.btnDeleteConfig);
            this.panelLeftHeader.Controls.Add(this.btnAddConfig);
            this.panelLeftHeader.Controls.Add(this.lblLeftTitle);
            resources.ApplyResources(this.panelLeftHeader, "panelLeftHeader");
            this.panelLeftHeader.Name = "panelLeftHeader";
            // 
            // btn_GetAgain
            // 
            resources.ApplyResources(this.btn_GetAgain, "btn_GetAgain");
            this.btn_GetAgain.Name = "btn_GetAgain";
            this.btn_GetAgain.Click += new System.EventHandler(this.btn_GetAgain_Click);
            //
            // btnDeleteConfig
            //
            resources.ApplyResources(this.btnDeleteConfig, "btnDeleteConfig");
            this.btnDeleteConfig.Name = "btnDeleteConfig";
            this.btnDeleteConfig.Click += new System.EventHandler(this.btnDeleteConfig_Click);
            //
            // btnAddConfig
            //
            resources.ApplyResources(this.btnAddConfig, "btnAddConfig");
            this.btnAddConfig.Name = "btnAddConfig";
            this.btnAddConfig.Click += new System.EventHandler(this.btnAddConfig_Click);
            // 
            // lblLeftTitle
            // 
            resources.ApplyResources(this.lblLeftTitle, "lblLeftTitle");
            this.lblLeftTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblLeftTitle.Name = "lblLeftTitle";
            // 
            // dgvProducts
            // 
            this.dgvProducts.AllowUserToAddRows = false;
            this.dgvProducts.AllowUserToDeleteRows = false;
            this.dgvProducts.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgvProducts.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(106)))), ((int)(((byte)(122)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            this.dgvProducts.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProducts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colIndex,
            this.colProductSerial,
            this.colMode});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(106)))), ((int)(((byte)(122)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvProducts.DefaultCellStyle = dataGridViewCellStyle4;
            resources.ApplyResources(this.dgvProducts, "dgvProducts");
            this.dgvProducts.EnableHeadersVisualStyles = false;
            this.dgvProducts.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            this.dgvProducts.Name = "dgvProducts";
            this.dgvProducts.RowHeadersVisible = false;
            this.dgvProducts.RowTemplate.Height = 26;
            this.dgvProducts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            // 
            // colIndex
            // 
            resources.ApplyResources(this.colIndex, "colIndex");
            this.colIndex.Name = "colIndex";
            this.colIndex.ReadOnly = true;
            // 
            // colProductSerial
            // 
            resources.ApplyResources(this.colProductSerial, "colProductSerial");
            this.colProductSerial.Name = "colProductSerial";
            // 
            // colMode
            // 
            this.colMode.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            resources.ApplyResources(this.colMode, "colMode");
            this.colMode.Items.AddRange(new object[] {
            "by_machine",
            "copy",
            "cut"});
            this.colMode.Name = "colMode";
            // 
            // panelRightHeader
            // 
            this.panelRightHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panelRightHeader.Controls.Add(this.btnAutoAdd);
            this.panelRightHeader.Controls.Add(this.btnDeleteProduct);
            this.panelRightHeader.Controls.Add(this.btnAddProduct);
            this.panelRightHeader.Controls.Add(this.lblRightTitle);
            resources.ApplyResources(this.panelRightHeader, "panelRightHeader");
            this.panelRightHeader.Name = "panelRightHeader";
            // 
            // btnAutoAdd
            // 
            resources.ApplyResources(this.btnAutoAdd, "btnAutoAdd");
            this.btnAutoAdd.Name = "btnAutoAdd";
            this.btnAutoAdd.Click += new System.EventHandler(this.btnAutoAdd_Click);
            //
            // btnDeleteProduct
            //
            resources.ApplyResources(this.btnDeleteProduct, "btnDeleteProduct");
            this.btnDeleteProduct.Name = "btnDeleteProduct";
            this.btnDeleteProduct.Click += new System.EventHandler(this.btnDeleteProduct_Click);
            //
            // btnAddProduct
            //
            resources.ApplyResources(this.btnAddProduct, "btnAddProduct");
            this.btnAddProduct.Name = "btnAddProduct";
            this.btnAddProduct.Click += new System.EventHandler(this.btnAddProduct_Click);
            // 
            // lblRightTitle
            // 
            resources.ApplyResources(this.lblRightTitle, "lblRightTitle");
            this.lblRightTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblRightTitle.Name = "lblRightTitle";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            this.panel1.Controls.Add(this.splitContainer1);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.panel1.Name = "panel1";
            // 
            // colConfigName
            // 
            resources.ApplyResources(this.colConfigName, "colConfigName");
            this.colConfigName.Name = "colConfigName";
            // 
            // colASolution
            // 
            this.colASolution.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            resources.ApplyResources(this.colASolution, "colASolution");
            this.colASolution.Name = "colASolution";
            // 
            // colAFlow
            // 
            this.colAFlow.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            resources.ApplyResources(this.colAFlow, "colAFlow");
            this.colAFlow.Name = "colAFlow";
            // 
            // colBSolution
            // 
            this.colBSolution.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            resources.ApplyResources(this.colBSolution, "colBSolution");
            this.colBSolution.Name = "colBSolution";
            // 
            // colBFlow
            // 
            this.colBFlow.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            resources.ApplyResources(this.colBFlow, "colBFlow");
            this.colBFlow.Name = "colBFlow";
            // 
            // colIsSwitch
            // 
            resources.ApplyResources(this.colIsSwitch, "colIsSwitch");
            this.colIsSwitch.Name = "colIsSwitch";
            // 
            // colProductCount
            // 
            resources.ApplyResources(this.colProductCount, "colProductCount");
            this.colProductCount.Name = "colProductCount";
            this.colProductCount.ReadOnly = true;
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
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPipeline)).EndInit();
            this.panelLeftHeader.ResumeLayout(false);
            this.panelLeftHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).EndInit();
            this.panelRightHeader.ResumeLayout(false);
            this.panelRightHeader.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        // 主容器
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;

        // 左侧 - 算法配置区
        private System.Windows.Forms.Panel panelLeftHeader;
        private System.Windows.Forms.Label lblLeftTitle;
        private DeepSightAI.StyledButton btnAddConfig;
        private DeepSightAI.StyledButton btnDeleteConfig;
        internal DeepSightAI.StyledButton btn_GetAgain;
        private System.Windows.Forms.DataGridView dgvPipeline;

        // 右侧 - 料号区
        private System.Windows.Forms.Panel panelRightHeader;
        private System.Windows.Forms.Label lblRightTitle;
        private DeepSightAI.StyledButton btnAddProduct;
        private DeepSightAI.StyledButton btnDeleteProduct;
        internal DeepSightAI.StyledButton btnAutoAdd;
        private System.Windows.Forms.DataGridView dgvProducts;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductSerial;
        private System.Windows.Forms.DataGridViewComboBoxColumn colMode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colConfigName;
        private System.Windows.Forms.DataGridViewComboBoxColumn colASolution;
        private System.Windows.Forms.DataGridViewComboBoxColumn colAFlow;
        private System.Windows.Forms.DataGridViewComboBoxColumn colBSolution;
        private System.Windows.Forms.DataGridViewComboBoxColumn colBFlow;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsSwitch;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductCount;
    }
}
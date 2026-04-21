namespace DeepSightAI.SettingPages
{
    partial class PgSettingAi
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PgSettingAi));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panelLeftContent = new System.Windows.Forms.Panel();
            this.dgvPipeline = new Sunny.UI.UIDataGridView();
            this.panelLeftHeader = new System.Windows.Forms.Panel();
            this.btn_GetAgain = new DeepSightAI.StyledButton();
            this.btnDeleteConfig = new DeepSightAI.StyledButton();
            this.btnAddConfig = new DeepSightAI.StyledButton();
            this.lblLeftTitle = new System.Windows.Forms.Label();
            this.panelRightContent = new System.Windows.Forms.Panel();
            this.dgvProducts = new Sunny.UI.UIDataGridView();
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
            this.colIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProductSerial = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMode = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colKeyDefectProfile = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.tableLayoutPanel1.SuspendLayout();
            this.panelLeftContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPipeline)).BeginInit();
            this.panelLeftHeader.SuspendLayout();
            this.panelRightContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).BeginInit();
            this.panelRightHeader.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.panelLeftContent, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelRightContent, 1, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // panelLeftContent
            // 
            this.panelLeftContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panelLeftContent.Controls.Add(this.dgvPipeline);
            this.panelLeftContent.Controls.Add(this.panelLeftHeader);
            resources.ApplyResources(this.panelLeftContent, "panelLeftContent");
            this.panelLeftContent.Name = "panelLeftContent";
            // 
            // dgvPipeline
            // 
            this.dgvPipeline.AllowUserToAddRows = false;
            this.dgvPipeline.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(58)))));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            this.dgvPipeline.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvPipeline.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgvPipeline.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvPipeline.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            this.dgvPipeline.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.dgvPipeline, "dgvPipeline");
            this.dgvPipeline.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvPipeline.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colConfigName,
            this.colASolution,
            this.colAFlow,
            this.colBSolution,
            this.colBFlow,
            this.colIsSwitch,
            this.colProductCount});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvPipeline.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvPipeline.EnableHeadersVisualStyles = false;
            this.dgvPipeline.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.dgvPipeline.MultiSelect = false;
            this.dgvPipeline.Name = "dgvPipeline";
            this.dgvPipeline.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvPipeline.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvPipeline.RowHeadersVisible = false;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dgvPipeline.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvPipeline.RowTemplate.Height = 28;
            this.dgvPipeline.ScrollBarBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgvPipeline.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.dgvPipeline.ScrollBarRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgvPipeline.ScrollBarStyleInherited = false;
            this.dgvPipeline.SelectedIndex = -1;
            this.dgvPipeline.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPipeline.StripeEvenColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.dgvPipeline.StripeOddColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(58)))));
            this.dgvPipeline.Style = Sunny.UI.UIStyle.Custom;
            this.dgvPipeline.StyleCustomMode = true;
            this.dgvPipeline.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPipeline_CellValueChanged);
            this.dgvPipeline.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvPipeline_DataError);
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
            this.btn_GetAgain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btn_GetAgain.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_GetAgain.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btn_GetAgain.Name = "btn_GetAgain";
            this.btn_GetAgain.UseVisualStyleBackColor = false;
            this.btn_GetAgain.Click += new System.EventHandler(this.btn_GetAgain_Click);
            // 
            // btnDeleteConfig
            // 
            resources.ApplyResources(this.btnDeleteConfig, "btnDeleteConfig");
            this.btnDeleteConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnDeleteConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteConfig.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnDeleteConfig.Name = "btnDeleteConfig";
            this.btnDeleteConfig.UseVisualStyleBackColor = false;
            this.btnDeleteConfig.Click += new System.EventHandler(this.btnDeleteConfig_Click);
            // 
            // btnAddConfig
            // 
            resources.ApplyResources(this.btnAddConfig, "btnAddConfig");
            this.btnAddConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnAddConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddConfig.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnAddConfig.Name = "btnAddConfig";
            this.btnAddConfig.UseVisualStyleBackColor = false;
            this.btnAddConfig.Click += new System.EventHandler(this.btnAddConfig_Click);
            // 
            // lblLeftTitle
            // 
            resources.ApplyResources(this.lblLeftTitle, "lblLeftTitle");
            this.lblLeftTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.lblLeftTitle.Name = "lblLeftTitle";
            // 
            // panelRightContent
            // 
            this.panelRightContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.panelRightContent.Controls.Add(this.dgvProducts);
            this.panelRightContent.Controls.Add(this.panelRightHeader);
            resources.ApplyResources(this.panelRightContent, "panelRightContent");
            this.panelRightContent.Name = "panelRightContent";
            // 
            // dgvProducts
            // 
            this.dgvProducts.AllowUserToAddRows = false;
            this.dgvProducts.AllowUserToDeleteRows = false;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(58)))));
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.White;
            this.dgvProducts.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dgvProducts.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgvProducts.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvProducts.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            dataGridViewCellStyle7.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.Color.White;
            this.dgvProducts.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            resources.ApplyResources(this.dgvProducts, "dgvProducts");
            this.dgvProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvProducts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colIndex,
            this.colProductSerial,
            this.colMode,
            this.colKeyDefectProfile});
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle8.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvProducts.DefaultCellStyle = dataGridViewCellStyle8;
            this.dgvProducts.EnableHeadersVisualStyles = false;
            this.dgvProducts.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.dgvProducts.Name = "dgvProducts";
            this.dgvProducts.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle9.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvProducts.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.dgvProducts.RowHeadersVisible = false;
            dataGridViewCellStyle10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle10.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dgvProducts.RowsDefaultCellStyle = dataGridViewCellStyle10;
            this.dgvProducts.RowTemplate.Height = 26;
            this.dgvProducts.ScrollBarBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgvProducts.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.dgvProducts.ScrollBarRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.dgvProducts.ScrollBarStyleInherited = false;
            this.dgvProducts.SelectedIndex = -1;
            this.dgvProducts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProducts.StripeEvenColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.dgvProducts.StripeOddColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(58)))));
            this.dgvProducts.Style = Sunny.UI.UIStyle.Custom;
            this.dgvProducts.StyleCustomMode = true;
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
            this.btnAutoAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnAutoAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAutoAdd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnAutoAdd.Name = "btnAutoAdd";
            this.btnAutoAdd.UseVisualStyleBackColor = false;
            this.btnAutoAdd.Click += new System.EventHandler(this.btnAutoAdd_Click);
            // 
            // btnDeleteProduct
            // 
            resources.ApplyResources(this.btnDeleteProduct, "btnDeleteProduct");
            this.btnDeleteProduct.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnDeleteProduct.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteProduct.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnDeleteProduct.Name = "btnDeleteProduct";
            this.btnDeleteProduct.UseVisualStyleBackColor = false;
            this.btnDeleteProduct.Click += new System.EventHandler(this.btnDeleteProduct_Click);
            // 
            // btnAddProduct
            // 
            resources.ApplyResources(this.btnAddProduct, "btnAddProduct");
            this.btnAddProduct.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnAddProduct.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddProduct.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btnAddProduct.Name = "btnAddProduct";
            this.btnAddProduct.UseVisualStyleBackColor = false;
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
            this.panel1.Controls.Add(this.tableLayoutPanel1);
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
            // colKeyDefectProfile
            // 
            this.colKeyDefectProfile.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            resources.ApplyResources(this.colKeyDefectProfile, "colKeyDefectProfile");
            this.colKeyDefectProfile.Name = "colKeyDefectProfile";
            // 
            // PgSettingAi
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.panel1);
            this.Name = "PgSettingAi";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panelLeftContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPipeline)).EndInit();
            this.panelLeftHeader.ResumeLayout(false);
            this.panelLeftHeader.PerformLayout();
            this.panelRightContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).EndInit();
            this.panelRightHeader.ResumeLayout(false);
            this.panelRightHeader.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        // 主容器
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panelLeftContent;
        private System.Windows.Forms.Panel panelRightContent;

        // 左侧 - 算法配置区
        private System.Windows.Forms.Panel panelLeftHeader;
        private System.Windows.Forms.Label lblLeftTitle;
        private DeepSightAI.StyledButton btnAddConfig;
        private DeepSightAI.StyledButton btnDeleteConfig;
        internal DeepSightAI.StyledButton btn_GetAgain;
        private Sunny.UI.UIDataGridView dgvPipeline;

        // 右侧 - 料号区
        private System.Windows.Forms.Panel panelRightHeader;
        private System.Windows.Forms.Label lblRightTitle;
        private DeepSightAI.StyledButton btnAddProduct;
        private DeepSightAI.StyledButton btnDeleteProduct;
        internal DeepSightAI.StyledButton btnAutoAdd;
        private Sunny.UI.UIDataGridView dgvProducts;
        private System.Windows.Forms.DataGridViewTextBoxColumn colConfigName;
        private System.Windows.Forms.DataGridViewComboBoxColumn colASolution;
        private System.Windows.Forms.DataGridViewComboBoxColumn colAFlow;
        private System.Windows.Forms.DataGridViewComboBoxColumn colBSolution;
        private System.Windows.Forms.DataGridViewComboBoxColumn colBFlow;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsSwitch;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductSerial;
        private System.Windows.Forms.DataGridViewComboBoxColumn colMode;
        private System.Windows.Forms.DataGridViewComboBoxColumn colKeyDefectProfile;
    }
}
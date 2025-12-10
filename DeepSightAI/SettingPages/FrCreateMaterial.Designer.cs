namespace DeepSightAI.SettingPages
{
    partial class FrCreateMaterial
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrCreateMaterial));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.uiGroupBox1 = new Sunny.UI.UIGroupBox();
            this.btn_selectA = new System.Windows.Forms.Button();
            this.btn_selectB = new System.Windows.Forms.Button();
            this.btnZipPic = new System.Windows.Forms.Button();
            this.btnClearDatabase = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.uiGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.uiGroupBox1, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            //
            // uiGroupBox1
            //
            this.uiGroupBox1.Controls.Add(this.btnClearDatabase);
            this.uiGroupBox1.Controls.Add(this.btnZipPic);
            this.uiGroupBox1.Controls.Add(this.btn_selectA);
            this.uiGroupBox1.Controls.Add(this.btn_selectB);
            resources.ApplyResources(this.uiGroupBox1, "uiGroupBox1");
            this.uiGroupBox1.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.uiGroupBox1.FillDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.uiGroupBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.uiGroupBox1.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_selectA
            // 
            this.btn_selectA.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btn_selectA.FlatAppearance.BorderSize = 0;
            this.btn_selectA.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_selectA.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_selectA.ForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.btn_selectA, "btn_selectA");
            this.btn_selectA.Name = "btn_selectA";
            this.btn_selectA.UseVisualStyleBackColor = false;
            this.btn_selectA.Click += new System.EventHandler(this.btn_selectA_Click);
            // 
            // btn_selectB
            // 
            this.btn_selectB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btn_selectB.FlatAppearance.BorderSize = 0;
            this.btn_selectB.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_selectB.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_selectB.ForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.btn_selectB, "btn_selectB");
            this.btn_selectB.Name = "btn_selectB";
            this.btn_selectB.UseVisualStyleBackColor = false;
            this.btn_selectB.Click += new System.EventHandler(this.btn_selectB_Click);
            //
            // btnZipPic
            //
            this.btnZipPic.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnZipPic.FlatAppearance.BorderSize = 0;
            this.btnZipPic.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnZipPic.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnZipPic.ForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.btnZipPic, "btnZipPic");
            this.btnZipPic.Name = "btnZipPic";
            this.btnZipPic.UseVisualStyleBackColor = false;
            this.btnZipPic.Click += new System.EventHandler(this.btnZipPic_Click);
            //
            // btnClearDatabase
            //
            this.btnClearDatabase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnClearDatabase.FlatAppearance.BorderSize = 0;
            this.btnClearDatabase.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnClearDatabase.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnClearDatabase.ForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.btnClearDatabase, "btnClearDatabase");
            this.btnClearDatabase.Name = "btnClearDatabase";
            this.btnClearDatabase.UseVisualStyleBackColor = false;
            this.btnClearDatabase.Click += new System.EventHandler(this.btnClearDatabase_Click);
            //
            // FrCreateMaterial
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrCreateMaterial";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.uiGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Sunny.UI.UIGroupBox uiGroupBox1;
        public System.Windows.Forms.Button btn_selectA;
        public System.Windows.Forms.Button btn_selectB;
        public System.Windows.Forms.Button btnZipPic;
        public System.Windows.Forms.Button btnClearDatabase;
    }
}
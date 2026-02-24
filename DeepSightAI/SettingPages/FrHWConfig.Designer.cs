
namespace DeepSightAI.SettingPages
{
    partial class FrHWConfig
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.uiGroupBox1 = new Sunny.UI.UIGroupBox();
            this.aviCtr2Container1 = new DeepSightAI.SettingPages.AviCtr2Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblstationcount = new System.Windows.Forms.Label();
            this.txt_station_count = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.uiGroupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txt_station_count)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.920188F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.07981F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(2250, 986);
            this.tableLayoutPanel1.TabIndex = 0;
            //
            // panel2
            //
            this.panel2.Controls.Add(this.uiGroupBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(4, 91);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(2242, 891);
            this.panel2.TabIndex = 1;
            //
            // uiGroupBox1
            //
            this.uiGroupBox1.Controls.Add(this.aviCtr2Container1);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.uiGroupBox1.FillDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(48)))), ((int)(((byte)(60)))));
            this.uiGroupBox1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.uiGroupBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Padding = new System.Windows.Forms.Padding(27, 25, 27, 25);
            this.uiGroupBox1.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(190)))), ((int)(((byte)(40)))));
            this.uiGroupBox1.Size = new System.Drawing.Size(2242, 891);
            this.uiGroupBox1.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox1.TabIndex = 27;
            this.uiGroupBox1.Text = "AVI配置";
            this.uiGroupBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // aviCtr2Container1
            // 
            this.aviCtr2Container1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aviCtr2Container1.Location = new System.Drawing.Point(27, 25);
            this.aviCtr2Container1.Margin = new System.Windows.Forms.Padding(48, 169, 48, 169);
            this.aviCtr2Container1.Name = "aviCtr2Container1";
            this.aviCtr2Container1.ShowDeleteButtons = true;
            this.aviCtr2Container1.Size = new System.Drawing.Size(1009, 829);
            this.aviCtr2Container1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblstationcount);
            this.panel1.Controls.Add(this.txt_station_count);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(4, 4);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(2242, 79);
            this.panel1.TabIndex = 0;
            // 
            // lblstationcount
            // 
            this.lblstationcount.AutoSize = true;
            this.lblstationcount.Location = new System.Drawing.Point(537, 25);
            this.lblstationcount.Name = "lblstationcount";
            this.lblstationcount.Size = new System.Drawing.Size(15, 15);
            this.lblstationcount.TabIndex = 26;
            this.lblstationcount.Text = "0";
            this.lblstationcount.Visible = false;
            // 
            // txt_station_count
            // 
            this.txt_station_count.Location = new System.Drawing.Point(183, 19);
            this.txt_station_count.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txt_station_count.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.txt_station_count.Name = "txt_station_count";
            this.txt_station_count.Size = new System.Drawing.Size(324, 25);
            this.txt_station_count.TabIndex = 23;
            this.txt_station_count.ValueChanged += new System.EventHandler(this.txt_station_count_ValueChanged);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.label2.Location = new System.Drawing.Point(11, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(188, 38);
            this.label2.TabIndex = 8;
            this.label2.Text = "AVI工站数量：";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FrHWConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            this.ClientSize = new System.Drawing.Size(2250, 986);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FrHWConfig";
            this.Text = "FrUserManagement";
            this.Shown += new System.EventHandler(this.FrHWConfig_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.uiGroupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txt_station_count)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        internal System.Windows.Forms.NumericUpDown txt_station_count;
        internal System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblstationcount;
        private Sunny.UI.UIGroupBox uiGroupBox1;
        private AviCtr2Container aviCtr2Container1;
    }
}
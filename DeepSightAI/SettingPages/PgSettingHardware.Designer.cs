
namespace DeepSightAI.SettingPages
{
    partial class PgSettingHardware
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
            this.machineStatusPanel1 = new DeepSightAI.SettingPages.UcMachineStatusPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_add_station = new DeepSightAI.StyledButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
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
            this.panel2.Controls.Add(this.machineStatusPanel1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(4, 91);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(2242, 891);
            this.panel2.TabIndex = 1;
            // 
            // machineStatusPanel1
            // 
            this.machineStatusPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.machineStatusPanel1.Location = new System.Drawing.Point(0, 0);
            this.machineStatusPanel1.Margin = new System.Windows.Forms.Padding(90, 352, 90, 352);
            this.machineStatusPanel1.Name = "machineStatusPanel1";
            this.machineStatusPanel1.ShowDeleteButtons = true;
            this.machineStatusPanel1.Size = new System.Drawing.Size(1868, 743);
            this.machineStatusPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btn_add_station);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(4, 4);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(2242, 79);
            this.panel1.TabIndex = 0;
            // 
            // btn_add_station
            // 
            this.btn_add_station.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btn_add_station.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_add_station.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_add_station.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btn_add_station.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(219)))), ((int)(((byte)(188)))));
            this.btn_add_station.Location = new System.Drawing.Point(11, 11);
            this.btn_add_station.Name = "btn_add_station";
            this.btn_add_station.Size = new System.Drawing.Size(160, 50);
            this.btn_add_station.TabIndex = 0;
            this.btn_add_station.Text = "＋ 添加机台";
            this.btn_add_station.UseVisualStyleBackColor = false;
            this.btn_add_station.Click += new System.EventHandler(this.btn_add_station_Click);
            // 
            // PgSettingHardware
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(76)))), ((int)(((byte)(80)))));
            this.ClientSize = new System.Drawing.Size(2250, 986);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "PgSettingHardware";
            this.Text = "FrmUserManagement";
            this.Shown += new System.EventHandler(this.FrHWConfig_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private DeepSightAI.StyledButton btn_add_station;
        private UcMachineStatusPanel machineStatusPanel1;
    }
}
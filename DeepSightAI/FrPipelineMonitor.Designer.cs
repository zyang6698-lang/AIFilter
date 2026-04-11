namespace DeepSightAI
{
    partial class FrPipelineMonitor
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblPipelineStatus = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.panelFlow = new System.Windows.Forms.Panel();
            this.dgvTasks = new System.Windows.Forms.DataGridView();
            this.colSN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSide = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTasks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // panelTop
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(22, 38, 50);
            this.panelTop.Controls.Add(this.lblPipelineStatus);
            this.panelTop.Controls.Add(this.btnClear);
            this.panelTop.Controls.Add(this.btnClose);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 50;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1100, 50);
            // lblPipelineStatus
            this.lblPipelineStatus.AutoSize = true;
            this.lblPipelineStatus.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.lblPipelineStatus.ForeColor = System.Drawing.Color.FromArgb(100, 200, 255);
            this.lblPipelineStatus.Location = new System.Drawing.Point(15, 12);
            this.lblPipelineStatus.Name = "lblPipelineStatus";
            this.lblPipelineStatus.Text = "Pipeline 监控";
            // btnClose
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(50, 80, 100);
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(1000, 8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(85, 34);
            this.btnClose.Text = "关闭";
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // btnClear
            this.btnClear.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnClear.BackColor = System.Drawing.Color.FromArgb(50, 80, 100);
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnClear.ForeColor = System.Drawing.Color.White;
            this.btnClear.Location = new System.Drawing.Point(900, 8);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(90, 34);
            this.btnClear.Text = "清空记录";
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // panelFlow
            this.panelFlow.BackColor = System.Drawing.Color.FromArgb(18, 30, 42);
            this.panelFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelFlow.Name = "panelFlow";
            this.panelFlow.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelFlow_Paint);
            // dgvTasks
            this.dgvTasks.AllowUserToAddRows = false;
            this.dgvTasks.AllowUserToDeleteRows = false;
            this.dgvTasks.AllowUserToResizeRows = false;
            this.dgvTasks.BackgroundColor = System.Drawing.Color.FromArgb(18, 30, 42);
            this.dgvTasks.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvTasks.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvTasks.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvTasks.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(30, 50, 65);
            this.dgvTasks.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(160, 200, 230);
            this.dgvTasks.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.dgvTasks.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(30, 50, 65);
            this.dgvTasks.ColumnHeadersHeight = 36;
            this.dgvTasks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colSN, this.colSide, this.colStage, this.colStatus, this.colMessage, this.colTime });
            this.dgvTasks.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(18, 30, 42);
            this.dgvTasks.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.dgvTasks.DefaultCellStyle.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.dgvTasks.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(40, 65, 85);
            this.dgvTasks.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            this.dgvTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTasks.EnableHeadersVisualStyles = false;
            this.dgvTasks.GridColor = System.Drawing.Color.FromArgb(35, 55, 70);
            this.dgvTasks.Name = "dgvTasks";
            this.dgvTasks.ReadOnly = true;
            this.dgvTasks.RowHeadersVisible = false;
            this.dgvTasks.RowTemplate.Height = 30;
            this.dgvTasks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            // columns
            this.colSN.HeaderText = "SN"; this.colSN.Name = "colSN"; this.colSN.Width = 220;
            this.colSide.HeaderText = "面别"; this.colSide.Name = "colSide"; this.colSide.Width = 60;
            this.colStage.HeaderText = "阶段"; this.colStage.Name = "colStage"; this.colStage.Width = 140;
            this.colStatus.HeaderText = "状态"; this.colStatus.Name = "colStatus"; this.colStatus.Width = 120;
            this.colMessage.HeaderText = "消息"; this.colMessage.Name = "colMessage"; this.colMessage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTime.HeaderText = "时间"; this.colTime.Name = "colTime"; this.colTime.Width = 100;
            // splitContainer
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 50);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainer.Panel1.Controls.Add(this.panelFlow);
            this.splitContainer.Panel2.Controls.Add(this.dgvTasks);
            this.splitContainer.Size = new System.Drawing.Size(1100, 600);
            this.splitContainer.SplitterDistance = 220;
            this.splitContainer.SplitterWidth = 5;
            this.splitContainer.BackColor = System.Drawing.Color.FromArgb(35, 55, 70);
            // refreshTimer
            this.refreshTimer.Interval = 500;
            this.refreshTimer.Tick += new System.EventHandler(this.RefreshTimer_Tick);
            // FrPipelineMonitor
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(18, 30, 42);
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.MaximizeBox = true;
            this.Name = "FrPipelineMonitor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pipeline 流水线监控";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTasks)).EndInit();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblPipelineStatus;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Panel panelFlow;
        private System.Windows.Forms.DataGridView dgvTasks;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSN;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSide;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMessage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Timer refreshTimer;
    }
}

using DeepSightModel;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    /// <summary>
    /// LevelDB 数据库配置界面
    /// </summary>
    public partial class FrLevelDbConfig : Form
    {
        public FrLevelDbConfig()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
        }

        /// <summary>
        /// 窗体实例对象（单例）
        /// </summary>
        private static FrLevelDbConfig _instance;

        public static FrLevelDbConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrLevelDbConfig();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 获取 DataGridView 的表头样式
        /// </summary>
        private static DataGridViewCellStyle GetHeaderStyle()
        {
            return new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(20, 38, 48),
                ForeColor = Color.FromArgb(216, 219, 188),
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
        }

        /// <summary>
        /// 获取 DataGridView 的单元格样式
        /// </summary>
        private static DataGridViewCellStyle GetCellStyle()
        {
            return new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(29, 48, 60),
                ForeColor = Color.FromArgb(216, 219, 188),
                SelectionBackColor = Color.FromArgb(0, 64, 82),
                SelectionForeColor = Color.White,
                Font = new Font("微软雅黑", 9F)
            };
        }

        /// <summary>
        /// 加载配置到 DataGridView
        /// </summary>
        private void FrLevelDbConfig_Load(object sender, EventArgs e)
        {
            LoadConfigToGrid();
        }

        /// <summary>
        /// 从配置管理器加载数据到 DataGridView
        /// </summary>
        private void LoadConfigToGrid()
        {
            dgvDatabases.Rows.Clear();
            var databases = LevelDbConfigManager.Instance.Databases;
            foreach (var db in databases)
            {
                int rowIndex = dgvDatabases.Rows.Add();
                var row = dgvDatabases.Rows[rowIndex];
                row.Cells["colDbName"].Value = db.DbName;
                row.Cells["colIP"].Value = db.IP;
                row.Cells["colPort"].Value = db.Port;
                row.Cells["colWriteBackDbName"].Value = db.WriteBackDbName;
                row.Cells["colIsEnabled"].Value = db.IsEnabled;
            }
        }

        /// <summary>
        /// 添加新的数据库配置
        /// </summary>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            int rowIndex = dgvDatabases.Rows.Add();
            var row = dgvDatabases.Rows[rowIndex];
            row.Cells["colDbName"].Value = "ai_merged_results";
            row.Cells["colIP"].Value = "http://127.0.0.1";
            row.Cells["colPort"].Value = "2000";
            row.Cells["colWriteBackDbName"].Value = "filter_time_to_airesults";
            row.Cells["colIsEnabled"].Value = true;

            dgvDatabases.CurrentCell = row.Cells["colDbName"];
            dgvDatabases.BeginEdit(true);
        }

        /// <summary>
        /// 删除选中的数据库配置
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvDatabases.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要删除的行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dgvDatabases.Rows.Count <= 1)
            {
                MessageBox.Show("至少保留一个数据库配置", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("确定要删除选中的数据库配置吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                foreach (DataGridViewRow row in dgvDatabases.SelectedRows)
                {
                    if (!row.IsNewRow)
                        dgvDatabases.Rows.Remove(row);
                }
            }
        }

        /// <summary>
        /// 从 DataGridView 获取配置并保存
        /// </summary>
        public bool SaveConfig()
        {
            try
            {
                var configList = new LevelDbConfigList();
                configList.Databases = new List<LevelDbConfig>();

                foreach (DataGridViewRow row in dgvDatabases.Rows)
                {
                    if (row.IsNewRow) continue;

                    var config = new LevelDbConfig
                    {
                        DbName = row.Cells["colDbName"].Value?.ToString() ?? "ai_merged_results",
                        IP = row.Cells["colIP"].Value?.ToString() ?? "http://127.0.0.1",
                        Port = row.Cells["colPort"].Value?.ToString() ?? "2000",
                        WriteBackDbName = row.Cells["colWriteBackDbName"].Value?.ToString() ?? "filter_time_to_airesults",
                        IsEnabled = row.Cells["colIsEnabled"].Value != null && (bool)row.Cells["colIsEnabled"].Value
                    };
                    configList.Databases.Add(config);
                }

                return LevelDbConfigManager.Save(configList);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"保存 LevelDB 配置失败: {ex.Message}");
                return false;
            }
        }
    }
}


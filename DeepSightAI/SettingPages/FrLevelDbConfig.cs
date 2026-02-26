using DeepSightModel;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;
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
        /// 连接测试超时时间（毫秒）
        /// </summary>
        private const int TestTimeoutMs = 3000;

        /// <summary>
        /// 是否正在加载数据（加载期间不触发 CellValueChanged 逻辑）
        /// </summary>
        private bool _isLoading = false;

        /// <summary>
        /// 加载配置到 DataGridView
        /// </summary>
        private void FrLevelDbConfig_Load(object sender, EventArgs e)
        {
            LoadConfigToGrid();
            // 加载完成后仅对已启用的数据库自动测试
            TestEnabledConnectionsAsync();
        }

        /// <summary>
        /// 从配置管理器加载数据到 DataGridView
        /// </summary>
        private void LoadConfigToGrid()
        {
            _isLoading = true;
            try
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
                    row.Cells["colConnectionStatus"].Value = "未测试";
                    row.Cells["colIsEnabled"].Value = db.IsEnabled;
                }
            }
            finally
            {
                _isLoading = false;
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
            row.Cells["colConnectionStatus"].Value = "未测试";
            row.Cells["colIsEnabled"].Value = false;

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

        #region 连接测试

        /// <summary>
        /// 全部测试按钮点击
        /// </summary>
        private void btnTestAll_Click(object sender, EventArgs e)
        {
            TestAllConnectionsAsync();
        }

        /// <summary>
        /// DataGridView 按钮列点击事件（测试连接按钮）
        /// </summary>
        private void dgvDatabases_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // 点击测试连接按钮
            if (dgvDatabases.Columns[e.ColumnIndex].Name == "colTestConnection")
            {
                TestConnectionAsync(e.RowIndex);
            }
        }

        /// <summary>
        /// 当 CheckBox 列的 dirty state 变化时立即提交，以便触发 CellValueChanged
        /// </summary>
        private void dgvDatabases_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvDatabases.IsCurrentCellDirty)
            {
                dgvDatabases.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        /// <summary>
        /// 单元格值改变事件 - IP/Port改变时自动测试，启用列需验证连接状态
        /// </summary>
        private void dgvDatabases_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _isLoading) return;

            string colName = dgvDatabases.Columns[e.ColumnIndex].Name;

            // IP 或端口改变时，重置状态并自动测试
            if (colName == "colIP" || colName == "colPort")
            {
                var row = dgvDatabases.Rows[e.RowIndex];
                row.Cells["colConnectionStatus"].Value = "未测试";
                row.Cells["colConnectionStatus"].Style.ForeColor = Color.FromArgb(216, 219, 188);
                // 连接参数变化后，禁用该行（需要重新测试才能启用）
                row.Cells["colIsEnabled"].Value = false;
                TestConnectionAsync(e.RowIndex);
            }

            // 尝试启用时，检查连接状态
            if (colName == "colIsEnabled")
            {
                var row = dgvDatabases.Rows[e.RowIndex];
                bool isEnabled = row.Cells["colIsEnabled"].Value != null && (bool)row.Cells["colIsEnabled"].Value;
                if (isEnabled)
                {
                    string status = row.Cells["colConnectionStatus"].Value?.ToString() ?? "";
                    if (status != "已连接 ✓")
                    {
                        row.Cells["colIsEnabled"].Value = false;
                    }
                }
            }
        }

        /// <summary>
        /// 异步测试所有行的连接
        /// </summary>
        private async void TestAllConnectionsAsync()
        {
            for (int i = 0; i < dgvDatabases.Rows.Count; i++)
            {
                if (dgvDatabases.Rows[i].IsNewRow) continue;
                await TestConnectionCoreAsync(i);
            }
        }

        /// <summary>
        /// 仅对已启用的数据库异步测试连接
        /// </summary>
        private async void TestEnabledConnectionsAsync()
        {
            for (int i = 0; i < dgvDatabases.Rows.Count; i++)
            {
                var row = dgvDatabases.Rows[i];
                if (row.IsNewRow) continue;
                bool isEnabled = row.Cells["colIsEnabled"].Value != null && (bool)row.Cells["colIsEnabled"].Value;
                if (isEnabled)
                {
                    await TestConnectionCoreAsync(i);
                }
            }
        }

        /// <summary>
        /// 异步测试指定行的连接
        /// </summary>
        private async void TestConnectionAsync(int rowIndex)
        {
            await TestConnectionCoreAsync(rowIndex);
        }

        /// <summary>
        /// 核心连接测试逻辑
        /// </summary>
        private async Task TestConnectionCoreAsync(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvDatabases.Rows.Count) return;

            var row = dgvDatabases.Rows[rowIndex];
            string ip = row.Cells["colIP"].Value?.ToString() ?? "";
            string port = row.Cells["colPort"].Value?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(port))
            {
                SetConnectionStatus(row, "配置不完整", Color.Orange);
                return;
            }

            string url = $"{ip}:{port}";
            SetConnectionStatus(row, "测试中...", Color.FromArgb(100, 180, 255));

            try
            {
                bool success = await Task.Run(() =>
                {
                    try
                    {
                        var request = (HttpWebRequest)WebRequest.Create(url);
                        request.Method = "POST";
                        request.Timeout = TestTimeoutMs;
                        request.ContentType = "application/json";

                        // 发送一个简单的查询请求来测试连通性
                        string testJson = "{\"uniqueKey\":\"test\",\"db_name\":\"test\",\"operation\":\"get\",\"key\":\"__connection_test__\"}";
                        byte[] data = System.Text.Encoding.UTF8.GetBytes(testJson);
                        request.ContentLength = data.Length;

                        using (var reqStream = request.GetRequestStream())
                        {
                            reqStream.Write(data, 0, data.Length);
                        }

                        using (var response = (HttpWebResponse)request.GetResponse())
                        {
                            return response.StatusCode == HttpStatusCode.OK;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                });

                if (success)
                {
                    SetConnectionStatus(row, "已连接 ✓", Color.FromArgb(0, 200, 83));
                }
                else
                {
                    SetConnectionStatus(row, "连接失败 ✗", Color.FromArgb(255, 82, 82));
                    // 连接失败时自动禁用
                    row.Cells["colIsEnabled"].Value = false;
                }
            }
            catch (Exception ex)
            {
                SetConnectionStatus(row, "连接失败 ✗", Color.FromArgb(255, 82, 82));
                row.Cells["colIsEnabled"].Value = false;
                LogTextHelper.Error($"连接测试异常: {url} - {ex.Message}");
            }
        }

        /// <summary>
        /// 设置连接状态单元格的显示文本和颜色
        /// </summary>
        private void SetConnectionStatus(DataGridViewRow row, string status, Color color)
        {
            row.Cells["colConnectionStatus"].Value = status;
            row.Cells["colConnectionStatus"].Style.ForeColor = color;
            row.Cells["colConnectionStatus"].Style.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
        }

        #endregion
    }
}


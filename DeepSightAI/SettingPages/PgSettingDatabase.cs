using DeepSightCommunication;
using DeepSightDB;
using DeepSightModel;
using DeepSightModel.Configuration;
using DeepSightTool;
using DeepSightWorkLib.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    /// <summary>
    /// LevelDB 数据库配置界面
    /// </summary>
    public partial class PgSettingDatabase : Sunny.UI.UIPage
    {
        public PgSettingDatabase()
        {
            InitializeComponent();
            PageIndex = 3;
            ShowTitle = false;
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            // 为容器面板开启双缓冲，消除切换 UISwitch 等控件时的页面闪烁
            EnableDoubleBuffered(tlpMain, tlpRight, tlpBasic, tlpAvi, tlpVrs, tlpMinio, tlpListButtons);
            HookDetailEvents();
        }

        private static void EnableDoubleBuffered(params Control[] controls)
        {
            var prop = typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop == null) return;
            foreach (var c in controls)
            {
                if (c != null) prop.SetValue(c, true, null);
            }
        }

        /// <summary>
        /// 窗体实例对象（单例）
        /// </summary>
        private static PgSettingDatabase _instance;

        public static PgSettingDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PgSettingDatabase();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 连接测试超时时间（毫秒）
        /// </summary>
        private const int TestTimeoutMs = 3000;

        /// <summary>
        /// 内存中维护的配置列表（左侧列表的数据源；保存时写入文件）
        /// </summary>
        private readonly List<LevelDbConfig> _configs = new List<LevelDbConfig>();

        /// <summary>
        /// 加载/切换选项时屏蔽 TextChanged/CheckedChanged 写回
        /// </summary>
        private bool _isBinding = false;

        /// <summary>
        /// 当前选中的配置（null 表示无选择）
        /// </summary>
        private LevelDbConfig CurrentConfig =>
            (lstDatabases.SelectedIndex >= 0 && lstDatabases.SelectedIndex < _configs.Count)
                ? _configs[lstDatabases.SelectedIndex]
                : null;

        /// <summary>
        /// 给详情区控件挂载事件，用于实时把编辑写回 _configs
        /// </summary>
        private void HookDetailEvents()
        {
            txtDbName.TextChanged += (s, e) => WriteBack(c => c.DbName = txtDbName.Text);
            txtIp.TextChanged += (s, e) => WriteBack(c => { c.IP = txtIp.Text; ResetAviStatus(); ResetVrsStatus(); });
            txtPort.TextChanged += (s, e) => WriteBack(c => { c.Port = txtPort.Text; ResetAviStatus(); ResetVrsStatus(); });
            txtWriteBackDbName.TextChanged += (s, e) => WriteBack(c => c.WriteBackDbName = txtWriteBackDbName.Text);
            txtVrsWriteBackDbName.TextChanged += (s, e) => WriteBack(c => c.VRSWriteBackDbName = txtVrsWriteBackDbName.Text);
            txtVrsWriteBackDbNameV1.TextChanged += (s, e) => WriteBack(c => c.VRSWriteBackDbNameV1 = txtVrsWriteBackDbNameV1.Text);
            txtVrsHistoryDbName.TextChanged += (s, e) => WriteBack(c => { c.VrsHistoryDbName = txtVrsHistoryDbName.Text; ResetVrsStatus(); });
            chkIsEnabled.ValueChanged += (s, v) => WriteBack(c => c.IsEnabled = chkIsEnabled.Active);
            txtMinioIpA.TextChanged += (s, e) => WriteBack(c => { c.MinioIpA = txtMinioIpA.Text; ResetMinioStatus("A"); });
            txtMinioIpB.TextChanged += (s, e) => WriteBack(c => { c.MinioIpB = txtMinioIpB.Text; ResetMinioStatus("B"); });
        }

        /// <summary>
        /// 把详情区编辑写回当前配置；同时刷新列表显示文本
        /// </summary>
        private void WriteBack(Action<LevelDbConfig> setter)
        {
            if (_isBinding) return;
            var c = CurrentConfig;
            if (c == null) return;
            setter(c);
            RefreshListItem(lstDatabases.SelectedIndex);
        }

        private void FrLevelDbConfig_Load(object sender, EventArgs e)
        {
            LoadConfigToList();
            // 加载完成后仅对已启用的数据库自动测试
            TestEnabledConnectionsAsync();
        }

        /// <summary>
        /// 从配置管理器加载数据到左侧列表
        /// </summary>
        private void LoadConfigToList()
        {
            _isBinding = true;
            try
            {
                _configs.Clear();
                lstDatabases.Items.Clear();
                foreach (var db in LevelDbConfigManager.Instance.Databases)
                {
                    _configs.Add(db);
                    lstDatabases.Items.Add(BuildListText(db));
                }
                if (_configs.Count > 0)
                {
                    lstDatabases.SelectedIndex = 0;
                }
                else
                {
                    BindDetail(null);
                }
            }
            finally
            {
                _isBinding = false;
            }
        }

        /// <summary>
        /// 列表项的显示文本
        /// </summary>
        private static string BuildListText(LevelDbConfig db)
        {
            string mark = db.IsEnabled ? "●" : "○";
            return $" {mark}  ({db.IP}:{db.Port})";
        }

        /// <summary>
        /// 刷新指定索引列表项的显示文本（保留选择）
        /// </summary>
        private void RefreshListItem(int index)
        {
            if (index < 0 || index >= _configs.Count) return;
            lstDatabases.Items[index] = BuildListText(_configs[index]);
            lstDatabases.Invalidate();
        }

        /// <summary>
        /// 列表选择变化 -> 把选中配置绑定到详情区
        /// </summary>
        private void lstDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindDetail(CurrentConfig);
        }

        /// <summary>
        /// 把指定配置绑定到详情区控件（绑定期间屏蔽 TextChanged 写回）
        /// </summary>
        private void BindDetail(LevelDbConfig db)
        {
            _isBinding = true;
            try
            {
                bool has = db != null;
                grpBasic.Enabled = grpAvi.Enabled = grpVrs.Enabled = grpMinio.Enabled = has;
                if (!has)
                {
                    txtDbName.Text = txtIp.Text = txtPort.Text = txtWriteBackDbName.Text = "";
                    txtVrsWriteBackDbName.Text = txtVrsWriteBackDbNameV1.Text = "";
                    txtVrsHistoryDbName.Text = txtVrsTestSn.Text = "";
                    txtMinioIpA.Text = txtMinioIpB.Text = "";
                    chkIsEnabled.Active = false;
                    SetAviStatus("未测试", Color.FromArgb(216, 219, 188));
                    SetVrsStatus("未测试", Color.FromArgb(216, 219, 188));
                    SetMinioStatus("A", "未测试", Color.FromArgb(216, 219, 188));
                    SetMinioStatus("B", "未测试", Color.FromArgb(216, 219, 188));
                    return;
                }
                txtDbName.Text = db.DbName ?? "";
                txtIp.Text = db.IP ?? "";
                txtPort.Text = db.Port ?? "";
                txtWriteBackDbName.Text = db.WriteBackDbName ?? "";
                txtVrsWriteBackDbName.Text = db.VRSWriteBackDbName ?? "";
                txtVrsWriteBackDbNameV1.Text = db.VRSWriteBackDbNameV1 ?? "";
                txtVrsHistoryDbName.Text = string.IsNullOrWhiteSpace(db.VrsHistoryDbName)
                    ? VrsHistoryService.DefaultDbName : db.VrsHistoryDbName;
                txtVrsTestSn.Text = "";
                chkIsEnabled.Active = db.IsEnabled;
                txtMinioIpA.Text = db.MinioIpA ?? "";
                txtMinioIpB.Text = db.MinioIpB ?? "";
                SetAviStatus("未测试", Color.FromArgb(216, 219, 188));
                SetVrsStatus("未测试", Color.FromArgb(216, 219, 188));
                SetMinioStatus("A", "未测试", Color.FromArgb(216, 219, 188));
                SetMinioStatus("B", "未测试", Color.FromArgb(216, 219, 188));
            }
            finally
            {
                _isBinding = false;
            }
        }

        /// <summary>
        /// 添加新的数据库配置（追加到列表并选中）
        /// </summary>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            var db = new LevelDbConfig
            {
                DbName = "ai_merged_results",
                IP = "127.0.0.1",
                Port = "9877",
                WriteBackDbName = "filter_time_to_airesults",
                VRSWriteBackDbName = "ai_detail_results_tovrs",
                VRSWriteBackDbNameV1 = "ai_inference_result",
                VrsHistoryDbName = VrsHistoryService.DefaultDbName,
                EnableVRSWriteBackV1 = true,
                IsEnabled = false,
                MinioIpA = "127.0.0.1",
                MinioIpB = "127.0.0.1"
            };
            _configs.Add(db);
            lstDatabases.Items.Add(BuildListText(db));
            lstDatabases.SelectedIndex = _configs.Count - 1;
            txtDbName.Focus();
        }

        /// <summary>
        /// 删除选中的数据库配置
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            int idx = lstDatabases.SelectedIndex;
            if (idx < 0)
            {
                MessageBox.Show("请先选择要删除的数据库", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_configs.Count <= 1)
            {
                MessageBox.Show("至少保留一个数据库配置", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var result = MessageBox.Show($"确定要删除\"{_configs[idx].DbName}\"吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            _isBinding = true;
            try
            {
                _configs.RemoveAt(idx);
                lstDatabases.Items.RemoveAt(idx);
            }
            finally
            {
                _isBinding = false;
            }
            if (_configs.Count > 0)
                lstDatabases.SelectedIndex = Math.Min(idx, _configs.Count - 1);
            else
                BindDetail(null);
        }

        /// <summary>
        /// 把内存列表保存到配置文件
        /// </summary>
        public bool SaveConfig()
        {
            try
            {
                var configList = new LevelDbConfigList { Databases = new List<LevelDbConfig>(_configs) };
                return LevelDbConfigManager.Save(configList);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"保存 LevelDB 配置失败: {ex.Message}");
                return false;
            }
        }

        #region 连接测试 - 状态显示与重置

        private static readonly Color StatusIdleColor = Color.FromArgb(216, 219, 188);
        private static readonly Color StatusBusyColor = Color.FromArgb(100, 180, 255);
        private static readonly Color StatusOkColor = Color.FromArgb(0, 200, 83);
        private static readonly Color StatusFailColor = Color.FromArgb(255, 82, 82);
        private static readonly Color StatusWarnColor = Color.Orange;

        /// <summary>
        /// 设置 AVI 状态文字与颜色
        /// </summary>
        private void SetAviStatus(string status, Color color)
        {
            lblAviStatus.Text = "状态：" + status;
            lblAviStatus.ForeColor = color;
        }

        /// <summary>
        /// 设置 VRS 状态文字与颜色
        /// </summary>
        private void SetVrsStatus(string status, Color color)
        {
            lblVrsStatus.Text = "状态：" + status;
            lblVrsStatus.ForeColor = color;
        }

        /// <summary>
        /// 设置 MinIO 指定面的状态文字与颜色
        /// </summary>
        private void SetMinioStatus(string side, string status, Color color)
        {
            var lbl = side == "A" ? lblMinioStatusA : lblMinioStatusB;
            lbl.Text = "状态：" + status;
            lbl.ForeColor = color;
        }

        /// <summary>
        /// IP/端口变更后重置 AVI 状态并把当前数据库置为未启用（必须重新测试才能启用）
        /// </summary>
        private void ResetAviStatus()
        {
            SetAviStatus("未测试", StatusIdleColor);
            var c = CurrentConfig;
            if (c != null && c.IsEnabled)
            {
                c.IsEnabled = false;
                _isBinding = true;
                try { chkIsEnabled.Active = false; }
                finally { _isBinding = false; }
                RefreshListItem(lstDatabases.SelectedIndex);
            }
        }

        private void ResetVrsStatus()
        {
            SetVrsStatus("未测试", StatusIdleColor);
        }

        private void ResetMinioStatus(string side)
        {
            SetMinioStatus(side, "未测试", StatusIdleColor);
        }

        #endregion

        #region 连接测试 - 入口

        /// <summary>
        /// 当前选中数据库的 AVI 测试：查询最近 100 条数据并以表格形式展示
        /// </summary>
        private async void btnAviTest_Click(object sender, EventArgs e)
        {
            var c = CurrentConfig;
            if (c == null) return;
            await TestAviAndShowLatestAsync(c);
        }

        /// <summary>
        /// 当前选中数据库的 VRS 测试。
        /// 测试 SN 为空：仅做连接探测；
        /// 测试 SN 非空：发起一次按 SN 的 VRS 历史查询，弹窗显示原始 value 与解析结果。
        /// </summary>
        private async void btnVrsTest_Click(object sender, EventArgs e)
        {
            var c = CurrentConfig;
            if (c == null) return;
            string sn = txtVrsTestSn.Text?.Trim();
            if (string.IsNullOrEmpty(sn))
            {
                await TestVrsAsync(c);
            }
            else
            {
                await TestVrsBySnAsync(c, sn);
            }
        }

        private async void btnMinioTestA_Click(object sender, EventArgs e)
        {
            var c = CurrentConfig;
            if (c == null) return;
            await TestMinioAsync(c, "A");
        }

        private async void btnMinioTestB_Click(object sender, EventArgs e)
        {
            var c = CurrentConfig;
            if (c == null) return;
            await TestMinioAsync(c, "B");
        }

        /// <summary>
        /// 全部测试：AVI + VRS + MinIO A/B
        /// </summary>
        private async void btnTestAll_Click(object sender, EventArgs e)
        {
            var c = CurrentConfig;
            if (c == null) return;
            await TestAviAsync(c, autoDisableOnFail: false);
            await TestVrsAsync(c);
            await TestMinioAsync(c, "A");
            await TestMinioAsync(c, "B");
        }

        /// <summary>
        /// 加载完成后，对每个已启用的数据库自动测试 AVI 连接（失败时自动取消启用）
        /// </summary>
        private async void TestEnabledConnectionsAsync()
        {
            int saved = lstDatabases.SelectedIndex;
            for (int i = 0; i < _configs.Count; i++)
            {
                var c = _configs[i];
                if (!c.IsEnabled) continue;
                lstDatabases.SelectedIndex = i;
                await TestAviAsync(c, autoDisableOnFail: true);
            }
            if (saved >= 0 && saved < _configs.Count)
                lstDatabases.SelectedIndex = saved;
        }

        #endregion

        #region 连接测试 - 核心实现

        /// <summary>
        /// MinIO 默认端口
        /// </summary>
        private static readonly string MinioDefaultPort = MinioSettings.Instance.DefaultPort;

        /// <summary>
        /// 测试 AVI 连接（http://{IP}:{Port}），结果只更新当前选中行的状态显示
        /// </summary>
        private async Task TestAviAsync(LevelDbConfig c, bool autoDisableOnFail)
        {
            if (string.IsNullOrWhiteSpace(c.IP) || string.IsNullOrWhiteSpace(c.Port))
            {
                if (CurrentConfig == c) SetAviStatus("配置不完整", StatusWarnColor);
                return;
            }
            string url = $"http://{c.IP}:{c.Port}";
            if (CurrentConfig == c) SetAviStatus("测试中...", StatusBusyColor);

            bool success = await PostLevelDbProbeAsync(url);
            if (CurrentConfig == c)
                SetAviStatus(success ? "已连接 ✓" : "连接失败 ✗", success ? StatusOkColor : StatusFailColor);

            if (!success && autoDisableOnFail && c.IsEnabled)
            {
                c.IsEnabled = false;
                if (CurrentConfig == c)
                {
                    _isBinding = true;
                    try { chkIsEnabled.Active = false; }
                    finally { _isBinding = false; }
                }
                int idx = _configs.IndexOf(c);
                if (idx >= 0) RefreshListItem(idx);
            }
        }

        /// <summary>
        /// 测试 VRS 连接（VRS 复用 AVI 的 IP/Port，URL 取自 c.VRSUrl）
        /// </summary>
        private async Task TestVrsAsync(LevelDbConfig c)
        {
            if (string.IsNullOrWhiteSpace(c.IP) || string.IsNullOrWhiteSpace(c.Port))
            {
                if (CurrentConfig == c) SetVrsStatus("配置不完整", StatusWarnColor);
                return;
            }
            string url = c.VRSUrl;
            if (CurrentConfig == c) SetVrsStatus("测试中...", StatusBusyColor);

            bool success = await PostLevelDbProbeAsync(url);
            if (CurrentConfig == c)
                SetVrsStatus(success ? "已连接 ✓" : "连接失败 ✗", success ? StatusOkColor : StatusFailColor);
        }

        /// <summary>
        /// 按 SN 测试 VRS 历史查询：发起一次查询，弹窗显示原始 value 与解析结果
        /// </summary>
        private async Task TestVrsBySnAsync(LevelDbConfig c, string sn)
        {
            if (string.IsNullOrWhiteSpace(c.IP) || string.IsNullOrWhiteSpace(c.Port))
            {
                if (CurrentConfig == c) SetVrsStatus("配置不完整", StatusWarnColor);
                return;
            }
            string url = c.VRSUrl;
            string dbName = string.IsNullOrWhiteSpace(c.VrsHistoryDbName)
                ? VrsHistoryService.DefaultDbName : c.VrsHistoryDbName;

            if (CurrentConfig == c) SetVrsStatus("查询中...", StatusBusyColor);

            string rawValue = null;
            string error = null;
            bool success = await Task.Run(() =>
            {
                try
                {
                    var svc = new VrsHistoryService(new LevelDbHttpClient());
                    return svc.TryQuery(url, dbName, sn, out rawValue, out error);
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }
            });

            VrsHistoryResult parsed = null;
            string parseError = null;
            if (success && !string.IsNullOrEmpty(rawValue))
            {
                try
                {
                    parsed = VrsHistoryService.Parse(sn, rawValue);
                    if (parsed == null) parseError = "解析返回 null";
                }
                catch (Exception ex)
                {
                    parseError = ex.Message;
                }
            }

            if (CurrentConfig == c)
            {
                SetVrsStatus(success ? "查询完成" : "查询失败 ✗", success ? StatusOkColor : StatusFailColor);
            }

            ShowVrsTestResult(url, dbName, sn, success, error, rawValue, parsed, parseError);
        }

        /// <summary>
        /// 弹窗显示 VRS 历史查询结果（精简：仅显示连接、获取、解析三段简要状态）
        /// </summary>
        private static void ShowVrsTestResult(string url, string dbName, string sn, bool success,
            string error, string rawValue, VrsHistoryResult parsed, string parseError)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"URL: {url}");
            sb.AppendLine($"db_name: {dbName}    SN: {sn}");
            sb.AppendLine();
            if (!success)
            {
                sb.AppendLine($"获取失败：{error ?? "未知错误"}");
            }
            else
            {
                int rawLen = rawValue?.Length ?? 0;
                sb.AppendLine($"获取成功：value 长度 {rawLen}");
                if (parsed == null)
                {
                    sb.AppendLine($"解析失败：{parseError ?? "未知"}");
                }
                else
                {
                    sb.AppendLine($"解析成功：A 面 {parsed.AsideInfo.Count} 项, B 面 {parsed.BsideInfo.Count} 项, 报点 {parsed.Entries.Count} 条");
                }
            }

            MessageBox.Show(sb.ToString(), success ? "VRS 查询结果" : "VRS 查询失败",
                MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        /// <summary>
        /// 测试 MinIO 指定面的连接（GET /minio/health/live）
        /// </summary>
        private async Task TestMinioAsync(LevelDbConfig c, string side)
        {
            string ip = side == "A" ? c.MinioIpA : c.MinioIpB;
            if (string.IsNullOrWhiteSpace(ip))
            {
                if (CurrentConfig == c) SetMinioStatus(side, "未配置", Color.Gray);
                return;
            }
            if (CurrentConfig == c) SetMinioStatus(side, "测试中...", StatusBusyColor);

            string url = $"http://{ip}:{MinioDefaultPort}/minio/health/live";
            bool success = await Task.Run(() =>
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Timeout = TestTimeoutMs;
                    using (var response = (HttpWebResponse)request.GetResponse())
                        return response.StatusCode == HttpStatusCode.OK;
                }
                catch { return false; }
            });

            if (CurrentConfig == c)
                SetMinioStatus(side, success ? "已连接 ✓" : "连接失败 ✗", success ? StatusOkColor : StatusFailColor);
            if (!success)
                LogTextHelper.Error($"MinIO {side}面连接测试失败: {url}");
        }

        /// <summary>
        /// 向 LevelDB HTTP 服务发送一个查询探测请求，用于判断是否可达
        /// </summary>
        private static Task<bool> PostLevelDbProbeAsync(string url)
        {
            return Task.Run(() =>
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.Timeout = TestTimeoutMs;
                    request.ContentType = "application/json";
                    string testJson = "{\"uniqueKey\":\"test\",\"db_name\":\"test\",\"operation\":\"get\",\"key\":\"__connection_test__\"}";
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(testJson);
                    request.ContentLength = data.Length;
                    using (var reqStream = request.GetRequestStream())
                        reqStream.Write(data, 0, data.Length);
                    using (var response = (HttpWebResponse)request.GetResponse())
                        return response.StatusCode == HttpStatusCode.OK;
                }
                catch { return false; }
            });
        }

        #endregion

        #region AVI 最近数据查询与展示

        /// <summary>
        /// AVI 测试：查询最近 100 条数据并弹窗展示
        /// </summary>
        private async Task TestAviAndShowLatestAsync(LevelDbConfig c)
        {
            if (string.IsNullOrWhiteSpace(c.IP) || string.IsNullOrWhiteSpace(c.Port))
            {
                if (CurrentConfig == c) SetAviStatus("配置不完整", StatusWarnColor);
                return;
            }
            if (string.IsNullOrWhiteSpace(c.DbName))
            {
                if (CurrentConfig == c) SetAviStatus("数据库名为空", StatusWarnColor);
                return;
            }

            string url = c.Url;
            string dbName = c.DbName;
            if (CurrentConfig == c) SetAviStatus("查询中...", StatusBusyColor);

            string rawResp = null;
            string error = null;
            bool ok = await Task.Run(() =>
            {
                try
                {
                    var http = new LevelDbHttpClient();
                    var req = new RootDbInfo
                    {
                        uniqueKey = Guid.NewGuid().ToString(),
                        db_name = dbName,
                        operation = "get",
                        is_select_range = "true",
                        op_mode = "all",
                        range_start = DateTime.Now.AddDays(-7).ToString("yyyyMMddHHmmssfff"),
                        range_end = DateTime.Now.Date.AddDays(1).AddTicks(-1).ToString("yyyyMMddHHmmssfff"),
                    };
                    return http.PostJson(url, req, LevelDbOperation.Read, out rawResp, "AVI测试查询");
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }
            });

            if (!ok)
            {
                if (CurrentConfig == c) SetAviStatus("连接失败 ✗", StatusFailColor);
                MessageBox.Show("AVI 测试失败：" + (error ?? "请求失败"), "AVI 测试",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var rows = ParseLatestAviRows(rawResp, 100, out string parseErr);
            if (CurrentConfig == c)
            {
                if (parseErr != null) SetAviStatus("已连接 ✓ 解析告警", StatusWarnColor);
                else SetAviStatus($"已连接 ✓ 共 {rows.Count} 行", StatusOkColor);
            }
            ShowAviLatestDialog(url, dbName, rows, parseErr);
        }

        /// <summary>
        /// 解析 AVI 响应 JSON，按 Key（时间戳）倒序取前 N 条并展平为表格行
        /// </summary>
        private static List<AviTestRow> ParseLatestAviRows(string jsonInfo, int maxCount, out string error)
        {
            error = null;
            var list = new List<AviTestRow>();
            if (string.IsNullOrEmpty(jsonInfo)) return list;

            try
            {
                var preCheck = JObject.Parse(jsonInfo);
                var resultToken = preCheck["result"];
                if (resultToken != null && resultToken.Type == JTokenType.String)
                {
                    var resultStr = resultToken.Value<string>();
                    if (!string.IsNullOrEmpty(resultStr) && resultStr.StartsWith("err"))
                    {
                        error = $"LevelDB 错误响应：{resultStr}";
                        return list;
                    }
                }

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var response = JsonConvert.DeserializeObject<AviResponse>(jsonInfo, settings);
                if (response?.DataList == null || response.DataList.Count == 0) return list;

                var items = new List<AviDataItem>();
                foreach (var item in response.DataList)
                {
                    if (item is string strItem)
                    {
                        if (strItem == "end_range_send") continue;
                        var di = JsonConvert.DeserializeObject<AviDataItem>(strItem, settings);
                        if (di != null) items.Add(di);
                    }
                    else if (item is JObject jObj)
                    {
                        var di = jObj.ToObject<AviDataItem>();
                        if (di != null) items.Add(di);
                    }
                }

                var latest = items
                    .Where(x => !string.IsNullOrEmpty(x.Key))
                    .OrderByDescending(x => x.Key, StringComparer.Ordinal)
                    .Take(maxCount);

                foreach (var di in latest)
                {
                    string timeText = di.Key;
                    if (DateTime.TryParseExact(di.Key, "yyyyMMddHHmmssfff",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    {
                        timeText = dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    }

                    AviValueData val = null;
                    try { val = JsonConvert.DeserializeObject<AviValueData>(di.Value, settings); }
                    catch { }

                    if (val?.ResultsInfo != null && val.ResultsInfo.Count > 0)
                    {
                        foreach (var ri in val.ResultsInfo)
                        {
                            list.Add(new AviTestRow
                            {
                                Time = timeText,
                                SerialNumber = val.SerialNumber ?? "",
                                Side = ri?.Side ?? "",
                                MinioIp = ri?.MinioIp ?? "",
                                MinioPort = ri == null ? "" : ri.MinioPort.ToString(),
                                ResultPath = ri?.ResultPath ?? "",
                            });
                        }
                    }
                    else
                    {
                        list.Add(new AviTestRow
                        {
                            Time = timeText,
                            SerialNumber = val?.SerialNumber ?? "",
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                LogTextHelper.Error($"解析 AVI 测试响应失败: {ex}");
            }

            return list;
        }

        /// <summary>
        /// 弹窗展示 AVI 最近数据（DataGridView）
        /// </summary>
        private static void ShowAviLatestDialog(string url, string dbName, List<AviTestRow> rows, string parseError)
        {
            Color bg = Color.FromArgb(29, 48, 60);
            Color fg = Color.FromArgb(216, 219, 188);
            Color border = Color.FromArgb(60, 80, 95);
            Color header = Color.FromArgb(0, 64, 82);
            Color altRow = Color.FromArgb(35, 56, 70);

            using (var dlg = new Form
            {
                Text = $"AVI 测试 - {dbName} ({url}) - 共 {rows.Count} 行",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(1000, 560),
                BackColor = bg,
                ForeColor = fg,
                Font = new Font("微软雅黑", 9F),
                MinimizeBox = false,
                MaximizeBox = true,
                ShowIcon = false,
            })
            {
                var top = new Label
                {
                    Dock = DockStyle.Top,
                    Height = 28,
                    BackColor = header,
                    ForeColor = fg,
                    Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0),
                    Text = parseError != null
                        ? $"  解析告警：{parseError}"
                        : $"  最近 {rows.Count} 条记录（按时间倒序）"
                };

                var dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    BackgroundColor = bg,
                    BorderStyle = BorderStyle.None,
                    GridColor = border,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    ReadOnly = true,
                    RowHeadersVisible = false,
                    EnableHeadersVisualStyles = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    MultiSelect = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    Font = new Font("微软雅黑", 9F),
                    ColumnHeadersHeight = 30,
                };
                dgv.DefaultCellStyle.BackColor = bg;
                dgv.DefaultCellStyle.ForeColor = fg;
                dgv.DefaultCellStyle.SelectionBackColor = header;
                dgv.DefaultCellStyle.SelectionForeColor = fg;
                dgv.AlternatingRowsDefaultCellStyle.BackColor = altRow;
                dgv.AlternatingRowsDefaultCellStyle.ForeColor = fg;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = header;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = fg;
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9.5F, FontStyle.Bold);
                dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = header;
                dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

                dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Time", HeaderText = "时间", FillWeight = 18 });
                dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "SerialNumber", HeaderText = "SN", FillWeight = 18 });
                dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Side", HeaderText = "面", FillWeight = 5 });
                dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "MinioIp", HeaderText = "MinioIP", FillWeight = 14 });
                dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "MinioPort", HeaderText = "MinioPort", FillWeight = 8 });
                dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ResultPath", HeaderText = "结果路径", FillWeight = 37 });

                foreach (var r in rows)
                {
                    dgv.Rows.Add(r.Time, r.SerialNumber, r.Side, r.MinioIp, r.MinioPort, r.ResultPath);
                }

                dlg.Controls.Add(dgv);
                dlg.Controls.Add(top);
                dlg.ShowDialog();
            }
        }

        /// <summary>
        /// 用于在 DataGridView 中展示一条 AVI 数据
        /// </summary>
        private class AviTestRow
        {
            public string Time { get; set; }
            public string SerialNumber { get; set; }
            public string Side { get; set; }
            public string MinioIp { get; set; }
            public string MinioPort { get; set; }
            public string ResultPath { get; set; }
        }

        #endregion
    }
}


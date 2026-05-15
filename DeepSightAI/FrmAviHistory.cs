using DeepSightCommunication;
using DeepSightDB;
using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// AVI历史数据查看窗口：按日期范围和LevelDB配置查询历史数据，
    /// 标记是否已过滤，支持对未过滤数据重新发送推理请求。
    /// </summary>
    public partial class FrmAviHistory : Form
    {
        private const string TimeFormat = "yyyyMMddHHmmssfff";
        private readonly LevelDbHttpClient _http = new LevelDbHttpClient();
        private DataTable _dataTable;

        /// <summary>
        /// 缓存查询结果中每行对应的原始 AVI 数据项 (Key, Value JSON)
        /// </summary>
        private readonly List<(string key, string valueJson, LevelDbConfig config)> _rowCache
            = new List<(string, string, LevelDbConfig)>();

        public FrmAviHistory()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
        }

        #region 初始化

        private void FrmAviHistory_Load(object sender, EventArgs e)
        {
            dtpStart.Value = DateTime.Now.Date;                       // 当天 00:00:00
            dtpEnd.Value = DateTime.Now.Date.AddDays(1).AddSeconds(-1); // 当天 23:59:59

            LoadDbConfigs();
            InitDataTable();
            dgvHistory.ApplyDarkTheme();
            dgvHistory.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvHistory.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvHistory.DefaultCellStyle.Font = new Font("微软雅黑", 10F);
            dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
        }

        private void LoadDbConfigs()
        {
            cmbDbConfig.Items.Clear();
            var configs = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled).ToList();
            foreach (var c in configs)
                cmbDbConfig.Items.Add(c);
            cmbDbConfig.DisplayMember = "DisplayName";
            if (cmbDbConfig.Items.Count > 0)
                cmbDbConfig.SelectedIndex = 0;
        }

        private void InitDataTable()
        {
            _dataTable = new DataTable();
            _dataTable.Columns.Add("时间", typeof(string));
            _dataTable.Columns.Add("SN", typeof(string));
            _dataTable.Columns.Add("已过滤", typeof(string));
            _dataTable.Columns.Add("发送状态", typeof(string));
            dgvHistory.DataSource = _dataTable;
        }

        #endregion

        #region 查询

        private async void btnQuery_Click(object sender, EventArgs e)
        {
            if (cmbDbConfig.SelectedItem == null)
            {
                MessageBox.Show("请选择数据源", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var config = (LevelDbConfig)cmbDbConfig.SelectedItem;
            btnQuery.Enabled = false;
            lblStatus.Text = "正在查询AVI数据...";
            _dataTable.Rows.Clear();
            _rowCache.Clear();

            try
            {
                string rangeStart = dtpStart.Value.ToString(TimeFormat);
                string rangeEnd = dtpEnd.Value.ToString(TimeFormat);

                // 1. 读取 AVI 源数据 (DbName)
                var aviItems = await Task.Run(() => ReadRange(config.Url, config.DbName, rangeStart, rangeEnd));
                if (aviItems == null || aviItems.Count == 0)
                {
                    lblStatus.Text = "未查询到AVI数据";
                    btnQuery.Enabled = true;
                    return;
                }

                lblStatus.Text = $"读取到 {aviItems.Count} 条，正在逐条检查过滤状态...";

                // 2. 逐条用相同 key 查询 WriteBackDbName，判断是否已过滤
                var filteredKeys = await Task.Run(() =>
                    CheckFilteredByKey(config.Url, config.WriteBackDbName, aviItems));

                // 3. 填充表格
                FillDataTable(aviItems, filteredKeys, config);

                lblStatus.Text = $"共 {_dataTable.Rows.Count} 条记录，" +
                    $"已过滤: {_dataTable.AsEnumerable().Count(r => r["已过滤"].ToString() == "是")}，" +
                    $"未过滤: {_dataTable.AsEnumerable().Count(r => r["已过滤"].ToString() == "否")}";
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("查询AVI历史数据失败", ex);
                lblStatus.Text = $"查询失败: {ex.Message}";
            }
            finally
            {
                btnQuery.Enabled = true;
            }
        }

        /// <summary>
        /// 从 LevelDB 按时间范围读取数据，返回 AviDataItem 列表
        /// </summary>
        private List<AviDataItem> ReadRange(string url, string dbName, string rangeStart, string rangeEnd)
        {
            var req = new RootDbInfo
            {
                uniqueKey = Guid.NewGuid().ToString(),
                db_name = dbName,
                operation = "get",
                is_select_range = "true",
                op_mode = "all",
                range_start = rangeStart,
                range_end = rangeEnd,
            };

            if (!_http.PostJson(url, req, LevelDbOperation.Read, out string resp, "AVI历史查询"))
                return null;

            return ParseAviDataItems(resp);
        }

        /// <summary>
        /// 逐条用相同 key 查询 WriteBackDbName，能查到 value 即为已过滤
        /// </summary>
        private HashSet<string> CheckFilteredByKey(string url, string writeBackDbName, List<AviDataItem> aviItems)
        {
            var filtered = new HashSet<string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(writeBackDbName) || aviItems == null) return filtered;

            // 收集所有不重复的 key
            var distinctKeys = aviItems
                .Where(i => !string.IsNullOrEmpty(i.Key))
                .Select(i => i.Key)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            foreach (var key in distinctKeys)
            {
                try
                {
                    var req = new RootDbInfo
                    {
                        uniqueKey = Guid.NewGuid().ToString(),
                        db_name = writeBackDbName,
                        operation = "get",
                        key = key
                    };

                    if (_http.PostJson(url, req, LevelDbOperation.Read, out string resp, "过滤状态查询"))
                    {
                        // 检查响应中是否有实际的 value
                        if (!string.IsNullOrEmpty(resp))
                        {
                            try
                            {
                                var root = JObject.Parse(resp);
                                var valueToken = root["value"];
                                if (valueToken != null && valueToken.Type != JTokenType.Null)
                                {
                                    string val = valueToken.Type == JTokenType.String
                                        ? valueToken.Value<string>()
                                        : valueToken.ToString(Formatting.None);
                                    if (!string.IsNullOrWhiteSpace(val))
                                        filtered.Add(key);
                                }
                            }
                            catch { /* 解析失败视为未过滤 */ }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Warn($"检查过滤状态失败: key={key}, 错误: {ex.Message}");
                }
            }
            return filtered;
        }

        /// <summary>
        /// 解析 AVI 响应 JSON 为 AviDataItem 列表
        /// </summary>
        private List<AviDataItem> ParseAviDataItems(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;

            try
            {
                var preCheck = JObject.Parse(json);
                var resultToken = preCheck["result"];
                if (resultToken != null && resultToken.Type == JTokenType.String)
                {
                    var s = resultToken.Value<string>();
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("err"))
                        return null;
                }
            }
            catch { }

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var response = JsonConvert.DeserializeObject<AviResponse>(json, settings);
            if (response?.DataList == null || response.DataList.Count == 0)
                return null;

            var result = new List<AviDataItem>();
            foreach (var item in response.DataList)
            {
                try
                {
                    if (item is string strItem)
                    {
                        if (strItem == "end_range_send") continue;
                        var dataItem = JsonConvert.DeserializeObject<AviDataItem>(strItem, settings);
                        if (dataItem != null) result.Add(dataItem);
                    }
                    else if (item is JObject jObj)
                    {
                        var dataItem = jObj.ToObject<AviDataItem>();
                        if (dataItem != null) result.Add(dataItem);
                    }
                }
                catch { }
            }
            return result;
        }

        /// <summary>
        /// 将 AVI 数据填充到 DataTable，每条 AviDataItem 提取 SN 生成一行
        /// </summary>
        private void FillDataTable(List<AviDataItem> aviItems, HashSet<string> filteredKeys, LevelDbConfig config)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            foreach (var item in aviItems)
            {
                if (string.IsNullOrEmpty(item.Key) || string.IsNullOrEmpty(item.Value))
                    continue;

                AviValueData valueData;
                try
                {
                    valueData = JsonConvert.DeserializeObject<AviValueData>(item.Value, settings);
                }
                catch { continue; }

                string sn = valueData?.SerialNumber ?? "-";
                bool isFiltered = filteredKeys.Contains(item.Key);

                // 格式化时间显示
                string timeStr = item.Key;
                if (DateTime.TryParseExact(item.Key, TimeFormat,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime dt))
                {
                    timeStr = dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
                }

                _dataTable.Rows.Add(timeStr, sn, isFiltered ? "是" : "否", "");
                _rowCache.Add((item.Key, item.Value, config));
            }

            // 标记颜色
            for (int i = 0; i < dgvHistory.Rows.Count; i++)
            {
                var row = dgvHistory.Rows[i];
                if (row.Cells["已过滤"].Value?.ToString() == "是")
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(100, 255, 100);
                else
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(255, 180, 100);
            }
        }

        #endregion

        #region 重发推理请求

        private async void btnResend_Click(object sender, EventArgs e)
        {
            if (dgvHistory.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要重发的行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 收集未过滤的选中行（记录行索引用于更新发送状态列）
            var toResend = new List<(int rowIndex, string key, string valueJson, LevelDbConfig config)>();
            foreach (DataGridViewRow row in dgvHistory.SelectedRows)
            {
                int idx = row.Index;
                if (idx < 0 || idx >= _rowCache.Count) continue;
                if (row.Cells["已过滤"].Value?.ToString() == "是") continue;
                var cached = _rowCache[idx];
                toResend.Add((idx, cached.key, cached.valueJson, cached.config));
            }

            if (toResend.Count == 0)
            {
                MessageBox.Show("选中的数据均已过滤，无需重发", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 按行索引排序，从上到下依次发送
            toResend.Sort((a, b) => a.rowIndex.CompareTo(b.rowIndex));

            var confirm = MessageBox.Show(
                $"确定要对 {toResend.Count} 条未过滤数据重新发送推理请求？\n（每条间隔1秒发送）",
                "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            btnResend.Enabled = false;
            btnQuery.Enabled = false;
            int success = 0, failed = 0;
            lblStatus.Text = "正在重发推理请求...";

            // 先清空选中行的发送状态
            foreach (var item in toResend)
                _dataTable.Rows[item.rowIndex]["发送状态"] = "等待发送";

            try
            {
                // 去重：同一 key + config 只发一次
                var distinct = toResend
                    .GroupBy(x => new { x.key, DbName = x.config.DbName })
                    .Select(g => g.First())
                    .ToList();

                for (int i = 0; i < distinct.Count; i++)
                {
                    var item = distinct[i];
                    _dataTable.Rows[item.rowIndex]["发送状态"] = "发送中...";
                    dgvHistory.InvalidateRow(item.rowIndex);

                    try
                    {
                        bool ok = await Task.Run(() =>
                        {
                            var dbInfo = new RootDbInfo
                            {
                                uniqueKey = Guid.NewGuid().ToString(),
                                db_name = item.config.DbName,
                                operation = "put",
                                op_mode = "all_ow",
                                key = DateTime.Now.ToString(TimeFormat),
                                value = item.valueJson
                            };
                            return _http.PostJson(item.config.Url, dbInfo, LevelDbOperation.Write, out _, "AVI历史重发");
                        });

                        if (ok)
                        {
                            success++;
                            _dataTable.Rows[item.rowIndex]["发送状态"] = "✔ 成功";
                            dgvHistory.Rows[item.rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(100, 255, 100);
                        }
                        else
                        {
                            failed++;
                            _dataTable.Rows[item.rowIndex]["发送状态"] = "✘ 失败";
                            dgvHistory.Rows[item.rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(255, 100, 100);
                        }
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        _dataTable.Rows[item.rowIndex]["发送状态"] = "✘ 异常";
                        dgvHistory.Rows[item.rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(255, 100, 100);
                        LogTextHelper.Error($"重发推理请求异常: Key={item.key}, 错误: {ex.Message}");
                    }

                    dgvHistory.InvalidateRow(item.rowIndex);
                    lblStatus.Text = $"重发中... 成功:{success} 失败:{failed} / 共{distinct.Count}（第{i + 1}条）";

                    // 间隔1秒发送下一条（最后一条不等待）
                    if (i < distinct.Count - 1)
                        await Task.Delay(1000);
                }

                lblStatus.Text = $"重发完成！成功:{success} 失败:{failed} / 共{distinct.Count}";
                MessageBox.Show($"重发完成！\n成功: {success}\n失败: {failed}",
                    "结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("重发推理请求失败", ex);
                lblStatus.Text = $"重发失败: {ex.Message}";
            }
            finally
            {
                btnResend.Enabled = true;
                btnQuery.Enabled = true;
            }
        }

        #endregion
    }
}

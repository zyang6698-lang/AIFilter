using DeepSightEvent;
using DeepSightModel.Alarm;
using DeepSightTool;
using DeepSightWorkLib.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class FrmAlarm : Form
    {
        private string logFilePath = null;

        private static readonly Dictionary<AlarmLevel, Color> LevelColorMap = new Dictionary<AlarmLevel, Color>
        {
            { AlarmLevel.Info,     Color.FromArgb(24, 144, 255) },
            { AlarmLevel.Warning,  Color.FromArgb(250, 219, 20) },
            { AlarmLevel.Error,    Color.FromArgb(250, 173, 20) },
            { AlarmLevel.Critical, Color.FromArgb(255, 77, 79) },
        };

        private static readonly Dictionary<AlarmLevel, string> LevelDisplayMap = new Dictionary<AlarmLevel, string>
        {
            { AlarmLevel.Info, "提示" }, { AlarmLevel.Warning, "警告" },
            { AlarmLevel.Error, "错误" }, { AlarmLevel.Critical, "严重" },
        };

        private static readonly Dictionary<AlarmCategory, string> CategoryDisplayMap = new Dictionary<AlarmCategory, string>
        {
            { AlarmCategory.System, "系统" }, { AlarmCategory.AI, "AI" },
            { AlarmCategory.Communication, "通信" }, { AlarmCategory.Defect, "缺陷" },
            { AlarmCategory.Hardware, "硬件" },
        };

        /// <summary>等级文本→AlarmLevel 反查（用于过滤）</summary>
        private static readonly Dictionary<string, AlarmLevel> LevelReverseMap = new Dictionary<string, AlarmLevel>
        {
            { "提示", AlarmLevel.Info }, { "警告", AlarmLevel.Warning },
            { "错误", AlarmLevel.Error }, { "严重", AlarmLevel.Critical },
        };

        /// <summary>当前等级过滤值（null 表示全部）</summary>
        private string _currentLevelFilter = null;

        public FrmAlarm()
        {
            InitializeComponent();
            dataGridViewData.ApplyDarkTheme();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            this.dataGridViewData.AutoGenerateColumns = false;
            this.dataGridViewData.CellDoubleClick += DataGridView_CellDoubleClick;
            AlarmService.Instance.OnAlarmRaised += OnStructuredAlarmReceived;
        }

        private void OnStructuredAlarmReceived(AlarmInfo alarm)
        {
            try
            {
                this.dataGridViewData.Invoke(new MethodInvoker(() =>
                {
                    string levelText = LevelDisplayMap.ContainsKey(alarm.Level)
                        ? LevelDisplayMap[alarm.Level] : alarm.Level.ToString();
                    string categoryText = CategoryDisplayMap.ContainsKey(alarm.Category)
                        ? CategoryDisplayMap[alarm.Category] : alarm.Category.ToString();

                    int rowIndex = this.dataGridViewData.Rows.Add(
                        levelText,
                        alarm.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                        categoryText, alarm.Source ?? "",
                        alarm.Message ?? "",
                        alarm.OccurrenceCount.ToString());

                    if (LevelColorMap.TryGetValue(alarm.Level, out Color color))
                    {
                        var row = this.dataGridViewData.Rows[rowIndex];
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.Style.ForeColor = color;
                            cell.Style.SelectionForeColor = color;
                        }
                    }

                    this.dataGridViewData.Rows[rowIndex].Tag = alarm.Id;

                    // 根据当前过滤条件控制行可见性
                    if (_currentLevelFilter != null)
                    {
                        this.dataGridViewData.Rows[rowIndex].Visible =
                            (levelText == _currentLevelFilter);
                    }

                    UpdateStatLabels();
                }));

                WriteAlarmToFile(alarm);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"[FrmAlarm] OnStructuredAlarmReceived error: {ex.Message}");
            }
        }

        private void WriteAlarmToFile(AlarmInfo alarm)
        {
            try
            {
                string logDir = Path.Combine(Application.StartupPath, "ExceptionLog",
                    DateTime.Now.ToString("yyyy-MM-dd"));
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                logFilePath = Path.Combine(logDir, "exception_log.csv");
                using (var w = new StreamWriter(logFilePath, true, Encoding.Default))
                {
                    w.WriteLine($"{alarm.Timestamp:yyyy-MM-dd HH:mm:ss.fff},{alarm.Level},{alarm.Category},{alarm.Source},{alarm.Message}");
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"[FrmAlarm] WriteAlarmToFile error: {ex.Message}");
            }
        }

        private void UpdateStatLabels()
        {
            var s = AlarmService.Instance.GetTodayStatistics();
            lblCriticalCount.Text = $"严重: {s.CriticalCount}";
            lblErrorCount.Text = $"错误: {s.ErrorCount}";
            lblWarningCount.Text = $"警告: {s.WarningCount}";
            lblInfoCount.Text = $"提示: {s.InfoCount}";
        }

        private static FrmAlarm _instance;
        public static FrmAlarm Instance
        {
            get
            {
                if (_instance == null) _instance = new FrmAlarm();
                return _instance;
            }
        }

        private void FrAlarm_Load(object sender, EventArgs e)
        {
            BindDataGrid();
            cboLevelFilter.SelectedIndex = 0; // "全部"
        }

        private void BindDataGrid() { }

        #region 等级过滤

        private void cboLevelFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = cboLevelFilter.SelectedItem?.ToString();
            _currentLevelFilter = (selected == "全部") ? null : selected;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            foreach (DataGridViewRow row in dataGridViewData.Rows)
            {
                if (_currentLevelFilter == null)
                {
                    row.Visible = true;
                }
                else
                {
                    string levelText = row.Cells[0]?.Value?.ToString() ?? "";
                    row.Visible = (levelText == _currentLevelFilter);
                }
            }
        }

        #endregion

        #region 双击确认告警

        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dataGridViewData.Rows[e.RowIndex];
            string alarmId = row.Tag as string;
            if (string.IsNullOrEmpty(alarmId)) return;

            AlarmService.Instance.AcknowledgeAlarm(alarmId);

            // 将行变灰，表示已确认
            var ackColor = Color.FromArgb(100, 120, 130);
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Style.ForeColor = ackColor;
                cell.Style.SelectionForeColor = ackColor;
            }
        }

        #endregion

        #region 测试告警

        /// <summary>
        /// 依次触发三种 AI 引擎告警，用于验证告警链路（Toast/表格/文件）
        /// </summary>
        private void btnTestAlarm_Click(object sender, EventArgs e)
        {
            AiEngineAlarm.TestFireAll();
        }

        #endregion

        #region 导出 CSV

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV 文件|*.csv";
                sfd.FileName = $"alarm_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    using (var w = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        // 表头
                        w.WriteLine("等级,时间,分类,来源,告警信息,次数");
                        foreach (DataGridViewRow row in dataGridViewData.Rows)
                        {
                            if (!row.Visible) continue;
                            var cells = new string[row.Cells.Count];
                            for (int i = 0; i < row.Cells.Count; i++)
                            {
                                cells[i] = row.Cells[i].Value?.ToString() ?? "";
                            }
                            w.WriteLine(string.Join(",", cells));
                        }
                    }
                    MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion
    }
}

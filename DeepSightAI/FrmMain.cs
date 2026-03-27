using DeepSightAI.Properties;
using DeepSightDB;
using DeepSightEvent;
using DeepSightModel;
using DeepSightModel.Configuration;
using DeepSightTool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class FrmMain : Form
    {
        public bool IsAllow = false;

        private DateTime _lastResetDate = DateTime.Now.Date;

        #region UI 批量刷新 — 缓冲区 & Timer

        /// <summary>任务状态待处理队列（后台线程写入，Timer 读取）</summary>
        private readonly ConcurrentQueue<TaskStatusInfo> _pendingTaskStatus = new ConcurrentQueue<TaskStatusInfo>();

        /// <summary>AVI 列待更新字典（key=sn_side, 最新计数覆盖旧值）</summary>
        private readonly ConcurrentDictionary<string, (string sn, string side, int count)> _pendingAviUpdates
            = new ConcurrentDictionary<string, (string sn, string side, int count)>();

        /// <summary>AI 列待更新字典</summary>
        private readonly ConcurrentDictionary<string, (string sn, string side, int count)> _pendingAiUpdates
            = new ConcurrentDictionary<string, (string sn, string side, int count)>();

        /// <summary>待刷新的图片 SN_Side（仅保留最新一条，通过 Interlocked 访问）</summary>
        private string _pendingImageRefreshKey;

        /// <summary>待更新的工站数据接收时间（工站名 → 最新接收）</summary>
        private readonly ConcurrentQueue<string> _pendingStationUpdates = new ConcurrentQueue<string>();

        /// <summary>是否需要刷新工站配置列表（新工站发现时设置）</summary>
        private volatile bool _machineConfigDirty;

        /// <summary>UI 刷新定时器（200ms 间隔，WinForms.Timer 天然在 UI 线程触发）</summary>
        private System.Windows.Forms.Timer _uiRefreshTimer;

        #endregion

        /// <summary>
        /// 窗体对象实例
        /// </summary>
        private static FrmMain _instance;

        public static FrmMain Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrmMain();
                }

                return _instance;
            }
        }

        #region 页面切换配置

        /// <summary>
        /// FormMode → Panel 映射（字典驱动，替代 switch 中的 panel 可见性切换）
        /// </summary>
        private Dictionary<FormMode, Panel> _pagePanels;

        /// <summary>
        /// FormMode → (按钮, 选中图, 未选中图) 映射（字典驱动，替代 SwitchButton 中的 switch）
        /// </summary>
        private Dictionary<FormMode, (PictureBox btn, Image activeImg, Image inactiveImg)> _buttonConfigs;

        /// <summary>
        /// 初始化页面切换所需的字典映射
        /// </summary>
        private void InitPageSwitchConfig()
        {
            _pagePanels = new Dictionary<FormMode, Panel>
            {
                { FormMode.MainForm,    panel1 },
                { FormMode.SettingForm, panel2 },
                { FormMode.SearchForm,  panel3 },
                { FormMode.AlarmForm,   panel4 },
                { FormMode.ChartForm,   panel5 },
            };

            _buttonConfigs = new Dictionary<FormMode, (PictureBox, Image, Image)>
            {
                { FormMode.MainForm,    (btnHome,   Resources.home2,   Resources.home1)   },
                { FormMode.SettingForm, (btnTool,   Resources.tool2,   Resources.tool1)   },
                { FormMode.AlarmForm,   (btnAlarm,  Resources.alarm2,  Resources.alarm1)  },
                { FormMode.ChartForm,   (btnChart,  Resources.chart2,  Resources.chart1)  },
                { FormMode.SearchForm,  (btnSearch, Resources.search2, Resources.search1) },
            };
        }

        #endregion

        public FrmMain()
        {
            InitializeComponent();

            // 注意：不再禁用跨线程检查，改用 InvokeOnUI 方法安全地访问 UI
            // Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            btnMax.BackgroundImage = Resources.min;
            this.Load += FrmMain_Load;
            this.FormClosing += FrMain_FormClosing;

            InitPageSwitchConfig();

            SystemEvent.EventSendTaskStatusToUI += new SendTaskStatus(SystemEvent_EventSendTaskStatusToUI);
            SystemEvent.EventSendAlarmToUI += new SendAlarm(SystemEvent_EventSendAlarmToUI);
            SystemEvent.EventSendDefectPanelInfoToUI += new SendDefectPanelInfo(SystemEvent_EventSendDefectPanelInfoToUI);
            SystemEvent.EventSendDefectResultInfoToUI += new SendDefectResultInfo(SystemEvent_EventSendDefectResultInfoToUI);
            SystemEvent.EventSendDefectRoiInfoToUI += new SendDefectRoiInfo(SystemEvent_EventSendDefectRoiInfoToUI);
            SystemEvent.EventSendDefectDetectInfoToUI += new SendDefectDetectInfo(SystemEvent_EventSendDefectDetectInfoToUI);
        }



        private void SystemEvent_EventSendDefectResultInfoToUI(string sn, string side, List<string> msg)
        {
            try
            {
                // 以SN+Side为单位存储结果（ConcurrentDictionary，线程安全）
                string key = $"{sn}_{side}";
                var resultList = FrHome.Instance.dic_Results.GetOrAdd(key, _ => new List<string>());
                int resultCount;
                lock (resultList)
                {
                    resultList.AddRange(msg);
                    resultCount = resultList.Count;
                }

                // 入队：AI 列待更新（Timer Tick 中批量刷新）
                _pendingAiUpdates[key] = (sn, side, resultCount);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("结果回调异常" + ex.ToString());
                SystemEvent.SendAlarmMsg("结果回调异常" + ex.ToString());
            }
        }

        private void SystemEvent_EventSendDefectRoiInfoToUI(string sn, string side, List<Roi> rois)
        {
            try
            {
                // 以SN+Side为单位存储ROI
                string key = $"{sn}_{side}";
                var roiList = FrHome.Instance.dic_DetectRois.GetOrAdd(key, _ => new List<Roi>());
                lock (roiList)
                {
                    roiList.AddRange(rois);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("ROI回调异常" + ex.ToString());
            }
        }

        private void SystemEvent_EventSendDefectDetectInfoToUI(string sn, string side, List<DetectInfo> detectInfos)
        {
            try
            {
                // 以SN+Side为单位存储DetectInfo（用于图片放大和单图测试）
                string key = $"{sn}_{side}";
                var infoList = FrHome.Instance.dic_DetectInfos.GetOrAdd(key, _ => new List<DetectInfo>());
                lock (infoList)
                {
                    infoList.AddRange(detectInfos);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("DetectInfo回调异常" + ex.ToString());
            }
        }

        private void SystemEvent_EventSendDefectPanelInfoToUI(string sn, string side, RootPanelInfoWithIP info)
        {
            try
            {
                // ── 数据存储（线程安全，可在后台线程执行） ──
                string key = $"{sn}_{side}";
                var infoList = FrHome.Instance.dic_Infos.GetOrAdd(key, _ => new List<RootPanelInfoWithIP>());
                lock (infoList)
                {
                    infoList.Add(info);
                }

                // 计算 AVI 计数并入队（Timer 中刷新 DataGridView）
                int aviCount = 0;
                lock (infoList)
                {
                    foreach (var inf in infoList)
                    {
                        if (inf?.RootInfo?.PcsInfo != null)
                        {
                            foreach (var pcs in inf.RootInfo.PcsInfo.Values)
                            {
                                aviCount += pcs.DefectInfo?.Count ?? 0;
                            }
                        }
                    }
                }
                _pendingAviUpdates[key] = (sn, side, aviCount);

                // ── 机台注册（配置写入，非 UI 操作，可在后台线程执行） ──
                string machineName = info?.RootInfo?.MachineName;
                if (!string.IsNullOrEmpty(machineName))
                {
                    bool registryExists = Machine.machineRegistry?.Machines?.Any(m => m.MachineName == machineName) ?? false;

                    if (!registryExists)
                    {
                        var newEntry = new MachineEntry
                        {
                            MachineName = machineName,
                            IsEnable = true,
                            DataSourceType = DataSourceType.LevelDb
                        };
                        Machine.machineRegistryManager.AddOrUpdate(newEntry);
                        Machine.machineRegistryManager.Read(out Machine.machineRegistry);

                        if (Machine.aviconfig?.WatchPaths != null)
                        {
                            bool aviExists = Machine.aviconfig.WatchPaths.Any(w => w.AviName == machineName);
                            if (!aviExists)
                            {
                                WatchPathConfig newStation = new WatchPathConfig()
                                {
                                    AviName = machineName,
                                    APath = "",
                                    BPath = "",
                                    Depth = 4,
                                    FileA = "",
                                    FileB = "",
                                    IsEnable = true,
                                };
                                Machine.aviconfig.WatchPaths.Add(newStation);
                                Machine.avi_class.Save(Machine.aviconfig);
                            }
                        }

                        // 标记：需要在 UI 线程刷新工站列表
                        _machineConfigDirty = true;
                        LogTextHelper.Info($"自动发现并添加新工站: {machineName}");
                    }

                    // 入队：工站数据接收时间更新（Timer 中刷新 UI）
                    _pendingStationUpdates.Enqueue(machineName);
                }

                // ── 图片刷新标记（仅当有缺陷图片时） ──
                bool hasImages = info?.RootInfo?.PcsInfo?.Values?.Any(pcs =>
                    pcs.DefectInfo?.Any(d => d.DefectVrsImages != null && d.DefectVrsImages.Count > 0) == true) == true;
                if (hasImages)
                {
                    // 原子写入：多次写入只保留最新的 key，Timer 中取最后值
                    Interlocked.Exchange(ref _pendingImageRefreshKey, key);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Panel回调异常" + ex.ToString());
                SystemEvent.SendAlarmMsg("Panel回调异常" + ex.ToString());
            }
        }


        private void FrmMain_Load(object sender, EventArgs e)
        {
            LoadMethod();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 1000;
            timer.Start();
            Machine.master.IsShowBox = this.btn_showBox.Checked;
            string filePath = Assembly.GetExecutingAssembly().Location;
            DateTime lastWriteTime = File.GetLastWriteTime(filePath);
            this.lbl_title.Text = "AI过滤软件 ~ " + lastWriteTime.ToString("MMdd");

            // 启动 UI 批量刷新定时器（200ms ≈ 5FPS，足够流畅且不卡顿）
            _uiRefreshTimer = new System.Windows.Forms.Timer();
            _uiRefreshTimer.Interval = 200;
            _uiRefreshTimer.Tick += UiRefreshTimer_Tick;
            _uiRefreshTimer.Start();
        }

        private void SystemEvent_EventSendAlarmToUI(string massage)
        {
            LogTextHelper.Error($"收到异常消息：{massage},任务已停止");
        }
        public static object Locker = new object();
        
        /// <summary>
        /// 新的任务状态事件处理（推荐使用）
        /// </summary>
        private void SystemEvent_EventSendTaskStatusToUI(TaskStatusInfo statusInfo)
        {
            if (statusInfo == null) return;
            // 入队即返回，不阻塞后台线程，Timer Tick 中批量消费
            _pendingTaskStatus.Enqueue(statusInfo);
        }
        


        /// <summary>
        /// 添加新任务行（区分AB面）
        /// 新排队任务始终插入到最顶部
        /// </summary>
        private void AddNewTaskRow(string sn, string side)
        {
            var dgv = FrHome.Instance.dataGridViewData;

            // 列顺序: SN[0], Side[1], AVI[2], AI[3], Time[4], Status[5]
            dgv.Rows.Insert(0, new object[] { sn, side, "0", "0", "", "排队中" });
            dgv.Rows[0].DefaultCellStyle.ForeColor = Color.Yellow;
        }

        /// <summary>
        /// 更新指定行的 AVI 列（仅在 UI 线程 Timer Tick 中调用）
        /// </summary>
        private void UpdateAVIColumnForRow(string sn, string side, int aviCount)
        {
            var row = FindRowBySnSide(sn, side) ?? FindRowBySn(sn);
            if (row != null) row.Cells[2].Value = aviCount;
        }

        /// <summary>
        /// 更新指定行的 AI 列（仅在 UI 线程 Timer Tick 中调用）
        /// </summary>
        private void UpdateAIColumnForRow(string sn, string side, int aiCount)
        {
            var row = FindRowBySnSide(sn, side) ?? FindRowBySn(sn);
            if (row != null) row.Cells[3].Value = aiCount;
        }

        /// <summary>
        /// 根据SN和Side查找行
        /// </summary>
        private DataGridViewRow FindRowBySnSide(string sn, string side)
        {
            foreach (DataGridViewRow row in FrHome.Instance.dataGridViewData.Rows)
            {
                if (row.Cells[0].Value?.ToString() == sn && row.Cells[1].Value?.ToString() == side)
                {
                    return row;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据SN查找行（不区分面，用于旧接口兼容）
        /// </summary>
        private DataGridViewRow FindRowBySn(string sn)
        {
            foreach (DataGridViewRow row in FrHome.Instance.dataGridViewData.Rows)
            {
                if (row.Cells[0].Value?.ToString() == sn)
                {
                    return row;
                }
            }
            return null;
        }

        /// <summary>
        /// 更新任务行状态（使用结构化状态）
        /// </summary>
        private void UpdateTaskRowWithStatus(TaskStatusInfo statusInfo)
        {
            // 按SN+Side精确查找行
            DataGridViewRow targetRow = FindRowBySnSide(statusInfo.SerialNumber, statusInfo.Side);

            // 兼容：如果找不到精确匹配，尝试按SN查找（旧数据兼容）
            if (targetRow == null)
            {
                targetRow = FindRowBySn(statusInfo.SerialNumber);
            }

            if (targetRow == null) return;

            // 设置颜色
            Color statusColor = TaskStatusHelper.GetStatusColor(statusInfo.Status);
            targetRow.DefaultCellStyle.ForeColor = statusColor;

            // 更新时间列[4]（如果有传入时间）
            if (statusInfo.ProcessingTimeMs > 0)
            {
                string existingTime = targetRow.Cells[4].Value?.ToString();
                if (!string.IsNullOrEmpty(existingTime))
                {
                    targetRow.Cells[4].Value = $"{existingTime}+{statusInfo.ProcessingTimeMs}";
                }
                else
                {
                    targetRow.Cells[4].Value = statusInfo.ProcessingTimeMs.ToString();
                }
            }

            // 处理 B 面完成的特殊逻辑
            if (statusInfo.Status == DeepSightModel.TaskStatus.Completed && statusInfo.Side == "B")
            {
                string displayMsg = statusInfo.GetFullDisplayMessage();
                UpdateBSideCompletedRow(targetRow, statusInfo.SerialNumber, ref displayMsg);
            }
            else
            {
                targetRow.Cells[5].Value = statusInfo.GetFullDisplayMessage();
            }
        }

        /// <summary>
        /// 更新 B 面完成的行数据
        /// </summary>
        private void UpdateBSideCompletedRow(DataGridViewRow row, string sn, ref string msg)
        {
            int count = 0;
            int ok = 0;
            int ng = 0;
            int byPass = 0;
            int aDefectCount = 0;
            int bDefectCount = 0;

            // 以SN+Side为单位获取A/B面缺陷数
            string keyA = $"{sn}_A";
            string keyB = $"{sn}_B";

            if (FrHome.Instance.dic_Infos.TryGetValue(keyA, out List<RootPanelInfoWithIP> aInfos))
            {
                foreach (var info in aInfos)
                {
                    foreach (var pcsInfo in info.RootInfo.PcsInfo.Values)
                    {
                        aDefectCount += pcsInfo.DefectInfo?.Count ?? 0;
                    }
                }
            }

            if (FrHome.Instance.dic_Infos.TryGetValue(keyB, out List<RootPanelInfoWithIP> bInfos))
            {
                foreach (var info in bInfos)
                {
                    foreach (var pcsInfo in info.RootInfo.PcsInfo.Values)
                    {
                        bDefectCount += pcsInfo.DefectInfo?.Count ?? 0;
                    }
                }
            }

            // 合并A/B面的结果进行统计
            var allResults = new List<string>();
            if (FrHome.Instance.dic_Results.TryGetValue(keyA, out List<string> aResults))
            {
                allResults.AddRange(aResults);
            }
            if (FrHome.Instance.dic_Results.TryGetValue(keyB, out List<string> bResults))
            {
                allResults.AddRange(bResults);
            }

            if (allResults.Count > 0)
            {
                count = allResults.Count;
                ok = allResults.Count(o => o.Contains("0"));
                ng = allResults.Count(o => o.Contains("1"));
                byPass = allResults.Count(o => o.Contains("2"));
                msg = $"{msg}_A面:{aDefectCount} B面:{bDefectCount}_OK:{ok} NG:{ng} ByPass:{byPass}";
            }

            // 列顺序: SN[0], Side[1], AVI[2], AI[3], Time[4], Status[5]
            // AVI列 = AVI缺陷图片总数（A面+B面），AI列 = AI推理结果总数
            row.Cells[2].Value = aDefectCount + bDefectCount;
            row.Cells[3].Value = count;
            row.Cells[5].Value = msg;
            FrHome.Instance.str_SN = $"{sn}_B";

            // B面完成时：仅当有缺陷图片时触发完整加载，否则仅更新AI结果标签
            bool hasImages = false;
            // 检查A面和B面是否有缺陷图片
            foreach (var key in new[] { keyA, keyB })
            {
                if (FrHome.Instance.dic_Infos.TryGetValue(key, out var sideInfos))
                {
                    hasImages = sideInfos.Any(inf => inf?.RootInfo?.PcsInfo?.Values?.Any(pcs =>
                        pcs.DefectInfo?.Any(d => d.DefectVrsImages != null && d.DefectVrsImages.Count > 0) == true) == true);
                    if (hasImages) break;
                }
            }
            if (hasImages)
            {
                FrHome.Instance.dataGridViewData_CellClick(null, null);
            }
            else
            {
                FrHome.Instance.UpdateAIResultLabels();
            }
        }

        /// <summary>
        /// 判断状态文本是否为终态（已完成/跳过/失败）
        /// </summary>
        private bool IsTerminalStatus(string statusText)
        {
            if (string.IsNullOrEmpty(statusText)) return false;
            return statusText.Contains("已完成") || statusText.Contains("跳过")
                || statusText.Contains("失败") || statusText.Contains("错误") || statusText.Contains("异常");
        }

        /// <summary>
        /// 清理超出限制的行 - 仅移除终态行，保护处理中和排队中的任务
        /// </summary>
        private void CleanupExcessRows()
        {
            const int MAX_ROWS = 27;
            var dgv = FrHome.Instance.dataGridViewData;

            if (dgv.Rows.Count > MAX_ROWS)
            {
                // 从底部向上查找可移除的终态行
                for (int i = dgv.Rows.Count - 1; i >= 0 && dgv.Rows.Count > MAX_ROWS; i--)
                {
                    string status = dgv.Rows[i].Cells[5].Value?.ToString() ?? "";
                    if (IsTerminalStatus(status))
                    {
                        string snToRemove = dgv.Rows[i].Cells[0].Value?.ToString() ?? "空值";
                        string sideToRemove = dgv.Rows[i].Cells[1].Value?.ToString() ?? "";

                        dgv.Rows.RemoveAt(i);
                        CleanupTaskData(snToRemove, sideToRemove);
                    }
                }

                // 如果移除所有终态行后仍超限（全是活跃任务），则禁止新数据进入
                Machine.master.IsAllow = dgv.Rows.Count <= MAX_ROWS;
            }
            else
            {
                Machine.master.IsAllow = true;
            }
        }

        /// <summary>
        /// 清理任务相关数据
        /// </summary>
        private void CleanupTaskData(string sn, string side)
        {
            string key = $"{sn}_{side}";
            FrHome.Instance.dic_Infos.TryRemove(key, out _);
            FrHome.Instance.dic_Results.TryRemove(key, out _);
            FrHome.Instance.dic_DetectRois.TryRemove(key, out _);
            // 不再删除 SnDebugInfoCache，让缓存独立管理生命周期（MaxCacheSize=1000）
            // FrSearch 需要访问历史调试数据，不能随 FrHome 行清理而删除
        }

        #region UI 批量刷新 — Timer Tick

        /// <summary>
        /// UI 刷新定时器核心回调（200ms 间隔，天然在 UI 线程执行）
        /// 一次 Tick 内批量处理所有缓冲区中的待更新数据
        /// </summary>
        private void UiRefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var dgv = FrHome.Instance.dataGridViewData;
                bool gridChanged = false;

                // ═══ 1. 批量处理任务状态队列 ═══
                if (!_pendingTaskStatus.IsEmpty)
                {
                    dgv.SuspendLayout();
                    try
                    {
                        while (_pendingTaskStatus.TryDequeue(out var statusInfo))
                        {
                            if (statusInfo.Status == DeepSightModel.TaskStatus.Queued)
                            {
                                AddNewTaskRow(statusInfo.SerialNumber, statusInfo.Side);
                            }
                            else
                            {
                                UpdateTaskRowWithStatus(statusInfo);
                            }
                            gridChanged = true;
                        }
                        if (gridChanged) CleanupExcessRows();
                    }
                    finally
                    {
                        dgv.ResumeLayout();
                    }
                }

                // ═══ 2. 批量更新 AVI 列 ═══
                if (!_pendingAviUpdates.IsEmpty)
                {
                    // 快照并清空，避免迭代中被修改
                    var snapshot = _pendingAviUpdates.ToArray();
                    foreach (var kv in snapshot)
                    {
                        _pendingAviUpdates.TryRemove(kv.Key, out _);
                        UpdateAVIColumnForRow(kv.Value.sn, kv.Value.side, kv.Value.count);
                    }
                }

                // ═══ 3. 批量更新 AI 列 ═══
                if (!_pendingAiUpdates.IsEmpty)
                {
                    var snapshot = _pendingAiUpdates.ToArray();
                    foreach (var kv in snapshot)
                    {
                        _pendingAiUpdates.TryRemove(kv.Key, out _);
                        UpdateAIColumnForRow(kv.Value.sn, kv.Value.side, kv.Value.count);
                    }
                }

                // ═══ 4. 刷新工站状态面板 ═══
                if (_machineConfigDirty)
                {
                    _machineConfigDirty = false;
                    FrHome.Instance.RefreshMachineStatusConfigs();
                }
                while (_pendingStationUpdates.TryDequeue(out var machineName))
                {
                    FrHome.Instance.UpdateStationDataReceived(machineName);
                }

                // ═══ 5. 图片显示刷新（仅最新一条，防抖） ═══
                string imageKey = Interlocked.Exchange(ref _pendingImageRefreshKey, null);
                if (imageKey != null)
                {
                    FrHome.Instance.str_SN = imageKey;
                    FrHome.Instance.dataGridViewData_CellClick(null, null);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"UiRefreshTimer_Tick 异常: {ex}");
            }
        }

        #endregion

        internal void LoadMethod()
        {
            try
            {
                // 统一嵌入子窗体到对应 Panel
                var formPanelMap = new (Form form, Panel panel)[]
                {
                    (FrHome.Instance,    panel1),
                    (FrSetting.Instance, panel2),
                    (FrSearch.Instance,  panel3),
                    (FrAlarm.Instance,   panel4),
                    (FrChart.Instance,   panel5),
                };

                foreach (var (form, panel) in formPanelMap)
                {
                    panel.Controls.Clear();
                    form.TopLevel = false;
                    form.Parent = panel;
                    form.Dock = DockStyle.Fill;
                    form.Show();
                }

                // FrHome 特有的初始化
                FrHome.Instance.InitMethod();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        private void FrMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("是否关闭程序？", "关闭提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
            try
            {
                Machine.config_class.Save(Machine.sysConfig);
                Application.DoEvents();
                Thread.Sleep(100);
                if (Machine.master != null)
                {
                    Machine.master.Dispose();
                }
                LogTextHelper.Info("程序关闭");
                LogTextHelper.CloseAndFlush();
                Process process = Process.GetCurrentProcess();
                process.Kill();
                process.Dispose();
            }
            catch (Exception)
            {
                Process process = Process.GetCurrentProcess();
                process.Kill();
                process.Dispose();
            }
        }

        private void btnMax_Click(object sender, EventArgs e)
        {

            if (WindowState == FormWindowState.Normal)
            {
                MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
                // MaximizedBounds = Screen.PrimaryScreen.Bounds;
                WindowState = FormWindowState.Maximized;
                btnMax.BackgroundImage = Resources.min;
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                //this.MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
                WindowState = FormWindowState.Normal;
                btnMax.BackgroundImage = Resources.max;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Machine.master.IsStart)
                {
                    Machine.master.IsStart = true;
                    btnStart.Image = Resources.pause2;
                    if (Machine.HasAgentMachines)
                    {
                        FrSetting.Instance.RestartApplication(FrSetting.Instance.appPath, FrSetting.Instance.appExe, Machine.sysConfig.AgentShutdownTimeout, true);
                    }
                    else
                    {
                        LogTextHelper.Info("未配置 Agent 类型机台，跳过启动 Agent 程序");
                    }

                    LogTextHelper.Info("开始作业...");
                }
                else
                {
                    Machine.master.IsStart = false;
                    btnStart.Image = Resources.start2;
                    if (Machine.HasAgentMachines)
                    {
                        FrSetting.Instance.KillProcessInDirectory(FrSetting.Instance.appPath, FrSetting.Instance.appExe, Machine.sysConfig.AgentShutdownTimeout);
                    }

                    LogTextHelper.Info("暂停作业...");
                }
                return;
            }
            catch (Exception)
            {

                throw;
            }

        }
        private void btnPause_Click(object sender, EventArgs e)
        {
            SwitchFrom(FormMode.SearchForm);
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void btnTool_Click(object sender, EventArgs e)
        {
            if (!IsAllow)
            {
                return;
            }
            SwitchFrom(FormMode.SettingForm);
        }
        private void btnAlarm_Click(object sender, EventArgs e)
        {
            if (!IsAllow)
            {
                return;
            }
            SwitchFrom(FormMode.AlarmForm);
        }
        private void btnChart_Click(object sender, EventArgs e)
        {
            if (!IsAllow)
            {
                return;
            }
            SwitchFrom(FormMode.ChartForm);
        }
        private void btnHome_Click(object sender, EventArgs e)
        {
            SwitchFrom(FormMode.MainForm);
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            FrLogin login = new FrLogin();
            login.ShowDialog();
            if (!string.IsNullOrWhiteSpace(Machine.LoginUserName))
            {
                btnLogin.Image = Resources.user2;
                if (string.IsNullOrWhiteSpace(Machine.LoginUserName))
                {
                    FrSetting.Instance.btnSave.Enabled = false;
                }
                else if (Machine.LoginUserName.Contains("操作员") || Machine.LoginUserName.Contains("Operator"))
                {
                    FrSetting.Instance.btnSave.Enabled = false;
                }
                else
                {
                    FrSetting.Instance.btnSave.Enabled = true;
                }
            }
            else
            {
                btnLogin.Image = Resources.user1;
            }
        }
        /// <summary>
        /// 当前窗体模式
        /// </summary>
        internal FormMode curFormMode = FormMode.None;

        internal void SwitchFrom(FormMode formMode)
        {
            if (curFormMode == formMode) return;

            // 1. 将当前按钮恢复为未选中图标
            if (_buttonConfigs.TryGetValue(curFormMode, out var oldCfg))
            {
                oldCfg.btn.Image = oldCfg.inactiveImg;
            }

            // 2. 切换 Panel 可见性（字典驱动）
            foreach (var kvp in _pagePanels)
            {
                bool isTarget = kvp.Key == formMode;
                kvp.Value.Visible = isTarget;
                if (isTarget)
                    kvp.Value.Dock = DockStyle.Fill;
            }
            // 隐藏未使用的 panel6、panel7
            panel6.Visible = false;
            panel7.Visible = false;

            // 3. 将新按钮设为选中图标
            if (_buttonConfigs.TryGetValue(formMode, out var newCfg))
            {
                newCfg.btn.Image = newCfg.activeImg;
            }

            curFormMode = formMode;
        }

        #region 窗体缩放
        private const int WM_NCHITTEST = 0x0084; //鼠标在窗体客户区（除标题栏和边框以外的部分）时发送的信息
        private const int HTLEFT = 10;  //左边
        private const int HTRIGHT = 11;  //右边
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;  //左上
        private const int HTTOPRIGHT = 14; //右上
        private const int HTBOTTOM = 15;  //下
        private const int HTBOTTOMLEFT = 0x10;  //左下
        private const int HTBOTTOMRIGHT = 17;  //右下
        private System.Drawing.Point vPoint = System.Drawing.Point.Empty;

        //自定义边框拉伸
        protected override void WndProc(ref Message m)
        {
            try
            {
                base.WndProc(ref m);
                switch (m.Msg)
                {
                    case WM_NCHITTEST:
                        vPoint = new System.Drawing.Point((int)m.LParam & 0xFFFF, (int)m.LParam >> 16 & 0xFFFF);
                        vPoint = PointToClient(vPoint);
                        if (vPoint.X <= 5)
                        {
                            if (vPoint.Y <= 5)
                            {
                                m.Result = (IntPtr)HTTOPLEFT;  //左上
                            }
                            else if (vPoint.Y >= ClientSize.Height - 5)
                            {
                                m.Result = (IntPtr)HTBOTTOMLEFT; //左下
                            }
                            else
                            {
                                m.Result = (IntPtr)HTLEFT;  //左边
                            }
                        }
                        else if (vPoint.X >= ClientSize.Width - 5)
                        {
                            if (vPoint.Y <= 5)
                            {
                                m.Result = (IntPtr)HTTOPRIGHT;  //右上
                            }
                            else if (vPoint.Y >= ClientSize.Height - 5)
                            {
                                m.Result = (IntPtr)HTBOTTOMRIGHT;  //右下
                            }
                            else
                            {
                                m.Result = (IntPtr)HTRIGHT;  //右
                            }
                        }
                        else if (vPoint.Y <= 5)
                        {
                            m.Result = (IntPtr)HTTOP;  //上
                        }
                        else if (vPoint.Y >= ClientSize.Height - 5)
                        {
                            m.Result = (IntPtr)HTBOTTOM; //下
                        }
                        else
                        {
                            base.WndProc(ref m);//如果去掉这一行代码,窗体将失去MouseMove..等事件
                            System.Drawing.Point lpint = new System.Drawing.Point((int)m.LParam);//可以得到鼠标坐标,这样就可以决定怎么处理这个消息了,是移动窗体,还是缩放,以及向哪向的缩放

                            m.Result = (IntPtr)0x2;//托动HTCAPTION=2 <0x2>
                        }
                        break;
                }
            }
            catch { }
        }

        #endregion 窗体缩放

        #region 窗体拖动
        private static bool IsDrag = false;
        private int enterX;
        private int enterY;

        //双击
        private void panelTile_DoubleClick(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                this.MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
                WindowState = FormWindowState.Maximized;
                btnMax.BackgroundImage = Resources.min;
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
                btnMax.BackgroundImage = Resources.max;
            }
        }

        private void setForm_MouseDown(object sender, MouseEventArgs e)
        {
            IsDrag = true;
            enterX = e.Location.X;
            enterY = e.Location.Y;
        }

        private void setForm_MouseUp(object sender, MouseEventArgs e)
        {
            IsDrag = false;
            enterX = 0;
            enterY = 0;
        }

        private void setForm_MouseLeave(object sender, EventArgs e)
        {
            IsDrag = false;
            enterX = 0;
            enterY = 0;
        }

        private void setForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDrag)
            {
                Left += e.Location.X - enterX;
                Top += e.Location.Y - enterY;
            }
        }

        #endregion 窗体拖动

        #region 右上角菜单

        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMin_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        #endregion 右上角菜单

        #region 状态栏-运行时间-当前时间
        private System.Timers.Timer timer = new System.Timers.Timer();

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        lbl_curTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }));
                }
                else
                {
                    lbl_curTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                DateTime now = DateTime.Now;

                if (now.Date > _lastResetDate)
                {
                    LogTextHelper.Info("Data Reset");
                    _lastResetDate = now.Date;
                }

            }
            catch (System.Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        #endregion 状态栏-运行时间-当前时间

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Machine.master.IsShowBox = btn_showBox.Checked;
        }

        private void btnModelB_Click(object sender, EventArgs e)
        {
            if (btnModelB.Checked)
            {
                Machine.ShowFlag = "B";
                btnModelC.Checked = false;
            }
        }

        private void btnModelC_Click(object sender, EventArgs e)
        {
            if (btnModelC.Checked)
            {
                Machine.ShowFlag = "C";
                btnModelB.Checked = false;
            }
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            Machine.master.IsStart = false;
            FrHome.Instance.ClearAllImages();
        }
        private async void btnModel_Click(object sender, EventArgs e)
        {
            //ATS增加模式切换
            if (this.btnModel.Text == "生产模式")
            {
                this.btnModel.Text = "设定模式";
                IsAllow = true;
            }
            else if (this.btnModel.Text == "设定模式")
            {
                this.btnModel.Text = "生产模式";
                IsAllow = false;
            }
            return;
        }

        public void btnRunVB_Click(object sender, EventArgs e)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                Machine.master.DefectService.AiDefect.Vision_Show_View(1);
            }));
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!IsAllow)
            {
                return;
            }
            SwitchFrom(FormMode.SearchForm);
        }
    }
    public enum FormMode
    {
        None,
        MainForm,
        SettingForm,
        CameraForm,
        AlarmForm,
        ChartForm,
        FnForm,
        SearchForm,
    }

}

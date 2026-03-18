using DeepSightAI.Properties;
using DeepSightEvent;
using DeepSightModel;
using DeepSightModel.Configuration;
using DeepSightTool;
using System;
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
            SystemEvent.EventSendTaskToUI += new SendTask(SystemEvent_EventSendTaskToUI);
            SystemEvent.EventSendAlarmToUI += new SendAlarm(SystemEvent_EventSendAlarmToUI);
            SystemEvent.EventSendDefectNumToUI += new SendDefectNum(SystemEvent_EventSendDefectNumToUI);
            SystemEvent.EventSendDefectPanelInfoToUI += new SendDefectPanelInfo(SystemEvent_EventSendDefectPanelInfoToUI);
            SystemEvent.EventSendDefectResultInfoToUI += new SendDefectResultInfo(SystemEvent_EventSendDefectResultInfoToUI);
            SystemEvent.EventSendDefectRoiInfoToUI += new SendDefectRoiInfo(SystemEvent_EventSendDefectRoiInfoToUI);
        }



        private void SystemEvent_EventSendDefectResultInfoToUI(string sn, List<string> msg)
        {
            try
            {
                // 使用 GetOrAdd 线程安全地获取或添加结果
                var resultList = FrHome.Instance.dic_Results.GetOrAdd(sn, _ => new List<string>());
                lock (resultList)
                {
                    resultList.AddRange(msg);
                }

            }
            catch (Exception ex)
            {
                LogTextHelper.Error("结果回调异常" + ex.ToString());
                SystemEvent.SendAlarmMsg("结果回调异常" + ex.ToString());
            }
        }

        private void SystemEvent_EventSendDefectRoiInfoToUI(string sn, List<Roi> rois)
        {
            try
            {
                var roiList = FrHome.Instance.dic_DetectRois.GetOrAdd(sn, _ => new List<Roi>());
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

        private void SystemEvent_EventSendDefectPanelInfoToUI(string sn, RootPanelInfoWithIP info)
        {
            try
            {
                // 使用 GetOrAdd 线程安全地获取或添加 Panel 信息
                var infoList = FrHome.Instance.dic_Infos.GetOrAdd(sn, _ => new List<RootPanelInfoWithIP>());
                lock (infoList)
                {
                    infoList.Add(info);
                }

                // 提取 MachineName，检查是否需要添加新工站
                string machineName = info?.RootInfo?.MachineName;
                if (!string.IsNullOrEmpty(machineName))
                {
                    // 检查工站是否已存在于机台注册表中
                    bool registryExists = Machine.machineRegistry?.Machines?.Any(m => m.MachineName == machineName) ?? false;

                    if (!registryExists)
                    {
                        // 1. 添加到机台注册表（主配置源）
                        var newEntry = new MachineEntry
                        {
                            MachineName = machineName,
                            IsEnable = true,
                            DataSourceType = DataSourceType.LevelDb  // 自动发现的工站默认 LevelDb
                        };
                        Machine.machineRegistryManager.AddOrUpdate(newEntry);
                        // 刷新内存中的注册表
                        Machine.machineRegistryManager.Read(out Machine.machineRegistry);

                        // 2. 同步添加到 aviconfig（保持向后兼容）
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

                        // 更新 UI 显示新工站
                        FrHome.Instance.RefreshAviCtrConfigs();

                        LogTextHelper.Info($"自动发现并添加新工站: {machineName}");
                    }

                    // 更新工站数据接收时间（状态变为绿色）
                    FrHome.Instance.UpdateStationDataReceived(machineName);
                }

                Machine.config_class.Save(Machine.sysConfig);

                // 自动触发图片显示：仅当面板数据包含缺陷图片时才刷新，避免清空已有显示
                bool hasImages = info?.RootInfo?.PcsInfo?.Values?.Any(pcs =>
                    pcs.DefectInfo?.Any(d => d.DefectVrsImages != null && d.DefectVrsImages.Count > 0) == true) == true;
                if (hasImages)
                {
                    FrHome.Instance.str_SN = sn;
                    if (FrHome.Instance.dataGridViewData.InvokeRequired)
                    {
                        FrHome.Instance.dataGridViewData.BeginInvoke(new MethodInvoker(() =>
                            FrHome.Instance.dataGridViewData_CellClick(null, null)));
                    }
                    else
                    {
                        FrHome.Instance.dataGridViewData_CellClick(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Panel回调异常" + ex.ToString());
                SystemEvent.SendAlarmMsg("Panel回调异常" + ex.ToString());
            }
        }
        private void SystemEvent_EventSendDefectNumToUI(int num)
        {

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
            this.lbl_title.Text = "ATS_AI ~ " + lastWriteTime.ToString("MMdd");

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

            try
            {
                // 确保在 UI 线程上执行
                if (FrHome.Instance.dataGridViewData.InvokeRequired)
                {
                    FrHome.Instance.dataGridViewData.BeginInvoke(new MethodInvoker(() => SystemEvent_EventSendTaskStatusToUI(statusInfo)));
                    return;
                }

                lock (Locker)
                {
                    if (statusInfo.Status == DeepSightModel.TaskStatus.Queued)
                    {
                        // 添加新任务
                        AddNewTaskRow(statusInfo.SerialNumber);
                    }
                    else
                    {
                        // 更新现有任务状态
                        UpdateTaskRowWithStatus(statusInfo);
                    }

                    // 清理超出限制的行
                    CleanupExcessRows();
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"SystemEvent_EventSendTaskStatusToUI 异常: {ex}");
            }
        }
        
        private void SystemEvent_EventSendTaskToUI(object task, string msg = "", long timeMs = 0)
        {
            if (task == null) return;

            try
            {
                string sn = task.ToString();

                // 确保在 UI 线程上执行（使用 BeginInvoke 避免阻塞）
                if (FrHome.Instance.dataGridViewData.InvokeRequired)
                {
                    FrHome.Instance.dataGridViewData.BeginInvoke(new MethodInvoker(() => SystemEvent_EventSendTaskToUI(task, msg, timeMs)));
                    return;
                }

                lock (Locker)
                {
                    if (string.IsNullOrEmpty(msg))
                    {
                        // 添加新任务
                        AddNewTaskRow(sn);
                    }
                    else
                    {
                        // 更新现有任务状态
                        UpdateTaskRow(sn, msg, timeMs);
                    }

                    // 清理超出限制的行
                    CleanupExcessRows();
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"SystemEvent_EventSendTaskToUI 异常: {ex}");
            }
        }

        /// <summary>
        /// 添加新任务行
        /// </summary>
        private void AddNewTaskRow(string sn)
        {
            FrHome.Instance.dataGridViewData.Rows.Insert(0, new object[] { sn, "0", "0", "", "排队中" });
            FrHome.Instance.dataGridViewData.Rows[0].DefaultCellStyle.ForeColor = Color.Yellow;
        }

        /// <summary>
        /// 更新任务行状态（使用结构化状态）
        /// </summary>
        private void UpdateTaskRowWithStatus(TaskStatusInfo statusInfo)
        {
            // 查找对应的行
            DataGridViewRow targetRow = null;
            foreach (DataGridViewRow row in FrHome.Instance.dataGridViewData.Rows)
            {
                if (row.Cells[0].Value?.ToString() == statusInfo.SerialNumber)
                {
                    targetRow = row;
                    break;
                }
            }

            if (targetRow == null) return;

            // 设置颜色
            Color statusColor = TaskStatusHelper.GetStatusColor(statusInfo.Status);
            targetRow.DefaultCellStyle.ForeColor = statusColor;

            // 更新时间列（如果有传入时间）
            if (statusInfo.ProcessingTimeMs > 0)
            {
                string existingTime = targetRow.Cells[3].Value?.ToString();
                if (!string.IsNullOrEmpty(existingTime))
                {
                    targetRow.Cells[3].Value = $"{existingTime}+{statusInfo.ProcessingTimeMs}";
                }
                else
                {
                    targetRow.Cells[3].Value = statusInfo.ProcessingTimeMs.ToString();
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
                targetRow.Cells[4].Value = statusInfo.GetFullDisplayMessage();
            }
        }

        /// <summary>
        /// 更新任务行状态
        /// </summary>
        /// <param name="timeMs">AI处理时间(毫秒)</param>
        private void UpdateTaskRow(string sn, string msg, long timeMs = 0)
        {
            // 查找对应的行
            DataGridViewRow targetRow = null;
            foreach (DataGridViewRow row in FrHome.Instance.dataGridViewData.Rows)
            {
                if (row.Cells[0].Value?.ToString() == sn)
                {
                    targetRow = row;
                    break;
                }
            }

            if (targetRow == null) return;

            // 根据消息内容设置颜色和更新数据
            Color statusColor = GetStatusColor(msg);
            targetRow.DefaultCellStyle.ForeColor = statusColor;

            // 更新时间列（如果有传入时间）
            if (timeMs > 0)
            {
                // 拼接显示时间，如 A面100ms + B面100ms 显示为 "100+100"
                string existingTime = targetRow.Cells[3].Value?.ToString();
                if (!string.IsNullOrEmpty(existingTime))
                {
                    targetRow.Cells[3].Value = $"{existingTime}+{timeMs}";
                }
                else
                {
                    targetRow.Cells[3].Value = timeMs.ToString();
                }
            }

            // 处理 B 面完成的特殊逻辑
            if (msg.Contains("已完成") && msg.Contains("B面"))
            {
                UpdateBSideCompletedRow(targetRow, sn, ref msg);
            }
            else
            {
                targetRow.Cells[4].Value = msg;
            }
        }

        /// <summary>
        /// 根据消息内容获取状态颜色
        /// </summary>
        private Color GetStatusColor(string msg)
        {
            if (msg.Contains("已完成"))
                return Color.Green;
            else if (msg.Contains("正在读取数据") || msg.Contains("正在加载图片"))
                return Color.Orange;
            else if (msg.Contains("图片加载完成"))
                return Color.DarkOrange;
            else if (msg.Contains("开始AI检测") || msg.Contains("处理中"))
                return Color.White;
            else if (msg.Contains("AI检测完成"))
                return Color.DarkCyan;
            else if (msg.Contains("正在回写结果"))
                return Color.Purple;
            else if (msg.Contains("错误") || msg.Contains("失败") || msg.Contains("异常"))
                return Color.Red;
            else
                return Color.Gray;
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

            // 获取A/B面缺陷数
            if (FrHome.Instance.dic_Infos.TryGetValue(sn, out List<RootPanelInfoWithIP> infos))
            {
                foreach (var info in infos)
                {
                    int sideDefectCount = 0;
                    foreach (var pcsInfo in info.RootInfo.PcsInfo.Values)
                    {
                        sideDefectCount += pcsInfo.DefectInfo?.Count ?? 0;
                    }

                    if (info.RootInfo.SideIndex == "A")
                    {
                        aDefectCount = sideDefectCount;
                    }
                    else if (info.RootInfo.SideIndex == "B")
                    {
                        bDefectCount = sideDefectCount;
                    }
                }
            }

            if (FrHome.Instance.dic_Results.TryGetValue(sn, out List<string> res_lbl))
            {
                count = res_lbl.Count;
                ok = res_lbl.Count(o => o.Contains("0"));
                ng = res_lbl.Count(o => o.Contains("1"));
                byPass = res_lbl.Count(o => o.Contains("2"));
                msg = $"{msg}_A面:{aDefectCount} B面:{bDefectCount}_OK:{ok} NG:{ng} ByPass:{byPass}";
            }

            row.Cells[1].Value = count;
            row.Cells[2].Value = count;
            row.Cells[4].Value = msg;
            FrHome.Instance.str_SN = sn;

            // B面完成时：仅当有缺陷图片时触发完整加载，否则仅更新AI结果标签
            bool hasImages = false;
            if (FrHome.Instance.dic_Infos.TryGetValue(sn, out var snInfos))
            {
                hasImages = snInfos.Any(inf => inf?.RootInfo?.PcsInfo?.Values?.Any(pcs =>
                    pcs.DefectInfo?.Any(d => d.DefectVrsImages != null && d.DefectVrsImages.Count > 0) == true) == true);
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
        /// 清理超出限制的行
        /// </summary>
        private void CleanupExcessRows()
        {
            const int MAX_ROWS = 27;
            const int KEEP_COMPLETED_ROWS = 8;

            if (FrHome.Instance.dataGridViewData.Rows.Count > MAX_ROWS)
            {
                int checkIndex = FrHome.Instance.dataGridViewData.Rows.Count - KEEP_COMPLETED_ROWS;
                if (checkIndex >= 0 && checkIndex < FrHome.Instance.dataGridViewData.Rows.Count)
                {
                    string status = FrHome.Instance.dataGridViewData.Rows[checkIndex].Cells[4].Value?.ToString() ?? "";

                        int lastIndex = FrHome.Instance.dataGridViewData.Rows.Count - 1;
                        string snToRemove = FrHome.Instance.dataGridViewData.Rows[lastIndex].Cells[0].Value?.ToString() ?? "空值";

                        // 移除行
                        FrHome.Instance.dataGridViewData.Rows.RemoveAt(lastIndex);

                        // 清理相关数据
                        CleanupTaskData(snToRemove);
                    
                }
                Machine.master.IsAllow = false;
            }
            else
            {
                Machine.master.IsAllow = true;
            }
        }

        /// <summary>
        /// 清理任务相关数据
        /// </summary>
        private void CleanupTaskData(string sn)
        {
            FrHome.Instance.dic_Infos.TryRemove(sn,out _);
            FrHome.Instance.dic_Results.TryRemove(sn, out _);
            // 清理SN调试信息缓存
            SnDebugInfoCache.Remove(sn, "A");
            SnDebugInfoCache.Remove(sn, "B");
        }
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
                FrHome.Instance.InitWork();
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
                    FrSetting.Instance.RestartApplication(FrSetting.Instance.appPath, FrSetting.Instance.appExe, Machine.sysConfig.AgentShutdownTimeout, true);

                    LogTextHelper.Info("开始作业...");
                }
                else
                {
                    Machine.master.IsStart = false;
                    btnStart.Image = Resources.start2;
                    FrSetting.Instance.KillProcessInDirectory(FrSetting.Instance.appPath, FrSetting.Instance.appExe, Machine.sysConfig.AgentShutdownTimeout);

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
                lbl_curTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                DateTime now = DateTime.Now;

                if (now.Date > _lastResetDate)
                {
                    ResetCount();
                    _lastResetDate = now.Date;
                }

            }
            catch (System.Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }
        private void ResetCount()
        {
            LogTextHelper.Info("Data Reset");
        }

        #endregion 状态栏-运行时间-当前时间

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!btn_showBox.Checked)
            {
                btn_showBox.Checked = false;
                Machine.master.IsShowBox = false;
            }
            else
            {
                btn_showBox.Checked = true;
                Machine.master.IsShowBox = true;
            }
           
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
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            return;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {

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

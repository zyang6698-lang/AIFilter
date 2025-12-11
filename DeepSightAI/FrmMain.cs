using DeepSightAI.Properties;
using DeepSightEvent;
using DeepSightModel;
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
        public FrmMain()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;
            MaximizedBounds = SystemInformation.WorkingArea;//Screen.PrimaryScreen.WorkingArea;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            //MaximizedBounds = Screen.PrimaryScreen.Bounds;
            WindowState = FormWindowState.Maximized;
            btnMax.BackgroundImage = Resources.min;
            this.Load += FrmMain_Load;
            this.FormClosing += FrMain_FormClosing;


            SystemEvent.EventSendTaskToUI += new SendTask(SystemEvent_EventSendTaskToUI);
            SystemEvent.EventSendAlarmToUI += new SendAlarm(SystemEvent_EventSendAlarmToUI);
            SystemEvent.EventSendDefectNumToUI += new SendDefectNum(SystemEvent_EventSendDefectNumToUI);
            SystemEvent.EventSendDefectPanelInfoToUI += new SendDefectPanelInfo(SystemEvent_EventSendDefectPanelInfoToUI);
            SystemEvent.EventSendDefectResultInfoToUI += new SendDefectResultInfo(SystemEvent_EventSendDefectResultInfoToUI);
        }



        private void SystemEvent_EventSendDefectResultInfoToUI(string sn, List<string> msg, List<string> details, PcsResult pcsResult)
        {
            try
            {
                if (!FrHome.Instance.dic_Results.ContainsKey(sn))
                {
                    List<string> result = new List<string>();
                    result.AddRange(msg);
                    FrHome.Instance.dic_Results.TryAdd(sn, result);
                }
                else
                {
                    FrHome.Instance.dic_Results.TryGetValue(sn, out List<string> result);
                    result.AddRange(msg);
                }
                //细节信息
                if (!FrHome.Instance.dic_Details.ContainsKey(sn))
                {
                    List<string> result = new List<string>();
                    result.AddRange(details);
                    FrHome.Instance.dic_Details.Add(sn, result);
                }
                else
                {
                    FrHome.Instance.dic_Details.TryGetValue(sn, out List<string> result);
                    result.AddRange(details);
                }

                //PCS缺陷坐标
                if (!FrHome.Instance.dic_PcsResult.ContainsKey(sn))
                {
                    PcsResult result = new PcsResult
                    {
                        vb_List = new List<VBRcvInfp>()
                    };
                    if (pcsResult?.vb_List != null)
                    {
                        result.vb_List.AddRange(pcsResult.vb_List);
                    }
                    FrHome.Instance.dic_PcsResult.Add(sn, result);
                }
                else
                {
                    FrHome.Instance.dic_PcsResult.TryGetValue(sn, out PcsResult result);
                    if (pcsResult?.vb_List != null)
                    {
                        result.vb_List.AddRange(pcsResult.vb_List);
                    }
                }

            }
            catch (Exception ex)
            {
                LogTextHelper.Error("结果回调异常" + ex.ToString());
                SystemEvent.SendAlarmMsg("结果回调异常" + ex.ToString());
            }
        }

        private void SystemEvent_EventSendDefectPanelInfoToUI(string sn, RootPanelInfoWithIP info)
        {
            try
            {
                if (!FrHome.Instance.dic_Infos.ContainsKey(sn))
                {
                    List<RootPanelInfoWithIP> resInfo = new List<RootPanelInfoWithIP>();
                    resInfo.Add(info);
                    FrHome.Instance.dic_Infos.Add(sn, resInfo);
                }
                else
                {
                    FrHome.Instance.dic_Infos.TryGetValue(sn, out List<RootPanelInfoWithIP> resInfo);
                    resInfo.Add(info);
                    //客户要求先屏蔽
                    //AddOrUpdateMachineData(info.rootInfo.StationName, info.rootInfo.ProductSerial, $"{info.rootInfo.LotId}_{info.rootInfo.LotBatch}");
                }
                Machine.sysConfig.Index = Machine.master.workClass.Index;
                Machine.config_class.Save(Machine.sysConfig);
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
            Machine.master.workClass.isShowBox = this.btn_showBox.Checked;
            string filePath = Assembly.GetExecutingAssembly().Location;
            DateTime lastWriteTime = File.GetLastWriteTime(filePath);
            this.lbl_title.Text = "ATS_AI ~ " + lastWriteTime.ToString("MMdd");

        }

        private void SystemEvent_EventSendAlarmToUI(string massage)
        {
            LogTextHelper.Warn($"收到异常消息：{massage},任务已停止");
        }
        public static object Locker = new object();
        private void SystemEvent_EventSendTaskToUI(object task, string msg = "")
        {
            if (task == null) return;

            try
            {
                string sn = task.ToString();

                // 确保在 UI 线程上执行
                if (FrHome.Instance.dataGridViewData.InvokeRequired)
                {
                    FrHome.Instance.dataGridViewData.Invoke(new MethodInvoker(() => SystemEvent_EventSendTaskToUI(task, msg)));
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
                        UpdateTaskRow(sn, msg);
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
            FrHome.Instance.dataGridViewData.Rows.Insert(0, new object[] { sn, "0", "0", "排队中" });
            FrHome.Instance.dataGridViewData.Rows[0].DefaultCellStyle.ForeColor = Color.Yellow;
        }

        /// <summary>
        /// 更新任务行状态
        /// </summary>
        private void UpdateTaskRow(string sn, string msg)
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

            // 处理 B 面完成的特殊逻辑
            if (msg.Contains("已完成") && msg.Contains("B面"))
            {
                UpdateBSideCompletedRow(targetRow, sn, ref msg);
            }
            else
            {
                targetRow.Cells[3].Value = msg;
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

            if (FrHome.Instance.dic_Results.TryGetValue(sn, out List<string> res_lbl))
            {
                count = res_lbl.Count;
                ok = res_lbl.Count(o => o.Contains("0"));
                ng = res_lbl.Count(o => o.Contains("1"));
                byPass = res_lbl.Count(o => o.Contains("2"));
                msg = $"{msg}_图片一致_OK:{ok} NG:{ng} ByPass:{byPass}";
            }

            row.Cells[1].Value = count;
            row.Cells[2].Value = count;
            row.Cells[3].Value = msg;
            FrHome.Instance.str_SN = sn;
            FrHome.Instance.dataGridViewData_CellClick(null, null);
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
                    string status = FrHome.Instance.dataGridViewData.Rows[checkIndex].Cells[3].Value?.ToString() ?? "";
                    if (status.Contains("已完成"))
                    {
                        int lastIndex = FrHome.Instance.dataGridViewData.Rows.Count - 1;
                        string snToRemove = FrHome.Instance.dataGridViewData.Rows[lastIndex].Cells[0].Value?.ToString() ?? "空值";

                        // 移除行
                        FrHome.Instance.dataGridViewData.Rows.RemoveAt(lastIndex);

                        // 清理相关数据
                        CleanupTaskData(snToRemove);
                    }
                }
                Machine.master.workClass.IsAllow = false;
            }
            else
            {
                Machine.master.workClass.IsAllow = true;
            }
        }

        /// <summary>
        /// 清理任务相关数据
        /// </summary>
        private void CleanupTaskData(string sn)
        {
            FrHome.Instance.dic_Infos.Remove(sn);
            FrHome.Instance.dic_Paths.Remove(sn);
            FrHome.Instance.dic_Results.TryRemove(sn, out _);
            FrHome.Instance.dic_PcsResult.Remove(sn);
            FrHome.Instance.dic_Details.Remove(sn);
        }
        internal void LoadMethod()
        {
            try
            {
                //主页
                FrmMain.Instance.panel1.Controls.Clear();
                FrHome.Instance.TopLevel = false;
                FrHome.Instance.Parent = FrmMain.Instance.panel1;
                FrHome.Instance.Dock = DockStyle.Fill;
                FrHome.Instance.Show();
                //FrHome.Instance.LoadMethod();
                FrHome.Instance.InitMethod();
                //事件订阅以及图片显示初始化
                FrHome.Instance.InitWork();

                ////设置
                FrmMain.Instance.panel2.Controls.Clear();
                FrSetting.Instance.TopLevel = false;
                FrSetting.Instance.Parent = FrmMain.Instance.panel2;
                FrSetting.Instance.Dock = DockStyle.Fill;
                FrSetting.Instance.Show();

                ////警报
                FrmMain.Instance.panel4.Controls.Clear();
                FrAlarm.Instance.TopLevel = false;
                FrAlarm.Instance.Parent = FrmMain.Instance.panel4;
                FrAlarm.Instance.Dock = DockStyle.Fill;
                FrAlarm.Instance.Show();

                ////图标
                FrmMain.Instance.panel5.Controls.Clear();
                FrChart.Instance.TopLevel = false;
                FrChart.Instance.Parent = FrmMain.Instance.panel5;
                FrChart.Instance.Dock = DockStyle.Fill;
                FrChart.Instance.Show();

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
                Machine.sysConfig.Index = Machine.master.workClass.Index;
                Machine.config_class.Save(Machine.sysConfig);
                Application.DoEvents();
                Thread.Sleep(100);
                if (Machine.master != null)
                {
                    Machine.master.Dispose();
                }
                LogTextHelper.Info("程序关闭");

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
                if (!Machine.master.workClass.isStart)
                {
                    Machine.master.workClass.isStart = true;
                    btnStart.Image = Resources.pause2;
                    FrSetting.Instance.RestartApplication(FrSetting.Instance.appPath, FrSetting.Instance.appExe, true);

                    LogTextHelper.Info("开始作业...");
                }
                else
                {
                    Machine.master.workClass.isStart = false;
                    btnStart.Image = Resources.start2;
                    FrSetting.Instance.KillProcessInDirectory(FrSetting.Instance.appPath, FrSetting.Instance.appExe);

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
            return;
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
            if (curFormMode != formMode)
            {
                SwitchButton();
            }
            switch (formMode)
            {
                case FormMode.MainForm:

                    if (curFormMode != FormMode.MainForm)
                    {
                        curFormMode = FormMode.MainForm;

                        btnHome.Image = Resources.home2;

                        FrmMain.Instance.panel1.Dock = DockStyle.Fill;

                        FrmMain.Instance.panel1.Visible = true;
                        FrmMain.Instance.panel2.Visible = false;
                        FrmMain.Instance.panel3.Visible = false;
                        FrmMain.Instance.panel4.Visible = false;
                        FrmMain.Instance.panel5.Visible = false;
                        FrmMain.Instance.panel6.Visible = false;
                        FrmMain.Instance.panel7.Visible = false;
                    }
                    break;

                case FormMode.SettingForm:

                    if (curFormMode != FormMode.SettingForm)
                    {
                        curFormMode = FormMode.SettingForm;

                        btnTool.Image = Resources.tool2;

                        FrmMain.Instance.panel2.Dock = DockStyle.Fill;

                        FrmMain.Instance.panel1.Visible = false;
                        FrmMain.Instance.panel2.Visible = true;
                        FrmMain.Instance.panel3.Visible = false;
                        FrmMain.Instance.panel4.Visible = false;
                        FrmMain.Instance.panel5.Visible = false;
                        FrmMain.Instance.panel6.Visible = false;
                        FrmMain.Instance.panel7.Visible = false;
                    }
                    break;

                case FormMode.AlarmForm:

                    if (curFormMode != FormMode.AlarmForm)
                    {
                        curFormMode = FormMode.AlarmForm;

                        btnAlarm.Image = Resources.alarm2;

                        FrmMain.Instance.panel4.Dock = DockStyle.Fill;

                        FrmMain.Instance.panel1.Visible = false;
                        FrmMain.Instance.panel2.Visible = false;
                        FrmMain.Instance.panel3.Visible = false;
                        FrmMain.Instance.panel4.Visible = true;
                        FrmMain.Instance.panel5.Visible = false;
                        FrmMain.Instance.panel6.Visible = false;
                        FrmMain.Instance.panel7.Visible = false;
                    }
                    break;

                case FormMode.ChartForm:

                    if (curFormMode != FormMode.ChartForm)
                    {
                        curFormMode = FormMode.ChartForm;

                        btnChart.Image = Resources.chart2;

                        FrmMain.Instance.panel5.Dock = DockStyle.Fill;

                        FrmMain.Instance.panel1.Visible = false;
                        FrmMain.Instance.panel2.Visible = false;
                        FrmMain.Instance.panel3.Visible = false;
                        FrmMain.Instance.panel4.Visible = false;
                        FrmMain.Instance.panel5.Visible = true;
                        FrmMain.Instance.panel6.Visible = false;
                        FrmMain.Instance.panel7.Visible = false;
                    }
                    break;

            }
        }
        private void SwitchButton()
        {
            try
            {
                switch (curFormMode)
                {
                    case FormMode.None:
                        btnHome.Image = Resources.home1;
                        btnTool.Image = Resources.tool1;
                        //btnCamera.Image = Resources.pho1;
                        btnAlarm.Image = Resources.alarm1;
                        btnChart.Image = Resources.chart1;
                        //btnFn.Image = Resources.Fn1;
                        //btnSearch.Image = Resources.search1;
                        break;

                    case FormMode.MainForm:
                        btnHome.Image = Resources.home1;
                        break;

                    case FormMode.SettingForm:
                        btnTool.Image = Resources.tool1;

                        break;

                    //case FormMode.CameraForm:
                    //    btnCamera.Image = Resources.pho1;
                    //    break;

                    case FormMode.AlarmForm:
                        btnAlarm.Image = Resources.alarm1;
                        break;

                    case FormMode.ChartForm:
                        btnChart.Image = Resources.chart1;
                        break;

                    //case FormMode.FnForm:
                    //    btnFn.Image = Resources.Fn1;
                    //    break;

                    //case FormMode.SearchForm:
                    //    btnSearch.Image = Resources.search1;
                    //    break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
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
                Machine.master.workClass.isShowBox = false;
            }
            else
            {
                btn_showBox.Checked = true;
                Machine.master.workClass.isShowBox = true;
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
        private async void btnClear_Click(object sender, EventArgs e)
        {
            Machine.master.workClass.isStart = false;
            // 生成索引集合（0-99）
            var indices = Enumerable.Range(0, FrHome.Instance.DispWin2.Length).ToList();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 2),
                CancellationToken = CancellationToken.None
            };
            await Task.Factory.StartNew(() =>
            {
                Parallel.ForEach(indices, options, i =>
                {
                    if (i < 0 || i >= FrHome.Instance.DispWin2.Length || FrHome.Instance.DispWin2[i] == null) return;

                    if (FrHome.Instance.DispWin2[i].InvokeRequired)
                    {
                        FrHome.Instance.DispWin2[i].BeginInvoke(new Action(() =>
                        {
                            FrHome.Instance.DispWin2[i].Clear();
                        }));
                    }
                    else
                    {
                        FrHome.Instance.DispWin2[i].Clear();
                    }
                });
            });
            //FrHome.Instance.DispWin1[0].Clear();
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
                Machine.master.workClass.defect.ai_Defect.Vision_Show_View(1);
            }));
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            return;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {

        }

        private void lbl_Count_Click(object sender, EventArgs e)
        {

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

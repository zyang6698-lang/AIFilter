using DeepSightAI.Properties;
using DeepSightDB;
using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using DeepSightWorkLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class FrmMain : Form
    {
        /// <summary>
        /// 总数计数
        /// </summary>
        //private int total_count = 0;
        /// <summary>
        /// OK计数
        /// </summary>
        private int ok_count = 0;
        /// <summary>
        /// NG计数
        /// </summary>
        private int ng_count = 0;
        /// <summary>
        ///AI PASSCount
        /// </summary>
        //private int AI_PassCount = 0;

        //private int AI_PassImageCount = 0;

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
                if (msg.Count > 0)
                {
                    for (int i = 0; i < msg.Count; i++)
                    {
                        switch (msg[i])
                        {
                            case "0":
                                Machine.sysConfig.AIPassImageCount++;
                                break;
                            case "1":
                                ng_count++;
                                break;
                            case "2":
                                
                                break;
                        }

                    }
                }
                Machine.sysConfig.AVIImageCount += msg.Count;
                Machine.sysConfig.ByPassCount += msg.Where(t => t == "2").Count();
                //结果
                //sn = sn.Split('_').ToArray()[0].ToString();
                if (!FrHome.Instance.dic_Results.ContainsKey(sn))
                {
                    List<string> result = new List<string>();
                    result.AddRange(msg);
                    FrHome.Instance.dic_Results.Add(sn, result);
                }
                else
                {
                    List<string> result = null;
                    FrHome.Instance.dic_Results.TryGetValue(sn, out result);
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
                    List<string> result = null;
                    FrHome.Instance.dic_Details.TryGetValue(sn, out result);
                    result.AddRange(details);
                }

                //PCS缺陷坐标
                if (!FrHome.Instance.dic_PcsResult.ContainsKey(sn))
                {
                    PcsResult result = new PcsResult();
                    result.vb_List = new List<VBRcvInfp>();
                    result.vb_List.AddRange(pcsResult.vb_List);
                    FrHome.Instance.dic_PcsResult.Add(sn, result);
                }
                else
                {
                    PcsResult result;
                    FrHome.Instance.dic_PcsResult.TryGetValue(sn, out result);
                    result.vb_List.AddRange(pcsResult.vb_List);
                }

                //FrHome.Instance.lbl_AVICount.Invoke(new Action(() =>
                //{
                //    FrHome.Instance.lbl_AVICount.Text = $"今日AVI产生图片数:{Machine.sysConfig.AVIImageCount}";
                //}));
                //FrHome.Instance.lbl_ImageCount.Invoke(new Action(() =>
                //{
                //    FrHome.Instance.lbl_ImageCount.Text = $"今日推理图片数:{Machine.sysConfig.AVIImageCount-Machine.sysConfig.ByPassCount}";
                //}));
                ////推理图片数
                //this.lbl_ImageCount.Invoke(new MethodInvoker(() =>
                //{
                //    this.lbl_ImageCount.Text = $"当前推理图片数:{ok_count + ng_count}";
                //}));
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
                    List<RootPanelInfoWithIP> resInfo = null;
                    FrHome.Instance.dic_Infos.TryGetValue(sn, out resInfo);
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
        public void AddOrUpdateMachineData(string machineName, string materialNo, string workOrderNo)
        {
            //客户要求先屏蔽
            //FrHome.Instance.dataProductInfo.Invoke(new MethodInvoker(() =>
            //{
            //    var existingRow = FrHome.Instance.dataProductInfo.Rows
            //        .Cast<DataGridViewRow>()
            //        .FirstOrDefault(row =>
            //            row.Cells["MachineNumber"].Value?.ToString() == machineName &&
            //            !row.IsNewRow);

            //    if (existingRow != null)
            //    {
            //        existingRow.Cells["liaohao"].Value = materialNo;
            //        existingRow.Cells["lot"].Value = workOrderNo;
            //    }
            //    else
            //    {
            //        int rowIndex = FrHome.Instance.dataProductInfo.Rows.Add();
            //        DataGridViewRow newRow = FrHome.Instance.dataProductInfo.Rows[rowIndex];
            //        newRow.Cells["MachineNumber"].Value = machineName;
            //        newRow.Cells["liaohao"].Value = materialNo;
            //        newRow.Cells["lot"].Value = workOrderNo;
            //        FrHome.Instance.dataProductInfo.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Green;
            //    }
            //}));

        }
        private void SystemEvent_EventSendDefectNumToUI(int num)
        {
            //FrHome.Instance.InitTableStyle(FrHome.Instance.table_Small, num);
            //FrHome.Instance.InitWork();
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
            //有异常发生，暂停任务
            btnPause_Click(null, null);
            LogTextHelper.Warn($"收到异常消息：{massage},任务已停止");
        }
        public static object Locker = new object();
        private void SystemEvent_EventSendTaskToUI(object task, string msg = "")
        {
            try
            {
                lock (Locker)
                {
                    if (msg == "")
                    {
                        FrHome.Instance.dataGridViewData.Invoke(new MethodInvoker(() =>
                        {
                            FrHome.Instance.dataGridViewData.Rows.Insert(0, new List<string> { task.ToString(), "0", "0", "排队中" }.ToArray());
                            FrHome.Instance.dataGridViewData.Rows[0].DefaultCellStyle.ForeColor = Color.Yellow;

                        }));
                        //this.lbl_Count.Invoke(new MethodInvoker(() =>
                        //{
                        //    this.lbl_Count.Text = $"当前总作业数:{++total_count}";
                        //}));


                        //int total = ok_count + ng_count;

                    }
                    else
                    {
                        for (int i = 0; i < FrHome.Instance.dataGridViewData.Rows.Count; i++)
                        {
                            string sn = FrHome.Instance.dataGridViewData.Rows[i].Cells[0].Value.ToString();
                            if (sn == task.ToString())
                            {
                                //FrHome.Instance.dataGridViewData.Rows[i].Cells[1].Value = msg;
                                if (msg.Contains("已完成"))
                                {
                                    FrHome.Instance.dataGridViewData.Rows[i].DefaultCellStyle.ForeColor = Color.Green;
                                    //0808增加需求
                                    if (msg.Contains("B面"))
                                    {
                                        List<string> res_lbl = null;
                                        int Count = 0;
                                        int OK = 0;
                                        int NG = 0;
                                        int ByPass = 0;
                                        if (FrHome.Instance.dic_Results.TryGetValue(sn, out res_lbl))
                                        {
                                            Count = res_lbl.Count();
                                            OK = res_lbl.Where(o => o.Contains("0")).Count();
                                            NG = res_lbl.Where(o => o.Contains("1")).Count();
                                            ByPass = res_lbl.Where(o => o.Contains("2")).Count();
                                            msg = $"{msg}_{"图片一致"}_OK:{OK} NG:{NG} ByPass{ByPass}";
                                            if (NG == 0)
                                            {
                                                Machine.sysConfig.AIPassPCS++;
                                            }
                                        }
                                        FrHome.Instance.dataGridViewData.Rows[i].Cells[1].Value = Count;
                                        FrHome.Instance.dataGridViewData.Rows[i].Cells[2].Value = Count;
                                        FrHome.Instance.dataGridViewData.Rows[i].Cells[3].Value = msg;
                                        FrHome.Instance.str_SN = sn;
                                        FrHome.Instance.dataGridViewData_CellClick(null, null);
                                    }
                                }
                                else if (msg.Contains("处理"))
                                {
                                    FrHome.Instance.dataGridViewData.Rows[i].Cells[3].Value = msg;
                                    FrHome.Instance.dataGridViewData.Rows[i].DefaultCellStyle.ForeColor = Color.Blue;
                                }
                                else
                                {
                                    FrHome.Instance.dataGridViewData.Rows[i].Cells[3].Value = msg;
                                    FrHome.Instance.dataGridViewData.Rows[i].DefaultCellStyle.ForeColor = Color.Red;
                                }
                            }
                        }
                    }

                    if (FrHome.Instance.dataGridViewData.Rows.Count > 27)
                    {
                        //保证有8行已经处理的给用户查看 不然每次用户看到的都是正在处理的数据  看不到已完成的数据
                        if (FrHome.Instance.dataGridViewData.Rows[FrHome.Instance.dataGridViewData.Rows.Count - 8].Cells[3].Value.ToString().Contains("已完成"))
                        {
                            string SN = FrHome.Instance.dataGridViewData.Rows[FrHome.Instance.dataGridViewData.Rows.Count - 1].Cells[0].Value?.ToString() ?? "空值";
                            FrHome.Instance.dataGridViewData.Invoke(new MethodInvoker(() =>
                            {
                                FrHome.Instance.dataGridViewData.Rows.RemoveAt(FrHome.Instance.dataGridViewData.Rows.Count - 1);
                            }));
                            //移除显示信息
                            FrHome.Instance.dic_Infos.Remove(SN);
                            FrHome.Instance.dic_Paths.Remove(SN);
                            FrHome.Instance.dic_Results.Remove(SN);
                            FrHome.Instance.dic_PcsResult.Remove(SN);
                            FrHome.Instance.dic_Details.Remove(SN);
                        }
                        Machine.master.workClass.IsAllow = false;
                    }
                    else
                    {
                        Machine.master.workClass.IsAllow = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
            }
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

                ////拍照
                //FrmMain.Instance.panel3.Controls.Clear();
                //FrCamera.Instance.TopLevel = false;
                //FrCamera.Instance.Parent = FrmMain.Instance.panel3;
                //FrCamera.Instance.Dock = DockStyle.Fill;
                //FrCamera.Instance.Show();

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

                ////Fn
                //FrmMain.Instance.panel6.Controls.Clear();
                //FrFn.Instance.TopLevel = false;
                //FrFn.Instance.Parent = FrmMain.Instance.panel6;
                //FrFn.Instance.Dock = DockStyle.Fill;
                //FrFn.Instance.Show();

                ////查询点检
                //FrmMain.Instance.panel7.Controls.Clear();
                //FrSearch.Instance.TopLevel = false;
                //FrSearch.Instance.Parent = FrmMain.Instance.panel7;
                //FrSearch.Instance.Dock = DockStyle.Fill;
                //FrSearch.Instance.Show();

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
            //if (FrHome.Instance.DispWin1 != null)
            //{
            //    FrHome.Instance.DispWin1[0].Refresh();

            //}
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

                btnStart.Image = Resources.start2;
                btnPause.Image = Resources.pause1;
                Machine.master.workClass.isStart = true;
                //开启Gennt 如果agent开，就不开，如果没开，就开
                //FrSetting.Instance.RunAppLication(FrSetting.Instance.appPath, FrSetting.Instance.appExe);
                FrSetting.Instance.RestartApplication(FrSetting.Instance.appPath, FrSetting.Instance.appExe, true);
                //this.Invoke(new MethodInvoker(() =>
                //{
                //    Machine.master.workClass.defect.ai_Defect.Vision_Show_View(0);
                //}));
                LogTextHelper.Info("开始作业...");
            }
            catch (Exception)
            {

                throw;
            }

        }
        private void btnPause_Click(object sender, EventArgs e)
        {
            return;
            Machine.master.workClass.isStart = false;
            btnStart.Image = Resources.start1;
            btnPause.Image = Resources.pause2;
            //关闭Gennt
            FrSetting.Instance.KillProcessInDirectory(FrSetting.Instance.appPath, FrSetting.Instance.appExe);

            LogTextHelper.Info("暂停作业...");
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

                //case FormMode.CameraForm:

                //    if (curFormMode != FormMode.CameraForm)
                //    {
                //        curFormMode = FormMode.CameraForm;

                //        btnCamera.Image = Resources.pho2;

                //        FrmMain.Instance.panel3.Dock = DockStyle.Fill;

                //        FrmMain.Instance.panel1.Visible = false;
                //        FrmMain.Instance.panel2.Visible = false;
                //        FrmMain.Instance.panel3.Visible = true;
                //        FrmMain.Instance.panel4.Visible = false;
                //        FrmMain.Instance.panel5.Visible = false;
                //        FrmMain.Instance.panel6.Visible = false;
                //        FrmMain.Instance.panel7.Visible = false;
                //    }
                //    break;

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

                    //case FormMode.FnForm:

                    //    if (curFormMode != FormMode.FnForm)
                    //    {
                    //        curFormMode = FormMode.FnForm;

                    //        btnFn.Image = Resources.Fn2;

                    //        FrmMain.Instance.panel6.Dock = DockStyle.Fill;

                    //        FrmMain.Instance.panel1.Visible = false;
                    //        FrmMain.Instance.panel2.Visible = false;
                    //        FrmMain.Instance.panel3.Visible = false;
                    //        FrmMain.Instance.panel4.Visible = false;
                    //        FrmMain.Instance.panel5.Visible = false;
                    //        FrmMain.Instance.panel6.Visible = true;
                    //        FrmMain.Instance.panel7.Visible = false;
                    //    }
                    //    break;

                    //case FormMode.SearchForm:

                    //    if (curFormMode != FormMode.SearchForm)
                    //    {
                    //        curFormMode = FormMode.SearchForm;

                    //        btnSearch.Image = Resources.search2;

                    //        FrmMain.Instance.panel7.Dock = DockStyle.Fill;

                    //        FrmMain.Instance.panel1.Visible = false;
                    //        FrmMain.Instance.panel2.Visible = false;
                    //        FrmMain.Instance.panel3.Visible = false;
                    //        FrmMain.Instance.panel4.Visible = false;
                    //        FrmMain.Instance.panel5.Visible = false;
                    //        FrmMain.Instance.panel6.Visible = false;
                    //        FrmMain.Instance.panel7.Visible = true;
                    //    }
                    //    break;
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
            ok_count = 0;
            ng_count = 0;
            Machine.sysConfig.TotalCount = 0;
            Machine.sysConfig.AIPassPCS = 0;
            Machine.sysConfig.AVIImageCount = 0;
            Machine.sysConfig.AIPassImageCount = 0;
            Machine.sysConfig.ByPassCount = 0;

        }

        #endregion 状态栏-运行时间-当前时间

        private async void toolStripMenuItem2_Click(object sender, EventArgs e)
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
            //////存储Lot与SN的关系
            ////RootDbInfo dbInfo = new RootDbInfo();
            ////dbInfo.db_name = "panel_list";
            ////dbInfo.operation = "put";
            ////dbInfo.op_mode = "ap";
            //////lot
            ////dbInfo.key = "444";
            ////dbInfo.value = "1";

            ////string Result;
            //////Task.Factory.StartNew(() =>
            //////{
            ////Machine.master.workClass.http_DB.HttpPostMethod("http://127.0.0.1:9877", dbInfo, 1, out Result);

            ////RootDbInfo dbInfo1 = new RootDbInfo();
            ////dbInfo1.db_name = "panel_list";
            ////dbInfo1.operation = "put";
            ////dbInfo1.op_mode = "ap";
            //////lot
            ////dbInfo1.key = "444";
            ////dbInfo1.value = "2";

            //////Task.Factory.StartNew(() =>
            //////{
            ////Machine.master.workClass.http_DB.HttpPostMethod("http://127.0.0.1:9877", dbInfo1, 1, out Result);



            //RootDbInfo info = new RootDbInfo();
            //info.db_name = "panel_list";
            //info.key = "600001234567_0002"; 
            //info.op_mode = "all";
            //info.uniqueKey = Guid.NewGuid().ToString();
            //info.operation = "get";
            //string outInfo = null;
            ////Machine.master.workClass.URL = "http://192.168.77.243:9877";
            //Machine.master.workClass.http_DB.HttpPostMethod("http://192.168.77.193:9877", info, 0, out outInfo);

            //if (outInfo != null)
            //{
            //    //解析VALUE拿到信息
            //    var json = JObject.Parse(outInfo);
            //    string res = json["value"].ToString();
            //    if (outInfo.Contains("err_key_found") || res == "")
            //    {

            //    }
            //    //取value里面的value
            //    //var val = JObject.Parse(res);
            //    //string res1 = val["value"].ToString().TrimEnd(';');
            //    List<string> ress= res.Split(';').ToList();
            //}
            ////var msg = File.ReadAllText($"E:/minio/deepiresults/20250712/20250712110043868/20250712110144273929/0001-vb.json");
            //////msg= msg.Replace("\"", "").Replace("\\", "\"");
            //////string unescapedJson = Regex.Unescape(msg);

            ////var obj = JsonConvert.DeserializeObject<RootVBOutInfo>(msg);
            ////Machine.master.workClass.minio.BuildClient("127.0.0.1", "9102");
            //string r = "D:/minio/deepiresults/20250813/2i02j0aaanc00/A/20250813235326770521";
            //string result_path = "D:/minio/deepiresults/20250813/2i02j0aaanc00/A/20250813235326770521/ICGoldFinger_AU Scratch DL/2527543301 2710-A-ICGoldFinger_AU Scratch DL-pcs-X1Y10-vrs0-0.jpg";//"E:\\minio\\deepiresults\\20250812\\2i02j0aaanc00\\A\\20250813235326770521";
            ////string result_path1 = "C:/minio/deepiresults/real_ats_data\\605000338587-0001\\000001B\\lot_id_605000338587-0001_sn_0001_panel_index_000001B-panel.json";


            //string path = string.Empty;
            //string result = string.Empty;
            //Machine.master.workClass.ParseMinioPath(result_path, out path, out result);
            ////await Task.Factory.StartNew(() =>
            ////{
            ////    Machine.master.workClass.minio.Download("deepiresults", $"{path}/", $"E:/minio/MinioDownLoad/{path}", "127.0.0.1");
            ////});

            //btn_showBox.Checked = !btn_showBox.Checked;
            //if (!btn_showBox.Checked)
            //{
            //    Machine.master.workClass.isShowBox = false;
            //}
            //else
            //{
            //    Machine.master.workClass.isShowBox = true;
            //}
            ////功能屏蔽


            //return;
            ////测试读写json
            //var info = File.ReadAllText(@"D:\lot_id_600002890251-0001_panel_index_000019A-panel.json");
            //var jsoninfo = JsonConvert.DeserializeObject<RootPanelInfo>(info);
            //return;

            ////临时用来测试算法接口
            //if (!Machine.master.workClass.TestFlag)
            //{
            //    MessageBox.Show("当前系统处于生产模式,请切换为样品板测试模式", "模式提示", MessageBoxButtons.OK, MessageBoxIcon.Question);
            //    return;
            //}

            //OpenFileDialog dig_openImage = new OpenFileDialog
            //{
            //    Title = "请选择图像文件",
            //    Filter = string.Format("{0} | *.bmp;*.jpg;*.jpeg;*.gif;*.png;*.tif", "图片文件"),
            //    RestoreDirectory = true,
            //    FilterIndex = 1
            //};
            //if (dig_openImage.ShowDialog() == DialogResult.OK)
            //{
            //    //展示VB
            //    // Machine.master.workClass.defect.ai_Defect.Vision_Show_View(1);
            //    //传图给VB
            //    string path = string.Empty;
            //    string result = string.Empty;
            //    Machine.master.workClass.ParseMinioPath(dig_openImage.FileName, out path, out result);

            //    //var res = Machine.master.workClass.ParseMinioPath(dig_openImage.FileName);
            //    string defectcode = path.Split('/')[1];

            //    RootVBInfo vBInfo = new RootVBInfo();
            //    vBInfo.MessageType = "visionbuilder_inference";
            //    vBInfo.paramsData = new ParamsData();
            //    //同一个任务的UUID是否要保持一致；
            //    vBInfo.paramsData.InferResUuid = Guid.NewGuid().ToString();
            //    vBInfo.paramsData.InferWholeData = new InferWholeData();
            //    vBInfo.paramsData.InferWholeData.ImageInferParams = new ImageInferParams();
            //    //solution_name
            //    //vBInfo.paramsData.InferWholeData.ImageInferParams.PipelineName = "Test";
            //    vBInfo.paramsData.InferWholeData.ImageInferParams.PipelineName = Machine.solution;
            //    vBInfo.paramsData.InferWholeData.ImageInferParams.NodeParams = new List<NodeParam>();
            //    vBInfo.paramsData.InferWholeData.ImageInferParams.NodeParams.Add(
            //      new NodeParam()
            //      {
            //          NodeName = Machine.flow,//"1",
            //          height = 200,
            //          width = 200,
            //      });

            //    vBInfo.paramsData.InferWholeData.ImageData = new ImageData();
            //    //#使⽤minio获取 则固定字段"minio"
            //    vBInfo.paramsData.InferWholeData.ImageData.DataType = "minio";
            //    vBInfo.paramsData.InferWholeData.ImageData.DataValue = new DataValue();
            //    vBInfo.paramsData.InferWholeData.ImageData.DataValue.InferImageGroup = new List<InferImageGroup>();

            //    InferImageGroup group = new InferImageGroup();
            //    group.GroupUuid = Guid.NewGuid().ToString();
            //    group.GroupInfos = new List<GroupInfo>();
            //    group.DefectCode = "";
            //    //这里判断当前是否应该调用Switch
            //    if (Machine.isSwitch)
            //    {
            //        group.DefectCode = defectcode;//"GP_AOC";
            //    }
            //    //for (int k = 0; k < 3; k++)
            //    for (int k = 0; k < 1; k++)
            //    {
            //        switch (k)
            //        {
            //            case 0:
            //                group.GroupInfos.Add(new GroupInfo()
            //                {
            //                    ImagePath =path, //"20250508152421059165/GP_AOC/p0_p1_p2_p3_GP_AOC_0_p6_p7_p8_924_0.jpg",
            //                    ImageUuid = Guid.NewGuid().ToString(),
            //                    ImageType = "defect",
            //                });
            //                break;
            //            //case 1:
            //            //    group.GroupInfos.Add(new GroupInfo()
            //            //    {
            //            //        ImagePath = "20250508152421059165/discolor/20250508152421059165-A-discolor-pcs-X1Y1-vrs0-0-template-1.jpg",
            //            //        ImageUuid = Guid.NewGuid().ToString(),
            //            //        ImageType = "template",
            //            //    });
            //            //    break;
            //            //case 2:
            //            //    group.GroupInfos.Add(new GroupInfo()
            //            //    {
            //            //        ImagePath = "20250508152421059165/discolor/20250508152421059165-A-discolor-pcs-X1Y1-vrs0-0-gerber-1.jpg",
            //            //        ImageUuid = Guid.NewGuid().ToString(),
            //            //        ImageType = "gerber",
            //            //    });
            //            //    break;
            //            default:
            //                break;
            //        }
            //    }

            //    vBInfo.paramsData.InferWholeData.ImageData.DataValue.InferImageGroup.Add(group);
            //    group.inspectDetails = new InspectDetails();
            //    group.inspectDetails.InferRois = new List<InferRoi>() { };

            //    vBInfo.paramsData.InferWholeData.OtherInfos = new Others();
            //    vBInfo.paramsData.InferWholeData.OtherInfos.imageminio = new ImageMminio();
            //    vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.access_key_id = "deepiobjectdata";
            //    vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.bucket = "deepiresults";
            //    vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.endpoint_url = "127.0.0.1";
            //    vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.secret_key = "deepiobject2019";
            //    vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.secret_port = "9102";

            //    List<string> resultlist = null;
            //    List<string> details = null;
            //    PcsResult pcsResult;
            //    //await Task.Factory.StartNew(() =>
            //    //{
            //    Machine.master.workClass.DefectMethod(vBInfo, out resultlist, out details, out pcsResult);
            //    // });
            //}
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
            if (Machine.master.workClass.isStart || FrHome.Instance.dataGridViewData.Rows.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show("当前系统正在运行中,是否继续切换模式？", "切换提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Cancel)
                {
                    return;
                }
            }

            btnPause_Click(null, null);

            for (int i = 0; i < FrHome.Instance.dataGridViewData.Rows.Count; i++)
            {
                FrHome.Instance.dataGridViewData.Rows.Clear();

            }
            for (int i = 0; i < FrHome.Instance.DispWin2.Count(); i++)
            {
                //FrHome.Instance.DispWin2[i].Dispose();
                FrHome.Instance.DispWin2[i].Clear();
            }
            FrHome.Instance.dic_Infos.Clear();
            FrHome.Instance.dic_Results.Clear();
            FrHome.Instance.dic_Details.Clear();
            FrHome.Instance.dic_PcsResult.Clear();
            FrHome.Instance.dic_Paths.Clear();
            while (Machine.master.workClass.que_AVI.Count > 0)
            {
                Thread.Sleep(10);
                VBModel model = null;
                Machine.master.workClass.que_AVI.TryDequeue(out model);
            }
            while (Machine.master.workClass.que_AI.Count > 0)
            {
                Thread.Sleep(10);
                Tuple<string, string, string, RootAIResult> info = null;
                Machine.master.workClass.que_AI.TryDequeue(out info);
            }
            if (this.btnModel.Text == "生产模式")
            {
                this.btnModel.Text = "样品板测试";
                Machine.master.workClass.TestFlag = true;


                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = "请选择所在文件夹";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (string.IsNullOrEmpty(dialog.SelectedPath))
                    {
                        MessageBox.Show(this, "文件夹路径不能为空", "提示");
                        return;
                    }
                    string SelectedPath = dialog.SelectedPath;
                    string[] directories = Directory.GetDirectories(SelectedPath);

                    // 遍历并输出文件夹名称
                    foreach (string directory in directories)
                    {
                        string folderName = Path.GetFileName(directory);
                        if (folderName.Contains("unknown"))
                        {
                            continue;
                        }
                        SystemEvent.SendTaskMsg(folderName);
                        string[] directories2 = Directory.GetDirectories(directory);
                        foreach (string directory2 in directories2)
                        {
                            string[] resultFiles = Directory.GetFiles(directory2, "*panel.json");

                            string path = string.Empty;
                            string result = string.Empty;
                            Machine.master.workClass.ParseMinioPath(resultFiles[0], out path, out result);

                            if (!FrHome.Instance.dic_Paths.ContainsKey(folderName))
                            {
                                List<string> res = new List<string>();
                                res.Add(resultFiles[0]);
                                FrHome.Instance.dic_Paths.Add(folderName, res);
                            }
                            else
                            {
                                List<string> res = null;
                                FrHome.Instance.dic_Paths.TryGetValue(folderName, out res);
                                res.Add(resultFiles[0]);
                            }
                            await Task.Factory.StartNew(() =>
                            {
                                Machine.master.workClass.ReadJsonByMinio("127.0.0.1", "9102", "111", result, folderName, "A", path);
                            });
                        }
                    }
                    IsAllow = false;
                }
                else
                {
                    IsAllow = true;
                }
            }
            else if (this.btnModel.Text == "样品板测试")
            {
                this.btnModel.Text = "生产模式";
                Machine.master.workClass.TestFlag = false;
                this.Invoke(new MethodInvoker(() =>
                {
                    Machine.master.workClass.defect.ai_Defect.Vision_Show_View(0);
                }));
                IsAllow = false;
            }
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
            Task.Run(() =>
            {
                while (Machine.master.workClass.isStart && Machine.master.workClass.TestFlag)
                {
                    if (!Machine.master.workClass.TestFlag)
                    {
                        //切换模式直接退出
                        break;
                    }
                    if (!IsAllow)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    Thread.Sleep(500);
                    string name = "20250521000015209608_";// + total_count;
                    if (!Directory.Exists("D:\\minio\\deepiresults\\0607\\20250521000015209608\\20250521000015209608\\20250521000015209608-panel.json"))
                    {
                        MessageBox.Show("循环测试文件不存在！", "文件不存在", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    }
                    if (!FrHome.Instance.dic_Paths.ContainsKey(name))
                    {
                        List<string> result = new List<string>();
                        result.Add("E:\\minio\\deepiresults\\0607\\20250521000015209608\\20250521000015209608\\20250521000015209608-panel.json");
                        FrHome.Instance.dic_Paths.Add(name, result);
                    }
                    else
                    {
                        List<string> result = null;
                        FrHome.Instance.dic_Paths.TryGetValue(name, out result);
                        result.Add("E:\\minio\\deepiresults\\0607\\20250521000015209608\\20250521000015209608\\20250521000015209608-panel.json");
                    }
                    SystemEvent.SendTaskMsg(name);
                    Machine.master.workClass.ReadJsonByMinio("127.0.0.1", "9102", "111", "0607/20250521000015209608/20250521000015209608", name,
                                                 "A", "0607/20250521000015209608/20250521000015209608/20250521000015209608-panel.json");


                }
            });
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

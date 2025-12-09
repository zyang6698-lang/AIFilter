using DeepSightAI.SettingPages;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class FrSetting : Form
    {
        public FrSetting()
        {
            InitializeComponent();


            treeNode1 = new System.Windows.Forms.TreeNode("常规配置");
            treeNode2 = new System.Windows.Forms.TreeNode("算法方案配置");
            treeNode3 = new System.Windows.Forms.TreeNode("机台配置");
            treeNode4 = new System.Windows.Forms.TreeNode("相机配置");
            //treeNode5 = new System.Windows.Forms.TreeNode("运动控制");
            //treeNode6 = new System.Windows.Forms.TreeNode("IO监控");
            //treeNode7 = new System.Windows.Forms.TreeNode("用户管理");

            treeNode1.Name = "节点0";
            treeNode1.Text = "常规配置";
            treeNode2.Name = "节点0";
            treeNode2.Text = "算法方案配置";
            treeNode3.Name = "节点0";
            treeNode3.Text = "机台配置";
            treeNode4.Name = "节点0";
            treeNode4.Text = "相机配置";
            //treeNode5.Name = "节点0";
            //treeNode5.Text = "运动控制";
            //treeNode6.Name = "节点0";
            //treeNode6.Text = "IO监控";
            //treeNode7.Name = "节点0";
            //treeNode7.Text = "用户管理";

            this.tvw_setting.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            //treeNode5,
            //treeNode6,
            //treeNode7,
            });

            Load += FrSetting_Load;
        }

        /// <summary>
        /// 窗体对象实例
        /// </summary>
        private static FrSetting _instance;

        public static FrSetting Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrSetting();
                }
                return _instance;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrSetting_Load(object sender, EventArgs e)
        {
            try
            {

                LoadMethod();
                tvw_setting.SelectedNode = tvw_setting.Nodes[0];

                Language(1);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }
        System.Windows.Forms.TreeNode treeNode1;
        System.Windows.Forms.TreeNode treeNode2;
        System.Windows.Forms.TreeNode treeNode3;
        System.Windows.Forms.TreeNode treeNode4;
        //System.Windows.Forms.TreeNode treeNode5;
        //System.Windows.Forms.TreeNode treeNode6;
        //System.Windows.Forms.TreeNode treeNode7;
        public void Language(int language)
        {
            if (language == 1)
            {

                btnSave.Text = "保存";
                this.treeNode1.Text = "常规配置";
                this.treeNode2.Text = "算法方案配置";
                this.treeNode3.Text = "机台配置";
                this.treeNode4.Text = "工具配置";
            }
            else
            {
                btnSave.Text = "Save";
                this.treeNode1.Text = "conventional";
                this.treeNode2.Text = "AVI";
                this.treeNode3.Text = "AI";
                this.treeNode4.Text = "工具配置";
            }

        }


        internal void LoadMethod()
        {
            try
            {
                //常规
                panel1.Controls.Clear();
                FrBaseConfig.Instance.TopLevel = false;
                FrBaseConfig.Instance.Parent = panel1;
                FrBaseConfig.Instance.Dock = DockStyle.Fill;
                FrBaseConfig.Instance.Show();

                //
                panel2.Controls.Clear();
                FrAIConfig.Instance.TopLevel = false;
                FrAIConfig.Instance.Parent = panel2;
                FrAIConfig.Instance.Dock = DockStyle.Fill;
                FrAIConfig.Instance.Show();

                ///
                panel3.Controls.Clear();
                FrHWConfig.Instance.TopLevel = false;
                FrHWConfig.Instance.Parent = panel3;
                FrHWConfig.Instance.Dock = DockStyle.Fill;
                FrHWConfig.Instance.Show();

                ////制作料号
                panel4.Controls.Clear();
                FrCreateMaterial.Instance.TopLevel = false;
                FrCreateMaterial.Instance.Parent = panel4;
                FrCreateMaterial.Instance.Dock = DockStyle.Fill;
                FrCreateMaterial.Instance.Show();

                ////设备变更
                //panel5.Controls.Clear();
                //FrChangeInfo.Instance.TopLevel = false;
                //FrChangeInfo.Instance.Parent = panel5;
                //FrChangeInfo.Instance.Dock = DockStyle.Fill;
                //FrChangeInfo.Instance.Show();

                ////点检维护
                //panel6.Controls.Clear();
                //FrCheckMonthInfo.Instance.TopLevel = false;
                //FrCheckMonthInfo.Instance.Parent = panel6;
                //FrCheckMonthInfo.Instance.Dock = DockStyle.Fill;
                //FrCheckMonthInfo.Instance.Show();

                ////后处理调参
                panel7.Controls.Clear();
                FrUserManagement.Instance.TopLevel = false;
                FrUserManagement.Instance.Parent = panel7;
                FrUserManagement.Instance.Dock = DockStyle.Fill;
                FrUserManagement.Instance.Show();

                ////测厚参数配置
                //panel8.Controls.Clear();
                //FrmThickness.Instance.TopLevel = false;
                //FrmThickness.Instance.Parent = panel8;
                //FrmThickness.Instance.Dock = DockStyle.Fill;
                //FrmThickness.Instance.Show();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvw_setting_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(Machine.LoginUserName))
                //{
                //    btnSave.Enabled = true;
                //}
                //else if (Machine.LoginUserName.Contains("操作员") || Machine.LoginUserName.Contains("Operator"))
                //{
                //    btnSave.Enabled = true;
                //}
                //else if (Machine.LoginUserName.Contains("工程师") || Machine.LoginUserName.Contains("管理员"))
                //{
                //    btnSave.Enabled = true;
                //}
                btnSave.Visible = true;
                TreeNode node = tvw_setting.SelectedNode;
                switch (node.Text)
                {
                    case "常规配置":
                        panel1.Dock = DockStyle.Fill;

                        panel1.Visible = true;
                        panel2.Visible = false;
                        panel3.Visible = false;
                        panel4.Visible = false;
                        panel5.Visible = false;
                        panel6.Visible = false;
                        panel7.Visible = false;
                        panel8.Visible = false;

                        break;

                    case "算法方案配置":
                        panel2.Dock = DockStyle.Fill;

                        panel1.Visible = false;
                        panel2.Visible = true;
                        panel3.Visible = false;
                        panel4.Visible = false;
                        panel5.Visible = false;
                        panel6.Visible = false;
                        panel7.Visible = false;
                        panel8.Visible = false;

                        break;

                    case "机台配置":
                        panel3.Dock = DockStyle.Fill;

                        panel1.Visible = false;
                        panel2.Visible = false;
                        panel3.Visible = true;
                        panel4.Visible = false;
                        panel5.Visible = false;
                        panel6.Visible = false;
                        panel7.Visible = false;
                        panel8.Visible = false;

                        break;

                    case "工具配置":
                        panel4.Dock = DockStyle.Fill;

                        panel1.Visible = false;
                        panel2.Visible = false;
                        panel3.Visible = false;
                        panel4.Visible = true;
                        panel5.Visible = false;
                        panel6.Visible = false;
                        panel7.Visible = false;
                        panel8.Visible = false;

                        break;

                    case "用户管理":
                    case "user":
                        panel7.Dock = DockStyle.Fill;

                        panel1.Visible = false;
                        panel2.Visible = false;
                        panel3.Visible = false;
                        panel4.Visible = false;
                        panel5.Visible = false;
                        panel6.Visible = false;
                        panel7.Visible = true;
                        panel8.Visible = false;
                        break;
                        //case "测厚参数配置":
                        //case "Thickness Parameters":
                        //    panel8.Dock = DockStyle.Fill;

                        //    panel1.Visible = false;
                        //    panel2.Visible = false;
                        //    panel3.Visible = false;
                        //    panel4.Visible = false;
                        //    panel5.Visible = false;
                        //    panel6.Visible = false;
                        //    panel7.Visible = false;
                        //    panel8.Visible = true;
                        //    FrmThickness.Instance.Language(Machine.sysConfig.Language);
                        //    break;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_saveSetting_Click(object sender, EventArgs e)
        {
            try
            {
                if (tvw_setting.SelectedNode.Text == "常规配置")
                {
                    FrBaseConfig.Instance.GetBaseParams();
                    if (Machine.config_class.Save(Machine.sysConfig))
                    {
                        Machine.master.workClass.sysConfig = Machine.sysConfig;
                        MessageBox.Show("保存配置文件成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                if (tvw_setting.SelectedNode.Text == "机台配置")
                {
                    // 检查软件是否处于运行状态
                    if (Machine.master != null && Machine.master.workClass != null && Machine.master.workClass.isStart)
                    {
                        MessageBox.Show("软件正在运行中，请先停止运行后再保存机台配置！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    FrHWConfig.Instance.GetStationParam();
                    if (Machine.avi_class.Save(Machine.aviconfig))
                    {
                        RestartApplication(appPath, appExe);
                        Machine.master.workClass.aviconfig = Machine.aviconfig;
                        // 更新FrHome中的AviCtr状态
                        FrHome.Instance.RefreshAviCtrConfigs();
                        MessageBox.Show("保存Agent配置文件成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                }
                if (tvw_setting.SelectedNode.Text == "算法方案配置")
                {
                    if (FrAIConfig.Instance.SaveParam())
                    {
                        Machine.master.workClass.solconfig = Machine.solconfig;
                        MessageBox.Show("方案及流程配置保存成功", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                }
            }
            catch (Exception)
            {
                throw;
            }
            //try
            //{
            //    if (tvw_setting.SelectedNode.Text == "常规")
            //    {
            //        if (Machine.objAdmin.GeneralSet != 1)
            //        {
            //            FrMessageBox.Instance.MessageBoxShow($"当前登录用户:{Machine.objAdmin.LoginName}\r\n不具有更改常规设置权限", TipType.Warn);
            //            return;
            //        }
            //        else
            //        {
            //            GetBaseParam();
            //            FrCameraConfig.Instance.GetStationParam();
            //            if (Machine.config_class.Save(Machine.sysConfig))
            //            {
            //                FrMessageBox.Instance.MessageBoxShow(string.Format("\r\n {0}", Machine.sysConfig.Language == 1 ? "保存参数成功！" : "Saving parameters succeeded !"), TipType.Tip);
            //                LogTextHelper.Enable = Machine.sysConfig.LogEnable;
            //                for (int i = 0; i < Machine.sysConfig.stationParam.Count; i++)
            //                {
            //                    try
            //                    {
            //                        Machine.masterWorkClass.stationWorkClass[i].SetBaseConfig(Machine.sysConfig.ImagePath, Machine.sysConfig.CutImagePath, Machine.sysConfig.ImageEnable,
            //                            Machine.sysConfig.ImageSuffix, Machine.sysConfig.SharedIP, Machine.sysConfig.SharedUserName,
            //                            Machine.sysConfig.SharedUserPwd, Machine.sysConfig.TestFlag, Machine.sysConfig.CutFlag,
            //                            Machine.sysConfig.FiltrationFlag, Machine.sysConfig.VisualizeFlag, Machine.sysConfig.SnapImgSaveFlag,
            //                            Machine.sysConfig.AgainSnap, Machine.sysConfig.ShowFilt, Machine.sysConfig.CutImgSaveFlag, Machine.sysConfig.SaveJsonFlag, Machine.sysConfig.ResultImgResize);
            //                        Machine.masterWorkClass.stationWorkClass[i].SeStationConfig(Machine.sysConfig.stationParam[i]);
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        LogTextHelper.Error("Error", ex);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    else if (tvw_setting.SelectedNode.Text == "项目")
            //    {
            //        if (Machine.objAdmin.ProSet != 1)
            //        {
            //            FrMessageBox.Instance.MessageBoxShow($"当前登录用户:{Machine.objAdmin.LoginName}\r\n不具有更改项目权限", TipType.Warn);
            //            return;
            //        }
            //        else
            //        {
            //            GetBaseParam();
            //            FrCameraConfig.Instance.GetStationParam();
            //            if (Machine.config_class.Save(Machine.sysConfig))
            //            {
            //                FrMessageBox.Instance.MessageBoxShow(string.Format("\r\n {0}", Machine.sysConfig.Language == 1 ? "保存参数成功！" : "Saving parameters succeeded !"), TipType.Tip);
            //                LogTextHelper.Enable = Machine.sysConfig.LogEnable;
            //                for (int i = 0; i < Machine.sysConfig.stationParam.Count; i++)
            //                {
            //                    try
            //                    {
            //                        Machine.masterWorkClass.stationWorkClass[i].SetBaseConfig(Machine.sysConfig.ImagePath, Machine.sysConfig.CutImagePath, Machine.sysConfig.ImageEnable,
            //                            Machine.sysConfig.ImageSuffix, Machine.sysConfig.SharedIP, Machine.sysConfig.SharedUserName,
            //                            Machine.sysConfig.SharedUserPwd, Machine.sysConfig.TestFlag, Machine.sysConfig.CutFlag,
            //                            Machine.sysConfig.FiltrationFlag, Machine.sysConfig.VisualizeFlag, Machine.sysConfig.SnapImgSaveFlag,
            //                            Machine.sysConfig.AgainSnap, Machine.sysConfig.ShowFilt, Machine.sysConfig.CutImgSaveFlag, Machine.sysConfig.SaveJsonFlag, Machine.sysConfig.ResultImgResize);
            //                        Machine.masterWorkClass.stationWorkClass[i].SeStationConfig(Machine.sysConfig.stationParam[i]);
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        LogTextHelper.Error("Error", ex);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogTextHelper.Error("Error", ex);
            //}
        }

        public void RestartApplication(string appDirectory, string exeName, bool isRun = false)
        {
            bool result = KillProcessInDirectory(appDirectory, exeName);
            Thread.Sleep(1000);
            if (result || isRun)
            {
                StartProcessFromDirectory(appDirectory, exeName);
            }

        }
        public void StartProcessFromDirectory(string directoryPath, string exeName)
        {
            string fullPath = Path.Combine(directoryPath, exeName);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"程序不存在: {fullPath}");
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fullPath,
                WorkingDirectory = directoryPath,
                UseShellExecute = true // 如果需要显示窗口
            };

            Process.Start(startInfo);
            Console.WriteLine($"已启动程序: {fullPath}");
        }

        public void RunAppLication(string appDirectory, string exeName)
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeName));
            //如果agent开，就不开，如果没开，就开
            if (processes.Length > 0)
            {
                return;
            }
            RestartApplication(appDirectory, exeName, true);
        }
        /// <summary>
        /// 通过Windows事件信号通知ATS_Agent进程优雅关闭
        /// </summary>
        /// <param name="directoryPath">进程目录路径</param>
        /// <param name="exeName">进程名称</param>
        /// <param name="timeoutMs">等待进程退出的超时时间（毫秒），默认10秒</param>
        /// <returns>如果进程存在并成功关闭返回true</returns>
        public bool KillProcessInDirectory(string directoryPath, string exeName, int timeoutMs = 10000)
        {
            bool result = false;
            // 获取所有同名进程
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeName));

            // 找到匹配目录的进程
            List<Process> targetProcesses = new List<Process>();
            foreach (Process process in processes)
            {
                try
                {
                    if (process.MainModule != null &&
                        Path.GetDirectoryName(process.MainModule.FileName)?.Equals(directoryPath, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        targetProcesses.Add(process);
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"获取进程信息失败: {ex.Message}");
                }
            }

            if (targetProcesses.Count == 0)
            {
                return false;
            }

            try
            {
                // 尝试打开已存在的Windows事件
                using (EventWaitHandle shutdownEvent = EventWaitHandle.OpenExisting(@"Global\ATS_Agent_Shutdown_Event"))
                {
                    // 发送关闭信号
                    shutdownEvent.Set();
                    LogTextHelper.Info("已发送关闭信号到 ATS_Agent");

                    // 等待所有目标进程退出
                    foreach (Process process in targetProcesses)
                    {
                        try
                        {
                            if (process.WaitForExit(timeoutMs))
                            {
                                LogTextHelper.Info($"进程 {process.Id} 已正常退出");
                                result = true;
                            }
                            else
                            {
                                // 超时后强制终止
                                LogTextHelper.Warn($"进程 {process.Id} 等待超时，强制终止");
                                process.Kill();
                                process.WaitForExit(5000);
                                result = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogTextHelper.Error($"等待进程退出失败: {ex.Message}");
                        }
                    }
                }
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // 事件不存在，说明ATS_Agent可能没有运行或未创建事件，使用强制终止
                LogTextHelper.Warn("关闭事件不存在，使用强制终止方式");
                foreach (Process process in targetProcesses)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                        LogTextHelper.Info($"已强制终止进程: {process.Id}");
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"强制终止进程失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"发送关闭信号失败: {ex.Message}");
                // 出现异常时也尝试强制终止
                foreach (Process process in targetProcesses)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                        result = true;
                    }
                    catch { }
                }
            }

            return result;
        }
        // 使用示例
        public string appPath = System.AppDomain.CurrentDomain.BaseDirectory + "ATS_Agent_EXE";// @"D:\DSCode\DeepSightAI\Bin\ATS_Agent_EXE";
        public string appExe = "ATS_Agent.exe";

        private void GetBaseParam()
        {
            try
            {
                //Machine.sysConfig.ProjectName = FrBaseConfig.Instance.txt_ProjectName.Text;
                //Machine.sysConfig.Line = FrBaseConfig.Instance.txt_Line.Text;
                //Machine.sysConfig.ImagePath = FrBaseConfig.Instance.txt_ImagePath.Text;
                //Machine.sysConfig.CutImagePath = FrBaseConfig.Instance.txt_cutPath.Text;
                //if (FrBaseConfig.Instance.radioImg1.Checked)
                //{
                //    Machine.sysConfig.ImageEnable = 0;
                //}
                //if (FrBaseConfig.Instance.radioImg2.Checked)
                //{
                //    Machine.sysConfig.ImageEnable = 1;
                //}
                //if (FrBaseConfig.Instance.radioImg3.Checked)
                //{
                //    Machine.sysConfig.ImageEnable = 2;
                //}

                //Machine.sysConfig.LogDay = (int)FrBaseConfig.Instance.txt_log_day.Value;
                //if (FrBaseConfig.Instance.radioLog1.Checked)
                //{
                //    Machine.sysConfig.LogEnable = true;
                //}
                //if (FrBaseConfig.Instance.radioLog2.Checked)
                //{
                //    Machine.sysConfig.LogEnable = false;
                //}

                //Machine.sysConfig.shifts.DayShift = FrBaseConfig.Instance.txt_DayShift.Value;
                //Machine.sysConfig.shifts.NightShift = FrBaseConfig.Instance.txt_NightShift.Value;

                //Machine.sysConfig.SnapImgSaveFlag = FrBaseConfig.Instance.checkBigImg.Checked;
                //Machine.sysConfig.TestFlag = !FrBaseConfig.Instance.checkImageDetection.Checked;
                //Machine.sysConfig.cehouFlag = FrBaseConfig.Instance.checkCeHou.Checked;
                //Machine.sysConfig.ShowFilt = FrBaseConfig.Instance.checkFilt.Checked;
                //Machine.sysConfig.ImageSuffix = FrBaseConfig.Instance.cmb_ImageSuffix.SelectedItem.ToString();
                //Machine.sysConfig.CutImgSaveFlag = FrBaseConfig.Instance.checkCutImgSaveFlag.Checked;
                //Machine.sysConfig.SaveJsonFlag = FrBaseConfig.Instance.checkSaveJsonFlag.Checked;

                //Machine.sysConfig.CameraView = (double)FrBaseConfig.Instance.txt_CameraView.Value;
                //Machine.sysConfig.ImageWidth = (int)FrBaseConfig.Instance.txt_ImageWidth.Value;
                //Machine.sysConfig.ImageHeight = (int)FrBaseConfig.Instance.txt_ImageHeight.Value;
                //Machine.sysConfig.NgMeters = (double)FrBaseConfig.Instance.txt_NgMeters.Value;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        private void FrSetting_Activated(object sender, EventArgs e)
        {


        }
    }
}
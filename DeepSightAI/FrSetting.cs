using DeepSightAI.SettingPages;
using DeepSightTool;
using System;
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
                this.treeNode4.Text = "Failure Mapping Layout";
            }
            else
            {
                btnSave.Text = "Save";
                this.treeNode1.Text = "conventional";
                this.treeNode2.Text = "AVI";
                this.treeNode3.Text = "AI";
                this.treeNode4.Text = "Failure Mapping Layout";
            }

            FrBaseConfig.Instance.Language(language);
            FrHWConfig.Instance.Language(language);
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

                    case "Failure Mapping Layout":
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

                    //case "项目":
                    //case "project":

                    //    panel2.Dock = DockStyle.Fill;

                    //    panel1.Visible = false;
                    //    panel2.Visible = true;
                    //    panel3.Visible = false;
                    //    panel4.Visible = false;
                    //    panel5.Visible = false;
                    //    panel6.Visible = false;
                    //    panel7.Visible = false;
                    //    panel8.Visible = false;
                    //    break;

                    //case "用户管理":
                    //case "User Management":

                    //    panel3.Dock = DockStyle.Fill;

                    //    panel1.Visible = false;
                    //    panel2.Visible = false;
                    //    panel3.Visible = true;
                    //    panel4.Visible = false;
                    //    panel5.Visible = false;
                    //    panel6.Visible = false;
                    //    panel7.Visible = false;
                    //    panel8.Visible = false;
                    //    break;

                    //case "日点检维护":
                    //case "Spot inspection and maintenance":

                    //    btnSave.Visible = false;

                    //    panel4.Dock = DockStyle.Fill;

                    //    panel1.Visible = false;
                    //    panel2.Visible = false;
                    //    panel3.Visible = false;
                    //    panel4.Visible = true;
                    //    panel5.Visible = false;
                    //    panel6.Visible = false;
                    //    panel7.Visible = false;
                    //    panel8.Visible = false;
                    //    if (string.IsNullOrWhiteSpace(Machine.LoginUserName))
                    //    {
                    //        FrCheckInfo.Instance.dataGridView.ContextMenuStrip = null;
                    //    }
                    //    else
                    //    {
                    //        FrCheckInfo.Instance.dataGridView.ContextMenuStrip = FrCheckInfo.Instance.contextMenuStrip1;
                    //    }

                    //    break;

                    //case "设备变更":
                    //case "Equipment Changes":

                    //    btnSave.Visible = false;

                    //    panel5.Dock = DockStyle.Fill;

                    //    panel1.Visible = false;
                    //    panel2.Visible = false;
                    //    panel3.Visible = false;
                    //    panel4.Visible = false;
                    //    panel5.Visible = true;
                    //    panel6.Visible = false;
                    //    panel7.Visible = false;
                    //    panel8.Visible = false;
                    //    if (string.IsNullOrWhiteSpace(Machine.LoginUserName))
                    //    {
                    //        FrChangeInfo.Instance.btnAdd.Visible = false;
                    //        FrChangeInfo.Instance.btndelete.Visible = false;
                    //        FrChangeInfo.Instance.btnupdate.Visible = false;
                    //    }
                    //    else
                    //    {
                    //        FrChangeInfo.Instance.btnAdd.Visible = true;
                    //        FrChangeInfo.Instance.btndelete.Visible = true;
                    //        FrChangeInfo.Instance.btnupdate.Visible = true;
                    //    }

                    //    break;

                    //case "月点检维护":
                    //case "Maintenance Monthly":

                    //    btnSave.Visible = false;

                    //    panel6.Dock = DockStyle.Fill;

                    //    panel1.Visible = false;
                    //    panel2.Visible = false;
                    //    panel3.Visible = false;
                    //    panel4.Visible = false;
                    //    panel5.Visible = false;
                    //    panel6.Visible = true;
                    //    panel7.Visible = false;
                    //    panel8.Visible = false;
                    //    if (string.IsNullOrWhiteSpace(Machine.LoginUserName))
                    //    {
                    //        FrCheckMonthInfo.Instance.dataGridView.ContextMenuStrip = null;
                    //    }
                    //    else
                    //    {
                    //        FrCheckMonthInfo.Instance.dataGridView.ContextMenuStrip = FrCheckMonthInfo.Instance.contextMenuStrip1;
                    //    }

                    //    break;

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
                    FrHWConfig.Instance.GetStationParam();
                    if (Machine.avi_class.Save(Machine.aviconfig))
                    {
                        RestartApplication(appPath, appExe);
                        Machine.master.workClass.aviconfig = Machine.aviconfig;
                        MessageBox.Show("保存Agent配置文件成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                }
                if (tvw_setting.SelectedNode.Text == "算法方案配置")
                {
                    FrAIConfig.Instance.SaveParam();
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
        public bool KillProcessInDirectory(string directoryPath, string exeName)
        {
            bool result = false;
            // 获取所有同名进程
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeName));
            foreach (Process process in processes)
            {
                try
                {
                    // 检查进程的启动目录是否匹配
                    if (process.MainModule != null &&
                        Path.GetDirectoryName(process.MainModule.FileName)?.Equals(directoryPath, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        process.Kill();
                        process.WaitForExit(5000); // 等待最多5秒
                        Console.WriteLine($"已终止进程: {process.Id}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"终止进程失败: {ex.Message}");
                }
                result = true;
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
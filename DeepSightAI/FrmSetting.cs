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
    public partial class FrmSetting : Form
    {
        /// <summary>
        /// 配置页索引常量，与 uiTabControl 中 TabPage 顺序保持一致
        /// </summary>
        private const int PageBase = 0;
        private const int PageAI = 1;
        private const int PageHW = 2;
        private const int PageDb = 3;
        private const int PageKeyDefect = 4;
        private const int PageShortcut = 5;
        private const int PageToolbox = 6;

        public FrmSetting()
        {
            InitializeComponent();

            navNodeBase = uiNavMenu.CreateNode("常规配置", PageBase);
            navNodeAI = uiNavMenu.CreateNode("算法方案配置", PageAI);
            navNodeHW = uiNavMenu.CreateNode("机台配置", PageHW);
            navNodeDb = uiNavMenu.CreateNode("数据库配置", PageDb);
            navNodeKeyDefect = uiNavMenu.CreateNode("重点缺陷管理", PageKeyDefect);
            navNodeShortcut = uiNavMenu.CreateNode("快捷键配置", PageShortcut);
            navNodeToolbox = uiNavMenu.CreateNode("工具箱", PageToolbox);

            Load += FrSetting_Load;
        }

        /// <summary>
        /// 供 FrmMain Ctrl+S 调用的公开保存入口
        /// </summary>
        public void SaveCurrentConfig()
        {
            btn_saveSetting_Click(this, EventArgs.Empty);
        }

        /// <summary>
        /// 窗体对象实例
        /// </summary>
        private static FrmSetting _instance;

        public static FrmSetting Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrmSetting();
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
                uiNavMenu.SelectFirst();

                Language(1);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        private System.Windows.Forms.TreeNode navNodeBase;
        private System.Windows.Forms.TreeNode navNodeAI;
        private System.Windows.Forms.TreeNode navNodeHW;
        private System.Windows.Forms.TreeNode navNodeDb;
        private System.Windows.Forms.TreeNode navNodeKeyDefect;
        private System.Windows.Forms.TreeNode navNodeShortcut;
        private System.Windows.Forms.TreeNode navNodeToolbox;

        public void Language(int language)
        {
            if (language == 1)
            {
                btnSave.Text = "保存";
                navNodeBase.Text = "常规配置";
                navNodeAI.Text = "算法方案配置";
                navNodeHW.Text = "机台配置";
                navNodeDb.Text = "数据库配置";
                navNodeKeyDefect.Text = "重点缺陷管理";
                navNodeShortcut.Text = "快捷键配置";
                navNodeToolbox.Text = "工具箱";
            }
            else
            {
                btnSave.Text = "Save";
                navNodeBase.Text = "conventional";
                navNodeAI.Text = "AVI";
                navNodeHW.Text = "AI";
                navNodeDb.Text = "Database";
                navNodeKeyDefect.Text = "Key Defect";
                navNodeShortcut.Text = "Shortcuts";
                navNodeToolbox.Text = "Toolbox";
            }
            uiNavMenu.Invalidate();
        }


        internal void LoadMethod()
        {
            try
            {
                uiTabControl.AddPage(PgSettingBase.Instance);
                uiTabControl.AddPage(PgSettingAi.Instance);
                uiTabControl.AddPage(PgSettingHardware.Instance);
                uiTabControl.AddPage(PgSettingDatabase.Instance);
                uiTabControl.AddPage(PgSettingKeyDefect.Instance);
                uiTabControl.AddPage(PgSettingShortcut.Instance);
                uiTabControl.AddPage(PgSettingToolbox.Instance);
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
                switch (uiTabControl.SelectedIndex)
                {
                    case PageBase:
                        PgSettingBase.Instance.GetBaseParams();
                        if (Machine.config_class.Save(Machine.sysConfig))
                        {
                            Machine.master.SysConfig = Machine.sysConfig;
                            MessageBox.Show("保存配置文件成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;

                    case PageAI:
                        if (PgSettingAi.Instance.SaveParam())
                        {
                            Machine.master.SolConfig = Machine.solconfig;
                            MessageBox.Show("方案及流程配置保存成功", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;

                    case PageHW:
                        // 检查软件是否处于运行状态
                        if (Machine.master != null && Machine.master.IsStart)
                        {
                            MessageBox.Show("软件正在运行中，请先停止运行后再保存机台配置！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        PgSettingHardware.Instance.GetStationParam();
                        if (Machine.avi_class.Save(Machine.aviconfig))
                        {
                            if (Machine.HasAgentMachines)
                            {
                                RestartApplication(appPath, appExe, Machine.sysConfig.AgentShutdownTimeout);
                            }
                            Machine.master.AviConfig = Machine.aviconfig;
                            // 更新FrHome中的AviCtr状态
                            FrmHome.Instance.RefreshMachineStatusConfigs();
                            MessageBox.Show("保存Agent配置文件成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;

                    case PageDb:
                        if (PgSettingDatabase.Instance.SaveConfig())
                        {
                            MessageBox.Show("数据库配置保存成功", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("数据库配置保存失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;

                    case PageKeyDefect:
                        if (PgSettingKeyDefect.Instance.SaveConfig())
                        {
                            MessageBox.Show("重点缺陷配置保存成功", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("重点缺陷配置保存失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;

                    case PageShortcut:
                        PgSettingShortcut.Instance.SaveToConfig();
                        if (Machine.config_class.Save(Machine.sysConfig))
                        {
                            Machine.master.SysConfig = Machine.sysConfig;
                            MessageBox.Show("快捷键配置保存成功", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("快捷键配置保存失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void RestartApplication(string appDirectory, string exeName, int timeoutMs = 2000, bool isRun = false)
        {
            bool result = KillProcessInDirectory(appDirectory, exeName, timeoutMs);
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
            RestartApplication(appDirectory, exeName, Machine.sysConfig.AgentShutdownTimeout, true);
        }
        /// <summary>
        /// 通过Windows事件信号通知ATS_Agent进程优雅关闭
        /// </summary>
        /// <param name="directoryPath">进程目录路径</param>
        /// <param name="exeName">进程名称</param>
        /// <param name="timeoutMs">等待进程退出的超时时间（毫秒），默认10秒</param>
        /// <returns>如果进程存在并成功关闭返回true</returns>
        public bool KillProcessInDirectory(string directoryPath, string exeName, int timeoutMs = 2000)
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
        public string appPath = System.AppDomain.CurrentDomain.BaseDirectory + "ATS_Agent_EXE";
        public string appExe = "ATS_Agent.exe";

        private void FrSetting_Activated(object sender, EventArgs e)
        {


        }
    }
}
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeepSightWorkLib;
using DeepSightModel;
using DeepSightModel.Configuration;
using System.Drawing;
using System.Diagnostics;

namespace DeepSightAI
{
    public class Machine
    {
        public static BusinessClass master = null;
        internal static string LoginUserName = string.Empty;//登录的用户名
        internal static string LoginPwd = string.Empty;//登录的用户密码

        //存储从数据库读上来的用户权限信息
        public static SysAdmins objAdmin { get; set; } = new SysAdmins();
        //常规配置
        internal static ConfigurationClass sysConfig;
        //其他配置
        internal static DeepSight_Config_class config_class = new DeepSight_Config_class();

        //常规配置
        internal static SolutionConfig solconfig;
        //其他配置
        internal static DeepSight_Solution sol_class = new DeepSight_Solution();

        //Agent配置
        internal static AVIConfig aviconfig;
        internal static DeepSight_AVI_class avi_class;
        internal static bool isSwitch = false;

        //机台注册表（通用，两套系统共用）
        internal static MachineRegistryConfig machineRegistry;
        internal static MachineRegistryManager machineRegistryManager = new MachineRegistryManager();

        /// <summary>
        /// 是否存在 Agent 类型的机台（决定是否需要启动 ats_agent.exe）
        /// </summary>
        internal static bool HasAgentMachines =>
            machineRegistry?.Machines?.Any(m => m.DataSourceType == DataSourceType.Agent && m.IsEnable) ?? false;

        internal static string solution = "";
        internal static string flow = "";
        internal static string productSerial = "";

        /// <summary>
        /// 是否正在启动
        /// </summary>
        internal static bool loading = true;
        /// <summary>
        /// 展示getber图还是temp
        /// </summary>
        internal static string ShowFlag = "C";

        internal static void Init()
        {
            loading = true;
            Application.DoEvents();
            try
            {
                UpdateStep(10, "Initialization...", true);
                Application.DoEvents();
                //初始化配置文件
                UpdateStep(20, "程序初始化中...", true);
                if (!config_class.Read(out sysConfig))
                {
                    FrWelcome.Instance.lbl_step.Text = "                  启动出错";
                    FrWelcome.Instance.lbl_step.ForeColor = Color.Red;
                    FrWelcome.Instance.Height = 356;

                    MessageBox.Show("\r\n启动出错,读取常规配置文件异常！", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Process process = Process.GetCurrentProcess();
                    process.Kill();
                    process.Dispose();
                    return;
                }
                if (!sol_class.Read(out solconfig))
                {
                    FrWelcome.Instance.lbl_step.Text = "                  启动出错";
                    FrWelcome.Instance.lbl_step.ForeColor = Color.Red;
                    FrWelcome.Instance.Height = 356;

                    MessageBox.Show("\r\n启动出错,读取AISolution配置文件异常！", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Process process = Process.GetCurrentProcess();
                    process.Kill();
                    process.Dispose();
                    return;
                }
                // 尝试读取 Agent 配置（可能不存在）
                bool hasAgentConfig = false;
                avi_class = new DeepSight_AVI_class_Safe();
                if (avi_class.Read(out aviconfig))
                {
                    hasAgentConfig = true;
                }
                else
                {
                    // Agent 配置不存在时，使用空配置（系统B不需要 Agent）
                    aviconfig = new AVIConfig();
                    LogTextHelper.Info("未找到 Agent 配置文件，将以无 Agent 模式运行");
                }
                // 初始化机台注册表（首次启动时从 AVIConfig 迁移）
                if (hasAgentConfig && aviconfig.WatchPaths != null && aviconfig.WatchPaths.Count > 0)
                {
                    machineRegistryManager.TryMigrateFromAviConfig(aviconfig.WatchPaths);
                }

                if (!machineRegistryManager.Read(out machineRegistry))
                {
                    // 注册表文件不存在且迁移也没生成，创建默认
                    machineRegistry = new MachineRegistryConfig();
                    machineRegistryManager.Save(machineRegistry);
                }
                master = new BusinessClass();
                master.SolConfig = solconfig;
                master.AviConfig = aviconfig;
                master.SysConfig = sysConfig;
                master.InitWork();

                UpdateStep(50, "读取配置文件中...", true);

                Application.DoEvents();
                //Thread.Sleep(400);

                // 获取 FrWelcome 所在的屏幕，让 FrmMain 显示在同一屏幕上
                var targetScreen = Screen.FromControl(FrWelcome.Instance);
                FrmMain.Instance.StartPosition = FormStartPosition.Manual;
                FrmMain.Instance.Location = targetScreen.WorkingArea.Location;
               // FrmMain.Instance.MaximizedBounds = targetScreen.WorkingArea;

                // 程序开启后是否自动最大化
                FrmMain.Instance.WindowState = FormWindowState.Maximized;
                FrmMain.Instance.Show();
                FrWelcome.Instance.Hide();
                UpdateStep(100, "程序启动完成", true);
                FrmMain.Instance.Opacity = 100;

                FrmMain.Instance.SwitchFrom(FormMode.MainForm);

                FrWelcome.Instance.bar_step.Value = 100;
                FrmMain.Instance.Activate();

                LogTextHelper.Info("程序启动");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
                throw;
            }

        }
        internal static void UpdateStep(int percentValue, string stepMsg, bool succeed)
        {
            try
            {
                LogTextHelper.Info(stepMsg);
                FrWelcome.Instance.bar_step.Value = percentValue;
                FrWelcome.Instance.lbl_step.Text = stepMsg + "......";
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }



    }
}

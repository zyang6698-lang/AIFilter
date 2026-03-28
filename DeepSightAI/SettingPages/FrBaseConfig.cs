using DeepSightTool;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    /// <summary>
    /// 基础参数
    /// </summary>
    public partial class FrBaseConfig : Form
    {
        public FrBaseConfig()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
        }

        /// <summary>
        /// 窗体实例对象
        /// </summary>
        private static FrBaseConfig _instance;

        public static FrBaseConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrBaseConfig();
                }

                return _instance;
            }
        }


        public void GetBaseParams()
        {
            try
            {
                if (int.TryParse(this.txt_MaxDefectCount.Text, out int maxDefectCount))
                {
                    Machine.sysConfig.MaxDefectCount = maxDefectCount;
                }
                if (int.TryParse(this.txt_AgentShutdownTimeout.Text, out int agentShutdownTimeout))
                {
                    Machine.sysConfig.AgentShutdownTimeout = agentShutdownTimeout;
                }
                if (int.TryParse(this.txt_GetInferResultTimeout.Text, out int getInferResultTimeout))
                {
                    Machine.aviconfig.GetInferResultTimeout = getInferResultTimeout;
                }
                Machine.sysConfig.WelcomeTitle = this.txt_WelcomeTitle.Text;
                if (float.TryParse(this.txt_WelcomeFontSize.Text, out float welcomeFontSize))
                {
                    Machine.sysConfig.WelcomeFontSize = welcomeFontSize;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
            }
        }
        private void FrBaseConfig_Load(object sender, EventArgs e)
        {
            this.txt_MaxDefectCount.Text = Machine.sysConfig.MaxDefectCount.ToString();
            this.txt_AgentShutdownTimeout.Text = Machine.sysConfig.AgentShutdownTimeout.ToString();
            this.txt_GetInferResultTimeout.Text = Machine.aviconfig.GetInferResultTimeout.ToString();
            this.txt_WelcomeTitle.Text = Machine.sysConfig.WelcomeTitle ?? "Deepsight AI";
            this.txt_WelcomeFontSize.Text = Machine.sysConfig.WelcomeFontSize.ToString();
        }
        // 获取相对路径（.NET Framework 无 Path.GetRelativePath）
        private static string GetRelativePath(string basePath, string fullPath)
        {
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                basePath += Path.DirectorySeparatorChar;
            if (fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                return fullPath.Substring(basePath.Length);
            return fullPath; // fallback
        }

 
    }
}
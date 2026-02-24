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
                Machine.sysConfig.endpoint_address = FrBaseConfig.Instance.txt_endpoint_address.Text;
                Machine.sysConfig.MinioPort = FrBaseConfig.Instance.txt_Minioport.Text;
                Machine.sysConfig.ServerIP = this.txt_severIP.Text;
                Machine.sysConfig.ServerPort = this.txt_severPort.Text;
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
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
            }
        }
        private void FrBaseConfig_Load(object sender, EventArgs e)
        {
            this.txt_severIP.Text = Machine.sysConfig.ServerIP;
            this.txt_severPort.Text = Machine.sysConfig.ServerPort;
            this.txt_endpoint_address.Text = Machine.sysConfig.endpoint_address;
            this.txt_Minioport.Text = Machine.sysConfig.MinioPort;
            this.txt_MaxDefectCount.Text = Machine.sysConfig.MaxDefectCount.ToString();
            this.txt_AgentShutdownTimeout.Text = Machine.sysConfig.AgentShutdownTimeout.ToString();
            this.txt_GetInferResultTimeout.Text = Machine.aviconfig.GetInferResultTimeout.ToString();
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
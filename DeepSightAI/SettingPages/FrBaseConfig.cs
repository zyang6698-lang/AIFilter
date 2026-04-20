using DeepSightTool;
using System;
using System.IO;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    /// <summary>
    /// 基础参数
    /// </summary>
    public partial class FrBaseConfig : Sunny.UI.UIPage
    {
        public FrBaseConfig()
        {
            InitializeComponent();
            PageIndex = 0;
            ShowTitle = false;
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
                if (int.TryParse(this.txt_GetInferResultTimeout.Text, out int MaxWaitTime))
                {
                    Machine.sysConfig.MaxWaitTime = MaxWaitTime;
                }
                Machine.sysConfig.WelcomeTitle = this.txt_WelcomeTitle.Text;
                if (float.TryParse(this.txt_WelcomeFontSize.Text, out float welcomeFontSize))
                {
                    Machine.sysConfig.WelcomeFontSize = welcomeFontSize;
                }
                // 图片类型：0=Template图（默认），1=Gerber图
                Machine.sysConfig.UseGerberImage = this.cmb_ImageType.SelectedIndex == 1;
                // 同步更新 Machine.ShowFlag
                Machine.ShowFlag = Machine.sysConfig.UseGerberImage ? "B" : "C";
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
            this.txt_GetInferResultTimeout.Text = Machine.sysConfig.MaxWaitTime.ToString();
            this.txt_WelcomeTitle.Text = Machine.sysConfig.WelcomeTitle ?? "Deepsight AI";
            this.txt_WelcomeFontSize.Text = Machine.sysConfig.WelcomeFontSize.ToString();
            // 图片类型：false(0)=Template图，true(1)=Gerber图
            this.cmb_ImageType.SelectedIndex = Machine.sysConfig.UseGerberImage ? 1 : 0;
        }

 
    }
}
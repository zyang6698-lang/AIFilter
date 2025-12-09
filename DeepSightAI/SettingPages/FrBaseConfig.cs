using DeepSightTool;
using System;
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
                Machine.sysConfig.ProjectName = FrBaseConfig.Instance.txt_ProjectName.Text;
                Machine.sysConfig.endpoint_address = FrBaseConfig.Instance.txt_endpoint_address.Text;
                Machine.sysConfig.MinioPort = FrBaseConfig.Instance.txt_Minioport.Text;
                Machine.sysConfig.DsCenterUrl = FrBaseConfig.Instance.txt_DsCenterURL.Text;
                Machine.sysConfig.ServerIP = this.txt_severIP.Text;
                Machine.sysConfig.ServerPort = this.txt_severPort.Text;
                if (int.TryParse(this.txt_MaxDefectCount.Text, out int maxDefectCount))
                {
                    Machine.sysConfig.MaxDefectCount = maxDefectCount;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
            }
        }
        private void FrBaseConfig_Load(object sender, EventArgs e)
        {
            this.txt_ProjectName.Text = Machine.sysConfig.ProjectName;
            this.txt_severIP.Text = Machine.sysConfig.ServerIP;
            this.txt_severPort.Text = Machine.sysConfig.ServerPort;
            this.txt_endpoint_address.Text = Machine.sysConfig.endpoint_address;
            this.txt_Minioport.Text = Machine.sysConfig.MinioPort;
            this.txt_DsCenterURL.Text = Machine.sysConfig.DsCenterUrl;
            this.txt_MaxDefectCount.Text = Machine.sysConfig.MaxDefectCount.ToString();
        }
    }
}
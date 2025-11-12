using DeepSightModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    public partial class AviCtr2 : UserControl
    {
        /// <summary>
        /// 控件状态枚举
        /// </summary>
        public enum ControlStatus
        {
            /// <summary>
            /// 正常 (绿色)
            /// </summary>
            Normal,
            /// <summary>
            /// 异常 (红色)
            /// </summary>
            Abnormal,
            /// <summary>
            /// 警告 (黄色)
            /// </summary>
            Warning,
            /// <summary>
            /// 未启用 (灰色)
            /// </summary>
            Disabled
        }
        private ToolTip toolTip;
        private WatchPathConfig _ctrConfig = new WatchPathConfig()
        {
            AviName = "AVI",
            IsEnable = false,
            APath = "C:\\workspace\\ats\\real_ats_data\\real_ats_data\\Verify_A-2025.04yue",
            BPath = "C:\\workspace\\ats\\real_ats_data\\real_ats_data\\Verify_B-2025.04yue",
            Depth = 4,
            FileA = "",
            FileB = "",
            // CopyOrCutMode="copy",
        };

        private double aviPassRate;

        public double AviPassRate
        {
            get { return aviPassRate; }
            set {
                if (aviPassRate!=value)
                {
                    aviPassRate = value;
                    UpdateDisplay();
                }
            }
        }


        private int _aiFilterCount;
        public int AiFilterCount
        {
            get => _aiFilterCount;
            set
            {
                if (_aiFilterCount != value)
                {
                    _aiFilterCount = value;
                    UpdateDisplay();
                }
            }
        }

        private int _aiOkImages;
        public int AiOkImages
        {
            get => _aiOkImages;
            set
            {
                if (_aiOkImages != value)
                {
                    _aiOkImages = value;
                    UpdateDisplay();
                }
            }
        }

        private string _productSerial;

        public string MachineName;
        public string ProductSerial
        {
            get => _productSerial;
            set
            {
                if (_productSerial != value)
                {
                    _productSerial = value;
                    UpdateDisplay();
                }
            }
        }

        private string _pathIndex;
        public string PathIndex
        {
            get => _pathIndex;
            set
            {
                if (_pathIndex != value)
                {
                    _pathIndex = value;
                    UpdateDisplay();
                }
            }
        }

        private string _lotId;
        public string LotId
        {
            get => _lotId;
            set
            {
                if (_lotId != value)
                {
                    _lotId = value;
                    UpdateDisplay();
                }
            }
        }

        private double _utilization;
        public double Utilization
        {
            get => _utilization;
            set
            {
                if (_utilization != value)
                {
                    _utilization = value;
                    UpdateDisplay();
                }
            }
        }

        public WatchPathConfig ctrConfig
        {
            get => _ctrConfig;
            set
            {
                if (_ctrConfig != value)
                {
                    _ctrConfig = value;
                }
            }
        }
        public AviCtr2(WatchPathConfig _config)
        {
            InitializeComponent();
            InitializeToolTip();
            ctrConfig = _config;
            SetName(ctrConfig.AviName);
            UpdateDisplay();
        }
        private void InitializeToolTip()
        {
            toolTip = new ToolTip();
        }

        /// <summary>
        /// 根据 ctrConfig 的内容更新 UI 显示
        /// </summary>
        public void UpdateDisplay()
        {
            var info = new StringBuilder();
            info.AppendLine($"PathIndex: {PathIndex}");
            info.AppendLine($"AI OK图片数: {AiOkImages}");
            info.AppendLine($"图片总数: {AiFilterCount}");
            double ratio = AiFilterCount > 0 ? (double)AiOkImages / AiFilterCount : 0;
            //SetAiPassRate($"{ratio:P2}");
            SetOperatingRate($"{Utilization:P2}");

            lblAiPassRate.Text= $"AI:{ratio:P2}";
            lblAviPassRate.Text= $"AVI:{AviPassRate:P2}";

            labelCurrentPartNumberValue.Text= ProductSerial;
            labelLotValue.Text = LotId;

            if (toolTip != null )
            {
                toolTip.SetToolTip(this, info.ToString());
            }
            // 根据 IsEnable 状态更新指示器
            if (ctrConfig != null)
            {
                SetStatus(ctrConfig.IsEnable ? ControlStatus.Normal : ControlStatus.Disabled);
            }
        }
        /// <summary>
        /// 设置状态指示器的颜色
        /// </summary>
        /// <param name="status">要设置的状态</param>
        public void SetStatus(ControlStatus status)
        {
            Color statusColor;
            switch (status)
            {
                case ControlStatus.Normal:
                    statusColor = Color.Green;
                    break;
                case ControlStatus.Abnormal:
                    statusColor = Color.Red;
                    break;
                case ControlStatus.Warning:
                    statusColor = Color.Yellow;
                    break;
                case ControlStatus.Disabled:
                default:
                    statusColor = Color.Gray;
                    break;
            }

            // 创建一个圆形的位图作为指示器
            Bitmap bmp = new Bitmap(pictureBoxStatus.Width, pictureBoxStatus.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (SolidBrush brush = new SolidBrush(statusColor))
                {
                    g.FillEllipse(brush, 0, 0, pictureBoxStatus.Width, pictureBoxStatus.Height);
                }
            }

            // 在UI线程上更新PictureBox的图像
            if (pictureBoxStatus.InvokeRequired)
            {
                pictureBoxStatus.BeginInvoke(new Action(() => {
                    pictureBoxStatus.Image = bmp;
                }));
            }
            else
            {
                pictureBoxStatus.Image = bmp;
            }
        }
        /// <summary>
        /// 设置名字
        /// </summary>
        /// <param name="strValue"></param>
        public void SetName(string name)
        {
            if (this.labelLineName.InvokeRequired)
            {
                this.labelLineName.BeginInvoke(new Action(() => {
                    this.labelLineName.Text = name;
                }));
            }
            else
            {
                this.labelLineName.Text = name;
            }
        }
        /// <summary>
        /// 设置稼动率
        /// </summary>
        /// <param name="strValue">稼动率值</param>
        public void SetOperatingRate(string strValue)
        {
            if (this.lblOperatingRate.InvokeRequired) {
                this.lblOperatingRate.BeginInvoke(new Action(() => {
                    this.lblOperatingRate.Text = strValue;
                }));
            }
            else {
                this.lblOperatingRate.Text = strValue;
            }
        }

        /// <summary>
        /// 设置 AI Pass Rate
        /// </summary>
        /// <param name="strValue">AI Pass Rate 值</param>
        public void SetAiPassRate(string strValue)
        {
            if (this.lblAiPassRate.InvokeRequired) {
                this.lblAiPassRate.BeginInvoke(new Action(() => {
                    this.lblAiPassRate.Text = strValue;
                }));
            }
            else {
                this.lblAiPassRate.Text = strValue;
            }
        }

        private void AviCtr2_DoubleClick(object sender, EventArgs e)
        {
            FrStationCofig frStation = new FrStationCofig(ctrConfig);
            if (frStation.ShowDialog() == DialogResult.OK)
            {
                //更新config
                //如果对象不相等，进行更新，并通知UI进行重绘
                //if (!object.ReferenceEquals(ctrConfig, frStation.stationConfig))
                {
                    //判断状态
                    if (frStation.stationConfig.IsEnable &&
                        (string.IsNullOrEmpty(frStation.stationConfig.APath) || string.IsNullOrEmpty(frStation.stationConfig.BPath) ||
                                               string.IsNullOrEmpty(frStation.stationConfig.FileA) || string.IsNullOrEmpty(frStation.stationConfig.FileB) ||
                                               string.IsNullOrEmpty(frStation.stationConfig.AviName) || string.IsNullOrEmpty(frStation.stationConfig.Depth.ToString())))
                    {
                        frStation.stationConfig.IsEnable = false;
                        MessageBox.Show("信息存在空值,设备不能设为启用状态,请检查！", "列表为空", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    ctrConfig = frStation.stationConfig;
                }
            }
        }
    }
}

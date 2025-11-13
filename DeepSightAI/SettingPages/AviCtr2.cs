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
            set
            {
                if (aviPassRate != value)
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
            // 设置半透明背景
            this.BackColor = Color.FromArgb(210, 47, 53, 77); // 100是透明度 (0-255), 后面是RGB颜色
            SetTransparentBackground(this);

            InitializeToolTip();
            ctrConfig = _config;
            SetName(ctrConfig.AviName);
            UpdateDisplay();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // 绘制边框
            Color borderColor = ColorTranslator.FromHtml("#5F78A0");
            int borderWidth = 2; // 边框宽度
            using (Pen borderPen = new Pen(borderColor, borderWidth))
            {
                // 绘制一个矩形作为边框
                // 为了让边框完全在控件内部，需要从 (borderWidth / 2) 开始绘制
                e.Graphics.DrawRectangle(borderPen,
                                         borderWidth / 2,
                                         borderWidth / 2,
                                         this.ClientSize.Width - borderWidth,
                                         this.ClientSize.Height - borderWidth);
            }
        }

        private void SetTransparentBackground(Control control)
        {
            foreach (Control c in control.Controls)
            {
                // 对 Label 和 PictureBox 设置透明背景
                if (c is Label || c is PictureBox)
                {
                    c.BackColor = Color.Transparent;
                }
                // 递归设置子控件
                if (c.HasChildren)
                {
                    SetTransparentBackground(c);
                }
            }
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

            lblAiPassRate.Text = $"AI Pass Rate:{ratio:P2}";
            lblAviPassRate.Text = $"AVI Pass Rate:{AviPassRate:P2}";

            labelCurrentPartNumberValue.Text = ProductSerial;
            labelLotValue.Text = LotId;

            if (toolTip != null)
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
            if (this.lblOperatingRate.InvokeRequired)
            {
                this.lblOperatingRate.BeginInvoke(new Action(() => {
                    this.lblOperatingRate.Text = strValue;
                }));
            }
            else
            {
                this.lblOperatingRate.Text = strValue;
            }
        }

        /// <summary>
        /// 设置 AI Pass Rate
        /// </summary>
        /// <param name="strValue">AI Pass Rate 值</param>
        public void SetAiPassRate(string strValue)
        {
            if (this.lblAiPassRate.InvokeRequired)
            {
                this.lblAiPassRate.BeginInvoke(new Action(() => {
                    this.lblAiPassRate.Text = strValue;
                }));
            }
            else
            {
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
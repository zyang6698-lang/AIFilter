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

        // 缓存画笔和状态位图以提高性能
        private readonly Pen _borderPen;
        private readonly Dictionary<ControlStatus, Bitmap> _statusBitmaps = new Dictionary<ControlStatus, Bitmap>();

        public AviCtr2(WatchPathConfig _config)
        {
            InitializeComponent();

            // 设置双缓冲
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            // 缓存边框画笔
            Color borderColor = ColorTranslator.FromHtml("#5F78A0");
            int borderWidth = 2;
            _borderPen = new Pen(borderColor, borderWidth);

            // 预先创建并缓存状态位图
            InitializeStatusBitmaps();


            // 设置半透明背景
            this.BackColor = Color.FromArgb(210, 47, 53, 77); // 100是透明度 (0-255), 后面是RGB颜色
            SetTransparentBackground(this);

            InitializeToolTip();
            ctrConfig = _config;
            SetName(ctrConfig.AviName);
            UpdateDisplay();
        }

        private void InitializeStatusBitmaps()
        {
            // 如果控件尺寸无效，则不执行操作
            if (pictureBoxStatus.Width <= 0 || pictureBoxStatus.Height <= 0) return;

            foreach (ControlStatus status in Enum.GetValues(typeof(ControlStatus)))
            {
                Color statusColor = GetColorForStatus(status);
                Bitmap bmp = new Bitmap(pictureBoxStatus.Width, pictureBoxStatus.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    // 使位图背景透明
                    g.Clear(Color.Transparent);

                    // 计算用于绘制居中圆形的矩形
                    int diameter = Math.Min(pictureBoxStatus.Width, pictureBoxStatus.Height);
                    // 留出一点边距，避免圆形紧贴边缘
                    int margin = 1;
                    int circleDiameter = Math.Max(diameter - (margin * 2), 1);
                    int x = (pictureBoxStatus.Width - circleDiameter) / 2;
                    int y = (pictureBoxStatus.Height - circleDiameter) / 2;
                    var circleRect = new Rectangle(x, y, circleDiameter, circleDiameter);

                    using (SolidBrush brush = new SolidBrush(statusColor))
                    {
                        g.FillEllipse(brush, circleRect);
                    }
                }
                _statusBitmaps[status] = bmp;
            }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // 绘制边框 (使用缓存的画笔)
            if (_borderPen != null)
            {
                e.Graphics.DrawRectangle(_borderPen,
                                         _borderPen.Width / 2,
                                         _borderPen.Width / 2,
                                         this.ClientSize.Width - _borderPen.Width,
                                         this.ClientSize.Height - _borderPen.Width);
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
            SetOperatingRate($"Utilization: {Utilization:P1}");

            lblAiPassRate.Text = $"AI Pass Rate:{ratio:P1}";
            lblAviPassRate.Text = $"AVI Pass Rate:{AviPassRate:P1}";

            labelCurrentPartNumberValue.Text =$"Part Number:{ProductSerial}";
            labelLotValue.Text =$"Lot: {LotId}";

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

        private Color GetColorForStatus(ControlStatus status)
        {
            switch (status)
            {
                case ControlStatus.Normal:
                    return Color.Green;
                case ControlStatus.Abnormal:
                    return Color.Red;
                case ControlStatus.Warning:
                    return Color.Yellow;
                case ControlStatus.Disabled:
                default:
                    return Color.Gray;
            }
        }

        /// <summary>
        /// 设置状态指示器的颜色
        /// </summary>
        /// <param name="status">要设置的状态</param>
        public void SetStatus(ControlStatus status)
        {
            if (_statusBitmaps.TryGetValue(status, out Bitmap bmp))
            {
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
            // 实时从文件读取最新配置
            WatchPathConfig latestConfig = ReadLatestConfigFromFile();
            if (latestConfig != null)
            {
                ctrConfig = latestConfig;
            }

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
                    // 更新UI显示（包括左上角状态指示器）
                    SetName(ctrConfig.AviName);
                    UpdateDisplay();
                }
            }
        }

        /// <summary>
        /// 从配置文件中实时读取当前机台的最新配置
        /// </summary>
        /// <returns>最新的配置，如果读取失败则返回null</returns>
        private WatchPathConfig ReadLatestConfigFromFile()
        {
            try
            {
                string configPath = System.AppDomain.CurrentDomain.BaseDirectory + "ATS_Agent_EXE\\config\\config.json";
                if (!System.IO.File.Exists(configPath))
                {
                    return null;
                }

                string jsonContent = System.IO.File.ReadAllText(configPath);
                AVIConfig aviConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<AVIConfig>(jsonContent);

                if (aviConfig?.WatchPaths == null)
                {
                    return null;
                }

                // 根据 AviName 查找对应的配置
                string currentAviName = ctrConfig?.AviName;
                if (string.IsNullOrEmpty(currentAviName))
                {
                    return null;
                }

                return aviConfig.WatchPaths.FirstOrDefault(w => w.AviName == currentAviName);
            }
            catch (Exception ex)
            {
                // 读取失败时记录日志，返回null使用现有配置
                DeepSightTool.LogTextHelper.Error("读取配置文件失败", ex);
                return null;
            }
        }


    }
}
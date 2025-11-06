using DeepSightAI.Properties;
using DeepSightModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    //public partial class AviCtr : UserControl
    //{
    //    #region 控件属性
    //    private WatchPathConfig _ctrConfig = new WatchPathConfig()
    //    {
    //        AviName = "AVI",
    //        IsEnable = false,
    //        APath = "C:\\workspace\\ats\\real_ats_data\\real_ats_data\\Verify_A-2025.04yue",
    //        BPath = "C:\\workspace\\ats\\real_ats_data\\real_ats_data\\Verify_B-2025.04yue",
    //        Depth = 4,
    //        FileA = "",
    //        FileB = "",
    //        // CopyOrCutMode="copy",
    //    };
    //    private Timer breathTimer;
    //    private bool isBreathing = false;
    //    private int alpha = 100;
    //    private int breathDirection =-10; // 透明度变化方向
    //    public WatchPathConfig ctrConfig
    //    {
    //        get => _ctrConfig;
    //        set
    //        {
    //            if (_ctrConfig != value)
    //            {
    //                _ctrConfig = value;
    //                UpdateDisplay();
    //            }
    //        }
    //    }
    //    #endregion
    //    public AviCtr()
    //    {
    //        InitializeComponent();
    //        InitializeBreathTimer();
    //    }
    //    private void InitializeBreathTimer()
    //    {
    //        breathTimer = new Timer();
    //        breathTimer.Interval = 50; // 50毫秒更新一次
    //        breathTimer.Tick += BreathTimer_Tick;
    //    }
    //    private void BreathTimer_Tick(object sender, EventArgs e)
    //    {
    //        if (!isBreathing) return;

    //        // 更新透明度
    //        alpha += breathDirection;

    //        // 反转方向
    //        if (alpha <= 100 || alpha >= 255)
    //        {
    //            breathDirection = -breathDirection;
    //        }

    //        // 确保透明度在有效范围内
    //        alpha = Math.Max(100, Math.Min(255, alpha));

    //        // 更新图片显示
    //        UpdateBreathImage();
    //    }
    //    private void UpdateBreathImage()
    //    {
    //        if (pic_AVI != null && ctrConfig.IsEnable)
    //        {
    //            // 创建带有透明度的图片
    //            Image originalImage = Resources.在线;
    //            Image breathImage = AdjustImageAlpha(originalImage, alpha);
    //            pic_AVI.Image = breathImage;
    //        }
    //    }
    //    private Image AdjustImageAlpha(Image image, int alpha)
    //    {
    //        Bitmap bmp = new Bitmap(image.Width, image.Height);
    //        using (Graphics g = Graphics.FromImage(bmp))
    //        {
    //            ColorMatrix matrix = new ColorMatrix();
    //            matrix.Matrix33 = alpha / 255f; // 设置透明度

    //            ImageAttributes attributes = new ImageAttributes();
    //            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

    //            g.DrawImage(image,
    //                       new Rectangle(0, 0, bmp.Width, bmp.Height),
    //                       0, 0, image.Width, image.Height,
    //                       GraphicsUnit.Pixel, attributes);
    //        }
    //        return bmp;
    //    }
    //    public AviCtr(WatchPathConfig _config)
    //    {
    //        InitializeComponent();
    //        ctrConfig = _config;
    //    }
    //    private void UpdateDisplay()
    //    {
    //        if (lblName != null)
    //            lblName.Text = ctrConfig.AviName;

    //        if (pic_AVI != null)//&& labelStatus != null)
    //        {
    //            if (ctrConfig.IsEnable)
    //            {
    //                // 启动呼吸效果
    //                if (breathTimer==null)
    //                {
    //                    InitializeBreathTimer();
    //                }
    //                isBreathing = true;
    //                breathTimer.Start();
    //                //pic_AVI.Image = Resources.在线;
    //            }
    //            else
    //            {
    //                pic_AVI.Image = Resources.离线;
    //            }
    //        }
    //    }

    //    private void pic_AVI_DoubleClick(object sender, EventArgs e)
    //    {
    //        FrStationCofig frStation = new FrStationCofig(ctrConfig);
    //        if (frStation.ShowDialog() == DialogResult.OK)
    //        {
    //            //更新config
    //            //如果对象不相等，进行更新，并通知UI进行重绘
    //            //if (!object.ReferenceEquals(ctrConfig, frStation.stationConfig))
    //            {
    //                //判断状态
    //                if (frStation.stationConfig.IsEnable &&
    //                    (string.IsNullOrEmpty(frStation.stationConfig.APath) || string.IsNullOrEmpty(frStation.stationConfig.BPath) ||
    //                                           string.IsNullOrEmpty(frStation.stationConfig.FileA) || string.IsNullOrEmpty(frStation.stationConfig.FileB) ||
    //                                           string.IsNullOrEmpty(frStation.stationConfig.AviName) || string.IsNullOrEmpty(frStation.stationConfig.Depth.ToString())))
    //                {
    //                    frStation.stationConfig.IsEnable = false;
    //                    MessageBox.Show("信息存在空值,设备不能设为启用状态,请检查！", "列表为空", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //                }
    //                ctrConfig = frStation.stationConfig;
    //            }
    //        }
    //    }
    //    // 释放资源

    //}
    public partial class AviCtr : UserControl
    {
        #region 控件属性
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
        private Timer breathTimer;
        private bool isBreathing = false;
        private int alpha = 100;
        private int breathDirection = -10; // 透明度变化方向

        private Label lblProductSerial;
        private Label lblLotId;
        private Label lblUtilization;
        private Label lblTotalImages;
        private Label lblAiOkImages;
        private Label lblAiOkRatio;

        private int _totalImages;
        public int TotalImages
        {
            get => _totalImages;
            set
            {
                if (_totalImages != value)
                {
                    _totalImages = value;
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
                    UpdateDisplay();
                }
            }
        }
        #endregion
        public AviCtr()
        {
            InitializeComponent();
            InitializeBreathTimer();
            InitializeCustomLabels();
        }
        private void InitializeCustomLabels()
        {
            lblProductSerial = new Label { ForeColor = Color.White, Location = new Point(pic_AVI.Right + 5, 80), AutoSize = true, BackColor = Color.Transparent };
            lblLotId = new Label { ForeColor = Color.White, Location = new Point(pic_AVI.Right + 5, 95), AutoSize = true, BackColor = Color.Transparent };
            lblUtilization = new Label { ForeColor = Color.White, Location = new Point(pic_AVI.Right + 5, 110), AutoSize = true, BackColor = Color.Transparent };
            lblTotalImages = new Label { ForeColor = Color.White, Location = new Point(pic_AVI.Right + 5, 125), AutoSize = true, BackColor = Color.Transparent };
            lblAiOkImages = new Label { ForeColor = Color.White, Location = new Point(pic_AVI.Right + 5, 140), AutoSize = true, BackColor = Color.Transparent };
            lblAiOkRatio = new Label { ForeColor = Color.White, Location = new Point(pic_AVI.Right + 5, 155), AutoSize = true, BackColor = Color.Transparent };


            this.Controls.Add(lblProductSerial);
            this.Controls.Add(lblLotId);
            this.Controls.Add(lblUtilization);
            this.Controls.Add(lblTotalImages);
            this.Controls.Add(lblAiOkImages);
            this.Controls.Add(lblAiOkRatio);

            lblProductSerial.BringToFront();
            lblLotId.BringToFront();
            lblUtilization.BringToFront();
            lblTotalImages.BringToFront();
            lblAiOkImages.BringToFront();
            lblAiOkRatio.BringToFront();
        }
        private void InitializeBreathTimer()
        {
            breathTimer = new Timer();
            breathTimer.Interval = 50; // 50毫秒更新一次
            breathTimer.Tick += BreathTimer_Tick;
        }
        private void BreathTimer_Tick(object sender, EventArgs e)
        {
            if (!isBreathing) return;

            // 更新透明度
            alpha += breathDirection;

            // 反转方向
            if (alpha <= 100 || alpha >= 255)
            {
                breathDirection = -breathDirection;
            }

            // 确保透明度在有效范围内
            alpha = Math.Max(100, Math.Min(255, alpha));

            // 更新图片显示
            UpdateBreathImage();
        }
        private void UpdateBreathImage()
        {
            if (pic_AVI != null && ctrConfig.IsEnable)
            {
                // 创建带有透明度的图片
                Image originalImage = Resources.在线;
                Image breathImage = AdjustImageAlpha(originalImage, alpha);
                pic_AVI.Image = breathImage;
            }
        }
        private Image AdjustImageAlpha(Image image, int alpha)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = alpha / 255f; // 设置透明度

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                g.DrawImage(image,
                           new Rectangle(0, 0, bmp.Width, bmp.Height),
                           0, 0, image.Width, image.Height,
                           GraphicsUnit.Pixel, attributes);
            }
            return bmp;
        }
        public AviCtr(WatchPathConfig _config)
        {
            InitializeComponent();
            InitializeCustomLabels();
            
            ctrConfig = _config;
            MachineName = _config.AviName;
            this.BackColor = Color.Transparent;
            if (pic_AVI != null)
            {
                pic_AVI.BackColor = Color.Transparent;
            }
        }
        private void UpdateDisplay()
        {
            if (lblName != null)
                lblName.Text = ctrConfig.AviName;

            if (lblProductSerial != null)
                lblProductSerial.Text = $"料号: {ProductSerial}";

            if (lblLotId != null)
                lblLotId.Text = $"Lot: {LotId}";

            if (lblUtilization != null)
                lblUtilization.Text = $"稼动率: {Utilization:P2}";

            if (lblTotalImages != null)
                lblTotalImages.Text = $"总图片数: {TotalImages}";

            if (lblAiOkImages != null)
                lblAiOkImages.Text = $"AI OK数: {AiOkImages}";

            if (lblAiOkRatio != null)
            {
                double ratio = TotalImages > 0 ? (double)AiOkImages / TotalImages : 0;
                lblAiOkRatio.Text = $"AI OK比例: {ratio:P2}";
            }

            if (pic_AVI != null)//&& labelStatus != null)
            {
                if (ctrConfig.IsEnable)
                {
                    // 启动呼吸效果
                    if (breathTimer == null)
                    {
                        InitializeBreathTimer();
                    }
                    isBreathing = true;
                    breathTimer.Start();
                    //pic_AVI.Image = Resources.在线;
                }
                else
                {
                    pic_AVI.Image = Resources.离线;
                }
            }
        }

        private void pic_AVI_DoubleClick(object sender, EventArgs e)
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
        // 释放资源

        // 添加获取控件中心位置的方法
        public Point GetCenterPoint()
        {
            // 获取控件在屏幕上的中心位置
            var centerX = this.Location.X + this.Width / 2;
            var centerY = this.Location.Y + this.Height / 2;
            return new Point(centerX, centerY);
        }

        // 添加获取顶部中心位置的方法（用于连接线）
        public Point GetTopCenterPoint()
        {
            var centerX = this.Location.X + this.Width / 2;
            var centerY = this.Location.Y;
            return new Point(centerX, centerY);
        }

        // 添加接收数据的方法
        public void ReceiveData(string data)
        {
            // 可以在这里添加数据接收的动画效果
            FlashBorder();
        }

        private async void FlashBorder()
        {
            var originalBorder = this.BorderStyle;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.FromArgb(50, 0, 255, 0); // 淡绿色背景

            await Task.Delay(300);

            this.BorderStyle = originalBorder;
            this.BackColor = SystemColors.Control;
        }

    }
}

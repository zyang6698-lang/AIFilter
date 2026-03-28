using DeepSightDB;
using DeepSightDisplay;
using DeepSightTool;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// 缺陷图片项控件 - 显示单个缺陷的原图、模板图和状态信息
    /// </summary>
    public partial class DefectImageItemControl : UserControl
    {
        private DetectInfo _point;
        private bool _isSelected;
        private Image _rawOriginalImage;

        /// <summary>
        /// 主界面背景色（用于空白图片时与主界面保持一致）
        /// </summary>
        private static readonly Color MainBackColor = Color.FromArgb(29, 48, 60);

        /// <summary>
        /// 可注入的图片加载委托（输入Minio路径，返回Bitmap）。
        /// 设置后将优先使用此委托加载图片，未设置时回退到 Machine.master.MinioService。
        /// </summary>
        public Func<string, Bitmap> ImageLoaderFunc { get; set; }

        /// <summary>
        /// 是否处于主界面模式。主界面模式下，状态栏只显示AI状态。
        /// </summary>
        public bool IsHomeMode { get; set; }

        /// <summary>
        /// 当请求运行单图测试时触发
        /// </summary>
        public event EventHandler<SingleImageTestEventArgs> RunTestRequested;

        /// <summary>
        /// 当控件被点击时触发
        /// </summary>
        public event EventHandler ItemClicked;

        /// <summary>
        /// 获取或设置关联的缺陷信息
        /// </summary>
        public DetectInfo HeatPoint
        {
            get => _point;
            set
            {
                _point = value;
                LoadData();
            }
        }

        /// <summary>
        /// 获取或设置是否选中状态
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                UpdateAppearance();
            }
        }

        public DefectImageItemControl()
        {
            InitializeComponent();
            _toolTip = new ToolTip();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            // 绑定点击事件
            this.Click += (s, e) => ItemClicked?.Invoke(this, EventArgs.Empty);
            panel_Header.Click += (s, e) => ItemClicked?.Invoke(this, EventArgs.Empty);
            label_SN.Click += (s, e) => ItemClicked?.Invoke(this, EventArgs.Empty);
            pictureBox_OriginalImage.Click += (s, e) => ItemClicked?.Invoke(this, EventArgs.Empty);
            pictureBox_TemplateImage.Click += (s, e) => ItemClicked?.Invoke(this, EventArgs.Empty);
            panel_Status.Click += (s, e) => ItemClicked?.Invoke(this, EventArgs.Empty);
            label_Status.Click += (s, e) => ItemClicked?.Invoke(this, EventArgs.Empty);

            // 双击图片弹出详情窗口
            pictureBox_OriginalImage.DoubleClick += PictureBox_DoubleClick;
            pictureBox_TemplateImage.DoubleClick += PictureBox_DoubleClick;

            // 运行按钮
            button_Run.Click += Button_Run_Click;
            button_Run.PreviewKeyDown += (s, e) => e.IsInputKey = false;
            _toolTip.SetToolTip(button_Run, "单图测试");
        }

        private void Button_Run_Click(object sender, EventArgs e)
        {
            if (_point != null)
            {
                RunTestRequested?.Invoke(this, new SingleImageTestEventArgs { HeatPoint = _point });
            }
        }

        private void PictureBox_DoubleClick(object sender, EventArgs e)
        {
            if (_point == null) return;

            using (var form = new FrDefectImageDetail(
                _point,
                _rawOriginalImage,
                pictureBox_TemplateImage.Image,
                pictureBox_OriginalImage.Image))
            {
                form.ShowDialog(this.FindForm());
            }
        }

        /// <summary>
        /// 加载缺陷数据
        /// </summary>
        private void LoadData()
        {
            if (_point == null)
            {
                label_SN.Text = "";
                label_Status.Text = "";
                pictureBox_OriginalImage.Image?.Dispose();
                pictureBox_OriginalImage.Image = null;
                pictureBox_TemplateImage.Image?.Dispose();
                pictureBox_TemplateImage.Image = null;
                return;
            }

            // 第一行：SN信息
            label_SN.Text = !string.IsNullOrEmpty(_point.DisplaySN) ? _point.DisplaySN : "";

            // 第四行：状态信息
            UpdateStatusLabel();

            // 第二行：加载原图
            LoadOriginalImage();

            // 第三行：加载模板图
            LoadTemplateImage();

            UpdateAppearance();
        }

        /// <summary>
        /// 从Minio路径加载图片。优先使用 ImageLoaderFunc 委托，未设置时回退到 Machine.master.MinioService。
        /// </summary>
        private Bitmap LoadImageFromMinio(string minioPath)
        {
            if (string.IsNullOrEmpty(minioPath)) return null;

            // 优先使用外部注入的加载器
            if (ImageLoaderFunc != null)
            {
                return ImageLoaderFunc(minioPath);
            }

            // 回退到默认加载逻辑（格式：IP:objectKey）
            var parts = minioPath.Split(':');
            if (parts.Length < 2) return null;

            string ip = parts[0];
            string objectKey = parts[1];

            using (var stream = Machine.master.MinioService.GetImageStreamSync("deepiresults", objectKey, ip))
            {
                if (stream == null || stream.Length == 0) return null;

                using (Mat mt = Cv2.ImDecode(stream.ToArray(), ImreadModes.Color))
                {
                    if (mt == null || mt.Empty()) return null;
                    return mt.ToBitmap();
                }
            }
        }

        /// <summary>
        /// 加载原图并绘制缺陷框（通过Minio加载）
        /// </summary>
        private void LoadOriginalImage()
        {
            pictureBox_OriginalImage.Image?.Dispose();
            pictureBox_OriginalImage.Image = null;

            if (string.IsNullOrEmpty(_point.ImagePath))
                return;

            try
            {
                var bmp = LoadImageFromMinio(_point.ImagePath);
                if (bmp != null)
                {
                    _rawOriginalImage?.Dispose();
                    _rawOriginalImage = bmp;
                    pictureBox_OriginalImage.Image = ImageHelper.DrawDefectBoxOnImage(bmp, _point);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"加载图片失败: {_point.ImagePath}, {ex.Message}");
            }
        }


        /// <summary>
        /// 加载模板图（通过Minio加载，路径从缺陷图路径派生）
        /// </summary>
        private void LoadTemplateImage()
        {
            pictureBox_TemplateImage.Image?.Dispose();
            pictureBox_TemplateImage.Image = null;

            if (string.IsNullOrEmpty(_point.ImagePath))
                return;

            try
            {
                string templatePath = _point.TempImagePath;
                if (string.IsNullOrEmpty(templatePath)) return;

                var bmp = LoadImageFromMinio(templatePath);
                if (bmp != null)
                {
                    pictureBox_TemplateImage.Image = ImageHelper.DrawDefectBoxOnImage(bmp, _point);
                    bmp.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"加载模板图片失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新状态标签
        /// </summary>
        public void UpdateStatusLabel()
        {
            if (_point == null) return;

            if (IsHomeMode)
            {
                label_Status.Text = $"AI: {GetStatusText(_point.AIStatus)}";
            }
            else
            {
                label_Status.Text = $"AI: {GetStatusText(_point.AIStatus)}  |  VVS: {GetStatusText(_point.VVSStatus)}";
            }
        }

        /// <summary>
        /// 更新控件外观（选中/未选中状态）
        /// </summary>
        private void UpdateAppearance()
        {
            Color borderColor;

            if (_isSelected)
            {
                // 选中状态：使用醒目的高亮颜色（亮青色）
                borderColor = Color.FromArgb(0, 200, 255);
                this.Padding = new Padding(6);
            }
            else
            {
                // 未选中状态：根据VVS状态显示边框颜色
                if (_point != null)
                {
                    switch (_point.VVSStatus)
                    {
                        case 1: // OK
                            borderColor = Color.Green;
                            break;
                        case 2: // NG
                            borderColor = Color.Red;
                            break;
                        default: // 0 未运行 或其他
                            borderColor = Color.FromArgb(60, 60, 60);
                            break;
                    }
                }
                else
                {
                    borderColor = Color.FromArgb(60, 60, 60);
                }
                this.Padding = new Padding(2);
            }

            this.BackColor = borderColor;
        }

        /// <summary>
        /// 将状态码转换为显示文本
        /// </summary>
        private string GetStatusText(int status)
        {
            switch (status)
            {
                case 0: return "未检测";
                case 1: return "OK";
                case 2: return "NG";
                case 3: return "异常";
                default: return status.ToString();
            }
        }

        /// <summary>
        /// 获取已加载的原图（不带缺陷框）
        /// </summary>
        public Image OriginalImage => _rawOriginalImage;

        /// <summary>
        /// 获取已加载的模板图
        /// </summary>
        public Image TemplateImage => pictureBox_TemplateImage.Image;

        /// <summary>
        /// 调整图片区域高度以适应容器
        /// </summary>
        public void AdjustImageHeight(int totalHeight)
        {
            // 减去Header(28) + Status(40) + Padding
            int availableHeight = totalHeight - panel_Header.Height - panel_Status.Height - this.Padding.Vertical;
            int imageHeight = availableHeight / 2;

            pictureBox_OriginalImage.Height = imageHeight;
            pictureBox_TemplateImage.Height = imageHeight;
        }

        /// <summary>
        /// 直接设置显示数据（用于 FrHome 等外部调用，不依赖 DetectInfo 的 ImagePath 自动加载）。
        /// 调用方负责加载图片并传入 Bitmap，控件仅负责显示。
        /// </summary>
        /// <param name="headerText">标题栏文本（如 "缺陷1:XX"）</param>
        /// <param name="originalImage">原图（已绘制缺陷框），控件获得所有权</param>
        /// <param name="rawOriginalImage">原图（未绘制缺陷框，用于详情弹窗），控件获得所有权</param>
        /// <param name="templateImage">模板图（已绘制缺陷框），控件获得所有权</param>
        /// <param name="statusText">状态文本（如 "AI结果: OK"）</param>
        /// <param name="detectInfo">可选的 DetectInfo，用于双击详情和运行测试</param>
        public void SetDisplayData(string headerText, Bitmap originalImage, Bitmap rawOriginalImage,
            Bitmap templateImage, string statusText, DetectInfo detectInfo = null)
        {
            _point = detectInfo;

            // 标题
            label_SN.Text = headerText ?? "";

            // 原图
            pictureBox_OriginalImage.Image?.Dispose();
            pictureBox_OriginalImage.Image = originalImage;

            // 原始未标注图
            _rawOriginalImage?.Dispose();
            _rawOriginalImage = rawOriginalImage;

            // 模板图
            pictureBox_TemplateImage.Image?.Dispose();
            pictureBox_TemplateImage.Image = templateImage;

            // 状态
            label_Status.Text = statusText ?? "";

            UpdateAppearance();
        }

        /// <summary>
        /// 仅更新状态文本（不重新加载图片），用于 AI 结果回调后轻量刷新。
        /// </summary>
        public void UpdateStatusText(string statusText)
        {
            label_Status.Text = statusText ?? "";
        }

        /// <summary>
        /// 当前控件是否已有图片显示内容（用于判断是否需要完整加载）
        /// </summary>
        public bool HasDisplayContent => pictureBox_OriginalImage.Image != null;

        /// <summary>
        /// 清空显示内容
        /// </summary>
        public void ClearDisplay()
        {
            _point = null;
            label_SN.Text = "";
            label_Status.Text = "";

            pictureBox_OriginalImage.Image?.Dispose();
            pictureBox_OriginalImage.Image = null;
            pictureBox_TemplateImage.Image?.Dispose();
            pictureBox_TemplateImage.Image = null;
            _rawOriginalImage?.Dispose();
            _rawOriginalImage = null;

            UpdateAppearance();
        }
    }
}


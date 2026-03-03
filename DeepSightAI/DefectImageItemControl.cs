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
        private DetectInfo _heatPoint;
        private bool _isSelected;
        private Image _rawOriginalImage;

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
            get => _heatPoint;
            set
            {
                _heatPoint = value;
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

            // 运行按钮
            button_Run.Click += Button_Run_Click;
            button_Run.PreviewKeyDown += (s, e) => e.IsInputKey = false;
            _toolTip.SetToolTip(button_Run, "单图测试");
        }

        private void Button_Run_Click(object sender, EventArgs e)
        {
            if (_heatPoint != null)
            {
                RunTestRequested?.Invoke(this, new SingleImageTestEventArgs { HeatPoint = _heatPoint });
            }
        }

        /// <summary>
        /// 加载缺陷数据
        /// </summary>
        private void LoadData()
        {
            if (_heatPoint == null)
            {
                label_SN.Text = "SN: -";
                label_Status.Text = "AI: 未检测  |  VVS: 未检测";
                pictureBox_OriginalImage.Image?.Dispose();
                pictureBox_OriginalImage.Image = null;
                pictureBox_TemplateImage.Image?.Dispose();
                pictureBox_TemplateImage.Image = null;
                return;
            }

            // 第一行：SN信息
            label_SN.Text = !string.IsNullOrEmpty(_heatPoint.DisplaySN) ? _heatPoint.DisplaySN : "SN: -";

            // 第四行：状态信息
            UpdateStatusLabel();

            // 第二行：加载原图
            LoadOriginalImage();

            // 第三行：加载模板图
            LoadTemplateImage();

            UpdateAppearance();
        }

        /// <summary>
        /// 从Minio路径加载图片（格式：IP:objectKey）
        /// </summary>
        private Bitmap LoadImageFromMinio(string minioPath)
        {
            if (string.IsNullOrEmpty(minioPath)) return null;

            var parts = minioPath.Split(':');
            if (parts.Length < 2) return null;

            // 格式：IP:objectKey
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

            if (string.IsNullOrEmpty(_heatPoint.ImagePath))
                return;

            try
            {
                var bmp = LoadImageFromMinio(_heatPoint.ImagePath);
                if (bmp != null)
                {
                    _rawOriginalImage?.Dispose();
                    _rawOriginalImage = bmp;
                    pictureBox_OriginalImage.Image = ImageHelper.DrawDefectBoxOnImage(bmp, _heatPoint);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"加载图片失败: {_heatPoint.ImagePath}, {ex.Message}");
            }
        }

        /// <summary>
        /// 从缺陷图Minio路径派生模板图路径（在扩展名前加[E]）
        /// </summary>
        private string BuildTemplateMinioPath(string defectMinioPath)
        {
            if (string.IsNullOrEmpty(defectMinioPath)) return null;

            // 格式: IP:objectKey，先分离IP和objectKey
            int colonIndex = defectMinioPath.IndexOf(':');
            if (colonIndex < 0) return null;

            string ip = defectMinioPath.Substring(0, colonIndex);
            string objectKey = defectMinioPath.Substring(colonIndex + 1);

            // 在扩展名前添加[E]
            int lastDotIndex = objectKey.LastIndexOf('.');
            if (lastDotIndex > 0)
                objectKey = objectKey.Substring(0, lastDotIndex) + "[E]" + objectKey.Substring(lastDotIndex);
            else
                objectKey = objectKey + "[E]";

            return $"{ip}:{objectKey}";
        }

        /// <summary>
        /// 加载模板图（通过Minio加载，路径从缺陷图路径派生）
        /// </summary>
        private void LoadTemplateImage()
        {
            pictureBox_TemplateImage.Image?.Dispose();
            pictureBox_TemplateImage.Image = null;

            if (string.IsNullOrEmpty(_heatPoint.ImagePath))
                return;

            try
            {
                string templatePath = BuildTemplateMinioPath(_heatPoint.ImagePath);
                if (string.IsNullOrEmpty(templatePath)) return;

                var bmp = LoadImageFromMinio(templatePath);
                if (bmp != null)
                {
                    pictureBox_TemplateImage.Image = bmp;
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
            if (_heatPoint == null) return;
            label_Status.Text = $"AI: {GetStatusText(_heatPoint.AIStatus)}  |  VVS: {GetStatusText(_heatPoint.VVSStatus)}";
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
                if (_heatPoint != null)
                {
                    switch (_heatPoint.VVSStatus)
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
    }
}


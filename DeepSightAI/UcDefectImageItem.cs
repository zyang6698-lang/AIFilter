using DeepSightDB;
using DeepSightModel;
using DeepSightDisplay;
using DeepSightTool;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public enum DefectDisplayImageType
    {
        DefectBox,
        Original,
        Template,
        Gerber,
        Avi
    }

    /// <summary>
    /// 缺陷图片项控件 - 显示单个缺陷的原图、模板图和状态信息
    /// </summary>
    public partial class UcDefectImageItem : UserControl
    {
        private DetectInfo _point;
        private bool _isSelected;
        private Image _rawOriginalImage;
        private Image _defectBoxImage;
        private Image _templateImage;
        private Image _gerberImage;
        private Image _aviImage;
        private bool _useHorizontalImageLayout;
        private List<DefectDisplayImageType> _displayImageTypes = CreateDefaultDisplayImageTypes();

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
        /// 获取或设置关联的缺陷信息。
        /// 设置时仅初始化文本和状态（同步，不加载图片）。
        /// 图片需要通过 <see cref="LoadImagesAsync"/> 异步加载。
        /// </summary>
        public DetectInfo HeatPoint
        {
            get => _point;
            set
            {
                _point = value;
                InitializeMetadata();
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

        public UcDefectImageItem()
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
            pictureBox_AviImage.Click += (s, e) => ItemClicked?.Invoke(this, EventArgs.Empty);
            panel_Status.Click += (s, e) => ItemClicked?.Invoke(this, EventArgs.Empty);
            label_Status.Click += (s, e) => ItemClicked?.Invoke(this, EventArgs.Empty);

            // 双击图片弹出详情窗口
            pictureBox_OriginalImage.DoubleClick += PictureBox_DoubleClick;
            pictureBox_TemplateImage.DoubleClick += PictureBox_DoubleClick;
            pictureBox_AviImage.DoubleClick += PictureBox_DoubleClick;

            // 运行按钮
            button_Run.PreviewKeyDown += (s, e) => e.IsInputKey = false;
            _toolTip.SetToolTip(button_Run, "单图测试");
        }

        private static void SetPictureBoxImage(PictureBox pictureBox, Image newImage)
        {
            if (pictureBox == null)
            {
                newImage?.Dispose();
                return;
            }

            var oldImage = pictureBox.Image;
            if (ReferenceEquals(oldImage, newImage)) return;

            pictureBox.Image = newImage;
            oldImage?.Dispose();
        }

        private static void ClearPictureBoxImage(PictureBox pictureBox)
        {
            SetPictureBoxImage(pictureBox, null);
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

            using (var form = new FrmDefectImageDetail(
                _point,
                _rawOriginalImage,
                _templateImage,
                _defectBoxImage,
                _aviImage))
            {
                form.ShowDialog(this.FindForm());
            }
        }

        /// <summary>
        /// 仅初始化文本和状态信息（同步、轻量），不加载图片。
        /// </summary>
        private void InitializeMetadata()
        {
            if (_point == null)
            {
                label_SN.Text = "";
                label_Status.Text = "";
                label_Index.Text = "";
                ClearLoadedImages();
                return;
            }

            // SN信息
            label_SN.Text = !string.IsNullOrEmpty(_point.DisplaySN) ? _point.DisplaySN : "";

            // 状态信息
            UpdateStatusLabel();
            UpdateIndexLabel();

            // 清空旧图片，显示为空白占位
            ClearLoadedImages();

            UpdateAppearance();
        }

        /// <summary>
        /// 异步加载原图和模板图。在后台线程加载和解码图片，完成后回到UI线程设置显示。
        /// 支持通过 CancellationToken 取消（翻页或重新加载时）。
        /// </summary>
        public async Task LoadImagesAsync(CancellationToken cancellationToken = default)
        {
            if (_point == null)
                return;

            var point = _point; // 捕获引用，防止加载期间 _point 被替换

            try
            {
                // 在后台线程执行耗时的网络I/O和图片解码
                var (originalMarked, rawOriginal, templateMarked, gerberMarked, aviImage) = await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Bitmap rawBmp = null;
                    Bitmap originalWithBox = null;
                    Bitmap templateWithBox = null;
                    Bitmap gerberWithBox = null;
                    Bitmap aviBmp = null;

                    // 加载原图
                    try
                    {
                        rawBmp = LoadImageFromMinio(point.ImagePath);
                        if (rawBmp != null)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            originalWithBox = ImageHelper.DrawDefectBoxOnImage(rawBmp, point);
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"加载图片失败: {point.ImagePath}, {ex.Message}");
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    // 加载模板图
                    try
                    {
                        string templatePath = point.TempImagePath;
                        if (!string.IsNullOrEmpty(templatePath))
                        {
                            var tempBmp = LoadImageFromMinio(templatePath);
                            if (tempBmp != null)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                templateWithBox = ImageHelper.DrawDefectBoxOnImage(tempBmp, point);
                                tempBmp.Dispose();
                            }
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"加载模板图片失败: {ex.Message}");
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        string gerberPath = point.GerberImagePath;
                        if (!string.IsNullOrEmpty(gerberPath))
                        {
                            var gerberBmp = LoadImageFromMinio(gerberPath);
                            if (gerberBmp != null)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                gerberWithBox = ImageHelper.DrawDefectBoxOnImage(gerberBmp, point);
                                gerberBmp.Dispose();
                            }
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"加载Gerber图片失败: {ex.Message}");
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        string aviPath = point.DefectAviImage;
                        if (!string.IsNullOrEmpty(aviPath))
                        {
                            aviBmp = LoadImageFromMinio(aviPath);
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"加载AVI图片失败: {ex.Message}");
                    }

                    return (originalWithBox, rawBmp, templateWithBox, gerberWithBox, aviBmp);
                }, cancellationToken);

                // 回到UI线程更新控件（Task.Run后自动回到调用线程的同步上下文）
                if (this.IsDisposed || _point != point)
                {
                    originalMarked?.Dispose();
                    rawOriginal?.Dispose();
                    templateMarked?.Dispose();
                    gerberMarked?.Dispose();
                    aviImage?.Dispose();
                    return;
                }

                ClearLoadedImages();
                _rawOriginalImage = rawOriginal;
                _defectBoxImage = originalMarked;
                _templateImage = templateMarked;
                _gerberImage = gerberMarked;
                _aviImage = aviImage;

                RefreshDisplayedImages();
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不需要处理
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"异步加载图片异常: {ex.Message}");
            }
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

        private void ClearAviImage()
        {
            ClearPictureBoxImage(pictureBox_AviImage);
            pictureBox_AviImage.Visible = false;
        }

        public void SetDisplayImageTypes(IEnumerable<DefectDisplayImageType> imageTypes)
        {
            _displayImageTypes = NormalizeDisplayImageTypes(imageTypes);
            RefreshDisplayedImages();
        }

        private static List<DefectDisplayImageType> CreateDefaultDisplayImageTypes()
        {
            return new List<DefectDisplayImageType>
            {
                DefectDisplayImageType.DefectBox,
                DefectDisplayImageType.Template,
                DefectDisplayImageType.Avi
            };
        }

        private static List<DefectDisplayImageType> NormalizeDisplayImageTypes(IEnumerable<DefectDisplayImageType> imageTypes)
        {
            var result = new List<DefectDisplayImageType>();
            foreach (var imageType in imageTypes ?? CreateDefaultDisplayImageTypes())
            {
                if (result.Contains(imageType)) continue;
                result.Add(imageType);
                if (result.Count == 3) break;
            }

            return result.Count > 0 ? result : CreateDefaultDisplayImageTypes();
        }

        private void RefreshDisplayedImages()
        {
            var firstImage = _displayImageTypes.Count > 0 ? GetLoadedImage(_displayImageTypes[0]) : null;
            var secondImage = _displayImageTypes.Count > 1 ? GetLoadedImage(_displayImageTypes[1]) : null;
            var thirdImage = _useHorizontalImageLayout && _displayImageTypes.Count > 2 ? GetLoadedImage(_displayImageTypes[2]) : null;

            SetPictureBoxImage(pictureBox_OriginalImage, CloneImage(firstImage));
            SetPictureBoxImage(pictureBox_TemplateImage, CloneImage(secondImage));
            SetPictureBoxImage(pictureBox_AviImage, CloneImage(thirdImage));
            pictureBox_AviImage.Visible = thirdImage != null;
            if (thirdImage != null) pictureBox_AviImage.BringToFront();
        }

        private Image GetLoadedImage(DefectDisplayImageType imageType)
        {
            switch (imageType)
            {
                case DefectDisplayImageType.Original:
                    return _rawOriginalImage;
                case DefectDisplayImageType.Template:
                    return _templateImage;
                case DefectDisplayImageType.Gerber:
                    return _gerberImage;
                case DefectDisplayImageType.Avi:
                    return _aviImage;
                default:
                    return _defectBoxImage;
            }
        }

        private static Image CloneImage(Image image)
        {
            return image == null ? null : (Image)image.Clone();
        }

        private void ClearLoadedImages()
        {
            ClearPictureBoxImage(pictureBox_OriginalImage);
            ClearPictureBoxImage(pictureBox_TemplateImage);
            ClearAviImage();
            _rawOriginalImage?.Dispose();
            _defectBoxImage?.Dispose();
            _templateImage?.Dispose();
            _gerberImage?.Dispose();
            _aviImage?.Dispose();
            _rawOriginalImage = null;
            _defectBoxImage = null;
            _templateImage = null;
            _gerberImage = null;
            _aviImage = null;
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
                label_Status.Text = $"AI: {GetStatusText(_point.AIStatus)}  |  VVS: {GetStatusText(_point.VVSStatus)}  |  VRS: {GetVrsStatusText(_point.VrsState)}";
            }
        }

        /// <summary>
        /// 更新 PcsIndex / DefectIndex 标签
        /// </summary>
        private void UpdateIndexLabel()
        {
            if (_point == null)
            {
                label_Index.Text = "";
                return;
            }
            label_Index.Text = $"PcsIndex:{_point.PcsIndex}  DefectIndex:{_point.DefectIndex}";
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
        /// 将 VRS 状态码转换为显示文本
        /// 约定：0=未判定, 1=OK, 2=NG, 3=忽略, 4=无结果, 5=NG不接收
        /// </summary>
        private string GetVrsStatusText(int vrsState)
        {
            switch (vrsState)
            {
                case 0: return "未判定";
                case 1: return "OK";
                case 2: return "NG";
                case 3: return "忽略";
                case 4: return "无结果";
                case 5: return "NG不接收";
                default: return vrsState.ToString();
            }
        }

        /// <summary>
        /// 获取已加载的原图（不带缺陷框）
        /// </summary>
        public Image OriginalImage => _rawOriginalImage;

        /// <summary>
        /// 获取已加载的模板图
        /// </summary>
        public Image TemplateImage => _templateImage;

        /// <summary>
        /// 调整图片区域高度以适应容器
        /// </summary>
        public void AdjustImageHeight(int totalHeight)
        {
            int availableHeight = totalHeight - panel_Header.Height - panel_Status.Height - this.Padding.Vertical;
            int imageHeight = _useHorizontalImageLayout ? availableHeight : availableHeight / 2;

            pictureBox_OriginalImage.Height = imageHeight;
            pictureBox_TemplateImage.Height = imageHeight;
        }

        public void SetImageLayout(bool horizontal)
        {
            if (_useHorizontalImageLayout == horizontal) return;

            _useHorizontalImageLayout = horizontal;
            tableImages.SuspendLayout();
            tableImages.Controls.Clear();
            tableImages.ColumnStyles.Clear();
            tableImages.RowStyles.Clear();

            if (horizontal)
            {
                tableImages.ColumnCount = 2;
                tableImages.RowCount = 1;
                tableImages.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                tableImages.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                tableImages.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                pictureBox_OriginalImage.Margin = new Padding(0, 0, 1, 0);
                pictureBox_TemplateImage.Margin = new Padding(1, 0, 0, 0);
                tableImages.Controls.Add(pictureBox_OriginalImage, 0, 0);
                tableImages.Controls.Add(pictureBox_TemplateImage, 1, 0);
            }
            else
            {
                tableImages.ColumnCount = 1;
                tableImages.RowCount = 2;
                tableImages.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tableImages.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                tableImages.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                pictureBox_OriginalImage.Margin = new Padding(0, 0, 0, 1);
                pictureBox_TemplateImage.Margin = new Padding(0, 1, 0, 0);
                tableImages.Controls.Add(pictureBox_OriginalImage, 0, 0);
                tableImages.Controls.Add(pictureBox_TemplateImage, 0, 1);
            }

            tableImages.ResumeLayout(true);
            RefreshDisplayedImages();
        }

        /// <summary>
        /// 直接设置显示数据（用于 FrmHome 等外部调用，不依赖 DetectInfo 的 ImagePath 自动加载）。
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

            ClearLoadedImages();
            _defectBoxImage = originalImage;
            _rawOriginalImage = rawOriginalImage;
            _templateImage = templateImage;
            RefreshDisplayedImages();

            // 状态
            label_Status.Text = statusText ?? "";

            // 索引信息（PcsIndex / DefectIndex）
            UpdateIndexLabel();

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
        public bool HasDisplayContent => _rawOriginalImage != null || _defectBoxImage != null || _templateImage != null || _gerberImage != null || _aviImage != null;

        /// <summary>
        /// 清空显示内容
        /// </summary>
        public void ClearDisplay()
        {
            _point = null;
            label_SN.Text = "";
            label_Status.Text = "";
            label_Index.Text = "";

            ClearLoadedImages();

            UpdateAppearance();
        }
    }
}


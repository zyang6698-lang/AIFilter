using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightDisplay.HeatMap
{
    public class HeatMapRenderer
    {
        private Color[] _heatPalette;
        private Bitmap _backgroundImage;
        private float _opacity = 0.7f;


        //public HeatMapRenderer(Bitmap backgroundImage = null)
        //{
        //    _backgroundImage = backgroundImage;
        //    InitializeHeatPalette();
        //}
        public HeatMapRenderer(Bitmap backgroundImage = null, float opacity = 0.7f)
        {
            _backgroundImage = backgroundImage;
            _opacity = Math.Max(0, Math.Min(1, opacity));
            InitializeHeatPalette();
        }
        // 设置透明度
        public void SetOpacity(float opacity)
        {
            _opacity = Math.Max(0, Math.Min(1, opacity));
            InitializeHeatPalette(); // 重新初始化调色板
        }

        // 初始化热力颜色调色板（从蓝色到红色）
        private void InitializeHeatPalette()
        {
            //_heatPalette = new Color[256];

            //// 创建从蓝色->青色->绿色->黄色->红色的渐变
            //for (int i = 0; i < 256; i++)
            //{
            //    float ratio = i / 255f;

            //    int r = (int)(255 * Math.Min(1, ratio * 2));
            //    int g = (int)(255 * Math.Min(1, (ratio - 0.25f) * 2));
            //    int b = (int)(255 * Math.Max(0, 1 - ratio * 2));

            //    // 确保颜色值在有效范围内
            //    r = Math.Max(0, Math.Min(255, r));
            //    g = Math.Max(0, Math.Min(255, g));
            //    b = Math.Max(0, Math.Min(255, b));

            //    // 根据设置的透明度调整Alpha值
            //    int alpha = (int)(0.8f * 255 * (0.3f + 0.7f * ratio)); // 低强度区域更透明
            //    alpha = Math.Max(0, Math.Min(255, alpha));

            //    _heatPalette[i] = Color.FromArgb(alpha, r, g, b);
            //    //_heatPalette[i] = Color.FromArgb(110, r, g, b); // 半透明效果
            //}

            //_heatPalette = new Color[256];

            //// 创建从蓝色->青色->绿色->黄色->红色的渐变
            //for (int i = 0; i < 256; i++)
            //{
            //    float ratio = i / 255f;

            //    int r = (int)(255 * Math.Min(1, ratio * 2));
            //    int g = (int)(255 * Math.Min(1, (ratio - 0.25f) * 2));
            //    int b = (int)(255 * Math.Max(0, 1 - ratio * 2));

            //    // 确保颜色值在有效范围内
            //    r = Math.Max(0, Math.Min(255, r));
            //    g = Math.Max(0, Math.Min(255, g));
            //    b = Math.Max(0, Math.Min(255, b));

            //    // 根据设置的透明度调整Alpha值
            //    int alpha = (int)(_opacity * 255 * (0.3f + 0.7f * ratio)); // 低强度区域更透明
            //    alpha = Math.Max(0, Math.Min(255, alpha));

            //    _heatPalette[i] = Color.FromArgb(alpha, r, g, b);
            //}


            //_heatPalette = new Color[256];

            //for (int i = 0; i < 256; i++)
            //{
            //    float ratio = i / 255f;

            //    // 我们只使用红色通道，绿色和蓝色通道根据比例调整
            //    int r = (int)(255 * ratio);
            //    int g = 0;
            //    int b = 0;

            //    // 调整透明度：低强度时透明，高强度时不透明
            //    // 使用非线性曲线调整透明度，使得低强度区域更透明，高强度区域更不透明
            //    // 例如：alpha = (ratio)^2 * 255 * _opacity
            //    float alphaFactor = (float)Math.Pow(ratio, 1.5); // 使用指数曲线，使得低强度区域更透明
            //    int alpha = (int)(alphaFactor * 255 * _opacity);

            //    _heatPalette[i] = Color.FromArgb(alpha, r, g, b);
            //}

            //_heatPalette = new Color[256];

            //for (int i = 0; i < 256; i++)
            //{
            //    float ratio = i / 255f;

            //    // 颜色从黄色（255,255,0）到红色（255,0,0）
            //    int r = 255;
            //    int g = (int)(255 * (1 - ratio));
            //    int b = 0;

            //    float alphaFactor = (float)Math.Pow(ratio, 1.5);
            //    int alpha = (int)(alphaFactor * 255 * _opacity);

            //    _heatPalette[i] = Color.FromArgb(alpha, r, g, b);
            //}

            _heatPalette = new Color[256];

            for (int i = 0; i < 256; i++)
            {
                float ratio = i / 255f;

                int r = (int)(255 * Math.Min(1, ratio * 2));
                int g = (int)(255 * Math.Min(1, (ratio - 0.25f) * 2));
                int b = (int)(255 * Math.Max(0, 1 - ratio * 2));

                r = Math.Max(0, Math.Min(255, r));
                g = Math.Max(0, Math.Min(255, g));
                b = Math.Max(0, Math.Min(255, b));

                // 调整透明度：使用ratio的平方根，使得低强度区域更透明
                float alphaFactor = (float)Math.Sqrt(ratio);
                int alpha = (int)(alphaFactor * 255 * _opacity);

                _heatPalette[i] = Color.FromArgb(alpha, r, g, b);
            }
        }
        // 生成热力分布图（作为透明覆盖层）
        public Bitmap GenerateHeatMapOverlay(List<HeatPointRenderer> points, Size imageSize)
        {
            if (points == null || points.Count == 0)
                return CreateTransparentBitmap(imageSize);

            Bitmap overlay = CreateTransparentBitmap(imageSize);
            float[,] heatData = new float[imageSize.Width, imageSize.Height];
            foreach (var point in points)
            {
                AddHeatPoint(heatData, point, imageSize);
            }
            ApplyHeatToOverlay(overlay, heatData);
            return overlay;
        }
        private Bitmap CreateTransparentBitmap(Size size)
        {
            Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
            }
            return bmp;
        }
        public void SetCustomPalette(Color[] colors)
        {
            if (colors != null && colors.Length == 256)
                _heatPalette = colors;
        }
       
        public Bitmap GenerateHeatMap(List<HeatPointRenderer> points, Size imageSize)
        {
            if (points == null || points.Count == 0)
                return _backgroundImage != null ? new Bitmap(_backgroundImage) : new Bitmap(imageSize.Width, imageSize.Height);

            float[,] heatData = new float[imageSize.Width, imageSize.Height];
            foreach (var point in points)
            {
                AddHeatPoint(heatData, point, imageSize);
            }

            Bitmap result = _backgroundImage != null ?
                new Bitmap(_backgroundImage) :
                new Bitmap(imageSize.Width, imageSize.Height);

            ApplyHeatToImage(result, heatData);
            return result;
        }

        private void AddHeatPoint(float[,] heatData, HeatPointRenderer point, Size imageSize)
        {
            int centerX = (int)point.Location.X;
            int centerY = (int)point.Location.Y;
            int radius = (int)point.Radius;

            // 计算影响区域
            int startX = Math.Max(0, centerX - radius);
            int endX = Math.Min(imageSize.Width - 1, centerX + radius);
            int startY = Math.Max(0, centerY - radius);
            int endY = Math.Min(imageSize.Height - 1, centerY + radius);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    float distance = Distance(new PointF(x, y), point.Location);
                    if (distance <= point.Radius)
                    {
                        // 高斯衰减函数
                        float influence = (float)Math.Exp(-distance * distance / (point.Radius * point.Radius * 0.5f));
                        heatData[x, y] += influence *1.0f;//point.Intensity;
                    }
                }
            }
        }

        private void ApplyHeatToImage(Bitmap image, float[,] heatData)
        {
            // 找到最大热力值用于归一化
            float maxHeat = 0;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < 450; y++)
                {
                    maxHeat = Math.Max(maxHeat, heatData[x, y]);
                }
            }
            maxHeat = (float)0.0490446575;
            if (maxHeat == 0) return;

            // 使用LockBits进行高性能像素操作
            BitmapData bmpData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;

                for (int y = 0; y < 450; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        float normalizedHeat = heatData[x, y] / maxHeat;
                        int paletteIndex = (int)(normalizedHeat * 255);
                        paletteIndex = Math.Max(0, Math.Min(255, paletteIndex));

                        Color heatColor = _heatPalette[paletteIndex];

                        if (heatColor.A > 0)
                        {
                            int index = y * bmpData.Stride + x * 4;

                            // 混合颜色
                            float alpha = heatColor.A / 255f;
                            ptr[index + 2] = (byte)(ptr[index + 2] * (1 - alpha) + heatColor.R * alpha); // R
                            ptr[index + 1] = (byte)(ptr[index + 1] * (1 - alpha) + heatColor.G * alpha); // G
                            ptr[index] = (byte)(ptr[index] * (1 - alpha) + heatColor.B * alpha);         // B
                        }
                    }
                }
            }

            image.UnlockBits(bmpData);
        }

        private void ApplyHeatToOverlay(Bitmap overlay, float[,] heatData)
        {
            // 找到最大热力值用于归一化
            float maxHeat = 0;
            for (int x = 0; x < overlay.Width; x++)
            {
                for (int y = 0; y < overlay.Height; y++)
                {
                    maxHeat = Math.Max(maxHeat, heatData[x, y]);
                }
            }

            if (maxHeat == 0) return;

            //使用LockBits进行高性能像素操作
            BitmapData bmpData = overlay.LockBits(
                new Rectangle(0, 0, overlay.Width, overlay.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;

                for (int y = 0; y < overlay.Height; y++)
                {
                    for (int x = 0; x < overlay.Width; x++)
                    {
                        float normalizedHeat = heatData[x, y] / maxHeat;
                        int paletteIndex = (int)(normalizedHeat * 255);
                        paletteIndex = Math.Max(0, Math.Min(255, paletteIndex));

                        Color heatColor = _heatPalette[paletteIndex];

                        int index = y * bmpData.Stride + x * 4;

                        // 直接设置覆盖层像素（不混合）
                        ptr[index + 3] = heatColor.A; // Alpha
                        ptr[index + 2] = heatColor.R; // R
                        ptr[index + 1] = heatColor.G; // G
                        ptr[index] = heatColor.B; // B
                    }
                }
            }

            overlay.UnlockBits(bmpData);
        }
        // 计算两点距离
        private float Distance(PointF p1, PointF p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
    public class HeatPointRenderer
    {
        public PointF Location { get; set; }
        public float Intensity { get; set; } // 0-1之间的强度值
        public float Radius { get; set; }    // 影响半径

        public HeatPointRenderer(PointF location, float intensity, float radius = 50)
        {
            Location = location;
            Intensity = Math.Max(0, Math.Min(1, intensity));
            Radius = radius;
        }
    }
}

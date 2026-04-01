using DeepSightDB;
using DeepSightModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightDisplay
{
    public static class ImageHelper
    {
        public static Bitmap CombineHeatMapWithBackground(Bitmap background, Bitmap heatMap)
        {
            if (background == null) return heatMap;
            if (heatMap == null) return background;

            Bitmap result = new Bitmap(background.Width, background.Height);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.DrawImage(background, 0, 0);
                g.DrawImage(heatMap, 0, 0);
            }
            return result;
        }

        /// <summary>
        /// 在图片上绘制缺陷框和缺陷名称
        /// </summary>
        /// <param name="originalImage">原始图片</param>
        /// <param name="heatPoint">缺陷信息</param>
        /// <returns>绘制了缺陷框的图片</returns>
        public static Bitmap DrawDefectBoxOnImage(Image originalImage, DetectInfo heatPoint)
        {
            Bitmap result = new Bitmap(originalImage);

            // 检查是否有有效的缺陷框信息
            if (heatPoint.Width <= 0 || heatPoint.Height <= 0)
            {
                return result;
            }

            using (Graphics g = Graphics.FromImage(result))
            {
                // 设置绘制质量
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // 缺陷框颜色 - 使用醒目的红色
                Color boxColor = Color.Red;
                using (Pen pen = new Pen(boxColor, 4))
                {
                    // 绘制缺陷框
                    Rectangle defectRect = new Rectangle(
                        heatPoint.RoiX,
                        heatPoint.RoiY,
                        heatPoint.Width,
                        heatPoint.Height);

                    g.DrawRectangle(pen, defectRect);
                }

                // 绘制缺陷名称
                if (!string.IsNullOrEmpty(heatPoint.DefectName))
                {
                    using (Font font = new Font("微软雅黑", 10F, FontStyle.Bold))
                    using (SolidBrush textBrush = new SolidBrush(Color.Yellow))
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                    {
                        // 计算文字大小
                        SizeF textSize = g.MeasureString(heatPoint.DefectName, font);

                        // 文字位置：缺陷框上方
                        float textX = heatPoint.RoiX;
                        float textY = heatPoint.RoiY - textSize.Height - 2;

                        // 如果文字超出图片上边界，则显示在缺陷框下方
                        if (textY < 0)
                        {
                            textY = heatPoint.RoiY + heatPoint.Height + 2;
                        }

                        // 如果文字超出图片右边界，调整位置
                        if (textX + textSize.Width > result.Width)
                        {
                            textX = result.Width - textSize.Width - 2;
                        }

                        // 确保文字不超出左边界
                        if (textX < 0)
                        {
                            textX = 2;
                        }

                        // 绘制文字背景
                        RectangleF bgRect = new RectangleF(textX - 2, textY - 1, textSize.Width + 4, textSize.Height + 2);
                        g.FillRectangle(bgBrush, bgRect);

                        // 绘制文字
                        g.DrawString(heatPoint.DefectName, font, textBrush, textX, textY);
                    }
                }
            }

            return result;
        }
    }
}

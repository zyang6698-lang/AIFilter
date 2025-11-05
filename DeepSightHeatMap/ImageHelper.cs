using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightHeatMap
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
    }
}

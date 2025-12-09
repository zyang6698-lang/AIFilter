using DeepSightDB;
using DeepSightHeatMap;
using DeepSightModel;
using DeepSightTool;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightHeatMap
{
    public class HeatMapManager
    {
        public HeatMapControl HeatMapControl { get; private set; }
        private readonly List<HeatPointRenderer> _heatPoints = new List<HeatPointRenderer>();
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public HeatMapManager()
        {
        }

        public void InitializeHeatMap(Mat sourceImage)
        {
            HeatMapControl = new HeatMapControl
            {
                Dock = DockStyle.Fill,
                BackgroundImage = sourceImage?.ToBitmap(),
                HeatMapOpacity = 1f
            };
            if (sourceImage != null)
            {
                HeatMapControl.Width = HeatMapControl.BackgroundImage.Width;
                HeatMapControl.Height = HeatMapControl.BackgroundImage.Height;
            }
        }

        public async Task UpdateHeatMapPointsAsync(
            ConcurrentDictionary<string, List<DetectInfo>> dicHeatPints,
            List<string> selectedDefectNames,
            Func<string,  int,  int, bool> tryParseSnPosition,
            Mat sourceImage,
            Action<Mat> updateDisplayAction)
        {
            if (HeatMapControl?.BackgroundImage == null)
            {
                return;
            }

            _heatPoints.Clear();

            var heatPoints = dicHeatPints
                .AsParallel()
                .SelectMany(kvp =>
                {
                    var sn = kvp.Key;
                    int row=0, col=0;
                    if (!tryParseSnPosition(sn,  row,  col))
                    {
                        return Enumerable.Empty<HeatPointRenderer>();
                    }

                    float productWidth = sourceImage?.Width ?? 0;
                    float productHeight = sourceImage?.Height ?? 0;
                    float colOffset = col * productWidth;
                    float rowOffset = row * productHeight;

                    return kvp.Value.Where(p => selectedDefectNames.Contains(p.DefectName))
                        .Select(pointInfo => new HeatPointRenderer(
                            location: new PointF(
                                (pointInfo.RoiX * 0.1f - OffsetX) + colOffset,
                                (pointInfo.RoiY * 0.1f - OffsetY) + rowOffset
                            ),
                            intensity: 0.25f,
                            radius: 25
                        ));
                })
                .ToList();

            _heatPoints.AddRange(heatPoints);
            LogTextHelper.Info($"热力点位数：{_heatPoints.Count}");

            await Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                HeatMapControl.SetHeatPoints(_heatPoints);
                if (HeatMapControl._heatMapOverlay != null)
                {
                    var finalImage = ImageHelper.CombineHeatMapWithBackground(HeatMapControl.BackgroundImage, HeatMapControl._heatMapOverlay);
                    updateDisplayAction(BitmapConverter.ToMat(finalImage));
                }
                sw.Stop();
                LogTextHelper.Info($"COST :{sw.ElapsedMilliseconds} ms");
            });
        }

        public void ClearHeatPoints()
        {
            _heatPoints.Clear();
            HeatMapControl?.ClearHeatPoints();
        }

        public void SetBackgroundImage(Bitmap image)
        {
            if (HeatMapControl != null)
            {
                HeatMapControl.BackgroundImage = image;
            }
        }
    }
}

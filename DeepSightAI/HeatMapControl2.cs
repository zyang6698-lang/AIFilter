using DeepSightDB;
using DeepSightDisplay;
using DeepSightModel;
using DeepSightTool;
using HalconDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class HeatMapControl2 : UserControl
    {
        private Dictionary<string, List<AVI_HeatPoints>> dic_heatPints = new Dictionary<string, List<AVI_HeatPoints>>();
        /// <summary>
        /// 产品图原点坐标距离大图原点坐标的偏移,渲染热力图坐标时需要减去此坐标
        /// </summary>
        public int offsetX;
        public int offsetY;
        private HeatMapRenderer _heatMapRenderer;
        private List<HeatPoint> _heatPoints = new List<HeatPoint>();
        private Bitmap _heatMapOverlay = null;

        public CvDisplay[] DispWinHeatMap = null;


        public HeatMapControl2()
        {
            InitializeComponent();
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            DispWinHeatMap = new CvDisplay[1];
            table_HeatMap.Controls.Clear();
            table_HeatMap.RowStyles.Clear();
            table_HeatMap.ColumnStyles.Clear();

            table_HeatMap.ColumnCount = 1;
            table_HeatMap.RowCount = 1;

            table_HeatMap.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            table_HeatMap.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));

            DispWinHeatMap[0] = new CvDisplay
            {
                Margin = new System.Windows.Forms.Padding(1),
                BackColor = ColorTranslator.FromHtml("#374c50"),
                Dock = System.Windows.Forms.DockStyle.Fill,
                Name = "DisplayHeatMap",
                AutoDisplay = CvDisplay.AutoDisplayMode.Fit,
                stationIndex = 1
            };
            table_HeatMap.Controls.Add(DispWinHeatMap[0], 0, 0);

        }

        private void btn_loadArryImage_Click(object sender, EventArgs e)
        {
            string path = SelectImageFile();
            if (!string.IsNullOrEmpty(path))
            {
                Rect rect = new Rect();
                GetProductROI(path, ref rect);
                offsetX = rect.X;
                offsetY = rect.Y;
                Mat mt = Cv2.ImRead(path);
                //缩放0.1倍
                Cv2.Resize(mt, mt, new OpenCvSharp.Size(mt.Width * 0.1, mt.Height * 0.1));
                mt = new Mat(mt, rect);
                DispWinHeatMap[0].Image = mt; // 设置背景图
                _heatMapOverlay = null; // 清除旧的热力图
                DispWinHeatMap[0].Invalidate(); // 触发重绘
            }
        }

        private void btn_setPanel_Click(object sender, EventArgs e)
        {
            try
            {
                if (DispWinHeatMap[0].Image == null)
                {
                    MessageBox.Show("请先加载Array图像");
                    return;
                }
                // 行列数
                int row = Convert.ToInt32(this.txt_Row.Text);
                int column = Convert.ToInt32(this.txt_Column.Text);

                Mat sourceMat = DispWinHeatMap[0].Image.Clone();
                if (sourceMat.Empty())
                {
                    LogTextHelper.Error("无法读取源图像");
                    return;
                }

                Mat[,] matGrid = new Mat[row, column];
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < column; j++)
                    {
                        matGrid[i, j] = sourceMat.Clone();
                    }
                }
                Mat resultImage = StitchImages(matGrid, row, column);
                DispWinHeatMap[0].Image = resultImage;
                _heatMapOverlay = null; // 清除热力图
                DispWinHeatMap[0].Invalidate(); // 触发重绘
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"制作Array图像失败: {ex.Message}");
            }
        }

        private async void rbn_Front_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateHeatMapPointsAsync();
        }

        private async void btn_queryHeatPoint_Click(object sender, EventArgs e)
        {
            try
            {
                dic_heatPints.Clear();
                _heatPoints.Clear();

                List<string> sn_list = GetSnListByLot(this.txt_heatCode.Text.ToString());
                if (sn_list.Count > 0)
                {
                    // 并行查询以提高效率
                    var tasks = sn_list.Select(sn => Task.Run(() => queryHeatDataBySn(sn))).ToList();
                    await Task.WhenAll(tasks);

                    await UpdateHeatMapPointsAsync();
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
            }
        }

        #region Selection (框选) Logic





        #endregion

        #region Helper Methods

        private async Task UpdateHeatMapPointsAsync()
        {
            if (DispWinHeatMap[0].Image == null)
            {
                MessageBox.Show("请先加载Array图像");
                return;
            }

            _heatPoints.Clear();
            string sideFilter = rbn_Front.Checked ? "A" : "B";

            foreach (var sn_dic in dic_heatPints)
            {
                foreach (var avi_points in sn_dic.Value.Where(p => p.Side == sideFilter))
                {
                    if (avi_points?.pointsInfos != null)
                    {
                        foreach (var pointInfo in avi_points.pointsInfos)
                        {
                            var point = new HeatPoint(
                                location: new PointF(
                                    pointInfo.X * 0.1f - offsetX,
                                    pointInfo.Y * 0.1f - offsetY
                                ),
                                intensity: 0.25f,
                                radius: 100
                            );
                            _heatPoints.Add(point);
                        }
                    }
                }
            }

            LogTextHelper.Info($"热力点位数：{_heatPoints.Count}");

            await Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                // 使用渲染器生成热力图
                _heatMapRenderer = new HeatMapRenderer(null, 0.7f);
                _heatMapOverlay = _heatMapRenderer.GenerateHeatMapOverlay(_heatPoints, new System.Drawing.Size(DispWinHeatMap[0].Image.Size().Width, DispWinHeatMap[0].Image.Size().Height));
                sw.Stop();
                LogTextHelper.Info($"COST :{sw.ElapsedMilliseconds} ms");
            });

            // 触发重绘以显示新的热力图
            DispWinHeatMap[0].Invalidate();
        }

        private RectangleF ConvertRectangleToImage(CvDisplay control, System.Drawing.Rectangle screenRect)
        {
            if (control.Image == null || control.GraphicsMat == null)
            {
                return RectangleF.Empty;
            }

            Rect2d dispRect = control.GraphicsMat.DispRect;
            if (dispRect.Width <= 0 || dispRect.Height <= 0)
            {
                return RectangleF.Empty;
            }

            // 计算从控件坐标到图像显示区域（DispRect）的缩放比例
            float scaleX = (float)(dispRect.Width / control.ClientSize.Width);
            float scaleY = (float)(dispRect.Height / control.ClientSize.Height);

            // 将屏幕矩形坐标转换为图像坐标
            float imageX = (float)dispRect.X + (screenRect.X * scaleX);
            float imageY = (float)dispRect.Y + (screenRect.Y * scaleY);
            float imageWidth = screenRect.Width * scaleX;
            float imageHeight = screenRect.Height * scaleY;

            return new RectangleF(imageX, imageY, imageWidth, imageHeight);
        }

        private string SelectImageFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "选择图片文件";
                openFileDialog.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.webp|所有文件|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = false;
                openFileDialog.CheckPathExists = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }
            }
            return null;
        }

        private void GetProductROI(string path, ref Rect rect)
        {
            HObject ho_Image = null, ho_GrayImage = null, ho_Edges = null, ho_SelectedEdges = null;
            HObject ho_ImageMean = null, ho_DarkRegions = null, ho_ClosedRegions = null;
            HObject ho_FilledRegions = null, ho_ConnectedRegions = null, ho_CurrentRegion = null;
            HObject ho_LargestRegion = null;

            HTuple hv_NumberOfRegions = new HTuple(), hv_MaxArea = new HTuple();
            HTuple hv_LargestRegionIndex = new HTuple(), hv_i = new HTuple();
            HTuple hv_Area = new HTuple(), hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
            HTuple hv_Row2 = new HTuple(), hv_Column2 = new HTuple();
            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();

            try
            {
                HOperatorSet.GenEmptyObj(out ho_Image);
                HOperatorSet.GenEmptyObj(out ho_GrayImage);
                HOperatorSet.GenEmptyObj(out ho_Edges);
                HOperatorSet.GenEmptyObj(out ho_SelectedEdges);
                HOperatorSet.GenEmptyObj(out ho_ImageMean);
                HOperatorSet.GenEmptyObj(out ho_DarkRegions);
                HOperatorSet.GenEmptyObj(out ho_ClosedRegions);
                HOperatorSet.GenEmptyObj(out ho_FilledRegions);
                HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
                HOperatorSet.GenEmptyObj(out ho_CurrentRegion);
                HOperatorSet.GenEmptyObj(out ho_LargestRegion);

                ho_Image.Dispose();
                HOperatorSet.ReadImage(out ho_Image, path);
                HOperatorSet.ZoomImageFactor(ho_Image, out ho_Image, 0.1, 0.1, "bilinear");
                ho_GrayImage.Dispose();
                HOperatorSet.Rgb1ToGray(ho_Image, out ho_GrayImage);
                ho_Edges.Dispose();
                HOperatorSet.EdgesSubPix(ho_GrayImage, out ho_Edges, "canny", 1.8, 20, 40);
                ho_SelectedEdges.Dispose();
                HOperatorSet.SelectShapeXld(ho_Edges, out ho_SelectedEdges, "rectangularity", "and", 0.8, 1000);
                ho_ImageMean.Dispose();
                HOperatorSet.MeanImage(ho_GrayImage, out ho_ImageMean, 20, 20);
                ho_DarkRegions.Dispose();
                HOperatorSet.DynThreshold(ho_GrayImage, ho_ImageMean, out ho_DarkRegions, 8, "dark");
                ho_ClosedRegions.Dispose();
                HOperatorSet.ClosingRectangle1(ho_DarkRegions, out ho_ClosedRegions, 2, 2);
                ho_FilledRegions.Dispose();
                HOperatorSet.FillUp(ho_ClosedRegions, out ho_FilledRegions);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_FilledRegions, out ho_ConnectedRegions);
                HOperatorSet.CountObj(ho_ConnectedRegions, out hv_NumberOfRegions);

                hv_MaxArea.Dispose();
                hv_MaxArea = 0;
                hv_LargestRegionIndex.Dispose();
                hv_LargestRegionIndex = -1;

                for (hv_i = 1; hv_i.Continue(hv_NumberOfRegions, 1); hv_i = hv_i.TupleAdd(1))
                {
                    ho_CurrentRegion.Dispose();
                    HOperatorSet.SelectObj(ho_ConnectedRegions, out ho_CurrentRegion, hv_i);
                    HOperatorSet.AreaCenter(ho_CurrentRegion, out hv_Area, out hv_Row, out hv_Column);
                    if ((int)(new HTuple(hv_Area.TupleGreater(hv_MaxArea))) != 0)
                    {
                        hv_MaxArea.Dispose();
                        hv_MaxArea = new HTuple(hv_Area);
                        hv_LargestRegionIndex.Dispose();
                        hv_LargestRegionIndex = new HTuple(hv_i);
                    }
                }
                if ((int)(new HTuple(hv_LargestRegionIndex.TupleNotEqual(-1))) != 0)
                {
                    ho_LargestRegion.Dispose();
                    HOperatorSet.SelectObj(ho_ConnectedRegions, out ho_LargestRegion, hv_LargestRegionIndex);
                    HOperatorSet.SmallestRectangle1(ho_LargestRegion, out hv_Row1, out hv_Column1, out hv_Row2, out hv_Column2);
                    hv_Width.Dispose();
                    hv_Width = hv_Column2.TupleSub(hv_Column1).TupleAdd(1);
                    hv_Height.Dispose();
                    hv_Height = hv_Row2.TupleSub(hv_Row1).TupleAdd(1);
                    rect.X = hv_Column1.I;
                    rect.Y = hv_Row1.I;
                    rect.Width = hv_Width.I;
                    rect.Height = hv_Height.I;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Warn("裁图算法异常" + ex.ToString());
            }
            finally
            {
                ho_Image?.Dispose();
                ho_GrayImage?.Dispose();
                ho_Edges?.Dispose();
                ho_SelectedEdges?.Dispose();
                ho_ImageMean?.Dispose();
                ho_DarkRegions?.Dispose();
                ho_ClosedRegions?.Dispose();
                ho_FilledRegions?.Dispose();
                ho_ConnectedRegions?.Dispose();
                ho_CurrentRegion?.Dispose();
                ho_LargestRegion?.Dispose();

                hv_NumberOfRegions?.Dispose();
                hv_MaxArea?.Dispose();
                hv_LargestRegionIndex?.Dispose();
                hv_i?.Dispose();
                hv_Area?.Dispose();
                hv_Row?.Dispose();
                hv_Column?.Dispose();
                hv_Row1?.Dispose();
                hv_Column1?.Dispose();
                hv_Row2?.Dispose();
                hv_Column2?.Dispose();
                hv_Width?.Dispose();
                hv_Height?.Dispose();
            }
        }

        private Mat StitchImages(Mat[,] matGrid, int rows, int cols)
        {
            Mat[] rowMats = new Mat[rows];
            for (int i = 0; i < rows; i++)
            {
                Mat[] colMats = new Mat[cols];
                for (int j = 0; j < cols; j++)
                {
                    colMats[j] = matGrid[i, j];
                }
                Mat rowMat = new Mat();
                Cv2.HConcat(colMats, rowMat);
                rowMats[i] = rowMat;
                foreach (var mat in colMats) mat?.Dispose();
            }
            Mat result = new Mat();
            Cv2.VConcat(rowMats, result);
            foreach (var rowMat in rowMats) rowMat?.Dispose();
            return result;
        }

        private List<string> GetSnListByLot(string Lot)
        {
            try
            {
                List<string> rtn_list = new List<string>();
                RootDbInfo info = new RootDbInfo
                {
                    db_name = "panel_list",
                    key = Lot,
                    op_mode = "all",
                    uniqueKey = Guid.NewGuid().ToString(),
                    operation = "get"
                };
                string outInfo = null;
                Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, info, 0, out outInfo);
                if (outInfo != null)
                {
                    var json = JObject.Parse(outInfo);
                    string res = json["value"].ToString();
                    if (outInfo.Contains("err_key_found") || res == "")
                    {
                        return new List<string>();
                    }
                    rtn_list.AddRange(res.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                }
                return rtn_list;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("GetSnListByLot 异常: " + ex.ToString());
                return new List<string>();
            }
        }

        private bool queryHeatDataBySn(string sn)
        {
            try
            {
                // Side A
                QueryAndStoreHeatPoints(sn, "A");
                // Side B
                QueryAndStoreHeatPoints(sn, "B");
                return true;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"queryHeatDataBySn for {sn} 异常: {ex.ToString()}");
                return false;
            }
        }

        private void QueryAndStoreHeatPoints(string sn, string side)
        {
            RootDbInfo Info = new RootDbInfo
            {
                db_name = "AVI_HeatPoints",
                operation = "get",
                op_mode = "last",
                key = $"{sn}_{side}"
            };
            string Result;
            Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, Info, 1, out Result);
            if (string.IsNullOrEmpty(Result)) return;

            var jsonStr = JObject.Parse(Result);
            string str = jsonStr["value"].ToString();
            if (!string.IsNullOrWhiteSpace(str) && !str.Contains("err_key_found"))
            {
                AVI_HeatPoints avi_HeatInfo = JsonConvert.DeserializeObject<AVI_HeatPoints>(str);
                if (avi_HeatInfo != null)
                {
                    // 使用线程安全的 ConcurrentDictionary 或 lock
                    lock (dic_heatPints)
                    {
                        if (!dic_heatPints.ContainsKey(sn))
                        {
                            dic_heatPints[sn] = new List<AVI_HeatPoints>();
                        }
                        dic_heatPints[sn].Add(avi_HeatInfo);
                    }
                }
            }
        }

        #endregion
    }
}
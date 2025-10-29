using DeepSightDB;
using DeepSightDisplay;
using DeepSightModel;
using DeepSightTool;
using HalconDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Sunny.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class HeatMapControl2 : UserControl
    {
        private readonly ConcurrentDictionary<string, List<AVI_HeatPoints>> _dicHeatPints = new ConcurrentDictionary<string, List<AVI_HeatPoints>>();
        /// <summary>
        /// 产品图原点坐标距离大图原点坐标的偏移,渲染热力图坐标时需要减去此坐标
        /// </summary>
        public int offsetX;
        public int offsetY;
        private HeatMapRenderer _heatMapRenderer;
        private readonly List<HeatPoint> _heatPoints = new List<HeatPoint>();
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
                using (Mat mt = Cv2.ImRead(path))
                {
                    if (mt.Empty()) return;
                    //缩放0.1倍
                    using (Mat resized = mt.Resize(new OpenCvSharp.Size(mt.Width * 0.1, mt.Height * 0.1)))
                    {
                        DispWinHeatMap[0].Image = new Mat(resized, rect); // 设置背景图
                    }
                }
                _heatMapOverlay?.Dispose();
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

                using (Mat sourceMat = DispWinHeatMap[0].Image)
                {
                    if (sourceMat.Empty())
                    {
                        LogTextHelper.Error("无法读取源图像");
                        return;
                    }

                    // 优化：直接引用源图像，避免多次克隆
                    Mat[,] matGrid = new Mat[row, column];
                    for (int i = 0; i < row; i++)
                    {
                        for (int j = 0; j < column; j++)
                        {
                            matGrid[i, j] = sourceMat;
                        }
                    }
                    DispWinHeatMap[0].Image = StitchImages(matGrid, row, column, cloneSource: false);
                }
                _heatMapOverlay?.Dispose();
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
            btn_queryHeatPoint.Enabled = false;
            try
            {
                List<string> sn_list = null;
                if (!txt_Lot.Text.IsNullOrEmpty())
                {
                    sn_list = await GetSnListByLotAsync(this.txt_Lot.Text);
                }
                else if (timePicker.Checked)
                {
                    // 如果 ComboBox 为空，则先根据日期填充料号列表
                    if (cmb_PartNumber.Items.Count == 0)
                    {
                        await GetSnByPnTimeAsync(timePicker.Value);
                        cmb_PartNumber.Items.Clear();
                        var partNumbers = dic_PN_SNList.Keys.Distinct().ToArray();
                        cmb_PartNumber.Items.AddRange(partNumbers);

                        if (partNumbers.Length > 0)
                        {
                            cmb_PartNumber.SelectedIndex = 0;
                            MessageBox.Show($"已加载当天料号列表，并已自动选择第一个料号进行查询。");
                        }
                        else
                        {
                            MessageBox.Show($"当天未查询到任何料号数据。");
                            return;
                        }
                    }

                    if (!string.IsNullOrEmpty(cmb_PartNumber.Text) && dic_PN_SNList.TryGetValue(this.cmb_PartNumber.Text, out sn_list))
                    {
                        // sn_list 已被赋值
                    }
                    else
                    {
                        MessageBox.Show($"未找到料号 {cmb_PartNumber.Text} 在该日期下的 SN 数据。");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("请输入Lot号，或勾选日期并选择一个料号。");
                    return;
                }

                if (sn_list != null && sn_list.Count > 0)
                {
                    await ProcessSnListAndUpdateHeatMapAsync(sn_list);
                }
                else
                {
                    MessageBox.Show("未查询到任何SN数据。");
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                MessageBox.Show($"查询失败: {ex.Message}");
            }
            finally
            {
                btn_queryHeatPoint.Enabled = true;
            }
        }

        private async Task ProcessSnListAndUpdateHeatMapAsync(List<string> sn_list)
        {
            if (sn_list == null || sn_list.Count == 0) return;

            _dicHeatPints.Clear();
            _heatPoints.Clear();

            // 使用 SemaphoreSlim 控制并发度，防止压垮服务器
            var semaphore = new SemaphoreSlim(10); // 例如，最多10个并发请求
            var tasks = new List<Task>();

            foreach (var sn in sn_list)
            {
                await semaphore.WaitAsync();
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await queryHeatDataBySnAsync(sn);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }
            await Task.WhenAll(tasks);

            await UpdateHeatMapPointsAsync();
        }

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

            // 数据准备阶段也放入后台线程
            var preparedPoints = await Task.Run(() =>
            {
                var points = new List<HeatPoint>();
                foreach (var sn_dic in _dicHeatPints)
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
                                points.Add(point);
                            }
                        }
                    }
                }
                return points;
            });

            _heatPoints.AddRange(preparedPoints);
            LogTextHelper.Info($"热力点位数：{_heatPoints.Count}");

            Bitmap newOverlay = null;
            if (_heatPoints.Count > 0)
            {
                newOverlay = await Task.Run(() =>
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var renderer = new HeatMapRenderer(null, 0.7f);
                    var overlay = renderer.GenerateHeatMapOverlay(_heatPoints,new System.Drawing.Size( DispWinHeatMap[0].Image.Size().Width, DispWinHeatMap[0].Image.Size().Height));
                    sw.Stop();
                    LogTextHelper.Info($"COST :{sw.ElapsedMilliseconds} ms");
                    return overlay;
                });
            }

            // 安全地替换和释放旧的位图
            var oldOverlay = _heatMapOverlay;
            _heatMapOverlay = newOverlay;
            oldOverlay?.Dispose();

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

        private Mat StitchImages(Mat[,] matGrid, int rows, int cols, bool cloneSource = true)
        {
            Mat[] rowMats = new Mat[rows];
            for (int i = 0; i < rows; i++)
            {
                Mat[] colMats = new Mat[cols];
                for (int j = 0; j < cols; j++)
                {
                    colMats[j] = cloneSource ? matGrid[i, j].Clone() : matGrid[i, j];
                }
                Mat rowMat = new Mat();
                Cv2.HConcat(colMats, rowMat);
                rowMats[i] = rowMat;
                if (cloneSource)
                {
                    foreach (var mat in colMats) mat?.Dispose();
                }
            }
            Mat result = new Mat();
            Cv2.VConcat(rowMats, result);
            foreach (var rowMat in rowMats) rowMat?.Dispose();
            return result;
        }

        private async Task<List<string>> GetSnListByLotAsync(string lot)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var rtn_list = new List<string>();
                    RootDbInfo info = new RootDbInfo
                    {
                        db_name = "panel_list",
                        key = lot,
                        op_mode = "all",
                        uniqueKey = Guid.NewGuid().ToString(),
                        operation = "get"
                    };
                    if (Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, info, 0, out string outInfo) && outInfo != null)
                    {
                        var json = JObject.Parse(outInfo);
                        string res = json["value"]?.ToString();
                        if (!string.IsNullOrEmpty(res) && !outInfo.Contains("err_key_found"))
                        {
                            rtn_list.AddRange(res.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                        }
                    }
                    return rtn_list;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error("GetSnListByLot 异常: " + ex.ToString());
                    return new List<string>();
                }
            });
        }

        private readonly ConcurrentDictionary<string, List<string>> dic_PN_SNList = new ConcurrentDictionary<string, List<string>>();
        private async Task GetSnByPnTimeAsync(DateTime date)
        {
            await Task.Run(() =>
            {
                try
                {
                    dic_PN_SNList.Clear(); // 开始前先清空，以防旧数据干扰

                    if (Machine.master.workClass.ReadPNSNByTime(date, out string outInfo) && !string.IsNullOrEmpty(outInfo))
                    {
                        var root = JObject.Parse(outInfo);
                        var dataList = root["data_list"] as JArray;

                        if (dataList == null) return;

                        foreach (var item in dataList)
                        {
                            string itemStr = item.ToString();
                            if (itemStr == "end_range_send") continue;

                            var itemObj = JObject.Parse(itemStr);
                            string valueStr = itemObj["value"]?.ToString();

                            if (string.IsNullOrEmpty(valueStr)) continue;

                            var valueObj = JObject.Parse(valueStr);
                            string productSerial = valueObj["ProductSerial"]?.ToString();
                            string serialNumber = valueObj["SerialNumber"]?.ToString();

                            if (!string.IsNullOrEmpty(productSerial) && !string.IsNullOrEmpty(serialNumber))
                            {
                                dic_PN_SNList.AddOrUpdate(productSerial,
                                    new List<string> { serialNumber },
                                    (key, existingList) =>
                                    {
                                        if (!existingList.Contains(serialNumber))
                                        {
                                            existingList.Add(serialNumber);
                                        }
                                        return existingList;
                                    });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error("GetSnByPnTime 异常: " + ex.ToString());
                }
            });
        }

        private async Task queryHeatDataBySnAsync(string sn)
        {
            await Task.WhenAll(
                QueryAndStoreHeatPointsAsync(sn, "A"),
                QueryAndStoreHeatPointsAsync(sn, "B")
            );
        }

        private async Task QueryAndStoreHeatPointsAsync(string sn, string side)
        {
            await Task.Run(() =>
            {
                try
                {
                    RootDbInfo Info = new RootDbInfo
                    {
                        db_name = "AVI_HeatPoints",
                        operation = "get",
                        op_mode = "last",
                        key = $"{sn}_{side}"
                    };
                    if (Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, Info, 1, out string Result) && !string.IsNullOrEmpty(Result))
                    {
                        var jsonStr = JObject.Parse(Result);
                        string str = jsonStr["value"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(str) && !str.Contains("err_key_found"))
                        {
                            AVI_HeatPoints avi_HeatInfo = JsonConvert.DeserializeObject<AVI_HeatPoints>(str);
                            if (avi_HeatInfo != null)
                            {
                                _dicHeatPints.AddOrUpdate(sn,
                                    new List<AVI_HeatPoints> { avi_HeatInfo },
                                    (key, existingList) => { existingList.Add(avi_HeatInfo); return existingList; });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"QueryAndStoreHeatPoints for {sn}_{side} 异常: {ex.ToString()}");
                }
            });
        }

        #endregion
    }
}
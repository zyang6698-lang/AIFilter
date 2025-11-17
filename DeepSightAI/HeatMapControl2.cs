#define TEST_ENV
using DeepSightDB;
using DeepSightDisplay;
using DeepSightHeatMap;
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
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class HeatMapControl2 : UserControl
    {
        #region Fields and Properties

        private readonly ConcurrentDictionary<string, List<AVI_HeatPoints>> dic_heatPints = new ConcurrentDictionary<string, List<AVI_HeatPoints>>();
        private readonly HeatMapManager _heatMapManager = new HeatMapManager();
        private Mat SourceImage = null;
        private ConcurrentDictionary<string, List<string>> dic_PN_SNList = new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// 产品图原点坐标距离大图原点坐标的偏移,渲染热力图坐标时需要减去此坐标
        /// </summary>
        public int offsetX
        {
            get => _heatMapManager.OffsetX;
            set => _heatMapManager.OffsetX = value;
        }
        public int offsetY
        {
            get => _heatMapManager.OffsetY;
            set => _heatMapManager.OffsetY = value;
        }

        public CvDisplay DispWinHeatMap = null;
        private List<dynamic> _pointsInSelection = new List<dynamic>();
        private int _loadedDetailsCount = 0;
        private const int PageSize = 50;
        private Button _loadMoreButton = null;
        private const int mockPointsCount = 30;
        private string[,] _arrayConfig = null;

        #endregion

        #region Initialization

        public HeatMapControl2()
        {
            InitializeComponent();
            InitializeLayout();
            this.VisibleChanged += HeatMapControl2_VisibleChanged;
        }

        private void HeatMapControl2_VisibleChanged(object sender, EventArgs e)
        {
            // 确保只在控件变为可见时加载，并且只加载一次
            if (this.Visible)
            {
                LoadInitialImage();
                InitializeHeatMap();

                // 取消订阅，避免重复加载
                this.VisibleChanged -= HeatMapControl2_VisibleChanged;
            }
        }

        private void InitializeLayout()
        {
            DispWinHeatMap = new CvDisplay();
            table_HeatMap.Controls.Clear();
            table_HeatMap.RowStyles.Clear();
            table_HeatMap.ColumnStyles.Clear();

            table_HeatMap.ColumnCount = 1;
            table_HeatMap.RowCount = 1;

            table_HeatMap.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            table_HeatMap.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));

            DispWinHeatMap = new CvDisplay
            {
                Margin = new System.Windows.Forms.Padding(1),
                BackColor = ColorTranslator.FromHtml("#374c50"),
                Dock = System.Windows.Forms.DockStyle.Fill,
                Name = "DisplayHeatMap",
                AutoDisplay = CvDisplay.AutoDisplayMode.Fit,
                stationIndex = 1
            };
            DispWinHeatMap.OnSelectionFinished += CvDisplay1_OnSelectionFinished;
            DispWinHeatMap.OnCallBackFullShowPro -= FrHome_OnCallBackFullShowPro;
            DispWinHeatMap.OnCallBackFullShowPro += FrHome_OnCallBackFullShowPro;
            table_HeatMap.Controls.Add(DispWinHeatMap, 0, 0);
        }


        private void InitializeHeatMap()
        {
            _heatMapManager.InitializeHeatMap(SourceImage);
        }

        private void LoadInitialImage()
        {
            string dirPath = Path.Combine(Application.StartupPath, "HotImage");
            string filePath = Path.Combine(dirPath, "background.png");

            if (File.Exists(filePath))
            {
                try
                {
                    Mat mt = Cv2.ImRead(filePath);
                    if (!mt.Empty())
                    {
                        SourceImage = mt;
                        DispWinHeatMap.Image = mt;
                        DispWinHeatMap.Invalidate();
                    }
                    else
                    {
                        MessageBox.Show("热力图自动加载背景图失败，图片为空，请手动加载。");
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"热力图自动加载背景图失败: {ex.Message}");
                    MessageBox.Show("热力图自动加载背景图失败，请手动加载。");
                }
            }
            else
            {
                MessageBox.Show("未找到可自动加载的背景图，请手动加载。");
            }
        }

        #endregion

        #region UI Event Handlers

        private async void btn_queryHeatPoint_Click(object sender, EventArgs e)
        {
            try
            {
#if TEST_ENV
                await ProcessSnListAndUpdateHeatMapAsync(new List<string> { "test_sn11", "test_sn22", "test_sn03" });
#else
                if (!txt_Lot.Text.IsNullOrEmpty())
                {
                    List<string> sn_list = GetSnListByLot(this.txt_Lot.Text.ToString());
                    await ProcessSnListAndUpdateHeatMapAsync(sn_list);
                }
                else if (timePicker.Checked)
                {
                    if (cmb_PartNumber.Items.Count == 0)
                    {
                        GetSnByPnTime(timePicker.Value);
                        cmb_PartNumber.Items.Clear();
                        foreach (var pn in dic_PN_SNList.Keys.Distinct())
                        {
                            cmb_PartNumber.Items.Add(pn);
                        }
                        if (cmb_PartNumber.Items.Count > 0)
                        {
                            cmb_PartNumber.SelectedIndex = 0;
                        }
                        MessageBox.Show($"已加载当天料号列表，请选择或输入一个料号后再次查询。");
                        return;
                    }

                    if (!string.IsNullOrEmpty(cmb_PartNumber.Text) && dic_PN_SNList.Count > 0)
                    {
                        if (dic_PN_SNList.TryGetValue(this.cmb_PartNumber.Text, out List<string> sn_list))
                        {
                            await ProcessSnListAndUpdateHeatMapAsync(sn_list);
                        }
                        else
                        {
                            MessageBox.Show($"未找到料号 {cmb_PartNumber.Text} 在该日期下的 SN 数据。");
                        }
                    }
                    else
                    {
                        MessageBox.Show("请先选择或输入一个料号。");
                    }
                }
                else
                {
                    MessageBox.Show("请输入Lot号，或勾选日期并选择一个料号。");
                }
#endif
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
            }
        }

        private async void btn_loadArryImage_Click(object sender, EventArgs e)
        {
            string path = SelectImageFile();
            if (!string.IsNullOrEmpty(path))
            {
                this.Enabled = false;
                try
                {
                    // 清空旧数据
                    dic_heatPints.Clear();
                    _heatMapManager.ClearHeatPoints();
                    this.Invoke(new Action(() =>
                    {
                        flowLayoutPanel_Defects.Controls.Clear();
                        flowLayoutPanel_Details.Controls.Clear();
                    }));

                    Mat mt = await Task.Run(() =>
                    {
                        Rect rect = new Rect();
                        // GetProductROI(path, ref rect);
                        offsetX = rect.X;
                        offsetY = rect.Y;
                        Mat originalMat = Cv2.ImRead(path);
                        Cv2.Resize(originalMat, originalMat, new OpenCvSharp.Size(originalMat.Width * 0.1, originalMat.Height * 0.1));
                        //return new Mat(originalMat, rect);
                        return originalMat;
                    });

                    SourceImage = mt;

                    // 更新 heatMapControl 的背景并清空热点
                    if (_heatMapManager.HeatMapControl != null)
                    {
                        _heatMapManager.SetBackgroundImage(SourceImage.ToBitmap());
                        _heatMapManager.ClearHeatPoints(); // 这会清除旧的热力图覆盖层
                    }

                    // 直接在显示控件中显示新的背景图
                    DispWinHeatMap.Image = mt;
                    DispWinHeatMap.Invalidate();

                    SaveBackgroundImage(mt);
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"加载Array图像失败: {ex.Message}");
                    MessageBox.Show("加载图像失败，请检查日志。");
                }
                finally
                {
                    this.Enabled = true;
                }
            }
        }

        private async void btn_setPanel_Click(object sender, EventArgs e)
        {
            try
            {
                if (SourceImage == null)
                {
                    MessageBox.Show("请先加载Array图像");
                    return;
                }
                int row = Convert.ToInt32(this.txt_Row.Text);
                int column = Convert.ToInt32(this.txt_Column.Text);

                using (var dialog = new ArrayConfigDialog(row, column))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _arrayConfig = dialog.GridData;
                        await UpdatePanelGrid(row, column);
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"制作Array图像失败: {ex.Message}");
                MessageBox.Show("制作Array图像失败，请检查日志。");
            }
        }

        private async void rbn_Front_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateHeatMapPointsAsync();
        }

        private async void DefectCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateHeatMapPointsAsync();
        }
        private void CvDisplay1_OnSelectionFinished(Rect selectionRect)
        {
            switch (_currentSelectionMode)
            {
                case SelectionMode.Select:
                    DisplayHeatPointDetailsInSelection(selectionRect);
                    //MessageBox.Show($"选定区域: X={selectionRect.X}, Y={selectionRect.Y}, Width={selectionRect.Width}, Height={selectionRect.Height}");
                    break;
                case SelectionMode.Clip:
                    CropImage(selectionRect);
                    break;
            }
            // 操作完成后重置模式
            _currentSelectionMode = SelectionMode.None;
            DispWinHeatMap.IsSelectionMode = false;
        }
        private void FrHome_OnCallBackFullShowPro(string station, int index, string m_station, string status, string ocr, Mat mat)
        {
            try
            {
                FrFullImage.Instance.cvDisplay1.stationName = "A";
                FrFullImage.Instance.cvDisplay1.stationIndex = 1;
                FrFullImage.Instance.LoadShow(mat.Clone());
                FrFullImage.Instance.cvDisplay1.DrawStation(m_station);
                FrFullImage.Instance.cvDisplay1.DrawStatus(status);
                FrFullImage.Instance.cvDisplay1.DrawOCR(ocr);
                FrFullImage.Instance.Show();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Core Logic

        private async Task ProcessSnListAndUpdateHeatMapAsync(List<string> sn_list)
        {
            if (sn_list.Count == 0) return;

            dic_heatPints.Clear();
            _heatMapManager.ClearHeatPoints();

#if TEST_ENV
            sn_list.ForEach(sn => GenerateMockHeatPoints(sn));
#else
            var stopwatch = Stopwatch.StartNew();

            // 1. 设置合理的并发限制，例如 64。这个值可以根据目标服务器的承受能力进行调整。
            int degreeOfParallelism = 20;
            var results = new ConcurrentBag<AVI_HeatPoints>();

            using (var semaphore = new SemaphoreSlim(degreeOfParallelism))
            {
                var tasks = new List<Task>();
                foreach (var sn in sn_list)
                {
                    await semaphore.WaitAsync(); // 等待一个可用的并发槽位

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            // 2. 并行查询 A/B 面，并将结果添加到线程安全的集合中
                            var taskA = queryHeatDataBySnSideAsync(sn, "A");
                            var taskB = queryHeatDataBySnSideAsync(sn, "B");
                            var heatPoints = await Task.WhenAll(taskA, taskB);

                            foreach (var point in heatPoints)
                            {
                                if (point != null)
                                {
                                    results.Add(point);
                                }
                            }
                        }
                        finally
                        {
                            semaphore.Release(); // 释放槽位
                        }
                    }));
                }
                await Task.WhenAll(tasks);
            }

            // 3. 将收集到的结果统一整理到字典中
            foreach (var result in results)
            {
                if (!dic_heatPints.ContainsKey(result.SN))
                {
                    dic_heatPints[result.SN] = new List<AVI_HeatPoints>();
                }
                dic_heatPints[result.SN].Add(result);
            }

            stopwatch.Stop();
            LogTextHelper.Info($"查询所有SN的热力点数据总耗时: {stopwatch.ElapsedMilliseconds} ms，并发度: {degreeOfParallelism}");
#endif


            int maxRow = 0;
            int maxCol = 0;
            foreach (var sn in sn_list)
            {
                if (TryParseSnPosition(sn, out int row, out int col))
                {
                    if (row > maxRow) maxRow = row;
                    if (col > maxCol) maxCol = col;
                }
            }
            this.txt_Row.Text = (maxRow + 1).ToString();
            this.txt_Column.Text = (maxCol + 1).ToString();
            await UpdatePanelGrid(maxRow + 1, maxCol + 1);

            UpdateDefectCheckboxes();
            await UpdateHeatMapPointsAsync();
        }
        private async Task UpdatePanelGrid(int rows, int columns)
        {
            if (SourceImage.Empty())
            {
                LogTextHelper.Error("无法读取源图像");
                return;
            }

            this.Enabled = false;
            try
            {
                Mat resultImage = await Task.Run(() =>
                {
                    Mat[,] matGrid = new Mat[rows, columns];
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            matGrid[i, j] = SourceImage.Clone();
                        }
                    }
                    return StitchImages(matGrid, rows, columns);
                });

                DispWinHeatMap.Image = resultImage;
                if (_heatMapManager.HeatMapControl != null)
                {
                    _heatMapManager.SetBackgroundImage(resultImage.ToBitmap());
                }
                await UpdateHeatMapPointsAsync();
                DispWinHeatMap.Invalidate();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"制作Array图像失败: {ex.Message}");
                throw;
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private async Task UpdateHeatMapPointsAsync()
        {
            if (DispWinHeatMap.Image == null)
            {
                MessageBox.Show("请先加载Array图像");
                return;
            }

            string sideFilter = rbn_Front.Checked ? "A" : "B";

            var selectedDefectNames = new List<string>();
            this.Invoke(new Action(() =>
            {
                selectedDefectNames = flowLayoutPanel_Defects.Controls.OfType<CheckBox>()
                    .Where(cb => cb.Checked)
                    .Select(cb => cb.Text)
                    .ToList();
            }));

            await _heatMapManager.UpdateHeatMapPointsAsync(
                dic_heatPints,
                sideFilter,
                selectedDefectNames,
                (sn, row, col) => TryParseSnPosition(sn, out row, out col),
                SourceImage,
                (mat) => DispWinHeatMap.Image = mat);

            DispWinHeatMap.Invalidate();
        }

        private void UpdateDefectCheckboxes()
        {
            var defectNames = dic_heatPints.Values
                .SelectMany(list => list)
                .SelectMany(points => points.pointsInfos)
                .Select(info => info.DefectName)
                .Distinct()
                .ToList();

            this.Invoke(new Action(() =>
            {
                flowLayoutPanel_Defects.Controls.Clear();

                if (defectNames.Any())
                {
                    // 添加“全选”复选框
                    var selectAllCheckBox = new CheckBox
                    {
                        Text = "全选",
                        Name = "chkSelectAll",
                        AutoSize = true,
                        ForeColor = Color.FromArgb(255, 255, 0), // 使用醒目的颜色
                        Checked = true
                    };
                    selectAllCheckBox.CheckedChanged += SelectAllCheckbox_CheckedChanged;
                    flowLayoutPanel_Defects.Controls.Add(selectAllCheckBox);
                }

                foreach (var name in defectNames)
                {
                    if (string.IsNullOrEmpty(name)) continue;

                    var checkBox = new CheckBox
                    {
                        Text = name,
                        AutoSize = true,
                        ForeColor = Color.FromArgb(216, 219, 188),
                        Checked = true
                    };
                    checkBox.CheckedChanged += DefectCheckbox_CheckedChanged;
                    flowLayoutPanel_Defects.Controls.Add(checkBox);
                }
            }));
        }

        private async void SelectAllCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var selectAllCheckBox = sender as CheckBox;
            if (selectAllCheckBox == null) return;

            // 避免在更新子项时重复触发事件
            flowLayoutPanel_Defects.Controls.OfType<CheckBox>()
                                 .Where(cb => cb != selectAllCheckBox)
                                 .ToList()
                                 .ForEach(cb => cb.CheckedChanged -= DefectCheckbox_CheckedChanged);

            foreach (var checkBox in flowLayoutPanel_Defects.Controls.OfType<CheckBox>())
            {
                if (checkBox != selectAllCheckBox)
                {
                    checkBox.Checked = selectAllCheckBox.Checked;
                }
            }

            // 重新订阅事件
            flowLayoutPanel_Defects.Controls.OfType<CheckBox>()
                                 .Where(cb => cb != selectAllCheckBox)
                                 .ToList()
                                 .ForEach(cb => cb.CheckedChanged += DefectCheckbox_CheckedChanged);

            await UpdateHeatMapPointsAsync();
        }

        #endregion

        #region Data Access

        private List<string> GetSnListByLot(string Lot)
        {
            var stopwatch = Stopwatch.StartNew();
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
            finally
            {
                stopwatch.Stop();
                LogTextHelper.Info($"GetSnListByLot(Lot: {Lot}) 耗时: {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private List<string> GetSnByPnTime(DateTime date)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                List<string> rtn_list = new List<string>();
                dic_PN_SNList.Clear();

                Machine.master.workClass.ReadPNSNByTime(date, out string outInfo);

                if (!string.IsNullOrEmpty(outInfo))
                {
                    var root = JObject.Parse(outInfo);
                    var dataList = root["data_list"] as JArray;

                    if (dataList == null) return new List<string>();

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
                return rtn_list.Distinct().ToList();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("GetSnByPnTime 异常: " + ex.ToString());
                return new List<string>();
            }
            finally
            {
                stopwatch.Stop();
                LogTextHelper.Info($"GetSnByPnTime(Date: {date:yyyy-MM-dd}) 耗时: {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private bool queryHeatDataBySn(string sn)
        {
            try
            {
                // 为了与新的并行模型兼容，此方法可以保持原样或标记为过时
                // 实际的查询逻辑已移至 queryHeatDataBySnSideAsync
                var taskA = Task.Run(() => QueryAndStoreHeatPoints(sn, "A"));
                var taskB = Task.Run(() => QueryAndStoreHeatPoints(sn, "B"));
                Task.WhenAll(taskA, taskB).Wait();
                return true;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"queryHeatDataBySn for {sn} 异常: {ex.ToString()}");
                return false;
            }
        }

        private async Task<AVI_HeatPoints> queryHeatDataBySnSideAsync(string sn, string side)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                RootDbInfo Info = new RootDbInfo
                {
                    db_name = "AVI_HeatPoints",
                    operation = "get",
                    op_mode = "last",
                    key = $"{sn}_{side}"
                };
                string Result=null;
                // 假设 HttpPostMethod 是同步的，我们在 Task.Run 中运行它
                await Task.Run(() => Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, Info, 1, out Result));

                if (string.IsNullOrEmpty(Result)) return null;

                var jsonStr = JObject.Parse(Result);
                string str = jsonStr["value"].ToString();

                if (!string.IsNullOrWhiteSpace(str) && !str.Contains("err_key_found"))
                {
                    return JsonConvert.DeserializeObject<AVI_HeatPoints>(str);
                }
                return null;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"queryHeatDataBySnSideAsync for {sn}_{side} 异常: {ex.Message}");
                return null;
            }
            finally
            {
                stopwatch.Stop();
                LogTextHelper.Info($"QueryAndStoreHeatPoints(SN: {sn}, Side: {side}) 耗时: {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void QueryAndStoreHeatPoints(string sn, string side)
        {
            var stopwatch = Stopwatch.StartNew();
            try
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
                        if (!dic_heatPints.ContainsKey(sn))
                        {
                            dic_heatPints[sn] = new List<AVI_HeatPoints>();
                        }
                        dic_heatPints[sn].Add(avi_HeatInfo);
                    }
                }
            }
            finally
            {
                stopwatch.Stop();
                LogTextHelper.Info($"QueryAndStoreHeatPoints(SN: {sn}, Side: {side}) 耗时: {stopwatch.ElapsedMilliseconds} ms");
            }
        }
        #endregion

        #region Image Processing & Utilities

        private bool TryParseSnPosition(string sn, out int row, out int col)
        {
            row = 0;
            col = 0;
            if (string.IsNullOrEmpty(sn) || sn.Length < 2)
            {
                return false;
            }

            if (rbn_Array.Checked)
            {
                if (_arrayConfig != null)
                {
                    string snSuffix = sn.Substring(sn.Length - 2);
                    for (int i = 0; i < _arrayConfig.GetLength(0); i++)
                    {
                        for (int j = 0; j < _arrayConfig.GetLength(1); j++)
                        {
                            if (_arrayConfig[i, j] == snSuffix)
                            {
                                row = i;
                                col = j;
                                return true;
                            }
                        }
                    }
                }
                // If we are in array mode but have no config, or SN not found, we can't position it.
                return false;
            }
            
            char rowChar = sn[sn.Length - 2];
            char colChar = sn[sn.Length - 1];

            if (int.TryParse(rowChar.ToString(), out row) && int.TryParse(colChar.ToString(), out col))
            {
                return true;
            }
            return false;
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
        private void SaveBackgroundImage(Mat mt)
        {
            string dirPath = Path.Combine(Application.StartupPath, "HotImage");
            Directory.CreateDirectory(dirPath);
            string filePath = Path.Combine(dirPath, "background.png");
            Cv2.ImWrite(filePath, mt);
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

        #endregion

        #region Test Environment
#if TEST_ENV
        private void GenerateMockHeatPoints(string sn)
        {
            var random = new Random(sn.GetHashCode());
            var defectTypes = new[] { "Scratch", "Dent", "Spot", "Contamination" };
            var pointsInfos = new List<PointsInfo>();

            if (!TryParseSnPosition(sn, out int row, out int col))
            {
                return;
            }

            for (int i = 0; i < mockPointsCount; i++)
            {
                pointsInfos.Add(new PointsInfo
                {
                    X = (int)(random.Next(0, SourceImage?.Width ?? 1000) / 0.1f),
                    Y = (int)(random.Next(0, SourceImage?.Height ?? 1000) / 0.1f),
                    DefectName = defectTypes[random.Next(defectTypes.Length)]
                });
            }

            var aviHeatPoints = new AVI_HeatPoints
            {
                SN = sn,
                Side = rbn_Front.Checked ? "A" : "B",
                pointsInfos = pointsInfos
            };

            if (!dic_heatPints.ContainsKey(sn))
            {
                dic_heatPints.TryAdd(sn, new List<AVI_HeatPoints>());
            }
            dic_heatPints[sn].Add(aviHeatPoints);
        }

        private async Task TestUpdateHeatMapPointsAsync()
        {
            var random = new Random();
            var points = new List<DeepSightHeatMap.HeatPoint>
            {
                new DeepSightHeatMap.HeatPoint(
                    location: new PointF(random.Next(100, 500) * 0.1f, random.Next(200, 600) * 0.1f),
                    intensity: 0.25f,
                    radius: 20),
                new DeepSightHeatMap.HeatPoint(
                    location: new PointF(random.Next(500, 1000) * 0.1f, random.Next(500, 1000) * 0.1f),
                    intensity: 0.25f,
                    radius: 20)
            };

            LogTextHelper.Info($"热力点位数：{points.Count}");

            await Task.Run(() =>
            {
                var sw = new Stopwatch();
                sw.Start();
                _heatMapManager.HeatMapControl.SetHeatPoints(points);
                if (_heatMapManager.HeatMapControl._heatMapOverlay != null)
                {
                    DispWinHeatMap.Image = BitmapConverter.ToMat(ImageHelper.CombineHeatMapWithBackground(
                        _heatMapManager.HeatMapControl.BackgroundImage,
                        _heatMapManager.HeatMapControl._heatMapOverlay));
                }
                sw.Stop();
                LogTextHelper.Info($"COST: {sw.ElapsedMilliseconds} ms");
            });

            DispWinHeatMap.Invalidate();
        }

        private void GenerateRandomHeatPoints()
        {
            var points = new List<DeepSightHeatMap.HeatPoint>();
            Random random = new Random();
            for (int i = 0; i < 100; i++)
            {
                var point = new DeepSightHeatMap.HeatPoint(
                    location: new PointF(
                    random.Next(_heatMapManager.HeatMapControl.Width),
                    random.Next(_heatMapManager.HeatMapControl.Height)
                    ),
                    intensity: (float)random.NextDouble(),
                    radius: 120
                );
                points.Add(point);
            }

            _heatMapManager.HeatMapControl.SetHeatPoints(points);
        }

        private void AddControlPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 150,
                BackColor = Color.LightGray
            };

            var lblOpacity = new Label { Text = "热力图透明度:", Location = new System.Drawing.Point(10, 10) };
            var trackOpacity = new TrackBar
            {
                Location = new System.Drawing.Point(10, 30),
                Width = 130,
                Minimum = 0,
                Maximum = 100,
                Value = (int)(_heatMapManager.HeatMapControl.HeatMapOpacity * 100)
            };
            trackOpacity.ValueChanged += (s, e) =>
            {
                _heatMapManager.HeatMapControl.HeatMapOpacity = trackOpacity.Value / 100f;
            };

            panel.Controls.AddRange(new Control[] { lblOpacity, trackOpacity });
            this.Controls.Add(panel);
        }
#endif
        #endregion
        #region 框选功能
        private enum SelectionMode
        {
            None,
            Select,
            Clip
        }
        private SelectionMode _currentSelectionMode = SelectionMode.None;
        private void btn_Select_Click(object sender, EventArgs e)
        {
            _currentSelectionMode = SelectionMode.Select;
            DispWinHeatMap.IsSelectionMode = true;

        }

        private void btnClip_Click(object sender, EventArgs e)
        {
            _currentSelectionMode = SelectionMode.Clip;
            DispWinHeatMap.IsSelectionMode = true;
        }


        private void CropImage(Rect selectionRect)
        {
            if (DispWinHeatMap.Image == null || DispWinHeatMap.Image.Empty())
            {
                MessageBox.Show("没有可供裁剪的图像。");
                return;
            }

            try
            {
                // 裁剪图像
                Mat croppedImage = new Mat(DispWinHeatMap.Image, selectionRect);
                offsetX = selectionRect.X;
                offsetY = selectionRect.Y;
                // 更新显示
                DispWinHeatMap.Image = croppedImage;
                SourceImage = croppedImage;
                DispWinHeatMap.Invalidate();
                SaveBackgroundImage(croppedImage);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"裁剪图像时出错: {ex.Message}");
                MessageBox.Show("裁剪图像时出错，请检查日志。");
            }
        }
        private async void DisplayHeatPointDetailsInSelection(Rect selectionRect)
        {
            this.Enabled = false;
            flowLayoutPanel_Details.Controls.Clear();
            _pointsInSelection.Clear();
            _loadedDetailsCount = 0;

            var loadingLabel = new Label { Text = "正在加载详细信息...", AutoSize = true, ForeColor = Color.White, Margin = new Padding(10) };
            flowLayoutPanel_Details.Controls.Add(loadingLabel);

            try
            {
                // 异步查询所有符合条件的点
                _pointsInSelection = await Task.Run(() =>
                {
                    string sideFilter = rbn_Front.Checked ? "A" : "B";
                    var selectedDefectNames = new HashSet<string>(
                        flowLayoutPanel_Defects.Controls.OfType<CheckBox>()
                                                .Where(cb => cb.Checked)
                                                .Select(cb => cb.Text)
                    );

                    return dic_heatPints
                        .AsParallel()
                        .SelectMany(kvp =>
                        {
                            var sn = kvp.Key;
                            if (!TryParseSnPosition(sn, out int row, out int col)) return Enumerable.Empty<dynamic>();

                            float productWidth = SourceImage?.Width ?? 0;
                            float productHeight = SourceImage?.Height ?? 0;
                            float colOffset = col * productWidth;
                            float rowOffset = row * productHeight;

                            return kvp.Value
                                .Where(p => p.Side == sideFilter && p?.pointsInfos != null)
                                .SelectMany(avi_points => avi_points.pointsInfos
                                    .Where(p => selectedDefectNames.Contains(p.DefectName))
                                    .Select(pointInfo => new
                                    {
                                        SN = sn,
                                        PointInfo = pointInfo,
                                        DisplayLocation = new PointF(
                                            (pointInfo.X * 0.1f - offsetX) + colOffset,
                                            (pointInfo.Y * 0.1f - offsetY) + rowOffset
                                        )
                                    }));
                        })
                        .Where(p => selectionRect.Contains((int)p.DisplayLocation.X, (int)p.DisplayLocation.Y))
                        .ToList<dynamic>();
                });

                flowLayoutPanel_Details.Controls.Remove(loadingLabel);

                if (_pointsInSelection.Count == 0)
                {
                    MessageBox.Show("选定区域内没有找到符合条件的热力点。");
                    return;
                }

                // 加载第一页
                LoadMoreDetails();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"显示热力点详情时出错: {ex.Message}");
                MessageBox.Show("加载详情时出错，请检查日志。");
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private async void LoadMoreDetails_Click(object sender, EventArgs e)
        {
            if (_loadMoreButton != null) _loadMoreButton.Enabled = false;
            await Task.Delay(100); // 短暂延迟以允许UI更新
            LoadMoreDetails();
            if (_loadMoreButton != null) _loadMoreButton.Enabled = true;
        }

        private async void LoadMoreDetails()
        {
            //this.Enabled = false;

            if (_loadMoreButton != null && flowLayoutPanel_Details.Controls.Contains(_loadMoreButton))
            {
                flowLayoutPanel_Details.Controls.Remove(_loadMoreButton);
            }

            int pointsToLoad = Math.Min(PageSize, _pointsInSelection.Count - _loadedDetailsCount);
            if (pointsToLoad <= 0)
            {
               // this.Enabled = true;
                return;
            }

            var itemsToLoad = _pointsInSelection.GetRange(_loadedDetailsCount, pointsToLoad);

            // Load image data in the background
            var imageData = await Task.Run(() =>
            {
                var data = new List<Tuple<dynamic, byte[]>>();
                foreach (var p in itemsToLoad)
                {
                    byte[] imageBytes = null;
                    if (!string.IsNullOrEmpty(p.PointInfo.ImagePath) && File.Exists(p.PointInfo.ImagePath))
                    {
                        try
                        {
                            imageBytes = File.ReadAllBytes(p.PointInfo.ImagePath);
                        }
                        catch (Exception ex)
                        {
                            LogTextHelper.Error($"加载图片失败 {p.PointInfo.ImagePath}: {ex.Message}");
                        }
                    }
                    data.Add(Tuple.Create((object)p, imageBytes));
                }
                return data;
            });

            // Create UI controls on the UI thread
            var panels = new List<Control>();
            foreach (var item in imageData)
            {
                var p = item.Item1;
                var imageBytes = item.Item2;

                var panel = new TableLayoutPanel
                {
                    ColumnCount = 2,
                    RowCount = 1,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Top,
                    Margin = new Padding(3),
                    Width = flowLayoutPanel_Details.ClientSize.Width - 25
                };
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F));
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

                var pictureBox = new PictureBox
                {
                    Size = new System.Drawing.Size(400, 400),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Margin = new Padding(3),
                    BackColor = Color.FromArgb(45, 45, 48),
                    Dock = DockStyle.Fill
                };

                if (imageBytes != null)
                {
                    using (var ms = new System.IO.MemoryStream(imageBytes))
                    {
                        pictureBox.Image = new Bitmap(ms);
                    }
                }
                else
                {
                    pictureBox.Image = pictureBox.ErrorImage;
                }
                panel.Controls.Add(pictureBox, 0, 0);

                var label = new Label
                {
                    Text = $"SN: {p.SN}\n缺陷: {p.PointInfo.DefectName}\n坐标: ({p.PointInfo.X}, {p.PointInfo.Y})",
                    AutoSize = true,
                    ForeColor = Color.White,
                    Margin = new Padding(10, 5, 5, 5),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                panel.Controls.Add(label, 1, 0);
                panels.Add(panel);
            }

            flowLayoutPanel_Details.Controls.AddRange(panels.ToArray());
            _loadedDetailsCount += pointsToLoad;

            // Update "Load More" button
            int remaining = _pointsInSelection.Count - _loadedDetailsCount;
            if (remaining > 0)
            {
                _loadMoreButton = new Button
                {
                    Text = $"加载更多 ({remaining} 个剩余)",
                    AutoSize = true,
                    Margin = new Padding(10),
                    FlatStyle = FlatStyle.System
                };
                _loadMoreButton.Click += LoadMoreDetails_Click;
                flowLayoutPanel_Details.Controls.Add(_loadMoreButton);
                flowLayoutPanel_Details.ScrollControlIntoView(_loadMoreButton);
            }
            else if (panels.Any())
            {
                flowLayoutPanel_Details.ScrollControlIntoView(panels.Last());
            }

           // this.Enabled = true;
        }
        #endregion

        #region Test Environment
        #endregion
    }
}
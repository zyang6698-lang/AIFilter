//#define TEST_ENV
using DeepSightDB;
using DeepSightModel;
using DeepSightDisplay;
using DeepSightDisplay.HeatMap;
using DeepSightTool;
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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class UcHeatMap : UserControl
    {
        #region Fields and Properties

        private readonly ConcurrentDictionary<string, List<DetectInfo>> _heatPoints = new ConcurrentDictionary<string, List<DetectInfo>>();
        private readonly HeatMapManager _heatMapManager = new HeatMapManager();
        private Mat SourceImage = null;

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
        /// <summary>
        /// 当前加载的模板图像路径，用于裁剪后保存
        /// </summary>
        private string _currentTemplateImagePath = null;

        #endregion

        #region Initialization

        /// <summary>
        /// 外部注入的缺陷查询控件（与 UcAiReview 共用，避免重复 UI 与重复查询）
        /// </summary>
        private UcDefectQuery queryControl;

        public UcHeatMap()
        {
            InitializeComponent();
            InitializeLayout();
            this.VisibleChanged += HeatMapControl2_VisibleChanged;
        }

        /// <summary>
        /// 绑定外部查询控件（由父容器在创建后调用）
        /// </summary>
        public void BindQuerySource(UcDefectQuery query)
        {
            if (query == null) return;
            if (queryControl != null)
            {
                queryControl.QueryClicked -= HeatMapQueryControl_QueryClicked;
                queryControl.FilterChanged -= QueryControl_FilterChanged;
            }
            queryControl = query;
            queryControl.QueryClicked += HeatMapQueryControl_QueryClicked;
            queryControl.FilterChanged += QueryControl_FilterChanged;
        }

        private void HeatMapControl2_VisibleChanged(object sender, EventArgs e)
        {
            // 确保只在控件变为可见时初始化，并且只初始化一次
            if (this.Visible)
            {
                // 不再自动加载背景图，改为根据查询条件动态加载模板图像
                InitializeHeatMap();

                // 取消订阅，避免重复初始化
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
            table_HeatMap.Controls.Add(DispWinHeatMap, 0, 0);
        }


        private void InitializeHeatMap()
        {
            _heatMapManager.InitializeHeatMap(SourceImage);
        }

        /// <summary>
        /// 根据料号、机台号和正反面加载模板图像
        /// 路径格式: TemplateImages/{料号}/{机台号}/{图片文件}
        /// 文件名末尾为A表示正面，为B表示反面
        /// </summary>
        /// <param name="partNumber">料号</param>
        /// <param name="machineId">机台号</param>
        /// <param name="side">正反面 (A=正面, B=反面, 空=全部)</param>
        /// <returns>是否成功加载</returns>
        private bool LoadTemplateImage(string partNumber, string machineId, string side)
        {
            if (string.IsNullOrEmpty(partNumber))
            {
                return false;
            }

            try
            {
                // 构建模板图像目录路径（使用Application.StartupPath的上一级目录）
                string parentDir = Directory.GetParent(Application.StartupPath)?.FullName ?? Application.StartupPath;
                string templateDir = Path.Combine(parentDir, "TemplateImages", partNumber);

                // 如果指定了机台号，则进入机台号子目录
                if (!string.IsNullOrEmpty(machineId))
                {
                    templateDir = Path.Combine(templateDir, machineId);
                }

                if (!Directory.Exists(templateDir))
                {
                    LogTextHelper.Warn($"模板图像目录不存在: {templateDir}");
                    return false;
                }

                // 获取目录下的所有图片文件
                var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".tiff" };
                var imageFiles = Directory.GetFiles(templateDir)
                    .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToList();

                if (imageFiles.Count == 0)
                {
                    LogTextHelper.Warn($"模板图像目录中没有图片文件: {templateDir}");
                    return false;
                }

                // 根据正反面筛选图片
                string targetFile = null;
                if (!string.IsNullOrEmpty(side))
                {
                    // 优先查找裁剪后的版本（文件名格式：xxx{side}_cropped_offsetX_offsetY）
                    targetFile = imageFiles.FirstOrDefault(f =>
                    {
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(f);
                        return nameWithoutExt.Contains("_cropped_") && nameWithoutExt.Contains(side);
                    });

                    // 如果没有裁剪版本，查找原始版本
                    if (string.IsNullOrEmpty(targetFile))
                    {
                        targetFile = imageFiles.FirstOrDefault(f =>
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(f);
                            return nameWithoutExt.EndsWith(side, StringComparison.OrdinalIgnoreCase)
                                   && !nameWithoutExt.Contains("_cropped_");
                        });
                    }
                }

                // 如果没有找到指定面的图片，或者未指定面，则使用第一张图片
                if (string.IsNullOrEmpty(targetFile))
                {
                    // 优先选择A面裁剪版本
                    targetFile = imageFiles.FirstOrDefault(f =>
                    {
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(f);
                        return nameWithoutExt.Contains("_cropped_") && nameWithoutExt.Contains("A");
                    });

                    // 其次选择A面原始版本
                    if (string.IsNullOrEmpty(targetFile))
                    {
                        targetFile = imageFiles.FirstOrDefault(f =>
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(f);
                            return nameWithoutExt.EndsWith("A", StringComparison.OrdinalIgnoreCase)
                                   && !nameWithoutExt.Contains("_cropped_");
                        }) ?? imageFiles.First();
                    }
                }

                // 从文件名解析offset信息（如果是裁剪版本）
                ParseOffsetFromFileName(targetFile);

                // 加载图片
                Mat mt = Cv2.ImRead(targetFile);
                if (!mt.Empty())
                {
                    SourceImage = mt;
                    DispWinHeatMap.Image = mt;

                    // 记录当前模板图路径，用于裁剪后保存
                    _currentTemplateImagePath = targetFile;

                    // 更新热力图管理器的背景
                    if (_heatMapManager.HeatMapControl != null)
                    {
                        _heatMapManager.SetBackgroundImage(mt.ToBitmap());
                        SourceImage = mt;
                    }

                    DispWinHeatMap.Invalidate();
                    LogTextHelper.Info($"成功加载模板图像: {targetFile}");
                    return true;
                }
                else
                {
                    LogTextHelper.Warn($"模板图像加载失败（图片为空）: {targetFile}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"加载模板图像异常: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region UI Event Handlers

        private async void HeatMapQueryControl_QueryClicked(object sender, EventArgs e)
        {
            await btn_queryHeatPoint_Click(sender, e);
        }

        private async void HeatMapQueryControl_SideSelectionChanged(object sender, EventArgs e)
        {
            await UpdateHeatMapPointsAsync();
        }

        /// <summary>
        /// 筛选条件变化事件处理（料号或机台号选择变化时自动筛选）
        /// </summary>
        private async void QueryControl_FilterChanged(object sender, EventArgs e)
        {
            await RefreshForCurrentQueryAsync();
        }

        /// <summary>
        /// 根据 queryControl 当前 QueryResult 重建热力图。
        /// 供父窗体在两段式加载第二阶段（cmb_Lot 选中 Lot 后加载明细）显式触发。
        /// </summary>
        public async Task RefreshForCurrentQueryAsync()
        {
            if (queryControl == null) return;

            // 根据筛选条件重新加载热力图数据
            _heatPoints.Clear();
            _heatMapManager.ClearHeatPoints();

            // 尝试根据料号、机台号和正反面加载模板图像
            string partNumber = queryControl.PartNumber;
            string machineId = queryControl.MachineID;
            string side = queryControl.SelectedSide;

            if (!string.IsNullOrEmpty(partNumber))
            {
                LoadTemplateImage(partNumber, machineId, side);
                InitializeHeatMap();

            }

            foreach (var res in queryControl.GetQueryResult())
            {
                if (!_heatPoints.ContainsKey(res.SerialNumber))
                {
                    // 汇总所有面的检测点（QueryControl已根据选择的面进行过滤）
                    var allDetectPoints = res.Sides?
                        .Where(s => s.DetectPoints != null)
                        .SelectMany(s => s.DetectPoints)
                        .ToList() ?? new List<DetectInfo>();

                    if (allDetectPoints.Count > 0)
                    {
                        _heatPoints[res.SerialNumber] = allDetectPoints;
                    }
                }
            }

            UpdateDefectCheckboxes();
            await UpdateHeatMapPointsAsync();
        }

        private async Task btn_queryHeatPoint_Click(object sender, EventArgs e)
        {
            // 点击查询：只获取数据并填充料号列表，不加载图片
            // 图片加载和热力图更新在选中料号后触发（FilterChanged事件）

            _heatPoints.Clear();
            _heatMapManager.ClearHeatPoints();

            // 清空缺陷复选框
            this.Invoke(new Action(() =>
            {
                flowLayoutPanel_Defects.Controls.Clear();
            }));

#if TEST_ENV
            List<string> sn_list = new List<string> { "test_sn11", "test_sn22", "test_sn03" };
            sn_list.ForEach(sn => GenerateMockHeatPoints(sn));
            UpdateDefectCheckboxes();
#else
            // 查询完成后，QueryControl会自动填充料号列表
            // 用户选择料号后会触发 FilterChanged 事件，届时再加载图片和热力图
            LogTextHelper.Info($"查询完成，共获取 {queryControl.GetQueryResult()?.Count ?? 0} 条记录，请选择料号");
#endif
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
                    _heatPoints.Clear();
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

     

        private async void rbn_Front_CheckedChanged(object sender, EventArgs e)
        {
            // This method is now handled by HeatMapQueryControl_SideSelectionChanged
            // Keep it for backward compatibility if directly called
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

        #endregion

        #region Core Logic

        private async Task UpdatePanelGrid(int rows, int columns)
        {
            if (SourceImage == null || SourceImage.Empty())
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
            // 如果没有背景图，跳过热力图更新（不再弹出提示）
            if (DispWinHeatMap.Image == null || SourceImage == null)
            {
                LogTextHelper.Warn("热力图更新跳过：未加载背景图像");
                return;
            }

            var selectedDefectNames = new List<string>();
            this.Invoke(new Action(() =>
            {
                selectedDefectNames = flowLayoutPanel_Defects.Controls.OfType<CheckBox>()
                    .Where(cb => cb.Checked)
                    .Select(cb => cb.Text)
                    .ToList();
            }));

            await _heatMapManager.UpdateHeatMapPointsAsync(
                _heatPoints,
                selectedDefectNames,
                (sn) =>
                {
                    bool success = TryParseSnPosition(sn, out int row, out int col);
                    return new HeatMapManager.SnPositionResult { Success = success, Row = row, Col = col };
                },
                SourceImage,
                (mat) => DispWinHeatMap.Image = mat);

            DispWinHeatMap.Invalidate();
        }

        private void UpdateDefectCheckboxes()
        {
            var defectNames = _heatPoints.Values
                .SelectMany(list => list)
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

        #region Image Processing & Utilities

        private bool TryParseSnPosition(string sn, out int row, out int col)
        {
            row = 0;
            col = 0;
            if (string.IsNullOrEmpty(sn) || sn.Length < 2)
            {
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
                Side = heatMapQueryControl.SelectedSide,
                pointsInfos = pointsInfos
            };

            if (!_heatPoints.ContainsKey(sn))
            {
                _heatPoints.TryAdd(sn, new List<AVI_HeatPoints>());
            }
            _heatPoints[sn].Add(aviHeatPoints);
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

                // 保存裁剪后的图像到模板图目录，用于下次自动加载
                SaveCroppedTemplateImage(croppedImage);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"裁剪图像时出错: {ex.Message}");
                MessageBox.Show("裁剪图像时出错，请检查日志。");
            }
        }

        /// <summary>
        /// 从文件名解析offset信息
        /// 文件名格式：xxx_cropped_offsetX_offsetY.扩展名
        /// </summary>
        /// <param name="filePath">文件路径</param>
        private void ParseOffsetFromFileName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            string nameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

            // 检查是否包含 _cropped_ 标记
            int croppedIndex = nameWithoutExt.IndexOf("_cropped_", StringComparison.OrdinalIgnoreCase);
            if (croppedIndex < 0)
            {
                // 不是裁剪版本，重置offset
                offsetX = 0;
                offsetY = 0;
                return;
            }

            try
            {
                // 提取 _cropped_ 后面的部分：offsetX_offsetY
                string offsetPart = nameWithoutExt.Substring(croppedIndex + "_cropped_".Length);
                string[] parts = offsetPart.Split('_');

                if (parts.Length >= 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                {
                    offsetX = x;
                    offsetY = y;
                    LogTextHelper.Info($"从文件名解析offset: X={offsetX}, Y={offsetY}");
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Warn($"解析offset失败: {ex.Message}");
                offsetX = 0;
                offsetY = 0;
            }
        }

        /// <summary>
        /// 保存裁剪后的图像到模板图目录（保留原图，另存一份裁剪版本）
        /// 文件名格式：原文件名_cropped_offsetX_offsetY.扩展名
        /// </summary>
        /// <param name="croppedImage">裁剪后的图像</param>
        private void SaveCroppedTemplateImage(Mat croppedImage)
        {
            if (string.IsNullOrEmpty(_currentTemplateImagePath))
            {
                LogTextHelper.Warn("未记录模板图路径，无法保存裁剪后的模板图");
                return;
            }

            try
            {
                // 生成裁剪后的文件路径：原文件名_cropped_offsetX_offsetY.扩展名
                string directory = Path.GetDirectoryName(_currentTemplateImagePath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(_currentTemplateImagePath);
                string extension = Path.GetExtension(_currentTemplateImagePath);
                string croppedFilePath = Path.Combine(directory, $"{fileNameWithoutExt}_cropped_{offsetX}_{offsetY}{extension}");

                // 保存裁剪后的图像
                Cv2.ImWrite(croppedFilePath, croppedImage);
                LogTextHelper.Info($"裁剪后的模板图已保存: {croppedFilePath}");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"保存裁剪后的模板图失败: {ex.Message}");
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
                    string sideFilter = queryControl.SelectedSide;
                    var selectedDefectNames = new HashSet<string>(
                        flowLayoutPanel_Defects.Controls.OfType<CheckBox>()
                                                .Where(cb => cb.Checked)
                                                .Select(cb => cb.Text)
                    );

                    return _heatPoints
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
                                    .Where(p => selectedDefectNames.Contains(p.DefectName))
                                    .Select(pointInfo => new
                                    {
                                        SN = sn,
                                        PointInfo = pointInfo,
                                        DisplayLocation = new PointF(
                                            (pointInfo.OriginRoiX  - offsetX) + colOffset,
                                            (pointInfo.OriginRoiY  - offsetY) + rowOffset
                                        )
                                    });
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
                    if (!string.IsNullOrEmpty(p.PointInfo.ImagePath))
                    {
                        try
                        {
                            // 通过Minio读取图片，参考LoadMinioImage方法
                            var parts = p.PointInfo.ImagePath.Split(':');
                            if (parts.Length >= 2)
                            {
                                string ip = parts[0];
                                string objectKey = parts[1];
                                using (var stream = Machine.master.MinioService.GetImageStreamSync("deepiresults", objectKey, ip))
                                {
                                    if (stream != null && stream.Length > 0)
                                    {
                                        imageBytes = stream.ToArray();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogTextHelper.Error($"从Minio加载图片失败 {p.PointInfo.ImagePath}: {ex.Message}");
                        }
                    }
                    data.Add(Tuple.Create((object)p, imageBytes));
                }
                return data;
            });

            // Create UI controls on the UI thread
            // 根据右侧面板宽度计算图片尺寸，适配不同分辨率
            int availableWidth = flowLayoutPanel_Details.ClientSize.Width - 30;
            int imageSize = Math.Max(80, availableWidth);

            var panels = new List<Control>();
            foreach (var item in imageData)
            {
                var p = item.Item1;
                var imageBytes = item.Item2;

                var panel = new TableLayoutPanel
                {
                    ColumnCount = 1,
                    RowCount = 2,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Top,
                    Margin = new Padding(3),
                    Width = availableWidth
                };
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, imageSize));
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var pictureBox = new PictureBox
                {
                    Size = new System.Drawing.Size(imageSize, imageSize),
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
                    Text = $"SN: {p.SN}\n缺陷: {p.PointInfo.DefectName}\n坐标: ({p.PointInfo.RoiX}, {p.PointInfo.RoiY})",
                    AutoSize = true,
                    ForeColor = Color.White,
                    Margin = new Padding(5, 3, 3, 3),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.TopLeft
                };
                panel.Controls.Add(label, 0, 1);
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
    }
}
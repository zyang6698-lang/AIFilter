using DeepSightModel;
using DeepSightTool;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using DeepSightDB;

namespace DeepSightAI
{
    public partial class DefectDetailControl : UserControl
    {
        private int _selectedIndex = -1;
        private List<DetectInfo> _allHeatPoints;
        private List<DetectInfo> _filteredHeatPoints; // For filtered data
        private int _currentPage = 1;
        private const int PageSize = 5; //
        private int _totalPages;
        private string _aiFilter = "All";
        private string _vvsFilter = "All";

        // 存储原始的DefectReviewItem列表，用于按SN分组检查VVS状态
        private List<DefectReviewItem> _sourceItems;
        // 记录已经触发过完成事件的SN（避免重复触发）
        private HashSet<string> _completedSnSet = new HashSet<string>();

        /// <summary>
        /// 当需要切换到下一行记录时触发（按Tab键时）
        /// </summary>
        public event EventHandler SelectNextRowRequested;

        /// <summary>
        /// 当某个SN的所有缺陷点VVS状态都已设置时触发
        /// </summary>
        public event EventHandler<VvsCompletedEventArgs> SnVvsCompleted;

        /// <summary>
        /// 当VVS状态改变时触发
        /// </summary>
        public event EventHandler VvsStatusChanged;

        public DefectDetailControl()
        {
            InitializeComponent();
            InitializeFilterControls();
            InitializePaginationControls();
        }

        private void InitializeFilterControls()
        {
            // AI Filter
            this.comboBox_FilterAI.Items.AddRange(new object[] { "All", "AI_OK", "AI_NG" });
            this.comboBox_FilterAI.SelectedIndex = 0;
            this.comboBox_FilterAI.SelectedIndexChanged += (s, e) =>
            {
                _aiFilter = this.comboBox_FilterAI.SelectedItem.ToString();
                ApplyFiltersAndReload();
            };

            // VVS Filter
            this.comboBox_FilterVVS.Items.AddRange(new object[] { "All", "VVS_OK", "VVS_NG", "NotSet" });
            this.comboBox_FilterVVS.SelectedIndex = 0;
            this.comboBox_FilterVVS.SelectedIndexChanged += (s, e) =>
            {
                _vvsFilter = this.comboBox_FilterVVS.SelectedItem.ToString();
                ApplyFiltersAndReload();
            };
        }

        private void ApplyFiltersAndReload()
        {
            _filteredHeatPoints = _allHeatPoints;

            if (_aiFilter != "All")
            {
                // AIStatus: 0 未运行 / 1 OK / 2 NG / 3 异常
                int targetAiStatus = _aiFilter == "AI_OK" ? 1 : 2;
                _filteredHeatPoints = _filteredHeatPoints.Where(p => p.AIStatus == targetAiStatus).ToList();
            }

            if (_vvsFilter != "All")
            {
                if (_vvsFilter == "NotSet")
                {
                    // VVSStatus: 0 未运行
                    _filteredHeatPoints = _filteredHeatPoints.Where(p => p.VVSStatus == 0).ToList();
                }
                else
                {
                    // VVSStatus: 1 OK / 2 NG
                    int targetVvsStatus = _vvsFilter == "VVS_OK" ? 1 : 2;
                    _filteredHeatPoints = _filteredHeatPoints.Where(p => p.VVSStatus == targetVvsStatus).ToList();
                }
            }

            _totalPages = (int)Math.Ceiling((double)_filteredHeatPoints.Count / PageSize);
            _currentPage = 1;
            LoadDefectsPage(_currentPage);
        }

        private void InitializePaginationControls()
        {
            // 控件已在 Designer 中创建，此处仅绑定事件和初始设置
            this.btnPrevPage.Click += (s, e) =>
            {
                if (_currentPage > 1)
                {
                    _currentPage--;
                    LoadDefectsPage(_currentPage);
                }
            };

            this.btnNextPage.Click += (s, e) =>
            {
                if (_currentPage < _totalPages)
                {
                    _currentPage++;
                    LoadDefectsPage(_currentPage);
                }
            };

            this.flowLayoutPanel_DefectImages.MouseWheel += FlowLayoutPanel_DefectImages_MouseWheel;
        }

        private void FlowLayoutPanel_DefectImages_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0) // Scroll down
            {
                if (_currentPage < _totalPages)
                {
                    _currentPage++;
                    LoadDefectsPage(_currentPage);
                }
            }
            else // Scroll up
            {
                if (_currentPage > 1)
                {
                    _currentPage--;
                    LoadDefectsPage(_currentPage);
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.D1 || keyData == Keys.NumPad1)
            {
                TagImage("VVS_OK", false);
                SelectNextImage();
                return true;
            }
            else if (keyData == Keys.D2 || keyData == Keys.NumPad2)
            {
                TagImage("VVS_NG", false);
                SelectNextImage();
                return true;
            }
            else if (keyData == Keys.D3 || keyData == Keys.NumPad3)
            {
                // 按键3：设置为未设置状态
                TagImage("VVS_NotSet", false);
                SelectNextImage();
                return true;
            }
            else if (keyData == Keys.Tab)
            {
                // 触发事件，通知父控件切换到下一行
                SelectNextRowRequested?.Invoke(this, EventArgs.Empty);
                return true;
            }
            else if (keyData == Keys.Down)
            {
                // 下键：下一张图片
                SelectNextImage();
                return true;
            }
            else if (keyData == Keys.Up)
            {
                // 上键：上一张图片
                SelectPreviousImage();
                return true;
            }
            else if (keyData == Keys.Right)
            {
                // 右键：下一页
                if (_currentPage < _totalPages)
                {
                    _currentPage++;
                    LoadDefectsPage(_currentPage);
                }
                return true;
            }
            else if (keyData == Keys.Left)
            {
                // 左键：上一页
                if (_currentPage > 1)
                {
                    _currentPage--;
                    LoadDefectsPage(_currentPage);
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void TagImage(string tag, bool isAiTag)
        {
            if (_selectedIndex < 0 || _selectedIndex >= flowLayoutPanel_DefectImages.Controls.Count)
                return;

            var control = flowLayoutPanel_DefectImages.Controls[_selectedIndex];

            if (control is Panel panel)
            {
                var imageContainer = panel.Controls.OfType<Panel>().FirstOrDefault();
                if (imageContainer == null) return;

                var topPictureBox = imageContainer.Controls.OfType<PictureBox>().FirstOrDefault();
                if (topPictureBox != null)
                {
                    var label = topPictureBox.Controls.OfType<Label>().FirstOrDefault();
                    if (label != null)
                    {
                        var heatPoint = label.Tag as DetectInfo;
                        if (heatPoint != null)
                        {
                            if (isAiTag)
                            {
                                // AI Status is not editable
                            }
                            else
                            {
                                // VVSStatus: 0 未设置 / 1 OK / 2 NG
                                if (tag == "VVS_OK")
                                    heatPoint.VVSStatus = 1;
                                else if (tag == "VVS_NG")
                                    heatPoint.VVSStatus = 2;
                                else if (tag == "VVS_NotSet")
                                    heatPoint.VVSStatus = 0;

                                // 检查是否所有缺陷点都已完成VVS复判
                                CheckAllVvsStatusSet();
                                
                                // 触发VVS状态改变事件，用于更新左下角复判详情
                                VvsStatusChanged?.Invoke(this, EventArgs.Empty);
                            }

                            label.Text = $"AI: {GetStatusText(heatPoint.AIStatus)}\n" +
                                         $"VVS: {GetStatusText(heatPoint.VVSStatus)}";

                            // Update border color after tagging
                            UpdatePanelAppearance(panel, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检查每个SN的所有缺陷点是否都已完成VVS复判，如果是则触发事件
        /// </summary>
        private void CheckAllVvsStatusSet()
        {
            if (_sourceItems == null || _sourceItems.Count == 0)
                return;

            // 遍历每个原始item（每个SN），检查是否所有缺陷点都已复判
            foreach (var item in _sourceItems)
            {
                if (item.HeatPoints == null || item.HeatPoints.Count == 0)
                    continue;

                // 生成唯一标识（SN + Side）
                string snKey = $"{item.SerialNumber}_{item.Side}";

                // 如果已经触发过，跳过
                if (_completedSnSet.Contains(snKey))
                    continue;

                // 检查该SN的所有缺陷点是否都已设置VVS状态
                bool allSet = item.HeatPoints.All(hp => hp.VVSStatus != 0);
                if (allSet)
                {
                    // 标记为已完成，避免重复触发
                    _completedSnSet.Add(snKey);

                    // 计算结果
                    bool allOk = item.HeatPoints.All(hp => hp.VVSStatus == 1);
                    int ngCount = item.HeatPoints.Count(hp => hp.VVSStatus == 2);

                    // 触发事件，传递该SN的HeatPoints
                    SnVvsCompleted?.Invoke(this, new VvsCompletedEventArgs
                    {
                        HeatPoints = item.HeatPoints,
                        AllOk = allOk,
                        NgCount = ngCount
                    });
                }
            }
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

        public void DisplayDefectDetails(DefectReviewItem item)
        {
            // 单个item显示，包装成列表调用
            DisplayDefectDetails(new List<DefectReviewItem> { item }, $" SN: {item.SerialNumber} ({item.Side})");
        }

        /// <summary>
        /// 显示多个DefectReviewItem的缺陷详情（用于Lot模式下查看多个SN）
        /// </summary>
        public void DisplayDefectDetails(List<DefectReviewItem> items, string title)
        {
            // 保存原始items列表，用于按SN分组检查VVS状态
            _sourceItems = items;
            _completedSnSet.Clear();

            // 合并所有HeatPoints
            _allHeatPoints = new List<DetectInfo>();
            foreach (var item in items)
            {
                if (item.HeatPoints != null)
                {
                    _allHeatPoints.AddRange(item.HeatPoints);
                }
            }

            // 【测试用】为没有缺陷名称的HeatPoints填充虚拟数据
            //FillTestDefectInfo(_allHeatPoints);

            _filteredHeatPoints = new List<DetectInfo>(_allHeatPoints);
            _totalPages = (int)Math.Ceiling((double)_filteredHeatPoints.Count / PageSize);
            _currentPage = 1;

            label_DetailTitle.Text = title;

            // Reset filters
            _aiFilter = "All";
            _vvsFilter = "All";
            comboBox_FilterAI.SelectedIndex = 0;
            comboBox_FilterVVS.SelectedIndex = 0;

            LoadDefectsPage(_currentPage);
        }

        /// <summary>
        /// 【测试用】为HeatPoints填充虚拟缺陷信息
        /// 正式环境请注释或删除此方法的调用
        /// </summary>
        private void FillTestDefectInfo(List<DetectInfo> heatPoints)
        {
            if (heatPoints == null || heatPoints.Count == 0)
                return;

            // 虚拟缺陷名称列表
            string[] testDefectNames = new string[]
            {
                "划伤", "异物", "凹坑", "气泡", "裂纹",
                "污渍", "变形", "缺角", "毛刺", "氧化"
            };

            Random random = new Random();

            for (int i = 0; i < heatPoints.Count; i++)
            {
                var hp = heatPoints[i];

                // 只为没有缺陷名称的项填充虚拟数据
                if (string.IsNullOrEmpty(hp.DefectName))
                {
                    // 随机选择一个缺陷名称
                    hp.DefectName = testDefectNames[random.Next(testDefectNames.Length)];
                }

                // 如果没有有效的缺陷框信息，填充虚拟数据
                if (hp.Width <= 0 || hp.Height <= 0)
                {
                    // 虚拟缺陷框位置和大小（基于图片中心区域）
                    hp.RoiX = 50 + random.Next(100);
                    hp.RoiY = 50 + random.Next(100);
                    hp.Width = 30 + random.Next(50);
                    hp.Height = 30 + random.Next(50);
                }
            }
        }

        private void LoadDefectsPage(int page)
        {
            flowLayoutPanel_DefectImages.Controls.Clear();
            _selectedIndex = -1;

            if (_filteredHeatPoints.Count == 0)
            {
                var noDataLabel = new Label
                {
                    Text = "该记录无缺陷图片",
                    AutoSize = true,
                    ForeColor = Color.White,
                    Font = new Font("微软雅黑", 10F),
                    Margin = new Padding(10)
                };
                flowLayoutPanel_DefectImages.Controls.Add(noDataLabel);
                lblPageInfo.Text = "第 0/0 页";
                btnPrevPage.Enabled = false;
                btnNextPage.Enabled = false;
                return;
            }

            _currentPage = page;
            var heatPointsToShow = _filteredHeatPoints.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();

            for (int i = 0; i < heatPointsToShow.Count; i++)
            {
                var panel = CreateDefectImagePanel(heatPointsToShow[i], i);
                flowLayoutPanel_DefectImages.Controls.Add(panel);
            }

            if (flowLayoutPanel_DefectImages.Controls.Count > 0)
            {
                SelectImage(0);
            }

            UpdatePaginationButtons();
        }

        private void UpdatePaginationButtons()
        {
            lblPageInfo.Text = $"第 {_currentPage}/{_totalPages} 页";
            btnPrevPage.Enabled = _currentPage > 1;
            btnNextPage.Enabled = _currentPage < _totalPages;
        }

        private void SelectImage(int index)
        {
            if (index < 0 || index >= flowLayoutPanel_DefectImages.Controls.Count)
                return;

            // Deselect old
            if (_selectedIndex >= 0 && _selectedIndex < flowLayoutPanel_DefectImages.Controls.Count)
            {
                if (flowLayoutPanel_DefectImages.Controls[_selectedIndex] is Panel oldP)
                {
                    UpdatePanelAppearance(oldP, false);
                }
            }

            // Select new
            _selectedIndex = index;
            if (flowLayoutPanel_DefectImages.Controls[_selectedIndex] is Panel newP)
            {
                UpdatePanelAppearance(newP, true);
                flowLayoutPanel_DefectImages.ScrollControlIntoView(newP);
                newP.Focus();
            }
        }

        private void UpdatePanelAppearance(Panel panel, bool isSelected)
        {
            var imageContainer = panel.Controls.OfType<Panel>().FirstOrDefault();
            if (imageContainer == null) return;

            var topPictureBox = imageContainer.Controls.OfType<PictureBox>().FirstOrDefault();
            if (topPictureBox != null)
            {
                var label = topPictureBox.Controls.OfType<Label>().FirstOrDefault();
                if (label != null && label.Tag is DetectInfo heatPoint)
                {
                    Color borderColor;

                    if (isSelected)
                    {
                        // 选中状态：使用醒目的高亮颜色（亮青色）
                        borderColor = Color.FromArgb(0, 200, 255);
                        panel.Padding = new Padding(6);
                    }
                    else
                    {
                        // 未选中状态：根据VVS状态显示边框颜色
                        // VVSStatus: 0 未运行 / 1 OK / 2 NG
                        switch (heatPoint.VVSStatus)
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
                        panel.Padding = new Padding(2);
                    }

                    panel.BackColor = borderColor;
                }
            }
        }

        private void SelectNextImage()
        {
            if (_filteredHeatPoints == null || _filteredHeatPoints.Count == 0) return;

            int globalIndex = ((_currentPage - 1) * PageSize) + _selectedIndex;
            int nextGlobalIndex = (globalIndex + 1) % _filteredHeatPoints.Count;
            if (nextGlobalIndex == 0) return;
            int nextPage = (nextGlobalIndex / PageSize) + 1;
            int nextLocalIndex = nextGlobalIndex % PageSize;

            if (nextPage != _currentPage)
            {
                LoadDefectsPage(nextPage);
            }
            SelectImage(nextLocalIndex);
        }

        private void SelectPreviousImage()
        {
            if (_filteredHeatPoints == null || _filteredHeatPoints.Count == 0) return;

            int globalIndex = ((_currentPage - 1) * PageSize) + _selectedIndex;
            int prevGlobalIndex = (globalIndex - 1 + _filteredHeatPoints.Count) % _filteredHeatPoints.Count;

            int prevPage = (prevGlobalIndex / PageSize) + 1;
            int prevLocalIndex = prevGlobalIndex % PageSize;

            if (prevPage != _currentPage)
            {
                LoadDefectsPage(prevPage);
            }
            SelectImage(prevLocalIndex);
        }

        private Panel CreateDefectImagePanel(DetectInfo heatPoint, int index)
        {
            var panel = new Panel
            {
                Width = 300,
                // Adjust height to fit the container, accounting for margins
                Height = flowLayoutPanel_DefectImages.ClientSize.Height - flowLayoutPanel_DefectImages.Padding.Vertical - 6, // 6 for top/bottom margin
                Margin = new Padding(3),
                BackColor = Color.FromArgb(37, 37, 38)
            };
            panel.Click += (s, e) => SelectImage(index);

            // Main container for the two images
            var imageContainer = new Panel { Dock = DockStyle.Fill };
            panel.Controls.Add(imageContainer);

            // Top PictureBox for the original image
            var topPictureBox = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = imageContainer.Height / 2,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            topPictureBox.Click += (s, e) => SelectImage(index);
            imageContainer.Controls.Add(topPictureBox);

            // Bottom PictureBox for the template image
            var bottomPictureBox = new PictureBox
            {
                Dock = DockStyle.Bottom,
                Height = imageContainer.Height / 2,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            bottomPictureBox.Click += (s, e) => SelectImage(index);
            imageContainer.Controls.Add(bottomPictureBox);

            // Load original image
            if (!string.IsNullOrEmpty(heatPoint.ImagePath) && File.Exists(heatPoint.ImagePath))
            {
                try
                {
                    using (var img = Image.FromFile(heatPoint.ImagePath))
                    {
                        // 绘制带缺陷框和缺陷名称的图片
                        topPictureBox.Image = DrawDefectBoxOnImage(img, heatPoint);
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"加载图片失败: {heatPoint.ImagePath}, {ex.Message}");
                }

                // Load template image
                try
                {
                    string dir = Path.GetDirectoryName(heatPoint.ImagePath);
                    string filename = Path.GetFileNameWithoutExtension(heatPoint.ImagePath);
                    string ext = Path.GetExtension(heatPoint.ImagePath);

                    // 新逻辑：在同目录中查找包含原图名、包含"template"并且扩展名相同的文件
                    var candidates = Directory.EnumerateFiles(dir)
                        .Where(p => string.Equals(Path.GetExtension(p), ext, StringComparison.OrdinalIgnoreCase))
                        .Where(p =>
                        {
                            var name = Path.GetFileNameWithoutExtension(p);
                            return name.IndexOf(filename, StringComparison.OrdinalIgnoreCase) >= 0
                                   && name.IndexOf("template", StringComparison.OrdinalIgnoreCase) >= 0;
                        })
                        .ToList();

                    string templatePath = candidates.FirstOrDefault();

                    if (!string.IsNullOrEmpty(templatePath) && File.Exists(templatePath))
                    {
                        using (var img = Image.FromFile(templatePath))
                        {
                            // 模板图也绘制缺陷框
                            bottomPictureBox.Image = DrawDefectBoxOnImage(img, heatPoint);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"加载模板图片失败: {ex.Message}");
                }
            }

            // 信息显示
            var infoLabel = new Label
            {
                Text = $"AI: {GetStatusText(heatPoint.AIStatus)}\n" +
                       $"VVS: {GetStatusText(heatPoint.VVSStatus)}",
                AutoSize = false,
                Dock = DockStyle.Bottom,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(128, 0, 0, 0),
                Font = new Font("微软雅黑", 9F),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 5, 0)
            };
            infoLabel.Tag = heatPoint;
            infoLabel.Click += (s, e) => SelectImage(index);

            topPictureBox.Controls.Add(infoLabel);

            UpdatePanelAppearance(panel, false); // Set initial appearance

            return panel;
        }

        public void ClearDetails()
        {
            flowLayoutPanel_DefectImages.Controls.Clear();
            label_DetailTitle.Text = "ȱ";
            _selectedIndex = -1;
            _allHeatPoints?.Clear();
            _filteredHeatPoints?.Clear();
            _currentPage = 1;
            _totalPages = 0;
            if(lblPageInfo != null)
            {
                lblPageInfo.Text = "";
                btnPrevPage.Enabled = false;
                btnNextPage.Enabled = false;
            }
        }

        public List<DetectInfo> GetHeatPoints()
        {
            return _allHeatPoints ?? new List<DetectInfo>();
        }

        public List<DetectInfo> GetFilteredHeatPoints()
        {
            return _filteredHeatPoints ?? new List<DetectInfo>();
        }

        public (string aiFilter, string vvsFilter) GetFilters()
        {
            return (_aiFilter, _vvsFilter);
        }

        /// <summary>
        /// 在图片上绘制缺陷框和缺陷名称
        /// </summary>
        /// <param name="originalImage">原始图片</param>
        /// <param name="heatPoint">缺陷信息</param>
        /// <returns>绘制了缺陷框的图片</returns>
        private Bitmap DrawDefectBoxOnImage(Image originalImage, DetectInfo heatPoint)
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
                using (Pen pen = new Pen(boxColor, 2))
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

    /// <summary>
    /// VVS复判完成事件参数
    /// </summary>
    public class VvsCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// 完成复判的缺陷点列表
        /// </summary>
        public List<DetectInfo> HeatPoints { get; set; }

        /// <summary>
        /// 是否全部OK（true=全部OK, false=有NG）
        /// </summary>
        public bool AllOk { get; set; }

        /// <summary>
        /// NG数量
        /// </summary>
        public int NgCount { get; set; }
    }
}

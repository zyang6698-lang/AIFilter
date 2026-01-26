using System;
using System.Drawing;
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
        private const int PageSize = 5;
        private int _totalPages;
        private string _aiFilter = "All";
        private string _vvsFilter = "All";

        // 存储原始的DefectReviewItem列表，用于按SN分组检查VVS状态
        private List<DefectReviewItem> _sourceItems;
        // 记录已经触发过完成事件的SN（避免重复触发）
        private readonly HashSet<string> _completedSnSet = new HashSet<string>();

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

        /// <summary>
        /// 当请求单图测试时触发
        /// </summary>
        public event EventHandler<SingleImageTestEventArgs> SingleImageTestRequested;

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

            if (control is DefectImageItemControl itemControl)
            {
                var heatPoint = itemControl.HeatPoint;
                if (heatPoint == null) return;

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

                // 更新状态显示
                itemControl.UpdateStatusLabel();

                // 更新边框颜色
                itemControl.IsSelected = true;
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

            // 合并所有HeatPoints，并设置DisplaySN
            _allHeatPoints = new List<DetectInfo>();
            foreach (var item in items)
            {
                if (item.HeatPoints != null)
                {
                    // 为每个缺陷点设置所属的SN和Side
                    foreach (var hp in item.HeatPoints)
                    {
                        hp.DisplaySN = $"{item.SerialNumber} ({item.Side})";
                    }
                    _allHeatPoints.AddRange(item.HeatPoints);
                }
            }

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

            // 计算控件高度
            int itemHeight = flowLayoutPanel_DefectImages.ClientSize.Height - flowLayoutPanel_DefectImages.Padding.Vertical - 6;

            for (int i = 0; i < heatPointsToShow.Count; i++)
            {
                var itemControl = CreateDefectImageItemControl(heatPointsToShow[i], i, itemHeight);
                flowLayoutPanel_DefectImages.Controls.Add(itemControl);
            }

            if (flowLayoutPanel_DefectImages.Controls.Count > 0)
            {
                SelectImage(0);
            }

            UpdatePaginationButtons();
        }

        /// <summary>
        /// 创建缺陷图片项控件
        /// </summary>
        private DefectImageItemControl CreateDefectImageItemControl(DetectInfo heatPoint, int index, int height)
        {
            var itemControl = new DefectImageItemControl
            {
                Width = 300,
                Height = height,
                Margin = new Padding(3),
                HeatPoint = heatPoint
            };

            // 调整图片高度
            itemControl.AdjustImageHeight(height);

            // 绑定点击事件
            itemControl.ItemClicked += (s, e) => SelectImage(index);

            // 绑定运行测试事件
            itemControl.RunTestRequested += (s, e) =>
            {
                SingleImageTestRequested?.Invoke(this, e);
            };

            return itemControl;
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
                if (flowLayoutPanel_DefectImages.Controls[_selectedIndex] is DefectImageItemControl oldItem)
                {
                    oldItem.IsSelected = false;
                }
            }

            // Select new
            _selectedIndex = index;
            if (flowLayoutPanel_DefectImages.Controls[_selectedIndex] is DefectImageItemControl newItem)
            {
                newItem.IsSelected = true;
                flowLayoutPanel_DefectImages.ScrollControlIntoView(newItem);
                // 将焦点设置到UserControl本身，确保ProcessCmdKey能正确处理键盘事件
                this.Focus();
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

    /// <summary>
    /// 单图测试事件参数
    /// </summary>
    public class SingleImageTestEventArgs : EventArgs
    {
        /// <summary>
        /// 要测试的缺陷点信息
        /// </summary>
        public DetectInfo HeatPoint { get; set; }
    }
}

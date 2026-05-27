using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using DeepSightDB;
using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;

namespace DeepSightAI
{
    public partial class UcDefectDetail : UserControl
    {
        private int _selectedIndex = -1;
        private List<DetectInfo> _allHeatPoints;
        private List<DetectInfo> _filteredHeatPoints; // For filtered data
        private int _currentPage = 1;
        private const int DefaultPageSize = 5;
        private const int TwoImagesPerPageSize = 1;
        private int _pageSize = DefaultPageSize;
        private bool _largeImageMode;
        private int _totalPages;
        private string _aiFilter = "All";
        private string _vvsFilter = "All";
        private string _vrsFilter = "All";
        private string _defectNameFilter = "All";
        private bool _comparisonMode = false;
        private string _comparisonOriginalAiFilter = "All";
        private string _comparisonNewAiFilter = "All";
        private string _comparisonChangeFilter = "All";
        private readonly Dictionary<DetectInfo, AiComparisonDefectItem> _comparisonItems = new Dictionary<DetectInfo, AiComparisonDefectItem>();
        private Label label_FilterOriginalAI;
        private ComboBox comboBox_FilterOriginalAI;
        private Label label_FilterNewAI;
        private ComboBox comboBox_FilterNewAI;
        private Label label_FilterChange;
        private ComboBox comboBox_FilterChange;
        private StyledButton btn_DisplayMode;
        private StyledButton btn_ImageTypeSelector;
        private CheckedListBox checkedListBox_ImageTypes;
        private ToolStripDropDown dropDown_ImageTypes;
        private bool _updatingImageTypeChecks;
        private readonly List<DefectDisplayImageType> _selectedDisplayImageTypes = new List<DefectDisplayImageType>
        {
            DefectDisplayImageType.DefectBox,
            DefectDisplayImageType.Template,
            DefectDisplayImageType.Avi
        };

        // 存储原始的DefectReviewItem列表，用于按SN分组检查VVS状态
        private List<DefectReviewItem> _sourceItems;
        // 记录已经触发过完成事件的SN（避免重复触发）
        private readonly HashSet<string> _completedSnSet = new HashSet<string>();
        // 用于取消正在进行的异步图片加载（翻页或重新加载时取消旧任务）
        private CancellationTokenSource _loadCts;

        /// <summary>
        /// 当需要切换到下一行记录时触发（按Tab键时）
        /// </summary>
        public event EventHandler SelectNextRowRequested;

        /// <summary>
        /// 当某个SN的所有缺陷点VVS状态都已设置时触发
        /// </summary>
        public event EventHandler<VvsCompletedEventArgs> SnVvsCompleted;

        /// <summary>
        /// 当VVS状态改变时触发，传递被修改的缺陷点信息
        /// </summary>
        public event EventHandler<VvsStatusChangedEventArgs> VvsStatusChanged;

        /// <summary>
        /// 当请求单图测试时触发
        /// </summary>
        public event EventHandler<SingleImageTestEventArgs> SingleImageTestRequested;

        /// <summary>
        /// 当请求导出时触发
        /// </summary>
        public event EventHandler ExportRequested;

        public UcDefectDetail()
        {
            InitializeComponent();
            InitializeFilterControls();
            InitializeComparisonFilterControls();
            InitializePaginationControls();
            InitializeDisplayModeButton();
            InitializeImageTypeSelector();
            InitializeExportButton();
        }

        private void InitializeDisplayModeButton()
        {
            btn_DisplayMode = new StyledButton
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(panel_Top.Width - 100, 5),
                Size = new Size(90, 30),
                Text = "大图模式"
            };
            btn_DisplayMode.Click += (s, e) => ToggleDisplayMode();
            panel_Top.Controls.Add(btn_DisplayMode);
            btn_DisplayMode.BringToFront();
        }

        private void InitializeImageTypeSelector()
        {
            btn_ImageTypeSelector = new StyledButton
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(panel_Top.Width - 290, 5),
                Size = new Size(180, 30),
                Text = "图片选择 ▼"
            };
            btn_ImageTypeSelector.Click += (s, e) => dropDown_ImageTypes.Show(btn_ImageTypeSelector, new Point(0, btn_ImageTypeSelector.Height));

            var dropPanel = new Panel
            {
                BackColor = Color.FromArgb(45, 45, 48),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8),
                Size = new Size(200, 170)
            };

            checkedListBox_ImageTypes = new CheckedListBox
            {
                BorderStyle = BorderStyle.None,
                CheckOnClick = true,
                Font = new Font("微软雅黑", 9F),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.FromArgb(216, 219, 188),
                IntegralHeight = false,
                Location = new Point(8, 8),
                Size = new Size(184, 126),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 24
            };

            checkedListBox_ImageTypes.Items.Add(new ImageTypeSelectionItem(DefectDisplayImageType.DefectBox, "缺陷框图"));
            checkedListBox_ImageTypes.Items.Add(new ImageTypeSelectionItem(DefectDisplayImageType.Original, "原图"));
            checkedListBox_ImageTypes.Items.Add(new ImageTypeSelectionItem(DefectDisplayImageType.Template, "模板图"));
            checkedListBox_ImageTypes.Items.Add(new ImageTypeSelectionItem(DefectDisplayImageType.Gerber, "Gerber图"));
            checkedListBox_ImageTypes.Items.Add(new ImageTypeSelectionItem(DefectDisplayImageType.Avi, "AVI图"));
            checkedListBox_ImageTypes.ItemCheck += CheckedListBox_ImageTypes_ItemCheck;
            checkedListBox_ImageTypes.DrawItem += CheckedListBox_ImageTypes_DrawItem;

            var labelHint = new Label
            {
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.Silver,
                Font = new Font("微软雅黑", 8F),
                Location = new Point(8, 138),
                Size = new Size(184, 24),
                Text = "最多3项，按选择顺序显示",
                TextAlign = ContentAlignment.MiddleLeft
            };

            dropPanel.Controls.Add(checkedListBox_ImageTypes);
            dropPanel.Controls.Add(labelHint);

            dropDown_ImageTypes = new ToolStripDropDown
            {
                Padding = Padding.Empty,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            dropDown_ImageTypes.Items.Add(new ToolStripControlHost(dropPanel)
            {
                AutoSize = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Size = dropPanel.Size
            });

            panel_Top.Controls.Add(btn_ImageTypeSelector);
            btn_ImageTypeSelector.BringToFront();
            SyncImageTypeChecks();
            UpdateImageTypeSelectorText();
        }

        private void InitializeExportButton()
        {
            this.btn_Export.Click += (s, e) =>
            {
                ExportRequested?.Invoke(this, EventArgs.Empty);
            };
        }

        private void CheckedListBox_ImageTypes_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_updatingImageTypeChecks) return;
            if (!(checkedListBox_ImageTypes.Items[e.Index] is ImageTypeSelectionItem item)) return;

            if (e.NewValue == CheckState.Checked)
            {
                if (!_selectedDisplayImageTypes.Contains(item.ImageType))
                {
                    _selectedDisplayImageTypes.Add(item.ImageType);
                }

                while (_selectedDisplayImageTypes.Count > 3)
                {
                    _selectedDisplayImageTypes.RemoveAt(0);
                }
            }
            else
            {
                if (_selectedDisplayImageTypes.Count <= 1 && _selectedDisplayImageTypes.Contains(item.ImageType))
                {
                    e.NewValue = CheckState.Checked;
                    return;
                }

                _selectedDisplayImageTypes.Remove(item.ImageType);
            }

            BeginInvoke((Action)(() =>
            {
                SyncImageTypeChecks();
                UpdateImageTypeSelectorText();
                ReloadCurrentPageForImageTypeChange();
            }));
        }

        private void CheckedListBox_ImageTypes_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool isChecked = checkedListBox_ImageTypes.GetItemChecked(e.Index);
            Color backColor = selected ? Color.FromArgb(0, 86, 110) : Color.FromArgb(45, 45, 48);
            Color textColor = Color.FromArgb(216, 219, 188);

            using (var backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            var checkRect = new Rectangle(e.Bounds.Left + 4, e.Bounds.Top + 5, 14, 14);
            ControlPaint.DrawCheckBox(e.Graphics, checkRect, isChecked ? ButtonState.Checked : ButtonState.Normal);

            var textRect = new Rectangle(e.Bounds.Left + 24, e.Bounds.Top, e.Bounds.Width - 28, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, checkedListBox_ImageTypes.Items[e.Index].ToString(), e.Font, textRect, textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private void SyncImageTypeChecks()
        {
            if (checkedListBox_ImageTypes == null) return;

            _updatingImageTypeChecks = true;
            try
            {
                for (int i = 0; i < checkedListBox_ImageTypes.Items.Count; i++)
                {
                    var item = checkedListBox_ImageTypes.Items[i] as ImageTypeSelectionItem;
                    checkedListBox_ImageTypes.SetItemChecked(i, item != null && _selectedDisplayImageTypes.Contains(item.ImageType));
                }
            }
            finally
            {
                _updatingImageTypeChecks = false;
            }
        }

        private void UpdateImageTypeSelectorText()
        {
            if (btn_ImageTypeSelector == null) return;
            btn_ImageTypeSelector.Text = "图片选择 ▼";
        }

        private string GetDisplayImageTypeText(DefectDisplayImageType imageType)
        {
            switch (imageType)
            {
                case DefectDisplayImageType.Original:
                    return "原图";
                case DefectDisplayImageType.Template:
                    return "模板图";
                case DefectDisplayImageType.Gerber:
                    return "Gerber图";
                case DefectDisplayImageType.Avi:
                    return "AVI图";
                default:
                    return "缺陷框图";
            }
        }

        private void ReloadCurrentPageForImageTypeChange()
        {
            if (_filteredHeatPoints == null) return;
            int selectedIndex = _selectedIndex;
            LoadDefectsPage(_currentPage);
            if (selectedIndex >= 0 && selectedIndex < flowLayoutPanel_DefectImages.Controls.Count)
            {
                SelectImage(selectedIndex);
            }
        }

        private void InitializeFilterControls()
        {
            // AI Filter
            this.comboBox_FilterAI.Items.AddRange(new object[] { "All", "AI_OK", "AI_NG", "AI_直报" });
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

            // VRS Filter
            this.comboBox_FilterVRS.Items.AddRange(new object[] { "All", "VRS_OK", "VRS_NG", "VRS_Ignore", "VRS_NoResult", "VRS_NotAcceptNg", "NotSet" });
            this.comboBox_FilterVRS.SelectedIndex = 0;
            this.comboBox_FilterVRS.SelectedIndexChanged += (s, e) =>
            {
                _vrsFilter = this.comboBox_FilterVRS.SelectedItem.ToString();
                ApplyFiltersAndReload();
            };

            // Defect Name Filter - 事件绑定在PopulateDefectNameFilter中管理
        }

        /// <summary>
        /// 初始化AI前后对比模式的筛选控件，默认隐藏，普通复判详情不受影响。
        /// </summary>
        private void InitializeComparisonFilterControls()
        {
            label_FilterOriginalAI = CreateFilterLabel("旧AI:", 20);
            comboBox_FilterOriginalAI = CreateComparisonComboBox(75);
            comboBox_FilterOriginalAI.SelectedIndexChanged += (s, e) =>
            {
                _comparisonOriginalAiFilter = comboBox_FilterOriginalAI.SelectedItem?.ToString() ?? "All";
                ApplyFiltersAndReload();
            };

            label_FilterNewAI = CreateFilterLabel("新AI:", 210);
            comboBox_FilterNewAI = CreateComparisonComboBox(265);
            comboBox_FilterNewAI.SelectedIndexChanged += (s, e) =>
            {
                _comparisonNewAiFilter = comboBox_FilterNewAI.SelectedItem?.ToString() ?? "All";
                ApplyFiltersAndReload();
            };

            label_FilterChange = CreateFilterLabel("变化:", 400);
            comboBox_FilterChange = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(455, 14),
                Size = new Size(120, 28),
                Visible = false
            };
            comboBox_FilterChange.Items.AddRange(new object[] { "All", "已变化", "无变化" });
            comboBox_FilterChange.SelectedIndex = 0;
            comboBox_FilterChange.SelectedIndexChanged += (s, e) =>
            {
                _comparisonChangeFilter = comboBox_FilterChange.SelectedItem?.ToString() ?? "All";
                ApplyFiltersAndReload();
            };

            panel_Filter.Controls.Add(label_FilterOriginalAI);
            panel_Filter.Controls.Add(comboBox_FilterOriginalAI);
            panel_Filter.Controls.Add(label_FilterNewAI);
            panel_Filter.Controls.Add(comboBox_FilterNewAI);
            panel_Filter.Controls.Add(label_FilterChange);
            panel_Filter.Controls.Add(comboBox_FilterChange);
            SetComparisonFilterVisible(false);
        }

        private Label CreateFilterLabel(string text, int x)
        {
            return new Label
            {
                AutoSize = true,
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.White,
                Location = new Point(x, 16),
                Text = text,
                Visible = false
            };
        }

        private ComboBox CreateComparisonComboBox(int x)
        {
            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(x, 14),
                Size = new Size(110, 28),
                Visible = false
            };
            combo.Items.AddRange(new object[] { "All", "未检测", "OK", "NG", "异常" });
            combo.SelectedIndex = 0;
            return combo;
        }

        private void SetComparisonFilterVisible(bool visible)
        {
            label_FilterAI.Visible = !visible;
            comboBox_FilterAI.Visible = !visible;
            label_FilterVVS.Visible = !visible;
            comboBox_FilterVVS.Visible = !visible;
            label_FilterVRS.Visible = !visible;
            comboBox_FilterVRS.Visible = !visible;

            label_FilterOriginalAI.Visible = visible;
            comboBox_FilterOriginalAI.Visible = visible;
            label_FilterNewAI.Visible = visible;
            comboBox_FilterNewAI.Visible = visible;
            label_FilterChange.Visible = visible;
            comboBox_FilterChange.Visible = visible;

            label_FilterDefectName.Location = visible ? new Point(595, 16) : new Point(470, 16);
            comboBox_FilterDefectName.Location = visible ? new Point(675, 14) : new Point(549, 14);
        }

        /// <summary>
        /// 根据当前所有缺陷点的DefectName，填充缺陷名称下拉框（带数量）
        /// </summary>
        private void PopulateDefectNameFilter()
        {
            comboBox_FilterDefectName.SelectedIndexChanged -= ComboBox_FilterDefectName_SelectedIndexChanged;
            comboBox_FilterDefectName.Items.Clear();

            int totalCount = _allHeatPoints?.Count ?? 0;
            comboBox_FilterDefectName.Items.Add($"All ({totalCount})");

            if (_allHeatPoints != null && _allHeatPoints.Count > 0)
            {
                // 按缺陷名称分组统计数量，空名称归入"其他"
                var groups = _allHeatPoints
                    .GroupBy(p => string.IsNullOrEmpty(p.DefectName) ? "其他" : p.DefectName)
                    .OrderByDescending(g => g.Count());

                foreach (var g in groups)
                {
                    comboBox_FilterDefectName.Items.Add($"{g.Key} ({g.Count()})");
                }
            }

            comboBox_FilterDefectName.SelectedIndex = 0;
            comboBox_FilterDefectName.SelectedIndexChanged += ComboBox_FilterDefectName_SelectedIndexChanged;
        }

        private void ComboBox_FilterDefectName_SelectedIndexChanged(object sender, EventArgs e)
        {
            _defectNameFilter = this.comboBox_FilterDefectName.SelectedItem?.ToString() ?? "All";
            // 提取纯名称部分（去掉括号中的数量）
            int parenIndex = _defectNameFilter.IndexOf(" (");
            if (parenIndex > 0)
                _defectNameFilter = _defectNameFilter.Substring(0, parenIndex);
            ApplyFiltersAndReload();
        }

        private void ApplyFiltersAndReload()
        {
            _filteredHeatPoints = _allHeatPoints ?? new List<DetectInfo>();

            if (_comparisonMode)
            {
                if (TryGetAiStatusFilter(_comparisonOriginalAiFilter, out int originalAiStatus))
                {
                    _filteredHeatPoints = _filteredHeatPoints
                        .Where(p => _comparisonItems.ContainsKey(p) && _comparisonItems[p].OriginalAIStatus == originalAiStatus)
                        .ToList();
                }

                if (TryGetAiStatusFilter(_comparisonNewAiFilter, out int newAiStatus))
                {
                    _filteredHeatPoints = _filteredHeatPoints
                        .Where(p => _comparisonItems.ContainsKey(p) && _comparisonItems[p].NewAIStatus == newAiStatus)
                        .ToList();
                }

                if (_comparisonChangeFilter == "已变化")
                {
                    _filteredHeatPoints = _filteredHeatPoints
                        .Where(p => _comparisonItems.ContainsKey(p) && _comparisonItems[p].IsChanged)
                        .ToList();
                }
                else if (_comparisonChangeFilter == "无变化")
                {
                    _filteredHeatPoints = _filteredHeatPoints
                        .Where(p => _comparisonItems.ContainsKey(p) && !_comparisonItems[p].IsChanged)
                        .ToList();
                }
            }
            else if (_aiFilter != "All")
            {
                // AIStatus: 0 未运行 / 1 OK / 2 NG / 3 异常 / 4 直报
                int targetAiStatus;
                switch (_aiFilter)
                {
                    case "AI_OK": targetAiStatus = 1; break;
                    case "AI_NG": targetAiStatus = 2; break;
                    case "AI_直报": targetAiStatus = 4; break;
                    default: targetAiStatus = -1; break;
                }
                if (targetAiStatus >= 0)
                    _filteredHeatPoints = _filteredHeatPoints.Where(p => p.AIStatus == targetAiStatus).ToList();
            }

            if (!_comparisonMode && _vvsFilter != "All")
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

            if (!_comparisonMode && _vrsFilter != "All")
            {
                // VrsState: 0=未判定, 1=OK, 2=NG, 3=忽略, 4=无结果, 5=NG不接收
                int targetVrsState;
                switch (_vrsFilter)
                {
                    case "NotSet": targetVrsState = 0; break;
                    case "VRS_OK": targetVrsState = 1; break;
                    case "VRS_NG": targetVrsState = 2; break;
                    case "VRS_Ignore": targetVrsState = 3; break;
                    case "VRS_NoResult": targetVrsState = 4; break;
                    case "VRS_NotAcceptNg": targetVrsState = 5; break;
                    default: targetVrsState = -1; break;
                }
                if (targetVrsState >= 0)
                    _filteredHeatPoints = _filteredHeatPoints.Where(p => p.VrsState == targetVrsState).ToList();
            }

            if (_defectNameFilter != "All")
            {
                if (_defectNameFilter == "其他")
                {
                    _filteredHeatPoints = _filteredHeatPoints.Where(p => string.IsNullOrEmpty(p.DefectName)).ToList();
                }
                else
                {
                    _filteredHeatPoints = _filteredHeatPoints.Where(p => p.DefectName == _defectNameFilter).ToList();
                }
            }

            _totalPages = CalculateTotalPages();
            _currentPage = 1;
            LoadDefectsPage(_currentPage);
        }

        private bool TryGetAiStatusFilter(string filter, out int status)
        {
            switch (filter)
            {
                case "未检测": status = 0; return true;
                case "OK": status = 1; return true;
                case "NG": status = 2; return true;
                case "异常": status = 3; return true;
                default: status = -1; return false;
            }
        }

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

        /// <summary>
        /// 将配置中的快捷键字符串解析为Keys枚举值
        /// </summary>
        private static Keys ParseShortcutKey(string keyName)
        {
            if (string.IsNullOrEmpty(keyName)) return Keys.None;
            if (Enum.TryParse(keyName, true, out Keys result))
                return result;
            return Keys.None;
        }

        /// <summary>
        /// 判断按下的键是否匹配配置的快捷键（同时匹配数字键和小键盘数字键）
        /// </summary>
        private static bool MatchShortcutKey(Keys keyData, string configKeyName)
        {
            Keys configKey = ParseShortcutKey(configKeyName);
            if (configKey == Keys.None) return false;
            if (keyData == configKey) return true;
            // D0-D9 同时匹配 NumPad0-NumPad9
            if (configKeyName != null && configKeyName.Length == 2 && configKeyName.StartsWith("D") && char.IsDigit(configKeyName[1]))
            {
                Keys numPadKey = ParseShortcutKey("NumPad" + configKeyName[1]);
                if (keyData == numPadKey) return true;
            }
            return false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var cfg = Machine.sysConfig;

            if (!_comparisonMode && cfg != null && MatchShortcutKey(keyData, cfg.ShortcutVvsOk))
            {
                TagImage("VVS_OK", false);
                SelectNextImage();
                return true;
            }
            else if (!_comparisonMode && cfg != null && MatchShortcutKey(keyData, cfg.ShortcutVvsNg))
            {
                TagImage("VVS_NG", false);
                SelectNextImage();
                return true;
            }
            else if (!_comparisonMode && cfg != null && MatchShortcutKey(keyData, cfg.ShortcutVvsNotSet))
            {
                TagImage("VVS_NotSet", false);
                SelectNextImage();
                return true;
            }
            else if (cfg != null && MatchShortcutKey(keyData, cfg.ShortcutNextRow))
            {
                // 触发事件，通知父控件切换到下一行
                SelectNextRowRequested?.Invoke(this, EventArgs.Empty);
                return true;
            }
            else if (cfg != null && MatchShortcutKey(keyData, cfg.ShortcutNextImage))
            {
                // 下一张图片
                SelectNextImage();
                return true;
            }
            else if (cfg != null && MatchShortcutKey(keyData, cfg.ShortcutPrevImage))
            {
                // 上一张图片
                SelectPreviousImage();
                return true;
            }
            else if (cfg != null && MatchShortcutKey(keyData, cfg.ShortcutNextPage))
            {
                // 下一页
                if (_currentPage < _totalPages)
                {
                    _currentPage++;
                    LoadDefectsPage(_currentPage);
                }
                return true;
            }
            else if (cfg != null && MatchShortcutKey(keyData, cfg.ShortcutPrevPage))
            {
                // 上一页
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

            if (control is UcDefectImageItem itemControl)
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

                    // 触发VVS状态改变事件，传递被修改的缺陷点
                    VvsStatusChanged?.Invoke(this, new VvsStatusChangedEventArgs { ModifiedHeatPoint = heatPoint });
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
            if (item == null)
            {
                ClearDetails();
                return;
            }

            DisplayDefectDetails(new List<DefectReviewItem> { item }, $" SN: {item.SerialNumber} ({item.Side})");
        }

        public void DisplayDefectDetailsTwoImagesPerPage(DefectReviewItem item)
        {
            if (item == null)
            {
                ClearDetails();
                return;
            }

            DisplayDefectDetailsTwoImagesPerPage(new List<DefectReviewItem> { item }, $" SN: {item.SerialNumber} ({item.Side})");
        }

        /// <summary>
        /// 显示多个DefectReviewItem的缺陷详情（用于Lot模式下查看多个SN）
        /// </summary>
        public void DisplayDefectDetails(List<DefectReviewItem> items, string title)
        {
            DisplayDefectDetailsCore(items, title, DefaultPageSize);
        }

        public void DisplayDefectDetailsTwoImagesPerPage(List<DefectReviewItem> items, string title)
        {
            DisplayDefectDetailsCore(items, title, TwoImagesPerPageSize);
        }

        private void DisplayDefectDetailsCore(List<DefectReviewItem> items, string title, int pageSize)
        {
            // 取消正在进行的异步图片加载
            CancelPendingImageLoads();

            _pageSize = NormalizePageSize(pageSize);
            _largeImageMode = _pageSize == TwoImagesPerPageSize;
            UpdateDisplayModeButton();

            _comparisonMode = false;
            _comparisonItems.Clear();
            SetComparisonFilterVisible(false);

            // 保存原始items列表，用于按SN分组检查VVS状态
            _sourceItems = items;
            _completedSnSet.Clear();

            // 合并所有HeatPoints，并设置DisplaySN
            _allHeatPoints = new List<DetectInfo>();
            foreach (var item in items ?? new List<DefectReviewItem>())
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
            _totalPages = CalculateTotalPages();
            _currentPage = 1;

            label_DetailTitle.Text = title;

            // Reset filters
            _aiFilter = "All";
            _vvsFilter = "All";
            _vrsFilter = "All";
            _defectNameFilter = "All";
            comboBox_FilterAI.SelectedIndex = 0;
            comboBox_FilterVVS.SelectedIndex = 0;
            comboBox_FilterVRS.SelectedIndex = 0;
            PopulateDefectNameFilter();

            LoadDefectsPage(_currentPage);
        }

        /// <summary>
        /// 使用现有缺陷详情控件展示AI前后结果对比，复用图片加载、缺陷框绘制、分页和缺陷名称筛选能力。
        /// </summary>
        public void DisplayAiComparisonDetails(List<AiComparisonDefectItem> items, string title)
        {
            CancelPendingImageLoads();

            _pageSize = DefaultPageSize;
            _largeImageMode = false;
            UpdateDisplayModeButton();

            _comparisonMode = true;
            _sourceItems = null;
            _completedSnSet.Clear();
            _comparisonItems.Clear();
            SetComparisonFilterVisible(true);

            _allHeatPoints = new List<DetectInfo>();
            foreach (var item in items ?? new List<AiComparisonDefectItem>())
            {
                var point = item.DetectInfo?.Clone();
                if (point == null)
                    continue;

                point.AIStatus = item.NewAIStatus;
                point.VVSStatus = item.IsChanged ? 2 : 1;
                point.DisplaySN = $"{item.SerialNumber} ({item.Side}) 缺陷{item.DefectIndex}";

                var displayItem = item.CloneFor(point);
                _comparisonItems[point] = displayItem;
                _allHeatPoints.Add(point);
            }

            _filteredHeatPoints = new List<DetectInfo>(_allHeatPoints);
            _totalPages = CalculateTotalPages();
            _currentPage = 1;
            label_DetailTitle.Text = title;

            _comparisonOriginalAiFilter = "All";
            _comparisonNewAiFilter = "All";
            _comparisonChangeFilter = "All";
            comboBox_FilterOriginalAI.SelectedIndex = 0;
            comboBox_FilterNewAI.SelectedIndex = 0;
            comboBox_FilterChange.SelectedIndex = 0;
            _defectNameFilter = "All";
            PopulateDefectNameFilter();

            LoadDefectsPage(_currentPage);
        }

        /// <summary>
        /// 取消正在进行的异步图片加载任务
        /// </summary>
        private void CancelPendingImageLoads()
        {
            if (_loadCts != null)
            {
                _loadCts.Cancel();
                _loadCts.Dispose();
                _loadCts = null;
            }
        }

        private void LoadDefectsPage(int page)
        {
            // 取消上一页未完成的图片加载
            CancelPendingImageLoads();

            try
            {
                flowLayoutPanel_DefectImages.SuspendLayout();

                // Dispose all existing child controls to release Win32 window handles before clearing
                var oldControls = new System.Windows.Forms.Control[flowLayoutPanel_DefectImages.Controls.Count];
                flowLayoutPanel_DefectImages.Controls.CopyTo(oldControls, 0);
                flowLayoutPanel_DefectImages.Controls.Clear();
                foreach (var ctrl in oldControls)
                    ctrl.Dispose();

                // 重置滚动位置，避免翻页后新控件被旧的滚动偏移遮挡
                flowLayoutPanel_DefectImages.AutoScrollPosition = new Point(0, 0);

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

                _currentPage = Math.Max(1, Math.Min(page, _totalPages));
                int pageSize = NormalizePageSize(_pageSize);
                var heatPointsToShow = _filteredHeatPoints.Skip((_currentPage - 1) * pageSize).Take(pageSize).ToList();

                // 计算控件高度
                int itemHeight = flowLayoutPanel_DefectImages.ClientSize.Height - flowLayoutPanel_DefectImages.Padding.Vertical - 6;
                // 确保最小高度，避免面板尚未布局时高度为0
                if (itemHeight < 100) itemHeight = 300;
                int itemWidth = CalculateItemWidth();

                // 第一步：同步创建所有占位控件（瞬间完成，立即显示SN和状态文本）
                var itemControls = new List<UcDefectImageItem>();
                for (int i = 0; i < heatPointsToShow.Count; i++)
                {
                    var itemControl = CreateDefectImageItemControl(heatPointsToShow[i], i, itemWidth, itemHeight);
                    flowLayoutPanel_DefectImages.Controls.Add(itemControl);
                    itemControls.Add(itemControl);
                }

                if (flowLayoutPanel_DefectImages.Controls.Count > 0)
                {
                    SelectImage(0);
                }

                UpdatePaginationButtons();

                // 第二步：异步逐个加载图片（不阻塞UI）
                _loadCts = new CancellationTokenSource();
                var token = _loadCts.Token;
                LoadPageImagesAsync(itemControls, token);
            }
            finally
            {
                flowLayoutPanel_DefectImages.ResumeLayout(true);
            }
        }

        /// <summary>
        /// 异步逐个加载当前页所有控件的图片。
        /// 每个控件的图片加载完成后立即显示，不阻塞UI线程。
        /// </summary>
        private async void LoadPageImagesAsync(List<UcDefectImageItem> controls, CancellationToken token)
        {
            foreach (var ctrl in controls)
            {
                if (token.IsCancellationRequested) return;
                if (ctrl.IsDisposed) return;

                try
                {
                    await ctrl.LoadImagesAsync(token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 创建缺陷图片项控件
        /// </summary>
        private UcDefectImageItem CreateDefectImageItemControl(DetectInfo heatPoint, int index, int width, int height)
        {
            var itemControl = new UcDefectImageItem
            {
                Width = width,
                Height = height,
                Margin = new Padding(3),
                HeatPoint = heatPoint
            };

            itemControl.SetImageLayout(_largeImageMode);
            itemControl.SetDisplayImageTypes(_selectedDisplayImageTypes);
            itemControl.AdjustImageHeight(height);
            if (_comparisonMode)
            {
                itemControl.UpdateStatusText(BuildComparisonStatusText(heatPoint));
            }

            // 绑定点击事件
            itemControl.ItemClicked += (s, e) => SelectImage(index);

            // 绑定运行测试事件
            itemControl.RunTestRequested += (s, e) =>
            {
                if (_comparisonMode && e?.HeatPoint != null && _comparisonItems.TryGetValue(e.HeatPoint, out var comparisonItem))
                {
                    SingleImageTestRequested?.Invoke(this, new SingleImageTestEventArgs
                    {
                        HeatPoint = e.HeatPoint,
                        ProductSerial = comparisonItem.ProductSerial,
                        MachineId = comparisonItem.MachineId,
                        Side = comparisonItem.Side
                    });
                    return;
                }

                SingleImageTestRequested?.Invoke(this, e);
            };

            return itemControl;
        }

        private string BuildComparisonStatusText(DetectInfo heatPoint)
        {
            if (heatPoint == null || !_comparisonItems.TryGetValue(heatPoint, out var item))
                return string.Empty;

            string changedText = item.IsChanged ? "已变化" : "无变化";
            string sourceText = string.IsNullOrWhiteSpace(item.SourceText) ? string.Empty : $"  |  来源: {item.SourceText}";
            return $"旧AI: {GetStatusText(item.OriginalAIStatus)}  →  新AI: {GetStatusText(item.NewAIStatus)}  |  {changedText}{sourceText}";
        }

        private void UpdatePaginationButtons()
        {
            lblPageInfo.Text = $"第 {_currentPage}/{_totalPages} 页";
            btnPrevPage.Enabled = _currentPage > 1;
            btnNextPage.Enabled = _currentPage < _totalPages;
        }

        private void ToggleDisplayMode()
        {
            int globalIndex = GetSelectedGlobalIndex();
            _largeImageMode = !_largeImageMode;
            _pageSize = _largeImageMode ? TwoImagesPerPageSize : DefaultPageSize;
            _totalPages = CalculateTotalPages();
            UpdateDisplayModeButton();

            if (_filteredHeatPoints == null)
                return;

            if (_filteredHeatPoints.Count == 0)
            {
                LoadDefectsPage(1);
                return;
            }

            int pageSize = NormalizePageSize(_pageSize);
            int targetPage = Math.Min(_totalPages, (globalIndex / pageSize) + 1);
            int targetLocalIndex = globalIndex % pageSize;
            LoadDefectsPage(targetPage);
            SelectImage(targetLocalIndex);
        }

        private int GetSelectedGlobalIndex()
        {
            int pageSize = NormalizePageSize(_pageSize);
            int localIndex = _selectedIndex >= 0 ? _selectedIndex : 0;
            int globalIndex = ((_currentPage - 1) * pageSize) + localIndex;
            int maxIndex = (_filteredHeatPoints?.Count ?? 1) - 1;
            return Math.Max(0, Math.Min(globalIndex, maxIndex));
        }

        private void UpdateDisplayModeButton()
        {
            if (btn_DisplayMode == null) return;
            btn_DisplayMode.Text = _largeImageMode ? "小图模式" : "大图模式";
        }

        private int NormalizePageSize(int pageSize)
        {
            return Math.Max(1, pageSize);
        }

        private int CalculateTotalPages()
        {
            int count = _filteredHeatPoints?.Count ?? 0;
            if (count == 0) return 0;
            return (int)Math.Ceiling((double)count / NormalizePageSize(_pageSize));
        }

        private int CalculateItemWidth()
        {
            if (NormalizePageSize(_pageSize) != TwoImagesPerPageSize)
                return 300;

            int availableWidth = flowLayoutPanel_DefectImages.ClientSize.Width
                - flowLayoutPanel_DefectImages.Padding.Horizontal
                - SystemInformation.VerticalScrollBarWidth
                - 12;
            return Math.Max(300, availableWidth);
        }

        private void SelectImage(int index)
        {
            if (index < 0 || index >= flowLayoutPanel_DefectImages.Controls.Count)
                return;

            // Deselect old
            if (_selectedIndex >= 0 && _selectedIndex < flowLayoutPanel_DefectImages.Controls.Count)
            {
                if (flowLayoutPanel_DefectImages.Controls[_selectedIndex] is UcDefectImageItem oldItem)
                {
                    oldItem.IsSelected = false;
                }
            }

            // Select new
            _selectedIndex = index;
            if (flowLayoutPanel_DefectImages.Controls[_selectedIndex] is UcDefectImageItem newItem)
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

            int pageSize = NormalizePageSize(_pageSize);
            int globalIndex = ((_currentPage - 1) * pageSize) + _selectedIndex;
            int nextGlobalIndex = (globalIndex + 1) % _filteredHeatPoints.Count;
            if (nextGlobalIndex == 0) return;
            int nextPage = (nextGlobalIndex / pageSize) + 1;
            int nextLocalIndex = nextGlobalIndex % pageSize;

            if (nextPage != _currentPage)
            {
                LoadDefectsPage(nextPage);
            }
            SelectImage(nextLocalIndex);
        }

        private void SelectPreviousImage()
        {
            if (_filteredHeatPoints == null || _filteredHeatPoints.Count == 0) return;

            int pageSize = NormalizePageSize(_pageSize);
            int globalIndex = ((_currentPage - 1) * pageSize) + _selectedIndex;
            int prevGlobalIndex = (globalIndex - 1 + _filteredHeatPoints.Count) % _filteredHeatPoints.Count;

            int prevPage = (prevGlobalIndex / pageSize) + 1;
            int prevLocalIndex = prevGlobalIndex % pageSize;

            if (prevPage != _currentPage)
            {
                LoadDefectsPage(prevPage);
            }
            SelectImage(prevLocalIndex);
        }

        public void ClearDetails()
        {
            // 取消正在进行的异步图片加载
            CancelPendingImageLoads();

            // Dispose all existing child controls to release Win32 window handles before clearing
            var oldControls = new System.Windows.Forms.Control[flowLayoutPanel_DefectImages.Controls.Count];
            flowLayoutPanel_DefectImages.Controls.CopyTo(oldControls, 0);
            flowLayoutPanel_DefectImages.Controls.Clear();
            foreach (var ctrl in oldControls)
                ctrl.Dispose();
            label_DetailTitle.Text = "-";
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

        private void btn_Export_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 导出缺陷图片到指定目录（直接从已加载的控件中获取内存图片保存）
        /// </summary>
        /// <param name="exportPath">导出目标目录</param>
        /// <param name="exportOriginal">是否导出原图</param>
        /// <param name="exportTemplate">是否导出模板图</param>
        /// <param name="exportGerber">是否导出 Gerber 图</param>
        public void ExportImages(string exportPath, bool exportOriginal, bool exportTemplate, bool exportGerber)
        {
            if (_filteredHeatPoints == null || _filteredHeatPoints.Count == 0)
                return;

            // 三种图片类型都未选择时，跳过图片和 .dpst 文件导出（仅由调用方生成 CSV）
            if (!exportOriginal && !exportTemplate && !exportGerber)
                return;

            // 生成随机起始id，后续每张图递增
            long currentId = new Random().Next(10000000, 99999999);

            // 直接遍历所有缺陷点，从Minio加载图片，不创建UI控件（避免跨线程异常）
            foreach (var hp in _filteredHeatPoints)
            {
                if (hp == null || string.IsNullOrEmpty(hp.ImagePath))
                    continue;

                string baseName = BuildExportFileName(hp.ImagePath);

                try
                {
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(baseName);

                    // 从Minio加载原图
                    Bitmap originalImage = null;
                    if (exportOriginal)
                    {
                        originalImage = LoadImageFromMinioPath(hp.ImagePath);
                        if (originalImage != null)
                        {
                            string fileName = nameWithoutExt + "_0.png";
                            originalImage.Save(Path.Combine(exportPath, fileName), ImageFormat.Png);
                        }
                    }

                    // 从Minio加载模板图
                    Bitmap templateImage = null;
                    if (exportTemplate)
                    {
                        string templatePath = hp.TempImagePath;
                        if (!string.IsNullOrEmpty(templatePath))
                        {
                            templateImage = LoadImageFromMinioPath(templatePath);
                            if (templateImage != null)
                            {
                                string fileName = nameWithoutExt + "_1.png";
                                templateImage.Save(Path.Combine(exportPath, fileName), ImageFormat.Png);
                            }
                        }
                    }

                    // 从Minio加载Gerber图
                    Bitmap gerberImage = null;
                    if (exportGerber)
                    {
                        string gerberPath = hp.GerberImagePath;
                        if (!string.IsNullOrEmpty(gerberPath))
                        {
                            gerberImage = LoadImageFromMinioPath(gerberPath);
                            if (gerberImage != null)
                            {
                                string fileName = nameWithoutExt + "_2.png";
                                gerberImage.Save(Path.Combine(exportPath, fileName), ImageFormat.Png);
                            }
                        }
                    }

                    // 如果需要dpst但没加载原图，补充加载一次用于获取尺寸
                    if (originalImage == null && !exportOriginal)
                    {
                        originalImage = LoadImageFromMinioPath(hp.ImagePath);
                    }

                    // 导出.dpst文件（与原图同名）
                    ExportDpstFile(exportPath, baseName, currentId, originalImage,
                        exportTemplate ? templateImage : null,
                        exportGerber ? gerberImage : null);
                    currentId++;

                    // 释放临时图片
                    originalImage?.Dispose();
                    templateImage?.Dispose();
                    gerberImage?.Dispose();
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"导出图片失败: {hp.ImagePath}, {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 从Minio路径加载图片（格式：IP:objectKey），可在非UI线程调用
        /// </summary>
        private Bitmap LoadImageFromMinioPath(string minioPath)
        {
            if (string.IsNullOrEmpty(minioPath)) return null;

            var parts = minioPath.Split(':');
            if (parts.Length < 2) return null;

            string ip = parts[0];
            string objectKey = parts[1];

            using (var stream = Machine.master.MinioService.GetImageStreamSync("deepiresults", objectKey, ip))
            {
                if (stream == null || stream.Length == 0) return null;

                using (var mt = OpenCvSharp.Cv2.ImDecode(stream.ToArray(), OpenCvSharp.ImreadModes.Color))
                {
                    if (mt == null || mt.Empty()) return null;
                    return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mt);
                }
            }
        }


        /// <summary>
        /// 从Minio路径构建导出文件名
        /// </summary>
        private string BuildExportFileName(string minioPath)
        {
            int colonIndex = minioPath.IndexOf(':');
            string objectKey = colonIndex >= 0 ? minioPath.Substring(colonIndex + 1) : minioPath;
            string fileName = objectKey.Replace('/', '_').Replace('\\', '_');

            string ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext) || !ext.Equals(".png", StringComparison.OrdinalIgnoreCase))
            {
                fileName = Path.ChangeExtension(fileName, ".png");
            }

            return fileName;
        }

        /// <summary>
        /// 导出.dpst文件（与原图同名，扩展名为.dpst）
        /// </summary>
        private void ExportDpstFile(string exportPath, string baseName, long id,
            Image originalImage, Image templateImage, Image gerberImage)
        {
            try
            {
                string nameWithoutExt = Path.GetFileNameWithoutExtension(baseName);
                string dpstFileName = nameWithoutExt + ".dpst";
                string originalFileName = nameWithoutExt + "_0.png";

                int imgWidth = originalImage?.Width ?? 0;
                int imgHeight = originalImage?.Height ?? 0;

                var channels = new List<int> { GetImageChannels(originalImage) };
                var localList = new List<string> { originalFileName };
                var sourceList = new List<string> { originalFileName };

                // 如果有模板图，追加到列表
                if (templateImage != null)
                {
                    string templateFileName = nameWithoutExt + "_1.png";
                    channels.Add(GetImageChannels(templateImage));
                    localList.Add(templateFileName);
                    sourceList.Add(templateFileName);
                }

                // 如果有Gerber图，追加到列表
                if (gerberImage != null)
                {
                    string gerberFileName = nameWithoutExt + "_2.png";
                    channels.Add(GetImageChannels(gerberImage));
                    localList.Add(gerberFileName);
                    sourceList.Add(gerberFileName);
                }

                var dpst = new DpstInfo
                {
                    Channel = channels,
                    Id = id.ToString(),
                    Height = imgHeight.ToString(),
                    Width = imgWidth.ToString(),
                    Local = localList,
                    Source = sourceList
                };

                string json = JsonConvert.SerializeObject(dpst, Formatting.Indented);
                File.WriteAllText(Path.Combine(exportPath, dpstFileName), json);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"导出dpst文件失败: {baseName}, {ex.Message}");
            }
        }

        /// <summary>
        /// 获取图片通道数（彩色3，黑白1）
        /// </summary>
        private int GetImageChannels(Image image)
        {
            if (image == null) return 3;
            var pixelFormat = image.PixelFormat;
            if (pixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                return 1;
            return 3;
        }
    }

    internal class ImageTypeSelectionItem
    {
        public ImageTypeSelectionItem(DefectDisplayImageType imageType, string text)
        {
            ImageType = imageType;
            Text = text;
        }

        public DefectDisplayImageType ImageType { get; }
        public string Text { get; }

        public override string ToString()
        {
            return Text;
        }
    }

    /// <summary>
    /// AI前后结果对比显示项。
    /// </summary>
    public class AiComparisonDefectItem
    {
        public string SerialNumber { get; set; }
        public string Side { get; set; }
        public int DefectIndex { get; set; }
        public DetectInfo DetectInfo { get; set; }
        public string ProductSerial { get; set; }
        public string MachineId { get; set; }
        public int OriginalAIStatus { get; set; }
        public int NewAIStatus { get; set; }
        public string SourceText { get; set; }
        public bool IsChanged => OriginalAIStatus != NewAIStatus;

        public AiComparisonDefectItem CloneFor(DetectInfo detectInfo)
        {
            return new AiComparisonDefectItem
            {
                SerialNumber = SerialNumber,
                Side = Side,
                DefectIndex = DefectIndex,
                DetectInfo = detectInfo,
                ProductSerial = ProductSerial,
                MachineId = MachineId,
                OriginalAIStatus = OriginalAIStatus,
                NewAIStatus = NewAIStatus,
                SourceText = SourceText
            };
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
        public string ProductSerial { get; set; }
        public string MachineId { get; set; }
        public string Side { get; set; }
    }

    /// <summary>
    /// VVS状态改变事件参数
    /// </summary>
    public class VvsStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 被修改的缺陷点
        /// </summary>
        public DetectInfo ModifiedHeatPoint { get; set; }
    }
}

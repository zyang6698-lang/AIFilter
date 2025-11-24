using DeepSightModel;
using DeepSightTool;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace DeepSightAI
{
    public partial class DefectDetailControl : UserControl
    {
        private int _selectedIndex = -1;
        private List<HeatPoint> _allHeatPoints;
        private List<HeatPoint> _filteredHeatPoints; // For filtered data
        private int _currentPage = 1;
        private const int PageSize = 50; // ÿҳʾͼƬ
        private int _totalPages;
        private string _aiFilter = "All";
        private string _vvsFilter = "All";

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
                _filteredHeatPoints = _filteredHeatPoints.Where(p => p.AIStatus == _aiFilter).ToList();
            }

            if (_vvsFilter != "All")
            {
                if (_vvsFilter == "NotSet")
                {
                    _filteredHeatPoints = _filteredHeatPoints.Where(p => string.IsNullOrEmpty(p.VVSStatus)).ToList();
                }
                else
                {
                    _filteredHeatPoints = _filteredHeatPoints.Where(p => p.VVSStatus == _vvsFilter).ToList();
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

            // 添加顶部边框线
            this.panel_Pagination.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(60, 60, 60)), 0, 0, this.panel_Pagination.Width, 0);
            };

            // 居中对齐
            this.panel_Pagination.Resize += (s, e) =>
            {
                var centeredFlowPanel = this.panel_Pagination.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
                if (centeredFlowPanel != null)
                {
                    centeredFlowPanel.Left = (this.panel_Pagination.Width - centeredFlowPanel.Width) / 2;
                    centeredFlowPanel.Top = (this.panel_Pagination.Height - centeredFlowPanel.Height) / 2;
                }
            };
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
            else if (keyData == Keys.Tab)
            {
                SelectNextImage();
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
                var pictureBox = panel.Controls.OfType<PictureBox>().FirstOrDefault();
                if (pictureBox != null)
                {
                    var label = pictureBox.Controls.OfType<Label>().FirstOrDefault();
                    if (label != null)
                    {
                        var heatPoint = label.Tag as HeatPoint;
                        if (heatPoint != null)
                        {
                            if (isAiTag)
                            {
                                // AI Status is not editable
                            }
                            else
                            {
                                heatPoint.VVSStatus = tag;
                            }

                            label.Text = $"AI: {heatPoint.AIStatus}\n" +
                                         $"VVS: {heatPoint.VVSStatus}";

                            // Update border color after tagging
                            UpdatePanelAppearance(panel, true);
                        }
                    }
                }
            }
        }

        public void DisplayDefectDetails(DefectReviewItem item)
        {
            _allHeatPoints = item.HeatPoints ?? new List<HeatPoint>();
            _filteredHeatPoints = new List<HeatPoint>(_allHeatPoints); // Initialize filtered list
            _totalPages = (int)Math.Ceiling((double)_filteredHeatPoints.Count / PageSize);
            _currentPage = 1;

            label_DetailTitle.Text = $"ȱ - SN: {item.SerialNumber} ({item.Side})";

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
            var pictureBox = panel.Controls.OfType<PictureBox>().FirstOrDefault();
            if (pictureBox != null)
            {
                var label = pictureBox.Controls.OfType<Label>().FirstOrDefault();
                if (label != null && label.Tag is HeatPoint heatPoint)
                {
                    Color borderColor;
                    switch (heatPoint.VVSStatus)
                    {
                        case "VVS_OK":
                            borderColor = Color.Green;
                            break;
                        case "VVS_NG":
                            borderColor = Color.Red;
                            break;
                        default:
                            borderColor = Color.FromArgb(60, 60, 60); // Neutral border for unset status
                            break;
                    }
                    panel.BackColor = borderColor;
                    panel.Padding = new Padding(isSelected ? 5 : 2); // Thicker border for selection
                }
            }
        }

        private void SelectNextImage()
        {
            if (_filteredHeatPoints == null || _filteredHeatPoints.Count == 0) return;

            int globalIndex = ((_currentPage - 1) * PageSize) + _selectedIndex;
            int nextGlobalIndex = (globalIndex + 1) % _filteredHeatPoints.Count;

            int nextPage = (nextGlobalIndex / PageSize) + 1;
            int nextLocalIndex = nextGlobalIndex % PageSize;

            if (nextPage != _currentPage)
            {
                LoadDefectsPage(nextPage);
            }
            SelectImage(nextLocalIndex);
        }

        private Panel CreateDefectImagePanel(HeatPoint heatPoint, int index)
        {
            var panel = new Panel
            {
                Width = 300,
                Height = 300,
                Margin = new Padding(3),
                BackColor = Color.FromArgb(37, 37, 38)
            };

            // 图片显示
            var pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            pictureBox.Click += (s, e) => SelectImage(index);
            panel.Click += (s, e) => SelectImage(index);
            if (!string.IsNullOrEmpty(heatPoint.ImagePath) && File.Exists(heatPoint.ImagePath))
            {
                try
                {
                    using (var img = Image.FromFile(heatPoint.ImagePath))
                    {
                        pictureBox.Image = new Bitmap(img);
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"加载图片失败: {heatPoint.ImagePath}, {ex.Message}");
                }
            }

            panel.Controls.Add(pictureBox);

            // 信息显示
            var infoLabel = new Label
            {
                Text = $"AI: {heatPoint.AIStatus}\n" +
                       $"VVS: {heatPoint.VVSStatus}",
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

            pictureBox.Controls.Add(infoLabel);

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

        public List<HeatPoint> GetHeatPoints()
        {
            return _allHeatPoints ?? new List<HeatPoint>();
        }
    }
}

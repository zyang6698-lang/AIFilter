using DeepSightDB;
using DeepSightModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// 缺陷图片详情弹窗 - 双击图片时显示放大图和详细信息
    /// </summary>
    public partial class FrmDefectImageDetail : Form
    {
        private readonly DetectInfo _detectInfo;
        private readonly Image _originalImage;
        private readonly Image _templateImage;
        private readonly Image _defectBoxImage;
        private readonly Image _aviImage;
        private readonly List<ImageTab> _selectedTabs = new List<ImageTab>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="detectInfo">缺陷信息</param>
        /// <param name="originalImage">原图（不带缺陷框）</param>
        /// <param name="templateImage">模板图</param>
        /// <param name="defectBoxImage">带缺陷框的图</param>
        /// <param name="aviImage">AVI 图</param>
        public FrmDefectImageDetail(DetectInfo detectInfo, Image originalImage, Image templateImage, Image defectBoxImage, Image aviImage = null)
        {
            InitializeComponent();
            dataGridView_Info.ApplyDarkTheme();

            _detectInfo = detectInfo;
            // Clone images so this form owns its own copies
            _originalImage = (Image)originalImage?.Clone();
            _templateImage = (Image)templateImage?.Clone();
            _defectBoxImage = (Image)defectBoxImage?.Clone();
            _aviImage = (Image)aviImage?.Clone();

            button_OriginalImage.Click += (s, e) => ToggleImage(ImageTab.Original);
            button_TemplateImage.Click += (s, e) => ToggleImage(ImageTab.Template);
            button_DefectBoxImage.Click += (s, e) => ToggleImage(ImageTab.DefectBox);
            button_AviImage.Click += (s, e) => ToggleImage(ImageTab.Avi);

            // 任意点击关闭（图片区域、空白区域等）
            pictureBox_Image.Click += (s, e) => this.Close();
            pictureBox_SecondImage.Click += (s, e) => this.Close();
            tableLayoutPanel_Images.Click += (s, e) => this.Close();
            splitContainer_Main.Panel1.Click += (s, e) => this.Close();
            this.Click += (s, e) => this.Close();
            // ESC 也可关闭
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
            // 滚轮切换图片
            pictureBox_Image.MouseWheel += PictureBox_MouseWheel;
            pictureBox_SecondImage.MouseWheel += PictureBox_MouseWheel;
            tableLayoutPanel_Images.MouseWheel += PictureBox_MouseWheel;
            this.MouseWheel += PictureBox_MouseWheel;

            this.Load += (s, e) =>
            {
                // 窗体最大化后按比例设置分割位置（左侧图片占70%）
                splitContainer_Main.SplitterDistance = (int)(this.ClientSize.Width * 0.7);
            };

            LoadInfo();
            InitializeImageSelection();
        }

        private enum ImageTab { Original, Template, DefectBox, Avi }

        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                RotateSelection(-1);
            else if (e.Delta < 0)
                RotateSelection(1);
        }

        private void InitializeImageSelection()
        {
            _selectedTabs.Clear();
            var preferredTabs = new[] { ImageTab.DefectBox, ImageTab.Template, ImageTab.Original, ImageTab.Avi };
            foreach (var tab in preferredTabs)
            {
                if (!HasImage(tab)) continue;
                _selectedTabs.Add(tab);
                if (_selectedTabs.Count == 2) break;
            }

            RefreshImageDisplay();
        }

        private void ToggleImage(ImageTab tab)
        {
            if (!HasImage(tab)) return;

            if (_selectedTabs.Contains(tab))
            {
                return;
            }
            else
            {
                _selectedTabs.Add(tab);
                while (_selectedTabs.Count > 2)
                {
                    _selectedTabs.RemoveAt(0);
                }
            }

            RefreshImageDisplay();
        }

        private void RotateSelection(int offset)
        {
            var availableTabs = GetAvailableTabs();
            if (availableTabs.Count <= 2)
            {
                _selectedTabs.Clear();
                _selectedTabs.AddRange(availableTabs);
                RefreshImageDisplay();
                return;
            }

            int currentIndex = _selectedTabs.Count > 0 ? availableTabs.IndexOf(_selectedTabs[0]) : 0;
            if (currentIndex < 0) currentIndex = 0;

            int startIndex = (currentIndex + offset) % availableTabs.Count;
            if (startIndex < 0) startIndex += availableTabs.Count;

            _selectedTabs.Clear();
            _selectedTabs.Add(availableTabs[startIndex]);
            _selectedTabs.Add(availableTabs[(startIndex + 1) % availableTabs.Count]);
            RefreshImageDisplay();
        }

        private void RefreshImageDisplay()
        {
            var activeColor = Color.FromArgb(0, 122, 204);
            var inactiveColor = Color.FromArgb(60, 60, 65);
            var disabledColor = Color.FromArgb(45, 45, 48);

            UpdateButton(button_OriginalImage, ImageTab.Original, activeColor, inactiveColor, disabledColor);
            UpdateButton(button_TemplateImage, ImageTab.Template, activeColor, inactiveColor, disabledColor);
            UpdateButton(button_DefectBoxImage, ImageTab.DefectBox, activeColor, inactiveColor, disabledColor);
            UpdateButton(button_AviImage, ImageTab.Avi, activeColor, inactiveColor, disabledColor);

            pictureBox_Image.Image = _selectedTabs.Count > 0 ? GetImage(_selectedTabs[0]) : null;
            pictureBox_SecondImage.Image = _selectedTabs.Count > 1 ? GetImage(_selectedTabs[1]) : null;
        }

        private void UpdateButton(Button button, ImageTab tab, Color activeColor, Color inactiveColor, Color disabledColor)
        {
            bool enabled = HasImage(tab);
            bool selected = _selectedTabs.Contains(tab);
            button.Enabled = enabled;
            button.BackColor = !enabled ? disabledColor : selected ? activeColor : inactiveColor;
            button.ForeColor = !enabled ? Color.Gray : selected ? Color.White : Color.Silver;
        }

        private bool HasImage(ImageTab tab)
        {
            return GetImage(tab) != null;
        }

        private List<ImageTab> GetAvailableTabs()
        {
            var tabs = new List<ImageTab> { ImageTab.Original, ImageTab.Template, ImageTab.DefectBox, ImageTab.Avi };
            tabs.RemoveAll(tab => !HasImage(tab));
            return tabs;
        }

        private Image GetImage(ImageTab tab)
        {
            switch (tab)
            {
                case ImageTab.Original:
                    return _originalImage;
                case ImageTab.Template:
                    return _templateImage;
                case ImageTab.DefectBox:
                    return _defectBoxImage;
                case ImageTab.Avi:
                    return _aviImage;
                default:
                    return null;
            }
        }

        private void LoadInfo()
        {
            if (_detectInfo == null) return;

            var rows = dataGridView_Info.Rows;

            AddRow(rows, "显示SN", _detectInfo.DisplaySN);
            AddRow(rows, "缺陷名称", _detectInfo.DefectName);
            AddRow(rows, "缺陷类型", _detectInfo.DefectType);
            AddRow(rows, "缺陷形状", _detectInfo.DefectShape);
            AddRow(rows, "ROI X", _detectInfo.RoiX.ToString());
            AddRow(rows, "ROI Y", _detectInfo.RoiY.ToString());
            AddRow(rows, "宽度", _detectInfo.Width.ToString());
            AddRow(rows, "高度", _detectInfo.Height.ToString());
            AddRow(rows, "原始 ROI X", _detectInfo.OriginRoiX.ToString());
            AddRow(rows, "原始 ROI Y", _detectInfo.OriginRoiY.ToString());
            AddRow(rows, "原始宽度", _detectInfo.OriginWidth.ToString());
            AddRow(rows, "原始高度", _detectInfo.OriginHeight.ToString());
            AddRow(rows, "图片路径", _detectInfo.ImagePath);
            AddRow(rows, "AI 状态", GetStatusText(_detectInfo.AIStatus));
            AddRow(rows, "VVS 状态", GetStatusText(_detectInfo.VVSStatus));
            AddRow(rows, "VRS 状态", GetStatusText(_detectInfo.VrsState));
            AddRow(rows, "最终状态", GetStatusText(_detectInfo.FinalState));

            // Image basic info
            if (_originalImage != null)
            {
                AddRow(rows, "图片宽度 (px)", _originalImage.Width.ToString());
                AddRow(rows, "图片高度 (px)", _originalImage.Height.ToString());
                AddRow(rows, "图片格式", _originalImage.RawFormat.ToString());
            }

            // 解析判别依据 (DrawInfo)
            LoadDrawInfo(rows);
        }

        /// <summary>
        /// 解析 DrawInfo JSON 并以可读方式展示判别依据
        /// </summary>
        private void LoadDrawInfo(DataGridViewRowCollection rows)
        {
            if (string.IsNullOrEmpty(_detectInfo.DrawInfo)) return;

            try
            {
                var drawInfoList = JsonConvert.DeserializeObject<List<DrawInfo>>(_detectInfo.DrawInfo);
                if (drawInfoList == null || drawInfoList.Count == 0) return;

                AddRow(rows, "── 判别依据 ──", "");

                for (int i = 0; i < drawInfoList.Count; i++)
                {
                    var info = drawInfoList[i];
                    string prefix = drawInfoList.Count > 1 ? $"[{i + 1}] " : "";

                    if (!string.IsNullOrEmpty(info.Name))
                        AddRow(rows, $"{prefix}检测名称", info.Name);
                    if (!string.IsNullOrEmpty(info.InspectName))
                        AddRow(rows, $"{prefix}检测项", info.InspectName);
                    if (!string.IsNullOrEmpty(info.InspectLabel))
                        AddRow(rows, $"{prefix}检测标签", info.InspectLabel);
                    if (info.Roi != null && info.Roi.Count >= 4)
                        AddRow(rows, $"{prefix}检测ROI", $"X={info.Roi[0]}, Y={info.Roi[1]}, W={info.Roi[2]}, H={info.Roi[3]}");
                    if (info.DefectRois != null && info.DefectRois.Count > 0)
                    {
                        for (int j = 0; j < info.DefectRois.Count; j++)
                        {
                            var roi = info.DefectRois[j];
                            if (roi != null && roi.Count >= 4)
                                AddRow(rows, $"{prefix}缺陷ROI-{j + 1}", $"X={roi[0]}, Y={roi[1]}, W={roi[2]}, H={roi[3]}");
                        }
                    }

                    // 判别条件
                    if (info.Conditions != null)
                    {
                        foreach (var cond in info.Conditions)
                        {
                            string thresholdStr = "";
                            if (cond.Threshold != null)
                            {
                                var parts = new List<string>();
                                if (cond.Threshold.Min.HasValue) parts.Add($"Min={cond.Threshold.Min.Value}");
                                if (cond.Threshold.Max.HasValue) parts.Add($"Max={cond.Threshold.Max.Value}");
                                thresholdStr = string.Join(", ", parts);
                            }
                            string unit = !string.IsNullOrEmpty(cond.Unit) ? $" {cond.Unit}" : "";
                            string valueStr = cond.Value.HasValue ? cond.Value.Value.ToString("F2") : "-";
                            AddRow(rows, $"{prefix}条件: {cond.Name}", $"值={valueStr}{unit}  阈值=[{thresholdStr}]");
                        }
                    }
                }
            }
            catch
            {
                // 解析失败时显示原始JSON
                AddRow(rows, "判别依据(原始)", _detectInfo.DrawInfo);
            }
        }

        private void AddRow(DataGridViewRowCollection rows, string property, string value)
        {
            rows.Add(property, value ?? "-");
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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            pictureBox_Image.Image = null;
            pictureBox_SecondImage.Image = null;
            _originalImage?.Dispose();
            _templateImage?.Dispose();
            _defectBoxImage?.Dispose();
            _aviImage?.Dispose();
            base.OnFormClosed(e);
        }
    }
}


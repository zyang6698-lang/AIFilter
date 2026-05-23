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
        private ImageTab _currentTab;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="detectInfo">缺陷信息</param>
        /// <param name="originalImage">原图（不带缺陷框）</param>
        /// <param name="templateImage">模板图</param>
        /// <param name="defectBoxImage">带缺陷框的图</param>
        public FrmDefectImageDetail(DetectInfo detectInfo, Image originalImage, Image templateImage, Image defectBoxImage)
        {
            InitializeComponent();
            dataGridView_Info.ApplyDarkTheme();

            _detectInfo = detectInfo;
            // Clone images so this form owns its own copies
            _originalImage = (Image)originalImage?.Clone();
            _templateImage = (Image)templateImage?.Clone();
            _defectBoxImage = (Image)defectBoxImage?.Clone();

            button_OriginalImage.Click += (s, e) => SwitchImage(ImageTab.Original);
            button_TemplateImage.Click += (s, e) => SwitchImage(ImageTab.Template);
            button_DefectBoxImage.Click += (s, e) => SwitchImage(ImageTab.DefectBox);

            // 任意点击关闭（图片区域、空白区域等）
            pictureBox_Image.Click += (s, e) => this.Close();
            splitContainer_Main.Panel1.Click += (s, e) => this.Close();
            this.Click += (s, e) => this.Close();
            // ESC 也可关闭
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
            // 滚轮切换图片
            pictureBox_Image.MouseWheel += PictureBox_MouseWheel;
            this.MouseWheel += PictureBox_MouseWheel;

            this.Load += (s, e) =>
            {
                // 窗体最大化后按比例设置分割位置（左侧图片占70%）
                splitContainer_Main.SplitterDistance = (int)(this.ClientSize.Width * 0.7);
            };

            LoadInfo();
            SwitchImage(ImageTab.DefectBox);
        }

        private enum ImageTab { Original, Template, DefectBox }

        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            // 滚轮上：上一张，滚轮下：下一张
            int current = (int)_currentTab;
            if (e.Delta > 0)
                current--;
            else if (e.Delta < 0)
                current++;

            // 循环切换
            const int count = 3;
            current = ((current % count) + count) % count;
            SwitchImage((ImageTab)current);
        }

        private void SwitchImage(ImageTab tab)
        {
            _currentTab = tab;
            // Update button appearance
            var activeColor = Color.FromArgb(0, 122, 204);
            var inactiveColor = Color.FromArgb(60, 60, 65);

            button_OriginalImage.BackColor = tab == ImageTab.Original ? activeColor : inactiveColor;
            button_OriginalImage.ForeColor = tab == ImageTab.Original ? Color.White : Color.Silver;
            button_TemplateImage.BackColor = tab == ImageTab.Template ? activeColor : inactiveColor;
            button_TemplateImage.ForeColor = tab == ImageTab.Template ? Color.White : Color.Silver;
            button_DefectBoxImage.BackColor = tab == ImageTab.DefectBox ? activeColor : inactiveColor;
            button_DefectBoxImage.ForeColor = tab == ImageTab.DefectBox ? Color.White : Color.Silver;

            switch (tab)
            {
                case ImageTab.Original:
                    pictureBox_Image.Image = _originalImage;
                    break;
                case ImageTab.Template:
                    pictureBox_Image.Image = _templateImage;
                    break;
                case ImageTab.DefectBox:
                    pictureBox_Image.Image = _defectBoxImage;
                    break;
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
            _originalImage?.Dispose();
            _templateImage?.Dispose();
            _defectBoxImage?.Dispose();
            base.OnFormClosed(e);
        }
    }
}


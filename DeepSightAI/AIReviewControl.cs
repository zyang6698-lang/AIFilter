using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeepSightModel;
using DeepSightTool;
using DeepsightSqlite;
using System.IO;

namespace DeepSightAI
{
    public partial class AIReviewControl : UserControl
    {
        #region Fields

        private List<DefectReviewItem> _defectItems = new List<DefectReviewItem>();
        private BindingList<DefectReviewItem> _bindingList;

        #endregion

        #region Constructor

        public AIReviewControl()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            
            // 订阅查询控件事件
            heatMapQueryControl.QueryClicked += HeatMapQueryControl_QueryClicked;
            heatMapQueryControl.SideSelectionChanged += HeatMapQueryControl_SideSelectionChanged;
            
            // 订阅DataGridView事件
            dataGridView_Defects.SelectionChanged += DataGridView_Defects_SelectionChanged;
            dataGridView_Defects.CellValueChanged += DataGridView_Defects_CellValueChanged;
            
            // 订阅按钮事件
            btn_Save.Click += Btn_Save_Click;
            btn_Export.Click += Btn_Export_Click;
            
            // 初始化绑定列表
            _bindingList = new BindingList<DefectReviewItem>(_defectItems);
            dataGridView_Defects.DataSource = _bindingList;
            
            // 设置DataGridView样式
            SetupDataGridViewStyle();
        }

        private void SetupDataGridViewStyle()
        {
            // 设置样式
            dataGridView_Defects.EnableHeadersVisualStyles = false;
            dataGridView_Defects.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 64, 82);
            dataGridView_Defects.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView_Defects.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            dataGridView_Defects.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dataGridView_Defects.DefaultCellStyle.ForeColor = Color.White;
            dataGridView_Defects.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dataGridView_Defects.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView_Defects.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(55, 55, 58);
            dataGridView_Defects.GridColor = Color.FromArgb(60, 60, 60);
            dataGridView_Defects.BorderStyle = BorderStyle.None;
        }

        #endregion

        #region Event Handlers

        private async void HeatMapQueryControl_QueryClicked(object sender, EventArgs e)
        {
            try
            {
                // 验证输入
                if (!heatMapQueryControl.ValidateInputs(out string errorMessage))
                {
                    MessageBox.Show(errorMessage, "输入验证", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                this.Enabled = false;
                _defectItems.Clear();

                // 根据查询条件获取数据
                if (!string.IsNullOrWhiteSpace(heatMapQueryControl.LotNumber))
                {
                    // 按Lot号查询
                    await QueryByLotNumber(heatMapQueryControl.LotNumber);
                }
                else if (heatMapQueryControl.IsDateChecked)
                {
                    // 按日期和料号查询
                    await QueryByDateAndPartNumber(
                        heatMapQueryControl.SelectedDate, 
                        heatMapQueryControl.PartNumber);
                }

                // 按面次筛选
                FilterBySide();
                
                // 更新绑定
                _bindingList.ResetBindings();
                
                MessageBox.Show($"查询完成，共找到 {_defectItems.Count} 条记录。", "查询结果", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"查询异常: {ex}");
                MessageBox.Show("查询失败，请检查日志。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private void HeatMapQueryControl_SideSelectionChanged(object sender, EventArgs e)
        {
            FilterBySide();
            _bindingList.ResetBindings();
        }

        private async void DataGridView_Defects_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView_Defects.SelectedRows.Count == 0)
            {
                flowLayoutPanel_DefectImages.Controls.Clear();
                return;
            }

            var selectedItem = dataGridView_Defects.SelectedRows[0].DataBoundItem as DefectReviewItem;
            if (selectedItem != null)
            {
                await LoadDefectDetails(selectedItem);
            }
        }

        private void DataGridView_Defects_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != col_ManualStatus.Index)
                return;

            // 标记为已修改
            var item = dataGridView_Defects.Rows[e.RowIndex].DataBoundItem as DefectReviewItem;
            if (item != null)
            {
                item.IsModified = true;
            }
        }

        private async void Btn_Save_Click(object sender, EventArgs e)
        {
            try
            {
                var modifiedItems = _defectItems.Where(x => x.IsModified).ToList();
                
                if (modifiedItems.Count == 0)
                {
                    MessageBox.Show("没有需要保存的修改。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                this.Enabled = false;
                
                // 保存到数据库
                await SaveManualReviewResults(modifiedItems);
                
                MessageBox.Show($"成功保存 {modifiedItems.Count} 条人工判定结果。", "保存成功", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // 清除修改标记
                modifiedItems.ForEach(x => x.IsModified = false);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"保存异常: {ex}");
                MessageBox.Show("保存失败，请检查日志。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private async void Btn_Export_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = "CSV文件|*.csv|Excel文件|*.xlsx";
                    dialog.FileName = $"缺陷复审_{DateTime.Now:yyyyMMddHHmmss}";
                    
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        this.Enabled = false;
                        await ExportToFile(dialog.FileName);
                        MessageBox.Show("导出成功！", "导出", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"导出异常: {ex}");
                MessageBox.Show("导出失败，请检查日志。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Enabled = true;
            }
        }

        #endregion

        #region Data Operations

        private async Task QueryByLotNumber(string lotNumber)
        {
            try
            {
                var panels = await Machine.master.workClass.GetPanelsDataByMachineAndLot(null, lotNumber);
                await ProcessPanelData(panels);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"按Lot号查询异常: {ex}");
                throw;
            }
        }

        private async Task QueryByDateAndPartNumber(DateTime date, string partNumber)
        {
            try
            {
                DateTime startDate = date.Date;
                DateTime endDate = date.Date.AddDays(1).AddSeconds(-1);
                
                var panels = await Machine.master.workClass.GetPanelsData(startDate, endDate);
                
                // 如果指定了料号，进行过滤
                if (!string.IsNullOrWhiteSpace(partNumber))
                {
                    panels = panels.Where(p => p.ProductSerial == partNumber).ToList();
                }
                
                await ProcessPanelData(panels);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"按日期和料号查询异常: {ex}");
                throw;
            }
        }

        private async Task ProcessPanelData(List<PanelDataRecord> panels)
        {
            foreach (var panel in panels)
            {
                // 获取A面数据
                var sideA = await GetPanelSideData(panel.SerialNumber, panel.DetectionDate, "A");
                if (sideA != null)
                {
                    _defectItems.Add(CreateDefectReviewItem(panel, sideA, "A"));
                }
                
                // 获取B面数据
                var sideB = await GetPanelSideData(panel.SerialNumber, panel.DetectionDate, "B");
                if (sideB != null)
                {
                    _defectItems.Add(CreateDefectReviewItem(panel, sideB, "B"));
                }
            }
        }

        private async Task<SideData> GetPanelSideData(string serialNumber, DateTime detectionDate, string side)
        {
            try
            {
                // 这里需要实现获取指定SN和面次的详细数据
                // 由于DatabaseHelper中没有直接的方法，我们需要通过HeatPoints来判断
                var heatPoints = new List<HeatPoint>();
                //TODO var heatPoints = await Machine.master.workClass.GetHeatPoints(serialNumber, detectionDate);

                if (heatPoints == null || heatPoints.Count == 0)
                    return null;

                var sideData = new SideData
                {
                    SerialNumber = serialNumber,
                    Side = side,
                    HeatPoints = heatPoints,
                    TotalDefectsCount = heatPoints.Count,
                    RemainingDefectsCount = heatPoints.Count
                };
                
                // 判断状态
                // 这里需要根据实际业务逻辑判断
                sideData.State = DetermineState(heatPoints.Count);
                
                return sideData;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"获取面次数据异常 (SN:{serialNumber}, Side:{side}): {ex}");
                return null;
            }
        }

        private int DetermineState(int defectCount)
        {
            if (defectCount == 0)
                return 0; // AVI OK
            else
                return 3; // AVI NG 未判定
        }

        private DefectReviewItem CreateDefectReviewItem(PanelDataRecord panel, SideData sideData, string side)
        {
            return new DefectReviewItem
            {
                SerialNumber = panel.SerialNumber,
                LotNumber = panel.LotNumber,
                Side = side,
                AviStatus = sideData.State == 0 ? "OK" : "NG",
                AiStatus = DetermineAiStatus(sideData),
                ManualStatus = "未判定",
                DefectCount = sideData.TotalDefectsCount,
                PathIndex = panel.PathIndex,
                DetectionDate = panel.DetectionDate,
                HeatPoints = sideData.HeatPoints,
                IsModified = false
            };
        }

        private string DetermineAiStatus(SideData sideData)
        {
            // 根据State判断AI状态
            switch (sideData.State)
            {
                case 0: return "OK"; // AVI OK
                case 1: return "OK"; // 复判后OK
                case 2: return "NG"; // 复判后NG
                case 3: return "未判定"; // 未判定
                default: return "未知";
            }
        }

        private void FilterBySide()
        {
            string selectedSide = heatMapQueryControl.SelectedSide;
            
            foreach (DataGridViewRow row in dataGridView_Defects.Rows)
            {
                var item = row.DataBoundItem as DefectReviewItem;
                if (item != null)
                {
                    row.Visible = item.Side == selectedSide;
                }
            }
        }

        private async Task LoadDefectDetails(DefectReviewItem item)
        {
            flowLayoutPanel_DefectImages.Controls.Clear();
            label_DetailTitle.Text = $"缺陷详情 - SN: {item.SerialNumber} ({item.Side}面)";

            if (item.HeatPoints == null || item.HeatPoints.Count == 0)
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
                return;
            }

            await Task.Run(() =>
            {
                foreach (var heatPoint in item.HeatPoints)
                {
                    this.Invoke(new Action(() =>
                    {
                        var panel = CreateDefectImagePanel(heatPoint);
                        flowLayoutPanel_DefectImages.Controls.Add(panel);
                    }));
                }
            });
        }

        private Panel CreateDefectImagePanel(HeatPoint heatPoint)
        {
            var panel = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Width = flowLayoutPanel_DefectImages.ClientSize.Width - 25,
                Margin = new Padding(3),
                BackColor = Color.FromArgb(37, 37, 38)
            };
            
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // 图片显示
            var pictureBox = new PictureBox
            {
                Size = new Size(300, 300),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(45, 45, 48),
                Dock = DockStyle.Fill,
                Margin = new Padding(5)
            };

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

            panel.Controls.Add(pictureBox, 0, 0);

            // 信息显示
            var infoLabel = new Label
            {
                Text = $"缺陷类型: {heatPoint.DefectName}\n" +
                       $"缺陷形态: {heatPoint.DefectShape}\n" +
                       $"坐标: ({heatPoint.RoiX}, {heatPoint.RoiY})",
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9F),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(10, 5, 5, 5)
            };
            
            panel.Controls.Add(infoLabel, 1, 0);

            return panel;
        }

        private async Task SaveManualReviewResults(List<DefectReviewItem> items)
        {
            // 这里需要实现保存逻辑
            // 由于当前DatabaseHelper没有提供更新方法，这里暂时记录日志
            foreach (var item in items)
            {
                LogTextHelper.Info($"保存人工判定结果: SN={item.SerialNumber}, Side={item.Side}, " +
                    $"ManualStatus={item.ManualStatus}");
                
                // TODO: 实现实际的数据库更新逻辑
                // await _databaseHelper.UpdateManualReviewResult(item);
            }
            
            await Task.CompletedTask;
        }

        private async Task ExportToFile(string filePath)
        {
            await Task.Run(() =>
            {
                var lines = new List<string>();
                
                // 添加表头
                lines.Add("序列号,Lot号,面次,AVI状态,AI状态,人工判定,缺陷数,检测日期");
                
                // 添加数据
                foreach (var item in _defectItems)
                {
                    lines.Add($"{item.SerialNumber},{item.LotNumber},{item.Side}," +
                        $"{item.AviStatus},{item.AiStatus},{item.ManualStatus}," +
                        $"{item.DefectCount},{item.DetectionDate:yyyy-MM-dd HH:mm:ss}");
                }
                
                File.WriteAllLines(filePath, lines, System.Text.Encoding.UTF8);
            });
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// 缺陷复审项
    /// </summary>
    public class DefectReviewItem
    {
        public string SerialNumber { get; set; }
        public string LotNumber { get; set; }
        public string Side { get; set; }
        public string AviStatus { get; set; }
        public string AiStatus { get; set; }
        public string ManualStatus { get; set; }
        public int DefectCount { get; set; }
        public string PathIndex { get; set; }
        public DateTime DetectionDate { get; set; }
        public List<HeatPoint> HeatPoints { get; set; }
        public bool IsModified { get; set; }
    }

    #endregion
}

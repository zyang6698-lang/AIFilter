using DeepSightModel;
using DeepsightSqlite;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class AIReviewControl : UserControl
    {
        #region Fields

        private List<DefectReviewItem> _defectItems = new List<DefectReviewItem>();
        private SortableBindingList<DefectReviewItem> _bindingList;

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
            QueryControl.QueryClicked += HeatMapQueryControl_QueryClicked;

            // 订阅DataGridView事件
            dataGridView_Defects.CellDoubleClick += DataGridView_Defects_CellDoubleClick;
            dataGridView_Defects.CellValueChanged += DataGridView_Defects_CellValueChanged;

            // 订阅详情控件的切换下一行事件
            defectDetailControl1.SelectNextRowRequested += DefectDetailControl_SelectNextRowRequested;

            // 初始化绑定列表（使用支持排序的SortableBindingList）
            _bindingList = new SortableBindingList<DefectReviewItem>(_defectItems);
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
                if (!QueryControl.ValidateInputs(out string errorMessage))
                {
                    MessageBox.Show(errorMessage, "输入验证", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                this.Enabled = false;
                _defectItems.Clear();
                defectDetailControl1.ClearDetails();

                foreach (var panel in QueryControl.GetQueryResult())
                {
                    // 可能不存在匹配的面，需防御处理
                    var side = panel.Sides != null && panel.Sides.Count > 0 ? panel.Sides[0] : null;
                    if (side == null)
                    {
                        // 无匹配面，跳过该panel
                        continue;
                    }
                    _defectItems.Add(CreateDefectReviewItem(panel, side));
                }

                _bindingList = new SortableBindingList<DefectReviewItem>(_defectItems);
                dataGridView_Defects.DataSource = _bindingList;
                _bindingList.ResetBindings();


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

        private void DataGridView_Defects_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // 忽略双击列头
            if (e.RowIndex < 0)
                return;

            var selectedItem = dataGridView_Defects.Rows[e.RowIndex].DataBoundItem as DefectReviewItem;
            if (selectedItem != null)
            {
                defectDetailControl1.DisplayDefectDetails(selectedItem);
                tabControl_Main.SelectedTab = tabPage_Details;
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

        private void DefectDetailControl_SelectNextRowRequested(object sender, EventArgs e)
        {
            // 切换到表格的下一行
            if (dataGridView_Defects.Rows.Count == 0)
                return;

            int currentRowIndex = dataGridView_Defects.CurrentCell?.RowIndex ?? -1;
            int nextRowIndex = (currentRowIndex + 1) % dataGridView_Defects.Rows.Count;

            // 选中下一行并显示详情
            dataGridView_Defects.ClearSelection();
            dataGridView_Defects.Rows[nextRowIndex].Selected = true;
            dataGridView_Defects.CurrentCell = dataGridView_Defects.Rows[nextRowIndex].Cells[0];

            var selectedItem = dataGridView_Defects.Rows[nextRowIndex].DataBoundItem as DefectReviewItem;
            if (selectedItem != null)
            {
                defectDetailControl1.DisplayDefectDetails(selectedItem);
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
                if (tabControl_Main.SelectedTab == tabPage_Details)
                {
                    // 获取当前选中的行
                    DefectReviewItem currentItem = null;
                    if (dataGridView_Defects.CurrentCell != null && dataGridView_Defects.CurrentCell.RowIndex >= 0)
                    {
                        currentItem = dataGridView_Defects.Rows[dataGridView_Defects.CurrentCell.RowIndex].DataBoundItem as DefectReviewItem;
                    }

                    if (currentItem == null)
                    {
                        MessageBox.Show("请先选择一条记录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    using (var dialog = new FolderBrowserDialog())
                    {
                        dialog.Description = "请选择要导出图片的文件夹";

                        // 构造默认导出路径：以 MaterialLocation 为基础，去掉末尾的 TemplateImages，追加 AIReview
                        try
                        {
                            var materialLocation = Machine.solconfig.MaterialLocation;
                            if (!string.IsNullOrWhiteSpace(materialLocation))
                            {
                                string baseDir = materialLocation;
                                // 如果路径存在并以 TemplateImages 结尾，使用其父目录
                                if (materialLocation.EndsWith("TemplateImages", StringComparison.OrdinalIgnoreCase))
                                {
                                    var parent = Path.GetDirectoryName(materialLocation);
                                    if (!string.IsNullOrEmpty(parent))
                                    {
                                        baseDir = parent;
                                    }
                                }

                                var defaultExportDir = Path.Combine(baseDir, "AIReview");
                                // 确保目录存在，以便作为对话框默认选中路径
                                if (!Directory.Exists(defaultExportDir))
                                {
                                    Directory.CreateDirectory(defaultExportDir);
                                }
                                dialog.SelectedPath = defaultExportDir;
                            }
                        }
                        catch (Exception exDefault)
                        {
                            LogTextHelper.Warn($"设置默认导出路径失败: {exDefault.Message}");
                        }

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            this.Enabled = false;
                            var filteredHeatPoints = defectDetailControl1.GetFilteredHeatPoints();
                            var (aiFilter, vvsFilter) = defectDetailControl1.GetFilters();

                            // 以SN命名子文件夹
                            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                            string snFolderName = $"{currentItem.SerialNumber}_{currentItem.Side}_{timestamp}";
                            string exportPath = Path.Combine(dialog.SelectedPath, snFolderName);
                            Directory.CreateDirectory(exportPath);

                            await Task.Run(() =>
                            {
                                // 导出图片
                                foreach (var heatPoint in filteredHeatPoints)
                                {
                                    if (!string.IsNullOrEmpty(heatPoint.ImagePath) && File.Exists(heatPoint.ImagePath))
                                    {
                                        try
                                        {
                                            var destFileName = Path.GetFileName(heatPoint.ImagePath);
                                            File.Copy(heatPoint.ImagePath, Path.Combine(exportPath, destFileName), true);
                                        }
                                        catch (Exception ex)
                                        {
                                            LogTextHelper.Error($"复制图片失败: {heatPoint.ImagePath}, {ex.Message}");
                                        }
                                    }
                                }

                                // 导出表格信息到CSV
                                var csvPath = Path.Combine(exportPath, $"{currentItem.SerialNumber}_{currentItem.Side}_info.csv");
                                var csvLines = new List<string>
                                {
                                    "序列号,Lot号,机台号,料号,面次,AVI状态,AI状态,人工判定,缺陷数,检测日期",
                                    $"{currentItem.SerialNumber},{currentItem.LotNumber},{currentItem.MachineId},{currentItem.ProductSerial}," +
                                    $"{currentItem.Side},{currentItem.AviStatus},{currentItem.AiStatus},{currentItem.ManualStatus}," +
                                    $"{currentItem.DefectCount},{currentItem.DetectionDate:yyyy-MM-dd HH:mm:ss}"
                                };

                                // 添加缺陷点详情
                                csvLines.Add("");
                                csvLines.Add("缺陷点详情");
                                csvLines.Add("图片路径,AI状态,VVS状态");
                                foreach (var hp in filteredHeatPoints)
                                {
                                    csvLines.Add($"{hp.ImagePath},{hp.AIStatus},{hp.VVSStatus}");
                                }

                                File.WriteAllLines(csvPath, csvLines, System.Text.Encoding.UTF8);
                            });

                            this.Enabled = true;
                            MessageBox.Show($"导出完成。\n导出路径: {exportPath}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
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

        private async void Btn_LoadImages_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    var imagePaths = Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith(".jpg") || s.EndsWith(".png") || s.EndsWith(".bmp"))
                        .ToList();

                    if (imagePaths.Count > 0)
                    {
                        _defectItems.Clear();
                        var defectItem = new DefectReviewItem
                        {
                            SerialNumber = "LOCAL_FILES",
                            Side = "A",
                            HeatPoints = imagePaths.Select(p => new HeatPoint { ImagePath = p }).ToList()
                        };
                        _defectItems.Add(defectItem);
                        _bindingList.ResetBindings();
                        dataGridView_Defects.Rows[0].Selected = true;
                        defectDetailControl1.DisplayDefectDetails(defectItem);
                        tabControl_Main.SelectedTab = tabPage_Details;
                    }
                }
            }
        }

        #endregion

        #region Data Operations


        private DefectReviewItem CreateDefectReviewItem(PanelDataRecord panel, SideData sideData)
        {
            return new DefectReviewItem
            {
                SerialNumber = panel.SerialNumber,
                LotNumber = panel.LotNumber,
                MachineId = panel.MachineId,
                ProductSerial = panel.ProductSerial,
                Side = sideData.Side,
                AviStatus = sideData.AviState == 1 ? "OK" : "NG",
                AiStatus =sideData.AiState==1?"OK":"NG",
                ManualStatus = "未判定",
                DefectCount = sideData.TotalDefectsCount,
                PathIndex = panel.PathIndex,
                DetectionDate = panel.DetectionDate,
                HeatPoints = sideData.HeatPoints,
                IsModified = false
            };
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

        private async Task ExportToFile(string filePath, string aiOkVvsNgDir, string aiNgVvsOkDir)
        {
            await Task.Run(() =>
            {
                var lines = new List<string>();

                // 添加表头
                lines.Add("序列号,Lot号,面次,AVI状态,AI状态,人工判定,缺陷数,检测日期");

                // 添加数据并复制图片
                foreach (var item in _defectItems)
                {
                    lines.Add($"{item.SerialNumber},{item.LotNumber},{item.Side}," +
                        $"{item.AviStatus},{item.AiStatus},{item.ManualStatus}," +
                        $"{item.DefectCount},{item.DetectionDate:yyyy-MM-dd HH:mm:ss}");

                    // AI OK, VVS NG
                    if (item.AiStatus == "OK" && item.AviStatus == "NG")
                    {
                        foreach (var heatPoint in item.HeatPoints)
                        {
                            if (File.Exists(heatPoint.ImagePath))
                            {
                                var destFileName = $"{item.SerialNumber}_{item.Side}_{Path.GetFileName(heatPoint.ImagePath)}";
                                File.Copy(heatPoint.ImagePath, Path.Combine(aiOkVvsNgDir, destFileName), true);
                            }
                        }
                    }
                    // AI NG, VVS OK
                    else if (item.AiStatus == "NG" && item.AviStatus == "OK")
                    {
                        foreach (var heatPoint in item.HeatPoints)
                        {
                            if (File.Exists(heatPoint.ImagePath))
                            {
                                var destFileName = $"{item.SerialNumber}_{item.Side}_{Path.GetFileName(heatPoint.ImagePath)}";
                                File.Copy(heatPoint.ImagePath, Path.Combine(aiNgVvsOkDir, destFileName), true);
                            }
                        }
                    }
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
        public string MachineId { get; set; }
        public string ProductSerial { get; set; }
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

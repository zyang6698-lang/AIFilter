using DeepSightDB;
using DeepSightModel;
using DeepSightTool;
using DeepSightWorkLib.Services;
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
        private List<DefectReviewItem> _allDefectItems = new List<DefectReviewItem>(); // 存储所有查询结果
        private Dictionary<string, List<DefectReviewItem>> _lotGroups = new Dictionary<string, List<DefectReviewItem>>(); // 按Lot分组
        private SortableBindingList<DefectReviewItem> _bindingList;
        private string _currentSelectedLot = null; // 当前选中的Lot

        // 复判详情相关字段
        private string _currentReviewLot = null;
        private DateTime _currentReviewTime = DateTime.MinValue;
        private int _vvsOkCount = 0;
        private int _vvsNgCount = 0;
        private int _vvsNotSetCount = 0;

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

            // 订阅VVS复判完成事件 - 当某SN所有缺陷点都完成VVS复判时自动更新人工判定状态
            defectDetailControl1.SnVvsCompleted += DefectDetailControl_SnVvsCompleted;
            
            // 订阅VVS状态改变事件 - 用于更新左下角复判详情显示
            defectDetailControl1.VvsStatusChanged += DefectDetailControl_VvsStatusChanged;

            // 订阅TreeView事件
            treeView_Lots.AfterSelect += TreeView_Lots_AfterSelect;
            treeView_Lots.BeforeExpand += TreeView_Lots_BeforeExpand;
            treeView_Lots.NodeMouseDoubleClick += TreeView_Lots_NodeMouseDoubleClick;
            treeView_Lots.MouseUp += TreeView_Lots_MouseUp;

            // 订阅SN搜索事件
            btn_SnSearch.Click += Btn_SnSearch_Click;
            txt_SnFilter.KeyDown += Txt_SnFilter_KeyDown;

            // 订阅右键菜单事件
            toolStripMenuItem_RunTest.Click += ToolStripMenuItem_RunTest_Click;

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

        private void HeatMapQueryControl_QueryClicked(object sender, EventArgs e)
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
                _allDefectItems.Clear();
                _lotGroups.Clear();
                treeView_Lots.Nodes.Clear();
                defectDetailControl1.ClearDetails();
                _currentSelectedLot = null;

                // 收集所有数据
                foreach (var panel in QueryControl.GetQueryResult())
                {
                    if (panel.Sides == null) continue;
                    foreach (var side in panel.Sides)
                    {
                        if (side != null && side.AviState != 1)
                        {
                            _allDefectItems.Add(CreateDefectReviewItem(panel, side));
                        }
                    }
                }

                // 按Lot分组
                _lotGroups = _allDefectItems
                    .GroupBy(x => x.LotNumber ?? "未知Lot")
                    .ToDictionary(g => g.Key, g => g.ToList());

                // 构建TreeView节点
                BuildLotTreeNodes();

                // 默认不加载任何数据到表格，提示用户选择Lot
                _defectItems.Clear();
                _bindingList = new SortableBindingList<DefectReviewItem>(_defectItems);
                dataGridView_Defects.DataSource = _bindingList;

                MessageBox.Show($"查询完成，共找到 {_allDefectItems.Count} 条记录，分布在 {_lotGroups.Count} 个Lot中。\n请在左侧选择Lot查看详情。",
                    "查询结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        /// <summary>
        /// 当某SN的所有缺陷点都完成VVS复判时，自动更新该SN的人工判定状态
        /// </summary>
        private void DefectDetailControl_SnVvsCompleted(object sender, VvsCompletedEventArgs e)
        {
            if (e.HeatPoints == null || e.HeatPoints.Count == 0)
                return;

            // 在所有数据中查找包含这些HeatPoints的DefectReviewItem
            DefectReviewItem targetItem = null;

            // 先在当前显示的列表中查找
            foreach (var item in _defectItems)
            {
                if (item.HeatPoints != null && item.HeatPoints == e.HeatPoints)
                {
                    targetItem = item;
                    break;
                }
            }

            // 如果没找到，在全部数据中查找
            if (targetItem == null)
            {
                foreach (var item in _allDefectItems)
                {
                    if (item.HeatPoints != null && item.HeatPoints == e.HeatPoints)
                    {
                        targetItem = item;
                        break;
                    }
                }
            }

            if (targetItem == null)
                return;

            // 根据VVS复判结果自动设置人工判定状态：全部OK则OK，有任一NG则NG
            targetItem.ManualStatus = e.AllOk ? "OK" : "NG";
            targetItem.IsModified = true;

            // 更新缺陷点数变化：原始数量 -> VVS复判后NG数量
            int originalCount = targetItem.DefectCount;
            targetItem.DefectChange = $"{originalCount} -> {e.NgCount}";

            // 刷新DataGridView显示
            _bindingList.ResetBindings();
            
            // 更新左下角复判详情
            UpdateVvsStatusSummary();
        }

        /// <summary>
        /// 当VVS状态改变时触发，用于更新左下角复判详情
        /// </summary>
        private void DefectDetailControl_VvsStatusChanged(object sender, EventArgs e)
        {
            // 如果当前有展示的详情，重新统计VVS状态
            if (_currentReviewLot != null)
            {
                UpdateVvsStatusSummary();
                RefreshReviewDetailDisplay();
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
                            var materialLocation = Machine.solconfig.PartNumberImagesLoc;
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

                            // 构建导出文件夹名
                            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                            string folderName;

                            // 判断是否为Lot查看模式（双击Lot节点时SerialNumber会以"Lot:"开头）
                            if (currentItem.SerialNumber != null && currentItem.SerialNumber.StartsWith("Lot:"))
                            {
                                // Lot查看模式：使用Lot名称（去除非法字符）
                                string safeLotName = currentItem.SerialNumber.Replace(":", "_").Replace(" ", "");
                                folderName = $"{safeLotName}_{timestamp}";
                            }
                            else if (!string.IsNullOrEmpty(_currentSelectedLot))
                            {
                                // 在Lot下选择了具体SN：包含Lot和SN
                                folderName = $"{_currentSelectedLot}_{currentItem.SerialNumber}_{currentItem.Side}_{timestamp}";
                            }
                            else
                            {
                                // 普通模式：仅SN
                                folderName = $"{currentItem.SerialNumber}_{currentItem.Side}_{timestamp}";
                            }

                            string exportPath = Path.Combine(dialog.SelectedPath, folderName);
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
                                            // 导出原图
                                            var destFileName = Path.GetFileName(heatPoint.ImagePath);
                                            File.Copy(heatPoint.ImagePath, Path.Combine(exportPath, destFileName), true);

                                            // 导出模板图
                                            string templatePath = FindTemplatePath(heatPoint.ImagePath);
                                            if (!string.IsNullOrEmpty(templatePath) && File.Exists(templatePath))
                                            {
                                                var templateDestFileName = Path.GetFileName(templatePath);
                                                File.Copy(templatePath, Path.Combine(exportPath, templateDestFileName), true);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            LogTextHelper.Error($"复制图片失败: {heatPoint.ImagePath}, {ex.Message}");
                                        }
                                    }
                                }

                                // 导出表格信息到CSV
                                var csvPath = Path.Combine(exportPath, $"{currentItem.SerialNumber}_{currentItem.Side}_info.csv");
                                var csvLines = new List<string>();

                                // 基本信息表头和数据
                                csvLines.Add("序列号,Lot号,机台号,料号,面次,AVI状态,AI状态,人工判定,缺陷数,缺陷变化,检测日期");
                                csvLines.Add($"{currentItem.SerialNumber},{currentItem.LotNumber},{currentItem.MachineId},{currentItem.ProductSerial}," +
                                    $"{currentItem.Side},{currentItem.AviStatus},{currentItem.AiStatus},{currentItem.ManualStatus}," +
                                    $"{currentItem.DefectCount},{currentItem.DefectChange ?? ""},{currentItem.DetectionDate:yyyy-MM-dd HH:mm:ss}");

                                // 统计信息
                                csvLines.Add("");
                                csvLines.Add("统计信息");
                                int aiOkCount = filteredHeatPoints.Count(hp => hp.AIStatus == 1);
                                int aiNgCount = filteredHeatPoints.Count(hp => hp.AIStatus == 2);
                                int vvsOkCount = filteredHeatPoints.Count(hp => hp.VVSStatus == 1);
                                int vvsNgCount = filteredHeatPoints.Count(hp => hp.VVSStatus == 2);
                                int vvsNotSetCount = filteredHeatPoints.Count(hp => hp.VVSStatus == 0);
                                csvLines.Add($"总缺陷数,{filteredHeatPoints.Count}");
                                csvLines.Add($"AI_OK数,{aiOkCount}");
                                csvLines.Add($"AI_NG数,{aiNgCount}");
                                csvLines.Add($"VVS_OK数,{vvsOkCount}");
                                csvLines.Add($"VVS_NG数,{vvsNgCount}");
                                csvLines.Add($"VVS未设置数,{vvsNotSetCount}");

                                // 添加缺陷点详情
                                csvLines.Add("");
                                csvLines.Add("缺陷点详情");
                                csvLines.Add("序号,缺陷名称,缺陷类型,坐标X,坐标Y,宽度,高度,AI状态,AI状态文本,VVS状态,VVS状态文本,VRS状态,最终状态,图片路径");
                                int index = 1;
                                foreach (var hp in filteredHeatPoints)
                                {
                                    string aiStatusText = GetStatusTextStatic(hp.AIStatus);
                                    string vvsStatusText = GetStatusTextStatic(hp.VVSStatus);
                                    csvLines.Add($"{index},{hp.DefectName ?? ""},{hp.DefectType ?? ""}," +
                                        $"{hp.RoiX},{hp.RoiY},{hp.Width},{hp.Height}," +
                                        $"{hp.AIStatus},{aiStatusText},{hp.VVSStatus},{vvsStatusText}," +
                                        $"{hp.VrsState},{hp.FinalState},{hp.ImagePath ?? ""}");
                                    index++;
                                }

                                File.WriteAllLines(csvPath, csvLines, System.Text.Encoding.UTF8);
                            });

                            this.Enabled = true;

                            // 自动打开导出文件夹
                            System.Diagnostics.Process.Start("explorer.exe", exportPath);
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

        private void Btn_LoadImages_Click(object sender, EventArgs e)
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
                            HeatPoints = imagePaths.Select(p => new DetectInfo { ImagePath = p }).ToList()
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

        /// <summary>
        /// TreeView节点选择事件 - 加载选中Lot的数据到表格
        /// </summary>
        private void TreeView_Lots_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;

            // 如果是Lot节点（第一层），加载该Lot的数据
            if (e.Node.Level == 0)
            {
                LoadLotData(e.Node.Name);
                // 清除复判详情，因为只是选择表格数据
                _currentReviewLot = null;
                RefreshReviewDetailDisplay();
            }
            // 如果是子分类节点（第二层，如按Side分组），加载该分类的数据
            else if (e.Node.Level == 1 && e.Node.Parent != null)
            {
                string lotNumber = e.Node.Parent.Name;
                string category = e.Node.Name; // 如 "A面", "B面" 等
                LoadLotDataByCategory(lotNumber, category);
                // 清除复判详情
                _currentReviewLot = null;
                RefreshReviewDetailDisplay();
            }
        }

        /// <summary>
        /// TreeView节点展开前事件 - 用于延迟加载子节点
        /// </summary>
        private void TreeView_Lots_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            // 如果节点包含占位符子节点，则加载真实子节点
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "加载中...")
            {
                e.Node.Nodes.Clear();
                LoadLotSubCategories(e.Node);
            }
        }

        /// <summary>
        /// TreeView节点双击事件 - 显示该Lot下所有缺陷图片并跳转到图片显示
        /// </summary>
        private void TreeView_Lots_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null) return;

            List<DefectReviewItem> itemsToDisplay = null;
            string displayTitle = string.Empty;

            // 如果是Lot节点（第一层），显示该Lot的所有缺陷图片
            if (e.Node.Level == 0)
            {
                string lotNumber = e.Node.Name;
                if (_lotGroups.TryGetValue(lotNumber, out var items))
                {
                    itemsToDisplay = items;
                    displayTitle = $"Lot: {lotNumber}";
                }
            }
            // 如果是子分类节点（第二层，如按Side分组），显示该分类的缺陷图片
            else if (e.Node.Level == 1 && e.Node.Parent != null)
            {
                string lotNumber = e.Node.Parent.Name;
                string category = e.Node.Name;
                if (_lotGroups.TryGetValue(lotNumber, out var items))
                {
                    itemsToDisplay = items.Where(x => x.Side == category).ToList();
                    displayTitle = $"Lot: {lotNumber} - {category}面";
                }
            }

            if (itemsToDisplay != null && itemsToDisplay.Count > 0)
            {
                // 检查是否有缺陷图片
                int totalHeatPoints = itemsToDisplay.Sum(item => item.HeatPoints?.Count ?? 0);

                if (totalHeatPoints > 0)
                {
                    // 更新复判详情显示（使用第一个项目的信息）
                    if (itemsToDisplay.Count > 0)
                    {
                        var firstItem = itemsToDisplay.FirstOrDefault();
                        if (firstItem != null)
                        {
                            _currentReviewLot = firstItem.LotNumber;
                            _currentReviewTime = firstItem.DetectionDate;
                            // 统计所有项目的VVS状态总和
                            _vvsOkCount = itemsToDisplay.Sum(item => item.HeatPoints?.Count(hp => hp.VVSStatus == 1) ?? 0);
                            _vvsNgCount = itemsToDisplay.Sum(item => item.HeatPoints?.Count(hp => hp.VVSStatus == 2) ?? 0);
                            _vvsNotSetCount = itemsToDisplay.Sum(item => item.HeatPoints?.Count(hp => hp.VVSStatus == 0) ?? 0);
                            RefreshReviewDetailDisplay();
                        }
                    }
                    
                    // 使用新的重载方法，传递原始items列表（保持HeatPoints引用），以便按SN分组检查VVS状态
                    defectDetailControl1.DisplayDefectDetails(itemsToDisplay, displayTitle);
                    tabControl_Main.SelectedTab = tabPage_Details;
                }
                else
                {
                    MessageBox.Show($"{displayTitle} 下没有缺陷图片。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }



        /// <summary>
        /// 构建Lot分组的TreeView节点
        /// </summary>
        private void BuildLotTreeNodes()
        {
            treeView_Lots.BeginUpdate();
            treeView_Lots.Nodes.Clear();

            foreach (var lotGroup in _lotGroups.OrderBy(x => x.Key))
            {
                var lotNode = new TreeNode
                {
                    Name = lotGroup.Key,
                    Text = $"{lotGroup.Key} ({lotGroup.Value.Count}条)",
                    ForeColor = Color.White
                };

                // 添加占位符子节点，用于延迟加载
                lotNode.Nodes.Add(new TreeNode("加载中..."));
                treeView_Lots.Nodes.Add(lotNode);
            }

            treeView_Lots.EndUpdate();
        }

        /// <summary>
        /// 加载Lot的子分类节点（按Side分组）
        /// </summary>
        private void LoadLotSubCategories(TreeNode lotNode)
        {
            string lotNumber = lotNode.Name;
            if (!_lotGroups.TryGetValue(lotNumber, out var items)) return;

            // 按Side分组
            var sideGroups = items.GroupBy(x => x.Side ?? "未知").OrderBy(g => g.Key);
            foreach (var sideGroup in sideGroups)
            {
                var sideNode = new TreeNode
                {
                    Name = sideGroup.Key,
                    Text = $"{sideGroup.Key}面 ({sideGroup.Count()}条)",
                    ForeColor = Color.LightGray
                };
                lotNode.Nodes.Add(sideNode);
            }
        }

        /// <summary>
        /// 加载指定Lot的所有数据到表格
        /// </summary>
        private void LoadLotData(string lotNumber)
        {
            if (!_lotGroups.TryGetValue(lotNumber, out var items)) return;

            _currentSelectedLot = lotNumber;
            _defectItems.Clear();
            _defectItems.AddRange(items);

            _bindingList = new SortableBindingList<DefectReviewItem>(_defectItems);
            dataGridView_Defects.DataSource = _bindingList;
            _bindingList.ResetBindings();

            label_LotTitle.Text = $"Lot: {lotNumber} ({items.Count}条)";
        }

        /// <summary>
        /// 按Lot和子分类加载数据到表格
        /// </summary>
        private void LoadLotDataByCategory(string lotNumber, string category)
        {
            if (!_lotGroups.TryGetValue(lotNumber, out var items)) return;

            var filteredItems = items.Where(x => x.Side == category).ToList();

            _currentSelectedLot = lotNumber;
            _defectItems.Clear();
            _defectItems.AddRange(filteredItems);

            _bindingList = new SortableBindingList<DefectReviewItem>(_defectItems);
            dataGridView_Defects.DataSource = _bindingList;
            _bindingList.ResetBindings();

            label_LotTitle.Text = $"Lot: {lotNumber} - {category}面 ({filteredItems.Count}条)";
        }

        /// <summary>
        /// SN搜索按钮点击事件
        /// </summary>
        private void Btn_SnSearch_Click(object sender, EventArgs e)
        {
            FilterBySn();
        }

        /// <summary>
        /// SN输入框回车事件
        /// </summary>
        private void Txt_SnFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                FilterBySn();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// 根据SN筛选数据
        /// </summary>
        private void FilterBySn()
        {
            string filterText = txt_SnFilter.Text.Trim();

            // 如果搜索框为空，恢复当前Lot的所有数据
            if (string.IsNullOrEmpty(filterText))
            {
                if (!string.IsNullOrEmpty(_currentSelectedLot))
                {
                    LoadLotData(_currentSelectedLot);
                }
                else
                {
                    // 如果没有选中Lot，搜索所有数据
                    SearchAllData(filterText);
                }
                return;
            }

            // 确定搜索范围：当前选中的Lot或所有数据
            List<DefectReviewItem> searchSource;
            if (!string.IsNullOrEmpty(_currentSelectedLot) && _lotGroups.TryGetValue(_currentSelectedLot, out var lotItems))
            {
                searchSource = lotItems;
            }
            else
            {
                searchSource = _allDefectItems;
            }

            // 模糊搜索SN（支持部分匹配）
            var filteredItems = searchSource
                .Where(x => x.SerialNumber != null && x.SerialNumber.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            // 更新表格
            _defectItems.Clear();
            _defectItems.AddRange(filteredItems);

            _bindingList = new SortableBindingList<DefectReviewItem>(_defectItems);
            dataGridView_Defects.DataSource = _bindingList;
            _bindingList.ResetBindings();

            // 更新标题
            string scopeText = string.IsNullOrEmpty(_currentSelectedLot) ? "全部" : $"Lot: {_currentSelectedLot}";
            label_LotTitle.Text = $"{scopeText} - 搜索: {filterText} ({filteredItems.Count}条)";

            // 如果只找到一条，自动选中并可选择显示详情
            if (filteredItems.Count == 1)
            {
                dataGridView_Defects.ClearSelection();
                dataGridView_Defects.Rows[0].Selected = true;
                dataGridView_Defects.CurrentCell = dataGridView_Defects.Rows[0].Cells[0];
            }
            else if (filteredItems.Count == 0)
            {
                MessageBox.Show($"未找到包含 \"{filterText}\" 的序列号。", "搜索结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 在所有数据中搜索
        /// </summary>
        private void SearchAllData(string filterText)
        {
            if (string.IsNullOrEmpty(filterText))
            {
                // 清空表格，提示用户选择Lot
                _defectItems.Clear();
                _bindingList = new SortableBindingList<DefectReviewItem>(_defectItems);
                dataGridView_Defects.DataSource = _bindingList;
                label_LotTitle.Text = "请选择Lot或输入SN搜索";
                return;
            }

            var filteredItems = _allDefectItems
                .Where(x => x.SerialNumber != null && x.SerialNumber.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            _defectItems.Clear();
            _defectItems.AddRange(filteredItems);

            _bindingList = new SortableBindingList<DefectReviewItem>(_defectItems);
            dataGridView_Defects.DataSource = _bindingList;
            _bindingList.ResetBindings();

            label_LotTitle.Text = $"全局搜索: {filterText} ({filteredItems.Count}条)";
        }

        /// <summary>
        /// TreeView 鼠标右键弹起事件 - 显示右键菜单
        /// </summary>
        private void TreeView_Lots_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var node = treeView_Lots.GetNodeAt(e.X, e.Y);
                if (node != null && node.Level == 0)  // 只对 Lot 节点显示右键菜单
                {
                    treeView_Lots.SelectedNode = node;
                    contextMenuStrip_Lot.Show(treeView_Lots, e.Location);
                }
            }
        }

        /// <summary>
        /// 运行模型一致性测试菜单项点击事件
        /// </summary>
        private async void ToolStripMenuItem_RunTest_Click(object sender, EventArgs e)
        {
            var selectedNode = treeView_Lots.SelectedNode;
            if (selectedNode == null || selectedNode.Level != 0)
            {
                MessageBox.Show("请先选择一个Lot节点。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string lotNumber = selectedNode.Name;

            if (!_lotGroups.TryGetValue(lotNumber, out var lotItems) || lotItems.Count == 0)
            {
                MessageBox.Show($"Lot {lotNumber} 中没有可测试的数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 确认对话框
            var result = MessageBox.Show(
                $"是否对 Lot: {lotNumber} 运行模型一致性测试？\n" +
                $"共 {lotItems.Count} 条记录，将重新进行AI推理并与原结果比对。",
                "确认测试",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            await RunValidationTestAsync(lotNumber, lotItems);
        }

        /// <summary>
        /// 运行模型验证测试
        /// </summary>
        private async Task RunValidationTestAsync(string lotNumber, List<DefectReviewItem> items)
        {
            var service = Machine.master?.workClass?.ValidationTestService;
            if (service == null)
            {
                MessageBox.Show("模型验证测试服务未初始化。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                this.Enabled = false;
                label_LotTitle.Text = $"正在启动 Lot: {lotNumber} 的测试任务...";

                // 收集所有 SN+Side 组合
                var serialNumbers = items.Select(i => i.SerialNumber).Distinct().ToList();

                // 根据 items 的时间范围确定查询条件
                var startDate = items.Min(i => i.DetectionDate).AddMinutes(-1);
                var endDate = items.Max(i => i.DetectionDate).AddMinutes(1);

                // 创建测试请求
                var request = new ValidationTestRequest
                {
                    LotNumber = lotNumber,
                    StartDate = startDate,
                    EndDate = endDate,
                    MaxRecords = items.Count * 2,  // 留一些余量
                    Description = $"Lot {lotNumber} 一致性测试"
                };

                // 启动测试任务
                var task = await service.CreateTestTaskAsync(request);

                if (task == null)
                {
                    MessageBox.Show("创建测试任务失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 切换到测试结果页并开始监控
                validationTestResultControl1.StartMonitoring(task);
                tabControl_Main.SelectedTab = tabPage_ValidationTest;

                MessageBox.Show(
                    $"测试任务已启动！\n" +
                    $"任务ID: {task.TaskId}\n" +
                    $"预计测试 {task.TotalRecords} 条记录\n\n" +
                    "请在【模型一致性测试】页查看进度和结果。",
                    "测试已启动",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"启动测试任务失败: {ex}");
                MessageBox.Show($"启动测试任务失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Enabled = true;
                label_LotTitle.Text = $"Lot: {lotNumber}";
            }
        }

        #endregion

        #region Data Operations

        /// <summary>
        /// 查找模板图片路径
        /// </summary>
        /// <param name="imagePath">原图路径</param>
        /// <returns>模板图片路径，未找到返回null</returns>
        private string FindTemplatePath(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                    return null;

                string dir = Path.GetDirectoryName(imagePath);
                string filename = Path.GetFileNameWithoutExtension(imagePath);
                string ext = Path.GetExtension(imagePath);

                // 在同目录中查找包含原图名、包含"template"并且扩展名相同的文件
                var candidates = Directory.EnumerateFiles(dir)
                    .Where(p => string.Equals(Path.GetExtension(p), ext, StringComparison.OrdinalIgnoreCase))
                    .Where(p =>
                    {
                        var name = Path.GetFileNameWithoutExtension(p);
                        return name.IndexOf(filename, StringComparison.OrdinalIgnoreCase) >= 0
                               && name.IndexOf("template", StringComparison.OrdinalIgnoreCase) >= 0;
                    })
                    .ToList();

                return candidates.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"查找模板图片失败: {imagePath}, {ex.Message}");
                return null;
            }
        }

        private DefectReviewItem CreateDefectReviewItem(PanelDataRecord panel, SideData sideData)
        {
            // 从缺陷点中提取所有不重复的缺陷名称
            var defectNames = sideData.DetectPoints?
                .Where(dp => !string.IsNullOrEmpty(dp.DefectName))
                .Select(dp => dp.DefectName)
                .Distinct()
                .ToList() ?? new List<string>();
            string defectNameStr = defectNames.Count > 0 ? string.Join(", ", defectNames) : "";

            return new DefectReviewItem
            {
                SerialNumber = panel.SerialNumber,
                LotNumber = panel.LotNumber,
                MachineId = panel.MachineId,
                ProductSerial = panel.ProductSerial,
                Side = sideData.Side,
                AviStatus = sideData.AviState == 1 ? "OK" : "NG",
                AiStatus = sideData.AiState == 1 ? "OK" : "NG",
                ManualStatus = sideData.VvsState == 0 ? "未判定" : sideData.VvsState == 1 ? "OK" : "NG",
                DefectCount = sideData.DetectPoints?.Count ?? 0,
                DefectName = defectNameStr,
                PathIndex = panel.PathIndex,
                DetectionDate = panel.DetectionDate,
                HeatPoints = sideData.DetectPoints,
                IsModified = false
            };
        }

        /// <summary>
        /// 将状态码转换为显示文本（静态方法，用于CSV导出）
        /// </summary>
        private static string GetStatusTextStatic(int status)
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


        /// <summary>
        /// 统计当前VVS状态
        /// </summary>
        private void UpdateVvsStatusSummary()
        {
            _vvsOkCount = 0;
            _vvsNgCount = 0;
            _vvsNotSetCount = 0;

            foreach (var item in _allDefectItems)
            {
                if (item.LotNumber==_currentReviewLot)
                {
                    if (item != null && item.HeatPoints != null)
                    {
                        foreach (var hp in item.HeatPoints)
                        {
                            if (hp.VVSStatus == 0) _vvsNotSetCount++;
                            else if (hp.VVSStatus == 1) _vvsOkCount++;
                            else if (hp.VVSStatus == 2) _vvsNgCount++;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 刷新复判详情的显示
        /// </summary>
        private void RefreshReviewDetailDisplay()
        {
            string detail = string.Empty;

            if (!string.IsNullOrEmpty(_currentReviewLot))
            {
                detail = $"【复判详情】\n\n";
                detail += $"Lot号: {_currentReviewLot}\n";
                detail += $"检测时间: {_currentReviewTime:yyyy-MM-dd HH:mm:ss}\n\n";
                detail += $"【VVS状态统计】\n";
                detail += $"OK: {_vvsOkCount}\n";
                detail += $"NG: {_vvsNgCount}\n";
                detail += $"未设置: {_vvsNotSetCount}\n";
                detail += $"总数: {_vvsOkCount + _vvsNgCount + _vvsNotSetCount}";
            }
            else
            {
                detail = "请从左侧选择缺陷记录\n以查看复判详情";
            }

            label_ReviewDetail.Text = detail;
        }

        private async Task SaveManualReviewResults(List<DefectReviewItem> items)
        {
            await Task.Run(() =>
            {
                foreach (var item in items)
                {
                    try
                    {
                        // 将人工判定状态转换为 FinalState
                        // ManualStatus: "OK" -> 1, "NG" -> 2, "未判定" -> 0
                        int finalState = 0;
                        if (item.ManualStatus == "OK") finalState = 1;
                        else if (item.ManualStatus == "NG") finalState = 2;

                        // 计算各阶段状态
                        int aviState = item.AviStatus == "OK" ? 1 : 2;
                        int aiState = item.AiStatus == "OK" ? 1 : 2;

                        // 计算 VVS 状态：基于 HeatPoints 中的 VVSStatus
                        int vvsState = 0;
                        if (item.HeatPoints != null && item.HeatPoints.Count > 0)
                        {
                            bool hasVvsSet = item.HeatPoints.Any(hp => hp.VVSStatus != 0);
                            if (hasVvsSet)
                            {
                                bool allVvsOk = item.HeatPoints.All(hp => hp.VVSStatus == 0 || hp.VVSStatus == 1);
                                vvsState = allVvsOk ? 1 : 2;
                            }
                        }

                        // 创建 PanelSideRecord 用于保存
                        var record = new PanelSideRecord
                        {
                            SerialNumber = item.SerialNumber,
                            LotNumber = item.LotNumber,
                            MachineId = item.MachineId,
                            ProductSerial = item.ProductSerial,
                            Side = item.Side,
                            PathIndex = item.PathIndex,
                            DetectionDate = item.DetectionDate,
                            Data = new SideData
                            {
                                Side = item.Side,
                                DetectPoints = item.HeatPoints ?? new List<DetectInfo>(),
                                AviState = aviState,
                                AiState = aiState,
                                VvsState = vvsState,
                                VrsState = 0, // VRS 状态暂不处理
                                FinalState = finalState
                            }
                        };

                        // 调用 SavePanelSide 保存到数据库（支持覆盖）
                        Machine.master.workClass.SavePanelSide(record);

                        LogTextHelper.Info($"保存人工判定结果成功: SN={item.SerialNumber}, Side={item.Side}, " +
                            $"ManualStatus={item.ManualStatus}, FinalState={finalState}");
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"保存人工判定结果失败: SN={item.SerialNumber}, Side={item.Side}, 错误: {ex.Message}");
                    }
                }
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
        [Browsable(false)]
        public string AviStatus { get; set; }
        public string AiStatus { get; set; }
        public string ManualStatus { get; set; }
        public int DefectCount { get; set; }
        /// <summary>
        /// 缺陷名称列表，多个缺陷名用逗号分隔
        /// </summary>
        public string DefectName { get; set; }
        /// <summary>
        /// 缺陷点数变化，格式：原始数量 -> VVS复判后NG数量
        /// </summary>
        public string DefectChange { get; set; }
        public string PathIndex { get; set; }
        public DateTime DetectionDate { get; set; }
        [Browsable(false)]
        public List<DetectInfo> HeatPoints { get; set; }
        [Browsable(false)]
        public bool IsModified { get; set; }
    }

    #endregion
}

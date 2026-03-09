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

        // 报点级别 VVS 交叉统计（动态计算）
        private int _aiOkVvsOkPointCount = 0;   // AI-OK & 人工OK 报点数
        private int _aiOkVvsNgPointCount = 0;   // AI-OK & 人工NG 报点数
        private int _aiNgVvsOkPointCount = 0;   // AI-NG & 人工OK 报点数
        private int _aiNgVvsNgPointCount = 0;   // AI-NG & 人工NG 报点数

        // PCS级别 VVS 交叉统计（动态计算）
        private int _aiOkVvsOkPcsCount = 0;     // AI-OK & 人工OK PCS数
        private int _aiOkVvsNgPcsCount = 0;     // AI-OK & 人工NG PCS数
        private int _aiNgVvsOkPcsCount = 0;     // AI-NG & 人工OK PCS数
        private int _aiNgVvsNgPcsCount = 0;     // AI-NG & 人工NG PCS数

        // 按Lot分组的统计数据（包含所有面板，用于计算统计指标）
        private Dictionary<string, LotStatistics> _lotStatistics = new Dictionary<string, LotStatistics>();

        // 自动保存相关字段
        private int _pendingSaveCount = 0;
        private const int AutoSaveBatchSize = 100;
        private bool _isFlushing = false;

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
            QueryControl.FilterChanged += QueryControl_FilterChanged;

            // 订阅DataGridView事件
            dataGridView_Defects.CellDoubleClick += DataGridView_Defects_CellDoubleClick;
            dataGridView_Defects.CellValueChanged += DataGridView_Defects_CellValueChanged;

            // 订阅详情控件的切换下一行事件
            defectDetailControl1.SelectNextRowRequested += DefectDetailControl_SelectNextRowRequested;

            // 订阅VVS复判完成事件 - 当某SN所有缺陷点都完成VVS复判时自动更新人工判定状态
            defectDetailControl1.SnVvsCompleted += DefectDetailControl_SnVvsCompleted;
            
            // 订阅VVS状态改变事件 - 用于更新左下角复判详情显示
            defectDetailControl1.VvsStatusChanged += DefectDetailControl_VvsStatusChanged;

            // 订阅单图测试事件
            defectDetailControl1.SingleImageTestRequested += DefectDetailControl_SingleImageTestRequested;

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
            toolStripMenuItem_SecondaryInference.Click += ToolStripMenuItem_SecondaryInference_Click;

            // 订阅页面切换事件 - 页面跳转时自动保存
            tabControl_Main.SelectedIndexChanged += TabControl_Main_SelectedIndexChanged;

            // 订阅导出事件 - DefectDetailControl中的导出按钮
            defectDetailControl1.ExportRequested += DefectDetailControl_ExportRequested;

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
                _lotStatistics.Clear();
                treeView_Lots.Nodes.Clear();
                defectDetailControl1.ClearDetails();
                _currentSelectedLot = null;

                // 收集所有数据，同时统计指标
                foreach (var panel in QueryControl.GetQueryResult())
                {
                    if (panel.Sides == null) continue;
                    string lotNumber = panel.LotNumber ?? "未知Lot";

                    // 确保该Lot的统计数据存在
                    if (!_lotStatistics.ContainsKey(lotNumber))
                    {
                        _lotStatistics[lotNumber] = new LotStatistics();
                    }
                    var stat = _lotStatistics[lotNumber];

                    // 记录基本信息（取第一条）
                    if (string.IsNullOrEmpty(stat.MachineId))
                    {
                        stat.MachineId = panel.MachineId;
                        stat.ProductSerial = panel.ProductSerial;
                    }

                    // Panel(Array)级别统计
                    stat.TotalPanelCount++;
                    bool panelAllAviOk = true;
                    bool panelAllAiPass = true;

                    foreach (var side in panel.Sides)
                    {
                        if (side == null) continue;

                        // PCS级别统计
                        stat.TotalPcsCount++;
                        if (side.AviState == 1)
                        {
                            stat.AviOkPcsCount++;
                        }
                        else
                        {
                            panelAllAviOk = false;
                            if (side.AiState == 1)
                                stat.AiOkPcsCount++;
                            else
                            {
                                stat.AiNgPcsCount++;
                                panelAllAiPass = false;
                            }
                        }

                        // 报点级别统计
                        if (side.DetectPoints != null)
                        {
                            stat.TotalPointCount += side.DetectPoints.Count;
                            stat.AiOkPointCount += side.DetectPoints.Count(p => p.AIStatus == 1);
                            stat.AiNgPointCount += side.DetectPoints.Count(p => p.AIStatus == 2);
                        }

                        // 只有 AVI NG 的数据才加入复判列表
                        if (side.AviState != 1)
                        {
                            _allDefectItems.Add(CreateDefectReviewItem(panel, side));
                        }
                    }

                    if (panelAllAviOk) stat.AviOkPanelCount++;
                    if (panelAllAiPass) stat.AiPassPanelCount++;
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

                //MessageBox.Show($"查询完成，共找到 {_allDefectItems.Count} 条记录，分布在 {_lotGroups.Count} 个Lot中。\n请在左侧选择Lot查看详情。",
                //    "查询结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        /// <summary>
        /// 筛选条件变化事件处理（料号或机台号选择变化时自动筛选）
        /// </summary>
        private void QueryControl_FilterChanged(object sender, EventArgs e)
        {
            try
            {
                this.Enabled = false;
                _defectItems.Clear();
                _allDefectItems.Clear();
                _lotGroups.Clear();
                _lotStatistics.Clear();
                treeView_Lots.Nodes.Clear();
                defectDetailControl1.ClearDetails();
                _currentSelectedLot = null;

                // 根据筛选条件重新收集数据，同时统计指标
                foreach (var panel in QueryControl.GetQueryResult())
                {
                    if (panel.Sides == null) continue;
                    string lotNumber = panel.LotNumber ?? "未知Lot";

                    // 确保该Lot的统计数据存在
                    if (!_lotStatistics.ContainsKey(lotNumber))
                    {
                        _lotStatistics[lotNumber] = new LotStatistics();
                    }
                    var stat = _lotStatistics[lotNumber];

                    // 记录基本信息（取第一条）
                    if (string.IsNullOrEmpty(stat.MachineId))
                    {
                        stat.MachineId = panel.MachineId;
                        stat.ProductSerial = panel.ProductSerial;
                    }

                    // Panel(Array)级别统计
                    stat.TotalPanelCount++;
                    bool panelAllAviOk = true;
                    bool panelAllAiPass = true;

                    foreach (var side in panel.Sides)
                    {
                        if (side == null) continue;

                        // PCS级别统计
                        stat.TotalPcsCount++;
                        if (side.AviState == 1)
                        {
                            stat.AviOkPcsCount++;
                        }
                        else
                        {
                            panelAllAviOk = false;
                            if (side.AiState == 1)
                                stat.AiOkPcsCount++;
                            else
                            {
                                stat.AiNgPcsCount++;
                                panelAllAiPass = false;
                            }
                        }

                        // 报点级别统计
                        if (side.DetectPoints != null)
                        {
                            stat.TotalPointCount += side.DetectPoints.Count;
                            stat.AiOkPointCount += side.DetectPoints.Count(p => p.AIStatus == 1);
                            stat.AiNgPointCount += side.DetectPoints.Count(p => p.AIStatus == 2);
                        }

                        // 只有 AVI NG 的数据才加入复判列表
                        if (side.AviState != 1)
                        {
                            _allDefectItems.Add(CreateDefectReviewItem(panel, side));
                        }
                    }

                    if (panelAllAviOk) stat.AviOkPanelCount++;
                    if (panelAllAiPass) stat.AiPassPanelCount++;
                }

                // 按Lot分组
                _lotGroups = _allDefectItems
                    .GroupBy(x => x.LotNumber ?? "未知Lot")
                    .ToDictionary(g => g.Key, g => g.ToList());

                // 构建TreeView节点
                BuildLotTreeNodes();

                // 更新表格显示
                _defectItems.Clear();
                _bindingList = new SortableBindingList<DefectReviewItem>(_defectItems);
                dataGridView_Defects.DataSource = _bindingList;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"筛选异常: {ex}");
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
        /// 当VVS状态改变时触发，用于更新左下角复判详情并执行自动保存动作
        /// </summary>
        private async void DefectDetailControl_VvsStatusChanged(object sender, EventArgs e)
        {
            // 如果当前有展示的详情，重新统计VVS状态
            if (_currentReviewLot != null)
            {
                UpdateVvsStatusSummary();
                RefreshReviewDetailDisplay();
            }

            // 每次VVS状态修改后，标记对应的DefectReviewItem为已修改，并累加保存计数
            MarkCurrentVvsItemModified();
            _pendingSaveCount++;

            // 积攒满100次时批量保存到数据库
            if (_pendingSaveCount >= AutoSaveBatchSize)
            {
                await FlushPendingSaves();
            }
        }

        /// <summary>
        /// 标记当前正在操作的缺陷点所属的DefectReviewItem为已修改
        /// </summary>
        private void MarkCurrentVvsItemModified()
        {
            // 通过DefectDetailControl获取当前显示的source items，标记所有为已修改
            // 因为VVS状态改变可能影响任何一个item
            if (_allDefectItems == null) return;

            foreach (var item in _allDefectItems)
            {
                if (item.HeatPoints != null && item.HeatPoints.Any(hp => hp.VVSStatus != 0))
                {
                    // 如果有任何一个缺陷点被设置过VVS状态，标记为已修改
                    item.IsModified = true;
                }
            }
        }

        /// <summary>
        /// 批量保存待保存的修改到数据库（静默保存，不弹窗提示）
        /// </summary>
        private async Task FlushPendingSaves()
        {
            if (_isFlushing) return;
            _isFlushing = true;

            try
            {
                var modifiedItems = _allDefectItems.Where(x => x.IsModified).ToList();
                if (modifiedItems.Count > 0)
                {
                    await SaveManualReviewResults(modifiedItems);
                    modifiedItems.ForEach(x => x.IsModified = false);
                    LogTextHelper.Info($"自动保存完成: 共保存 {modifiedItems.Count} 条记录");
                }
                _pendingSaveCount = 0;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"自动保存失败: {ex.Message}");
            }
            finally
            {
                _isFlushing = false;
            }
        }

        /// <summary>
        /// 当请求单图测试时触发
        /// </summary>
        private async void DefectDetailControl_SingleImageTestRequested(object sender, SingleImageTestEventArgs e)
        {
            if (e?.HeatPoint == null)
                return;

            var service = Machine.master?.ValidationTestService;
            if (service == null)
            {
                MessageBox.Show("模型验证测试服务未初始化。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 查找当前缺陷所属的DefectReviewItem以获取料号、机台等信息
            var sourceItem = FindSourceItemForHeatPoint(e.HeatPoint);
            if (sourceItem == null)
            {
                MessageBox.Show("无法确定缺陷所属的记录信息，无法进行测试。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                this.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                var result = await service.RunSingleImageTestAsync(
                    e.HeatPoint,
                    sourceItem.ProductSerial,
                    sourceItem.MachineId,
                    sourceItem.Side,
                    timeout: 30);

                if (result.Success)
                {
                    string originalStatusText = GetAIStatusText(result.OriginalAIStatus);
                    string newStatusText = GetAIStatusText(result.NewAIStatus);
                    string consistentText = result.IsConsistent ? "✓ 一致" : "✗ 不一致";

                    MessageBox.Show(
                        $"单图测试完成！\n\n" +
                        $"原始AI结果: {originalStatusText}\n" +
                        $"新AI结果: {newStatusText}\n" +
                        $"比对结果: {consistentText}",
                        "单图测试结果",
                        MessageBoxButtons.OK,
                        result.IsConsistent ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(
                        $"单图测试失败：{result.ErrorMessage}",
                        "测试失败",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"单图测试异常: {ex}");
                MessageBox.Show($"单图测试异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// 根据缺陷点查找其所属的DefectReviewItem
        /// </summary>
        private DefectReviewItem FindSourceItemForHeatPoint(DeepSightDB.DetectInfo heatPoint)
        {
            // 先在当前显示的列表中查找
            foreach (var item in _defectItems)
            {
                if (item.HeatPoints != null && item.HeatPoints.Contains(heatPoint))
                    return item;
            }

            // 如果没找到，在全部数据中查找
            foreach (var item in _allDefectItems)
            {
                if (item.HeatPoints != null && item.HeatPoints.Contains(heatPoint))
                    return item;
            }

            return null;
        }

        /// <summary>
        /// 获取AI状态的显示文本
        /// </summary>
        private string GetAIStatusText(int status)
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
        /// 页面切换时自动保存待保存的修改
        /// </summary>
        private async void TabControl_Main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_pendingSaveCount > 0)
            {
                await FlushPendingSaves();
            }
        }

        /// <summary>
        /// DefectDetailControl中导出按钮的事件处理
        /// </summary>
        private void DefectDetailControl_ExportRequested(object sender, EventArgs e)
        {
            Btn_Export_Click(sender, e);
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

                    // 先弹出导出选项对话框，让用户选择导出原图和/或模板图
                    bool exportOriginal;
                    bool exportTemplate;
                    using (var optionsDialog = new ExportOptionsDialog())
                    {
                        if (optionsDialog.ShowDialog() != DialogResult.OK)
                            return;
                        exportOriginal = optionsDialog.ExportOriginalImage;
                        exportTemplate = optionsDialog.ExportTemplateImage;
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
                                // 从Minio加载图片并保存到本地
                                defectDetailControl1.ExportImages(exportPath, exportOriginal, exportTemplate);

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
                _currentReviewLot = e.Node.Name;
                RefreshReviewDetailDisplay();
            }
            // 如果是子分类节点（第二层，如按Side分组），加载该分类的数据
            else if (e.Node.Level == 1 && e.Node.Parent != null)
            {
                string lotNumber = e.Node.Parent.Name;
                string category = e.Node.Name; // 如 "A面", "B面" 等
                LoadLotDataByCategory(lotNumber, category);
                // 清除复判详情
                _currentReviewLot = lotNumber;
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
                            // 更新统计数据（包括VVS状态和统计指标）
                            UpdateVvsStatusSummary();
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
            var service = Machine.master?.ValidationTestService;
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

        /// <summary>
        /// 运行二次推理菜单项点击事件
        /// </summary>
        private async void ToolStripMenuItem_SecondaryInference_Click(object sender, EventArgs e)
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
                MessageBox.Show($"Lot {lotNumber} 中没有可推理的数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 统计 AI NG 的数量
            int ngCount = lotItems.Count(i => i.AiStatus == "NG");
            if (ngCount == 0)
            {
                MessageBox.Show($"Lot {lotNumber} 中没有 AI NG 的数据，无需二次推理。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 确认对话框
            var result = MessageBox.Show(
                $"是否对 Lot: {lotNumber} 运行二次推理？\n" +
                $"共 {lotItems.Count} 条记录，其中 {ngCount} 条 AI NG。\n\n" +
                "二次推理将重新对 AI NG 的点进行推理，并【覆盖】原有的 AI 状态。",
                "确认二次推理",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            await RunSecondaryInferenceAsync(lotNumber, lotItems);
        }

        /// <summary>
        /// 运行二次推理
        /// </summary>
        private async Task RunSecondaryInferenceAsync(string lotNumber, List<DefectReviewItem> items)
        {
            var service = Machine.master?.ValidationTestService;
            if (service == null)
            {
                MessageBox.Show("模型验证测试服务未初始化。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                this.Enabled = false;
                label_LotTitle.Text = $"正在启动 Lot: {lotNumber} 的二次推理任务...";

                // 根据 items 的时间范围确定查询条件
                var startDate = items.Min(i => i.DetectionDate).AddMinutes(-1);
                var endDate = items.Max(i => i.DetectionDate).AddMinutes(1);

                // 创建二次推理请求
                var request = new SecondaryInferenceRequest
                {
                    LotNumber = lotNumber,
                    StartDate = startDate,
                    EndDate = endDate,
                    MaxRecords = items.Count * 2,  // 留一些余量
                    Description = $"Lot {lotNumber} 二次推理"
                };

                // 启动二次推理任务
                var task = await service.CreateSecondaryInferenceTaskAsync(request);

                if (task == null)
                {
                    MessageBox.Show("创建二次推理任务失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 切换到测试结果页并开始监控（复用现有控件）
                validationTestResultControl1.StartMonitoringSecondaryInference(task);
                tabControl_Main.SelectedTab = tabPage_ValidationTest;

                MessageBox.Show(
                    $"二次推理任务已启动！\n" +
                    $"任务ID: {task.TaskId}\n" +
                    $"预计处理 {task.TotalRecords} 条记录\n\n" +
                    "请在【模型一致性测试】页查看进度和结果。",
                    "二次推理已启动",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"启动二次推理任务失败: {ex}");
                MessageBox.Show($"启动二次推理任务失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Enabled = true;
                label_LotTitle.Text = $"Lot: {lotNumber}";
            }
        }

        #endregion

        #region Data Operations


        private DefectReviewItem CreateDefectReviewItem(PanelDataRecord panel, SideData sideData)
        {
            // 从缺陷点中提取所有不重复的缺陷名称
            var defectNames = sideData.DetectPoints?
                .Where(dp => !string.IsNullOrEmpty(dp.DefectName))
                .Select(dp => dp.DefectName)
                .Distinct()
                .ToList();
            string defectNameStr = defectNames.Count > 0 ? string.Join(", ", defectNames) : "";

            return new DefectReviewItem
            {
                SerialNumber = panel.SerialNumber,
                LotNumber = panel.LotNumber ?? "未知Lot",
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
        /// 统计当前VVS状态和其他统计指标（包含报点级别和PCS级别的交叉统计）
        /// </summary>
        private void UpdateVvsStatusSummary()
        {
            // 重置所有计数器
            _vvsOkCount = 0;
            _vvsNgCount = 0;
            _vvsNotSetCount = 0;
            _aiOkVvsOkPointCount = 0;
            _aiOkVvsNgPointCount = 0;
            _aiNgVvsOkPointCount = 0;
            _aiNgVvsNgPointCount = 0;
            _aiOkVvsOkPcsCount = 0;
            _aiOkVvsNgPcsCount = 0;
            _aiNgVvsOkPcsCount = 0;
            _aiNgVvsNgPcsCount = 0;

            if (string.IsNullOrEmpty(_currentReviewLot)) return;

            // VVS 状态统计需要从当前数据实时计算（因为用户可能会修改 VVS 状态）
            foreach (var item in _allDefectItems)
            {
                if (item.LotNumber != _currentReviewLot) continue;
                if (item.HeatPoints == null) continue;

                bool isAiOkPcs = item.AiStatus == "OK";

                // 报点级别交叉统计
                foreach (var hp in item.HeatPoints)
                {
                    if (hp.VVSStatus == 0) _vvsNotSetCount++;
                    else if (hp.VVSStatus == 1) _vvsOkCount++;
                    else if (hp.VVSStatus == 2) _vvsNgCount++;

                    bool isAiOkPoint = hp.AIStatus == 1;
                    if (isAiOkPoint && hp.VVSStatus == 1) _aiOkVvsOkPointCount++;
                    else if (isAiOkPoint && hp.VVSStatus == 2) _aiOkVvsNgPointCount++;
                    else if (!isAiOkPoint && hp.VVSStatus == 1) _aiNgVvsOkPointCount++;
                    else if (!isAiOkPoint && hp.VVSStatus == 2) _aiNgVvsNgPointCount++;
                }

                // PCS级别交叉统计（需要有VVS结果的PCS才计入）
                bool hasVvsResult = item.HeatPoints.Any(hp => hp.VVSStatus != 0);
                if (hasVvsResult)
                {
                    bool pcsVvsOk = item.HeatPoints.All(hp => hp.VVSStatus == 0 || hp.VVSStatus == 1);
                    if (isAiOkPcs && pcsVvsOk) _aiOkVvsOkPcsCount++;
                    else if (isAiOkPcs && !pcsVvsOk) _aiOkVvsNgPcsCount++;
                    else if (!isAiOkPcs && pcsVvsOk) _aiNgVvsOkPcsCount++;
                    else if (!isAiOkPcs && !pcsVvsOk) _aiNgVvsNgPcsCount++;
                }
            }
        }

        /// <summary>
        /// 刷新复判详情的显示
        /// </summary>
        private void RefreshReviewDetailDisplay()
        {
            if (string.IsNullOrEmpty(_currentReviewLot))
            {
                label_ReviewDetail.Text = "请从左侧选择缺陷记录\n以查看复判详情";
                return;
            }

            var stat = _lotStatistics.TryGetValue(_currentReviewLot, out var s) ? s : new LotStatistics();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("【详情】");
            sb.AppendLine();

            // 1. 机台号  2. 料号  3. 工单
            sb.AppendLine($"机台号: {stat.MachineId ?? "-"}");
            sb.AppendLine($"料号: {stat.ProductSerial ?? "-"}");
            sb.AppendLine($"工单: {_currentReviewLot}");
            sb.AppendLine();

            // 4. 报点统计
            int totalPoints = stat.TotalPointCount;
            int aiOkPoints = stat.AiOkPointCount;
            int aiNgPoints = stat.AiNgPointCount;

            sb.AppendLine($"总报点数: {totalPoints}");
            sb.AppendLine($"  AI-OK报点数: {aiOkPoints}");
            sb.AppendLine($"    AI-OK&人工OK: {_aiOkVvsOkPointCount}");
            sb.AppendLine($"    AI-OK&人工NG: {_aiOkVvsNgPointCount}");
            sb.AppendLine($"  AI-NG报点数: {aiNgPoints}");
            sb.AppendLine($"    AI-NG&人工OK: {_aiNgVvsOkPointCount}");
            sb.AppendLine($"    AI-NG&人工NG: {_aiNgVvsNgPointCount}");
            sb.AppendLine($"  AI过滤率: {FormatPercent(aiOkPoints, totalPoints)}");
            sb.AppendLine($"  AI漏失率: {FormatPercent(_aiOkVvsNgPointCount, totalPoints)}");
            sb.AppendLine($"  AI准确率: {FormatPercent(_aiOkVvsOkPointCount + _aiNgVvsNgPointCount, totalPoints)}");
            sb.AppendLine();

            // 5. PCS统计
            int totalPcs = stat.TotalPcsCount;
            int aviOkPcs = stat.AviOkPcsCount;
            int aiOkPcs = stat.AiOkPcsCount;
            int aiNgPcs = stat.AiNgPcsCount;

            sb.AppendLine($"总PCS数: {totalPcs}");
            sb.AppendLine($"  AI-OK PCS数: {aiOkPcs}");
            sb.AppendLine($"    AI-OK&人工OK: {_aiOkVvsOkPcsCount}");
            sb.AppendLine($"    AI-OK&人工NG: {_aiOkVvsNgPcsCount}");
            sb.AppendLine($"  AI-NG PCS数: {aiNgPcs}");
            sb.AppendLine($"    AI-NG&人工OK: {_aiNgVvsOkPcsCount}");
            sb.AppendLine($"    AI-NG&人工NG: {_aiNgVvsNgPcsCount}");
            sb.AppendLine($"  AI PCS过滤率: {FormatPercent(aiOkPcs, totalPcs)}");
            sb.AppendLine($"  AI PCS通过率: {FormatPercent(aviOkPcs + aiOkPcs, totalPcs)}");
            sb.AppendLine($"  AI PCS漏失率: {FormatPercent(_aiOkVvsNgPcsCount, totalPcs)}");
            sb.AppendLine($"  AI PCS准确率: {FormatPercent(_aiOkVvsOkPcsCount + _aiNgVvsNgPcsCount, totalPcs)}");
            sb.AppendLine();

            // 6. & 7. Panel通过率
            sb.AppendLine($"Panel一次通过率: {FormatPercent(stat.AviOkPanelCount, stat.TotalPanelCount)}");
            sb.AppendLine($"Panel AI通过率: {FormatPercent(stat.AiPassPanelCount, stat.TotalPanelCount)}");
            sb.AppendLine();

            // 8-11. VVS状态统计
            sb.AppendLine($"VVS已判定总数: {_vvsOkCount + _vvsNgCount}");
            sb.AppendLine($"VVS未判定数量: {_vvsNotSetCount}");
            sb.AppendLine($"VVS判定OK数: {_vvsOkCount}");
            sb.Append($"VVS判定NG数: {_vvsNgCount}");

            label_ReviewDetail.Text = sb.ToString();
        }

        /// <summary>
        /// 格式化百分比显示
        /// </summary>
        private string FormatPercent(int numerator, int denominator)
        {
            if (denominator <= 0)
            {
                return "0.0%";
            }
            return ((double)numerator / denominator * 100).ToString("0.0") + "%";
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
                        Machine.master.SavePanelSide(record);

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

    /// <summary>
    /// Lot统计数据（包含所有面板，用于计算统计指标）
    /// </summary>
    public class LotStatistics
    {
        // 基本信息
        public string MachineId { get; set; }          // 机台号
        public string ProductSerial { get; set; }      // 料号

        // Panel(Array) 级别
        public int TotalPanelCount { get; set; }       // 总Panel数（去重后的panel）
        public int AviOkPanelCount { get; set; }       // AVI-OK Panel数（所有面均AVI OK）
        public int AiPassPanelCount { get; set; }      // AI后通过的Panel数（所有面均AVI-OK或AI-OK）

        // PCS 级别（每个SN+Side为一个PCS）
        public int TotalPcsCount { get; set; }         // 总PCS数
        public int AviOkPcsCount { get; set; }         // AVI-OK PCS数
        public int AiOkPcsCount { get; set; }          // AI-OK PCS数（AVI-NG中AI判OK）
        public int AiNgPcsCount { get; set; }          // AI-NG PCS数（AVI-NG中AI判NG）

        // 报点级别
        public int TotalPointCount { get; set; }       // 总报点数
        public int AiOkPointCount { get; set; }        // AI-OK报点数
        public int AiNgPointCount { get; set; }        // AI-NG报点数
    }

    #endregion
}

using DeepSightAI.Services;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class FrmAiReview : Form
    {
        /// <summary>
        /// 窗体对象实例
        /// </summary>
        private static FrmAiReview _instance;

        public static FrmAiReview Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrmAiReview();
                }

                return _instance;
            }
        }


        #region Fields

        private List<DefectReviewItem> _defectItems = new List<DefectReviewItem>(); // 当前 Lot 数据缓存（业务逻辑用）
        private List<DefectReviewItem> _allDefectItems = new List<DefectReviewItem>(); // 存储所有查询结果
        private Dictionary<string, List<DefectReviewItem>> _lotGroups = new Dictionary<string, List<DefectReviewItem>>(); // 按Lot分组
        private string _currentSelectedLot = null; // 当前选中的Lot

        // 复判详情相关字段
        private string _currentReviewLot = null;
        private DateTime _currentReviewTime = DateTime.MinValue;
        // 当前查看的单个SN项；非空时左侧详情面板显示该SN的具体信息，为空时显示Lot级聚合统计
        private DefectReviewItem _currentReviewSnItem = null;
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

        // VRS 报点级状态计数（动态计算，统计口径与 VVS 一致）
        private int _vrsOkCount = 0;
        private int _vrsNgCount = 0;
        private int _vrsNotSetCount = 0;

        // 报点级别 VRS 交叉统计（动态计算）
        // 注：_aiOkVrsNgPointCount/_aiNgVrsNgPointCount 沿用原 NG 口径（状态2+状态5），
        // 用于漏失率/准确率公式；显示时单独细分各 VRS 状态
        private int _aiOkVrsOkPointCount = 0;
        private int _aiOkVrsNgPointCount = 0;
        private int _aiNgVrsOkPointCount = 0;
        private int _aiNgVrsNgPointCount = 0;

        // VRS 各剩余状态交叉统计（报点级，动态计算）
        // VRS-无结论 = VRS未判定(0) + VRS忽略(3) + VRS无结果(4)，即未给出明确 OK/NG 结论的报点
        private int _aiOkVrsNoConclusionPointCount = 0; // AI-OK & VRS无结论(0/3/4)
        private int _aiOkVrsNgRejectPointCount = 0;     // AI-OK & VRS NG不接收(5)
        private int _aiNgVrsNoConclusionPointCount = 0; // AI-NG & VRS无结论(0/3/4)
        private int _aiNgVrsNgRejectPointCount = 0;     // AI-NG & VRS NG不接收(5)

        // PCS级别 VRS 交叉统计（动态计算）
        private int _aiOkVrsOkPcsCount = 0;
        private int _aiOkVrsNgPcsCount = 0;
        private int _aiNgVrsOkPcsCount = 0;
        private int _aiNgVrsNgPcsCount = 0;

        // 按Lot分组的统计数据（包含所有面板，用于计算统计指标）
        private Dictionary<string, LotStatistics> _lotStatistics = new Dictionary<string, LotStatistics>();

        // 自动保存相关字段
        private int _pendingSaveCount = 0;
        private const int AutoSaveBatchSize = 100;
        private bool _isFlushing = false;

        // 复判详情面板的样式常量（深色背景下使用，由 RichTextBox 的 SelectionFont/SelectionColor 应用）
        private static readonly Color DetailColorNormal = Color.White;
        private static readonly Color DetailColorMuted = Color.FromArgb(150, 165, 180);
        private static readonly Color DetailColorSection = Color.FromArgb(99, 197, 226);
        private static readonly Color DetailColorKpiTitle = Color.FromArgb(255, 205, 100);
        private static readonly Color DetailColorBad = Color.FromArgb(255, 107, 107);
        private static readonly Color DetailColorGood = Color.FromArgb(141, 218, 123);
        private static readonly Font DetailFontNormal = new Font("微软雅黑", 9F, FontStyle.Regular);
        private static readonly Font DetailFontSection = new Font("微软雅黑", 10F, FontStyle.Bold);
        private static readonly Font DetailFontKpiLabel = new Font("微软雅黑", 9F, FontStyle.Bold);
        private static readonly Font DetailFontKpiValue = new Font("微软雅黑", 14F, FontStyle.Bold);

        #endregion

        #region Constructor

        public FrmAiReview()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            InitializeControl();
        }

        private void InitializeControl()
        {
            // 订阅查询控件事件
            UcDefectQuery.QueryClicked += HeatMapQueryControl_QueryClicked;
            UcDefectQuery.FilterChanged += QueryControl_FilterChanged;
            // Lot 下拉框选择变化 → 切换 DGV 显示对应 Lot 的明细
            UcDefectQuery.LotComboBox.SelectedIndexChanged += LotComboBox_SelectedIndexChanged;

            // 将共享的查询控件绑定到热力图，避免重复 UI 与重复查询
            heatMapControl1.BindQuerySource(UcDefectQuery);

            // 订阅缺陷列表面板事件
            ucDefectListPanel.ItemDoubleClicked += UcDefectListPanel_ItemDoubleClicked;
            ucDefectListPanel.RunTestClicked += ToolStripMenuItem_RunTest_Click;
            ucDefectListPanel.SecondaryInferenceClicked += ToolStripMenuItem_SecondaryInference_Click;
            ucDefectListPanel.AddToDatasetClicked += ToolStripMenuItem_AddToDataset_Click;

            // 订阅详情控件的切换下一行事件
            defectDetailControl1.SelectNextRowRequested += DefectDetailControl_SelectNextRowRequested;

            // 订阅VVS复判完成事件 - 当某SN所有缺陷点都完成VVS复判时自动更新人工判定状态
            defectDetailControl1.SnVvsCompleted += DefectDetailControl_SnVvsCompleted;

            // 订阅VVS状态改变事件 - 用于更新左下角复判详情显示
            defectDetailControl1.VvsStatusChanged += DefectDetailControl_VvsStatusChanged;

            // 订阅单图测试事件
            defectDetailControl1.SingleImageTestRequested += DefectDetailControl_SingleImageTestRequested;

            // 订阅页面切换事件 - 页面跳转时自动保存
            tabControl_Main.SelectedIndexChanged += TabControl_Main_SelectedIndexChanged;

            // 订阅导出事件 - DefectDetailControl中的导出按钮
            defectDetailControl1.ExportRequested += DefectDetailControl_ExportRequested;
        }

        #endregion

        #region Event Handlers

        private void HeatMapQueryControl_QueryClicked(object sender, EventArgs e)
        {
            // 验证输入
            if (!UcDefectQuery.ValidateInputs(out string errorMessage))
            {
                MessageBox.Show(errorMessage, "输入验证", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RunQueryWithProgress("正在加载查询结果...", isFilterChange: false);
        }

        /// <summary>
        /// 筛选条件变化事件处理（料号或机台号选择变化时自动筛选）
        /// </summary>
        private void QueryControl_FilterChanged(object sender, EventArgs e)
        {
            RunQueryWithProgress("正在应用筛选条件...", isFilterChange: true);
        }

        /// <summary>
        /// 以模态进度对话框包裹查询/筛选后的处理流程，期间禁用所有其他操作。
        /// </summary>
        private void RunQueryWithProgress(string title, bool isFilterChange)
        {
            using (var dlg = new DlgQueryProgress(title))
            {
                dlg.WorkAsync = (progress, ct) => ExecuteQueryPipelineAsync(progress, ct, isFilterChange);
                dlg.ShowDialog(this.FindForm() ?? (Form)this);

                if (dlg.Error != null)
                {
                    string msg = isFilterChange ? "筛选失败，请检查日志。" : "查询失败，请检查日志。";
                    MessageBox.Show(msg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 查询/筛选的核心处理流程，UI 操作发生在 UI 线程，重计算工作在 Task.Run 中执行。
        /// 在关键节点检查 CancellationToken，支持用户中途取消。
        /// </summary>
        private async Task ExecuteQueryPipelineAsync(IProgress<string> progress, CancellationToken ct, bool isFilterChange)
        {
            progress.Report("正在清理上次结果...");
            _defectItems.Clear();
            _allDefectItems.Clear();
            _lotGroups.Clear();
            _lotStatistics.Clear();
            defectDetailControl1.ClearDetails();
            _currentSelectedLot = null;
            ct.ThrowIfCancellationRequested();

            // 两段式加载：按日期查询第一阶段仅取了 Lot 列表，明细推迟到用户选 Lot 后再加载
            if (UcDefectQuery.IsLotListOnly)
            {
                progress.Report("已加载 Lot 列表，请从下拉框选择具体 Lot 加载明细...");
                ucDefectListPanel.ClearItems("请从下拉框选择 Lot 加载明细");
                _defectItems.Clear();
                paretoChart1?.Clear("请从下拉框选择 Lot 加载明细");
                _currentReviewLot = null;
                _currentReviewSnItem = null;
                RefreshReviewDetailDisplay();
                return;
            }

            progress.Report("正在筛选数据...");
            var queryResult = UcDefectQuery.GetQueryResult();
            var panelList = queryResult as IList<PanelDataRecord> ?? queryResult.ToList();
            ct.ThrowIfCancellationRequested();

            progress.Report($"正在统计 {panelList.Count} 条记录...");
            var statistics = await Task.Run(() => LotStatisticsCalculator.Calculate(panelList), ct);
            _lotStatistics = statistics;
            ct.ThrowIfCancellationRequested();

            bool onlyAviNg = UcDefectQuery.OnlyAviNg;
            progress.Report("正在构建缺陷列表...");
            var collected = await Task.Run(() =>
            {
                var list = new List<DefectReviewItem>();
                foreach (var panel in panelList)
                {
                    ct.ThrowIfCancellationRequested();
                    if (panel.Sides == null) continue;
                    foreach (var side in panel.Sides)
                    {
                        if (side == null) continue;
                        if (!onlyAviNg || side.AviState != 1)
                            list.Add(CreateDefectReviewItem(panel, side));
                    }
                }
                return list;
            }, ct);
            _allDefectItems.AddRange(collected);
            ct.ThrowIfCancellationRequested();

            progress.Report("正在按 Lot 分组...");
            _lotGroups = _allDefectItems
                .GroupBy(x => x.LotNumber ?? "未知Lot")
                .ToDictionary(g => g.Key, g => g.ToList());

            progress.Report("正在更新界面...");
            // 先把 DGV 复位到"等待选 Lot"占位状态；BuildLotTreeNodes 内若命中单 Lot 会通过 SwitchToLot 填充明细
            string placeholder = _lotGroups.Count == 0 ? "未查询到任何数据" : "请从下拉框选择 Lot 加载明细";
            ucDefectListPanel.ClearItems(placeholder);
            _defectItems.Clear();
            BuildLotTreeNodes();

            // 若 BuildLotTreeNodes 内的 SyncLotComboBox 已通过 SwitchToLot 选定了 Lot 并刷新过详情，则直接保留
            if (!string.IsNullOrEmpty(_currentReviewLot) && _lotStatistics.ContainsKey(_currentReviewLot))
            {
                if (!isFilterChange) _currentReviewSnItem = null;
                UpdateVvsStatusSummary();
            }
            else
            {
                _currentReviewLot = null;
                _currentReviewSnItem = null;
            }
            RefreshReviewDetailDisplay();
        }

        private void UcDefectListPanel_ItemDoubleClicked(DefectReviewItem selectedItem)
        {
            if (selectedItem == null) return;
            // 先切换到详情页，确保面板布局正确后再加载图片
            tabControl_Main.SelectedTab = tabPage_Details;
            defectDetailControl1.DisplayDefectDetails(selectedItem);
            EnterSnReviewMode(selectedItem);
        }

        private void DefectDetailControl_SelectNextRowRequested(object sender, EventArgs e)
        {
            var selectedItem = ucDefectListPanel.SelectNextRow();
            if (selectedItem != null)
            {
                defectDetailControl1.DisplayDefectDetails(selectedItem);
                EnterSnReviewMode(selectedItem);
            }
        }

        /// <summary>
        /// 进入单SN复判模式：更新当前Lot和SN上下文，刷新左侧详情面板显示该SN的具体信息
        /// </summary>
        private void EnterSnReviewMode(DefectReviewItem item)
        {
            if (item == null) return;
            _currentReviewSnItem = item;
            _currentReviewLot = item.LotNumber;
            _currentReviewTime = item.DetectionDate;
            UpdateVvsStatusSummary();
            RefreshReviewDetailDisplay();
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
            ucDefectListPanel.ResetBindings();
            
            // 更新左下角复判详情
            UpdateVvsStatusSummary();
        }

        /// <summary>
        /// 当VVS状态改变时触发，用于更新左下角复判详情并执行自动保存动作
        /// </summary>
        private async void DefectDetailControl_VvsStatusChanged(object sender, VvsStatusChangedEventArgs e)
        {
            // 如果当前有展示的详情，重新统计VVS状态
            if (_currentReviewLot != null)
            {
                UpdateVvsStatusSummary();
                RefreshReviewDetailDisplay();
            }

            // 精确标记被修改的缺陷点所属的 DefectReviewItem
            MarkModifiedItem(e?.ModifiedHeatPoint);
            _pendingSaveCount++;

            // 积攒满100次时批量保存到数据库
            if (_pendingSaveCount >= AutoSaveBatchSize)
            {
                await FlushPendingSaves();
            }
        }

        /// <summary>
        /// 根据被修改的缺陷点，精确标记其所属的 DefectReviewItem 为已修改
        /// </summary>
        private void MarkModifiedItem(DetectInfo modifiedHeatPoint)
        {
            if (modifiedHeatPoint == null || _allDefectItems == null) return;

            foreach (var item in _allDefectItems)
            {
                if (item.HeatPoints != null && item.HeatPoints.Contains(modifiedHeatPoint))
                {
                    item.IsModified = true;
                    return; // 一个缺陷点只属于一个 item，找到即可返回
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

                    string message = $"单图测试完成！\n\n" +
                        $"原始AI结果: {originalStatusText}\n" +
                        $"新AI结果: {newStatusText}\n" +
                        $"比对结果: {consistentText}";

                    // 追加复判详情
                    if (!string.IsNullOrEmpty(result.InferDetailText))
                    {
                        message += $"\n\n--- 复判详情 ---\n{result.InferDetailText}";
                    }
                    else if (!string.IsNullOrEmpty(result.DefectName))
                    {
                        message += $"\n\n缺陷名称: {result.DefectName}";
                        if (!string.IsNullOrEmpty(result.DefectArea))
                            message += $"\n缺陷面积: {result.DefectArea}";
                    }

                    MessageBox.Show(
                        message,
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
        private DefectReviewItem FindSourceItemForHeatPoint(DetectInfo heatPoint)
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
                    DefectReviewItem currentItem = ucDefectListPanel.CurrentItem;

                    if (currentItem == null)
                    {
                        MessageBox.Show("请先选择一条记录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 先弹出导出选项对话框，让用户选择导出原图、模板图、Gerber 图
                    bool exportOriginal;
                    bool exportTemplate;
                    bool exportGerber;
                    using (var optionsDialog = new DlgExportOptions())
                    {
                        if (optionsDialog.ShowDialog() != DialogResult.OK)
                            return;
                        exportOriginal = optionsDialog.ExportOriginalImage;
                        exportTemplate = optionsDialog.ExportTemplateImage;
                        exportGerber = optionsDialog.ExportGerberImage;
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

                            // 在 UI 线程上捕获复判详情文本，供后台线程写入 CSV
                            string reviewDetailText = label_ReviewDetail?.Text ?? string.Empty;

                            await Task.Run(() =>
                            {
                                // 从Minio加载图片并保存到本地
                                defectDetailControl1.ExportImages(exportPath, exportOriginal, exportTemplate, exportGerber);

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

                                // 复判详情（label_ReviewDetail 内容）
                                csvLines.Add("");
                                csvLines.Add("复判详情");
                                csvLines.Add("项目,数值");
                                AppendReviewDetailToCsv(csvLines, reviewDetailText);

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
        /// Lot 下拉框选择变化事件 - 切换显示对应 Lot 的明细。
        /// 仅在 _lotGroups 中包含该 Lot 时生效，手动输入未查询过的 Lot 不会误触发。
        /// </summary>
        private async void LotComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string lotNumber = UcDefectQuery?.LotNumber?.Trim();
            if (string.IsNullOrEmpty(lotNumber)) return;

            // 已加载明细：直接切换显示
            if (_lotGroups.ContainsKey(lotNumber))
            {
                SwitchToLot(lotNumber);
                return;
            }

            // 未加载明细（两段式查询第一阶段刚选中 Lot）：触发明细查询
            // cmb_Lot.Text 此时已为目标 Lot，QueryDataAsync 走 Lot 号分支取明细
            await UcDefectQuery.QueryDataAsync();
            RunQueryWithProgress("正在加载 Lot 明细...", isFilterChange: false);
        }

        /// <summary>
        /// 切换到指定 Lot：加载明细到 DGV、刷新帕累托/热力图与左侧详情面板。
        /// </summary>
        private void SwitchToLot(string lotNumber)
        {
            if (string.IsNullOrEmpty(lotNumber)) return;
            if (!_lotGroups.TryGetValue(lotNumber, out var lotItems)) return;

            LoadLotData(lotNumber);
            _currentReviewLot = lotNumber;
            _currentReviewSnItem = null;

            // 联动帕累托图与热力图（两段式第二阶段没有 QueryClicked 事件，需显式驱动）
            paretoChart1?.ShowLot(lotNumber, lotItems);
            _ = heatMapControl1?.RefreshForCurrentQueryAsync();

            UpdateVvsStatusSummary();
            RefreshReviewDetailDisplay();
        }

        /// <summary>
        /// 查询完成后同步 Lot 列表到查询控件下拉框，便于通过 cmb_Lot 选择切换 Lot。
        /// </summary>
        private void BuildLotTreeNodes()
        {
            SyncLotComboBox();
        }

        /// <summary>
        /// 把当前 _lotGroups 的 Lot 列表写入 UcDefectQuery 的下拉框，并自动选中"当前 Lot"。
        /// 仅一个 Lot 时直接选中并触发切换；多个 Lot 时保留用户操作空间。
        /// </summary>
        private void SyncLotComboBox()
        {
            if (UcDefectQuery == null) return;

            // 两段式查询第一阶段：cmb_Lot 已由 UcDefectQuery 填好完整 Lot 列表，不再覆盖
            if (UcDefectQuery.IsLotListOnly) return;

            var lots = _lotGroups.Keys.OrderBy(k => k).ToList();
            var combo = UcDefectQuery.LotComboBox;

            // 临时摘下事件，避免 SetLotItems / 自动选中过程触发 SelectedIndexChanged 重入
            combo.SelectedIndexChanged -= LotComboBox_SelectedIndexChanged;
            try
            {
                // 若 cmb_Lot 已包含本次 Lot 集的全部项（两段式后二次明细查询），保留更大的列表
                bool cmbContainsAll = lots.Count > 0 && lots.All(l => combo.Items.Contains(l));
                if (!cmbContainsAll)
                {
                    UcDefectQuery.SetLotItems(lots);
                }

                // 单 Lot 场景（按 Lot 号查询或日期只命中一个 Lot）：自动选中并切换
                if (lots.Count == 1)
                {
                    if (!string.Equals(combo.Text, lots[0], StringComparison.Ordinal))
                    {
                        combo.SelectedItem = lots[0];
                    }
                    SwitchToLot(lots[0]);
                }
                // 多 Lot 场景：若用户输入的 Lot 正好在结果集中，则保持选中
                else if (lots.Count > 1 && !string.IsNullOrEmpty(combo.Text) && lots.Contains(combo.Text))
                {
                    SwitchToLot(combo.Text);
                }
            }
            finally
            {
                combo.SelectedIndexChanged += LotComboBox_SelectedIndexChanged;
            }
        }

        /// <summary>
        /// 加载指定Lot的所有数据到表格
        /// </summary>
        private void LoadLotData(string lotNumber)
        {
            if (!_lotGroups.TryGetValue(lotNumber, out var items)) return;

            _currentSelectedLot = lotNumber;
            RefreshDataGridView(items);
        }

        /// <summary>
        /// 取当前激活 Lot 的明细数据。优先使用 UcDefectQuery.LotNumber（用户选择/输入的 Lot），
        /// 回退到 _currentSelectedLot（左侧 TreeView 选中的 Lot，过渡期保留）。
        /// </summary>
        /// <param name="dataKindHint">数据类型提示词（如"可测试"/"可推理"），用于错误提示拼接，可为 null。</param>
        private bool TryGetActiveLotData(string dataKindHint, out string lotNumber, out List<DefectReviewItem> lotItems)
        {
            lotNumber = (UcDefectQuery?.LotNumber ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(lotNumber))
                lotNumber = _currentSelectedLot;

            if (string.IsNullOrEmpty(lotNumber))
            {
                MessageBox.Show("请先选择一个 Lot。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                lotItems = null;
                return false;
            }

            if (!_lotGroups.TryGetValue(lotNumber, out lotItems) || lotItems.Count == 0)
            {
                string suffix = string.IsNullOrEmpty(dataKindHint) ? "数据" : $"{dataKindHint}的数据";
                MessageBox.Show($"Lot {lotNumber} 中没有{suffix}。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 运行模型一致性测试菜单项点击事件
        /// </summary>
        private async void ToolStripMenuItem_RunTest_Click(object sender, EventArgs e)
        {
            if (!TryGetActiveLotData("可测试", out string lotNumber, out var lotItems))
                return;

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
                LogTextHelper.Info($"正在启动 Lot: {lotNumber} 的测试任务...");

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
            }
        }

        /// <summary>
        /// 添加到一致性测试数据集菜单项点击事件
        /// </summary>
        private void ToolStripMenuItem_AddToDataset_Click(object sender, EventArgs e)
        {
            if (!TryGetActiveLotData(null, out string lotNumber, out var lotItems))
                return;

            try
            {
                var dataStore = new ConsistencyTestDataStore();
                var datasets = dataStore.LoadDatasets();

                using (var dialog = new DlgAddToDataset(datasets, lotNumber, lotItems))
                {
                    if (dialog.ShowDialog(this.FindForm()) == DialogResult.OK)
                    {
                        // 保存更新后的数据集
                        dataStore.SaveDataset(dialog.SelectedDataset);
                        // 刷新看板
                        consistencyTestDashboard1?.LoadDatasets();
                        MessageBox.Show(
                            $"已将 Lot: {lotNumber} ({lotItems.Count} 条记录) 添加到数据集 \"{dialog.SelectedDataset.Name}\"。",
                            "添加成功",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"添加到数据集失败: {ex}");
                MessageBox.Show($"添加到数据集失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 运行二次推理菜单项点击事件
        /// </summary>
        private async void ToolStripMenuItem_SecondaryInference_Click(object sender, EventArgs e)
        {
            if (!TryGetActiveLotData("可推理", out string lotNumber, out var lotItems))
                return;

            // 统计 AI NG 的数量
            int ngCount = lotItems.Count(i => i.AiStatus == "NG");
            //if (ngCount == 0)
            //{
            //    MessageBox.Show($"Lot {lotNumber} 中没有 AI NG 的数据，无需二次推理。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

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
                LogTextHelper.Info($"正在启动 Lot: {lotNumber} 的二次推理任务...");

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
            }
        }

        #endregion

        #region Data Operations

        /// <summary>
        /// 刷新缺陷列表面板的数据（同步 _defectItems 缓存并通知 UC 重绘）
        /// </summary>
        private void RefreshDataGridView(List<DefectReviewItem> items = null)
        {
            _defectItems.Clear();
            if (items != null)
                _defectItems.AddRange(items);
            ucDefectListPanel.LoadItems(_defectItems);
        }

        /// <summary>
        /// 遍历面板列表，填充 _lotStatistics 和 _allDefectItems，并重建 _lotGroups 及 TreeView 节点。
        /// 供查询和筛选两个事件处理器共用，消除重复代码。
        /// </summary>
        private void CollectPanelData(IEnumerable<PanelDataRecord> panels)
        {
            var panelList = panels as IList<PanelDataRecord> ?? panels.ToList();

            // 使用 LotStatisticsCalculator 计算统计指标（可独立单元测试）
            _lotStatistics = LotStatisticsCalculator.Calculate(panelList);

            // 构建缺陷明细列表
            bool onlyAviNg = UcDefectQuery.OnlyAviNg;
            foreach (var panel in panelList)
            {
                if (panel.Sides == null) continue;
                foreach (var side in panel.Sides)
                {
                    if (side == null) continue;
                    // 根据复选框决定是否仅加入 AVI NG 数据
                    if (!onlyAviNg || side.AviState != 1)
                        _allDefectItems.Add(CreateDefectReviewItem(panel, side));
                }
            }

            // 按Lot分组
            _lotGroups = _allDefectItems
                .GroupBy(x => x.LotNumber ?? "未知Lot")
                .ToDictionary(g => g.Key, g => g.ToList());

            // 构建TreeView节点
            BuildLotTreeNodes();
        }

        private DefectReviewItem CreateDefectReviewItem(PanelDataRecord panel, SideData sideData)
        {
            // 从缺陷点中提取所有不重复的缺陷名称
            var defectNames = sideData.DetectPoints?
                .Where(dp => !string.IsNullOrEmpty(dp.DefectName))
                .Select(dp => dp.DefectName)
                .Distinct()
                .ToList();
            string defectNameStr = defectNames?.Count > 0 ? string.Join(", ", defectNames) : "";

            return new DefectReviewItem
            {
                SerialNumber = panel.SerialNumber,
                LotNumber = panel.LotNumber ?? "未知Lot",
                MachineId = panel.MachineId,
                ProductSerial = panel.ProductSerial,
                Side = sideData.Side,
                AviStatus = GetStatusTextStatic(sideData.AviState),
                AiStatus = GetStatusTextStatic(sideData.AiState),
                OriginalAviState = sideData.AviState,
                OriginalAiState = sideData.AiState,
                ManualStatus = sideData.VvsState == 0 ? "未判定" : sideData.VvsState == 1 ? "OK" : "NG",
                VrsStatus = GetVrsStatusText(sideData.VrsState),
                DefectCount = sideData.DetectPoints?.Count ?? 0,
                DefectName = defectNameStr,
                DetectionDate = panel.DetectionDate,
                HeatPoints = sideData.DetectPoints,
                IsModified = false
            };
        }

        /// <summary>
        /// 将复判详情文本（label_ReviewDetail.Text）按"项目,数值"两列形式追加到 CSV
        /// </summary>
        private static void AppendReviewDetailToCsv(List<string> csvLines, string reviewDetailText)
        {
            if (string.IsNullOrEmpty(reviewDetailText))
                return;

            var rawLines = reviewDetailText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            foreach (var rawLine in rawLines)
            {
                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    csvLines.Add("");
                    continue;
                }

                // 优先按中文冒号切分，其次按英文冒号
                int sepIdx = rawLine.IndexOf('：');
                int sepLen = 1;
                if (sepIdx < 0)
                {
                    sepIdx = rawLine.IndexOf(':');
                    sepLen = 1;
                }

                string keyCell;
                string valueCell;
                if (sepIdx > 0)
                {
                    keyCell = rawLine.Substring(0, sepIdx).TrimEnd();
                    valueCell = rawLine.Substring(sepIdx + sepLen).Trim();
                }
                else
                {
                    keyCell = rawLine.TrimEnd();
                    valueCell = string.Empty;
                }

                csvLines.Add($"{EscapeCsvField(keyCell)},{EscapeCsvField(valueCell)}");
            }
        }

        /// <summary>
        /// CSV 字段转义：包含逗号、引号或换行时，整体加引号并把内部引号双写
        /// </summary>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;
            if (field.IndexOf(',') >= 0 || field.IndexOf('"') >= 0 || field.IndexOf('\n') >= 0 || field.IndexOf('\r') >= 0)
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
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
        /// 将 VRS 状态码转换为显示文本
        /// 约定：0=未判定, 1=OK, 2=NG, 3=忽略, 4=无结果, 5=NG不接收
        /// </summary>
        private static string GetVrsStatusText(int vrsState)
        {
            switch (vrsState)
            {
                case 0: return "未判定";
                case 1: return "OK";
                case 2: return "NG";
                case 3: return "忽略";
                case 4: return "无结果";
                case 5: return "NG不接收";
                default: return vrsState.ToString();
            }
        }

        private static bool IsAiOkStatus(int status)
        {
            return status == 1;
        }

        private static bool IsAiNgStatus(int status)
        {
            return status == 2;
        }


        /// <summary>
        /// 统计当前VVS/VRS状态和其他统计指标（包含报点级别和PCS级别的交叉统计）
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

            _vrsOkCount = 0;
            _vrsNgCount = 0;
            _vrsNotSetCount = 0;
            _aiOkVrsOkPointCount = 0;
            _aiOkVrsNgPointCount = 0;
            _aiNgVrsOkPointCount = 0;
            _aiNgVrsNgPointCount = 0;
            _aiOkVrsNoConclusionPointCount = 0;
            _aiOkVrsNgRejectPointCount = 0;
            _aiNgVrsNoConclusionPointCount = 0;
            _aiNgVrsNgRejectPointCount = 0;
            _aiOkVrsOkPcsCount = 0;
            _aiOkVrsNgPcsCount = 0;
            _aiNgVrsOkPcsCount = 0;
            _aiNgVrsNgPcsCount = 0;

            if (string.IsNullOrEmpty(_currentReviewLot)) return;

            // VVS/VRS 状态统计需要从当前数据实时计算（因为用户可能会修改 VVS 状态）
            foreach (var item in _allDefectItems)
            {
                if (item.LotNumber != _currentReviewLot) continue;
                if (item.HeatPoints == null) continue;

                // 报点级别交叉统计
                foreach (var hp in item.HeatPoints)
                {
                    if (hp.VVSStatus == 0) _vvsNotSetCount++;
                    else if (hp.VVSStatus == 1) _vvsOkCount++;
                    else if (hp.VVSStatus == 2) _vvsNgCount++;

                    bool isAiOkPoint = IsAiOkStatus(hp.AIStatus);
                    bool isAiNgPoint = IsAiNgStatus(hp.AIStatus);
                    if (isAiOkPoint && hp.VVSStatus == 1) _aiOkVvsOkPointCount++;
                    else if (isAiOkPoint && hp.VVSStatus == 2) _aiOkVvsNgPointCount++;
                    else if (isAiNgPoint && hp.VVSStatus == 1) _aiNgVvsOkPointCount++;
                    else if (isAiNgPoint && hp.VVSStatus == 2) _aiNgVvsNgPointCount++;

                    // VRS 报点级统计（NG 包含 2=NG 与 5=NG不接收）
                    bool isVrsOkPoint = IsVrsOkPoint(hp.VrsState);
                    bool isVrsNgPoint = IsVrsNgPoint(hp.VrsState);
                    if (isVrsOkPoint) _vrsOkCount++;
                    else if (isVrsNgPoint) _vrsNgCount++;
                    else if (hp.VrsState == 0) _vrsNotSetCount++;

                    if (isAiOkPoint && isVrsOkPoint) _aiOkVrsOkPointCount++;
                    else if (isAiOkPoint && isVrsNgPoint) _aiOkVrsNgPointCount++;
                    else if (isAiNgPoint && isVrsOkPoint) _aiNgVrsOkPointCount++;
                    else if (isAiNgPoint && isVrsNgPoint) _aiNgVrsNgPointCount++;

                    // VRS 各剩余状态细分（与 AI-OK/AI-NG 交叉）
                    if (isAiOkPoint)
                    {
                        switch (hp.VrsState)
                        {
                            case 0:
                            case 3:
                            case 4: _aiOkVrsNoConclusionPointCount++; break;
                            case 5: _aiOkVrsNgRejectPointCount++; break;
                        }
                    }
                    else if (isAiNgPoint)
                    {
                        switch (hp.VrsState)
                        {
                            case 0:
                            case 3:
                            case 4: _aiNgVrsNoConclusionPointCount++; break;
                            case 5: _aiNgVrsNgRejectPointCount++; break;
                        }
                    }
                }

                // PCS级别交叉统计：按 PcsIndex 分组，同一 PcsIndex 为同一片 PCS（需有结果才计入）
                var pcsGroups = item.HeatPoints.GroupBy(hp => hp.PcsIndex);
                foreach (var pcsGroup in pcsGroups)
                {
                    var pts = pcsGroup.ToList();

                    // 根据该 PCS 内各点的 AIStatus 判断 PCS 的 AI 状态
                    bool pcsAiOk = pts.All(p => p.AIStatus == 1);
                    bool pcsAiNg = pts.Any(p => p.AIStatus == 2);

                    // VVS PCS 级别（需有VVS结果才计入）
                    bool hasVvsResult = pts.Any(p => p.VVSStatus != 0);
                    if (hasVvsResult)
                    {
                        // 有任意一点 VVS 判定为 NG，则该 PCS 为人工 NG
                        bool pcsVvsOk = pts.All(p => p.VVSStatus == 0 || p.VVSStatus == 1);
                        if (pcsAiOk && pcsVvsOk) _aiOkVvsOkPcsCount++;
                        else if (pcsAiOk && !pcsVvsOk) _aiOkVvsNgPcsCount++;
                        else if (pcsAiNg && pcsVvsOk) _aiNgVvsOkPcsCount++;
                        else if (pcsAiNg && !pcsVvsOk) _aiNgVvsNgPcsCount++;
                    }

                    // VRS PCS 级别（需有VRS结果才计入）
                    bool hasVrsResult = pts.Any(p => p.VrsState != 0);
                    if (hasVrsResult)
                    {
                        // 有任意一点 VRS 判定为 NG(2/5)，则该 PCS 为 VRS NG
                        bool pcsVrsNg = pts.Any(p => IsVrsNgPoint(p.VrsState));
                        bool pcsVrsOk = !pcsVrsNg;
                        if (pcsAiOk && pcsVrsOk) _aiOkVrsOkPcsCount++;
                        else if (pcsAiOk && pcsVrsNg) _aiOkVrsNgPcsCount++;
                        else if (pcsAiNg && pcsVrsOk) _aiNgVrsOkPcsCount++;
                        else if (pcsAiNg && pcsVrsNg) _aiNgVrsNgPcsCount++;
                    }
                }
            }
        }

        private static bool IsVrsOkPoint(int vrsState)
        {
            return vrsState == 1;
        }

        private static bool IsVrsNgPoint(int vrsState)
        {
            return vrsState == 2 || vrsState == 5;
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

            // 单SN模式：显示该SN的具体信息和统计
            if (_currentReviewSnItem != null)
            {
                label_ReviewDetail.Text = BuildSnReviewDetailText(_currentReviewSnItem);
                return;
            }

            var stat = _lotStatistics.TryGetValue(_currentReviewLot, out var s) ? s : new LotStatistics();
            RenderLotReviewDetail(stat);
        }

        /// <summary>
        /// 向复判详情面板追加一段带样式的文本（颜色/字体）
        /// </summary>
        private void AppendDetail(string text, Font font, Color color)
        {
            label_ReviewDetail.SelectionStart = label_ReviewDetail.TextLength;
            label_ReviewDetail.SelectionLength = 0;
            label_ReviewDetail.SelectionAlignment = HorizontalAlignment.Left;
            label_ReviewDetail.SelectionFont = font;
            label_ReviewDetail.SelectionColor = color;
            label_ReviewDetail.AppendText(text);
        }

        /// <summary>
        /// 渲染 Lot 聚合视图的复判详情：核心 KPI 卡片置顶，分组明细在下；
        /// 利用 RichTextBox 富文本能力按重要性分配颜色/字号，使关键指标一眼可见。
        /// </summary>
        private void RenderLotReviewDetail(LotStatistics stat)
        {
            int totalPoints = stat.TotalPointCount;
            int totalPcs = stat.TotalPcsCount;

            // VRS-NG 中纯状态2(NG) 的数量 = 合并NG - NG不接收(状态5)
            int aiOkVrsNgPureCount = _aiOkVrsNgPointCount - _aiOkVrsNgRejectPointCount;
            int aiNgVrsNgPureCount = _aiNgVrsNgPointCount - _aiNgVrsNgRejectPointCount;
            int aiOkVrsNoConclusion = _aiOkVrsNoConclusionPointCount + _aiOkVrsNgRejectPointCount;
            int aiNgVrsNoConclusion = _aiNgVrsNoConclusionPointCount + _aiNgVrsNgRejectPointCount;

            label_ReviewDetail.Clear();

            // ===== 核心 KPI 卡片（最显眼）=====
            AppendDetail("◆ 核心指标 (VRS - 报点)\n", DetailFontSection, DetailColorKpiTitle);
            AppendDetail("  AI 漏失率   ", DetailFontKpiLabel, DetailColorMuted);
            AppendDetail($"{FormatPercent(_aiOkVrsNgPointCount, totalPoints)}\n", DetailFontKpiValue, DetailColorBad);
            AppendDetail("  AI 准确率   ", DetailFontKpiLabel, DetailColorMuted);
            AppendDetail($"{FormatPercent(_aiOkVrsOkPointCount + _aiNgVrsNgPointCount, totalPoints)}\n", DetailFontKpiValue, DetailColorGood);
            AppendDetail("  Panel通过率 ", DetailFontKpiLabel, DetailColorMuted);
            AppendDetail($"{FormatPercent(stat.AviOkPanelCount, stat.TotalPanelCount)}\n\n", DetailFontKpiValue, DetailColorGood);

            // ===== 基本信息 =====
            AppendDetail("▶ 基本信息\n", DetailFontSection, DetailColorSection);
            AppendDetail($"  机台号 : {stat.MachineId ?? "-"}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  料号   : {stat.ProductSerial ?? "-"}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  工单   : {_currentReviewLot}\n\n", DetailFontNormal, DetailColorNormal);

            // ===== 报点统计 =====
            AppendDetail($"▶ 报点统计  (共 {totalPoints} 点)\n", DetailFontSection, DetailColorSection);
            AppendDetail($"  AI-OK     : {stat.AiOkPointCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-NG     : {stat.AiNgPointCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-异常   : {stat.AiExceptionPointCount}\n", DetailFontNormal, DetailColorMuted);
            AppendDetail($"  AI-未检测 : {stat.AiUninspectedPointCount}\n", DetailFontNormal, DetailColorMuted);
            AppendDetail($"  AI过滤率  : {FormatPercent(stat.AiOkPointCount, totalPoints)}\n\n", DetailFontNormal, DetailColorNormal);

            // ===== VRS 明细（报点）=====
            AppendDetail("▶ VRS明细 (报点)\n", DetailFontSection, DetailColorSection);
            AppendDetail($"  AI-OK & VRS-OK     : {_aiOkVrsOkPointCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-OK & VRS-NG     : {aiOkVrsNgPureCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-OK & VRS-无结论 : {aiOkVrsNoConclusion}\n", DetailFontNormal, DetailColorMuted);
            AppendDetail($"  AI-NG & VRS-OK     : {_aiNgVrsOkPointCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-NG & VRS-NG     : {aiNgVrsNgPureCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-NG & VRS-无结论 : {aiNgVrsNoConclusion}\n\n", DetailFontNormal, DetailColorMuted);

            // ===== VVS 明细（报点）=====
            AppendDetail("▶ VVS明细 (报点)\n", DetailFontSection, DetailColorSection);
            AppendDetail($"  AI-OK & VVS-OK : {_aiOkVvsOkPointCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-OK & VVS-NG : {_aiOkVvsNgPointCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-NG & VVS-OK : {_aiNgVvsOkPointCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-NG & VVS-NG : {_aiNgVvsNgPointCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI漏失率(VVS)  : {FormatPercent(_aiOkVvsNgPointCount, totalPoints)}\n", DetailFontNormal, DetailColorBad);
            AppendDetail($"  AI准确率(VVS)  : {FormatPercent(_aiOkVvsOkPointCount + _aiNgVvsNgPointCount, totalPoints)}\n\n", DetailFontNormal, DetailColorGood);

            // ===== PCS 统计 =====
            AppendDetail($"▶ PCS统计  (共 {totalPcs} 片)\n", DetailFontSection, DetailColorSection);
            AppendDetail($"  AI-OK     : {stat.AiOkPcsCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-NG     : {stat.AiNgPcsCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-异常   : {stat.AiExceptionPcsCount}\n", DetailFontNormal, DetailColorMuted);
            AppendDetail($"  AI-未检测 : {stat.AiUninspectedPcsCount}\n", DetailFontNormal, DetailColorMuted);
            AppendDetail($"  PCS过滤率 : {FormatPercent(stat.AiOkPcsCount, totalPcs)}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  PCS通过率 : {FormatPercent(stat.AviOkPcsCount + stat.AiOkPcsCount, totalPcs)}\n\n", DetailFontNormal, DetailColorNormal);

            // ===== VRS 明细（PCS）=====
            AppendDetail("▶ VRS明细 (PCS)\n", DetailFontSection, DetailColorSection);
            AppendDetail($"  AI-OK & VRS-OK : {_aiOkVrsOkPcsCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-OK & VRS-NG : {_aiOkVrsNgPcsCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-NG & VRS-OK : {_aiNgVrsOkPcsCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-NG & VRS-NG : {_aiNgVrsNgPcsCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  PCS漏失率(VRS) : {FormatPercent(_aiOkVrsNgPcsCount, totalPcs)}\n", DetailFontNormal, DetailColorBad);
            AppendDetail($"  PCS准确率(VRS) : {FormatPercent(_aiOkVrsOkPcsCount + _aiNgVrsNgPcsCount, totalPcs)}\n\n", DetailFontNormal, DetailColorGood);

            // ===== VVS 明细（PCS）=====
            AppendDetail("▶ VVS明细 (PCS)\n", DetailFontSection, DetailColorSection);
            AppendDetail($"  AI-OK & 人工OK : {_aiOkVvsOkPcsCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-OK & 人工NG : {_aiOkVvsNgPcsCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-NG & 人工OK : {_aiNgVvsOkPcsCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI-NG & 人工NG : {_aiNgVvsNgPcsCount}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  PCS漏失率(VVS) : {FormatPercent(_aiOkVvsNgPcsCount, totalPcs)}\n", DetailFontNormal, DetailColorBad);
            AppendDetail($"  PCS准确率(VVS) : {FormatPercent(_aiOkVvsOkPcsCount + _aiNgVvsNgPcsCount, totalPcs)}\n\n", DetailFontNormal, DetailColorGood);

            // ===== Panel 通过率 =====
            AppendDetail("▶ Panel 通过率\n", DetailFontSection, DetailColorSection);
            AppendDetail($"  一次通过率 : {FormatPercent(stat.AviOkPanelCount, stat.TotalPanelCount)}\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  AI通过率   : {FormatPercent(stat.AiPassPanelCount, stat.TotalPanelCount)}\n\n", DetailFontNormal, DetailColorNormal);

            // ===== 复判进度 =====
            AppendDetail("▶ 复判进度\n", DetailFontSection, DetailColorSection);
            AppendDetail($"  VRS 已判定 : {_vrsOkCount + _vrsNgCount}  (OK {_vrsOkCount} / NG {_vrsNgCount})\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  VRS 未判定 : {_vrsNotSetCount}\n", DetailFontNormal, DetailColorMuted);
            AppendDetail($"  VVS 已判定 : {_vvsOkCount + _vvsNgCount}  (OK {_vvsOkCount} / NG {_vvsNgCount})\n", DetailFontNormal, DetailColorNormal);
            AppendDetail($"  VVS 未判定 : {_vvsNotSetCount}", DetailFontNormal, DetailColorMuted);

            // 回到顶部，避免新内容停在底部
            label_ReviewDetail.SelectionStart = 0;
            label_ReviewDetail.SelectionLength = 0;
            label_ReviewDetail.ScrollToCaret();
        }

        /// <summary>
        /// 构建单SN的复判详情文本：基本信息 + AVI/AI/VVS/VRS 状态 + 报点级别状态分布
        /// </summary>
        private string BuildSnReviewDetailText(DefectReviewItem item)
        {
            var sb = new System.Text.StringBuilder();

            // VRS 读取状态：任一报点 VrsState!=0 视为已读取到 VRS 结果
            var hpForVrs = item.HeatPoints;
            string vrsReadText;
            if (hpForVrs == null || hpForVrs.Count == 0)
                vrsReadText = "无报点";
            else if (hpForVrs.Any(p => p.VrsState != 0))
                vrsReadText = "已读取";
            else
                vrsReadText = "未读取";
            sb.AppendLine($"【SN详情】(VRS结果: {vrsReadText})");
            sb.AppendLine();

            // 基本信息
            sb.AppendLine($"SN: {item.SerialNumber ?? "-"}");
            sb.AppendLine($"Side: {item.Side ?? "-"}");
            sb.AppendLine($"Lot: {item.LotNumber ?? "-"}");
            sb.AppendLine($"机台号: {item.MachineId ?? "-"}");
            sb.AppendLine($"料号: {item.ProductSerial ?? "-"}");
            sb.AppendLine($"检测时间: {(item.DetectionDate == DateTime.MinValue ? "-" : item.DetectionDate.ToString("yyyy-MM-dd HH:mm:ss"))}");
            sb.AppendLine();

            // 状态汇总
            sb.AppendLine("【状态】");
            sb.AppendLine($"AVI状态: {item.AviStatus ?? "-"}");
            sb.AppendLine($"AI状态: {item.AiStatus ?? "-"}");
            sb.AppendLine($"VVS状态: {item.ManualStatus ?? "-"}");
            sb.AppendLine($"VRS状态: {item.VrsStatus ?? "-"}");
            sb.AppendLine();

            // 缺陷概要
            sb.AppendLine("【缺陷概要】");
            sb.AppendLine($"缺陷数: {item.DefectCount}");
            if (!string.IsNullOrEmpty(item.DefectName))
            {
                sb.AppendLine($"缺陷名称: {item.DefectName}");
            }
            if (!string.IsNullOrEmpty(item.DefectChange))
            {
                sb.AppendLine($"缺陷变化: {item.DefectChange}");
            }
            sb.AppendLine();

            // 报点级别状态分布
            var heatPoints = item.HeatPoints ?? new List<DetectInfo>();
            int totalPoints = heatPoints.Count;

            int aiOk = 0, aiNg = 0, aiException = 0, aiUninspected = 0;
            int vvsOk = 0, vvsNg = 0, vvsNotSet = 0;
            int vrsNotSet = 0, vrsOk = 0, vrsNg = 0, vrsIgnore = 0, vrsNoResult = 0, vrsNgReject = 0, vrsOther = 0;
            foreach (var hp in heatPoints)
            {
                switch (hp.AIStatus)
                {
                    case 0: aiUninspected++; break;
                    case 1: aiOk++; break;
                    case 2: aiNg++; break;
                    case 3: aiException++; break;
                }
                switch (hp.VVSStatus)
                {
                    case 0: vvsNotSet++; break;
                    case 1: vvsOk++; break;
                    case 2: vvsNg++; break;
                }
                switch (hp.VrsState)
                {
                    case 0: vrsNotSet++; break;
                    case 1: vrsOk++; break;
                    case 2: vrsNg++; break;
                    case 3: vrsIgnore++; break;
                    case 4: vrsNoResult++; break;
                    case 5: vrsNgReject++; break;
                    default: vrsOther++; break;
                }
            }

            sb.AppendLine("【报点统计】");
            sb.AppendLine($"总报点数: {totalPoints}");
            sb.AppendLine($"  AI-OK: {aiOk}");
            sb.AppendLine($"  AI-NG: {aiNg}");
            sb.AppendLine($"  AI-异常: {aiException}");
            sb.AppendLine($"  AI-未检测: {aiUninspected}");
            sb.AppendLine($"  VVS-OK: {vvsOk}");
            sb.AppendLine($"  VVS-NG: {vvsNg}");
            sb.AppendLine($"  VVS-未判定: {vvsNotSet}");
            sb.AppendLine($"  VRS-OK: {vrsOk}");
            sb.AppendLine($"  VRS-NG: {vrsNg}");
            sb.AppendLine($"  VRS-忽略: {vrsIgnore}");
            sb.AppendLine($"  VRS-无结果: {vrsNoResult}");
            sb.AppendLine($"  VRS-NG不接收: {vrsNgReject}");
            sb.AppendLine($"  VRS-未判定: {vrsNotSet}");
            if (vrsOther > 0)
            {
                sb.AppendLine($"  VRS-其它: {vrsOther}");
            }

            // PCS数（按 PcsIndex 去重）
            int pcsCount = heatPoints.Select(hp => hp.PcsIndex).Distinct().Count();
            sb.Append($"涉及PCS数: {pcsCount}");

            return sb.ToString();
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
                // 批量构建所有待保存记录
                var records = new List<PanelSideRecord>();
                foreach (var item in items)
                {
                    // 将人工判定状态转换为 FinalState
                    // ManualStatus: "OK" -> 1, "NG" -> 2, "未判定" -> 0
                    int finalState = 0;
                    if (item.ManualStatus == "OK") finalState = 1;
                    else if (item.ManualStatus == "NG") finalState = 2;

                    // 使用原始状态码回写，避免丢失 0(未检测)/3(异常) 等状态
                    int aviState = item.OriginalAviState;
                    int aiState = item.OriginalAiState;

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

                    records.Add(new PanelSideRecord
                    {
                        SerialNumber = item.SerialNumber,
                        LotNumber = item.LotNumber,
                        MachineId = item.MachineId,
                        ProductSerial = item.ProductSerial,
                        Side = item.Side,
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
                    });
                }

                // 使用批量保存接口（事务性，保证数据一致性）
                try
                {
                    Machine.master.SavePanelSidesBatch(records);
                    foreach (var item in items)
                    {
                        LogTextHelper.Info($"保存人工判定结果成功: SN={item.SerialNumber}, Side={item.Side}, ManualStatus={item.ManualStatus}");
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"批量保存人工判定结果失败({records.Count}条): {ex.Message}");
                    throw; // 向上层抛出，让调用者感知失败
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
        /// <summary>
        /// VRS 终判状态显示文本（未判定/OK/NG/忽略/无结果/NG不接收）
        /// </summary>
        public string VrsStatus { get; set; }
        public int DefectCount { get; set; }
        /// <summary>
        /// 缺陷名称列表，多个缺陷名用逗号分隔
        /// </summary>
        public string DefectName { get; set; }
        /// <summary>
        /// 缺陷点数变化，格式：原始数量 -> VVS复判后NG数量
        /// </summary>
        public string DefectChange { get; set; }
        public DateTime DetectionDate { get; set; }
        [Browsable(false)]
        public List<DetectInfo> HeatPoints { get; set; }
        [Browsable(false)]
        public bool IsModified { get; set; }

        /// <summary>
        /// 原始 AVI 状态码（0=未检测, 1=OK, 2=NG, 3=异常），用于保存时回写准确值
        /// </summary>
        [Browsable(false)]
        public int OriginalAviState { get; set; }
        /// <summary>
        /// 原始 AI 状态码（0=未检测, 1=OK, 2=NG, 3=异常），用于保存时回写准确值
        /// </summary>
        [Browsable(false)]
        public int OriginalAiState { get; set; }
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
        public int AiExceptionPcsCount { get; set; }   // AI-异常 PCS数
        public int AiUninspectedPcsCount { get; set; } // AI-未检测 PCS数

        // 报点级别
        public int TotalPointCount { get; set; }       // 总报点数
        public int AiOkPointCount { get; set; }        // AI-OK报点数
        public int AiNgPointCount { get; set; }        // AI-NG报点数
        public int AiExceptionPointCount { get; set; } // AI-异常报点数
        public int AiUninspectedPointCount { get; set; } // AI-未检测报点数
    }

    #endregion
}

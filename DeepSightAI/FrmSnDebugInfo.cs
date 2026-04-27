using DeepSightModel;
using DeepSightModel.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// SN调试信息显示窗口 - 双击任务队列时弹出
    /// </summary>
    public partial class FrmSnDebugInfo : Form
    {
        private int _lastSearchIndex = 0;
        private TreeNode _lastSearchNode = null;

        /// <summary>
        /// 存储每个Tab对应的原始JSON字符串，用于复制功能
        /// </summary>
        private readonly Dictionary<TabPage, string> _tabRawJsonMap = new Dictionary<TabPage, string>();

        /// <summary>
        /// Tab数据状态：用于控制Tab标签背景色
        /// </summary>
        private enum TabDataStatus { Empty, HasData, Warning, Error }
        private readonly Dictionary<TabPage, TabDataStatus> _tabStatusMap = new Dictionary<TabPage, TabDataStatus>();

        /// <summary>
        /// 大JSON阈值（字节）：超过则启用懒加载并跳过重格式化
        /// </summary>
        private const int LargeJsonThreshold = 200 * 1024;

        /// <summary>
        /// 懒加载下立即展开的层数（0=只渲染首层）
        /// </summary>
        private const int LazyEagerDepth = 1;

        /// <summary>
        /// TabControl 标签栏深色背景色（与窗体主背景一致）
        /// </summary>
        private static readonly Color TabStripBackColor = Color.FromArgb(29, 48, 60);

        public FrmSnDebugInfo(string sn, SnDebugInfo debugInfo)
        {
            InitializeComponent();
            Text = $"SN调试信息 - {sn}";

            EnableTabControlDarkBackground();
            SetupTreeViewsCopySupport();
            LoadData(sn, debugInfo);
        }

        /// <summary>
        /// 通过反射启用TabControl的UserPaint，使标签栏背景可自绘为深色
        /// </summary>
        private void EnableTabControlDarkBackground()
        {
            var setStyle = typeof(Control).GetMethod("SetStyle",
                BindingFlags.Instance | BindingFlags.NonPublic);
            setStyle?.Invoke(tabControl, new object[]
            {
                ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw,
                true
            });
            tabControl.BackColor = TabStripBackColor;
            tabControl.Paint += TabControl_Paint;
        }

        /// <summary>
        /// 自绘TabControl：填充深色背景 + 重绘所有tab项
        /// </summary>
        private void TabControl_Paint(object sender, PaintEventArgs e)
        {
            // 整个控件区域填充为深色，覆盖默认浅灰色tab strip背景
            using (var brush = new SolidBrush(TabStripBackColor))
                e.Graphics.FillRectangle(brush, tabControl.ClientRectangle);

            // 重绘每一个tab项
            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                var bounds = tabControl.GetTabRect(i);
                var state = i == tabControl.SelectedIndex
                    ? DrawItemState.Selected
                    : DrawItemState.None;
                var args = new DrawItemEventArgs(e.Graphics, tabControl.Font, bounds, i, state);
                TabControl_DrawItem(sender, args);
            }
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            _lastSearchIndex = 0;
            _lastSearchNode = null;
            tabControl.Invalidate(); // UserPaint模式下需主动刷新以更新选中态
        }

        #region TabControl 绘制

        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= tabControl.TabPages.Count) return;

            var tabPage = tabControl.TabPages[e.Index];
            var bounds = e.Bounds;
            bool isSelected = (tabControl.SelectedIndex == e.Index);
            Color bgColor = GetTabBgColor(tabPage, isSelected);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            bool isVertical = tabControl.Alignment == TabAlignment.Left
                              || tabControl.Alignment == TabAlignment.Right;

            if (isVertical)
                DrawVerticalTab(e.Graphics, bounds, tabPage, isSelected, bgColor);
            else
                DrawHorizontalTab(e.Graphics, bounds, e.Index, tabControl.TabPages.Count, tabPage, isSelected, bgColor);
        }

        /// <summary>
        /// 横向箭头流程绘制（保留原样式）
        /// </summary>
        private void DrawHorizontalTab(Graphics g, Rectangle bounds, int index, int count, TabPage tabPage, bool isSelected, Color bgColor)
        {
            int arrowW = 10;
            var pts = new List<Point>();
            if (index == 0)
            {
                pts.Add(new Point(bounds.Left, bounds.Top));
                pts.Add(new Point(bounds.Right - arrowW, bounds.Top));
                pts.Add(new Point(bounds.Right, bounds.Top + bounds.Height / 2));
                pts.Add(new Point(bounds.Right - arrowW, bounds.Bottom));
                pts.Add(new Point(bounds.Left, bounds.Bottom));
            }
            else if (index == count - 1)
            {
                pts.Add(new Point(bounds.Left, bounds.Top));
                pts.Add(new Point(bounds.Right, bounds.Top));
                pts.Add(new Point(bounds.Right, bounds.Bottom));
                pts.Add(new Point(bounds.Left, bounds.Bottom));
                pts.Add(new Point(bounds.Left + arrowW, bounds.Top + bounds.Height / 2));
            }
            else
            {
                pts.Add(new Point(bounds.Left, bounds.Top));
                pts.Add(new Point(bounds.Right - arrowW, bounds.Top));
                pts.Add(new Point(bounds.Right, bounds.Top + bounds.Height / 2));
                pts.Add(new Point(bounds.Right - arrowW, bounds.Bottom));
                pts.Add(new Point(bounds.Left, bounds.Bottom));
                pts.Add(new Point(bounds.Left + arrowW, bounds.Top + bounds.Height / 2));
            }

            using (var brush = new LinearGradientBrush(bounds,
                ControlPaint.Light(bgColor, 0.15f), bgColor, LinearGradientMode.Vertical))
            {
                g.FillPolygon(brush, pts.ToArray());
            }

            if (isSelected)
            {
                using (var pen = new Pen(Color.FromArgb(100, 180, 255), 1.5f))
                    g.DrawPolygon(pen, pts.ToArray());
            }

            var textRect = bounds;
            if (index > 0) { textRect.X += arrowW; textRect.Width -= arrowW; }
            if (index < count - 1) { textRect.Width -= arrowW; }

            int dotSize = 7;
            int dotX = textRect.Left + 4;
            int dotY = textRect.Top + (textRect.Height - dotSize) / 2;
            using (var dotBrush = new SolidBrush(GetStatusDotColor(tabPage)))
                g.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);

            var txtRect = new Rectangle(textRect.X + dotSize + 6, textRect.Y, textRect.Width - dotSize - 8, textRect.Height);
            TextRenderer.DrawText(g, tabPage.Text, tabControl.Font, txtRect,
                isSelected ? Color.White : Color.FromArgb(200, 210, 220),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        /// <summary>
        /// 纵向侧栏式绘制：左侧高亮条 + 状态点 + 文字（圆角矩形渐变背景）
        /// </summary>
        private void DrawVerticalTab(Graphics g, Rectangle bounds, TabPage tabPage, bool isSelected, Color bgColor)
        {
            // 内缩留白，避免标签紧贴
            var rect = new Rectangle(bounds.Left + 2, bounds.Top + 2, bounds.Width - 4, bounds.Height - 4);

            // 圆角矩形背景渐变
            using (var path = CreateRoundedRect(rect, 5))
            using (var brush = new LinearGradientBrush(rect,
                ControlPaint.Light(bgColor, 0.20f), bgColor, LinearGradientMode.Horizontal))
            {
                g.FillPath(brush, path);

                if (isSelected)
                {
                    using (var pen = new Pen(Color.FromArgb(100, 180, 255), 1.4f))
                        g.DrawPath(pen, path);
                }
            }

            // 左侧 3px 高亮条（选中态/状态色）
            Color barColor = isSelected
                ? Color.FromArgb(120, 200, 255)
                : GetStatusDotColor(tabPage);
            var barRect = new Rectangle(rect.Left + 1, rect.Top + 4, 3, rect.Height - 8);
            using (var barBrush = new SolidBrush(barColor))
                g.FillRectangle(barBrush, barRect);

            // 状态点
            int dotSize = 7;
            int dotX = rect.Left + 12;
            int dotY = rect.Top + (rect.Height - dotSize) / 2;
            using (var dotBrush = new SolidBrush(GetStatusDotColor(tabPage)))
                g.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);

            // 文字
            var txtRect = new Rectangle(dotX + dotSize + 6, rect.Top, rect.Right - (dotX + dotSize + 6) - 4, rect.Height);
            TextRenderer.DrawText(g, tabPage.Text, tabControl.Font, txtRect,
                isSelected ? Color.White : Color.FromArgb(200, 210, 220),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Color GetTabBgColor(TabPage tab, bool isSelected)
        {
            if (!_tabStatusMap.TryGetValue(tab, out var status))
                status = TabDataStatus.Empty;

            switch (status)
            {
                case TabDataStatus.HasData:
                    return isSelected ? Color.FromArgb(35, 100, 60) : Color.FromArgb(28, 68, 45);
                case TabDataStatus.Warning:
                    return isSelected ? Color.FromArgb(150, 105, 35) : Color.FromArgb(110, 78, 25);
                case TabDataStatus.Error:
                    return isSelected ? Color.FromArgb(140, 45, 45) : Color.FromArgb(100, 35, 35);
                default:
                    return isSelected ? Color.FromArgb(55, 75, 90) : Color.FromArgb(38, 55, 68);
            }
        }

        private Color GetStatusDotColor(TabPage tab)
        {
            if (!_tabStatusMap.TryGetValue(tab, out var status))
                status = TabDataStatus.Empty;

            switch (status)
            {
                case TabDataStatus.HasData: return Color.FromArgb(80, 220, 100);
                case TabDataStatus.Warning: return Color.FromArgb(255, 190, 70);
                case TabDataStatus.Error: return Color.FromArgb(255, 80, 60);
                default: return Color.FromArgb(100, 110, 120);
            }
        }

        #endregion

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnFind_Click(object sender, EventArgs e)
        {
            FindNext();
        }

        private void BtnCloseSearch_Click(object sender, EventArgs e)
        {
            searchPanel.Visible = false;
        }

        #region LoadData

        private void LoadData(string sn, SnDebugInfo debugInfo)
        {
            // 初始化所有Tab状态为Empty
            foreach (TabPage tab in tabControl.TabPages)
                _tabStatusMap[tab] = TabDataStatus.Empty;

            if (debugInfo == null )
            {
                SetTreeViewPlaceholder(tvLevelDbJson, "暂无数据 - 该SN尚未完成数据读取");
                SetTreeViewPlaceholder(tvPanelInfoJson, "暂无数据");
                SetTreeViewPlaceholder(tvVbInferenceJson, "暂无数据");
                SetTreeViewPlaceholder(tvInferenceReturnJson, "暂无数据");
                SetTreeViewPlaceholder(tvAviWriteBackJson, "暂无数据");
                SetTreeViewPlaceholder(tvVrsWriteBackJson, "暂无数据");
                SetTreeViewPlaceholder(tvVrsV1WriteBackJson, "暂无数据");
                SetTreeViewPlaceholder(tvInferAnalysis, "暂无数据");
                txtOtherInfo.Text = "暂无数据";
                tabControl.Invalidate(); // 刷新Tab绘制
                return;
            }

            LoadJsonToTreeView(tvLevelDbJson, tabLevelDb, debugInfo.RawLevelDbJson);

            // 推理结果解析的统计计数（用于全OK/全NG警告判断）
            int analysisOkCount = 0;
            int analysisNgCount = 0;

            // 其余按AB侧分开显示（已排序A在前）
            var sbOther = new System.Text.StringBuilder();

            {
                // PanelInfo JSON - 按面添加到TreeView（启用大文件懒加载）
                LoadLargeSideJsonToTreeView(tvPanelInfoJson, debugInfo.PanelInfoJson);

                // VB Inference JSON（启用大文件懒加载）
                LoadLargeSideJsonToTreeView(tvVbInferenceJson, debugInfo.VbInferenceJson);

                // 推理返回JSON（启用大文件懒加载）
                LoadLargeSideJsonToTreeView(tvInferenceReturnJson, debugInfo.InferenceReturnJson);

                // 推理结果解析
                LoadInferenceAnalysis(debugInfo, out analysisOkCount, out analysisNgCount);

                // 回写AVI JSON
                LoadJsonToTreeView(tvAviWriteBackJson, tabAviWriteBack, debugInfo.AviWriteBackJson);

                // 回写VRS JSON
                LoadJsonToTreeView(tvVrsWriteBackJson, tabVrsWriteBack, debugInfo.VrsWriteBackJson);

                // 回写VRS V1.0 JSON
                LoadJsonToTreeView(tvVrsV1WriteBackJson, tabVrsV1WriteBack, debugInfo.VrsV1WriteBackJson);

                // Other info
                sbOther.AppendLine($"SN:              {debugInfo.SerialNumber}");
                sbOther.AppendLine($"面别:            {debugInfo.Side}");
                sbOther.AppendLine($"缺陷数量:        {debugInfo.DefectCount}");
                sbOther.AppendLine($"PCS数量:         {debugInfo.PcsCount}");
                sbOther.AppendLine($"图片数量:        {debugInfo.ImageCount}");
                sbOther.AppendLine($"数据源DB:        {debugInfo.SourceDbName}");
                sbOther.AppendLine($"AVI侧URL:        {debugInfo.SourceDbUrl}");
                sbOther.AppendLine($"VRS侧URL:        {debugInfo.SourceVRSDbUrl}");
                sbOther.AppendLine($"Minio路径:       {debugInfo.MinioPath}");
                sbOther.AppendLine($"数据获取时间:    {debugInfo.CreateTime:yyyy-MM-dd HH:mm:ss.fff}");

                if (debugInfo.HasError)
                {
                    sbOther.AppendLine();
                    sbOther.AppendLine("---------- 错误信息 ----------");
                    sbOther.AppendLine($"出错步骤:        {debugInfo.ErrorStep}");
                    sbOther.AppendLine($"错误原因:        {debugInfo.ErrorMessage}");
                    sbOther.AppendLine($"错误时间:        {debugInfo.ErrorTime:yyyy-MM-dd HH:mm:ss.fff}");
                }
                sbOther.AppendLine();
            }

            // 存储AB侧的原始JSON用于复制
            StoreSideRawJson(tabPanelInfo, debugInfo, d => d.PanelInfoJson);
            StoreSideRawJson(tabVbJson, debugInfo, d => d.VbInferenceJson);
            StoreSideRawJson(tabInferReturn, debugInfo, d => d.InferenceReturnJson);

            txtOtherInfo.Text = sbOther.ToString();

            // 设置Tab状态（有数据=绿色, 无数据=灰色, 出错=红色）
            _tabStatusMap[tabLevelDb] = !string.IsNullOrEmpty(debugInfo.RawLevelDbJson) ? TabDataStatus.HasData : TabDataStatus.Empty;
            _tabStatusMap[tabPanelInfo] = !string.IsNullOrEmpty(debugInfo.PanelInfoJson) ? TabDataStatus.HasData : TabDataStatus.Empty;
            _tabStatusMap[tabVbJson] = !string.IsNullOrEmpty(debugInfo.VbInferenceJson) ? TabDataStatus.HasData : TabDataStatus.Empty;
            _tabStatusMap[tabInferReturn] = !string.IsNullOrEmpty(debugInfo.InferenceReturnJson) ? TabDataStatus.HasData : TabDataStatus.Empty;
            // 推理结果解析：所有结果均为OK或均为NG时显示警告色
            int analysisTotal = analysisOkCount + analysisNgCount;
            bool allSameResult = analysisTotal > 0 && (analysisOkCount == 0 || analysisNgCount == 0);
            if (allSameResult)
                _tabStatusMap[tabInferAnalysis] = TabDataStatus.Warning;
            else
                _tabStatusMap[tabInferAnalysis] = !string.IsNullOrEmpty(debugInfo.InferenceReturnJson) ? TabDataStatus.HasData : TabDataStatus.Empty;
            _tabStatusMap[tabAviWriteBack] = !string.IsNullOrEmpty(debugInfo.AviWriteBackJson) ? TabDataStatus.HasData : TabDataStatus.Empty;
            _tabStatusMap[tabVrsWriteBack] = !string.IsNullOrEmpty(debugInfo.VrsWriteBackJson) ? TabDataStatus.HasData : TabDataStatus.Empty;
            _tabStatusMap[tabVrsV1WriteBack] = !string.IsNullOrEmpty(debugInfo.VrsV1WriteBackJson) ? TabDataStatus.HasData : TabDataStatus.Empty;
            _tabStatusMap[tabOther] = debugInfo.HasError ? TabDataStatus.Error : TabDataStatus.HasData;

            tabControl.Invalidate(); // 刷新Tab绘制
        }

        /// <summary>
        /// 加载推理结果解析 - 使用TreeView展示，支持折叠/展开
        /// </summary>
        /// <param name="analysisOk">输出：推理结果中OK的数量</param>
        /// <param name="analysisNg">输出：推理结果中NG的数量</param>
        private void LoadInferenceAnalysis(SnDebugInfo debugInfo, out int analysisOk, out int analysisNg)
        {
            analysisOk = 0;
            analysisNg = 0;
            tvInferAnalysis.Nodes.Clear();

            if (string.IsNullOrEmpty(debugInfo.InferenceReturnJson))
            {
                tvInferAnalysis.Nodes.Add(new TreeNode("暂无数据 - 推理尚未返回结果"));
                return;
            }

            try
            {
                // 1. 解析推理返回JSON
                var outInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<RootVBOutInfo>(debugInfo.InferenceReturnJson);
                if (outInfo == null)
                {
                    tvInferAnalysis.Nodes.Add(new TreeNode("推理返回JSON解析失败"));
                    return;
                }

                string code = outInfo.Code?.ToString() ?? "N/A";
                string message = outInfo.Message?.ToString() ?? "N/A";

                // 2. 解析VB推理请求JSON获取DefectCode
                var requestDefectCodes = new List<string>();
                if (!string.IsNullOrEmpty(debugInfo.VbInferenceJson))
                {
                    try
                    {
                        var vbInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<RootVBInfo>(debugInfo.VbInferenceJson);
                        var groups = vbInfo?.paramsData?.InferWholeData?.ImageData?.DataValue?.InferImageGroup;
                        if (groups != null)
                            foreach (var g in groups)
                                requestDefectCodes.Add(g.DefectCode ?? "");
                    }
                    catch { /* 忽略解析失败 */ }
                }

                // 3. 解析PanelInfo获取PCS与缺陷的映射
                var allPanelDefects = new List<(string DefectCode, int PcsIndex, int DefectIndex)>();
                string productSerial = debugInfo.ProductSerial ?? "";
                if (!string.IsNullOrEmpty(debugInfo.PanelInfoJson))
                {
                    try
                    {
                        var panelInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<RootPanelInfo>(debugInfo.PanelInfoJson);
                        if (panelInfo?.PcsInfo != null)
                        {
                            productSerial = panelInfo.ProductSerial ?? productSerial;
                            foreach (var pcsEntry in panelInfo.PcsInfo.Values)
                            {
                                if (pcsEntry?.DefectInfo == null) continue;
                                foreach (var defect in pcsEntry.DefectInfo)
                                    allPanelDefects.Add((defect.DefectCode ?? "", defect.PcsIndex, defect.DefectIndex));
                            }
                        }
                    }
                    catch { /* 忽略解析失败 */ }
                }

                // 4. 区分直报缺陷和推理缺陷
                var inferDefects = new List<(string DefectCode, int PcsIndex, int DefectIndex)>();
                var directReportDefects = new List<(string DefectCode, int PcsIndex, int DefectIndex)>();
                foreach (var d in allPanelDefects)
                {
                    if (KeyDefectConfigManager.Instance.IsDirectReportByProduct(d.DefectCode, productSerial))
                        directReportDefects.Add(d);
                    else
                        inferDefects.Add(d);
                }

                var inferResults = outInfo.Data?.InferWholeData?.InferResults;
                int inferResultCount = inferResults?.Count ?? 0;

                // ==================== 推理概览（展开） ====================
                var nodeOverview = new TreeNode("推理概览") { ForeColor = Color.FromArgb(100, 200, 255) };
                nodeOverview.Nodes.Add(CreateValueNode($"返回码: {code}"));
                nodeOverview.Nodes.Add(CreateValueNode($"消息: {message}"));
                nodeOverview.Nodes.Add(CreateValueNode($"推理缺陷数: {inferResultCount}"));
                nodeOverview.Nodes.Add(CreateValueNode($"直报缺陷数: {directReportDefects.Count}"));
                nodeOverview.Nodes.Add(CreateValueNode($"缺陷总数: {inferResultCount + directReportDefects.Count}"));
                tvInferAnalysis.Nodes.Add(nodeOverview);
                nodeOverview.Expand();

                // ==================== 统计计算 ====================
                int okCount = 0, ngCount = 0, keyDefectNgCount = 0;
                var allPcsIds = new HashSet<int>();   // 所有涉及的PCS（按PcsIndex去重）
                var pcsNgSet = new HashSet<int>();    // 含NG缺陷的PCS
                var pcsDirectSet = new HashSet<int>(); // 含直报缺陷的PCS

                // 收集直报缺陷的PCS
                foreach (var d in directReportDefects)
                {
                    allPcsIds.Add(d.PcsIndex);
                    pcsDirectSet.Add(d.PcsIndex);
                }

                // 收集推理缺陷的PCS
                foreach (var d in inferDefects)
                    allPcsIds.Add(d.PcsIndex);

                if (code == "200" && inferResults != null && inferResults.Count > 0)
                {
                    // ==================== 推理缺陷逐项分析（默认收起） ====================
                    var nodeDefects = new TreeNode($"推理缺陷逐项分析  [ {inferResults.Count} ]")
                    { ForeColor = Color.FromArgb(200, 180, 100) };

                    for (int i = 0; i < inferResults.Count; i++)
                    {
                        var result = inferResults[i];
                        string inferResult = result.Infer_Result ?? "N/A";
                        bool isNG = inferResult.Equals("NG", StringComparison.OrdinalIgnoreCase);
                        bool isOK = inferResult.Equals("OK", StringComparison.OrdinalIgnoreCase);

                        string defectCode = !string.IsNullOrEmpty(result.Defect_code)
                            ? result.Defect_code
                            : (i < requestDefectCodes.Count ? requestDefectCodes[i] : "");

                        int pcsIndex = i < inferDefects.Count ? inferDefects[i].PcsIndex : -1;
                        bool isKeyDefect = isNG && !string.IsNullOrEmpty(defectCode)
                            && KeyDefectConfigManager.Instance.IsKeyDefectByProduct(defectCode, productSerial);

                        if (isOK) okCount++;
                        if (isNG)
                        {
                            ngCount++;
                            if (isKeyDefect) keyDefectNgCount++;
                            if (pcsIndex >= 0) pcsNgSet.Add(pcsIndex);
                        }

                        // 缺陷节点标题
                        string pcsStr = pcsIndex >= 0 ? pcsIndex.ToString() : "-";
                        string keyStr = isKeyDefect ? " ★重点" : "";
                        var resultColor = isNG ? Color.FromArgb(255, 120, 100) : Color.FromArgb(100, 230, 120);
                        var defectNode = new TreeNode($"#{i + 1}  {defectCode,-10}  PCS:{pcsStr,-4}  {inferResult}{keyStr}")
                        { ForeColor = resultColor };

                        // 推理依据子节点
                        if (result.InferDetails != null)
                        {
                            if (!string.IsNullOrEmpty(result.InferDetails.DefectArea))
                                defectNode.Nodes.Add(CreateDetailNode($"面积: {result.InferDetails.DefectArea}"));

                            if (result.InferDetails.DrawInfoList != null)
                            {
                                foreach (var drawInfo in result.InferDetails.DrawInfoList)
                                {
                                    if (drawInfo.Conditions != null)
                                    {
                                        foreach (var cond in drawInfo.Conditions)
                                        {
                                            string thresholdStr = "";
                                            if (cond.Threshold != null)
                                            {
                                                var min = cond.Threshold.Min?.ToString() ?? "-∞";
                                                var max = cond.Threshold.Max?.ToString() ?? "+∞";
                                                thresholdStr = $" (阈值: {min}~{max})";
                                            }
                                            string valStr = cond.Value.HasValue ? cond.Value.Value.ToString("F2") : "N/A";
                                            string unitStr = !string.IsNullOrEmpty(cond.Unit) ? cond.Unit : "";
                                            defectNode.Nodes.Add(CreateDetailNode($"依据: {cond.Name ?? ""} = {valStr}{unitStr}{thresholdStr}"));
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(drawInfo.InspectLabel))
                                        defectNode.Nodes.Add(CreateDetailNode($"检测标签: {drawInfo.InspectLabel}"));
                                }
                            }

                            if (result.InferDetails.Details != null && result.InferDetails.Details.Count > 0)
                                defectNode.Nodes.Add(CreateDetailNode($"详情: {string.Join(", ", result.InferDetails.Details)}"));
                        }

                        nodeDefects.Nodes.Add(defectNode);
                    }

                    tvInferAnalysis.Nodes.Add(nodeDefects);
                    // 默认收起，不调用 Expand
                }
                else
                {
                    var nodeWarn = new TreeNode(code != "200"
                        ? $"⚠ 推理返回非200 (Code={code})，无缺陷级结果"
                        : "推理结果为空")
                    { ForeColor = Color.FromArgb(255, 200, 80) };
                    tvInferAnalysis.Nodes.Add(nodeWarn);
                }

                // ==================== 直报缺陷列表（默认收起） ====================
                if (directReportDefects.Count > 0)
                {
                    var nodeDirect = new TreeNode($"直报缺陷列表 (跳过AI推理)  [ {directReportDefects.Count} ]")
                    { ForeColor = Color.FromArgb(200, 180, 100) };

                    for (int i = 0; i < directReportDefects.Count; i++)
                    {
                        var d = directReportDefects[i];
                        bool isKey = KeyDefectConfigManager.Instance.IsKeyDefectByProduct(d.DefectCode, productSerial);
                        var dn = new TreeNode($"#{i + 1}  {d.DefectCode,-10}  PCS:{d.PcsIndex,-4}  直报{(isKey ? " ★重点" : "")}")
                        { ForeColor = Color.FromArgb(180, 180, 180) };
                        nodeDirect.Nodes.Add(dn);
                    }

                    tvInferAnalysis.Nodes.Add(nodeDirect);
                }

                // ==================== 统计汇总（展开） ====================
                var nodeStat = new TreeNode("统计汇总") { ForeColor = Color.FromArgb(100, 200, 255) };

                // 缺陷级别
                int totalInfer = okCount + ngCount;
                double ngRatio = totalInfer > 0 ? (double)ngCount / totalInfer * 100 : 0;
                double okRatio = totalInfer > 0 ? (double)okCount / totalInfer * 100 : 0;

                var nodeDefectStat = new TreeNode("缺陷级别统计") { ForeColor = Color.FromArgb(150, 200, 255) };
                nodeDefectStat.Nodes.Add(CreateStatNode($"OK数量: {okCount}  ({okRatio:F1}%)", false));
                nodeDefectStat.Nodes.Add(CreateStatNode($"NG数量: {ngCount}  ({ngRatio:F1}%)", ngCount > 0));
                nodeDefectStat.Nodes.Add(CreateStatNode($"重点缺陷NG: {keyDefectNgCount}", keyDefectNgCount > 0));
                nodeDefectStat.Nodes.Add(CreateStatNode($"直报缺陷: {directReportDefects.Count}", false));
                nodeStat.Nodes.Add(nodeDefectStat);

                // PCS级别 - 按PcsIndex去重统计
                int totalPcs = allPcsIds.Count;
                int ngPcsCount = pcsNgSet.Count;
                int directPcsCount = pcsDirectSet.Count;
                double pcsNgRatio = totalPcs > 0 ? (double)ngPcsCount / totalPcs * 100 : 0;

                var nodePcsStat = new TreeNode("PCS级别统计 (按PcsIndex去重)") { ForeColor = Color.FromArgb(150, 200, 255) };
                nodePcsStat.Nodes.Add(CreateStatNode($"PCS总数(去重): {totalPcs}", false));
                nodePcsStat.Nodes.Add(CreateStatNode($"含NG缺陷的PCS: {ngPcsCount}  ({pcsNgRatio:F1}%)", ngPcsCount > 0));
                nodePcsStat.Nodes.Add(CreateStatNode($"含直报缺陷的PCS: {directPcsCount}", false));
                nodePcsStat.Nodes.Add(CreateStatNode($"无异常的PCS: {totalPcs - ngPcsCount - pcsDirectSet.Except(pcsNgSet).Count()}", false));
                nodeStat.Nodes.Add(nodePcsStat);

                tvInferAnalysis.Nodes.Add(nodeStat);
                nodeStat.ExpandAll();

                // 输出统计结果
                analysisOk = okCount;
                analysisNg = ngCount;

                // 存储用于复制
                _tabRawJsonMap[tabInferAnalysis] = BuildAnalysisText(tvInferAnalysis);
            }
            catch (Exception ex)
            {
                tvInferAnalysis.Nodes.Add(new TreeNode($"解析推理结果异常: {ex.Message}") { ForeColor = Color.FromArgb(255, 100, 100) });
            }
        }

        private TreeNode CreateValueNode(string text)
        {
            return new TreeNode(text) { ForeColor = Color.FromArgb(180, 230, 150) };
        }

        private TreeNode CreateDetailNode(string text)
        {
            return new TreeNode(text) { ForeColor = Color.FromArgb(200, 160, 255) };
        }

        private TreeNode CreateStatNode(string text, bool highlight)
        {
            return new TreeNode(text)
            { ForeColor = highlight ? Color.FromArgb(255, 150, 100) : Color.FromArgb(200, 220, 240) };
        }

        /// <summary>
        /// 将TreeView内容转为纯文本（用于复制功能）
        /// </summary>
        private string BuildAnalysisText(TreeView tv)
        {
            var sb = new System.Text.StringBuilder();
            foreach (TreeNode node in tv.Nodes)
                AppendNodeText(sb, node, 0);
            return sb.ToString();
        }

        private void AppendNodeText(System.Text.StringBuilder sb, TreeNode node, int indent)
        {
            sb.AppendLine(new string(' ', indent * 2) + node.Text);
            foreach (TreeNode child in node.Nodes)
                AppendNodeText(sb, child, indent + 1);
        }

        #endregion

        #region JSON → TreeView 辅助方法

        private void SetTreeViewPlaceholder(TreeView tv, string text)
        {
            tv.Nodes.Clear();
            tv.Nodes.Add(new TreeNode(text));
        }

        /// <summary>
        /// 将单个JSON字符串加载到TreeView（用于LevelDB等不分面的场景）
        /// </summary>
        private void LoadJsonToTreeView(TreeView tv, TabPage tab, string json)
        {
            tv.Nodes.Clear();
            if (string.IsNullOrEmpty(json))
            {
                tv.Nodes.Add(new TreeNode("暂无数据"));
                return;
            }

            try
            {
                var token = JToken.Parse(json);
                var rootNode = new TreeNode("JSON");
                AddJTokenNodes(rootNode, token);
                tv.Nodes.Add(rootNode);
                rootNode.Expand();
                _tabRawJsonMap[tab] = token.ToString(Newtonsoft.Json.Formatting.Indented);
            }
            catch
            {
                tv.Nodes.Add(new TreeNode(json));
                _tabRawJsonMap[tab] = json;
            }
        }

        /// <summary>
        /// 递归将JToken转换为TreeNode
        /// </summary>
        private void AddJTokenNodes(TreeNode parentNode, JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var prop in ((JObject)token).Properties())
                    {
                        if (prop.Value.Type == JTokenType.Object)
                        {
                            var childCount = ((JObject)prop.Value).Count;
                            var node = new TreeNode($"{prop.Name}  {{ {childCount} }}");
                            node.ForeColor = Color.FromArgb(150, 200, 255);
                            AddJTokenNodes(node, prop.Value);
                            parentNode.Nodes.Add(node);
                        }
                        else if (prop.Value.Type == JTokenType.Array)
                        {
                            var arr = (JArray)prop.Value;
                            var node = new TreeNode($"{prop.Name}  [ {arr.Count} ]");
                            node.ForeColor = Color.FromArgb(200, 180, 100);
                            AddJTokenNodes(node, prop.Value);
                            parentNode.Nodes.Add(node);
                        }
                        else
                        {
                            var valStr = prop.Value.Type == JTokenType.Null ? "null" : prop.Value.ToString();
                            var node = new TreeNode($"{prop.Name}: {valStr}");
                            node.ForeColor = GetValueColor(prop.Value);
                            parentNode.Nodes.Add(node);
                        }
                    }
                    break;

                case JTokenType.Array:
                    var array = (JArray)token;
                    for (int i = 0; i < array.Count; i++)
                    {
                        var item = array[i];
                        if (item.Type == JTokenType.Object)
                        {
                            var childCount = ((JObject)item).Count;
                            var node = new TreeNode($"[{i}]  {{ {childCount} }}");
                            node.ForeColor = Color.FromArgb(150, 200, 255);
                            AddJTokenNodes(node, item);
                            parentNode.Nodes.Add(node);
                        }
                        else if (item.Type == JTokenType.Array)
                        {
                            var innerArr = (JArray)item;
                            var node = new TreeNode($"[{i}]  [ {innerArr.Count} ]");
                            node.ForeColor = Color.FromArgb(200, 180, 100);
                            AddJTokenNodes(node, item);
                            parentNode.Nodes.Add(node);
                        }
                        else
                        {
                            var valStr = item.Type == JTokenType.Null ? "null" : item.ToString();
                            var node = new TreeNode($"[{i}]: {valStr}");
                            node.ForeColor = GetValueColor(item);
                            parentNode.Nodes.Add(node);
                        }
                    }
                    break;

                default:
                    parentNode.Nodes.Add(new TreeNode(token.ToString()));
                    break;
            }
        }

        private Color GetValueColor(JToken value)
        {
            switch (value.Type)
            {
                case JTokenType.String:
                    return Color.FromArgb(180, 230, 150); // 绿色 - 字符串
                case JTokenType.Integer:
                case JTokenType.Float:
                    return Color.FromArgb(200, 160, 255); // 紫色 - 数字
                case JTokenType.Boolean:
                    return Color.FromArgb(255, 150, 100); // 橙色 - 布尔
                case JTokenType.Null:
                    return Color.FromArgb(128, 128, 128); // 灰色 - null
                default:
                    return Color.FromArgb(200, 220, 240);
            }
        }

        private void ExpandFirstLevel(TreeView tv)
        {
            foreach (TreeNode node in tv.Nodes)
            {
                node.Expand();
            }
        }

        private void StoreSideRawJson(TabPage tab, SnDebugInfo debugInfo, Func<SnDebugInfo, string> jsonSelector)
        {
            var sb = new System.Text.StringBuilder();
            {
                sb.AppendLine($"===== {debugInfo.Side}面 =====");
                var json = jsonSelector(debugInfo);
                if (!string.IsNullOrEmpty(json))
                {
                    // 大JSON跳过格式化，直接使用原始字符串避免再分配大内存
                    if (json.Length > LargeJsonThreshold)
                    {
                        sb.AppendLine(json);
                    }
                    else
                    {
                        try
                        {
                            sb.AppendLine(JToken.Parse(json).ToString(Newtonsoft.Json.Formatting.Indented));
                        }
                        catch
                        {
                            sb.AppendLine(json);
                        }
                    }
                }
                else
                {
                    sb.AppendLine("暂无数据");
                }
                sb.AppendLine();
            }
            _tabRawJsonMap[tab] = sb.ToString();
        }

        /// <summary>
        /// 大JSON懒加载：仅渲染前几层，深层节点放占位符 + JToken Tag，BeforeExpand时再展开
        /// </summary>
        private void LoadLargeSideJsonToTreeView(TreeView tv, string json)
        {
            tv.Nodes.Clear();
            var sideNode = new TreeNode { ForeColor = Color.FromArgb(100, 200, 255) };

            if (string.IsNullOrEmpty(json))
            {
                sideNode.Text = "暂无数据";
                tv.Nodes.Add(sideNode);
                return;
            }

            bool isLarge = json.Length > LargeJsonThreshold;
            try
            {
                var token = JToken.Parse(json);
                if (isLarge)
                {
                    sideNode.Text = $"JSON  ({json.Length / 1024.0:F1} KB) — 大文件，按需展开";
                    tv.BeginUpdate();
                    try { AddJTokenNodesLazy(sideNode, token, 0); }
                    finally { tv.EndUpdate(); }
                    tv.Nodes.Add(sideNode);
                    sideNode.Expand();
                }
                else
                {
                    AddJTokenNodes(sideNode, token);
                    tv.Nodes.Add(sideNode);
                    tv.ExpandAll();
                }
            }
            catch
            {
                // 解析失败：截断显示，避免一次性塞超长字符串到节点
                var preview = json.Length > 4096 ? json.Substring(0, 4096) + " ... (已截断)" : json;
                sideNode.Nodes.Add(new TreeNode(preview));
                tv.Nodes.Add(sideNode);
            }
        }

        /// <summary>
        /// 懒加载版的JToken→TreeNode转换：超过 LazyEagerDepth 的容器节点不立即展开
        /// </summary>
        private void AddJTokenNodesLazy(TreeNode parentNode, JToken token, int depth)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var prop in ((JObject)token).Properties())
                    {
                        if (prop.Value.Type == JTokenType.Object)
                        {
                            var childCount = ((JObject)prop.Value).Count;
                            var node = new TreeNode($"{prop.Name}  {{ {childCount} }}")
                            { ForeColor = Color.FromArgb(150, 200, 255) };
                            AttachLazyOrExpand(node, prop.Value, depth);
                            parentNode.Nodes.Add(node);
                        }
                        else if (prop.Value.Type == JTokenType.Array)
                        {
                            var arr = (JArray)prop.Value;
                            var node = new TreeNode($"{prop.Name}  [ {arr.Count} ]")
                            { ForeColor = Color.FromArgb(200, 180, 100) };
                            AttachLazyOrExpand(node, prop.Value, depth);
                            parentNode.Nodes.Add(node);
                        }
                        else
                        {
                            var valStr = prop.Value.Type == JTokenType.Null ? "null" : prop.Value.ToString();
                            var node = new TreeNode($"{prop.Name}: {valStr}")
                            { ForeColor = GetValueColor(prop.Value) };
                            parentNode.Nodes.Add(node);
                        }
                    }
                    break;

                case JTokenType.Array:
                    var array = (JArray)token;
                    for (int i = 0; i < array.Count; i++)
                    {
                        var item = array[i];
                        if (item.Type == JTokenType.Object)
                        {
                            var childCount = ((JObject)item).Count;
                            var node = new TreeNode($"[{i}]  {{ {childCount} }}")
                            { ForeColor = Color.FromArgb(150, 200, 255) };
                            AttachLazyOrExpand(node, item, depth);
                            parentNode.Nodes.Add(node);
                        }
                        else if (item.Type == JTokenType.Array)
                        {
                            var innerArr = (JArray)item;
                            var node = new TreeNode($"[{i}]  [ {innerArr.Count} ]")
                            { ForeColor = Color.FromArgb(200, 180, 100) };
                            AttachLazyOrExpand(node, item, depth);
                            parentNode.Nodes.Add(node);
                        }
                        else
                        {
                            var valStr = item.Type == JTokenType.Null ? "null" : item.ToString();
                            var node = new TreeNode($"[{i}]: {valStr}")
                            { ForeColor = GetValueColor(item) };
                            parentNode.Nodes.Add(node);
                        }
                    }
                    break;

                default:
                    parentNode.Nodes.Add(new TreeNode(token.ToString()));
                    break;
            }
        }

        private void AttachLazyOrExpand(TreeNode node, JToken token, int currentDepth)
        {
            if (currentDepth < LazyEagerDepth)
            {
                AddJTokenNodesLazy(node, token, currentDepth + 1);
            }
            else
            {
                node.Tag = token;
                node.Nodes.Add(new TreeNode("加载中...") { ForeColor = Color.FromArgb(128, 128, 128) });
            }
        }

        private void TreeView_BeforeExpandLazy(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node?.Tag is JToken token)
            {
                var tv = (TreeView)sender;
                tv.BeginUpdate();
                try
                {
                    e.Node.Nodes.Clear();
                    AddJTokenNodesLazy(e.Node, token, 0);
                    e.Node.Tag = null;
                }
                finally
                {
                    tv.EndUpdate();
                }
            }
        }

        #endregion

        #region TreeView 选中复制支持

        /// <summary>
        /// 为所有TreeView挂载右键菜单 + Ctrl+C复制 + 右键自动选中
        /// </summary>
        private void SetupTreeViewsCopySupport()
        {
            var trees = new[]
            {
                tvLevelDbJson, tvPanelInfoJson, tvVbInferenceJson, tvInferenceReturnJson,
                tvInferAnalysis, tvAviWriteBackJson, tvVrsWriteBackJson, tvVrsV1WriteBackJson
            };

            foreach (var tv in trees)
            {
                tv.ContextMenuStrip = BuildTreeContextMenu(tv);
                tv.MouseDown += TreeView_MouseDownSelect;
                tv.KeyDown += TreeView_KeyDownCopy;
            }

            // PanelInfo / VB推理JSON / 推理返回JSON 启用大文件懒加载展开
            tvPanelInfoJson.BeforeExpand += TreeView_BeforeExpandLazy;
            tvVbInferenceJson.BeforeExpand += TreeView_BeforeExpandLazy;
            tvInferenceReturnJson.BeforeExpand += TreeView_BeforeExpandLazy;
        }

        private ContextMenuStrip BuildTreeContextMenu(TreeView tv)
        {
            var menu = new ContextMenuStrip
            {
                BackColor = Color.FromArgb(40, 60, 75),
                ForeColor = Color.White
            };

            var miCopy = new ToolStripMenuItem("复制节点文本") { ShortcutKeyDisplayString = "Ctrl+C" };
            miCopy.Click += (s, e) => CopySelectedNodeText(tv, false);

            var miCopySub = new ToolStripMenuItem("复制节点(含子节点)");
            miCopySub.Click += (s, e) => CopySelectedNodeText(tv, true);

            var miExpand = new ToolStripMenuItem("展开当前节点");
            miExpand.Click += (s, e) => tv.SelectedNode?.ExpandAll();

            var miCollapse = new ToolStripMenuItem("折叠当前节点");
            miCollapse.Click += (s, e) => tv.SelectedNode?.Collapse();

            menu.Items.Add(miCopy);
            menu.Items.Add(miCopySub);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(miExpand);
            menu.Items.Add(miCollapse);
            return menu;
        }

        private void TreeView_MouseDownSelect(object sender, MouseEventArgs e)
        {
            var tv = (TreeView)sender;
            var node = tv.GetNodeAt(e.X, e.Y);
            if (node != null) tv.SelectedNode = node;
        }

        private void TreeView_KeyDownCopy(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                e.SuppressKeyPress = true;
                CopySelectedNodeText((TreeView)sender, false);
            }
        }

        private void CopySelectedNodeText(TreeView tv, bool includeChildren)
        {
            var node = tv.SelectedNode;
            if (node == null) return;

            string text;
            if (includeChildren && node.Nodes.Count > 0)
            {
                var sb = new System.Text.StringBuilder();
                AppendNodeText(sb, node, 0);
                text = sb.ToString();
            }
            else
            {
                text = node.Text;
            }

            if (!string.IsNullOrEmpty(text))
            {
                try { Clipboard.SetText(text); } catch { /* 剪贴板被占用时忽略 */ }
            }
        }

        #endregion

        #region 复制 / 搜索

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                var currentTab = tabControl.SelectedTab;

                // 优先从存储的原始JSON获取
                if (_tabRawJsonMap.TryGetValue(currentTab, out var rawJson) && !string.IsNullOrEmpty(rawJson))
                {
                    Clipboard.SetText(rawJson);
                    MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 其他信息Tab仍使用RichTextBox
                if (currentTab?.Controls.Count > 0 && currentTab.Controls[0] is RichTextBox rtb)
                {
                    if (!string.IsNullOrEmpty(rtb.Text))
                    {
                        Clipboard.SetText(rtb.Text);
                        MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrSnDebugInfo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                e.SuppressKeyPress = true;
                searchPanel.Visible = true;
                txtSearch.Focus();
                txtSearch.SelectAll();
            }
            else if (e.KeyCode == Keys.Escape && searchPanel.Visible)
            {
                searchPanel.Visible = false;
                e.SuppressKeyPress = true;
            }
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                FindNext();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                searchPanel.Visible = false;
                e.SuppressKeyPress = true;
            }
        }

        private void FindNext()
        {
            string keyword = txtSearch.Text;
            if (string.IsNullOrEmpty(keyword)) return;

            var currentTab = tabControl.SelectedTab;
            if (currentTab?.Controls.Count > 0)
            {
                if (currentTab.Controls[0] is TreeView tv)
                {
                    FindNextInTreeView(tv, keyword);
                }
                else if (currentTab.Controls[0] is RichTextBox rtb)
                {
                    FindNextInRichTextBox(rtb, keyword);
                }
            }
        }

        private void FindNextInTreeView(TreeView tv, string keyword)
        {
            // 收集所有节点为平铺列表
            var allNodes = new List<TreeNode>();
            CollectAllNodes(tv.Nodes, allNodes);

            if (allNodes.Count == 0) return;

            // 找到上次搜索节点的位置
            int startIdx = 0;
            if (_lastSearchNode != null)
            {
                int lastIdx = allNodes.IndexOf(_lastSearchNode);
                if (lastIdx >= 0) startIdx = lastIdx + 1;
            }

            // 从上次位置开始搜索
            for (int i = 0; i < allNodes.Count; i++)
            {
                int idx = (startIdx + i) % allNodes.Count;
                var node = allNodes[idx];
                if (node.Text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // 如果回到了起始点之前说明已经绕了一圈
                    if (i > 0 && idx < startIdx && _lastSearchNode != null)
                    {
                        // 继续，回绕搜索
                    }

                    tv.SelectedNode = node;
                    node.EnsureVisible();
                    _lastSearchNode = node;
                    tv.Focus();
                    return;
                }
            }

            MessageBox.Show("未找到匹配内容", "搜索", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _lastSearchNode = null;
        }

        private void CollectAllNodes(TreeNodeCollection nodes, List<TreeNode> list)
        {
            foreach (TreeNode node in nodes)
            {
                list.Add(node);
                if (node.Nodes.Count > 0)
                    CollectAllNodes(node.Nodes, list);
            }
        }

        private void FindNextInRichTextBox(RichTextBox rtb, string keyword)
        {
            int startIndex = _lastSearchIndex;
            if (startIndex >= rtb.TextLength) startIndex = 0;

            int index = rtb.Find(keyword, startIndex, RichTextBoxFinds.None);
            if (index >= 0)
            {
                rtb.Select(index, keyword.Length);
                rtb.SelectionBackColor = Color.FromArgb(255, 200, 50);
                rtb.SelectionColor = Color.Black;
                rtb.ScrollToCaret();
                _lastSearchIndex = index + keyword.Length;
            }
            else
            {
                if (startIndex > 0)
                {
                    index = rtb.Find(keyword, 0, RichTextBoxFinds.None);
                    if (index >= 0)
                    {
                        rtb.Select(index, keyword.Length);
                        rtb.SelectionBackColor = Color.FromArgb(255, 200, 50);
                        rtb.SelectionColor = Color.Black;
                        rtb.ScrollToCaret();
                        _lastSearchIndex = index + keyword.Length;
                        return;
                    }
                }
                MessageBox.Show("未找到匹配内容", "搜索", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _lastSearchIndex = 0;
            }
        }

        #endregion
    }
}


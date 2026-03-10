using DeepSightAI.SettingPages;
using DeepSightDB;
using DeepSightDisplay;
using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class FrHome : Form
    {
        #region 单例模式

        private static FrHome _instance;

        public static FrHome Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FrHome();
                return _instance;
            }
        }

        #endregion

        #region 字段

        // 公共数据（线程安全）
        public ConcurrentDictionary<string, List<RootPanelInfoWithIP>> dic_Infos = new ConcurrentDictionary<string, List<RootPanelInfoWithIP>>();
        public ConcurrentDictionary<string, List<string>> dic_Results = new ConcurrentDictionary<string, List<string>>();
        /// <summary>推理后的缺陷ROI信息（来源于PostProcessService推理结果）</summary>
        public ConcurrentDictionary<string, List<Roi>> dic_DetectRois = new ConcurrentDictionary<string, List<Roi>>();

        // 面板信息
        private List<RootPanelInfoWithIP> info = null;
        private List<DisPlayInfo> disInfosList = new List<DisPlayInfo>();

        // 图片路径与缺陷信息
        private List<string> imagePaths = new List<string>();          // 缺陷图
        private List<string> imagePaths_Gerber = new List<string>();   // Gerber图
        private List<string> imagePaths_Template = new List<string>(); // Template图
        private List<Roi> defectRois = new List<Roi>();                // 缺陷框信息

        // 分页
        public int Index = 0;            // 缺陷小图索引
        private int totalPages = 0;      // 总页数
        private int currentPage = 1;     // 当前页码
        public string str_SN = "";       // 记录点击的SN

        // 显示控件
        public CvDisplay[] DispWin2 = null;

        // 定时器
        public System.Timers.Timer uph_timer = new System.Timers.Timer();
        private readonly object _updateLock = new object();

        // 日志去重
        private string logstr = string.Empty;

        #endregion

        #region 构造与初始化

        public FrHome()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            InitializeUI();
            Load += FrHome_Load;

            aviCtr2Container.ShowDeleteButtons = false;
            uph_timer.Interval = 1000 * 6;
            uph_timer.Enabled = true;
            uph_timer.Elapsed += Uph_timer_Elapsed;
        }

        private void FrHome_Load(object sender, EventArgs e)
        {
            LogTextHelper.OnCallBackLogProc -= Log_single_OnCallBackLogProc;
            LogTextHelper.OnCallBackLogProc += Log_single_OnCallBackLogProc;
        }

        private void InitializeUI()
        {
            aviCtr2Container.CreateMachinePanels(Machine.aviconfig.WatchPaths);
        }

        #endregion

        #region 日志输出

        private void Log_single_OnCallBackLogProc(string msg, Color color)
        {
            try
            {
                if (logstr != msg)
                {
                    OutputMsg(msg, color);
                }
                logstr = msg;
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 显示提示信息
        /// </summary>
        /// <param name="msg">信息内容</param>
        /// <param name="color">颜色显示</param>
        public void OutputMsg(string msg, Color color)
        {
            try
            {
                // 使用 BeginInvoke 异步更新 UI，避免阻塞工作线程
                rich_log.BeginInvoke(new MethodInvoker(() =>
                {
                    if (rich_log.Lines.Length > 300)
                    {
                        rich_log.Clear();
                    }
                    int Start = rich_log.SelectionStart;
                    rich_log.SelectionStart = rich_log.Text.Length;//设置插入符位置为文本框末
                    rich_log.SelectionColor = color;
                    rich_log.AppendText(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss,fff]----> ") + msg + "\r\n");
                    rich_log.SelectionStart = rich_log.TextLength;
                    rich_log.ScrollToCaret();
                }));
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        /// <summary>
        /// 清空日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearLog_Click(object sender, EventArgs e)
        {
            try
            {
                this.rich_log.Invoke(new MethodInvoker(() =>
                {
                    rich_log.Clear();
                }));
            }
            catch (System.Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }
        private void btnShowLog_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Application.StartupPath + "\\Log\\");
            }
            catch (System.Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        #endregion

        #region 菜单与工具事件

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
                if (toolStripMenuItem.Tag != null)
                {
                    int index = (int)toolStripMenuItem.Tag;

                    string[] str = toolStripMenuItem.Name.Split(';');
                    string strcamera = toolStripMenuItem.Text;

                    string station = str[0];
                    int workIndex = int.Parse(str[1]);
                    int snapIndex = int.Parse(str[2]);
                    if (index >= 0)
                    {
                        OpenFileDialog dig_openImage = new OpenFileDialog
                        {
                            Title = "请选择图像文件",
                            RestoreDirectory = true,
                            FilterIndex = 1
                        };
                        dig_openImage.Filter = string.Format("{1} | *{0}*.bmp; *{0}*.jpg; *{0}*.jpeg; *{0}*.gif; *{0}*.png; *{0}*.tif;", station, "图片文件");
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        public void InitWork()
        {
            try
            {
                var displaysList = new List<CvDisplay>();
                for (int j = 0; j < DispWin2.Length; ++j)
                {
                    displaysList.Add(DispWin2[j]);
                }
                Machine.master.SetHWindow(displaysList);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
            }
        }

        #endregion

        #region 定时器与看板更新

        private void Uph_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 防止重入：如果上一次更新还在运行，则跳过本次更新
            if (!Monitor.TryEnter(_updateLock))
            {
                LogTextHelper.Info("定时器更新被跳过，因为上一次更新仍在进行中");
                return;
            }

            try
            {

                // 使用Task.Run在后台线程执行异步操作，避免阻塞定时器线程
                Task.Run(async () => await Uph_timer_UpdateAsync());
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
            finally
            {
                Monitor.Exit(_updateLock);
            }
        }

        /// <summary>
        /// 定时器更新的异步处理逻辑，包含超时保护
        /// </summary>
        private async Task Uph_timer_UpdateAsync()
        {
            // 使用CancellationToken实现超时保护（5秒内必须完成）
            using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                try
                {
                    // 更新右下角统计信息
                    UpdateMainBorad();

                    // 更新机台看板（异步操作）
                    await UpdateMachineBoardWithTimeout(cts.Token);

                    // 更新LotSn
                    UpdateLotSn();

                    // 检查所有工站是否超时（10秒无数据则状态变灰）
                    CheckAllStationsTimeout();
                }
                catch (OperationCanceledException)
                {
                    LogTextHelper.Warn("定时器更新超时（超过5秒），本次更新被中止");
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error("定时器更新异常", ex);
                }
            }
        }

        /// <summary>
        /// 带超时保护的机台看板更新
        /// </summary>
        private async Task UpdateMachineBoardWithTimeout(CancellationToken cancellationToken)
        {
            try
            {
                await UpdateMachineBoard().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                LogTextHelper.Warn("机台看板更新超时");
                throw;
            }
        }

        private void UpdateMainBorad()
        {
            var boardStat = BoardStatCache.GetTodayStat();

            if (this.IsHandleCreated)
            {
                string FormatPercent(int numerator, int denominator)
                {
                    if (denominator <= 0)
                    {
                        return "0.0%";
                    }
                    return ((double)numerator / denominator).ToString("P1");
                }

                int aiProcessedCount = Math.Max(0, boardStat.AiFilterCount - boardStat.AiFilterUninspectedCount);
                string avgDefectText = boardStat.AviPanelCount > 0
                    ? ((double)boardStat.AiFilterCount / boardStat.AviPanelCount).ToString("0.0")
                    : "0.0";
                string aviPassRateBefore = FormatPercent(boardStat.AviPanelOKCount, boardStat.AviPanelCount);
                string aviPassRateAfter = FormatPercent(boardStat.AiPanelOKCount, boardStat.AviPanelCount);
                string aiPassRate = FormatPercent(boardStat.AiFilterOKCount, aiProcessedCount);

                this.BeginInvoke(new Action(() =>
                {
                    lbl_SnTotalCount.Text = $"今日总产量\n{boardStat.AviPanelCount}";
                    lbl_totalDefectCount.Text = $"AVI产生图片数\n{boardStat.AiFilterCount}";
                    lbl_AiAllCount.Text = $"AI推理图片数\n{aiProcessedCount}";

                    lbl_aiFilterOKCount.Text = $"AI通过图片数\n{boardStat.AiFilterOKCount}";
                    lbl_aviPassRateCount.Text = $"AVI一次通过率\n{aviPassRateBefore}";
                    lbl_filteredOkCount.Text = $"报点过滤率\n{aiPassRate}";

                    lbl_utilizationRate.Text = $"今日机台利用率\n{boardStat.Utilization:P1}";
                    lbl_boardAiPassRate.Text = $"AI通过率\n{aviPassRateAfter}";
                    lbl_CountPerPanel.Text = $"平均报点数\n{avgDefectText}";
                }));
            }
        }

        private async Task UpdateMachineBoard()
        {
            await aviCtr2Container.UpdateMachineBoardFromCache();
        }

        #endregion

        #region 控件布局初始化

        /// <summary>
        /// 窗体初始化 - 创建显示控件布局
        /// </summary>
        internal void InitMethod()
        {
            try
            {
                DispWin2 = new CvDisplay[10];
                //布局
                table_Small.Controls.Clear();
                table_Small.RowStyles.Clear();
                table_Small.ColumnStyles.Clear();

                //table_Small.ColumnCount = 4;
                table_Small.ColumnCount = 2;
                table_Small.RowCount = 3;

                int index = 0;
                for (int i = 0; i < table_Small.RowCount; i++)
                {
                    table_Small.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (100F / 1)));

                    for (int j = 0; j < table_Small.ColumnCount; j++)
                    {
                        table_Small.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F / 20));

                        DispWin2[index] = new CvDisplay
                        {
                            Margin = new System.Windows.Forms.Padding(1),
                            BackColor = Color.FromArgb(29, 48, 60),//ColorTranslator.FromHtml("#374c50"),//System.Drawing.SystemColors.InactiveCaption,
                            Dock = System.Windows.Forms.DockStyle.Fill,
                            Name = "Display" + index,
                            AutoDisplay = CvDisplay.AutoDisplayMode.Fit,
                        };
                        DispWin2[index].stationIndex = index + 1;
                        DispWin2[index].OnCallBackFullShowPro -= FrHome_OnCallBackFullShowPro;
                        DispWin2[index].OnCallBackFullShowPro += FrHome_OnCallBackFullShowPro;
                        DispWin2[index].OnCallBackRoiIndexAndInfo -= FrHome_OnCallBackRoiIndexAndInfo;
                        DispWin2[index].OnCallBackRoiIndexAndInfo += FrHome_OnCallBackRoiIndexAndInfo;
                        table_Small.Controls.Add(DispWin2[index], j, i);
                        index++;
                    }
                }
                InitWork();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }
        /// <summary>
        /// 重置panel
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="count">图片数量</param>
        public void InitTableStyle(TableLayoutPanel panel, int pixNum, List<DisPlayInfo> infos)
        {
            try
            {
                int index = 0;
                for (int i = 0; i < table_Small.RowCount; i++)
                {
                    for (int j = 0; j < table_Small.ColumnCount / 2; j++)
                    {
                        if (Index + index < infos.Count)
                        {
                            DispWin2[(index + 1) * 2 - 2].info = infos[Index + index];
                            DispWin2[(index + 1) * 2 - 1].info = infos[Index + index];
                            if (index + 1 <= pixNum)
                            {
                                DispWin2[(index + 1) * 2 - 2].DrawStation($"缺陷{Index + index + 1}:{infos[Index + index].defect_code}");
                                DispWin2[(index + 1) * 2 - 1].DrawStation($"缺陷{Index + index + 1}:{infos[Index + index].defect_code}");
                            }
                        }
                        else
                        {
                            DispWin2[(index + 1) * 2 - 2].DrawStation($"");
                            DispWin2[(index + 1) * 2 - 1].DrawStation($"");
                        }
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                throw;
            }

        }
        private void FrHome_OnCallBackRoiIndexAndInfo(int index, DisPlayInfo info)
        {
            // 预留回调
        }

        /// <summary>
        /// 递归加载JSON到TreeView
        /// </summary>
        /// <param name="token"></param>
        /// <param name="parentNodes"></param>
        private void LoadJsonToTreeView(JToken token, TreeNodeCollection parentNodes)
        {
            try
            {
                if (token == null) return;
                //
                switch (token.Type)
                {
                    case JTokenType.Object:
                        foreach (var prop in (JObject)token)
                        {
                            var node = parentNodes.Add(prop.Key);
                            LoadJsonToTreeView(prop.Value, node.Nodes);
                        }
                        break;

                    case JTokenType.Array:
                        int index = 0;
                        foreach (var item in (JArray)token)
                        {
                            var node = parentNodes.Add($"[{index}]");
                            LoadJsonToTreeView(item, node.Nodes);
                            index++;
                        }
                        break;

                    default:
                        parentNodes.Add(token.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                throw;
            }

        }
        public void FrHome_OnCallBackFullShowPro(string station, int index, string m_station, string status, string ocr, global::OpenCvSharp.Mat mat)
        {
            try
            {
                FrFullImage.Instance.cvDisplay1.stationName = "A";
                FrFullImage.Instance.cvDisplay1.stationIndex = 1;
                FrFullImage.Instance.LoadShow(mat.Clone());
                FrFullImage.Instance.cvDisplay1.DrawStation(m_station);
                FrFullImage.Instance.cvDisplay1.DrawStatus(status);
                FrFullImage.Instance.cvDisplay1.DrawOCR(ocr);
                FrFullImage.Instance.Show();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
            }

        }

        #endregion

        #region 数据网格与分页

        public void dataGridViewData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //此标志打开，手动点击才生效，否则，不允许手动点击，只允许自动生产的刷新
            if (!FrmMain.Instance.IsAllow)
            {
                if (e != null)
                {
                    return;
                }
            }
            try
            {
                if (e != null)
                {
                    if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                }
                string SN = string.Empty;
                if (e == null)
                {
                    SN = str_SN;
                }
                else
                {
                    int rowIndex = e.RowIndex;
                    SN = dataGridViewData.Rows[rowIndex].Cells[0].Value?.ToString() ?? "空值";
                    str_SN = SN;
                }
                //初始化信息
                Index = 0;
                currentPage = 1;
                imagePaths?.Clear();
                imagePaths_Gerber?.Clear();
                imagePaths_Template?.Clear();
                defectRois?.Clear();
                disInfosList?.Clear();
                if (dic_Infos.TryGetValue(SN, out info))
                {
                    //这里可能涉及到A/B面的切换
                    //A面  info[0]
                    //B面  info[1]
                    //读取 缺陷小图
                    int index = 0;

                    // 获取推理后的ROI数据（来自PostProcessService推理结果）
                    List<Roi> inferRois = null;
                    dic_DetectRois.TryGetValue(SN, out inferRois);
                    int roiIndex = 0;

                    for (int i = 0; i < info.Count; i++)
                    {

                        for (int j = 0; j < info[i].RootInfo.PcsInfo.Count; j++)
                        {
                            if (info[i].RootInfo.PcsInfo.TryGetValue((j + 1).ToString(), out PcsInfo pcsInfo))
                            {
                                for (int k = 0; k < pcsInfo.DefectInfo.Count; k++)
                                {
                                    disInfosList.Add(new DisPlayInfo()
                                    {
                                        defect_code = pcsInfo.DefectInfo[k].DefectCode,
                                        defect_index = pcsInfo.DefectInfo[k].DefectIndex.ToString(),
                                        product_serial = info[i].RootInfo.ProductSerial,
                                        pcs_index = pcsInfo.DefectInfo[k].PcsIndex.ToString(),
                                        defect_location = pcsInfo.DefectInfo[k].DefectLocation,
                                        sn = SN,//info[i].rootInfo.SerialNumber,
                                        process_time = info[i].RootInfo.EndTime,
                                        ai_infer_result = pcsInfo.DefectInfo[k].AiInferResult,
                                        station_name = info[i].RootInfo.StationName,
                                        dateil = "",
                                        side_index = info[i].RootInfo.SideIndex,
                                        lot_id = info[i].RootInfo.LotId,
                                        lot_batch = info[i].RootInfo.LotBatch,
                                    });
                                    if (pcsInfo.DefectInfo[k].DefectVrsImages != null)
                                    {
                                        imagePaths.Add($"{info[i].Head}/{pcsInfo.DefectInfo[k].DefectVrsImages[0].ToString()}:{info[i].IP}");
                                        // 使用推理后的ROI数据（来自PostProcessService的InferDetails.Location）
                                        if (inferRois != null && roiIndex < inferRois.Count
                                            && (inferRois[roiIndex].Width > 0 || inferRois[roiIndex].Height > 0))
                                        {
                                            defectRois.Add(inferRois[roiIndex]);
                                        }
                                        else
                                        {
                                            defectRois.Add(pcsInfo.DefectInfo[k].DefectRoi);
                                        }
                                        roiIndex++;
                                    }
                                    if (pcsInfo.DefectInfo[k].DefectVrsGerberImages != null)
                                    {
                                        imagePaths_Gerber.Add($"{info[i].Head}/{pcsInfo.DefectInfo[k].DefectVrsGerberImages[0].ToString()}:{info[i].IP}");
                                    }
                                    if (pcsInfo.DefectInfo[k].DefectVrsOkImages != null)
                                    {
                                        imagePaths_Template.Add($"{info[i].Head}/{pcsInfo.DefectInfo[k].DefectVrsOkImages[0].ToString()}:{info[i].IP}");
                                    }
                                    index++;
                                }
                            }
                        }
                    }
                    totalPages = (int)Math.Ceiling((double)index / table_Small.RowCount);
                    ShowImage();
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常" + ex.ToString());
            }

        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            try
            {
                //获取上一页的索引
                if (currentPage < totalPages)
                {
                    btnNext.Enabled = false;
                    btnPrevious.Enabled = false;
                    currentPage++;
                    ShowImage();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentPage > 1)
                {
                    btnNext.Enabled = false;
                    btnPrevious.Enabled = false;
                    currentPage--;
                    ShowImage();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 右键菜单 - 显示详情
        /// </summary>
        private void btnShowDebugInfo_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewData.CurrentRow == null) return;

                string sn = dataGridViewData.CurrentRow.Cells[0].Value?.ToString();
                if (string.IsNullOrEmpty(sn)) return;

                // 获取该SN的所有调试信息
                var debugInfos = SnDebugInfoCache.GetBySn(sn);

                // 弹出调试信息窗口
                var form = new FrSnDebugInfo(sn, debugInfos);
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                DeepSightTool.LogTextHelper.Error($"显示SN详情异常: {ex}");
            }
        }

        #endregion

        #region 图片显示

        private async void ShowImage()
        {
            try
            {
                int pageSize = table_Small.RowCount;
                int skipCount = (currentPage - 1) * pageSize;
                Index = skipCount;

                InitTableStyle(table_Small, pageSize, disInfosList);
                InitWork();

                // 获取当前页数据
                var gerberOrtemp_paths = Machine.ShowFlag == "B" ? imagePaths_Gerber : imagePaths_Template;
                var defect_pagedData = imagePaths.Skip(skipCount).Take(pageSize).ToList();
                var gerberOrtemp_pagedData = gerberOrtemp_paths.Skip(skipCount).Take(pageSize).ToList();
                var defect_pagedRois = defectRois.Skip(skipCount).Take(pageSize).ToList();
                var defect_indexPaths = defect_pagedData.Select((path, idx) => new { Path = path, Index = idx }).ToList();
                var gerberOrtemp_indexPaths = gerberOrtemp_pagedData.Select((path, idx) => new { Path = path, Index = idx }).ToList();

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount - 1,
                    CancellationToken = CancellationToken.None
                };

                dic_Results.TryGetValue(str_SN, out List<string> res_lbl);

                // 显示缺陷图
                await Task.Factory.StartNew(() =>
                {
                    Parallel.ForEach(defect_indexPaths, parallelOptions, item =>
                    {
                        string labelText = GetLabelText(res_lbl, skipCount + item.Index);
                        Roi roi = (item.Index >= 0 && item.Index < defect_pagedRois.Count) ? defect_pagedRois[item.Index] : null;
                        Machine.master.ShowImage(item.Path, (item.Index + 1) * 2 - 2, labelText, roi);
                    });
                });

                // 显示Gerber/Template图
                await Task.Factory.StartNew(() =>
                {
                    Parallel.ForEach(gerberOrtemp_indexPaths, parallelOptions, item =>
                    {
                        string labelText = GetLabelText(res_lbl, skipCount + item.Index);
                        Machine.master.ShowImage(item.Path, (item.Index + 1) * 2 - 1, labelText);
                    });
                });

                // 最后一页时清空多余的显示位
                if (currentPage == totalPages)
                {
                    await Task.Factory.StartNew(() =>
                    {
                        Parallel.For(defect_pagedData.Count, pageSize, item =>
                        {
                            Machine.master.ShowImage("", (item + 1) * 2 - 2);
                        });
                    });
                    await Task.Factory.StartNew(() =>
                    {
                        Parallel.For(gerberOrtemp_pagedData.Count, pageSize, item =>
                        {
                            Machine.master.ShowImage("", (item + 1) * 2 - 1);
                        });
                    });
                }

                UpdatePagingControls();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常" + ex.ToString());
            }
        }

        /// <summary>
        /// 获取缺陷标签文本
        /// </summary>
        private string GetLabelText(List<string> resultLabels, int index)
        {
            if (resultLabels != null && index >= 0 && index < resultLabels.Count)
                return resultLabels[index] ?? "未处理";
            return "未处理";
        }

        private void UpdatePagingControls()
        {
            try
            {
                lblPageInfo.Text = $"第 {currentPage} 页 / 共 {totalPages} 页";
                btnPrevious.Enabled = currentPage > 1;
                btnNext.Enabled = currentPage < totalPages;
            }
            catch (Exception ex)
            {
                LogTextHelper.Info("更新上下页异常" + ex.ToString());
            }
        }

        #endregion

        #region 配置与状态管理

        private void UpdateLotSn()
        {
            aviCtr2Container.UpdateAllAviCtrLotSnFromCache();
        }
        /// <summary>
        /// 更新所有AviCtr控件的配置（保存机台配置后调用）
        /// </summary>
        public void RefreshAviCtrConfigs()
        {
            if (this.IsHandleCreated)
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() => aviCtr2Container.CreateMachinePanels(Machine.aviconfig.WatchPaths)));
                }
                else
                {
                    aviCtr2Container.CreateMachinePanels(Machine.aviconfig.WatchPaths);
                }
            }
        }

        /// <summary>
        /// 更新指定工站的数据接收时间（状态变为绿色）
        /// </summary>
        /// <param name="machineName">工站名称</param>
        public void UpdateStationDataReceived(string machineName)
        {
            aviCtr2Container.UpdateStationDataReceived(machineName);
        }

        /// <summary>
        /// 检查所有工站是否超时，超时则将状态设为灰色
        /// </summary>
        public void CheckAllStationsTimeout()
        {
            aviCtr2Container.CheckAllStationsTimeout();
        }

        #endregion
    }
}


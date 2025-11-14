using DeepSightAI.SettingPages;
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

        /// <summary>
        /// 显示控件的数量
        /// </summary>
        private int Num = 0;

        public Dictionary<string, List<RootPanelInfoWithIP>> dic_Infos = new Dictionary<string, List<RootPanelInfoWithIP>>();
        public Dictionary<string, List<string>> dic_Results = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> dic_Details = new Dictionary<string, List<string>>();
        public Dictionary<string, PcsResult> dic_PcsResult = new Dictionary<string, PcsResult>();
        public Dictionary<string, List<string>> dic_Paths = new Dictionary<string, List<string>>();


        private List<RootPanelInfoWithIP> info = null;
        private List<DisPlayInfo> disInfosList = new List<DisPlayInfo>();
        //缺陷图
        private List<string> imagePaths = new List<string>();
        //gerber图
        private List<string> imagePaths_Gerber = new List<string>();
        //template图
        private List<string> imagePaths_Template = new List<string>();
        public int Index = 0;//缺陷小图索引
        private int totalPages = 0; // 总页数
        private int currentPage = 1;// 当前页码
        //记录点击的SN
        public string str_SN = "";

        /// <summary>
        /// 窗体对象实例（单例模式）
        /// </summary>
        private static FrHome _instance;

        public static FrHome Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrHome();
                }

                return _instance;
            }
        }

        public FrHome()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            InitializeUI();
            Load += FrHome_Load;
            FormClosing += FrHome_FormClosing;

            uph_timer.Interval = 1000 * 4;
            uph_timer.Enabled = true;
            uph_timer.Elapsed += Uph_timer_Elapsed;
        }

        private void FrHome_Load(object sender, EventArgs e)
        {
            //LoadMethod();
            LogTextHelper.OnCallBackLogProc -= Log_single_OnCallBackLogProc;
            LogTextHelper.OnCallBackLogProc += Log_single_OnCallBackLogProc;
            //this.Shown += FrHome_Shown;
            //InitializeUI();
            // 禁用视觉样式
            //dataProductInfo.EnableHeadersVisualStyles = false;
        }

        private void FrHome_Shown(object sender, EventArgs e)
        {
            //InitializeUI();
        }

        private void InitializeUI()
        {
            aviCtr2Container.CreateMachinePanels(Machine.aviconfig.WatchPaths);
        }

        private string logstr = string.Empty;//主要用于判断回调多次 
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
                rich_log.Invoke(new MethodInvoker(() =>
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
        private void FrHome_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        /// <summary>
        /// 0:表示白班
        /// 1：表示晚班
        /// </summary>
        private int shiftFlag = -1;


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

                        //if (dig_openImage.ShowDialog() == DialogResult.OK)
                        //{
                        //    Task.Factory.StartNew(() =>
                        //    {
                        //        for (int i = 0; i < Machine.masterWorkClass.stationWorkClass.Length; i++)
                        //        {
                        //            if (Machine.masterWorkClass.stationWorkClass[i].stationConfig.Station == station)
                        //            {
                        //                Machine.masterWorkClass.stationWorkClass[i].workClasses[workIndex].TestImage(snapIndex, dig_openImage.FileName);
                        //            }
                        //        }
                        //    });
                        //}
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
                List<CvDisplay> DisplaysList = new List<CvDisplay>();
                try
                {
                    for (int j = 0; j < DispWin2.Length; ++j)
                    {
                        DisplaysList.Add(DispWin2[j]);
                    }
                    Machine.master.workClass.setHWindow(DisplaysList);
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error("异常", ex);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
            }
        }


        private void MasterWorkClass_OnWorkResultPro(string productId, int status)
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }
        // string m_productId = string.Empty;
        private void MasterWorkClass_OnWorkTotalPro(string productId)
        {
            try
            {



            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        public System.Timers.Timer uph_timer = new System.Timers.Timer();
        private void Uph_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                //var today = DateTime.Today;
                //var stats = Machine.master.workClass.GetDefectCountsPerMachine(today, today.AddDays(1).AddTicks(-1));
                //foreach (var machineStat in stats)
                //{
                //    string machineName = machineStat.Key;
                //    int totalDefects = (int)machineStat.Value.TotalDefects;
                //    int aiOkDefects = (int)machineStat.Value.AIOkDefects;

                //    if (this.IsHandleCreated)
                //    {
                //        this.BeginInvoke(new Action(() =>
                //        {
                //            aviCtr2Container.UpdateAviCtrStats(machineName, totalDefects, aiOkDefects);
                //        }));
                //    }
                //}
                UpdateMainBorad();
                UpdateMachineBoard();
                UpdateLotSn();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        private async void UpdateMainBorad()
        {
            var today = DateTime.Today;
            var AllData = await Machine.master.workClass.GetPanelsData(today, today.AddDays(1).AddTicks(-1));
            var boardStat =PanelDataRecord.GetBoardStat(AllData);

            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    lbl_SnTotalCount.Text = $"今日产量Array\n{boardStat.aviPanelCount}";
                    lbl_totalDefectCount.Text = $"AVI产生图片数\n{boardStat.aiFilterCount}";
                    lbl_AiAllCount.Text = $"AI推理图片数\n{boardStat.aiFilterCount - boardStat.aiFilterUninspectedCount}";

                    lbl_aiFilterOKCount.Text = $"AI Pass 图片数\n{boardStat.aiFilterOKCount}";
                    lbl_aviPassRateCount.Text = $"AVI Pass Rate_AI前\n{(double)boardStat.aviPanelOKCount / (boardStat.aviPanelCount):P1}";
                    lbl_filteredOkCount.Text = $"AI Pass Rate\n{(double)boardStat.aiFilterOKCount / (boardStat.aiFilterCount - boardStat.aiFilterUninspectedCount):P1}";

                    lbl_utilizationRate.Text= $"今日机台利用率\n-";
                    lbl_boardAiPassRate.Text=$"AVI Pass Rate_AI后\n{(double)(boardStat.aviPanelOKCount+boardStat.aiPanelOKCount) / (boardStat.aviPanelCount):P1}";
                    lbl_CountPerPanel.Text=$"平均报点数\n{(double)boardStat.aiFilterCount/boardStat.aviPanelCount:0.0}";
                }));
            }
        }

        private async Task UpdateMachineBoard()
        {
           await aviCtr2Container.UpdateAll(Machine.master.workClass.GetLatestLotAndProductSerial, Machine.master.workClass.GetPanelsDataByMachineAndLot);
        }


        //public CvDisplay[] DispWin1 = null;
        public CvDisplay[] DispWin2 = null;
        /// <summary>
        /// 窗体初始化
        /// </summary>
        internal void InitMethod()
        {
            try
            {
                #region 大图显示 //屏蔽
                //DispWin1 = new CvDisplay[1];
                //布局
                //table_show.Controls.Clear();
                //table_show.RowStyles.Clear();
                //table_show.ColumnStyles.Clear();

                //table_show.ColumnCount = 1;
                //table_show.RowCount = 1;
                //布局

                //int index = 0;
                //for (int i = 0; i < 1; i++)
                //{
                //    table_show.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (100F / 1)));

                //    for (int j = 0; j < 1; j++)
                //    {
                //        table_show.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F / 1));

                //        DispWin1[index] = new CvDisplay
                //        {
                //            Margin = new System.Windows.Forms.Padding(1),
                //            BackColor = ColorTranslator.FromHtml("#374c50"),//System.Drawing.SystemColors.ActiveCaptionText,//ActiveCaptionText,//Highlight,//
                //            Dock = System.Windows.Forms.DockStyle.Fill,
                //            Name = "Display" + index,
                //            AutoDisplay = CvDisplay.AutoDisplayMode.Fit,
                //        };
                //        DispWin1[index].OnCallBackFullShowPro -= FrHome_OnCallBackFullShowPro;
                //        DispWin1[index].OnCallBackFullShowPro += FrHome_OnCallBackFullShowPro;

                //        table_show.Controls.Add(DispWin1[index], j, i);
                //        index++;
                //    }
                //}
                #endregion
                //DispWin2 = new CvDisplay[20];
                DispWin2 = new CvDisplay[10];
                //布局
                table_Small.Controls.Clear();
                table_Small.RowStyles.Clear();
                table_Small.ColumnStyles.Clear();

                //table_Small.ColumnCount = 4;
                table_Small.ColumnCount = 2;
                table_Small.RowCount = 5;

                int index = 0;
                for (int i = 0; i < table_Small.RowCount; i++)
                {
                    table_Small.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (100F / 1)));

                    for (int j = 0; j < table_Small.ColumnCount; j++)
                    {
                        //if (j == 1 || j == 2)
                        //{
                        //    table_Small.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F / 15));
                        //}
                        //else
                        //{
                        //    table_Small.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F / 20));
                        //}
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
                        //0604增加单图测试
                        DispWin2[index].OnCallBackSingleTest -= FrHome_OnCallBackSingleTest;
                        DispWin2[index].OnCallBackSingleTest += FrHome_OnCallBackSingleTest;
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
        //单图测试
        private async void FrHome_OnCallBackSingleTest(int index)
        {
            try
            {
                if (!Machine.master.workClass.TestFlag)
                {
                    MessageBox.Show("当前系统处于生产模式,请切换为样品板测试模式", "模式提示", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    return;
                }
                if (Machine.master.workClass.isStart)
                {
                    MessageBox.Show("当前系统正在运行中,请先点击暂停", "运行提示", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    return;
                }
                if (imagePaths.Count <= 0)//|| imagePaths_Gerber.Count <= 0 || imagePaths_Template.Count <= 0)
                {
                    MessageBox.Show("缺陷图路径列表为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    return;
                }
                if (index % 2 == 0)//点击了gerber图或者是tem图
                {
                    MessageBox.Show("请点击缺陷图进行单图测试", "测试提示", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    return;
                }
                //展示VB
                //await Task.Factory.StartNew(()=>
                //  {

                this.Invoke(new MethodInvoker(() =>
                {
                    Machine.master.workClass.defect.ai_Defect.Vision_Show_View(1);
                }));
                // });

                //传图给VB
                RootVBInfo vBInfo = new RootVBInfo();

                vBInfo.MessageType = "visionbuilder_inference";
                vBInfo.paramsData = new ParamsData();
                //同一个任务的UUID是否要保持一致；
                vBInfo.paramsData.InferResUuid = Guid.NewGuid().ToString();
                vBInfo.paramsData.InferWholeData = new InferWholeData();
                vBInfo.paramsData.InferWholeData.ImageInferParams = new ImageInferParams();
                //solution_name
                vBInfo.paramsData.InferWholeData.ImageInferParams.PipelineName = Machine.solution; //"Test";
                vBInfo.paramsData.InferWholeData.ImageInferParams.NodeParams = new List<NodeParam>();
                vBInfo.paramsData.InferWholeData.ImageInferParams.NodeParams.Add(
                  new NodeParam()
                  {
                      NodeName = Machine.flow,//"1",
                      height = 200,
                      width = 200,
                  });

                vBInfo.paramsData.InferWholeData.ImageData = new ImageData();
                //#使⽤minio获取 则固定字段"minio"
                vBInfo.paramsData.InferWholeData.ImageData.DataType = "minio";
                vBInfo.paramsData.InferWholeData.ImageData.DataValue = new DataValue();
                vBInfo.paramsData.InferWholeData.ImageData.DataValue.InferImageGroup = new List<InferImageGroup>();

                InferImageGroup group = new InferImageGroup();
                group.GroupUuid = Guid.NewGuid().ToString();
                group.GroupInfos = new List<GroupInfo>();
                group.DefectCode = "";
                if (Machine.isSwitch)
                {
                    group.DefectCode = disInfosList[(currentPage - 1) * 5 + index].defect_code;
                }
                for (int k = 0; k < 3; k++)
                {
                    switch (k)
                    {
                        case 0:
                            if (imagePaths != null && imagePaths.Count > 0)
                            {
                                group.GroupInfos.Add(new GroupInfo()
                                {
                                    ImagePath = imagePaths[(currentPage - 1) * 5 + index / 2].Split(':').ToArray()[0].ToString(),//"20250508152421059165/discolor/20250508152421059165-A-discolor-pcs-X1Y1-vrs0-0.jpg",
                                    ImageUuid = Guid.NewGuid().ToString(),
                                    ImageType = "defect",
                                });
                            }
                            break;
                        case 1:
                            if (imagePaths_Template != null && imagePaths_Template.Count > 0)
                            {
                                group.GroupInfos.Add(new GroupInfo()
                                {
                                    ImagePath = imagePaths_Template[(currentPage - 1) * 5 + index / 2].Split(':').ToArray()[0].ToString(),//"20250508152421059165/discolor/20250508152421059165-A-discolor-pcs-X1Y1-vrs0-0-template-1.jpg",
                                    ImageUuid = Guid.NewGuid().ToString(),
                                    ImageType = "template",
                                });
                            }
                            break;
                        case 2:
                            if (imagePaths_Gerber != null && imagePaths_Gerber.Count > 0)
                            {
                                group.GroupInfos.Add(new GroupInfo()
                                {
                                    ImagePath = imagePaths_Gerber[(currentPage - 1) * 5 + index / 2].Split(':').ToArray()[0].ToString(),//"20250508152421059165/discolor/20250508152421059165-A-discolor-pcs-X1Y1-vrs0-0-gerber-1.jpg",
                                    ImageUuid = Guid.NewGuid().ToString(),
                                    ImageType = "gerber",
                                });
                            }
                            break;
                        default:
                            break;
                    }
                }
                vBInfo.paramsData.InferWholeData.ImageData.DataValue.InferImageGroup.Add(group);
                group.inspectDetails = new InspectDetails();
                group.inspectDetails.InferRois = new List<InferRoi>() { };

                vBInfo.paramsData.InferWholeData.OtherInfos = new Others();
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio = new ImageMminio();
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.access_key_id = "deepiobjectdata";
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.bucket = "deepiresults";
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.endpoint_url = "127.0.0.1";
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.secret_key = "deepiobject2019";
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.secret_port = "9102";

                List<string> result = null;
                List<string> details = null;
                RootPanelInfo panelinfo = null;
                PcsResult pcsResult;
                string vbJson;
                result = await Task.Factory.StartNew(() =>
                {
                    Machine.master.workClass.DefectMethod(new VBModel() { VbInfo = vBInfo, panelInfo = panelinfo }, out result, out details, out pcsResult, out vbJson);
                    return result;
                });
                if (result.Count() > 0)
                {
                    string res = Convert.ToInt32(result[0]) == 1 ? "NG" : "OK";
                    LogTextHelper.Info($"算法单图处理结果：{res}");
                    MessageBox.Show($"算法单图处理结果：{res}");
                }
                else
                {
                    LogTextHelper.Info($"算法单图处理失败，返回为空！");
                    MessageBox.Show($"算法单图处理失败，返回为空！");
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("单图处理异常" + ex.ToString());
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
                            //DispWin2[index].Image = null;
                            //DispWin2[index].Clear(); 
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
            try
            {
                //if (DispWin2[index - 1].Image == null)
                //{
                //    MessageBox.Show("图像为空，请检查！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
                //if (info == null)
                //{
                //    MessageBox.Show("信息为空，请检查！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}

                //this.lbl_defectName.Text = $"AVI缺陷名:{info.defect_code}";
                //this.lbl_liaohao.Text = $"料号名:{info.product_serial}";
                //this.lbl_dianwei.Text = $"点位号:{info.defect_location}";
                //this.lbl_gongdan.Text = $"工单号:{info.lot_id}";
                //this.lbl_batch.Text = $"批次号:{info.lot_batch}";
                //this.lbl_pcsIndex.Text = $"PCS号:{info.pcs_index}";
                //this.lbl_sn.Text = $"二维码:{info.sn}";//{info.sn.Split('_').ToArray()[0].ToString()}";
                //this.lbl_defectIndex.Text = $"缺陷号:{info.defect_index}";
                //this.lbl_time.Text = $"过站时间:{info.process_time}";
                //List<string> result = null;
                //if (dic_Results.TryGetValue(info.sn, out result))
                //{
                //    string res = Convert.ToInt32(result[(currentPage - 1) * 5 + index / 2]) == 1 ? "NG" : "OK";
                //    this.lbl_result.Text = $"AI结果:{res}";
                //}
                //else
                //{
                //    this.lbl_result.Text = $"AI结果:未处理";
                //}

                //this.lbl_station.Text = $"站别名:{info.station_name}";
                //this.lbl_details.Text = $"AI详情:{info.dateil}";
                //this.lbl_sideIndex.Text = $"面次信息:{info.side_index}";

                //显示Details
                // 清空 TreeView 
                //ATS 取消此显示
                //treeViewJson.Nodes.Clear();
                //// 解析 JSON 并填充 TreeView
                //try
                //{
                //    List<string> details = null;
                //    if (dic_Details.TryGetValue(info.sn, out details))
                //    {
                //        string res = details[(currentPage - 1) * 10 + index / 2];
                //        JToken rootToken = JToken.Parse(res);
                //        LoadJsonToTreeView(rootToken, treeViewJson.Nodes);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show($"JSON 解析错误: {ex.Message}");
                //}
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常" + ex.ToString());
            }

        }
        /// <summary>
        /// 递归节点
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
                disInfosList?.Clear();
                if (dic_Infos.TryGetValue(SN, out info))
                {
                    //这里可能涉及到A/B面的切换
                    //A面  info[0]
                    //B面  info[1]
                    //读取 缺陷小图
                    int index = 0;
                    for (int i = 0; i < info.Count; i++)
                    {
                        if (Machine.master.workClass.TestFlag)
                        {
                            List<string> paths = null;
                            if (dic_Paths.TryGetValue(SN, out paths))
                            {
                                info[i].rootInfo.LocalDescribePath = paths[i];
                            }
                        }
                        string path = string.Empty;
                        string result = string.Empty;
                        Machine.master.workClass.ParseMinioPath(info[i].rootInfo.LocalDescribePath, out path, out result);

                        for (int j = 0; j < info[i].rootInfo.PcsInfo.Count; j++)
                        {
                            PcsInfo pcsInfo = null;
                            if (info[i].rootInfo.PcsInfo.TryGetValue((j + 1).ToString(), out pcsInfo))
                            {
                                for (int k = 0; k < pcsInfo.DefectInfo.Count; k++)
                                {
                                    disInfosList.Add(new DisPlayInfo()
                                    {
                                        defect_code = pcsInfo.DefectInfo[k].DefectCode,
                                        defect_index = pcsInfo.DefectInfo[k].DefectIndex.ToString(),
                                        product_serial = info[i].rootInfo.ProductSerial,
                                        pcs_index = pcsInfo.DefectInfo[k].PcsIndex.ToString(),
                                        defect_location = pcsInfo.DefectInfo[k].DefectLocation,
                                        sn = SN,//info[i].rootInfo.SerialNumber,
                                        process_time = info[i].rootInfo.EndTime,
                                        ai_infer_result = pcsInfo.DefectInfo[k].AiInferResult,
                                        station_name = info[i].rootInfo.StationName,
                                        dateil = "",
                                        side_index = info[i].rootInfo.SideIndex,
                                        lot_id = info[i].rootInfo.LotId,
                                        lot_batch = info[i].rootInfo.LotBatch,
                                    });
                                    if (pcsInfo.DefectInfo[k].DefectVrsImages != null)
                                    {
                                        imagePaths.Add($"{result}/{pcsInfo.DefectInfo[k].DefectVrsImages[0].ToString()}:{info[i].IP}");
                                    }
                                    if (pcsInfo.DefectInfo[k].DefectVrsGerberImages != null)
                                    {
                                        imagePaths_Gerber.Add($"{result}/{pcsInfo.DefectInfo[k].DefectVrsGerberImages[0].ToString()}:{info[i].IP}");
                                    }
                                    if (pcsInfo.DefectInfo[k].DefectVrsOkImages != null)
                                    {
                                        imagePaths_Template.Add($"{result}/{pcsInfo.DefectInfo[k].DefectVrsOkImages[0].ToString()}:{info[i].IP}");
                                    }
                                    index++;
                                }
                            }
                        }
                    }
                    totalPages = (int)Math.Ceiling((double)index / 5);
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
        private async void ShowImage()
        {
            try
            {
                Index = (currentPage - 1) * 5;
                FrHome.Instance.InitTableStyle(FrHome.Instance.table_Small, 5, disInfosList);
                FrHome.Instance.InitWork();
                //根据页索引获取图像源
                // List<string> paths = Machine.ShowFlag == "A" ? imagePaths : Machine.ShowFlag == "B" ? imagePaths_Gerber : imagePaths_Template; 
                List<string> defect_paths = imagePaths;
                List<string> gerberOrtemp_paths = Machine.ShowFlag == "B" ? imagePaths_Gerber : imagePaths_Template;
                var defect_pagedData = defect_paths.Skip((currentPage - 1) * 5).Take(5).ToList(); //imagePaths.Skip((currentPage - 1) * 30).Take(30).ToList();
                var gerberOrtemp_pagedData = gerberOrtemp_paths.Skip((currentPage - 1) * 5).Take(5).ToList(); //imagePaths.Skip((currentPage - 1) * 30).Take(30).ToList();
                var defect_indexPaths = defect_pagedData.Select((path, index1) => new { Path = path, Index = index1 }).ToList();
                var gerberOrtemp_indexPaths = gerberOrtemp_pagedData.Select((path, index1) => new { Path = path, Index = index1 }).ToList();

                //配置并行操作
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount - 1,
                    CancellationToken = CancellationToken.None
                };
                List<string> res_lbl = null;
                PcsResult pcsResult;
                if (dic_Results.TryGetValue(str_SN, out res_lbl) && dic_PcsResult.TryGetValue(str_SN, out pcsResult))
                {
                    await Task.Factory.StartNew(() =>
                    {
                        Parallel.ForEach(defect_indexPaths, parallelOptions, item =>
                        {
                            // 在访问前添加检查
                            int index1 = (currentPage - 1) * 5 + item.Index;
                            string labelText = "未处理";
                            if (res_lbl != null && index1 >= 0 && index1 < res_lbl.Count) // 如果是数组
                            {
                                labelText = res_lbl[index1] ?? "未处理";
                            }
                            else if (res_lbl is IList<string> list && index1 >= 0 && index1 < list.Count)
                            {
                                labelText = list[index1] ?? "未处理";
                            }

                            int index2 = (currentPage - 1) * 5 + item.Index;
                            VBRcvInfp vbValue = null;
                            if (pcsResult?.vb_List != null && index2 >= 0 && index2 < pcsResult.vb_List.Count)
                            {
                                vbValue = pcsResult.vb_List[index2];
                            }
                            Machine.master.workClass.showImage(item.Path, (item.Index + 1) * 2 - 2, labelText, vbValue);
                        });
                    });
                    await Task.Factory.StartNew(() =>
                    {
                        Parallel.ForEach(gerberOrtemp_indexPaths, parallelOptions, item =>
                        {
                            // 在访问前添加检查
                            int index1 = (currentPage - 1) * 5 + item.Index;
                            string labelText = "未处理";
                            if (res_lbl != null && index1 >= 0 && index1 < res_lbl.Count) // 如果是数组
                            {
                                labelText = res_lbl[index1] ?? "未处理";
                            }
                            else if (res_lbl is IList<string> list && index1 >= 0 && index1 < list.Count)
                            {
                                labelText = list[index1] ?? "未处理";
                            }

                            int index2 = (currentPage - 1) * 5 + item.Index;
                            VBRcvInfp vbValue = null;
                            if (pcsResult?.vb_List != null && index2 >= 0 && index2 < pcsResult.vb_List.Count)
                            {
                                vbValue = pcsResult.vb_List[index2];
                            }
                            Machine.master.workClass.showImage(item.Path, (item.Index + 1) * 2 - 1, labelText, vbValue);
                        });
                    });
                }
                else
                {
                    await Task.Factory.StartNew(() =>
                    {
                        Parallel.ForEach(defect_indexPaths, parallelOptions, item =>
                        {
                            //Machine.master.workClass.showImage(item.Path, item.Index*2-1);
                            Machine.master.workClass.showImage(item.Path, (item.Index + 1) * 2 - 2, "未处理");
                        });
                    });
                    await Task.Factory.StartNew(() =>
                    {
                        Parallel.ForEach(gerberOrtemp_indexPaths, parallelOptions, item =>
                        {
                            Machine.master.workClass.showImage(item.Path, (item.Index + 1) * 2 - 1, "未处理");
                        });
                    });
                }




                if (currentPage == totalPages)
                {
                    await Task.Factory.StartNew(() =>
                    {
                        Parallel.For(defect_pagedData.Count, 5, item =>
                        {
                            Machine.master.workClass.showImage("", (item + 1) * 2 - 2);
                        });
                    });
                    await Task.Factory.StartNew(() =>
                    {
                        Parallel.For(gerberOrtemp_pagedData.Count, 5, item =>
                        {
                            Machine.master.workClass.showImage("", (item + 1) * 2 - 1);
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
        private void UpdatePagingControls()
        {
            try
            {
                lblPageInfo.Text = $"第 {currentPage} 页 / 共 {totalPages} 页";
                btnPrevious.Enabled = (currentPage > 1);
                btnNext.Enabled = (currentPage < totalPages);
            }
            catch (Exception ex)
            {
                LogTextHelper.Info("更新上下页异常" + ex.ToString());
            }
        }
        public void UpdateAviCtrInfo(string aviName, string productSerial, string lotId, double utilization)
        {
            aviCtr2Container.UpdateAviCtrInfo(aviName, productSerial, lotId, utilization);
        }

        public void UpdateAviCtrStats(string aviName, int totalImages, int aiOkImages)
        {
            aviCtr2Container.UpdateAviCtrStats(aviName, totalImages, aiOkImages);
        }
        private void UpdateLotSn()
        {
            aviCtr2Container.UpdateAllAviCtrLotSn(Machine.master.workClass.GetLatestPanelInfoByMachineId);
        }
        
    }
}


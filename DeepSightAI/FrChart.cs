using DeepSightDB;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using DeepSightModel;
using Aspose.Cells;
using DeepSightDisplay;
using OpenCvSharp;
using System.Drawing;
using System.Text.RegularExpressions;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using HalconDotNet;

namespace DeepSightAI
{
    public partial class FrChart : Form
    {

        private int currentPageOfChat = 1; // 当前页码
        private int pageSizeOfChat = 10;   // 每页显示行数
        private int totalPagesOfChat = 0;  // 总页数
        private List<Data> allData;     // 本地数据集合

        //0626增加图片查询显示功能
        public CvDisplay[] DispWin1 = null;
        public CvDisplay[] DispWinHeatMap = null;

        //是否启用对比标志位 false:不对比，true:对比
        private bool isContrast = false;

        //缺陷图
        private List<string> ImagePaths = new List<string>();
        //gerber图
        private List<string> ImagePaths_Gerber = new List<string>();
        //template图
        private List<string> ImagePaths_Template = new List<string>();
        //每次查询时存储的缺陷显示信息
        private List<DisPlayInfo> DisInfosList = new List<DisPlayInfo>();

        //缺陷图临时
        private List<string> imagePaths = new List<string>();
        //gerber图临时
        private List<string> imagePaths_Gerber = new List<string>();
        //template图临时
        private List<string> imagePaths_Template = new List<string>();
        //每次查询时存储的缺陷显示信息
        private List<DisPlayInfo> disInfosList = new List<DisPlayInfo>();

        public int Index = 0;//缺陷小图索引
        private int totalPagesOfImage = 0; // 总页数
        private int currentPageOfImage = 1;// 当前页码

        //缺陷名集合
        private List<string> defectTypeList = new List<string>();
        //点击选择的图片路径集合
        private List<string> selectPathList = new List<string>();
        //图片标记记录 
        private Dictionary<string, bool> mark_List = new Dictionary<string, bool>();
        //缺陷显示集合
        public Dictionary<string, PcsResult> dic_PcsResult = new Dictionary<string, PcsResult>();
        public Dictionary<string, List<string>> dic_Results = new Dictionary<string, List<string>>();
        //热力图
        private HeatMapControl heatMapControl;
        private Random random = new Random();

        private Dictionary<string, List<AVI_HeatPoints>> dic_heatPints = new Dictionary<string, List<AVI_HeatPoints>>();
        public FrChart()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            this.dataGridViewData.AutoGenerateColumns = false;
            this.Load += FrChart_Load;
            dataSN.KeyDown += DataGridView1_KeyDown;
        }
        /// <summary>
        /// 窗体对象实例
        /// </summary>
        private static FrChart _instance;

        public static FrChart Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrChart();
                }

                return _instance;
            }
        }

        private void FrChart_Load(object sender, EventArgs e)
        {
            try
            {
                dateTimePickerStart.Value = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
                dateTimePickerEnd.Value = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
                dateTimeStart.Value = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
                dateTimeEnd.Value = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
                InitMethod();
                cmb_byWhat.SelectedIndex = 0;
                cmb_dataByWhat.SelectedIndex = 0;
                //苹果视察 隐藏未实现的功能
                tabControl1.TabPages.Remove(tabPage6);
                //tabControl1.TabPages.Remove(tabPage7);
                tabControl1.TabPages.Remove(tabPage8);

                //cmb_aoi.SelectedIndex = 0;

                //LoadGridColumn();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
            }
        }
        /// <summary>
        /// 窗体初始化
        /// </summary>
        internal void InitMethod()
        {
            try
            {
                DispWin1 = new CvDisplay[30];
                DispWinHeatMap = new CvDisplay[1];
                //布局
                table_Small.Controls.Clear();
                table_Small.RowStyles.Clear();
                table_Small.ColumnStyles.Clear();

                table_Small.ColumnCount = 6;
                table_Small.RowCount = 5;
                int index = 0;
                for (int i = 0; i < table_Small.RowCount; i++)
                {
                    table_Small.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (100F / 1)));

                    for (int j = 0; j < table_Small.ColumnCount; j++)
                    {
                        table_Small.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F / 20));

                        DispWin1[index] = new CvDisplay
                        {
                            Margin = new System.Windows.Forms.Padding(1),
                            BackColor = ColorTranslator.FromHtml("#374c50"),//System.Drawing.SystemColors.InactiveCaption,
                            Dock = System.Windows.Forms.DockStyle.Fill,
                            Name = "Display" + index,
                            AutoDisplay = CvDisplay.AutoDisplayMode.Fit,
                        };

                        DispWin1[index].stationIndex = index + 1;
                        DispWin1[index].OnCallBackFullShowPro -= FrHome_OnCallBackFullShowPro;
                        DispWin1[index].OnCallBackFullShowPro += FrHome_OnCallBackFullShowPro;
                        DispWin1[index].OnCallBackClickOpreation -= FrHome_OnCallBackClickOpreation;
                        DispWin1[index].OnCallBackClickOpreation += FrHome_OnCallBackClickOpreation;
                        //DispWin1[index].OnCallBackRoiIndex -= FrHome_OnCallBackRoiIndex;
                        //DispWin1[index].OnCallBackRoiIndex += FrHome_OnCallBackRoiIndex;
                        //0604增加单图测试
                        //DispWin1[index].OnCallBackSingleTest -= FrHome_OnCallBackSingleTest;
                        //DispWin1[index].OnCallBackSingleTest += FrHome_OnCallBackSingleTest;
                        table_Small.Controls.Add(DispWin1[index], j, i);
                        index++;
                    }
                }
                index = 0;
                InitWork();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }



        private void FrHome_OnCallBackFullShowPro(string station, int index, string m_station, string status, string ocr, Mat mat)
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
            catch (Exception)
            {

                throw;
            }

        }
        /// <summary>
        /// 选中或取消
        /// </summary>
        /// <param name="index"></param>
        /// <param name="opreation">true:选中 false：取消选中</param>
        private void FrHome_OnCallBackClickOpreation(int index, bool opreation)
        {
            try
            {
                if (imagePaths.Count > 0)
                {
                    if (((currentPageOfImage - 1) * 30) + index > imagePaths.Count)
                    {
                        MessageBox.Show("当前索引数已经超过路径总数,请检查！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else
                    {
                        if (opreation)
                        {
                            LogTextHelper.Info("index::::" + index);
                            if (!selectPathList.Contains(imagePaths[((currentPageOfImage - 1) * 30) + index - 1]))
                            {
                                selectPathList.Add(imagePaths[((currentPageOfImage - 1) * 30) + index - 1]);
                                if (mark_List.ContainsKey((((currentPageOfImage - 1) * 30) + index - 1).ToString()))
                                {
                                    mark_List[(((currentPageOfImage - 1) * 30) + index - 1).ToString()] = true;
                                }
                            }
                            //要将报点的一组图全部导出

                            if (((currentPageOfImage - 1) * 30) + index - 1 >= 0 && ((currentPageOfImage - 1) * 30) + index - 1 < imagePaths_Gerber.Count)
                            {
                                if (!selectPathList.Contains(imagePaths_Gerber[((currentPageOfImage - 1) * 30) + index - 1]))
                                {
                                    selectPathList.Add(imagePaths_Gerber[((currentPageOfImage - 1) * 30) + index - 1]);
                                }
                            }
                            if (((currentPageOfImage - 1) * 30) + index - 1 >= 0 && ((currentPageOfImage - 1) * 30) + index - 1 < imagePaths_Template.Count)
                            {
                                if (!selectPathList.Contains(imagePaths_Template[((currentPageOfImage - 1) * 30) + index - 1]))
                                {
                                    selectPathList.Add(imagePaths_Template[((currentPageOfImage - 1) * 30) + index - 1]);
                                }
                            }

                        }
                        else
                        {
                            if (selectPathList.Contains(imagePaths[((currentPageOfImage - 1) * 30) + index - 1]))
                            {
                                selectPathList.Remove(imagePaths[((currentPageOfImage - 1) * 30) + index - 1]);
                                if (mark_List.ContainsKey((((currentPageOfImage - 1) * 30) + index - 1).ToString()))
                                {
                                    mark_List[(((currentPageOfImage - 1) * 30) + index - 1).ToString()] = false;
                                }
                            }
                            //要将报点的一组图全部导出
                            if (((currentPageOfImage - 1) * 30) + index - 1 >= 0 && ((currentPageOfImage - 1) * 30) + index - 1 < imagePaths_Gerber.Count)
                            {
                                if (!selectPathList.Contains(imagePaths_Gerber[((currentPageOfImage - 1) * 30) + index - 1]))
                                {
                                    selectPathList.Remove(imagePaths_Gerber[((currentPageOfImage - 1) * 30) + index - 1]);
                                }
                            }
                            if (((currentPageOfImage - 1) * 30) + index - 1 >= 0 && ((currentPageOfImage - 1) * 30) + index - 1 < imagePaths_Template.Count)
                            {
                                if (!selectPathList.Contains(imagePaths_Template[((currentPageOfImage - 1) * 30) + index - 1]))
                                {
                                    selectPathList.Remove(imagePaths_Template[((currentPageOfImage - 1) * 30) + index - 1]);
                                }
                            }
                            //if (selectPathList.Contains(imagePaths_Gerber[((currentPageOfImage - 1) * 30) + index - 1]))
                            //{
                            //    selectPathList.Remove(imagePaths_Gerber[((currentPageOfImage - 1) * 30) + index - 1]);
                            //}
                            //if (selectPathList.Contains(imagePaths_Template[((currentPageOfImage - 1) * 30) + index - 1]))
                            //{
                            //    selectPathList.Remove(imagePaths_Template[((currentPageOfImage - 1) * 30) + index - 1]);
                            //}
                        }
                        this.lbl_selectCount.Text = (selectPathList.Count() / 3).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("选中回调失败:" + ex.ToString());
            }
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonQuery_Click(object sender, EventArgs e)
        {

            allData = new List<Data>();
            currentPageOfChat = 1;
            //工单
            if (cmb_dataByWhat.SelectedIndex == 0)
            {
                string LotName = this.txt_dataCode.Text;// Regex.Match(this.txt_dataCode.Text, @"[^_]+$").Value;
                RootDbInfo lotInfo = new RootDbInfo();
                lotInfo.uniqueKey = Guid.NewGuid().ToString();
                lotInfo.db_name = "panel_list";
                lotInfo.operation = "get";
                lotInfo.op_mode = "all";
                lotInfo.key = LotName;
                string res = "";
                Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, lotInfo, 0, out res);
                var jsonStr = JObject.Parse(res);
                string str = jsonStr["value"].ToString();
                if (jsonStr["msg"].ToString().Contains("err") || string.IsNullOrEmpty(jsonStr["value"].ToString()))
                {
                    MessageBox.Show($"此Lot【{this.txt_dataCode.Text}】未查询到绑定的SN");
                    return;
                }
                var root = JObject.Parse(res);
                string[] strSN = root["value"].ToString().Split(';');
                foreach (var item in strSN)
                {
                    if (!GetSNInfoMethod(item))
                    {
                        continue;
                    }
                }
            }
            //料号
            else if (cmb_dataByWhat.SelectedIndex == 1)
            {
                string liaohaoName = this.txt_dataCode.Text;
                RootDbInfo liaohaoInfo = new RootDbInfo();
                liaohaoInfo.uniqueKey = Guid.NewGuid().ToString();
                liaohaoInfo.db_name = "panel_list";
                liaohaoInfo.operation = "get";
                liaohaoInfo.op_mode = "all";
                liaohaoInfo.key = liaohaoName;
                string res = "";
                Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, liaohaoInfo, 0, out res);
                var jsonStr = JObject.Parse(res);
                string str = jsonStr["value"].ToString();
                if (jsonStr["msg"].ToString().Contains("err") || string.IsNullOrEmpty(jsonStr["value"].ToString()))
                {
                    MessageBox.Show($"此Lot【{this.txt_dataCode.Text}】未查询到绑定的SN");
                    return;
                }
                var root = JObject.Parse(res);
                string[] strLOT = root["value"].ToString().Split(';');
                foreach (var item in strLOT)
                {
                    if (!GetSNInfoMethod(item))
                    {
                        continue;
                    }
                }
            }
            else
            {
                RootDbInfo getInfo = new RootDbInfo();
                getInfo.uniqueKey = Guid.NewGuid().ToString();
                getInfo.db_name = "ai_merged_results";
                getInfo.operation = "get";
                getInfo.is_select_range = "true";
                getInfo.op_mode = "all";
                getInfo.range_start = dateTimePickerStart.Value.ToString("yyyyMMddHHmmssfff");

                getInfo.range_end = dateTimePickerEnd.Value.ToString("yyyyMMddHHmmssfff");
                string Result = "";
                //Machine.master.workClass.URL = "http://127.0.0.1:9877";
                Machine.master.workClass.http_DB.HttpPostMethod2(Machine.master.workClass.URL, getInfo, 0, out Result);
                if (Result == "")
                {
                    MessageBox.Show("未查询到结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var root = JObject.Parse(Result);
                int num = root["data_list"].Count();//num等于1相当于没有数据
                if (num == 1)
                {
                    return;
                }
                foreach (var item in root["data_list"])
                {
                    Data data = new Data();
                    string itemStr = item.ToString();
                    if (itemStr == "end_range_send") continue;
                    var itemObj = JObject.Parse(itemStr);
                    string key = itemObj["key"].ToString();
                    string valueStr = itemObj["value"].ToString();
                    var valueObj = JObject.Parse(valueStr);
                    string serialNumber = valueObj["serial_number"].ToString();
                    data.CreateTime = key;
                    data.Code = serialNumber;
                    //总PCS数
                    int baodian = 0;
                    int pcs = 0;
                    foreach (var info1 in valueObj["results_info"])
                    {
                        string side = info1["side"].ToString();
                        string minio_ip = info1["minio_ip"].ToString();
                        string minio_port = info1["minio_port"].ToString();
                        if (string.IsNullOrEmpty(minio_ip) || string.IsNullOrEmpty(minio_port) || minio_port == "0")
                        {
                            continue;
                        }
                        string result_path = info1["result_path"].ToString();
                        string path = string.Empty;
                        string result = string.Empty;
                        Machine.master.workClass.ParseMinioPath(result_path, out path, out result);
                        Machine.master.workClass.minio.BuildClient(minio_ip, minio_port);
                        string json = Machine.master.workClass.minio.ReadJsonSync("deepiresults", path, minio_ip);
                        var panelJson = JsonConvert.DeserializeObject<RootPanelInfo>(json);
                        pcs = panelJson.PcsInfo.Count();
                        PcsInfo pcsInfo = null;
                        for (int i = 0; i < pcs; i++)
                        {
                            if (panelJson.PcsInfo.TryGetValue((i + 1).ToString(), out pcsInfo))
                            {
                                for (int j = 0; j < pcsInfo.DefectInfo.Count; j++)
                                {
                                    baodian++;
                                }
                            }
                        }
                    }
                    data.TotalCount = pcs.ToString();
                    data.BD_Count = baodian.ToString();

                    //这里要根据信息去AI处理表里面查询AI_OK AI_NG
                    //获取这个KEY的降报点数
                    RootDbInfo Info = new RootDbInfo();
                    Info.uniqueKey = Guid.NewGuid().ToString();
                    Info.db_name = "filter_time_to_airesults";
                    Info.operation = "get";
                    Info.op_mode = "all";
                    Info.key = key;
                    //Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, Info, 0, out Result);
                    //Machine.master.workClass.http_DB.HttpPosmtMethod(Machine.master.workClass.URL, Info, 0, out Result);
                    var jsonStr = JObject.Parse(Result);
                    string str = jsonStr["value"].ToString();
                    if (jsonStr["msg"].ToString().Contains("err"))
                    {
                        continue;
                    }
                    string[] jsonParts = str.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    int ngcount = 0;
                    for (int i = 0; i < jsonParts.Count(); i++)
                    {
                        List<ResultInfo> ai_result = JsonConvert.DeserializeObject<List<ResultInfo>>(jsonParts[i]);
                        foreach (var obj in ai_result)
                        {
                            ngcount += Convert.ToInt32(obj.ResultInfos.Split('_').ToList().Last());
                        }
                    }
                    data.OK_Count = (baodian - ngcount).ToString();
                    data.NG_Count = ngcount.ToString();
                    allData.Add(data);
                }
            }
            LoadCurrentPage();
        }
        private bool GetSNInfoMethod(string sn)
        {
            string res = "";
            Data data = new Data();
            //SN
            data.Code = sn;
            //总PCS数
            int baodian = 0;
            int pcs = 0;
            string keyid = GetKeyBySN(sn);
            //根据ID获取信息
            RootDbInfo getInfo = new RootDbInfo();
            getInfo.uniqueKey = Guid.NewGuid().ToString();
            getInfo.db_name = "ai_merged_results";
            getInfo.operation = "get";
            getInfo.key = keyid;
            getInfo.is_select_range = "false";
            getInfo.op_mode = "all";
            string Result = "";
            Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, getInfo, 0, out Result);
            //解析minioIp minioPort
            var root = JObject.Parse(Result);
            string valueStr = root["value"].ToString();
            if (valueStr == "")
            {
                return false;
            }
            string key = root["key"].ToString();
            var valueObj = JObject.Parse(valueStr);
            string serialNumber = valueObj["serial_number"].ToString();
            // 遍历 result_infos  A/B面
            foreach (var info1 in valueObj["results_info"])
            {
                string minio_ip = info1["minio_ip"].ToString();
                string side = info1["side"].ToString();
                string minio_port = info1["minio_port"].ToString();
                string result_path = info1["result_path"].ToString();

                string path = string.Empty;
                string result = string.Empty;
                Machine.master.workClass.ParseMinioPath(result_path, out path, out result);
                Machine.master.workClass.minio.BuildClient(minio_ip, "9102");
                string json = Machine.master.workClass.minio.ReadJsonSync("deepiresults", path, minio_ip);
                var panelJson = JsonConvert.DeserializeObject<RootPanelInfo>(json);
                pcs = panelJson.PcsInfo.Count();
                data.CreateTime = panelJson.EndTime;
                PcsInfo pcsInfo = null;
                for (int j = 0; j < pcs; j++)
                {
                    if (panelJson.PcsInfo.TryGetValue((j + 1).ToString(), out pcsInfo))
                    {
                        for (int k = 0; k < pcsInfo.DefectInfo.Count; k++)
                        {
                            baodian++;
                        }
                    }
                }
            }
            data.TotalCount = pcs.ToString();
            data.BD_Count = baodian.ToString();
            //这里要根据信息去AI处理表里面查询AI_OK AI_NG
            //获取这个KEY的降报点数
            RootDbInfo resultInfo = new RootDbInfo();
            resultInfo.uniqueKey = Guid.NewGuid().ToString();
            resultInfo.db_name = "filter_time_to_airesults";
            resultInfo.operation = "get";
            resultInfo.op_mode = "all";
            resultInfo.key = sn;
            Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, resultInfo, 0, out res);
            var jsonRes = JObject.Parse(res);
            string str = jsonRes["value"].ToString();
            if (jsonRes["msg"].ToString().Contains("err"))
            {
                MessageBox.Show($"此sn：{sn}未查询到推理结果");
                return false;
            }
            string[] jsonParts = str.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            int ngcount = 0;
            for (int i = 0; i < jsonParts.Count(); i++)
            {
                List<ResultInfo> ai_result = JsonConvert.DeserializeObject<List<ResultInfo>>(jsonParts[i]);
                foreach (var obj in ai_result)
                {
                    ngcount += Convert.ToInt32(obj.ResultInfos.Split('_').ToList().Last());
                }
            }
            data.OK_Count = (baodian - ngcount).ToString();
            data.NG_Count = ngcount.ToString();
            allData.Add(data);
            return true;
        }
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Export_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "文件|*.xls",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string localFilePath = saveFileDialog.FileName.ToString();
                    //DataTable dt = GetDgvToTable(dataGridViewData);
                    //if (dt != null && dt.Rows.Count > 0)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            btn_Export.Enabled = false;
                            dataGridViewData.Enabled = false;

                            //DataGridView newView = dataGridViewData;
                            //newView.DataSource = allData.ToList();
                            // if (ExportExcelWithAspose(dataGridViewData, localFilePath))//导出
                            if (ExportExcelWithAsposeAndAllData(dataGridViewData, allData, localFilePath))//导出
                            {
                                //导出成功
                                if (MessageBox.Show("导出数据完成，是否打开excel文件？", "导出提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    System.Diagnostics.Process.Start(localFilePath);
                                }
                            }
                            else
                            {
                                //导出失败
                                LogTextHelper.Warn("导出数据失败");
                            }

                            btn_Export.Enabled = true;
                            dataGridViewData.Enabled = true;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("导出异常" + ex.ToString());
                throw;
            }
        }

        public List<string> defectNames = new List<string>();

        private void FrChart_Shown(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogTextHelper.Error("导出异常" + ex.ToString());
                throw;
            }
        }

        private Mutex addMutex = new Mutex();
        private bool flag = false;
        private DataTable dt2 = new DataTable();
        /// <summary>
        /// Cell点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void dataGridViewData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && dataGridViewData.Columns[e.ColumnIndex].Name == "btnColumn")
                {
                    string id = dataGridViewData.Rows[e.RowIndex].Cells["CreateTime"].Value.ToString();

                    //根据ID获取信息
                    RootDbInfo getInfo = new RootDbInfo();
                    getInfo.uniqueKey = Guid.NewGuid().ToString();
                    getInfo.db_name = "ai_merged_results";
                    getInfo.operation = "get";
                    getInfo.key = id;
                    getInfo.is_select_range = "false";
                    getInfo.op_mode = "all";
                    string Result = "";
                    Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, getInfo, 0, out Result);
                    //解析minioIp minioPort
                    var root = JObject.Parse(Result);
                    string valueStr = root["value"].ToString();
                    string key = root["key"].ToString();
                    var valueObj = JObject.Parse(valueStr);
                    string serialNumber = valueObj["serial_number"].ToString();

                    // 遍历 result_infos
                    foreach (var info1 in valueObj["results_info"])
                    {
                        string minio_ip = info1["minio_ip"].ToString();
                        string side = info1["side"].ToString();
                        string minio_port = info1["minio_port"].ToString();
                        string result_path = info1["result_path"].ToString();
                        Machine.master.workClass.minio.BuildClient(minio_ip, minio_port);

                        string path = string.Empty;
                        string result = string.Empty;
                        Machine.master.workClass.ParseMinioPath(result_path, out path, out result);

                        await Task.Factory.StartNew(() =>
                        {
                            Machine.master.workClass.minio.Download("deepiresults", $"{result}/", $"E:/minio/MinioDownLoad/{result}", minio_ip);
                        });
                    }
                    MessageBox.Show($"文件下载成功！", "下载提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("下载异常" + ex.ToString());
                throw;
            }
        }


        private void btnPrevious_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentPageOfChat > 1)
                {
                    currentPageOfChat--;
                    LoadCurrentPage();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentPageOfChat < totalPagesOfChat)
                {
                    currentPageOfChat++;
                    LoadCurrentPage();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        // 加载当前页数据
        private void LoadCurrentPage()
        {
            try
            {
                if (allData == null || allData.Count == 0)
                {
                    dataGridViewData.DataSource = null;
                    lblPageInfo.Text = "暂无数据";
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    return;
                }
                totalPagesOfChat = (int)Math.Ceiling((double)allData.Count / pageSizeOfChat);
                var pagedData = allData
                    .Skip((currentPageOfChat - 1) * pageSizeOfChat)
                    .Take(pageSizeOfChat)
                    .ToList();
                dataGridViewData.DataSource = pagedData;
                UpdatePagingControls();
            }
            catch (Exception)
            {

                throw;
            }
        }

        // 更新分页控件
        private void UpdatePagingControls()
        {
            try
            {
                lblPageInfo.Text = $"第 {currentPageOfChat} 页 / 共 {totalPagesOfChat} 页";
                btnPrevious.Enabled = (currentPageOfChat > 1);
                btnNext.Enabled = (currentPageOfChat < totalPagesOfChat);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private int CalculatePageSize()
        {
            try
            {
                int availableHeight = dataGridViewData.ClientSize.Height - dataGridViewData.ColumnHeadersHeight;
                if (dataGridViewData.ScrollBars.HasFlag(ScrollBars.Horizontal))
                {
                    availableHeight -= SystemInformation.HorizontalScrollBarHeight;
                }
                int rowHeight = dataGridViewData.Rows.Count > 0
                                ? dataGridViewData.Rows[0].Height
                                : dataGridViewData.RowTemplate.Height;
                int pageSize = availableHeight / rowHeight;
                return Math.Max(pageSize, 1);
            }
            catch (Exception)
            {
                return -1;
                throw;
            }

        }

        private void AdjustPageSizeAndReload()
        {
            try
            {
                int firstRowIndex = (currentPageOfChat - 1) * pageSizeOfChat;
                pageSizeOfChat = CalculatePageSize();
                currentPageOfChat = (int)Math.Ceiling((double)(firstRowIndex + 1) / pageSizeOfChat);
                LoadCurrentPage();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void FrChart_Resize(object sender, EventArgs e)
        {
            AdjustPageSizeAndReload();
        }

        private void dataGridViewData_Resize(object sender, EventArgs e)
        {
            AdjustPageSizeAndReload();
        }
        private bool ExportExcelWithAspose(DataGridView gridViewData, string filepath)
        {
            try
            {
                if (gridViewData == null || gridViewData.Rows.Count == 0)
                {
                    MessageBox.Show("数据为空");
                    return false;
                }
                //缺陷汇总  总表
                Workbook book = new Workbook(); //创建工作簿
                Worksheet sheet = book.Worksheets[0]; //创建工作表
                sheet.Name = "报点汇总";
                Cells cells = sheet.Cells; //单元格

                //创建样式
                Aspose.Cells.Style style = book.Styles[book.Styles.Add()];
                style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                style.HorizontalAlignment = TextAlignmentType.Center; //单元格内容的水平对齐方式文字居中
                style.Font.Name = "宋体"; //字体

                //style1.Font.IsBold = true; //设置粗体
                style.Font.Size = 11; //设置字体大小

                //style.ForegroundColor = System.Drawing.Color.FromArgb(153, 204, 0); //背景色

                //style.Pattern = Aspose.Cells.BackgroundType.Solid;

                int Colnum = gridViewData.Columns.Count;//表格列数
                int Rownum = gridViewData.Rows.Count;//表格行数 
                //生成行 列名行
                for (int i = 0; i < Colnum; i++)
                {
                    string ColumnName = gridViewData.Columns[i].HeaderText;
                    cells[0, i].PutValue(ColumnName); //添加表头
                    cells[0, i].SetStyle(style); //添加样式
                }


                for (int i = 0; i < Rownum; i++)
                {
                    for (int j = 0; j < Colnum; j++)
                    {
                        cells[1 + i, j].PutValue(gridViewData.Rows[i].Cells[j].Value?.ToString()); //添加数据
                        cells[1 + i, j].SetStyle(style); //添加样式
                    }
                }
                sheet.Cells.DeleteColumn(0);
                sheet.AutoFitColumns(); //自适应宽
                book.Save(filepath);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
                return false;
            }
            return true;
        }
        private bool ExportExcelWithAsposeAndAllData(DataGridView gridViewData, List<Data> listData, string filepath)
        {
            try
            {
                if (gridViewData == null || gridViewData.Rows.Count == 0 || listData == null)
                {
                    MessageBox.Show("数据为空");
                    return false;
                }
                //缺陷汇总  总表
                Workbook book = new Workbook(); //创建工作簿
                Worksheet sheet = book.Worksheets[0]; //创建工作表
                sheet.Name = "报点汇总";
                Cells cells = sheet.Cells; //单元格

                //创建样式
                Aspose.Cells.Style style = book.Styles[book.Styles.Add()];
                style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                style.HorizontalAlignment = TextAlignmentType.Center; //单元格内容的水平对齐方式文字居中
                style.Font.Name = "宋体"; //字体

                //style1.Font.IsBold = true; //设置粗体
                style.Font.Size = 11; //设置字体大小

                //style.ForegroundColor = System.Drawing.Color.FromArgb(153, 204, 0); //背景色

                //style.Pattern = Aspose.Cells.BackgroundType.Solid;

                int Colnum = gridViewData.Columns.Count;//表格列数
                int Rownum = gridViewData.Rows.Count;//表格行数 
                //生成行 列名行
                for (int i = 0; i < Colnum; i++)
                {
                    string ColumnName = gridViewData.Columns[i].HeaderText;
                    cells[0, i].PutValue(ColumnName); //添加表头
                    cells[0, i].SetStyle(style); //添加样式
                }


                for (int i = 0; i < listData.Count; i++)
                {
                    for (int j = 0; j < Colnum; j++)
                    {
                        switch (j)
                        {

                            case 0:
                                cells[1 + i, j].PutValue(listData[i].CreateTime.ToString()); //添加数据
                                cells[1 + i, j].SetStyle(style); //添加样式
                                break;
                            case 1:
                                cells[1 + i, j].PutValue(listData[i].Code.ToString().ToString()); //添加数据
                                cells[1 + i, j].SetStyle(style); //添加样式
                                break;
                            case 2:
                                cells[1 + i, j].PutValue(listData[i].TotalCount.ToString().ToString()); //添加数据
                                cells[1 + i, j].SetStyle(style); //添加样式
                                break;
                            case 3:
                                cells[1 + i, j].PutValue(listData[i].BD_Count.ToString().ToString()); //添加数据
                                cells[1 + i, j].SetStyle(style); //添加样式
                                break;
                            case 4:
                                cells[1 + i, j].PutValue(listData[i].OK_Count.ToString().ToString()); //添加数据
                                cells[1 + i, j].SetStyle(style); //添加样式
                                break;
                            case 5:
                                cells[1 + i, j].PutValue(listData[i].NG_Count.ToString().ToString()); //添加数据
                                cells[1 + i, j].SetStyle(style); //添加样式
                                break;
                            default:
                                cells[1 + i, j].PutValue(""); //添加数据
                                cells[1 + i, j].SetStyle(style); //添加样式
                                break;
                        }

                    }
                }
                //sheet.Cells.DeleteColumn(0);
                sheet.AutoFitColumns(); //自适应宽
                book.Save(filepath);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
                return false;
            }
            return true;
        }
        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                //测试
                //Mat mat = new Mat(@"E:\minio\deepiresults\20250529115740287433\Defect-PFM\2022080802200102188G-10edb528-3c41-11f0-bb91-6cb3115dc0af-A-Defect-PFM-pcs-11-vrs0-0.jpg");//ConvertHImageToMat(himgs[i]);
                //DispWin1[0].Image = mat;

                //根据当前条件查询显示   by工单 sn 时间
                //最后都是加载图片显示   看是显示一组中的几张？
                //①根据工单查询  首先要查到这个工单下的所有sn，再通过遍历所有的

                Clear(1);
                switch (this.cmb_byWhat.SelectedIndex)
                {
                    case 0://工单
                        //查询工单下的sn
                        queryDataByLot(this.txt_code.Text.Trim());
                        break;
                    case 1://sn
                        queryDataBySn(this.txt_code.Text.Trim());
                        break;
                    case 2://时间
                        queryDataByTime();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("查询异常！！！" + ex.ToString());
            }
        }



        public void queryDataByLot(string Lot)
        {
            try
            {
                dataLot.Rows.Clear();
                dataSN.Rows.Clear();
                List<string> sn_list = GetSnListByLot(Lot);

                bool isFirst = false;
                if (sn_list.Count > 0)
                {
                    int rowIndex = dataLot.Rows.Add();
                    dataLot.Rows[rowIndex].Cells["isLotSelect"].Value = true;
                    dataLot.Rows[rowIndex].Cells["lot"].Value = Lot;
                    //目前先加载一个sn的信息
                    //如果加载的sn信息不存在，就默认往后加载
                    for (int i = 0; i < sn_list.Count; i++)
                    {
                        if (!isFirst)
                        {
                            isFirst = true;
                            rowIndex = dataSN.Rows.Add();
                            dataSN.Rows[rowIndex].Cells["isSnSelect"].Value = true;
                            dataSN.Rows[rowIndex].Cells["serialNumber"].Value = sn_list[i];
                        }
                        else
                        {
                            rowIndex = dataSN.Rows.Add();
                            dataSN.Rows[rowIndex].Cells["isSnSelect"].Value = false;
                            dataSN.Rows[rowIndex].Cells["serialNumber"].Value = sn_list[i];
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"此Lot:{Lot}未查询到结果", "查询提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public bool queryDataBySn(string sn)
        {
            try
            {
                Clear();
                bool result = false;
                //返回这个SN所有的图片路径和minio:IP信息
                //filter_time_to_sn  此表储存sn 与 key的关系
                //要先在这个表中查key  再通过key从ai_merged_results中获取信息
                //20250528095631399 A31HGVDDDGGBB
                string key = GetKeyBySN(sn);
                if (!string.IsNullOrEmpty(key))
                {
                    //开始查询minio，并且吧sn所有的报点缺陷路径信息进行存储
                    GetMinioInfoByKey(key);
                    if (imagePaths.Count > 0)
                    {
                        str_SN = sn;
                        ShowImage();
                        result = true;
                    }
                    else
                    {
                        MessageBox.Show($"此SN:{sn}未查询到产品路径信息", "查询提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        result = false;
                    }
                }
                else
                {
                    result = false;
                    MessageBox.Show($"此SN:{sn}未查询到Key", "查询提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return result;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }
        /// <summary>
        /// BySN查询时间戳key
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        private string GetKeyBySN(string sn)
        {
            string key = string.Empty;
            try
            {
                RootDbInfo info = new RootDbInfo();
                info.db_name = "filter_time_to_sn";
                info.key = sn;
                info.op_mode = "all";
                info.uniqueKey = Guid.NewGuid().ToString();
                info.operation = "get";
                string outInfo = null;
                //Machine.master.workClass.URL = "http://192.168.77.243:9877";
                Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, info, 0, out outInfo);
                if (outInfo != null)
                {
                    //解析VALUE拿到信息
                    var json = JObject.Parse(outInfo);
                    key = json["value"].ToString();
                }
            }
            catch (Exception ex)
            {
                key = "";
                LogTextHelper.Error("查询异常" + ex.ToString());
            }
            return key;
        }
        /// <summary>
        /// ByLot号查询SN列表
        /// </summary>
        /// <param name="Lot"></param>
        /// <returns></returns>
        private List<string> GetSnListByLot(string Lot)
        {
            try
            {
                List<string> rtn_list = new List<string>();
                RootDbInfo info = new RootDbInfo();
                info.db_name = "panel_list";
                info.key = Lot;
                info.op_mode = "all";
                info.uniqueKey = Guid.NewGuid().ToString();
                info.operation = "get";
                string outInfo = null;
                Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, info, 0, out outInfo);
                if (outInfo != null)
                {
                    //解析VALUE拿到信息
                    var json = JObject.Parse(outInfo);
                    string res = json["value"].ToString();
                    if (outInfo.Contains("err_key_found") || res == "")
                    {
                        return new List<string>();
                    }
                    //取value里面的value
                    //var val = JObject.Parse(res);
                    //string res1 = val["value"].ToString().TrimEnd(';');
                    rtn_list.AddRange(res.Split(';').ToList());
                }
                return rtn_list;
            }
            catch (Exception)
            {
                return new List<string>();
                throw;
            }

        }
        /// <summary>
        /// 通过Key查询Minio信息
        /// </summary>
        /// <param name="keyid"></param>
        private void GetMinioInfoByKey(string keyid)
        {
            try
            {
                //根据ID获取信息
                RootDbInfo getInfo = new RootDbInfo();
                getInfo.uniqueKey = Guid.NewGuid().ToString();
                getInfo.db_name = "ai_merged_results";
                getInfo.operation = "get";
                getInfo.key = keyid;
                getInfo.is_select_range = "false";
                getInfo.op_mode = "all";
                string Result = "";
                Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, getInfo, 0, out Result);
                //解析minioIp minioPort
                var root = JObject.Parse(Result);
                string valueStr = root["value"].ToString();
                string key = root["key"].ToString();
                var valueObj = JObject.Parse(valueStr);
                string serialNumber = valueObj["serial_number"].ToString();
                // 遍历 result_infos  A/B面
                int index = 0;
                foreach (var info1 in valueObj["results_info"])
                {
                    string minio_ip = info1["minio_ip"].ToString();
                    string side = info1["side"].ToString();
                    string minio_port = info1["minio_port"].ToString();
                    string result_path = info1["result_path"].ToString();

                    string path = string.Empty;
                    string result = string.Empty;
                    Machine.master.workClass.ParseMinioPath(result_path, out path, out result);
                    //20250831 将缺陷信息读出，在查询时进行显示
                    LogTextHelper.Info($"要查询的sn:{serialNumber}");
                    GenSNDefectRoi(serialNumber, result);
                    Machine.master.workClass.minio.BuildClient(minio_ip, minio_port);
                    string json = Machine.master.workClass.minio.ReadJsonSync("deepiresults", path, minio_ip);
                    if (json == "")
                    {
                        MessageBox.Show($"panel.json文件为空，请检查！", "查询提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    var info = JsonConvert.DeserializeObject<RootPanelInfo>(json);

                    for (int i = 0; i < info.PcsInfo.Count; i++)
                    {
                        PcsInfo pcsInfo = null;
                        if (info.PcsInfo.TryGetValue((i + 1).ToString(), out pcsInfo))
                        {
                            for (int j = 0; j < pcsInfo.DefectInfo.Count; j++)
                            {
                                DisInfosList.Add(new DisPlayInfo()
                                {
                                    defect_code = pcsInfo.DefectInfo[j].DefectCode,
                                    defect_index = pcsInfo.DefectInfo[j].DefectIndex.ToString(),
                                    product_serial = info.ProductSerial,
                                    pcs_index = pcsInfo.DefectInfo[j].PcsIndex.ToString(),
                                    defect_location = pcsInfo.DefectInfo[j].DefectLocation,
                                    //sn = SN,//info[i].rootInfo.SerialNumber,
                                    process_time = info.EndTime,
                                    ai_infer_result = pcsInfo.DefectInfo[j].AiInferResult,
                                    station_name = info.StationName,
                                    dateil = "",
                                    side_index = info.SideIndex,
                                    lot_id = info.LotId,
                                    lot_batch = info.LotBatch,
                                });
                                //添加标记状态
                                if (!mark_List.ContainsKey(index.ToString()))
                                {
                                    mark_List.Add(index.ToString(), false);
                                }
                                //添加缺陷类型
                                if (!defectTypeList.Contains(pcsInfo.DefectInfo[j].DefectCode))
                                {
                                    defectTypeList.Add(pcsInfo.DefectInfo[j].DefectCode);
                                }
                                //可能有的项目图不完整
                                if (pcsInfo.DefectInfo[j].DefectVrsImages != null)
                                {
                                    ImagePaths.Add($"{result}/{pcsInfo.DefectInfo[j].DefectVrsImages[0].ToString()}:{minio_ip}");
                                }
                                if (pcsInfo.DefectInfo[j].DefectVrsOkImages != null)
                                {
                                    ImagePaths_Template.Add($"{result}/{pcsInfo.DefectInfo[j].DefectVrsOkImages[0].ToString()}:{minio_ip}");
                                }
                                if (pcsInfo.DefectInfo[j].DefectVrsGerberImages != null)
                                {
                                    ImagePaths_Gerber.Add($"{result}/{pcsInfo.DefectInfo[j].DefectVrsGerberImages[0].ToString()}:{minio_ip}");
                                }
                                index++;
                            }
                        }
                    }
                }
                dataDefectType.Rows.Clear();
                //给临时变量赋值
                imagePaths = ImagePaths;
                imagePaths_Template = ImagePaths_Template;
                imagePaths_Gerber = ImagePaths_Gerber;
                disInfosList = DisInfosList;

                for (int i = 0; i < defectTypeList.Count(); i++)
                {
                    int rowIndex = dataDefectType.Rows.Add();
                    dataDefectType.Rows[rowIndex].Cells["isDefectSelect"].Value = false;
                    dataDefectType.Rows[rowIndex].Cells["defectType"].Value = defectTypeList[i];
                }
                totalPagesOfImage = (int)Math.Ceiling((double)index / 30);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void queryDataByTime()
        {
            try
            {
                //返回所有的Lot还是返回所有的sn？？？？？？？？？？？？？？
                //如果是所有的sn  则先查key,再通过key查询sn，
                RootDbInfo getInfo = new RootDbInfo();
                getInfo.uniqueKey = Guid.NewGuid().ToString();
                getInfo.db_name = "ai_merged_results";
                getInfo.operation = "get";
                getInfo.is_select_range = "true";
                getInfo.op_mode = "all";
                getInfo.range_start = dateTimeStart.Value.ToString("yyyyMMddHHmmssfff");
                getInfo.range_end = dateTimeEnd.Value.ToString("yyyyMMddHHmmssfff");

                string Result = "";
                //Machine.master.workClass.URL = "http://192.168.77.243:9877";
                Machine.master.workClass.http_DB.HttpPostMethod(Machine.master.workClass.URL, getInfo, 0, out Result);

                //遍历解析sn
                if (Result == "")
                {
                    MessageBox.Show("此时间段未查询到数据！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var root = JObject.Parse(Result);
                int num = root["data_list"].Count();//num等于1相当于没有数据
                if (num == 1)
                {
                    MessageBox.Show("此时间段未查询到数据！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                bool isFirst = false;
                foreach (var item in root["data_list"])
                {
                    //作业时间 二维码 总PCS数 报点数 AI_OK AI_NG 查看详情
                    //string[] obj = new string[] { "", "", "", "", "", "", ""};
                    Data data = new Data();
                    string itemStr = item.ToString();
                    // 跳过"end_range_send"
                    if (itemStr == "end_range_send") continue;
                    // 解析data_list
                    var itemObj = JObject.Parse(itemStr);
                    string key = itemObj["key"].ToString();

                    string valueStr = itemObj["value"].ToString();
                    LogTextHelper.Info("value值：" + valueStr);
                    var valueObj = JObject.Parse(valueStr);
                    //var valueObj = JObject.Parse(valueStr.Trim().TrimEnd(';'));

                    string serialNumber = valueObj["serial_number"].ToString();

                    int rowIndex = 0;
                    if (!isFirst)
                    {
                        isFirst = true;
                        rowIndex = dataSN.Rows.Add();
                        dataSN.Rows[rowIndex].Cells["isSnSelect"].Value = true;
                        dataSN.Rows[rowIndex].Cells["serialNumber"].Value = serialNumber;
                    }
                    else
                    {
                        rowIndex = dataSN.Rows.Add();
                        dataSN.Rows[rowIndex].Cells["isSnSelect"].Value = false;
                        dataSN.Rows[rowIndex].Cells["serialNumber"].Value = serialNumber;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void GenSNDefectRoi(string sn, string path)
        {
            try
            {
                var msg = File.ReadAllText($"D:/minio/deepiresults/{path}/{sn}-vb.json");
                var obj = JsonConvert.DeserializeObject<RootVBOutInfo>(msg);
                PcsResult pcsResult = new PcsResult();
                List<string> resList = new List<string>();
                string code = obj.Code.ToString();
                if (code != "200")
                {
                    return;
                }
                JObject root = JObject.Parse(msg);
                if (code == "200")
                {
                    pcsResult.vb_List = new List<VBRcvInfp>();
                    //这个有几个  就是几个报点图各自的结果，
                    for (int i = 0; i < obj.Data.InferWholeData.InferResults.Count; i++)
                    {
                        VBRcvInfp vBRcv = new VBRcvInfp();
                        vBRcv.bbox = new List<List<double>>();
                        for (int j = 0; j < obj.Data.InferWholeData.InferResults[i].inferDetails.Location.Count; j++)
                        {
                            vBRcv.raw_bbox = new List<double>();
                            vBRcv.raw_bbox.Add(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].X);
                            vBRcv.raw_bbox.Add(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Y);
                            vBRcv.raw_bbox.Add(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Width);
                            vBRcv.raw_bbox.Add(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Height);
                            vBRcv.bbox.Add(vBRcv.raw_bbox);
                        }
                        resList.Add(obj.Data.InferWholeData.InferResults[i].Infer_Result == "NG" ? "1" : "0");
                        pcsResult.vb_List.Add(vBRcv);
                    }
                    //PCS缺陷坐标
                    if (!dic_PcsResult.ContainsKey(sn))
                    {
                        PcsResult result = new PcsResult();
                        result.vb_List = new List<VBRcvInfp>();
                        result.vb_List.AddRange(pcsResult.vb_List);
                        dic_PcsResult.Add(sn, result);
                    }
                    else
                    {
                        PcsResult result;
                        dic_PcsResult.TryGetValue(sn, out result);
                        result.vb_List.AddRange(pcsResult.vb_List);
                    }
                    //结果
                    if (!dic_Results.ContainsKey(sn))
                    {
                        List<string> result = new List<string>();
                        result.AddRange(resList);
                        dic_Results.Add(sn, result);
                    }
                    else
                    {
                        List<string> result = null;
                        dic_Results.TryGetValue(sn, out result);
                        result.AddRange(resList);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                this.lbl_selectCount.Text = "0";
                this.lbl_selectCount.Visible = chk_Mark.Checked;
                if (!chk_Mark.Checked)
                {
                    ResetAllToFalse();
                }
                for (int i = 0; i < DispWin1.Count(); i++)
                {
                    if (!chk_Mark.Checked)
                    {
                        DispWin1[i].lable = "";
                    }
                    DispWin1[i].isSelect = chk_Mark.Checked;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ResetAllToFalse()
        {
            try
            {
                selectPathList.Clear();
                List<string> keys = mark_List.Keys.ToList();
                foreach (string key in keys)
                {
                    mark_List[key] = false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void chk_Contrast_CheckedChanged(object sender, EventArgs e)
        {
            this.isContrast = chk_Contrast.Checked;
        }

        private async void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                //在这里用minio,下载到本地
                if (selectPathList.Count > 0)
                {
                    for (int i = 0; i < selectPathList.Count; i++)
                    {
                        string minio_ip = selectPathList[i].Split(':')[1];
                        Machine.master.workClass.minio.BuildClient(minio_ip, "9102");

                        string path = string.Empty;
                        string result = string.Empty;
                        //Machine.master.workClass.ParseMinioPath(selectPathList[i].Split(':')[0], out path, out result);
                        result = selectPathList[i].Split(':')[0].ToString().Split('/')[0].ToString();
                        await Task.Factory.StartNew(() =>
                        {
                            //Machine.master.workClass.minio.Download("deepiresults", $"{result}/", $"E:/minio/deepiresults/MarkImage/{selectPathList[i].Split(':')[0]}", minio_ip);
                            Machine.master.workClass.minio.Download("deepiresults", $"{selectPathList[i].Split(':')[0]}/", $"E:/MarkImage", minio_ip, true);
                        });
                    }
                    //selectPathList.Clear();
                    ResetAllToFalse();
                    foreach (var item in DispWin1)
                    {
                        item.lable = "";
                        item.Refresh();
                    }
                    this.lbl_selectCount.Text = "0";
                    MessageBox.Show($"图片导出完成！", "导出提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("标记图片列表为空！请标记后再点击导出！", "列表为空", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("导出异常" + ex.ToString());
            }
        }

        private void btnImagePrevious_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentPageOfImage > 1)
                {
                    btnImageNext.Enabled = false;
                    btnImagePrevious.Enabled = false;
                    currentPageOfImage--;
                    ShowImage();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void btnImageNext_Click(object sender, EventArgs e)
        {
            try
            {
                //获取上一页的索引
                if (currentPageOfImage < totalPagesOfImage)
                {
                    btnImageNext.Enabled = false;
                    btnImagePrevious.Enabled = false;
                    currentPageOfImage++;
                    ShowImage();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void DataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                {
                    // 延迟执行，确保选中行已经改变
                    this.BeginInvoke(new Action(() =>
                    {
                        if (dataSN.CurrentCell != null)
                        {
                            // 手动触发 CellClick 事件
                            DataGridViewCellEventArgs args = new DataGridViewCellEventArgs(
                                dataSN.CurrentCell.ColumnIndex,
                                dataSN.CurrentCell.RowIndex);

                            dataSN_CellClick(sender, args);
                        }
                    }));
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
                Index = (currentPageOfImage - 1) * 30;
                InitTableStyle(table_Small, 30, disInfosList);

                InitWork();
                //根据页索引获取图像源
                List<string> defect_paths = imagePaths;
                //List<string> gerberOrtemp_paths = Machine.ShowFlag == "B" ? imagePaths_Gerber : imagePaths_Template;
                var defect_pagedData = defect_paths.Skip((currentPageOfImage - 1) * 30).Take(30).ToList(); //imagePaths.Skip((currentPage - 1) * 30).Take(30).ToList();
                //var gerberOrtemp_pagedData = gerberOrtemp_paths.Skip((currentPageOfImage - 1) * 30).Take(30).ToList(); //imagePaths.Skip((currentPage - 1) * 30).Take(30).ToList();
                var defect_indexPaths = defect_pagedData.Select((path, index1) => new { Path = path, Index = index1 }).ToList();
                //var gerberOrtemp_indexPaths = gerberOrtemp_pagedData.Select((path, index1) => new { Path = path, Index = index1 }).ToList();

                //0630增加兼容三张图拼接显示
                List<List<string>> listOfAllImages = new List<List<string>>();
                // 初始化所有子列表
                for (int i = 0; i < defect_paths.Count; i++)
                {
                    listOfAllImages.Add(new List<string>());
                }
                if (isContrast)
                {
                    for (int i = 0; i < defect_paths.Count; i++)
                    {
                        List<string> item = new List<string>();
                        if (i >= 0 && i < imagePaths.Count)
                        {
                            item.Add(imagePaths[i]);
                        }
                        if (i >= 0 && i < imagePaths_Gerber.Count)
                        {
                            item.Add(imagePaths_Gerber[i]);
                        }
                        if (i >= 0 && i < imagePaths_Template.Count)
                        {
                            item.Add(imagePaths_Template[i]);
                        }

                        listOfAllImages[i] = item;
                    }
                }
                else
                {
                    for (int i = 0; i < defect_paths.Count; i++)
                    {
                        listOfAllImages[i] = new List<string>();
                        List<string> item = new List<string>();
                        item.Add(imagePaths[i]);
                        listOfAllImages[i] = item;
                    }
                }

                var list_allImages = listOfAllImages.Skip((currentPageOfImage - 1) * 30).Take(30).ToList();
                var all_Images = list_allImages.Select((path, index1) => new { Path = path, Index = index1 }).ToList();

                //并行
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount - 1,
                    CancellationToken = CancellationToken.None
                };
                List<string> res_lbl = null;
                PcsResult pcsResult;
                LogTextHelper.Info($"准备从{str_SN}开始从字典中开始取值");
                if (dic_Results.TryGetValue(str_SN, out res_lbl) && dic_PcsResult.TryGetValue(str_SN, out pcsResult))
                {
                    LogTextHelper.Info($"222");
                    await Task.Factory.StartNew(() =>
                    {
                        Parallel.ForEach(all_Images, parallelOptions, item =>
                        {
                            // 在访问前添加检查
                            int index1 = (currentPageOfImage - 1) * 10 + item.Index;
                            string labelText = "未处理";
                            if (res_lbl != null && index1 >= 0 && index1 < res_lbl.Count) // 如果是数组
                            {
                                labelText = res_lbl[index1] ?? "未处理";
                            }
                            else if (res_lbl is IList<string> list && index1 >= 0 && index1 < list.Count)
                            {
                                labelText = list[index1] ?? "未处理";
                            }

                            int index2 = (currentPageOfImage - 1) * 10 + item.Index;
                            VBRcvInfp vbValue = null;
                            if (pcsResult?.vb_List != null && index2 >= 0 && index2 < pcsResult.vb_List.Count)
                            {
                                vbValue = pcsResult.vb_List[index2];
                            }
                            LogTextHelper.Info($"{labelText},{vbValue}");
                            if (!isContrast)
                            {
                                Machine.master.workClass.showImage2(item.Index, item.Path, labelText, vbValue);
                            }
                            else
                            {
                                Machine.master.workClass.showImage2(item.Index, item.Path, labelText, vbValue);
                            }
                        });
                    });

                    if (currentPageOfImage == totalPagesOfImage)
                    {
                        await Task.Factory.StartNew(() =>
                        {
                            Parallel.For(defect_pagedData.Count, 30, item =>
                            {
                                Machine.master.workClass.showImage2(item, new List<string>());
                            });
                        });
                    }
                }
                else
                {
                    LogTextHelper.Info($"在字典中未找到信息...");
                }
                //await Task.Factory.StartNew(() =>
                //{
                //    Parallel.ForEach(all_Images, parallelOptions, item =>
                //    {
                //        if (!isContrast)
                //        {
                //            Machine.master.workClass.showImage2(item.Index, item.Path);
                //        }
                //        else
                //        {
                //            Machine.master.workClass.showImage2(item.Index, item.Path);
                //        }
                //    });
                //});


                //if (currentPageOfImage == totalPagesOfImage)
                //{
                //    await Task.Factory.StartNew(() =>
                //    {
                //        Parallel.For(defect_pagedData.Count, 30, item =>
                //        {
                //            Machine.master.workClass.showImage2(item, new List<string>());
                //        });
                //    });
                //}
                UpdatePagingControlsOfImage();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常" + ex.ToString());
            }
        }

        public void InitWork()
        {
            try
            {
                List<CvDisplay> DisplaysList = new List<CvDisplay>();
                try
                {
                    for (int j = 0; j < DispWin1.Length; ++j)
                    {
                        DisplaysList.Add(DispWin1[j]);
                    }
                    Machine.master.workClass.setHWindow2(DisplaysList);
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
        public void InitTableStyle(TableLayoutPanel panel, int pixNum, List<DisPlayInfo> infos)
        {
            //int row = 1;
            //int column = 10;
            //panel.ColumnCount = 10;
            //if (pixNum % 10 == 0)
            //{
            //    table_Small.RowCount = row = (pixNum / 10);
            //}
            //else
            //{
            //    table_Small.RowCount = row = (pixNum / 10) + 1;
            //}

            int index = 0;
            for (int i = 0; i < table_Small.RowCount; i++)
            {
                for (int j = 0; j < table_Small.ColumnCount; j++)
                {
                    if (Index + index < infos.Count)
                    {
                        DispWin1[index].info = infos[Index + index];
                        if (index + 1 <= pixNum)
                        {

                            DispWin1[index].DrawStation($"缺陷{Index + index + 1}:{infos[Index + index].defect_code}");
                            DispWin1[index].DrawSelect(mark_List[(Index + index).ToString()]);
                        }
                    }
                    else
                    {
                        DispWin1[index].DrawStation($"");
                        DispWin1[index].DrawSelect(false);
                    }
                    index++;
                }
            }
        }
        private void UpdatePagingControlsOfImage()
        {
            try
            {
                lblImagePageInfo.Text = $"第 {currentPageOfImage} 页 / 共 {totalPagesOfImage} 页";
                btnImagePrevious.Enabled = (currentPageOfImage > 1);
                btnImageNext.Enabled = (currentPageOfImage < totalPagesOfImage);
            }
            catch (Exception ex)
            {
                LogTextHelper.Info("更新上下页异常" + ex.ToString());
            }

        }

        private void cmb_byWhat_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.txt_code.Clear();
            if (cmb_byWhat.SelectedIndex == 2)
            {
                this.dateTimeStart.Visible = true;
                this.dateTimeEnd.Visible = true;
                lbl_start.Visible = true;
                lbl_end.Visible = true;
            }
            else
            {
                this.dateTimeStart.Visible = false;
                this.dateTimeEnd.Visible = false;
                this.lbl_start.Visible = false;
                this.lbl_end.Visible = false;
            }
        }


        private void Clear(int level = 0)
        {
            try
            {
                lblImagePageInfo.Text = $"第 {1} 页 / 共 {1} 页";
                imagePaths.Clear();
                imagePaths_Gerber.Clear();
                imagePaths_Template.Clear();
                disInfosList.Clear();
                ImagePaths.Clear();
                ImagePaths_Gerber.Clear();
                ImagePaths_Template.Clear();
                DisInfosList.Clear();
                Index = 0;
                totalPagesOfImage = 0;
                currentPageOfImage = 1;

                selectPathList.Clear();
                lbl_selectCount.Text = "0";
                mark_List.Clear();
                defectTypeList.Clear();

                //清空显示控件
                foreach (var item in DispWin1)
                {
                    if (item.Image != null)
                    {
                        item.lable = "";
                        item.DrawStation("");
                        item.Clear();
                    }
                }

                if (level == 1)
                {
                    dataSN.Rows.Clear();
                    dataLot.Rows.Clear();
                    dataDefectType.Rows.Clear();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        string str_SN = string.Empty;
        private void dataSN_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                //判断是否是第二列
                if (e.ColumnIndex == 0 && dataSN.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
                {
                    //先清空其它列的勾选
                    // 遍历所有行
                    foreach (DataGridViewRow row in dataSN.Rows)
                    {
                        // 跳过新行（编辑行）
                        if (!row.IsNewRow)
                        {
                            // 找到复选框列并设置为未选中
                            DataGridViewCheckBoxCell cell = row.Cells["isSnSelect"] as DataGridViewCheckBoxCell;
                            if (cell != null)
                            {
                                cell.Value = false;
                            }
                        }
                    }
                    //获取sn
                    str_SN = dataSN.Rows[e.RowIndex].Cells[1].Value?.ToString() ?? "空值";
                    queryDataBySn(str_SN);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void dataDefectType_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                //判断是否是第二列
                if (e.ColumnIndex == 0 && dataDefectType.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
                {
                    //先清空其它列的勾选
                    // 遍历所有行
                    foreach (DataGridViewRow row in dataDefectType.Rows)
                    {
                        // 跳过新行（编辑行）
                        if (!row.IsNewRow)
                        {
                            // 找到复选框列并设置为未选中
                            DataGridViewCheckBoxCell cell = row.Cells["isDefectSelect"] as DataGridViewCheckBoxCell;
                            if (cell != null)
                            {
                                cell.Value = false;
                            }
                        }
                    }
                    //根据缺陷加载
                    string defectName = dataDefectType.Rows[e.RowIndex].Cells[1].Value?.ToString() ?? "空值";

                    //将各类图的源数据重新绑定
                    imagePaths = ImagePaths.Where(o => o.Contains(defectName)).ToList();
                    imagePaths_Gerber = ImagePaths_Gerber.Where(o => o.Contains(defectName)).ToList();
                    imagePaths_Template = ImagePaths_Template.Where(o => o.Contains(defectName)).ToList();
                    disInfosList = DisInfosList.Where(o => o.defect_code == defectName).ToList();

                    //
                    totalPagesOfImage = (int)Math.Ceiling((double)(disInfosList.Count - 1) / 30);
                    currentPageOfImage = 1;
                    ShowImage();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void cmb_dataByWhat_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.txt_dataCode.Clear();
        }
        /// <summary>
        /// 产品图原点坐标距离大图原点坐标的偏移,渲染热力图坐标时需要减去此坐标
        /// </summary>
        public int offsetX;
        public int offsetY;




        // 数据模型
        public class Data
        {
            public string CreateTime { get; set; }
            public string Code { get; set; }
            public string TotalCount { get; set; }
            public string BD_Count { get; set; }
            public string OK_Count { get; set; }
            public string NG_Count { get; set; }
        }


    }
}

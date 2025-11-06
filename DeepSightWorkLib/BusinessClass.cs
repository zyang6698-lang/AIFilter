using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeepSightTool;
using DeepSightEvent;
using DeepSightModel;
using DeepSightCommunication;
using DeepSightDisplay;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Runtime.InteropServices;
using DeepSightDB;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Minio;
using Minio.Exceptions;
using Minio.DataModel.Args;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using DeepsightSqlite;

namespace DeepSightWorkLib
{
    //后续考虑是否设计为抽象，支持传统及AI调用
    public class BusinessClass : IDisposable
    {
        //算法检测对象
        public DefectClass defect = null;
        //levelDB交互
        public HttpClass http_DB = null;
        //Minio服务
        public MinioClass minio = null;

        private DatabaseHelper databaseHelper = null;
        //图片路径
        public string ImagePath { get; set; }
        //处理索引
        public int count_Index { get; set; } = 0;
        //读取AVI存储对象 <Key,SN,Side,DefectIndex,VBInfo>
        //public ConcurrentQueue<Tuple<string, string, string,List<int>, RootVBInfo>> que_AVI = new ConcurrentQueue<Tuple<string, string,string, List<int>, RootVBInfo>>();
        public ConcurrentQueue<VBModel> que_AVI = new ConcurrentQueue<VBModel>();
        //算法处理结果存储对象<Key,SN,,DbInfo>
        public ConcurrentQueue<Tuple<string, string, string, RootAIResult>> que_AI = new ConcurrentQueue<Tuple<string, string, string, RootAIResult>>();

        public ConcurrentDictionary<string, MinioClient> dic_Minio = new ConcurrentDictionary<string, MinioClient>();
        //奥特斯项目上传中台
        public ConcurrentDictionary<string, DsCenterInfo> dic_DsCenterInfo = new ConcurrentDictionary<string, DsCenterInfo>();
        //游标索引
        public string Index { get; set; } = "";
        //结束游标记录
        public string endIndex { get; set; } = "";
        //开始/停⽌作业
        public bool isStart = false;

        public bool isShowBox = false;

        public bool IsAllow = true;

        /// <summary>
        /// 是否测试模式
        /// true：测试模式
        /// </summary>
        public bool TestFlag = false;//{ get; private set; }
        //URL
        public string URL { get; set; }
        public string ProductSerial { get; set; } = "";
        public string solution { get; set; } = "";
        public string flow { get; set; } = "";
        public bool isSwitch { get; set; } = false;

        public SolutionConfig solconfig = new SolutionConfig();
        public AVIConfig aviconfig = new AVIConfig();
        public ConfigurationClass sysConfig = new ConfigurationClass();

        //大图显示集合
        public CvDisplay DisWin { get; set; }
        //小图显示集合
        public List<CvDisplay> DisplaysList { get; set; }
        public List<CvDisplay> DisplaysList2 { get; set; }
        public BusinessClass()
        {
            defect = new DefectClass();
            http_DB = new HttpClass();
            minio = new MinioClass();
            databaseHelper = new DatabaseHelper();
            DatabaseHelper.InitializeDatabase();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="Index">游标索引</param>
        public void InitWork(string URL, string Index)
        {
            this.URL = URL;
            this.Index = Index;
            this.isStart = false;
            //工作线程
            start_readAVI();
            start_Defect();
            start_ReturnAVI();
        }
        public void SetConfig(bool TestFlag, bool IsStart)
        {
            this.TestFlag = TestFlag;
            this.isStart = IsStart;
        }
        public void setHWindow(List<CvDisplay> displaysList)
        {
            try
            {
                if (DisplaysList == null)
                {
                    DisplaysList = new List<CvDisplay>();
                }
                DisplaysList.Clear();
                for (int i = 0; i < displaysList.Count; i++)
                {
                    DisplaysList.Add(displaysList[i]);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
            }
        }
        public void setHWindow2(List<CvDisplay> displaysList)
        {
            try
            {
                if (DisplaysList2 == null)
                {
                    DisplaysList2 = new List<CvDisplay>();
                }
                DisplaysList2.Clear();
                for (int i = 0; i < displaysList.Count; i++)
                {
                    DisplaysList2.Add(displaysList[i]);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
            }
        }


        //读取AVI结果线程
        //算法处理图片线程
        //回写结果线程
        //开始与暂停的标志位控制整体逻辑运行（软件开启默认为暂停）

        /// <summary>
        /// 获取AVI结果线程
        /// </summary>
        private Thread th_ReadAVI = null;
        /// <summary> 
        /// 算法处理线程
        /// </summary>
        private Thread th_Defect = null;
        /// <summary>
        /// 回写结果线程
        /// </summary>
        private Thread th_ReturnAVI = null;
        /// <summary>
        /// 线程标识
        /// </summary>
        private volatile bool _shouldStop_ReadAVI = true;
        /// <summary>
        /// 线程标识
        /// </summary>
        private volatile bool _shouldStop_Defect = true;
        /// <summary>
        /// 线程标识
        /// </summary>
        private volatile bool _shouldStop_ReturnAVI = true;
        /// <summary>
        /// 开始线程
        /// </summary>
        private void start_readAVI()
        {
            try
            {
                if (th_ReadAVI != null)
                {
                    stop_readAVI();
                }
                th_ReadAVI = new Thread(new ThreadStart(ThreadReadAVI))
                {
                    IsBackground = true
                };
                _shouldStop_ReadAVI = false;
                th_ReadAVI.Start();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 停止线程
        /// </summary>
        private void stop_readAVI()
        {
            try
            {
                if (th_ReadAVI != null)
                {
                    _shouldStop_ReadAVI = true;
                    while (th_ReadAVI.IsAlive)
                    {
                        Thread.Sleep(5);
                    }
                    th_ReadAVI.Abort();
                    th_ReadAVI = null;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 线程处理
        /// </summary>
        private void ThreadReadAVI()
        {
            while (!_shouldStop_ReadAVI)
            {
                Thread.Sleep(1000);
                try
                {
                    if (!isStart || TestFlag || !IsAllow)
                    {
                        continue;
                    }
                    //读取AVI数据
                    string result = "";
                    //URL = "http://192.168.77.243:9877";
                    if (ReadAVI(URL, out result))
                    {
                        doAviJson(result);
                        //que_AVI.Enqueue(result);
                        //发送当前未完成任务队列至UI
                        //SystemEvent.SendTaskMsg(que_AVI);

                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error(ex.ToString());
                }
            }
        }
        public void doAviJson(string jsonInfo)
        {
            try
            {
                // 解析外层 JSON
                var root = JObject.Parse(jsonInfo);
                string db_name = root["db_name"].ToString();
                string operation = root["operation"].ToString();
                string resultCode = root["resultCode"].ToString();
                int num = root["data_list"].Count();//
                if (num == 1)
                {
                    return;
                }
                Index = endIndex;
                foreach (var item in root["data_list"])
                {
                    try
                    {
                        string itemStr = item.ToString();
                        // 跳过"end_range_send"
                        if (itemStr == "end_range_send") continue;

                        // 解析data_list 
                        var itemObj = JObject.Parse(itemStr);
                        //string key = "20250528203728639334";//itemObj["key"].ToString();
                        string key = itemObj["key"].ToString();
                        string valueStr = itemObj["value"].ToString();

                        var valueObj = JObject.Parse(valueStr);
                        //string serialNumber = $"{valueObj["serial_number"].ToString()}_{++count_Index}";
                        string serialNumber = $"{valueObj["serial_number"].ToString()}";

                        LogTextHelper.Info($"获取到{serialNumber}的数据");
                        // 遍历 result_infos
                        if (valueObj["results_info"] == null) continue;
                        foreach (var info1 in valueObj["results_info"])
                        {
                            string side = info1["side"].ToString();
                            string minio_ip = info1["minio_ip"].ToString();
                            string minio_port = info1["minio_port"].ToString();

                            if (string.IsNullOrEmpty(minio_ip) || string.IsNullOrEmpty(minio_port) || minio_port == "0")
                            {
                                Thread.Sleep(500);
                                SystemEvent.SendTaskMsg(serialNumber, $"{side}面Minio格式错误");
                                SystemEvent.SendAlarmMsg($"{"SN:"}{serialNumber} {side}面 {"Minio格式错误"};具体信息 MinioIP:{minio_ip}MinioPort:{minio_port}");
                                continue;
                            }
                            string result_path = info1["result_path"].ToString();
                            LogTextHelper.Info($"SN:{serialNumber}PATH:{result_path}");
                            string path = string.Empty;
                            string result = string.Empty;
                            ParseMinioPath(result_path, out path, out result);
                            //测试
                            //path = @"20250508152421059165/20250508152421059165-panel.json";
                            //ReadJsonByPath(key, serialNumber, side, path);
                            //if (side == "A")
                            //{
                            //    minio_ip = "192.168.77.165";
                            //}
                            //else
                            //{
                            //    continue;
                            //    minio_ip = "192.168.77.5";
                            //}
                            ReadJsonByMinio(minio_ip, minio_port, key, result, serialNumber, side, path);
                        }

                        //在这里存储 SN & KEY 关系
                        //A\B面只存储一次
                        RootDbInfo snInfo = new RootDbInfo();
                        snInfo.db_name = "filter_time_to_sn";
                        snInfo.operation = "put";
                        snInfo.op_mode = "all_ow";
                        //snInfo.key = key;
                        //snInfo.value = serialNumber.Split('_').ToArray()[0];
                        snInfo.key = serialNumber;//.Split('_').ToArray()[0];
                        snInfo.value = key;
                        string Result;
                        //Task.Factory.StartNew(() =>
                        //{
                        http_DB.HttpPostMethod(URL, snInfo, 1, out Result);
                        //});
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error(ex.ToString());
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("处理AVI_JSON异常" + ex.ToString());
            }
        }
        /// <summary>
        /// 解析Minio路径
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        //public (string path, string result) ParseMinioPath(string fullPath)
        public void ParseMinioPath(string fullPath, out string path, out string result)
        {
            try
            {
                string normalizedPath = fullPath.Replace('\\', '/');
                string[] parts = normalizedPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                int minioIndex = Array.IndexOf(parts, "deepiresults");

                if (minioIndex == -1)
                {
                    throw new ArgumentException("路径中未包含 'minio' 目录");
                }

                if (parts.Length < minioIndex + 3)
                {
                    throw new ArgumentException("路径不完整，缺少存储桶或对象键");
                }
                string bucketName = parts[minioIndex + 1];
                string objectKey = string.Join("/", parts, minioIndex + 2, parts.Length - minioIndex - 2);

                string str = string.Join("/", parts, minioIndex + 1, parts.Length - minioIndex - 2);
                path = $"{bucketName}/{objectKey}";
                result = str;
            }
            catch (Exception ex)
            {
                path = "";
                result = "";
                LogTextHelper.Error(ex.ToString());
            }

            //return ($"{bucketName}/{objectKey}", str);
        }


        public bool ReadAVI(string url, out string Result)
        {
            RootDbInfo getInfo = new RootDbInfo();
            getInfo.uniqueKey = Guid.NewGuid().ToString();
            getInfo.db_name = "ai_merged_results";
            getInfo.operation = "get";
            getInfo.is_select_range = "true";
            getInfo.op_mode = "all";
            getInfo.range_start = Index;//DateTime.Now.ToString("yyyyMMddHHmmssfff");
            endIndex = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            getInfo.range_end = endIndex;
            return http_DB.HttpPostMethod(url, getInfo, 0, out Result);
        }
        /// <summary>
        /// 同时日期范围读取料号对应的PN信息
        /// </summary>
        /// <param name="url"></param>
        /// <param name="productSerial"></param>
        /// <param name="date"></param>
        /// <param name="Result"></param>
        /// <returns></returns>
        public bool ReadPNSNByTime(DateTime date, out string Result)
        {
            string a = Guid.NewGuid().ToString();
            RootDbInfo getInfo = new RootDbInfo();
            getInfo.uniqueKey = Guid.NewGuid().ToString();
            getInfo.db_name = "product_panel";
            getInfo.operation = "get";
            getInfo.is_select_range = "true";
            getInfo.op_mode = "all";
            getInfo.range_start = date.Date.ToString("yyyyMMddHHmmssfff");
            getInfo.range_end = date.Date.AddDays(1).AddTicks(-1).ToString("yyyyMMddHHmmssfff");
            return http_DB.HttpPostMethod(URL, getInfo, 0, out Result);
        }



        /// <summary>
        /// 开始线程
        /// </summary>
        private void start_Defect()
        {
            try
            {
                if (th_Defect != null)
                {
                    stop_Defect();
                }
                th_Defect = new Thread(new ThreadStart(ThreadDefect))
                {
                    IsBackground = true
                };
                _shouldStop_Defect = false;
                th_Defect.Start();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 停止线程
        /// </summary>
        private void stop_Defect()
        {
            try
            {
                if (th_Defect != null)
                {
                    _shouldStop_Defect = true;
                    while (th_Defect.IsAlive)
                    {
                        Thread.Sleep(1);
                    }
                    th_Defect.Abort();
                    th_Defect = null;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
            }
        }

        private int Acount = 0;
        /// <summary>
        /// 线程处理
        /// </summary>
        private void ThreadDefect()
        {
            while (!_shouldStop_Defect)
            {
                Thread.Sleep(15);

                if (!isStart)
                {
                    continue;
                }
                if (que_AVI.Count > 0)
                {
                    VBModel info = null;
                    if (que_AVI.TryDequeue(out info))
                    {
                        try
                        {
                            SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面处理中");
                            //return;
                            //调用算法处理
                            LogTextHelper.Info("准备DefectMethod");
                            List<string> msg;
                            List<string> details;
                            PcsResult pcsResult;
                            string vbJson = null;

                            //test
                            if (true)
                            {
                                UpdateProductPanel(info);
                            }

                            if (DefectMethod(info.VbInfo, info.panelInfo, out msg, out details, out pcsResult, out vbJson))
                            {
                                SystemEvent.SendResultInfo(info.SN, msg, details, pcsResult);
                                if (TestFlag)
                                {
                                    SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面已完成");
                                    LogTextHelper.Info($"算法返回结果：{string.Join(",", msg)}");
                                }
                                else
                                {
                                    RootAIResult data = new RootAIResult();
                                    data.DbName = "filter_time_to_airesults";
                                    data.Operation = "put";
                                    data.OpMode = "all_ow";
                                    if (info.Side == "B")
                                    {
                                        data.OpMode = "ap";
                                    }
                                    data.Key = info.Key;
                                    List<ResultInfo> results = new List<ResultInfo>();
                                    for (int i = 0; i < info.DefectIndex.Count; i++)
                                    {
                                        ResultInfo res = new ResultInfo();
                                        res.ResultInfos = $"{info.Side}_{info.PcsIndex[i]}_{info.DefectIndex[i]}_{msg[i]}";
                                        //res.ResultInfos = $"{info.Side}_{info.PcsIndex[i]}_{info.DefectIndex[i]}_1";
                                        res.Details = new Details();
                                        results.Add(res);
                                    }
                                    JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };//去掉空值NULL
                                    data.Value = JsonConvert.SerializeObject(results, Formatting.None, jsonSetting);

                                    Tuple<string, string, string, RootAIResult> dbTub = Tuple.Create(info.Key, info.SN, info.Side, data);
                                    //存储算法处理的结果
                                    que_AI.Enqueue(dbTub);
                                }
                                LogTextHelper.SaveVBResultInfo($"{info.SN}：{string.Join(",", msg)}");
                                LogTextHelper.WriteJsonFile($"D:/minio/deepiresults/{info.minioPath}/{info.SN}-vb.json", vbJson);
                                if (info.Side == "B")
                                {
                                    RootDbInfo dbInfo = new RootDbInfo();
                                    dbInfo.db_name = "panel_list";
                                    dbInfo.operation = "put";
                                    dbInfo.op_mode = "ap";
                                    //lot
                                    dbInfo.key = $"{info.panelInfo.LotId}";
                                    dbInfo.value = info.panelInfo.SerialNumber;
                                    http_DB.HttpPostMethod("http://127.0.0.1:9877", dbInfo, 1, out _);
                                    //机台
                                    dbInfo.db_name = "machine_panel";
                                    dbInfo.key = $"{info.panelInfo.MachineName}";
                                    http_DB.HttpPostMethod("http://127.0.0.1:9877", dbInfo, 1, out _);
                                    //料号
                                    UpdateProductPanel(info);

                                    
                                    //databaseHelper.UpdateDailyStats(info.panelInfo.MachineName,info.panelInfo.StartTime,true);

                                    //中台
                                    DsCenterInfo dsinfo;
                                    dic_DsCenterInfo.TryRemove($"{info.panelInfo.LotId}_{info.panelInfo.SerialNumber}", out dsinfo);
                                    string outInfo = null;
                                    http_DB.HttpPostMethod2(sysConfig.DsCenterUrl, dsinfo, 0, out outInfo);
                                    LogTextHelper.Info($"sn:{info.SN}_中台数据返回信息:{outInfo}");
                                }
                            }
                            else
                            {
                                LogTextHelper.Warn($"KEY:{info.Key} SN:{info.SN}检测失败！");
                                SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面已完成");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogTextHelper.Error(ex.ToString());
                            SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面已完成");
                        }
                    }
                }
            }
        }


        private void UpdateProductPanel(VBModel info)
        {
            RootDbInfo dbInfo = new RootDbInfo()
            {
                db_name = "product_panel",
                operation = "put",
                op_mode = "ap",

                key = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                value = JsonConvert.SerializeObject(new { ProductSerial = info.panelInfo.ProductSerial, SerialNumber = info.panelInfo.SerialNumber }),
            };
            http_DB.HttpPostMethod("http://127.0.0.1:9877", dbInfo, 1, out _);
        }
        public void ReadJsonByPath(string key, string sn, string side, string path)
        {
            try
            {
                ////这里用的minio存储 下面方式可能需要更改
                //string json = File.ReadAllText(path);
                //var obj = JsonConvert.DeserializeObject<RootPanelInfo>(json);



                //SystemEvent.SendPanelInfo(sn, obj);
                ////在这里处理算法需要的参数
                //List<int> defectIndex = new List<int>();
                //List<int> pcsList = new List<int>();
                //RootVBInfo vbInfo = PanelJsonToVBInfo(key, obj, ref defectIndex, ref pcsList);
                ////考虑用Model 这个方式
                ////Tuple<string, string,string,List<int>, RootVBInfo> vbTub = Tuple.Create(key, sn,side, defectIndex, vbInfo);
                //VBModel model = new VBModel();
                //model.Key = key;
                //model.SN = sn;
                //model.Side = side;
                //model.DefectIndex = defectIndex;
                //model.PcsIndex = pcsList;
                //model.VbInfo = vbInfo;
                //que_AVI.Enqueue(model);

            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常" + ex.ToString());
            }

        }
        /// <summary>
        /// 通过Minio读取Json文件
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="key"></param>
        /// <param name="head"></param>
        /// <param name="sn"></param>
        /// <param name="side"></param>
        /// <param name="path"></param>
        public void ReadJsonByMinio(string ip, string port, string key, string head, string sn, string side, string path)
        {
            try
            {
                LogTextHelper.Info("准备ReadJsonByMinio");
                minio.BuildClient(ip, port);
                string json = minio.ReadJsonSync("deepiresults", path, ip);
                var obj = JsonConvert.DeserializeObject<RootPanelInfo>(json);
                //LogTextHelper.Info($"{sn}_{side}面json为{json}");
                //在这里处理算法需要的参数
                List<int> defectIndex = new List<int>();
                List<int> pcsList = new List<int>();
                if (TestFlag)
                {
                    //测试模式下要从json里面获取一下side
                    side = obj.SideIndex;
                }
                //sn = $"{sn}_{obj.StationName}";
                if (side == "A")
                {
                    SystemEvent.SendTaskMsg(sn);
                }
                RootVBInfo vbInfo = PanelJsonToVBInfo(ip, port, head, obj, ref defectIndex, ref pcsList);

                //考虑用Model方式
                //Tuple<string, string,string,List<int>, RootVBInfo> vbTub = Tuple.Create(key, sn,side, defectIndex, vbInfo);
                VBModel model = new VBModel();
                model.Key = key;
                model.SN = sn;
                model.Side = side;
                model.DefectIndex = defectIndex;
                model.PcsIndex = pcsList;
                model.VbInfo = vbInfo;
                model.minioPath = head;
                model.panelInfo = obj;
                que_AVI.Enqueue(model);
                RootPanelInfoWithIP rootobj = new RootPanelInfoWithIP()
                {
                    IP = ip,
                    rootInfo = obj,
                };
                SystemEvent.SendPanelInfo(sn, rootobj);
                LogTextHelper.Info("ReadJsonByMinio完成,入队列que_AVI成功");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常" + ex.ToString());
            }
        }

        public RootVBInfo PanelJsonToVBInfo(string minioip, string minioport, string head, RootPanelInfo info, ref List<int> defectList, ref List<int> pcsList)
        {
            try
            {
                LogTextHelper.Info("ProcuctSerial:" + info.ProductSerial);
                //20250811 奥特斯项目将料号与solution/flow绑定，实时根据配置档传进的进行匹配
                List<SolutionAndFlow> listSolutionFlow = solconfig.solus.FindAll(o => o.ProductSerial == info.ProductSerial).ToList();
                if (listSolutionFlow.Count > 0)
                {
                    if (info.SideIndex == "A")
                    {
                        solution = listSolutionFlow[0].Asolution;
                        flow = listSolutionFlow[0].Aflow;
                    }
                    else
                    {
                        solution = listSolutionFlow[0].Bsolution;
                        flow = listSolutionFlow[0].Bflow;
                    }
                    isSwitch = listSolutionFlow[0].IsSwitch;
                }
                else
                {
                    List<SolutionAndFlow> defaultSolutionFlow = solconfig.solus.FindAll(o => o.ProductSerial.ToUpper() == "DEFAULT").ToList();
                    if (defaultSolutionFlow.Count > 0)
                    {
                        if (info.SideIndex == "A")
                        {
                            solution = defaultSolutionFlow[0].Asolution;
                            flow = defaultSolutionFlow[0].Aflow;
                        }
                        else
                        {
                            solution = defaultSolutionFlow[0].Bsolution;
                            flow = defaultSolutionFlow[0].Bflow;
                        }
                        isSwitch = defaultSolutionFlow[0].IsSwitch;
                    }
                    else
                    {
                        solution = "0729";
                        flow = "0729";
                        isSwitch = false;
                    }
                }

                //奥特斯项目增加上传中台
                DsCenterInfo dsInfo = new DsCenterInfo();
                dsInfo.Project = sysConfig.ProjectName;
                PanelData panelData = new PanelData();
                panelData.Project = sysConfig.ProjectName;
                if (info.SideIndex == "A")
                {
                    panelData.Product = info.ProductSerial;
                    panelData.Lot = info.LotId;
                    panelData.Sn = info.SerialNumber;
                    panelData.Avi = info.StationName;
                    panelData.CustomTags = "";
                }
                else
                {

                    dic_DsCenterInfo.TryGetValue($"{info.LotId}_{info.SerialNumber}", out dsInfo);
                    JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };//去掉空值NULL
                    string infoJson = JsonConvert.SerializeObject(dsInfo, Formatting.None, jsonSetting);
                    //LogTextHelper.Info("DSINFO:" + infoJson);

                }

                LogTextHelper.Info($"当前产品:{info.SerialNumber},{info.SideIndex}面,所属料号:{info.ProductSerial},切换参数-->方案:{solution},flow:{flow}");
                RootVBInfo vBInfo = new RootVBInfo();
                vBInfo.MessageType = "visionbuilder_inference";
                vBInfo.paramsData = new ParamsData();
                //同一个任务的UUID是否要保持一致；
                vBInfo.paramsData.InferResUuid = Guid.NewGuid().ToString();
                vBInfo.paramsData.InferWholeData = new InferWholeData();
                vBInfo.paramsData.InferWholeData.ImageInferParams = new ImageInferParams();
                vBInfo.paramsData.InferWholeData.ImageInferParams.PipelineName = solution;//"test";
                vBInfo.paramsData.InferWholeData.ImageInferParams.NodeParams = new List<NodeParam>();
                vBInfo.paramsData.InferWholeData.ImageInferParams.NodeParams.Add(
                  new NodeParam()
                  {
                      NodeName = flow,//"1",
                      height = 200,
                      width = 200,
                  });

                vBInfo.paramsData.InferWholeData.ImageData = new ImageData();
                //#使⽤minio获取 则固定字段"minio"
                vBInfo.paramsData.InferWholeData.ImageData.DataType = "minio";
                vBInfo.paramsData.InferWholeData.ImageData.DataValue = new DataValue();
                vBInfo.paramsData.InferWholeData.ImageData.DataValue.InferImageGroup = new List<InferImageGroup>();
                for (int i = 0; i < info.PcsInfo.Count; i++)
                {
                    ContentItem item = new ContentItem();
                    if (info.SideIndex == "A")
                    {
                        item.MachineName = info.StationName;
                        item.ProductSerial = info.ProductSerial;
                        item.SerialNumber = info.SerialNumber;
                        item.ProcessTimeA = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
                        item.LotId = info.LotId;
                        item.Lot = info.LotId;
                        item.Product = info.ProductSerial;
                        item.OperateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        item.ExpansionAndContraction = null;
                        item.PieceIndex = (i + 1).ToString();
                        item.PieceVesIndex = (i + 1).ToString();
                    }
                    else
                    {
                        JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };//去掉空值NULL
                        string infoJson = JsonConvert.SerializeObject(dsInfo, Formatting.None, jsonSetting);
                        //LogTextHelper.Info("DSINFO:"+infoJson);
                        dsInfo.Data[0].Content.TryGetValue((i + 1).ToString(), out item);
                        item.ProcessTimeB = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
                    }

                    PcsInfo pcsInfo = null;
                    if (info.PcsInfo.TryGetValue((i + 1).ToString(), out pcsInfo))
                    {
                        for (int j = 0; j < pcsInfo.DefectInfo.Count; j++)
                        {
                            //奥特斯
                            DsCenterDefectInfo dsDefectinfo = new DsCenterDefectInfo();
                            dsDefectinfo.SideType = info.SideIndex;
                            dsDefectinfo.SideType2 = info.SideIndex;
                            dsDefectinfo.PcsIndex = i + 1;
                            dsDefectinfo.PcsVesIndex = (i + 1).ToString();
                            dsDefectinfo.DefectRoi = pcsInfo.DefectInfo[j].DefectRoi;

                            dsDefectinfo.DefectOriginRoi = pcsInfo.DefectInfo[j].DefectOriginRoi;
                            dsDefectinfo.DefectsRoi.Add(pcsInfo.DefectInfo[j].DefectRoi.X);
                            dsDefectinfo.DefectsRoi.Add(pcsInfo.DefectInfo[j].DefectRoi.Y);
                            dsDefectinfo.DefectsRoi.Add(pcsInfo.DefectInfo[j].DefectRoi.Height);
                            dsDefectinfo.DefectsRoi.Add(pcsInfo.DefectInfo[j].DefectRoi.Width);

                            InferImageGroup group = new InferImageGroup();
                            group.GroupUuid = Guid.NewGuid().ToString();
                            group.GroupInfos = new List<GroupInfo>();
                            group.DefectCode = "";
                            group.TempImgPath = $"D:\\ATS_AI_INSTALL\\TemplateImages\\{info.ProductSerial}\\{info.ProductSerial}[{info.SideIndex}].jpg";
                            //group.TempImgPath = $"D:\\ATS_AI_INSTALL\\TemplateImages\\NYA1548\\NYA1548[{info.SideIndex}].jpg" ;
                            group.ImgROI = new List<int>();
                            group.ImgROI.Add(pcsInfo.DefectInfo[j].DefectRoi.X);
                            group.ImgROI.Add(pcsInfo.DefectInfo[j].DefectRoi.Y);
                            group.ImgROI.Add(pcsInfo.DefectInfo[j].DefectRoi.Width);
                            group.ImgROI.Add(pcsInfo.DefectInfo[j].DefectRoi.Height);
                            if (isSwitch)
                            {
                                group.DefectCode = pcsInfo.DefectInfo[j].DefectCode;
                            }
                            WatchPathConfig config = aviconfig.WatchPaths.Find(o => o.AviName == info.StationName);
                            for (int k = 0; k < 3; k++)
                            {
                                switch (k)
                                {
                                    case 0:
                                        if (pcsInfo.DefectInfo[j].DefectVrsImages != null)
                                        {
                                            group.GroupInfos.Add(new GroupInfo()
                                            {
                                                ImagePath = $"{head}/{pcsInfo.DefectInfo[j].DefectVrsImages[0].ToString()}",
                                                ImageUuid = Guid.NewGuid().ToString(),
                                                ImageType = "defect",
                                            });

                                            dsDefectinfo.DefectImages.Add($"http://{config.MinioConfig}/deepiresults/{head}/{pcsInfo.DefectInfo[j].DefectVrsImages[0].ToString()}");
                                        }

                                        break;
                                    case 1:
                                        if (pcsInfo.DefectInfo[j].DefectVrsOkImages != null)
                                        {
                                            group.GroupInfos.Add(new GroupInfo()
                                            {
                                                ImagePath = $"{head}/{pcsInfo.DefectInfo[j].DefectVrsOkImages[0].ToString()}",
                                                ImageUuid = Guid.NewGuid().ToString(),
                                                ImageType = "template",
                                            });
                                        }
                                        break;
                                    case 2:
                                        if (pcsInfo.DefectInfo[j].DefectVrsGerberImages != null)
                                        {
                                            group.GroupInfos.Add(new GroupInfo()
                                            {
                                                ImagePath = $"{head}/{pcsInfo.DefectInfo[j].DefectVrsGerberImages[0].ToString()}",
                                                ImageUuid = Guid.NewGuid().ToString(),
                                                ImageType = "gerber",
                                            });
                                            //奥特斯
                                            dsDefectinfo.DefectGerberImages.Add($"http://{config.MinioConfig}/deepiresults/{head}/{pcsInfo.DefectInfo[j].DefectVrsGerberImages[0].ToString()}");
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            vBInfo.paramsData.InferWholeData.ImageData.DataValue.InferImageGroup.Add(group);
                            group.inspectDetails = new InspectDetails();
                            group.inspectDetails.InferRois = new List<InferRoi>() { };
                            defectList.Add(j);
                            pcsList.Add(pcsInfo.DefectInfo[j].PcsIndex);
                            item.DefectsInfo.Add(dsDefectinfo);
                        }
                    }
                    if (info.SideIndex == "A")
                    {
                        panelData.Content.Add((i + 1).ToString(), item);
                    }
                }
                vBInfo.paramsData.InferWholeData.OtherInfos = new Others();
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio = new ImageMminio();
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.access_key_id = "deepiobjectdata";
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.bucket = "deepiresults";
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.endpoint_url = minioip; //"192.168.77.243";//"127.0.0.1";
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.secret_key = "deepiobject2019";
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.secret_port = minioport;//"9102";

                if (info.SideIndex == "A")
                {
                    dsInfo.Data.Add(panelData);
                    dic_DsCenterInfo.TryAdd($"{info.LotId}_{info.SerialNumber}", dsInfo);
                    LogTextHelper.Info($"{info.LotId}_{info.SerialNumber}_在A面创建dsinfo成功");
                }
                else
                {
                    dic_DsCenterInfo[$"{info.LotId}_{info.SerialNumber}"] = dsInfo;
                    LogTextHelper.Info($"{info.LotId}_{info.SerialNumber}_在B面取得dsinfo成功");
                }
                return vBInfo;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                throw;
            }
        }
        public bool DefectMethod(RootVBInfo info, RootPanelInfo panelInfo, out List<string> resList, out List<string> detailsList, out PcsResult pcsResult, out string vbJson)
        {
            bool result = false;

            try
            {
                resList = new List<string>();
                pcsResult = new PcsResult();
                detailsList = new List<string>();
                vbJson = string.Empty;
                string msg = "";
                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };//去掉空值NULL
                string infoJson = JsonConvert.SerializeObject(info, Formatting.None, jsonSetting);
                //LogTextHelper.Info("准备调用算法,参数为：" + infoJson);

                defect.DefectMethod(info, out msg);

                //LogTextHelper.Info("算法返回推理结果：" + msg);
                //将RootVBOutInfo结果msg处理
                var obj = JsonConvert.DeserializeObject<RootVBOutInfo>(msg);
                string code = obj.Code.ToString();
                if (code != "200")
                {
                    return false;
                }
                JObject root = JObject.Parse(msg);
                if (root["data"]?["infer_whole_data"]?["infer_results"] is JArray inferResults)
                {
                    foreach (var result1 in inferResults)
                    {
                        JObject nodeDetails = result1["infer_details"]["node_details"] as JObject;

                        if (nodeDetails != null)
                        {
                            string nodeDetailsJson = nodeDetails.ToString();
                            detailsList.Add(nodeDetailsJson);
                        }
                    }
                }
                DsCenterInfo dsCenterInfo = null;
                if (dic_DsCenterInfo.TryGetValue($"{panelInfo.LotId}_{panelInfo.SerialNumber}", out dsCenterInfo))
                {
                    LogTextHelper.Info($"取出{panelInfo.LotId}_{panelInfo.SerialNumber}的中台数据，准备更新...");
                }

                if (code == "200")
                {
                    pcsResult.vb_List = new List<VBRcvInfp>();
                    string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //热力点对象
                    AVI_HeatPoints avi_HeatInfo = new AVI_HeatPoints();
                    avi_HeatInfo.SN = panelInfo.SerialNumber;
                    avi_HeatInfo.Side = panelInfo.SideIndex;

                    //这个有几个  就是几个报点图各自的结果，
                    for (int i = 0; i < obj.Data.InferWholeData.InferResults.Count; i++)
                    {
                        VBRcvInfp vBRcv = new VBRcvInfp();
                        vBRcv.bbox = new List<List<double>>();
                        dsCenterInfo.Data[0].Content["1"].DefectsCount++;

                        try
                        {
                            //更新中台数据
                            if (panelInfo.SideIndex == "A")
                            {
                                dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].AiResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower(); ;
                                dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].ManualResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                                dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].ManualDefectCode = obj.Data.InferWholeData.InferResults[i].Defect_name;
                                dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].DefectCode = obj.Data.InferWholeData.InferResults[i].Defect_name;
                            }
                            else
                            {
                                dsCenterInfo.Data[0].Content["1"].EndTime = time;
                                panelInfo.EndTime = time;
                                //因为上传中台数据A/B面的一个pcs信息在一个包，但A/B面处理是分开的；
                                //如果是B的话，先计算A面的报点数
                                int Bcount = panelInfo.PcsInfo["1"].DefectInfo.Count;
                                int ALLcount = dsCenterInfo.Data[0].Content["1"].DefectsInfo.Count;
                                int index = ALLcount - Bcount;
                                dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].AiResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                                dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].ManualResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                                dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].ManualDefectCode = obj.Data.InferWholeData.InferResults[i].Defect_name;
                                dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].DefectCode = obj.Data.InferWholeData.InferResults[i].Defect_name;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogTextHelper.Error("更新中台数据异常" + ex.ToString());
                        }

                        for (int j = 0; j < obj.Data.InferWholeData.InferResults[i].inferDetails.Location.Count; j++)
                        {
                            //奥特斯
                            DsCenterSubDefectInfo subDefectInfo = new DsCenterSubDefectInfo();
                            subDefectInfo.SubDefectArea = Convert.ToDouble(obj.Data.InferWholeData.InferResults[i].inferDetails.DefectArea);
                            string sub_defectName = obj.Data.InferWholeData.InferResults[i].Defect_name;
                            subDefectInfo.SubDefectCode = sub_defectName;

                            int defectX = 0;
                            int defectY = 0;
                            if (panelInfo.SideIndex == "A")
                            {
                                defectX = dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].DefectRoi.X;
                                defectY = dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].DefectRoi.Y;
                            }
                            else
                            {
                                int Bcount = panelInfo.PcsInfo["1"].DefectInfo.Count;
                                int ALLcount = dsCenterInfo.Data[0].Content["1"].DefectsInfo.Count;
                                int index = ALLcount - Bcount;
                                defectX = dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].DefectRoi.X;
                                defectY = dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].DefectRoi.Y;
                            }

                            int subX = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].X);
                            int subY = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Y);
                            int subH = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Height);
                            int subW = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Width);

                            subDefectInfo.SubDefectHeight = subH;//Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Height);
                            subDefectInfo.SubDefectWidth = subW;//Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].inferDetails.Location[j].Width);
                            subDefectInfo.SubDefectIndex = j;
                            subDefectInfo.SubDefectRoi.Add(subX);
                            subDefectInfo.SubDefectRoi.Add(subY);
                            subDefectInfo.SubDefectRoi.Add(subW);
                            subDefectInfo.SubDefectRoi.Add(subH);
                            //热力图参数  
                            int CenterPointX = defectX + subX / 2 + subW / 4;
                            int CenterPointY = defectY + subY / 2 + subH / 4;
                            subDefectInfo.CenterPoint.Add(CenterPointX);
                            subDefectInfo.CenterPoint.Add(CenterPointY);
                            //热力点
                            if (j == 0)
                            {
                                PointsInfo heatInfo = new PointsInfo();
                                heatInfo.DefectName = sub_defectName;
                                heatInfo.X = CenterPointX;
                                heatInfo.Y = CenterPointY;
                                int index = panelInfo.LocalDescribeDir.IndexOf("deepiresults", StringComparison.OrdinalIgnoreCase);
                                if (index == -1)
                                {
                                    Console.WriteLine("第一个路径中未找到 'deepiresults'");
                                }
                                // 截取到 "deepiresults" 所在目录的完整路径（包含自身）
                                string basePath = panelInfo.LocalDescribeDir.Substring(0, index + "deepiresults".Length);
                                // 将第二个路径的斜杠统一转换为Windows的反斜杠
                                string relativePath = obj.Data.InferWholeData.InferResults[i].GroupInfos[0].ImagePath.Replace('/', '\\');
                                // 合并路径
                                string mergedPath = Path.Combine(basePath, relativePath);
                                heatInfo.ImagePath = mergedPath;
                                avi_HeatInfo.pointsInfos.Add(heatInfo);
                                //这里在生产时根据缺陷名称将缺陷形态赋值,（点状与线状）
                                if (sub_defectName == "AU10" || sub_defectName == "CU10" || sub_defectName == "CU41"
                                    || sub_defectName == "HO01" || sub_defectName == "SM10")
                                {
                                    heatInfo.DefectShape = "dot";
                                }
                                else
                                {
                                    heatInfo.DefectShape = "line";
                                }
                            }

                            vBRcv.raw_bbox = new List<double>();
                            vBRcv.raw_bbox.Add(subX);
                            vBRcv.raw_bbox.Add(subY);
                            vBRcv.raw_bbox.Add(subW);
                            vBRcv.raw_bbox.Add(subH);
                            vBRcv.bbox.Add(vBRcv.raw_bbox);
                            vBRcv.sub_DefectNames.Add(sub_defectName);

                            //更新中台数据
                            if (panelInfo.SideIndex == "A")
                            {
                                //dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].AiResult= obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower(); ;
                                //dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].ManualResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                                //dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].ManualDefectCode= obj.Data.InferWholeData.InferResults[i].Defect_name;
                                dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].SubDefectsInfo.Add(subDefectInfo);
                            }
                            else
                            {
                                //因为上传中台数据A/B面的一个pcs信息在一个包，但A/B面处理是分开的；
                                //如果是B的话，先计算A面的报点数
                                int Bcount = panelInfo.PcsInfo["1"].DefectInfo.Count;
                                int ALLcount = dsCenterInfo.Data[0].Content["1"].DefectsInfo.Count;
                                int index = ALLcount - Bcount;
                                //dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].AiResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                                //dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].ManualResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                                //dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].ManualDefectCode = obj.Data.InferWholeData.InferResults[i].Defect_name;
                                dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].SubDefectsInfo.Add(subDefectInfo);
                            }
                        }

                        resList.Add(obj.Data.InferWholeData.InferResults[i].Infer_Result == "NG" ? "1" : "0");
                        pcsResult.vb_List.Add(vBRcv);
                    }

                    if (avi_HeatInfo.pointsInfos.Count > 0)
                    {
                        RootDbInfo Info = new RootDbInfo();
                        Info.db_name = "AVI_HeatPoints";
                        Info.operation = "put";
                        Info.op_mode = "all_ow";
                        //snInfo.key = key;
                        //snInfo.value = serialNumber.Split('_').ToArray()[0];
                        Info.key = $"{panelInfo.SerialNumber}_{panelInfo.SideIndex}";//.Split('_').ToArray()[0];
                        Info.value = JsonConvert.SerializeObject(avi_HeatInfo, Formatting.None, jsonSetting);
                        string Result;
                        //Task.Factory.StartNew(() =>
                        //{
                        http_DB.HttpPostMethod(URL, Info, 1, out Result);
                        LogTextHelper.Info($"HeatPoints:{avi_HeatInfo.pointsInfos.Count},SN:{avi_HeatInfo.SN},KEY:{Info.key}");
                    }

                    databaseHelper.SavePanelSide(new PanelSideRecord()
                    {
                        Data = new SideData()
                        {
                            HeatPoints = avi_HeatInfo.pointsInfos.Select(o => new HeatPoint()
                            {
                                DefectName = o.DefectName,
                                RoiX = o.X,
                                RoiY = o.Y,
                                DefectType = o.DefectName,
                                ImagePath = o.ImagePath,

                            }).ToList(),
                            RemainingDefectsCount = resList.Where(t => t == "1").Count(),
                            TotalDefectsCount = resList.Count()
                        },
                        DetectionDate = DateTime.Now,
                        LotNumber = panelInfo.LotId,
                        SerialNumber = panelInfo.SerialNumber,
                        MachineId = panelInfo.MachineName,
                        Side = panelInfo.SideIndex,
                        
                    });

                    //B面做完判断总结果
                    if (panelInfo.SideIndex == "B")
                    {
                        if (dsCenterInfo.Data[0].Content["1"].DefectsInfo.Exists(o => o.AiResult.ToLower() == "ng"))
                        {
                            dsCenterInfo.Data[0].Content["1"].ConfirmResult = "ng";
                        }
                        else
                        {
                            dsCenterInfo.Data[0].Content["1"].ConfirmResult = "ok";
                        }
                        dsCenterInfo.Data[0].EndTime = time;
                        dsCenterInfo.Data[0].Content["1"].EndTime = time;
                    }
                    vbJson = msg;
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                vbJson = string.Empty;
                resList = null;
                detailsList = null;
                result = false;
                pcsResult = null;
                SystemEvent.SendAlarmMsg("算法处理异常" + ex.ToString());
            }
            return result;
        }

        public async void showImage(string path, int index, string result = "", VBRcvInfp box = null)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    DisplaysList[index].Image = null;
                    DisplaysList[index].Clear();
                    return;
                }
                string[] str = path.Split(':').ToArray();
                using (var stream = minio.GetImageStreamSync("deepiresults", str[0], str[1]))
                {
                    if (stream.Length == 0)
                    {
                        Console.WriteLine("图片数据为空");
                        return;
                    }
                    Mat mt = Cv2.ImDecode(stream.ToArray(), ImreadModes.Color);

                    long len = mt.Total() * mt.ElemSize();
                    byte[] buf = new byte[len];
                    Marshal.Copy(mt.Data, buf, 0, (int)len);

                    if (isShowBox)
                    {
                        if (box != null)
                        {
                            List<string> content = new List<string>();
                            List<System.Drawing.Point> location = new List<System.Drawing.Point>();
                            for (int i = 0; i < box.bbox.Count(); i++)
                            {
                                Rect rect = new Rect((int)box.bbox[i][0], (int)box.bbox[i][1], (int)box.bbox[i][2], (int)box.bbox[i][3]);
                                location.Add(new System.Drawing.Point((int)box.bbox[i][0] + 10, (int)box.bbox[i][1] + 30));
                                mt.Rectangle(rect, Scalar.Red, 2);
                            }
                            content.AddRange(box.sub_DefectNames);
                            PutTextAll(ref mt, content.ToArray(), location.ToArray(), Color.Yellow, 24);
                        }
                    }
                    DisplaysList[index].Image = mt;
                    if (!string.IsNullOrEmpty(result))
                    {
                        int res;
                        if (int.TryParse(result, out res))
                        {
                            if (res == 1)
                            {
                                result = "NG";
                            }
                            else
                            {
                                result = "OK";
                            }
                        }
                        DisplaysList[index].DrawStatus($"AI结果:{result}");
                    }
                    else
                    {
                        DisplaysList[index].DrawStatus("");
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常(可能未找到Minio路径图像),Index为" + index.ToString()+"\n"+ex.ToString());
            }
        }


        public async void showImage2(int index, List<string> paths, string result = "", VBRcvInfp box = null)
        {
            Mat[] mats = new Mat[paths.Count];
            try
            {
                if (paths.Count <= 0)
                {
                    DisplaysList2[index].Image = null;
                    DisplaysList2[index].Clear();
                    return;
                }
                if (paths.Count == 1)
                {
                    string[] str = paths[0].Split(':').ToArray();
                    using (var stream = minio.GetImageStreamSync("deepiresults", str[0], str[1]))
                    {
                        if (stream.Length == 0)
                        {
                            Console.WriteLine("图片数据为空");
                            return;
                        }
                        Mat mt = Cv2.ImDecode(stream.ToArray(), ImreadModes.Color);

                        long len = mt.Total() * mt.ElemSize();
                        byte[] buf = new byte[len];
                        Marshal.Copy(mt.Data, buf, 0, (int)len);
                        //20250821 奥特斯在查询时显示结果
                        if (box != null)
                        {
                            List<string> content = new List<string>();
                            List<System.Drawing.Point> location = new List<System.Drawing.Point>();
                            for (int i = 0; i < box.bbox.Count(); i++)
                            {
                                Rect rect = new Rect((int)box.bbox[i][0], (int)box.bbox[i][1], (int)box.bbox[i][2], (int)box.bbox[i][3]);
                                mt.Rectangle(rect, Scalar.Red, 2);
                            }
                            PutTextAll(ref mt, content.ToArray(), location.ToArray(), Color.Yellow, 30);
                        }
                        DisplaysList2[index].Image = mt;
                        if (!string.IsNullOrEmpty(result))
                        {
                            int res;
                            if (int.TryParse(result, out res))
                            {
                                if (res == 1)
                                {
                                    result = "NG";
                                }
                                else
                                {
                                    result = "OK";
                                }
                            }
                            DisplaysList2[index].DrawStatus($"AI结果:{result}");
                        }
                        else
                        {
                            DisplaysList2[index].DrawStatus("");
                        }
                    }
                }
                if (paths.Count > 1)//拼接显示
                {
                    for (int i = 0; i < paths.Count; i++)
                    {
                        string[] str = paths[i].Split(':').ToArray();
                        using (var stream = minio.GetImageStreamSync("deepiresults", str[0], str[1]))
                        {

                            if (stream.Length == 0)
                            {
                                Console.WriteLine("图片数据为空");
                                return;
                            }
                            mats[i] = Cv2.ImDecode(stream.ToArray(), ImreadModes.Color);
                            long len = mats[i].Total() * mats[i].ElemSize();
                            byte[] buf = new byte[len];
                            Marshal.Copy(mats[i].Data, buf, 0, (int)len);
                            if (i == 0)
                            {
                                if (box != null)
                                {
                                    List<string> content = new List<string>();
                                    List<System.Drawing.Point> location = new List<System.Drawing.Point>();
                                    for (int j = 0; j < box.bbox.Count(); j++)
                                    {
                                        Rect rect = new Rect((int)box.bbox[j][0], (int)box.bbox[j][1], (int)box.bbox[j][2], (int)box.bbox[j][3]);
                                        mats[i].Rectangle(rect, Scalar.Red, 2);
                                    }
                                    PutTextAll(ref mats[i], content.ToArray(), location.ToArray(), Color.Yellow, 30);
                                }
                            }
                        }
                    }
                    Mat mt = new Mat();
                    Cv2.HConcat(mats, mt);
                    DisplaysList2[index].Image = mt;
                    if (!string.IsNullOrEmpty(result))
                    {
                        int res;
                        if (int.TryParse(result, out res))
                        {
                            if (res == 1)
                            {
                                result = "NG";
                            }
                            else
                            {
                                result = "OK";
                            }
                        }
                        DisplaysList2[index].DrawStatus($"AI结果:{result}");
                    }
                    else
                    {
                        DisplaysList2[index].DrawStatus("");
                    }
                }

            }
            catch (Exception ex)
            {
                Mat mt = new Mat();
                List<Mat> mts_list = new List<Mat>();
                for (int i = 0; i < mats.Count(); i++)
                {
                    if (mats[i] != null)
                    {
                        mts_list.Add(mats[i]);
                    }
                }
                Cv2.HConcat(mts_list.ToArray(), mt);
                DisplaysList2[index].Image = mt;
                LogTextHelper.Error("异常(可能未找到Minio路径图像),Index为" + index.ToString() + "\n" + ex.ToString());
            }
        }
        public void PutTextAll(ref Mat mat, string[] content, System.Drawing.Point[] location,
        Color color, float fontSzie = 8, string familyName = "宋体")
        {
            try
            {
                using (Bitmap bit = mat.ToBitmap())
                {
                    using (Image tempImg = (Image)bit)
                    {
                        for (int i = 0; i < content.Length; i++)
                        {
                            DrawString(tempImg, content[i], location[i], color, fontSzie, familyName);
                        }
                        var tempMat = ToMat(tempImg);
                        tempMat.CopyTo(mat);
                        tempMat.Dispose();
                        //2024/1/7
                        tempImg.Dispose();
                        bit.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        public void DrawString(Image image, string content, System.Drawing.Point location,
            Color color, float fontSzie = 10, string familyName = "宋体")
        {
            try
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    Font font = new Font(familyName, fontSzie, FontStyle.Regular, GraphicsUnit.Pixel);
                    g.DrawString(content, font, new SolidBrush(color), location);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
            }
        }

        public Mat ToMat(Image image)
        {
            try
            {
                return image == null ? null : Cv2.ImDecode(ToBinary(image), ImreadModes.Color);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                return new Mat();
            }
        }

        public byte[] ToBinary(Image image)
        {
            try
            {
                if (image == null)
                    return new byte[0];
                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Bmp);
                    BinaryReader reader = new BinaryReader(stream);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                return new MemoryStream().ToArray();
            }
        }

        #region test

        /// <summary>
        /// 用于测试数据库读写功能的方法
        /// </summary>
        public void TestDatabaseReadWrite()
        {
            try
            {
                LogTextHelper.Info("--- 开始数据库读写测试 ---");

                var testDate = DateTime.Now;
                string machineId = "TestMachine-01";
                string sn = $"TestSN-{Guid.NewGuid().ToString().Substring(0, 8)}";
                string lot = "TestLot-123";

                // 1. 准备测试数据并写入数据库
                LogTextHelper.Info($"准备写入数据: SN={sn}, Lot={lot}");

                // A面数据: 10个总缺陷, 0个剩余缺陷 (AI OK)
                var recordA = new PanelSideRecord
                {
                    MachineId = machineId,
                    DetectionDate = testDate,
                    SerialNumber = sn,
                    LotNumber = lot,
                    Side = "A",
                    Data = new SideData
                    {
                        TotalDefectsCount = 10,
                        RemainingDefectsCount = 0,
                        HeatPoints = new List<HeatPoint>
                        {
                            new HeatPoint { DefectName = "Scratch", RoiX = 100, RoiY = 150 },
                            new HeatPoint { DefectName = "Open", RoiX = 200, RoiY = 250 }
                        }
                    }
                };
                databaseHelper.SavePanelSide(recordA);
                LogTextHelper.Info("A面数据写入成功。");

                // B面数据: 8个总缺陷, 0个剩余缺陷 (AI OK)
                var recordB = new PanelSideRecord
                {
                    MachineId = machineId,
                    DetectionDate = testDate,
                    SerialNumber = sn,
                    LotNumber = lot,
                    Side = "B",
                    Data = new SideData
                    {
                        TotalDefectsCount = 8,
                        RemainingDefectsCount = 0,
                        HeatPoints = new List<HeatPoint>
                        {
                            new HeatPoint { DefectName = "Short", RoiX = 300, RoiY = 350 }
                        }
                    }
                };
                databaseHelper.SavePanelSide(recordB);
                LogTextHelper.Info("B面数据写入成功，面板 IsAIOk 状态已更新。");

                // 2. 执行读取测试
                LogTextHelper.Info("--- 开始读取验证 ---");

                // 查询逻辑 1: 根据 lot 号获取所有 sn
                var sns = databaseHelper.GetSerialNumbersByLot(lot);
                LogTextHelper.Info($"[查询1] Lot '{lot}' 包含的SN: {string.Join(", ", sns)}. (应包含 {sn})");

                // 查询逻辑 2: 根据时间和 sn 获取所有 heatpoint 信息
                var heatpoints = databaseHelper.GetHeatPoints(sn, testDate);
                LogTextHelper.Info($"[查询2] SN '{sn}' 在日期 '{testDate.ToShortDateString()}' 的HeatPoints数量: {heatpoints.Count}. (应为 3)");

                // 查询逻辑 3 & 5: 获取机台在时间段内的板数统计
                var boardCounts = databaseHelper.GetBoardCounts(testDate.Date, testDate.Date.AddDays(1), machineId);
                LogTextHelper.Info($"[查询3/5] 机台 '{machineId}' 在今天总板数: {boardCounts.TotalBoards}, AI OK 板数: {boardCounts.AIOkBoards}.");

                // 查询逻辑 4 & 6: 获取机台在时间段内的报点数统计
                var defectCounts = databaseHelper.GetDefectCounts(testDate.Date, testDate.Date.AddDays(1), machineId);
                LogTextHelper.Info($"[查询4/6] 机台 '{machineId}' 在今天总报点数: {defectCounts.TotalDefects}, AI OK 报点数: {defectCounts.AIOkDefects}. (此面板贡献18个)");

                LogTextHelper.Info("--- 数据库读写测试结束 ---");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("数据库测试时发生异常", ex);
            }
        }
        public void TestDatabaseWrite()
        {
            DatabaseHelper.GenerateTestData();
        }
        public Dictionary<string, (long TotalDefects, long AIOkDefects)> GetDefectCountsPerMachine(DateTime start,DateTime end)=>
            databaseHelper.GetDefectCountsPerMachine(start,end);

        #endregion
        /// <summary>
        /// 开始线程
        /// </summary>
        private void start_ReturnAVI()
        {
            try
            {
                if (th_ReturnAVI != null)
                {
                    stop_ReturnAVI();
                }
                th_ReturnAVI = new Thread(new ThreadStart(ThreadReturnAVI))
                {
                    IsBackground = true
                };
                _shouldStop_ReturnAVI = false;
                th_ReturnAVI.Start();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 停止线程
        /// </summary>
        private void stop_ReturnAVI()
        {
            try
            {
                if (th_ReturnAVI != null)
                {
                    _shouldStop_ReturnAVI = true;
                    while (th_ReturnAVI.IsAlive)
                    {
                        Thread.Sleep(1);
                    }
                    th_ReturnAVI.Abort();
                    th_ReturnAVI = null;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 线程处理
        /// </summary>
        private void ThreadReturnAVI()
        {
            while (!_shouldStop_ReturnAVI)
            {
                Thread.Sleep(15);
                try
                {
                    if (que_AI.Count > 0)
                    {
                        Tuple<string, string, string, RootAIResult> info = null;
                        if (que_AI.TryDequeue(out info))
                        {
                            //回写处理
                            ReturnAVI(info);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error(ex.ToString());
                }
            }
        }
        public bool ReturnAVI(Tuple<string, string, string, RootAIResult> info)
        {
            try
            {
                string result = "";
                SystemEvent.SendTaskMsg(info.Item2, $"{info.Item3}面已完成");
                if (http_DB.HttpPostMethod(URL, info.Item4, 1, out result))
                {
                    //返回更新界面信息
                    SystemEvent.SendTaskMsg(info.Item2, $"{info.Item3}面已完成");

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                return false;
            }
        }
        #region 释放资源
        public void Dispose()
        {
            try
            {
                if (th_ReadAVI.IsAlive)
                {
                    th_ReadAVI.Abort();
                }
                if (th_ReturnAVI.IsAlive)
                {
                    th_ReturnAVI.Abort();
                }
                if (th_Defect.IsAlive)
                {
                    th_Defect.Abort();
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        #endregion
    }
}

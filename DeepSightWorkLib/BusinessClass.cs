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

        //读取AVI存储对象 <Key,SN,Side,DefectIndex,VBInfo>
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
            //工作线程 -> 使用Task管理
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            _readAviTask = Task.Run(() => ThreadReadAVI(token), token);
            _defectTask = Task.Run(() => ThreadDefect(token), token);
            _returnAviTask = Task.Run(() => ThreadReturnAVI(token), token);
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

        // Task-based management
        private CancellationTokenSource _cancellationTokenSource;
        private Task _readAviTask;
        private Task _defectTask;
        private Task _returnAviTask;

        /// <summary>
        /// 开始线程 (保留旧API但不再使用)
        /// </summary>
        private void start_readAVI()
        {
            try
            {
                if (th_ReadAVI != null)
                {
                    stop_readAVI();
                }
                th_ReadAVI = new Thread(new ThreadStart(() => ThreadReadAVI(CancellationToken.None)))
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
        /// 停止线程 (保留旧API)
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
        /// 线程处理 (Task循环)
        /// </summary>
        private void ThreadReadAVI(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Task.Delay(1000, token).Wait(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    if (!isStart || TestFlag || !IsAllow)
                    {
                        continue;
                    }
                    //读取AVI数据
                    if (ReadAVI(URL, out string result))
                    {
                        doAviJson(result);
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
                            ParseMinioPath(result_path, out string path, out string result);
                            LogTextHelper.Info($"SN:{serialNumber} 解析Minio路径完成");
                            ReadJsonByMinio(minio_ip, minio_port, key, result, serialNumber, side, path);
                            LogTextHelper.Info($"SN:{serialNumber} 通过Minio读取Json完成");
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
        /// 开始线程 (保留旧API但不再使用)
        /// </summary>
        private void start_Defect()
        {
            try
            {
                if (th_Defect != null)
                {
                    stop_Defect();
                }
                th_Defect = new Thread(new ThreadStart(() => ThreadDefect(CancellationToken.None)))
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
        /// 停止线程 (保留旧API)
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
        /// 线程处理 (Task循环)
        /// </summary>
        private void ThreadDefect(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Task.Delay(15, token).Wait(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

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

                            if (DefectMethod(info, out msg, out details, out pcsResult, out vbJson))
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
                                    dbInfo.key = $"{info.panelInfo.StationName}";
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
                if (!File.Exists($"D:\\ATS_AI_INSTALL\\TemplateImages\\{obj.ProductSerial}\\{obj.ProductSerial}[{obj.SideIndex}].jpg"))
                {
                    model.isByPass = true;
                }
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
                var solutionFlow = solconfig.solus.FirstOrDefault(o => o.ProductSerial == info.ProductSerial);
                if (solutionFlow != null)
                {
                    if (info.SideIndex == "A")
                    {
                        solution = solutionFlow.Asolution;
                        flow = solutionFlow.Aflow;
                    }
                    else
                    {
                        solution = solutionFlow.Bsolution;
                        flow = solutionFlow.Bflow;
                    }
                    isSwitch = solutionFlow.IsSwitch;
                }
                else
                {
                    var defaultSolutionFlow = solconfig.solus.FirstOrDefault(o => o.ProductSerial.ToUpper() == "DEFAULT");
                    if (defaultSolutionFlow != null)
                    {
                        if (info.SideIndex == "A")
                        {
                            solution = defaultSolutionFlow.Asolution;
                            flow = defaultSolutionFlow.Aflow;
                        }
                        else
                        {
                            solution = defaultSolutionFlow.Bsolution;
                            flow = defaultSolutionFlow.Bflow;
                        }
                        isSwitch = defaultSolutionFlow.IsSwitch;
                    }
                    else
                    {
                        solution = "0729";
                        flow = "0729";
                        isSwitch = false;
                    }
                }

                DsCenterInfo dsInfo = new DsCenterInfo();
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
                vBInfo.paramsData.InferResUuid = Guid.NewGuid().ToString();
                vBInfo.paramsData.InferWholeData = new InferWholeData();
                vBInfo.paramsData.InferWholeData.ImageInferParams = new ImageInferParams();
                vBInfo.paramsData.InferWholeData.ImageInferParams.PipelineName = solution;
                vBInfo.paramsData.InferWholeData.ImageInferParams.NodeParams = new List<NodeParam>();
                vBInfo.paramsData.InferWholeData.ImageInferParams.NodeParams.Add(
                  new NodeParam()
                  {
                      NodeName = flow,
                      height = 200,
                      width = 200,
                  });

                vBInfo.paramsData.InferWholeData.ImageData = new ImageData();
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

                    // info的PcsInfo在ATS只有一条，做其他项目时要注意
                    if (info.PcsInfo.TryGetValue((i + 1).ToString(), out pcsInfo))
                    {
                        LogTextHelper.Info($"SN:{info.SerialNumber}_{info.SideIndex}面报点数据为:{pcsInfo.DefectInfo.Count}");  

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
                            group.MachineTemplateInfo = new MachineTemplateInfo()
                            {
                                MachineName = info.StationName,
                                product = info.ProductSerial,
                                Side = info.SideIndex,
                            };
                            group.GroupUuid = Guid.NewGuid().ToString();
                            group.GroupInfos = new List<GroupInfo>();
                            group.DefectCode = "";
                            group.TempImgPath = $"D:\\ATS_AI_INSTALL\\TemplateImages\\{info.ProductSerial}\\{info.ProductSerial}[{info.SideIndex}].jpg";
                            group.ImgROI = new List<int>();
                            group.ImgROI.Add(pcsInfo.DefectInfo[j].DefectRoi.X);
                            group.ImgROI.Add(pcsInfo.DefectInfo[j].DefectRoi.Y);
                            group.ImgROI.Add(pcsInfo.DefectInfo[j].DefectRoi.Width);
                            group.ImgROI.Add(pcsInfo.DefectInfo[j].DefectRoi.Height);
                            if (isSwitch)
                            {
                                group.DefectCode = pcsInfo.DefectInfo[j].DefectCode;
                            }
                            WatchPathConfig config = aviconfig.WatchPaths.FirstOrDefault(o => o.AviName == info.StationName);
                            if (config!=null)
                            {
                                void AddGroupInfo(List<string> images, string imageType, Action<string> addUrlAction = null)
                                {
                                    if (images != null && images.Count > 0)
                                    {
                                        var imagePath = $"{head}/{images[0]}";
                                        group.GroupInfos.Add(new GroupInfo()
                                        {
                                            ImagePath = imagePath,
                                            ImageUuid = Guid.NewGuid().ToString(),
                                            ImageType = imageType,
                                        });
                                        addUrlAction?.Invoke($"http://{config.MinioConfig}/deepiresults/{imagePath}");
                                    }
                                }
                                AddGroupInfo(pcsInfo.DefectInfo[j].DefectVrsImages, "defect", url => dsDefectinfo.DefectImages.Add(url));
                                AddGroupInfo(pcsInfo.DefectInfo[j].DefectVrsOkImages, "template");
                                AddGroupInfo(pcsInfo.DefectInfo[j].DefectVrsGerberImages, "gerber", url => dsDefectinfo.DefectGerberImages.Add(url));
                            }
                            else
                            {
                                LogTextHelper.Error($"{pcsInfo.PcsSerialNumber}:panel的machineID:{info.StationName} 未找到对应机台的machineID");
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
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.endpoint_url = minioip;
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.secret_key = "deepiobject2019";
                vBInfo.paramsData.InferWholeData.OtherInfos.imageminio.secret_port = minioport;

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
        public bool DefectMethod(VBModel vBModel, out List<string> resList, out List<string> detailsList, out PcsResult pcsResult, out string vbJson)
        {
            bool result = false;
            RootVBInfo info = vBModel.VbInfo;
            RootPanelInfo panelInfo = vBModel.panelInfo;
            try
            {
                resList = new List<string>();
                pcsResult = new PcsResult();
                detailsList = new List<string>();
                vbJson = string.Empty;
                string msg = "";
                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };//去掉空值NULL
                string infoJson = JsonConvert.SerializeObject(info, Formatting.None, jsonSetting);
                LogTextHelper.Info("准备调用算法,参数为：" + infoJson);

                defect.DefectMethod(info, out msg);

                LogTextHelper.Info($"算法返回原始结果 for Side {panelInfo.SideIndex}: {msg}"); // <-- 增加此行日志
                //将RootVBOutInfo结果msg处理
                var obj = JsonConvert.DeserializeObject<RootVBOutInfo>(msg);
                string code = obj.Code.ToString();
                string message = obj.Message.ToString();
                if (code != "200")
                {
                    LogTextHelper.Warn($"算法调用失败 for Side {panelInfo.SideIndex}，返回码: {code}，返回信息：{message}"); // <-- 增加此行日志
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
                    List<HeatPoint> avi_HeatInfo = new List<HeatPoint>();

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
                            //热力点参数  
                            int CenterPointX = defectX + subX / 2 + subW / 4;
                            int CenterPointY = defectY + subY / 2 + subH / 4;
                            subDefectInfo.CenterPoint.Add(CenterPointX);
                            subDefectInfo.CenterPoint.Add(CenterPointY);
                            //热力点
                            if (j == 0)
                            {
                                HeatPoint heatInfo = new HeatPoint();
                                heatInfo.DefectName = sub_defectName;
                                heatInfo.RoiX = CenterPointX;
                                heatInfo.RoiY = CenterPointY;
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
                                avi_HeatInfo.Add(heatInfo);
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
                        // 0为OK 1为NG 2为bypass
                        if (vBModel.isByPass)
                        {
                            resList.Add("2");
                        }
                        else
                        {
                            resList.Add(obj.Data.InferWholeData.InferResults[i].Infer_Result == "NG" ? "1" : "0");
                        }
                        pcsResult.vb_List.Add(vBRcv);
                    }

                    DateTime detectionDate;
                    if (!DateTime.TryParse(panelInfo.AviCreateTime, out detectionDate))
                    {
                        detectionDate = DateTime.Now;
                        LogTextHelper.Info($"无法解析 AviCreateTime '{panelInfo.AviCreateTime}'。将使用当前时间 '{detectionDate}' 作为备用。");
                    }
                    //存数据到db 加一个bypass数量
                    LogTextHelper.Info("Reslist:"+   string.Join(", ", resList));
                    LogTextHelper.Info($"存储{panelInfo.SerialNumber} PanelSide数据到数据库...");
                    databaseHelper.SavePanelSide(new PanelSideRecord()
                    {
                        Data = new SideData()
                        {
                            HeatPoints = avi_HeatInfo,
                            AviState = resList.Count == 0 ? 1 : 2,
                            AiState=resList.Contains("2")?3: resList.Contains("1")?2:1,
                            RemainingDefectsCount = resList.Where(t => t == "1").Count(),
                            TotalDefectsCount = resList.Where(t => t == "1" || t == "0").Count()
                        },
                        ProductSerial = panelInfo.ProductSerial,
                        DetectionDate = DateTime.Now,
                        AviCreationTime = detectionDate,
                        LotNumber = panelInfo.LotId,
                        SerialNumber = panelInfo.SerialNumber,
                        MachineId = panelInfo.StationName,
                        Side = panelInfo.SideIndex,
                        PathIndex = panelInfo.PathIndex

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

        public void showImage(string path, int index, string result = "", VBRcvInfp box = null)
        {
            Task.Run(() =>
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
                                switch (res)
                                {
                                    case 0:
                                        result = "OK";
                                        break;
                                    case 1:
                                        result = "NG";
                                        break;
                                    case 2:
                                        result = "ByPass";
                                        break;
                                    default:
                                        break;
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
                    LogTextHelper.Error("异常(可能未找到Minio路径图像),Index为" + index.ToString() + "\n" + ex.ToString());
                }
            });
        }


        public void showImage2(int index, List<string> paths, string result = "", VBRcvInfp box = null)
        {
            Task.Run(() =>
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
                                    switch (res)
                                    {
                                        case 0:
                                            result = "OK";
                                            break;
                                        case 1:
                                            result = "NG";
                                            break;
                                        case 2:
                                            result = "ByPass";
                                            break;
                                        default:
                                            break;
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
                                switch (res)
                                {
                                    case 0:
                                        result = "OK";
                                        break;
                                    case 1:
                                        result = "NG";
                                        break;
                                    case 2:
                                        result = "ByPass";
                                        break;
                                    default:
                                        break;
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
            });
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

        #region 数据库操作

        /// <summary>
        /// 用于测试数据库读写功能的方法
        /// </summary>
        public void TestDatabaseWrite()
        {
            DatabaseHelper.GenerateTestData();
        }
        public Task< Dictionary<string, (long TotalDefects, long AIOkDefects)>> GetDefectCountsPerMachine(DateTime start,DateTime end)=>
            databaseHelper.GetDefectCountsPerMachine(start,end);
        public void GenerateVRSTestData()=>DatabaseHelper.GenerateEmployeeReportTestData(5000);

        public Task< (string SerialNumber, string LotNumber,string ProductSerial,string PathIndex)> GetLatestPanelInfoByMachineId(string machineId)=>
            databaseHelper.GetLatestPanelInfoByMachineId(machineId);

        public void SaveEmployeeReport(EmployeeReport report)=>
            databaseHelper.SaveEmployeeReport(report);

        public Task< (int totalSnCount, int uninspectedCount, int stillNgCount, int aviOkCount, int filteredOkCount)> GetSnStateCountsByLot(string lotNumber) =>
            databaseHelper.GetSnStateCountsByLot(lotNumber);

        public Task< List<PanelDataRecord> >GetPanelsData(DateTime start, DateTime end,string partnumber=null)=>
            databaseHelper.GetPanelsData(start, end, partnumber);

        public Task<(string LotNumber, string ProductSerial)> GetLatestLotAndProductSerial(string machineId)=>
             databaseHelper.GetLatestLotAndProductSerial(machineId);

        public Task<List<PanelDataRecord>> GetPanelsDataByMachineAndLot(string machineId, string lotNumber)=>
            databaseHelper.GetPanelsDataByMachineAndLot(machineId, lotNumber);
        public Task<List<string>> GetSerialNumbersByLot(string lotNumber) =>
            databaseHelper.GetSerialNumbersByLot(lotNumber);
        #endregion
        /// <summary>
        /// 开始线程 (保留旧API但不再使用)
        /// </summary>
        private void start_ReturnAVI()
        {
            try
            {
                if (th_ReturnAVI != null)
                {
                    stop_ReturnAVI();
                }
                th_ReturnAVI = new Thread(new ThreadStart(() => ThreadReturnAVI(CancellationToken.None)))
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
        /// 停止线程 (保留旧API)
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
        /// 线程处理 (Task循环)
        /// </summary>
        private void ThreadReturnAVI(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Task.Delay(15, token).Wait(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

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
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                    try
                    {
                        Task.WaitAll(new[] { _readAviTask, _defectTask, _returnAviTask }, 5000);
                    }
                    catch (AggregateException)
                    {
                        // 忽略因取消导致的任务异常
                    }
                    _cancellationTokenSource.Dispose();
                }

                // 兼容旧线程释放
                if (th_ReadAVI != null && th_ReadAVI.IsAlive)
                {
                    th_ReadAVI.Abort();
                }
                if (th_ReturnAVI != null && th_ReturnAVI.IsAlive)
                {
                    th_ReturnAVI.Abort();
                }
                if (th_Defect != null && th_Defect.IsAlive)
                {
                    th_Defect.Abort();
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Dispose an exception occurred during task cancellation:" + ex.ToString());
            }

        }
        #endregion
    }
}

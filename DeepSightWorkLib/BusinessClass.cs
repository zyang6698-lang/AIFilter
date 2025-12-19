using DeepSightCommunication;
using DeepSightDB;
using DeepSightDisplay;
using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using Minio;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using DeepSightWorkLib.Services;

namespace DeepSightWorkLib
{
    //后续考虑是否设计为抽象，支持传统及AI调用
    public class BusinessClass : IDisposable
    {
        #region 私有字段

        private readonly DatabaseHelper _databaseHelper;
        private const int PanelSideBatchSize = 100;
        private readonly object _panelRecordLock = new object();
        private readonly List<PanelSideRecord> _pendingPanelSideRecords = new List<PanelSideRecord>();

        /// <summary>
        /// 读取 AVI 存储对象队列
        /// </summary>
        private readonly ConcurrentQueue<VBModel> _aviQueue = new ConcurrentQueue<VBModel>();

        /// <summary>
        /// 算法处理结果存储对象队列 (Key, SN, Side, RootAIResult, DsCenterInfo)
        /// </summary>
        private readonly ConcurrentQueue<Tuple<string, string, string, RootAIResult, DsCenterInfo>> _aiResultQueue = new ConcurrentQueue<Tuple<string, string, string, RootAIResult, DsCenterInfo>>();

        /// <summary>
        /// 图片加载队列（解耦图片读取和推理）
        /// </summary>
        private readonly ConcurrentQueue<ImageLoadModel> _imageLoadQueue = new ConcurrentQueue<ImageLoadModel>();

        /// <summary>
        /// 推理后处理队列（异步处理推理结果）
        /// </summary>
        private readonly ConcurrentQueue<InferenceResultModel> _inferencePostProcessQueue = new ConcurrentQueue<InferenceResultModel>();

        /// <summary>
        /// 奥特斯项目上传中台数据字典
        /// </summary>
        private readonly ConcurrentDictionary<string, DsCenterInfo> _dsCenterInfoDict = new ConcurrentDictionary<string, DsCenterInfo>();

        /// <summary>
        /// 正在处理的SN+Side集合（防止重复检测）
        /// Key格式："{SerialNumber}_{Side}"
        /// Value：入队时间戳
        /// </summary>
        private readonly ConcurrentDictionary<string, DateTime> _processingSnSet = new ConcurrentDictionary<string, DateTime>();

        /// <summary>
        /// 图像显示服务
        /// </summary>
        private ImageDisplayService ImageDisplay { get; set; }
        // services
        private AviReaderService _aviReaderService;
        private ImageLoaderService _imageLoaderService;
        private DefectProcessor _defectProcessor;
        private ResultWriterService _resultWriterService;
        private PostProcessService _postProcessService;

        /// <summary>
        /// 缓存过期时间（分钟），超过此时间自动清理，防止内存泄漏
        /// </summary>
        private const int CACHE_EXPIRE_MINUTES = 30;
        /// <summary>
        /// 开始/停止作业标志（内部字段）
        /// </summary>
        private volatile bool _isStart = false;
        private Stopwatch AIStopwatch;

        // Task-based management（线程管理）
        private CancellationTokenSource _cancellationTokenSource;
        private Task _readAviTask;
        private Task _defectTask;
        private Task _returnAviTask;
        private Task _imageLoadTask;
        private Task _postProcessTask;
        private Task _cleanupTask;

        #endregion

        #region 公共属性

        /// <summary>
        /// 算法检测对象
        /// </summary>
        public DefectClass Defect { get; private set; }

        /// <summary>
        /// LevelDB 交互服务
        /// </summary>
        public HttpClass HttpDb { get; private set; }

        /// <summary>
        /// Minio 对象存储服务
        /// </summary>
        public MinioClass Minio { get; private set; }

        /// <summary>
        /// 开始/停止作业标志
        /// </summary>
        public bool IsStart
        {
            get => _isStart;
            set
            {
                bool previousValue = _isStart;
                _isStart = value;

                // 当从 true 变为 false 时，清空所有正在处理的队列
                if (previousValue && !value)
                {
                    ClearAllProcessingQueues();
                    FlushPendingPanelSideRecords();
                }
                // 当从 false 变为 true 时，更新推理请求开始时间
                if (!previousValue && value)
                {
                   _aviReaderService.fetchTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// 是否显示检测框（同步到 ImageDisplayService）
        /// </summary>
        public bool IsShowBox
        {
            get => ImageDisplay?.IsShowBox ?? false;
            set
            {
                if (ImageDisplay != null)
                {
                    ImageDisplay.IsShowBox = value;
                }
            }
        }

        /// <summary>
        /// 是否允许处理
        /// </summary>
        public bool IsAllow { get; set; } = true;

        /// <summary>
        /// LevelDB 服务 URL
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// 产品料号
        /// </summary>
        public string ProductSerial { get; set; } = "";

        /// <summary>
        /// 当前方案名称
        /// </summary>
        public string Solution { get; set; } = "";

        /// <summary>
        /// 当前流程名称
        /// </summary>
        public string Flow { get; set; } = "";

        /// <summary>
        /// 是否切换模式
        /// </summary>
        public bool IsSwitch { get; set; } = false;

        /// <summary>
        /// 方案配置
        /// </summary>
        public SolutionConfig SolConfig { get; set; } = new SolutionConfig();

        /// <summary>
        /// AVI 配置
        /// </summary>
        public AVIConfig AviConfig { get; set; } = new AVIConfig();

        /// <summary>
        /// 系统配置
        /// </summary>
        public ConfigurationClass SysConfig { get; set; } = new ConfigurationClass();

        #endregion

        #region 兼容性属性 (已弃用，保持向后兼容)

        [Obsolete("请使用 Defect 属性")]
        public DefectClass defect { get => Defect; set => Defect = value; }

        [Obsolete("请使用 IsStart 属性")]
        public bool isStart { get => IsStart; set => IsStart = value; }

        [Obsolete("请使用 IsShowBox 属性")]
        public bool isShowBox { get => IsShowBox; set => IsShowBox = value; }

        /// <summary>
        /// 小图显示集合
        /// </summary>
        public List<CvDisplay> DisplaysList { get; set; }

        #endregion

        #region 构造函数和初始化

        public BusinessClass()
        {
            Defect = new DefectClass();
            HttpDb = new HttpClass();
            Minio = new MinioClass();
            ImageDisplay = new ImageDisplayService(Minio);
            // 先初始化数据库（确保数据库和表存在），然后再创建 DatabaseHelper 实例
            DatabaseHelper.InitializeDatabase();
            _databaseHelper = new DatabaseHelper();
            // 初始化拆分后的服务
            _aviReaderService = new AviReaderService(HttpDb, _processingSnSet, ReadJsonByMinio);
            _imageLoaderService = new ImageLoaderService(Minio);
            _defectProcessor = new DefectProcessor(Defect, ImageDisplay, _imageLoaderService, _aviQueue, _aiResultQueue, _inferencePostProcessQueue);
            // Post-process service handles inference result parsing and DB save, decoupled from BusinessClass
            _postProcessService = new PostProcessService(_dsCenterInfoDict, SysConfig, SavePanelSideToDatabase);
        }

        /// <summary>
        /// 初始化工作线程
        /// </summary>
        /// <param name="url">LevelDB 服务 URL</param>
        /// <param name="index">游标索引</param>
        public void InitWork(string url)
        {
            this.URL = url;
            this.IsStart = false;

            // 创建 ResultWriter 需要 URL
            _resultWriterService = new ResultWriterService(HttpDb, _processingSnSet, URL,SysConfig.DsCenterUrl);

            // 工作线程 -> 使用 Task 管理
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            _readAviTask = Task.Run(() => ThreadReadAVI(token), token);
            _imageLoadTask = Task.Run(() => ThreadImageLoad(token), token);
            _defectTask = Task.Run(() => ThreadDefect(token), token);
            _returnAviTask = Task.Run(() => ThreadReturnAVI(token), token);
            _postProcessTask = Task.Run(() => ThreadPostProcess(token), token);
            AIStopwatch = new Stopwatch();
            // ⭐ 关键修改点7：启动清理线程
            _cleanupTask = Task.Factory.StartNew(() => ThreadCleanupProcessingCache(token),
                token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            LogTextHelper.Info("BusinessClass 初始化完成，所有线程已启动");
        }

        /// <summary>
        /// 设置显示窗口列表（委托给 ImageDisplayService）
        /// </summary>
        public void SetHWindow(List<CvDisplay> displaysList)
        {
            ImageDisplay.SetDisplayList(displaysList);
            DisplaysList = ImageDisplay.DisplaysList;
        }

        #region 兼容性方法（已弃用）

        [Obsolete("请使用 ImageDisplay.ShowImage 方法")]
        public void showImage(string path, int index, string result = "", VBRcvInfp box = null) => ShowImage(path, index, result, box);

        /// <summary>
        /// 显示图片（委托给 ImageDisplayService）
        /// </summary>
        public void ShowImage(string path, int index, string result = "", VBRcvInfp box = null)
        {
            ImageDisplay.ShowImage(path, index, result, box);
        }

        #endregion

        #endregion

        #region 辅助方法

        #endregion

        #region 线程管理


        /// <summary>
        /// 线程处理 (Task循环)
        /// </summary>
        private void ThreadReadAVI(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 使用 Thread.Sleep 替代 Task.Delay().Wait()，避免阻塞线程池线程
                    Thread.Sleep(100);
                    if (token.IsCancellationRequested) break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    if (!IsStart  || !IsAllow)
                    {
                        continue;
                    }
                    // 读取 AVI 数据
                    if (_aviReaderService.ReadAVI(URL, out string result))
                    {
                        _aviReaderService.DoAviJsonTyped(result);
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error(ex.ToString());
                }
            }
        }

        #endregion

        #region AVI 数据读取与处理

        /// <summary>
        /// 解析Minio路径
        /// </summary>
        public void ParseMinioPath(string fullPath, out string path, out string result)=>AviReaderService.ParseMinioPath(fullPath,out path, out result);

        #endregion

        #region 算法检测处理

        /// <summary>
        /// 算法检测线程处理 (Task循环)
        /// </summary>
        private void ThreadDefect(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 使用 Thread.Sleep 替代 Task.Delay().Wait()，避免阻塞线程池线程
                    Thread.Sleep(15);
                    if (token.IsCancellationRequested) break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (!IsStart)
                {
                    continue;
                }
                if (_aviQueue.Count > 0)
                {
                    if (_aviQueue.TryDequeue(out VBModel info))
                    {
                        try
                        {
                            // 检查点1：出队后立即检查是否应该中止
                            if (!IsStart)
                            {
                                LogTextHelper.Info($"AI检测任务被暂停中止（出队后），SN:{info?.SN}");
                                SystemEvent.SendTaskMsg(info?.SN, "暂停-AI检测已中止");
                                continue; // finally 会释放资源
                            }

                            SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面开始AI检测(缺陷数:{info.Mats.Count})");
                            AIStopwatch.Restart();

                            //调用算法处理
                            LogTextHelper.Info($"准备DefectMethod，SN:{info.SN}，图片数量:{info.Mats.Count}");
                            if (_defectProcessor.DefectMethod(info,SysConfig.MaxDefectCount, out List<string> msg, out List<string> details, out PcsResult pcsResult, out string vbJson))
                            {
                                // 检查点2：推理完成后检查是否应该中止（不再回写结果）
                                if (!IsStart)
                                {
                                    LogTextHelper.Info($"AI检测任务被暂停中止（推理完成后），SN:{info.SN}");
                                    SystemEvent.SendTaskMsg(info.SN, "暂停-AI检测已中止（结果未回写）");
                                    continue; // finally 会释放资源
                                }
                                SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面AI检测完成");
                                SystemEvent.SendResultInfo(info.SN, msg, details, pcsResult);
                            }
                            else
                            {
                                LogTextHelper.Warn($"KEY:{info.Key} SN:{info.SN}检测失败！");
                                SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面已完成");

                            }
                            _defectProcessor.EnqueueAIResult(info, msg);

                        }
                        catch (Exception ex)
                        {
                            LogTextHelper.Error(ex.ToString());
                            SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面已完成");
                        }
                        finally
                        {
                            AIStopwatch.Stop();
                            var elapsedMs = AIStopwatch.ElapsedMilliseconds;
                            if (elapsedMs>20)
                            {
                                SystemEvent.SendTaskMsg(info.SN, $"{info.Side}面AI耗时:{elapsedMs}ms", elapsedMs);
                            }
                            // 释放 Mat 资源，防止内存泄漏
                            if (info?.Mats != null)
                            {
                                foreach (var mat in info.Mats)
                                {
                                    mat?.Dispose();
                                }
                                info.Mats.Clear();
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Minio 操作与数据转换

        /// <summary>
        /// 通过Minio读取Json文件
        /// </summary>
        public void ReadJsonByMinio(string ip, string port, string key, string head, string sn, string side, string path)
        {
            try
            {
                LogTextHelper.Info($"{sn} 准备ReadJsonByMinio");
                SystemEvent.SendTaskMsg(sn, $"{side}面正在读取数据");
                Minio.BuildClient(ip, port);
                string json = Minio.ReadJsonSync("deepiresults", path, ip);
                var obj = JsonConvert.DeserializeObject<RootPanelInfo>(json);
                List<int> defectIndex = new List<int>();
                List<int> pcsList = new List<int>();
                if (side == "A")
                {
                    SystemEvent.SendTaskMsg(sn);
                }
                LogTextHelper.Info($"{sn} {side} 开始将json转为vbinfo");
                RootVBInfo vbInfo = PanelJsonToVBInfo(ip, port, head, obj, ref defectIndex, ref pcsList, out bool isByPass);

                RootPanelInfoWithIP rootobj = new RootPanelInfoWithIP()
                {
                    IP = ip,
                    rootInfo = obj,
                };

                // 解耦：先获取图片Key列表，入图片加载队列，非阻塞
                var imageKeys = _imageLoaderService.GetAllMinioImageKeys(rootobj);

                //考虑用Model方式
                VBModel model = new VBModel
                {
                    Key = key,
                    SN = sn,
                    Side = side,
                    DefectIndex = defectIndex,
                    PcsIndex = pcsList,
                    VbInfo = vbInfo,
                    minioPath = head,
                    panelInfo = obj,
                    isByPass = isByPass,
                    ImageKeys=imageKeys
                };

                var loadModel = new ImageLoadModel
                {
                    Model = model,
                    RootPanelInfo = rootobj
                };
                _imageLoadQueue.Enqueue(loadModel);

                LogTextHelper.Info($"{sn} {side} ReadJsonByMinio完成,入队列_imageLoadQueue成功,待加载图片数量:{imageKeys.Count}");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常" + ex.ToString());
            }
        }

        public RootVBInfo PanelJsonToVBInfo(string minioip, string minioport, string head, RootPanelInfo info, ref List<int> defectList, ref List<int> pcsList, out bool isByPass)
        {
            try
            {
                isByPass = false;
                LogTextHelper.Info($"{info.SerialNumber} {info.SideIndex}  ProcuctSerial:" + info.ProductSerial);
                var solutionFlow = SolConfig.solus.FirstOrDefault(o => o.ProductSerial == info.ProductSerial);
                if (solutionFlow != null)
                {
                    if (info.SideIndex == "A")
                    {
                        Solution = solutionFlow.Asolution;
                        Flow = solutionFlow.Aflow;
                    }
                    else
                    {
                        Solution = solutionFlow.Bsolution;
                        Flow = solutionFlow.Bflow;
                    }
                    IsSwitch = solutionFlow.IsSwitch;
                }
                else
                {
                    isByPass = true;
                    LogTextHelper.Warn($"料号 {info.ProductSerial} 未配置，使用默认方案，标记为ByPass");
                    var defaultSolutionFlow = SolConfig.solus.FirstOrDefault(o => o.ProductSerial.ToUpper() == "DEFAULT");
                    if (defaultSolutionFlow != null)
                    {
                        if (info.SideIndex == "A")
                        {
                            Solution = defaultSolutionFlow.Asolution;
                            Flow = defaultSolutionFlow.Aflow;
                        }
                        else
                        {
                            Solution = defaultSolutionFlow.Bsolution;
                            Flow = defaultSolutionFlow.Bflow;
                        }
                        IsSwitch = defaultSolutionFlow.IsSwitch;
                    }
                    else
                    {
                        throw new Exception("找不到default的算法流程");
                    }
                }

                DsCenterInfo dsInfo = new DsCenterInfo();
                PanelData panelData = new PanelData
                {
                    Project = SysConfig.ProjectName
                };
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
                    _dsCenterInfoDict.TryGetValue($"{info.LotId}_{info.SerialNumber}", out dsInfo);
                    JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                    string infoJson = JsonConvert.SerializeObject(dsInfo, Formatting.None, jsonSetting);
                }

                LogTextHelper.Info($"当前产品:{info.SerialNumber},{info.SideIndex}面,所属料号:{info.ProductSerial},切换参数-->方案:{Solution},flow:{Flow}");
                RootVBInfo vBInfo = new RootVBInfo
                {
                    MessageType = "visionbuilder_inference",
                    paramsData = new ParamsData()
                };
                vBInfo.paramsData.InferResUuid = Guid.NewGuid().ToString();
                vBInfo.paramsData.InferWholeData = new InferWholeData
                {
                    ImageInferParams = new ImageInferParams
                    {
                        PipelineName = Solution,
                        NodeParams = new List<NodeParam>
                        {
                            new NodeParam()
                            {
                                NodeName = Flow,
                                height = 200,
                                width = 200,
                            }
                        }
                    },
                    ImageData = new ImageData()
                    {
                        DataType = "minio",
                        DataValue = new DataValue
                        {
                            InferImageGroup = new List<InferImageGroup>()
                        }
                    }
                };

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
                        dsInfo?.Data[0].Content.TryGetValue((i + 1).ToString(), out item);
                        item.ProcessTimeB = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
                    }


                    // info的PcsInfo在ATS只有一条，做其他项目时要注意
                    if (info.PcsInfo.TryGetValue((i + 1).ToString(), out PcsInfo pcsInfo))
                    {
                        LogTextHelper.Info($"SN:{info.SerialNumber}_{info.SideIndex}面报点数据为:{pcsInfo.DefectInfo.Count}");

                        for (int j = 0; j < pcsInfo.DefectInfo.Count; j++)
                        {
                            //奥特斯
                            DsCenterDefectInfo dsDefectinfo = new DsCenterDefectInfo
                            {
                                SideType = info.SideIndex,
                                SideType2 = info.SideIndex,
                                PcsIndex = i + 1,
                                PcsVesIndex = (i + 1).ToString(),
                                DefectRoi = pcsInfo.DefectInfo[j].DefectRoi,

                                DefectOriginRoi = pcsInfo.DefectInfo[j].DefectOriginRoi,
                                DefectsRoi = new List<int>()
                                {
                                    pcsInfo.DefectInfo[j].DefectRoi.X,
                                    pcsInfo.DefectInfo[j].DefectRoi.Y,
                                    pcsInfo.DefectInfo[j].DefectRoi.Height,
                                    pcsInfo.DefectInfo[j].DefectRoi.Width
                                }
                            };
                            InferImageGroup group = new InferImageGroup
                            {
                                MachineTemplateInfo = new MachineTemplateInfo()
                                {
                                    MachineName = info.StationName,
                                    product = info.ProductSerial,
                                    Side = info.SideIndex,
                                },
                                GroupUuid = Guid.NewGuid().ToString(),
                                GroupInfos = new List<GroupInfo>(),
                                DefectCode = "",
                                TempImgPath = Path.Combine(SolConfig.PartNumberImagesLoc, $"{info.ProductSerial}\\{info.ProductSerial}[{info.SideIndex}].jpg"),
                                ImgROI = new List<int>
                                {
                                    pcsInfo.DefectInfo[j].DefectRoi.X,
                                    pcsInfo.DefectInfo[j].DefectRoi.Y,
                                    pcsInfo.DefectInfo[j].DefectRoi.Width,
                                    pcsInfo.DefectInfo[j].DefectRoi.Height
                                }
                            };
                            if (IsSwitch)
                            {
                                group.DefectCode = pcsInfo.DefectInfo[j].DefectCode;
                            }
                            WatchPathConfig config = AviConfig.WatchPaths.FirstOrDefault(o => o.AviName == info.StationName);
                            if (config != null)
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
                            group.inspectDetails = new InspectDetails
                            {
                                InferRois = new List<InferRoi>() { }
                            };
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
                    _dsCenterInfoDict.TryAdd($"{info.LotId}_{info.SerialNumber}", dsInfo);
                    LogTextHelper.Info($"{info.LotId}_{info.SerialNumber}_在A面创建dsinfo成功");
                }
                else
                {
                    _dsCenterInfoDict[$"{info.LotId}_{info.SerialNumber}"] = dsInfo;
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

        #endregion

        #region 算法调用与结果处理

        public bool DefectMethod(VBModel vBModel, out List<string> resList, out List<string> detailsList, out PcsResult pcsResult, out string vbJson)=>_defectProcessor.DefectMethod(vBModel,SysConfig.MaxDefectCount, out resList, out detailsList, out pcsResult, out vbJson);

        #endregion

        #region 数据库操作

        public void GenerateVRSTestData() => DatabaseHelper.GenerateEmployeeReportTestData(5000);

        public void SaveEmployeeReport(EmployeeReport report) =>
            _databaseHelper.SaveEmployeeReport(report);

        /// <summary>
        /// 清空数据库所有表的数据
        /// </summary>
        public Task<bool> ClearAllDatabaseData() =>
            _databaseHelper.ClearAllData();

        /// <summary>
        /// 保存/更新 PanelSide 数据到数据库（支持覆盖现有数据）
        /// </summary>
        /// <param name="record">PanelSideRecord 记录</param>
        public void SavePanelSide(PanelSideRecord record) =>
            _databaseHelper.SavePanelSide(record);

        /// <summary>
        /// 保存 PanelSide 数据到数据库（从 RootPanelInfo 构建记录）
        /// </summary>
        /// <param name="panelInfo">面板信息</param>
        /// <param name="detectPoints">缺陷点列表</param>
        /// <param name="aviState">AVI 状态 (1: OK, 2: NG)</param>
        /// <param name="aiState">AI 状态 (1: OK, 2: NG, 3: Bypass)</param>
        private void SavePanelSideToDatabase(RootPanelInfo panelInfo, List<DetectInfo> detectPoints, int aviState, int aiState)
        {
            if (!DateTime.TryParse(panelInfo.AviCreateTime, out DateTime aviCreationTime))
            {
                aviCreationTime = DateTime.Now;
                LogTextHelper.Info($"无法解析 AviCreateTime '{panelInfo.AviCreateTime}'。将使用当前时间 '{aviCreationTime}' 作为备用。");
            }

            LogTextHelper.Info($"存储 SN={panelInfo.SerialNumber}, Side={panelInfo.SideIndex}, AviState={aviState}, AiState={aiState}, DefectCount={detectPoints?.Count ?? 0} 到数据库...");
            var record = new PanelSideRecord()
            {
                Data = new SideData()
                {
                    Side = panelInfo.SideIndex,
                    DetectPoints = detectPoints,
                    AviState = aviState,
                    AiState = aiState,
                    VvsState = 0,
                    VrsState = 0,
                    FinalState = 0
                },
                ProductSerial = panelInfo.ProductSerial,
                DetectionDate = DateTime.Now,
                AviCreationTime = aviCreationTime,
                LotNumber = panelInfo.LotId,
                SerialNumber = panelInfo.SerialNumber,
                MachineId = panelInfo.StationName,
                Side = panelInfo.SideIndex,
                PathIndex = panelInfo.PathIndex
            };
            BoardStatCache.Update(record);
            List<PanelSideRecord> batchToFlush = null;
            lock (_panelRecordLock)
            {
                _pendingPanelSideRecords.Add(record);
                if (_pendingPanelSideRecords.Count >= PanelSideBatchSize)
                {
                    batchToFlush = new List<PanelSideRecord>(_pendingPanelSideRecords);
                    _pendingPanelSideRecords.Clear();
                }
            }

            if (batchToFlush != null)
            {
                _databaseHelper.SavePanelSidesBatch(batchToFlush);
            }
        }

        private void FlushPendingPanelSideRecords()
        {
            List<PanelSideRecord> snapshot = null;
            lock (_panelRecordLock)
            {
                if (_pendingPanelSideRecords.Count > 0)
                {
                    snapshot = new List<PanelSideRecord>(_pendingPanelSideRecords);
                    _pendingPanelSideRecords.Clear();
                }
            }

            if (snapshot != null)
            {
                _databaseHelper.SavePanelSidesBatch(snapshot);
            }
        }

        public Task<List<PanelDataRecord>> GetPanelsData(DateTime start, DateTime end, string partnumber = null) =>
            _databaseHelper.GetPanelsData(start, end, partnumber);

        public Task<List<PanelDataRecord>> GetPanelsDataByMachineAndLot(string machineId, string lotNumber) =>
            _databaseHelper.GetPanelsDataByMachineAndLot(machineId, lotNumber);
        #endregion

        #region 后台线程处理

        /// <summary>
        /// 图片加载线程处理 (解耦图片读取和推理)
        /// </summary>
        private void ThreadImageLoad(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 使用 Thread.Sleep 替代 Task.Delay().Wait()，避免阻塞线程池线程
                    Thread.Sleep(15);
                    if (token.IsCancellationRequested) break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (!IsStart)
                {
                    continue;
                }

                if (_imageLoadQueue.TryDequeue(out ImageLoadModel loadModel))
                {
                    try
                    {
                        // 检查点1：出队后立即检查是否应该中止
                        if (!IsStart)
                        {
                            LogTextHelper.Info($"图片加载任务被暂停中止，SN:{loadModel.Model?.SN}");
                            SystemEvent.SendTaskMsg(loadModel.Model?.SN, "暂停-图片加载已中止");
                            // 清理已加载的资源
                            if (loadModel.Model?.Mats != null)
                            {
                                foreach (var mat in loadModel.Model.Mats)
                                {
                                    mat?.Dispose();
                                }
                                loadModel.Model.Mats.Clear();
                            }
                            continue;
                        }

                        LogTextHelper.Info($"开始加载图片，SN:{loadModel.Model.SN}，数量：{loadModel.Model.ImageKeys.Count}");
                        SystemEvent.SendTaskMsg(loadModel.Model.SN, $"{loadModel.Model.Side}面正在加载图片");

                        // 使用 ImageLoaderService 并行加载图片
                        loadModel.Model.Mats = _imageLoaderService.LoadImages(loadModel.Model.ImageKeys);

                        // 检查点2：图片加载完成后检查是否应该中止
                        if (!IsStart)
                        {
                            LogTextHelper.Info($"图片加载后任务被暂停中止，SN:{loadModel.Model.SN}");
                            SystemEvent.SendTaskMsg(loadModel.Model.SN, "暂停-图片加载后已中止");
                            // 释放已加载的图片资源
                            if (loadModel.Model.Mats != null)
                            {
                                foreach (var mat in loadModel.Model.Mats)
                                {
                                    mat?.Dispose();
                                }
                                loadModel.Model.Mats.Clear();
                            }
                            continue;
                        }

                        // 检查加载结果
                        int expectedCount = loadModel.Model.ImageKeys.Count;
                        int actualCount = loadModel.Model.Mats.Count;
                        if (expectedCount == 0)
                        {
                            // 本身就没有报点数据，不是错误
                            LogTextHelper.Info($"SN:{loadModel.Model.SN} 无报点数据，无需加载图片");
                        }
                        else if (actualCount == 0)
                        {
                            LogTextHelper.Error($"图片加载失败，SN:{loadModel.Model.SN}，期望{expectedCount}张图片，实际加载0张！");
                        }
                        else if (actualCount < expectedCount)
                        {
                            LogTextHelper.Warn($"图片部分加载失败，SN:{loadModel.Model.SN}，期望{expectedCount}张，实际{actualCount}张");
                        }

                        // 图片加载完成，入推理队列
                        _aviQueue.Enqueue(loadModel.Model);

                        // 发送 PanelInfo 事件
                        SystemEvent.SendPanelInfo(loadModel.Model.SN, loadModel.RootPanelInfo);

                        SystemEvent.SendTaskMsg(loadModel.Model.SN, $"{loadModel.Model.Side}面图片加载完成");
                        LogTextHelper.Info($"图片加载完成，SN:{loadModel.Model.SN}，实际加载:{loadModel.Model.Mats.Count}张，入队列_aviQueue成功");
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"图片加载异常，SN:{loadModel.Model?.SN}：" + ex.ToString());
                    }
                }
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
                    // 使用 Thread.Sleep 替代 Task.Delay().Wait()，避免阻塞线程池线程
                    Thread.Sleep(15);
                    if (token.IsCancellationRequested) break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (!IsStart)
                {
                    continue;
                }

                try
                {
                    if (_aiResultQueue.Count > 0)
                    {
                        if (_aiResultQueue.TryDequeue(out Tuple<string, string, string, RootAIResult, DsCenterInfo> info))
                        {
                            // 检查点：出队后检查是否应该中止
                            if (!IsStart)
                            {
                                LogTextHelper.Info($"结果回写任务被暂停中止，SN:{info?.Item2}");
                                SystemEvent.SendTaskMsg(info?.Item2, "暂停-结果回写已中止");
                                continue;
                            }
                            // 回写处理
                            _resultWriterService?.ReturnAVI(info);

                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 推理后处理线程（异步处理推理结果）
        /// </summary>
        private void ThreadPostProcess(CancellationToken token)
        {
            LogTextHelper.Info("推理后处理线程已启动");

            while (!token.IsCancellationRequested)
            {
                if (!IsStart)
                {
                    continue;
                }
                try
                {
                    if (_inferencePostProcessQueue.TryDequeue(out InferenceResultModel resultModel))
                    {
                        try
                        {
                            // 检查点：出队后检查是否应该中止
                            if (!IsStart)
                            {
                                LogTextHelper.Info($"后处理任务被暂停中止，SN:{resultModel.VBModel?.SN}");
                                SystemEvent.SendTaskMsg(resultModel.VBModel?.SN, "暂停-后处理已中止");
                                continue;
                            }
                            // Delegate to PostProcessService
                            _postProcessService.ProcessInferenceResult(resultModel);
                        }
                        catch (Exception ex)
                        {
                            LogTextHelper.Error($"后处理异常: SN={resultModel.VBModel?.SN}, 错误={ex}");
                        }
                    }
                    else
                    {
                        Thread.Sleep(10); // 队列为空时短暂休眠
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"后处理线程异常: {ex}");
                }
            }

            LogTextHelper.Info("推理后处理线程已停止");
        }

        /// <summary>
        /// 定期清理过期的处理标记（防止内存泄漏）
        /// </summary>
        private void ThreadCleanupProcessingCache(CancellationToken token)
        {
            LogTextHelper.Info("处理缓存清理线程已启动");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(60000); // 每分钟清理一次

                    if (token.IsCancellationRequested) break;

                    var now = DateTime.Now;
                    var expiredKeys = _processingSnSet
                        .Where(kvp => (now - kvp.Value).TotalMinutes > CACHE_EXPIRE_MINUTES)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in expiredKeys)
                    {
                        if (_processingSnSet.TryRemove(key, out DateTime addTime))
                        {
                            var duration = now - addTime;
                            LogTextHelper.Warn($"清理过期处理标记：{key}，已超时 {duration.TotalMinutes:F1} 分钟");
                        }
                    }

                    if (expiredKeys.Count > 0)
                    {
                        LogTextHelper.Info($"清理了 {expiredKeys.Count} 个过期处理标记，当前集合大小：{_processingSnSet.Count}");
                    }

                    // 输出统计信息
                    LogProcessingStatistics();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"清理处理缓存异常: {ex}");
                }
            }

            LogTextHelper.Info("处理缓存清理线程已停止");
        }

        /// <summary>
        /// 定期输出处理统计信息
        /// </summary>
        private void LogProcessingStatistics()
        {
            try
            {
                LogTextHelper.Info($"处理统计 - 处理中:{_processingSnSet.Count}, " +
                                  $"图片加载队列:{_imageLoadQueue.Count}, " +
                                  $"推理队列:{_aviQueue.Count}, " +
                                  $"后处理队列:{_inferencePostProcessQueue.Count}, " +
                                  $"结果队列:{_aiResultQueue.Count}"+
                                  $"数据库队列{_databaseHelper.GetQueueLength()}");

                // 告警：如果处理集合持续增长超过阈值
                if (_processingSnSet.Count > 100)
                {
                    LogTextHelper.Warn($"⚠️ 处理集合过大({_processingSnSet.Count})，可能存在处理阻塞或标记未清理！");

                    // 输出前10个最老的处理项
                    var oldestItems = _processingSnSet
                        .OrderBy(kvp => kvp.Value)
                        .Take(10)
                        .ToList();

                    foreach (var item in oldestItems)
                    {
                        var duration = DateTime.Now - item.Value;
                        LogTextHelper.Warn($"  - {item.Key}: 已处理 {duration.TotalMinutes:F1} 分钟");
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"输出统计信息异常: {ex}");
            }
        }

        #endregion

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
                        // ⭐ 关键修改点8：等待所有任务完成，包括清理线程
                        Task.WaitAll(new[] {
                            _readAviTask,
                            _imageLoadTask,
                            _defectTask,
                            _returnAviTask,
                            _postProcessTask,
                            _cleanupTask  // 新增
                        }, 5000);
                    }
                    catch (AggregateException)
                    {
                        // 忽略因取消导致的任务异常
                    }
                    _cancellationTokenSource.Dispose();
                }

                FlushPendingPanelSideRecords();

                // 清理队列中残留的 Mat 资源
                while (_aviQueue.TryDequeue(out var vbModel))
                {
                    if (vbModel?.Mats != null)
                    {
                        foreach (var mat in vbModel.Mats)
                        {
                            mat?.Dispose();
                        }
                        vbModel.Mats.Clear();
                    }
                }

                // 清理图片加载队列中残留的资源
                while (_imageLoadQueue.TryDequeue(out var loadModel))
                {
                    if (loadModel?.Model?.Mats != null)
                    {
                        foreach (var mat in loadModel.Model.Mats)
                        {
                            mat?.Dispose();
                        }
                        loadModel.Model.Mats.Clear();
                    }
                }

                // 清理后处理队列
                while (_inferencePostProcessQueue.TryDequeue(out var _))
                {
                    // 仅清空队列，不需要释放资源
                }
                LogTextHelper.Info("后处理队列已清空");

                // ⭐ 关键修改点9：清理处理集合
                _processingSnSet.Clear();
                LogTextHelper.Info($"已清理处理集合，释放资源完成");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Dispose an exception occurred during task cancellation:" + ex.ToString());
            }

        }
        #endregion

        #region 推理结果处理相关
        /// <summary>
        /// 清空所有正在处理的队列（当 IsStart 设置为 false 时调用）
        /// </summary>
        private void ClearAllProcessingQueues()
        {
            try
            {
                LogTextHelper.Info("IsStart 设置为 false，开始清空所有处理队列...");

                // 用于收集被清除的 SN 列表
                HashSet<string> clearedSnSet = new HashSet<string>();

                int aviQueueCount = 0;
                int imageLoadQueueCount = 0;
                int aiResultQueueCount = 0;
                int postProcessQueueCount = 0;

                // 清空 _aviQueue 并释放 Mat 资源
                while (_aviQueue.TryDequeue(out var vbModel))
                {
                    if (vbModel != null)
                    {
                        if (!string.IsNullOrEmpty(vbModel.SN))
                        {
                            clearedSnSet.Add(vbModel.SN);
                        }
                        if (vbModel.Mats != null)
                        {
                            foreach (var mat in vbModel.Mats)
                            {
                                mat?.Dispose();
                            }
                            vbModel.Mats.Clear();
                        }
                    }
                    aviQueueCount++;
                }

                // 清空 _imageLoadQueue 并释放 Mat 资源
                while (_imageLoadQueue.TryDequeue(out var loadModel))
                {
                    if (loadModel?.Model != null)
                    {
                        if (!string.IsNullOrEmpty(loadModel.Model.SN))
                        {
                            clearedSnSet.Add(loadModel.Model.SN);
                        }
                        if (loadModel.Model.Mats != null)
                        {
                            foreach (var mat in loadModel.Model.Mats)
                            {
                                mat?.Dispose();
                            }
                            loadModel.Model.Mats.Clear();
                        }
                    }
                    imageLoadQueueCount++;
                }

                // 清空 _aiResultQueue
                while (_aiResultQueue.TryDequeue(out var aiResult))
                {
                    if (aiResult != null && !string.IsNullOrEmpty(aiResult.Item2))
                    {
                        clearedSnSet.Add(aiResult.Item2); // Item2 是 SN
                    }
                    aiResultQueueCount++;
                }

                // 清空 _inferencePostProcessQueue
                while (_inferencePostProcessQueue.TryDequeue(out var postProcess))
                {
                    if (postProcess?.VBModel != null && !string.IsNullOrEmpty(postProcess.VBModel.SN))
                    {
                        clearedSnSet.Add(postProcess.VBModel.SN);
                    }
                    postProcessQueueCount++;
                }

                // 清空 _processingSnSet
                int processingSnCount = _processingSnSet.Count;
                _processingSnSet.Clear();

                // 清空 _dsCenterInfoDict
                int dsCenterInfoCount = _dsCenterInfoDict.Count;
                _dsCenterInfoDict.Clear();

                LogTextHelper.Info($"所有处理队列已清空 - " +
                    $"AVI队列:{aviQueueCount}, " +
                    $"图片加载队列:{imageLoadQueueCount}, " +
                    $"AI结果队列:{aiResultQueueCount}, " +
                    $"后处理队列:{postProcessQueueCount}, " +
                    $"处理中集合:{processingSnCount}, " +
                    $"中台数据字典:{dsCenterInfoCount}");

                // 通知界面被清除的 SN
                foreach (var sn in clearedSnSet)
                {
                    SystemEvent.SendTaskMsg(sn, "暂停-已从队列移除");
                }

                if (clearedSnSet.Count > 0)
                {
                    LogTextHelper.Info($"已通知界面 {clearedSnSet.Count} 个SN被暂停移除：{string.Join(", ", clearedSnSet)}");
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"清空处理队列时发生异常: {ex}");
            }
        }

        #endregion
    }
}

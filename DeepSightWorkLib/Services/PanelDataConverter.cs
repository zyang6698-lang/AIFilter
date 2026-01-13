using DeepSightModel;
using DeepSightTool;
using DeepSightWorkLib.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// Panel 数据转换服务
    /// 负责将 RootPanelInfo 转换为 AI 推理所需的 RootVBInfo 格式
    /// </summary>
    public class PanelDataConverter : IPanelDataConverter
    {
        /// <summary>
        /// 中台数据字典缓存
        /// Key: "{LotId}_{SerialNumber}"
        /// </summary>
        private readonly ConcurrentDictionary<string, DsCenterInfo> _dsCenterInfoDict = new ConcurrentDictionary<string, DsCenterInfo>();

        /// <summary>
        /// JSON 序列化设置（静态复用，避免重复创建）
        /// </summary>
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <inheritdoc/>
        public PanelConvertResult Convert(RootPanelInfo panelInfo, PanelConvertContext context)
        {
            if (panelInfo == null) throw new ArgumentNullException(nameof(panelInfo));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var result = new PanelConvertResult();
            
            try
            {
                LogTextHelper.Info($"{panelInfo.SerialNumber} {panelInfo.SideIndex} ProductSerial: {panelInfo.ProductSerial}");

                // 1. 解析方案配置
                var solutionInfo = ResolveSolution(panelInfo, context.SolutionConfig, out bool isByPass);
                result.IsByPass = isByPass;

                // 2. 初始化中台数据
                var dsInfo = InitializeDsCenterInfo(panelInfo, context.ProjectName);
                result.DsCenterInfo = dsInfo;

                // 3. 构建 VBInfo
                result.VBInfo = BuildVBInfo(panelInfo, context, solutionInfo, dsInfo, 
                    result.DefectIndexList, result.PcsIndexList);

                // 4. 存储中台数据
                StoreDsCenterInfo(panelInfo, dsInfo);

                return result;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"PanelDataConverter.Convert 异常: {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public DsCenterInfo GetDsCenterInfo(string lotId, string serialNumber)
        {
            var key = $"{lotId}_{serialNumber}";
            _dsCenterInfoDict.TryGetValue(key, out var info);
            return info;
        }

        /// <inheritdoc/>
        public void SetDsCenterInfo(string lotId, string serialNumber, DsCenterInfo info)
        {
            var key = $"{lotId}_{serialNumber}";
            _dsCenterInfoDict[key] = info;
        }

        /// <inheritdoc/>
        public void ClearDsCenterInfoCache()
        {
            _dsCenterInfoDict.Clear();
            LogTextHelper.Info("PanelDataConverter: 中台数据缓存已清空");
        }

        #region 私有方法

        /// <summary>
        /// 解析方案配置
        /// </summary>
        private (string Solution, string Flow, bool IsSwitch) ResolveSolution(
            RootPanelInfo panelInfo, SolutionConfig solConfig, out bool isByPass)
        {
            isByPass = false;
            string solution = "";
            string flow = "";
            bool isSwitch = false;

            var solutionFlow = solConfig?.solus?.FirstOrDefault(o => o.ProductSerial == panelInfo.ProductSerial);
            
            if (solutionFlow != null)
            {
                (solution, flow) = panelInfo.SideIndex == "A" 
                    ? (solutionFlow.Asolution, solutionFlow.Aflow)
                    : (solutionFlow.Bsolution, solutionFlow.Bflow);
                isSwitch = solutionFlow.IsSwitch;
            }
            else
            {
                isByPass = true;
                LogTextHelper.Warn($"料号 {panelInfo.ProductSerial} 未配置，使用默认方案，标记为ByPass");
                
                var defaultFlow = solConfig?.solus?.FirstOrDefault(o => 
                    o.ProductSerial?.ToUpper() == "DEFAULT");
                    
                if (defaultFlow != null)
                {
                    (solution, flow) = panelInfo.SideIndex == "A"
                        ? (defaultFlow.Asolution, defaultFlow.Aflow)
                        : (defaultFlow.Bsolution, defaultFlow.Bflow);
                    isSwitch = defaultFlow.IsSwitch;
                }
                else
                {
                    throw new Exception("找不到 default 的算法流程配置");
                }
            }

            LogTextHelper.Info($"当前产品:{panelInfo.SerialNumber},{panelInfo.SideIndex}面,所属料号:{panelInfo.ProductSerial},方案:{solution},flow:{flow}");
            return (solution, flow, isSwitch);
        }

        /// <summary>
        /// 初始化中台数据
        /// </summary>
        private DsCenterInfo InitializeDsCenterInfo(RootPanelInfo panelInfo, string projectName)
        {
            DsCenterInfo dsInfo;

            if (panelInfo.SideIndex == "A")
            {
                dsInfo = new DsCenterInfo();
            }
            else
            {
                // B面从缓存获取A面创建的数据
                dsInfo = GetDsCenterInfo(panelInfo.LotId, panelInfo.SerialNumber) ?? new DsCenterInfo();
            }

            return dsInfo;
        }

        /// <summary>
        /// 构建 VBInfo 对象
        /// </summary>
        private RootVBInfo BuildVBInfo(
            RootPanelInfo panelInfo,
            PanelConvertContext context,
            (string Solution, string Flow, bool IsSwitch) solutionInfo,
            DsCenterInfo dsInfo,
            List<int> defectList,
            List<int> pcsList)
        {
            var vBInfo = CreateBaseVBInfo(solutionInfo.Solution, solutionInfo.Flow);

            // 创建 PanelData 用于A面
            PanelData panelData = null;
            if (panelInfo.SideIndex == "A")
            {
                panelData = new PanelData
                {
                    Project = context.ProjectName,
                    Product = panelInfo.ProductSerial,
                    Lot = panelInfo.LotId,
                    Sn = panelInfo.SerialNumber,
                    Avi = panelInfo.StationName,
                    CustomTags = ""
                };
            }

            // 处理 PCS 信息
            ProcessPcsInfo(panelInfo, context, solutionInfo.IsSwitch, dsInfo, panelData, vBInfo, defectList, pcsList);

            // 设置 Minio 信息
            SetMinioInfo(vBInfo, context.MinioIP, context.MinioPort);

            // 存储 PanelData 到 dsInfo
            if (panelInfo.SideIndex == "A" && panelData != null)
            {
                dsInfo.Data.Add(panelData);
            }

            return vBInfo;
        }

        /// <summary>
        /// 创建基础 VBInfo 结构
        /// </summary>
        private RootVBInfo CreateBaseVBInfo(string solution, string flow)
        {
            return new RootVBInfo
            {
                MessageType = "visionbuilder_inference",
                paramsData = new ParamsData
                {
                    InferResUuid = Guid.NewGuid().ToString(),
                    InferWholeData = new InferWholeData
                    {
                        ImageInferParams = new ImageInferParams
                        {
                            PipelineName = solution,
                            NodeParams = new List<NodeParam>
                            {
                                new NodeParam { NodeName = flow, height = 200, width = 200 }
                            }
                        },
                        ImageData = new ImageData
                        {
                            DataType = "minio",
                            DataValue = new DataValue { InferImageGroup = new List<InferImageGroup>() }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// 处理 PCS 信息
        /// </summary>
        private void ProcessPcsInfo(
            RootPanelInfo panelInfo,
            PanelConvertContext context,
            bool isSwitch,
            DsCenterInfo dsInfo,
            PanelData panelData,
            RootVBInfo vBInfo,
            List<int> defectList,
            List<int> pcsList)
        {
            for (int i = 0; i < panelInfo.PcsInfo.Count; i++)
            {
                var pcsKey = (i + 1).ToString();
                ContentItem item = CreateContentItem(panelInfo, dsInfo, i);

                if (panelInfo.PcsInfo.TryGetValue(pcsKey, out PcsInfo pcsInfo))
                {
                    LogTextHelper.Info($"SN:{panelInfo.SerialNumber}_{panelInfo.SideIndex}面报点数据为:{pcsInfo.DefectInfo.Count}");
                    ProcessDefects(panelInfo, context, isSwitch, pcsInfo, i, item, vBInfo, defectList, pcsList);
                }

                if (panelInfo.SideIndex == "A" && panelData != null)
                {
                    panelData.Content.Add(pcsKey, item);
                }
            }
        }

        /// <summary>
        /// 创建内容项
        /// </summary>
        private ContentItem CreateContentItem(RootPanelInfo panelInfo, DsCenterInfo dsInfo, int index)
        {
            ContentItem item;
            var pcsKey = (index + 1).ToString();

            if (panelInfo.SideIndex == "A")
            {
                item = new ContentItem
                {
                    MachineName = panelInfo.StationName,
                    ProductSerial = panelInfo.ProductSerial,
                    SerialNumber = panelInfo.SerialNumber,
                    ProcessTimeA = DateTime.Now.ToString("yyyyMMddHHmmssffffff"),
                    LotId = panelInfo.LotId,
                    Lot = panelInfo.LotId,
                    Product = panelInfo.ProductSerial,
                    OperateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ExpansionAndContraction = null,
                    PieceIndex = pcsKey,
                    PieceVesIndex = pcsKey
                };
            }
            else
            {
                item = new ContentItem();
                dsInfo?.Data[0]?.Content?.TryGetValue(pcsKey, out item);
                if (item != null)
                {
                    item.ProcessTimeB = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
                }
                else
                {
                    item = new ContentItem { ProcessTimeB = DateTime.Now.ToString("yyyyMMddHHmmssffffff") };
                }
            }

            return item;
        }

        /// <summary>
        /// 处理缺陷信息
        /// </summary>
        private void ProcessDefects(
            RootPanelInfo panelInfo,
            PanelConvertContext context,
            bool isSwitch,
            PcsInfo pcsInfo,
            int pcsIndex,
            ContentItem item,
            RootVBInfo vBInfo,
            List<int> defectList,
            List<int> pcsList)
        {
            for (int j = 0; j < pcsInfo.DefectInfo.Count; j++)
            {
                var defect = pcsInfo.DefectInfo[j];

                // 创建中台缺陷信息
                var dsDefectInfo = CreateDsCenterDefectInfo(panelInfo, pcsIndex, defect);

                // 创建推理图片组
                var group = CreateInferImageGroup(panelInfo, context, isSwitch, defect);

                // 添加图片信息
                AddDefectImages(context, defect, group, dsDefectInfo);

                vBInfo.paramsData.InferWholeData.ImageData.DataValue.InferImageGroup.Add(group);
                group.inspectDetails = new InspectDetails { InferRois = new List<InferRoi>() };

                defectList.Add(j);
                pcsList.Add(defect.PcsIndex);
                item.DefectsInfo.Add(dsDefectInfo);
            }
        }

        /// <summary>
        /// 创建中台缺陷信息
        /// </summary>
        private DsCenterDefectInfo CreateDsCenterDefectInfo(RootPanelInfo panelInfo, int pcsIndex, DefectInfo defect)
        {
            return new DsCenterDefectInfo
            {
                SideType = panelInfo.SideIndex,
                SideType2 = panelInfo.SideIndex,
                PcsIndex = pcsIndex + 1,
                PcsVesIndex = (pcsIndex + 1).ToString(),
                DefectRoi = defect.DefectRoi,
                DefectOriginRoi = defect.DefectOriginRoi,
                DefectsRoi = new List<int>
                {
                    defect.DefectRoi.X,
                    defect.DefectRoi.Y,
                    defect.DefectRoi.Height,
                    defect.DefectRoi.Width
                }
            };
        }

        /// <summary>
        /// 创建推理图片组
        /// </summary>
        private InferImageGroup CreateInferImageGroup(
            RootPanelInfo panelInfo,
            PanelConvertContext context,
            bool isSwitch,
            DefectInfo defect)
        {
            var group = new InferImageGroup
            {
                MachineTemplateInfo = new MachineTemplateInfo
                {
                    MachineName = panelInfo.StationName,
                    product = panelInfo.ProductSerial,
                    Side = panelInfo.SideIndex
                },
                GroupUuid = Guid.NewGuid().ToString(),
                GroupInfos = new List<GroupInfo>(),
                DefectCode = isSwitch ? defect.DefectCode : "",
                TempImgPath = panelInfo.TemplateImgPath,
                ImgROI = new List<int>
                {
                    defect.DefectRoi.X,
                    defect.DefectRoi.Y,
                    defect.DefectRoi.Width,
                    defect.DefectRoi.Height
                }
            };

            return group;
        }

        /// <summary>
        /// 添加缺陷图片信息
        /// </summary>
        private void AddDefectImages(
            PanelConvertContext context,
            DefectInfo defect,
            InferImageGroup group,
            DsCenterDefectInfo dsDefectInfo)
        {
            var watchConfig = context.AviConfig?.WatchPaths?
                .FirstOrDefault(o => o.AviName == group.MachineTemplateInfo.MachineName);

            if (watchConfig == null)
            {
                LogTextHelper.Error($"Panel machineID:{group.MachineTemplateInfo.MachineName} 未找到对应机台配置");
                return;
            }

            AddGroupInfo(defect.DefectVrsImages, "defect", context.Head, group,
                url => dsDefectInfo.DefectImages.Add(url), watchConfig.MinioConfig);
            AddGroupInfo(defect.DefectVrsOkImages, "template", context.Head, group, null, watchConfig.MinioConfig);
            AddGroupInfo(defect.DefectVrsGerberImages, "gerber", context.Head, group,
                url => dsDefectInfo.DefectGerberImages.Add(url), watchConfig.MinioConfig);
        }

        /// <summary>
        /// 添加图片组信息
        /// </summary>
        private void AddGroupInfo(
            List<string> images,
            string imageType,
            string head,
            InferImageGroup group,
            Action<string> addUrlAction,
            string minioConfig)
        {
            if (images == null || images.Count == 0) return;

            var imagePath = $"{head}/{images[0]}";
            group.GroupInfos.Add(new GroupInfo
            {
                ImagePath = imagePath,
                ImageUuid = Guid.NewGuid().ToString(),
                ImageType = imageType
            });

            addUrlAction?.Invoke($"http://{minioConfig}/deepiresults/{imagePath}");
        }

        /// <summary>
        /// 设置 Minio 信息
        /// </summary>
        private void SetMinioInfo(RootVBInfo vBInfo, string minioIP, string minioPort)
        {
            vBInfo.paramsData.InferWholeData.OtherInfos = new Others
            {
                imageminio = new ImageMminio
                {
                    access_key_id = "deepiobjectdata",
                    bucket = "deepiresults",
                    endpoint_url = minioIP,
                    secret_key = "deepiobject2019",
                    secret_port = minioPort
                }
            };
        }

        /// <summary>
        /// 存储中台数据信息
        /// </summary>
        private void StoreDsCenterInfo(RootPanelInfo panelInfo, DsCenterInfo dsInfo)
        {
            var key = $"{panelInfo.LotId}_{panelInfo.SerialNumber}";

            if (panelInfo.SideIndex == "A")
            {
                _dsCenterInfoDict.TryAdd(key, dsInfo);
                LogTextHelper.Info($"{key}_在A面创建dsinfo成功");
            }
            else
            {
                _dsCenterInfoDict[key] = dsInfo;
                LogTextHelper.Info($"{key}_在B面更新dsinfo成功");
            }
        }

        #endregion
    }
}


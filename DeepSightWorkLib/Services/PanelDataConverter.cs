using DeepSightModel;
using DeepSightModel.Configuration;
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
        /// Minio 配置引用
        /// </summary>
        private static MinioSettings MinioSettingsConfig => MinioSettings.Instance;

        /// <summary>
        /// 用于防止多线程同时新增同一料号的锁对象
        /// </summary>
        private static readonly object _solutionLock = new object();

        /// <inheritdoc/>
        public PanelConvertResult Convert(RootPanelInfo panelInfo, PanelConvertContext context)
        {
            if (panelInfo == null) throw new ArgumentNullException(nameof(panelInfo));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var result = new PanelConvertResult();

            try
            {
                LogTextHelper.Info($"{panelInfo.SerialNumber} {panelInfo.SideIndex} ProductSerial: {panelInfo.ProductSerial}");

                //  解析方案配置
                var solutionInfo = ResolveSolution(panelInfo, context, out bool isByPass);
                result.IsByPass = isByPass;

                //  构建 VBInfo
                result.VBInfo = BuildVBInfo(panelInfo, context, solutionInfo,
                    result.DefectIndexList, result.PcsIndexList);

                return result;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"PanelDataConverter.Convert 异常: {ex}");
                throw;
            }
        }


        #region 私有方法

        /// <summary>
        /// 解析方案配置（料号未配置时自动基于 DEFAULT 新增并保存）
        /// </summary>
        private (string Solution, string Flow, bool IsSwitch) ResolveSolution(
            RootPanelInfo panelInfo, PanelConvertContext context, out bool isByPass)
        {
            isByPass = false;
            var solConfig = context.SolutionConfig;

            var solutionFlow = solConfig?.solus?.FirstOrDefault(o => o.ProductSerial == panelInfo.ProductSerial);

            if (solutionFlow == null)
            {
                // 料号未配置，基于 DEFAULT 自动新增
                isByPass = true;    
                solutionFlow = AutoAddProductSerial(panelInfo.ProductSerial, context);
            }

            string solution, flow;
            (solution, flow) = panelInfo.SideIndex == "A"
                ? (solutionFlow.Asolution, solutionFlow.Aflow)
                : (solutionFlow.Bsolution, solutionFlow.Bflow);

            LogTextHelper.Info($"当前产品:{panelInfo.SerialNumber},{panelInfo.SideIndex}面,所属料号:{panelInfo.ProductSerial},方案:{solution},flow:{flow}");
            return (solution, flow, solutionFlow.IsSwitch);
        }

        /// <summary>
        /// 自动基于 DEFAULT 配置新增料号，添加到内存配置并持久化保存
        /// </summary>
        private SolutionAndFlow AutoAddProductSerial(string productSerial, PanelConvertContext context)
        {
            var solConfig = context.SolutionConfig;

            lock (_solutionLock)
            {
                // 双重检查：可能其他线程已经添加
                var existing = solConfig?.solus?.FirstOrDefault(o => o.ProductSerial == productSerial);
                if (existing != null) return existing;

                var defaultFlow = solConfig?.solus?.FirstOrDefault(o =>
                    o.ProductSerial?.ToUpper() == "DEFAULT");

                if (defaultFlow == null)
                {
                    throw new Exception("找不到 DEFAULT 的算法流程配置，无法自动新增料号");
                }

                var newFlow = new SolutionAndFlow
                {
                    ProductSerial = productSerial,
                    Asolution = defaultFlow.Asolution,
                    Aflow = defaultFlow.Aflow,
                    Bsolution = defaultFlow.Bsolution,
                    Bflow = defaultFlow.Bflow,
                    IsSwitch = defaultFlow.IsSwitch,
                };

                if (solConfig.solus == null) solConfig.solus = new List<SolutionAndFlow>();
                solConfig.solus.Add(newFlow);

                LogTextHelper.Info($"料号 {productSerial} 未配置，已基于 DEFAULT 自动新增并采用默认算法流程");

                // 持久化保存
                try
                {
                    context.OnSolutionConfigChanged?.Invoke(solConfig);
                    LogTextHelper.Info($"料号 {productSerial} 配置已保存");
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"保存料号 {productSerial} 配置失败: {ex.Message}");
                }

                return newFlow;
            }
        }

        /// <summary>
        /// 构建 VBInfo 对象
        /// </summary>
        private RootVBInfo BuildVBInfo(
            RootPanelInfo panelInfo,
            PanelConvertContext context,
            (string Solution, string Flow, bool IsSwitch) solutionInfo,
            List<int> defectList,
            List<int> pcsList)
        {
            var vBInfo = CreateBaseVBInfo(solutionInfo.Solution, solutionInfo.Flow);

            // 处理 PCS 信息
            ProcessPcsInfo(panelInfo, context, solutionInfo.IsSwitch,vBInfo, defectList, pcsList);

            // 设置 Minio 信息
            SetMinioInfo(vBInfo, context.MinioIP, context.MinioPort);

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
            RootVBInfo vBInfo,
            List<int> defectList,
            List<int> pcsList)
        {
            for (int i = 0; i < panelInfo.PcsInfo.Count; i++)
            {
                var pcsKey = (i + 1).ToString();

                if (panelInfo.PcsInfo.TryGetValue(pcsKey, out PcsInfo pcsInfo))
                {
                    LogTextHelper.Info($"SN:{panelInfo.SerialNumber}_{panelInfo.SideIndex}面报点数据为:{pcsInfo.DefectInfo.Count}");
                    ProcessDefects(panelInfo, context, isSwitch, pcsInfo, i, vBInfo, defectList, pcsList);
                }
            }
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
            RootVBInfo vBInfo,
            List<int> defectList,
            List<int> pcsList)
        {
            for (int j = 0; j < pcsInfo.DefectInfo.Count; j++)
            {
                var defect = pcsInfo.DefectInfo[j];

                // 创建推理图片组
                var group = CreateInferImageGroup(panelInfo, context, isSwitch, defect);

                // 添加图片信息
                AddDefectImages(context, defect, group);

                vBInfo.paramsData.InferWholeData.ImageData.DataValue.InferImageGroup.Add(group);
                group.inspectDetails = new InspectDetails { InferRois = new List<InferRoi>() };

                defectList.Add(j);
                pcsList.Add(defect.PcsIndex);
            }
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
                    MachineName = panelInfo.MachineName,
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
            InferImageGroup group
            )
        {


            AddGroupInfo(defect.DefectVrsImages, "defect", context.Head, group);
            AddGroupInfo(defect.DefectVrsOkImages, "template", context.Head, group);
            AddGroupInfo(defect.DefectVrsGerberImages, "gerber", context.Head, group);
        }

        /// <summary>
        /// 添加图片组信息
        /// </summary>
        private void AddGroupInfo(
            List<string> images,
            string imageType,
            string head,
            InferImageGroup group)
        {
            if (images == null || images.Count == 0) return;

            var imagePath = $"{head}/{images[0]}";
            group.GroupInfos.Add(new GroupInfo
            {
                ImagePath = imagePath,
                ImageUuid = Guid.NewGuid().ToString(),
                ImageType = imageType
            });

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
                    access_key_id = MinioSettingsConfig.AccessKey,
                    bucket = MinioSettingsConfig.DefaultBucket,
                    endpoint_url = minioIP,
                    secret_key = MinioSettingsConfig.SecretKey,
                    secret_port = minioPort
                }
            };
        }

        #endregion
    }
}


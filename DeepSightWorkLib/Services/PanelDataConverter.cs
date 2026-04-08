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
                var solutionInfo = ResolveSolution(panelInfo, context);

                //  构建 VBInfo
                result.VBInfo = BuildVBInfo(panelInfo, context, solutionInfo,
                    result.DefectIndexList, result.PcsIndexList,
                    result.DirectReportDefectIndices, result.DirectReportPcsIndices);

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
        /// 解析方案配置（料号未配置时自动归入 DEFAULT 流程并保存）
        /// </summary>
        private (string Solution, string Flow, bool IsSwitch) ResolveSolution(
            RootPanelInfo panelInfo, PanelConvertContext context)
        {
            var solConfig = context.SolutionConfig;

            // 查找料号所属的算法流程配置
            // 料号未配置，自动归入 DEFAULT 流程
            var pipeline = (solConfig?.FindPipelineByProduct(panelInfo.ProductSerial)) ?? AutoAddProductSerial(panelInfo.ProductSerial, context);
            string solution, flow;
            (solution, flow) = panelInfo.SideIndex == "A"
                ? (pipeline.Asolution, pipeline.Aflow)
                : (pipeline.Bsolution, pipeline.Bflow);

            LogTextHelper.Info($"当前产品:{panelInfo.SerialNumber},{panelInfo.SideIndex}面,所属料号:{panelInfo.ProductSerial},流程:{pipeline.Name},方案:{solution},flow:{flow}");
            return (solution, flow, pipeline.IsSwitch);
        }

        /// <summary>
        /// 将未配置的料号自动添加到 DEFAULT 流程下，并持久化保存
        /// </summary>
        private PipelineFlowConfig AutoAddProductSerial(string productSerial, PanelConvertContext context)
        {
            var solConfig = context.SolutionConfig;

            lock (_solutionLock)
            {
                // 双重检查：可能其他线程已经添加
                var existing = solConfig?.FindPipelineByProduct(productSerial);
                if (existing != null) return existing;

                var defaultPipeline = (solConfig?.GetDefaultPipeline()) ?? throw new Exception("找不到 DEFAULT 的算法流程配置，无法自动新增料号");
                if (defaultPipeline.ProductSerials == null)
                    defaultPipeline.ProductSerials = new List<string>();

                defaultPipeline.ProductSerials.Add(productSerial);

                LogTextHelper.Info($"料号 {productSerial} 未配置，已自动归入 DEFAULT 流程");

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

                return defaultPipeline;
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
            List<int> pcsList,
            List<int> directReportDefectList,
            List<int> directReportPcsList)
        {
            var vBInfo = CreateBaseVBInfo(solutionInfo.Solution, solutionInfo.Flow);

            // 处理 PCS 信息
            ProcessPcsInfo(panelInfo, context, solutionInfo.IsSwitch, vBInfo, defectList, pcsList,
                directReportDefectList, directReportPcsList);
           
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
            List<int> pcsList,
            List<int> directReportDefectList,
            List<int> directReportPcsList)
        {
            int defectCount = 0;
            foreach (var item in panelInfo.PcsInfo)
            {
                for (int j = 0; j < item.Value.DefectInfo.Count; j++)
                {
                    defectCount++;
                    var defect = item.Value.DefectInfo[j];

                    // 检查是否为直报缺陷（根据料号对应的 profile）
                    if (KeyDefectConfigManager.Instance.IsDirectReportByProduct(defect.DefectCode, panelInfo.ProductSerial))
                    {
                        directReportDefectList.Add(j);
                        directReportPcsList.Add(defect.PcsIndex);
                        LogTextHelper.Info($"SN:{panelInfo.SerialNumber} 缺陷 {defect.DefectCode} (Pcs:{defect.PcsIndex}, Defect:{j}) 标记为直报，跳过AI推理");
                        continue;
                    }

                    // 创建推理图片组
                    var group = CreateInferImageGroup(panelInfo, isSwitch, defect);

                    // 添加图片信息
                    AddDefectImages(context, defect, group);

                    vBInfo.paramsData.InferWholeData.ImageData.DataValue.InferImageGroup.Add(group);
                    group.inspectDetails = new InspectDetails { InferRois = new List<InferRoi>() };

                    defectList.Add(j);
                    pcsList.Add(defect.PcsIndex);
                }
            }
            LogTextHelper.Info($"SN:{panelInfo.SerialNumber}_{panelInfo.SideIndex}面报点数据为:{defectCount}");

        }

        /// <summary>
        /// 创建推理图片组
        /// </summary>
        private InferImageGroup CreateInferImageGroup(
            RootPanelInfo panelInfo,
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


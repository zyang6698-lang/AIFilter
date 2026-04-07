using DeepSightDB;
using DeepSightModel;
using DeepSightModel.Configuration;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// VBModel 统一构建器 - 用于一致性测试、二次推理、单图测试
    /// </summary>
    public class VBModelBuilder
    {
        private readonly SolutionConfig _solutionConfig;
        private static MinioSettings MinioSettingsConfig => MinioSettings.Instance;

        public VBModelBuilder(SolutionConfig solutionConfig)
        {
            _solutionConfig = solutionConfig;
        }

        /// <summary>
        /// 构建推理用的 VBModel（根据配置跳过直报缺陷）
        /// </summary>
        public VBModel Build(VBModelBuildContext context)
        {
            if (context.DefectPoints == null || context.DefectPoints.Count == 0)
                return null;

            var imageKeys = new List<string>();
            var defectIndexList = new List<int>();
            var pcsIndexList = new List<int>();
            var originalAIResults = new Dictionary<int, int>();
            var originalVVSResults = new Dictionary<int, int>();
            var originalDetectInfos = new Dictionary<int, DetectInfo>();
            var directReportFlags = new List<bool>();

            // 全量路径（包含直报），用于后续保存完整缺陷信息
            var allDefectImageKeys = new List<string>();
            var allDefectGerberKeys = new List<string>();
            var allDefectTempKeys = new List<string>();
            var allDefectCodes = new List<string>();

            // 检查是否有VVS数据
            bool hasVVSData = context.Side?.VvsState > 0 || context.DefectPoints.Any(d => d.VVSStatus > 0);

            for (int i = 0; i < context.DefectPoints.Count; i++)
            {
                var defect = context.DefectPoints[i];
                string aviDefectCode = !string.IsNullOrWhiteSpace(defect.DefectName)
                    ? defect.DefectName
                    : defect.DefectType ?? "";

                var defectClone = defect.Clone();
                defectClone.DefectName = aviDefectCode;
                originalDetectInfos[i] = defectClone;

                // 记录全量路径和缺陷名（验证流中 DefectName 即为原始 AVI 报码）
                allDefectImageKeys.Add(defect.ImagePath ?? "");
                allDefectGerberKeys.Add(defect.GerberImagePath ?? "");
                allDefectTempKeys.Add(defect.TempImagePath ?? "");
                allDefectCodes.Add(aviDefectCode);

                // 根据配置判断是否为直报缺陷（与主流程一致）
                bool isDirectReport = KeyDefectConfigManager.Instance.IsDirectReportByProduct(
                    aviDefectCode, context.ProductSerial);
                directReportFlags.Add(isDirectReport);

                if (isDirectReport || string.IsNullOrEmpty(defect.ImagePath))
                    continue;

                imageKeys.Add(defect.ImagePath);
                defectIndexList.Add(i);
                pcsIndexList.Add(i);
                originalAIResults[i] = defectClone.AIStatus;
                originalVVSResults[i] = defectClone.VVSStatus;
            }

            // 全部直报或无有效图片时，仍返回 VBModel（ImageKeys 为空），
            // 由调用方区分"全部直报跳过"和"真无数据"
            RootVBInfo vbInfo = null;
            if (imageKeys.Count > 0)
            {
                // 构建非直报缺陷子列表，用于 VBInfo 构建
                var filteredDefects = defectIndexList.Select(idx => context.DefectPoints[idx]).ToList();
                vbInfo = BuildVBInfo(context, imageKeys, filteredDefects);
            }

            return new VBModel
            {
                Key = $"{GetModePrefix(context.Mode)}_{context.TaskId}_{context.SerialNumber}_{context.SideName}",
                SN = context.SerialNumber,
                Side = context.SideName,
                DefectIndex = defectIndexList,
                PcsIndex = pcsIndexList,
                ImageKeys = imageKeys,
                VbInfo = vbInfo,
                IsValidationTest = true,
                InferenceMode = context.Mode,
                OriginalAIResults = originalAIResults,
                OriginalVVSResults = originalVVSResults,
                HasVVSData = hasVVSData,
                OriginalDetectInfos = originalDetectInfos,
                TestTaskId = context.TaskId,
                AllDefectImageKeys = allDefectImageKeys,
                AllDefectGerberKeys = allDefectGerberKeys,
                AllDefectTempKeys = allDefectTempKeys,
                DirectReportFlags = directReportFlags,
                AllDefectCodes = allDefectCodes
            };
        }

        private string GetModePrefix(InferenceMode mode)
        {
            switch (mode)
            {
                case InferenceMode.ConsistencyTest: return "TEST";
                case InferenceMode.SecondaryInference: return "SECONDARY";
                case InferenceMode.SingleImageTest: return "SINGLE";
                default: return "UNKNOWN";
            }
        }

        /// <summary>
        /// 构建 VBInfo
        /// </summary>
        private RootVBInfo BuildVBInfo(VBModelBuildContext context, List<string> imageKeys, List<DetectInfo> filteredDefects)
        {
            // 从配置中查找料号所属的算法流程，找不到则使用 DEFAULT
            var pipeline = _solutionConfig?.FindPipelineByProduct(context.ProductSerial)
                ?? _solutionConfig?.GetDefaultPipeline();

            string solution = context.SideName == "A" ? (pipeline?.Asolution ?? "default") : (pipeline?.Bsolution ?? "default");
            string flow = context.SideName == "A" ? (pipeline?.Aflow ?? "1") : (pipeline?.Bflow ?? "1");

            var vbInfo = new RootVBInfo
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
                        },
                        OtherInfos = new Others
                        {
                            imageminio = new ImageMminio
                            {
                                access_key_id = MinioSettingsConfig.AccessKey,
                                bucket = MinioSettingsConfig.DefaultBucket,
                                endpoint_url = "",
                                secret_key = MinioSettingsConfig.SecretKey,
                                secret_port = MinioSettingsConfig.DefaultPort
                            }
                        }
                    }
                }
            };

            // 构造模板图片路径
            string tempImgPath = BuildTemplateImagePath(context.ProductSerial, context.MachineId, context.SideName);

            // 添加图片信息（filteredDefects 与 imageKeys 一一对应，均已排除直报缺陷）
            for (int i = 0; i < imageKeys.Count && i < filteredDefects.Count; i++)
            {
                var defect = filteredDefects[i];
                var group = BuildImageGroup(defect, tempImgPath, context);
                vbInfo.paramsData.InferWholeData.ImageData.DataValue.InferImageGroup.Add(group);
            }

            return vbInfo;
        }

        /// <summary>
        /// 构建单个图片组
        /// </summary>
        private InferImageGroup BuildImageGroup(DetectInfo defect, string tempImgPath, VBModelBuildContext context)
        {
            // 获取ROI信息 - 优先使用非Origin的ROI值
            int roiX, roiY, roiW, roiH;
            if (defect.Width > 0 && defect.Height > 0)
            {
                roiX = defect.RoiX;
                roiY = defect.RoiY;
                roiW = defect.Width;
                roiH = defect.Height;
            }
            else
            {
                roiX = defect.OriginRoiX;
                roiY = defect.OriginRoiY;
                roiW = defect.OriginWidth;
                roiH = defect.OriginHeight;
            }

            string defectMinioPath = ExtractMinioPath(defect.ImagePath);
            string templateMinioPath = BuildTemplateMinioPath(defectMinioPath);

            return new InferImageGroup
            {
                GroupUuid = Guid.NewGuid().ToString(),
                GroupInfos = new List<GroupInfo>
                {
                    new GroupInfo
                    {
                        ImagePath = defectMinioPath,
                        ImageUuid = Guid.NewGuid().ToString(),
                        ImageType = "defect"
                    },
                    new GroupInfo
                    {
                        ImagePath = templateMinioPath,
                        ImageUuid = Guid.NewGuid().ToString(),
                        ImageType = "template"
                    }
                },
                MachineTemplateInfo = new MachineTemplateInfo
                {
                    MachineName = context.MachineId,
                    product = context.ProductSerial,
                    Side = context.SideName
                },
                ImgROI = new List<int> { roiX, roiY, roiW, roiH },
                DefectCode = defect.DefectName ?? defect.DefectType ?? "",
                TempImgPath = tempImgPath,
                inspectDetails = new InspectDetails { InferRois = new List<InferRoi>() }
            };
        }

        /// <summary>
        /// 构造模板图片本地路径
        /// </summary>
        public string BuildTemplateImagePath(string productSerial, string machineName, string side)
        {
            if (string.IsNullOrEmpty(productSerial) || string.IsNullOrEmpty(machineName))
                return "";

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string parentDir = Directory.GetParent(baseDir.TrimEnd('\\')).FullName;
            return Path.Combine(parentDir, "TemplateImages", productSerial, machineName, $"{productSerial}[{side}].jpg");
        }

        /// <summary>
        /// 从defect图片路径构造template图片路径（在文件名后添加[E]）
        /// </summary>
        public string BuildTemplateMinioPath(string defectPath)
        {
            if (string.IsNullOrEmpty(defectPath))
                return "";

            int lastDotIndex = defectPath.LastIndexOf('.');
            if (lastDotIndex > 0)
                return defectPath.Substring(0, lastDotIndex) + "[E]" + defectPath.Substring(lastDotIndex);

            return defectPath + "[E]";
        }

        /// <summary>
        /// 从完整路径提取 Minio 相对路径
        /// </summary>
        public string ExtractMinioPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return fullPath;

            int index = fullPath.IndexOf("deepiresults", StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                var relativePath = fullPath.Substring(index + "deepiresults".Length).TrimStart('\\', '/');
                return relativePath.Replace('\\', '/');
            }

            if (fullPath.Contains(":"))
            {
                var parts = fullPath.Split(':');
                if (parts.Length >= 2)
                    return parts[1];
            }

            return fullPath;
        }

    }

    /// <summary>
    /// VBModel 构建上下文
    /// </summary>
    public class VBModelBuildContext
    {
        public InferenceMode Mode { get; set; }
        public string TaskId { get; set; }
        public string SerialNumber { get; set; }
        public string SideName { get; set; }
        public string ProductSerial { get; set; }
        public string MachineId { get; set; }
        public SideData Side { get; set; }
        public List<DetectInfo> DefectPoints { get; set; }
    }
}


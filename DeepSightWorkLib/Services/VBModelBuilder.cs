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
        /// 构建推理用的 VBModel
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
            var originalDetectInfos = new Dictionary<int, object>();

            // 检查是否有VVS数据
            bool hasVVSData = context.Side?.VvsState > 0 || context.DefectPoints.Any(d => d.VVSStatus > 0);

            for (int i = 0; i < context.DefectPoints.Count; i++)
            {
                var defect = context.DefectPoints[i];
                if (string.IsNullOrEmpty(defect.ImagePath)) continue;

                imageKeys.Add(defect.ImagePath);
                defectIndexList.Add(i);
                pcsIndexList.Add(i);
                originalAIResults[i] = defect.AIStatus;
                originalVVSResults[i] = defect.VVSStatus;
                originalDetectInfos[i] = defect;
            }

            if (imageKeys.Count == 0) return null;

            // 构建 VBInfo
            var vbInfo = BuildVBInfo(context, imageKeys);

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
                TestTaskId = context.TaskId
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
        private RootVBInfo BuildVBInfo(VBModelBuildContext context, List<string> imageKeys)
        {
            // 从配置中查找料号对应的方案和流程
            var solutionFlow = _solutionConfig?.solus?.FirstOrDefault(o => o.ProductSerial == context.ProductSerial)
                ?? _solutionConfig?.solus?.FirstOrDefault(o => o.ProductSerial?.ToUpper() == "DEFAULT");

            string solution = context.SideName == "A" ? (solutionFlow?.Asolution ?? "default") : (solutionFlow?.Bsolution ?? "default");
            string flow = context.SideName == "A" ? (solutionFlow?.Aflow ?? "1") : (solutionFlow?.Bflow ?? "1");

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

            // 添加图片信息
            for (int i = 0; i < imageKeys.Count && i < context.DefectPoints.Count; i++)
            {
                var defect = context.DefectPoints[i];
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
                DefectCode = defect.DefectType ?? "",
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

        /// <summary>
        /// 加载测试用图片
        /// </summary>
        public List<Mat> LoadImages(List<DetectInfo> defects)
        {
            var mats = new List<Mat>();
            foreach (var defect in defects)
            {
                if (string.IsNullOrEmpty(defect.ImagePath)) continue;

                try
                {
                    var mat = Cv2.ImRead(defect.ImagePath);
                    if (mat != null && !mat.Empty())
                        mats.Add(mat);
                }
                catch (Exception ex)
                {
                    DeepSightTool.LogTextHelper.Warn($"加载测试图片失败: {defect.ImagePath}, {ex.Message}");
                }
            }
            return mats;
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


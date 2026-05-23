using DeepSightCommunication;
using DeepSightModel;
using DeepSightModel.Configuration;
using DeepSightTool;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 从 Minio 加载图片并转换为 OpenCvSharp.Mat 的服务
    /// </summary>
    public class ImageLoaderService
    {
        private readonly MinioClass _minio;

        public ImageLoaderService(MinioClass minio)
        {
            _minio = minio ?? throw new ArgumentNullException(nameof(minio));
        }

        /// <summary>
        /// 尝试从 "endpoint:objectKey" 格式的路径加载图片并返回 Mat，失败返回 null
        /// </summary>
        public Mat LoadMinioImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            try
            {
                var parts = path.Split(':');
                if (parts.Length < 2)
                {
                    LogTextHelper.Warn("图片路径格式错误(缺少冒号)：" + path);
                    return null;
                }
                using (var stream = _minio.GetImageStreamSync("deepiresults", parts[1], parts[0]))
                {
                    if (stream == null || stream.Length == 0)
                    {
                        LogTextHelper.Warn("Minio返回空图片数据：" + path);
                        return null;
                    }
                    return Cv2.ImDecode(stream.ToArray(), ImreadModes.Color);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("加载 Minio 图片异常：" + ex);
                return null;
            }
        }

        /// <summary>
        /// 并行加载多张图片，返回非空 Mat 列表
        /// </summary>
        public List<Mat> LoadImages(IEnumerable<string> paths)
        {
            if (paths == null) return new List<Mat>();
            try
            {
                var list = paths
                    .AsParallel()
                    .AsOrdered()
                    .Select(p => LoadMinioImage(p))
                    .Where(m => m != null)
                    .ToList();
                return list;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("LoadImages 异常：" + ex);
                return new List<Mat>();
            }
        }

        /// <summary>
        /// 构建所有缺陷（包含直报）的三类图片路径、直报标记列表和 AVI 原始报码。
        /// 返回的六个列表索引对齐：AllImageKeys[i]、AllGerberKeys[i]、AllTempKeys[i]、DirectReportFlags[i]、AllDefectCodes[i]、GlobalFlags[i] 对应同一个缺陷。
        /// 迭代顺序：先 PcsInfo（pcs_info），后 PanelInfo（panel_info，全局点）。
        /// </summary>
        public (List<string> AllImageKeys, List<string> AllGerberKeys, List<string> AllTempKeys, List<string> AllAviKeys, List<bool> DirectReportFlags, List<string> AllDefectCodes, List<bool> GlobalFlags)
            GetAllImageKeysWithDirectReportFlags(RootPanelInfo panel, string ip, string head)
        {
            var allImageKeys = new List<string>();
            var allGerberKeys = new List<string>();
            var allTempKeys = new List<string>();
            var allAviKeys = new List<string>();
            var flags = new List<bool>();
            var allDefectCodes = new List<string>();
            var globalFlags = new List<bool>();

            try
            {
                if (panel == null || string.IsNullOrWhiteSpace(ip))
                {
                    LogTextHelper.Warn("GetAllImageKeysWithDirectReportFlags: 参数为空或 IP 缺失！");
                    return (allImageKeys, allGerberKeys, allTempKeys, allAviKeys, flags, allDefectCodes, globalFlags);
                }

                AppendDefects(panel.PcsInfo?.Values, panel.ProductSerial, ip, head, isGlobal: false,
                    allImageKeys, allGerberKeys, allTempKeys, allAviKeys, flags, allDefectCodes, globalFlags);
                AppendDefects(panel.PanelInfo != null ? new[] { panel.PanelInfo } : null,
                    panel.ProductSerial, ip, head, isGlobal: true,
                    allImageKeys, allGerberKeys, allTempKeys, allAviKeys, flags, allDefectCodes, globalFlags);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("GetAllImageKeysWithDirectReportFlags 异常：" + ex);
            }

            return (allImageKeys, allGerberKeys, allTempKeys, allAviKeys, flags, allDefectCodes, globalFlags);
        }

        /// <summary>
        /// 将一组 PcsInfo（pcs_info 的值集合或 panel_info 单元素）的缺陷追加到对齐列表中
        /// </summary>
        private static void AppendDefects(
            IEnumerable<PcsInfo> source,
            string productSerial, string ip, string head, bool isGlobal,
            List<string> allImageKeys, List<string> allGerberKeys, List<string> allTempKeys,
            List<string> allAviKeys, List<bool> flags, List<string> allDefectCodes, List<bool> globalFlags)
        {
            if (source == null) return;

            foreach (var pcs in source)
            {
                if (pcs == null || pcs.DefectInfo == null || pcs.DefectInfo.Count == 0)
                    continue;

                for (int j = 0; j < pcs.DefectInfo.Count; j++)
                {
                    var defect = pcs.DefectInfo[j];
                    if (defect == null) continue;

                    bool isDirectReport = KeyDefectConfigManager.Instance
                        .IsDirectReportByProduct(defect.DefectCode, productSerial);

                    // VRS 图片路径（如果有多张取第一张，与 AddImages 逻辑保持一致）
                    string imgKey = BuildFirstImageKey(defect.DefectVrsImages, ip, head);
                    string gerberKey = BuildFirstImageKey(defect.DefectVrsGerberImages, ip, head);
                    string tempKey = BuildFirstImageKey(defect.DefectVrsOkImages, ip, head);
                    string aviKey = BuildFirstImageKey(defect.DefectAviImages, ip, head);

                    allImageKeys.Add(imgKey);
                    allGerberKeys.Add(gerberKey);
                    allTempKeys.Add(tempKey);
                    allAviKeys.Add(aviKey);
                    flags.Add(isDirectReport);
                    allDefectCodes.Add(defect.DefectCode ?? "");
                    globalFlags.Add(isGlobal);
                }
            }
        }

        /// <summary>
        /// 从全量列表 + 直报标记中派生出仅非直报缺陷的过滤列表
        /// </summary>
        public static (List<string> ImageKeys, List<string> GerberKeys, List<string> TempKeys) DeriveFilteredKeys(
            List<string> allImageKeys, List<string> allGerberKeys, List<string> allTempKeys, List<bool> directReportFlags)
        {
            var imageKeys = new List<string>();
            var gerberKeys = new List<string>();
            var tempKeys = new List<string>();

            if (allImageKeys == null || allImageKeys.Count == 0)
                return (imageKeys, gerberKeys, tempKeys);

            for (int i = 0; i < allImageKeys.Count; i++)
            {
                bool isDirectReport = directReportFlags != null && i < directReportFlags.Count && directReportFlags[i];
                if (isDirectReport) continue;

                imageKeys.Add(allImageKeys[i]);
                gerberKeys.Add(allGerberKeys != null && i < allGerberKeys.Count ? allGerberKeys[i] : "");
                tempKeys.Add(allTempKeys != null && i < allTempKeys.Count ? allTempKeys[i] : "");
            }

            return (imageKeys, gerberKeys, tempKeys);
        }

        /// <summary>
        /// 构建第一张图片的 endpoint:objectKey 路径，无图则返回空字符串
        /// </summary>
        private static string BuildFirstImageKey(List<string> images, string endpoint, string prefix)
        {
            if (images == null || images.Count == 0) return "";
            var rel = images[0];
            if (string.IsNullOrWhiteSpace(rel)) return "";
            var normalizedRel = rel.Replace('\\', '/').TrimStart('/');
            return $"{endpoint}:{prefix}/{normalizedRel}";
        }
    }
}

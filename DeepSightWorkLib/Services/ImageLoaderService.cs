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
                    return null;
                }
                using (var stream = _minio.GetImageStreamSync("deepiresults", parts[1], parts[0]))
                {
                    if (stream == null || stream.Length == 0)
                    {
                        return null;
                    }
                    return Cv2.ImDecode(stream.ToArray(), ImreadModes.Color);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Debug("加载 Minio 图片异常：" + ex);
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
                var pathList = paths.ToList();
                var results = pathList
                    .AsParallel()
                    .AsOrdered()
                    .Select(p => new { Path = p, Mat = LoadMinioImage(p) })
                    .ToList();

                int failCount = results.Count(r => r.Mat == null);
                if (failCount > 0)
                {
                    var failedPaths = results
                        .Where(r => r.Mat == null)
                        .Select(r => string.IsNullOrWhiteSpace(r.Path) ? "<空路径>" : r.Path)
                        .Take(5)
                        .ToList();
                    string moreText = failCount > failedPaths.Count ? $" 等{failCount}张" : string.Empty;
                    LogTextHelper.WarnFormat("图片加载失败 {0}/{1} 张，失败路径: {2}{3}", failCount, results.Count, string.Join(" | ", failedPaths), moreText);
                }

                return results.Where(r => r.Mat != null).Select(r => r.Mat).ToList();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("LoadImages 异常：" + ex);
                return new List<Mat>();
            }
        }

        public static List<DetectInfo> BuildAllDefectInfos(RootPanelInfo panel, string ip, string head)
        {
            var defects = new List<DetectInfo>();
            if (panel == null || string.IsNullOrWhiteSpace(ip)) return defects;

            AppendDetectInfos(panel.PcsInfo?.Values, panel.ProductSerial, ip, head, false, defects);
            AppendDetectInfos(panel.PanelInfo != null ? new[] { panel.PanelInfo } : null,
                panel.ProductSerial, ip, head, true, defects);
            return defects;
        }

        private static void AppendDetectInfos(
            IEnumerable<PcsInfo> source,
            string productSerial, string ip, string head, bool isGlobal,
            List<DetectInfo> defects)
        {
            if (source == null) return;

            foreach (var pcs in source)
            {
                if (pcs == null || pcs.DefectInfo == null || pcs.DefectInfo.Count == 0)
                    continue;

                foreach (var defect in pcs.DefectInfo)
                {
                    if (defect == null) continue;

                    string defectCode = defect.DefectCode ?? "";
                    bool isDirectReport = KeyDefectConfigManager.Instance.IsDirectReportByProduct(defectCode, productSerial);
                    var roi = defect.DefectRoi;
                    var originRoi = defect.DefectOriginRoi ?? defect.DefectRoi;

                    defects.Add(new DetectInfo
                    {
                        OriginDefectName = defectCode,
                        DefectName = defectCode,
                        DefectType = defect.DefectLabel,
                        RoiX = roi?.X ?? 0,
                        RoiY = roi?.Y ?? 0,
                        Width = roi?.Width ?? 0,
                        Height = roi?.Height ?? 0,
                        OriginRoiX = originRoi?.X ?? 0,
                        OriginRoiY = originRoi?.Y ?? 0,
                        OriginWidth = originRoi?.Width ?? 0,
                        OriginHeight = originRoi?.Height ?? 0,
                        ImagePath = BuildFirstImageKey(defect.DefectVrsImages, ip, head),
                        GerberImagePath = BuildFirstImageKey(defect.DefectVrsGerberImages, ip, head),
                        TempImagePath = BuildFirstImageKey(defect.DefectVrsOkImages, ip, head),
                        DefectAviImage = BuildFirstImageKey(defect.DefectAviImages, ip, head),
                        DefectIndex = defect.DefectIndex,
                        PcsIndex = defect.PcsIndex,
                        AIStatus = isDirectReport ? 4 : 0,
                        IsGlobal = isGlobal
                    });
                }
            }
        }

        /// <summary>
        /// 从全量缺陷中派生出仅非直报缺陷的过滤图片路径列表
        /// </summary>
        public static (List<string> ImageKeys, List<string> GerberKeys, List<string> TempKeys) DeriveFilteredKeys(
            List<DetectInfo> allDefectInfos)
        {
            var imageKeys = new List<string>();
            var gerberKeys = new List<string>();
            var tempKeys = new List<string>();

            if (allDefectInfos == null || allDefectInfos.Count == 0)
                return (imageKeys, gerberKeys, tempKeys);

            foreach (var defect in allDefectInfos)
            {
                if (defect == null || defect.AIStatus == 4) continue;

                imageKeys.Add(defect.ImagePath ?? "");
                gerberKeys.Add(defect.GerberImagePath ?? "");
                tempKeys.Add(defect.TempImagePath ?? "");
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

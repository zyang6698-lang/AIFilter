using DeepSightCommunication;
using DeepSightTool;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using DeepSightModel;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 负责从 Minio 下载图片并解码为 OpenCvSharp.Mat 的服务
    /// </summary>
    public class ImageLoaderService
    {
        private readonly MinioClass _minio;

        public ImageLoaderService(MinioClass minio)
        {
            _minio = minio ?? throw new ArgumentNullException(nameof(minio));
        }

        /// <summary>
        /// 从自定义的 "endpoint:objectKey" 格式加载单张图片并返回 Mat，失败返回 null
        /// </summary>
        public Mat LoadMinioImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            try
            {
                var parts = path.Split(':');
                if (parts.Length < 2)
                {
                    LogTextHelper.Warn("图片路径格式错误(需包含冒号)：" + path);
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
        /// 并行加载多张图片并返回非空 Mat 列表
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
        /// Build list of Minio image keys (endpoint:objectKey) from panel info.
        /// This centralizes logic previously in BusinessClass.GetAllMinioImageKeys
        /// </summary>
        public List<string> GetAllMinioImageKeys(RootPanelInfoWithIP info)
        {
            var results = new List<string>();
            try
            {
                if (info == null || info.rootInfo == null || string.IsNullOrWhiteSpace(info.IP))
                {
                    LogTextHelper.Warn("GetAllMinioImageKeys: 参数为空或 IP 缺失。");
                    return results;
                }

                var panel = info.rootInfo;

                string head = ExtractHeadFromLocalDescribeDir(panel.LocalDescribeDir);
                if (string.IsNullOrWhiteSpace(head))
                {
                    LogTextHelper.Warn($"GetAllMinioImageKeys: 无法从 LocalDescribeDir 解析 head。LocalDescribeDir={panel.LocalDescribeDir}");
                    return results;
                }

                if (panel.PcsInfo == null || panel.PcsInfo.Count == 0)
                {
                    LogTextHelper.Info($"GetAllMinioImageKeys: PcsInfo 为空。SN={panel.SerialNumber}");
                    return results;
                }

                foreach (var kvp in panel.PcsInfo)
                {
                    var pcs = kvp.Value;
                    if (pcs == null || pcs.DefectInfo == null || pcs.DefectInfo.Count == 0)
                    {
                        continue;
                    }

                    for (int j = 0; j < pcs.DefectInfo.Count; j++)
                    {
                        var defect = pcs.DefectInfo[j];
                        if (defect == null)
                            continue;

                        AddImages(defect.DefectVrsImages, info.IP, head, results);
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("GetAllMinioImageKeys 异常：" + ex);
            }

            return results;

            void AddImages(List<string> images, string endpoint, string prefix, List<string> output)
            {
                if (images == null || images.Count == 0) return;
                foreach (var rel in images)
                {
                    if (string.IsNullOrWhiteSpace(rel)) continue;
                    var normalizedRel = rel.Replace('\\', '/').TrimStart('/');
                    var objectKey = $"{prefix}/{normalizedRel}";
                    output.Add($"{endpoint}:{objectKey}");
                }
            }
        }

        private string ExtractHeadFromLocalDescribeDir(string localDescribeDir)
        {
            if (string.IsNullOrWhiteSpace(localDescribeDir)) return string.Empty;

            try
            {
                var normalized = localDescribeDir.Replace('\\', '/');
                var idx = normalized.IndexOf("deepiresults", StringComparison.OrdinalIgnoreCase);
                if (idx < 0) return string.Empty;
                var after = normalized.Substring(idx + "deepiresults".Length).Trim('/');
                return after;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}

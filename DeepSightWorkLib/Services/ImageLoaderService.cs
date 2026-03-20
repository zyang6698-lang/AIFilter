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
        /// 从面板信息构建 Minio 图片键列表（endpoint:objectKey 格式）
        /// 此方法集中了原来 BusinessClass.GetAllMinioImageKeys 中的逻辑
        /// </summary>
        public List<string> GetAllMinioImageKeys(RootPanelInfoWithIP info)
        {
            var results = new List<string>();
            try
            {
                if (info == null || info.RootInfo == null || string.IsNullOrWhiteSpace(info.IP))
                {
                    LogTextHelper.Warn("GetAllMinioImageKeys: 参数为空或 IP 缺失！");
                    return results;
                }

                var panel = info.RootInfo;


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

                        AddImages(defect.DefectVrsImages, info.IP, info.Head, results);
                       // AddImages(defect.DefectVrsOkImages,info.IP, info.Head, results) ;
                       // AddImages(defect.DefectVrsGerberImages, info.IP, info.Head, results);
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("GetAllMinioImageKeys 异常：" + ex);
            }

            return results;
        }

        /// <summary>
        /// 从面板信息构建 Gerber 图片键列表（endpoint:objectKey 格式）
        /// </summary>
        public List<string> GetAllMinioGerberImageKeys(RootPanelInfoWithIP info)
        {
            return GetImageKeysByType(info, defect => defect.DefectVrsGerberImages, "GetAllMinioGerberImageKeys");
        }

        /// <summary>
        /// 从面板信息构建 Template 图片键列表（endpoint:objectKey 格式）
        /// </summary>
        public List<string> GetAllMinioTemplateImageKeys(RootPanelInfoWithIP info)
        {
            return GetImageKeysByType(info, defect => defect.DefectVrsOkImages, "GetAllMinioTemplateImageKeys");
        }

        /// <summary>
        /// 通用方法：按类型提取图片键列表
        /// </summary>
        private List<string> GetImageKeysByType(RootPanelInfoWithIP info, Func<DefectInfo, List<string>> imageSelector, string methodName)
        {
            var results = new List<string>();
            try
            {
                if (info == null || info.RootInfo == null || string.IsNullOrWhiteSpace(info.IP))
                {
                    LogTextHelper.Warn($"{methodName}: 参数为空或 IP 缺失！");
                    return results;
                }

                var panel = info.RootInfo;

                if (panel.PcsInfo == null || panel.PcsInfo.Count == 0)
                {
                    return results;
                }

                foreach (var kvp in panel.PcsInfo)
                {
                    var pcs = kvp.Value;
                    if (pcs == null || pcs.DefectInfo == null || pcs.DefectInfo.Count == 0)
                        continue;

                    for (int j = 0; j < pcs.DefectInfo.Count; j++)
                    {
                        var defect = pcs.DefectInfo[j];
                        if (defect == null) continue;

                        var images = imageSelector(defect);
                        AddImages(images, info.IP, info.Head, results);
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"{methodName} 异常：" + ex);
            }

            return results;
        }

        private static void AddImages(List<string> images, string endpoint, string prefix, List<string> output)
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
}

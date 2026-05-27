using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeepSightWorkLib.Services.Pipeline.Stages
{
    /// <summary>
    /// 阶段2: 图片加载 — 从 MinIO 加载缺陷图片
    /// 对应原 BusinessClass.WorkerImageLoad
    /// </summary>
    public class ImageLoadStage
    {
        private readonly ImageLoaderService _imageLoaderService;
        private readonly Func<bool> _useGerberImageProvider;

        public ImageLoadStage(ImageLoaderService imageLoaderService, Func<bool> useGerberImageProvider = null)
        {
            _imageLoaderService = imageLoaderService ?? throw new ArgumentNullException(nameof(imageLoaderService));
            _useGerberImageProvider = useGerberImageProvider ?? (() => false);
        }

        /// <summary>
        /// 执行图片加载阶段（供 Pipeline TransformBlock 使用）
        /// </summary>
        public PipelineContext Execute(PipelineContext ctx)
        {
            var loadModel = ctx.LoadModel;
            var model = loadModel.Model;

            // 如果图片已预加载（离线/验证测试任务已在投递前完成图片加载），跳过加载阶段
            if (model.Mats != null && model.Mats.Count > 0)
            {
                LogTextHelper.Info($"SN:{model.SN} 图片已预加载({model.Mats.Count}张)，跳过图片加载阶段（离线任务）");
                return ctx;
            }

            bool useGerber = _useGerberImageProvider();

            LogTextHelper.Info($"开始加载图片，SN:{model.SN}，数量：{model.ImageKeys.Count}，图片类型：{(useGerber ? "Gerber" : "Template")}");
            TaskStatusSender.SendLoadingImages(model.SN, model.Side);

            // 缺陷图与参考图（Template/Gerber）并行加载
            var refKeys = useGerber ? model.ImageKeys_Gerber : model.ImageKeys_Temp;
            List<Mat> defectMats = null;
            List<Mat> refMats = null;

            Parallel.Invoke(
                () => defectMats = _imageLoaderService.LoadImages(model.ImageKeys),
                () => refMats = _imageLoaderService.LoadImages(refKeys)
            );

            model.Mats = defectMats;

            if (useGerber)
            {
                model.Mats_Gerber = refMats;
                model.Mats_Temp = refMats;
                int defectCount = model.ImageKeys?.Count ?? 0;
                int expectedGerberCount = model.ImageKeys_Gerber?.Count ?? 0;
                int actualGerberCount = model.Mats_Gerber?.Count ?? 0;

                if (defectCount > 0 && actualGerberCount < Math.Max(1, expectedGerberCount))
                {
                    LogTextHelper.WarnFormat("Gerber参考图缺失或加载失败 SN={0} Side={1}，当前配置仅使用Gerber参考图，期望:{2}，实际:{3}", model.SN, model.Side, expectedGerberCount, actualGerberCount);
                }
            }
            else
            {
                model.Mats_Temp = refMats;
                int defectCount = model.ImageKeys?.Count ?? 0;
                int expectedTemplateCount = model.ImageKeys_Temp?.Count ?? 0;
                int actualTemplateCount = model.Mats_Temp?.Count ?? 0;

                if (defectCount > 0 && actualTemplateCount < Math.Max(1, expectedTemplateCount))
                {
                    LogTextHelper.WarnFormat("模板参考图缺失或加载失败 SN={0} Side={1}，当前配置不回退Gerber图，期望:{2}，实际:{3}", model.SN, model.Side, expectedTemplateCount, actualTemplateCount);
                }
            }

            LogImageLoadResult(loadModel);

            // 向 UI 发送面板信息
            SystemEvent.SendPanelInfo(model.SN, model.Side, loadModel.PanelView);
            TaskStatusSender.SendImagesLoaded(model.SN, model.Side, model.Mats.Count);

            LogTextHelper.Info($"图片加载完成，SN:{model.SN}，实际加载:{model.Mats.Count}张");
            return ctx;
        }

        private void LogImageLoadResult(ImageLoadModel loadModel)
        {
            int expectedCount = loadModel.Model.ImageKeys.Count;
            int actualCount = loadModel.Model.Mats.Count;

            if (expectedCount == 0)
            {
                LogTextHelper.Info($"SN:{loadModel.Model.SN} 无报点数据，无需加载图片");
            }
            else if (actualCount == 0)
            {
                LogTextHelper.ErrorFormat("图片加载失败 SN={0}，期望{1}张，实际0张", loadModel.Model.SN, expectedCount);
            }
            else if (actualCount < expectedCount)
            {
                LogTextHelper.WarnFormat("图片部分加载失败 SN={0}，期望{1}张，实际{2}张", loadModel.Model.SN, expectedCount, actualCount);
            }
        }
    }
}

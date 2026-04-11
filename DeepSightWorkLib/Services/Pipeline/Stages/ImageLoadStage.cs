using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using System;

namespace DeepSightWorkLib.Services.Pipeline.Stages
{
    /// <summary>
    /// 阶段2: 图片加载 — 从 MinIO 加载缺陷图片
    /// 对应原 BusinessClass.WorkerImageLoad
    /// </summary>
    public class ImageLoadStage
    {
        private readonly ImageLoaderService _imageLoaderService;

        public ImageLoadStage(ImageLoaderService imageLoaderService)
        {
            _imageLoaderService = imageLoaderService ?? throw new ArgumentNullException(nameof(imageLoaderService));
        }

        /// <summary>
        /// 执行图片加载阶段（供 Pipeline TransformBlock 使用）
        /// </summary>
        public PipelineContext Execute(PipelineContext ctx)
        {
            var loadModel = ctx.LoadModel;
            var model = loadModel.Model;

            LogTextHelper.Info($"开始加载图片，SN:{model.SN}，数量：{model.ImageKeys.Count}");
            TaskStatusSender.SendLoadingImages(model.SN, model.Side);

            model.Mats = _imageLoaderService.LoadImages(model.ImageKeys);
            model.Mats_Temp = _imageLoaderService.LoadImages(model.ImageKeys_Temp);

            // 当 temp 图为空时，使用 Gerber 图替代
            if (model.Mats_Temp == null || model.Mats_Temp.Count == 0)
            {
                LogTextHelper.Info($"SN:{model.SN} Temp图为空，使用Gerber图替代");
                model.Mats_Temp = _imageLoaderService.LoadImages(model.ImageKeys_Gerber);
            }

            LogImageLoadResult(loadModel);

            // 向 UI 发送面板信息
            SystemEvent.SendPanelInfo(model.SN, model.Side, loadModel.RootPanelInfo);
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
                LogTextHelper.Error($"图片加载失败，SN:{loadModel.Model.SN}，期望{expectedCount}张图片，实际加载0张！");
            }
            else if (actualCount < expectedCount)
            {
                LogTextHelper.Warn($"图片部分加载失败，SN:{loadModel.Model.SN}，期望{expectedCount}张，实际{actualCount}张");
            }
        }
    }
}

using DeepSightTool;
using System;

namespace DeepSightWorkLib.Services.Pipeline.Stages
{
    /// <summary>
    /// 阶段4b: 后处理 — 解析推理结果并存储到数据库
    /// 对应原 BusinessClass.WorkerPostProcess
    /// </summary>
    public class PostProcessStage
    {
        private readonly PostProcessService _postProcessService;

        public PostProcessStage(PostProcessService postProcessService)
        {
            _postProcessService = postProcessService ?? throw new ArgumentNullException(nameof(postProcessService));
        }

        /// <summary>
        /// 执行后处理阶段（供 Pipeline ActionBlock 使用）
        /// 注意：即使 ctx.HasError 也会被调用，以确保 finally 中 ReleaseDownstreamRef 执行
        /// </summary>
        public void Execute(PipelineContext ctx)
        {
            try
            {
                if (ctx.HasError) return;

                if (ctx.PostProcessModel == null)
                {
                    LogTextHelper.Warn($"SN:{ctx.SN} PostProcessModel 为空，跳过后处理");
                    ctx.SetError("后处理", "PostProcessModel为空");
                    if (!ctx.IsValidationTest)
                        throw new InvalidOperationException("PostProcessModel为空");
                    return;
                }

                _postProcessService.ProcessInferenceResult(ctx.PostProcessModel);
            }
            finally
            {
                // 通知 PipelineContext 此下游已完成（引用计数减1，最后一个完成时触发完成回调并释放 VBModel）
                ctx.ReleaseDownstreamRef();
            }
        }
    }
}

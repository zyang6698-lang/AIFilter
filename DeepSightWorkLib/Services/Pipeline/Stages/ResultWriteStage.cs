using DeepSightTool;
using System;

namespace DeepSightWorkLib.Services.Pipeline.Stages
{
    /// <summary>
    /// 阶段4a: 结果回写 — 将 AI 结果写回 LevelDB
    /// 对应原 BusinessClass.WorkerReturnAVI
    /// 注意：验证测试任务跳过回写
    /// </summary>
    public class ResultWriteStage
    {
        private readonly ResultWriterService _resultWriterService;

        /// <summary>
        /// 创建结果回写阶段
        /// </summary>
        /// <param name="resultWriterService">结果回写服务</param>
        public ResultWriteStage(ResultWriterService resultWriterService)
        {
            _resultWriterService = resultWriterService ?? throw new ArgumentNullException(nameof(resultWriterService));
        }

        /// <summary>
        /// 执行结果回写阶段（供 Pipeline ActionBlock 使用）
        /// 注意：即使 ctx.HasError 也会被调用，以确保 finally 中 ReleaseDownstreamRef 执行
        /// </summary>
        public void Execute(PipelineContext ctx)
        {
            try
            {
                if (ctx.HasError) return;

                // 验证测试任务不需要回写到 LevelDB
                if (ctx.IsValidationTest)
                {
                    LogTextHelper.Info($"[验证测试] SN:{ctx.SN} 跳过结果回写");
                    return;
                }

                if (ctx.AIResult == null)
                {
                    LogTextHelper.Warn($"SN:{ctx.SN} AIResult 为空，跳过回写");
                    return;
                }

                _resultWriterService.ReturnAVIVRS(ctx.AIResult);
            }
            finally
            {
                // 无论回写成功与否，通知 PipelineContext 此下游已完成
                ctx.ReleaseDownstreamRef();
            }
        }
    }
}

using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DeepSightWorkLib.Services.Pipeline.Stages
{
    /// <summary>
    /// 阶段3: AI 推理 — 调用算法进行缺陷检测
    /// 对应原 BusinessClass.WorkerDefect
    /// 注意：不再使用共享的 AIStopwatch，改用 PipelineContext.Stopwatch
    /// 注意：不再直接 Dispose VBModel，由 PipelineContext 引用计数管理
    /// </summary>
    public class InferenceStage
    {
        private readonly DefectProcessor _defectProcessor;
        private readonly Func<int> _maxDefectCountProvider;
        private readonly Func<int> _maxWaitTimeProvider;

        /// <summary>
        /// 创建推理阶段
        /// </summary>
        /// <param name="defectProcessor">缺陷处理器（使用 Infer 方法，不入队）</param>
        /// <param name="maxDefectCountProvider">获取最大缺陷数配置的委托</param>
        /// <param name="maxWaitTimeProvider">获取最大等待时间配置的委托</param>
        public InferenceStage(
            DefectProcessor defectProcessor,
            Func<int> maxDefectCountProvider,
            Func<int> maxWaitTimeProvider)
        {
            _defectProcessor = defectProcessor ?? throw new ArgumentNullException(nameof(defectProcessor));
            _maxDefectCountProvider = maxDefectCountProvider ?? throw new ArgumentNullException(nameof(maxDefectCountProvider));
            _maxWaitTimeProvider = maxWaitTimeProvider ?? throw new ArgumentNullException(nameof(maxWaitTimeProvider));
        }

        /// <summary>
        /// 执行推理阶段（供 Pipeline TransformBlock 使用）
        /// </summary>
        public PipelineContext Execute(PipelineContext ctx)
        {
            var model = ctx.LoadModel.Model;
            string taskPrefix = model.IsValidationTest ? "[验证测试]" : "";

            TaskStatusSender.SendAIDetecting(model.SN, model.Side);

            // 使用局部 Stopwatch 代替共享的 AIStopwatch
            var sw = Stopwatch.StartNew();

            try
            {
                var stageResult = _defectProcessor.Infer(model, _maxDefectCountProvider(), _maxWaitTimeProvider());

                sw.Stop();
                ctx.InferenceElapsedMs = sw.ElapsedMilliseconds;
                ctx.InferenceSuccess = stageResult.Success;
                ctx.InferenceMessages = stageResult.Messages;
                ctx.PostProcessModel = stageResult.PostProcessModel;

                if (stageResult.Success)
                {
                    //TaskStatusSender.SendAICompleted(model.SN, model.Side);

                    if (!model.IsValidationTest)
                    {
                        SystemEvent.SendResultInfo(model.SN, model.Side, stageResult.Messages);
                    }

                    // 非验证测试任务才构建 AI 回写结果
                    if (!model.IsValidationTest)
                    {
                        TaskStatusSender.SendWritingResults(model.SN, model.Side);
                        ctx.AIResult = DefectProcessor.BuildAIResult(model, stageResult.Messages);
                    }
                }
                else
                {
                    LogTextHelper.Error($"{taskPrefix}KEY:{model.Key} SN:{model.SN}检测失败！");
                    ctx.SetError("推理", $"SN:{model.SN} 检测失败");
                }

                if (sw.ElapsedMilliseconds > 20)
                {
                    TaskStatusSender.SendAICompleted(model.SN, model.Side, sw.ElapsedMilliseconds);
                }
            }
            catch
            {
                sw.Stop();
                ctx.InferenceElapsedMs = sw.ElapsedMilliseconds;
                throw; // 由 Pipeline SafeExecuteTransform 捕获
            }

            return ctx;
        }
    }
}

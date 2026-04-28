using DeepSightDB;
using DeepSightModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DeepSightWorkLib.Services.Pipeline
{
    /// <summary>
    /// Pipeline 流转上下文 - 携带数据在各阶段之间传递
    /// 每个 SN+Side 的处理对应一个 PipelineContext 实例
    /// </summary>
    public class PipelineContext
    {
        #region 输入信息（阶段1: AVI读取后设置）

        /// <summary>
        /// AVI 处理上下文（MinIO连接信息、SN、面别等）
        /// </summary>
        public AviProcessingContext AviContext { get; set; }

        #endregion

        #region JSON解析结果（阶段2: JSON解析后设置）

        /// <summary>
        /// 图片加载模型（含 VBModel 和 PanelView）
        /// </summary>
        public ImageLoadModel LoadModel { get; set; }

        #endregion

        #region 推理结果（阶段3: 推理完成后设置）

        /// <summary>
        /// 推理是否成功
        /// </summary>
        public bool InferenceSuccess { get; set; }

        /// <summary>
        /// 推理返回的消息列表
        /// </summary>
        public List<string> InferenceMessages { get; set; }

        /// <summary>
        /// 推理耗时（毫秒）
        /// </summary>
        public long InferenceElapsedMs { get; set; }

        /// <summary>
        /// AI 结果（用于回写 LevelDB）
        /// </summary>
        public RootAIResult AIResult { get; set; }

        /// <summary>
        /// 推理结果模型（用于后处理）
        /// </summary>
        public InferenceResultModel PostProcessModel { get; set; }

        #endregion

        #region 流转控制

        /// <summary>
        /// 当前阶段名称（用于日志和错误追踪）
        /// </summary>
        public string CurrentStage { get; set; }

        /// <summary>
        /// 是否已发生错误（后续阶段应跳过处理）
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 错误发生的阶段
        /// </summary>
        public string ErrorStage { get; set; }

        /// <summary>
        /// Pipeline 入口时间戳
        /// </summary>
        public DateTime EnqueueTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 全流程计时器
        /// </summary>
        public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();

        /// <summary>
        /// 是否为验证测试任务
        /// </summary>
        public bool IsValidationTest => LoadModel?.Model?.IsValidationTest ?? false;

        #endregion

        #region 便捷属性

        /// <summary>产品序列号</summary>
        public string SN => AviContext?.SerialNumber ?? LoadModel?.Model?.SN;

        /// <summary>面别</summary>
        public string Side => AviContext?.Side ?? LoadModel?.Model?.Side;

        #endregion

        #region 资源释放（引用计数）

        /// <summary>
        /// BroadcastBlock 后有多个下游消费者，用引用计数确保最后一个完成后释放 VBModel。
        /// 由 <see cref="ProcessingPipeline"/> 在投递前通过 <see cref="SetDownstreamRefCount"/> 设置。
        /// </summary>
        private int _downstreamRefCount;

        /// <summary>
        /// 所有下游阶段都完成后触发的回调（统计耗时、清理 ProcessingSnSet 等）
        /// </summary>
        public Action<PipelineContext> OnAllDownstreamCompleted { get; set; }

        /// <summary>
        /// 设置下游消费者引用计数（由 Pipeline 在投递前调用）
        /// </summary>
        public void SetDownstreamRefCount(int count)
        {
            Interlocked.Exchange(ref _downstreamRefCount, count);
        }

        /// <summary>
        /// 通知一个下游阶段已完成处理。当所有下游都完成时，触发完成回调并释放资源。
        /// </summary>
        public void ReleaseDownstreamRef()
        {
            if (Interlocked.Decrement(ref _downstreamRefCount) <= 0)
            {
                FinalizeAndDispose();
            }
        }

        /// <summary>
        /// 触发完成回调并释放 VBModel 持有的 Mat 等非托管资源
        /// </summary>
        private void FinalizeAndDispose()
        {
            try
            {
                OnAllDownstreamCompleted?.Invoke(this);
            }
            catch (Exception ex)
            {
                DeepSightTool.LogTextHelper.Warn($"PipelineContext.OnAllDownstreamCompleted 回调异常: {ex.Message}");
            }

            try
            {
                LoadModel?.Model?.Dispose();
            }
            catch (Exception ex)
            {
                DeepSightTool.LogTextHelper.Warn($"PipelineContext.DisposeResources 异常: {ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// 标记当前上下文为错误状态
        /// </summary>
        public void SetError(string stage, string message)
        {
            HasError = true;
            ErrorStage = stage;
            ErrorMessage = message;
        }
    }
}

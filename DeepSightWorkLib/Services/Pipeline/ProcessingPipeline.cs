using DeepSightModel.Configuration;
using DeepSightTool;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DeepSightWorkLib.Services.Pipeline
{
    /// <summary>
    /// 基于 TPL Dataflow 的处理 Pipeline
    /// 流程：AviContext → JSON解析 → 图片加载 → 推理 → 结果分发 → [回写 + 后处理]
    /// </summary>
    public class ProcessingPipeline : IDisposable
    {
        #region Pipeline Blocks

        /// <summary>Pipeline 入口：JSON 解析阶段</summary>
        private TransformBlock<PipelineContext, PipelineContext> _jsonParseBlock;

        /// <summary>图片加载阶段</summary>
        private TransformBlock<PipelineContext, PipelineContext> _imageLoadBlock;

        /// <summary>AI 推理阶段</summary>
        private TransformBlock<PipelineContext, PipelineContext> _inferenceBlock;

        /// <summary>结果分发（广播给回写和后处理）</summary>
        private BroadcastBlock<PipelineContext> _broadcastBlock;

        /// <summary>结果回写阶段</summary>
        private ActionBlock<PipelineContext> _resultWriteBlock;

        /// <summary>后处理阶段</summary>
        private ActionBlock<PipelineContext> _postProcessBlock;

        #endregion

        #region 阶段委托

        private Func<PipelineContext, PipelineContext> _jsonParseFunc;
        private Func<PipelineContext, PipelineContext> _imageLoadFunc;
        private Func<PipelineContext, PipelineContext> _inferenceFunc;
        private Action<PipelineContext> _resultWriteAction;
        private Action<PipelineContext> _postProcessAction;
        private Action<PipelineContext> _errorHandler;
        private Action<PipelineContext> _completionHandler;

        #endregion

        #region 容量配置

        /// <summary>
        /// 推理队列最大积压数量（除当前正在推理的一帧外，最多允许 N 帧已加载图片等待推理）
        /// 对应 _inferenceBlock 和 _imageLoadBlock 的 BoundedCapacity = MaxPendingInferenceCount + 1
        /// </summary>
        private int _maxPendingInferenceCount = DefaultValues.MaxPendingInferenceCount;

        #endregion

        private CancellationTokenSource _cts;
        private bool _isRunning;
        private bool _disposed;

        /// <summary>
        /// BroadcastBlock 后的下游消费者数量（用于引用计数）
        /// </summary>
        private int _downstreamCount = 2;

        #region 已处理计数（线程安全）

        private long _jsonParseProcessed;
        private long _imageLoadProcessed;
        private long _inferenceProcessed;
        private long _resultWriteProcessed;
        private long _postProcessProcessed;

        #endregion

        /// <summary>
        /// Pipeline 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        #region 配置 Builder 方法

        /// <summary>设置 JSON 解析阶段处理逻辑</summary>
        public ProcessingPipeline WithJsonParseStage(Func<PipelineContext, PipelineContext> func)
        {
            _jsonParseFunc = func ?? throw new ArgumentNullException(nameof(func));
            return this;
        }

        /// <summary>设置图片加载阶段处理逻辑</summary>
        public ProcessingPipeline WithImageLoadStage(Func<PipelineContext, PipelineContext> func)
        {
            _imageLoadFunc = func ?? throw new ArgumentNullException(nameof(func));
            return this;
        }

        /// <summary>设置推理阶段处理逻辑</summary>
        public ProcessingPipeline WithInferenceStage(Func<PipelineContext, PipelineContext> func)
        {
            _inferenceFunc = func ?? throw new ArgumentNullException(nameof(func));
            return this;
        }

        /// <summary>设置结果回写阶段处理逻辑</summary>
        public ProcessingPipeline WithResultWriteStage(Action<PipelineContext> action)
        {
            _resultWriteAction = action ?? throw new ArgumentNullException(nameof(action));
            return this;
        }

        /// <summary>设置后处理阶段处理逻辑</summary>
        public ProcessingPipeline WithPostProcessStage(Action<PipelineContext> action)
        {
            _postProcessAction = action ?? throw new ArgumentNullException(nameof(action));
            return this;
        }

        /// <summary>设置全局错误处理器</summary>
        public ProcessingPipeline WithErrorHandler(Action<PipelineContext> handler)
        {
            _errorHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <summary>设置所有下游阶段完成后的回调（统计耗时、清理标记等）</summary>
        public ProcessingPipeline WithCompletionHandler(Action<PipelineContext> handler)
        {
            _completionHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <summary>设置推理队列最大积压数量（控制内存占用，作用于 Inference 和 ImageLoad Block 的 BoundedCapacity）</summary>
        public ProcessingPipeline WithMaxPendingInferenceCount(int maxPendingInferenceCount)
        {
            _maxPendingInferenceCount = Math.Max(0, maxPendingInferenceCount);
            return this;
        }

        #endregion

        #region 生命周期

        /// <summary>
        /// 构建并启动 Pipeline
        /// </summary>
        /// <param name="maxDegreeOfParallelism">每个阶段的最大并行度（默认1，保持顺序）</param>
        public void Start(int maxDegreeOfParallelism = 1)
        {
            if (_isRunning)
            {
                LogTextHelper.Warn("ProcessingPipeline: 已在运行中");
                return;
            }

            ValidateConfiguration();

            _cts = new CancellationTokenSource();
            BuildBlocks(maxDegreeOfParallelism);
            LinkBlocks();
            _isRunning = true;

            LogTextHelper.Info("ProcessingPipeline: 已启动");
        }

        /// <summary>
        /// 向 Pipeline 投递一个处理上下文（简易版）
        /// </summary>
        public bool Post(PipelineContext context)
        {
            return Post(context, out _);
        }

        /// <summary>
        /// 向 Pipeline 投递一个处理上下文
        /// </summary>
        /// <param name="context">处理上下文</param>
        /// <param name="failureReason">投递失败时的详细原因</param>
        /// <returns>是否成功投递</returns>
        public bool Post(PipelineContext context, out string failureReason)
        {
            failureReason = null;

            if (_disposed)
            {
                failureReason = "Pipeline 已被释放(Disposed)";
                return false;
            }

            if (!_isRunning)
            {
                failureReason = "Pipeline 未启动";
                return false;
            }

            PrepareContext(context);
            bool posted = _jsonParseBlock.Post(context);

            if (!posted)
            {
                // 无界队列下 Post 失败通常意味着 Block 已 Complete 或 Faulted
                var stats = GetStatistics();
                var completion = _jsonParseBlock.Completion;
                string blockState = completion.IsCompleted ? $"已完成(Status={completion.Status})" : "运行中";
                failureReason = $"入口Block拒绝投递(状态={blockState}), 队列深度=[{stats}]";
            }

            return posted;
        }

        /// <summary>
        /// 异步投递（支持等待有界容量释放）
        /// </summary>
        public async Task<bool> SendAsync(PipelineContext context)
        {
            if (!_isRunning)
            {
                LogTextHelper.Warn("ProcessingPipeline: 未启动，无法投递");
                return false;
            }

            PrepareContext(context);

            return await _jsonParseBlock.SendAsync(context, _cts.Token);
        }

        /// <summary>
        /// 停止 Pipeline 并等待所有正在处理的数据完成
        /// </summary>
        public void Stop(int timeoutMs = 10000)
        {
            if (!_isRunning) return;

            LogTextHelper.Info("ProcessingPipeline: 正在停止...");

            try
            {
                // 通知入口 Block 不再接受新数据
                _jsonParseBlock.Complete();

                // 等待所有 Block 完成
                var completionTask = Task.WhenAll(
                    _resultWriteBlock.Completion,
                    _postProcessBlock.Completion);

                if (!completionTask.Wait(timeoutMs))
                {
                    LogTextHelper.Warn("ProcessingPipeline: 等待完成超时，强制取消");
                    _cts?.Cancel();
                }
            }
            catch (AggregateException ex)
            {
                foreach (var inner in ex.InnerExceptions)
                {
                    if (!(inner is TaskCanceledException))
                        LogTextHelper.Error($"ProcessingPipeline 停止异常: {inner}");
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"ProcessingPipeline 停止异常: {ex}");
            }
            finally
            {
                _isRunning = false;
                _cts?.Dispose();
                _cts = null;
                LogTextHelper.Info("ProcessingPipeline: 已停止");
            }
        }

        #endregion

        #region 内部构建

        private void ValidateConfiguration()
        {
            if (_jsonParseFunc == null) throw new InvalidOperationException("未配置 JSON 解析阶段");
            if (_imageLoadFunc == null) throw new InvalidOperationException("未配置图片加载阶段");
            if (_inferenceFunc == null) throw new InvalidOperationException("未配置推理阶段");
            if (_resultWriteAction == null) throw new InvalidOperationException("未配置结果回写阶段");
            if (_postProcessAction == null) throw new InvalidOperationException("未配置后处理阶段");
        }

        private void BuildBlocks(int maxParallelism)
        {
            // 推理/图像加载阶段的容量限制 = MaxPendingInferenceCount + 1（当前正在处理的一帧 + N 帧积压）
            int boundedCapacity = _maxPendingInferenceCount + 1;

            // JSON 解析、结果回写、后处理不限制容量
            var unboundedOptions = new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = maxParallelism,
                BoundedCapacity = -1
            };

            // 图片加载阶段：限制容量，推理慢时阻塞加载，避免内存堆积
            var imageLoadOptions = new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = maxParallelism,
                BoundedCapacity = boundedCapacity
            };

            // 推理阶段固定单并行（算法通常非线程安全），同时限制容量
            var inferenceOptions = new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = 1,
                BoundedCapacity = boundedCapacity
            };

            _jsonParseBlock = new TransformBlock<PipelineContext, PipelineContext>(
                ctx => { Interlocked.Increment(ref _jsonParseProcessed); return SafeExecuteTransform(ctx, "JSON解析", _jsonParseFunc); }, unboundedOptions);

            _imageLoadBlock = new TransformBlock<PipelineContext, PipelineContext>(
                ctx => { Interlocked.Increment(ref _imageLoadProcessed); return SafeExecuteTransform(ctx, "图片加载", _imageLoadFunc); }, imageLoadOptions);

            _inferenceBlock = new TransformBlock<PipelineContext, PipelineContext>(
                ctx => { Interlocked.Increment(ref _inferenceProcessed); return SafeExecuteTransform(ctx, "推理", _inferenceFunc); }, inferenceOptions);

            _broadcastBlock = new BroadcastBlock<PipelineContext>(ctx => ctx,
                new DataflowBlockOptions { CancellationToken = _cts.Token });

            _resultWriteBlock = new ActionBlock<PipelineContext>(
                ctx => { Interlocked.Increment(ref _resultWriteProcessed); SafeExecuteAction(ctx, "结果回写", _resultWriteAction); }, unboundedOptions);

            _postProcessBlock = new ActionBlock<PipelineContext>(
                ctx => { Interlocked.Increment(ref _postProcessProcessed); SafeExecuteAction(ctx, "后处理", _postProcessAction); }, unboundedOptions);
        }

        private void LinkBlocks()
        {
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            _jsonParseBlock.LinkTo(_imageLoadBlock, linkOptions);
            _imageLoadBlock.LinkTo(_inferenceBlock, linkOptions);
            _inferenceBlock.LinkTo(_broadcastBlock, linkOptions);
            _broadcastBlock.LinkTo(_resultWriteBlock, linkOptions);
            _broadcastBlock.LinkTo(_postProcessBlock, linkOptions);
        }

        #endregion

        #region 安全执行包装

        private PipelineContext SafeExecuteTransform(PipelineContext ctx, string stageName,
            Func<PipelineContext, PipelineContext> func)
        {
            // 已出错的上下文直接透传
            if (ctx.HasError) return ctx;

            ctx.CurrentStage = stageName;
            try
            {
                return func(ctx);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"Pipeline [{stageName}] 异常: SN={ctx.SN}, {ex}");
                ctx.SetError(stageName, ex.Message);
                _errorHandler?.Invoke(ctx);
                return ctx;
            }
        }

        /// <summary>
        /// 安全执行 ActionBlock 委托。
        /// 即使 ctx.HasError 也会调用 action，确保 action 内 finally 块的清理逻辑（如 ReleaseDownstreamRef）始终执行。
        /// action 内部应自行检查 HasError 跳过业务逻辑。
        /// </summary>
        private void SafeExecuteAction(PipelineContext ctx, string stageName, Action<PipelineContext> action)
        {
            ctx.CurrentStage = stageName;
            try
            {
                action(ctx);
            }
            catch (Exception ex)
            {
                // 仅在本阶段首次出错时记录（避免重复记录上游错误）
                if (!ctx.HasError)
                {
                    LogTextHelper.Error($"Pipeline [{stageName}] 异常: SN={ctx.SN}, {ex}");
                    ctx.SetError(stageName, ex.Message);
                    _errorHandler?.Invoke(ctx);
                }
                else
                {
                    LogTextHelper.Warn($"Pipeline [{stageName}] 在已有错误的上下文中再次异常: SN={ctx.SN}, {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 投递前初始化上下文：设置引用计数和完成回调
        /// </summary>
        private void PrepareContext(PipelineContext ctx)
        {
            ctx.SetDownstreamRefCount(_downstreamCount);
            ctx.OnAllDownstreamCompleted = _completionHandler;
        }

        #endregion

        #region 状态查询

        /// <summary>
        /// 获取各阶段的输入队列大小
        /// </summary>
        public PipelineStatistics GetStatistics()
        {
            if (!_isRunning) return new PipelineStatistics();

            return new PipelineStatistics
            {
                JsonParseInputCount = _jsonParseBlock.InputCount,
                ImageLoadInputCount = _imageLoadBlock.InputCount,
                InferenceInputCount = _inferenceBlock.InputCount,
                ResultWriteInputCount = _resultWriteBlock.InputCount,
                PostProcessInputCount = _postProcessBlock.InputCount,

                JsonParseStatus = _jsonParseBlock.Completion.Status,
                ImageLoadStatus = _imageLoadBlock.Completion.Status,
                InferenceStatus = _inferenceBlock.Completion.Status,
                BroadcastStatus = _broadcastBlock.Completion.Status,
                ResultWriteStatus = _resultWriteBlock.Completion.Status,
                PostProcessStatus = _postProcessBlock.Completion.Status,

                JsonParseProcessed = Interlocked.Read(ref _jsonParseProcessed),
                ImageLoadProcessed = Interlocked.Read(ref _imageLoadProcessed),
                InferenceProcessed = Interlocked.Read(ref _inferenceProcessed),
                ResultWriteProcessed = Interlocked.Read(ref _resultWriteProcessed),
                PostProcessProcessed = Interlocked.Read(ref _postProcessProcessed),
            };
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_isRunning)
            {
                Stop();
            }

            _cts?.Dispose();
            LogTextHelper.Info("ProcessingPipeline: 资源已释放");
        }

        #endregion
    }

    /// <summary>
    /// Pipeline 各阶段统计信息
    /// </summary>
    public class PipelineStatistics
    {
        public int JsonParseInputCount { get; set; }
        public int ImageLoadInputCount { get; set; }
        public int InferenceInputCount { get; set; }
        public int ResultWriteInputCount { get; set; }
        public int PostProcessInputCount { get; set; }

        /// <summary>各阶段 Block 的 Completion 状态</summary>
        public TaskStatus JsonParseStatus { get; set; } = TaskStatus.WaitingForActivation;
        public TaskStatus ImageLoadStatus { get; set; } = TaskStatus.WaitingForActivation;
        public TaskStatus InferenceStatus { get; set; } = TaskStatus.WaitingForActivation;
        public TaskStatus BroadcastStatus { get; set; } = TaskStatus.WaitingForActivation;
        public TaskStatus ResultWriteStatus { get; set; } = TaskStatus.WaitingForActivation;
        public TaskStatus PostProcessStatus { get; set; } = TaskStatus.WaitingForActivation;

        /// <summary>各阶段累计已处理数量</summary>
        public long JsonParseProcessed { get; set; }
        public long ImageLoadProcessed { get; set; }
        public long InferenceProcessed { get; set; }
        public long ResultWriteProcessed { get; set; }
        public long PostProcessProcessed { get; set; }

        public override string ToString()
        {
            return $"JSON解析:{JsonParseInputCount}, 图片加载:{ImageLoadInputCount}, " +
                   $"推理:{InferenceInputCount}, 回写:{ResultWriteInputCount}, 后处理:{PostProcessInputCount}";
        }
    }
}

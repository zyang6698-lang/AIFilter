using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 工作线程配置
    /// </summary>
    public class WorkerConfig
    {
        /// <summary>
        /// 工作线程名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 轮询间隔（毫秒）
        /// </summary>
        public int PollIntervalMs { get; set; } = 15;

        /// <summary>
        /// 是否为长时间运行任务
        /// </summary>
        public bool IsLongRunning { get; set; } = false;
    }

    /// <summary>
    /// 工作线程管理器 - 统一管理所有后台工作线程的生命周期
    /// </summary>
    public class WorkerThreadManager : IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource;
        private readonly List<Task> _workerTasks = new List<Task>();
        private readonly List<string> _workerNames = new List<string>();
        private readonly object _lock = new object();
        private bool _isRunning = false;
        private bool _disposed = false;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 获取取消令牌
        /// </summary>
        public CancellationToken CancellationToken => _cancellationTokenSource?.Token ?? CancellationToken.None;

        /// <summary>
        /// 启动所有工作线程
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                if (_isRunning)
                {
                    LogTextHelper.Warn("WorkerThreadManager: 已经在运行中");
                    return;
                }

                _cancellationTokenSource = new CancellationTokenSource();
                _isRunning = true;
                LogTextHelper.Info("WorkerThreadManager: 已启动");
            }
        }

        /// <summary>
        /// 注册工作线程
        /// </summary>
        /// <param name="workerAction">工作线程执行的动作</param>
        /// <param name="config">工作线程配置</param>
        public void RegisterWorker(Action<CancellationToken> workerAction, WorkerConfig config)
        {
            if (workerAction == null) throw new ArgumentNullException(nameof(workerAction));
            if (config == null) throw new ArgumentNullException(nameof(config));

            lock (_lock)
            {
                if (!_isRunning)
                {
                    throw new InvalidOperationException("WorkerThreadManager 尚未启动，请先调用 Start()");
                }

                var token = _cancellationTokenSource.Token;
                Task task;

                if (config.IsLongRunning)
                {
                    task = Task.Factory.StartNew(
                        () => workerAction(token),
                        token,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default);
                }
                else
                {
                    task = Task.Run(() => workerAction(token), token);
                }

                _workerTasks.Add(task);
                _workerNames.Add(config.Name);
                LogTextHelper.Info($"WorkerThreadManager: 已注册工作线程 [{config.Name}]");
            }
        }

        /// <summary>
        /// 创建标准的轮询工作线程
        /// 使用自适应退避策略：当没有工作时增加等待时间，有工作时减少等待时间
        /// </summary>
        /// <param name="workAction">每次轮询执行的工作（返回true表示有工作处理，返回false表示空闲）</param>
        /// <param name="config">工作线程配置</param>
        public void RegisterPollingWorker(Func<bool> workAction, WorkerConfig config)
        {
            RegisterWorker(token =>
            {
                LogTextHelper.Info($"工作线程 [{config.Name}] 已启动");

                // 自适应退避参数
                int currentWaitMs = config.PollIntervalMs;
                int maxWaitMs = Math.Max(config.PollIntervalMs * 10, 500); // 最大等待时间
                int consecutiveIdleCount = 0;

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // 使用 WaitHandle 等待，支持取消
                        if (token.WaitHandle.WaitOne(currentWaitMs))
                        {
                            // 收到取消信号
                            break;
                        }

                        bool hasWork = workAction?.Invoke() ?? false;

                        // 自适应退避策略
                        if (hasWork)
                        {
                            // 有工作时，重置等待时间为最小值
                            currentWaitMs = config.PollIntervalMs;
                            consecutiveIdleCount = 0;
                        }
                        else
                        {
                            // 空闲时，逐渐增加等待时间（指数退避）
                            consecutiveIdleCount++;
                            if (consecutiveIdleCount > 5)
                            {
                                currentWaitMs = Math.Min(currentWaitMs * 2, maxWaitMs);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"工作线程 [{config.Name}] 异常: {ex}");
                        // 异常后短暂等待，避免快速循环
                        token.WaitHandle.WaitOne(100);
                    }
                }

                LogTextHelper.Info($"工作线程 [{config.Name}] 已停止");
            }, config);
        }

        /// <summary>
        /// 停止所有工作线程
        /// </summary>
        /// <param name="timeoutMs">等待超时时间（毫秒）</param>
        public void Stop(int timeoutMs = 5000)
        {
            lock (_lock)
            {
                if (!_isRunning)
                {
                    return;
                }

                LogTextHelper.Info("WorkerThreadManager: 正在停止所有工作线程...");

                try
                {
                    _cancellationTokenSource?.Cancel();

                    if (_workerTasks.Count > 0)
                    {
                        try
                        {
                            Task.WaitAll(_workerTasks.ToArray(), timeoutMs);
                        }
                        catch (AggregateException)
                        {
                            // 忽略因取消导致的任务异常
                        }
                    }

                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;

                    LogTextHelper.Info($"WorkerThreadManager: 已停止 {_workerTasks.Count} 个工作线程 [{string.Join(", ", _workerNames)}]");

                    _workerTasks.Clear();
                    _workerNames.Clear();
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"WorkerThreadManager 停止异常: {ex}");
                }
                finally
                {
                    _isRunning = false;
                }
            }
        }

        /// <summary>
        /// 获取工作线程状态报告
        /// </summary>
        public string GetStatusReport()
        {
            lock (_lock)
            {
                if (!_isRunning)
                {
                    return "WorkerThreadManager: 未运行";
                }

                var completedCount = 0;
                var runningCount = 0;
                var faultedCount = 0;

                foreach (var task in _workerTasks)
                {
                    if (task.IsCompleted && !task.IsFaulted)
                        completedCount++;
                    else if (task.IsFaulted)
                        faultedCount++;
                    else
                        runningCount++;
                }

                return $"WorkerThreadManager: 运行中={runningCount}, 已完成={completedCount}, 异常={faultedCount}";
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Stop();
            }

            _disposed = true;
        }

        #endregion
    }
}


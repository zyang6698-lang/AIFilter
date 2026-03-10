using DeepSightDB;
using DeepSightModel;
using DeepSightTool;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 队列管理器 - 统一管理所有处理队列
    /// </summary>
    public class QueueManager : IDisposable
    {
        #region 队列定义

        /// <summary>
        /// 图片加载队列（解耦图片读取和推理）
        /// </summary>
        public ConcurrentQueue<ImageLoadModel> ImageLoadQueue { get; } = new ConcurrentQueue<ImageLoadModel>();

        /// <summary>
        /// AVI/推理处理队列
        /// </summary>
        public ConcurrentQueue<VBModel> AviQueue { get; } = new ConcurrentQueue<VBModel>();

        /// <summary>
        /// AI 结果回写队列 (Key, SN, Side, RootAIResult)
        /// </summary>
        public ConcurrentQueue<Tuple<string, string, string, RootAIResult>> AIResultQueue { get; } 
            = new ConcurrentQueue<Tuple<string, string, string, RootAIResult>>();

        /// <summary>
        /// 推理后处理队列
        /// </summary>
        public ConcurrentQueue<InferenceResultModel> PostProcessQueue { get; } = new ConcurrentQueue<InferenceResultModel>();

        /// <summary>
        /// 正在处理的 SN+Side 集合（防止重复检测）
        /// Key格式："{SerialNumber}_{Side}", Value：入队时间戳
        /// </summary>
        public ConcurrentDictionary<string, DateTime> ProcessingSnSet { get; } = new ConcurrentDictionary<string, DateTime>();

        #endregion

        #region 队列状态

        /// <summary>
        /// 获取队列统计信息
        /// </summary>
        public QueueStatistics GetStatistics()
        {
            return new QueueStatistics
            {
                ImageLoadQueueCount = ImageLoadQueue.Count,
                AviQueueCount = AviQueue.Count,
                AIResultQueueCount = AIResultQueue.Count,
                PostProcessQueueCount = PostProcessQueue.Count,
                ProcessingSnCount = ProcessingSnSet.Count
            };
        }

        /// <summary>
        /// 检查是否有待处理的任务
        /// </summary>
        public bool HasPendingTasks()
        {
            return ImageLoadQueue.Count > 0 ||
                   AviQueue.Count > 0 ||
                   AIResultQueue.Count > 0 ||
                   PostProcessQueue.Count > 0;
        }

        #endregion

        #region 队列操作

        /// <summary>
        /// 添加处理中标记
        /// </summary>
        /// <param name="sn">序列号</param>
        /// <param name="side">面别</param>
        /// <returns>是否成功添加（如果已存在则返回false）</returns>
        public bool TryAddProcessing(string sn, string side)
        {
            var key = $"{sn}_{side}";
            return ProcessingSnSet.TryAdd(key, DateTime.Now);
        }

        /// <summary>
        /// 移除处理中标记
        /// </summary>
        /// <param name="sn">序列号</param>
        /// <param name="side">面别</param>
        /// <returns>是否成功移除</returns>
        public bool TryRemoveProcessing(string sn, string side)
        {
            var key = $"{sn}_{side}";
            return ProcessingSnSet.TryRemove(key, out _);
        }

        /// <summary>
        /// 检查是否正在处理
        /// </summary>
        public bool IsProcessing(string sn, string side)
        {
            var key = $"{sn}_{side}";
            return ProcessingSnSet.ContainsKey(key);
        }

        #endregion

        #region 清理操作

        /// <summary>
        /// 从所有队列中移除指定SN的数据
        /// </summary>
        /// <param name="sn">要移除的SN</param>
        /// <returns>实际移除的项目数量</returns>
        public int RemoveSnFromAllQueues(string sn)
        {
            if (string.IsNullOrEmpty(sn)) return 0;

            int removedCount = 0;

            // 从 AviQueue 中过滤移除
            removedCount += DrainAndFilter(AviQueue, item =>
            {
                if (item?.SN == sn) { DisposeMats(item.Mats); return true; }
                return false;
            });

            // 从 ImageLoadQueue 中过滤移除
            removedCount += DrainAndFilter(ImageLoadQueue, item =>
            {
                if (item?.Model?.SN == sn) { DisposeMats(item.Model.Mats); return true; }
                return false;
            });

            // 从 AIResultQueue 中过滤移除
            removedCount += DrainAndFilter(AIResultQueue, item =>
            {
                return item?.Item2 == sn;
            });

            // 从 PostProcessQueue 中过滤移除
            removedCount += DrainAndFilter(PostProcessQueue, item =>
            {
                return item?.VBModel?.SN == sn;
            });

            // 从 ProcessingSnSet 中移除
            var keysToRemove = new List<string>();
            foreach (var kvp in ProcessingSnSet)
            {
                if (kvp.Key.StartsWith(sn + "_"))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            foreach (var key in keysToRemove)
            {
                if (ProcessingSnSet.TryRemove(key, out _))
                    removedCount++;
            }

            if (removedCount > 0)
            {
                LogTextHelper.Info($"已从所有队列中移除SN={sn}的数据，共移除{removedCount}项");
            }

            return removedCount;
        }

        /// <summary>
        /// 从ConcurrentQueue中过滤移除匹配项（drain-and-requeue模式）
        /// </summary>
        private int DrainAndFilter<T>(ConcurrentQueue<T> queue, Func<T, bool> shouldRemove)
        {
            int removedCount = 0;
            int count = queue.Count;
            for (int i = 0; i < count; i++)
            {
                if (queue.TryDequeue(out T item))
                {
                    if (shouldRemove(item))
                    {
                        removedCount++;
                    }
                    else
                    {
                        queue.Enqueue(item);
                    }
                }
            }
            return removedCount;
        }

        /// <summary>
        /// 清空所有队列（停止作业时调用）
        /// </summary>
        /// <returns>被清理的 SN 集合</returns>
        public HashSet<string> ClearAllQueues()
        {
            var clearedSnSet = new HashSet<string>();
            int aviCount = 0, imageLoadCount = 0, aiResultCount = 0, postProcessCount = 0;

            // 清空 AviQueue 并释放 Mat 资源
            while (AviQueue.TryDequeue(out var vbModel))
            {
                if (vbModel != null)
                {
                    if (!string.IsNullOrEmpty(vbModel.SN)) clearedSnSet.Add(vbModel.SN);
                    DisposeMats(vbModel.Mats);
                }
                aviCount++;
            }

            // 清空 ImageLoadQueue
            while (ImageLoadQueue.TryDequeue(out var loadModel))
            {
                if (loadModel?.Model != null)
                {
                    if (!string.IsNullOrEmpty(loadModel.Model.SN)) clearedSnSet.Add(loadModel.Model.SN);
                    DisposeMats(loadModel.Model.Mats);
                }
                imageLoadCount++;
            }

            // 清空 AIResultQueue
            while (AIResultQueue.TryDequeue(out var info))
            {
                if (!string.IsNullOrEmpty(info?.Item2)) clearedSnSet.Add(info.Item2);
                aiResultCount++;
            }

            // 清空 PostProcessQueue
            while (PostProcessQueue.TryDequeue(out var postProcess))
            {
                if (!string.IsNullOrEmpty(postProcess?.VBModel?.SN)) clearedSnSet.Add(postProcess.VBModel.SN);
                postProcessCount++;
            }

            // 清空处理中集合
            int processingCount = ProcessingSnSet.Count;
            ProcessingSnSet.Clear();

            LogTextHelper.Info($"所有队列已清空 - AVI:{aviCount}, 图片加载:{imageLoadCount}, " +
                $"AI结果:{aiResultCount}, 后处理:{postProcessCount}, 处理中:{processingCount}");

            return clearedSnSet;
        }

        /// <summary>
        /// 清理过期的处理标记
        /// </summary>
        /// <param name="expireMinutes">过期时间（分钟）</param>
        /// <returns>清理的数量</returns>
        public int CleanupExpiredProcessing(int expireMinutes)
        {
            var now = DateTime.Now;
            var expiredKeys = new List<string>();

            foreach (var kvp in ProcessingSnSet)
            {
                if ((now - kvp.Value).TotalMinutes > expireMinutes)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                if (ProcessingSnSet.TryRemove(key, out DateTime addTime))
                {
                    var duration = now - addTime;
                    LogTextHelper.Warn($"清理过期处理标记：{key}，已超时 {duration.TotalMinutes:F1} 分钟");
                }
            }

            return expiredKeys.Count;
        }

        /// <summary>
        /// 释放 Mat 列表资源
        /// </summary>
        private void DisposeMats(List<Mat> mats)
        {
            if (mats == null) return;
            foreach (var mat in mats)
            {
                mat?.Dispose();
            }
            mats.Clear();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            // 清空所有队列并释放资源
            ClearAllQueues();
            LogTextHelper.Info("QueueManager 已释放资源");
        }

        #endregion
    }

    /// <summary>
    /// 队列统计信息
    /// </summary>
    public class QueueStatistics
    {
        public int ImageLoadQueueCount { get; set; }
        public int AviQueueCount { get; set; }
        public int AIResultQueueCount { get; set; }
        public int PostProcessQueueCount { get; set; }
        public int ProcessingSnCount { get; set; }

        public override string ToString()
        {
            return $"处理中:{ProcessingSnCount}, 图片加载:{ImageLoadQueueCount}, " +
                   $"推理:{AviQueueCount}, 后处理:{PostProcessQueueCount}, 回写:{AIResultQueueCount}";
        }
    }
}


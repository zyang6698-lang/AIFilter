using System;
using System.Collections.Concurrent;

namespace DeepSightModel
{
    /// <summary>
    /// SN调试信息 - 用于双击任务队列时显示的详细数据
    /// </summary>
    public class SnDebugInfo
    {
        /// <summary>
        /// SN序列号
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 面别 (A/B)
        /// </summary>
        public string Side { get; set; }

        /// <summary>
        /// 从LevelDB读取到的原始JSON推理请求
        /// </summary>
        public string RawLevelDbJson { get; set; }

        /// <summary>
        /// 通过Minio读取到的PanelInfo JSON
        /// </summary>
        public string PanelInfoJson { get; set; }

        /// <summary>
        /// 转换后给VB推理的JSON
        /// </summary>
        public string VbInferenceJson { get; set; }

        /// <summary>
        /// 推理返回的JSON
        /// </summary>
        public string InferenceReturnJson { get; set; }

        /// <summary>
        /// 缺陷数量
        /// </summary>
        public int DefectCount { get; set; }

        /// <summary>
        /// PCS数量
        /// </summary>
        public int PcsCount { get; set; }

        /// <summary>
        /// 图片数量
        /// </summary>
        public int ImageCount { get; set; }

        /// <summary>
        /// 是否ByPass
        /// </summary>
        public bool IsByPass { get; set; }

        /// <summary>
        /// 数据来源DB名称
        /// </summary>
        public string SourceDbName { get; set; }

        /// <summary>
        /// 数据来源DB URL
        /// </summary>
        public string SourceDbUrl { get; set; }

        /// <summary>
        /// Minio路径
        /// </summary>
        public string MinioPath { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否发生错误
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// 出错步骤（如：图片加载、AI检测、结果回写、后处理）
        /// </summary>
        public string ErrorStep { get; set; }

        /// <summary>
        /// 错误原因
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 错误发生时间
        /// </summary>
        public DateTime? ErrorTime { get; set; }
    }

    /// <summary>
    /// SN调试信息缓存 - 静态缓存，供全局访问
    /// </summary>
    public static class SnDebugInfoCache
    {
        /// <summary>
        /// 缓存字典，Key格式: "{SN}_{Side}"
        /// </summary>
        private static readonly ConcurrentDictionary<string, SnDebugInfo> _cache
            = new ConcurrentDictionary<string, SnDebugInfo>();

        /// <summary>
        /// 最大缓存数量
        /// </summary>
        private const int MaxCacheSize = 100;

        public static SnDebugInfo GetOrCreate(string sn, string side)
        {
            string key = $"{sn}_{side}";
            return _cache.GetOrAdd(key, _ => new SnDebugInfo
            {
                SerialNumber = sn,
                Side = side
            });
        }

        public static bool TryGet(string sn, string side, out SnDebugInfo info)
        {
            string key = $"{sn}_{side}";
            return _cache.TryGetValue(key, out info);
        }

        /// <summary>
        /// 获取指定SN的所有面的调试信息
        /// </summary>
        public static SnDebugInfo[] GetBySn(string sn)
        {
            var list = new System.Collections.Generic.List<SnDebugInfo>();
            foreach (var kvp in _cache)
            {
                if (kvp.Value.SerialNumber == sn)
                    list.Add(kvp.Value);
            }
            // A侧排在前面
            list.Sort((a, b) => string.Compare(a.Side, b.Side, StringComparison.OrdinalIgnoreCase));
            return list.ToArray();
        }

        public static void Remove(string sn, string side)
        {
            string key = $"{sn}_{side}";
            _cache.TryRemove(key, out _);
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public static void Clear()
        {
            _cache.Clear();
        }

        /// <summary>
        /// 清理过期缓存（保留最近的MaxCacheSize条）
        /// </summary>
        public static void Cleanup()
        {
            if (_cache.Count <= MaxCacheSize) return;
            // Simple cleanup: remove oldest entries
            var sorted = new System.Collections.Generic.SortedList<DateTime, string>();
            foreach (var kvp in _cache)
            {
                if (!sorted.ContainsKey(kvp.Value.CreateTime))
                    sorted.Add(kvp.Value.CreateTime, kvp.Key);
            }
            int toRemove = _cache.Count - MaxCacheSize;
            foreach (var kvp in sorted)
            {
                if (toRemove <= 0) break;
                _cache.TryRemove(kvp.Value, out _);
                toRemove--;
            }
        }
    }
}


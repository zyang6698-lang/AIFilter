using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepSightTool
{
    public class MathHelper
    {
        /// <summary>
        /// 计算机台稼动率（Utilization Rate）
        /// 通过分析相邻时间点的差值来判断工作状态：
        /// - 如果相邻时间差值 &lt;= 阈值，则认为这段时间是工作状态
        /// - 如果相邻时间差值 &gt; 阈值，则认为这段时间是非工作状态（待机/停机）
        /// </summary>
        /// <param name="timestamps">时间点集合（无需排序，方法内部会排序）</param>
        /// <param name="idleThresholdMinutes">判断非工作状态的时间阈值（分钟），默认为2分钟</param>
        /// <returns>
        /// 返回元组包含:
        /// - UtilizationRate: 稼动率 (0-1之间的小数，例如0.85表示85%)
        /// - WorkingTime: 工作时间（TimeSpan）
        /// - TotalTime: 总时间跨度（TimeSpan）
        /// 如果时间点少于2个，返回 (0, TimeSpan.Zero, TimeSpan.Zero)
        /// </returns>
        public static (double UtilizationRate, TimeSpan WorkingTime, TimeSpan TotalTime) CalculateUtilizationRate(
            IEnumerable<DateTime> timestamps,
            double idleThresholdMinutes = 2.0)
        {
            // 参数校验
            if (timestamps == null)
            {
                return (0, TimeSpan.Zero, TimeSpan.Zero);
            }

            // 排序时间点
            var sortedTimestamps = timestamps.OrderBy(t => t).ToList();

            if (sortedTimestamps.Count < 2)
            {
                return (0, TimeSpan.Zero, TimeSpan.Zero);
            }

            // 计算工作时间（相邻时间差 <= 阈值的累加）
            TimeSpan workingTime = TimeSpan.Zero;
            var threshold = TimeSpan.FromMinutes(idleThresholdMinutes);

            for (int i = 1; i < sortedTimestamps.Count; i++)
            {
                var interval = sortedTimestamps[i] - sortedTimestamps[i - 1];
                if (interval <= threshold)
                {
                    workingTime += interval;
                }
            }

            // 计算总时间跨度（首尾时间差）
            TimeSpan totalTime = sortedTimestamps.Last() - sortedTimestamps.First();

            // 计算稼动率
            double utilizationRate = totalTime.TotalSeconds > 0
                ? workingTime.TotalSeconds / totalTime.TotalSeconds
                : 0;

            return (utilizationRate, workingTime, totalTime);
        }

        /// <summary>
        /// 计算机台稼动率（简化版，只返回稼动率百分比）
        /// </summary>
        /// <param name="timestamps">时间点集合</param>
        /// <param name="idleThresholdMinutes">判断非工作状态的时间阈值（分钟），默认为2分钟</param>
        /// <returns>稼动率百分比 (0-100之间的数字，例如85.5表示85.5%)</returns>
        public static double CalculateUtilizationRatePercent(
            IEnumerable<DateTime> timestamps,
            double idleThresholdMinutes = 2.0)
        {
            var (rate, _, _) = CalculateUtilizationRate(timestamps, idleThresholdMinutes);
            return rate * 100;
        }
    }
}

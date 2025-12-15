using System;
using System.Collections.Generic;
using System.Text;

namespace DeepSightDB
{
    /// <summary>
    /// 数据库统计指标
    /// </summary>
    public class DatabaseStats
    {
        /// <summary>
        /// 统计收集时间
        /// </summary>
        public DateTime CollectedAt { get; set; }

        /// <summary>
        /// 待处理的数据库操作队列长度
        /// </summary>
        public int QueueLength { get; set; }

        /// <summary>
        /// 数据库大小（字节）
        /// </summary>
        public long DatabaseSizeBytes { get; set; }

        /// <summary>
        /// 数据库大小（MB）
        /// </summary>
        public double DatabaseSizeMB => DatabaseSizeBytes / (1024.0 * 1024.0);

        /// <summary>
        /// 活动连接数
        /// </summary>
        public int ActiveConnections { get; set; }

        /// <summary>
        /// 总连接数
        /// </summary>
        public int TotalConnections { get; set; }

        /// <summary>
        /// 缓存命中率 (%)
        /// </summary>
        public double CacheHitRatio { get; set; }

        /// <summary>
        /// 已提交事务数
        /// </summary>
        public long TransactionsCommitted { get; set; }

        /// <summary>
        /// 已回滚事务数
        /// </summary>
        public long TransactionsRolledBack { get; set; }

        /// <summary>
        /// 返回的元组数
        /// </summary>
        public long TuplesReturned { get; set; }

        /// <summary>
        /// 获取的元组数
        /// </summary>
        public long TuplesFetched { get; set; }

        /// <summary>
        /// 插入的元组数
        /// </summary>
        public long TuplesInserted { get; set; }

        /// <summary>
        /// 更新的元组数
        /// </summary>
        public long TuplesUpdated { get; set; }

        /// <summary>
        /// 删除的元组数
        /// </summary>
        public long TuplesDeleted { get; set; }

        /// <summary>
        /// 各表统计信息
        /// </summary>
        public List<TableStats> TableStats { get; set; } = new List<TableStats>();

        /// <summary>
        /// 索引使用统计
        /// </summary>
        public List<IndexStats> IndexStats { get; set; } = new List<IndexStats>();

        /// <summary>
        /// 生成摘要信息
        /// </summary>
        public string GetSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== 数据库统计 ({CollectedAt:yyyy-MM-dd HH:mm:ss}) ===");
            sb.AppendLine($"数据库大小: {DatabaseSizeMB:F2} MB");
            sb.AppendLine($"连接数: {ActiveConnections} 活动 / {TotalConnections} 总计");
            sb.AppendLine($"操作队列: {QueueLength} 待处理");
            sb.AppendLine($"缓存命中率: {CacheHitRatio:F2}%");
            sb.AppendLine($"事务: {TransactionsCommitted} 提交, {TransactionsRolledBack} 回滚");
            sb.AppendLine($"元组操作: {TuplesInserted} 插入, {TuplesUpdated} 更新, {TuplesDeleted} 删除");
            
            if (TableStats.Count > 0)
            {
                sb.AppendLine("\n--- 表统计 ---");
                foreach (var t in TableStats)
                {
                    sb.AppendLine($"  {t.TableName}: {t.RowCount} 行, {t.DeadRows} 死行, 顺序扫描:{t.SequentialScans}, 索引扫描:{t.IndexScans}");
                }
            }
            
            return sb.ToString();
        }
    }

    /// <summary>
    /// 表统计信息
    /// </summary>
    public class TableStats
    {
        public string TableName { get; set; }
        public long RowCount { get; set; }
        public long DeadRows { get; set; }
        public long SequentialScans { get; set; }
        public long IndexScans { get; set; }
        public long Inserts { get; set; }
        public long Updates { get; set; }
        public long Deletes { get; set; }
    }

    /// <summary>
    /// 索引统计信息
    /// </summary>
    public class IndexStats
    {
        public string IndexName { get; set; }
        public string TableName { get; set; }
        public long Scans { get; set; }
        public long TuplesRead { get; set; }
        public long TuplesFetched { get; set; }
    }
}


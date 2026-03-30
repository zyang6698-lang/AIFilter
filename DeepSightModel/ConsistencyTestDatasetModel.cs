using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DeepSightModel
{
    /// <summary>
    /// 一致性测试数据集 - 包含多个Lot的测试集合
    /// </summary>
    public class ConsistencyTestDataset
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("createTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [JsonProperty("updateTime")]
        public DateTime UpdateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 数据集中包含的Lot条目
        /// </summary>
        [JsonProperty("lotEntries")]
        public List<DatasetLotEntry> LotEntries { get; set; } = new List<DatasetLotEntry>();

        /// <summary>
        /// 获取数据集中的总记录数
        /// </summary>
        [JsonIgnore]
        public int TotalRecords => LotEntries.Sum(l => l.RecordCount);
    }

    /// <summary>
    /// 数据集中的单个Lot条目
    /// </summary>
    public class DatasetLotEntry
    {
        [JsonProperty("lotNumber")]
        public string LotNumber { get; set; }

        [JsonProperty("addTime")]
        public DateTime AddTime { get; set; } = DateTime.Now;

        [JsonProperty("recordCount")]
        public int RecordCount { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("productSerial")]
        public string ProductSerial { get; set; }
    }

    /// <summary>
    /// 一致性测试轮次记录 - 一次多轮测试的结果
    /// </summary>
    public class ConsistencyTestRound
    {
        [JsonProperty("roundId")]
        public string RoundId { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("datasetId")]
        public string DatasetId { get; set; }

        [JsonProperty("datasetName")]
        public string DatasetName { get; set; }

        [JsonProperty("roundNumber")]
        public int RoundNumber { get; set; }

        [JsonProperty("modelName")]
        public string ModelName { get; set; } = "";

        [JsonProperty("startTime")]
        public DateTime StartTime { get; set; }

        [JsonProperty("endTime")]
        public DateTime? EndTime { get; set; }

        [JsonProperty("state")]
        public ConsistencyTestRoundState State { get; set; } = ConsistencyTestRoundState.Pending;

        // 统计数据
        [JsonProperty("totalRecords")]
        public int TotalRecords { get; set; }

        [JsonProperty("consistentRecords")]
        public int ConsistentRecords { get; set; }

        [JsonProperty("inconsistentRecords")]
        public int InconsistentRecords { get; set; }

        [JsonProperty("vvsRecords")]
        public int VVSRecords { get; set; }

        [JsonProperty("totalMissCount")]
        public int TotalMissCount { get; set; }

        [JsonProperty("totalOverKillCount")]
        public int TotalOverKillCount { get; set; }

        [JsonProperty("totalDefects")]
        public int TotalDefects { get; set; }

        // 计算属性
        [JsonIgnore]
        public double ConsistencyRate => TotalRecords > 0 ? (double)ConsistentRecords / TotalRecords * 100 : 0;

        [JsonIgnore]
        public double MissRate => TotalDefects > 0 ? (double)TotalMissCount / TotalDefects * 100 : 0;

        [JsonIgnore]
        public double OverKillRate => TotalDefects > 0 ? (double)TotalOverKillCount / TotalDefects * 100 : 0;

        /// <summary>
        /// 关联的任务ID（用于追踪InferenceTask）
        /// </summary>
        [JsonProperty("taskId")]
        public string TaskId { get; set; }
    }

    /// <summary>
    /// 测试轮次状态
    /// </summary>
    public enum ConsistencyTestRoundState
    {
        Pending = 0,
        Running = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }

    /// <summary>
    /// 多轮测试历史记录 - 汇总一个数据集的多轮测试
    /// </summary>
    public class ConsistencyTestHistory
    {
        [JsonProperty("datasetId")]
        public string DatasetId { get; set; }

        [JsonProperty("datasetName")]
        public string DatasetName { get; set; }

        [JsonProperty("rounds")]
        public List<ConsistencyTestRound> Rounds { get; set; } = new List<ConsistencyTestRound>();
    }
}


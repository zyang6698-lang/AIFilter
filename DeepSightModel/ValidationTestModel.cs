using System;
using System.Collections.Generic;

namespace DeepSightModel
{
    /// <summary>
    /// 模型验证测试状态枚举
    /// </summary>
    public enum ValidationTestState
    {
        NotTested = 0,      // 未测试
        Consistent = 1,     // 一致
        Inconsistent = 2,   // 不一致
        Testing = 3,        // 测试中
        TestError = 4       // 测试异常
    }

    /// <summary>
    /// 原始数据来源枚举
    /// </summary>
    public enum OriginalDataSourceType
    {
        AI = 0,     // 原始数据来源为AI判定
        VVS = 1     // 原始数据来源为VVS复判
    }

    /// <summary>
    /// 单个缺陷的测试比对结果
    /// </summary>
    public class DefectTestResult
    {
        public int DefectIndex { get; set; }
        public string ImagePath { get; set; }
        public int OriginalAIStatus { get; set; }
        /// <summary>
        /// 原始VVS状态（如果有VVS数据）
        /// </summary>
        public int OriginalVVSStatus { get; set; }
        public int NewAIStatus { get; set; }
        /// <summary>
        /// 数据来源（AI或VVS）
        /// </summary>
        public OriginalDataSourceType DataSource { get; set; }
        /// <summary>
        /// 用于比对的原始状态（如果有VVS则用VVS，否则用AI）
        /// </summary>
        public int EffectiveOriginalStatus => DataSource == OriginalDataSourceType.VVS ? OriginalVVSStatus : OriginalAIStatus;
        public bool IsConsistent => EffectiveOriginalStatus == NewAIStatus;
        /// <summary>
        /// 是否为漏失（VVS=NG但新AI=OK），仅VVS数据有效
        /// </summary>
        public bool IsMiss => DataSource == OriginalDataSourceType.VVS && OriginalVVSStatus == 2 && NewAIStatus == 1;
        /// <summary>
        /// 是否为误报（VVS=OK但新AI=NG），仅VVS数据有效
        /// </summary>
        public bool IsOverKill => DataSource == OriginalDataSourceType.VVS && OriginalVVSStatus == 1 && NewAIStatus == 2;
    }

    /// <summary>
    /// 单面测试结果汇总
    /// </summary>
    public class SideTestResult
    {
        public string SerialNumber { get; set; }
        public string Side { get; set; }
        public DateTime TestTime { get; set; }
        public int TotalDefects { get; set; }
        public int ConsistentCount { get; set; }
        public int InconsistentCount { get; set; }
        public double ConsistencyRate => TotalDefects > 0 ? (double)ConsistentCount / TotalDefects * 100 : 100;
        public List<DefectTestResult> DefectResults { get; set; } = new List<DefectTestResult>();
        public ValidationTestState State { get; set; }
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 原始面级别判定 (OK/NG)
        /// </summary>
        public string OriginalSideResult { get; set; }

        /// <summary>
        /// 新面级别判定 (OK/NG)
        /// </summary>
        public string NewSideResult { get; set; }

        /// <summary>
        /// 数据来源（AI或VVS）
        /// </summary>
        public OriginalDataSourceType DataSource { get; set; }

        /// <summary>
        /// 是否包含VVS数据
        /// </summary>
        public bool HasVVSData => DataSource == OriginalDataSourceType.VVS;

        /// <summary>
        /// 漏失数量（仅VVS数据有效）：VVS=NG但新AI=OK
        /// </summary>
        public int MissCount { get; set; }

        /// <summary>
        /// 误报数量（仅VVS数据有效）：VVS=OK但新AI=NG
        /// </summary>
        public int OverKillCount { get; set; }

        /// <summary>
        /// 漏失率（仅VVS数据有效）
        /// </summary>
        public double MissRate => HasVVSData && TotalDefects > 0 ? (double)MissCount / TotalDefects * 100 : 0;

        /// <summary>
        /// 误报率（仅VVS数据有效）
        /// </summary>
        public double OverKillRate => HasVVSData && TotalDefects > 0 ? (double)OverKillCount / TotalDefects * 100 : 0;
    }

    /// <summary>
    /// 批量测试任务
    /// </summary>
    public class ValidationTestTask
    {
        public string TaskId { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Description { get; set; }

        // 筛选条件
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string LotNumber { get; set; }
        public string ProductSerial { get; set; }
        public int? MaxRecords { get; set; }

        // 进度
        public int TotalRecords { get; set; }
        /// <summary>
        /// 已入队等待推理的记录数（用于显示入队进度）
        /// </summary>
        public int EnqueuedRecords { get; set; }
        /// <summary>
        /// 已完成推理并返回结果的记录数（用于计算实际进度）
        /// </summary>
        public int ProcessedRecords { get; set; }
        public int ConsistentRecords { get; set; }
        public int InconsistentRecords { get; set; }
        public int ErrorRecords { get; set; }

        /// <summary>
        /// 包含VVS数据的记录数
        /// </summary>
        public int VVSRecords { get; set; }
        /// <summary>
        /// 总漏失数（仅VVS数据）
        /// </summary>
        public int TotalMissCount { get; set; }
        /// <summary>
        /// 总误报数（仅VVS数据）
        /// </summary>
        public int TotalOverKillCount { get; set; }

        /// <summary>
        /// 入队进度（任务入队到推理队列的进度）
        /// </summary>
        public double EnqueueProgress => TotalRecords > 0 ? (double)EnqueuedRecords / TotalRecords * 100 : 0;
        /// <summary>
        /// 实际处理进度（推理结果返回的进度）
        /// </summary>
        public double Progress => TotalRecords > 0 ? (double)ProcessedRecords / TotalRecords * 100 : 0;
        public double OverallConsistencyRate => ProcessedRecords > 0
            ? (double)ConsistentRecords / ProcessedRecords * 100 : 0;
        /// <summary>
        /// 判断任务是否真正完成（所有入队的记录都已返回结果）
        /// </summary>
        public bool IsReallyCompleted => EnqueuedRecords > 0 && ProcessedRecords >= EnqueuedRecords;

        public ValidationTestTaskState State { get; set; } = ValidationTestTaskState.Created;
        public List<SideTestResult> Results { get; set; } = new List<SideTestResult>();
    }

    /// <summary>
    /// 测试任务状态
    /// </summary>
    public enum ValidationTestTaskState
    {
        Created = 0,
        Running = 1,
        Completed = 2,
        Cancelled = 3,
        Failed = 4
    }

    /// <summary>
    /// 用于提取测试数据的查询请求
    /// </summary>
    public class ValidationTestRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string LotNumber { get; set; }
        public string ProductSerial { get; set; }
        public int? MaxRecords { get; set; } = 100;
        public string Description { get; set; }
    }
}


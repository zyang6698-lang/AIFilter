using System;
using System.Collections.Generic;

namespace DeepSightModel
{
    /// <summary>
    /// 推理任务模式枚举 - 统一三种测试类型
    /// </summary>
    public enum InferenceMode
    {
        /// <summary>
        /// 一致性测试：比对新旧结果，不修改数据库
        /// </summary>
        ConsistencyTest = 0,

        /// <summary>
        /// 二次推理：重新推理NG点并覆写数据库
        /// </summary>
        SecondaryInference = 1,

        /// <summary>
        /// 单图测试：单张图片快速验证，同步等待结果
        /// </summary>
        SingleImageTest = 2
    }

    /// <summary>
    /// 统一的推理任务状态枚举
    /// </summary>
    public enum InferenceTaskState
    {
        Created = 0,
        Running = 1,
        Completed = 2,
        Cancelled = 3,
        Failed = 4
    }

    /// <summary>
    /// 统一的推理任务请求
    /// </summary>
    public class InferenceTaskRequest
    {
        public InferenceMode Mode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string LotNumber { get; set; }
        public string ProductSerial { get; set; }
        public int? MaxRecords { get; set; } = 100;
        public string Description { get; set; }
    }

    /// <summary>
    /// 统一的推理任务 - 替代 ValidationTestTask 和 SecondaryInferenceTask
    /// </summary>
    public class InferenceTask
    {
        public string TaskId { get; set; } = Guid.NewGuid().ToString();
        public InferenceMode Mode { get; set; }
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

        // 进度统计
        public int TotalRecords { get; set; }
        public int EnqueuedRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public int ErrorRecords { get; set; }

        // 一致性测试专用统计
        public int ConsistentRecords { get; set; }
        public int InconsistentRecords { get; set; }
        public int VVSRecords { get; set; }
        public int TotalMissCount { get; set; }
        public int TotalOverKillCount { get; set; }

        // 二次推理专用统计
        public int OkRecords { get; set; }
        public int NgRecords { get; set; }

        // 计算属性
        public double EnqueueProgress => TotalRecords > 0 ? (double)EnqueuedRecords / TotalRecords * 100 : 0;
        public double Progress => TotalRecords > 0 ? (double)ProcessedRecords / TotalRecords * 100 : 0;
        public bool IsReallyCompleted => EnqueuedRecords > 0 && ProcessedRecords >= EnqueuedRecords;
        public double OverallConsistencyRate => ProcessedRecords > 0 ? (double)ConsistentRecords / ProcessedRecords * 100 : 0;

        public InferenceTaskState State { get; set; } = InferenceTaskState.Created;

        // 结果存储
        public List<SideTestResult> ConsistencyResults { get; set; } = new List<SideTestResult>();
        public List<SecondaryInferenceResult> SecondaryResults { get; set; } = new List<SecondaryInferenceResult>();
    }

    /// <summary>
    /// 单图测试结果
    /// </summary>
    public class SingleImageTestResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int OriginalAIStatus { get; set; }
        public int NewAIStatus { get; set; }
        public bool IsConsistent => OriginalAIStatus == NewAIStatus;
        public string ImagePath { get; set; }
    }
}


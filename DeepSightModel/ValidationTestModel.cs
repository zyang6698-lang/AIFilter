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
        /// <summary>
        /// 原始缺陷信息快照，用于结果对比界面展示图片、ROI缺陷框和缺陷名称。
        /// </summary>
        public DetectInfo DetectInfo { get; set; }
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
        public string ProductSerial { get; set; }
        public string MachineId { get; set; }
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

    /// <summary>
    /// 二次推理请求
    /// </summary>
    public class SecondaryInferenceRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string LotNumber { get; set; }
        public string ProductSerial { get; set; }
        public int? MaxRecords { get; set; } = 100;
        public string Description { get; set; }
    }


    /// <summary>
    /// 二次推理单条结果
    /// </summary>
    public class SecondaryInferenceResult
    {
        public string SerialNumber { get; set; }
        public string Side { get; set; }
        public string ProductSerial { get; set; }
        public string MachineId { get; set; }
        public DateTime InferenceTime { get; set; }
        /// <summary>
        /// 推理前的NG点数
        /// </summary>
        public int OriginalNgCount { get; set; }
        /// <summary>
        /// 推理前状态为OK(1)的点数
        /// </summary>
        public int OriginalOkCount { get; set; }
        /// <summary>
        /// 推理前状态为异常/直报(3)的点数
        /// </summary>
        public int OriginalBypassCount { get; set; }
        /// <summary>
        /// 推理前状态为未检测(0)的点数
        /// </summary>
        public int OriginalUndetectedCount { get; set; }
        /// <summary>
        /// 推理后仍为NG的点数
        /// </summary>
        public int FinalNgCount { get; set; }
        /// <summary>
        /// 推理后变为OK的点数
        /// </summary>
        public int ChangedToOkCount { get; set; }
        /// <summary>
        /// 推理后状态为OK(1)的点数
        /// </summary>
        public int FinalOkCount { get; set; }
        /// <summary>
        /// 推理后状态为异常/直报(3)的点数
        /// </summary>
        public int FinalBypassCount { get; set; }
        /// <summary>
        /// 推理后状态为未检测(0)的点数
        /// </summary>
        public int FinalUndetectedCount { get; set; }
        /// <summary>
        /// 各点的详细结果
        /// </summary>
        public List<SecondaryInferencePointResult> PointResults { get; set; } = new List<SecondaryInferencePointResult>();
        public SecondaryInferenceResultState State { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 二次推理单点结果
    /// </summary>
    public class SecondaryInferencePointResult
    {
        public int DefectIndex { get; set; }
        public string ImagePath { get; set; }
        /// <summary>
        /// 推理前的缺陷信息快照，用于结果对比界面展示图片和缺陷框。
        /// </summary>
        public DetectInfo DetectInfo { get; set; }
        /// <summary>
        /// 原始AI状态（推理前，都是NG=2）
        /// </summary>
        public int OriginalAIStatus { get; set; }
        /// <summary>
        /// 新AI状态（推理后）
        /// </summary>
        public int NewAIStatus { get; set; }
        /// <summary>
        /// 是否发生变化（新状态与原始状态不同）
        /// </summary>
        public bool IsChanged => OriginalAIStatus != NewAIStatus;
    }

    /// <summary>
    /// 二次推理结果状态
    /// </summary>
    public enum SecondaryInferenceResultState
    {
        Pending = 0,
        Completed = 1,
        Error = 2
    }
}


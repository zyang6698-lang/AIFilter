using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightModel
{
    public class VBModel
    {
        public string Key { get; set; }
        public string SN { get; set; }
        public string Side { get; set; }
        public List<int> DefectIndex { get; set; }
        public List<int> PcsIndex { get; set; }
        public List<Mat> Mats { get;set; }
        public List<Mat> Mats_Temp { get; set; }
        public List<string > ImageKeys { get; set; }
        public List<string > ImageKeys_Gerber { get; set; }
        public List<string > ImageKeys_Temp { get; set; }

        public RootVBInfo VbInfo;
        //2025/08/21/增加minio路径信息
        public string  minioPath;
        public RootPanelInfo panelInfo { get; set; }
        //用于判断ai是否部署该料号，若未部署则为true
        public bool isByPass { get; set; } = false;

        /// <summary>
        /// 直报缺陷的原始 DefectIndex 列表（这些缺陷跳过AI推理，结果标记为bypass）
        /// </summary>
        public List<int> DirectReportDefectIndices { get; set; } = new List<int>();

        /// <summary>
        /// 直报缺陷对应的 PcsIndex 列表
        /// </summary>
        public List<int> DirectReportPcsIndices { get; set; } = new List<int>();

        #region 源数据库追踪（多DB回写支持）
        /// <summary>
        /// 数据来源的 LevelDB 服务器 URL（IP:Port），用于回写时定位目标服务器
        /// </summary>
        public string SourceDbUrl { get; set; }

        /// <summary>
        /// 回写目标数据库名称（对应 LevelDbConfig.WriteBackDbName）
        /// </summary>
        public string SourceWriteBackDbName { get; set; }
        #endregion

        #region 推理测试相关属性
        /// <summary>
        /// 是否为测试任务（用于区分正常推理和测试推理）
        /// </summary>
        public bool IsValidationTest { get; set; } = false;

        /// <summary>
        /// 推理模式（一致性测试/二次推理/单图测试）
        /// </summary>
        public InferenceMode InferenceMode { get; set; } = InferenceMode.ConsistencyTest;

        /// <summary>
        /// 测试任务 ID
        /// </summary>
        public string TestTaskId { get; set; }

        /// <summary>
        /// 原始推理结果（用于比对）格式: defectIndex -> AIStatus
        /// </summary>
        public Dictionary<int, int> OriginalAIResults { get; set; }

        /// <summary>
        /// 原始VVS复判结果（用于比对）格式: defectIndex -> VVSStatus
        /// </summary>
        public Dictionary<int, int> OriginalVVSResults { get; set; }

        /// <summary>
        /// 是否包含VVS数据
        /// </summary>
        public bool HasVVSData { get; set; } = false;

        /// <summary>
        /// 原始缺陷索引到DetectInfo的映射（用于二次推理更新数据库）
        /// </summary>
        public Dictionary<int, object> OriginalDetectInfos { get; set; }

        // 兼容属性 - 基于 InferenceMode 计算
        public bool IsSingleImageTest => InferenceMode == InferenceMode.SingleImageTest;
        public bool IsSecondaryInference => InferenceMode == InferenceMode.SecondaryInference;
        public bool IsConsistencyTest => InferenceMode == InferenceMode.ConsistencyTest;
        #endregion
    }

    /// <summary>
    /// 图片加载模型，用于解耦图片读取和推理
    /// </summary>
    public class ImageLoadModel
    {
        /// <summary>
        /// VB模型数据
        /// </summary>
        public VBModel Model { get; set; }


        /// <summary>
        /// 用于发送PanelInfo的对象
        /// </summary>
        public RootPanelInfoWithIP RootPanelInfo { get; set; }
    }
}

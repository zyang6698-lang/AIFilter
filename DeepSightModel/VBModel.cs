using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightModel
{
    public class VBModel : IDisposable
    {
        public string Key { get; set; }
        public string SN { get; set; }
        public string Side { get; set; }
        public List<int> DefectIndex { get; set; }
        public List<int> PcsIndex { get; set; }
        public List<Mat> Mats { get;set; }
        public List<Mat> Mats_Temp { get; set; }
        public List<Mat> Mats_Gerber { get; set; }
        public List<string > ImageKeys { get; set; }
        public List<string > ImageKeys_Gerber { get; set; }
        public List<string > ImageKeys_Temp { get; set; }

        public RootVBInfo VbInfo;
        //2025/08/21/增加minio路径信息
        public string  minioPath;

        #region 来源 PanelInfo 的扁平字段（JsonParseStage 解析后填充，避免下游直接依赖 RootPanelInfo DTO）
        /// <summary>
        /// 产品料号（来源 RootPanelInfo.ProductSerial）
        /// </summary>
        public string ProductSerial { get; set; }

        /// <summary>
        /// AVI 生成时间（原始字符串，来源 RootPanelInfo.AviCreateTime；下游负责解析）
        /// </summary>
        public string AviCreateTime { get; set; }

        /// <summary>
        /// 批次号（来源 RootPanelInfo.LotId）
        /// </summary>
        public string LotId { get; set; }

        /// <summary>
        /// 线体名称（来源 RootPanelInfo.LineName）
        /// </summary>
        public string LineName { get; set; }

        /// <summary>
        /// PanelInfo 本地描述文件路径（来源 RootPanelInfo.LocalDescribePath，用于回写 PanelJsonPath）
        /// </summary>
        public string LocalDescribePath { get; set; }

        /// <summary>
        /// 面板序列号（来源 RootPanelInfo.SerialNumber，仅用作 VRSDbKey；与 VBModel.SN 含义不同）
        /// </summary>
        public string PanelSerialNumber { get; set; }
        #endregion

        /// <summary>
        /// 直报缺陷的原始 DefectIndex 列表（这些缺陷跳过AI推理，结果标记为bypass）
        /// </summary>
        public List<int> DirectReportDefectIndices { get; set; } = new List<int>();

        /// <summary>
        /// 直报缺陷对应的 PcsIndex 列表
        /// </summary>
        public List<int> DirectReportPcsIndices { get; set; } = new List<int>();

        #region 全量图片路径（包含直报缺陷，用于后处理保存）
        /// <summary>
        /// 所有缺陷的 VRS 图片路径（包含直报缺陷，与 DirectReportFlags 索引对齐）
        /// </summary>
        public List<string> AllDefectImageKeys { get; set; }

        /// <summary>
        /// 所有缺陷的 Gerber 图片路径（包含直报缺陷，与 DirectReportFlags 索引对齐）
        /// </summary>
        public List<string> AllDefectGerberKeys { get; set; }

        /// <summary>
        /// 所有缺陷的 Template 图片路径（包含直报缺陷，与 DirectReportFlags 索引对齐）
        /// </summary>
        public List<string> AllDefectTempKeys { get; set; }

        /// <summary>
        /// 每个缺陷是否为直报缺陷的标记列表（与 AllDefectXxxKeys 索引对齐，true=直报）
        /// </summary>
        public List<bool> DirectReportFlags { get; set; }

        /// <summary>
        /// 所有缺陷的 AVI 原始报码（DefectCode），与 AllDefectXxxKeys 索引对齐，
        /// 用于后处理时填充 DetectInfo.DefectName，确保直报缺陷落库信息完整。
        /// </summary>
        public List<string> AllDefectCodes { get; set; }

        /// <summary>
        /// 每个缺陷是否为全局点的标记列表（与 AllDefectXxxKeys 索引对齐，
        /// true=来源 RootPanelInfo.PanelInfo（panel_info），false=来源 PcsInfo（pcs_info））
        /// </summary>
        public List<bool> GlobalFlags { get; set; }
        #endregion

        #region 源数据库追踪（多DB回写支持）
        /// <summary>
        /// AVI 侧 LevelDB 服务器 URL（IP:Port），用于 AVI 源读取与 AVI 回写
        /// </summary>
        public string SourceDbUrl { get; set; }

        /// <summary>
        /// VRS 侧 LevelDB 服务器 URL（IP:Port），用于 VRS 回写与 VRS V1.0 回写
        /// </summary>
        public string SourceVRSDbUrl { get; set; }

        /// <summary>
        /// 回写目标数据库名称（对应 LevelDbConfig.WriteBackDbName）
        /// </summary>
        public string SourceWriteBackDbName { get; set; }

        /// <summary>
        /// VRS回写目标数据库名称（对应 LevelDbConfig.VRSWriteBackDbName）
        /// </summary>
        public string SourceVRSWriteBackDbName { get; set; }
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
        public Dictionary<int, DetectInfo> OriginalDetectInfos { get; set; }

        // 兼容属性 - 基于 InferenceMode 计算
        public bool IsSingleImageTest => InferenceMode == InferenceMode.SingleImageTest;
        public bool IsSecondaryInference => InferenceMode == InferenceMode.SecondaryInference;
        public bool IsConsistencyTest => InferenceMode == InferenceMode.ConsistencyTest;
        #endregion

        #region IDisposable

        /// <summary>
        /// 释放所有 Mat 资源（Mats 和 Mats_Temp）
        /// </summary>
        public void Dispose()
        {
            DisposeMats(Mats);
            Mats = null;
            DisposeMats(Mats_Temp);
            Mats_Temp = null;
            DisposeMats(Mats_Gerber);
            Mats_Gerber = null;
        }

        private static void DisposeMats(List<Mat> mats)
        {
            if (mats == null) return;
            foreach (var mat in mats)
            {
                if (mat != null && !mat.IsDisposed)
                {
                    mat.Dispose();
                }
            }
            mats.Clear();
        }

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
        /// 面板信息 UI 投影（仅给 ImageLoadStage 通过 SystemEvent.SendPanelInfo 发往 UI 使用）
        /// </summary>
        public PanelInfoView PanelView { get; set; }
    }
}

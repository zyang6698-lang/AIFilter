using System.Collections.Generic;

namespace DeepSightModel
{
    /// <summary>
    /// 面板信息 UI 投影模型。由 JsonParseStage 从 RootPanelInfo + IP/Head 投影而来，
    /// 用于解耦 UI 通路与原始 JSON DTO。
    /// 字段集合限定在 UI 实际消费范围；缺陷已按 PcsInfo 字典顺序展平到 Defects。
    /// </summary>
    public class PanelInfoView
    {
        public string SN { get; set; }
        public string Side { get; set; }

        /// <summary>产品料号（来源 RootPanelInfo.ProductSerial）</summary>
        public string ProductSerial { get; set; }

        /// <summary>线体名称（来源 RootPanelInfo.LineName）</summary>
        public string LineName { get; set; }

        /// <summary>工站名（来源 RootPanelInfo.StationName）</summary>
        public string StationName { get; set; }

        /// <summary>批次号（来源 RootPanelInfo.LotId）</summary>
        public string LotId { get; set; }

        /// <summary>批号（来源 RootPanelInfo.LotBatch）</summary>
        public string LotBatch { get; set; }

        /// <summary>原始 SideIndex 字符串（来源 RootPanelInfo.SideIndex）</summary>
        public string SideIndex { get; set; }

        /// <summary>AVI 处理结束时间（来源 RootPanelInfo.EndTime，UI 显示用）</summary>
        public string EndTime { get; set; }

        /// <summary>MinIO URL 前缀（用于拼接缺陷图片地址）</summary>
        public string Head { get; set; }

        /// <summary>MinIO 服务 IP</summary>
        public string IP { get; set; }

        /// <summary>展平后的缺陷列表，顺序与 ImageLoaderService.GetAllImageKeysWithDirectReportFlags 对齐</summary>
        public List<DefectInfoView> Defects { get; set; } = new List<DefectInfoView>();
    }

    /// <summary>
    /// 单个缺陷的 UI 投影：仅保留 UI 实际需要的字段。
    /// </summary>
    public class DefectInfoView
    {
        public string DefectCode { get; set; }
        public int DefectIndex { get; set; }
        public int PcsIndex { get; set; }
        public string DefectLocation { get; set; }
        public string AiInferResult { get; set; }
        public Roi DefectRoi { get; set; }

        /// <summary>VRS 缺陷图（取 DefectVrsImages[0]，无则为 null）</summary>
        public string DefectVrsImage { get; set; }

        /// <summary>VRS Gerber 图（取 DefectVrsGerberImages[0]，无则为 null）</summary>
        public string DefectVrsGerberImage { get; set; }

        /// <summary>VRS OK 图（取 DefectVrsOkImages[0]，无则为 null）</summary>
        public string DefectVrsOkImage { get; set; }
    }
}

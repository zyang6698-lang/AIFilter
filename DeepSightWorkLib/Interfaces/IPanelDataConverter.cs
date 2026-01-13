using DeepSightModel;
using System.Collections.Generic;

namespace DeepSightWorkLib.Interfaces
{
    /// <summary>
    /// Panel 数据转换上下文（包含转换所需的配置和状态）
    /// </summary>
    public class PanelConvertContext
    {
        /// <summary>
        /// Minio IP 地址
        /// </summary>
        public string MinioIP { get; set; }

        /// <summary>
        /// Minio 端口
        /// </summary>
        public string MinioPort { get; set; }

        /// <summary>
        /// Minio 路径前缀（head）
        /// </summary>
        public string Head { get; set; }

        /// <summary>
        /// 方案配置
        /// </summary>
        public SolutionConfig SolutionConfig { get; set; }

        /// <summary>
        /// AVI 配置
        /// </summary>
        public AVIConfig AviConfig { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }
    }

    /// <summary>
    /// Panel 数据转换结果
    /// </summary>
    public class PanelConvertResult
    {
        /// <summary>
        /// 转换后的 VB 信息
        /// </summary>
        public RootVBInfo VBInfo { get; set; }

        /// <summary>
        /// 缺陷索引列表
        /// </summary>
        public List<int> DefectIndexList { get; set; } = new List<int>();

        /// <summary>
        /// PCS 索引列表
        /// </summary>
        public List<int> PcsIndexList { get; set; } = new List<int>();

        /// <summary>
        /// 是否为 ByPass 模式（料号未配置）
        /// </summary>
        public bool IsByPass { get; set; }

        /// <summary>
        /// 中台数据信息
        /// </summary>
        public DsCenterInfo DsCenterInfo { get; set; }
    }

    /// <summary>
    /// Panel 数据转换服务接口
    /// 负责将 RootPanelInfo 转换为 AI 推理所需的 RootVBInfo 格式
    /// </summary>
    public interface IPanelDataConverter
    {
        /// <summary>
        /// 将 Panel JSON 数据转换为 VB 推理信息
        /// </summary>
        /// <param name="panelInfo">原始 Panel 信息</param>
        /// <param name="context">转换上下文（包含配置信息）</param>
        /// <returns>转换结果</returns>
        PanelConvertResult Convert(RootPanelInfo panelInfo, PanelConvertContext context);

        /// <summary>
        /// 获取或创建指定 Panel 的中台数据信息
        /// </summary>
        /// <param name="lotId">批次ID</param>
        /// <param name="serialNumber">序列号</param>
        /// <returns>中台数据信息（如果存在）</returns>
        DsCenterInfo GetDsCenterInfo(string lotId, string serialNumber);

        /// <summary>
        /// 存储中台数据信息
        /// </summary>
        /// <param name="lotId">批次ID</param>
        /// <param name="serialNumber">序列号</param>
        /// <param name="info">中台数据信息</param>
        void SetDsCenterInfo(string lotId, string serialNumber, DsCenterInfo info);

        /// <summary>
        /// 清空所有中台数据缓存
        /// </summary>
        void ClearDsCenterInfoCache();
    }
}


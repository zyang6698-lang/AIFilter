using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DeepSightModel.Configuration
{
    /// <summary>
    /// 数据源类型枚举
    /// </summary>
    public enum DataSourceType
    {
        /// <summary>
        /// 通过 ATS_Agent.exe 接入数据（需要完整的 AVIConfig 配置）
        /// </summary>
        Agent,

        /// <summary>
        /// 直接通过 LevelDB 接入数据（不需要 ATS_Agent.exe）
        /// </summary>
        LevelDb
    }

    /// <summary>
    /// 机台基础信息（两套数据接入系统共用）
    /// </summary>
    public class MachineEntry
    {
        /// <summary>
        /// 机台名称（唯一标识，与 WatchPathConfig.AviName 关联）
        /// </summary>
        [JsonProperty("machine_name")]
        public string MachineName { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [JsonProperty("is_enable")]
        public bool IsEnable { get; set; }

        /// <summary>
        /// 数据源类型，决定该机台走哪套数据接入流程
        /// </summary>
        [JsonProperty("data_source_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DataSourceType DataSourceType { get; set; } = DataSourceType.LevelDb;
    }

    /// <summary>
    /// 机台注册表（统一管理所有机台的基础信息）
    /// </summary>
    public class MachineRegistryConfig
    {
        /// <summary>
        /// 所有注册的机台列表
        /// </summary>
        [JsonProperty("machines")]
        public List<MachineEntry> Machines { get; set; } = new List<MachineEntry>();
    }
}


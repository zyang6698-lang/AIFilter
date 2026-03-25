using System.Collections.Generic;

namespace DeepSightModel.Configuration
{
    /// <summary>
    /// 单个缺陷条目（已知缺陷列表中的一项）
    /// </summary>
    public class KeyDefectEntry
    {
        /// <summary>
        /// 缺陷名称（如 AU10, CU10 等）
        /// </summary>
        public string DefectName { get; set; }

        /// <summary>
        /// 是否标记为重点缺陷
        /// </summary>
        public bool IsKey { get; set; }

        /// <summary>
        /// 是否由系统自动发现（而非手动添加）
        /// </summary>
        public bool AutoDiscovered { get; set; }
    }

    /// <summary>
    /// 重点缺陷报警配置
    /// </summary>
    public class KeyDefectAlarmConfig
    {
        /// <summary>
        /// 是否启用重点缺陷报警
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 报警阈值：重点缺陷数 / 总缺陷数 的比例（0~1），超过此值触发报警
        /// </summary>
        public double AlarmRatioThreshold { get; set; } = 0.3;

        /// <summary>
        /// 报警阈值：重点缺陷绝对数量，超过此值触发报警（0 表示不启用绝对数量报警）
        /// </summary>
        public int AlarmCountThreshold { get; set; } = 0;

        /// <summary>
        /// 报警冷却时间（秒），避免短时间内重复报警
        /// </summary>
        public int AlarmCooldownSeconds { get; set; } = 300;
    }

    /// <summary>
    /// 重点缺陷管理完整配置
    /// </summary>
    public class KeyDefectConfig
    {
        /// <summary>
        /// 已知缺陷列表（自动发现 + 手动添加）
        /// </summary>
        public List<KeyDefectEntry> DefectEntries { get; set; } = new List<KeyDefectEntry>();

        /// <summary>
        /// 报警配置
        /// </summary>
        public KeyDefectAlarmConfig AlarmConfig { get; set; } = new KeyDefectAlarmConfig();
    }
}


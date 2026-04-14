namespace DeepSightModel.Alarm
{
    /// <summary>
    /// 告警等级
    /// </summary>
    public enum AlarmLevel
    {
        /// <summary>提示信息（队列恢复正常等）</summary>
        Info = 0,

        /// <summary>警告（推理耗时偏高、磁盘空间不足等）</summary>
        Warning = 1,

        /// <summary>错误（算法异常、通信失败等）</summary>
        Error = 2,

        /// <summary>严重（AI服务不可用、系统即将宕机等）</summary>
        Critical = 3
    }
}

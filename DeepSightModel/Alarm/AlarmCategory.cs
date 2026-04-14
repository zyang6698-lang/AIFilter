namespace DeepSightModel.Alarm
{
    /// <summary>
    /// 告警分类
    /// </summary>
    public enum AlarmCategory
    {
        /// <summary>系统级（内存、磁盘、CPU）</summary>
        System = 0,

        /// <summary>AI推理相关（算法异常、超时）</summary>
        AI = 1,

        /// <summary>通信相关（LevelDB、Minio）</summary>
        Communication = 2,

        /// <summary>缺陷业务相关（重点缺陷报警）</summary>
        Defect = 3,

        /// <summary>硬件设备相关</summary>
        Hardware = 4
    }
}

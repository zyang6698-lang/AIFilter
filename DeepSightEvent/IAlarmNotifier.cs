using DeepSightModel.Alarm;

namespace DeepSightEvent
{
    /// <summary>
    /// 告警通知渠道接口
    /// </summary>
    public interface IAlarmNotifier
    {
        /// <summary>通知器名称</summary>
        string Name { get; }

        /// <summary>最低触发等级（低于此等级的告警不通知）</summary>
        AlarmLevel MinLevel { get; }

        /// <summary>发送告警通知</summary>
        void Notify(AlarmInfo alarm);
    }
}

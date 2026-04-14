using DeepSightModel.Alarm;
using DeepSightTool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DeepSightEvent
{
    /// <summary>
    /// 告警核心服务（单例）—— 接收、冷却/聚合、分发告警
    /// </summary>
    public class AlarmService
    {
        #region 单例

        private static readonly Lazy<AlarmService> _lazy =
            new Lazy<AlarmService>(() => new AlarmService());

        public static AlarmService Instance => _lazy.Value;

        private AlarmService() { }

        #endregion

        #region 事件（供 UI 直接订阅）

        /// <summary>
        /// 新告警事件（UI 层订阅此事件刷新界面）
        /// </summary>
        public event Action<AlarmInfo> OnAlarmRaised;

        #endregion

        #region 配置

        /// <summary>默认冷却时间（秒）</summary>
        public int DefaultCooldownSeconds { get; set; } = 30;

        /// <summary>告警历史最大保留条数</summary>
        public int MaxHistoryCount { get; set; } = 500;

        #endregion

        #region 内部状态

        // 冷却字典：key = CooldownKey, value = (上次触发时间, 累计次数, 最近一条AlarmInfo)
        private readonly ConcurrentDictionary<string, CooldownEntry> _cooldownMap =
            new ConcurrentDictionary<string, CooldownEntry>();

        // 告警历史（环形缓冲）
        private readonly ConcurrentQueue<AlarmInfo> _history = new ConcurrentQueue<AlarmInfo>();

        // 通知渠道
        private readonly List<IAlarmNotifier> _notifiers = new List<IAlarmNotifier>();
        private readonly object _notifierLock = new object();

        #endregion

        #region 公共方法

        /// <summary>
        /// 注册通知渠道
        /// </summary>
        public void RegisterNotifier(IAlarmNotifier notifier)
        {
            if (notifier == null) return;
            lock (_notifierLock)
            {
                _notifiers.Add(notifier);
            }
            LogTextHelper.Info($"[AlarmService] 已注册通知渠道: {notifier.Name} (MinLevel={notifier.MinLevel})");
        }

        /// <summary>
        /// 发起结构化告警
        /// </summary>
        public void RaiseAlarm(AlarmLevel level, AlarmCategory category,
            string source, string message, string detail = null, string relatedSN = null)
        {
            var alarm = AlarmInfo.Create(level, category, source, message, detail, relatedSN);
            ProcessAlarm(alarm);
        }

        /// <summary>
        /// 发起结构化告警（直接传 AlarmInfo）
        /// </summary>
        public void RaiseAlarm(AlarmInfo alarm)
        {
            if (alarm == null) return;
            ProcessAlarm(alarm);
        }

        /// <summary>
        /// 向后兼容：从旧的 string 消息推断等级和分类
        /// </summary>
        public void RaiseAlarmFromLegacy(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            var level = InferLevel(message);
            var category = InferCategory(message);
            RaiseAlarm(level, category, "Legacy", message);
        }

        /// <summary>确认告警</summary>
        public void AcknowledgeAlarm(string alarmId)
        {
            foreach (var alarm in _history)
            {
                if (alarm.Id == alarmId && !alarm.IsAcknowledged)
                {
                    alarm.IsAcknowledged = true;
                    alarm.AcknowledgedTime = DateTime.Now;
                    break;
                }
            }
        }

        /// <summary>获取最近的告警历史</summary>
        public List<AlarmInfo> GetRecentAlarms(int count = 100)
        {
            return _history.ToArray()
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();
        }

        /// <summary>获取今日各等级告警统计</summary>
        public AlarmStatistics GetTodayStatistics()
        {
            var today = DateTime.Today;
            var todayAlarms = _history.Where(a => a.Timestamp >= today).ToArray();
            return new AlarmStatistics
            {
                InfoCount = todayAlarms.Count(a => a.Level == AlarmLevel.Info),
                WarningCount = todayAlarms.Count(a => a.Level == AlarmLevel.Warning),
                ErrorCount = todayAlarms.Count(a => a.Level == AlarmLevel.Error),
                CriticalCount = todayAlarms.Count(a => a.Level == AlarmLevel.Critical),
                TotalCount = todayAlarms.Length,
                UnacknowledgedCount = todayAlarms.Count(a => !a.IsAcknowledged)
            };
        }

        #endregion

        #region 内部逻辑

        private void ProcessAlarm(AlarmInfo alarm)
        {
            // 冷却/聚合检查
            string cooldownKey = alarm.GetCooldownKey();
            var now = DateTime.Now;

            var entry = _cooldownMap.GetOrAdd(cooldownKey, _ => new CooldownEntry());
            lock (entry)
            {
                if ((now - entry.LastFireTime).TotalSeconds < DefaultCooldownSeconds)
                {
                    // 冷却期内：仅累加计数，不分发
                    entry.Count++;
                    if (entry.LatestAlarm != null)
                    {
                        entry.LatestAlarm.OccurrenceCount = entry.Count;
                    }
                    return;
                }

                // 冷却期已过：重置并分发
                entry.LastFireTime = now;
                entry.Count = 1;
                entry.LatestAlarm = alarm;
            }

            // 写入历史
            _history.Enqueue(alarm);
            while (_history.Count > MaxHistoryCount)
            {
                _history.TryDequeue(out _);
            }

            // 触发事件
            try { OnAlarmRaised?.Invoke(alarm); }
            catch (Exception ex) { LogTextHelper.Error($"[AlarmService] OnAlarmRaised handler error: {ex.Message}"); }

            // 分发到各通知渠道
            DispatchToNotifiers(alarm);
        }

        private void DispatchToNotifiers(AlarmInfo alarm)
        {
            IAlarmNotifier[] notifiers;
            lock (_notifierLock)
            {
                notifiers = _notifiers.ToArray();
            }

            foreach (var notifier in notifiers)
            {
                if (alarm.Level >= notifier.MinLevel)
                {
                    try
                    {
                        notifier.Notify(alarm);
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"[AlarmService] Notifier '{notifier.Name}' error: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>根据消息关键词推断告警等级</summary>
        private static AlarmLevel InferLevel(string message)
        {
            if (string.IsNullOrEmpty(message)) return AlarmLevel.Info;
            string msg = message.ToUpperInvariant();
            if (msg.Contains("严重") || msg.Contains("CRITICAL") || msg.Contains("不可用"))
                return AlarmLevel.Critical;
            if (msg.Contains("异常") || msg.Contains("EXCEPTION") || msg.Contains("ERROR") || msg.Contains("失败"))
                return AlarmLevel.Error;
            if (msg.Contains("报警") || msg.Contains("WARNING") || msg.Contains("超时") || msg.Contains("TIMEOUT"))
                return AlarmLevel.Warning;
            return AlarmLevel.Info;
        }

        /// <summary>根据消息关键词推断告警分类</summary>
        private static AlarmCategory InferCategory(string message)
        {
            if (string.IsNullOrEmpty(message)) return AlarmCategory.System;
            string msg = message.ToUpperInvariant();
            if (msg.Contains("算法") || msg.Contains("推理") || msg.Contains("INFER") || msg.Contains("AI") || msg.Contains("VB"))
                return AlarmCategory.AI;
            if (msg.Contains("LEVELDB") || msg.Contains("MINIO") || msg.Contains("通信") || msg.Contains("HTTP") || msg.Contains("拉取"))
                return AlarmCategory.Communication;
            if (msg.Contains("缺陷") || msg.Contains("DEFECT") || msg.Contains("报警"))
                return AlarmCategory.Defect;
            return AlarmCategory.System;
        }

        #endregion

        #region 内部类型

        private class CooldownEntry
        {
            public DateTime LastFireTime = DateTime.MinValue;
            public int Count;
            public AlarmInfo LatestAlarm;
        }

        #endregion
    }

    /// <summary>
    /// 告警统计信息
    /// </summary>
    public class AlarmStatistics
    {
        public int InfoCount { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        public int CriticalCount { get; set; }
        public int TotalCount { get; set; }
        public int UnacknowledgedCount { get; set; }
    }
}

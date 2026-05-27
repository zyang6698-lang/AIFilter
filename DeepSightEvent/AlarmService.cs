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

        private const string LogSourceName = "LogTextHelper";
        private const string LegacySourceName = "Legacy";

        private AlarmService()
        {
            LogTextHelper.OnLogWritten += HandleLogWritten;
        }

        #endregion

        #region 事件（供 UI 直接订阅）

        /// <summary>
        /// 新告警事件（UI 层订阅此事件刷新界面）
        /// </summary>
        public event Action<AlarmInfo> OnAlarmRaised;

        public event Action<RuntimeMessageInfo> OnRuntimeMessageRaised;

        #endregion

        #region 配置

        /// <summary>默认冷却时间（秒）</summary>
        public int DefaultCooldownSeconds { get; set; } = 30;

        /// <summary>告警历史最大保留条数</summary>
        public int MaxHistoryCount { get; set; } = 500;

        public int RuntimeMessageCooldownSeconds { get; set; } = 5;

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

        private readonly ConcurrentDictionary<string, DateTime> _runtimeMessageCooldownMap =
            new ConcurrentDictionary<string, DateTime>();

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

        public void EnsureInitialized()
        {
        }

        /// <summary>
        /// 发起结构化告警
        /// </summary>
        public void RaiseAlarm(AlarmLevel level, AlarmCategory category,
            string source, string message, string detail = null, string relatedSN = null,
            string code = null, string cooldownKey = null)
        {
            var alarm = AlarmInfo.Create(level, category, source, message, detail, relatedSN, code, cooldownKey);
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
            RaiseAlarm(level, category, LegacySourceName, message);
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
            if (alarm == null) return;
            ProcessAlarmCore(alarm);
        }

        /// <summary>
        /// 把告警同步落一行日志，等级映射到 LogTextHelper
        /// 调用点不再需要双写 (LogTextHelper.Xxx + RaiseAlarm)，集中在此处一次输出
        /// </summary>
        private static void WriteAlarmLog(AlarmInfo alarm)
        {
            try
            {
                if (string.Equals(alarm.Source, LogSourceName, StringComparison.Ordinal))
                {
                    return;
                }

                var sb = new System.Text.StringBuilder(128);
                sb.Append("[Alarm][").Append(alarm.Level).Append("][")
                  .Append(alarm.Category).Append('/').Append(alarm.Source).Append("] ")
                  .Append(alarm.Message);
                if (!string.IsNullOrEmpty(alarm.Code))
                    sb.Append(" Code=").Append(alarm.Code);
                if (!string.IsNullOrEmpty(alarm.RelatedSN))
                    sb.Append(" SN=").Append(alarm.RelatedSN);
                if (!string.IsNullOrEmpty(alarm.Detail))
                    sb.Append(" | ").Append(alarm.Detail);
                string line = sb.ToString();

                switch (alarm.Level)
                {
                    case AlarmLevel.Critical:
                    case AlarmLevel.Error:
                        LogTextHelper.WriteFromAlarmService(LogTextHelper.LogTextLevel.Error, line);
                        break;
                    case AlarmLevel.Warning:
                        LogTextHelper.WriteFromAlarmService(LogTextHelper.LogTextLevel.Warn, line);
                        break;
                    default:
                        LogTextHelper.WriteFromAlarmService(LogTextHelper.LogTextLevel.Info, line);
                        break;
                }
            }
            catch
            {
                // 日志失败不影响告警分发
            }
        }

        private void HandleLogWritten(LogTextHelper.LogTextLevel level, string message, Exception exception, string cooldownKey, string source)
        {
            if (string.IsNullOrEmpty(message)) return;

            var alarmLevel = ConvertLogLevel(level, message);
            var detail = exception?.ToString();
            string alarmSource = string.IsNullOrEmpty(source) ? LogSourceName : source;

            if (level >= LogTextHelper.LogTextLevel.Warn)
            {
                var category = InferCategory(message);
                var alarm = AlarmInfo.Create(alarmLevel, category, alarmSource, message, detail,
                    cooldownKey: cooldownKey);
                ProcessAlarmCore(alarm, cooldownKey, skipLog: true);
            }
            else
            {
                PublishRuntimeMessage(alarmLevel, alarmSource, message, detail, cooldownKey);
            }
        }

        private void ProcessAlarmCore(AlarmInfo alarm, string cooldownKeyOverride = null, bool skipLog = false)
        {
            string cooldownKey = GetCooldownKey(alarm, cooldownKeyOverride);
            var now = DateTime.Now;

            var entry = _cooldownMap.GetOrAdd(cooldownKey, _ => new CooldownEntry());
            lock (entry)
            {
                if ((now - entry.LastFireTime).TotalSeconds < DefaultCooldownSeconds)
                {
                    entry.Count++;
                    if (entry.LatestAlarm != null)
                    {
                        entry.LatestAlarm.OccurrenceCount = entry.Count;
                    }
                    return;
                }

                entry.LastFireTime = now;
                entry.Count = 1;
                entry.LatestAlarm = alarm;
            }

            _history.Enqueue(alarm);
            while (_history.Count > MaxHistoryCount)
            {
                _history.TryDequeue(out _);
            }

            if (!skipLog)
            {
                WriteAlarmLog(alarm);
            }

            PublishRuntimeMessage(alarm.Level, alarm.Source, alarm.Message, alarm.Detail, null, false);

            try { OnAlarmRaised?.Invoke(alarm); }
            catch (Exception ex) { LogTextHelper.Error($"[AlarmService] OnAlarmRaised handler error: {ex.Message}"); }

            DispatchToNotifiers(alarm);
        }

        private void PublishRuntimeMessage(AlarmLevel level, string source, string message, string detail = null, string cooldownKey = null, bool applyCooldown = true)
        {
            var runtimeMessage = RuntimeMessageInfo.Create(level, source, message, detail);
            if (applyCooldown && IsRuntimeMessageCooling(runtimeMessage, cooldownKey)) return;

            try { OnRuntimeMessageRaised?.Invoke(runtimeMessage); }
            catch (Exception ex) { LogTextHelper.WriteFromAlarmService(LogTextHelper.LogTextLevel.Error, $"[AlarmService] OnRuntimeMessageRaised handler error: {ex.Message}"); }
        }

        private bool IsRuntimeMessageCooling(RuntimeMessageInfo runtimeMessage, string cooldownKey)
        {
            string key = string.IsNullOrWhiteSpace(cooldownKey)
                ? runtimeMessage.GetCooldownKey()
                : $"{runtimeMessage.Level}_{runtimeMessage.Source}_{cooldownKey}";
            var now = DateTime.Now;
            var lastFire = _runtimeMessageCooldownMap.GetOrAdd(key, _ => DateTime.MinValue);
            if ((now - lastFire).TotalSeconds < RuntimeMessageCooldownSeconds)
            {
                return true;
            }

            _runtimeMessageCooldownMap.TryUpdate(key, now, lastFire);
            return false;
        }

        private static string GetCooldownKey(AlarmInfo alarm, string cooldownKeyOverride = null)
        {
            if (!string.IsNullOrWhiteSpace(cooldownKeyOverride))
            {
                return $"{alarm.Category}_{alarm.Source}_{cooldownKeyOverride}";
            }

            if (string.Equals(alarm.Source, LogSourceName, StringComparison.Ordinal) ||
                string.Equals(alarm.Source, LegacySourceName, StringComparison.Ordinal))
            {
                string msgKey = alarm.Message?.Length > 80 ? alarm.Message.Substring(0, 80) : (alarm.Message ?? "");
                return $"{alarm.Category}_{msgKey}";
            }

            return alarm.GetCooldownKey();
        }

        private static AlarmLevel ConvertLogLevel(LogTextHelper.LogTextLevel level, string message)
        {
            switch (level)
            {
                case LogTextHelper.LogTextLevel.Warn:
                    return AlarmLevel.Warning;
                case LogTextHelper.LogTextLevel.Error:
                    return InferLevel(message) > AlarmLevel.Error ? AlarmLevel.Critical : AlarmLevel.Error;
                default:
                    return AlarmLevel.Info;
            }
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

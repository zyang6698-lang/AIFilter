using System;

namespace DeepSightModel.Alarm
{
    /// <summary>
    /// 结构化告警信息
    /// </summary>
    public class AlarmInfo
    {
        /// <summary>唯一标识</summary>
        public string Id { get; set; }

        /// <summary>发生时间</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>告警等级</summary>
        public AlarmLevel Level { get; set; }

        /// <summary>告警分类</summary>
        public AlarmCategory Category { get; set; }

        /// <summary>来源模块（如 DefectProcessor、AviReader）</summary>
        public string Source { get; set; }

        public string Code { get; set; }

        public string CooldownKey { get; set; }

        /// <summary>告警消息（简短描述）</summary>
        public string Message { get; set; }

        /// <summary>详细信息（堆栈、上下文等）</summary>
        public string Detail { get; set; }

        /// <summary>关联的产品序列号（可选）</summary>
        public string RelatedSN { get; set; }

        /// <summary>是否已确认</summary>
        public bool IsAcknowledged { get; set; }

        /// <summary>确认时间</summary>
        public DateTime? AcknowledgedTime { get; set; }

        /// <summary>聚合计数（冷却期内相同告警的累计次数）</summary>
        public int OccurrenceCount { get; set; } = 1;

        /// <summary>
        /// 创建告警信息的工厂方法
        /// </summary>
        public static AlarmInfo Create(AlarmLevel level, AlarmCategory category,
            string source, string message, string detail = null, string relatedSN = null,
            string code = null, string cooldownKey = null)
        {
            return new AlarmInfo
            {
                Id = Guid.NewGuid().ToString("N"),
                Timestamp = DateTime.Now,
                Level = level,
                Category = category,
                Source = source,
                Code = code,
                CooldownKey = cooldownKey,
                Message = message,
                Detail = detail,
                RelatedSN = relatedSN
            };
        }

        /// <summary>
        /// 生成冷却 Key（相同 Source + Category + Message 前50字符）
        /// </summary>
        public string GetCooldownKey()
        {
            if (!string.IsNullOrWhiteSpace(CooldownKey))
            {
                return $"{Category}_{Source}_{CooldownKey}";
            }

            if (!string.IsNullOrWhiteSpace(Code))
            {
                return $"{Category}_{Source}_{Code}";
            }

            string msgKey = Message?.Length > 50 ? Message.Substring(0, 50) : (Message ?? "");
            return $"{Category}_{Source}_{msgKey}";
        }

        public override string ToString()
        {
            return $"[{Level}][{Category}] {Timestamp:HH:mm:ss} [{Source}] {Message}";
        }
    }
}

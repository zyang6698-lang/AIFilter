using System;

namespace DeepSightModel.Alarm
{
    public class RuntimeMessageInfo
    {
        public DateTime Timestamp { get; set; }

        public AlarmLevel Level { get; set; }

        public string Source { get; set; }

        public string Message { get; set; }

        public string Detail { get; set; }

        public static RuntimeMessageInfo Create(AlarmLevel level, string source, string message, string detail = null)
        {
            return new RuntimeMessageInfo
            {
                Timestamp = DateTime.Now,
                Level = level,
                Source = source,
                Message = message,
                Detail = detail
            };
        }

        public string GetCooldownKey()
        {
            string msgKey = Message?.Length > 80 ? Message.Substring(0, 80) : (Message ?? string.Empty);
            return $"{Level}_{Source}_{msgKey}";
        }
    }
}

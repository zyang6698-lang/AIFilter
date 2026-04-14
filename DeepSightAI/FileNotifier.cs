using DeepSightEvent;
using DeepSightModel.Alarm;
using DeepSightTool;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace DeepSightAI
{
    /// <summary>
    /// 告警文件持久化通知器 —— 将结构化告警以 JSON 行格式写入日志文件
    /// 每天一个文件：ExceptionLog/{yyyy-MM-dd}/alarm.jsonl
    /// </summary>
    public class FileNotifier : IAlarmNotifier
    {
        public string Name => "FileNotifier";
        public AlarmLevel MinLevel { get; set; }

        private readonly string _baseDir;
        private readonly object _writeLock = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="baseDir">日志根目录（默认 App\ExceptionLog）</param>
        /// <param name="minLevel">最低记录等级（默认 Info，即全部记录）</param>
        public FileNotifier(string baseDir = null, AlarmLevel minLevel = AlarmLevel.Info)
        {
            _baseDir = baseDir ?? Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "ExceptionLog");
            MinLevel = minLevel;
        }

        public void Notify(AlarmInfo alarm)
        {
            if (alarm == null) return;
            try
            {
                string dayDir = Path.Combine(_baseDir, DateTime.Now.ToString("yyyy-MM-dd"));
                if (!Directory.Exists(dayDir))
                {
                    Directory.CreateDirectory(dayDir);
                }

                string filePath = Path.Combine(dayDir, "alarm.jsonl");
                string json = JsonConvert.SerializeObject(alarm, Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DateFormatString = "yyyy-MM-dd HH:mm:ss.fff"
                    });

                lock (_writeLock)
                {
                    using (var writer = new StreamWriter(filePath, true, Encoding.UTF8))
                    {
                        writer.WriteLine(json);
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"[FileNotifier] Write error: {ex.Message}");
            }
        }
    }
}

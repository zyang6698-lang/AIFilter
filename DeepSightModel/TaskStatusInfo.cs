using System;

namespace DeepSightModel
{
    /// <summary>
    /// 任务状态信息模型
    /// </summary>
    public class TaskStatusInfo
    {
        /// <summary>
        /// 序列号（SN）
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatus Status { get; set; }

        /// <summary>
        /// 面（A/B）
        /// </summary>
        public string Side { get; set; }

        /// <summary>
        /// 附加消息（可选）
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// AI处理时间（毫秒）
        /// </summary>
        public long ProcessingTimeMs { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        public TaskStatusInfo()
        {
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// 获取完整的显示消息
        /// </summary>
        public string GetFullDisplayMessage()
        {
            string baseMessage = TaskStatusHelper.GetStatusDisplayText(Status, Side);
            
            if (!string.IsNullOrEmpty(Message))
            {
                return $"{baseMessage} - {Message}";
            }
            
            return baseMessage;
        }

        /// <summary>
        /// 创建新任务（排队状态）
        /// </summary>
        public static TaskStatusInfo CreateQueued(string serialNumber)
        {
            return new TaskStatusInfo
            {
                SerialNumber = serialNumber,
                Status = TaskStatus.Queued,
                Side = ""
            };
        }

        /// <summary>
        /// 创建状态更新
        /// </summary>
        public static TaskStatusInfo Create(string serialNumber, TaskStatus status, string side = "", string message = "", long timeMs = 0)
        {
            return new TaskStatusInfo
            {
                SerialNumber = serialNumber,
                Status = status,
                Side = side,
                Message = message,
                ProcessingTimeMs = timeMs
            };
        }

        public override string ToString()
        {
            return $"[{SerialNumber}] {GetFullDisplayMessage()} ({Timestamp:HH:mm:ss})";
        }
    }
}

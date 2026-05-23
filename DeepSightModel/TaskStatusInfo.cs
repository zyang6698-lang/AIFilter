using System;

namespace DeepSightModel
{
    /// <summary>
    /// ����״̬��Ϣģ��
    /// </summary>
    public class TaskStatusInfo
    {
        private const string OnlineTaskType = "\u5728\u7ebf";

        /// <summary>
        /// ���кţ�SN��
        /// </summary>
        public string SerialNumber { get; set; }

        public string TaskId { get; set; }

        public string TaskType { get; set; } = OnlineTaskType;

        public string DisplayName { get; set; }

        public int TotalCount { get; set; }

        public int FinishedCount { get; set; }

        /// <summary>
        /// ����״̬
        /// </summary>
        public TaskStatus Status { get; set; }

        /// <summary>
        /// �棨A/B��
        /// </summary>
        public string Side { get; set; }

        /// <summary>
        /// ������Ϣ����ѡ��
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// AI����ʱ�䣨���룩
        /// </summary>
        public long ProcessingTimeMs { get; set; }

        /// <summary>
        /// ʱ���
        /// </summary>
        public DateTime Timestamp { get; set; }

        public TaskStatusInfo()
        {
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// ��ȡ��������ʾ��Ϣ
        /// </summary>
        public string GetFullDisplayMessage()
        {
            string baseMessage = TaskStatusHelper.GetStatusDisplayText(Status, Side);
            
            if (!string.IsNullOrEmpty(Message))
            {
                baseMessage = $"{baseMessage} - {Message}";
            }
            
            if (TotalCount > 0)
            {
                double progress = Math.Min(100, Math.Max(0, (double)FinishedCount / TotalCount * 100));
                return $"{baseMessage} [{FinishedCount}/{TotalCount} {progress:F0}%]";
            }

            return baseMessage;
        }

        public string GetRowKey()
        {
            if (!string.IsNullOrWhiteSpace(TaskId))
            {
                return $"TASK|{TaskType}|{TaskId}";
            }

            return $"SN|{SerialNumber}|{Side}";
        }

        public string GetDisplayName()
        {
            string name = !string.IsNullOrWhiteSpace(DisplayName) ? DisplayName : SerialNumber;
            string taskType = string.IsNullOrWhiteSpace(TaskType) ? OnlineTaskType : TaskType;

            if (taskType == OnlineTaskType)
            {
                return name;
            }

            return $"[{taskType}] {name}";
        }

        /// <summary>
        /// �����������Ŷ�״̬��
        /// </summary>
        public static TaskStatusInfo CreateQueued(string serialNumber, string side = "")
        {
            return new TaskStatusInfo
            {
                SerialNumber = serialNumber,
                Status = TaskStatus.Queued,
                Side = side
            };
        }

        /// <summary>
        /// ����״̬����
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

        public static TaskStatusInfo CreateTask(string taskId, string taskType, string displayName, TaskStatus status, string message = "", int totalCount = 0, int finishedCount = 0, long timeMs = 0)
        {
            return new TaskStatusInfo
            {
                TaskId = taskId,
                SerialNumber = taskId,
                TaskType = string.IsNullOrWhiteSpace(taskType) ? "\u79bb\u7ebf" : taskType,
                DisplayName = displayName,
                Status = status,
                Message = message,
                TotalCount = totalCount,
                FinishedCount = finishedCount,
                ProcessingTimeMs = timeMs
            };
        }

        public override string ToString()
        {
            return $"[{GetDisplayName()}] {GetFullDisplayMessage()} ({Timestamp:HH:mm:ss})";
        }
    }
}

using System;

namespace DeepSightModel
{
    /// <summary>
    /// ����״̬��Ϣģ��
    /// </summary>
    public class TaskStatusInfo
    {
        /// <summary>
        /// ���кţ�SN��
        /// </summary>
        public string SerialNumber { get; set; }

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
                return $"{baseMessage} - {Message}";
            }
            
            return baseMessage;
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

        public override string ToString()
        {
            return $"[{SerialNumber}] {GetFullDisplayMessage()} ({Timestamp:HH:mm:ss})";
        }
    }
}

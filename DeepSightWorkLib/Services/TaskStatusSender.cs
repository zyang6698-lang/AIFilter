using DeepSightEvent;
using DeepSightModel;
using System;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 任务状态发送器 - 从DefectProcessor中的状态发送抽离
    /// </summary>
    public static class TaskStatusSender
    {
        /// <summary>
        /// 发送排队状态
        /// </summary>
        public static void SendQueued(string serialNumber, string side = "")
        {
            SystemEvent.SendTaskStatus(TaskStatusInfo.CreateQueued(serialNumber, side));
        }

        /// <summary>
        /// 发送正在读取数据状态
        /// </summary>
        public static void SendReadingData(string serialNumber, string side)
        {
            SystemEvent.SendTaskStatus(TaskStatusInfo.Create(serialNumber, TaskStatus.ReadingData, side));
        }

        /// <summary>
        /// 发送正在加载图片状态
        /// </summary>
        public static void SendLoadingImages(string serialNumber, string side)
        {
            SystemEvent.SendTaskStatus(TaskStatusInfo.Create(serialNumber, TaskStatus.LoadingImages, side));
        }

        /// <summary>
        /// 发送图片加载完成状态
        /// </summary>
        public static void SendImagesLoaded(string serialNumber, string side, int imageCount = 0)
        {
            string message = imageCount > 0 ? $"已加载{imageCount}张图片" : "";
            SystemEvent.SendTaskStatus(TaskStatusInfo.Create(serialNumber, TaskStatus.ImagesLoaded, side, message));
        }

        /// <summary>
        /// 发送开始AI检测状态
        /// </summary>
        public static void SendAIDetecting(string serialNumber, string side)
        {
            SystemEvent.SendTaskStatus(TaskStatusInfo.Create(serialNumber, TaskStatus.AIDetecting, side));
        }

        /// <summary>
        /// 发送AI检测完成状态
        /// </summary>
        public static void SendAICompleted(string serialNumber, string side, long processingTimeMs = 0)
        {
            SystemEvent.SendTaskStatus(TaskStatusInfo.Create(serialNumber, TaskStatus.AICompleted, side, "", processingTimeMs));
        }

        /// <summary>
        /// 发送正在回写结果状态
        /// </summary>
        public static void SendWritingResults(string serialNumber, string side)
        {
            SystemEvent.SendTaskStatus(TaskStatusInfo.Create(serialNumber, TaskStatus.WritingResults, side));
        }

        /// <summary>
        /// 发送完成状态
        /// </summary>
        public static void SendCompleted(string serialNumber, string side, long processingTimeMs = 0)
        {
            SystemEvent.SendTaskStatus(TaskStatusInfo.Create(serialNumber, TaskStatus.Completed, side, "", processingTimeMs));
        }

        /// <summary>
        /// 发送跳过处理状态
        /// </summary>
        public static void SendSkipped(string serialNumber, string side, string reason)
        {
            SystemEvent.SendTaskStatus(TaskStatusInfo.Create(serialNumber, TaskStatus.Skipped, side, reason));
        }

        /// <summary>
        /// 发送失败状态
        /// </summary>
        public static void SendFailed(string serialNumber, string side, string errorMessage)
        {
            SystemEvent.SendTaskStatus(TaskStatusInfo.Create(serialNumber, TaskStatus.Failed, side, errorMessage));
        }
    }
}

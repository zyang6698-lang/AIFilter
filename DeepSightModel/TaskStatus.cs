using System;
using System.Drawing;

namespace DeepSightModel
{
    /// <summary>
    /// 任务状态枚举
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// 排队中
        /// </summary>
        Queued,
        
        /// <summary>
        /// 正在读取数据
        /// </summary>
        ReadingData,
        
        /// <summary>
        /// 正在加载图片
        /// </summary>
        LoadingImages,
        
        /// <summary>
        /// 图片加载完成
        /// </summary>
        ImagesLoaded,
        
        /// <summary>
        /// 开始AI检测
        /// </summary>
        AIDetecting,
        
        /// <summary>
        /// AI检测完成
        /// </summary>
        AICompleted,
        
        /// <summary>
        /// 正在回写结果
        /// </summary>
        WritingResults,
        
        /// <summary>
        /// 已完成
        /// </summary>
        Completed,
        
        /// <summary>
        /// 跳过检测
        /// </summary>
        Skipped,
        
        /// <summary>
        /// 错误/失败
        /// </summary>
        Failed
    }

    /// <summary>
    /// 任务状态辅助类
    /// </summary>
    public static class TaskStatusHelper
    {
        /// <summary>
        /// 获取状态对应的显示颜色
        /// </summary>
        public static Color GetStatusColor(TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.Queued:
                    return Color.Yellow;
                
                case TaskStatus.ReadingData:
                case TaskStatus.LoadingImages:
                    return Color.Orange;
                
                case TaskStatus.ImagesLoaded:
                    return Color.DarkOrange;
                
                case TaskStatus.AIDetecting:
                    return Color.White;
                
                case TaskStatus.AICompleted:
                    return Color.DarkCyan;
                
                case TaskStatus.WritingResults:
                    return Color.Purple;
                
                case TaskStatus.Completed:
                    return Color.Green;
                
                case TaskStatus.Skipped:
                    return Color.Gray;
                
                case TaskStatus.Failed:
                    return Color.Red;
                
                default:
                    return Color.Gray;
            }
        }

        /// <summary>
        /// 获取状态的友好显示文本
        /// </summary>
        public static string GetStatusDisplayText(TaskStatus status, string side = "")
        {
            string sidePrefix = string.IsNullOrEmpty(side) ? "" : $"{side}面";
            
            switch (status)
            {
                case TaskStatus.Queued:
                    return "排队中";
                
                case TaskStatus.ReadingData:
                    return $"{sidePrefix}正在读取数据";
                
                case TaskStatus.LoadingImages:
                    return $"{sidePrefix}正在加载图片";
                
                case TaskStatus.ImagesLoaded:
                    return $"{sidePrefix}图片加载完成";
                
                case TaskStatus.AIDetecting:
                    return $"{sidePrefix}开始AI检测";
                
                case TaskStatus.AICompleted:
                    return $"{sidePrefix}AI检测完成";
                
                case TaskStatus.WritingResults:
                    return $"{sidePrefix}正在回写结果";
                
                case TaskStatus.Completed:
                    return $"{sidePrefix}已完成";
                
                case TaskStatus.Skipped:
                    return $"{sidePrefix}跳过检测";
                
                case TaskStatus.Failed:
                    return $"{sidePrefix}失败";
                
                default:
                    return "未知状态";
            }
        }
    }
}

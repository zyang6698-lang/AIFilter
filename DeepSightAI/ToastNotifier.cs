using DeepSightEvent;
using DeepSightModel.Alarm;
using Sunny.UI;
using System;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// 右下角弹窗通知器（基于 SunnyUI 的 UINotifier）
    /// </summary>
    public class ToastNotifier : IAlarmNotifier
    {
        public string Name => "ToastNotifier";
        public AlarmLevel MinLevel { get; set; }

        /// <summary>弹窗自动关闭时间（毫秒）</summary>
        public int DisplayMilliseconds { get; set; } = 5000;

        /// <summary>UI 线程的 Control（用于 Invoke）</summary>
        private readonly Control _uiContext;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="uiContext">主窗体或任意 UI 线程上的 Control</param>
        /// <param name="minLevel">最低触发等级（默认 Error 及以上才弹窗，Warning 不弹）</param>
        public ToastNotifier(Control uiContext, AlarmLevel minLevel = AlarmLevel.Error)
        {
            _uiContext = uiContext ?? throw new ArgumentNullException(nameof(uiContext));
            MinLevel = minLevel;
        }

        public void Notify(AlarmInfo alarm)
        {
            if (alarm == null) return;

            try
            {
                if (_uiContext.InvokeRequired)
                {
                    _uiContext.BeginInvoke(new Action(() => ShowToast(alarm)));
                }
                else
                {
                    ShowToast(alarm);
                }
            }
            catch
            {
                // UI 已关闭或异常，静默忽略
            }
        }

        private void ShowToast(AlarmInfo alarm)
        {
            string title = $"{LevelText(alarm.Level)} · {alarm.Source}";
            string description = $"[{alarm.Timestamp:HH:mm:ss}] {alarm.Message}";

            UINotifier.Show(
                description,
                ToNotifierType(alarm.Level),
                title,
                false,
                DisplayMilliseconds,
                _uiContext.FindForm(),
                null);
        }

        private static UINotifierType ToNotifierType(AlarmLevel level)
        {
            switch (level)
            {
                case AlarmLevel.Info: return UINotifierType.INFO;
                case AlarmLevel.Warning: return UINotifierType.WARNING;
                case AlarmLevel.Error: return UINotifierType.ERROR;
                case AlarmLevel.Critical: return UINotifierType.ERROR;
                default: return UINotifierType.INFO;
            }
        }

        private static string LevelText(AlarmLevel level)
        {
            switch (level)
            {
                case AlarmLevel.Info: return "提示";
                case AlarmLevel.Warning: return "警告";
                case AlarmLevel.Error: return "错误";
                case AlarmLevel.Critical: return "严重";
                default: return level.ToString();
            }
        }
    }
}

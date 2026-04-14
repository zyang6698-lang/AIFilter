using DeepSightEvent;
using DeepSightModel.Alarm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// 右下角弹窗通知器（IAlarmNotifier 实现）
    /// 管理弹窗的堆叠位置，避免重叠
    /// </summary>
    public class ToastNotifier : IAlarmNotifier
    {
        public string Name => "ToastNotifier";
        public AlarmLevel MinLevel { get; set; }

        /// <summary>弹窗自动关闭时间（毫秒）</summary>
        public int DisplayMilliseconds { get; set; } = 5000;

        /// <summary>最大同时显示的弹窗数量</summary>
        public int MaxVisibleToasts { get; set; } = 5;

        /// <summary>当前显示中的弹窗列表</summary>
        private readonly List<ToastNotificationForm> _activeToasts = new List<ToastNotificationForm>();
        private readonly object _lock = new object();

        /// <summary>UI 线程的 Control（用于 Invoke）</summary>
        private readonly Control _uiContext;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="uiContext">主窗体或任意 UI 线程上的 Control</param>
        /// <param name="minLevel">最低触发等级（默认 Warning 以上弹窗）</param>
        public ToastNotifier(Control uiContext, AlarmLevel minLevel = AlarmLevel.Warning)
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
            lock (_lock)
            {
                // 清理已关闭的弹窗
                _activeToasts.RemoveAll(t => t.IsDisposed);

                // 超过最大数量时关闭最旧的
                while (_activeToasts.Count >= MaxVisibleToasts)
                {
                    var oldest = _activeToasts[0];
                    _activeToasts.RemoveAt(0);
                    if (!oldest.IsDisposed) oldest.Close();
                }
            }

            var toast = new ToastNotificationForm(alarm, DisplayMilliseconds);
            toast.FormClosed += (s, e) =>
            {
                lock (_lock) { _activeToasts.Remove(toast); }
                RepositionToasts();
            };

            lock (_lock) { _activeToasts.Add(toast); }

            PositionAndShow(toast);
        }

        private void PositionAndShow(ToastNotificationForm toast)
        {
            var screen = Screen.FromControl(_uiContext);
            var workArea = screen.WorkingArea;

            int index;
            lock (_lock) { index = _activeToasts.IndexOf(toast); }

            int margin = 8;
            int x = workArea.Right - toast.Width - margin;
            int y = workArea.Bottom - (toast.Height + margin) * (index + 1);

            toast.Location = new Point(x, y);
            toast.Show();
        }

        /// <summary>重新排列所有活跃弹窗的位置</summary>
        private void RepositionToasts()
        {
            try
            {
                if (_uiContext.IsDisposed) return;
                if (_uiContext.InvokeRequired)
                {
                    _uiContext.BeginInvoke(new Action(DoReposition));
                }
                else
                {
                    DoReposition();
                }
            }
            catch { }
        }

        private void DoReposition()
        {
            lock (_lock)
            {
                _activeToasts.RemoveAll(t => t.IsDisposed);
                var screen = Screen.FromControl(_uiContext);
                var workArea = screen.WorkingArea;
                int margin = 8;

                for (int i = 0; i < _activeToasts.Count; i++)
                {
                    var t = _activeToasts[i];
                    if (t.IsDisposed) continue;
                    int x = workArea.Right - t.Width - margin;
                    int y = workArea.Bottom - (t.Height + margin) * (i + 1);
                    t.Location = new Point(x, y);
                }
            }
        }
    }
}

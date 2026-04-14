using DeepSightModel.Alarm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// 右下角弹窗通知窗体（无焦点、自动消失、支持堆叠）
    /// </summary>
    public class ToastNotificationForm : Form
    {
        private readonly Timer _closeTimer;
        private readonly Timer _fadeTimer;
        private readonly Label _lblLevel;
        private readonly Label _lblMessage;
        private readonly Label _lblTime;
        private double _opacity = 0.95;

        // 等级→左侧色条颜色
        private static readonly Dictionary<AlarmLevel, Color> LevelAccentColors = new Dictionary<AlarmLevel, Color>
        {
            { AlarmLevel.Info,     Color.FromArgb(24, 144, 255) },
            { AlarmLevel.Warning,  Color.FromArgb(250, 219, 20) },
            { AlarmLevel.Error,    Color.FromArgb(250, 173, 20) },
            { AlarmLevel.Critical, Color.FromArgb(255, 77, 79) },
        };

        private static readonly Dictionary<AlarmLevel, string> LevelIcons = new Dictionary<AlarmLevel, string>
        {
            { AlarmLevel.Info, "ℹ" }, { AlarmLevel.Warning, "⚠" },
            { AlarmLevel.Error, "✖" }, { AlarmLevel.Critical, "🔴" },
        };

        private readonly Color _accentColor;

        public ToastNotificationForm(AlarmInfo alarm, int displayMs = 5000)
        {
            _accentColor = LevelAccentColors.ContainsKey(alarm.Level)
                ? LevelAccentColors[alarm.Level] : Color.Gray;

            // 窗体基本设置
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Size = new Size(360, 80);
            this.BackColor = Color.FromArgb(35, 55, 70);
            this.Opacity = _opacity;
            this.Padding = new Padding(0);

            // 等级图标 + 文字
            string icon = LevelIcons.ContainsKey(alarm.Level) ? LevelIcons[alarm.Level] : "•";
            _lblLevel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = _accentColor,
                Location = new Point(12, 8),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // 时间 + 来源
            _lblTime = new Label
            {
                Text = $"{alarm.Timestamp:HH:mm:ss}  [{alarm.Source}]",
                Font = new Font("微软雅黑", 8F),
                ForeColor = Color.FromArgb(160, 180, 195),
                Location = new Point(48, 8),
                Size = new Size(300, 16),
                BackColor = Color.Transparent
            };

            // 消息内容
            string msg = alarm.Message ?? "";
            if (msg.Length > 80) msg = msg.Substring(0, 80) + "...";
            _lblMessage = new Label
            {
                Text = msg,
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.FromArgb(240, 250, 255),
                Location = new Point(48, 28),
                Size = new Size(300, 44),
                BackColor = Color.Transparent
            };

            this.Controls.Add(_lblLevel);
            this.Controls.Add(_lblTime);
            this.Controls.Add(_lblMessage);

            // 点击关闭
            this.Click += (s, e) => this.Close();
            _lblLevel.Click += (s, e) => this.Close();
            _lblTime.Click += (s, e) => this.Close();
            _lblMessage.Click += (s, e) => this.Close();

            // 自动关闭定时器
            _closeTimer = new Timer { Interval = displayMs };
            _closeTimer.Tick += (s, e) => { _closeTimer.Stop(); StartFadeOut(); };

            // 淡出定时器
            _fadeTimer = new Timer { Interval = 30 };
            _fadeTimer.Tick += FadeTimer_Tick;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // 左侧色条
            using (var brush = new SolidBrush(_accentColor))
            {
                e.Graphics.FillRectangle(brush, 0, 0, 4, this.Height);
            }
            // 底部分隔线
            using (var pen = new Pen(Color.FromArgb(60, 80, 100), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _closeTimer.Start();
        }

        // 不抢焦点
        protected override bool ShowWithoutActivation => true;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOPMOST = 0x00000008;
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOPMOST;
                return cp;
            }
        }

        private void StartFadeOut()
        {
            _fadeTimer.Start();
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            _opacity -= 0.08;
            if (_opacity <= 0)
            {
                _fadeTimer.Stop();
                this.Close();
            }
            else
            {
                this.Opacity = _opacity;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _closeTimer?.Dispose();
                _fadeTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

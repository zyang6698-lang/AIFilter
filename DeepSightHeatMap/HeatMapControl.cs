using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;


namespace DeepSightHeatMap
{
    public partial class HeatMapControl : Control
    {
        public Bitmap _backgroundImage;
        public Bitmap _heatMapOverlay;

        private List<HeatPoint> _heatPoints;
        private HeatMapRenderer _renderer;
        private Bitmap _currentHeatMap;
        private Timer _refreshTimer;
        private float _opacity = 2;//0.7f;
        public HeatMapControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            _heatPoints = new List<HeatPoint>();
            _renderer = new HeatMapRenderer();

            // 使用定时器延迟渲染，避免频繁重绘
            _refreshTimer = new Timer();
            _refreshTimer.Interval = 100;
            _refreshTimer.Tick += (s, e) =>
            {
                _refreshTimer.Stop();
                RefreshHeatMap();
            };
        }

        public float HeatMapOpacity
        {
            get => _opacity;
            set
            {
                _opacity = Math.Max(0, Math.Min(1, value));
                _renderer.SetOpacity(_opacity);
                ScheduleRefresh();
            }
        }

        public Bitmap BackgroundImage
        {
            get => _backgroundImage;
            set
            {
                _backgroundImage = value;
                ScheduleRefresh();
            }
        }
        private void RefreshHeatMap()
        {
            if (Width == 0 || Height == 0) return;

            _heatMapOverlay?.Dispose();
            _heatMapOverlay = _renderer.GenerateHeatMapOverlay(_heatPoints,BackgroundImage.Size);
           
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_backgroundImage != null)
            {
                e.Graphics.DrawImage(_backgroundImage, ClientRectangle);
            }
            else
            {
                using (var brush = new SolidBrush(BackColor))
                    e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            if (_heatMapOverlay != null)
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                e.Graphics.DrawImage(_heatMapOverlay, ClientRectangle);
            }

            // 绘制边框
            using (var pen = new Pen(Color.Gray, 1))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        public void AddHeatPoint(HeatPoint point)
        {
            _heatPoints.Add(point);
            ScheduleRefresh();
        }

        public void AddHeatPoints(IEnumerable<HeatPoint> points)
        {
            _heatPoints.AddRange(points);
            ScheduleRefresh();
        }

        public void ClearHeatPoints()
        {
            _heatPoints.Clear();
            ScheduleRefresh();
        }

        public void SetHeatPoints(List<HeatPoint> points)
        {
            _heatPoints = points ?? new List<HeatPoint>();
            ScheduleRefresh();
        }

        private void ScheduleRefresh()
        {
            RefreshHeatMap();
            //_refreshTimer.Stop();
            //_refreshTimer.Start();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ScheduleRefresh();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _currentHeatMap?.Dispose();
                _backgroundImage?.Dispose();
                _refreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.BackColor = Color.Black;
            this.ResumeLayout(false);
        }
    }
}

using DeepSightAI.SettingPages;
using DeepSightModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class DataFlowAnimation2 : UserControl
    {
        private Timer animationTimer;
        private List<FlowPath> flowPaths = new List<FlowPath>();
        private Random random = new Random();

        // Animation configuration
        private int flowSpeed = 5;
        private int dataPointCount = 1;
        private Color flowColor = Color.FromArgb(0, 120, 215); // Blue data flow

        private List<AviCtr> aviCtrs = new List<AviCtr>();

        public DataFlowAnimation2()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            InitializeAnimation();
        }

        private void InitializeAnimation()
        {
            animationTimer = new Timer();
            animationTimer.Interval = 100;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        public void CreateMachinePanels(List<WatchPathConfig> watchPaths)
        {

            for (int i = 0; i < watchPaths.Count; i++)
            {
                AddAviControl(watchPaths[i], i);
            }
            int X = aviCtrs.Count > 3? aviCtrs[3].Location.X + aviCtrs[3].Width / 2:0;
            int Y=aviCtrs.Count > 3 ? aviCtrs[3].Location.Y + aviCtrs[3].Height : 0;

            for (int i = 0; i < aviCtrs.Count; i++)
            {
                if (!aviCtrs[i].ctrConfig.IsEnable)
                {
                    continue;
                }
                int index = i;
                this.BeginInvoke((Action)(() =>
                {
                    Point startPoint = new Point(X, Y);
                    Point endPoint = aviCtrs[index].GetCenterPoint();
                    AddFlowPath(startPoint, endPoint);
                }));
            }
        }

        private void AddAviControl(WatchPathConfig watchPath, int index)
        {
            AviCtr ctr = new AviCtr(watchPath)
            {
                Size = new Size(150, 150), // Adjust size as needed
                Dock = DockStyle.Fill,
                AutoSize = true,
                Margin = new Padding(0)
            };


            aviCtrs.Add(ctr);

            int row = index / tableLayoutPanel1.ColumnCount;
            int col = index % tableLayoutPanel1.ColumnCount;

            if (col >= tableLayoutPanel1.ColumnCount)
            {
                row++;
                col = 0;
            }


            if (row < tableLayoutPanel1.RowCount)
            {
                this.tableLayoutPanel1.Controls.Add(ctr, col, row);
            }
        }


        public void AddFlowPath(Point startPoint, Point endPoint, string dataContent = "")
        {
            var path = new FlowPath
            {
                StartPoint = startPoint,
                EndPoint = endPoint,
                DataPoints = new List<DataPoint>(),
                DataContent = dataContent,
                IsActive = true
            };

            for (int i = 0; i < dataPointCount; i++)
            {
                path.DataPoints.Add(new DataPoint
                {
                    Position = startPoint,
                    Progress = 0,
                    Size = 8
                });
            }

            flowPaths.Add(path);
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            foreach (var path in flowPaths)
            {
                if (!path.IsActive) continue;

                foreach (var dataPoint in path.DataPoints)
                {
                    dataPoint.Progress += flowSpeed;

                    if (dataPoint.Progress >= 100)
                    {
                        dataPoint.Progress = 0;
                        dataPoint.Size = 8;
                    }

                    dataPoint.Position = CalculatePoint(path.StartPoint, path.EndPoint, dataPoint.Progress / 100f);
                }
            }
            Invalidate();
        }

        private Point CalculatePoint(Point start, Point end, float progress)
        {
            float curveFactor = (float)(Math.Sin(progress * Math.PI) * 20);

            return new Point(
                (int)(start.X + (end.X - start.X) * progress + curveFactor),
                (int)(start.Y + (end.Y - start.Y) * progress)
            );
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var path in flowPaths)
            {
                if (!path.IsActive) continue;

                using (var pathPen = new Pen(Color.FromArgb(50, flowColor), 7))
                {
                    g.DrawLine(pathPen, path.StartPoint, path.EndPoint);
                }

                foreach (var dataPoint in path.DataPoints)
                {
                    using (var brush = new SolidBrush(flowColor))
                    {
                        g.FillEllipse(brush,
                            dataPoint.Position.X - dataPoint.Size / 2,
                            dataPoint.Position.Y - dataPoint.Size / 2,
                            dataPoint.Size, dataPoint.Size);
                    }
                }
            }
        }

        public void StartAnimation()
        {
            animationTimer.Start();
        }

        public void StopAnimation()
        {
            animationTimer.Stop();
        }

        public void UpdateAviCtrInfo(string aviName, string productSerial, string lotId, double utilization)
        {
            foreach (var ctr in aviCtrs)
            {
                if (ctr.ctrConfig.AviName == aviName)
                {
                    ctr.ProductSerial = productSerial;
                    ctr.LotId = lotId;
                    ctr.Utilization = utilization;
                    break;
                }
            }
        }

        public void UpdateAviCtrStats(string aviName, int totalImages, int aiOkImages)
        {
            foreach (var ctr in aviCtrs)
            {
                if (ctr.ctrConfig.AviName == aviName)
                {
                    ctr.TotalImages = totalImages;
                    ctr.AiOkImages = aiOkImages;
                    break;
                }
            }
        }

        private class FlowPath
        {
            public Point StartPoint { get; set; }
            public Point EndPoint { get; set; }
            public List<DataPoint> DataPoints { get; set; }
            public string DataContent { get; set; }
            public bool IsActive { get; set; }
        }

        private class DataPoint
        {
            public Point Position { get; set; }
            public float Progress { get; set; }
            public int Size { get; set; }
        }
    }
}

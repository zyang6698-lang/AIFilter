using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;
using DeepSightAI.SettingPages;
using DeepSightModel;
using System.Linq;

namespace DeepSightAI
{
    public class DataFlowAnimation : UserControl
    {
        private System.Windows.Forms.Timer animationTimer;
        private List<FlowPath> flowPaths = new List<FlowPath>();

        // 动画配置
        private int flowSpeed = 5;
        private int dataPointCount = 1;
        private Color flowColor = Color.FromArgb(0, 120, 215); // 蓝色数据流

        private List<AviCtr> aviCtrs = new List<AviCtr>();

        public DataFlowAnimation()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.AutoScroll = true;
        }

        private void InitializeComponent()
        {
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 100;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        public void CreateMachinePanels(List<WatchPathConfig> watchPaths)
        {
            var bt = new Button
            {
                Text = "ATS_AI系统",
                Font = new Font("微软雅黑", 16, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(200, 50),
                Location = new Point(515, 10),
                FlatStyle = FlatStyle.Flat,
            };
            this.Controls.Add(bt);

            for (int i = 0; i < watchPaths.Count; i++)
            {
                AddAviControl(watchPaths[i], i);
            }

            for (int i = 0; i < aviCtrs.Count; i++)
            {
                Point startPoint = new Point(bt.Location.X + bt.Width / 2, 60); // 中央面板底部
                if (!aviCtrs[i].ctrConfig.IsEnable)
                {
                    continue;
                }
                Point endPoint = aviCtrs[i].GetCenterPoint(); // 机台顶部
                AddFlowPath(startPoint, endPoint);
            }
        }

        private void AddAviControl(WatchPathConfig watchPath, int index)
        {
            try
            {
                AviCtr ctr = new AviCtr(watchPath);
                ctr.Size = new Size(150, 150); // 根据需求调整

                // 计算位置 - 根据索引排列
                int cols = 7; // 每行显示4个
                int spacing = 0; // 间距

                int x = (index % cols) * (ctr.Width + spacing) + spacing;
                int y = (index / cols) * (ctr.Height + spacing) + spacing;

                ctr.Location = new Point(x, y + 50);
                aviCtrs.Add(ctr);
                this.Controls.Add(ctr);
            }
            catch (Exception ex)
            {
                // Consider logging the exception
            }
        }


        // 添加数据流动路径
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

            // 初始化数据点
            for (int i = 0; i < dataPointCount; i++)
            {
                path.DataPoints.Add(new DataPoint
                {
                    Position = startPoint,
                    Progress = 0,
                    Size = 8//random.Next(3, 8)
                }); ;
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
                        dataPoint.Size = 8;//random.Next(3, 8); // 随机大小
                    }

                    // 计算当前位置
                    dataPoint.Position = CalculatePoint(path.StartPoint, path.EndPoint, dataPoint.Progress / 100f);
                }
            }
            Invalidate(); // 重绘控件
        }

        private Point CalculatePoint(Point start, Point end, float progress)
        {
            // 添加一些曲线效果
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

            // Apply scrolling transformation
            g.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);

            foreach (var path in flowPaths)
            {
                if (!path.IsActive) continue;

                // 绘制路径线
                using (var pathPen = new Pen(Color.FromArgb(50, flowColor), 7))
                {
                    g.DrawLine(pathPen, path.StartPoint, path.EndPoint);
                }

                // 绘制数据点
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

                // 绘制数据内容（可选）
                if (!string.IsNullOrEmpty(path.DataContent))
                {
                    var textPoint = new Point(
                        (path.StartPoint.X + path.EndPoint.X) / 2,
                        (path.StartPoint.Y + path.EndPoint.Y) / 2 - 20
                    );

                    using (var textBrush = new SolidBrush(Color.White))
                    using (var font = new Font("Arial", 8))
                    {
                        g.DrawString(path.DataContent, font, textBrush, textPoint);
                    }
                }
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
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

        public void UpdateAllAviCtrLotSn(Func<string, (string LotNumber, string SerialNumber,string ProductSerial)> getLatestPanelInfo)
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    foreach (var ctr in aviCtrs)
                    {
                        if (ctr.ctrConfig.IsEnable)
                        {
                            var tmp = getLatestPanelInfo(ctr.ctrConfig.AviName);
                            if (tmp.LotNumber != null && tmp.SerialNumber != null)
                            {
                                ctr.LotId = tmp.LotNumber;
                                ctr.ProductSerial = tmp.SerialNumber;
                            }
                        }
                    }
                }));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer?.Stop();
                animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        // 辅助类
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
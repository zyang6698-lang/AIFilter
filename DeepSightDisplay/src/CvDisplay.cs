using OpenCvSharp;
using System;
using System.Linq;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Point = OpenCvSharp.Point;
using DeepSightTool;
using DeepSightModel;
using System.Collections.Generic;

namespace DeepSightDisplay
{
    public class CvDisplay : PictureBox
    {
        public delegate void CallBackFullShowPro(string station, int index, string m_station, string status, string ocr, Mat mat);
        public event CallBackFullShowPro OnCallBackFullShowPro;

        public delegate void CallBackRoiPro(string station, int index, Mat mat);
        public event CallBackRoiPro OnCallBackRoiPro;
        //回调返回页面点击索引
        public delegate void CallBackRoiIndexAndInfo(int index, DisPlayInfo info);
        public event CallBackRoiIndexAndInfo OnCallBackRoiIndexAndInfo;
        //单图测试
        public delegate void CallBackSingleTest(int index);
        public event CallBackSingleTest OnCallBackSingleTest;

        /// <summary>
        /// 选中还是取消
        /// </summary>
        /// <param name="index"></param>
        /// <param name="opreation">选中 true； 取消 false</param>
        public delegate void CallBackClickOpreation(int index, bool opreation);
        public event CallBackClickOpreation OnCallBackClickOpreation;

        public delegate void SelectionFinishedHandler(Rect selectionRect);
        public event SelectionFinishedHandler OnSelectionFinished;

        public DisPlayInfo info = null;
        public string OCR { get; set; }
        public string ProductId { get; set; }

        public string lable { get; set; } = "";

        public List<string> list_Paths;
        /// <summary>
        /// 是否选中模式
        /// </summary>
        public bool isSelect = false;
        /// <summary>
        /// 工站
        /// </summary>
        public string stationName { get; set; }
        /// <summary>
        /// 工站中图片的序号
        /// </summary>
        public int stationIndex { get; set; }

        #region 内部操作数据

        protected CvDisplayGraphicsMat _cdgMat; //Mat绘制类

        private readonly object _syncLock = new object();
        private bool _disposed = false;


        protected bool _isMouseMoving = false; //鼠标是否允许移动
        protected Point _mouseDownLocation; //鼠标点下的坐标

        protected System.Drawing.Point _mouseLocation; //鼠标实时位置

        protected Point _mousePixcelLocation; //鼠标放置位置的像素实际坐标

        private bool _isSelecting = false;
        private System.Drawing.Point _selectionStartPoint;
        private System.Drawing.Point _selectionEndPoint;
        private Rectangle? _persistentSelectionRect = null;

        #endregion 内部操作数据
        private bool drawModel = false;   //绘制模式下,不允许缩放和鼠标右键菜单

        private bool _isSelectionMode = false;
        public bool IsSelectionMode
        {
            get => _isSelectionMode;
            set
            {
                _isSelectionMode = value;
                // 进入或退出选择模式时，相应地设置绘制模式
                DrawModel = value;
                if (value)
                {
                    Cursor = Cursors.Cross;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        /// <summary>
        /// 绘制模式下,不允许缩放和鼠标右键菜单
        /// </summary>
        public bool DrawModel
        {
            get => drawModel;
            set
            {
                //缩放控制
                _cdgMat.drawModel = value;
                drawModel = value;
            }
        }

        #region 事件

        /// <summary>
        /// 当前像元位置变化
        /// </summary>
        public event EventHandler<PosChangedEventArgs> PositionChanged;

        #endregion 事件

        #region 公开属性

        public CvDisplayGraphicsMat GraphicsMat => _cdgMat;

        public enum AutoDisplayMode
        {
            Original,
            Fit,
            Full
        }

        /// <summary>
        /// 绘图元素集合
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CvDisplayGraphicsShapeCollection GraphicsShapes => GraphicsMat.GraphicsShapes;

        [EditorBrowsable(EditorBrowsableState.Always)]
        [CategoryAttribute("CvDisplay"), DescriptionAttribute("自动显示图片模式")]
        public AutoDisplayMode AutoDisplay
        {
            get;
            set;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [CategoryAttribute("CvDisplay"), DescriptionAttribute("OpenCv2 Mat图片数据类")]
        public new Mat Image
        {
            get => _cdgMat.Image;
            set
            {
                if (_disposed) return;
                try
                {
                    m_text = string.Empty;
                    m_ocr = string.Empty;

                    _cdgMat.Image = value;
                    ImageResize();
                    this.Invalidate(); // 触发重绘
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error("Error setting image", ex);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Image BackgroundImage
        {
            get => base.BackgroundImage;
            set => base.BackgroundImage = null;
        }

        #endregion 公开属性

        public CvDisplay()
        {
            _cdgMat = new CvDisplayGraphicsMat();
            DoubleBuffered = true;

            AutoDisplay = AutoDisplayMode.Original;
            this.ContextMenuStrip = new ContextMenuStrip();

            ContextMenuStrip.Items.Add("全屏显示", null, OnShowFullClick);
            ContextMenuStrip.Items.Add("单图测试", null, OnTestImageClick);
            //ContextMenuStrip.Items.Add("ROI", null, OnRoiClick);
            ContextMenuStrip.Items.Add("Fit image", null, OnFitImageClick);
            ContextMenuStrip.Items.Add("Original image", null, OnOriginalImageClick);
            ContextMenuStrip.Items.Add("Full image", null, OnFullImageClick);
            ContextMenuStrip.Items.Add("Save as", null, OnSaveAsClick);

            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
        }

        #region 事件处理

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            //返回缺陷小图索引，大图不需返回索引，
            //用此字段区分
            if (stationIndex != 0)
            {
                if (OnCallBackRoiIndexAndInfo != null)
                {
                    OnCallBackRoiIndexAndInfo(stationIndex, info);
                }
            }
            //如果当前模式为选择模式
            if (!string.IsNullOrWhiteSpace(lable) && isSelect)
            {
                lable = "";
                if (OnCallBackClickOpreation != null)
                {
                    OnCallBackClickOpreation(stationIndex, false);
                }
                Fit();
                return;
            }
            if (isSelect)
            {
                lable = "已选中";
            }
            if (isSelect)
            {
                if (!string.IsNullOrWhiteSpace(lable))
                {
                    if (OnCallBackClickOpreation != null)
                    {
                        OnCallBackClickOpreation(stationIndex, true);
                    }
                }
            }

            Fit();
        }

        protected virtual void OnShowFullClick(object sender, EventArgs e)
        {
            if (Image == null)
            {
                return;
            }
            if (OnCallBackFullShowPro != null)
            {
                OnCallBackFullShowPro(this.stationName, this.stationIndex, m_station, m_text, m_ocr, Image);
            }
        }
        protected virtual void OnRoiClick(object sender, EventArgs e)
        {
            if (Image == null)
            {
                return;
            }
            if (OnCallBackRoiPro != null)
            {
                OnCallBackRoiPro(this.stationName, this.stationIndex, Image);
            }
        }


        protected virtual void OnFitImageClick(object sender, EventArgs e)
        {
            Fit();
        }

        protected virtual void OnOriginalImageClick(object sender, EventArgs e)
        {
            OriginalSize();
        }

        protected virtual void OnFullImageClick(object sender, EventArgs e)
        {
            Full();
        }
        protected virtual void OnTestImageClick(object sender, EventArgs e)
        {
            //测试流程
            if (stationIndex == 0)
            {
                return;
            }
            if (Image == null)
            {
                MessageBox.Show("图像为空");
                return;
            }
            if (OnCallBackSingleTest != null)
            {
                OnCallBackSingleTest(stationIndex);
            }
        }
        protected virtual void OnSaveAsClick(object sender, EventArgs e)
        {
            if (Image == null)
            {
                return;
            }

            using (SaveFileDialog ofd = new SaveFileDialog())
            {
                ofd.Filter = "Bitmap|*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    SaveAs(ofd.FileName);
                }
            }
        }

        #endregion 事件处理

        #region 父类重载

        private bool mousePressed = false;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (IsSelectionMode)
            {
                if (e.Button == MouseButtons.Left)
                {
                    // 开始新选择时，清除上一个暂留的选框
                    if (_persistentSelectionRect.HasValue)
                    {
                        _persistentSelectionRect = null;
                        Refresh();
                    }

                    _isSelecting = true;
                    _selectionStartPoint = e.Location;
                    _selectionEndPoint = e.Location;
                }
                return;
            }
            if (drawModel)
            {
                return;
            }
            bool shapeMove = false;
            foreach (CvDisplayGraphicsShape shape in GraphicsShapes)
            {
                shape.OnMouseDown(e);
                shapeMove |= shape.Selected;
            }

            if (e.Button == MouseButtons.Left && !shapeMove)
            {
                mousePressed = true;
                Cursor = Cursors.SizeAll;
                _isMouseMoving = true;
                _mouseDownLocation = new Point(e.Location.X, e.Location.Y);
            }

            Refresh();
            base.OnMouseDown(e);
        }

        protected virtual void ImageResize()
        {
            switch (AutoDisplay)
            {
                case AutoDisplayMode.Original:
                    OriginalSize();
                    break;

                case AutoDisplayMode.Fit:
                    Fit();
                    break;

                case AutoDisplayMode.Full:
                    Full();
                    break;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            if (drawModel)
            {
                return;
            }
            if (Width != 0 && Height != 0)
            {
                ImageResize();
            }
            Refresh();
            base.OnResize(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (IsSelectionMode)
            {
                if (e.Button == MouseButtons.Left && _isSelecting)
                {
                    _isSelecting = false;
                    // 不再立即退出选择模式，而是将 IsSelectionMode 的控制权交给调用方
                    // IsSelectionMode = false; 

                    // 计算最终选框并暂存
                    int x = Math.Min(_selectionStartPoint.X, _selectionEndPoint.X);
                    int y = Math.Min(_selectionStartPoint.Y, _selectionEndPoint.Y);
                    int width = Math.Abs(_selectionStartPoint.X - _selectionEndPoint.X);
                    int height = Math.Abs(_selectionStartPoint.Y - _selectionEndPoint.Y);
                   // _persistentSelectionRect = new Rectangle(x, y, width, height);

                    Refresh();

                    // 转换坐标并触发事件
                    Point start = _cdgMat.TransformPixelPostion(_selectionStartPoint);
                    Point end = _cdgMat.TransformPixelPostion(_selectionEndPoint);

                    int rectX = Math.Min(start.X, end.X);
                    int rectY = Math.Min(start.Y, end.Y);
                    int rectWidth = Math.Abs(start.X - end.X);
                    int rectHeight = Math.Abs(start.Y - end.Y);

                    if (rectWidth > 0 && rectHeight > 0)
                    {
                        OnSelectionFinished?.Invoke(new Rect(rectX, rectY, rectWidth, rectHeight));
                    }
                }
                return;
            }
            if (drawModel)
            {
                return;
            }
            foreach (CvDisplayGraphicsShape shape in GraphicsShapes)
            {
                shape.OnMouseUp(e);
            }
            mousePressed = false;
            Cursor = Cursors.Default;
            _isMouseMoving = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (drawModel)
            {
                return;
            }
            if (e.Delta > 0)
            {
                Zoom(2, 2, new PointF(e.X, e.Y));
            }
            else
            {
                Zoom(0.5, 0.5, new PointF(e.X, e.Y));
            }
            base.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsSelectionMode)
            {
                if (_isSelecting)
                {
                    _selectionEndPoint = e.Location;
                    Refresh(); // 重绘以显示选择框
                }
                return;
            }
            //if (drawModel)
            //{
            //    return;
            //}
            if (_disposed || DrawModel) return;
            if (!mousePressed)
            {
                return;
            }

            _mouseLocation = e.Location;

            foreach (CvDisplayGraphicsShape shape in GraphicsShapes)
            {
                shape.OnMouseMove(e);
            }
            if (_isMouseMoving && Image != null)
            {
                //移动图片
                Point nowLocation = new Point(e.X, e.Y);
                Point move = (nowLocation - _mouseDownLocation);

                SyncUpdateOrigin(_cdgMat.DisplayOrigin + move);

                //Refresh();
                _mouseDownLocation = nowLocation;
            }
            else if (_cdgMat.IsMouseIn(e.Location))
            {
                //坐标在绘图区域内
                //记录实际像素点和颜色 ，提示在tooltip上
                Cursor = Cursors.Cross;
                Point p = _cdgMat.TransformPixelPostion(e.Location);
                if (!p.Equals(_mouseLocation) && !p.Equals(_mousePixcelLocation))
                {
                    string tip = string.Format("({0},{1})", p.X, p.Y);
                    object[] res = null;
                    MatHelper.GetMatChannelValues(Image, p.X, p.Y, out res);
                    tip += " [";
                    foreach (object obj in res)
                    {
                        tip += obj + ",";
                    }
                    tip = tip.Substring(0, tip.Length - 1) + ']';

                    Console.WriteLine(tip);

                    if (PositionChanged != null)
                    {
                        PositionChanged(this, new PosChangedEventArgs(p, res));
                    }
                }

                _mousePixcelLocation = p;
            }
            else
            {
                //坐标不在绘图区域内
                _mousePixcelLocation = new Point(-1, -1);
            }

            Refresh();
            base.OnMouseMove(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_disposed) return;
            base.OnPaint(e);
            Graphics gh = e.Graphics;
            gh.Clear(BackColor);

            // 绘制图像
            _cdgMat.OnPaint(e);


            //base.OnPaint(e);
            //Graphics gh = e.Graphics;
            //gh.Clear(BackColor);
            //if (Image != null)
            //{
            //    _cdgMat.OnPaint(e);
            //}
            Font smallFont = new Font("Arial", 8);
            if (!string.IsNullOrWhiteSpace(m_station))
            {
                gh.DrawString(m_station, smallFont, Brushes.Green, 1, 1);
            }

            if (!string.IsNullOrWhiteSpace(m_text))
            {
                Brush brush;
                if (m_text.Contains("NG"))
                {
                    brush = Brushes.Red;
                }
                else if (m_text.Contains("未处理"))
                {
                    brush = Brushes.Blue;
                }
                else
                {
                    brush = Brushes.Green;

                }
                gh.DrawString(m_text, smallFont, brush, 1, Font.GetHeight() + 3);
            }

            if (!string.IsNullOrWhiteSpace(m_ocr))
            {
                gh.DrawString(m_ocr, smallFont, Brushes.Green, 1, Font.GetHeight() * 2 + 3);
            }
            Font bigFont = new Font("Arial", 12, FontStyle.Bold);
            if (isSelect)
            {
                if (!string.IsNullOrWhiteSpace(lable))
                {
                    //if (OnCallBackClickOpreation!=null)
                    //{
                    //    OnCallBackClickOpreation(stationIndex,true);
                    //}
                    gh.DrawString(lable, bigFont, Brushes.Blue, 1, Font.GetHeight() * 3 + 3);
                }
            }
            if (_isSelecting || _persistentSelectionRect.HasValue)
            {
                using (Pen selectionPen = new Pen(Color.LimeGreen, 2))
                {
                    selectionPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    Rectangle rectToDraw = _isSelecting
                        ? new Rectangle(
                            Math.Min(_selectionStartPoint.X, _selectionEndPoint.X),
                            Math.Min(_selectionStartPoint.Y, _selectionEndPoint.Y),
                            Math.Abs(_selectionStartPoint.X - _selectionEndPoint.X),
                            Math.Abs(_selectionStartPoint.Y - _selectionEndPoint.Y))
                        : _persistentSelectionRect.Value;

                    gh.DrawRectangle(selectionPen, rectToDraw);
                }
            }

        }

        #endregion 父类重载

        #region 内部使用函数

        /// <summary>
        /// 同步更新所有绘图的原点
        /// </summary>
        /// <param name="p"></param>
        protected void SyncUpdateOrigin(Point2d p)
        {
            _cdgMat.DisplayOrigin = p;
        }

        private static System.Drawing.Point ConvertCvPoint2DrawingPoint(Point p)
        {
            return new System.Drawing.Point(p.X, p.Y);
        }

        private static Point ConvertDrawingPoint2CvPoint(System.Drawing.Point p)
        {
            return new Point(p.X, p.Y);
        }

        #endregion 内部使用函数

        #region 对外接口

        public string m_station = string.Empty;

        /// <summary>
        /// 在控件上显示
        /// </summary>
        /// <param name="text"></param>
        public void DrawStation(string text)
        {
            m_station = text;
            Refresh();
        }

        public void DrawSelect(bool select)
        {
            if (select)
            {
                lable = "已选中";
            }
            else
            {
                lable = "";
            }
            Refresh();
        }

        public string m_text = string.Empty;

        /// <summary>
        /// 在控件上显示
        /// </summary>
        /// <param name="text"></param>
        public void DrawStatus(string text)
        {
            m_text = text;
            Refresh();
        }

        public string m_ocr = string.Empty;
        /// <summary>
        /// 在控件上显示
        /// </summary>
        /// <param name="text"></param>
        public void DrawOCR(string text)
        {
            m_ocr = text;
            Refresh();
        }


        /// <summary>
        /// 在图上写文字
        /// </summary>
        /// <param name="text"></param>
        /// <param name="org"></param>
        /// <param name="fontScale"></param>
        /// <param name="color"></param>
        public void DrawString(string text, Point org, double fontScale, Scalar color)
        {
            _cdgMat.DrawString(text, org, fontScale, color);
            Refresh();
        }
        public void DisVisibleContextMenuStrip()
        {
            if (this.ContextMenuStrip.Items.Count > 0)
            {
                this.ContextMenuStrip.Items.Clear();
            }
        }
        /// <summary>
        /// 图片缩放
        /// </summary>
        /// <param name="scale">x,y等比例缩放参数</param>
        public void Zoom(double scale)
        {
            Zoom(scale, scale);
        }

        /// <summary>
        /// 另存为
        /// </summary>
        /// <param name="filepath"></param>
        public void SaveAs(string filepath)
        {
            if (Image == null)
            {
                return;
            }

            Cv2.ImWrite(filepath, Image);
        }

        /// <summary>
        /// 图片缩放
        /// </summary>
        /// <param name="xScale">x缩放参数</param>
        /// <param name="yScale">y缩放参数</param>
        public void Zoom(double xScale, double yScale)
        {
            Zoom(xScale, yScale, new PointF(0, 0));
        }

        /// <summary>
        /// 根据某个原点进行缩放
        /// </summary>
        /// <param name="xScale">x缩放参数</param>
        /// <param name="yScale">y缩放参数</param>
        /// <param name="zoomOrign">缩放参考点</param>
        public void Zoom(double xScale, double yScale, PointF zoomOrign)
        {
            //if (Image == null)
            //{
            //    return;
            //}
            if (_disposed || Image == null) return;
            double newXPixelSize = Math.Abs(xScale) * _cdgMat.PixelSize.Width;
            double newYPixelSize = Math.Abs(yScale) * _cdgMat.PixelSize.Height;
            if (newXPixelSize > 0 && newYPixelSize > 0)
            {
                int dispPixelX = (int)(Width / newXPixelSize),
                    dispPixelY = (int)(Height / newYPixelSize);
                if (dispPixelX < 1 || dispPixelY < 1) //最少显示一个像素点
                {
                    return;
                }

                if (_cdgMat.IsMouseIn(zoomOrign)) //如果在聚焦在图片某点放大
                {
                    //变换前 图片绘制坐标原点距离 当前鼠标鼠标的距离
                    double disX = zoomOrign.X - _cdgMat.DisplayOrigin.X,
                        disY = zoomOrign.Y - _cdgMat.DisplayOrigin.Y;

                    //缩放后的距离
                    disX *= xScale;
                    disY *= yScale;

                    //同步更新所有需要绘图的元素的原点
                    SyncUpdateOrigin(new Point2d(zoomOrign.X - disX, zoomOrign.Y - disY));
                }
                _cdgMat.PixelSize = new Size2d(newXPixelSize, newYPixelSize);
                Refresh();
            }
        }
        /// <summary>
        /// 自动放大功能
        /// </summary>
        /// <param name="xZoom">需要放大的x坐标</param>
        /// <param name="yZoom">需要放大的y坐标</param>
        public void AutoZoom(PointF pointImage)
        {
            if (Image == null)
            {
                return;
            }
            double xZoom = pointImage.X / Image.Width;
            double yZoom = pointImage.Y / Image.Height;

            //用像素点位计算控件的点位
            PointF point = new PointF();
            point.X = (float)xZoom * Width;
            point.Y = (float)yZoom * Height;
            Zoom(8, 8, point);
        }

        /// <summary>
        /// 整个图片充满控件
        /// </summary>
        public virtual void Full()
        {
            if (Image == null)
            {
                return;
            }
            //换算单个像素尺寸
            _cdgMat.PixelSize = new Size2d(Width / (double)Image.Width, Height / (double)Image.Height);

            _cdgMat.DisplayOrigin = new Point2d(0, 0);

            Refresh();
        }

        /// <summary>
        /// 自适应图片的横纵比最大化
        /// </summary>
        public virtual void Fit()
        {
            // 获取一次快照
            Mat img = _cdgMat.Image; // 返回的是克隆副本，线程安全
            if (img == null) return;

            try
            {
                Size2d newsize = new Size2d();
                double hvScale1 = Width / (double)Height;
                double hvScale2 = img.Width / (double)img.Height;

                if (hvScale1 > hvScale2)
                {
                    newsize.Height = Height;
                    newsize.Width = (img.Width * ((double)newsize.Height / img.Height));
                }
                else
                {
                    newsize.Width = Width;
                    newsize.Height = (img.Height * ((double)newsize.Width / img.Width));
                }

                _cdgMat.PixelSize = new Size2d(newsize.Width / (double)img.Width, newsize.Height / (double)img.Height);

                SyncUpdateOrigin(new Point2d((Width - _cdgMat.DispRect.Width) / 2,
                    (Height - _cdgMat.DispRect.Height) / 2));

                Refresh();
            }
            finally
            {
                img.Dispose();
            }
        }

        /// <summary>
        /// 恢复图片原始比例
        /// </summary>
        public virtual void OriginalSize()
        {
            if (Image == null)
            {
                return;
            }

            _cdgMat.PixelSize = new Size2d(1, 1);
            SyncUpdateOrigin(new Point2d(0, 0));

            Refresh();
        }

        public virtual void Clear()
        {
            Image = null;
            GraphicsShapes.Clear();
            Refresh();
        }
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _cdgMat?.Dispose();
                ContextMenuStrip?.Dispose();
            }

            base.Dispose(disposing);
            _disposed = true;
        }
        #endregion 对外接口

        private void InitializeComponent()
        {
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
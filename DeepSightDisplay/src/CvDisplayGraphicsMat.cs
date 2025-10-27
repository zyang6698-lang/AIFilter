
using DeepSightTool;
using OpenCvSharp;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CvPoint = OpenCvSharp.Point;
using CvSize = OpenCvSharp.Size;
using SdPoint = System.Drawing.Point;

namespace DeepSightDisplay
{
    /// <summary>
    /// 需要绘制的Mat对象
    /// </summary>
    public class CvDisplayGraphicsMat : CvDisplayGraphicsObject
    {
        public bool drawModel = false;
        protected Mat _Image = null;
        private readonly object _imageSyncLock = new object();
        private bool _disposed = false;

        public Mat Image1
        {
            get => _Image;
            set
            {
                //Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (true)
                        {
                            Thread.Sleep(5);
                            if (!paintFlag)
                            {
                                break;
                            }
                        }
                        drawModel = true;
                        if (_Image != null)
                        {
                            _Image.Dispose();
                            _Image = null;
                        }
                        if (value != null)
                        {
                            if (_Image == null)
                            {
                                _Image = new Mat();
                            }
                            _Image = value.Clone();
                            //value.CopyTo(_Image);
                            //_Image = new Mat(value, new Rect(0, 0, value.Width, value.Height));
                        }
                    }
                    catch (Exception ex)
                    {
                        //处理Error
                        LogTextHelper.Error("Error", ex);
                    }

                    Reset();
                    drawModel = false;
                }
                //);
            }
        }

        public Mat Image
        {
            get
            {
                lock (_imageSyncLock)
                {
                    return _Image?.Clone(); // 返回副本，避免外部修改
                }
            }
            set
            {
                Mat newImage = null;
                Mat oldImage = null;
                try
                {
                    // 在后台线程处理克隆和验证
                    if (value != null && !value.IsDisposed && value.CvPtr != IntPtr.Zero)
                    {
                        newImage = value.Clone();
                    }

                    lock (_imageSyncLock)
                    {
                        // 交换引用
                        oldImage = _Image;
                        _Image = newImage;
                        newImage = null; // 防止被下面的finally释放
                    }

                    // 触发重置和重绘
                    Reset();
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error("Error setting image", ex);
                }
                finally
                {
                    // 安全释放旧图像和临时图像
                    oldImage?.Dispose();
                    newImage?.Dispose();
                }
            }
        }

        /// <summary>
        /// 实际显示在屏幕区域内的ROI
        /// </summary>
        public Rect2d DispRect => new Rect2d(DisplayOrigin, _displaySize);

        public void DrawString(string text, OpenCvSharp.Point org, double fontScale, Scalar color)
        {
            Cv2.PutText(_Image, text, org, HersheyFonts.HersheySimplex, fontScale, color, 2, LineTypes.Link4);
        }

        /// <summary>
        /// 实际显示在屏幕可见区域内的图片ROI
        /// </summary>
        public Rect ShowMatRect { get; protected set; }

        /// <summary>
        /// 单像素在屏幕中显示的大小
        /// </summary>
        public override Size2d PixelSize
        {
            //get => _pixelSize;
            //set
            //{
            //    _pixelSize = value;
            //    if (Image == null)
            //    {
            //        _displaySize = new Size2d(0, 0);
            //    }
            //    else
            //    {
            //        _displaySize = new Size2d(
            //            Image.Width * _pixelSize.Width, Image.Height * _pixelSize.Height
            //            );
            //    }
            //}
            get => _pixelSize;
            set
            {
                _pixelSize = value;
                lock (_imageSyncLock)
                {
                    if (_Image == null || _Image.IsDisposed || _Image.CvPtr == IntPtr.Zero)
                    {
                        _displaySize = new Size2d(0, 0);
                    }
                    else
                    {
                        _displaySize = new Size2d(
                            _Image.Width * _pixelSize.Width,
                            _Image.Height * _pixelSize.Height
                        );
                    }
                }
            }
        }

        protected Size2d _displaySize;

        /// <summary>
        /// 整张图片需要显示在屏幕中的大小
        /// </summary>
        public Size2d DisplaySize => _displaySize;

        public CvDisplayGraphicsShapeCollection GraphicsShapes
        {
            get; protected set;
        }

        public CvDisplayGraphicsMat()
        {
            PixelSize = new Size2d(1, 1);

            GraphicsShapes = new CvDisplayGraphicsShapeCollection(this);
        }

        #region override

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (drawModel)
            {
                return;
            }
            base.OnMouseDown(e);
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (drawModel)
            {
                return;
            }
            base.OnMouseMove(e);
        }

        public override void Reset()
        {
            base.Reset();
            PixelSize = new Size2d(1, 1);
        }

        public override void Dispose()
        {
            //if (_Image != null)
            //{
            //    _Image.Dispose();
            //}
            //base.Dispose();
            if (_disposed) return;

            lock (_imageSyncLock)
            {
                _Image?.Dispose();
                _Image = null;
            }

            base.Dispose();
            _disposed = true;
        }

        //Mutex mutex = new Mutex();
        private bool paintFlag = false;

        public override void OnPaint(PaintEventArgs e)
        {
            //if (paintFlag)
            //{
            //    paintFlag = false;
            //    return;
            //}
            //try
            //{

            //    paintFlag = true;
            //    //mutex.WaitOne();
            //    if (drawModel)
            //    {
            //        paintFlag = false;
            //        return;
            //    }
            //    if (Image != null)
            //    {
            //        try
            //        {
            //            Rect showMatRect = new Rect(); //需要裁减的图片范围
            //            System.Drawing.PointF drawImageStartPos = new System.Drawing.PointF(); //绘制showMatRect的起始点
            //            if (DispRect.X < 0)
            //            {
            //                //显示区域的起始点X不在屏幕内
            //                showMatRect.X = (int)(Math.Abs(DispRect.X) / PixelSize.Width);
            //                drawImageStartPos.X = (float)(showMatRect.X * PixelSize.Width + DispRect.X);
            //            }
            //            else
            //            {
            //                showMatRect.X = 0;
            //                drawImageStartPos.X = (float)DispRect.X;
            //            }
            //            showMatRect.Width = (int)((e.ClipRectangle.Width - drawImageStartPos.X) / PixelSize.Width) + 1;

            //            if (DispRect.Y < 0)
            //            {
            //                //显示区域的起始点Y不在屏幕内
            //                showMatRect.Y = (int)(Math.Abs(DispRect.Y) / PixelSize.Height);
            //                drawImageStartPos.Y = (float)(showMatRect.Y * PixelSize.Height + DispRect.Y);
            //            }
            //            else
            //            {
            //                showMatRect.Y = 0;
            //                drawImageStartPos.Y = (float)DispRect.Y;
            //            }
            //            showMatRect.Height = (int)((e.ClipRectangle.Height - drawImageStartPos.Y) / PixelSize.Height) + 1;

            //            AdjustMatRect(Image, ref showMatRect);//调整需要显示Mat区域，以免截取的区域超出图片范围

            //            //Mat displayMat = new Mat(Image, showMatRect);
            //            using (Mat displayMat = new Mat(Image, showMatRect))
            //            {
            //                //计算截取区域需要显示在屏幕中的大小
            //                CvSize drawSize = new CvSize((int)(displayMat.Width * PixelSize.Width),
            //               (int)(displayMat.Height * PixelSize.Height));

            //                if (drawSize.Width < 1)
            //                {
            //                    drawSize.Width = 1;
            //                }

            //                if (drawSize.Height < 1)
            //                {
            //                    drawSize.Height = 1;
            //                }

            //                Mat resizeMat = new Mat();

            //                //以Nearest的方式缩放图片尺寸
            //                //if (drawModel)
            //                //{
            //                //    mutex.ReleaseMutex();
            //                //    return;
            //                //}
            //                Cv2.Resize(displayMat, resizeMat, drawSize, 0, 0, InterpolationFlags.Nearest);

            //                //缩放完的图片直接画在控件上
            //                System.Drawing.Image drawImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(resizeMat);
            //                e.Graphics.DrawImage(drawImage, drawImageStartPos);
            //            }
            //            //displayMat.Dispose();
            //            ShowMatRect = showMatRect;
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex.Message);
            //        }
            //    }

            //    foreach (CvDisplayGraphicsShape shape in GraphicsShapes)
            //    {
            //        shape.OnPaint(e);
            //    }
            //    //mutex.ReleaseMutex();
            //}
            //catch (Exception ex)
            //{
            //    //处理Error
            //    LogTextHelper.Error("Error", ex);
            //}
            //finally
            //{
            //    paintFlag = false;
            //}

            if (_disposed) return;

            Mat currentImage = null;
            try
            {
                // 快速获取图像副本
                lock (_imageSyncLock)
                {
                    if (_Image != null && !_Image.IsDisposed && _Image.CvPtr != IntPtr.Zero)
                    {
                        currentImage = _Image.Clone();
                    }
                }

                if (currentImage == null) return;

                // 计算显示区域
                Rect showMatRect = new Rect();
                PointF drawImageStartPos = new PointF();

                CalculateDisplayRegion(currentImage, e.ClipRectangle, ref showMatRect, ref drawImageStartPos);
                AdjustMatRect(currentImage, ref showMatRect);

                // 使用子矩阵和缩放
                using (Mat displayMat = new Mat(currentImage, showMatRect))
                using (Mat resizeMat = ResizeMatForDisplay(displayMat))
                {
                    if (resizeMat != null)
                    {
                        using (var drawImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(resizeMat))
                        {
                            e.Graphics.DrawImage(drawImage, drawImageStartPos);
                        }
                    }
                }
                ShowMatRect = showMatRect;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPaint error: {ex.Message}");
            }
            finally
            {
                currentImage?.Dispose();
            }

            // 绘制图形元素
            lock (_imageSyncLock)
            {
                foreach (CvDisplayGraphicsShape shape in GraphicsShapes)
                {
                    shape.OnPaint(e);
                }
            }
        }
        private void CalculateDisplayRegion(Mat image, Rectangle clipRect, ref Rect showMatRect, ref PointF drawStartPos)
        {
            if (DispRect.X < 0)
            {
                showMatRect.X = (int)(Math.Abs(DispRect.X) / PixelSize.Width);
                drawStartPos.X = (float)(showMatRect.X * PixelSize.Width + DispRect.X);
            }
            else
            {
                showMatRect.X = 0;
                drawStartPos.X = (float)DispRect.X;
            }

            if (DispRect.Y < 0)
            {
                showMatRect.Y = (int)(Math.Abs(DispRect.Y) / PixelSize.Height);
                drawStartPos.Y = (float)(showMatRect.Y * PixelSize.Height + DispRect.Y);
            }
            else
            {
                showMatRect.Y = 0;
                drawStartPos.Y = (float)DispRect.Y;
            }

            showMatRect.Width = Math.Max(1, (int)((clipRect.Width - drawStartPos.X) / PixelSize.Width));
            showMatRect.Height = Math.Max(1, (int)((clipRect.Height - drawStartPos.Y) / PixelSize.Height));
        }

        private Mat ResizeMatForDisplay(Mat sourceMat)
        {
            if (sourceMat == null || sourceMat.IsDisposed) return null;

            CvSize drawSize = new CvSize(
                (int)(sourceMat.Width * PixelSize.Width),
                (int)(sourceMat.Height * PixelSize.Height)
            );

            drawSize.Width = Math.Max(1, drawSize.Width);
            drawSize.Height = Math.Max(1, drawSize.Height);

            Mat resizeMat = new Mat();
            try
            {
                Cv2.Resize(sourceMat, resizeMat, drawSize, 0, 0, InterpolationFlags.Nearest);
                return resizeMat;
            }
            catch
            {
                resizeMat.Dispose();
                return null;
            }
        }

        public override bool IsMouseIn(PointF pos)
        {
            return DispRect.Contains(pos.X, pos.Y);
        }

        #endregion override

        #region public method

        /// <summary>
        /// 转换屏幕坐标为图片中的像素坐标
        /// </summary>
        /// <param name="pos">屏幕坐标</param>
        /// <returns></returns>
        public CvPoint TransformPixelPostion(SdPoint pos)
        {
            CvPoint res = new CvPoint(-1, -1);
            if (IsMouseIn(pos))
            {
                res.X = (int)((pos.X - DispRect.X) / PixelSize.Width);
                res.Y = (int)((pos.Y - DispRect.Y) / PixelSize.Height);
            }
            return res;
        }

        #endregion public method

        #region protected method

        /// <summary>
        /// 调整显示的图片区域，以免截取的mat越界
        /// </summary>
        /// <param name="mt"></param>
        /// <param name="rect"></param>
        protected void AdjustMatRect(Mat mt, ref Rect rect)
        {
            //调整XY坐标
            if (rect.X < 0)
            {
                rect.X = 0;
            }

            if (rect.X >= mt.Width)
            {
                rect.X = mt.Width - 1;
            }

            if (rect.Y < 0)
            {
                rect.Y = 0;
            }

            if (rect.Y >= mt.Height)
            {
                rect.Y = mt.Height - 1;
            }

            //调整长宽
            if (rect.Width + rect.X > mt.Width)
            {
                rect.Width = mt.Width - rect.X;
            }

            if (rect.Height + rect.Y > mt.Height)
            {
                rect.Height = mt.Height - rect.Y;
            }
        }

        #endregion protected method
    }
}
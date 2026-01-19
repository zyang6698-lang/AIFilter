using DeepSightCommunication;
using DeepSightDisplay;
using DeepSightModel;
using DeepSightTool;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 图像显示服务 - 负责图像的显示、绘制和转换
    /// </summary>
    public class ImageDisplayService
    {
        #region 私有字段

        private readonly MinioClass _minio;
        private const string DefaultBucket = "deepiresults";

        #endregion

        #region 公共属性

        /// <summary>
        /// 小图显示集合
        /// </summary>
        public List<CvDisplay> DisplaysList { get; private set; }

        /// <summary>
        /// 小图显示集合2
        /// </summary>
        public List<CvDisplay> DisplaysList2 { get; private set; }

        /// <summary>
        /// 是否显示检测框
        /// </summary>
        public bool IsShowBox { get; set; } = false;

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建图像显示服务实例
        /// </summary>
        /// <param name="minio">Minio服务实例</param>
        public ImageDisplayService(MinioClass minio)
        {
            _minio = minio ?? throw new ArgumentNullException(nameof(minio));
        }

        #endregion

        #region 显示窗口设置

        /// <summary>
        /// 设置显示窗口列表
        /// </summary>
        public void SetDisplayList(List<CvDisplay> displaysList)
        {
            DisplaysList = SetDisplayListInternal(displaysList, DisplaysList);
        }

        /// <summary>
        /// 设置显示窗口列表2
        /// </summary>
        public void SetDisplayList2(List<CvDisplay> displaysList)
        {
            DisplaysList2 = SetDisplayListInternal(displaysList, DisplaysList2);
        }

        /// <summary>
        /// 通用设置显示列表方法
        /// </summary>
        private List<CvDisplay> SetDisplayListInternal(List<CvDisplay> source, List<CvDisplay> target)
        {
            try
            {
                if (target == null)
                {
                    target = new List<CvDisplay>();
                }
                target.Clear();
                target.AddRange(source);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("设置显示列表异常", ex);
            }
            return target;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 将 AI 结果代码转换为显示文本
        /// </summary>
        /// <param name="resultCode">结果代码字符串 (0=OK, 1=NG, 2=ByPass)</param>
        /// <returns>转换后的文本</returns>
        public static string ConvertResultCodeToText(string resultCode)
        {
            if (string.IsNullOrEmpty(resultCode)) return resultCode;
            if (int.TryParse(resultCode, out int code))
            {
                switch (code)
                {
                    case 0: return "OK";
                    case 1: return "NG";
                    case 2: return "ByPass";
                    default: return resultCode;
                }
            }
            return resultCode;
        }

        #endregion

        #region 图像显示方法

        /// <summary>
        /// 显示图片（单图）
        /// </summary>
        /// <param name="path">图片路径（格式：endpoint:objectKey）</param>
        /// <param name="index">显示窗口索引</param>
        /// <param name="result">AI结果代码</param>
        /// <param name="box">检测框信息</param>
        public void ShowImage(string path, int index, string result = "")
        {
            Task.Run(() =>
            {
                Mat mt = null;
                try
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        DisplaysList[index].Image = null;
                        DisplaysList[index].Clear();
                        return;
                    }
                    string[] str = path.Split(':').ToArray();
                    using (var stream = _minio.GetImageStreamSync(DefaultBucket, str[0], str[1]))
                    {
                        if (stream.Length == 0)
                        {
                            Console.WriteLine("图片数据为空");
                            return;
                        }
                        mt = Cv2.ImDecode(stream.ToArray(), ImreadModes.Color);

                        DisplaysList[index].Image = mt;
                        mt = null; // 所有权已转移
                        string displayText = ConvertResultCodeToText(result);
                        DisplaysList[index].DrawStatus(string.IsNullOrEmpty(displayText) ? "" : $"AI结果:{displayText}");
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error("异常(可能未找到Minio路径图像),Index为" + index.ToString() + "\n" + ex.ToString());
                }
                finally
                {
                    mt?.Dispose();
                }
            });
        }



        #endregion

        #region 图像绘制方法

        /// <summary>
        /// 在图片上绘制多行文字
        /// </summary>
        public void PutTextAll(ref Mat mat, System.Drawing.Point[] location,
            Color color, float fontSize = 8, string familyName = "宋体")
        {
            try
            {
                using (Bitmap bit = mat.ToBitmap())
                {
                    using (Image tempImg = (Image)bit)
                    {
                        var tempMat = ToMat(tempImg);
                        tempMat.CopyTo(mat);
                        tempMat.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("PutTextAll Error", ex);
            }
        }

        /// <summary>
        /// 在图片上绘制单行文字
        /// </summary>
        public void DrawString(Image image, string content, System.Drawing.Point location,
            Color color, float fontSize = 10, string familyName = "宋体")
        {
            try
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    Font font = new Font(familyName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                    g.DrawString(content, font, new SolidBrush(color), location);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
            }
        }

        #endregion

        #region 图像转换方法

        /// <summary>
        /// Image 转 Mat
        /// </summary>
        public Mat ToMat(Image image)
        {
            try
            {
                return image == null ? null : Cv2.ImDecode(ToBinary(image), ImreadModes.Color);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                return new Mat();
            }
        }

        /// <summary>
        /// Image 转字节数组
        /// </summary>
        public byte[] ToBinary(Image image)
        {
            try
            {
                if (image == null)
                    return new byte[0];
                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Bmp);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                return new MemoryStream().ToArray();
            }
        }

        #endregion
    }
}

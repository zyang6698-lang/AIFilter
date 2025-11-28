using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeepSightWorkLib
{

    /// <summary>
    /// 算法检测类
    /// </summary>
    public class DefectClass
    {
        public AI_DefectClass ai_Defect = null;
        public DefectClass()
        {
            ai_Defect = new AI_DefectClass();
        }

        /// <summary>
        /// 原有推理方法（C++端从Minio读取图片）
        /// </summary>
        public void DefectMethod(RootVBInfo info, out string vb_outStr)
        {
            try
            {
                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                IntPtr input = Marshal.StringToHGlobalAnsi(JsonConvert.SerializeObject(info, Formatting.None, jsonSetting));
                IntPtr result = IntPtr.Zero;
                ai_Defect.Vision_runMethod(input, out result);
                vb_outStr = Marshal.PtrToStringAnsi(result);
            }
            catch (Exception ex)
            {
                vb_outStr = "";
                SystemEvent.SendAlarmMsg($"VB算法调用异常:{ex.ToString()}");
            }
        }

        /// <summary>
        /// 带图片数据的推理方法（直接传递图片指针给C++，避免C++读取Minio）
        /// </summary>
        /// <param name="info">推理参数信息</param>
        /// <param name="mats">图片数据列表（已解码的Mat）</param>
        /// <param name="vb_outStr">推理结果输出</param>
        public void DefectMethodWithImages(RootVBInfo info, List<Mat> mats, out string vb_outStr)
        {
            try
            {
                // 检查 mats 是否为空
                if (mats == null || mats.Count == 0)
                {
                    vb_outStr = "";
                    LogTextHelper.Info("DefectMethodWithImages: mats为空或没有图片数据");
                }


                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                string jsonStr = JsonConvert.SerializeObject(info, Formatting.None, jsonSetting);

                LogTextHelper.Info($"DefectMethodWithImages: 准备调用推理，图片数量={mats.Count}");

                // 使用 BatchImageData 管理图片数据的非托管内存
                using (var batchData = ImageHelper.CreateBatchImageData(mats))
                {
                    LogTextHelper.Info($"DefectMethodWithImages: BatchImageData创建完成，ImagesPtr={batchData.ImagesPtr}, Count={batchData.Count}");

                    IntPtr result = IntPtr.Zero;
                    int ret = ai_Defect.InferenceWithImages(jsonStr, batchData.ImagesPtr, batchData.Count, out result);

                    LogTextHelper.Info($"DefectMethodWithImages: 推理返回，ret={ret}, result={result}");

                    vb_outStr = Marshal.PtrToStringAnsi(result);

                }
            }
            catch (Exception ex)
            {
                vb_outStr = "";
                SystemEvent.SendAlarmMsg($"VB算法调用异常(WithImages):{ex.ToString()}");
            }
        }
    }

    //C++接口实现
    public class AI_DefectClass
    {
        public static IntPtr handler = IntPtr.Zero;

        private const string strName = @"ProxyServer.dll";

        #region 原有接口
        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr create_basehandler();

        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int vision_init(int p);

        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int vision_run();

        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int basehandler_handle_message(IntPtr handler, IntPtr input, out IntPtr output);

        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int basehandler_init(IntPtr handler);

        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void vision_show_view(int isShow);
        #endregion

        #region 新增接口 - 直接传递图片数据
        /// <summary>
        /// 带图片数据的推理接口（C++端需要实现此接口）
        /// ImageInfo 结构: { IntPtr data, int width, int height, int channels, int step }
        /// </summary>
        /// <param name="handler">句柄</param>
        /// <param name="json_input">JSON参数字符串</param>
        /// <param name="images">ImageDataInfo 结构体数组指针</param>
        /// <param name="image_count">图片数量</param>
        /// <param name="output">输出结果字符串指针</param>
        /// <returns>0=成功，其他=失败</returns>
        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int basehandler_handle_message_with_images(
            IntPtr handler,
            [MarshalAs(UnmanagedType.LPStr)] string json_input,
            IntPtr images,
            int image_count,
            out IntPtr output);
        #endregion

        public AI_DefectClass()
        {
            try
            {
                handler = create_basehandler();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Vision_Show_View(int isShow)
        {
            vision_show_view(isShow);
        }

        /// <summary>
        /// 原有推理方法（通过JSON传递Minio路径，C++端读取图片）
        /// </summary>
        public int Vision_runMethod(IntPtr input, out IntPtr output)
        {
            return basehandler_handle_message(handler, input, out output);
        }

        /// <summary>
        /// 带图片数据的推理方法（直接传递图片指针）
        /// </summary>
        /// <param name="jsonInput">JSON参数</param>
        /// <param name="imagesPtr">ImageDataInfo数组指针</param>
        /// <param name="imageCount">图片数量</param>
        /// <param name="output">输出结果</param>
        /// <returns>0=成功</returns>
        public int InferenceWithImages(string jsonInput, IntPtr imagesPtr, int imageCount, out IntPtr output)
        {
            return basehandler_handle_message_with_images(handler, jsonInput, imagesPtr, imageCount, out output);
        }
    }
}

using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using DeepSightWorkLib.Interfaces;
using DeepSightWorkLib.Services;
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
    public class DefectClass : IDefectService
    {
        private AI_DefectClass _aiDefect = null;
        private string _initError = null;

        /// <summary>
        /// 获取底层 AI 检测类实例
        /// </summary>
        public AI_DefectClass AiDefect => _aiDefect;

        /// <summary>
        /// AI 引擎是否初始化成功（ProxyServer.dll 加载成功）
        /// </summary>
        public bool IsInitialized => _aiDefect != null;

        /// <summary>
        /// 初始化失败时的错误信息（成功时为 null）
        /// </summary>
        public string InitError => _initError;

        public DefectClass()
        {
            try
            {
                _aiDefect = new AI_DefectClass();
            }
            catch (Exception ex)
            {
                _initError = ex.Message;
                LogTextHelper.Warn($"DefectClass 初始化失败，AI 推理功能不可用（通常是 ProxyServer.dll 缺失或加载失败）：{ex.Message}");
                // 此处不直接 RaiseAlarm，因为 Notifier 尚未注册，Toast 无法弹出；
                // 统一由 Machine.Init 在注册 Notifier 后根据 InitError 补报
                // 不重新抛出异常，允许软件在没有 AI 引擎的情况下正常启动
            }
        }

        /// <summary>
        /// 原有推理方法（C++端从Minio读取图片）
        /// </summary>
        public void DefectMethod(RootVBInfo info, out string vb_outStr)
        {
            if (_aiDefect == null)
            {
                vb_outStr = "";
                LogTextHelper.Warn("DefectMethod: AI 引擎未初始化，跳过推理");
                return;
            }
            try
            {
                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                IntPtr input = Marshal.StringToHGlobalAnsi(JsonConvert.SerializeObject(info, Formatting.None, jsonSetting));
                IntPtr result = IntPtr.Zero;
                _aiDefect.Vision_runMethod(input, out result);
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
            if (_aiDefect == null)
            {
                vb_outStr = "";
                LogTextHelper.Warn("DefectMethodWithImages: AI 引擎未初始化，跳过推理");
                return;
            }
            try
            {
                // 检查 mats 是否为空
                if (mats == null || mats.Count == 0)
                {
                    vb_outStr = "";
                    LogTextHelper.Info("DefectMethodWithImages: mats为空或没有图片数据");
                    return;
                }

                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                string jsonStr = JsonConvert.SerializeObject(info, Formatting.None, jsonSetting);

                LogTextHelper.Info($"DefectMethodWithImages: 准备调用推理，图片数量={mats.Count}");

                // 使用 BatchImageData 管理图片数据的非托管内存
                using (var batchData = ImageHelper.CreateBatchImageData(mats))
                {
                    LogTextHelper.Info($"DefectMethodWithImages: BatchImageData创建完成，ImagesPtr={batchData.ImagesPtr}, Count={batchData.Count}");

                    IntPtr result = IntPtr.Zero;
                    int ret = _aiDefect.InferenceWithImages(jsonStr, batchData.ImagesPtr, batchData.Count, out result);

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

        /// <summary>
        /// 带图片数据的推理方法（原图+模板图，直接传递图片指针给C++，避免C++读取Minio）
        /// </summary>
        /// <param name="info">推理参数信息</param>
        /// <param name="mats">原图数据列表（已解码的Mat）</param>
        /// <param name="mats_Tmp">模板图数据列表（已解码的Mat）</param>
        /// <param name="vb_outStr">推理结果输出</param>
        public void DefectMethodWithImages2(RootVBInfo info, List<Mat> mats, List<Mat> mats_Tmp, out string vb_outStr)
        {
            if (_aiDefect == null)
            {
                vb_outStr = "";
                LogTextHelper.Warn("DefectMethodWithImages2: AI 引擎未初始化，跳过推理");
                return;
            }
            try
            {
                // 检查 mats 是否为空
                if (mats == null || mats.Count == 0)
                {
                    vb_outStr = "";
                    LogTextHelper.Info("DefectMethodWithImages2: mats为空或没有图片数据");
                    return;
                }

                // 检查 mats_Tmp 是否为空
                if (mats_Tmp == null || mats_Tmp.Count == 0)
                {
                    vb_outStr = "";
                    LogTextHelper.Info("DefectMethodWithImages2: mats_Tmp为空或没有模板图数据");
                    return;
                }

                // 检查是否有已释放的 Mat 对象（防御性检查）
                int disposedCount = mats.Count(m => m == null || m.IsDisposed);
                int disposedTmpCount = mats_Tmp.Count(m => m == null || m.IsDisposed);
                if (disposedCount > 0 || disposedTmpCount > 0)
                {
                    LogTextHelper.Warn($"DefectMethodWithImages2: 检测到已释放的Mat对象! mats中{disposedCount}/{mats.Count}个, mats_Tmp中{disposedTmpCount}/{mats_Tmp.Count}个");
                }

                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                string jsonStr = JsonConvert.SerializeObject(info, Formatting.None, jsonSetting);

                LogTextHelper.Info($"DefectMethodWithImages2: 准备调用推理，原图数量={mats.Count}，模板图数量={mats_Tmp.Count}");

                // 使用 BatchImageData 管理图片数据的非托管内存
                using (var batchData = ImageHelper.CreateBatchImageData(mats))
                {
                    using (var batchData_Tmp = ImageHelper.CreateBatchImageData(mats_Tmp))
                    {
                        LogTextHelper.Info($"DefectMethodWithImages2: BatchImageData创建完成，ImagesPtr={batchData.ImagesPtr}, Count={batchData.Count}");

                        IntPtr result = IntPtr.Zero;
                        int ret = _aiDefect.InferenceWithImages2(jsonStr, batchData.ImagesPtr, batchData_Tmp.ImagesPtr, batchData.Count, out result);

                        LogTextHelper.Info($"DefectMethodWithImages2: 推理返回，ret={ret}, result={result}");

                        vb_outStr = Marshal.PtrToStringAnsi(result);
                    }
                } 
            }
            catch (Exception ex)
            {
                vb_outStr = "";
                SystemEvent.SendAlarmMsg($"VB算法调用异常(WithImages2):{ex.ToString()}");
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

        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int basehandler_handle_message_with_images_template(
    IntPtr handler,
    [MarshalAs(UnmanagedType.LPStr)] string json_input,
    IntPtr images,IntPtr images_temp,
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
            try
            {
                vision_show_view(isShow);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"Vision_Show_View 调用失败: {ex.Message}");
                AiEngineAlarm.ReportShowViewFailed(ex);
                throw;
            }
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
        /// <summary>
        /// 原图+模板图推理
        /// </summary>
        /// <param name="jsonInput"></param>
        /// <param name="imagesPtr"></param>
        /// <param name="imagesPtr_Temp"></param>
        /// <param name="imageCount"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public int InferenceWithImages2(string jsonInput, IntPtr imagesPtr,IntPtr imagesPtr_Temp, int imageCount, out IntPtr output)
        {
            return basehandler_handle_message_with_images_template(handler, jsonInput, imagesPtr, imagesPtr_Temp, imageCount, out output);
        }
    }
}

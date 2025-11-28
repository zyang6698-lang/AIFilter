using DeepSightModel;
using DeepSightTool;
using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace DeepSightWorkLib
{
    /// <summary>
    /// 图片数据转换辅助类
    /// 用于将 OpenCvSharp.Mat 转换为可传递给 C++ 的指针格式
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// 将单个 Mat 转换为 ImageDataInfo（直接使用 Mat 内部数据指针，零拷贝）
        /// 注意：调用方必须确保 Mat 在 C++ 使用期间不被释放！
        /// 注意：如果 Mat 不连续，会原地调用 Clone() 使其连续，调用方需保持原 Mat 引用
        /// </summary>
        /// <param name="mat">OpenCV Mat 对象（如果不连续会被替换为连续副本）</param>
        /// <returns>图片数据信息结构</returns>
        public static ImageDataInfo MatToImageDataInfo(Mat mat)
        {
            if (mat == null || mat.IsDisposed || mat.Empty())
            {
                LogTextHelper.Warn($"MatToImageDataInfo: Mat无效 - null={mat == null}, disposed={mat?.IsDisposed}, empty={mat?.Empty()}");
                return new ImageDataInfo
                {
                    Data = IntPtr.Zero,
                    Width = 0,
                    Height = 0,
                    Channels = 0,
                    Step = 0,
                };
            }

            // 如果 Mat 不连续，直接使用原 Mat 的数据（大多数情况下 ImDecode 出来的 Mat 是连续的）
            // 不再克隆，避免克隆的 Mat 被 GC 回收导致指针失效
            if (!mat.IsContinuous())
            {
                LogTextHelper.Warn($"MatToImageDataInfo: Mat数据不连续，可能导致C++端读取异常, Size={mat.Cols}x{mat.Rows}");
            }
           // LogTextHelper.Info($"MatToImageDataInfo: Width={mat.Cols}, Height={mat.Rows}, Channels={mat.Channels()}, Step={mat.Step()}, StepAsInt={(int)mat.Step()}");
            return new ImageDataInfo
            {
                Data = mat.Data,          // 直接获取数据指针
                Width = mat.Cols,
                Height = mat.Rows,
                Channels = mat.Channels(),
                Step=(int)mat.Step(),
            };
        }

        /// <summary>
        /// 将 Mat 列表转换为 ImageDataInfo 数组
        /// </summary>
        /// <param name="mats">Mat 列表</param>
        /// <returns>图片数据信息数组</returns>
        public static ImageDataInfo[] MatsToImageDataInfoArray(List<Mat> mats)
        {
            if (mats == null || mats.Count == 0)
            {
                return Array.Empty<ImageDataInfo>();
            }

            var result = new ImageDataInfo[mats.Count];
            for (int i = 0; i < mats.Count; i++)
            {
                result[i] = MatToImageDataInfo(mats[i]);
            }
            return result;
        }

        /// <summary>
        /// 创建批量图片数据（用于传递给 C++ 接口）
        /// 使用示例:
        /// using (var batchData = ImageHelper.CreateBatchImageData(mats))
        /// {
        ///     // 调用 C++ 接口
        ///     ai_Defect.InferenceWithImages(jsonInput, batchData.ImagesPtr, batchData.Count, out result);
        /// }
        /// </summary>
        /// <param name="mats">Mat 列表</param>
        /// <returns>批量图片数据对象（实现了 IDisposable）</returns>
        public static BatchImageData CreateBatchImageData(List<Mat> mats)
        {
            var imageInfos = MatsToImageDataInfoArray(mats);
            return new BatchImageData(imageInfos);
        }
    }
}


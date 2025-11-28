using System;
using System.Runtime.InteropServices;

namespace DeepSightModel
{
    /// <summary>
    /// 图片数据信息结构（用于与C++接口交互）
    /// 与C++端 ImageInfo 结构体对应
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ImageDataInfo
    {
        /// <summary>
        /// 图片数据指针（指向像素数据）
        /// </summary>
        public IntPtr Data;

        /// <summary>
        /// 图片宽度
        /// </summary>
        public int Width;

        /// <summary>
        /// 图片高度
        /// </summary>
        public int Height;

        /// <summary>
        /// 通道数 (1=灰度, 3=BGR, 4=BGRA)
        /// </summary>
        public int Channels;

        public int Step;

    }

    /// <summary>
    /// 批量图片数据信息（用于传递多张图片）
    /// </summary>
    public class BatchImageData : IDisposable
    {
        /// <summary>
        /// 图片信息数组的非托管内存指针
        /// </summary>
        public IntPtr ImagesPtr { get; private set; }

        /// <summary>
        /// 图片数量
        /// </summary>
        public int Count { get; private set; }

        private ImageDataInfo[] _imageInfos;
        private bool _disposed = false;

        public BatchImageData(ImageDataInfo[] imageInfos)
        {
            _imageInfos = imageInfos ?? throw new ArgumentNullException(nameof(imageInfos));
            Count = imageInfos.Length;

            if (Count > 0)
            {
                // 分配非托管内存存储 ImageDataInfo 数组
                int structSize = Marshal.SizeOf<ImageDataInfo>();
                ImagesPtr = Marshal.AllocHGlobal(structSize * Count);

                // 将结构体数组复制到非托管内存
                for (int i = 0; i < Count; i++)
                {
                    IntPtr ptr = IntPtr.Add(ImagesPtr, i * structSize);
                    Marshal.StructureToPtr(imageInfos[i], ptr, false);
                }
            }
            else
            {
                ImagesPtr = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (ImagesPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ImagesPtr);
                    ImagesPtr = IntPtr.Zero;
                }
                _disposed = true;
            }
        }

        ~BatchImageData()
        {
            Dispose(false);
        }
    }
}


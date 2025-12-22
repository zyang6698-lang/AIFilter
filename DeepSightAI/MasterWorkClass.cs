using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DeepSightTool;
using DeepSightWorkLib;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeepSightAI
{
    public class MasterWorkClass : IDisposable
    {
        public BusinessClass workClass = null;
        private readonly MinioClient _minioClient;
        public MasterWorkClass()
        {
            workClass = new BusinessClass();
           
        }
        public void InitWork()
        {
            workClass.InitWork($"{Machine.sysConfig.ServerIP}:{Machine.sysConfig.ServerPort}");
        }
        public async Task<MemoryStream> ReadImageToMemoryAsync(string bucketName, string objectName)
        {
            try
            {
                var memoryStream = new MemoryStream();
                var args = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(async (inputStream) =>
                    {
                        await inputStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                    });
                await _minioClient.GetObjectAsync(args);
                return memoryStream;
            }
            catch (MinioException ex)
            {
                Console.WriteLine($"MinIO 错误: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"未知错误: {ex.Message}");
                return null;
            }
        }

        #region 释放
        private bool disposed = false;

        /// <summary>
        /// 析构函数
        /// </summary>
        ~MasterWorkClass()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    workClass.Dispose();
                }
                disposed = true;
            }
        }
        #endregion 释放
    }
}

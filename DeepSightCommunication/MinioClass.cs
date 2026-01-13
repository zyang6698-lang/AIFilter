using DeepSightCommunication.Interfaces;
using DeepSightTool;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeepSightCommunication
{
    /// <summary>
    /// Minio 对象存储服务实现类
    /// </summary>
    public class MinioClass : IMinioService
    {
        public static ConcurrentDictionary<string, MinioClient> dic_Minio = new ConcurrentDictionary<string, MinioClient>();
        public MinioClient _minioClient;
        private bool _overwriteExisting = true;
        public void BuildClient(string minio_ip, string minio_port)
        {
            if (!dic_Minio.ContainsKey(minio_ip))
            {
                var endpoint = $"{minio_ip}:{minio_port}";
                var accessKey = "deepiobjectdata";
                var secretKey = "deepiobject2019";
                _minioClient = (MinioClient)new MinioClient().WithEndpoint(endpoint).WithCredentials(accessKey, secretKey)
               //.WithSSL()
               .Build();
                dic_Minio.TryAdd(minio_ip, _minioClient);
                LogTextHelper.Info("创建Minio_Ip:Minio_Port完成" + endpoint);
            }
        }

        public string ReadJsonSync(string bucketName, string objectName, string ip)
        {
            try
            {
                MemoryStream memoryStream = Task.Run(() => GetImageStreamInternalAsync(bucketName, objectName, ip)).GetAwaiter().GetResult();
                using (var reader = new StreamReader(memoryStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (AggregateException ex) when (ex.InnerException is MinioException)
            {
                throw new Exception($"MinIO 错误: {ex.InnerException.Message}");
            }
            catch (JsonException ex)
            {
                throw new Exception($"JSON 解析失败: {ex.Message}");
            }
        }

        public MemoryStream GetImageStreamSync(string bucketName, string objectName, string ip)
        {
            try
            {
                return Task.Run(() => GetImageStreamInternalAsync(bucketName, objectName, ip)).GetAwaiter().GetResult();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is MinioException minioEx)
                {
                    throw new Exception($"MinIO 错误: {minioEx.Message}");
                }
                throw new Exception($"操作失败: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// 内部异步实现
        /// </summary>
        private async Task<MemoryStream> GetImageStreamInternalAsync(string bucketName, string objectName, string ip)
        {
            var memoryStream = new MemoryStream();
            try
            {
                var args = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    //.WithCallbackStream(async (inputStream) =>
                    .WithCallbackStream(inputStream =>
                    {
                        //await inputStream.CopyToAsync(memoryStream);
                        inputStream.CopyTo(memoryStream);
                        memoryStream.Position = 0; // 重置流位置
                    });

                if (dic_Minio.TryGetValue(ip, out _minioClient))
                {
                    await _minioClient.GetObjectAsync(args);
                    return memoryStream;
                }

                return new MemoryStream();
            }
            catch
            {
                memoryStream.Dispose();
                throw;
            }
        }
        /// <summary>
        /// 批量文件下载
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="fileName"></param>
        /// <param name="ip"></param>
        public void Download(string bucketName, string objectName, string fileName, string ip, bool isSigle = false)
        {
            try
            {
                if (isSigle)
                {
                    Task.Run(async () => await DownloadSingleAsync(bucketName, objectName, fileName, ip, true)).GetAwaiter().GetResult();
                }
                else
                {
                    Task.Run(async () => await DownloadFolderAsync(bucketName, objectName, fileName, ip,true)).GetAwaiter().GetResult();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        //单个文件
        public async Task DownloadSingleAsync(string bucket, string minioFolderPath, string localRootPath, string ip, bool recursive = true)
        {
            if (!minioFolderPath.EndsWith("/"))
                throw new ArgumentException("MinIO文件夹路径必须以'/'结尾");
            minioFolderPath = minioFolderPath.TrimEnd('/');
            Directory.CreateDirectory(localRootPath);
            if (dic_Minio.TryGetValue(ip, out _minioClient))
            {
                try
                {
                    List<string> objects = new List<string>();
                    objects.Add($"{minioFolderPath}");
                    var tasks = new List<Task>();
                    foreach (var obj in objects)
                    {
                        tasks.Add(ProcessObjectAsync(bucket, obj, $"{minioFolderPath.Split('/')[0]}/", localRootPath));
                        if (tasks.Count >= 1)
                        {
                            await Task.WhenAll(tasks);
                            tasks.Clear();
                        }
                    }

                    await Task.WhenAll(tasks);
                }
                catch (MinioException ex)
                {
                    HandleMinioError(ex);
                }
            }
        }
        //文件夹
        public async Task DownloadFolderAsync(string bucket, string minioFolderPath, string localRootPath, string ip, bool recursive = true)
        {
            if (!minioFolderPath.EndsWith("/"))
                throw new ArgumentException("MinIO文件夹路径必须以'/'结尾");

            Directory.CreateDirectory(localRootPath);
            if (dic_Minio.TryGetValue(ip, out _minioClient))
            {
                try
                {
                    var objects = await ListObjectsAsync(bucket, minioFolderPath, recursive);
                    var tasks = new List<Task>();
                    foreach (var obj in objects)
                    {
                        tasks.Add(ProcessObjectAsync(bucket, obj, minioFolderPath, localRootPath,true));
                        if (tasks.Count >= 15)
                        {
                            await Task.WhenAll(tasks);
                            tasks.Clear();
                        }
                    }
                    await Task.WhenAll(tasks);
                }
                catch (MinioException ex)
                {
                    HandleMinioError(ex);
                }
            }
        }


        private async Task<List<string>> ListObjectsAsync(string bucket, string prefix, bool recursive)
        {
            var tcs = new TaskCompletionSource<List<string>>();
            var objects = new List<string>();

            var observable = _minioClient.ListObjectsAsync(
                new ListObjectsArgs()
                    .WithBucket(bucket)
                    .WithPrefix(prefix)
                    .WithRecursive(recursive)
            );

            var subscription = observable.Subscribe(
                onNext: item =>
                {
                    if (!item.IsDir) objects.Add(item.Key);
                },
                onError: ex => tcs.TrySetException(ex),
                onCompleted: () => tcs.TrySetResult(objects)
            );

            try
            {
                return await tcs.Task;
            }
            finally
            {
                subscription.Dispose();
            }
        }

        /// <summary>
        /// 处理单个文件下载
        /// </summary>
        private async Task ProcessObjectAsync(string bucket, string objectKey, string minioFolderPath, string localRootPath,bool relative=false)
        {
            try
            {
                //var relativePath1 = objectKey.Substring(minioFolderPath.Length);
                var relativePath = objectKey.Substring(minioFolderPath.Length).Replace('/', Path.DirectorySeparatorChar);
                relativePath = relativePath.Replace('\\', '/');
                string[] parts = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Count()>0)
                {
                    if (!relative)
                    {
                        relativePath = parts[parts.Count() - 1];
                    }
                }
                var localFullPath = Path.Combine(localRootPath, relativePath);
                localFullPath = $"{localRootPath}/{relativePath}";
                var localDir = Path.GetDirectoryName(localFullPath);

                if (!Directory.Exists(localDir))
                    Directory.CreateDirectory(localDir);

                if (File.Exists(localFullPath) && !_overwriteExisting)
                {
                    Console.WriteLine($"跳过已存在文件: {localFullPath}");
                    return;
                }

                using (var fileStream = new FileStream(localFullPath, FileMode.Create, FileAccess.Write))
                {
                    var args = new GetObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(objectKey)
                    .WithCallbackStream(async (stream, cancellationToken) =>
                    {
                        await stream.CopyToAsync(fileStream, 81920, cancellationToken);
                        await fileStream.FlushAsync(cancellationToken);
                    });
                    await _minioClient.GetObjectAsync(args);
                }

                Console.WriteLine($"成功下载: {localFullPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下载失败 [{objectKey}]: {ex.Message}");
            }
        }

        /// <summary>
        /// 错误处理
        /// </summary>
        private void HandleMinioError(MinioException ex)
        {
            switch (ex.ServerResponse?.StatusCode)
            {
                case System.Net.HttpStatusCode.NotFound:
                    throw new FileNotFoundException("目标路径不存在", ex);
                case System.Net.HttpStatusCode.Forbidden:
                    throw new UnauthorizedAccessException("无访问权限", ex);
                default:
                    throw new ApplicationException($"MinIO错误: {ex.Message}", ex);
            }
        }


    }
}

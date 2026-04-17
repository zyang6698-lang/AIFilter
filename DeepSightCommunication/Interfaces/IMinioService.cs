using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DeepSightCommunication.Interfaces
{
    /// <summary>
    /// Minio 对象存储服务接口
    /// </summary>
    public interface IMinioService
    {
        /// <summary>
        /// 构建 Minio 客户端连接
        /// </summary>
        /// <param name="minio_ip">Minio 服务器 IP</param>
        /// <param name="minio_port">Minio 服务器端口</param>
        void BuildClient(string minio_ip, string minio_port);

        /// <summary>
        /// 同步读取 JSON 文件内容
        /// </summary>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">对象名称/路径</param>
        /// <param name="ip">Minio 服务器 IP</param>
        /// <returns>JSON 字符串</returns>
        string ReadJsonSync(string bucketName, string objectName, string ip);

        /// <summary>
        /// 同步获取图片流
        /// </summary>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">对象名称/路径</param>
        /// <param name="ip">Minio 服务器 IP</param>
        /// <returns>图片内存流</returns>
        MemoryStream GetImageStreamSync(string bucketName, string objectName, string ip);

        /// <summary>
        /// 下载文件或文件夹
        /// </summary>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">对象名称/路径</param>
        /// <param name="fileName">本地保存路径</param>
        /// <param name="ip">Minio 服务器 IP</param>
        /// <param name="isSingle">是否为单个文件下载</param>
        void Download(string bucketName, string objectName, string fileName, string ip, bool isSingle = false);

        /// <summary>
        /// 异步下载单个文件
        /// </summary>
        Task DownloadSingleAsync(string bucket, string minioFolderPath, string localRootPath, string ip, bool recursive = true);

        /// <summary>
        /// 异步下载文件夹
        /// </summary>
        Task DownloadFolderAsync(string bucket, string minioFolderPath, string localRootPath, string ip, bool recursive = true);

        /// <summary>
        /// 非递归列出指定前缀下的子文件夹（公共前缀），返回以 '/' 结尾的前缀列表。
        /// </summary>
        /// <param name="bucket">存储桶名称</param>
        /// <param name="prefix">当前前缀（如 "" 表示桶根，或 "SN123/"）</param>
        /// <param name="ip">Minio 服务器 IP</param>
        Task<List<string>> ListSubFoldersAsync(string bucket, string prefix, string ip);

        /// <summary>
        /// 递归列出指定前缀下的所有文件对象键（不含目录项）。
        /// </summary>
        /// <param name="bucket">存储桶名称</param>
        /// <param name="prefix">检索前缀（如 "" 表示桶根，或 "SN123/"）</param>
        /// <param name="ip">Minio 服务器 IP</param>
        Task<List<string>> ListAllObjectKeysAsync(string bucket, string prefix, string ip);
    }
}


namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// AVI 处理流程的上下文参数对象，封装从 AviReaderService 传递到 BusinessClass 的所有参数。
    /// 替代原先 10 个 string 参数的长参数列表，提高可读性和可维护性。
    /// </summary>
    public class AviProcessingContext
    {
        /// <summary>
        /// MinIO 服务器 IP 地址
        /// </summary>
        public string MinioIp { get; set; }

        /// <summary>
        /// MinIO 服务器端口
        /// </summary>
        public string MinioPort { get; set; }

        /// <summary>
        /// LevelDB 中的数据 Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// MinIO 路径头部（bucket 内的前缀路径）
        /// </summary>
        public string Head { get; set; }

        /// <summary>
        /// 产品序列号（SN）
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 面别（A/B）
        /// </summary>
        public string Side { get; set; }

        /// <summary>
        /// MinIO 中 JSON 文件的完整路径
        /// </summary>
        public string MinioPath { get; set; }

        /// <summary>
        /// 回写目标数据库名称
        /// </summary>
        public string WriteBackDbName { get; set; }

        /// <summary>
        /// AVI 侧数据库服务器 URL（用于 AVI 源读取与 AVI 回写）
        /// </summary>
        public string DbUrl { get; set; }

        /// <summary>
        /// VRS 回写目标数据库名称
        /// </summary>
        public string VrsWriteBackDbName { get; set; }

        /// <summary>
        /// VRS 侧数据库服务器 URL（用于 VRS 回写与 VRS V1.0 回写）
        /// </summary>
        public string VrsDbUrl { get; set; }
    }
}

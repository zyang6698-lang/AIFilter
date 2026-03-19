using System;
using System.IO;
using Newtonsoft.Json;
using DeepSightTool;

namespace DeepSightModel.Configuration
{
    /// <summary>
    /// Minio 对象存储设置类
    /// 注意：命名为 MinioSettings 以避免与 Minio SDK 中的 Minio.MinioConfig 类冲突
    /// </summary>
    public class MinioSettings
    {
        /// <summary>
        /// Access Key（用户名）
        /// </summary>
        public string AccessKey { get; set; } = "deepiobjectdata";

        /// <summary>
        /// Secret Key（密码）
        /// </summary>
        public string SecretKey { get; set; } = "deepiobject2019";

        /// <summary>
        /// 默认 Bucket 名称
        /// </summary>
        public string DefaultBucket { get; set; } = "deepiresults";

        /// <summary>
        /// 是否启用 SSL
        /// </summary>
        public bool UseSsl { get; set; } = false;

        /// <summary>
        /// 最大并发下载任务数
        /// </summary>
        public int MaxConcurrentDownloads { get; set; } = 15;

        /// <summary>
        /// 流复制缓冲区大小（字节）
        /// </summary>
        public int StreamBufferSize { get; set; } = 81920;

        /// <summary>
        /// 默认 Minio 端口（用于动态创建客户端时的缺省端口）
        /// </summary>
        public string DefaultPort { get; set; } = "9102";

        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static readonly string ConfigFilePath = Path.Combine(
            ConfigPaths.ConfigDirectory,
            "minio.config.json");

        /// <summary>
        /// 单例实例
        /// </summary>
        private static MinioSettings _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// 获取配置单例
        /// </summary>
        public static MinioSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = Load();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 从配置文件加载配置
        /// </summary>
        public static MinioSettings Load()
        {
            try
            {
                ConfigPaths.EnsureConfigDirectory();
                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    return JsonConvert.DeserializeObject<MinioSettings>(json) ?? new MinioSettings();
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"加载 Minio 配置文件失败: {ex.Message}");
            }

            // 如果文件不存在或加载失败，创建默认配置并保存
            var defaultConfig = new MinioSettings();
            defaultConfig.Save();
            return defaultConfig;
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        public void Save()
        {
            try
            {
                ConfigPaths.EnsureConfigDirectory();
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"保存 Minio 配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        public static void Reload()
        {
            lock (_lock)
            {
                _instance = Load();
            }
        }
    }
}


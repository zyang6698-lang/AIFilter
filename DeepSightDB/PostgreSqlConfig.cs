using System;
using System.IO;
using Newtonsoft.Json;

namespace DeepSightDB
{
    /// <summary>
    /// PostgreSQL 数据库配置类
    /// </summary>
    public class PostgreSqlConfig
    {
        /// <summary>
        /// 数据库服务器地址
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// 数据库端口
        /// </summary>
        public int Port { get; set; } = 5432;

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string Database { get; set; } = "deepsight";

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = "postgres";

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = "deepsightai";

        /// <summary>
        /// 连接超时时间（秒）
        /// </summary>
        public int Timeout { get; set; } = 30;

        /// <summary>
        /// 最大连接池大小
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// 最小连接池大小
        /// </summary>
        public int MinPoolSize { get; set; } = 1;

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        public string GetConnectionString()
        {
            // 性能优化参数:
            // - Pooling=true: 启用连接池
            // - No Reset On Close=true: 关闭连接时不重置连接状态，提高性能
            // - Write Buffer Size: 增大写缓冲区
            // - Read Buffer Size: 增大读缓冲区
            // - Socket Receive Buffer Size / Socket Send Buffer Size: 增大 Socket 缓冲区
            // - Tcp Keepalive: 保持连接活跃
            // - Keepalive: 连接保活间隔（秒）
            return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};" +
                   $"Timeout={Timeout};Maximum Pool Size={MaxPoolSize};Minimum Pool Size={MinPoolSize};" +
                   $"Pooling=true;No Reset On Close=true;" +
                   $"Write Buffer Size=16384;Read Buffer Size=16384;" +
                   $"Tcp Keepalive=true;Keepalive=60;";
        }

        /// <summary>
        /// 获取连接到 postgres 数据库的连接字符串（用于创建数据库）
        /// </summary>
        public string GetPostgresConnectionString()
        {
            return $"Host={Host};Port={Port};Database=postgres;Username={Username};Password={Password};Timeout={Timeout};Encoding=UTF8;Client Encoding=UTF8;";
        }

        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"configs","postgresql_config.json");

        /// <summary>
        /// 从配置文件加载配置
        /// </summary>
        public static PostgreSqlConfig Load()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    return JsonConvert.DeserializeObject<PostgreSqlConfig>(json) ?? new PostgreSqlConfig();
                }
            }
            catch (Exception ex)
            {
                DeepSightTool.LogTextHelper.Error($"加载 PostgreSQL 配置文件失败: {ex.Message}");
            }

            // 如果文件不存在或加载失败，创建默认配置并保存
            var defaultConfig = new PostgreSqlConfig();
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
                // 确保配置文件所在目录存在
                var directory = Path.GetDirectoryName(ConfigFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                DeepSightTool.LogTextHelper.Error($"保存 PostgreSQL 配置文件失败: {ex.Message}");
            }
        }
    }
}


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
        /// 命令执行超时时间（秒），默认 30。建表 / 建索引等 DDL 可临时调大。
        /// </summary>
        public int CommandTimeout { get; set; } = 30;

        /// <summary>
        /// idle_in_transaction_session_timeout（毫秒），由 PG 服务端兜底自动断开
        /// 长时间未提交 / 未回滚的事务连接，避免客户端崩溃后表锁被永久持有。
        /// 0 表示不启用。
        /// </summary>
        public int IdleInTransactionSessionTimeoutMs { get; set; } = 60000;

        /// <summary>
        /// 最大连接池大小
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// 最小连接池大小
        /// </summary>
        public int MinPoolSize { get; set; } = 1;

        /// <summary>
        /// 写缓冲区大小（字节）
        /// </summary>
        public int WriteBufferSize { get; set; } = 16384;

        /// <summary>
        /// 读缓冲区大小（字节）
        /// </summary>
        public int ReadBufferSize { get; set; } = 16384;

        /// <summary>
        /// 是否启用 TCP Keepalive
        /// </summary>
        public bool TcpKeepalive { get; set; } = true;

        /// <summary>
        /// Keepalive 间隔（秒）
        /// </summary>
        public int KeepaliveInterval { get; set; } = 60;

        /// <summary>
        /// 是否启用连接池
        /// </summary>
        public bool Pooling { get; set; } = true;

        /// <summary>
        /// 关闭连接时是否重置连接状态
        /// </summary>
        public bool NoResetOnClose { get; set; } = true;

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
            // - Tcp Keepalive: 保持连接活跃
            // - Keepalive: 连接保活间隔（秒）
            // - Command Timeout: 单条命令超时（秒）
            // 说明：idle_in_transaction_session_timeout 在 Npgsql 4.x 下不能通过连接串下发，
            //      由 DatabaseHelper 在 Open() 后用 SET 语句应用（见 ApplySessionSettings）。
            return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};" +
                   $"Timeout={Timeout};Command Timeout={CommandTimeout};" +
                   $"Maximum Pool Size={MaxPoolSize};Minimum Pool Size={MinPoolSize};" +
                   $"Pooling={Pooling};No Reset On Close={NoResetOnClose};" +
                   $"Write Buffer Size={WriteBufferSize};Read Buffer Size={ReadBufferSize};" +
                   $"Tcp Keepalive={TcpKeepalive};Keepalive={KeepaliveInterval};";
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


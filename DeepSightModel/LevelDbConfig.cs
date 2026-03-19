using DeepSightTool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DeepSightModel
{
    /// <summary>
    /// 单个 LevelDB 数据库配置
    /// </summary>
    public class LevelDbConfig
    {
        /// <summary>
        /// 数据库名称（唯一标识）
        /// </summary>
        [JsonProperty("db_name")]
        public string DbName { get; set; } = "ai_merged_results";

        /// <summary>
        /// 数据库服务器 IP
        /// </summary>
        [JsonProperty("ip")]
        public string IP { get; set; } = "http://127.0.0.1";

        /// <summary>
        /// 数据库服务器端口
        /// </summary>
        [JsonProperty("port")]
        public string Port { get; set; } = "9877";

        /// <summary>
        /// 回写目标数据库名称（写入AI结果时使用的db_name）
        /// </summary>
        [JsonProperty("write_back_db_name")]
        public string WriteBackDbName { get; set; } = "filter_time_to_airesults";

        /// <summary>
        /// 是否启用该数据库
        /// </summary>
        [JsonProperty("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 获取完整的 URL
        /// </summary>
        [JsonIgnore]
        public string Url => $"{IP}:{Port}";

        /// <summary>
        /// 用于显示的友好名称
        /// </summary>
        [JsonIgnore]
        public string DisplayName => $"{DbName} ({IP}:{Port})";
    }

    /// <summary>
    /// LevelDB 配置列表
    /// </summary>
    public class LevelDbConfigList
    {
        /// <summary>
        /// 数据库配置列表
        /// </summary>
        [JsonProperty("databases")]
        public List<LevelDbConfig> Databases { get; set; } = new List<LevelDbConfig>();
    }

    /// <summary>
    /// LevelDB 配置管理类
    /// </summary>
    public class LevelDbConfigManager
    {
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string ConfigDirectory = Path.Combine(BaseDirectory, "configs");
        private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "leveldb_config.json");

        private static LevelDbConfigList _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// 获取配置实例（单例模式）
        /// </summary>
        public static LevelDbConfigList Instance
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
        public static LevelDbConfigList Load()
        {
            try
            {
                EnsureConfigDirectory();

                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    var config = JsonConvert.DeserializeObject<LevelDbConfigList>(json);
                    if (config != null && config.Databases != null)
                    {
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"加载 LevelDB 配置文件失败: {ex.Message}");
            }

            // 如果文件不存在或加载失败，创建默认配置并保存
            var defaultConfig = GetDefaultConfig();
            Save(defaultConfig);
            return defaultConfig;
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        public static bool Save(LevelDbConfigList config)
        {
            try
            {
                EnsureConfigDirectory();
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
                _instance = config; // 更新缓存
                return true;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"保存 LevelDB 配置文件失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 刷新配置（重新从文件加载）
        /// </summary>
        public static void Refresh()
        {
            lock (_lock)
            {
                _instance = Load();
            }
        }

        /// <summary>
        /// 获取默认配置
        /// </summary>
        private static LevelDbConfigList GetDefaultConfig()
        {
            return new LevelDbConfigList
            {
                Databases = new List<LevelDbConfig>
                {
                    new LevelDbConfig
                    {
                        DbName = "ai_merged_results",
                        IP = "http://127.0.0.1",
                        Port = "9877",
                        IsEnabled = true
                    }
                }
            };
        }

        private static void EnsureConfigDirectory()
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
        }
    }
}


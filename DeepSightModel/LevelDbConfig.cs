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
        /// AVI 侧数据库服务器 IP（同时用于 AVI 源读取与 AVI 回写；仅存储纯IP/主机名）
        /// </summary>
        [JsonProperty("ip")]
        public string IP { get; set; } = "127.0.0.1";

        /// <summary>
        /// AVI 侧数据库服务器端口（同时用于 AVI 源读取与 AVI 回写）
        /// </summary>
        [JsonProperty("port")]
        public string Port { get; set; } = "9877";

        /// <summary>
        /// 回写目标数据库名称（写入AI结果时使用的db_name）
        /// </summary>
        [JsonProperty("write_back_db_name")]
        public string WriteBackDbName { get; set; } = "filter_time_to_airesults";

        /// <summary>
        /// VRS 侧数据库服务器 IP（同时用于 VRS 回写与 VRS V1.0 回写；
        /// 默认 null 表示沿用 AVI 侧 IP）
        /// </summary>
        [JsonProperty("vrs_ip")]
        public string VRSIP { get; set; } = null;

        /// <summary>
        /// VRS 侧数据库服务器端口（同时用于 VRS 回写与 VRS V1.0 回写；
        /// 默认 null 表示沿用 AVI 侧 Port）
        /// </summary>
        [JsonProperty("vrs_port")]
        public string VRSPort { get; set; } = null;

        /// <summary>
        /// VRS回写目标数据库名称（写入VRS详细结果时使用的db_name）
        /// </summary>
        [JsonProperty("vrs_write_back_db_name")]
        public string VRSWriteBackDbName { get; set; } = "ai_detail_results_tovrs";

        /// <summary>
        /// VRS回写数据库V1.0（写入V1.0版本VRS推理结果时使用的db_name）
        /// </summary>
        [JsonProperty("vrs_write_back_db_name_v1")]
        public string VRSWriteBackDbNameV1 { get; set; } = "ai_inference_result";

        /// <summary>
        /// VRS 历史结果库名称（按 SN 查询 VRS 历史判定结果时使用的 db_name）
        /// </summary>
        [JsonProperty("vrs_history_db_name")]
        public string VrsHistoryDbName { get; set; } = "vrs_history_result";

        /// <summary>
        /// 是否启用 VRS V1.0 回写（默认开启）
        /// </summary>
        [JsonProperty("enable_vrs_write_back_v1")]
        public bool EnableVRSWriteBackV1 { get; set; } = true;

        /// <summary>
        /// 是否启用该数据库
        /// </summary>
        [JsonProperty("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// A面 MinIO 服务器 IP（仅IP，端口使用 MinioSettings.DefaultPort）
        /// </summary>
        [JsonProperty("minio_ip_a")]
        public string MinioIpA { get; set; } = "127.0.0.1";

        /// <summary>
        /// B面 MinIO 服务器 IP（仅IP，端口使用 MinioSettings.DefaultPort）
        /// </summary>
        [JsonProperty("minio_ip_b")]
        public string MinioIpB { get; set; } = "127.0.0.1";

        /// <summary>
        /// 上次读取游标时间（持久化保存，用于断点续读）
        /// </summary>
        [JsonProperty("last_fetch_time")]
        public DateTime? LastFetchTime { get; set; }

        /// <summary>
        /// AVI 侧完整 URL（自动加 http:// 前缀；用于 AVI 源读取 + AVI 回写）
        /// </summary>
        [JsonIgnore]
        public string Url => $"http://{IP}:{Port}";

        /// <summary>
        /// VRS 侧完整 URL（自动加 http:// 前缀；用于 VRS 回写 + VRS V1.0 回写；
        /// 当 VRSIP/VRSPort 为空时回落到 AVI 侧）
        /// </summary>
        [JsonIgnore]
        public string VRSUrl
        {
            get
            {
                var ip = string.IsNullOrWhiteSpace(VRSIP) ? IP : VRSIP;
                var port = string.IsNullOrWhiteSpace(VRSPort) ? Port : VRSPort;
                return $"http://{ip}:{port}";
            }
        }

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
                        IP = "127.0.0.1",
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


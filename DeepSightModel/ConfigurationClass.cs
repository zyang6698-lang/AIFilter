using DeepSightModel.Configuration;
using DeepSightTool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightModel
{
    [Serializable]
    //常规配置
    public class ConfigurationClass
    {
        /// <summary>
        /// 项目名
        /// </summary>
        public string ProjectName { get; set; }
        /// <summary>
        /// 日志存储天数
        /// </summary>
        public int LogDay { get; set; }
        /// <summary>
        /// 最大缺陷数
        /// </summary>
        public int MaxDefectCount { get; set; }

        /// <summary>
        /// Agent关闭超时时间（毫秒）
        /// </summary>
        public int AgentShutdownTimeout { get; set; } = 2000;

        /// <summary>
        /// 欢迎页标题文字
        /// </summary>
        public string WelcomeTitle { get; set; } = "Deepsight AI";

        /// <summary>
        /// 欢迎页标题字体大小
        /// </summary>
        public float WelcomeFontSize { get; set; } = 36f;

        #region 快捷键配置（存储 System.Windows.Forms.Keys 枚举的字符串名称）

        /// <summary>
        /// 标记VVS_OK的快捷键
        /// </summary>
        public string ShortcutVvsOk { get; set; } = "D1";

        /// <summary>
        /// 标记VVS_NG的快捷键
        /// </summary>
        public string ShortcutVvsNg { get; set; } = "D2";

        /// <summary>
        /// 标记VVS未设置的快捷键
        /// </summary>
        public string ShortcutVvsNotSet { get; set; } = "D3";

        /// <summary>
        /// 切换到下一行的快捷键
        /// </summary>
        public string ShortcutNextRow { get; set; } = "Tab";

        /// <summary>
        /// 下一张图片的快捷键
        /// </summary>
        public string ShortcutNextImage { get; set; } = "Down";

        /// <summary>
        /// 上一张图片的快捷键
        /// </summary>
        public string ShortcutPrevImage { get; set; } = "Up";

        /// <summary>
        /// 下一页的快捷键
        /// </summary>
        public string ShortcutNextPage { get; set; } = "Right";

        /// <summary>
        /// 上一页的快捷键
        /// </summary>
        public string ShortcutPrevPage { get; set; } = "Left";

        #endregion

        /// <summary>
        /// 最大等待时间（秒）
        /// </summary>
        public int MaxWaitTime { get; set; } = DefaultValues.MaxWaitTime;

        /// <summary>
        /// 推理队列最大积压数量（除当前正在推理的一帧外，最多允许 N 帧已加载图片等待推理）
        /// 配合 TPL Dataflow 的 BoundedCapacity 实现背压，限制内存占用
        /// </summary>
        public int MaxPendingInferenceCount { get; set; } = DefaultValues.MaxPendingInferenceCount;

        /// <summary>
        /// 是否使用 Gerber 图进行推理和显示（false=使用 Template 图，true=使用 Gerber 图）
        /// </summary>
        public bool UseGerberImage { get; set; } = DefaultValues.UseGerberImage;

        public ConfigurationClass()
        {

        }
    }

    /// <summary>
    /// 常规配置读写类（JSON 格式）
    /// </summary>
    public class DeepSight_Config_class
    {
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string ConfigDirectory = Path.Combine(BaseDirectory, "configs");

        private static readonly string JsonConfigPath = Path.Combine(ConfigDirectory, "general.config.json");

        public DeepSight_Config_class()
        {
            EnsureConfigDirectory();

            if (!File.Exists(JsonConfigPath))
            {
                default_dat_config();
            }
        }

        public bool default_dat_config()
        {
            try
            {
                ConfigurationClass config = new ConfigurationClass
                {
                    ProjectName = DefaultValues.ProjectName,
                    LogDay = DefaultValues.LogDay,
                    MaxDefectCount = DefaultValues.MaxDefectCount,
                    AgentShutdownTimeout = DefaultValues.AgentShutdownTimeout,
                    WelcomeTitle = DefaultValues.WelcomeTitle,
                    WelcomeFontSize = DefaultValues.WelcomeFontSize,
                    ShortcutVvsOk = DefaultValues.ShortcutVvsOk,
                    ShortcutVvsNg = DefaultValues.ShortcutVvsNg,
                    ShortcutVvsNotSet = DefaultValues.ShortcutVvsNotSet,
                    ShortcutNextRow = DefaultValues.ShortcutNextRow,
                    ShortcutNextImage = DefaultValues.ShortcutNextImage,
                    ShortcutPrevImage = DefaultValues.ShortcutPrevImage,
                    ShortcutNextPage = DefaultValues.ShortcutNextPage,
                    ShortcutPrevPage = DefaultValues.ShortcutPrevPage,
                    UseGerberImage = DefaultValues.UseGerberImage
                };
                return Save(config);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取配置信息
        /// </summary>
        public bool Read(out ConfigurationClass system_config)
        {
            system_config = new ConfigurationClass();
            try
            {
                if (File.Exists(JsonConfigPath))
                {
                    var json = File.ReadAllText(JsonConfigPath);
                    system_config = JsonConvert.DeserializeObject<ConfigurationClass>(json) ?? new ConfigurationClass();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("读取常规配置异常", ex);
                return false;
            }
        }

        /// <summary>
        /// 保存配置信息（保存为 JSON 格式）
        /// </summary>
        public bool Save(ConfigurationClass system_config)
        {
            try
            {
                EnsureConfigDirectory();
                var json = JsonConvert.SerializeObject(system_config, Formatting.Indented);
                File.WriteAllText(JsonConfigPath, json);
                return true;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("保存常规配置异常", ex);
                return false;
            }
        }

        private static void EnsureConfigDirectory()
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
        }


    }

    public class WatchPathConfig
    {
        [JsonProperty("avi_name")]
        public string AviName { get; set; }

        [JsonProperty("A_path")]
        public string APath { get; set; }

        [JsonProperty("B_path")]
        public string BPath { get; set; }

        [JsonProperty("depth_from_watch_path_to_result_ini")]
        public int Depth { get; set; }

        [JsonProperty("IsEnable")]
        public bool IsEnable { get; set; }

        [JsonProperty("temporary_file_storage_area_A")]
        public string FileA { get; set; }
        [JsonProperty("temporary_file_storage_area_B")]
        public string FileB { get; set; }
        [JsonProperty("minio_url")]
        public string MinioConfig { get; set; }

        [JsonProperty("deepsight_agent_data_workspace")]
        public string DeepsightAgentDataWorkspace { get; set; }
        [JsonProperty("copy_or_cut_mode")]
        public string CopyOrCutMode { get; set; }
        [JsonProperty("B_path_index_timestamp")]
        public string BPathIndexTimestamp { get; set; }
        [JsonProperty("B_lot_timestamp")]
        public string BLotTimestamp { get; set; }
        [JsonProperty("B_panel_index_timestamp")]
        public string BPanelIndexTimestamp { get; set; }
        [JsonProperty("active_or_passive")]
        public string ActiveOrPassive { get; set; }
    }
    /// <summary>
    /// 属于Agent的类，只用得到WatchPaths，其余用不到
    /// </summary>
    public class AVIConfig
    {
        [JsonProperty("watch_path")]
        public List<WatchPathConfig> WatchPaths { get; set; } = new List<WatchPathConfig>();
        [JsonProperty("wait_flag")]
        public string WaitFlag { get; set; }
        [JsonProperty("finish_flag")]
        public string Finishflag { get; set; }
        [JsonProperty("A_minio_config")]
        public string AMinioConfig { get; set; }
        [JsonProperty("B_minio_config")]
        public string BMinioConfig { get; set; }
        [JsonProperty("LDB_endpoint")]
        public string LDBEndpoint { get; set; }
        [JsonProperty("infer_request_timeout")]
        public int InferRequestTimeout { get; set; }
        [JsonProperty("get_infer_result_interval")]
        public int GetInferResultInterval { get; set; }
        [JsonProperty("get_infer_result_timeout")]
        public int GetInferResultTimeout { get; set; }
        [JsonProperty("deepsight_agent_data_workspace")]
        public string DeepsightAgentDataWorkspace { get; set; }
        [JsonProperty("temporary_file_storage_area_A")]
        public string TemporaryFileStorageArea_A { get; set; }
        [JsonProperty("temporary_file_storage_area_B")]
        public string TemporaryFileStorageArea_B { get; set; }
    }
    public class DeepSight_AVI_class
    {
        public DeepSight_AVI_class() : this(skipInit: false)
        {
        }

        /// <summary>
        /// 构造函数，skipInit=true 时不自动创建目录和默认配置
        /// </summary>
        protected DeepSight_AVI_class(bool skipInit)
        {
            if (skipInit) return;

            if (!Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "ATS_Agent_EXE\\config"))
            {
                Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "config");
            }
            if (!File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "ATS_Agent_EXE\\config\\config.json"))
            {
                default_dat_config();
            }
        }

        public bool default_dat_config()
        {
            try
            {
                AVIConfig aviConfig = new AVIConfig
                {
                    WatchPaths = new List<WatchPathConfig>
                    {
                        new WatchPathConfig()
                        {
                            AviName = DefaultValues.AviName,
                            APath = string.Empty,  // 由用户配置
                            BPath = string.Empty,  // 由用户配置
                            IsEnable = false,
                        }
                    },
                    WaitFlag = DefaultValues.WaitFlag,
                    Finishflag = DefaultValues.FinishFlag,
                    AMinioConfig = DefaultValues.MinioConfig,
                    BMinioConfig = DefaultValues.MinioConfig,
                    LDBEndpoint = DefaultValues.LDBEndpoint,
                    GetInferResultInterval = DefaultValues.GetInferResultInterval,
                    GetInferResultTimeout = DefaultValues.GetInferResultTimeout,
                    DeepsightAgentDataWorkspace = string.Empty,  // 由用户配置
                    TemporaryFileStorageArea_A = string.Empty,   // 由用户配置
                    TemporaryFileStorageArea_B = string.Empty   // 由用户配置
                };
                return Save(aviConfig);
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="system_config"></param>
        /// <returns></returns>
        public bool Read(out AVIConfig _config)
        {
            bool result = false;
            _config = new AVIConfig();
            try
            {
                if (!File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "ATS_Agent_EXE\\config\\config.json"))
                {
                    result = false;
                }
                else
                {
                    // 加载配置
                    _config = JsonConvert.DeserializeObject<AVIConfig>(File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + "ATS_Agent_EXE\\config\\config.json")) ?? new AVIConfig();
                    result = true;
                }
            }
            catch (System.Exception ex)
            {
                LogTextHelper.Error("异常", ex);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 更新配置信息
        /// </summary>
        /// <param name="crane_config"></param>
        /// <returns></returns>
        public bool Save(AVIConfig _config)
        {
            bool result;
            try
            {
                if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "ATS_Agent_EXE\\config\\config.json"))
                {
                    File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + "ATS_Agent_EXE\\config\\config.json");
                }
                string json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + "ATS_Agent_EXE\\config\\config.json", json);
                result = true;
            }

            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
                result = false;
            }
            return result;
        }

    }

    /// <summary>
    /// 安全版本的 AVI 配置读写类，构造时不强制创建目录和默认配置。
    /// 适用于不确定是否存在 ATS_Agent_EXE 目录的场景（系统B）。
    /// </summary>
    public class DeepSight_AVI_class_Safe : DeepSight_AVI_class
    {
        public DeepSight_AVI_class_Safe() : base(skipInit: true)
        {
        }
    }
}

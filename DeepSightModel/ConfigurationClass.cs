using DeepSightTool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DeepSightModel
{
    [Serializable]
    [XmlRoot("DeepSight")]
    //常规配置
    public class ConfigurationClass
    {
        /// <summary>
        /// 项目名
        /// </summary>
        public string ProjectName { get; set; }
        /// <summary>
        /// 线体
        /// </summary>
        public string Line { get; set; }
        /// <summary>
        /// 日志存储天数
        /// </summary>
        public int LogDay { get; set; }
        /// <summary>
        /// 数据库IP
        /// </summary>
        public string ServerIP { get; set; }
        /// <summary>
        /// 数据库端口
        /// </summary>
        public string ServerPort { get; set; }
        /// <summary>
        /// minioIP
        /// </summary>
        public string endpoint_address { get; set; }
        /// <summary>
        /// minio端口
        /// </summary>
        public string MinioPort { get; set; }
        /// <summary>
        /// minio端口
        /// </summary>
        public string DsCenterUrl { get; set; }
        /// <summary>
        /// 最大缺陷数
        /// </summary>
        public int MaxDefectCount { get; set; } 

        public ConfigurationClass()
        {

        }
    }

    public class DeepSight_Config_class
    {
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string NewConfigDirectory = Path.Combine(BaseDirectory, "configs");
        private static readonly string NewConfigFileName = "general.config.xml";
        private static readonly string NewConfigPath = Path.Combine(NewConfigDirectory, NewConfigFileName);
        private static readonly string LegacyConfigDirectory = Path.Combine(BaseDirectory, "config");
        private static readonly string LegacyConfigPath = Path.Combine(LegacyConfigDirectory, "config.xml");

        public DeepSight_Config_class()
        {
            EnsureConfigDirectory();
            TryMigrateLegacyConfig();

            if (!File.Exists(NewConfigPath))
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
                    ProjectName = "DeepSight_AI",
                    Line = "Line1",
                    ServerIP = "http://",
                    ServerPort = "2000",
                    LogDay = 7,
                    endpoint_address = "127.0.0.1",
                    MinioPort = "9102",
                    DsCenterUrl = "http://dp55.local:82/api/zmq/dataImport",
                    MaxDefectCount = 200,
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
        /// <param name="system_config"></param>
        /// <returns></returns>
        public bool Read(out ConfigurationClass system_config)
        {
            bool result = false;
            system_config = new ConfigurationClass();
            try
            {
                string configPath = ResolveReadableConfigPath();
                if (!File.Exists(configPath))
                {
                    result = false;
                }
                else
                {
                    using (FileStream stream = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(ConfigurationClass));
                        system_config = (ConfigurationClass)xs.Deserialize(stream);
                    }
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
        public bool Save(ConfigurationClass system_config)
        {
            bool result = false;
            try
            {
                EnsureConfigDirectory();
                using (FileStream stream = new FileStream(NewConfigPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ConfigurationClass));
                    xs.Serialize(stream, system_config);
                }
                result = true;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
                result = false;
            }
            return result;
        }

        private static void EnsureConfigDirectory()
        {
            if (!Directory.Exists(NewConfigDirectory))
            {
                Directory.CreateDirectory(NewConfigDirectory);
            }
        }

        private static void TryMigrateLegacyConfig()
        {
            if (!File.Exists(NewConfigPath) && File.Exists(LegacyConfigPath))
            {
                try
                {
                    File.Copy(LegacyConfigPath, NewConfigPath, true);
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error("迁移旧常规配置失败", ex);
                }
            }
        }

        private static string ResolveReadableConfigPath()
        {
            if (File.Exists(NewConfigPath))
            {
                return NewConfigPath;
            }

            if (File.Exists(LegacyConfigPath))
            {
                return LegacyConfigPath;
            }

            return NewConfigPath;
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

    public class AVIConfig
    {
        [JsonProperty("watch_path")]
        public List<WatchPathConfig> WatchPaths { get; set; } = new List<WatchPathConfig>();
        [JsonProperty("max_wait_time")]
        public int MaxWaitTime { get; set; }
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
        public DeepSight_AVI_class()
        {
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
                AVIConfig aviConfig = new AVIConfig();
                WatchPathConfig watchPath = new WatchPathConfig()
                {
                    APath = "C:\\workspace\\ats\\real_ats_data\\real_ats_data\\Verify_A-2025.04yue",
                    BPath = "C:\\workspace\\ats\\real_ats_data\\real_ats_data\\Verify_B-2025.04yue",
                    Depth = 4,
                    IsEnable = false,
                };
                aviConfig.WatchPaths = new List<WatchPathConfig>();
                aviConfig.WatchPaths.Add(watchPath);
                aviConfig.MaxWaitTime = 5;
                aviConfig.WaitFlag = "wait_format.flag";
                aviConfig.Finishflag = "finish_format.flag";
                aviConfig.AMinioConfig = "192.168.77.126:9102";
                aviConfig.BMinioConfig = "192.168.77.126:9102";
                aviConfig.LDBEndpoint = "192.168.77.126:9877";
                aviConfig.GetInferResultInterval = 5;
                aviConfig.GetInferResultTimeout = 300;
                aviConfig.DeepsightAgentDataWorkspace = "C:\\minio\\deepiresults\\real_ats_data";
                aviConfig.TemporaryFileStorageArea_A = "C:\\workspace\\ats\\ats_data\\temporary_file_storage_area_A";
                aviConfig.TemporaryFileStorageArea_B = "C:\\workspace\\ats\\ats_data\\temporary_file_storage_area_B";
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
}

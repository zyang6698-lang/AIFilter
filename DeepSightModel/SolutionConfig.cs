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

    /// <summary>
    /// 缺陷设定文件
    /// </summary>
    [Serializable]
    [XmlRoot("AI_SolutionConfig")]
    public class SolutionConfig
    {
        /// <summary>
        /// 方案信息
        /// </summary>
        [XmlElementAttribute("SolutionConfig", IsNullable = false)]
        public List<SolutionAndFlow> solus { get; set; }
        [XmlElementAttribute("PartNumberImagesLoc", IsNullable = false)]
        public string PartNumberImagesLoc { get; set; } =@"D:\ATS_AI_INSTALL\TemplateImages";

        public SolutionConfig()
        {

        }
    }

    [XmlRootAttribute("SolutionAndFlow")]
    public class SolutionAndFlow
    {
        /// <summary>
        /// 料号
        /// </summary>
        [XmlAttribute("ProductSerial")]
        public string ProductSerial { get; set; }
       
        /// <summary>
        /// 方案
        /// </summary>
        [XmlAttribute("Asolution")]
        public string Asolution { get; set; }

        /// <summary>
        /// 流程flow
        /// </summary>
        [XmlAttribute("Aflow")]
        public string Aflow { get; set; }
        /// <summary>
        /// 方案
        /// </summary>
        [XmlAttribute("Bsolution")]
        public string Bsolution { get; set; }

        /// <summary>
        /// 流程flow
        /// </summary>
        [XmlAttribute("Bflow")]
        public string Bflow { get; set; }
        /// <summary>
        /// 是否switch
        /// </summary>
        [XmlAttribute("IsSwitch")]
        public bool IsSwitch { get; set; }

    }

    /// <summary>
    /// AI 方案配置读写类（已升级为 JSON 格式，兼容旧 XML 配置自动迁移）
    /// </summary>
    [Serializable]
    public class DeepSight_Solution
    {
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string ConfigDirectory = Path.Combine(BaseDirectory, "configs");

        // JSON 配置路径（新格式，优先使用）
        private static readonly string JsonConfigPath = Path.Combine(ConfigDirectory, "aisolution.config.json");
        // XML 配置路径（旧格式，用于兼容和迁移）
        private static readonly string XmlConfigPath = Path.Combine(ConfigDirectory, "aisolution.config.xml");
        // 更旧的配置路径
        private static readonly string LegacyConfigDirectory = Path.Combine(BaseDirectory, "AISolutionAndFlow");
        private static readonly string LegacyConfigPath = Path.Combine(LegacyConfigDirectory, "config.xml");

        public DeepSight_Solution()
        {
            EnsureConfigDirectory();
            TryMigrateToJson();

            if (!File.Exists(JsonConfigPath))
            {
                default_dat_config();
            }
        }

        /// <summary>
        /// 默认参数
        /// </summary>
        private bool default_dat_config()
        {
            try
            {
                SolutionConfig config = new SolutionConfig
                {
                    solus = new List<SolutionAndFlow>
                    {
                        new SolutionAndFlow
                        {
                            ProductSerial = "A123",
                            Asolution = "0317",
                            Aflow = "flow1",
                            Bsolution = "0317",
                            Bflow = "flow1",
                            IsSwitch = false
                        }
                    }
                };
                return Save(config);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取配置信息（优先读取 JSON，兼容旧 XML）
        /// </summary>
        public bool Read(out SolutionConfig sol_config)
        {
            sol_config = new SolutionConfig();
            try
            {
                // 优先读取 JSON 配置
                if (File.Exists(JsonConfigPath))
                {
                    var json = File.ReadAllText(JsonConfigPath);
                    sol_config = JsonConvert.DeserializeObject<SolutionConfig>(json) ?? new SolutionConfig();
                    return true;
                }

                // 兼容旧 XML 配置
                string xmlPath = File.Exists(XmlConfigPath) ? XmlConfigPath :
                                 File.Exists(LegacyConfigPath) ? LegacyConfigPath : null;

                if (xmlPath != null)
                {
                    using (var stream = new FileStream(xmlPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var xs = new XmlSerializer(typeof(SolutionConfig));
                        sol_config = (SolutionConfig)xs.Deserialize(stream);
                    }
                    // 自动迁移到 JSON
                    Save(sol_config);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("读取 AI 方案配置异常", ex);
                return false;
            }
        }

        /// <summary>
        /// 保存配置信息（保存为 JSON 格式）
        /// </summary>
        public bool Save(SolutionConfig sol_config)
        {
            try
            {
                EnsureConfigDirectory();
                var json = JsonConvert.SerializeObject(sol_config, Formatting.Indented);
                File.WriteAllText(JsonConfigPath, json);
                return true;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("保存 AI 方案配置异常", ex);
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

        /// <summary>
        /// 尝试从旧 XML 配置迁移到 JSON
        /// </summary>
        private void TryMigrateToJson()
        {
            if (File.Exists(JsonConfigPath))
            {
                return; // 已有 JSON 配置，无需迁移
            }

            // 按优先级查找旧配置
            var legacyPaths = new[] { XmlConfigPath, LegacyConfigPath };
            foreach (var path in legacyPaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var xs = new XmlSerializer(typeof(SolutionConfig));
                            var config = (SolutionConfig)xs.Deserialize(stream);
                            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                            File.WriteAllText(JsonConfigPath, json);
                            LogTextHelper.Info($"AI 方案配置已从 XML 迁移到 JSON: {path} -> {JsonConfigPath}");
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"迁移 AI 方案配置失败: {path}", ex);
                    }
                }
            }
        }
    }
}
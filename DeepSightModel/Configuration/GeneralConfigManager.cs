using System;
using System.IO;
using System.Xml.Serialization;
using DeepSightTool;

namespace DeepSightModel.Configuration
{
    /// <summary>
    /// 常规配置管理器（使用 JSON 格式）
    /// </summary>
    public class GeneralConfigManager : JsonConfigBase<ConfigurationClass>
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        protected override string ConfigPath => ConfigPaths.GeneralConfigPath;

        /// <summary>
        /// 获取默认配置
        /// </summary>
        protected override ConfigurationClass GetDefaultConfig()
        {
            return new ConfigurationClass
            {
                ProjectName = DefaultValues.ProjectName,
                LogDay = DefaultValues.LogDay,
                endpoint_address = DefaultValues.MinioIP,
                MinioPort = DefaultValues.MinioPort,
                MaxDefectCount = DefaultValues.MaxDefectCount,
                AgentShutdownTimeout = DefaultValues.AgentShutdownTimeout,
                WelcomeTitle = DefaultValues.WelcomeTitle,
                WelcomeFontSize = DefaultValues.WelcomeFontSize
            };
        }

        /// <summary>
        /// 尝试从旧版 XML 配置迁移
        /// </summary>
        protected override void TryMigrateLegacyConfig()
        {
            // 如果新配置已存在，无需迁移
            if (File.Exists(ConfigPath))
            {
                return;
            }

            // 尝试从旧版 XML 配置迁移
            var legacyPaths = new[]
            {
                ConfigPaths.LegacyGeneralConfigPath,  // configs/general.config.xml
                Path.Combine(ConfigPaths.LegacyConfigDirectory, "config.xml")  // config/config.xml
            };

            foreach (var legacyPath in legacyPaths)
            {
                if (File.Exists(legacyPath))
                {
                    try
                    {
                        var config = ReadXmlConfig(legacyPath);
                        if (config != null)
                        {
                            Save(config);
                            LogTextHelper.Info($"成功从旧版配置迁移: {legacyPath} -> {ConfigPath}");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"迁移旧版配置失败: {legacyPath}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// 读取旧版 XML 配置
        /// </summary>
        private ConfigurationClass ReadXmlConfig(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var xs = new XmlSerializer(typeof(ConfigurationClass));
                return (ConfigurationClass)xs.Deserialize(stream);
            }
        }
    }
}


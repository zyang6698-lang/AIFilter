using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using DeepSightTool;

namespace DeepSightModel.Configuration
{
    /// <summary>
    /// AI 方案配置管理器（使用 JSON 格式）
    /// </summary>
    public class AISolutionConfigManager : JsonConfigBase<SolutionConfig>
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        protected override string ConfigPath => ConfigPaths.AISolutionConfigPath;

        /// <summary>
        /// 获取默认配置
        /// </summary>
        protected override SolutionConfig GetDefaultConfig()
        {
            return new SolutionConfig
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
                },
                PartNumberImagesLoc = @"D:\ATS_AI_INSTALL\TemplateImages"
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
                ConfigPaths.LegacyAISolutionConfigPath,  // configs/aisolution.config.xml
                Path.Combine(ConfigPaths.LegacyAISolutionDirectory, "config.xml")  // AISolutionAndFlow/config.xml
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
                            LogTextHelper.Info($"成功从旧版 AI 方案配置迁移: {legacyPath} -> {ConfigPath}");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"迁移旧版 AI 方案配置失败: {legacyPath}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// 读取旧版 XML 配置
        /// </summary>
        private SolutionConfig ReadXmlConfig(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var xs = new XmlSerializer(typeof(SolutionConfig));
                return (SolutionConfig)xs.Deserialize(stream);
            }
        }

        /// <summary>
        /// 根据料号获取方案配置
        /// </summary>
        /// <param name="productSerial">料号</param>
        /// <returns>方案配置，未找到返回 null</returns>
        public SolutionAndFlow GetSolutionByProduct(string productSerial)
        {
            if (Read(out var config) && config?.solus != null)
            {
                return config.solus.Find(s => 
                    string.Equals(s.ProductSerial, productSerial, StringComparison.OrdinalIgnoreCase));
            }
            return null;
        }
    }
}


using System;
using System.IO;

namespace DeepSightModel.Configuration
{
    /// <summary>
    /// 统一管理所有配置文件路径
    /// </summary>
    public static class ConfigPaths
    {
        /// <summary>
        /// 应用程序基础目录
        /// </summary>
        public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// 配置文件根目录
        /// </summary>
        public static readonly string ConfigDirectory = Path.Combine(BaseDirectory, "configs");

        /// <summary>
        /// 旧版配置目录（用于迁移）
        /// </summary>
        public static readonly string LegacyConfigDirectory = Path.Combine(BaseDirectory, "config");

        /// <summary>
        /// 旧版 AI 方案配置目录
        /// </summary>
        public static readonly string LegacyAISolutionDirectory = Path.Combine(BaseDirectory, "AISolutionAndFlow");

        #region 配置文件名常量

        /// <summary>
        /// 常规配置文件名
        /// </summary>
        public const string GeneralConfigFileName = "general.config.json";

        /// <summary>
        /// AI 方案配置文件名
        /// </summary>
        public const string AISolutionConfigFileName = "aisolution.config.json";

        /// <summary>
        /// PostgreSQL 配置文件名
        /// </summary>
        public const string PostgreSqlConfigFileName = "postgresql.config.json";

        /// <summary>
        /// 机台注册表配置文件名
        /// </summary>
        public const string MachineRegistryConfigFileName = "machines.config.json";

        /// <summary>
        /// 旧版常规配置文件名 (XML)
        /// </summary>
        public const string LegacyGeneralConfigFileName = "general.config.xml";

        /// <summary>
        /// 旧版 AI 方案配置文件名 (XML)
        /// </summary>
        public const string LegacyAISolutionConfigFileName = "aisolution.config.xml";

        #endregion

        #region 完整路径属性

        /// <summary>
        /// 常规配置完整路径
        /// </summary>
        public static string GeneralConfigPath => Path.Combine(ConfigDirectory, GeneralConfigFileName);

        /// <summary>
        /// AI 方案配置完整路径
        /// </summary>
        public static string AISolutionConfigPath => Path.Combine(ConfigDirectory, AISolutionConfigFileName);

        /// <summary>
        /// PostgreSQL 配置完整路径
        /// </summary>
        public static string PostgreSqlConfigPath => Path.Combine(ConfigDirectory, PostgreSqlConfigFileName);

        /// <summary>
        /// 机台注册表配置完整路径
        /// </summary>
        public static string MachineRegistryConfigPath => Path.Combine(ConfigDirectory, MachineRegistryConfigFileName);

        /// <summary>
        /// 旧版常规配置完整路径 (XML)
        /// </summary>
        public static string LegacyGeneralConfigPath => Path.Combine(ConfigDirectory, LegacyGeneralConfigFileName);

        /// <summary>
        /// 旧版 AI 方案配置完整路径 (XML)
        /// </summary>
        public static string LegacyAISolutionConfigPath => Path.Combine(ConfigDirectory, LegacyAISolutionConfigFileName);

        #endregion

        /// <summary>
        /// 确保配置目录存在
        /// </summary>
        public static void EnsureConfigDirectory()
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
        }

        /// <summary>
        /// 获取配置文件完整路径
        /// </summary>
        /// <param name="fileName">配置文件名</param>
        /// <returns>完整路径</returns>
        public static string GetConfigPath(string fileName)
        {
            return Path.Combine(ConfigDirectory, fileName);
        }
    }
}


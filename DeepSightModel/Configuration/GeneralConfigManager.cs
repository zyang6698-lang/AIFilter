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
                MaxDefectCount = DefaultValues.MaxDefectCount,
                MaxPendingInferenceCount = DefaultValues.MaxPendingInferenceCount,
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
        }

    }
}


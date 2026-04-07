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
                Pipelines = new List<PipelineFlowConfig>
                {
                    new PipelineFlowConfig
                    {
                        Name = PipelineFlowConfig.DefaultName,
                        Asolution = "",
                        Aflow = "",
                        Bsolution = "",
                        Bflow = "",
                        IsSwitch = false,
                        ProductSerials = new List<string>()
                    }
                },
                PartNumberImagesLoc = @"D:\ATS_AI_INSTALL\TemplateImages"
            };
        }

        /// <summary>
        /// 读取配置（支持旧格式自动迁移）
        /// </summary>
        public override bool Read(out SolutionConfig config)
        {
            if (base.Read(out config))
            {
                // 旧格式迁移
                if ((config.Pipelines == null || config.Pipelines.Count == 0) && config.solus != null && config.solus.Count > 0)
                {
                    config.MigrateFromLegacy();
                    Save(config);
                }
                config.EnsureDefaultPipeline();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据料号获取所属的算法流程配置
        /// </summary>
        /// <param name="productSerial">料号</param>
        /// <returns>流程配置，未找到返回 null</returns>
        public PipelineFlowConfig GetPipelineByProduct(string productSerial)
        {
            if (Read(out var config))
            {
                return config.FindPipelineByProduct(productSerial);
            }
            return null;
        }
    }
}


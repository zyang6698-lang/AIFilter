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

    /// <summary>
    /// AI 方案配置（层级模型：算法流程 → 料号列表）
    /// </summary>
    [Serializable]
    public class SolutionConfig
    {
        /// <summary>
        /// 算法流程配置列表（新格式）
        /// </summary>
        public List<PipelineFlowConfig> Pipelines { get; set; }

        /// <summary>
        /// 旧格式兼容字段（仅用于反序列化迁移，保存时不写出）
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<SolutionAndFlow> solus { get; set; }

        public string PartNumberImagesLoc { get; set; } = @"D:\ATS_AI_INSTALL\TemplateImages";

        public SolutionConfig()
        {
        }

        /// <summary>
        /// 根据料号查找所属的算法流程配置
        /// </summary>
        public PipelineFlowConfig FindPipelineByProduct(string productSerial)
        {
            if (Pipelines == null || string.IsNullOrEmpty(productSerial)) return null;
            return Pipelines.FirstOrDefault(p =>
                p.ProductSerials != null &&
                p.ProductSerials.Any(ps => string.Equals(ps, productSerial, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// 获取 DEFAULT 算法流程配置
        /// </summary>
        public PipelineFlowConfig GetDefaultPipeline()
        {
            return Pipelines?.FirstOrDefault(p =>
                string.Equals(p.Name, PipelineFlowConfig.DefaultName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 确保 DEFAULT 流程始终存在。如果不存在，基于第一条已有配置复制生成。
        /// </summary>
        public void EnsureDefaultPipeline()
        {
            if (Pipelines == null) Pipelines = new List<PipelineFlowConfig>();

            if (GetDefaultPipeline() == null)
            {
                var template = Pipelines.FirstOrDefault();
                Pipelines.Insert(0, new PipelineFlowConfig
                {
                    Name = PipelineFlowConfig.DefaultName,
                    Asolution = template?.Asolution ?? "",
                    Aflow = template?.Aflow ?? "",
                    Bsolution = template?.Bsolution ?? "",
                    Bflow = template?.Bflow ?? "",
                    IsSwitch = template?.IsSwitch ?? false,
                    ProductSerials = new List<string>(),
                });
                LogTextHelper.Info("配置中缺少 DEFAULT 流程，已自动补充");
            }
        }

        /// <summary>
        /// 从旧格式 solus 迁移到新格式 Pipelines
        /// </summary>
        public void MigrateFromLegacy()
        {
            if (solus == null || solus.Count == 0) return;
            if (Pipelines != null && Pipelines.Count > 0) return; // 已有新格式，不迁移

            Pipelines = new List<PipelineFlowConfig>();

            // 按 (Asolution, Aflow, Bsolution, Bflow, IsSwitch) 分组
            var groups = solus
                .GroupBy(s => new { s.Asolution, s.Aflow, s.Bsolution, s.Bflow, s.IsSwitch })
                .ToList();

            int index = 1;
            foreach (var group in groups)
            {
                var products = group
                    .Where(s => !string.Equals(s.ProductSerial, PipelineFlowConfig.DefaultName, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.ProductSerial)
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList();

                // 检查这组是否包含 DEFAULT 条目
                bool isDefault = group.Any(s =>
                    string.Equals(s.ProductSerial, PipelineFlowConfig.DefaultName, StringComparison.OrdinalIgnoreCase));

                Pipelines.Add(new PipelineFlowConfig
                {
                    Name = isDefault ? PipelineFlowConfig.DefaultName : $"配置{index++}",
                    Asolution = group.Key.Asolution,
                    Aflow = group.Key.Aflow,
                    Bsolution = group.Key.Bsolution,
                    Bflow = group.Key.Bflow,
                    IsSwitch = group.Key.IsSwitch,
                    ProductSerials = products,
                });
            }

            // 迁移完成后清除旧数据
            solus = null;
            LogTextHelper.Info($"已从旧格式迁移 {groups.Count} 组算法流程配置");
        }
    }

    /// <summary>
    /// 算法流程配置（一个流程配置下挂多个料号）
    /// </summary>
    [Serializable]
    public class PipelineFlowConfig
    {
        /// <summary>
        /// 默认流程名称常量
        /// </summary>
        public const string DefaultName = "DEFAULT";

        /// <summary>
        /// 配置名称（如 "DEFAULT"、"配置1"）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A 面方案
        /// </summary>
        public string Asolution { get; set; }

        /// <summary>
        /// A 面流程
        /// </summary>
        public string Aflow { get; set; }

        /// <summary>
        /// B 面方案
        /// </summary>
        public string Bsolution { get; set; }

        /// <summary>
        /// B 面流程
        /// </summary>
        public string Bflow { get; set; }

        /// <summary>
        /// 是否 switch
        /// </summary>
        public bool IsSwitch { get; set; }

        /// <summary>
        /// 该流程配置下的料号列表
        /// </summary>
        public List<string> ProductSerials { get; set; } = new List<string>();
    }

    /// <summary>
    /// 旧格式兼容类（仅用于反序列化迁移）
    /// </summary>
    [Serializable]
    public class SolutionAndFlow
    {
        public string ProductSerial { get; set; }
        public string Asolution { get; set; }
        public string Aflow { get; set; }
        public string Bsolution { get; set; }
        public string Bflow { get; set; }
        public bool IsSwitch { get; set; }
    }

    /// <summary>
    /// AI 方案配置读写类（JSON 格式，支持旧格式自动迁移）
    /// </summary>
    [Serializable]
    public class DeepSight_Solution
    {
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string ConfigDirectory = Path.Combine(BaseDirectory, "configs");
        private static readonly string JsonConfigPath = Path.Combine(ConfigDirectory, "aisolution.config.json");

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        public DeepSight_Solution()
        {
            EnsureConfigDirectory();

            if (!File.Exists(JsonConfigPath))
            {
                CreateDefaultConfig();
            }
        }

        /// <summary>
        /// 创建默认配置（包含 DEFAULT 流程）
        /// </summary>
        private bool CreateDefaultConfig()
        {
            try
            {
                var config = new SolutionConfig
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
        /// 获取配置信息（读取后自动迁移旧格式并确保 DEFAULT 流程存在）
        /// </summary>
        public bool Read(out SolutionConfig sol_config)
        {
            sol_config = new SolutionConfig();
            try
            {
                if (File.Exists(JsonConfigPath))
                {
                    var json = File.ReadAllText(JsonConfigPath);
                    sol_config = JsonConvert.DeserializeObject<SolutionConfig>(json, JsonSettings) ?? new SolutionConfig();

                    // 旧格式迁移：如果有 solus 但没有 Pipelines
                    if ((sol_config.Pipelines == null || sol_config.Pipelines.Count == 0) && sol_config.solus != null && sol_config.solus.Count > 0)
                    {
                        sol_config.MigrateFromLegacy();
                        LogTextHelper.Info("检测到旧格式配置，已自动迁移到新格式");
                        // 迁移后立即保存新格式
                        Save(sol_config);
                    }

                    sol_config.EnsureDefaultPipeline();
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
        /// 保存配置信息（保存前确保 DEFAULT 流程存在，清除旧格式字段）
        /// </summary>
        public bool Save(SolutionConfig sol_config)
        {
            try
            {
                sol_config.EnsureDefaultPipeline();
                // 保存时清除旧格式字段，只保留新格式
                sol_config.solus = null;

                EnsureConfigDirectory();
                var json = JsonConvert.SerializeObject(sol_config, JsonSettings);
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
    }
}
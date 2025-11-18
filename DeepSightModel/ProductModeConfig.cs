using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using DeepSightTool;

namespace DeepSightModel
{
    /// <summary>
    /// 单个产品的裁剪/拷贝模式及偏移配置
    /// </summary>
    public class ProductModeItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("copy_cut_mode")]
        public string CopyCutMode { get; set; } // "copy" | "cut" | 其他扩展

        [JsonProperty("x_offset")]
        public int XOffset { get; set; }

        [JsonProperty("y_offset")]
        public int YOffset { get; set; }
    }

    /// <summary>
    /// 产品模式配置集合
    /// </summary>
    public class ProductModeConfig
    {
        [JsonProperty("products")]
        public List<ProductModeItem> Products { get; set; } = new List<ProductModeItem>();
    }

    /// <summary>
    /// 产品模式配置读写类（文件: ATS_Agent_EXE\\config\\config.json）
    /// 注意: 若与 AVIConfig 共用同一路径文件将产生冲突；建议分离文件名，如需共存可更改 FILE_NAME。
    /// </summary>
    public class DeepSight_ProductMode_class
    {
        private const string DIR = "ATS_Agent_EXE\\config";
        private const string FILE_NAME = "products.json"; // 与现有 AVI 使用同名时请确认文件结构是否已更新
        private static readonly string FullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DIR, FILE_NAME);

        public DeepSight_ProductMode_class()
        {
            EnsureDirectory();
            if (!File.Exists(FullPath))
            {
                DefaultConfig();
            }
        }

        private void EnsureDirectory()
        {
            var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DIR);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        /// <summary>
        /// 生成默认配置文件
        /// </summary>
        public bool DefaultConfig()
        {
            try
            {
                var cfg = new ProductModeConfig
                {
                    Products = new List<ProductModeItem>
                    {
                        new ProductModeItem{ Name = "MRT3905", CopyCutMode = "cut",  XOffset = 0, YOffset = 0},
                        new ProductModeItem{ Name = "NAT1834", CopyCutMode = "copy", XOffset = 0, YOffset = 0},
                    }
                };
                return Save(cfg);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("默认产品模式配置生成异常", ex);
                return false;
            }
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        public bool Read(out ProductModeConfig config)
        {
            config = new ProductModeConfig();
            try
            {
                if (!File.Exists(FullPath))
                {
                    return false;
                }
                var json = File.ReadAllText(FullPath);
                config = JsonConvert.DeserializeObject<ProductModeConfig>(json) ?? new ProductModeConfig();
                return true;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("读取产品模式配置异常", ex);
                return false;
            }
        }

        /// <summary>
        /// 保存配置（覆盖写入）
        /// </summary>
        public bool Save(ProductModeConfig config)
        {
            try
            {
                EnsureDirectory();
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(FullPath, json);
                return true;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("保存产品模式配置异常", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取指定料号的模式项, 未找到返回 null
        /// </summary>
        public ProductModeItem GetProduct(string name, ProductModeConfig config)
        {
            if (config?.Products == null) return null;
            return config.Products.Find(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 更新或新增某产品配置并保存
        /// </summary>
        public bool UpsertProduct(ProductModeItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Name)) return false;
            if (!Read(out var cfg))
            {
                cfg = new ProductModeConfig();
            }
            var existing = GetProduct(item.Name, cfg);
            if (existing == null)
            {
                cfg.Products.Add(item);
            }
            else
            {
                existing.CopyCutMode = item.CopyCutMode;
                existing.XOffset = item.XOffset;
                existing.YOffset = item.YOffset;
            }
            return Save(cfg);
        }
    }
}

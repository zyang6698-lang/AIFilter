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
    /// 缺陷设定文件
    /// </summary>
    [Serializable]
    public class SolutionConfig
    {
        /// <summary>
        /// 方案信息
        /// </summary>
        public List<SolutionAndFlow> solus { get; set; }
        public string PartNumberImagesLoc { get; set; } = @"D:\ATS_AI_INSTALL\TemplateImages";

        public SolutionConfig()
        {

        }
    }

    public class SolutionAndFlow
    {
        /// <summary>
        /// 料号
        /// </summary>
        public string ProductSerial { get; set; }

        /// <summary>
        /// 方案
        /// </summary>
        public string Asolution { get; set; }

        /// <summary>
        /// 流程flow
        /// </summary>
        public string Aflow { get; set; }
        /// <summary>
        /// 方案
        /// </summary>
        public string Bsolution { get; set; }

        /// <summary>
        /// 流程flow
        /// </summary>
        public string Bflow { get; set; }
        /// <summary>
        /// 是否switch
        /// </summary>
        public bool IsSwitch { get; set; }

    }

    /// <summary>
    /// AI 方案配置读写类（JSON 格式）
    /// </summary>
    [Serializable]
    public class DeepSight_Solution
    {
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string ConfigDirectory = Path.Combine(BaseDirectory, "configs");

        private static readonly string JsonConfigPath = Path.Combine(ConfigDirectory, "aisolution.config.json");

        public DeepSight_Solution()
        {
            EnsureConfigDirectory();

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
        /// 获取配置信息
        /// </summary>
        public bool Read(out SolutionConfig sol_config)
        {
            sol_config = new SolutionConfig();
            try
            {
                if (File.Exists(JsonConfigPath))
                {
                    var json = File.ReadAllText(JsonConfigPath);
                    sol_config = JsonConvert.DeserializeObject<SolutionConfig>(json) ?? new SolutionConfig();
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


    }
}
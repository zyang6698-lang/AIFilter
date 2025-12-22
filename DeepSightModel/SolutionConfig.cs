using DeepSightTool;
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
        public string PartNumberImagesLoc { get; set;} = @"D:\ATS_AI_INSTALL\TemplateImages";

        public SolutionConfig()
        {

        }
    }
    /// <summary>
    /// 项
    /// </summary>ATS_Agent_EXEATS_Agent_EXE
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

    //******************读写配置文件XML******************
    [Serializable]
    public class DeepSight_Solution
    {
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string NewConfigDirectory = Path.Combine(BaseDirectory, "configs");
        private static readonly string NewConfigFileName = "aisolution.config.xml";
        private static readonly string NewConfigPath = Path.Combine(NewConfigDirectory, NewConfigFileName);
        private static readonly string LegacyConfigDirectory = Path.Combine(BaseDirectory, "AISolutionAndFlow");
        private static readonly string LegacyConfigPath = Path.Combine(LegacyConfigDirectory, "config.xml");

        public DeepSight_Solution()
        {
            EnsureConfigDirectory();
            TryMigrateLegacyConfig();

            if (!File.Exists(NewConfigPath))
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
                    solus = new List<SolutionAndFlow>(),
                };
                SolutionAndFlow defect = new SolutionAndFlow
                {
                    ProductSerial= "A123",
                    Asolution = "0317",
                    Aflow = "flow1",
                    Bsolution = "0317",
                    Bflow = "flow1",
                    IsSwitch = false,
                };
                config.solus.Add(defect);
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
        public bool Read(out SolutionConfig sol_config)
        {
            bool result = false;
            sol_config = new SolutionConfig();
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
                        XmlSerializer xs = new XmlSerializer(typeof(SolutionConfig));
                        sol_config = (SolutionConfig)xs.Deserialize(stream);
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
        public bool Save(SolutionConfig sol_config)
        {
            bool result = false;
            try
            {
                EnsureConfigDirectory();
                using (TextWriter sw = TextWriter.Synchronized(new StreamWriter(NewConfigPath, false, Encoding.UTF8)))
                {
                    XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(SolutionConfig));
                    xml.Serialize(sw, sol_config);
                    result = true;
                }
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
                    LogTextHelper.Error("迁移AISolution配置失败", ex);
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
}
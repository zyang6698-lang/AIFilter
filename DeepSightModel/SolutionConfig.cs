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
        /// 当前料号
        /// </summary>
        [XmlAttribute("CurrentProductSerial")]
        public string CurrentProductSerial { get; set; }
        /// <summary>
        /// 当前方案
        /// </summary>
        [XmlAttribute("CurrentSolution")]

        public string CurrentSolution { get; set; }
        /// <summary>
        /// 当前流程
        /// </summary>
        [XmlAttribute("CurrentFlow")]
        public string CurrentFlow { get; set; }
        /// <summary>
        /// 是否switch
        /// </summary>
        [XmlAttribute("CurrentisSwitch")]
        public bool CurrentisSwitch { get; set; }
        /// <summary>
        /// 方案信息
        /// </summary>
        [XmlElementAttribute("SolutionConfig", IsNullable = false)]
        public List<SolutionAndFlow> solus { get; set; }
        public SolutionConfig()
        {

        }
    }
    /// <summary>
    /// 项
    /// </summary>
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
        public DeepSight_Solution()
        {
            if (!Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "AISolutionAndFlow"))
            {
                Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "AISolutionAndFlow");
            }
            if (!File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "AISolutionAndFlow\\config.xml"))
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
                config.CurrentProductSerial = "A123";
                config.CurrentSolution = "solution1";
                config.CurrentFlow = "flow1";
                SolutionAndFlow defect = new SolutionAndFlow
                {
                    ProductSerial= "A123",
                    Asolution = "0317",
                    Aflow = "flow1",
                    Bsolution = "0317",
                    Bflow = "flow1",
                    //Asolution = "085-a",
                    //Aflow = "085-a",
                    //Bsolution = "085-b",
                    //Bflow = "085-b",
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
                if (!File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "AISolutionAndFlow\\config.xml"))
                {
                    result = false;
                }
                else
                {
                    FileStream stream = new FileStream(System.AppDomain.CurrentDomain.BaseDirectory + "AISolutionAndFlow\\config.xml", FileMode.Open, FileAccess.Read, FileShare.Read);
                    XmlSerializer xs = new XmlSerializer(typeof(SolutionConfig));
                    sol_config = (SolutionConfig)xs.Deserialize(stream);
                    stream.Close();
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
                string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "AISolutionAndFlow");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = Path.Combine(path, "config.xml");
                using (TextWriter sw = System.IO.StreamWriter.Synchronized(new System.IO.StreamWriter(path, false, Encoding.UTF8)))
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
    }
}
using DeepSightModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    public partial class AviCtr2Container : UserControl
    {
        private List<AviCtr2> aviCtr2Controls = new List<AviCtr2>();

        public AviCtr2Container()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 根据配置列表创建并添加 AviCtr2 控件
        /// </summary>
        /// <param name="watchPaths">配置列表</param>
        public void CreateMachinePanels(List<WatchPathConfig> watchPaths)
        {
            // 清除旧控件
            flowLayoutPanel1.Controls.Clear();
            aviCtr2Controls.Clear();

            foreach (var config in watchPaths)
            {
                AddAviControl(config);
            }
        }

        /// <summary>
        /// 添加单个 AviCtr2 控件
        /// </summary>
        /// <param name="watchPath">配置信息</param>
        private void AddAviControl(WatchPathConfig watchPath)
        {
            try
            {
                AviCtr2 ctr = new AviCtr2(watchPath);
                aviCtr2Controls.Add(ctr);
                flowLayoutPanel1.Controls.Add(ctr);
            }
            catch (Exception ex)
            {
                // 可以在这里记录异常
                Console.WriteLine($"Failed to create AviCtr2: {ex.Message}");
            }
        }
        /// <summary>
        /// 获取所有 AviCtr2 控件的配置列表
        /// </summary>
        /// <returns>WatchPathConfig 列表</returns>
        public List<WatchPathConfig> GetAllConfigs()
        {
            return aviCtr2Controls.Select(ctr => ctr.ctrConfig).ToList();
        }
        /// <summary>
        /// 更新指定名称的 AviCtr2 控件的统计信息
        /// </summary>
        /// <param name="aviName">AVI 名称</param>
        /// <param name="totalImages">图片总数</param>
        /// <param name="aiOkImages">AI OK 图片数</param>
        public void UpdateAviCtrStats(string aviName, int totalImages, int aiOkImages)
        {
            var ctr = aviCtr2Controls.FirstOrDefault(c => c.ctrConfig.AviName == aviName);
            if (ctr != null)
            {
                ctr.TotalImages = totalImages;
                ctr.AiOkImages = aiOkImages;
            }
        }

        /// <summary>
        /// 更新所有 AviCtr2 控件的信息
        /// </summary>
        /// <param name="getLatestPanelInfo">用于获取最新信息的委托</param>
        public void UpdateAllAviCtrsInfo(Func<string, (string LotNumber, string SerialNumber, string ProductSerial, string PathIndex, double Utilization)> getLatestPanelInfo)
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    foreach (var ctr in aviCtr2Controls)
                    {
                        if (ctr.ctrConfig.IsEnable)
                        {
                            var info = getLatestPanelInfo(ctr.ctrConfig.AviName);
                            ctr.LotId = info.LotNumber;
                            ctr.ProductSerial = info.SerialNumber;
                            ctr.PathIndex = info.PathIndex;
                            ctr.Utilization = info.Utilization;
                        }
                    }
                }));
            }
        }

        public void UpdateAviCtrInfo(string aviName, string productSerial, string lotId, double utilization)
        {
            foreach (var ctr in aviCtr2Controls)
            {
                if (ctr.ctrConfig.AviName == aviName)
                {
                    ctr.ProductSerial = productSerial;
                    ctr.LotId = lotId;
                    ctr.Utilization = utilization;
                    break;
                }
            }
        }


        public void UpdateAllAviCtrLotSn(Func<string, (string LotNumber, string SerialNumber, string ProductSerial, string PathIndex)> getLatestPanelInfo)
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    foreach (var ctr in aviCtr2Controls)
                    {
                        if (ctr.ctrConfig.IsEnable)
                        {
                            var tmp = getLatestPanelInfo(ctr.ctrConfig.AviName);
                            if (tmp.LotNumber != null && tmp.SerialNumber != null)
                            {
                                ctr.LotId = tmp.LotNumber;
                                ctr.ProductSerial = tmp.ProductSerial;
                                ctr.PathIndex = tmp.PathIndex;
                            }
                        }
                    }
                }));
            }
        }

    }
}

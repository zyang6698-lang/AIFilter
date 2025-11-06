using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightModel
{
    public class VBModel
    {
        public string Key { get; set; }
        public string SN { get; set; }
        public string Side { get; set; }
        public List<int> DefectIndex { get; set; }
        public List<int> PcsIndex { get; set; }

        public RootVBInfo VbInfo;
        //2025/08/21/增加minio路径信息
        public string  minioPath;
        public RootPanelInfo panelInfo { get; set; }
        //用于判断ai是否部署该料号，若未部署则为true
        public bool isByPass { get; set; } = false;
    }
}

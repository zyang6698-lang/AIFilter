using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightModel
{
    //AVI 热力点
    public class AVI_HeatPoints
    {
        /// <summary>
        /// 缺陷图所属SN
        /// </summary>
        public string SN { get; set; }
        /// <summary>
        /// 缺陷图所属面次
        /// </summary>
        public string Side { get; set; }

        //public PointsInfo pointsInfo { get; set; } = new PointsInfo();
        /// <summary>
        /// 每个sn单面的缺陷点位
        /// </summary>
        public List<PointsInfo> pointsInfos { get; set; } = new List<PointsInfo>();
    }

    public class PointsInfo
    {
        /// <summary>
        /// 缺陷名
        /// </summary>
        public string DefectName { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        /// <summary>
        /// 缺陷形态（客户需要根据缺陷形态查看分布）
        /// 目前暂时分为点状（dot）/线状（line）
        /// </summary>
        public string DefectShape { get; set; }

        public string ImagePath { get; set; }
    }
}

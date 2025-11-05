using System;

namespace DeepSightModel
{
    public class PassThroughtAviData
    {
        //机台号
        public string MachineID { get; set; }
        //AVI板子数量
        public int AVICount { get; set; }
        //AI判定OK的板子数量
        public int AIOkCount { get; set; }
        //AI通过的板子比例
        public double AIPassRate { get { return AIOkCount * 1.0 / AVICount; } set {; } }
        //统计日期
        public DateTime GetDataTime { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace DeepSightModel
{
    /// <summary>
    /// 单个缺陷的详细信息
    /// </summary>
    public class HeatPoint
    {
        public string DefectName { get; set; }
        public string DefectType { get; set; }
        public string DefectShape { get; set; }
        public int RoiX { get; set; }
        public int RoiY { get; set; }
        public string ImagePath { get; set; }
    }

    /// <summary>
    /// 单面（A/B面）的数据
    /// </summary>
    public class SideData
    {
        public List<HeatPoint> HeatPoints { get; set; } = new List<HeatPoint>();
        //总报点数量
        public int TotalDefectsCount { get; set; }
        //过滤后报点信息
        public int RemainingDefectsCount { get; set; }
        public string SerialNumber { get; set; }
        public string Side { get; set; }
    }

    /// <summary>
    /// 用于传入单面数据的记录
    /// </summary>
    public class PanelSideRecord
    {
        public string MachineId { get; set; }
        public DateTime DetectionDate { get; set; }
        public string SerialNumber { get; set; }
        public string LotNumber { get; set; }
        public string Side { get; set; } // "A" 或 "B"
        public SideData Data { get; set; } = new SideData();
    }


    /// <summary>
    /// 数据库中完整的面板检测记录（用于读取）
    /// </summary>
    public class PanelDataRecord
    {
        public int Id { get; set; } // 数据库主键
        public string MachineId { get; set; }
        public DateTime DetectionDate { get; set; }
        public string SerialNumber { get; set; }
        public string LotNumber { get; set; }
        public bool IsAIOk { get; set; }
    }

    /// <summary>
    /// 用于统计查询的每日数据摘要
    /// </summary>
    public class DailyStat
    {
        public DateTime Date { get; set; }
        public string MachineId { get; set; }
        public int TotalBoards { get; set; }
        public int AIOkBoards { get; set; }
    }
}
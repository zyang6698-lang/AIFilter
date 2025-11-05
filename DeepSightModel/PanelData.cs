using System;
using System.Collections.Generic;

namespace DeepSightModel
{
    /// <summary>
    /// 单个缺陷的详细信息
    /// </summary>
    public class DefectDetail
    {
        public string DefectName { get; set; }
        public string DefectType { get; set; }
        public int RoiX { get; set; }
        public int RoiY { get; set; }
        public string ImagePath { get; set; }
    }

    /// <summary>
    /// 单面（A/B面）的数据
    /// </summary>
    public class SideData
    {
        //过滤后报点信息
        public List<DefectDetail> RemainingDefectInfoList { get; set; } = new List<DefectDetail>();
        //总报点数量
        public int TotalDefectsCount { get; set; }
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
        public SideData SideA { get; set; } = new SideData();
        public SideData SideB { get; set; } = new SideData();
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
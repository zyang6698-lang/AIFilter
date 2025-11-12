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
        // 0: AVI OK
        // 1: AVI NG 但过滤后OK
        // 2: AVI NG 过滤后仍NG
        // 3: AVI NG 未过滤
        public int State { get; set; }
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
        public string ProductSerial { get; set; }
        public string Side { get; set; } // "A"  "B"
        public SideData Data { get; set; } = new SideData();
        public string PathIndex { get; set; }
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
        public string ProductSerial { get; set; }
        public bool IsAIOk { get; set; }
        public string PathIndex { get; set; }
        public List<SideData> Sides { get; set; }

        public static BoardStat GetBoardStat(List<PanelDataRecord> records)
        {

            int aviCount = 0;
            int aviOKCount = 0;
            int aiFilterCount = 0;
            int aiFilterOKCount = 0;
            int aiFilterUninspectedCount = 0;

            for (int i = 0; i < records.Count; i++)
            {
                aviCount++;

                if (records[i].Sides.Count == 2)
                {
                    var sideA = records[i].Sides[0];
                    var sideB = records[i].Sides[1];
                    var stateA = sideA.State;
                    var stateB = sideB.State;

                    aiFilterCount += sideA.TotalDefectsCount + sideB.TotalDefectsCount;

                    if (stateA == 0 && stateB == 0)
                        aviOKCount++;
                    else if (stateA == 3 || stateB == 3)
                        aiFilterUninspectedCount += sideA.TotalDefectsCount + sideB.TotalDefectsCount;
                    else
                    {
                        aiFilterOKCount = (sideA.TotalDefectsCount + sideB.TotalDefectsCount) - (sideA.RemainingDefectsCount + sideB.RemainingDefectsCount);
                    }
                }
            }
            return new BoardStat
            {
                aviCount = aviCount,
                aviOKCount = aviOKCount,
                aiFilterCount = aiFilterCount,
                aiFilterOKCount = aiFilterOKCount,
                aiFilterUninspectedCount = aiFilterUninspectedCount
            };
        }
    }

    public class BoardStat
    {
      public int aviCount { get; set; }
      public int aviOKCount { get; set; } 
      public int aiFilterCount { get; set; } 
      public int aiFilterOKCount { get; set; } 
      public int aiFilterUninspectedCount { get; set; } 
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
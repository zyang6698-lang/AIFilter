using System;
using System.Collections.Generic;
using System.Linq;

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
        // 3: AVI NG 未过滤/过滤异常
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
        public DateTime? AviCreationTime { get; set; }

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
        public DateTime? AviCreationTime { get; set; }
        public List<SideData> Sides { get; set; }

        public static BoardStat GetBoardStat(List<PanelDataRecord> records)
        {
            // 使用 PLINQ 并行处理记录
            return records.AsParallel()
                .Select(record =>
                {
                    var stat = new BoardStat();
                    stat.aviPanelCount = 1; // 每个记录计为1

                    if (record.Sides.Count == 2)
                    {
                        var sideA = record.Sides[0];
                        var sideB = record.Sides[1];
                        var stateA = sideA.State;
                        var stateB = sideB.State;

                        stat.aiFilterCount = sideA.TotalDefectsCount + sideB.TotalDefectsCount;
                        stat.aiFilterOKCount = (sideA.TotalDefectsCount + sideB.TotalDefectsCount) - (sideA.RemainingDefectsCount + sideB.RemainingDefectsCount);

                        if (stateA == 0 && stateB == 0)
                            stat.aviPanelOKCount = 1;
                        else if (stateA == 3 || stateB == 3)
                            stat.aiFilterUninspectedCount = sideA.TotalDefectsCount + sideB.TotalDefectsCount;

                        if ((sideA.TotalDefectsCount + sideB.TotalDefectsCount)>0&& (sideA.RemainingDefectsCount + sideB.RemainingDefectsCount)==0)
                        {
                            stat.aiPanelOKCount = 1;
                        }
                    }
                    return stat;
                })
                .Aggregate(
                    new BoardStat(), // 初始累加器
                    (total, current) => // 聚合函数
                    {
                        total.aviPanelCount += current.aviPanelCount;
                        total.aviPanelOKCount += current.aviPanelOKCount;
                        total.aiFilterCount += current.aiFilterCount;
                        total.aiPanelOKCount += current.aiPanelOKCount;
                        total.aiFilterOKCount += current.aiFilterOKCount;
                        total.aiFilterUninspectedCount += current.aiFilterUninspectedCount;
                        return total;
                    });
        }
    }

    public class BoardStat
    {
        //  面板维度

        // AVI检测面板总数
        public int aviPanelCount { get; set; }
        // AVI检测面板OK数
        public int aviPanelOKCount { get; set; }
        //  AI检测面板OK总数
        public int aiPanelOKCount { get; set; }
        //  报点维度

        // AI检测报点总数
        public int aiFilterCount { get; set; }
        // AI检测报点OK数
        public int aiFilterOKCount { get; set; }
        // AI检测未检测报点数
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
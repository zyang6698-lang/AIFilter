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
        public string AIStatus { get; set; }
        public string VVSStatus { get; set; } = "";
    }

    /// <summary>
    /// 单面（A/B面）的数据
    /// </summary>
    public class SideData
    {
        public List<HeatPoint> HeatPoints { get; set; } = new List<HeatPoint>();
        // 总报点数量 (AVI 检测出的缺陷数)
        public int TotalDefectsCount { get; set; }
        // 过滤后保留的缺陷数 (AI / 复判后仍存在的缺陷)
        public int RemainingDefectsCount { get; set; }
        public string SerialNumber { get; set; }
        public string Side { get; set; }

        // 分阶段状态：0 未运行 / 1 OK / 2 NG / 3 异常
        public int AviState { get; set; }      // AVI 阶段
        public int AiState { get; set; }       // AI 过滤阶段
        public int VvsState { get; set; }      // VVS 复判阶段
        public int VrsState { get; set; }      // VRS 终判阶段
        // 最终状态：0 待处理 / 1 最终OK / 2 最终NG
        public int FinalState { get; set; }
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
        // 料号
        public string ProductSerial { get; set; }
        public bool IsAIOk { get; set; }
        public string PathIndex { get; set; }
        public DateTime? AviCreationTime { get; set; }
        public List<SideData> Sides { get; set; }

        public static BoardStat GetBoardStat(List<PanelDataRecord> records)
        {
            return records.AsParallel()
                .Select(record =>
                {
                    var stat = new BoardStat();
                    stat.aviPanelCount = 1;

                    if (record.Sides != null && record.Sides.Count == 2)
                    {
                        var sideA = record.Sides[0];
                        var sideB = record.Sides[1];

                        // AVI 面板 OK：两面 AVI OK
                        if (sideA.AviState == 1 && sideB.AviState == 1)
                            stat.aviPanelOKCount = 1;

                        // AI 面板 OK：两面 AI OK (且均经过 AVI 检测)
                        if (sideA.AviState > 0 && sideB.AviState > 0 && sideA.AiState == 1 && sideB.AiState == 1)
                            stat.aiPanelOKCount = 1;

                        // 报点总数 (以 AVI 提取的缺陷数为基础)
                        stat.aiFilterCount = sideA.TotalDefectsCount + sideB.TotalDefectsCount;

                        // AI 过滤 OK 的报点数 (被过滤掉的缺陷数)
                        stat.aiFilterOKCount = (sideA.TotalDefectsCount + sideB.TotalDefectsCount) - (sideA.RemainingDefectsCount + sideB.RemainingDefectsCount);

                        // 未检测报点：任一面 AI 未运行 (AiState == 0 且 AviState != 0)
                        if ((sideA.AiState == 0 && sideA.AviState != 0) || (sideB.AiState == 0 && sideB.AviState != 0))
                        {
                            stat.aiFilterUninspectedCount = sideA.TotalDefectsCount + sideB.TotalDefectsCount;
                        }
                    }
                    return stat;
                })
                .Aggregate(new BoardStat(), (total, current) =>
                {
                    total.aviPanelCount += current.aviPanelCount;
                    total.aviPanelOKCount += current.aviPanelOKCount;
                    total.aiPanelOKCount += current.aiPanelOKCount;
                    total.aiFilterCount += current.aiFilterCount;
                    total.aiFilterOKCount += current.aiFilterOKCount;
                    total.aiFilterUninspectedCount += current.aiFilterUninspectedCount;
                    return total;
                });
        }
    }

    public class BoardStat
    {
        // 面板维度
        public int aviPanelCount { get; set; }
        public int aviPanelOKCount { get; set; }
        public int aiPanelOKCount { get; set; }
        // 报点维度
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
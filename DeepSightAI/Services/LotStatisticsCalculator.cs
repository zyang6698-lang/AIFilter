using DeepSightAI;
using DeepSightModel;
using System.Collections.Generic;
using System.Linq;

namespace DeepSightAI.Services
{
    /// <summary>
    /// Lot 统计指标计算器，负责从面板数据中计算 PCS / 报点 / Panel 级别的统计指标。
    /// 从 UcAiReview.CollectPanelData 中抽离，便于单独单元测试。
    /// </summary>
    public static class LotStatisticsCalculator
    {
        /// <summary>
        /// 根据面板列表计算统计指标，按 Lot 分组返回。
        /// </summary>
        public static Dictionary<string, LotStatistics> Calculate(IEnumerable<PanelDataRecord> panels)
        {
            var result = new Dictionary<string, LotStatistics>();

            foreach (var panel in panels)
            {
                if (panel.Sides == null) continue;
                string lotNumber = panel.LotNumber ?? "未知Lot";

                if (!result.ContainsKey(lotNumber))
                    result[lotNumber] = new LotStatistics();
                var stat = result[lotNumber];

                if (string.IsNullOrEmpty(stat.MachineId))
                {
                    stat.MachineId = panel.MachineId;
                    stat.ProductSerial = panel.ProductSerial;
                }

                stat.TotalPanelCount++;
                bool panelAllAviOk = true;
                bool panelAllAiPass = true;

                foreach (var side in panel.Sides)
                {
                    if (side == null) continue;
                    var boardSide = GetBoardSideStats(stat, side.Side);
                    CalculateSideStatistics(side, stat, boardSide, ref panelAllAviOk, ref panelAllAiPass);
                }

                if (panelAllAviOk) stat.AviOkPanelCount++;
                if (panelAllAiPass) stat.AiPassPanelCount++;
            }

            return result;
        }

        private static BoardSideStatistics GetBoardSideStats(LotStatistics stat, string side)
        {
            if (string.Equals(side, "A", System.StringComparison.OrdinalIgnoreCase))
                return stat.SideAStats;
            if (string.Equals(side, "B", System.StringComparison.OrdinalIgnoreCase))
                return stat.SideBStats;
            return null;
        }

        /// <summary>
        /// 计算单面的 PCS 和报点级别统计
        /// </summary>
        private static void CalculateSideStatistics(SideData side, LotStatistics stat,
            BoardSideStatistics boardSide, ref bool panelAllAviOk, ref bool panelAllAiPass)
        {
            if (boardSide != null) boardSide.TotalBoardCount++;
            bool sideAviOk = false;
            bool sideAiPass = true;

            // PCS 级别统计
            if (side.AviState == 1)
            {
                stat.TotalPcsCount++;
                stat.AviOkPcsCount++;
                if (boardSide != null)
                {
                    boardSide.TotalPcsCount++;
                    boardSide.AviOkPcsCount++;
                }
                sideAviOk = true;
            }
            else
            {
                panelAllAviOk = false;
                if (side.DetectPoints != null && side.DetectPoints.Count > 0)
                {
                    var pcsGroups = side.DetectPoints.GroupBy(p => p.PcsIndex);
                    foreach (var pcsGroup in pcsGroups)
                    {
                        var pts = pcsGroup.ToList();
                        stat.TotalPcsCount++;
                        if (boardSide != null) boardSide.TotalPcsCount++;
                        if (pts.Any(p => p.AIStatus == 2 || p.AIStatus == 4))
                        {
                            stat.AiNgPcsCount++;
                            if (boardSide != null) boardSide.AiNgPcsCount++;
                            panelAllAiPass = false;
                            sideAiPass = false;
                        }
                        else if (pts.Any(p => p.AIStatus == 3))
                        {
                            stat.AiExceptionPcsCount++;
                            if (boardSide != null) boardSide.AiExceptionPcsCount++;
                            panelAllAiPass = false;
                            sideAiPass = false;
                        }
                        else if (pts.All(p => p.AIStatus == 1))
                        {
                            stat.AiOkPcsCount++;
                            if (boardSide != null) boardSide.AiOkPcsCount++;
                        }
                        else
                        {
                            stat.AiUninspectedPcsCount++;
                            if (boardSide != null) boardSide.AiUninspectedPcsCount++;
                            panelAllAiPass = false;
                            sideAiPass = false;
                        }
                    }
                }
                else
                {
                    stat.TotalPcsCount++;
                    if (boardSide != null) boardSide.TotalPcsCount++;
                    switch (side.AiState)
                    {
                        case 1:
                            stat.AiOkPcsCount++;
                            if (boardSide != null) boardSide.AiOkPcsCount++;
                            break;
                        case 2:
                            stat.AiNgPcsCount++;
                            if (boardSide != null) boardSide.AiNgPcsCount++;
                            panelAllAiPass = false;
                            sideAiPass = false;
                            break;
                        case 3:
                            stat.AiExceptionPcsCount++;
                            if (boardSide != null) boardSide.AiExceptionPcsCount++;
                            panelAllAiPass = false;
                            sideAiPass = false;
                            break;
                        default:
                            stat.AiUninspectedPcsCount++;
                            if (boardSide != null) boardSide.AiUninspectedPcsCount++;
                            panelAllAiPass = false;
                            sideAiPass = false;
                            break;
                    }
                }
            }

            if (boardSide != null)
            {
                if (sideAviOk) boardSide.AviOkBoardCount++;
                if (sideAviOk || sideAiPass) boardSide.AiPassBoardCount++;
            }

            // 报点级别统计
            if (side.DetectPoints != null)
            {
                int count = side.DetectPoints.Count;
                int okCount = side.DetectPoints.Count(p => p.AIStatus == 1);
                int ngCount = side.DetectPoints.Count(p => p.AIStatus == 2 || p.AIStatus == 4);
                int exCount = side.DetectPoints.Count(p => p.AIStatus == 3);
                int unCount = side.DetectPoints.Count(p => p.AIStatus == 0);

                stat.TotalPointCount += count;
                stat.AiOkPointCount += okCount;
                stat.AiNgPointCount += ngCount;
                stat.AiExceptionPointCount += exCount;
                stat.AiUninspectedPointCount += unCount;

                if (boardSide != null)
                {
                    boardSide.TotalPointCount += count;
                    boardSide.AiOkPointCount += okCount;
                    boardSide.AiNgPointCount += ngCount;
                    boardSide.AiExceptionPointCount += exCount;
                    boardSide.AiUninspectedPointCount += unCount;
                }
            }
        }
    }
}

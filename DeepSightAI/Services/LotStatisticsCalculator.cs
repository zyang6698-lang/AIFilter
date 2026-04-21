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
                    CalculateSideStatistics(side, stat, ref panelAllAviOk, ref panelAllAiPass);
                }

                if (panelAllAviOk) stat.AviOkPanelCount++;
                if (panelAllAiPass) stat.AiPassPanelCount++;
            }

            return result;
        }

        /// <summary>
        /// 计算单面的 PCS 和报点级别统计
        /// </summary>
        private static void CalculateSideStatistics(SideData side, LotStatistics stat,
            ref bool panelAllAviOk, ref bool panelAllAiPass)
        {
            // PCS 级别统计
            if (side.AviState == 1)
            {
                stat.TotalPcsCount++;
                stat.AviOkPcsCount++;
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
                        if (pts.Any(p => p.AIStatus == 2))
                        {
                            stat.AiNgPcsCount++;
                            panelAllAiPass = false;
                        }
                        else if (pts.Any(p => p.AIStatus == 3))
                        {
                            stat.AiExceptionPcsCount++;
                            panelAllAiPass = false;
                        }
                        else if (pts.All(p => p.AIStatus == 1))
                        {
                            stat.AiOkPcsCount++;
                        }
                        else
                        {
                            stat.AiUninspectedPcsCount++;
                            panelAllAiPass = false;
                        }
                    }
                }
                else
                {
                    stat.TotalPcsCount++;
                    switch (side.AiState)
                    {
                        case 1: stat.AiOkPcsCount++; break;
                        case 2: stat.AiNgPcsCount++; panelAllAiPass = false; break;
                        case 3: stat.AiExceptionPcsCount++; panelAllAiPass = false; break;
                        default: stat.AiUninspectedPcsCount++; panelAllAiPass = false; break;
                    }
                }
            }

            // 报点级别统计
            if (side.DetectPoints != null)
            {
                stat.TotalPointCount += side.DetectPoints.Count;
                stat.AiOkPointCount += side.DetectPoints.Count(p => p.AIStatus == 1);
                stat.AiNgPointCount += side.DetectPoints.Count(p => p.AIStatus == 2);
                stat.AiExceptionPointCount += side.DetectPoints.Count(p => p.AIStatus == 3);
                stat.AiUninspectedPointCount += side.DetectPoints.Count(p => p.AIStatus == 0);
            }
        }
    }
}

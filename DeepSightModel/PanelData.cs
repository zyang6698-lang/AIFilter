using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepSightModel
{
    /// <summary>
    /// 单个缺陷的详细信息
    /// </summary>
    public class DetectInfo
    {
        public string DefectName { get; set; }
        public string DefectType { get; set; }
        public string DefectShape { get; set; }
        public int RoiX { get; set; }
        public int RoiY { get; set; }
        public int Width {  get; set; }
        public int Height { get; set; }
        public int OriginRoiX { get; set; }
        public int OriginRoiY { get; set; }
        public int OriginWidth { get; set; }
        public int OriginHeight { get; set; }
        public string ImagePath { get; set; }
        public string TempImagePath { get; set; }
        public string GerberImagePath {  get; set; }
        public string DrawInfo { get; set; }
        public int DefectIndex {  get; set; }
        public int PcsIndex {  get; set; }
        // 分阶段状态：0 未运行 / 1 OK / 2 NG / 3 异常
        public int AIStatus { get; set; }
        public int VVSStatus { get; set; }
        public int VrsState { get; set; }
        public int FinalState { get; set; }
        /// <summary>
        /// 是否为重点缺陷（运行时标记，不持久化到数据库）
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsKeyDefect { get; set; }

        /// <summary>
        /// 用于显示的序列号（非持久化字段，由界面赋值）
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string DisplaySN { get; set; }

        /// <summary>
        /// 浅数据克隆（当前属性均为值类型或字符串，可直接逐项复制）
        /// </summary>
        public DetectInfo Clone()
        {
            return new DetectInfo
            {
                DefectName = DefectName,
                DefectType = DefectType,
                DefectShape = DefectShape,
                RoiX = RoiX,
                RoiY = RoiY,
                Width = Width,
                Height = Height,
                OriginRoiX = OriginRoiX,
                OriginRoiY = OriginRoiY,
                OriginWidth = OriginWidth,
                OriginHeight = OriginHeight,
                ImagePath = ImagePath,
                TempImagePath = TempImagePath,
                GerberImagePath = GerberImagePath,
                DrawInfo = DrawInfo,
                AIStatus = AIStatus,
                VVSStatus = VVSStatus,
                VrsState = VrsState,
                FinalState = FinalState,
                IsKeyDefect = IsKeyDefect,
                DisplaySN = DisplaySN
            };
        }
    }

    /// <summary>
    /// 单面（A/B面）的数据
    /// </summary>
    public class SideData
    {
        public List<DetectInfo> DetectPoints { get; set; } = new List<DetectInfo>();
        public string Side { get; set; }

        // 分阶段状态：0 未运行 / 1 OK / 2 NG / 3 异常
        public int AviState { get; set; }      // AVI 阶段
        public int AiState { get; set; }       // AI 过滤阶段
        public int VvsState { get; set; }      // VVS 复判阶段
        public int VrsState { get; set; }      // VRS 终判阶段
        // 最终状态：0 待处理 / 1 最终OK / 2 最终NG
        public int FinalState { get; set; }

        // 模型验证测试状态：0 未测试 / 1 一致 / 2 不一致 / 3 测试中 / 4 测试异常
        public int TestState { get; set; }
        // 最近一次测试时间
        public DateTime? LastTestTime { get; set; }
        // DrawInfo JSON 字符串（存储绘制/阈值信息）
        public string DrawInfo { get; set; }
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
        public DateTime? AviCreationTime { get; set; }
        public List<SideData> Sides { get; set; }
        //尽量少用，数据库负担会比较大
        public static BoardStat GetBoardStat(List<PanelDataRecord> records)
        {
            if (records == null || records.Count == 0)
                return new BoardStat();

            // 计算各机台的稼动率并取平均值 - 按机台分组并行计算
            var utilizationByMachine = records
                .Where(r => r.AviCreationTime.HasValue && r.AviCreationTime.Value.Date == DateTime.Now.Date)
                .GroupBy(r => r.MachineId)
                .AsParallel()
                .Select(g => MathHelper.CalculateUtilizationRatePercent(g.Select(r => r.AviCreationTime.Value)))
                .Where(u => u > 0)
                .ToList();

            double avgUtilization = utilizationByMachine.Count > 0 ? utilizationByMachine.Average() : 0;

            // 统计面板和报点数据
            var stat = records.AsParallel()
                .Select(record =>
                {
                    var s = new BoardStat { AviPanelCount = 1 };

                    if (record.Sides?.Count != 2)
                        return s;

                    var sideA = record.Sides[0];
                    var sideB = record.Sides[1];
                    var pointsA = sideA.DetectPoints ?? new List<DetectInfo>();
                    var pointsB = sideB.DetectPoints ?? new List<DetectInfo>();

                    // AVI 面板 OK：两面 AVI OK
                    if (sideA.AviState == 1 && sideB.AviState == 1)
                        s.AviPanelOKCount = 1;

                    // AI 面板 OK：两面 AI OK (且均经过 AVI 检测)
                    if (sideA.AviState > 0 && sideB.AviState > 0 && sideA.AiState == 1 && sideB.AiState == 1)
                        s.AiPanelOKCount = 1;

                    // 报点总数
                    s.AiFilterCount = pointsA.Count + pointsB.Count;

                    // AI 过滤 OK 的报点数
                    s.AiFilterOKCount = pointsA.Count(t => t.AIStatus == 1) + pointsB.Count(t => t.AIStatus == 1);

                    // 未检测报点（累加两面）
                    s.AiFilterUninspectedCount =
                        (sideA.AiState == 3 ? pointsA.Count(t => t.AIStatus == 3) : 0) +
                        (sideB.AiState == 3 ? pointsB.Count(t => t.AIStatus == 3) : 0);

                    // 重点缺陷统计
                    int keyCount = pointsA.Count(t => t.IsKeyDefect) + pointsB.Count(t => t.IsKeyDefect);
                    s.KeyDefectCount = keyCount;
                    if (keyCount > 0) s.KeyDefectPanelCount = 1;

                    return s;
                })
                .Aggregate(new BoardStat(), (total, current) =>
                {
                    total.AviPanelCount += current.AviPanelCount;
                    total.AviPanelOKCount += current.AviPanelOKCount;
                    total.AiPanelOKCount += current.AiPanelOKCount;
                    total.AiFilterCount += current.AiFilterCount;
                    total.AiFilterOKCount += current.AiFilterOKCount;
                    total.AiFilterUninspectedCount += current.AiFilterUninspectedCount;
                    total.KeyDefectCount += current.KeyDefectCount;
                    total.KeyDefectPanelCount += current.KeyDefectPanelCount;
                    return total;
                });

            stat.Utilization = avgUtilization;
            return stat;
        }
    }

    public class BoardStat
    {
        // 面板维度
        public int AviPanelCount { get; set; }
        public int AviPanelOKCount { get; set; }
        public int AiPanelOKCount { get; set; }
        // 报点维度
        public int AiFilterCount { get; set; }
        public int AiFilterOKCount { get; set; }
        public int AiFilterUninspectedCount { get; set; }
        public double Utilization { get; set; }

        // 重点缺陷维度
        /// <summary>
        /// 重点缺陷报点总数（AI 判定为 NG 且缺陷名在重点列表中）
        /// </summary>
        public int KeyDefectCount { get; set; }
        /// <summary>
        /// 含有重点缺陷的面板数量
        /// </summary>
        public int KeyDefectPanelCount { get; set; }
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


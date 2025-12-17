using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DeepSightTool;

namespace DeepSightDB
{
    /// <summary>
    /// Maintains lightweight board statistics in memory to avoid repeatedly querying the database.
    /// </summary>
    public static class BoardStatCache
    {
        public sealed class SideSnapshot
        {
            public int AviState { get; set; }
            public int AiState { get; set; }
            public int TotalPoints { get; set; }
            public int AiOkPoints { get; set; }
            public int UninspectedPoints { get; set; }

            public static SideSnapshot FromRecord(PanelSideRecord record)
            {
                var points = record.Data?.DetectPoints ?? new List<DetectInfo>();
                return new SideSnapshot
                {
                    AviState = record.Data?.AviState ?? 0,
                    AiState = record.Data?.AiState ?? 0,
                    TotalPoints = points.Count,
                    AiOkPoints = points.Count(p => p.AIStatus == 1),
                    UninspectedPoints = points.Count(p => p.AIStatus == 3)
                };
            }
        }

        private sealed class PanelStatEntry
        {
            private SideSnapshot _sideA;
            private SideSnapshot _sideB;
            private BoardStat _contribution = new BoardStat();

            public bool IsEmpty => _sideA == null && _sideB == null;

            public SideSnapshot SideA => _sideA;
            public SideSnapshot SideB => _sideB;

            public BoardStat Update(string side, SideSnapshot snapshot)
            {
                var normalizedSide = string.IsNullOrWhiteSpace(side) ? "A" : side.Trim();
                if (string.Equals(normalizedSide, "A", StringComparison.OrdinalIgnoreCase))
                {
                    _sideA = snapshot;
                }
                else if (string.Equals(normalizedSide, "B", StringComparison.OrdinalIgnoreCase))
                {
                    _sideB = snapshot;
                }
                else
                {
                    _sideA = snapshot;
                }

                var newContribution = CalculateContribution();
                var delta = new BoardStat
                {
                    AviPanelCount = newContribution.AviPanelCount - _contribution.AviPanelCount,
                    AviPanelOKCount = newContribution.AviPanelOKCount - _contribution.AviPanelOKCount,
                    AiPanelOKCount = newContribution.AiPanelOKCount - _contribution.AiPanelOKCount,
                    AiFilterCount = newContribution.AiFilterCount - _contribution.AiFilterCount,
                    AiFilterOKCount = newContribution.AiFilterOKCount - _contribution.AiFilterOKCount,
                    AiFilterUninspectedCount = newContribution.AiFilterUninspectedCount - _contribution.AiFilterUninspectedCount,
                    Utilization = 0
                };

                _contribution = newContribution;
                return delta;
            }

            public BoardStat GetContribution()
            {
                return CalculateContribution();
            }

            private BoardStat CalculateContribution()
            {
                var result = new BoardStat();

                if (_sideA == null && _sideB == null)
                {
                    return result;
                }

                result.AviPanelCount = 1;

                if (_sideA != null)
                {
                    result.AiFilterCount += _sideA.TotalPoints;
                    result.AiFilterOKCount += _sideA.AiOkPoints;
                    if (_sideA.AiState == 3)
                    {
                        result.AiFilterUninspectedCount += _sideA.UninspectedPoints;
                    }
                }

                if (_sideB != null)
                {
                    result.AiFilterCount += _sideB.TotalPoints;
                    result.AiFilterOKCount += _sideB.AiOkPoints;
                    if (_sideB.AiState == 3)
                    {
                        result.AiFilterUninspectedCount += _sideB.UninspectedPoints;
                    }
                }

                if (_sideA != null && _sideB != null)
                {
                    if (_sideA.AviState == 1 && _sideB.AviState == 1)
                    {
                        result.AviPanelOKCount = 1;
                    }

                    if (_sideA.AviState > 0 && _sideB.AviState > 0 && _sideA.AiState == 1 && _sideB.AiState == 1)
                    {
                        result.AiPanelOKCount = 1;
                    }
                }

                return result;
            }
        }

        // Persistence DTOs
        [XmlRoot("BoardStatCacheState")]
        public class BoardStatCacheState
        {
            public DateTime Date { get; set; }
            public List<PanelEntryState> Panels { get; set; }
            public List<MachineTimestampsState> Machines { get; set; }
        }

        public class PanelEntryState
        {
            public string SerialNumber { get; set; }
            public SideSnapshot SideA { get; set; }
            public SideSnapshot SideB { get; set; }
        }

        public class MachineTimestampsState
        {
            public string MachineId { get; set; }
            public List<DateTime> Timestamps { get; set; }
        }

        private static readonly object SyncRoot = new object();
        private static readonly Dictionary<string, PanelStatEntry> PanelEntries = new Dictionary<string, PanelStatEntry>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, List<DateTime>> MachineTimestamps = new Dictionary<string, List<DateTime>>(StringComparer.OrdinalIgnoreCase);
        // 每台机台各自的统计
        private static readonly Dictionary<string, BoardStat> MachineTotals = new Dictionary<string, BoardStat>(StringComparer.OrdinalIgnoreCase);
        // SN -> 机台 的映射（用于将面板统计增量计入机台）
        private static readonly Dictionary<string, string> SerialToMachine = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // 每台机台的最新 Lot/SN 等
        private sealed class MachineLotSn
        {
            public string LotNumber { get; set; }
            public string SerialNumber { get; set; }
            public string ProductSerial { get; set; }
            public string PathIndex { get; set; }
        }
        private static readonly Dictionary<string, MachineLotSn> MachineLatestLotSn = new Dictionary<string, MachineLotSn>(StringComparer.OrdinalIgnoreCase);

        private static BoardStat _totals = new BoardStat();
        private static DateTime _currentDate = DateTime.Today;
        private static bool _stateLoadedForDate = false;

        private static void EnsureCurrentDate()
        {
            var today = DateTime.Today;
            if (today == _currentDate)
            {
                return;
            }

            // Optionally persist previous date before reset
            try { SaveStateInternal(_currentDate); } catch { /* ignore */ }

            PanelEntries.Clear();
            MachineTimestamps.Clear();
            MachineTotals.Clear();
            SerialToMachine.Clear();
            MachineLatestLotSn.Clear();
            _totals = new BoardStat();
            _currentDate = today;
            _stateLoadedForDate = false;
        }

        private static void EnsureStateForToday()
        {
            EnsureCurrentDate();
            if (_stateLoadedForDate)
            {
                return;
            }

            try
            {
                LoadStateInternal(_currentDate);
            }
            catch
            {
                // ignore load failures
            }
            finally
            {
                _stateLoadedForDate = true;
            }
        }

        private static void ApplyDelta(BoardStat delta)
        {
            _totals.AviPanelCount += delta.AviPanelCount;
            _totals.AviPanelOKCount += delta.AviPanelOKCount;
            _totals.AiPanelOKCount += delta.AiPanelOKCount;
            _totals.AiFilterCount += delta.AiFilterCount;
            _totals.AiFilterOKCount += delta.AiFilterOKCount;
            _totals.AiFilterUninspectedCount += delta.AiFilterUninspectedCount;
        }

        private static void ApplyDeltaToMachine(string machineId, BoardStat delta)
        {
            if (string.IsNullOrWhiteSpace(machineId)) return;
            if (!MachineTotals.TryGetValue(machineId, out var s))
            {
                s = new BoardStat();
                MachineTotals[machineId] = s;
            }
            s.AviPanelCount += delta.AviPanelCount;
            s.AviPanelOKCount += delta.AviPanelOKCount;
            s.AiPanelOKCount += delta.AiPanelOKCount;
            s.AiFilterCount += delta.AiFilterCount;
            s.AiFilterOKCount += delta.AiFilterOKCount;
            s.AiFilterUninspectedCount += delta.AiFilterUninspectedCount;
        }

        private static void RecomputeTotals()
        {
            _totals = new BoardStat();
            foreach (var kv in PanelEntries)
            {
                var contribution = kv.Value.GetContribution();
                ApplyDelta(contribution);
                if (SerialToMachine.TryGetValue(kv.Key, out var mid))
                {
                    ApplyDeltaToMachine(mid, contribution);
                }
            }
        }

        private static double CalculateUtilization()
        {
            if (MachineTimestamps.Count == 0)
            {
                return 0;
            }

            var perMachineRates = MachineTimestamps
                .Where(kvp => kvp.Value.Count >= 2)
                .Select(kvp => MathHelper.CalculateUtilizationRatePercent(kvp.Value));

            return perMachineRates.Any() ? perMachineRates.Average() : 0;
        }

        private static double CalculateUtilization(string machineId)
        {
            if (string.IsNullOrWhiteSpace(machineId)) return 0;
            if (!MachineTimestamps.TryGetValue(machineId, out var list) || list == null || list.Count < 2) return 0;
            return MathHelper.CalculateUtilizationRatePercent(list);
        }

        private static string GetStateDirectory()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeepSightAI", "BoardStatCache");
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static string GetStateFilePath(DateTime date)
        {
            return Path.Combine(GetStateDirectory(), date.ToString("yyyyMMdd") + ".xml");
        }

        private static void SaveStateInternal(DateTime date)
        {
            var state = new BoardStatCacheState
            {
                Date = date,
                Panels = PanelEntries.Select(kvp => new PanelEntryState
                {
                    SerialNumber = kvp.Key,
                    SideA = kvp.Value.SideA,
                    SideB = kvp.Value.SideB
                }).ToList(),
                Machines = MachineTimestamps.Select(kvp => new MachineTimestampsState
                {
                    MachineId = kvp.Key,
                    Timestamps = kvp.Value.ToList()
                }).ToList()
            };

            var path = GetStateFilePath(date);
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                var serializer = new XmlSerializer(typeof(BoardStatCacheState));
                serializer.Serialize(fs, state);
            }
        }

        private static void LoadStateInternal(DateTime date)
        {
            var path = GetStateFilePath(date);
            if (!File.Exists(path))
            {
                return;
            }

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var serializer = new XmlSerializer(typeof(BoardStatCacheState));
                var obj = serializer.Deserialize(fs) as BoardStatCacheState;
                if (obj == null)
                {
                    return;
                }

                PanelEntries.Clear();
                MachineTimestamps.Clear();
                MachineTotals.Clear();
                SerialToMachine.Clear();
                MachineLatestLotSn.Clear();

                if (obj.Panels != null)
                {
                    foreach (var p in obj.Panels)
                    {
                        if (string.IsNullOrWhiteSpace(p.SerialNumber))
                        {
                            continue;
                        }

                        var entry = new PanelStatEntry();
                        if (p.SideA != null)
                        {
                            entry.Update("A", p.SideA);
                        }
                        if (p.SideB != null)
                        {
                            entry.Update("B", p.SideB);
                        }
                        PanelEntries[p.SerialNumber] = entry;
                    }
                }

                if (obj.Machines != null)
                {
                    foreach (var m in obj.Machines)
                    {
                        if (string.IsNullOrWhiteSpace(m.MachineId))
                        {
                            continue;
                        }
                        MachineTimestamps[m.MachineId] = m.Timestamps ?? new List<DateTime>();
                    }
                }

                RecomputeTotals();
            }
        }

        public static void Update(PanelSideRecord record)
        {
            if (record == null || record.Data == null || string.IsNullOrWhiteSpace(record.SerialNumber))
            {
                return;
            }

            var today = DateTime.Today;
            if (record.DetectionDate.Date != today)
            {
                return;
            }

            lock (SyncRoot)
            {
                EnsureStateForToday();

                if (!PanelEntries.TryGetValue(record.SerialNumber, out var entry))
                {
                    entry = new PanelStatEntry();
                    PanelEntries[record.SerialNumber] = entry;
                }

                // 记录 SN -> 机台 映射
                if (!string.IsNullOrWhiteSpace(record.MachineId) && !SerialToMachine.ContainsKey(record.SerialNumber))
                {
                    SerialToMachine[record.SerialNumber] = record.MachineId;
                }
                UpdateMachineLotSn(record.MachineId, record.LotNumber, record.SerialNumber,record.ProductSerial, record.PathIndex);
                bool wasEmpty = entry.IsEmpty;
                var delta = entry.Update(record.Side, SideSnapshot.FromRecord(record));
                ApplyDelta(delta);
                if (SerialToMachine.TryGetValue(record.SerialNumber, out var mid))
                {
                    ApplyDeltaToMachine(mid, delta);
                }

                if (wasEmpty && record.AviCreationTime.HasValue && !string.IsNullOrWhiteSpace(record.MachineId))
                {
                    if (!MachineTimestamps.TryGetValue(record.MachineId, out var timestamps))
                    {
                        timestamps = new List<DateTime>();
                        MachineTimestamps[record.MachineId] = timestamps;
                    }
                    timestamps.Add(record.AviCreationTime.Value);
                }

                try { SaveStateInternal(_currentDate); } catch { /* ignore */ }
            }
        }

        public static BoardStat GetTodayStat()
        {
            lock (SyncRoot)
            {
                EnsureStateForToday();

                return new BoardStat
                {
                    AviPanelCount = _totals.AviPanelCount,
                    AviPanelOKCount = _totals.AviPanelOKCount,
                    AiPanelOKCount = _totals.AiPanelOKCount,
                    AiFilterCount = _totals.AiFilterCount,
                    AiFilterOKCount = _totals.AiFilterOKCount,
                    AiFilterUninspectedCount = _totals.AiFilterUninspectedCount,
                    Utilization = CalculateUtilization()
                };
            }
        }

        // 获取某机台今日统计
        public static BoardStat GetTodayStatForMachine(string machineId)
        {
            lock (SyncRoot)
            {
                EnsureStateForToday();
                if (string.IsNullOrWhiteSpace(machineId)) return new BoardStat();
                MachineTotals.TryGetValue(machineId, out var s);
                s = s ?? new BoardStat();
                return new BoardStat
                {
                    AviPanelCount = s.AviPanelCount,
                    AviPanelOKCount = s.AviPanelOKCount,
                    AiPanelOKCount = s.AiPanelOKCount,
                    AiFilterCount = s.AiFilterCount,
                    AiFilterOKCount = s.AiFilterOKCount,
                    AiFilterUninspectedCount = s.AiFilterUninspectedCount,
                    Utilization = CalculateUtilization(machineId)
                };
            }
        }

        // 更新某机台最新的 Lot/SN 等（供业务侧在获取时机更新）
        public static void UpdateMachineLotSn(string machineId, string lotNumber, string serialNumber, string productSerial, string pathIndex)
        {
            if (string.IsNullOrWhiteSpace(machineId)) return;
            lock (SyncRoot)
            {
                EnsureStateForToday();
                if (!MachineLatestLotSn.TryGetValue(machineId, out var s))
                {
                    s = new MachineLotSn();
                    MachineLatestLotSn[machineId] = s;
                }
                if (!string.IsNullOrWhiteSpace(lotNumber)) s.LotNumber = lotNumber;
                if (!string.IsNullOrWhiteSpace(serialNumber)) s.SerialNumber = serialNumber;
                if (!string.IsNullOrWhiteSpace(productSerial)) s.ProductSerial = productSerial;
                if (!string.IsNullOrWhiteSpace(pathIndex)) s.PathIndex = pathIndex;
            }
        }

        // 读取某机台最新 Lot/SN 信息
        public static (string LotNumber, string SerialNumber, string ProductSerial, string PathIndex) GetLatestLotSn(string machineId)
        {
            lock (SyncRoot)
            {
                EnsureStateForToday();
                if (string.IsNullOrWhiteSpace(machineId)) return (null, null, null, null);
                if (MachineLatestLotSn.TryGetValue(machineId, out MachineLotSn s) && s != null)
                {
                    return (s.LotNumber, s.SerialNumber, s.ProductSerial, s.PathIndex);
                }
                return (null, null, null, null);
            }
        }
    }
}

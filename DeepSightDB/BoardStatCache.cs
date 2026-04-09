using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DeepSightModel;
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
            /// <summary>
            /// 该面中重点缺陷的数量
            /// </summary>
            public int KeyDefectPoints { get; set; }

            public static SideSnapshot FromRecord(PanelSideRecord record)
            {
                var points = record.Data?.DetectPoints ?? new List<DetectInfo>();
                return new SideSnapshot
                {
                    AviState = record.Data?.AviState ?? 0,
                    AiState = record.Data?.AiState ?? 0,
                    TotalPoints = points.Count,
                    AiOkPoints = points.Count(p => p.AIStatus == 1),
                    UninspectedPoints = points.Count(p => p.AIStatus == 3),
                    KeyDefectPoints = points.Count(p => p.IsKeyDefect)
                };
            }
        }

        private sealed class PanelStatEntry
        {
            private SideSnapshot _sideA;
            private SideSnapshot _sideB;
            private BoardStat _contribution = new BoardStat();
            
            // 记录该面板首次被记录的小时
            public int Hour { get; set; } = -1;

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

                int keyDefectTotal = 0;

                if (_sideA != null)
                {
                    result.AiFilterCount += _sideA.TotalPoints;
                    result.AiFilterOKCount += _sideA.AiOkPoints;
                    keyDefectTotal += _sideA.KeyDefectPoints;
                    if (_sideA.AiState == 3)
                    {
                        result.AiFilterUninspectedCount += _sideA.UninspectedPoints;
                    }
                }

                if (_sideB != null)
                {
                    result.AiFilterCount += _sideB.TotalPoints;
                    result.AiFilterOKCount += _sideB.AiOkPoints;
                    keyDefectTotal += _sideB.KeyDefectPoints;
                    if (_sideB.AiState == 3)
                    {
                        result.AiFilterUninspectedCount += _sideB.UninspectedPoints;
                    }
                }

                result.KeyDefectCount = keyDefectTotal;
                if (keyDefectTotal > 0) result.KeyDefectPanelCount = 1;

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
            public int Hour { get; set; }
            public List<PanelEntryState> Panels { get; set; }
            public List<MachineTimestampsState> Machines { get; set; }
        }

        public class PanelEntryState
        {
            public string SerialNumber { get; set; }
            public SideSnapshot SideA { get; set; }
            public SideSnapshot SideB { get; set; }
            public int Hour { get; set; }
        }

        public class MachineTimestampsState
        {
            public string MachineId { get; set; }
            public List<DateTime> Timestamps { get; set; }
        }

        private static readonly object SyncRoot = new object();
        private static readonly Dictionary<string, PanelStatEntry> PanelEntries = new Dictionary<string, PanelStatEntry>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, List<DateTime>> MachineTimestamps = new Dictionary<string, List<DateTime>>(StringComparer.OrdinalIgnoreCase);
        // 每台机台单独的统计
        private static readonly Dictionary<string, BoardStat> MachineTotals = new Dictionary<string, BoardStat>(StringComparer.OrdinalIgnoreCase);
        // SN -> 机台 的映射（用于将面板统计归属到机台）
        private static readonly Dictionary<string, string> SerialToMachine = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // 每台机台最新的 Lot/SN 等
        private sealed class MachineLotSn
        {
            public string LotNumber { get; set; }
            public string SerialNumber { get; set; }
            public string ProductSerial { get; set; }
        }
        private static readonly Dictionary<string, MachineLotSn> MachineLatestLotSn = new Dictionary<string, MachineLotSn>(StringComparer.OrdinalIgnoreCase);

        private const int SaveBatchSize = 100;
        private static int _pendingSaveCount = 0;

        private static BoardStat _totals = new BoardStat();
        private static DateTime _currentDate = DateTime.Today;
        private static bool _stateLoadedForDate = false;

        static BoardStatCache()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                try
                {
                    lock (SyncRoot)
                    {
                        SaveStateIfNeeded(force: true);
                    }
                }
                catch
                {
                    // ignore
                }
            };
        }

        /// <summary>
        /// 清空所有缓存数据（清空数据库时调用）
        /// </summary>
        public static void Clear()
        {
            lock (SyncRoot)
            {
                PanelEntries.Clear();
                MachineTimestamps.Clear();
                MachineTotals.Clear();
                SerialToMachine.Clear();
                MachineLatestLotSn.Clear();
                _totals = new BoardStat();
                _pendingSaveCount = 0;
                _stateLoadedForDate = true; // 防止重新加载旧状态
                LogTextHelper.Info("BoardStatCache 已清空");
            }
        }

        private static void EnsureCurrentDate()
        {
            var today = DateTime.Today;
            if (today == _currentDate)
            {
                return;
            }

            // Optionally persist previous date before reset
            try { SaveStateInternal(_currentDate); _pendingSaveCount = 0; } catch { /* ignore */ }

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
            _totals.KeyDefectCount += delta.KeyDefectCount;
            _totals.KeyDefectPanelCount += delta.KeyDefectPanelCount;
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
            s.KeyDefectCount += delta.KeyDefectCount;
            s.KeyDefectPanelCount += delta.KeyDefectPanelCount;
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

        private static string GetStateFilePath(DateTime date, int hour)
        {
            return Path.Combine(GetStateDirectory(), date.ToString("yyyyMMdd") + "_" + hour.ToString("D2") + ".xml");
        }

        private static void SaveStateInternal(DateTime date)
        {
            var currentHour = DateTime.Now.Hour;
            
            // 只保存属于当前小时的面板数据
            var panelsForCurrentHour = PanelEntries
                .Where(kvp => kvp.Value.Hour == currentHour)
                .Select(kvp => new PanelEntryState
                {
                    SerialNumber = kvp.Key,
                    SideA = kvp.Value.SideA,
                    SideB = kvp.Value.SideB,
                    Hour = kvp.Value.Hour
                }).ToList();
            
            // 只保存当前小时的机台时间戳
            var machinesForCurrentHour = MachineTimestamps
                .Select(kvp => new MachineTimestampsState
                {
                    MachineId = kvp.Key,
                    Timestamps = kvp.Value.Where(t => t.Hour == currentHour && t.Date == date.Date).ToList()
                })
                .Where(m => m.Timestamps.Count > 0)
                .ToList();

            var state = new BoardStatCacheState
            {
                Date = date,
                Hour = currentHour,
                Panels = panelsForCurrentHour,
                Machines = machinesForCurrentHour
            };

            var path = GetStateFilePath(date, currentHour);
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                var serializer = new XmlSerializer(typeof(BoardStatCacheState));
                serializer.Serialize(fs, state);
            }
        }

        private static void SaveStateIfNeeded(bool force = false)
        {
            _pendingSaveCount++;
            if (!force && _pendingSaveCount < SaveBatchSize)
            {
                return;
            }

            try { SaveStateInternal(_currentDate); }
            catch (Exception ex) { LogTextHelper.Error($"[BoardStatCache] SaveStateInternal 失败: {ex.Message}"); }
            _pendingSaveCount = 0;
        }

        /// <summary>
        /// 强制将当前缓存数据写入 XML 文件（停止作业或释放资源时调用）
        /// </summary>
        public static void Flush()
        {
            lock (SyncRoot)
            {
                if (_pendingSaveCount > 0)
                {
                    try
                    {
                        SaveStateInternal(_currentDate);
                        LogTextHelper.Info($"[BoardStatCache] Flush 完成，已保存 {PanelEntries.Count} 条面板数据");
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"[BoardStatCache] Flush 失败: {ex.Message}");
                    }
                    _pendingSaveCount = 0;
                }
            }
        }

        private static void LoadStateInternal(DateTime date)
        {
            PanelEntries.Clear();
            MachineTimestamps.Clear();
            MachineTotals.Clear();
            SerialToMachine.Clear();
            MachineLatestLotSn.Clear();

            // 加载当天所有小时的文件
            for (int hour = 0; hour < 24; hour++)
            {
                var path = GetStateFilePath(date, hour);
                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var serializer = new XmlSerializer(typeof(BoardStatCacheState));
                        if (!(serializer.Deserialize(fs) is BoardStatCacheState obj))
                        {
                            continue;
                        }

                        // 合并面板数据
                        if (obj.Panels != null)
                        {
                            foreach (var p in obj.Panels)
                            {
                                if (string.IsNullOrWhiteSpace(p.SerialNumber))
                                {
                                    continue;
                                }

                                // 如果已存在，合并数据；否则创建新条目
                                if (!PanelEntries.TryGetValue(p.SerialNumber, out var entry))
                                {
                                    entry = new PanelStatEntry();
                                    entry.Hour = p.Hour > 0 ? p.Hour : hour;
                                    PanelEntries[p.SerialNumber] = entry;
                                }
                                
                                if (p.SideA != null)
                                {
                                    entry.Update("A", p.SideA);
                                }
                                if (p.SideB != null)
                                {
                                    entry.Update("B", p.SideB);
                                }
                            }
                        }

                        // 合并机台时间戳
                        if (obj.Machines != null)
                        {
                            foreach (var m in obj.Machines)
                            {
                                if (string.IsNullOrWhiteSpace(m.MachineId))
                                {
                                    continue;
                                }

                                if (!MachineTimestamps.TryGetValue(m.MachineId, out var timestamps))
                                {
                                    timestamps = new List<DateTime>();
                                    MachineTimestamps[m.MachineId] = timestamps;
                                }

                                // 只添加当天的时间戳，过滤掉非当前日期的数据
                                var todayTimestamps = (m.Timestamps ?? new List<DateTime>())
                                    .Where(t => t.Date == date.Date);
                                timestamps.AddRange(todayTimestamps);
                            }
                        }
                    }
                }
                catch
                {
                    // 忽略单个文件的加载失败，继续处理其他小时的文件
                }
            }

            // 对每个机台的时间戳去重并排序
            foreach (var kvp in MachineTimestamps.ToList())
            {
                MachineTimestamps[kvp.Key] = kvp.Value.Distinct().OrderBy(t => t).ToList();
            }

            RecomputeTotals();
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

                var currentHour = DateTime.Now.Hour;
                bool isNewEntry = false;

                if (!PanelEntries.TryGetValue(record.SerialNumber, out var entry))
                {
                    entry = new PanelStatEntry();
                    entry.Hour = currentHour;
                    PanelEntries[record.SerialNumber] = entry;
                    isNewEntry = true;
                }

                // 记录 SN -> 机台 映射
                if (!string.IsNullOrWhiteSpace(record.MachineId) && !SerialToMachine.ContainsKey(record.SerialNumber))
                {
                    SerialToMachine[record.SerialNumber] = record.MachineId;
                }
                UpdateMachineLotSn(record.MachineId, record.LotNumber, record.SerialNumber, record.ProductSerial);
                bool wasEmpty = entry.IsEmpty;
                var delta = entry.Update(record.Side, SideSnapshot.FromRecord(record));
                ApplyDelta(delta);
                if (SerialToMachine.TryGetValue(record.SerialNumber, out var mid))
                {
                    ApplyDeltaToMachine(mid, delta);
                }

                // 诊断日志：记录每次更新的关键信息
                if (isNewEntry || delta.AviPanelCount != 0)
                {
                    LogTextHelper.Info($"[BoardStatCache] Update SN={record.SerialNumber}, Side={record.Side}, " +
                        $"IsNew={isNewEntry}, Delta.AviPanelCount={delta.AviPanelCount}, " +
                        $"Total.AviPanelCount={_totals.AviPanelCount}, PanelEntries.Count={PanelEntries.Count}");
                }

                if (wasEmpty && record.AviCreationTime.HasValue && !string.IsNullOrWhiteSpace(record.MachineId))
                {
                    var aviTime = record.AviCreationTime.Value;
                    // 只记录当天的时间戳，非当天的不记录
                    if (aviTime.Date == today)
                    {
                        if (!MachineTimestamps.TryGetValue(record.MachineId, out var timestamps))
                        {
                            timestamps = new List<DateTime>();
                            MachineTimestamps[record.MachineId] = timestamps;
                        }
                        timestamps.Add(aviTime);
                    }
                }

                SaveStateIfNeeded();
            }
        }

        private static int _integrityCheckCounter = 0;

        public static BoardStat GetTodayStat()
        {
            lock (SyncRoot)
            {
                EnsureStateForToday();

                // 每50次查询做一次完整性校验，防止_totals与实际数据不一致
                _integrityCheckCounter++;
                if (_integrityCheckCounter >= 50)
                {
                    _integrityCheckCounter = 0;
                    var verifyTotal = new BoardStat();
                    foreach (var kv in PanelEntries)
                    {
                        var c = kv.Value.GetContribution();
                        verifyTotal.AviPanelCount += c.AviPanelCount;
                        verifyTotal.AiFilterCount += c.AiFilterCount;
                    }
                    if (verifyTotal.AviPanelCount != _totals.AviPanelCount ||
                        verifyTotal.AiFilterCount != _totals.AiFilterCount)
                    {
                        LogTextHelper.Warn($"[BoardStatCache] 完整性校验发现偏差! " +
                            $"_totals.AviPanelCount={_totals.AviPanelCount} vs Verify={verifyTotal.AviPanelCount}, " +
                            $"_totals.AiFilterCount={_totals.AiFilterCount} vs Verify={verifyTotal.AiFilterCount}, " +
                            $"PanelEntries.Count={PanelEntries.Count}. 正在自动修复...");
                        RecomputeTotals();
                    }
                }

                return new BoardStat
                {
                    AviPanelCount = _totals.AviPanelCount,
                    AviPanelOKCount = _totals.AviPanelOKCount,
                    AiPanelOKCount = _totals.AiPanelOKCount,
                    AiFilterCount = _totals.AiFilterCount,
                    AiFilterOKCount = _totals.AiFilterOKCount,
                    AiFilterUninspectedCount = _totals.AiFilterUninspectedCount,
                    Utilization = CalculateUtilization(),
                    KeyDefectCount = _totals.KeyDefectCount,
                    KeyDefectPanelCount = _totals.KeyDefectPanelCount
                };
            }
        }

        // 获取某机台当天统计
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
                    Utilization = CalculateUtilization(machineId),
                    KeyDefectCount = s.KeyDefectCount,
                    KeyDefectPanelCount = s.KeyDefectPanelCount
                };
            }
        }

        // 更新某机台最新的 Lot/SN 等（在业务逻辑获取时更新）
        public static void UpdateMachineLotSn(string machineId, string lotNumber, string serialNumber, string productSerial)
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
            }
        }

        // 获取某机台最新 Lot/SN 信息
        public static (string LotNumber, string SerialNumber, string ProductSerial) GetLatestLotSn(string machineId)
        {
            lock (SyncRoot)
            {
                EnsureStateForToday();
                if (string.IsNullOrWhiteSpace(machineId)) return (null, null, null);
                if (MachineLatestLotSn.TryGetValue(machineId, out MachineLotSn s) && s != null)
                {
                    return (s.LotNumber, s.SerialNumber, s.ProductSerial);
                }
                return (null, null, null);
            }
        }
    }
}

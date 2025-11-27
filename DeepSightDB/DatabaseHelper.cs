using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeepsightSqlite
{
    public class DatabaseHelper : IDisposable
    {
        private static readonly string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "deepsight.db");
        private static readonly string connectionString = $"Data Source={dbPath};Version=3;";
        private readonly BlockingCollection<Action<SQLiteConnection>> _dbQueue = new BlockingCollection<Action<SQLiteConnection>>();
        private readonly Thread _dbThread;
        private bool _disposed = false;

        public DatabaseHelper()
        {
            _dbThread = new Thread(ProcessQueue)
            {
                IsBackground = true,
                Name = "DatabaseThread"
            };
            _dbThread.Start();
        }

        private void ProcessQueue()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                foreach (var action in _dbQueue.GetConsumingEnumerable())
                {
                    if (_disposed) break;
                    try
                    {
                        action(connection);
                    }
                    catch (Exception ex)
                    {
                        // It's important to log exceptions from the queue
                        LogTextHelper.Error($"Exception in database queue: {ex}");
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _dbQueue.CompleteAdding();
                _dbThread.Join();
                _dbQueue.Dispose();
            }
        }

        public static void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createPanelsTable = @"
                CREATE TABLE IF NOT EXISTS Panels (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MachineId TEXT NOT NULL,
                    SerialNumber TEXT NOT NULL UNIQUE,
                    LotNumber TEXT NOT NULL,
                    ProductSerial TEXT,
                    DetectionDate DATETIME NOT NULL,
                    PathIndex TEXT,
                    AviCreationTime DATETIME
                );";

                string createPanelSidesTable = @"
                CREATE TABLE IF NOT EXISTS PanelSides (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PanelId INTEGER NOT NULL,
                    Side TEXT NOT NULL, -- 'A' 或 'B'
                    TotalDefectsCount INTEGER NOT NULL,
                    RemainingDefectsCount INTEGER NOT NULL,
                    HeatPoints TEXT, -- 存储 HeatPoint 列表的 JSON 字符串
                    AviState INTEGER DEFAULT 0, -- 0: 未运行, 1: OK, 2: NG
                    AiState INTEGER DEFAULT 0,  -- 0: 未运行, 1: OK, 2: NG
                    VvsState INTEGER DEFAULT 0, -- 0: 未运行, 1: OK, 2: NG
                    VrsState INTEGER DEFAULT 0, -- 0: 未运行, 1: OK, 2: NG
                    FinalState INTEGER DEFAULT 0, -- 0: 待处理, 1: 最终OK, 2: 最终NG
                    FOREIGN KEY (PanelId) REFERENCES Panels(Id) ON DELETE CASCADE
                );";


                string createEmployeeReportsTable = @"
                CREATE TABLE IF NOT EXISTS EmployeeReports (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EmployeeID TEXT NOT NULL,
                    SN TEXT NOT NULL,
                    AllNGNumber INTEGER NOT NULL,
                    StartTime DATETIME NOT NULL,
                    EndTime DATETIME NOT NULL,
                    VRSOKNumber INTEGER NOT NULL
                );";

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = createPanelsTable;
                    command.ExecuteNonQuery();
                    command.CommandText = createPanelSidesTable;
                    command.ExecuteNonQuery();
                    command.CommandText = createEmployeeReportsTable;
                    command.ExecuteNonQuery();

                }
            }
        }

        /// <summary>
        /// 存储单面数据
        /// </summary>
        public void SavePanelSide(PanelSideRecord record)
        {
            _dbQueue.Add(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    long panelId;

                    // 1. 查找或创建 Panel 记录
                    using (var cmd = new SQLiteCommand("SELECT Id FROM Panels WHERE SerialNumber = @SN", connection))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@SN", record.SerialNumber);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            panelId = (long)result;
                        }
                        else
                        {
                            var insertPanelCmd = new SQLiteCommand(
                                "INSERT INTO Panels (MachineId, SerialNumber, LotNumber, DetectionDate, ProductSerial, PathIndex, AviCreationTime) VALUES (@MachineId, @SN, @Lot, @Date, @ProductSerial, @PathIndex, @AviCreationTime); SELECT last_insert_rowid();",
                                connection, transaction);
                            insertPanelCmd.Parameters.AddWithValue("@MachineId", record.MachineId);
                            insertPanelCmd.Parameters.AddWithValue("@SN", record.SerialNumber);
                            insertPanelCmd.Parameters.AddWithValue("@Lot", record.LotNumber);
                            insertPanelCmd.Parameters.AddWithValue("@Date", record.DetectionDate);
                            insertPanelCmd.Parameters.AddWithValue("@ProductSerial", record.ProductSerial);
                            insertPanelCmd.Parameters.AddWithValue("@PathIndex", record.PathIndex);
                            insertPanelCmd.Parameters.AddWithValue("@AviCreationTime", record.AviCreationTime);
                            panelId = (long)insertPanelCmd.ExecuteScalar();
                        }
                    }

                    // 2. 检查该面数据是否已存在，如果存在则更新，否则插入
                    long? existingSideId = null;
                    using (var checkSideCmd = new SQLiteCommand("SELECT Id FROM PanelSides WHERE PanelId = @PanelId AND Side = @Side", connection, transaction))
                    {
                        checkSideCmd.Parameters.AddWithValue("@PanelId", panelId);
                        checkSideCmd.Parameters.AddWithValue("@Side", record.Side);
                        var sideResult = checkSideCmd.ExecuteScalar();
                        if (sideResult != null)
                        {
                            existingSideId = (long)sideResult;
                        }
                    }

                    if (existingSideId.HasValue)
                    {
                        // 更新现有的面数据
                        var updateSideCmd = new SQLiteCommand(
                            "UPDATE PanelSides SET TotalDefectsCount = @Total, RemainingDefectsCount = @Remaining, HeatPoints = @HeatPoints, AviState = @AviState, AiState = @AiState, VvsState = @VvsState, VrsState = @VrsState, FinalState = @FinalState WHERE Id = @Id",
                            connection, transaction);
                        updateSideCmd.Parameters.AddWithValue("@Total", record.Data.TotalDefectsCount);
                        updateSideCmd.Parameters.AddWithValue("@Remaining", record.Data.RemainingDefectsCount);
                        updateSideCmd.Parameters.AddWithValue("@HeatPoints", JsonConvert.SerializeObject(record.Data.HeatPoints));
                        updateSideCmd.Parameters.AddWithValue("@AviState", record.Data.AviState);
                        updateSideCmd.Parameters.AddWithValue("@AiState", record.Data.AiState);
                        updateSideCmd.Parameters.AddWithValue("@VvsState", record.Data.VvsState);
                        updateSideCmd.Parameters.AddWithValue("@VrsState", record.Data.VrsState);
                        updateSideCmd.Parameters.AddWithValue("@FinalState", record.Data.FinalState);
                        updateSideCmd.Parameters.AddWithValue("@Id", existingSideId.Value);
                        updateSideCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        // 插入新的面数据
                        var insertSideCmd = new SQLiteCommand(
                            "INSERT INTO PanelSides (PanelId, Side, TotalDefectsCount, RemainingDefectsCount, HeatPoints, AviState, AiState, VvsState, VrsState, FinalState) VALUES (@PanelId, @Side, @Total, @Remaining, @HeatPoints, @AviState, @AiState, @VvsState, @VrsState, @FinalState)",
                            connection, transaction);
                        insertSideCmd.Parameters.AddWithValue("@PanelId", panelId);
                        insertSideCmd.Parameters.AddWithValue("@Side", record.Side);
                        insertSideCmd.Parameters.AddWithValue("@Total", record.Data.TotalDefectsCount);
                        insertSideCmd.Parameters.AddWithValue("@Remaining", record.Data.RemainingDefectsCount);
                        insertSideCmd.Parameters.AddWithValue("@HeatPoints", JsonConvert.SerializeObject(record.Data.HeatPoints));
                        insertSideCmd.Parameters.AddWithValue("@AviState", record.Data.AviState);
                        insertSideCmd.Parameters.AddWithValue("@AiState", record.Data.AiState);
                        insertSideCmd.Parameters.AddWithValue("@VvsState", record.Data.VvsState);
                        insertSideCmd.Parameters.AddWithValue("@VrsState", record.Data.VrsState);
                        insertSideCmd.Parameters.AddWithValue("@FinalState", record.Data.FinalState);
                        insertSideCmd.ExecuteNonQuery();
                    }

                    // 3. (移除 IsAIOk 列逻辑) 不再更新 Panels.IsAIOk，统计时动态计算

                    transaction.Commit();
                }
            });
        }

        // 查询逻辑 1: 根据 lot 号获取所有 sn
        public Task<List<string>> GetSerialNumbersByLot(string lotNumber)
        {
            var tcs = new TaskCompletionSource<List<string>>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var serialNumbers = new List<string>();
                    using (var cmd = new SQLiteCommand("SELECT SerialNumber FROM Panels WHERE LotNumber = @Lot", connection))
                    {
                        cmd.Parameters.AddWithValue("@Lot", lotNumber);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                serialNumbers.Add(reader.GetString(0));
                            }
                        }
                    }
                    tcs.SetResult(serialNumbers);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        // 查询逻辑 2: 根据时间和 sn 获取所有 heatpoint 信息
        public Task<List<HeatPoint>> GetHeatPoints(string serialNumber, DateTime detectionDate)
        {
            var tcs = new TaskCompletionSource<List<HeatPoint>>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var heatPoints = new List<HeatPoint>();
                    using (var cmd = new SQLiteCommand(
                        "SELECT ps.HeatPoints FROM PanelSides ps JOIN Panels p ON ps.PanelId = p.Id WHERE p.SerialNumber = @SN AND date(p.DetectionDate) = date(@Date)",
                        connection))
                    {
                        cmd.Parameters.AddWithValue("@SN", serialNumber);
                        cmd.Parameters.AddWithValue("@Date", detectionDate.Date);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                {
                                    var heatPointsJson = reader.GetString(0);
                                    var points = JsonConvert.DeserializeObject<List<HeatPoint>>(heatPointsJson);
                                    if (points != null)
                                    {
                                        heatPoints.AddRange(points);
                                    }
                                }
                            }
                        }
                    }
                    tcs.SetResult(heatPoints);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        // 查询逻辑 3 & 5 的组合: 获取机台在时间段内的板数统计
        public Task<(int TotalBoards, int AIOkBoards)> GetBoardCounts(DateTime start, DateTime end, string machineId = null)
        {
            var tcs = new TaskCompletionSource<(int, int)>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    // 总板数按 Panels 计数；AI OK 板数为两面 AiState 都为 1 的 Panel
                    var totalSql = "SELECT COUNT(*) FROM Panels WHERE DetectionDate BETWEEN @Start AND @End" + (string.IsNullOrEmpty(machineId) ? string.Empty : " AND MachineId = @MachineId");
                    int totalBoards = 0;
                    using (var totalCmd = new SQLiteCommand(totalSql, connection))
                    {
                        totalCmd.Parameters.AddWithValue("@Start", start);
                        totalCmd.Parameters.AddWithValue("@End", end);
                        if (!string.IsNullOrEmpty(machineId)) totalCmd.Parameters.AddWithValue("@MachineId", machineId);
                        totalBoards = Convert.ToInt32(totalCmd.ExecuteScalar() ?? 0);
                    }

                    var aiOkSql = @"
                        SELECT COUNT(*) FROM (
                          SELECT p.Id
                          FROM Panels p
                          JOIN PanelSides ps ON p.Id = ps.PanelId
                          WHERE p.DetectionDate BETWEEN @Start AND @End" + (string.IsNullOrEmpty(machineId) ? string.Empty : " AND p.MachineId = @MachineId") + @"
                          GROUP BY p.Id
                          HAVING COUNT(ps.Id) = 2 AND SUM(CASE WHEN ps.AiState = 1 THEN 1 ELSE 0 END) = 2
                        ) t";
                    int aiOkBoards = 0;
                    using (var aiOkCmd = new SQLiteCommand(aiOkSql, connection))
                    {
                        aiOkCmd.Parameters.AddWithValue("@Start", start);
                        aiOkCmd.Parameters.AddWithValue("@End", end);
                        if (!string.IsNullOrEmpty(machineId)) aiOkCmd.Parameters.AddWithValue("@MachineId", machineId);
                        aiOkBoards = Convert.ToInt32(aiOkCmd.ExecuteScalar() ?? 0);
                    }
                    tcs.SetResult((totalBoards, aiOkBoards));
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        // 查询逻辑 4 & 6 的组合: 获取机台在时间段内的报点数统计
        public Task<(long TotalDefects, long AIOkDefects)> GetDefectCounts(DateTime start, DateTime end, string machineId = null)
        {
            var tcs = new TaskCompletionSource<(long, long)>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var sql = @"
                        SELECT 
                          SUM(ps.TotalDefectsCount) AS TotalDefects,
                          SUM(CASE WHEN both.AiOkBothSides = 1 THEN ps.TotalDefectsCount ELSE 0 END) AS AIOkDefects
                        FROM PanelSides ps
                        JOIN Panels p ON ps.PanelId = p.Id
                        LEFT JOIN (
                          SELECT PanelId,
                                 CASE WHEN COUNT(*) = 2 AND SUM(CASE WHEN AiState = 1 THEN 1 ELSE 0 END) = 2 THEN 1 ELSE 0 END AS AiOkBothSides
                          FROM PanelSides
                          GROUP BY PanelId
                        ) both ON ps.PanelId = both.PanelId
                        WHERE p.DetectionDate BETWEEN @Start AND @End" + (string.IsNullOrEmpty(machineId) ? string.Empty : " AND p.MachineId = @MachineId");

                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);
                        if (!string.IsNullOrEmpty(machineId)) cmd.Parameters.AddWithValue("@MachineId", machineId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                long totalDefects = reader.IsDBNull(0) ? 0 : Convert.ToInt64(reader.GetValue(0));
                                long aiOkDefects = reader.IsDBNull(1) ? 0 : Convert.ToInt64(reader.GetValue(1));
                                tcs.SetResult((totalDefects, aiOkDefects));
                            }
                            else
                            {
                                tcs.SetResult((0, 0));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }


        public void SaveEmployeeReport(EmployeeReport report)
        {
            _dbQueue.Add(connection =>
            {
                var sql = "INSERT INTO EmployeeReports (EmployeeID, SN, AllNGNumber, StartTime, EndTime, VRSOKNumber) VALUES (@ID, @SN, @AllNGNumber, @StartTime, @EndTime, @VRSOKNumber)";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ID", report.ID);
                    cmd.Parameters.AddWithValue("@SN", report.SN);
                    cmd.Parameters.AddWithValue("@AllNGNumber", report.AllNGNumber);
                    cmd.Parameters.AddWithValue("@StartTime", report.StartTime);
                    cmd.Parameters.AddWithValue("@EndTime", report.EndTime);
                    cmd.Parameters.AddWithValue("@VRSOKNumber", report.VRSOKNumber);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public Task<List<EmployeeReport>> GetEmployeeReports(DateTime start, DateTime end)
        {
            var tcs = new TaskCompletionSource<List<EmployeeReport>>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var reports = new List<EmployeeReport>();
                    var sql = "SELECT EmployeeID, SN, AllNGNumber, StartTime, EndTime, VRSOKNumber FROM EmployeeReports WHERE StartTime >= @Start AND EndTime <= @End";
                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reports.Add(new EmployeeReport
                                {
                                    ID = reader.GetString(0),
                                    SN = reader.GetString(1),
                                    AllNGNumber = reader.GetInt32(2),
                                    StartTime = reader.GetDateTime(3),
                                    EndTime = reader.GetDateTime(4),
                                    VRSOKNumber = reader.GetInt32(5)
                                });
                            }
                        }
                    }
                    tcs.SetResult(reports);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        /// <summary>
        /// 获取数据库中所有唯一的 MachineId
        /// </summary>
        public Task<List<string>> GetAllMachineIds()
        {
            var tcs = new TaskCompletionSource<List<string>>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var machineIds = new List<string>();
                    using (var cmd = new SQLiteCommand("SELECT DISTINCT MachineId FROM Panels", connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                machineIds.Add(reader.GetString(0));
                            }
                        }
                    }
                    tcs.SetResult(machineIds);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        /// <summary>
        /// 获取每个机台在时间段内的报点数统计
        /// </summary>
        public async Task<Dictionary<string, (long TotalDefects, long AIOkDefects)>> GetDefectCountsPerMachine(DateTime start, DateTime end)
        {
            var results = new Dictionary<string, (long TotalDefects, long AIOkDefects)>();
            var machineIds = await GetAllMachineIds();

            foreach (var machineId in machineIds)
            {
                var counts = await GetDefectCounts(start, end, machineId);
                if (counts.TotalDefects > 0) // 只添加有数据的机台
                {
                    results[machineId] = counts;
                }
            }
            return results;
        }

        /// <summary>
        /// 获取一个时间段内所有的 DetectionDate
        /// </summary>
        public Task<List<DateTime>> GetDetectionDates(DateTime start, DateTime end)
        {
            var tcs = new TaskCompletionSource<List<DateTime>>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var detectionDates = new List<DateTime>();
                    using (var cmd = new SQLiteCommand("SELECT DISTINCT DetectionDate FROM Panels WHERE DetectionDate BETWEEN @Start AND @End", connection))
                    {
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                detectionDates.Add(reader.GetDateTime(0));
                            }
                        }
                    }
                    tcs.SetResult(detectionDates);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        /// <summary>
        /// 根据 machineID 获取最新的 SN 和 Lot
        /// </summary>
        /// <param name="machineId">机器ID</param>
        /// <returns>最新的 SN 和 Lot</returns>
        public Task<(string SerialNumber, string LotNumber, string ProductSerial, string PathIndex)> GetLatestPanelInfoByMachineId(string machineId)
        {
            var tcs = new TaskCompletionSource<(string, string, string, string)>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    using (var cmd = new SQLiteCommand("SELECT SerialNumber, LotNumber, ProductSerial, PathIndex, AviCreationTime FROM Panels WHERE MachineId = @MachineId ORDER BY DetectionDate DESC LIMIT 1", connection))
                    {
                        cmd.Parameters.AddWithValue("@MachineId", machineId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string serialNumber = reader.GetString(0);
                                string lotNumber = reader.GetString(1);
                                string ProductSerial = reader.IsDBNull(2) ? null : reader.GetString(2);
                                string pathIndex = reader.IsDBNull(3) ? null : reader.GetString(3);
                                tcs.SetResult((serialNumber, lotNumber, ProductSerial, pathIndex));
                            }
                            else
                            {
                                tcs.SetResult((null, null, null, null));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    LogTextHelper.Warn($"Error in GetLatestPanelInfoByMachineId: {ex.Message}");
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        /// <summary>
        /// 生成测试数据
        /// </summary>
        public static void GenerateTestData()
        {
            var dbHelper = new DatabaseHelper();
            var random = new Random();
            var startDate = DateTime.Now.AddMonths(-3);
            var endDate = DateTime.Now;

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                int numberOfEntries = random.Next(50, 101);
                for (int i = 0; i < numberOfEntries; i++)
                {
                    string machineId = $"Machine-{random.Next(1, 4)}";
                    string lotNumber = $"Lot-{date:yyyyMMdd}";
                    string serialNumber = $"{lotNumber}-SN{i:D3}";
                    string ProductSerial = $"ProductSerial-{random.Next(1, 5)}";
                    string pathIndex = $"Path/Index/{Guid.NewGuid().ToString().Substring(0, 8)}";
                    DateTime detectionDate = date.AddHours(random.Next(0, 24)).AddMinutes(random.Next(0, 60));
                    DateTime? aviCreationTime = detectionDate.AddSeconds(-random.Next(30, 300));

                    // Side A
                    var sideAData = new SideData
                    {
                        TotalDefectsCount = random.Next(0, 10),
                        HeatPoints = new List<HeatPoint>()
                    };
                    sideAData.RemainingDefectsCount = random.Next(0, sideAData.TotalDefectsCount + 1);

                    // 根据缺陷数生成 State
                    if (sideAData.TotalDefectsCount == 0)
                    {
                        sideAData.AviState = 1; // AVI OK
                        sideAData.AiState = 1; // AI 默认也 OK
                        sideAData.FinalState = 1; // 最终 OK
                    }
                    else
                    {
                        sideAData.AviState = 2; // AVI NG
                        // 模拟AI处理
                        if (sideAData.RemainingDefectsCount == 0)
                        {
                            sideAData.AiState = 1; // AI OK
                            sideAData.FinalState = 1; // 最终 OK
                        }
                        else
                        {
                            sideAData.AiState = 2; // AI NG
                            // 模拟VVS/VRS
                            if (random.Next(0, 2) == 0) // 50% 概率 VVS/VRS OK
                            {
                                sideAData.VvsState = 1;
                                sideAData.VrsState = 1;
                                sideAData.FinalState = 1; // 最终 OK
                            }
                            else
                            {
                                sideAData.VvsState = 2;
                                sideAData.VrsState = 2;
                                sideAData.FinalState = 2; // 最终 NG
                            }
                        }
                    }


                    for (int j = 0; j < sideAData.TotalDefectsCount; j++)
                    {
                        sideAData.HeatPoints.Add(new HeatPoint());
                    }

                    var recordA = new PanelSideRecord
                    {
                        MachineId = machineId,
                        DetectionDate = detectionDate,
                        SerialNumber = serialNumber,
                        LotNumber = lotNumber,
                        ProductSerial = ProductSerial,
                        PathIndex = pathIndex,
                        Side = "A",
                        Data = sideAData,
                        AviCreationTime = aviCreationTime
                    };
                    dbHelper.SavePanelSide(recordA);

                    // Side B
                    var sideBData = new SideData
                    {
                        TotalDefectsCount = random.Next(0, 10),
                        HeatPoints = new List<HeatPoint>()
                    };
                    sideBData.RemainingDefectsCount = random.Next(0, sideBData.TotalDefectsCount + 1);

                    // 根据缺陷数生成 State
                    if (sideBData.TotalDefectsCount == 0)
                    {
                        sideBData.AviState = 1; // AVI OK
                        sideBData.AiState = 1; // AI 默认也 OK
                        sideBData.FinalState = 1; // 最终 OK
                    }
                    else
                    {
                        sideBData.AviState = 2; // AVI NG
                        // 模拟AI处理
                        if (sideBData.RemainingDefectsCount == 0)
                        {
                            sideBData.AiState = 1; // AI OK
                            sideBData.FinalState = 1; // 最终 OK
                        }
                        else
                        {
                            sideBData.AiState = 2; // AI NG
                            // 模拟VVS/VRS
                            if (random.Next(0, 2) == 0) // 50% 概率 VVS/VRS OK
                            {
                                sideBData.VvsState = 1;
                                sideBData.VrsState = 1;
                                sideBData.FinalState = 1; // 最终 OK
                            }
                            else
                            {
                                sideBData.VvsState = 2;
                                sideBData.VrsState = 2;
                                sideBData.FinalState = 2; // 最终 NG
                            }
                        }
                    }

                    for (int j = 0; j < sideBData.TotalDefectsCount; j++)
                    {
                        sideBData.HeatPoints.Add(new HeatPoint());
                    }

                    var recordB = new PanelSideRecord
                    {
                        MachineId = machineId,
                        DetectionDate = detectionDate,
                        SerialNumber = serialNumber,
                        LotNumber = lotNumber,
                        ProductSerial = ProductSerial,
                        PathIndex = pathIndex,
                        Side = "B",
                        Data = sideBData,
                        AviCreationTime = aviCreationTime
                    };
                    dbHelper.SavePanelSide(recordB);
                }
            }
        }
        /// <summary>
        /// 1.	如果有一个state为3，则添加到未检测结果数
        /// 2.	如果有一个state为2，则添加到过滤后仍NG结果数
        /// 3.	如果有两个state为0，则添加到AVI OK的结果数
        /// 4.	剩余情况，添加到过滤后OK的数 所以总共获取5个数字
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public Task<(int totalSnCount, int uninspectedCount, int stillNgCount, int aviOkCount, int filteredOkCount)> GetSnStateCounts(DateTime start, DateTime end)
        {
            var tcs = new TaskCompletionSource<(int, int, int, int, int)>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var snStates = new Dictionary<string, List<int?>>();
                    var sql = @"
                    SELECT p.SerialNumber, ps.State
                    FROM Panels p
                    JOIN PanelSides ps ON p.Id = ps.PanelId
                    WHERE p.DetectionDate BETWEEN @Start AND @End";

                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string sn = reader.GetString(0);
                                int? state = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1);

                                if (!snStates.ContainsKey(sn))
                                {
                                    snStates[sn] = new List<int?>();
                                }
                                snStates[sn].Add(state);
                            }
                        }
                    }

                    int totalSnCount = snStates.Count;
                    int uninspectedCount = 0;
                    int stillNgCount = 0;
                    int aviOkCount = 0;

                    foreach (var states in snStates.Values)
                    {
                        // 1. 如果有一个state为3，则添加到未检测结果数
                        if (states.Any(s => s == 3))
                        {
                            uninspectedCount++;
                        }
                        // 2. 如果有一个state为2，则添加到过滤后仍NG结果数
                        else if (states.Any(s => s == 2))
                        {
                            stillNgCount++;
                        }
                        // 3. 如果有两个state为0，则添加到AVI OK的结果数
                        else if (states.Count(s => s == 0) == 2)
                        {
                            aviOkCount++;
                        }
                    }

                    // 4. 剩余情况，添加到过滤后OK的数
                    int filteredOkCount = totalSnCount - uninspectedCount - stillNgCount - aviOkCount;

                    tcs.SetResult((totalSnCount, uninspectedCount, stillNgCount, aviOkCount, filteredOkCount));
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }


        /// <summary>
        /// 根据 LotNumber 获取 SN 状态统计
        /// 1.	如果有一个state为3，则添加到未检测结果数
        /// 2.	如果有一个state为2，则添加到过滤后仍NG结果数
        /// 3.	如果有两个state为0，则添加到AVI OK的结果数
        /// 4.	剩余情况，添加到过滤后OK的数 所以总共获取5个数字
        /// </summary>
        /// <param name="lotNumber"></param>
        /// <returns></returns>
        public Task<(int totalSnCount, int uninspectedCount, int stillNgCount, int aviOkCount, int filteredOkCount)> GetSnStateCountsByLot(string lotNumber)
        {
            var tcs = new TaskCompletionSource<(int, int, int, int, int)>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var snStates = new Dictionary<string, List<int?>>();
                    var sql = @"
                    SELECT p.SerialNumber, ps.State
                    FROM Panels p
                    JOIN PanelSides ps ON p.Id = ps.PanelId
                    WHERE p.LotNumber = @LotNumber";

                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@LotNumber", lotNumber);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string sn = reader.GetString(0);
                                int? state = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1);

                                if (!snStates.ContainsKey(sn))
                                {
                                    snStates[sn] = new List<int?>();
                                }
                                snStates[sn].Add(state);
                            }
                        }
                    }

                    int totalSnCount = snStates.Count;
                    int uninspectedCount = 0;
                    int stillNgCount = 0;
                    int aviOkCount = 0;

                    foreach (var states in snStates.Values)
                    {
                        // 1. 如果有一个state为3，则添加到未检测结果数
                        if (states.Any(s => s == 3))
                        {
                            uninspectedCount++;
                        }
                        // 2. 如果有一个state为2，则添加到过滤后仍NG结果数
                        else if (states.Any(s => s == 2))
                        {
                            stillNgCount++;
                        }
                        // 3. 如果有两个state为0，则添加到AVI OK的结果数
                        else if (states.Count(s => s == 0) == 2)
                        {
                            aviOkCount++;
                        }
                    }

                    // 4. 剩余情况，添加到过滤后OK的数
                    int filteredOkCount = totalSnCount - uninspectedCount - stillNgCount - aviOkCount;

                    tcs.SetResult((totalSnCount, uninspectedCount, stillNgCount, aviOkCount, filteredOkCount));
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        public Task<(int totalSnCount, int aviOkCount, int aiOkCount, int aiNgCount, int uninspectedCount)> GetSnStateCountsByTime(DateTime start, DateTime end)
        {
            var tcs = new TaskCompletionSource<(int, int, int, int, int)>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var snStates = new Dictionary<string, List<(int AviState, int FinalState)>>();
                    var sql = @"
                    SELECT p.SerialNumber, ps.AviState, ps.FinalState
                    FROM Panels p
                    JOIN PanelSides ps ON p.Id = ps.PanelId
                    WHERE p.DetectionDate BETWEEN @Start AND @End";

                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string sn = reader.GetString(0);
                                int aviState = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                                int finalState = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

                                if (!snStates.ContainsKey(sn))
                                {
                                    snStates[sn] = new List<(int, int)>();
                                }
                                snStates[sn].Add((aviState, finalState));
                            }
                        }
                    }

                    int totalSnCount = snStates.Count;
                    int aviOkCount = 0;
                    int aiOkCount = 0;
                    int aiNgCount = 0;
                    int uninspectedCount = 0;

                    foreach (var states in snStates.Values)
                    {
                        // 检查未检测: 只要有一个面的 AviState 是 0 (未运行)
                        if (states.Any(s => s.AviState == 0))
                        {
                            uninspectedCount++;
                            continue;
                        }

                        // 检查AVI OK: 两面都必须是 AVI OK (AviState = 1)
                        if (states.Count == 2 && states.All(s => s.AviState == 1))
                        {
                            aviOkCount++;
                            continue;
                        }

                        // 剩下的都是 AVI NG 的板
                        // 检查最终状态: 只要有一个面最终是 NG (FinalState = 2)，整个板就是 NG
                        if (states.Any(s => s.FinalState == 2))
                        {
                            aiNgCount++;
                        }
                        else // 否则，所有面最终都是 OK
                        {
                            aiOkCount++;
                        }
                    }

                    tcs.SetResult((totalSnCount, aviOkCount, aiOkCount, aiNgCount, uninspectedCount));
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        public Task<(string LotNumber, string ProductSerial)> GetLatestLotAndProductSerial(string machineId)
        {
            var tcs = new TaskCompletionSource<(string, string)>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    using (var cmd = new SQLiteCommand("SELECT LotNumber, ProductSerial FROM Panels WHERE MachineId = @MachineId ORDER BY DetectionDate DESC LIMIT 1", connection))
                    {
                        cmd.Parameters.AddWithValue("@MachineId", machineId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string lotNumber = reader.GetString(0);
                                string productSerial = reader.IsDBNull(1) ? null : reader.GetString(1);
                                tcs.SetResult((lotNumber, productSerial));
                            }
                            else
                            {
                                tcs.SetResult((null, null));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    LogTextHelper.Warn($"Error in GetLatestLotAndProductSerial: {ex.Message}");
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        public Task<List<PanelDataRecord>> GetPanelsDataByMachineAndLot(string machineId, string lotNumber)
        {
            var tcs = new TaskCompletionSource<List<PanelDataRecord>>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var panelRecords = new List<PanelDataRecord>();
                    var sqlBuilder = new System.Text.StringBuilder("SELECT Id, MachineId, SerialNumber, LotNumber, ProductSerial, DetectionDate, PathIndex, AviCreationTime FROM Panels WHERE LotNumber = @LotNumber");

                    if (!string.IsNullOrWhiteSpace(machineId))
                    {
                        sqlBuilder.Append(" AND MachineId = @MachineId");
                    }

                    using (var cmd = new SQLiteCommand(sqlBuilder.ToString(), connection))
                    {
                        cmd.Parameters.AddWithValue("@LotNumber", lotNumber);
                        if (!string.IsNullOrWhiteSpace(machineId))
                        {
                            cmd.Parameters.AddWithValue("@MachineId", machineId);
                        }

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                panelRecords.Add(new PanelDataRecord
                                {
                                    Id = reader.GetInt32(0),
                                    MachineId = reader.GetString(1),
                                    SerialNumber = reader.GetString(2),
                                    LotNumber = reader.GetString(3),
                                    ProductSerial = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    DetectionDate = reader.GetDateTime(5),
                                    PathIndex = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    AviCreationTime = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                                    Sides = new List<SideData>()
                                });
                            }
                        }
                    }

                    foreach (var record in panelRecords)
                    {
                        // 填充 sides
                        var sidesSql = "SELECT Side, TotalDefectsCount, RemainingDefectsCount, HeatPoints, AviState, AiState, VvsState, VrsState, FinalState FROM PanelSides WHERE PanelId = @PanelId";
                        using (var sidesCmd = new SQLiteCommand(sidesSql, connection))
                        {
                            sidesCmd.Parameters.AddWithValue("@PanelId", record.Id);
                            using (var sidesReader = sidesCmd.ExecuteReader())
                            {
                                while (sidesReader.Read())
                                {
                                    var sideData = new SideData
                                    {
                                        Side = sidesReader.GetString(0),
                                        TotalDefectsCount = sidesReader.GetInt32(1),
                                        RemainingDefectsCount = sidesReader.GetInt32(2),
                                        AviState = sidesReader.IsDBNull(4) ? 0 : sidesReader.GetInt32(4),
                                        AiState = sidesReader.IsDBNull(5) ? 0 : sidesReader.GetInt32(5),
                                        VvsState = sidesReader.IsDBNull(6) ? 0 : sidesReader.GetInt32(6),
                                        VrsState = sidesReader.IsDBNull(7) ? 0 : sidesReader.GetInt32(7),
                                        FinalState = sidesReader.IsDBNull(8) ? 0 : sidesReader.GetInt32(8)
                                    };

                                    if (!sidesReader.IsDBNull(3))
                                    {
                                        sideData.HeatPoints = JsonConvert.DeserializeObject<List<HeatPoint>>(sidesReader.GetString(3));
                                    }
                                    else
                                    {
                                        sideData.HeatPoints = new List<HeatPoint>();
                                    }
                                    record.Sides.Add(sideData);
                                }
                            }
                        }

                        // 衍生 IsAIOk
                        record.IsAIOk = record.Sides.Count == 2 && record.Sides.All(s => s.AiState == 1);
                    }
                    tcs.SetResult(panelRecords);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        /// <summary>
        /// 1.	输入起止时间，输出panels数据库所有的数据
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="partNumber">料号 (可选)</param>
        /// <returns></returns>
        public Task<List<PanelDataRecord>> GetPanelsData(DateTime start, DateTime end, string partNumber = null)
        {
            var tcs = new TaskCompletionSource<List<PanelDataRecord>>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var panelRecords = new List<PanelDataRecord>();
                    var sqlBuilder = new System.Text.StringBuilder("SELECT Id, MachineId, SerialNumber, LotNumber, ProductSerial, DetectionDate, PathIndex, avicreationtime FROM Panels WHERE DetectionDate BETWEEN @Start AND @End");

                    if (!string.IsNullOrWhiteSpace(partNumber))
                    {
                        sqlBuilder.Append(" AND ProductSerial = @ProductSerial");
                    }

                    using (var cmd = new SQLiteCommand(sqlBuilder.ToString(), connection))
                    {
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);
                        if (!string.IsNullOrWhiteSpace(partNumber))
                        {
                            cmd.Parameters.AddWithValue("@ProductSerial", partNumber);
                        }

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                panelRecords.Add(new PanelDataRecord
                                {
                                    Id = reader.GetInt32(0),
                                    MachineId = reader.GetString(1),
                                    SerialNumber = reader.GetString(2),
                                    LotNumber = reader.GetString(3),
                                    ProductSerial = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    DetectionDate = reader.GetDateTime(5),
                                    PathIndex = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    AviCreationTime = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                                    Sides = new List<SideData>()
                                });
                            }
                        }
                    }

                    foreach (var record in panelRecords)
                    {
                        var sidesSql = "SELECT Side, TotalDefectsCount, RemainingDefectsCount, HeatPoints, AviState, AiState, VvsState, VrsState, FinalState FROM PanelSides WHERE PanelId = @PanelId";
                        using (var sidesCmd = new SQLiteCommand(sidesSql, connection))
                        {
                            sidesCmd.Parameters.AddWithValue("@PanelId", record.Id);
                            using (var sidesReader = sidesCmd.ExecuteReader())
                            {
                                while (sidesReader.Read())
                                {
                                    var sideData = new SideData
                                    {
                                        Side = sidesReader.GetString(0),
                                        TotalDefectsCount = sidesReader.GetInt32(1),
                                        RemainingDefectsCount = sidesReader.GetInt32(2),
                                        AviState = sidesReader.IsDBNull(4) ? 0 : sidesReader.GetInt32(4),
                                        AiState = sidesReader.IsDBNull(5) ? 0 : sidesReader.GetInt32(5),
                                        VvsState = sidesReader.IsDBNull(6) ? 0 : sidesReader.GetInt32(6),
                                        VrsState = sidesReader.IsDBNull(7) ? 0 : sidesReader.GetInt32(7),
                                        FinalState = sidesReader.IsDBNull(8) ? 0 : sidesReader.GetInt32(8)
                                    };

                                    if (!sidesReader.IsDBNull(3))
                                    {
                                        sideData.HeatPoints = JsonConvert.DeserializeObject<List<HeatPoint>>(sidesReader.GetString(3));
                                    }
                                    else
                                    {
                                        sideData.HeatPoints = new List<HeatPoint>();
                                    }
                                    record.Sides.Add(sideData);
                                }
                            }
                        }

                        // 衍生 IsAIOk
                        record.IsAIOk = record.Sides.Count == 2 && record.Sides.All(s => s.AiState == 1);
                    }
                    tcs.SetResult(panelRecords);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        /// <summary>
        /// 生成 EmployeeReport 测试数据
        /// </summary>
        /// <param name="recordCount">要生成的记录数</param>
        public static void GenerateEmployeeReportTestData(int recordCount)
        {
            var dbHelper = new DatabaseHelper();
            var random = new Random();
            var startDate = DateTime.Now.AddMonths(-3);
            int totalDays = (DateTime.Now - startDate).Days;

            for (int i = 0; i < recordCount; i++)
            {
                var report = new EmployeeReport
                {
                    ID = $"Employee-{random.Next(1, 11)}",
                    SN = $"SN-{Guid.NewGuid().ToString().Substring(0, 8)}",
                    AllNGNumber = random.Next(1, 20),
                    StartTime = startDate.AddDays(random.Next(totalDays)).AddHours(random.Next(0, 24)),
                    VRSOKNumber = random.Next(0, 5)
                };
                report.EndTime = report.StartTime.AddMinutes(random.Next(5, 60));
                dbHelper.SaveEmployeeReport(report);
            }
        }
    }
}
using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace DeepsightSqlite
{
    public class DatabaseHelper
    {
        private static readonly string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "deepsight.db");
        private static readonly string connectionString = $"Data Source={dbPath};Version=3;";

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
                    IsAIOk BOOLEAN NOT NULL,
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
                    State INTEGER,
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
        /// 存储单面数据。如果另一面数据已存在，则更新IsAIOk状态。
        /// </summary>
        public void SavePanelSide(PanelSideRecord record)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    long panelId;

                    // 1. 查找或创建 Panel 记录
                    using (var cmd = new SQLiteCommand("SELECT Id FROM Panels WHERE SerialNumber = @SN", connection))
                    {
                        cmd.Parameters.AddWithValue("@SN", record.SerialNumber);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            panelId = (long)result;
                        }
                        else
                        {
                            var insertPanelCmd = new SQLiteCommand(
                                "INSERT INTO Panels (MachineId, SerialNumber, LotNumber, DetectionDate, IsAIOk, ProductSerial, PathIndex, AviCreationTime) VALUES (@MachineId, @SN, @Lot, @Date, @IsAIOk, @ProductSerial, @PathIndex, @AviCreationTime); SELECT last_insert_rowid();",
                                connection);
                            insertPanelCmd.Parameters.AddWithValue("@MachineId", record.MachineId);
                            insertPanelCmd.Parameters.AddWithValue("@SN", record.SerialNumber);
                            insertPanelCmd.Parameters.AddWithValue("@Lot", record.LotNumber);
                            insertPanelCmd.Parameters.AddWithValue("@Date", record.DetectionDate);
                            insertPanelCmd.Parameters.AddWithValue("@IsAIOk", false); // 初始默认为 false
                            insertPanelCmd.Parameters.AddWithValue("@ProductSerial", record.ProductSerial);
                            insertPanelCmd.Parameters.AddWithValue("@PathIndex", record.PathIndex);
                            insertPanelCmd.Parameters.AddWithValue("@AviCreationTime", record.AviCreationTime);
                            panelId = (long)insertPanelCmd.ExecuteScalar();
                        }
                    }

                    // 2. 插入 SideData
                    var insertSideCmd = new SQLiteCommand(
                        "INSERT INTO PanelSides (PanelId, Side, TotalDefectsCount, RemainingDefectsCount, HeatPoints, State) VALUES (@PanelId, @Side, @Total, @Remaining, @HeatPoints, @State)",
                        connection);
                    insertSideCmd.Parameters.AddWithValue("@PanelId", panelId);
                    insertSideCmd.Parameters.AddWithValue("@Side", record.Side);
                    insertSideCmd.Parameters.AddWithValue("@Total", record.Data.TotalDefectsCount);
                    insertSideCmd.Parameters.AddWithValue("@Remaining", record.Data.RemainingDefectsCount);
                    insertSideCmd.Parameters.AddWithValue("@HeatPoints", JsonConvert.SerializeObject(record.Data.HeatPoints));
                    insertSideCmd.Parameters.AddWithValue("@State", record.Data.State);
                    insertSideCmd.ExecuteNonQuery();

                    // 3. 检查是否双面数据都已存在，并更新 IsAIOk
                    var checkSidesCmd = new SQLiteCommand("SELECT Side, TotalDefectsCount, RemainingDefectsCount FROM PanelSides WHERE PanelId = @PanelId", connection);
                    checkSidesCmd.Parameters.AddWithValue("@PanelId", panelId);
                    var sides = new List<(string Side, int Total, int Remaining)>();
                    using (var reader = checkSidesCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sides.Add((reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2)));
                        }
                    }

                    if (sides.Count == 2)
                    {
                        bool isSideAOk = sides.Any(s => s.Side == "A" && s.Total > 0 && s.Remaining == 0);
                        bool isSideBOk = sides.Any(s => s.Side == "B" && s.Total > 0 && s.Remaining == 0);
                        bool isPanelAIOk = isSideAOk && isSideBOk;

                        var updatePanelCmd = new SQLiteCommand("UPDATE Panels SET IsAIOk = @IsAIOk WHERE Id = @PanelId", connection);
                        updatePanelCmd.Parameters.AddWithValue("@IsAIOk", isPanelAIOk);
                        updatePanelCmd.Parameters.AddWithValue("@PanelId", panelId);
                        updatePanelCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        // 查询逻辑 1: 根据 lot 号获取所有 sn
        public List<string> GetSerialNumbersByLot(string lotNumber)
        {
            var serialNumbers = new List<string>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand("SELECT SerialNumber FROM Panels WHERE LotNumber = @Lot", connection);
                cmd.Parameters.AddWithValue("@Lot", lotNumber);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        serialNumbers.Add(reader.GetString(0));
                    }
                }
            }
            return serialNumbers;
        }

        // 查询逻辑 2: 根据时间和 sn 获取所有 heatpoint 信息
        public List<HeatPoint> GetHeatPoints(string serialNumber, DateTime detectionDate)
        {
            var heatPoints = new List<HeatPoint>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    "SELECT ps.HeatPoints FROM PanelSides ps JOIN Panels p ON ps.PanelId = p.Id WHERE p.SerialNumber = @SN AND date(p.DetectionDate) = date(@Date)",
                    connection);
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
            return heatPoints;
        }

        // 查询逻辑 3 & 5 的组合: 获取机台在时间段内的板数统计
        public (int TotalBoards, int AIOkBoards) GetBoardCounts(DateTime start, DateTime end, string machineId = null)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var sql = "SELECT COUNT(*), SUM(CASE WHEN IsAIOk = 1 THEN 1 ELSE 0 END) FROM Panels WHERE DetectionDate BETWEEN @Start AND @End";
                if (!string.IsNullOrEmpty(machineId))
                {
                    sql += " AND MachineId = @MachineId";
                }

                var cmd = new SQLiteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@End", end);
                if (!string.IsNullOrEmpty(machineId))
                {
                    cmd.Parameters.AddWithValue("@MachineId", machineId);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int totalBoards = reader.GetInt32(0);
                        int aiOkBoards = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        return (totalBoards, aiOkBoards);
                    }
                }
            }
            return (0, 0);
        }

        // 查询逻辑 4 & 6 的组合: 获取机台在时间段内的报点数统计
        public (long TotalDefects, long AIOkDefects) GetDefectCounts(DateTime start, DateTime end, string machineId = null)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var sql = @"
                SELECT 
                    SUM(ps.TotalDefectsCount),
                    SUM(CASE WHEN p.IsAIOk = 1 THEN ps.TotalDefectsCount ELSE 0 END)
                FROM PanelSides ps
                JOIN Panels p ON ps.PanelId = p.Id
                WHERE p.DetectionDate BETWEEN @Start AND @End";

                if (!string.IsNullOrEmpty(machineId))
                {
                    sql += " AND p.MachineId = @MachineId";
                }

                var cmd = new SQLiteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@End", end);
                if (!string.IsNullOrEmpty(machineId))
                {
                    cmd.Parameters.AddWithValue("@MachineId", machineId);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        long totalDefects = reader.IsDBNull(0) ? 0 : Convert.ToInt64(reader.GetValue(0));
                        long aiOkDefects = reader.IsDBNull(1) ? 0 : Convert.ToInt64(reader.GetValue(1));
                        return (totalDefects, aiOkDefects);
                    }
                }
            }
            return (0, 0);
        }


        public void SaveEmployeeReport(EmployeeReport report)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
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
            }
        }

        public List<EmployeeReport> GetEmployeeReports(DateTime start, DateTime end)
        {
            var reports = new List<EmployeeReport>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
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
            }
            return reports;
        }

        /// <summary>
        /// 获取数据库中所有唯一的 MachineId
        /// </summary>
        public List<string> GetAllMachineIds()
        {
            var machineIds = new List<string>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand("SELECT DISTINCT MachineId FROM Panels", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        machineIds.Add(reader.GetString(0));
                    }
                }
            }
            return machineIds;
        }

        /// <summary>
        /// 获取每个机台在时间段内的板数统计
        /// </summary>
        public Dictionary<string, (int TotalBoards, int AIOkBoards)> GetBoardCountsPerMachine(DateTime start, DateTime end)
        {
            var results = new Dictionary<string, (int TotalBoards, int AIOkBoards)>();
            var machineIds = GetAllMachineIds();

            foreach (var machineId in machineIds)
            {
                var counts = GetBoardCounts(start, end, machineId);
                if (counts.TotalBoards > 0) // 只添加有数据的机台
                {
                    results[machineId] = counts;
                }
            }
            return results;
        }

        /// <summary>
        /// 获取每个机台在时间段内的报点数统计
        /// </summary>
        public Dictionary<string, (long TotalDefects, long AIOkDefects)> GetDefectCountsPerMachine(DateTime start, DateTime end)
        {
            var results = new Dictionary<string, (long TotalDefects, long AIOkDefects)>();
            var machineIds = GetAllMachineIds();

            foreach (var machineId in machineIds)
            {
                var counts = GetDefectCounts(start, end, machineId);
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
        public List<DateTime> GetDetectionDates(DateTime start, DateTime end)
        {
            var detectionDates = new List<DateTime>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand("SELECT DISTINCT DetectionDate FROM Panels WHERE DetectionDate BETWEEN @Start AND @End", connection);
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
            return detectionDates;
        }

        /// <summary>
        /// 根据 machineID 获取最新的 SN 和 Lot
        /// </summary>
        /// <param name="machineId">机器ID</param>
        /// <returns>最新的 SN 和 Lot</returns>
        public (string SerialNumber, string LotNumber, string ProductSerial,string PathIndex) GetLatestPanelInfoByMachineId(string machineId)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    var cmd = new SQLiteCommand("SELECT SerialNumber, LotNumber, ProductSerial, PathIndex, AviCreationTime FROM Panels WHERE MachineId = @MachineId ORDER BY DetectionDate DESC LIMIT 1", connection);
                    cmd.Parameters.AddWithValue("@MachineId", machineId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string serialNumber = reader.GetString(0);
                            string lotNumber = reader.GetString(1);
                            string ProductSerial = reader.IsDBNull(2) ? null : reader.GetString(2);
                            string pathIndex = reader.IsDBNull(3) ? null : reader.GetString(3);
                            DateTime? AviCreationTime = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4);
                            return (serialNumber, lotNumber, ProductSerial, pathIndex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                LogTextHelper.Warn($"Error in GetLatestPanelInfoByMachineId: {ex.Message}");
            }
            return (null, null, null, null);
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

                    // Side A
                    var sideAData = new SideData
                    {
                        TotalDefectsCount = random.Next(0, 10),
                        HeatPoints = new List<HeatPoint>()
                    };
                    sideAData.RemainingDefectsCount = random.Next(0, sideAData.TotalDefectsCount + 1);
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
                        Data = sideAData
                    };
                    dbHelper.SavePanelSide(recordA);

                    // Side B
                    var sideBData = new SideData
                    {
                        TotalDefectsCount = random.Next(0, 10),
                        HeatPoints = new List<HeatPoint>()
                    };
                    sideBData.RemainingDefectsCount = random.Next(0, sideBData.TotalDefectsCount + 1);
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
                        Data = sideBData
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
        public (int totalSnCount, int uninspectedCount, int stillNgCount, int aviOkCount, int filteredOkCount) GetSnStateCounts(DateTime start, DateTime end)
        {
            var snStates = new Dictionary<string, List<int?>>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
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

            return (totalSnCount, uninspectedCount, stillNgCount, aviOkCount, filteredOkCount);
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
        public (int totalSnCount, int uninspectedCount, int stillNgCount, int aviOkCount, int filteredOkCount) GetSnStateCountsByLot(string lotNumber)
        {
            var snStates = new Dictionary<string, List<int?>>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
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

            return (totalSnCount, uninspectedCount, stillNgCount, aviOkCount, filteredOkCount);
        }

        public (int totalSnCount, int uninspectedCount, int stillNgCount, int aviOkCount, int filteredOkCount) GetSnStateCountsByTime(DateTime start, DateTime end)
        {
            var snStates = new Dictionary<string, List<int?>>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
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

            return (totalSnCount, uninspectedCount, stillNgCount, aviOkCount, filteredOkCount);
        }

        public (string LotNumber, string ProductSerial) GetLatestLotAndProductSerial(string machineId)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    var cmd = new SQLiteCommand("SELECT LotNumber, ProductSerial FROM Panels WHERE MachineId = @MachineId ORDER BY DetectionDate DESC LIMIT 1", connection);
                    cmd.Parameters.AddWithValue("@MachineId", machineId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string lotNumber = reader.GetString(0);
                            string productSerial = reader.IsDBNull(1) ? null : reader.GetString(1);
                            return (lotNumber, productSerial);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                LogTextHelper.Warn($"Error in GetLatestLotAndProductSerial: {ex.Message}");
            }
            return (null, null);
        }

        public List<PanelDataRecord> GetPanelsDataByMachineAndLot(string machineId, string lotNumber)
        {
            var panelRecords = new List<PanelDataRecord>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var sql = "SELECT Id, MachineId, SerialNumber, LotNumber, ProductSerial, DetectionDate, IsAIOk, PathIndex, AviCreationTime FROM Panels WHERE MachineId = @MachineId AND LotNumber = @LotNumber";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@MachineId", machineId);
                    cmd.Parameters.AddWithValue("@LotNumber", lotNumber);
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
                                IsAIOk = reader.GetBoolean(6),
                                PathIndex = reader.IsDBNull(7) ? null : reader.GetString(7),
                                AviCreationTime = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                                Sides = new List<SideData>()
                            });
                        }
                    }
                }

                foreach (var record in panelRecords)
                {
                    var sidesSql = "SELECT Side, TotalDefectsCount, RemainingDefectsCount, HeatPoints, State FROM PanelSides WHERE PanelId = @PanelId";
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
                                    State = sidesReader.IsDBNull(4) ? 0 : sidesReader.GetInt32(4)
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
                }
            }
            return panelRecords;
        }

        /// <summary>
        /// 1.	输入起止时间，输出panels数据库所有的数据
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<PanelDataRecord> GetPanelsData(DateTime start, DateTime end)
        {
            var panelRecords = new List<PanelDataRecord>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var sql = "SELECT Id, MachineId, SerialNumber, LotNumber, ProductSerial, DetectionDate, IsAIOk, PathIndex, avicreationtime FROM Panels WHERE DetectionDate BETWEEN @Start AND @End";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Start", start);
                    cmd.Parameters.AddWithValue("@End", end);
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
                                IsAIOk = reader.GetBoolean(6),
                                PathIndex = reader.IsDBNull(7) ? null : reader.GetString(7),
                                AviCreationTime = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                                Sides = new List<SideData>()
                            });
                        }
                    }
                }

                foreach (var record in panelRecords)
                {
                    var sidesSql = "SELECT Side, TotalDefectsCount, RemainingDefectsCount, HeatPoints, State FROM PanelSides WHERE PanelId = @PanelId";
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
                                    State = sidesReader.IsDBNull(4) ? 0 : sidesReader.GetInt32(4)
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
                }
            }
            return panelRecords;
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
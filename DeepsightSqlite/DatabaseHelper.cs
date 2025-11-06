using DeepSightModel;
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
                    DetectionDate DATETIME NOT NULL,
                    IsAIOk BOOLEAN NOT NULL
                );";

                string createPanelSidesTable = @"
                CREATE TABLE IF NOT EXISTS PanelSides (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PanelId INTEGER NOT NULL,
                    Side TEXT NOT NULL, -- 'A' 或 'B'
                    TotalDefectsCount INTEGER NOT NULL,
                    RemainingDefectsCount INTEGER NOT NULL,
                    HeatPoints TEXT, -- 存储 HeatPoint 列表的 JSON 字符串
                    FOREIGN KEY (PanelId) REFERENCES Panels(Id) ON DELETE CASCADE
                );";

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = createPanelsTable;
                    command.ExecuteNonQuery();
                    command.CommandText = createPanelSidesTable;
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
                                "INSERT INTO Panels (MachineId, SerialNumber, LotNumber, DetectionDate, IsAIOk) VALUES (@MachineId, @SN, @Lot, @Date, @IsAIOk); SELECT last_insert_rowid();",
                                connection);
                            insertPanelCmd.Parameters.AddWithValue("@MachineId", record.MachineId);
                            insertPanelCmd.Parameters.AddWithValue("@SN", record.SerialNumber);
                            insertPanelCmd.Parameters.AddWithValue("@Lot", record.LotNumber);
                            insertPanelCmd.Parameters.AddWithValue("@Date", record.DetectionDate);
                            insertPanelCmd.Parameters.AddWithValue("@IsAIOk", false); // 初始默认为 false
                            panelId = (long)insertPanelCmd.ExecuteScalar();
                        }
                    }

                    // 2. 插入 SideData
                    var insertSideCmd = new SQLiteCommand(
                        "INSERT INTO PanelSides (PanelId, Side, TotalDefectsCount, RemainingDefectsCount, HeatPoints) VALUES (@PanelId, @Side, @Total, @Remaining, @HeatPoints)",
                        connection);
                    insertSideCmd.Parameters.AddWithValue("@PanelId", panelId);
                    insertSideCmd.Parameters.AddWithValue("@Side", record.Side);
                    insertSideCmd.Parameters.AddWithValue("@Total", record.Data.TotalDefectsCount);
                    insertSideCmd.Parameters.AddWithValue("@Remaining", record.Data.RemainingDefectsCount);
                    insertSideCmd.Parameters.AddWithValue("@HeatPoints", JsonConvert.SerializeObject(record.Data.HeatPoints));
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
    }
}
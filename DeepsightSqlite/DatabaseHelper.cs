using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using DeepSightModel;

namespace DeepsightSqlite
{
    public class DatabaseHelper
    {
        private readonly string connectionString;

        public DatabaseHelper(string databaseFileName)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, databaseFileName);
            connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
        }

        public void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 创建面板摘要表
                string createPanelSummariesTableSql = @"
                CREATE TABLE IF NOT EXISTS PanelSummaries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MachineId TEXT NOT NULL,
                    DetectionDate TEXT NOT NULL,
                    SerialNumber TEXT NOT NULL,
                    LotNumber TEXT,
                    SideADefectCount INTEGER,
                    SideBDefectCount INTEGER,
                    SideARemainingDefects INTEGER,
                    SideBRemainingDefects INTEGER,
                    IsAIOk INTEGER,
                    UNIQUE(SerialNumber, LotNumber)
                );";
                ExecuteNonQuery(createPanelSummariesTableSql);

                // 创建缺陷详情表
                string createDefectDetailsTableSql = @"
                CREATE TABLE IF NOT EXISTS DefectDetails (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PanelSummaryId INTEGER NOT NULL,
                    Side TEXT NOT NULL,
                    DefectName TEXT,
                    DefectType TEXT,
                    RoiX INTEGER,
                    RoiY INTEGER,
                    ImagePath TEXT,
                    FOREIGN KEY(PanelSummaryId) REFERENCES PanelSummaries(Id)
                );";
                ExecuteNonQuery(createDefectDetailsTableSql);
            }
        }

        /// <summary>
        /// 写入或更新一条面板单面数据
        /// </summary>
        public void AddOrUpdatePanelSide(PanelSideRecord record)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    // 1. 查找或创建摘要记录
                    long panelSummaryId;
                    string findSql = "SELECT Id FROM PanelSummaries WHERE SerialNumber = @SerialNumber AND LotNumber = @LotNumber;";
                    var findParams = new[]
                    {
                        new SQLiteParameter("@SerialNumber", record.SerialNumber),
                        new SQLiteParameter("@LotNumber", record.LotNumber)
                    };

                    object existingId = ExecuteScalar(findSql, findParams);

                    if (existingId != null)
                    {
                        panelSummaryId = Convert.ToInt64(existingId);
                    }
                    else
                    {
                        string insertSummarySql = @"
                        INSERT INTO PanelSummaries (MachineId, DetectionDate, SerialNumber, LotNumber)
                        VALUES (@MachineId, @DetectionDate, @SerialNumber, @LotNumber);";
                        var insertParams = new[]
                        {
                            new SQLiteParameter("@MachineId", record.MachineId),
                            new SQLiteParameter("@DetectionDate", record.DetectionDate.ToString("yyyy-MM-dd HH:mm:ss.fff")),
                            new SQLiteParameter("@SerialNumber", record.SerialNumber),
                            new SQLiteParameter("@LotNumber", record.LotNumber)
                        };
                        ExecuteNonQuery(insertSummarySql, insertParams);
                        panelSummaryId = connection.LastInsertRowId;
                    }

                    // 2. 更新摘要表中的侧面信息
                    string sideColumnPrefix = record.Side.ToUpper() == "A" ? "SideA" : "SideB";
                    string updateSummarySql = $@"
                    UPDATE PanelSummaries SET
                        {sideColumnPrefix}RemainingDefectCount = @RemainingDefectCount,
                        {sideColumnPrefix}TotalDefectsCount = @TotalDefectsCount
                    WHERE Id = @Id;";

                    var updateParams = new[]
                    {
                        new SQLiteParameter("@RemainingDefectCount", record.Data.RemainingDefectInfoList.Count),
                        new SQLiteParameter("@TotalDefectsCount", record.Data.TotalDefectsCount),
                        new SQLiteParameter("@Id", panelSummaryId)
                    };
                    ExecuteNonQuery(updateSummarySql, updateParams);

                    // 3. 清除旧的缺陷详情并插入新的
                    ExecuteNonQuery("DELETE FROM DefectDetails WHERE PanelSummaryId = @PanelSummaryId AND Side = @Side;", new[] { new SQLiteParameter("@PanelSummaryId", panelSummaryId), new SQLiteParameter("@Side", record.Side) });
                    InsertDefectDetails(panelSummaryId, record.Side, record.Data.RemainingDefectInfoList);

                    // 4. 更新 IsAIOk 状态
                    UpdateIsAIOk(panelSummaryId);

                    transaction.Commit();
                }
            }
        }

        private void UpdateIsAIOk(long panelSummaryId)
        {
            string querySql = "SELECT SideADefectCount, SideBDefectCount, SideARemainingDefects, SideBRemainingDefects FROM PanelSummaries WHERE Id = @Id;";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(querySql, connection))
                {
                    command.Parameters.AddWithValue("@Id", panelSummaryId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // 使用 nullable int 来处理可能尚未记录的侧面数据
                            int? sideADefects = reader["SideADefectCount"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["SideADefectCount"]);
                            int? sideBDefects = reader["SideBDefectCount"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["SideBDefectCount"]);
                            int? sideARemaining = reader["SideARemainingDefects"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["SideARemainingDefects"]);
                            int? sideBRemaining = reader["SideBRemainingDefects"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["SideBRemainingDefects"]);

                            // 只有当两面的数据都存在时才计算最终结果
                            if (sideADefects.HasValue && sideBDefects.HasValue)
                            {
                                bool isOk = (sideADefects.Value + sideBDefects.Value > 0) && (sideARemaining.Value + sideBRemaining.Value == 0);
                                string updateSql = "UPDATE PanelSummaries SET IsAIOk = @IsAIOk WHERE Id = @Id;";
                                ExecuteNonQuery(updateSql, new[] { new SQLiteParameter("@IsAIOk", isOk ? 1 : 0), new SQLiteParameter("@Id", panelSummaryId) });
                            }
                        }
                    }
                }
            }
        }

        private void InsertDefectDetails(long panelSummaryId, string side, List<DefectDetail> defects)
        {
            if (defects == null || defects.Count == 0) return;

            string insertDetailSql = @"
            INSERT INTO DefectDetails (PanelSummaryId, Side, DefectName, DefectType, RoiX, RoiY, ImagePath)
            VALUES (@PanelSummaryId, @Side, @DefectName, @DefectType, @RoiX, @RoiY, @ImagePath);";

            foreach (var defect in defects)
            {
                var detailParams = new[]
                {
                    new SQLiteParameter("@PanelSummaryId", panelSummaryId),
                    new SQLiteParameter("@Side", side),
                    new SQLiteParameter("@DefectName", defect.DefectName),
                    new SQLiteParameter("@DefectType", defect.DefectType),
                    new SQLiteParameter("@RoiX", defect.RoiX),
                    new SQLiteParameter("@RoiY", defect.RoiY),
                    new SQLiteParameter("@ImagePath", defect.ImagePath)
                };
                ExecuteNonQuery(insertDetailSql, detailParams);
            }
        }

        /// <summary>
        /// 按天读取指定时间段内所有机台的板子数量和AI判定OK的板子数量
        /// </summary>
        public List<DailyStat> GetDailyStatsForAllMachines(DateTime startDate, DateTime endDate)
        {
            var stats = new List<DailyStat>();
            string query = @"
            SELECT
                date(DetectionDate) as StatDate,
                MachineId,
                COUNT(*) as TotalBoards,
                SUM(CASE WHEN IsAIOk = 1 THEN 1 ELSE 0 END) as AIOkBoards
            FROM PanelSummaries
            WHERE date(DetectionDate) BETWEEN date(@StartDate) AND date(@EndDate)
            GROUP BY StatDate, MachineId
            ORDER BY StatDate, MachineId;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd"));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stats.Add(new DailyStat
                            {
                                Date = DateTime.Parse(reader["StatDate"].ToString()),
                                MachineId = reader["MachineId"].ToString(),
                                TotalBoards = Convert.ToInt32(reader["TotalBoards"]),
                                AIOkBoards = Convert.ToInt32(reader["AIOkBoards"])
                            });
                        }
                    }
                }
            }
            return stats;
        }

        /// <summary>
        /// 按天读取指定时间段内单个机台的板子数量和AI判定OK的板子数量
        /// </summary>
        public List<DailyStat> GetDailyStatsForMachine(string machineId, DateTime startDate, DateTime endDate)
        {
            var stats = new List<DailyStat>();
            string query = @"
            SELECT
                date(DetectionDate) as StatDate,
                COUNT(*) as TotalBoards,
                SUM(CASE WHEN IsAIOk = 1 THEN 1 ELSE 0 END) as AIOkBoards
            FROM PanelSummaries
            WHERE MachineId = @MachineId AND date(DetectionDate) BETWEEN date(@StartDate) AND date(@EndDate)
            GROUP BY StatDate
            ORDER BY StatDate;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MachineId", machineId);
                    command.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd"));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stats.Add(new DailyStat
                            {
                                Date = DateTime.Parse(reader["StatDate"].ToString()),
                                MachineId = machineId,
                                TotalBoards = Convert.ToInt32(reader["TotalBoards"]),
                                AIOkBoards = Convert.ToInt32(reader["AIOkBoards"])
                            });
                        }
                    }
                }
            }
            return stats;
        }

        private void ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    command.ExecuteNonQuery();
                }
            }
        }
        
        private object ExecuteScalar(string sql, params SQLiteParameter[] parameters)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    return command.ExecuteScalar();
                }
            }
        }
        // ... 保留您现有的其他方法 ...
        public void AddRecord(ProcessingRecord record){}
        public List<ProcessingRecord> GetRecords(){ return null; }
        public void UpdateDailyStats(string machineId,DateTime dateTime, bool isOk){}
        public PassThroughtAviData GetStatsByDayAndMachine(string machineId, DateTime date){ return null; }
        public List<PassThroughtAviData> GetStatsForMachine(string machineId, DateTime startDate, DateTime endDate){ return null; }
        public List<PassThroughtAviData> GetStatsByDate(DateTime date){ return null; }
    }
}
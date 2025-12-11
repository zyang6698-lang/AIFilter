using DeepSightTool;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Npgsql;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeepSightDB
{
    public class DatabaseHelper : IDisposable
    {
        private static readonly PostgreSqlConfig _config = PostgreSqlConfig.Load();
        private static readonly string connectionString = _config.GetConnectionString();
        private readonly BlockingCollection<Action<NpgsqlConnection>> _dbQueue = new BlockingCollection<Action<NpgsqlConnection>>();
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
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // PostgreSQL 不需要 PRAGMA 设置，连接池和性能由 Npgsql 自动管理

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

        /// <summary>
        /// 确保数据库存在，如果不存在则自动创建
        /// </summary>
        private static void EnsureDatabaseExists()
        {
            try
            {
                // 尝试连接到目标数据库，如果成功则数据库已存在
                using (var testConnection = new NpgsqlConnection(connectionString))
                {
                    testConnection.Open();
                    LogTextHelper.Info($"数据库 '{_config.Database}' 已存在");
                    return;
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                // 错误代码 3D000 表示数据库不存在
                if (ex.SqlState == "3D000")
                {
                    LogTextHelper.Warn($"数据库 '{_config.Database}' 不存在，正在自动创建...");

                    try
                    {
                        // 连接到 postgres 数据库来创建新数据库
                        using (var connection = new NpgsqlConnection(_config.GetPostgresConnectionString()))
                        {
                            connection.Open();

                            // 创建数据库 - 使用简化的语法，继承模板数据库的排序规则
                            // 这样可以避免与中文 Windows 系统的默认排序规则冲突
                            string createDbSql = $@"
                                CREATE DATABASE {_config.Database}
                                WITH
                                OWNER = {_config.Username}
                                ENCODING = 'UTF8'";

                            using (var command = new NpgsqlCommand(createDbSql, connection))
                            {
                                command.ExecuteNonQuery();
                            }

                            LogTextHelper.Info($"数据库 '{_config.Database}' 创建成功！");
                        }
                    }
                    catch (Exception createEx)
                    {
                        LogTextHelper.Error($"创建数据库失败: {createEx.Message}");
                        throw new Exception($"无法创建数据库 '{_config.Database}'。请确保 PostgreSQL 服务正在运行，并且用户 '{_config.Username}' 有创建数据库的权限。", createEx);
                    }
                }
                else
                {
                    // 其他类型的错误，直接抛出
                    LogTextHelper.Error($"连接数据库时发生错误: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"检查数据库存在性时发生错误: {ex.Message}");
                throw;
            }
        }

        public static void InitializeDatabase()
        {
            // 首先确保数据库存在
            EnsureDatabaseExists();

            // 然后创建表结构
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string createPanelsTable = @"
                CREATE TABLE IF NOT EXISTS Panels (
                    Id SERIAL PRIMARY KEY,
                    MachineId TEXT NOT NULL,
                    SerialNumber TEXT NOT NULL UNIQUE,
                    LotNumber TEXT NOT NULL,
                    ProductSerial TEXT,
                    DetectionDate TIMESTAMP NOT NULL,
                    PathIndex TEXT,
                    AviCreationTime TIMESTAMP
                );";

                string createPanelSidesTable = @"
                CREATE TABLE IF NOT EXISTS PanelSides (
                    Id SERIAL PRIMARY KEY,
                    PanelId INTEGER NOT NULL,
                    Side TEXT NOT NULL, -- 'A' 或 'B'
                    HeatPoints TEXT, -- 存储 DetectInfo 列表的 JSON 字符串
                    AviState INTEGER DEFAULT 0, -- 0: 未运行, 1: OK, 2: NG
                    AiState INTEGER DEFAULT 0,  -- 0: 未运行, 1: OK, 2: NG
                    VvsState INTEGER DEFAULT 0, -- 0: 未运行, 1: OK, 2: NG
                    VrsState INTEGER DEFAULT 0, -- 0: 未运行, 1: OK, 2: NG
                    FinalState INTEGER DEFAULT 0, -- 0: 待处理, 1: 最终OK, 2: 最终NG
                    FOREIGN KEY (PanelId) REFERENCES Panels(Id) ON DELETE CASCADE
                );";


                string createEmployeeReportsTable = @"
                CREATE TABLE IF NOT EXISTS EmployeeReports (
                    Id SERIAL PRIMARY KEY,
                    EmployeeID TEXT NOT NULL,
                    SN TEXT NOT NULL,
                    AllNGNumber INTEGER NOT NULL,
                    StartTime TIMESTAMP NOT NULL,
                    EndTime TIMESTAMP NOT NULL,
                    VRSOKNumber INTEGER NOT NULL
                );";

                // 创建索引以提升查询性能
                // PanelSides 的唯一索引，同时支持 INSERT ON CONFLICT
                string createPanelSidesUniqueIndex = @"
                CREATE UNIQUE INDEX IF NOT EXISTS idx_panelsides_panelid_side ON PanelSides (PanelId, Side);";

                string createPanelsDetectionDateIndex = @"
                CREATE INDEX IF NOT EXISTS idx_panels_detectiondate ON Panels (DetectionDate);";

                string createPanelsMachineIdIndex = @"
                CREATE INDEX IF NOT EXISTS idx_panels_machineid ON Panels (MachineId);";

                string createPanelsLotNumberIndex = @"
                CREATE INDEX IF NOT EXISTS idx_panels_lotnumber ON Panels (LotNumber);";

                using (var command = new NpgsqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = createPanelsTable;
                    command.ExecuteNonQuery();
                    command.CommandText = createPanelSidesTable;
                    command.ExecuteNonQuery();
                    command.CommandText = createEmployeeReportsTable;
                    command.ExecuteNonQuery();

                    // 创建索引
                    command.CommandText = createPanelSidesUniqueIndex;
                    command.ExecuteNonQuery();
                    command.CommandText = createPanelsDetectionDateIndex;
                    command.ExecuteNonQuery();
                    command.CommandText = createPanelsMachineIdIndex;
                    command.ExecuteNonQuery();
                    command.CommandText = createPanelsLotNumberIndex;
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 存储单面数据
        /// </summary>
        public void SavePanelSide(PanelSideRecord record)
        {
            // 参数验证
            if (record == null)
            {
                LogTextHelper.Error("SavePanelSide: record 为 null，无法保存");
                return;
            }
            if (string.IsNullOrEmpty(record.SerialNumber))
            {
                LogTextHelper.Error("SavePanelSide: SerialNumber 为空，无法保存");
                return;
            }
            if (string.IsNullOrEmpty(record.Side))
            {
                LogTextHelper.Error($"SavePanelSide: Side 为空，SN={record.SerialNumber}，无法保存");
                return;
            }
            if (record.Data == null)
            {
                LogTextHelper.Error($"SavePanelSide: Data 为 null，SN={record.SerialNumber}，Side={record.Side}，无法保存");
                return;
            }

            // 提前序列化 HeatPoints，避免在数据库操作中序列化
            var heatPointsJson = JsonConvert.SerializeObject(record.Data.DetectPoints ?? new List<DetectInfo>());

            _dbQueue.Add(connection =>
            {
                NpgsqlTransaction transaction = null;
                try
                {
                    transaction = connection.BeginTransaction();
                    long panelId;
                    bool isNewPanel;

                    // 1. 使用 INSERT ON CONFLICT DO NOTHING + SELECT 获取 PanelId
                    using (var insertCmd = new NpgsqlCommand(
                        "INSERT INTO Panels (MachineId, SerialNumber, LotNumber, DetectionDate, ProductSerial, PathIndex, AviCreationTime) VALUES (@MachineId, @SN, @Lot, @Date, @ProductSerial, @PathIndex, @AviCreationTime) ON CONFLICT (SerialNumber) DO NOTHING",
                        connection, transaction))
                    {
                        insertCmd.Parameters.AddWithValue("@MachineId", record.MachineId ?? string.Empty);
                        insertCmd.Parameters.AddWithValue("@SN", record.SerialNumber);
                        insertCmd.Parameters.AddWithValue("@Lot", record.LotNumber ?? string.Empty);
                        insertCmd.Parameters.AddWithValue("@Date", record.DetectionDate);
                        insertCmd.Parameters.AddWithValue("@ProductSerial", (object)record.ProductSerial ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@PathIndex", (object)record.PathIndex ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@AviCreationTime", (object)record.AviCreationTime ?? DBNull.Value);
                        // ExecuteNonQuery 返回受影响的行数，0 表示 SN 重复被忽略
                        int rowsAffected = insertCmd.ExecuteNonQuery();
                        isNewPanel = rowsAffected > 0;
                    }

                    using (var selectCmd = new NpgsqlCommand("SELECT Id FROM Panels WHERE SerialNumber = @SN", connection, transaction))
                    {
                        selectCmd.Parameters.AddWithValue("@SN", record.SerialNumber);
                        panelId = Convert.ToInt64(selectCmd.ExecuteScalar());
                    }

                    // 检查当前 Side 是否已存在数据
                    bool sideExists = false;
                    if (!isNewPanel)
                    {
                        using (var checkSideCmd = new NpgsqlCommand(
                            "SELECT COUNT(*) FROM PanelSides WHERE PanelId = @PanelId AND Side = @Side",
                            connection, transaction))
                        {
                            checkSideCmd.Parameters.AddWithValue("@PanelId", panelId);
                            checkSideCmd.Parameters.AddWithValue("@Side", record.Side);
                            sideExists = Convert.ToInt32(checkSideCmd.ExecuteScalar()) > 0;
                        }
                    }

                    // 如果是重复数据写入（SN存在且当前面数据也已存在），更新 DetectionDate 为最新时间
                    if (!isNewPanel && sideExists)
                    {
                        using (var updateDateCmd = new NpgsqlCommand(
                            "UPDATE Panels SET DetectionDate = @Date WHERE Id = @PanelId",
                            connection, transaction))
                        {
                            updateDateCmd.Parameters.AddWithValue("@Date", record.DetectionDate);
                            updateDateCmd.Parameters.AddWithValue("@PanelId", panelId);
                            updateDateCmd.ExecuteNonQuery();
                        }
                    }

                    // 2. 使用 INSERT ON CONFLICT DO UPDATE 一条语句搞定插入或更新
                    using (var upsertCmd = new NpgsqlCommand(
                        @"INSERT INTO PanelSides (PanelId, Side, HeatPoints, AviState, AiState, VvsState, VrsState, FinalState)
                          VALUES (@PanelId, @Side, @HeatPoints, @AviState, @AiState, @VvsState, @VrsState, @FinalState)
                          ON CONFLICT (PanelId, Side) DO UPDATE SET
                          HeatPoints = EXCLUDED.HeatPoints,
                          AviState = EXCLUDED.AviState,
                          AiState = EXCLUDED.AiState,
                          VvsState = EXCLUDED.VvsState,
                          VrsState = EXCLUDED.VrsState,
                          FinalState = EXCLUDED.FinalState",
                        connection, transaction))
                    {
                        upsertCmd.Parameters.AddWithValue("@PanelId", panelId);
                        upsertCmd.Parameters.AddWithValue("@Side", record.Side);
                        upsertCmd.Parameters.AddWithValue("@HeatPoints", heatPointsJson);
                        upsertCmd.Parameters.AddWithValue("@AviState", record.Data.AviState);
                        upsertCmd.Parameters.AddWithValue("@AiState", record.Data.AiState);
                        upsertCmd.Parameters.AddWithValue("@VvsState", record.Data.VvsState);
                        upsertCmd.Parameters.AddWithValue("@VrsState", record.Data.VrsState);
                        upsertCmd.Parameters.AddWithValue("@FinalState", record.Data.FinalState);
                        upsertCmd.ExecuteNonQuery();
                    }

                    // 3. 检查是否双面数据都已存在（用于日志记录和验证）
                    int sidesCount = 0;
                    using (var checkSidesCmd = new NpgsqlCommand(
                        "SELECT COUNT(*) FROM PanelSides WHERE PanelId = @PanelId",
                        connection, transaction))
                    {
                        checkSidesCmd.Parameters.AddWithValue("@PanelId", panelId);
                        sidesCount = Convert.ToInt32(checkSidesCmd.ExecuteScalar());
                    }

                    transaction.Commit();

                    if (isNewPanel)
                    {
                        LogTextHelper.Info($"SavePanelSide: 成功保存 SN={record.SerialNumber}, Side={record.Side}");
                    }
                    else if (sideExists)
                    {
                        // SN存在且当前面数据也已存在，警告覆盖
                        LogTextHelper.Warn($"SavePanelSide: SN={record.SerialNumber} 的 Side={record.Side} 数据已存在，正在覆盖");
                    }
                    else
                    {
                        // SN存在但是存的是另一面的数据，正常情况
                        LogTextHelper.Info($"SavePanelSide: 成功保存 SN={record.SerialNumber}, Side={record.Side} (另一面已存在)");
                    }

                    // 如果双面数据都已存在，记录日志
                    if (sidesCount == 2)
                    {
                        LogTextHelper.Info($"SavePanelSide: SN={record.SerialNumber} 的 A、B 两面数据已完整");
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"SavePanelSide: 保存失败 SN={record.SerialNumber}, Side={record.Side}, 错误: {ex.Message}");
                    LogTextHelper.Error($"SavePanelSide: 详细堆栈: {ex}");
                    try
                    {
                        transaction?.Rollback();
                    }
                    catch (Exception rollbackEx)
                    {
                        LogTextHelper.Error($"SavePanelSide: 回滚事务失败: {rollbackEx.Message}");
                    }
                }
                finally
                {
                    transaction?.Dispose();
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
                    using (var cmd = new NpgsqlCommand("SELECT SerialNumber FROM Panels WHERE LotNumber = @Lot", connection))
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
        public Task<List<DetectInfo>> GetHeatPoints(string serialNumber, DateTime detectionDate)
        {
            var tcs = new TaskCompletionSource<List<DetectInfo>>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var heatPoints = new List<DetectInfo>();
                    using (var cmd = new NpgsqlCommand(
                        "SELECT ps.HeatPoints FROM PanelSides ps JOIN Panels p ON ps.PanelId = p.Id WHERE p.SerialNumber = @SN AND DATE(p.DetectionDate) = DATE(@Date)",
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
                                    var points = JsonConvert.DeserializeObject<List<DetectInfo>>(heatPointsJson);
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
                    using (var totalCmd = new NpgsqlCommand(totalSql, connection))
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
                    using (var aiOkCmd = new NpgsqlCommand(aiOkSql, connection))
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
        // 由于 TotalDefectsCount 已经从数据库中移除，现在通过解析 HeatPoints JSON 来计算缺陷数
        public Task<(long TotalDefects, long AIOkDefects)> GetDefectCounts(DateTime start, DateTime end, string machineId = null)
        {
            var tcs = new TaskCompletionSource<(long, long)>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    long totalDefects = 0;
                    long aiOkDefects = 0;

                    // 首先获取所有 Panel 及其 AiOkBothSides 状态
                    var panelAiOkSql = @"
                        SELECT PanelId,
                               CASE WHEN COUNT(*) = 2 AND SUM(CASE WHEN AiState = 1 THEN 1 ELSE 0 END) = 2 THEN 1 ELSE 0 END AS AiOkBothSides
                        FROM PanelSides ps
                        JOIN Panels p ON ps.PanelId = p.Id
                        WHERE p.DetectionDate BETWEEN @Start AND @End" + (string.IsNullOrEmpty(machineId) ? string.Empty : " AND p.MachineId = @MachineId") + @"
                        GROUP BY PanelId";

                    var panelAiOkDict = new Dictionary<long, bool>();
                    using (var cmd = new NpgsqlCommand(panelAiOkSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);
                        if (!string.IsNullOrEmpty(machineId)) cmd.Parameters.AddWithValue("@MachineId", machineId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                long panelId = reader.GetInt64(0);
                                bool isAiOk = reader.GetInt32(1) == 1;
                                panelAiOkDict[panelId] = isAiOk;
                            }
                        }
                    }

                    // 然后获取每个 PanelSide 的 HeatPoints 并计算缺陷数
                    var sidesSql = @"
                        SELECT ps.PanelId, ps.HeatPoints
                        FROM PanelSides ps
                        JOIN Panels p ON ps.PanelId = p.Id
                        WHERE p.DetectionDate BETWEEN @Start AND @End" + (string.IsNullOrEmpty(machineId) ? string.Empty : " AND p.MachineId = @MachineId");

                    using (var cmd = new NpgsqlCommand(sidesSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);
                        if (!string.IsNullOrEmpty(machineId)) cmd.Parameters.AddWithValue("@MachineId", machineId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                long panelId = reader.GetInt64(0);
                                int defectCount = 0;

                                if (!reader.IsDBNull(1))
                                {
                                    var heatPointsJson = reader.GetString(1);
                                    var points = JsonConvert.DeserializeObject<List<DetectInfo>>(heatPointsJson);
                                    defectCount = points?.Count ?? 0;
                                }

                                totalDefects += defectCount;
                                if (panelAiOkDict.TryGetValue(panelId, out bool isAiOk) && isAiOk)
                                {
                                    aiOkDefects += defectCount;
                                }
                            }
                        }
                    }

                    tcs.SetResult((totalDefects, aiOkDefects));
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
                using (var cmd = new NpgsqlCommand(sql, connection))
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
                    using (var cmd = new NpgsqlCommand(sql, connection))
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
                    using (var cmd = new NpgsqlCommand("SELECT DISTINCT MachineId FROM Panels", connection))
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
                    using (var cmd = new NpgsqlCommand("SELECT DISTINCT DetectionDate FROM Panels WHERE DetectionDate BETWEEN @Start AND @End", connection))
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
                    using (var cmd = new NpgsqlCommand("SELECT SerialNumber, LotNumber, ProductSerial, PathIndex, AviCreationTime FROM Panels WHERE MachineId = @MachineId ORDER BY DetectionDate DESC LIMIT 1", connection))
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
                    int totalDefectsCountA = random.Next(0, 10);
                    var sideAData = new SideData
                    {
                        Side = "A",
                        DetectPoints = new List<DetectInfo>()
                    };

                    // 生成缺陷点
                    for (int j = 0; j < totalDefectsCountA; j++)
                    {
                        sideAData.DetectPoints.Add(new DetectInfo
                        {
                            DefectName = $"Defect-{j}",
                            DefectType = $"Type-{random.Next(1, 5)}",
                            AIStatus = random.Next(0, 3),
                            VVSStatus = random.Next(0, 3),
                            VrsState = random.Next(0, 3),
                            FinalState = random.Next(0, 3)
                        });
                    }

                    // 根据缺陷数生成 State
                    if (totalDefectsCountA == 0)
                    {
                        sideAData.AviState = 1; // AVI OK
                        sideAData.AiState = 1; // AI 默认也 OK
                        sideAData.FinalState = 1; // 最终 OK
                    }
                    else
                    {
                        sideAData.AviState = 2; // AVI NG
                        // 模拟AI处理
                        int remainingA = sideAData.DetectPoints.Count(d => d.AIStatus != 1);
                        if (remainingA == 0)
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
                    int totalDefectsCountB = random.Next(0, 10);
                    var sideBData = new SideData
                    {
                        Side = "B",
                        DetectPoints = new List<DetectInfo>()
                    };

                    // 生成缺陷点
                    for (int j = 0; j < totalDefectsCountB; j++)
                    {
                        sideBData.DetectPoints.Add(new DetectInfo
                        {
                            DefectName = $"Defect-{j}",
                            DefectType = $"Type-{random.Next(1, 5)}",
                            AIStatus = random.Next(0, 3),
                            VVSStatus = random.Next(0, 3),
                            VrsState = random.Next(0, 3),
                            FinalState = random.Next(0, 3)
                        });
                    }

                    // 根据缺陷数生成 State
                    if (totalDefectsCountB == 0)
                    {
                        sideBData.AviState = 1; // AVI OK
                        sideBData.AiState = 1; // AI 默认也 OK
                        sideBData.FinalState = 1; // 最终 OK
                    }
                    else
                    {
                        sideBData.AviState = 2; // AVI NG
                        // 模拟AI处理
                        int remainingB = sideBData.DetectPoints.Count(d => d.AIStatus != 1);
                        if (remainingB == 0)
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

                    using (var cmd = new NpgsqlCommand(sql, connection))
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

                    using (var cmd = new NpgsqlCommand(sql, connection))
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

                    using (var cmd = new NpgsqlCommand(sql, connection))
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
                    using (var cmd = new NpgsqlCommand("SELECT LotNumber, ProductSerial FROM Panels WHERE MachineId = @MachineId ORDER BY DetectionDate DESC LIMIT 1", connection))
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

                    // 如果 lotNumber 为空，直接返回空列表
                    if (string.IsNullOrWhiteSpace(lotNumber))
                    {
                        tcs.SetResult(panelRecords);
                        return;
                    }

                    var sqlBuilder = new System.Text.StringBuilder("SELECT Id, MachineId, SerialNumber, LotNumber, ProductSerial, DetectionDate, PathIndex, AviCreationTime FROM Panels WHERE LotNumber = @LotNumber");

                    if (!string.IsNullOrWhiteSpace(machineId))
                    {
                        sqlBuilder.Append(" AND MachineId = @MachineId");
                    }

                    using (var cmd = new NpgsqlCommand(sqlBuilder.ToString(), connection))
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
                        var sidesSql = "SELECT Side, HeatPoints, AviState, AiState, VvsState, VrsState, FinalState FROM PanelSides WHERE PanelId = @PanelId";
                        using (var sidesCmd = new NpgsqlCommand(sidesSql, connection))
                        {
                            sidesCmd.Parameters.AddWithValue("@PanelId", record.Id);
                            using (var sidesReader = sidesCmd.ExecuteReader())
                            {
                                while (sidesReader.Read())
                                {
                                    var sideData = new SideData
                                    {
                                        Side = sidesReader.GetString(0),
                                        AviState = sidesReader.IsDBNull(2) ? 0 : sidesReader.GetInt32(2),
                                        AiState = sidesReader.IsDBNull(3) ? 0 : sidesReader.GetInt32(3),
                                        VvsState = sidesReader.IsDBNull(4) ? 0 : sidesReader.GetInt32(4),
                                        VrsState = sidesReader.IsDBNull(5) ? 0 : sidesReader.GetInt32(5),
                                        FinalState = sidesReader.IsDBNull(6) ? 0 : sidesReader.GetInt32(6)
                                    };

                                    if (!sidesReader.IsDBNull(1))
                                    {
                                        sideData.DetectPoints = JsonConvert.DeserializeObject<List<DetectInfo>>(sidesReader.GetString(1));
                                    }
                                    else
                                    {
                                        sideData.DetectPoints = new List<DetectInfo>();
                                    }
                                    record.Sides.Add(sideData);
                                }
                            }
                        }
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

                    using (var cmd = new NpgsqlCommand(sqlBuilder.ToString(), connection))
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
                        var sidesSql = "SELECT Side, HeatPoints, AviState, AiState, VvsState, VrsState, FinalState FROM PanelSides WHERE PanelId = @PanelId";
                        using (var sidesCmd = new NpgsqlCommand(sidesSql, connection))
                        {
                            sidesCmd.Parameters.AddWithValue("@PanelId", record.Id);
                            using (var sidesReader = sidesCmd.ExecuteReader())
                            {
                                while (sidesReader.Read())
                                {
                                    var sideData = new SideData
                                    {
                                        Side = sidesReader.GetString(0),
                                        AviState = sidesReader.IsDBNull(2) ? 0 : sidesReader.GetInt32(2),
                                        AiState = sidesReader.IsDBNull(3) ? 0 : sidesReader.GetInt32(3),
                                        VvsState = sidesReader.IsDBNull(4) ? 0 : sidesReader.GetInt32(4),
                                        VrsState = sidesReader.IsDBNull(5) ? 0 : sidesReader.GetInt32(5),
                                        FinalState = sidesReader.IsDBNull(6) ? 0 : sidesReader.GetInt32(6)
                                    };

                                    if (!sidesReader.IsDBNull(1))
                                    {
                                        sideData.DetectPoints = JsonConvert.DeserializeObject<List<DetectInfo>>(sidesReader.GetString(1));
                                    }
                                    else
                                    {
                                        sideData.DetectPoints = new List<DetectInfo>();
                                    }
                                    record.Sides.Add(sideData);
                                }
                            }
                        }
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

        /// <summary>
        /// 清空数据库所有表的数据
        /// </summary>
        public Task<bool> ClearAllData()
        {
            var tcs = new TaskCompletionSource<bool>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 按照外键依赖顺序删除数据
                            // 先删除子表 PanelSides 和 EmployeeReports
                            using (var cmd1 = new NpgsqlCommand("DELETE FROM PanelSides", connection, transaction))
                            {
                                cmd1.ExecuteNonQuery();
                            }

                            using (var cmd2 = new NpgsqlCommand("DELETE FROM EmployeeReports", connection, transaction))
                            {
                                cmd2.ExecuteNonQuery();
                            }

                            // 再删除主表 Panels
                            using (var cmd3 = new NpgsqlCommand("DELETE FROM Panels", connection, transaction))
                            {
                                cmd3.ExecuteNonQuery();
                            }

                            // 重置序列（自增ID）
                            using (var cmd4 = new NpgsqlCommand("ALTER SEQUENCE panels_id_seq RESTART WITH 1", connection, transaction))
                            {
                                cmd4.ExecuteNonQuery();
                            }

                            using (var cmd5 = new NpgsqlCommand("ALTER SEQUENCE panelsides_id_seq RESTART WITH 1", connection, transaction))
                            {
                                cmd5.ExecuteNonQuery();
                            }

                            using (var cmd6 = new NpgsqlCommand("ALTER SEQUENCE employeereports_id_seq RESTART WITH 1", connection, transaction))
                            {
                                cmd6.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            LogTextHelper.Info("数据库清空成功");
                            tcs.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogTextHelper.Error($"清空数据库失败: {ex.Message}");
                            tcs.SetException(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"清空数据库时发生错误: {ex.Message}");
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }
    }
}
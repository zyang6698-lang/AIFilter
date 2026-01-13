using DeepSightDB.Interfaces;
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
    /// <summary>
    /// 数据库服务实现类
    /// </summary>
    public class DatabaseHelper : IDatabaseService
    {
        /// <summary>
        /// 默认配置（兼容已有 WinForms 程序使用）
        /// </summary>
        private static readonly PostgreSqlConfig DefaultConfig = PostgreSqlConfig.Load();
        private static readonly string DefaultConnectionString = DefaultConfig.GetConnectionString();

        /// <summary>
        /// 当前实例使用的配置和连接串（支持多机台 / 多数据库）
        /// </summary>
        private readonly PostgreSqlConfig _config;
        private readonly string _connectionString;

        private readonly BlockingCollection<Action<NpgsqlConnection>> _dbQueue = new BlockingCollection<Action<NpgsqlConnection>>();
        private readonly Thread _dbThread;
        private bool _disposed = false;

        // 预编译的 SQL 命令（用于提高性能）
        private NpgsqlCommand _upsertPanelCmd;
        private NpgsqlCommand _upsertPanelSideCmd;

        /// <summary>
        /// 使用默认配置的构造函数（保持向后兼容）
        /// </summary>
        public DatabaseHelper() : this(DefaultConfig)
        {
        }

        /// <summary>
        /// 使用指定 PostgreSqlConfig 的构造函数（推荐：允许为不同机台传入不同配置）
        /// </summary>
        /// <param name="config">PostgreSQL 配置</param>
        public DatabaseHelper(PostgreSqlConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _connectionString = _config.GetConnectionString();

            _dbThread = new Thread(ProcessQueue)
            {
                IsBackground = true,
                Name = "DatabaseThread"
            };
            _dbThread.Start();
        }

        /// <summary>
        /// 直接使用连接字符串的构造函数（适合 ASP.NET 中从配置读取完整连接串）
        /// 说明：使用该构造函数不会自动创建数据库和表结构，请在外部确保库表已存在。
        /// </summary>
        /// <param name="connectionString">PostgreSQL 连接字符串</param>
        public DatabaseHelper(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString 不能为空", nameof(connectionString));

            _connectionString = connectionString;

            _dbThread = new Thread(ProcessQueue)
            {
                IsBackground = true,
                Name = "DatabaseThread"
            };
            _dbThread.Start();
        }

        /// <summary>
        /// 初始化预编译命令
        /// </summary>
        private void InitializePreparedCommands(NpgsqlConnection connection)
        {
            // 预编译 Panel UPSERT 命令
            _upsertPanelCmd = new NpgsqlCommand(
                @"INSERT INTO Panels (MachineId, SerialNumber, LotNumber, DetectionDate, ProductSerial, PathIndex, AviCreationTime)
                  VALUES (@MachineId, @SN, @Lot, @Date, @ProductSerial, @PathIndex, @AviCreationTime)
                  ON CONFLICT (SerialNumber) DO UPDATE SET DetectionDate = EXCLUDED.DetectionDate
                  RETURNING Id",
                connection);
            _upsertPanelCmd.Parameters.Add(new NpgsqlParameter("@MachineId", NpgsqlTypes.NpgsqlDbType.Text));
            _upsertPanelCmd.Parameters.Add(new NpgsqlParameter("@SN", NpgsqlTypes.NpgsqlDbType.Text));
            _upsertPanelCmd.Parameters.Add(new NpgsqlParameter("@Lot", NpgsqlTypes.NpgsqlDbType.Text));
            _upsertPanelCmd.Parameters.Add(new NpgsqlParameter("@Date", NpgsqlTypes.NpgsqlDbType.Timestamp));
            _upsertPanelCmd.Parameters.Add(new NpgsqlParameter("@ProductSerial", NpgsqlTypes.NpgsqlDbType.Text) { IsNullable = true });
            _upsertPanelCmd.Parameters.Add(new NpgsqlParameter("@PathIndex", NpgsqlTypes.NpgsqlDbType.Text) { IsNullable = true });
            _upsertPanelCmd.Parameters.Add(new NpgsqlParameter("@AviCreationTime", NpgsqlTypes.NpgsqlDbType.Timestamp) { IsNullable = true });
            _upsertPanelCmd.Prepare();

            // 预编译 PanelSide UPSERT 命令
            _upsertPanelSideCmd = new NpgsqlCommand(
                @"INSERT INTO PanelSides (PanelId, Side, HeatPoints, AviState, AiState, VvsState, VrsState, FinalState)
                  VALUES (@PanelId, @Side, @HeatPoints, @AviState, @AiState, @VvsState, @VrsState, @FinalState)
                  ON CONFLICT (PanelId, Side) DO UPDATE SET
                  HeatPoints = EXCLUDED.HeatPoints,
                  AviState = EXCLUDED.AviState,
                  AiState = EXCLUDED.AiState,
                  VvsState = EXCLUDED.VvsState,
                  VrsState = EXCLUDED.VrsState,
                  FinalState = EXCLUDED.FinalState",
                connection);
            _upsertPanelSideCmd.Parameters.Add(new NpgsqlParameter("@PanelId", NpgsqlTypes.NpgsqlDbType.Bigint));
            _upsertPanelSideCmd.Parameters.Add(new NpgsqlParameter("@Side", NpgsqlTypes.NpgsqlDbType.Text));
            _upsertPanelSideCmd.Parameters.Add(new NpgsqlParameter("@HeatPoints", NpgsqlTypes.NpgsqlDbType.Text));
            _upsertPanelSideCmd.Parameters.Add(new NpgsqlParameter("@AviState", NpgsqlTypes.NpgsqlDbType.Integer));
            _upsertPanelSideCmd.Parameters.Add(new NpgsqlParameter("@AiState", NpgsqlTypes.NpgsqlDbType.Integer));
            _upsertPanelSideCmd.Parameters.Add(new NpgsqlParameter("@VvsState", NpgsqlTypes.NpgsqlDbType.Integer));
            _upsertPanelSideCmd.Parameters.Add(new NpgsqlParameter("@VrsState", NpgsqlTypes.NpgsqlDbType.Integer));
            _upsertPanelSideCmd.Parameters.Add(new NpgsqlParameter("@FinalState", NpgsqlTypes.NpgsqlDbType.Integer));
            _upsertPanelSideCmd.Prepare();

            LogTextHelper.Info("PostgreSQL 预编译命令初始化完成");
        }

        private void ProcessQueue()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // 初始化预编译命令
                InitializePreparedCommands(connection);

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

                // 清理预编译命令
                _upsertPanelCmd?.Dispose();
                _upsertPanelSideCmd?.Dispose();
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
        /// 获取当前数据库队列长度（待处理的操作数）
        /// </summary>
        public int GetQueueLength() => _dbQueue.Count;
        /// <summary>
        /// 确保数据库存在，如果不存在则自动创建（使用指定配置）
        /// </summary>
        private static void EnsureDatabaseExists(PostgreSqlConfig config)
        {
            var connectionString = config.GetConnectionString();
            try
            {
                // 尝试连接到目标数据库，如果成功则数据库已存在
                using (var testConnection = new NpgsqlConnection(connectionString))
                {
                    testConnection.Open();
                    LogTextHelper.Info($"数据库 '{config.Database}' 已存在");
                    return;
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                // 错误代码 3D000 表示数据库不存在
                if (ex.SqlState == "3D000")
                {
                    LogTextHelper.Warn($"数据库 '{config.Database}' 不存在，正在自动创建...");

                    try
                    {
                        // 连接到 postgres 数据库来创建新数据库
                        using (var connection = new NpgsqlConnection(config.GetPostgresConnectionString()))
                        {
                            connection.Open();

                            // 创建数据库 - 使用简化的语法，继承模板数据库的排序规则
                            // 这样可以避免与中文 Windows 系统的默认排序规则冲突
                            string createDbSql = $@"
                                CREATE DATABASE {config.Database}
                                WITH
                                OWNER = {config.Username}
                                ENCODING = 'UTF8'";

                            using (var command = new NpgsqlCommand(createDbSql, connection))
                            {
                                command.ExecuteNonQuery();
                            }

                            LogTextHelper.Info($"数据库 '{config.Database}' 创建成功！");
                        }
                    }
                    catch (Exception createEx)
                    {
                        LogTextHelper.Error($"创建数据库失败: {createEx.Message}");
                        throw new Exception($"无法创建数据库 '{config.Database}'。请确保 PostgreSQL 服务正在运行，并且用户 '{config.Username}' 有创建数据库的权限。", createEx);
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

        /// <summary>
        /// 使用默认配置初始化数据库（向后兼容）
        /// </summary>
        public static void InitializeDatabase()
        {
            InitializeDatabase(DefaultConfig);
        }

        /// <summary>
        /// 使用指定配置初始化数据库（适用于 ASP.NET 为不同机台使用不同连接参数的场景）
        /// </summary>
        /// <param name="config">PostgreSQL 配置</param>
        public static void InitializeDatabase(PostgreSqlConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            // 首先确保数据库存在
            EnsureDatabaseExists(config);

            var connectionString = config.GetConnectionString();

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

                // EmployeeReports 表的索引，优化时间范围查询
                string createEmployeeReportsStartTimeIndex = @"
                CREATE INDEX IF NOT EXISTS idx_employeereports_starttime ON EmployeeReports (StartTime);";

                string createEmployeeReportsEndTimeIndex = @"
                CREATE INDEX IF NOT EXISTS idx_employeereports_endtime ON EmployeeReports (EndTime);";

                // 性能优化索引 - 复合索引支持常用查询模式
                // 1. 支持按 MachineId + DetectionDate 排序查询 (GetLatestPanelInfoByMachineId)
                string createPanelsMachineIdDetectionDateIndex = @"
                CREATE INDEX IF NOT EXISTS idx_panels_machineid_detectiondate ON Panels (MachineId, DetectionDate DESC);";

                // 2. PanelSides 的 PanelId 索引优化 JOIN 查询
                string createPanelSidesPanelIdIndex = @"
                CREATE INDEX IF NOT EXISTS idx_panelsides_panelid ON PanelSides (PanelId);";

                // 3. PanelSides 状态字段复合索引，优化聚合查询
                string createPanelSidesStatesIndex = @"
                CREATE INDEX IF NOT EXISTS idx_panelsides_states ON PanelSides (PanelId, AviState, FinalState);";

                // 4. ProductSerial 索引，优化按料号查询
                string createPanelsProductSerialIndex = @"
                CREATE INDEX IF NOT EXISTS idx_panels_productserial ON Panels (ProductSerial) WHERE ProductSerial IS NOT NULL;";

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
                    command.CommandText = createEmployeeReportsStartTimeIndex;
                    command.ExecuteNonQuery();
                    command.CommandText = createEmployeeReportsEndTimeIndex;
                    command.ExecuteNonQuery();

                    // 性能优化索引
                    command.CommandText = createPanelsMachineIdDetectionDateIndex;
                    command.ExecuteNonQuery();
                    command.CommandText = createPanelSidesPanelIdIndex;
                    command.ExecuteNonQuery();
                    command.CommandText = createPanelSidesStatesIndex;
                    command.ExecuteNonQuery();
                    command.CommandText = createPanelsProductSerialIndex;
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

                    // 使用预编译命令提高性能
                    _upsertPanelCmd.Transaction = transaction;
                    _upsertPanelCmd.Parameters["@MachineId"].Value = record.MachineId ?? string.Empty;
                    _upsertPanelCmd.Parameters["@SN"].Value = record.SerialNumber;
                    _upsertPanelCmd.Parameters["@Lot"].Value = record.LotNumber ?? string.Empty;
                    _upsertPanelCmd.Parameters["@Date"].Value = record.DetectionDate;
                    _upsertPanelCmd.Parameters["@ProductSerial"].Value = (object)record.ProductSerial ?? DBNull.Value;
                    _upsertPanelCmd.Parameters["@PathIndex"].Value = (object)record.PathIndex ?? DBNull.Value;
                    _upsertPanelCmd.Parameters["@AviCreationTime"].Value = (object)record.AviCreationTime ?? DBNull.Value;
                    long panelId = Convert.ToInt64(_upsertPanelCmd.ExecuteScalar());

                    // 使用预编译命令插入 PanelSide
                    _upsertPanelSideCmd.Transaction = transaction;
                    _upsertPanelSideCmd.Parameters["@PanelId"].Value = panelId;
                    _upsertPanelSideCmd.Parameters["@Side"].Value = record.Side;
                    _upsertPanelSideCmd.Parameters["@HeatPoints"].Value = heatPointsJson;
                    _upsertPanelSideCmd.Parameters["@AviState"].Value = record.Data.AviState;
                    _upsertPanelSideCmd.Parameters["@AiState"].Value = record.Data.AiState;
                    _upsertPanelSideCmd.Parameters["@VvsState"].Value = record.Data.VvsState;
                    _upsertPanelSideCmd.Parameters["@VrsState"].Value = record.Data.VrsState;
                    _upsertPanelSideCmd.Parameters["@FinalState"].Value = record.Data.FinalState;
                    _upsertPanelSideCmd.ExecuteNonQuery();

                    transaction.Commit();
                    LogTextHelper.Info($"SavePanelSide: 成功保存 SN={record.SerialNumber}, Side={record.Side}");
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
        /// <summary>
        /// 批量存储面板数据 - 用于大量数据导入场景
        /// 使用单个事务提交多条记录，显著提升插入性能
        /// </summary>
        /// <param name="records">要保存的记录列表</param>
        /// <param name="batchSize">每批次提交的记录数，默认100</param>
        public Task SavePanelSidesBatch(IEnumerable<PanelSideRecord> records, int batchSize = 100)
        {
            var tcs = new TaskCompletionSource<bool>();
            var recordList = records?.ToList();

            if (recordList == null || recordList.Count == 0)
            {
                tcs.SetResult(true);
                return tcs.Task;
            }

            // 预先序列化所有 HeatPoints
            var serializedRecords = recordList
                .Where(r => r != null && !string.IsNullOrEmpty(r.SerialNumber) && !string.IsNullOrEmpty(r.Side) && r.Data != null)
                .Select(r => new
                {
                    Record = r,
                    HeatPointsJson = JsonConvert.SerializeObject(r.Data.DetectPoints ?? new List<DetectInfo>())
                })
                .ToList();

            _dbQueue.Add(connection =>
            {
                int totalSaved = 0;
                int totalFailed = 0;

                // 分批处理
                for (int i = 0; i < serializedRecords.Count; i += batchSize)
                {
                    var batch = serializedRecords.Skip(i).Take(batchSize).ToList();
                    NpgsqlTransaction transaction = null;

                    try
                    {
                        transaction = connection.BeginTransaction();

                        foreach (var item in batch)
                        {
                            var record = item.Record;

                            _upsertPanelCmd.Transaction = transaction;
                            _upsertPanelCmd.Parameters["@MachineId"].Value = record.MachineId ?? string.Empty;
                            _upsertPanelCmd.Parameters["@SN"].Value = record.SerialNumber;
                            _upsertPanelCmd.Parameters["@Lot"].Value = record.LotNumber ?? string.Empty;
                            _upsertPanelCmd.Parameters["@Date"].Value = record.DetectionDate;
                            _upsertPanelCmd.Parameters["@ProductSerial"].Value = (object)record.ProductSerial ?? DBNull.Value;
                            _upsertPanelCmd.Parameters["@PathIndex"].Value = (object)record.PathIndex ?? DBNull.Value;
                            _upsertPanelCmd.Parameters["@AviCreationTime"].Value = (object)record.AviCreationTime ?? DBNull.Value;
                            long panelId = Convert.ToInt64(_upsertPanelCmd.ExecuteScalar());

                            _upsertPanelSideCmd.Transaction = transaction;
                            _upsertPanelSideCmd.Parameters["@PanelId"].Value = panelId;
                            _upsertPanelSideCmd.Parameters["@Side"].Value = record.Side;
                            _upsertPanelSideCmd.Parameters["@HeatPoints"].Value = item.HeatPointsJson;
                            _upsertPanelSideCmd.Parameters["@AviState"].Value = record.Data.AviState;
                            _upsertPanelSideCmd.Parameters["@AiState"].Value = record.Data.AiState;
                            _upsertPanelSideCmd.Parameters["@VvsState"].Value = record.Data.VvsState;
                            _upsertPanelSideCmd.Parameters["@VrsState"].Value = record.Data.VrsState;
                            _upsertPanelSideCmd.Parameters["@FinalState"].Value = record.Data.FinalState;
                            _upsertPanelSideCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        totalSaved += batch.Count;
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"SavePanelSidesBatch: 批量保存失败，批次 {i / batchSize + 1}, 错误: {ex.Message}");
                        totalFailed += batch.Count;
                        try
                        {
                            transaction?.Rollback();
                        }
                        catch (Exception rollbackEx)
                        {
                            LogTextHelper.Error($"SavePanelSidesBatch: 回滚事务失败: {rollbackEx.Message}");
                        }
                    }
                    finally
                    {
                        transaction?.Dispose();
                    }
                }

                LogTextHelper.Info($"SavePanelSidesBatch: 批量保存完成，成功 {totalSaved} 条，失败 {totalFailed} 条");
                tcs.SetResult(totalFailed == 0);
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

        /// <summary>
        /// 获取指定时间范围内的员工报告数据
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns>员工报告列表</returns>
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
        /// 获取指定时间范围内的所有员工ID列表
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns>员工ID列表</returns>
        public Task<List<string>> GetEmployeeIds(DateTime start, DateTime end)
        {
            var tcs = new TaskCompletionSource<List<string>>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var employeeIds = new List<string>();
                    var sql = "SELECT DISTINCT EmployeeID FROM EmployeeReports WHERE StartTime >= @Start AND EndTime <= @End ORDER BY EmployeeID";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                employeeIds.Add(reader.GetString(0));
                            }
                        }
                    }
                    tcs.SetResult(employeeIds);
                }
                catch (Exception ex)
                {
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
                    var panelRecords = new Dictionary<int, PanelDataRecord>();

                    // 如果 lotNumber 为空，直接返回空列表
                    if (string.IsNullOrWhiteSpace(lotNumber))
                    {
                        tcs.SetResult(new List<PanelDataRecord>());
                        return;
                    }

                    // 使用 LEFT JOIN 一次性查询所有数据，避免 N+1 问题
                    var sqlBuilder = new System.Text.StringBuilder(@"
                        SELECT p.Id, p.MachineId, p.SerialNumber, p.LotNumber, p.ProductSerial, p.DetectionDate, p.PathIndex, p.AviCreationTime,
                               ps.Side, ps.HeatPoints, ps.AviState, ps.AiState, ps.VvsState, ps.VrsState, ps.FinalState
                        FROM Panels p
                        LEFT JOIN PanelSides ps ON p.Id = ps.PanelId
                        WHERE p.LotNumber = @LotNumber");

                    if (!string.IsNullOrWhiteSpace(machineId))
                    {
                        sqlBuilder.Append(" AND p.MachineId = @MachineId");
                    }
                    sqlBuilder.Append(" ORDER BY p.Id");

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
                                int panelId = reader.GetInt32(0);

                                // 如果 Panel 不存在，创建新的
                                if (!panelRecords.TryGetValue(panelId, out var panelRecord))
                                {
                                    panelRecord = new PanelDataRecord
                                    {
                                        Id = panelId,
                                        MachineId = reader.GetString(1),
                                        SerialNumber = reader.GetString(2),
                                        LotNumber = reader.GetString(3),
                                        ProductSerial = reader.IsDBNull(4) ? null : reader.GetString(4),
                                        DetectionDate = reader.GetDateTime(5),
                                        PathIndex = reader.IsDBNull(6) ? null : reader.GetString(6),
                                        AviCreationTime = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                                        Sides = new List<SideData>()
                                    };
                                    panelRecords[panelId] = panelRecord;
                                }

                                // 如果有 Side 数据，添加到 Sides 列表
                                if (!reader.IsDBNull(8))
                                {
                                    var sideData = new SideData
                                    {
                                        Side = reader.GetString(8),
                                        AviState = reader.IsDBNull(10) ? 0 : reader.GetInt32(10),
                                        AiState = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                                        VvsState = reader.IsDBNull(12) ? 0 : reader.GetInt32(12),
                                        VrsState = reader.IsDBNull(13) ? 0 : reader.GetInt32(13),
                                        FinalState = reader.IsDBNull(14) ? 0 : reader.GetInt32(14)
                                    };

                                    if (!reader.IsDBNull(9))
                                    {
                                        sideData.DetectPoints = JsonConvert.DeserializeObject<List<DetectInfo>>(reader.GetString(9));
                                    }
                                    else
                                    {
                                        sideData.DetectPoints = new List<DetectInfo>();
                                    }
                                    panelRecord.Sides.Add(sideData);
                                }
                            }
                        }
                    }

                    tcs.SetResult(panelRecords.Values.ToList());
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
                    var panelRecords = new Dictionary<int, PanelDataRecord>();

                    // 使用 LEFT JOIN 一次性查询所有数据，避免 N+1 问题
                    var sqlBuilder = new System.Text.StringBuilder(@"
                        SELECT p.Id, p.MachineId, p.SerialNumber, p.LotNumber, p.ProductSerial, p.DetectionDate, p.PathIndex, p.AviCreationTime,
                               ps.Side, ps.HeatPoints, ps.AviState, ps.AiState, ps.VvsState, ps.VrsState, ps.FinalState
                        FROM Panels p
                        LEFT JOIN PanelSides ps ON p.Id = ps.PanelId
                        WHERE p.DetectionDate BETWEEN @Start AND @End");

                    if (!string.IsNullOrWhiteSpace(partNumber))
                    {
                        sqlBuilder.Append(" AND p.ProductSerial = @ProductSerial");
                    }
                    sqlBuilder.Append(" ORDER BY p.Id");

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
                                int panelId = reader.GetInt32(0);

                                // 如果 Panel 不存在，创建新的
                                if (!panelRecords.TryGetValue(panelId, out var panelRecord))
                                {
                                    panelRecord = new PanelDataRecord
                                    {
                                        Id = panelId,
                                        MachineId = reader.GetString(1),
                                        SerialNumber = reader.GetString(2),
                                        LotNumber = reader.GetString(3),
                                        ProductSerial = reader.IsDBNull(4) ? null : reader.GetString(4),
                                        DetectionDate = reader.GetDateTime(5),
                                        PathIndex = reader.IsDBNull(6) ? null : reader.GetString(6),
                                        AviCreationTime = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                                        Sides = new List<SideData>()
                                    };
                                    panelRecords[panelId] = panelRecord;
                                }

                                // 如果有 Side 数据，添加到 Sides 列表
                                if (!reader.IsDBNull(8))
                                {
                                    var sideData = new SideData
                                    {
                                        Side = reader.GetString(8),
                                        AviState = reader.IsDBNull(10) ? 0 : reader.GetInt32(10),
                                        AiState = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                                        VvsState = reader.IsDBNull(12) ? 0 : reader.GetInt32(12),
                                        VrsState = reader.IsDBNull(13) ? 0 : reader.GetInt32(13),
                                        FinalState = reader.IsDBNull(14) ? 0 : reader.GetInt32(14)
                                    };

                                    if (!reader.IsDBNull(9))
                                    {
                                        sideData.DetectPoints = JsonConvert.DeserializeObject<List<DetectInfo>>(reader.GetString(9));
                                    }
                                    else
                                    {
                                        sideData.DetectPoints = new List<DetectInfo>();
                                    }
                                    panelRecord.Sides.Add(sideData);
                                }
                            }
                        }
                    }

                    tcs.SetResult(panelRecords.Values.ToList());
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
        /// 从导出的CSV数据导入到数据库
        /// 解析 Panels 和 PanelSides 数据，并通过 SavePanelSide 存储
        /// </summary>
        /// <param name="panelsCsvLines">Panels 表的 CSV 行数据（格式: Id,MachineId,SerialNumber,LotNumber,ProductSerial,DetectionDate,PathIndex,AviCreationTime）</param>
        /// <param name="panelSidesCsvLines">PanelSides 表的 CSV 行数据（格式: Id,PanelId,Side,HeatPoints,AviState,AiState,VvsState,VrsState,FinalState）</param>
        /// <param name="progressCallback">进度回调 (percent, message)</param>
        public Task<(int success, int failed)> ImportFromCsvData(string[] panelsCsvLines, string[] panelSidesCsvLines, Action<int, string> progressCallback = null)
        {
            var tcs = new TaskCompletionSource<(int success, int failed)>();

            if (panelsCsvLines == null || panelSidesCsvLines == null)
            {
                LogTextHelper.Error("ImportFromCsvData: 输入数据为空");
                tcs.SetResult((0, 0));
                return tcs.Task;
            }

            // 将所有处理都放入数据库队列，在单独线程执行
            _dbQueue.Add(conn =>
            {
                int totalSaved = 0;
                int totalFailed = 0;
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    progressCallback?.Invoke(20, "解析 Panels 数据...");

                    // 第一步：解析 Panels 数据（通常数据量小，不需要并行）
                    var panelsDict = new Dictionary<int, (string MachineId, string SerialNumber, string LotNumber, string ProductSerial, DateTime DetectionDate, string PathIndex, DateTime? AviCreationTime)>();

                    foreach (var line in panelsCsvLines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            var parts = line.Split(',');
                            if (parts.Length < 8) continue;

                            int pId = int.Parse(parts[0]);
                            panelsDict[pId] = (
                                parts[1],
                                parts[2],
                                parts[3],
                                parts[4],
                                DateTime.Parse(parts[5]),
                                parts[6],
                                string.IsNullOrWhiteSpace(parts[7]) ? (DateTime?)null : DateTime.Parse(parts[7])
                            );
                        }
                        catch { }
                    }

                    LogTextHelper.Info($"[性能] 解析 Panels 耗时: {stopwatch.ElapsedMilliseconds}ms, 共 {panelsDict.Count} 条");
                    stopwatch.Restart();

                    progressCallback?.Invoke(35, $"并行解析 PanelSides... ({panelSidesCsvLines.Length} 行)");

                    // 第二步：并行解析 PanelSides 数据
                    int totalLines = panelSidesCsvLines.Length;
                    int processedCount = 0;
                    int parseErrorCount = 0;
                    int panelNotFoundCount = 0;

                    // 调试：打印第一行的解析结果
                    if (totalLines > 0)
                    {
                        var debugLine = panelSidesCsvLines[0];
                        var debugParts = ParseCsvLineWithJson(debugLine);
                        LogTextHelper.Info($"[调试] 第一行解析: 共 {debugParts.Length} 个字段");
                        for (int d = 0; d < Math.Min(debugParts.Length, 5); d++)
                        {
                            LogTextHelper.Info($"[调试] 字段[{d}]: {(debugParts[d].Length > 100 ? debugParts[d].Substring(0, 100) + "..." : debugParts[d])}");
                        }
                        if (debugParts.Length >= 2)
                        {
                            int debugPanelId = int.Parse(debugParts[1]);
                            LogTextHelper.Info($"[调试] PanelId={debugPanelId}, 是否存在于 panelsDict: {panelsDict.ContainsKey(debugPanelId)}");
                        }
                    }

                    // 预分配数组，避免 ConcurrentBag 的开销
                    var parsedResults = new (PanelSideRecord Record, string HeatPointsJson)?[totalLines];

                    Parallel.For(0, totalLines, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                    {
                        var line = panelSidesCsvLines[i];
                        if (string.IsNullOrWhiteSpace(line)) return;

                        try
                        {
                            // 快速解析 CSV（针对固定格式优化）
                            var parts = ParseCsvLineWithJson(line);
                            if (parts.Length < 9)
                            {
                                Interlocked.Increment(ref parseErrorCount);
                                return;
                            }

                            int pId = int.Parse(parts[1]);
                            if (!panelsDict.TryGetValue(pId, out var panelInfo))
                            {
                                Interlocked.Increment(ref panelNotFoundCount);
                                return;
                            }

                            string side = parts[2];
                            string heatPointsJson = parts[3];

                            // 直接使用原始 JSON，不在解析阶段反序列化
                            string cleanedJson = string.IsNullOrWhiteSpace(heatPointsJson) ? "[]" : heatPointsJson.Replace("'\"", "\"").Replace("\"'", "\"");

                            var record = new PanelSideRecord
                            {
                                MachineId = panelInfo.MachineId,
                                SerialNumber = panelInfo.SerialNumber,
                                LotNumber = panelInfo.LotNumber,
                                ProductSerial = panelInfo.ProductSerial,
                                DetectionDate = panelInfo.DetectionDate,
                                PathIndex = panelInfo.PathIndex,
                                AviCreationTime = panelInfo.AviCreationTime,
                                Side = side,
                                Data = new SideData
                                {
                                    Side = side,
                                    DetectPoints = null, // 不在这里反序列化
                                    AviState = int.Parse(parts[4]),
                                    AiState = int.Parse(parts[5]),
                                    VvsState = int.Parse(parts[6]),
                                    VrsState = int.Parse(parts[7]),
                                    FinalState = int.Parse(parts[8])
                                }
                            };

                            parsedResults[i] = (record, cleanedJson);

                            int count = Interlocked.Increment(ref processedCount);
                            if (count % 10000 == 0)
                                progressCallback?.Invoke(35 + (int)((double)count / totalLines * 10), $"解析: {count}/{totalLines}");
                        }
                        catch { Interlocked.Increment(ref parseErrorCount); }
                    });

                    LogTextHelper.Info($"[调试] 解析结果: 成功={processedCount}, 解析错误={parseErrorCount}, Panel未找到={panelNotFoundCount}");

                    // 过滤掉 null 值
                    var serializedRecords = parsedResults.Where(x => x.HasValue).Select(x => x.Value).ToList();

                    LogTextHelper.Info($"[性能] 解析 PanelSides 耗时: {stopwatch.ElapsedMilliseconds}ms, 共 {serializedRecords.Count} 条");
                    stopwatch.Restart();

                    if (serializedRecords.Count == 0)
                    {
                        LogTextHelper.Warn("ImportFromCsvData: 没有有效记录可导入");
                        tcs.SetResult((0, 0));
                        return;
                    }

                    progressCallback?.Invoke(50, $"开始导入 {serializedRecords.Count} 条记录...");
                    LogTextHelper.Info($"ImportFromCsvData: 开始批量保存 {serializedRecords.Count} 条记录...");

                    // 第三步：批量插入数据库（增大批次大小）
                    int batchSize = 500;
                    int totalRecords = serializedRecords.Count;

                    for (int i = 0; i < totalRecords; i += batchSize)
                    {
                        int currentBatchSize = Math.Min(batchSize, totalRecords - i);
                        NpgsqlTransaction transaction = null;

                        try
                        {
                            transaction = conn.BeginTransaction();

                            for (int j = 0; j < currentBatchSize; j++)
                            {
                                var (record, heatPointsJson) = serializedRecords[i + j];

                                _upsertPanelCmd.Transaction = transaction;
                                _upsertPanelCmd.Parameters["@MachineId"].Value = record.MachineId ?? string.Empty;
                                _upsertPanelCmd.Parameters["@SN"].Value = record.SerialNumber;
                                _upsertPanelCmd.Parameters["@Lot"].Value = record.LotNumber ?? string.Empty;
                                _upsertPanelCmd.Parameters["@Date"].Value = record.DetectionDate;
                                _upsertPanelCmd.Parameters["@ProductSerial"].Value = (object)record.ProductSerial ?? DBNull.Value;
                                _upsertPanelCmd.Parameters["@PathIndex"].Value = (object)record.PathIndex ?? DBNull.Value;
                                _upsertPanelCmd.Parameters["@AviCreationTime"].Value = (object)record.AviCreationTime ?? DBNull.Value;
                                long panelId = Convert.ToInt64(_upsertPanelCmd.ExecuteScalar());

                                _upsertPanelSideCmd.Transaction = transaction;
                                _upsertPanelSideCmd.Parameters["@PanelId"].Value = panelId;
                                _upsertPanelSideCmd.Parameters["@Side"].Value = record.Side;
                                _upsertPanelSideCmd.Parameters["@HeatPoints"].Value = heatPointsJson;
                                _upsertPanelSideCmd.Parameters["@AviState"].Value = record.Data.AviState;
                                _upsertPanelSideCmd.Parameters["@AiState"].Value = record.Data.AiState;
                                _upsertPanelSideCmd.Parameters["@VvsState"].Value = record.Data.VvsState;
                                _upsertPanelSideCmd.Parameters["@VrsState"].Value = record.Data.VrsState;
                                _upsertPanelSideCmd.Parameters["@FinalState"].Value = record.Data.FinalState;
                                _upsertPanelSideCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            totalSaved += currentBatchSize;

                            int percent = 50 + (int)((double)(i + currentBatchSize) / totalRecords * 45);
                            progressCallback?.Invoke(percent, $"已导入 {totalSaved}/{totalRecords}");
                        }
                        catch (Exception ex)
                        {
                            LogTextHelper.Error($"ImportFromCsvData: 批量保存失败，批次 {i / batchSize + 1}, 错误: {ex.Message}");
                            totalFailed += currentBatchSize;
                            try { transaction?.Rollback(); } catch { }
                        }
                        finally
                        {
                            transaction?.Dispose();
                        }
                    }

                    LogTextHelper.Info($"[性能] 数据库写入耗时: {stopwatch.ElapsedMilliseconds}ms");
                    LogTextHelper.Info($"ImportFromCsvData: 导入完成，成功 {totalSaved} 条，失败 {totalFailed} 条");
                    progressCallback?.Invoke(100, $"完成! 成功 {totalSaved}, 失败 {totalFailed}");
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"ImportFromCsvData: 发生异常: {ex.Message}");
                    progressCallback?.Invoke(100, $"错误: {ex.Message}");
                }

                tcs.SetResult((totalSaved, totalFailed));
            });

            return tcs.Task;
        }

        /// <summary>
        /// 解析普通 CSV 行（不含复杂 JSON）
        /// </summary>
        private string[] ParseCsvLine(string line)
        {
            return line.Split(',');
        }

        /// <summary>
        /// 解析包含 JSON 字段的 CSV 行
        /// JSON 字段用双引号包裹，内部使用 '" 和 "' 作为属性引号
        /// 格式: id,panelId,side,"[{'"key'":'"value'",...}]",aviState,...
        /// </summary>
        private string[] ParseCsvLineWithJson(string line)
        {
            var result = new List<string>();
            bool inJsonField = false;
            var current = new System.Text.StringBuilder();
            int i = 0;

            while (i < line.Length)
            {
                char c = line[i];

                // 检测 JSON 字段的开始: ,"[ 或行首 "[
                if (!inJsonField && c == '"' && i + 1 < line.Length && line[i + 1] == '[')
                {
                    inJsonField = true;
                    i++; // 跳过开头的 "
                    continue;
                }

                // 检测 JSON 字段的结束: ]"
                if (inJsonField && c == ']' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append(c); // 添加 ]
                    inJsonField = false;
                    i += 2; // 跳过 ]"
                    continue;
                }

                // 检测 JSON 字段结束（空数组情况）: "[]"
                if (inJsonField && c == ']' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append(c);
                    inJsonField = false;
                    i += 2;
                    continue;
                }

                // 普通逗号分隔（不在 JSON 字段内）
                if (c == ',' && !inJsonField)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    i++;
                    continue;
                }

                current.Append(c);
                i++;
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        /// <summary>
        /// 获取数据库中所有唯一的 MachineId
        /// </summary>
        /// <returns>所有不重复的机台ID列表</returns>
        public Task<List<string>> GetAllMachineIds()
        {
            var tcs = new TaskCompletionSource<List<string>>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var machineIds = new List<string>();
                    using (var cmd = new NpgsqlCommand("SELECT DISTINCT MachineId FROM Panels ORDER BY MachineId", connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                {
                                    machineIds.Add(reader.GetString(0));
                                }
                            }
                        }
                    }
                    tcs.SetResult(machineIds);
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"获取机台ID列表失败: {ex.Message}");
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
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
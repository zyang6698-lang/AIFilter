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

        // 预编译的 SQL 命令（用于提高性能）
        private NpgsqlCommand _upsertPanelCmd;
        private NpgsqlCommand _upsertPanelSideCmd;

        public DatabaseHelper()
        {
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
            using (var connection = new NpgsqlConnection(connectionString))
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
        /// 获取数据库统计指标
        /// </summary>
        public Task<DatabaseStats> GetDatabaseStats()
        {
            var tcs = new TaskCompletionSource<DatabaseStats>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var stats = new DatabaseStats
                    {
                        QueueLength = _dbQueue.Count,
                        CollectedAt = DateTime.Now
                    };

                    // 1. 获取数据库大小
                    using (var cmd = new NpgsqlCommand($"SELECT pg_database_size('{_config.Database}')", connection))
                    {
                        stats.DatabaseSizeBytes = Convert.ToInt64(cmd.ExecuteScalar() ?? 0);
                    }

                    // 2. 获取活动连接数
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT count(*) FROM pg_stat_activity
                        WHERE datname = @dbname AND state = 'active'", connection))
                    {
                        cmd.Parameters.AddWithValue("@dbname", _config.Database);
                        stats.ActiveConnections = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    }

                    // 3. 获取总连接数
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT count(*) FROM pg_stat_activity
                        WHERE datname = @dbname", connection))
                    {
                        cmd.Parameters.AddWithValue("@dbname", _config.Database);
                        stats.TotalConnections = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    }

                    // 4. 获取表统计信息
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT
                            relname as table_name,
                            n_live_tup as row_count,
                            n_dead_tup as dead_rows,
                            seq_scan,
                            idx_scan,
                            n_tup_ins as inserts,
                            n_tup_upd as updates,
                            n_tup_del as deletes
                        FROM pg_stat_user_tables
                        WHERE schemaname = 'public'
                        ORDER BY n_live_tup DESC", connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                stats.TableStats.Add(new TableStats
                                {
                                    TableName = reader.GetString(0),
                                    RowCount = reader.IsDBNull(1) ? 0 : reader.GetInt64(1),
                                    DeadRows = reader.IsDBNull(2) ? 0 : reader.GetInt64(2),
                                    SequentialScans = reader.IsDBNull(3) ? 0 : reader.GetInt64(3),
                                    IndexScans = reader.IsDBNull(4) ? 0 : reader.GetInt64(4),
                                    Inserts = reader.IsDBNull(5) ? 0 : reader.GetInt64(5),
                                    Updates = reader.IsDBNull(6) ? 0 : reader.GetInt64(6),
                                    Deletes = reader.IsDBNull(7) ? 0 : reader.GetInt64(7)
                                });
                            }
                        }
                    }

                    // 5. 获取索引使用情况
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT
                            indexrelname as index_name,
                            relname as table_name,
                            idx_scan as scans,
                            idx_tup_read as tuples_read,
                            idx_tup_fetch as tuples_fetched
                        FROM pg_stat_user_indexes
                        WHERE schemaname = 'public'
                        ORDER BY idx_scan DESC", connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                stats.IndexStats.Add(new IndexStats
                                {
                                    IndexName = reader.GetString(0),
                                    TableName = reader.GetString(1),
                                    Scans = reader.IsDBNull(2) ? 0 : reader.GetInt64(2),
                                    TuplesRead = reader.IsDBNull(3) ? 0 : reader.GetInt64(3),
                                    TuplesFetched = reader.IsDBNull(4) ? 0 : reader.GetInt64(4)
                                });
                            }
                        }
                    }

                    // 6. 获取缓存命中率
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT
                            ROUND(100.0 * sum(blks_hit) / NULLIF(sum(blks_hit) + sum(blks_read), 0), 2) as cache_hit_ratio
                        FROM pg_stat_database
                        WHERE datname = @dbname", connection))
                    {
                        cmd.Parameters.AddWithValue("@dbname", _config.Database);
                        var result = cmd.ExecuteScalar();
                        stats.CacheHitRatio = result == DBNull.Value ? 0 : Convert.ToDouble(result);
                    }

                    // 7. 获取事务统计
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT
                            xact_commit,
                            xact_rollback,
                            tup_returned,
                            tup_fetched,
                            tup_inserted,
                            tup_updated,
                            tup_deleted
                        FROM pg_stat_database
                        WHERE datname = @dbname", connection))
                    {
                        cmd.Parameters.AddWithValue("@dbname", _config.Database);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                stats.TransactionsCommitted = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                                stats.TransactionsRolledBack = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                                stats.TuplesReturned = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
                                stats.TuplesFetched = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
                                stats.TuplesInserted = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
                                stats.TuplesUpdated = reader.IsDBNull(5) ? 0 : reader.GetInt64(5);
                                stats.TuplesDeleted = reader.IsDBNull(6) ? 0 : reader.GetInt64(6);
                            }
                        }
                    }

                    tcs.SetResult(stats);
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"获取数据库统计信息失败: {ex.Message}");
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
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

                    // 使用窗口函数一次性查询所有数据，避免两次查询
                    // 计算每个 Panel 是否两面都 AI OK，并同时获取 HeatPoints
                    var sql = @"
                        SELECT ps.PanelId, ps.HeatPoints,
                               (SELECT CASE WHEN COUNT(*) = 2 AND SUM(CASE WHEN AiState = 1 THEN 1 ELSE 0 END) = 2 THEN 1 ELSE 0 END
                                FROM PanelSides ps2 WHERE ps2.PanelId = ps.PanelId) AS AiOkBothSides
                        FROM PanelSides ps
                        JOIN Panels p ON ps.PanelId = p.Id
                        WHERE p.DetectionDate BETWEEN @Start AND @End" + (string.IsNullOrEmpty(machineId) ? string.Empty : " AND p.MachineId = @MachineId");

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);
                        if (!string.IsNullOrEmpty(machineId)) cmd.Parameters.AddWithValue("@MachineId", machineId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int defectCount = 0;

                                if (!reader.IsDBNull(1))
                                {
                                    var heatPointsJson = reader.GetString(1);
                                    var points = JsonConvert.DeserializeObject<List<DetectInfo>>(heatPointsJson);
                                    defectCount = points?.Count ?? 0;
                                }

                                totalDefects += defectCount;

                                bool isAiOk = reader.GetInt32(2) == 1;
                                if (isAiOk)
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
        /// 优化: 使用递归 CTE 进行索引跳跃扫描，避免全表扫描
        /// </summary>
        public Task<List<string>> GetAllMachineIds()
        {
            var tcs = new TaskCompletionSource<List<string>>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    var machineIds = new List<string>();
                    // 使用递归 CTE 实现索引跳跃扫描 (Index Skip Scan)
                    // 对于 MachineId 基数较低的情况，性能远优于 DISTINCT
                    const string sql = @"
                        WITH RECURSIVE machine_cte AS (
                            (SELECT MachineId FROM Panels ORDER BY MachineId LIMIT 1)
                            UNION ALL
                            SELECT (SELECT MachineId FROM Panels WHERE MachineId > machine_cte.MachineId ORDER BY MachineId LIMIT 1)
                            FROM machine_cte
                            WHERE machine_cte.MachineId IS NOT NULL
                        )
                        SELECT MachineId FROM machine_cte WHERE MachineId IS NOT NULL";
                    using (var cmd = new NpgsqlCommand(sql, connection))
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
        /// 1.	如果有一个 FinalState 为3，则添加到未检测结果数
        /// 2.	如果有一个 FinalState 为2，则添加到过滤后仍NG结果数
        /// 3.	如果有两个 AviState 为1 (AVI OK)，则添加到AVI OK的结果数
        /// 4.	剩余情况，添加到过滤后OK的数 所以总共获取5个数字
        /// 优化: 使用 SQL 聚合在数据库端完成统计，避免传输大量数据到内存
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
                    // 使用 SQL 聚合在数据库端完成所有统计
                    const string sql = @"
                    WITH panel_stats AS (
                        SELECT
                            p.Id,
                            COUNT(ps.Id) AS side_count,
                            MAX(CASE WHEN ps.FinalState = 3 THEN 1 ELSE 0 END) AS has_uninspected,
                            MAX(CASE WHEN ps.FinalState = 2 THEN 1 ELSE 0 END) AS has_ng,
                            SUM(CASE WHEN ps.AviState = 1 THEN 1 ELSE 0 END) AS avi_ok_count
                        FROM Panels p
                        JOIN PanelSides ps ON p.Id = ps.PanelId
                        WHERE p.DetectionDate BETWEEN @Start AND @End
                        GROUP BY p.Id
                    )
                    SELECT
                        COUNT(*) AS total_sn_count,
                        SUM(CASE WHEN has_uninspected = 1 THEN 1 ELSE 0 END) AS uninspected_count,
                        SUM(CASE WHEN has_uninspected = 0 AND has_ng = 1 THEN 1 ELSE 0 END) AS still_ng_count,
                        SUM(CASE WHEN has_uninspected = 0 AND has_ng = 0 AND avi_ok_count = 2 THEN 1 ELSE 0 END) AS avi_ok_count
                    FROM panel_stats";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int totalSnCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                int uninspectedCount = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetInt64(1));
                                int stillNgCount = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetInt64(2));
                                int aviOkCount = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetInt64(3));
                                int filteredOkCount = totalSnCount - uninspectedCount - stillNgCount - aviOkCount;
                                tcs.SetResult((totalSnCount, uninspectedCount, stillNgCount, aviOkCount, filteredOkCount));
                            }
                            else
                            {
                                tcs.SetResult((0, 0, 0, 0, 0));
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

        /// <summary>
        /// 优化: 使用 SQL 聚合在数据库端完成统计，避免传输大量数据到内存
        /// </summary>
        public Task<(int totalSnCount, int aviOkCount, int aiOkCount, int aiNgCount, int uninspectedCount)> GetSnStateCountsByTime(DateTime start, DateTime end)
        {
            var tcs = new TaskCompletionSource<(int, int, int, int, int)>();
            _dbQueue.Add(connection =>
            {
                try
                {
                    // 使用 SQL 聚合在数据库端完成所有统计
                    const string sql = @"
                    WITH panel_stats AS (
                        SELECT
                            p.Id,
                            COUNT(ps.Id) AS side_count,
                            SUM(CASE WHEN ps.AviState = 0 THEN 1 ELSE 0 END) AS uninspected_sides,
                            SUM(CASE WHEN ps.AviState = 1 THEN 1 ELSE 0 END) AS avi_ok_sides,
                            MAX(CASE WHEN ps.FinalState = 2 THEN 1 ELSE 0 END) AS has_final_ng
                        FROM Panels p
                        JOIN PanelSides ps ON p.Id = ps.PanelId
                        WHERE p.DetectionDate BETWEEN @Start AND @End
                        GROUP BY p.Id
                    )
                    SELECT
                        COUNT(*) AS total_sn_count,
                        SUM(CASE WHEN side_count = 2 AND avi_ok_sides = 2 THEN 1 ELSE 0 END) AS avi_ok_count,
                        SUM(CASE WHEN uninspected_sides = 0 AND avi_ok_sides < 2 AND has_final_ng = 0 THEN 1 ELSE 0 END) AS ai_ok_count,
                        SUM(CASE WHEN uninspected_sides = 0 AND avi_ok_sides < 2 AND has_final_ng = 1 THEN 1 ELSE 0 END) AS ai_ng_count,
                        SUM(CASE WHEN uninspected_sides > 0 THEN 1 ELSE 0 END) AS uninspected_count
                    FROM panel_stats";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int totalSnCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                int aviOkCount = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetInt64(1));
                                int aiOkCount = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetInt64(2));
                                int aiNgCount = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetInt64(3));
                                int uninspectedCount = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetInt64(4));
                                tcs.SetResult((totalSnCount, aviOkCount, aiOkCount, aiNgCount, uninspectedCount));
                            }
                            else
                            {
                                tcs.SetResult((0, 0, 0, 0, 0));
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
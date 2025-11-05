using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace DeepsightSqlite
{
    public class DatabaseHelper
    {
        private readonly string dbFilePath;
        private readonly string connectionString;

        public DatabaseHelper(string databaseFileName)
        {
            // Get the base directory of the application
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            dbFilePath = Path.Combine(baseDir, databaseFileName);
            connectionString = $"Data Source={dbFilePath};Version=3;";
        }

        public void InitializeDatabase()
        {
            if (!File.Exists(dbFilePath))
            {
                SQLiteConnection.CreateFile(dbFilePath);
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS ProcessingRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EmployeeId TEXT NOT NULL,
                    BatchInfo TEXT,
                    StartTime DATETIME NOT NULL,
                    EndTime DATETIME NOT NULL,
                    Quantity INTEGER NOT NULL
                );";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddRecord(ProcessingRecord record)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertQuery = @"
                INSERT INTO ProcessingRecords (EmployeeId, BatchInfo, StartTime, EndTime, Quantity)
                VALUES (@EmployeeId, @BatchInfo, @StartTime, @EndTime, @Quantity);";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@EmployeeId", record.EmployeeId);
                    command.Parameters.AddWithValue("@BatchInfo", record.BatchInfo);
                    command.Parameters.AddWithValue("@StartTime", record.StartTime);
                    command.Parameters.AddWithValue("@EndTime", record.EndTime);
                    command.Parameters.AddWithValue("@Quantity", record.Quantity);

                    command.ExecuteNonQuery();
                }
            }
        }

        public List<ProcessingRecord> GetRecords()
        {
            var records = new List<ProcessingRecord>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT * FROM ProcessingRecords;";

                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            records.Add(new ProcessingRecord
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                EmployeeId = Convert.ToString(reader["EmployeeId"]),
                                BatchInfo = Convert.ToString(reader["BatchInfo"]),
                                StartTime = Convert.ToDateTime(reader["StartTime"]),
                                EndTime = Convert.ToDateTime(reader["EndTime"]),
                                Quantity = Convert.ToInt32(reader["Quantity"])
                            });
                        }
                    }
                }
            }

            return records;
        }
    }
}

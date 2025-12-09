using CsvHelper;
using CsvHelper.Configuration;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DeepSightDB
{
    public static class AnalyticsHelper
    {
        public class ReadAndProcessCSV
        {
            public static IEnumerable<string> ProcessCsvFiles(string directoryPath)
            {
                try
                {
                    var csvFiles = Directory.EnumerateFiles(directoryPath, "*.csv", SearchOption.AllDirectories);

                    return csvFiles;
                }
                catch (UnauthorizedAccessException)
                {
                    LogTextHelper.Info("错误：没有权限访问该文件夹。");
                    return null;
                }
                catch (DirectoryNotFoundException)
                {
                    LogTextHelper.Info("错误：找不到指定的文件夹。");
                    return null;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Info($"发生未知错误: {ex.Message}");
                    return null;
                }
            }

            public static List<EmployeeReport> ReadCsvFile(string filePath)
            {
                var coding = Encoding.GetEncoding("GB18030");

                var records = new List<EmployeeReport>();

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Encoding = coding,

                    HasHeaderRecord = true,
                };

                using (var reader = new StreamReader(filePath, coding))

                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<CsvRecordMap>();

                    records = csv.GetRecords<EmployeeReport>().ToList();
                }

                return records;
            }
        }

    }


    public sealed class CsvRecordMap : ClassMap<EmployeeReport>
    {
        public CsvRecordMap()
        {
            Map(m => m.ID).Index(1);

            Map(m => m.SN).Index(2);

            Map(m => m.AllNGNumber).Index(3);

            Map(m => m.StartTime).Index(4);

            Map(m => m.EndTime).Index(5);
        }
    }

    public class EmployeeReport
    {
        public string ID { get; set; }
        public string SN { get; set; }
        public int AllNGNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int VRSOKNumber { get; set; }
    }


}

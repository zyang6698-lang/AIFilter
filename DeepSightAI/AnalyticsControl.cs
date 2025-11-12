using DeepSightModel;
using DeepSightTool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DeepSightModel.AnalyticsHelper;

namespace DeepSightAI
{
    public partial class AnalyticsControl : UserControl
    {
        ConcurrentQueue<EmployeeReport> dataQueues = new ConcurrentQueue<EmployeeReport>();
        public AnalyticsControl()
        {
            InitializeComponent();
        }

        private void btnShowAnalytics_Click(object sender, EventArgs e)
        {
            try
            {
                string analyticsAppPath = Path.Combine(Application.StartupPath, "Analytics", "Deepsight.Analytics.UI.exe");
                if (File.Exists(analyticsAppPath))
                {
                    Process.Start(analyticsAppPath);
                }
                else
                {
                    MessageBox.Show("分析工具不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("启动分析工具失败", ex);
                MessageBox.Show($"启动分析工具失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnReadEmployeeData_Click(object sender, EventArgs e)
        {
            var AllEmployeeReports = new List<EmployeeReport>();

            var dlg = new FolderBrowserDialog();
            string folder = string.Empty;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                folder = dlg.SelectedPath;
            }

            var allCsv = ReadAndProcessCSV.ProcessCsvFiles(folder);
            LogTextHelper.Info("获取了所有的csv文件路径");

            if (allCsv != null)
            {
                LogTextHelper.Info("开始读取并处理csv文件");

                await Task.Run(() =>
                {
                    foreach (string filePath in allCsv)
                    {
                        var list = ReadAndProcessCSV.ReadCsvFile(filePath);
                        AllEmployeeReports.AddRange(list);
                    }
                });

                foreach (var record in AllEmployeeReports)
                {
                    dataQueues.Enqueue(record); // Enqueue() 将元素添加到队尾
                    Machine.master.workClass.SaveEmployeeReport(record);
                    LogTextHelper.Info($"员工报点数据已解析,员工工号：{record.ID}，SN：{record.SN}，总NG数：{record.AllNGNumber}，起始时间：{record.StartTime}，结束时间：{record.StartTime}");
                }

                LogTextHelper.Info($"数据已经全部解析，总数为{dataQueues.Count}");
            }
            else
            {
                LogTextHelper.Error("未找到csv文件！");
            }
        }



        private void btnTestDB_Click(object sender, EventArgs e)
        {
            //Machine.master.workClass.TestDatabaseReadWrite();
            //Machine.master.workClass.TestDatabaseWrite();
            Machine.master.workClass.GenerateVRSTestData();
        }
    }
}

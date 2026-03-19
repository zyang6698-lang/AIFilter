using DeepSightDB;
using DeepSightModel;
using DeepSightTool;
using DeepSightWorkLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DeepSightDB.AnalyticsHelper;

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

            btnReadEmployeeData.Enabled = false;
            progressBarImport.Value = 0;
            UpdateProgress(0, "开始读取员工数据...");

            try
            {
                // 收集所有启用的配置路径
                var enabledPaths = Machine.aviconfig.WatchPaths.Where(cfg => cfg.IsEnable).ToList();
                if (enabledPaths.Count == 0)
                {
                    UpdateProgress(0, "没有启用的监控路径");
                    return;
                }

                int pathIndex = 0;
                foreach (var cfg in enabledPaths)
                {
                    var folder = cfg.DeepsightAgentDataWorkspace;
                    UpdateProgress(5, $"正在扫描文件夹: {folder}");

                    var allCsvEnum = ReadAndProcessCSV.ProcessCsvFiles(folder);
                    LogTextHelper.Info($"获取了{folder}所有的csv文件路径");

                    var allCsv = allCsvEnum?.ToList();
                    if (allCsv != null && allCsv.Count > 0)
                    {
                        LogTextHelper.Info("开始读取并处理csv文件");
                        int totalFiles = allCsv.Count;
                        int processedFiles = 0;

                        await Task.Run(() =>
                        {
                            foreach (string filePath in allCsv)
                            {
                                var list = ReadAndProcessCSV.ReadCsvFile(filePath);
                                AllEmployeeReports.AddRange(list);
                                processedFiles++;
                                int percent = 10 + (int)(processedFiles * 40.0 / totalFiles);
                                UpdateProgress(percent, $"读取CSV文件 ({processedFiles}/{totalFiles})...");
                            }
                        });

                        UpdateProgress(50, $"开始保存员工数据，共 {AllEmployeeReports.Count} 条...");

                        int totalRecords = AllEmployeeReports.Count;
                        int savedRecords = 0;

                        foreach (var record in AllEmployeeReports)
                        {
                            dataQueues.Enqueue(record);
                            await Task.Run(() => Machine.master.SaveEmployeeReport(record));
                            savedRecords++;
                            int percent = 50 + (int)(savedRecords * 45.0 / totalRecords);
                            UpdateProgress(percent, $"保存员工数据 ({savedRecords}/{totalRecords})...");
                            LogTextHelper.Info($"员工报点数据已解析,员工工号：{record.ID}，SN：{record.SN}，总NG数：{record.AllNGNumber}，起始时间：{record.StartTime}，结束时间：{record.StartTime}");
                        }

                        LogTextHelper.Info($"数据已经全部解析，总数为{dataQueues.Count}");
                    }
                    else
                    {
                        LogTextHelper.Error("未找到csv文件！");
                        UpdateProgress(0, "未找到csv文件");
                    }

                    pathIndex++;
                }

                UpdateProgress(100, $"完成！共处理 {AllEmployeeReports.Count} 条员工数据");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("读取员工数据失败", ex);
                UpdateProgress(0, $"读取失败: {ex.Message}");
                MessageBox.Show($"读取员工数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnReadEmployeeData.Enabled = true;
            }
        }



        private void btnTestDB_Click(object sender, EventArgs e)
        {
            Machine.master.GenerateVRSTestData();
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            // CSV 文件路径
            string basePath = @"C:\Users\zhangyang\Desktop\AI过滤软件资料\排查\log";
            string panelsFilePath = Path.Combine(basePath, "panels.csv");
            string panelSidesFilePath = Path.Combine(basePath, "panelsides.csv");

            // 检查文件是否存在
            if (!File.Exists(panelsFilePath))
            {
                MessageBox.Show($"Panels 文件不存在: {panelsFilePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!File.Exists(panelSidesFilePath))
            {
                MessageBox.Show($"PanelSides 文件不存在: {panelSidesFilePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnTest.Enabled = false;
            progressBarImport.Value = 0;
            lblImportStatus.Text = "开始导入...";

            try
            {
                var dbHelper = Machine.master.GetDatabaseService();
                if (dbHelper == null)
                {
                    MessageBox.Show("无法获取 DatabaseHelper 实例", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 全部在后台线程执行，包括文件读取和解析
                var result = await Task.Run(async () =>
                {
                    UpdateProgress(5, "读取 Panels 文件...");
                    string[] panelsLines = File.ReadAllLines(panelsFilePath,System.Text. Encoding.UTF8);

                    UpdateProgress(10, "读取 PanelSides 文件...");
                    string[] panelSidesLines = File.ReadAllLines(panelSidesFilePath, System.Text.Encoding.UTF8);

                    UpdateProgress(15, $"文件读取完成: Panels={panelsLines.Length}, PanelSides={panelSidesLines.Length}");
                    LogTextHelper.Info($"开始导入 CSV: Panels={panelsLines.Length}, PanelSides={panelSidesLines.Length}");

                    // 调用导入方法
                    return await dbHelper.ImportFromCsvData(panelsLines, panelSidesLines, UpdateProgress);
                });

                MessageBox.Show($"CSV 数据导入完成!\n成功: {result.success} 条\n失败: {result.failed} 条",
                    "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"导入 CSV 数据失败: {ex.Message}", ex);
                MessageBox.Show($"导入失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateProgress(0, $"导入失败: {ex.Message}");
            }
            finally
            {
                btnTest.Enabled = true;
            }
        }

        private void UpdateProgress(int percent, string status)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateProgress(percent, status)));
                return;
            }
            progressBarImport.Value = Math.Min(percent, 100);
            lblImportStatus.Text = status;
        }

        #region 图片压缩功能
        // 获取相对路径（.NET Framework 无 Path.GetRelativePath）
        private static string GetRelativePath(string basePath, string fullPath)
        {
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                basePath += Path.DirectorySeparatorChar;
            if (fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                return fullPath.Substring(basePath.Length);
            return fullPath; // fallback
        }

        private void btnZipPic_Click(object sender, EventArgs e)
        {
            try
            {
                int quality = 75; // 默认 JPEG 质量
                string sourceFolderPath = SelectFolder();
                if (string.IsNullOrEmpty(sourceFolderPath)) return;

                string parentPath = Path.GetDirectoryName(sourceFolderPath);
                string sourceFolderName = Path.GetFileName(sourceFolderPath);
                string destinationFolderName = sourceFolderName + "_compressed";
                string destinationFolderPath = Path.Combine(parentPath, destinationFolderName);
                if (!Directory.Exists(destinationFolderPath)) Directory.CreateDirectory(destinationFolderPath);

                string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" };
                // 递归获取所有文件
                var imageFiles = Directory.EnumerateFiles(sourceFolderPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToList();

                if (imageFiles.Count == 0)
                {
                    MessageBox.Show("所选文件夹及其子文件夹中没有找到图片文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ImageCodecInfo jpegCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                if (jpegCodec == null)
                {
                    MessageBox.Show("未找到 JPEG 编码器，无法压缩。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (var encoderParams = new EncoderParameters(1))
                {
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                    int success = 0, failed = 0;
                    foreach (var imagePath in imageFiles)
                    {
                        try
                        {
                            using (Image img = Image.FromFile(imagePath))
                            {
                                string relative = GetRelativePath(sourceFolderPath, imagePath);
                                string relDir = Path.GetDirectoryName(relative) ?? string.Empty;
                                string targetDir = Path.Combine(destinationFolderPath, relDir);
                                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(imagePath);
                                string newFileName = fileNameWithoutExt + ".jpg";
                                string newFilePath = Path.Combine(targetDir, newFileName);
                                img.Save(newFilePath, jpegCodec, encoderParams);
                                success++;
                            }
                        }
                        catch (Exception exImg)
                        {
                            failed++;
                            LogTextHelper.Error($"压缩失败: {imagePath} -> {exImg.Message}");
                        }
                    }

                    MessageBox.Show($"图片压缩完成！成功:{success} 失败:{failed}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                MessageBox.Show("处理过程中发生错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string SelectFolder()
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "请选择图片所在的文件夹";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    return folderDialog.SelectedPath;
                }
            }
            return null;
        }
        #endregion

        #region 清空数据库功能
        private async void btnClearDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                // 弹出确认对话框
                DialogResult result = MessageBox.Show(
                    "确定要清空数据库吗？此操作将删除所有数据且不可恢复！",
                    "警告",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    // 禁用按钮，防止重复点击
                    btnClearDatabase.Enabled = false;
                    btnClearDatabase.Text = "清空中...";

                    try
                    {
                        // 调用清空数据库方法
                        bool success = await Machine.master.ClearAllDatabaseData();

                        if (success)
                        {
                            // 清空所有内存缓存
                            BoardStatCache.Clear();
                            SnDebugInfoCache.Clear();
                            FrHome.Instance.dic_Infos.Clear();
                            FrHome.Instance.dic_Results.Clear();
                            FrHome.Instance.dic_DetectRois.Clear();
                            FrHome.Instance.str_SN = "";

                            // 清空 DataGridView 行
                            if (FrHome.Instance.dataGridViewData.InvokeRequired)
                            {
                                FrHome.Instance.dataGridViewData.BeginInvoke(new MethodInvoker(() =>
                                    FrHome.Instance.dataGridViewData.Rows.Clear()));
                            }
                            else
                            {
                                FrHome.Instance.dataGridViewData.Rows.Clear();
                            }

                            // 清空缺陷图片显示
                            FrHome.Instance.ClearAllImages();

                            LogTextHelper.Info("数据库及所有内存缓存已清空");
                            MessageBox.Show("数据库及缓存清空成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("数据库清空失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error($"清空数据库时发生错误: {ex}");
                        MessageBox.Show($"清空数据库时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        // 恢复按钮状态
                        btnClearDatabase.Enabled = true;
                        btnClearDatabase.Text = "清空数据库";
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                MessageBox.Show("处理过程中发生错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}

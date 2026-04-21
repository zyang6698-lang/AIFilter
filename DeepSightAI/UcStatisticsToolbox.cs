using DeepSightCommunication;
using DeepSightDB;
using DeepSightModel;
using DeepSightModel.Configuration;
using DeepSightTool;
using DeepSightWorkLib;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DeepSightDB.AnalyticsHelper;

namespace DeepSightAI
{
    public partial class UcStatisticsToolbox : UserControl
    {
        ConcurrentQueue<EmployeeReport> dataQueues = new ConcurrentQueue<EmployeeReport>();
        public UcStatisticsToolbox()
        {
            InitializeComponent();
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

        #region 导出今日日志功能
        private async void btnExportLogs_Click(object sender, EventArgs e)
        {
            btnExportLogs.Enabled = false;
            UpdateProgress(0, "开始导出今日日志...");

            try
            {
                string today = DateTime.Now.ToString("yyyyMMdd");
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string logFolder = Path.Combine(baseDir, "Log");
                string configLogFolder = Path.Combine(baseDir, "人员操作配置记录");

                // 导出目标文件夹
                string exportDir = Path.Combine(baseDir, "LogExport");
                if (!Directory.Exists(exportDir))
                    Directory.CreateDirectory(exportDir);

                string zipFileName = $"Logs_{today}_{DateTime.Now:HHmmss}.zip";
                string zipFilePath = Path.Combine(exportDir, zipFileName);

                int fileCount = await Task.Run(() =>
                {
                    // 收集今日的日志文件
                    var filesToZip = new List<(string fullPath, string entryName)>();

                    // 1. 从 Log 文件夹收集今日文件（Serilog 按小时滚动，文件名含日期）
                    if (Directory.Exists(logFolder))
                    {
                        var logFiles = Directory.GetFiles(logFolder, "*.*", SearchOption.AllDirectories)
                            .Where(f =>
                            {
                                var fileName = Path.GetFileName(f);
                                return fileName.Contains(today) ||
                                       fileName.Contains(DateTime.Now.ToString("yyyy-MM-dd")) ||
                                       fileName.Contains(DateTime.Now.ToString("yyyyMMdd"));
                            })
                            .ToList();

                        // 如果按文件名没找到，按最后写入时间筛选
                        if (logFiles.Count == 0)
                        {
                            logFiles = Directory.GetFiles(logFolder, "*.*", SearchOption.AllDirectories)
                                .Where(f => File.GetLastWriteTime(f).Date == DateTime.Today)
                                .ToList();
                        }

                        foreach (var file in logFiles)
                        {
                            string relative = GetRelativePath(logFolder, file);
                            filesToZip.Add((file, Path.Combine("Log", relative)));
                        }
                    }

                    // 2. 从人员操作配置记录文件夹收集今日文件
                    if (Directory.Exists(configLogFolder))
                    {
                        var configFiles = Directory.GetFiles(configLogFolder, "*.*", SearchOption.AllDirectories)
                            .Where(f =>
                            {
                                var fileName = Path.GetFileName(f);
                                return fileName.Contains(today) ||
                                       fileName.Contains(DateTime.Now.ToString("yyyy-MM-dd")) ||
                                       File.GetLastWriteTime(f).Date == DateTime.Today;
                            })
                            .ToList();

                        foreach (var file in configFiles)
                        {
                            string relative = GetRelativePath(configLogFolder, file);
                            filesToZip.Add((file, Path.Combine("人员操作配置记录", relative)));
                        }
                    }

                    if (filesToZip.Count == 0)
                        return 0;

                    // 创建 zip 文件
                    using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                    {
                        foreach (var (fullPath, entryName) in filesToZip)
                        {
                            try
                            {
                                // 使用 FileShare.ReadWrite 读取可能被占用的日志文件
                                using (var sourceStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
                                    using (var entryStream = entry.Open())
                                    {
                                        sourceStream.CopyTo(entryStream);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogTextHelper.Error($"添加日志文件到压缩包失败: {fullPath} -> {ex.Message}");
                            }
                        }
                    }

                    return filesToZip.Count;
                });

                if (fileCount == 0)
                {
                    UpdateProgress(0, "未找到今日的日志文件");
                    MessageBox.Show("未找到今日的日志文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                UpdateProgress(100, $"导出完成！共 {fileCount} 个文件 -> {zipFileName}");
                MessageBox.Show($"今日日志导出完成！\n共 {fileCount} 个文件\n保存至: {zipFilePath}",
                    "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 打开导出文件夹并选中 zip 文件
                Process.Start("explorer.exe", $"/select,\"{zipFilePath}\"");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("导出日志失败", ex);
                UpdateProgress(0, $"导出失败: {ex.Message}");
                MessageBox.Show($"导出日志失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExportLogs.Enabled = true;
            }
        }
        #endregion

        #region 生成推理请求功能

        /// <summary>
        /// 供外部调用的公共方法，触发生成推理请求
        /// </summary>
        public void TriggerGenerateInference()
        {
            btnGenerateInference_Click(this, EventArgs.Empty);
        }

        private async void btnGenerateInference_Click(object sender, EventArgs e)
        {
            // 1. 获取 LevelDB 配置（用于发送请求的目标 DB）
            var dbConfigs = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled && db.DbName == "ai_merged_results")
                .ToList();
            if (dbConfigs.Count == 0)
            {
                MessageBox.Show("未找到已启用的 ai_merged_results 数据库配置，请先在设置中配置 LevelDB。",
                    "配置缺失", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. 收集所有启用数据库中的 MinIO IP（A/B 合并去重）作为可选列表
            var availableIps = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled)
                .SelectMany(db => new[] { db.MinioIpA, db.MinioIpB })
                .Where(ip => !string.IsNullOrWhiteSpace(ip))
                .Distinct()
                .ToList();
            if (availableIps.Count == 0)
            {
                MessageBox.Show("未在已启用的 LevelDB 配置中找到任何 MinIO IP。", "配置缺失",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. 弹出 MinIO 目录浏览器选择前缀
            var minioInstance = (Machine.master?.MinioService as MinioClass) ?? new MinioClass();
            string bucket = MinioSettings.Instance.DefaultBucket ?? "deepiresults";
            string selectedIp;
            string selectedPrefix;
            using (var picker = new DlgMinioFolderPicker(minioInstance, availableIps, bucket))
            {
                if (picker.ShowDialog(this) != DialogResult.OK) return;
                selectedIp = picker.SelectedIp;
                selectedPrefix = picker.SelectedPrefix ?? string.Empty;
            }

            btnGenerateInference.Enabled = false;
            UpdateProgress(0, $"正在扫描 {bucket}/{selectedPrefix} 下的 panel.json ...");

            try
            {
                // 4. 递归列出选中前缀下的所有对象键，筛选 *-panel.json
                var allKeys = await minioInstance.ListAllObjectKeysAsync(bucket, selectedPrefix, selectedIp);
                var panelFiles = allKeys
                    .Where(k => !string.IsNullOrEmpty(k) && k.EndsWith("-panel.json", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (panelFiles.Count == 0)
                {
                    UpdateProgress(0, "未找到 panel.json 文件");
                    MessageBox.Show("所选 MinIO 目录下未找到任何 *-panel.json 对象。", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                UpdateProgress(10, $"找到 {panelFiles.Count} 个 panel.json 文件，正在解析...");

                // 5. 从 MinIO 下载并解析，按 SN 分组
                // SN -> { "A" -> objectKey, "B" -> objectKey }
                var snGroups = new Dictionary<string, Dictionary<string, string>>();
                int parsed = 0;
                foreach (var objectKey in panelFiles)
                {
                    try
                    {
                        string json = minioInstance.ReadJsonSync(bucket, objectKey, selectedIp);
                        if (string.IsNullOrWhiteSpace(json)) continue;
                        var panelInfo = JsonConvert.DeserializeObject<RootPanelInfo>(json);
                        if (panelInfo == null || string.IsNullOrEmpty(panelInfo.SerialNumber)) continue;

                        string sn = panelInfo.SerialNumber;
                        string side = panelInfo.SideIndex?.ToUpper() ?? "";
                        // SideIndex 可能是 "A"/"B" 或 "0"/"1" 等，统一处理
                        if (side == "0") side = "A";
                        else if (side == "1") side = "B";

                        if (side != "A" && side != "B") continue;

                        if (!snGroups.ContainsKey(sn))
                            snGroups[sn] = new Dictionary<string, string>();

                        // 存为 "bucket/objectKey" 形式，下游 ParseMinioPath 以 bucket 标识切分
                        snGroups[sn][side] = $"{bucket}/{objectKey}";
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Warn($"解析 panel.json 失败: {objectKey}, 错误: {ex.Message}");
                    }
                    parsed++;
                    UpdateProgress(10 + (int)(parsed * 30.0 / panelFiles.Count),
                        $"解析文件 ({parsed}/{panelFiles.Count})...");
                }

                // 5. 筛选至少具有 A 面或 B 面的 SN
                var validEntries = snGroups
                    .Where(kv => kv.Value.ContainsKey("A") || kv.Value.ContainsKey("B"))
                    .ToList();

                if (validEntries.Count == 0)
                {
                    UpdateProgress(0, "未找到有效的SN数据");
                    MessageBox.Show("未找到有效的 SN 数据。", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int bothCount = validEntries.Count(kv => kv.Value.ContainsKey("A") && kv.Value.ContainsKey("B"));
                int onlyA = validEntries.Count(kv => kv.Value.ContainsKey("A") && !kv.Value.ContainsKey("B"));
                int onlyB = validEntries.Count(kv => !kv.Value.ContainsKey("A") && kv.Value.ContainsKey("B"));
                UpdateProgress(45, $"找到 {validEntries.Count} 个有效 SN（AB:{bothCount} 仅A:{onlyA} 仅B:{onlyB}），开始发送请求...");

                // 6. 逐个发送推理请求
                var httpClient = new HttpClass();
                var minioPort = MinioSettings.Instance.DefaultPort;
                int sent = 0, success = 0, failed = 0;

                foreach (var entry in validEntries)
                {
                    string sn = entry.Key;
                    bool hasA = entry.Value.ContainsKey("A");
                    bool hasB = entry.Value.ContainsKey("B");

                    // 对每个启用的 LevelDB 配置都发送
                    foreach (var dbConfig in dbConfigs)
                    {
                        try
                        {
                            // 构建 results_info 列表，只包含存在的面
                            // A/B 两面均使用用户在 MinIO 浏览器中选定的 IP（浏览源即请求源，保持一致）
                            var resultsInfo = new List<object>();
                            if (hasA)
                                resultsInfo.Add(new { side = "A", minio_ip = selectedIp, minio_port = minioPort, result_path = entry.Value["A"] });
                            if (hasB)
                                resultsInfo.Add(new { side = "B", minio_ip = selectedIp, minio_port = minioPort, result_path = entry.Value["B"] });

                            var valueObj = new
                            {
                                serial_number = sn,
                                results_info = resultsInfo
                            };
                            string valueStr = JsonConvert.SerializeObject(valueObj);

                            // 构建 LevelDB 请求
                            string timeKey = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            var dbInfo = new RootDbInfo
                            {
                                uniqueKey = Guid.NewGuid().ToString(),
                                db_name = "ai_merged_results",
                                operation = "put",
                                op_mode = "all_ow",
                                key = timeKey,
                                value = valueStr
                            };

                            if (httpClient.HttpPostMethod(dbConfig.Url, dbInfo, 1, out string result))
                            {
                                success++;
                                LogTextHelper.Info($"推理请求发送成功: SN={sn}, DB={dbConfig.DisplayName}");
                            }
                            else
                            {
                                failed++;
                                LogTextHelper.Warn($"推理请求发送失败: SN={sn}, DB={dbConfig.DisplayName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            LogTextHelper.Error($"推理请求发送异常: SN={sn}, 错误: {ex.Message}");
                        }

                        // 请求间隔，避免过快
                        await Task.Delay(500);
                    }

                    sent++;
                    UpdateProgress(45 + (int)(sent * 50.0 / validEntries.Count),
                        $"发送请求 ({sent}/{validEntries.Count})，成功:{success} 失败:{failed}");
                }

                UpdateProgress(100, $"完成！共 {validEntries.Count} 个SN，成功:{success} 失败:{failed}");
                MessageBox.Show($"推理请求发送完成！\n共 {validEntries.Count} 个 SN（AB:{bothCount} 仅A:{onlyA} 仅B:{onlyB}）\n成功: {success}\n失败: {failed}",
                    "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("生成推理请求失败", ex);
                UpdateProgress(0, $"失败: {ex.Message}");
                MessageBox.Show($"生成推理请求失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGenerateInference.Enabled = true;
            }
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
                            FrmHome.Instance.dic_Infos.Clear();
                            FrmHome.Instance.dic_Results.Clear();
                            FrmHome.Instance.dic_DetectRois.Clear();
                            FrmHome.Instance.str_SN = "";

                            // 清空 DataGridView 行
                            if (FrmHome.Instance.dataGridViewData.InvokeRequired)
                            {
                                FrmHome.Instance.dataGridViewData.BeginInvoke(new MethodInvoker(() =>
                                    FrmHome.Instance.dataGridViewData.Rows.Clear()));
                            }
                            else
                            {
                                FrmHome.Instance.dataGridViewData.Rows.Clear();
                            }

                            // 清空缺陷图片显示
                            FrmHome.Instance.ClearAllImages();

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

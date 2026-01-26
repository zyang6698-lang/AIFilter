using DeepSightTool;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    /// <summary>
    /// 基础参数
    /// </summary>
    public partial class FrBaseConfig : Form
    {
        public FrBaseConfig()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
        }

        /// <summary>
        /// 窗体实例对象
        /// </summary>
        private static FrBaseConfig _instance;

        public static FrBaseConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrBaseConfig();
                }

                return _instance;
            }
        }


        public void GetBaseParams()
        {
            try
            {
                Machine.sysConfig.endpoint_address = FrBaseConfig.Instance.txt_endpoint_address.Text;
                Machine.sysConfig.MinioPort = FrBaseConfig.Instance.txt_Minioport.Text;
                Machine.sysConfig.ServerIP = this.txt_severIP.Text;
                Machine.sysConfig.ServerPort = this.txt_severPort.Text;
                if (int.TryParse(this.txt_MaxDefectCount.Text, out int maxDefectCount))
                {
                    Machine.sysConfig.MaxDefectCount = maxDefectCount;
                }
                if (int.TryParse(this.txt_AgentShutdownTimeout.Text, out int agentShutdownTimeout))
                {
                    Machine.sysConfig.AgentShutdownTimeout = agentShutdownTimeout;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
            }
        }
        private void FrBaseConfig_Load(object sender, EventArgs e)
        {
            this.txt_severIP.Text = Machine.sysConfig.ServerIP;
            this.txt_severPort.Text = Machine.sysConfig.ServerPort;
            this.txt_endpoint_address.Text = Machine.sysConfig.endpoint_address;
            this.txt_Minioport.Text = Machine.sysConfig.MinioPort;
            this.txt_MaxDefectCount.Text = Machine.sysConfig.MaxDefectCount.ToString();
            this.txt_AgentShutdownTimeout.Text = Machine.sysConfig.AgentShutdownTimeout.ToString();
        }
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
                        bool success = await Machine.master.workClass.ClearAllDatabaseData();

                        if (success)
                        {
                            MessageBox.Show("数据库清空成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}
using DeepSightTool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DeepSightModel;
using System.Collections.Generic;
using Sunny.UI;
using System.IO;
using HalconDotNet;
using System.Drawing;
using System.Drawing.Imaging; // 添加以使用编码器

namespace DeepSightAI.SettingPages
{
    /// <summary>
    /// 基础参数
    /// </summary>
    public partial class FrCreateMaterial : Form
    {
        public FrCreateMaterial()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            MaximizedBounds = Screen.PrimaryScreen.WorkingArea;

        }
        private Dictionary<string, List<string>> dic_solutionAndFlow = new Dictionary<string, List<string>>();
        /// <summary>
        /// 窗体实例对象
        /// </summary>
        private static FrCreateMaterial _instance;
        public static FrCreateMaterial Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrCreateMaterial();
                }

                return _instance;
            }
        }

        private void btn_selectA_Click(object sender, EventArgs e)
        {
            try
            {
                string Apath = SelectImageFile();
                if (!string.IsNullOrEmpty(Apath))
                {
                    HImage himage = new HImage();
                    HTuple width;
                    HTuple height;
                    himage.ReadImage(Apath);
                    himage.GetImageSize(out width, out height);
                    //准备料号JSON
                    JObject jsonObject = CreateJsonObject(width, height);

                    string converted = Path.GetFileNameWithoutExtension(Apath).Replace("[", "_").Replace("]", "");


                    string jsonfileName = converted + ".json";
                    string imagefileName= converted + ".bmp";
                    string filePath= Path.Combine(Path.GetDirectoryName(Apath), jsonfileName);
                    File.WriteAllText(filePath, jsonObject.ToString());
                    HOperatorSet.WriteImage(himage, "bmp", 0, Path.Combine(Path.GetDirectoryName(Apath), imagefileName));

                    himage.Dispose();
                    himage = null;
                    MessageBox.Show("A面料号制作成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
            }
        }

        private void btn_selectB_Click(object sender, EventArgs e)
        {
            string Bpath = SelectImageFile();
            if (!string.IsNullOrEmpty(Bpath))
            {
                HImage himage = new HImage();
                HTuple width;
                HTuple height;
                himage.ReadImage(Bpath);
                himage.GetImageSize(out width, out height);
                //准备料号JSON
                JObject jsonObject = CreateJsonObject(width, height);
                string converted = Path.GetFileNameWithoutExtension(Bpath).Replace("[", "_").Replace("]", "");
                string jsonfileName = converted + ".json";
                string imagefileName = converted + ".bmp";
                string filePath = Path.Combine(Path.GetDirectoryName(Bpath), jsonfileName);
                File.WriteAllText(filePath, jsonObject.ToString());
                HOperatorSet.WriteImage(himage, "bmp", 0, Path.Combine(Path.GetDirectoryName(Bpath), imagefileName));
                himage.Dispose();
                himage = null;
                MessageBox.Show("B面料号制作成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string SelectImageFile()
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "选择图片文件";
                    openFileDialog.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.webp|JPEG 文件|*.jpg;*.jpeg|PNG 文件|*.png|位图文件|*.bmp|所有文件|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.Multiselect = false; 
                    openFileDialog.CheckPathExists = true;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        return openFileDialog.FileName;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static JObject CreateJsonObject(int width, int height)
        {
            JObject mainObject = new JObject();
            mainObject["actual_scale"] = 0.005;
            mainObject["gerber_scale"] = 0.005;

            JObject barcode = new JObject();
            barcode["height"] = 0;
            barcode["width"] = 0;
            barcode["x"] = 0;
            barcode["y"] = 0;
            mainObject["barcode"] = barcode;

            JObject gerberSize = new JObject();
            gerberSize["height"] = height;
            gerberSize["width"] = width;
            mainObject["gerber_size"] = gerberSize;

            JArray singlePcsList = new JArray();
            JObject singlePcs = new JObject();
            singlePcs["id"] = "1";
            JObject position = new JObject();
            position["height"] = height;
            position["width"] = width;
            position["x"] = 0;
            position["y"] = 0;
            singlePcs["position"] = position;

            JObject bPoint = new JObject();
            bPoint["height"] = 0;
            bPoint["width"] = 0;
            bPoint["x"] = 0;
            bPoint["y"] = 0;
            singlePcs["b_point"] = bPoint;

            singlePcsList.Add(singlePcs);

            mainObject["single_pcs_list"] = singlePcsList;
            return mainObject;
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
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
                //string fileName = Path.GetFileNameWithoutExtension(Bpath) + ".json";
                //string filePath = Path.Combine(Path.GetDirectoryName(Bpath), fileName);
                //File.WriteAllText(filePath, jsonObject.ToString());
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
    }
}
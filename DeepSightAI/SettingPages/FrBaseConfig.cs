using DeepSightTool;
using System;
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
                Machine.sysConfig.ProjectName = FrBaseConfig.Instance.txt_ProjectName.Text;
                Machine.sysConfig.Line = FrBaseConfig.Instance.txt_Line.Text;
                Machine.sysConfig.ImagePath = FrBaseConfig.Instance.txt_ImagePath.Text;
                Machine.sysConfig.defectPath = FrBaseConfig.Instance.txt_defectPath.Text;

                Machine.sysConfig.access_key = FrBaseConfig.Instance.txt_access_key.Text;
                Machine.sysConfig.bucket = FrBaseConfig.Instance.txt_bucket.Text;
                Machine.sysConfig.endpoint_address = FrBaseConfig.Instance.txt_endpoint_address.Text;
                Machine.sysConfig.access_secret = FrBaseConfig.Instance.txt_defectPath.Text;
                Machine.sysConfig.MinioPort = FrBaseConfig.Instance.txt_Minioport.Text;
                Machine.sysConfig.DsCenterUrl = FrBaseConfig.Instance.txt_DsCenterURL.Text;

                if (FrBaseConfig.Instance.radioImg1.Checked)
                {
                    Machine.sysConfig.ImageEnable = 0;
                }
                if (FrBaseConfig.Instance.radioImg2.Checked)
                {
                    Machine.sysConfig.ImageEnable = 1;
                }
                if (FrBaseConfig.Instance.radioImg3.Checked)
                {
                    Machine.sysConfig.ImageEnable = 2;
                }
                Machine.sysConfig.ServerIP = this.txt_severIP.Text;
                Machine.sysConfig.ServerPort = this.txt_severPort.Text;
                Machine.sysConfig.LogDay = (int)FrBaseConfig.Instance.txt_log_day.Value;


            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
            }
        }
        private void FrBaseConfig_Load(object sender, EventArgs e)
        {
            FrBaseConfig.Instance.txt_ProjectName.Text = Machine.sysConfig.ProjectName;
            FrBaseConfig.Instance.txt_Line.Text = Machine.sysConfig.Line;
            FrBaseConfig.Instance.txt_ImagePath.Text = Machine.sysConfig.ImagePath;
            FrBaseConfig.Instance.txt_defectPath.Text = Machine.sysConfig.defectPath;
            FrBaseConfig.Instance.txt_log_day.Value = Machine.sysConfig.LogDay;
            FrBaseConfig.Instance.checkImageDetection.Checked = !Machine.sysConfig.TestFlag;
            FrBaseConfig.Instance.txt_severIP.Text = Machine.sysConfig.ServerIP;
            FrBaseConfig.Instance.txt_severPort.Text = Machine.sysConfig.ServerPort;


            FrBaseConfig.Instance.txt_access_key.Text = Machine.sysConfig.access_key;
            FrBaseConfig.Instance.txt_bucket.Text = Machine.sysConfig.bucket;
            FrBaseConfig.Instance.txt_endpoint_address.Text = Machine.sysConfig.endpoint_address;
            //FrBaseConfig.Instance.txt.Text = Machine.sysConfig.access_secret;
            FrBaseConfig.Instance.txt_Minioport.Text = Machine.sysConfig.MinioPort;
            FrBaseConfig.Instance.txt_DsCenterURL.Text = Machine.sysConfig.DsCenterUrl;

            if (Machine.sysConfig.ImageEnable == 0)
            {
                FrBaseConfig.Instance.radioImg1.Checked = true;
            }
            if (Machine.sysConfig.ImageEnable == 1)
            {
                FrBaseConfig.Instance.radioImg2.Checked = true;
            }
            if (Machine.sysConfig.ImageEnable == 2)
            {
                FrBaseConfig.Instance.radioImg3.Checked = true;
            }


            if (Machine.sysConfig.LogEnable)
            {
                FrBaseConfig.Instance.radioLog1.Checked = true;
            }
            if (!Machine.sysConfig.LogEnable)
            {
                FrBaseConfig.Instance.radioLog2.Checked = true;
            }

            //FrBaseConfig.Instance.checkBigImg.Checked = Machine.sysConfig.SnapImgSaveFlag;
            //FrBaseConfig.Instance.checkImageDetection.Checked = !Machine.sysConfig.TestFlag;
            //FrBaseConfig.Instance.checkCeHou.Checked = Machine.sysConfig.cehouFlag;
            //FrBaseConfig.Instance.checkFilt.Checked = Machine.sysConfig.ShowFilt;
            //FrBaseConfig.Instance.cmb_ImageSuffix.SelectedItem = Machine.sysConfig.ImageSuffix;
            //FrBaseConfig.Instance.checkCutImgSaveFlag.Checked = Machine.sysConfig.CutImgSaveFlag;
            //FrBaseConfig.Instance.checkSaveJsonFlag.Checked = Machine.sysConfig.SaveJsonFlag;

            //if (Machine.sysConfig.ResultImgResize == 0)
            //{
            //    Machine.sysConfig.ResultImgResize = 0.3;
            //}
            //FrBaseConfig.Instance.txt_result_size.Value = (decimal)Machine.sysConfig.ResultImgResize;

        }
        public void Language(int language)
        {
            if (language == 1)
            {


            }
            else
            {

            }
        }

        private void txt_ImagePath_DoubleClick(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog
            {
                SelectedPath = txt_ImagePath.Text
            };
            if (folder.ShowDialog().Equals(DialogResult.OK))
            {
                txt_ImagePath.Text = folder.SelectedPath;
            }
        }

        private void radioImg3_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton radioButton = sender as RadioButton;

                if (radioButton.Checked)
                {
                    if (radioButton.Text == radioImg1.Text)
                    {
                        radioImg2.Checked = false;
                        radioImg3.Checked = false;
                    }
                    if (radioButton.Text == radioImg2.Text)
                    {
                        radioImg1.Checked = false;
                        radioImg3.Checked = false;
                    }
                    if (radioButton.Text == radioImg3.Text)
                    {
                        radioImg1.Checked = false;
                        radioImg2.Checked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        private void radioLog2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton radioButton = sender as RadioButton;

                if (radioButton.Checked)
                {
                    if (radioButton.Text == radioLog1.Text)
                    {
                        radioLog2.Checked = false;
                    }
                    if (radioButton.Text == radioLog2.Text)
                    {
                        radioLog1.Checked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        private void txt_cutPath_DoubleClick(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog
            {
                SelectedPath = txt_defectPath.Text
            };
            if (folder.ShowDialog().Equals(DialogResult.OK))
            {
                txt_defectPath.Text = folder.SelectedPath;
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }
    }
}
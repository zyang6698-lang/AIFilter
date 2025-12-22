using DeepSightModel;
using DeepSightTool;
using System;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    public partial class FrStationCofig : Form
    {
        public FrStationCofig(WatchPathConfig _config)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            stationConfig = _config;
        }
        public WatchPathConfig stationConfig = new WatchPathConfig();
        public void Language(int language)
        {
            if (language == 1)
            {
                label2.Text = "A_Path：";
                label3.Text = "A_Path：";
                label4.Text = "文件深度";
                label5.Text = "是否启用AVI:";
                radiotcp1.Text = "启用";
                radiotcp2.Text = "禁用";
            }
            else
            {
                label2.Text = "A_Path:";
                label3.Text = "B_Path:";
                label4.Text = "Depth:";
                label5.Text = "ISAVIEnable:";
                radiotcp1.Text = "enable";
                radiotcp2.Text = "disenable";
            }
        }


        private void btn_OK_Click(object sender, EventArgs e)
        {
            stationConfig = new WatchPathConfig();
            stationConfig.APath = this.txt_APath.Text;
            stationConfig.BPath = this.txt_BPath.Text;
            stationConfig.AviName = this.txt_stationName.Text;
            stationConfig.Depth = Convert.ToInt32(this.txt_Depth.Text);
            stationConfig.FileA = this.txt_temporary_file_storage_area_A.Text;
            stationConfig.FileB = this.txt_temporary_file_storage_area_B.Text;
            stationConfig.IsEnable = radiotcp1.Checked ? true : false;
            //0822新加字段
            stationConfig.MinioConfig = this.txt_A_minio_config.Text;
            stationConfig.DeepsightAgentDataWorkspace = this.txt_deepsight_agent_data_workspace.Text;
            if (radiomode2.Checked)
            {
                stationConfig.CopyOrCutMode = "cut";
            }
            if (radiomode1.Checked)
            {
                stationConfig.CopyOrCutMode = "copy";
            }
            stationConfig.BPathIndexTimestamp = this.txt_B_path_index_timestamp.Text;
            stationConfig.BLotTimestamp = this.txt_B_lot_timestamp.Text;
            stationConfig.BPanelIndexTimestamp = this.txt_B_panel_index_timestamp.Text;
            //active_or_passive字段
            if (radioActive.Checked)
            {
                stationConfig.ActiveOrPassive = "active";
            }
            if (radioPassive.Checked)
            {
                stationConfig.ActiveOrPassive = "passive";
            }
            this.DialogResult = DialogResult.OK;
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void radiotcp1_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void FrStationCofig_Shown(object sender, EventArgs e)
        {
            this.txt_stationName.Text = stationConfig.AviName;
            this.txt_APath.Text = stationConfig.APath;
            this.txt_BPath.Text = stationConfig.BPath;
            this.txt_Depth.Text = stationConfig.Depth.ToString();
            this.txt_temporary_file_storage_area_A.Text = stationConfig.FileA;
            this.txt_temporary_file_storage_area_B.Text = stationConfig.FileB;
            if (stationConfig.CopyOrCutMode=="copy")
            {
                this.radiomode1.Checked = true;
            }
            else
            {
                this.radiomode2.Checked = true;
            }

            if (stationConfig.IsEnable)
            {
                this.radiotcp1.Checked = true;
                this.radiotcp2.Checked = false;
            }
            else if (!stationConfig.IsEnable)
            {
                this.radiotcp1.Checked = false;
                this.radiotcp2.Checked = true;
            }
            //0822新加字段
            this.txt_A_minio_config.Text = stationConfig.MinioConfig;
            this.txt_deepsight_agent_data_workspace.Text = stationConfig.DeepsightAgentDataWorkspace;
            this.txt_B_path_index_timestamp.Text = stationConfig.BPathIndexTimestamp;
            this.txt_B_lot_timestamp.Text = stationConfig.BLotTimestamp;
            this.txt_B_panel_index_timestamp.Text = stationConfig.BPanelIndexTimestamp;
            //active_or_passive字段
            if (stationConfig.ActiveOrPassive == "passive")
            {
                this.radioPassive.Checked = true;
            }
            else
            {
                this.radioActive.Checked = true;
            }
        }
    }
}
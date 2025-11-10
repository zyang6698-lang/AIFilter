using DeepSightModel;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;

namespace DeepSightAI.SettingPages
{
    public partial class FrHWConfig : Form
    {
        internal FrHWConfig()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
        }
        #region 窗体拖动
        private static bool IsDrag = false;
        private int enterX;
        private int enterY;

        //双击

        private void setForm_MouseDown(object sender, MouseEventArgs e)
        {
            IsDrag = true;
            enterX = e.Location.X;
            enterY = e.Location.Y;
        }

        private void setForm_MouseUp(object sender, MouseEventArgs e)
        {
            IsDrag = false;
            enterX = 0;
            enterY = 0;
        }

        private void setForm_MouseLeave(object sender, EventArgs e)
        {
            IsDrag = false;
            enterX = 0;
            enterY = 0;
        }

        private void setForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDrag)
            {
                Left += e.Location.X - enterX;
                Top += e.Location.Y - enterY;
            }
        }

        #endregion 窗体拖动
        /// <summary>
        /// 窗体实例对象
        /// </summary>
        private static FrHWConfig _instance;

        public static FrHWConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrHWConfig();
                }
                return _instance;
            }
        }
        public void Language(int language)
        {
            if (language == 1)
            {
                label2.Text = "工站数量:";
            }
            else
            {
                label2.Text = "Number of plant stations:";
            }

            for (int i = 0; i < Machine.aviconfig.WatchPaths.Count; i++)
            {
                LanguageParam(i);
            }
        }
        private bool LanguageParam(int i)
        {
            try
            {
                //if (FrHWConfig.Instance.tabControl.Controls.Count <= i)
                //{
                //    return false;
                //}
                //TabPage tabPage = FrHWConfig.Instance.tabControl.Controls[i] as TabPage;

                //if (tabPage.Controls.Count > 0)
                //{
                //    tabPage.Text = "工站" + (i + 1);
                //    //工站信息
                //    Panel panel1 = tabPage.Controls[0] as Panel;
                //    if (panel1.Controls.Count > 0)
                //    {
                //        FrStationCofig frStationCofig = panel1.Controls[0] as FrStationCofig;
                //        if (frStationCofig == null)
                //        {
                //            return false;
                //        }
                //        frStationCofig.Language(1);
                //    }
                //}
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
                return false;
            }
            return true;
        }
        private void txt_station_count_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (txt_station_count.Value > int.Parse(lblstationcount.Text))//说明增加了
                {
                    //增加的数量
                    int num = (int)txt_station_count.Value - int.Parse(lblstationcount.Text);

                    for (int i = 0; i < num; i++)
                    {

                        //TabPage tabPage = new TabPage
                        //{
                        //    Text = "工站" + (tabControl.Controls.Count + 1),
                        //};

                        //tabPage.Controls.Clear();
                        //Panel panel = new Panel
                        //{
                        //    BackColor = System.Drawing.SystemColors.Control,
                        //    Dock = System.Windows.Forms.DockStyle.Fill
                        //};

                        //FrStationCofig frStationCofig = new FrStationCofig
                        //{
                        //    TopLevel = false,
                        //    Parent = panel,
                        //    Dock = DockStyle.Fill
                        //};
                        //frStationCofig.Show();
                        //tabPage.Controls.Add(panel);

                        //tabControl.Controls.Add(tabPage);

                        ////**************************************
                        if (txt_station_count.Value > Machine.aviconfig.WatchPaths.Count())
                        {
                            WatchPathConfig watchPath = new WatchPathConfig()
                            {
                                AviName = $"AVI{Machine.aviconfig.WatchPaths.Count() + 1}",
                                APath = "",
                                BPath = "",
                                Depth = 4,
                                FileA = "",
                                FileB = "",
                                IsEnable = false,
                            };
                            Machine.aviconfig.WatchPaths.Add(watchPath);
                            AddParam(Machine.aviconfig.WatchPaths.Count() - 1);
                        }
                        //**********************************
                    }

                    lblstationcount.Text = txt_station_count.Value.ToString();
                }
                else if (txt_station_count.Value < int.Parse(lblstationcount.Text))//说明增加了
                {
                    //减少的数量
                    int num = int.Parse(lblstationcount.Text) - (int)txt_station_count.Value;

                    //for (int i = 0; i < num; i++)
                    //{
                    //    int index = tabControl.Controls.Count;

                    //    tabControl.Controls.RemoveAt(index - 1);
                    //}
                    for (int i = 0; i < num; i++)
                    {
                        int index = avi_panel.Controls.Count;

                        avi_panel.Controls.RemoveAt(index - 1);
                    }
                    lblstationcount.Text = txt_station_count.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        private void FrHWConfig_Shown(object sender, EventArgs e)
        {
            SetStationParam();
        }

        private void SetStationParam()
        {
            try
            {
                // FrHWConfig.Instance.tabControl.Controls.Clear();//先清空
                FrHWConfig.Instance.txt_station_count.Value = Machine.aviconfig.WatchPaths.Count;
                //公共参数
                //FrHWConfig.Instance.txt_depth_from_watch_path_to_result_ini.Text = Machine.aviconfig.Depth.ToString();
                FrHWConfig.Instance.txt_max_wait_time.Text = Machine.aviconfig.MaxWaitTime.ToString(); ;
                //FrHWConfig.Instance.txt_depth_from_watch_path_to_result_ini.Text = Machine.aviconfig.WaitFlag;
                //FrHWConfig.Instance.txt_depth_from_watch_path_to_result_ini.Text = Machine.aviconfig.Finishflag;
                //FrHWConfig.Instance.txt_A_minio_config.Text = Machine.aviconfig.AMinioConfig;
                //FrHWConfig.Instance.txt_B_minio_config.Text = Machine.aviconfig.BMinioConfig;
                FrHWConfig.Instance.txt_LDB_endpoint.Text = Machine.aviconfig.LDBEndpoint;
                FrHWConfig.Instance.txt_get_infer_result_interval.Text = Machine.aviconfig.GetInferResultInterval.ToString();
                FrHWConfig.Instance.txt_infer_request_timeout.Text = Machine.aviconfig.InferRequestTimeout.ToString();
                FrHWConfig.Instance.txt_get_infer_result_timeout.Text = Machine.aviconfig.GetInferResultTimeout.ToString();
                if (Machine.aviconfig.CopyOrCutMode == "copy")
                {
                    FrHWConfig.Instance.radiomode1.Checked = true;
                }
                else
                {
                    FrHWConfig.Instance.radiomode2.Checked = true;
                }
                //FrHWConfig.Instance.txt_copy_or_cut_mode.Text = Machine.aviconfig.CopyOrCutMode;
                //FrHWConfig.Instance.txt_deepsight_agent_data_workspace.Text = Machine.aviconfig.DeepsightAgentDataWorkspace;
                //FrHWConfig.Instance.txt_temporary_file_storage_area_A.Text = Machine.aviconfig.TemporaryFileStorageArea_A;
                //FrHWConfig.Instance.txt_temporary_file_storage_area_B.Text = Machine.aviconfig.TemporaryFileStorageArea_B;



                for (int i = 0; i < Machine.aviconfig.WatchPaths.Count; i++)
                {
                    AddAviControl(Machine.aviconfig.WatchPaths[i], i);
                   // AddParam(i);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }


        private void AddAviControl(WatchPathConfig watchPath, int index)
        {
            try
            {
                AviCtr ctr = new AviCtr(watchPath);
                ctr.Size = new Size(150, 150); // 根据需求调整

                // 计算位置 - 根据索引排列
                int cols = 7; // 每行显示4个
                int spacing = 0; // 间距

                int x = (index % cols) * (ctr.Width + spacing) + spacing;
                int y = (index / cols) * (ctr.Height + spacing) + spacing;

                ctr.Location = new Point(x, y + 50);
                this.avi_panel.Controls.Add(ctr);
            }
            catch (Exception ex)
            {
                // Consider logging the exception
            }
        }


        private bool AddParam(int i)
        {
            try
            {
                AviCtr ctr = new AviCtr(Machine.aviconfig.WatchPaths[i]);

                ctr.Size = new Size(150, 150);


                this.avi_panel.Controls.Add(ctr);

                //TabPage tabPage = FrHWConfig.Instance.tabControl.Controls[i] as TabPage;
                //if (tabPage.Controls.Count > 0)
                //{
                //    //工站信息
                //    Panel panel1 = tabPage.Controls[0] as Panel;
                //    if (panel1.Controls.Count > 0)
                //    {
                //        FrStationCofig frStationCofig = panel1.Controls[0] as FrStationCofig;
                //        if (frStationCofig == null)
                //        {
                //            return false;
                //        }
                //        frStationCofig.txt_APath.Text = Machine.aviconfig.WatchPaths[i].APath;

                //        frStationCofig.txt_BPath.Text = Machine.aviconfig.WatchPaths[i].BPath;
                //        frStationCofig.txt_Depth.Text = Machine.aviconfig.WatchPaths[i].Depth.ToString();

                //        if (Machine.aviconfig.WatchPaths[i].IsEnable)
                //        {
                //            frStationCofig.radiotcp1.Checked = true;
                //            frStationCofig.radiotcp2.Checked = false;
                //        }
                //        else if (!Machine.aviconfig.WatchPaths[i].IsEnable)
                //        {
                //            frStationCofig.radiotcp1.Checked = false;
                //            frStationCofig.radiotcp2.Checked = true;
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
                return false;
            }
            return true;
        }
        public void GetStationParam()
        {
            try
            {
                //List<WatchPathConfig> list_stationParams = new List<WatchPathConfig>();
                //list_stationParams.AddRange(Machine.aviconfig.WatchPaths);

                Machine.aviconfig.WatchPaths.Clear();
                //公共参数
                //Machine.aviconfig.Depth = Convert.ToInt32(FrHWConfig.Instance.txt_depth_from_watch_path_to_result_ini.Text);
                Machine.aviconfig.MaxWaitTime = Convert.ToInt32(FrHWConfig.Instance.txt_max_wait_time.Text);
                //FrHWConfig.Instance.txt_depth_from_watch_path_to_result_ini.Text = Machine.aviconfig.WaitFlag;
                //FrHWConfig.Instance.txt_depth_from_watch_path_to_result_ini.Text = Machine.aviconfig.Finishflag;
                //Machine.aviconfig.AMinioConfig = FrHWConfig.Instance.txt_A_minio_config.Text;
                //Machine.aviconfig.BMinioConfig = FrHWConfig.Instance.txt_B_minio_config.Text;
                Machine.aviconfig.LDBEndpoint = FrHWConfig.Instance.txt_LDB_endpoint.Text;
                Machine.aviconfig.GetInferResultInterval = Convert.ToInt32(FrHWConfig.Instance.txt_get_infer_result_interval.Text);
                Machine.aviconfig.InferRequestTimeout = Convert.ToInt32(FrHWConfig.Instance.txt_infer_request_timeout.Text);
                Machine.aviconfig.GetInferResultTimeout = Convert.ToInt32(FrHWConfig.Instance.txt_get_infer_result_timeout.Text);
                if (radiomode2.Checked)
                {
                    Machine.aviconfig.CopyOrCutMode = "cut";
                }
                if (radiomode1.Checked)
                {
                    Machine.aviconfig.CopyOrCutMode = "copy";
                }
                //Machine.aviconfig.CopyOrCutMode = FrHWConfig.Instance.txt_copy_or_cut_mode.Text;
                //Machine.aviconfig.DeepsightAgentDataWorkspace = FrHWConfig.Instance.txt_deepsight_agent_data_workspace.Text;
                //Machine.aviconfig.TemporaryFileStorageArea_A = FrHWConfig.Instance.txt_temporary_file_storage_area_A.Text;
                //Machine.aviconfig.TemporaryFileStorageArea_B = FrHWConfig.Instance.txt_temporary_file_storage_area_B.Text;

                for (int i = 0; i < FrHWConfig.Instance.txt_station_count.Value; i++)
                {
                    AviCtr ctr = FrHWConfig.Instance.avi_panel.Controls[i] as AviCtr;
                    if (ctr.Controls.Count > 0)
                    {
                        //工站信息
                        WatchPathConfig stationParam = ctr.ctrConfig;
                        Machine.aviconfig.WatchPaths.Add(stationParam);
                    }
                }

                //for (int i = 0; i < FrHWConfig.Instance.txt_station_count.Value; i++)
                //{
                //    TabPage tabPage = FrHWConfig.Instance.tabControl.Controls[i] as TabPage;
                //    if (tabPage.Controls.Count > 0)
                //    {
                //        //工站信息
                //        Panel panel1 = tabPage.Controls[0] as Panel;
                //        if (panel1.Controls.Count > 0)
                //        {
                //            //工站信息
                //            WatchPathConfig stationParam = new WatchPathConfig();
                //            FrStationCofig frStationCofig = panel1.Controls[0] as FrStationCofig;
                //            stationParam.APath = frStationCofig.txt_APath.Text;

                //            stationParam.BPath = frStationCofig.txt_BPath.Text;
                //            stationParam.Depth = Convert.ToInt32(frStationCofig.txt_Depth.Text);

                //            if (frStationCofig.radiotcp1.Checked)
                //            {
                //                stationParam.IsEnable = true;
                //            }
                //            else if (frStationCofig.radiotcp2.Checked)
                //            {
                //                stationParam.IsEnable = false;
                //            }

                //            Machine.aviconfig.WatchPaths.Add(stationParam);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        private void radiomode1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton radioButton = sender as RadioButton;
                if (radioButton.Checked)
                {
                    if (radioButton.Text == radiomode1.Text)
                    {
                        radiomode2.Checked = false;
                    }
                    if (radioButton.Text == radiomode2.Text)
                    {
                        radiomode1.Checked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        private void btn_RunAgent_Click(object sender, EventArgs e)
        {
            //开启Gennt
            FrSetting.Instance.RestartApplication(FrSetting.Instance.appPath, FrSetting.Instance.appExe, true);
        }
        private void btn_KillAgent_Click(object sender, EventArgs e)
        {
            //关闭Gennt
            FrSetting.Instance.KillProcessInDirectory(FrSetting.Instance.appPath, FrSetting.Instance.appExe);
        }
    }
}

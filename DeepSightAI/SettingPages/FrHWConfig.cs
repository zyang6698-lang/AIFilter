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

        private void btn_add_station_Click(object sender, EventArgs e)
        {
            try
            {
                WatchPathConfig newStation = new WatchPathConfig()
                {
                    AviName = $"AVI{Machine.aviconfig.WatchPaths.Count + 1}",
                    APath = "",
                    BPath = "",
                    Depth = 4,
                    FileA = "",
                    FileB = "",
                    IsEnable = false,
                };

                using (FrStationCofig frStation = new FrStationCofig(newStation))
                {
                    if (frStation.ShowDialog() == DialogResult.OK)
                    {
                        Machine.aviconfig.WatchPaths.Add(frStation.stationConfig);
                        aviCtr2Container1.CreateMachinePanels(Machine.aviconfig.WatchPaths);
                    }
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
                aviCtr2Container1.CreateMachinePanels(Machine.aviconfig.WatchPaths);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        public void GetStationParam()
        {
            try
            {
                Machine.aviconfig.WatchPaths.Clear();

                var watchPaths = aviCtr2Container1.GetAllConfigs();

                foreach (var stationParam in watchPaths)
                {
                    Machine.aviconfig.WatchPaths.Add(stationParam);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

    }
}
using DeepSightModel;
using DeepSightModel.Configuration;
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
    public partial class PgSettingHardware : Sunny.UI.UIPage
    {
        internal PgSettingHardware()
        {
            InitializeComponent();
            PageIndex = 2;
            ShowTitle = false;
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
        private static PgSettingHardware _instance;

        public static PgSettingHardware Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PgSettingHardware();
                }
                return _instance;
            }
        }

        private void btn_add_station_Click(object sender, EventArgs e)
        {
            try
            {
                var allMachines = GetMergedWatchPaths();
                WatchPathConfig newStation = new WatchPathConfig()
                {
                    AviName = $"AVI{allMachines.Count + 1}",
                    APath = "",
                    BPath = "",
                    Depth = 4,
                    FileA = "",
                    FileB = "",
                    IsEnable = false,
                };

                using (DlgStationConfig frStation = new DlgStationConfig(newStation))
                {
                    if (frStation.ShowDialog() == DialogResult.OK)
                    {
                        var cfg = frStation.stationConfig;
                        // 同步添加到 AVIConfig
                        Machine.aviconfig.WatchPaths.Add(cfg);
                        // 同步添加到机台注册表
                        var newEntry = new MachineEntry
                        {
                            MachineName = cfg.AviName,
                            IsEnable = cfg.IsEnable,
                            DataSourceType = (!string.IsNullOrEmpty(cfg.APath) || !string.IsNullOrEmpty(cfg.BPath))
                                ? DataSourceType.Agent
                                : DataSourceType.LevelDb
                        };
                        Machine.machineRegistryManager.AddOrUpdate(newEntry);
                        Machine.machineRegistryManager.Read(out Machine.machineRegistry);

                        machineStatusPanel1.CreateMachinePanels(GetMergedWatchPaths());
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
                machineStatusPanel1.CreateMachinePanels(GetMergedWatchPaths());
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

                var watchPaths = machineStatusPanel1.GetAllConfigs();

                foreach (var stationParam in watchPaths)
                {
                    Machine.aviconfig.WatchPaths.Add(stationParam);
                }

                // 同步更新机台注册表
                if (Machine.machineRegistry != null)
                {
                    Machine.machineRegistry.Machines = watchPaths.Select(w => new MachineEntry
                    {
                        MachineName = w.AviName,
                        IsEnable = w.IsEnable,
                        DataSourceType = (!string.IsNullOrEmpty(w.APath) || !string.IsNullOrEmpty(w.BPath))
                            ? DataSourceType.Agent
                            : DataSourceType.LevelDb
                    }).ToList();
                    Machine.machineRegistryManager.Save(Machine.machineRegistry);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }

        /// <summary>
        /// 以 MachineRegistry 为主，合并 AVIConfig 中的详细配置，
        /// 生成统一的 WatchPathConfig 列表供 UI 展示。
        /// 对于非 Agent 机台，自动生成轻量占位 WatchPathConfig。
        /// </summary>
        private List<WatchPathConfig> GetMergedWatchPaths()
        {
            var result = new List<WatchPathConfig>();

            if (Machine.machineRegistry?.Machines == null || Machine.machineRegistry.Machines.Count == 0)
            {
                // 回退：注册表为空时使用旧配置
                return Machine.aviconfig?.WatchPaths ?? new List<WatchPathConfig>();
            }

            var aviMap = Machine.aviconfig?.WatchPaths?
                .ToDictionary(w => w.AviName, w => w, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, WatchPathConfig>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in Machine.machineRegistry.Machines)
            {
                if (aviMap.TryGetValue(entry.MachineName, out var existing))
                {
                    // 使用 Agent 详细配置，但以注册表的 IsEnable 为准
                    existing.IsEnable = entry.IsEnable;
                    result.Add(existing);
                }
                else
                {
                    // 非 Agent 机台或尚未配置 Agent 的机台，生成占位配置
                    result.Add(new WatchPathConfig
                    {
                        AviName = entry.MachineName,
                        IsEnable = entry.IsEnable,
                        APath = "",
                        BPath = "",
                        Depth = 4,
                        FileA = "",
                        FileB = ""
                    });
                }
            }

            return result;
        }

    }
}
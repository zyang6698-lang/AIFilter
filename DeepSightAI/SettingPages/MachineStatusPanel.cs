using DeepSightDB;
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

namespace DeepSightAI.SettingPages
{
    public partial class MachineStatusPanel : UserControl
    {
        private List<MachineStatusCard> _machineCards = new List<MachineStatusCard>();

        /// <summary>
        /// 控制所有子控件的删除按钮是否可见
        /// </summary>
        private bool _showDeleteButtons = true;
        public bool ShowDeleteButtons
        {
            get => _showDeleteButtons;
            set
            {
                if (_showDeleteButtons != value)
                {
                    _showDeleteButtons = value;
                    // 更新所有现有控件的删除按钮可见性
                    foreach (var ctr in _machineCards)
                    {
                        ctr.ShowDeleteButton = value;
                    }
                }
            }
        }

        public MachineStatusPanel()
        {
            InitializeComponent();
            flowLayoutPanel1.BackColor = Color.Transparent;
            typeof(FlowLayoutPanel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(flowLayoutPanel1, true, null);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        // 使用组合双缓冲（注意：可能影响层级控件显示，若有问题可移除）
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        /// <summary>
        /// 根据配置列表增量创建/更新机台状态卡片（避免每次全清导致重绘开销）
        /// </summary>
        /// <param name="watchPaths">配置列表</param>
        public void CreateMachinePanels(List<WatchPathConfig> watchPaths)
        {
            if (watchPaths == null) return;

            // 现有映射
            var existingMap = _machineCards.ToDictionary(c => c.ctrConfig.AviName, c => c);
            var incomingNames = new HashSet<string>(watchPaths.Select(w => w.AviName));

            flowLayoutPanel1.SuspendLayout();
            try
            {
                // 移除不存在的
                for (int i = _machineCards.Count - 1; i >= 0; i--)
                {
                    var ctr = _machineCards[i];
                    if (!incomingNames.Contains(ctr.ctrConfig.AviName))
                    {
                        flowLayoutPanel1.Controls.Remove(ctr);
                        _machineCards.RemoveAt(i);
                        ctr.Dispose();
                    }
                }

                // 添加或更新现有
                foreach (var cfg in watchPaths)
                {
                    if (existingMap.TryGetValue(cfg.AviName, out var ctr))
                    {
                        // 更新配置引用（假设属性用于显示）
                        ctr.ctrConfig = cfg;
                        ctr.UpdateDisplay();
                    }
                    else
                    {
                        AddMachineCard(cfg);
                    }
                }
            }
            finally
            {
                flowLayoutPanel1.ResumeLayout(true);
            }
        }

        public void UpdateAllMachinePanels(List<WatchPathConfig> watchPaths)
        {
            if (watchPaths == null) return;
            var configMap = watchPaths.ToDictionary(w => w.AviName);
            foreach (var ctr in _machineCards)
            {
                if (configMap.TryGetValue(ctr.ctrConfig.AviName, out var cfg))
                {
                    ctr.ctrConfig = cfg;
                    ctr.UpdateDisplay();
                }
            }
        }

        /// <summary>
        /// 添加单个机台状态卡片
        /// </summary>
        private void AddMachineCard(WatchPathConfig watchPath)
        {
            try
            {
                MachineStatusCard ctr = new MachineStatusCard(watchPath);
                // 应用删除按钮可见性设置
                ctr.ShowDeleteButton = _showDeleteButtons;
                // 订阅删除事件
                ctr.DeleteRequested += OnMachineCardDeleteRequested;
                _machineCards.Add(ctr);
                flowLayoutPanel1.Controls.Add(ctr);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create MachineStatusCard: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理机台状态卡片删除请求事件
        /// </summary>
        private void OnMachineCardDeleteRequested(object sender, EventArgs e)
        {
            if (sender is MachineStatusCard ctr)
            {
                RemoveMachineCard(ctr);
            }
        }

        /// <summary>
        /// 移除指定的机台状态卡片
        /// </summary>
        /// <param name="ctr">要移除的控件</param>
        public void RemoveMachineCard(MachineStatusCard ctr)
        {
            if (ctr == null) return;

            flowLayoutPanel1.SuspendLayout();
            try
            {
                // 取消订阅事件
                ctr.DeleteRequested -= OnMachineCardDeleteRequested;

                flowLayoutPanel1.Controls.Remove(ctr);
                _machineCards.Remove(ctr);
                ctr.Dispose();
            }
            finally
            {
                flowLayoutPanel1.ResumeLayout(true);
            }
        }

        /// <summary>
        /// 根据 AviName 移除机台状态卡片
        /// </summary>
        /// <param name="aviName">要移除的机台名称</param>
        public void RemoveMachineCardByName(string aviName)
        {
            var ctr = _machineCards.FirstOrDefault(c => c.ctrConfig.AviName == aviName);
            if (ctr != null)
            {
                RemoveMachineCard(ctr);
            }
        }

        /// <summary>
        /// 获取所有机台状态卡片的配置列表
        /// </summary>
        public List<WatchPathConfig> GetAllConfigs()
        {
            return _machineCards.Select(ctr => ctr.ctrConfig).ToList();
        }

        /// <summary>
        /// 更新指定名称的机台状态卡片的统计信息
        /// </summary>
        public void UpdateMachineCardStats(string aviName, int totalImages, int aiOkImages)
        {
            var ctr = _machineCards.FirstOrDefault(c => c.ctrConfig.AviName == aviName);
            if (ctr != null)
            {
                ctr.AiFilterCount = totalImages;
                ctr.AiOkImages = aiOkImages;
            }
        }

        /// <summary>
        /// 更新所有机台状态卡片的信息
        /// </summary>
        public void UpdateAllMachineCardsInfo(Func<string, (string LotNumber, string SerialNumber, string ProductSerial, double Utilization)> getLatestPanelInfo)
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    foreach (var ctr in _machineCards)
                    {
                        if (ctr.ctrConfig.IsEnable)
                        {
                            var info = getLatestPanelInfo(ctr.ctrConfig.AviName);
                            ctr.LotId = info.LotNumber;
                            ctr.ProductSerial = info.SerialNumber;
                            ctr.Utilization = info.Utilization;
                        }
                    }
                }));
            }
        }

        public void UpdateMachineCardInfo(string aviName, string productSerial, string lotId, double utilization)
        {
            foreach (var ctr in _machineCards)
            {
                if (ctr.ctrConfig.AviName == aviName)
                {
                    ctr.ProductSerial = productSerial;
                    ctr.LotId = lotId;
                    ctr.Utilization = utilization;
                    break;
                }
            }
        }




        // 从缓存更新所有机台的 Lot/SN（不访问数据库）
        public void UpdateAllMachineCardLotSnFromCache()
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    foreach (var ctr in _machineCards.Where(c => c.ctrConfig.IsEnable))
                    {
                        var (lot, sn, productSerial) = BoardStatCache.GetLatestLotSn(ctr.ctrConfig.AviName);
                        if (!string.IsNullOrEmpty(lot) || !string.IsNullOrEmpty(sn) || !string.IsNullOrEmpty(productSerial))
                        {
                            ctr.LotId = lot;
                            ctr.ProductSerial = productSerial;
                        }
                    }
                }));
            }
        }


        // 使用缓存统计更新机台面板（不访问数据库）
        public async Task UpdateMachineBoardFromCache()
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    foreach (var ctr in _machineCards.Where(c => c.ctrConfig.IsEnable))
                    {
                        var machineId = ctr.ctrConfig.AviName;
                        var stat = BoardStatCache.GetTodayStatForMachine(machineId);
                        ctr.AiOkImages = stat.AiFilterOKCount;
                        ctr.AiFilterCount = stat.AiFilterCount;
                        ctr.AviPassRate = stat.AviPanelCount == 0 ? 0 : (double)stat.AviPanelOKCount / stat.AviPanelCount;
                        ctr.Utilization = stat.Utilization;

                        var (lot, sn, productSerial) = BoardStatCache.GetLatestLotSn(machineId);
                        if (!string.IsNullOrEmpty(lot) || !string.IsNullOrEmpty(sn) || !string.IsNullOrEmpty(productSerial))
                        {
                            ctr.LotId = lot;
                            ctr.ProductSerial = productSerial;
                        }
                    }
                }));
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 检查所有工站是否超时，超时则将状态设为灰色
        /// </summary>
        public void CheckAllStationsTimeout()
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    foreach (var ctr in _machineCards.Where(c => c.ctrConfig.IsEnable))
                    {
                        ctr.CheckTimeoutAndUpdateStatus();
                    }
                }));
            }
        }

        /// <summary>
        /// 根据 MachineName 更新工站的数据接收时间
        /// </summary>
        /// <param name="machineName">机器名称</param>
        public void UpdateStationDataReceived(string machineName)
        {
            if (string.IsNullOrEmpty(machineName)) return;

            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    // 根据 MachineName 查找对应的工站控件
                    var ctr = _machineCards.FirstOrDefault(c =>
                        c.ctrConfig.AviName == machineName ||
                        c.MachineName == machineName);

                    if (ctr != null)
                    {
                        ctr.MachineName = machineName;
                        ctr.UpdateDataReceived();
                    }
                }));
            }
        }

        /// <summary>
        /// 根据 MachineName 查找工站是否已存在
        /// </summary>
        /// <param name="machineName">机器名称</param>
        /// <returns>如果存在返回 true</returns>
        public bool ContainsStation(string machineName)
        {
            if (string.IsNullOrEmpty(machineName)) return false;

            return _machineCards.Any(c =>
                c.ctrConfig.AviName == machineName ||
                c.MachineName == machineName);
        }

        /// <summary>
        /// 获取内部控件列表（用于外部访问）
        /// </summary>
        public IReadOnlyList<MachineStatusCard> GetMachineCards()
        {
            return _machineCards.AsReadOnly();
        }
    }
}

using DeepSightAI.Properties;
using DeepSightModel;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    public partial class AviCtr2Container : UserControl
    {
        private List<AviCtr2> aviCtr2Controls = new List<AviCtr2>();
        // 缓存处理过的背景，避免每次 OnPaint 重新应用 ColorMatrix 和缩放
        private Bitmap _cachedBackground;
        private readonly object _bgLock = new object();

        public AviCtr2Container()
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
        /// 根据配置列表增量创建/更新 AviCtr2 控件（避免每次全清导致重绘开销）
        /// </summary>
        /// <param name="watchPaths">配置列表</param>
        public void CreateMachinePanels(List<WatchPathConfig> watchPaths)
        {
            if (watchPaths == null) return;

            // 现有映射
            var existingMap = aviCtr2Controls.ToDictionary(c => c.ctrConfig.AviName, c => c);
            var incomingNames = new HashSet<string>(watchPaths.Select(w => w.AviName));

            flowLayoutPanel1.SuspendLayout();
            try
            {
                // 移除不存在的
                for (int i = aviCtr2Controls.Count - 1; i >= 0; i--)
                {
                    var ctr = aviCtr2Controls[i];
                    if (!incomingNames.Contains(ctr.ctrConfig.AviName))
                    {
                        flowLayoutPanel1.Controls.Remove(ctr);
                        aviCtr2Controls.RemoveAt(i);
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
                        AddAviControl(cfg);
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
            foreach (var ctr in aviCtr2Controls)
            {
                if (configMap.TryGetValue(ctr.ctrConfig.AviName, out var cfg))
                {
                    ctr.ctrConfig = cfg;
                    ctr.UpdateDisplay();
                }
            }
        }

        /// <summary>
        /// 添加单个 AviCtr2 控件
        /// </summary>
        private void AddAviControl(WatchPathConfig watchPath)
        {
            try
            {
                AviCtr2 ctr = new AviCtr2(watchPath);
                aviCtr2Controls.Add(ctr);
                flowLayoutPanel1.Controls.Add(ctr);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create AviCtr2: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取所有 AviCtr2 控件的配置列表
        /// </summary>
        public List<WatchPathConfig> GetAllConfigs()
        {
            return aviCtr2Controls.Select(ctr => ctr.ctrConfig).ToList();
        }

        /// <summary>
        /// 更新指定名称的 AviCtr2 控件的统计信息
        /// </summary>
        public void UpdateAviCtrStats(string aviName, int totalImages, int aiOkImages)
        {
            var ctr = aviCtr2Controls.FirstOrDefault(c => c.ctrConfig.AviName == aviName);
            if (ctr != null)
            {
                ctr.AiFilterCount = totalImages;
                ctr.AiOkImages = aiOkImages;
            }
        }

        /// <summary>
        /// 更新所有 AviCtr2 控件的信息
        /// </summary>
        public void UpdateAllAviCtrsInfo(Func<string, (string LotNumber, string SerialNumber, string ProductSerial, string PathIndex, double Utilization)> getLatestPanelInfo)
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    foreach (var ctr in aviCtr2Controls)
                    {
                        if (ctr.ctrConfig.IsEnable)
                        {
                            var info = getLatestPanelInfo(ctr.ctrConfig.AviName);
                            ctr.LotId = info.LotNumber;
                            ctr.ProductSerial = info.SerialNumber;
                            ctr.PathIndex = info.PathIndex;
                            ctr.Utilization = info.Utilization;
                        }
                    }
                }));
            }
        }

        public void UpdateAviCtrInfo(string aviName, string productSerial, string lotId, double utilization)
        {
            foreach (var ctr in aviCtr2Controls)
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

        readonly ColorMatrix colorMatrix = new ColorMatrix(new float[][]
        {
            new float[] {1, 0, 0, 0, 0},
            new float[] {0, 1, 0, 0, 0},
            new float[] {0, 0, 1, 0, 0},
            new float[] {0, 0, 0, 0.725f, 0},
            new float[] {0, 0, 0, 0, 1}
        });

        // 减少背景重复绘制开销
        private void RebuildBackgroundCache()
        {
            lock (_bgLock)
            {
                _cachedBackground?.Dispose();
                _cachedBackground = null;
                var source = Resources.background;
                if (source == null || Width <= 0 || Height <= 0) return;
                var bmp = new Bitmap(Width, Height);
                using (var g = Graphics.FromImage(bmp))
                using (var attr = new ImageAttributes())
                {
                    attr.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.DrawImage(source, new Rectangle(0, 0, Width, Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attr);
                }
                _cachedBackground = bmp;
            }
            Invalidate(); // 刷新显示
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RebuildBackgroundCache();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            RebuildBackgroundCache();
        }

        // 避免默认背景擦除导致闪烁
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // 不调用 base，改由 OnPaint 使用缓存图
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); // 保留子控件等绘制
            try
            {
                lock (_bgLock)
                {
                    if (_cachedBackground != null)
                    {
                        e.Graphics.DrawImageUnscaled(_cachedBackground, 0, 0);
                        return;
                    }
                }
                // 兜底路径（首次或资源为空）
                Image backgroundImage = Resources.background;
                if (backgroundImage != null)
                {
                    using (var imageAttributes = new ImageAttributes())
                    {
                        imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        var destRect = new Rectangle(0, 0, this.Width, this.Height);
                        e.Graphics.DrawImage(backgroundImage, destRect, 0, 0, backgroundImage.Width, backgroundImage.Height, GraphicsUnit.Pixel, imageAttributes);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to draw background image: " + ex.Message);
            }
        }

        public void UpdateAllAviCtrLotSn(Func<string, Task<(string SerialNumber, string LotNumber, string ProductSerial, string PathIndex)>> getLatestPanelInfo)
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(async () =>
                {
                    // 并行获取减少多次 UI 刷新
                    var tasks = aviCtr2Controls
                        .Where(c => c.ctrConfig.IsEnable)
                        .Select(async c => (c, info: await getLatestPanelInfo(c.ctrConfig.AviName)))
                        .ToList();
                    var results = await Task.WhenAll(tasks);
                    foreach (var tuple in results)
                    {
                        var ctr = tuple.c;
                        var tmp = tuple.info;
                        if (tmp.LotNumber != null && tmp.SerialNumber != null)
                        {
                            ctr.LotId = tmp.LotNumber;
                            ctr.ProductSerial = tmp.ProductSerial;
                            ctr.PathIndex = tmp.PathIndex;
                        }
                    }
                }));
            }
        }

        public async Task UpdateMachineBoard(Func<string, Task<(string, string)>> GetLatestLotAndProductSerial, Func<string, string, Task<List<PanelDataRecord>>> getLatestPanelData)
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(async () =>
                {
                    // 并行收集所有需要的数据，减少 UI 线程切换
                    var lotTasks = aviCtr2Controls
                        .Where(c => c.ctrConfig.IsEnable)
                        .Select(async c => (c, lotAndSerial: await GetLatestLotAndProductSerial(c.ctrConfig.AviName)))
                        .ToList();
                    var lotResults = await Task.WhenAll(lotTasks);

                    // 获取 panel 数据
                    var panelTasks = lotResults.Select(async r => (r.c, r.lotAndSerial, data: await getLatestPanelData(r.c.ctrConfig.AviName, r.lotAndSerial.Item1))).ToList();
                    var panelResults = await Task.WhenAll(panelTasks);

                    foreach (var (c, lotAndSerial, data) in panelResults)
                    {
                        var ctr = c;
                        var tmp = lotAndSerial;
                        ctr.LotId = tmp.Item1;
                        ctr.ProductSerial = tmp.Item2;
                        var boardStat = PanelDataRecord.GetBoardStat(data);
                        ctr.AiOkImages = boardStat.aiFilterOKCount;
                        ctr.AiFilterCount = boardStat.aiFilterCount;
                        ctr.AviPassRate = boardStat.aviPanelCount == 0 ? 0 : (double)boardStat.aviPanelOKCount / boardStat.aviPanelCount;
                        ctr.Utilization = MathHelper.CalculateUtilizationRatePercent(
                        data.Where(t => t.AviCreationTime.HasValue && t.AviCreationTime.Value.Date == DateTime.Now.Date)
                            .Select(t => t.AviCreationTime.Value));
                    }
                }));
            }
        }
    }
}

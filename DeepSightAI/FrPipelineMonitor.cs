using DeepSightEvent;
using DeepSightModel;
using DeepSightWorkLib.Services.Pipeline;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// Pipeline 流水线可视化监控窗口
    /// 用于实时查看流水线各阶段队列深度、任务处理状态，方便排查问题
    /// </summary>
    public partial class FrPipelineMonitor : Form
    {
        /// <summary>最大保留的任务记录数</summary>
        private const int MaxTaskRows = 200;

        /// <summary>当前 Pipeline 统计信息（由定时器刷新）</summary>
        private PipelineStatistics _currentStats = new PipelineStatistics();

        /// <summary>上次采样的统计信息（用于对比已处理计数差值）</summary>
        private PipelineStatistics _prevStats = new PipelineStatistics();

        /// <summary>各阶段运行状态枚举</summary>
        private enum StageState
        {
            /// <summary>Pipeline 未运行 / Block 未激活</summary>
            Idle,
            /// <summary>正常运行中，队列为空</summary>
            Empty,
            /// <summary>正常运行中，队列有数据在处理</summary>
            Running,
            /// <summary>队列积压较多（>2）</summary>
            Busy,
            /// <summary>Block 已 Faulted 或 Canceled</summary>
            Error,
            /// <summary>Block 已正常完成 (RanToCompletion)</summary>
            Completed,
            /// <summary>阶段功能不可用（如 AI 引擎未初始化）</summary>
            Warning,
        }

        /// <summary>各状态对应的背景颜色</summary>
        private static readonly Color BgIdle      = Color.FromArgb(45, 55, 65);
        private static readonly Color BgEmpty     = Color.FromArgb(35, 80, 110);
        private static readonly Color BgRunning   = Color.FromArgb(30, 105, 65);
        private static readonly Color BgBusy      = Color.FromArgb(140, 100, 20);
        private static readonly Color BgError     = Color.FromArgb(130, 35, 35);
        private static readonly Color BgCompleted = Color.FromArgb(50, 75, 110);
        private static readonly Color BgWarning   = Color.FromArgb(130, 110, 0);

        /// <summary>Pipeline 各阶段定义（名称、填充色）</summary>
        private static readonly StageInfo[] _stages = new StageInfo[]
        {
            new StageInfo("JSON解析",  Color.FromArgb(45, 90, 138)),
            new StageInfo("图片加载",  Color.FromArgb(45, 90, 138)),
            new StageInfo("推理",      Color.FromArgb(138, 90, 45)),
            new StageInfo("广播",      Color.FromArgb(90, 90, 90)),
            new StageInfo("结果回写",  Color.FromArgb(45, 138, 90)),
            new StageInfo("后处理",    Color.FromArgb(45, 138, 90)),
        };

        public FrPipelineMonitor()
        {
            InitializeComponent();
            // 为流程图面板开启双缓冲，消除定时刷新导致的闪烁
            typeof(System.Windows.Forms.Panel)
                .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(panelFlow, true);
            this.Load += FrPipelineMonitor_Load;
            this.FormClosing += FrPipelineMonitor_FormClosing;
        }

        private void FrPipelineMonitor_Load(object sender, EventArgs e)
        {
            SystemEvent.EventSendTaskStatusToUI += OnTaskStatusReceived;
            refreshTimer.Start();
        }

        private void FrPipelineMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            refreshTimer.Stop();
            SystemEvent.EventSendTaskStatusToUI -= OnTaskStatusReceived;
        }

        #region 定时刷新

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                bool running = Machine.master?.IsPipelineRunning ?? false;
                _prevStats = _currentStats;
                _currentStats = Machine.master?.GetPipelineStatistics() ?? new PipelineStatistics();

                lblPipelineStatus.Text = running
                    ? $"● Pipeline 运行中  |  {_currentStats}"
                    : "○ Pipeline 未运行";
                lblPipelineStatus.ForeColor = running
                    ? Color.FromArgb(100, 230, 150)
                    : Color.FromArgb(200, 100, 100);

                panelFlow.Invalidate();
            }
            catch { }
        }

        #endregion

        #region 任务状态事件

        private void OnTaskStatusReceived(TaskStatusInfo statusInfo)
        {
            if (statusInfo == null || this.IsDisposed || !this.IsHandleCreated) return;

            try
            {
                this.BeginInvoke(new Action(() => AddTaskRow(statusInfo)));
            }
            catch { }
        }

        private void AddTaskRow(TaskStatusInfo info)
        {
            // 限制最大行数
            while (dgvTasks.Rows.Count >= MaxTaskRows)
            {
                dgvTasks.Rows.RemoveAt(dgvTasks.Rows.Count - 1);
            }

            dgvTasks.Rows.Insert(0, 1);
            var row = dgvTasks.Rows[0];
            row.Cells[0].Value = info.SerialNumber ?? "";
            row.Cells[1].Value = info.Side ?? "";
            row.Cells[2].Value = info.GetFullDisplayMessage();
            row.Cells[3].Value = info.Status.ToString();
            row.Cells[4].Value = info.Message ?? "";
            row.Cells[5].Value = info.Timestamp.ToString("HH:mm:ss.fff");
            row.DefaultCellStyle.ForeColor = TaskStatusHelper.GetStatusColor(info.Status);
        }

        #endregion

        #region 流程图绘制

        private void PanelFlow_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int panelW = panelFlow.Width;
            int panelH = panelFlow.Height;
            if (panelW < 100 || panelH < 60) return;

            int[] queueCounts = GetQueueCountArray();
            long[] processedDeltas = GetProcessedDeltaArray();
            StageState[] states = GetStageStateArray(queueCounts, processedDeltas);

            // 布局参数
            int boxW = 120, boxH = 56;
            int arrowLen = 40;
            int totalLinearW = 4 * boxW + 3 * arrowLen;
            int startX = (panelW - totalLinearW) / 2;
            int centerY = panelH / 2 - 10;

            // 绘制前4个阶段（JSON解析 → 图片加载 → 推理 → 广播）
            int x = startX;
            for (int i = 0; i < 4; i++)
            {
                DrawStageBox(g, x, centerY - boxH / 2, boxW, boxH, _stages[i], queueCounts[i], states[i]);
                if (i < 3)
                    DrawArrow(g, x + boxW, centerY, x + boxW + arrowLen, centerY);
                x += boxW + arrowLen;
            }

            // 广播后分叉
            int broadcastRight = startX + 3 * (boxW + arrowLen) + boxW;
            int branchX = broadcastRight + arrowLen;
            int branchOffsetY = 40;

            DrawArrow(g, broadcastRight, centerY, branchX, centerY - branchOffsetY);
            DrawStageBox(g, branchX, centerY - branchOffsetY - boxH / 2, boxW, boxH, _stages[4], queueCounts[4], states[4]);

            DrawArrow(g, broadcastRight, centerY, branchX, centerY + branchOffsetY);
            DrawStageBox(g, branchX, centerY + branchOffsetY - boxH / 2, boxW, boxH, _stages[5], queueCounts[5], states[5]);

            // 绘制图例
            DrawLegend(g, 10, panelH - 25);
        }

        private int[] GetQueueCountArray()
        {
            return new int[]
            {
                _currentStats.JsonParseInputCount,
                _currentStats.ImageLoadInputCount,
                _currentStats.InferenceInputCount,
                0,
                _currentStats.ResultWriteInputCount,
                _currentStats.PostProcessInputCount,
            };
        }

        /// <summary>计算两次采样间各阶段的已处理增量</summary>
        private long[] GetProcessedDeltaArray()
        {
            return new long[]
            {
                _currentStats.JsonParseProcessed  - _prevStats.JsonParseProcessed,
                _currentStats.ImageLoadProcessed  - _prevStats.ImageLoadProcessed,
                _currentStats.InferenceProcessed  - _prevStats.InferenceProcessed,
                0, // 广播无计数
                _currentStats.ResultWriteProcessed - _prevStats.ResultWriteProcessed,
                _currentStats.PostProcessProcessed - _prevStats.PostProcessProcessed,
            };
        }

        /// <summary>根据队列深度 + Block 状态 + 处理增量 计算各阶段的运行状态</summary>
        private StageState[] GetStageStateArray(int[] queueCounts, long[] processedDeltas)
        {
            bool defectReady = Machine.master?.DefectService?.IsInitialized ?? true;

            bool running = Machine.master?.IsPipelineRunning ?? false;
            if (!running)
            {
                var idleStates = new StageState[] { StageState.Idle, StageState.Idle, StageState.Idle,
                                                    StageState.Idle, StageState.Idle, StageState.Idle };
                if (!defectReady)
                    idleStates[2] = StageState.Warning;
                return idleStates;
            }

            System.Threading.Tasks.TaskStatus[] blockStatuses = new System.Threading.Tasks.TaskStatus[]
            {
                _currentStats.JsonParseStatus,
                _currentStats.ImageLoadStatus,
                _currentStats.InferenceStatus,
                _currentStats.BroadcastStatus,
                _currentStats.ResultWriteStatus,
                _currentStats.PostProcessStatus,
            };

            var states = new StageState[6];
            for (int i = 0; i < 6; i++)
            {
                states[i] = ResolveStageState(blockStatuses[i], queueCounts[i], processedDeltas[i]);
            }

            // 广播块（index=3）无缓冲队列和计数，从上下游推断状态
            if (states[3] != StageState.Error && states[3] != StageState.Completed)
            {
                bool neighborActive = processedDeltas[2] > 0 || processedDeltas[4] > 0 || processedDeltas[5] > 0
                                   || queueCounts[4] > 0 || queueCounts[5] > 0;
                states[3] = neighborActive ? StageState.Running : StageState.Empty;
            }

            // AI 引擎未初始化时，推理阶段始终显示警告
            if (!defectReady)
                states[2] = StageState.Warning;

            return states;
        }

        /// <summary>根据 Block TaskStatus、队列深度、处理增量推断可视状态</summary>
        private StageState ResolveStageState(System.Threading.Tasks.TaskStatus taskStatus, int queueCount, long processedDelta)
        {
            if (taskStatus == System.Threading.Tasks.TaskStatus.Faulted || taskStatus == System.Threading.Tasks.TaskStatus.Canceled)
                return StageState.Error;
            if (taskStatus == System.Threading.Tasks.TaskStatus.RanToCompletion)
                return StageState.Completed;
            // Block 仍在运行
            if (queueCount > 2)
                return StageState.Busy;
            if (queueCount > 0 || processedDelta > 0)
                return StageState.Running;
            return StageState.Empty;
        }

        /// <summary>根据状态获取背景颜色</summary>
        private Color GetBackgroundColor(StageState state)
        {
            switch (state)
            {
                case StageState.Idle:      return BgIdle;
                case StageState.Empty:     return BgEmpty;
                case StageState.Running:   return BgRunning;
                case StageState.Busy:      return BgBusy;
                case StageState.Error:     return BgError;
                case StageState.Completed: return BgCompleted;
                case StageState.Warning:   return BgWarning;
                default:                   return BgIdle;
            }
        }

        /// <summary>根据状态获取状态简称文本</summary>
        private string GetStateLabel(StageState state)
        {
            switch (state)
            {
                case StageState.Idle:      return "未运行";
                case StageState.Empty:     return "空闲";
                case StageState.Running:   return "处理中";
                case StageState.Busy:      return "积压";
                case StageState.Error:     return "异常";
                case StageState.Completed: return "已完成";
                case StageState.Warning:   return "未就绪";
                default:                   return "";
            }
        }

        private void DrawStageBox(Graphics g, int x, int y, int w, int h,
            StageInfo stage, int queueCount, StageState state)
        {
            Color bgColor = GetBackgroundColor(state);

            using (var bgBrush = new SolidBrush(bgColor))
            using (var borderPen = new Pen(Color.FromArgb(60, 80, 95), 1f))
            using (var nameFont = new Font("微软雅黑", 10F, FontStyle.Bold))
            using (var infoFont = new Font("微软雅黑", 8F))
            {
                var rect = new Rectangle(x, y, w, h);
                g.FillRectangle(bgBrush, rect);
                g.DrawRectangle(borderPen, rect);

                // 阶段名称
                using (var nameBrush = new SolidBrush(Color.White))
                {
                    var nameSize = g.MeasureString(stage.Name, nameFont);
                    g.DrawString(stage.Name, nameFont, nameBrush,
                        x + (w - nameSize.Width) / 2, y + 5);
                }

                // 状态标签 + 队列数
                string infoText = $"{GetStateLabel(state)} | 队列:{queueCount}";
                using (var infoBrush = new SolidBrush(Color.FromArgb(210, 225, 240)))
                {
                    var infoSize = g.MeasureString(infoText, infoFont);
                    g.DrawString(infoText, infoFont, infoBrush,
                        x + (w - infoSize.Width) / 2, y + h - infoSize.Height - 4);
                }
            }
        }

        private void DrawArrow(Graphics g, int x1, int y1, int x2, int y2)
        {
            using (var pen = new Pen(Color.FromArgb(100, 160, 200), 2f))
            {
                pen.CustomEndCap = new AdjustableArrowCap(5, 5);
                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        /// <summary>绘制底部图例</summary>
        private void DrawLegend(Graphics g, int x, int y)
        {
            var items = new[] {
                (BgIdle,      "未运行"),
                (BgEmpty,     "空闲"),
                (BgRunning,   "处理中"),
                (BgBusy,      "积压"),
                (BgError,     "异常"),
                (BgCompleted, "已完成"),
                (BgWarning,   "未就绪"),
            };

            using (var font = new Font("微软雅黑", 8F))
            {
                int cx = x;
                foreach (var (color, label) in items)
                {
                    using (var brush = new SolidBrush(color))
                    using (var textBrush = new SolidBrush(Color.FromArgb(180, 200, 220)))
                    {
                        g.FillRectangle(brush, cx, y, 14, 14);
                        g.DrawString(label, font, textBrush, cx + 18, y - 1);
                        cx += (int)g.MeasureString(label, font).Width + 28;
                    }
                }
            }
        }

        #endregion

        #region 按钮事件

        private void BtnClose_Click(object sender, EventArgs e) => this.Close();

        private void BtnClear_Click(object sender, EventArgs e) => dgvTasks.Rows.Clear();

        #endregion

        /// <summary>流程阶段信息</summary>
        private class StageInfo
        {
            public string Name { get; }
            public Color Color { get; }

            public StageInfo(string name, Color color)
            {
                Name = name;
                Color = color;
            }
        }
    }
}
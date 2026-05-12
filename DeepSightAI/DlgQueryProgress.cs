using DeepSightTool;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// 查询进度对话框：以模态方式显示，期间禁用其他操作。
    /// 在 OnShown 时自动调用 WorkAsync 委托执行后台工作，完成、异常或取消后自动关闭。
    /// 用户可点击取消按钮发起取消请求，WorkAsync 应配合 CancellationToken 响应。
    /// </summary>
    public partial class DlgQueryProgress : Form
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _completed;
        private bool _allowClose;

        /// <summary>
        /// 待执行的异步工作，通过 IProgress 报告阶段文本，可通过 CancellationToken 响应取消。
        /// </summary>
        public Func<IProgress<string>, CancellationToken, Task> WorkAsync { get; set; }

        /// <summary>
        /// 执行过程中捕获到的异常（若有，OperationCanceledException 除外）。
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// 任务是否被用户主动取消。
        /// </summary>
        public bool IsCanceled { get; private set; }

        public DlgQueryProgress(string title = "正在查询数据...")
        {
            InitializeComponent();
            lblTitle.Text = title;
            this.Text = title;
        }

        /// <summary>
        /// 更新状态文本（线程安全）。
        /// </summary>
        public void SetStatus(string text)
        {
            if (IsDisposed || Disposing) return;
            string value = text ?? string.Empty;
            if (lblStatus.InvokeRequired)
            {
                try { lblStatus.BeginInvoke(new Action(() => { if (!IsDisposed) lblStatus.Text = value; })); }
                catch (ObjectDisposedException) { }
                catch (InvalidOperationException) { }
            }
            else
            {
                lblStatus.Text = value;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // 推迟到消息循环空闲后再启动任务，确保窗体已完成首次绘制
            BeginInvoke(new Action(async () => await RunWorkAsync()));
        }

        private async Task RunWorkAsync()
        {
            var progress = new Progress<string>(SetStatus);
            try
            {
                if (WorkAsync != null)
                {
                    await WorkAsync(progress, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                IsCanceled = true;
                LogTextHelper.Info("DlgQueryProgress: 任务已取消");
            }
            catch (Exception ex)
            {
                Error = ex;
                LogTextHelper.Error($"DlgQueryProgress 执行异常: {ex}");
            }
            finally
            {
                _completed = true;
                _allowClose = true;
                if (!IsDisposed)
                {
                    DialogResult = IsCanceled ? DialogResult.Cancel : DialogResult.OK;
                    Close();
                }
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            RequestCancel();
        }

        /// <summary>
        /// 发起取消：禁用按钮，更新文本，触发 CancellationToken；
        /// 真正关闭由任务结束后的 finally 完成。
        /// </summary>
        private void RequestCancel()
        {
            if (_completed || _cts.IsCancellationRequested) return;
            btnCancel.Enabled = false;
            btnCancel.Text = "正在取消...";
            SetStatus("正在取消，请稍候...");
            try { _cts.Cancel(); } catch { /* ignore */ }
        }

        /// <summary>
        /// 用户主动关闭（X 按钮 / Alt+F4）：转为取消请求；任务结束后由代码关闭。
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_allowClose && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                RequestCancel();
                return;
            }
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try { _cts?.Dispose(); } catch { /* ignore */ }
            base.OnFormClosed(e);
        }
    }
}

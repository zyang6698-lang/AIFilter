using DeepSightCommunication;
using DeepSightModel;
using DeepSightModel.Configuration;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// MinIO 目录浏览选择对话框：顶部下拉选择已启用的 MinIO IP，
    /// 下方 TreeView 懒加载 bucket 下的目录结构，确认后返回选中的 IP 与对象键前缀。
    /// </summary>
    internal class FrMinioFolderPicker : Form
    {
        private readonly MinioClass _minio;
        private readonly string _bucket;

        private ComboBox _cmbIp;
        private TreeView _tree;
        private Button _btnOk;
        private Button _btnCancel;
        private Label _lblIp;
        private Label _lblStatus;

        private const string LoadingPlaceholder = "__loading__";

        /// <summary>
        /// 用户选择的 MinIO IP。
        /// </summary>
        public string SelectedIp { get; private set; }

        /// <summary>
        /// 用户选择的对象键前缀（以 '/' 结尾；为空表示桶根）。
        /// </summary>
        public string SelectedPrefix { get; private set; } = string.Empty;

        public FrMinioFolderPicker(MinioClass minio, IEnumerable<string> availableIps, string bucket)
        {
            _minio = minio ?? throw new ArgumentNullException(nameof(minio));
            _bucket = string.IsNullOrWhiteSpace(bucket) ? "deepiresults" : bucket;
            BuildUi(availableIps?.ToList() ?? new List<string>());
        }

        private void BuildUi(List<string> ips)
        {
            Text = $"选择 MinIO 目录（bucket: {_bucket}）";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(480, 520);
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(360, 360);

            _lblIp = new Label { Text = "MinIO IP:", Left = 12, Top = 14, AutoSize = true };
            Controls.Add(_lblIp);

            _cmbIp = new ComboBox
            {
                Left = 80,
                Top = 10,
                Width = ClientSize.Width - 92,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            foreach (var ip in ips) _cmbIp.Items.Add(ip);
            if (_cmbIp.Items.Count > 0) _cmbIp.SelectedIndex = 0;
            _cmbIp.SelectedIndexChanged += async (s, e) => await ReloadRootAsync();
            Controls.Add(_cmbIp);

            _tree = new TreeView
            {
                Left = 12,
                Top = 44,
                Width = ClientSize.Width - 24,
                Height = ClientSize.Height - 44 - 50,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                HideSelection = false
            };
            _tree.BeforeExpand += Tree_BeforeExpand;
            Controls.Add(_tree);

            _lblStatus = new Label
            {
                Left = 12,
                Top = ClientSize.Height - 44,
                Width = ClientSize.Width - 190,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Text = ""
            };
            Controls.Add(_lblStatus);

            _btnOk = new Button
            {
                Text = "确定",
                Left = ClientSize.Width - 170,
                Top = ClientSize.Height - 40,
                Width = 75,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.OK
            };
            _btnOk.Click += (s, e) => OnOkClicked();
            Controls.Add(_btnOk);

            _btnCancel = new Button
            {
                Text = "取消",
                Left = ClientSize.Width - 90,
                Top = ClientSize.Height - 40,
                Width = 75,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(_btnCancel);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;

            Shown += async (s, e) => await ReloadRootAsync();
        }

        private void OnOkClicked()
        {
            SelectedIp = _cmbIp.SelectedItem as string;
            var node = _tree.SelectedNode;
            SelectedPrefix = node?.Name ?? string.Empty; // Node.Name 中存储完整前缀
            if (string.IsNullOrWhiteSpace(SelectedIp))
            {
                MessageBox.Show(this, "请先选择 MinIO IP。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        }

        private async Task ReloadRootAsync()
        {
            _tree.Nodes.Clear();
            string ip = _cmbIp.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(ip)) return;

            _lblStatus.Text = $"正在加载 {_bucket}/ ...";
            try
            {
                var children = await _minio.ListSubFoldersAsync(_bucket, string.Empty, ip);
                foreach (var prefix in children)
                {
                    _tree.Nodes.Add(BuildFolderNode(prefix));
                }
                _lblStatus.Text = $"已加载 {children.Count} 个目录";
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"FrMinioFolderPicker 加载根目录失败: {ex.Message}");
                _lblStatus.Text = $"加载失败: {ex.Message}";
            }
        }

        private async void Tree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var node = e.Node;
            // 仅当只有占位节点时才做懒加载
            if (node.Nodes.Count != 1 || node.Nodes[0].Name != LoadingPlaceholder) return;

            string ip = _cmbIp.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(ip)) { e.Cancel = true; return; }

            string prefix = node.Name;
            node.Nodes.Clear();
            _lblStatus.Text = $"正在加载 {prefix} ...";
            try
            {
                var children = await _minio.ListSubFoldersAsync(_bucket, prefix, ip);
                foreach (var child in children)
                {
                    node.Nodes.Add(BuildFolderNode(child));
                }
                _lblStatus.Text = $"{prefix} 加载完成（{children.Count} 项）";
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"FrMinioFolderPicker 加载 {prefix} 失败: {ex.Message}");
                _lblStatus.Text = $"加载失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 创建一个目录节点：显示名去掉末尾 '/'，Name 存完整前缀，并挂一个占位子节点以显示展开箭头
        /// </summary>
        private static TreeNode BuildFolderNode(string fullPrefix)
        {
            var display = fullPrefix.TrimEnd('/');
            int slash = display.LastIndexOf('/');
            if (slash >= 0) display = display.Substring(slash + 1);
            var node = new TreeNode(display) { Name = fullPrefix };
            node.Nodes.Add(new TreeNode("正在加载...") { Name = LoadingPlaceholder });
            return node;
        }
    }
}

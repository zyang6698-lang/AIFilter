using DeepSightCommunication;
using DeepSightModel;
using DeepSightModel.Configuration;
using DeepSightTool;
using DeepSightWorkLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// 推理请求生成对话框：TabControl 两个 Tab 分别提供"按MinIO目录"和"按Lot批次"两种生成方式。
    /// 实际业务逻辑委托给 <see cref="InferenceRequestService"/>。
    /// </summary>
    public partial class DlgGenerateInferenceRequest : Form
    {
        private readonly MinioClass _minio;
        private readonly InferenceRequestService _service;
        private List<LevelDbConfig> _dbConfigs;

        public DlgGenerateInferenceRequest(MinioClass minio)
        {
            _minio = minio ?? new MinioClass();
            _service = new InferenceRequestService();
            InitializeComponent();
            InitLotTab();
        }

        private void InitLotTab()
        {
            _dbConfigs = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled)
                .ToList();
            cmbDb.Items.Clear();
            foreach (var c in _dbConfigs) cmbDb.Items.Add(c.DisplayName);
            if (cmbDb.Items.Count > 0) cmbDb.SelectedIndex = 0;
            cmbLot.Items.Clear();
        }

        #region Tab1：按 MinIO 目录

        private async void btnRunByMinio_Click(object sender, EventArgs e)
        {
            var dbConfigs = LevelDbConfigManager.Instance.Databases
                .Where(db => db.IsEnabled)
                .ToList();
            if (dbConfigs.Count == 0)
            {
                MessageBox.Show("未找到已启用的 LevelDB 配置，请先在设置中配置。", "配置缺失",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var availableIps = dbConfigs
                .SelectMany(db => new[] { db.MinioIpA, db.MinioIpB })
                .Where(ip => !string.IsNullOrWhiteSpace(ip))
                .Distinct()
                .ToList();
            if (availableIps.Count == 0)
            {
                MessageBox.Show("未在已启用的 LevelDB 配置中找到任何 MinIO IP。", "配置缺失",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string bucket = MinioSettings.Instance.DefaultBucket ?? "deepiresults";
            string selectedIp, selectedPrefix;
            using (var picker = new DlgMinioFolderPicker(_minio, availableIps, bucket))
            {
                if (picker.ShowDialog(this) != DialogResult.OK) return;
                selectedIp = picker.SelectedIp;
                selectedPrefix = picker.SelectedPrefix ?? string.Empty;
            }

            btnRunByMinio.Enabled = false;
            try
            {
                SetMinioStatus(0, $"正在扫描 {bucket}/{selectedPrefix} 下的 panel.json ...");

                var snGroups = await _service.ScanAndGroupPanelsAsync(_minio, bucket, selectedPrefix, selectedIp,
                    (parsed, total) =>
                    {
                        if (total <= 0) return;
                        int pct = 10 + (int)(parsed * 30.0 / total);
                        SetMinioStatus(pct, $"解析 panel.json ({parsed}/{total})");
                    });

                var validEntries = snGroups
                    .Where(kv => kv.Value.ContainsKey("A") || kv.Value.ContainsKey("B"))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
                if (validEntries.Count == 0)
                {
                    SetMinioStatus(0, "未找到有效的 SN 数据");
                    MessageBox.Show("未找到有效的 SN 数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int bothCount = validEntries.Count(kv => kv.Value.ContainsKey("A") && kv.Value.ContainsKey("B"));
                int onlyA = validEntries.Count(kv => kv.Value.ContainsKey("A") && !kv.Value.ContainsKey("B"));
                int onlyB = validEntries.Count(kv => !kv.Value.ContainsKey("A") && kv.Value.ContainsKey("B"));
                SetMinioStatus(45, $"找到 {validEntries.Count} 个有效 SN（AB:{bothCount} 仅A:{onlyA} 仅B:{onlyB}），开始发送请求...");

                var minioPort = MinioSettings.Instance.DefaultPort;
                var result = await _service.SendMergedInferenceRequestsAsync(
                    dbConfigs, validEntries, selectedIp, minioPort, 500,
                    (sent, total, succ, fail) =>
                    {
                        int pct = 45 + (int)(sent * 50.0 / total);
                        SetMinioStatus(pct, $"发送请求 ({sent}/{total})，成功:{succ} 失败:{fail}");
                    });

                SetMinioStatus(100, $"完成！共 {validEntries.Count} 个SN，成功:{result.success} 失败:{result.failed}");
                MessageBox.Show(
                    $"推理请求发送完成！\n共 {validEntries.Count} 个 SN（AB:{bothCount} 仅A:{onlyA} 仅B:{onlyB}）\n成功: {result.success}\n失败: {result.failed}",
                    "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("按MinIO目录生成推理请求失败", ex);
                SetMinioStatus(0, $"失败: {ex.Message}");
                MessageBox.Show($"生成推理请求失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRunByMinio.Enabled = true;
            }
        }

        private void SetMinioStatus(int percent, string msg)
        {
            if (InvokeRequired) { BeginInvoke((Action)(() => SetMinioStatus(percent, msg))); return; }
            progressBarMinio.Value = Math.Max(0, Math.Min(100, percent));
            lblMinioStatus.Text = msg;
        }

        #endregion

        #region Tab2：按 Lot 批次

        private async void btnLoadLots_Click(object sender, EventArgs e)
        {
            var sourceConfig = GetSelectedDbConfig();
            if (sourceConfig == null)
            {
                MessageBox.Show("请先选择源 LevelDB。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnLoadLots.Enabled = false;
            cmbLot.Items.Clear();
            try
            {
                SetLotStatus(10, $"正在读取 {sourceConfig.DisplayName} 的 lot_panel ...");

                bool ok = false;
                Dictionary<string, List<string>> lotToSns = null;
                string err = null;
                await System.Threading.Tasks.Task.Run(() =>
                {
                    ok = _service.TryReadLotToSnMapping(sourceConfig, out lotToSns, out err);
                });

                if (!ok)
                {
                    SetLotStatus(0, $"读取失败: {err}");
                    MessageBox.Show(err ?? "读取 lot_panel 失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (lotToSns == null || lotToSns.Count == 0)
                {
                    SetLotStatus(0, "lot_panel 中未解析到有效 lot");
                    MessageBox.Show("lot_panel 中未解析到任何 lot。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (var lot in lotToSns.Keys.OrderBy(k => k))
                    cmbLot.Items.Add(lot);
                if (cmbLot.Items.Count > 0) cmbLot.SelectedIndex = 0;

                SetLotStatus(100, $"共加载 {cmbLot.Items.Count} 个 lot，请选择并点击\"按选定 Lot 生成\"");
                LogTextHelper.Info($"加载 lot 列表完成：db={sourceConfig.DisplayName}, 数量={cmbLot.Items.Count}");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("加载 lot 列表失败", ex);
                SetLotStatus(0, $"失败: {ex.Message}");
                MessageBox.Show($"加载 lot 列表失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLoadLots.Enabled = true;
            }
        }

        private async void btnRunByLot_Click(object sender, EventArgs e)
        {
            var sourceConfig = GetSelectedDbConfig();
            if (sourceConfig == null)
            {
                MessageBox.Show("请先选择源 LevelDB。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string selectedLot = cmbLot.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedLot))
            {
                MessageBox.Show("请先加载并选择 Lot。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnRunByLot.Enabled = false;
            try
            {
                SetLotStatus(10, $"正在读取 lot={selectedLot} 的 SN 列表 ...");
                var sns = await _service.FetchSnsByLotAsync(sourceConfig, selectedLot);
                if (sns == null || sns.Count == 0)
                {
                    SetLotStatus(0, $"lot={selectedLot} 未解析到任何 SN");
                    MessageBox.Show($"lot={selectedLot} 未解析到任何 SN。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                LogTextHelper.Info($"按Lot生成推理请求：选中 lot={selectedLot}, SN数={sns.Count}");
                SetLotStatus(50, $"lot={selectedLot}, SN数={sns.Count}，正在读取 AVI_results_db ...");

                var snValues = await _service.FetchAviResultsBySnsAsync(sourceConfig, sns,
                    (idx, total, succ, fail) =>
                    {
                        int pct = 50 + (int)(idx * 45.0 / total);
                        SetLotStatus(pct, $"读取 AVI_results_db ({idx}/{total}), 成功:{succ} 失败:{fail}");
                    });

                int finalSucc = snValues.Count;
                int finalFail = sns.Count - finalSucc;
                SetLotStatus(100, $"完成：lot={selectedLot}, SN={sns.Count}, 成功:{finalSucc} 失败:{finalFail}");
                MessageBox.Show(
                    $"lot={selectedLot}\nSN总数: {sns.Count}\n读取成功: {finalSucc}\n读取失败: {finalFail}\n\n(后续步骤待扩展)",
                    "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("按Lot生成推理请求失败", ex);
                SetLotStatus(0, $"失败: {ex.Message}");
                MessageBox.Show($"按Lot生成推理请求失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRunByLot.Enabled = true;
            }
        }

        private LevelDbConfig GetSelectedDbConfig()
        {
            int idx = cmbDb.SelectedIndex;
            if (_dbConfigs == null || idx < 0 || idx >= _dbConfigs.Count) return null;
            return _dbConfigs[idx];
        }

        private void SetLotStatus(int percent, string msg)
        {
            if (InvokeRequired) { BeginInvoke((Action)(() => SetLotStatus(percent, msg))); return; }
            progressBarLot.Value = Math.Max(0, Math.Min(100, percent));
            lblLotStatus.Text = msg;
        }

        #endregion
    }
}

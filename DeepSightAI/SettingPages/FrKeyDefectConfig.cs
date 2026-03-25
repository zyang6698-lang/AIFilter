using DeepSightModel.Configuration;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    /// <summary>
    /// 重点缺陷管理配置界面
    /// </summary>
    public partial class FrKeyDefectConfig : Form
    {
        public FrKeyDefectConfig()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
        }

        private static FrKeyDefectConfig _instance;
        public static FrKeyDefectConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FrKeyDefectConfig();
                return _instance;
            }
        }

        private bool _isLoading = false;

        private void FrKeyDefectConfig_Load(object sender, EventArgs e)
        {
            LoadConfigToGrid();
            LoadAlarmConfig();
        }

        /// <summary>
        /// 从配置管理器加载数据到 DataGridView
        /// </summary>
        private void LoadConfigToGrid()
        {
            _isLoading = true;
            try
            {
                dgvDefects.Rows.Clear();
                var config = KeyDefectConfigManager.Instance.GetCachedConfig();
                foreach (var entry in config.DefectEntries)
                {
                    int rowIndex = dgvDefects.Rows.Add();
                    var row = dgvDefects.Rows[rowIndex];
                    row.Cells["colDefectName"].Value = entry.DefectName;
                    row.Cells["colIsKey"].Value = entry.IsKey;
                    row.Cells["colAutoDiscovered"].Value = entry.AutoDiscovered ? "自动发现" : "手动添加";
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// 加载报警配置到UI
        /// </summary>
        private void LoadAlarmConfig()
        {
            var alarm = KeyDefectConfigManager.Instance.GetAlarmConfig();
            chkAlarmEnabled.Checked = alarm.Enabled;
            nudRatioThreshold.Value = (decimal)(alarm.AlarmRatioThreshold * 100);
            nudCountThreshold.Value = alarm.AlarmCountThreshold;
            nudCooldown.Value = alarm.AlarmCooldownSeconds;
        }

        /// <summary>
        /// 手动添加缺陷名称
        /// </summary>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string name = txtNewDefectName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("请输入缺陷名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 检查重复
            foreach (DataGridViewRow row in dgvDefects.Rows)
            {
                if (row.IsNewRow) continue;
                if (string.Equals(row.Cells["colDefectName"].Value?.ToString(), name, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"缺陷 '{name}' 已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            int idx = dgvDefects.Rows.Add();
            var newRow = dgvDefects.Rows[idx];
            newRow.Cells["colDefectName"].Value = name;
            newRow.Cells["colIsKey"].Value = true;
            newRow.Cells["colAutoDiscovered"].Value = "手动添加";
            txtNewDefectName.Text = "";
        }

        /// <summary>
        /// 删除选中行
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvDefects.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要删除的行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            foreach (DataGridViewRow row in dgvDefects.SelectedRows)
            {
                if (!row.IsNewRow)
                    dgvDefects.Rows.Remove(row);
            }
        }

        /// <summary>
        /// 全选/全不选重点标记
        /// </summary>
        private void btnToggleAll_Click(object sender, EventArgs e)
        {
            bool anyUnchecked = dgvDefects.Rows.Cast<DataGridViewRow>()
                .Any(r => !r.IsNewRow && (r.Cells["colIsKey"].Value == null || !(bool)r.Cells["colIsKey"].Value));
            foreach (DataGridViewRow row in dgvDefects.Rows)
            {
                if (!row.IsNewRow)
                    row.Cells["colIsKey"].Value = anyUnchecked;
            }
        }

        /// <summary>
        /// 当 CheckBox 列的 dirty state 变化时立即提交
        /// </summary>
        private void dgvDefects_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvDefects.IsCurrentCellDirty)
                dgvDefects.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public bool SaveConfig()
        {
            try
            {
                var config = new KeyDefectConfig();
                config.DefectEntries = new List<KeyDefectEntry>();

                foreach (DataGridViewRow row in dgvDefects.Rows)
                {
                    if (row.IsNewRow) continue;
                    config.DefectEntries.Add(new KeyDefectEntry
                    {
                        DefectName = row.Cells["colDefectName"].Value?.ToString() ?? "",
                        IsKey = row.Cells["colIsKey"].Value != null && (bool)row.Cells["colIsKey"].Value,
                        AutoDiscovered = row.Cells["colAutoDiscovered"].Value?.ToString() == "自动发现"
                    });
                }

                config.AlarmConfig = new KeyDefectAlarmConfig
                {
                    Enabled = chkAlarmEnabled.Checked,
                    AlarmRatioThreshold = (double)nudRatioThreshold.Value / 100.0,
                    AlarmCountThreshold = (int)nudCountThreshold.Value,
                    AlarmCooldownSeconds = (int)nudCooldown.Value
                };

                return KeyDefectConfigManager.Instance.SaveAndReload(config);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"保存重点缺陷配置失败: {ex.Message}");
                return false;
            }
        }
    }
}


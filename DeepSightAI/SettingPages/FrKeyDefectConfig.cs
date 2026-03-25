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
    /// 重点缺陷管理配置界面（支持多 profile + 料号映射）
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

        /// <summary>
        /// 当前选中的 profile 名称
        /// </summary>
        private string CurrentProfileName =>
            cboProfile.SelectedItem?.ToString() ?? KeyDefectConfigManager.DefaultProfileName;

        private void FrKeyDefectConfig_Load(object sender, EventArgs e)
        {
            LoadProfileList();
        }

        #region Profile 管理

        /// <summary>
        /// 加载 profile 列表到 ComboBox，并选中 Default
        /// </summary>
        private void LoadProfileList()
        {
            _isLoading = true;
            try
            {
                cboProfile.Items.Clear();
                var names = KeyDefectConfigManager.Instance.GetProfileNames();
                foreach (var name in names)
                    cboProfile.Items.Add(name);

                var defaultIdx = cboProfile.Items.IndexOf(KeyDefectConfigManager.DefaultProfileName);
                cboProfile.SelectedIndex = defaultIdx >= 0 ? defaultIdx : 0;
            }
            finally
            {
                _isLoading = false;
            }

            LoadConfigToGrid();
            LoadAlarmConfig();
        }

        private void cboProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            LoadConfigToGrid();
            LoadAlarmConfig();
        }

        private void btnNewProfile_Click(object sender, EventArgs e)
        {
            string name = ShowInputDialog("请输入新配置名称：", "新建缺陷配置");
            if (string.IsNullOrEmpty(name)) return;

            if (name.Equals(KeyDefectConfigManager.DefaultProfileName, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("不能使用保留名称 'Default'", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!KeyDefectConfigManager.Instance.CreateProfile(name))
            {
                MessageBox.Show($"配置 '{name}' 已存在或创建失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isLoading = true;
            cboProfile.Items.Add(name);
            _isLoading = false;
            cboProfile.SelectedItem = name;
        }

        private void btnDeleteProfile_Click(object sender, EventArgs e)
        {
            var name = CurrentProfileName;
            if (name.Equals(KeyDefectConfigManager.DefaultProfileName, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("不能删除默认配置 'Default'", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"确定要删除配置 '{name}' 吗？\n引用此配置的料号将回退到 Default。",
                "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            if (KeyDefectConfigManager.Instance.DeleteProfile(name))
            {
                LoadProfileList();
            }
        }

        #endregion

        #region 缺陷列表

        private void LoadConfigToGrid()
        {
            _isLoading = true;
            try
            {
                dgvDefects.Rows.Clear();
                var config = KeyDefectConfigManager.Instance.GetCachedConfig(CurrentProfileName);
                foreach (var entry in config.DefectEntries)
                {
                    int rowIndex = dgvDefects.Rows.Add();
                    var row = dgvDefects.Rows[rowIndex];
                    row.Cells["colDefectName"].Value = entry.DefectName;
                    row.Cells["colIsKey"].Value = entry.IsKey;
                    row.Cells["colIsDirectReport"].Value = entry.IsDirectReport;
                    row.Cells["colAutoDiscovered"].Value = entry.AutoDiscovered ? "自动发现" : "手动添加";
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void LoadAlarmConfig()
        {
            var alarm = KeyDefectConfigManager.Instance.GetAlarmConfig(CurrentProfileName);
            chkAlarmEnabled.Checked = alarm.Enabled;
            nudRatioThreshold.Value = (decimal)(alarm.AlarmRatioThreshold * 100);
            nudCountThreshold.Value = alarm.AlarmCountThreshold;
            nudCooldown.Value = alarm.AlarmCooldownSeconds;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string name = txtNewDefectName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("请输入缺陷名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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

        private void dgvDefects_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvDefects.IsCurrentCellDirty)
                dgvDefects.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 简易输入对话框（替代 Microsoft.VisualBasic.Interaction.InputBox）
        /// </summary>
        private static string ShowInputDialog(string prompt, string title)
        {
            using (var form = new Form())
            using (var lbl = new Label())
            using (var txt = new TextBox())
            using (var btnOk = new Button())
            using (var btnCancel = new Button())
            {
                form.Text = title;
                form.ClientSize = new Size(320, 110);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.BackColor = Color.FromArgb(20, 38, 48);

                lbl.Text = prompt;
                lbl.ForeColor = Color.FromArgb(216, 219, 188);
                lbl.SetBounds(12, 12, 296, 18);

                txt.SetBounds(12, 34, 296, 21);
                txt.BackColor = Color.FromArgb(29, 48, 60);
                txt.ForeColor = Color.FromArgb(216, 219, 188);

                btnOk.Text = "确定";
                btnOk.DialogResult = DialogResult.OK;
                btnOk.SetBounds(148, 65, 75, 28);
                btnOk.BackColor = Color.FromArgb(0, 64, 82);
                btnOk.ForeColor = Color.FromArgb(216, 219, 188);
                btnOk.FlatStyle = FlatStyle.Flat;

                btnCancel.Text = "取消";
                btnCancel.DialogResult = DialogResult.Cancel;
                btnCancel.SetBounds(233, 65, 75, 28);
                btnCancel.BackColor = Color.FromArgb(0, 64, 82);
                btnCancel.ForeColor = Color.FromArgb(216, 219, 188);
                btnCancel.FlatStyle = FlatStyle.Flat;

                form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                return form.ShowDialog() == DialogResult.OK ? txt.Text.Trim() : "";
            }
        }

        #endregion

        #region 保存

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
                        IsDirectReport = row.Cells["colIsDirectReport"].Value != null && (bool)row.Cells["colIsDirectReport"].Value,
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

                if (!KeyDefectConfigManager.Instance.SaveAndReload(config, CurrentProfileName))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"保存重点缺陷配置失败: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}


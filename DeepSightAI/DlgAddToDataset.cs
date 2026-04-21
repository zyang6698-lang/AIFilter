using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DeepSightModel;

namespace DeepSightAI
{
    /// <summary>
    /// 添加Lot到一致性测试数据集的对话框
    /// </summary>
    public class DlgAddToDataset : Form
    {
        private ComboBox comboBox_Datasets;
        private TextBox textBox_NewName;
        private TextBox textBox_Description;
        private Label label_Info;
        private Button btn_OK;
        private Button btn_Cancel;
        private RadioButton radio_Existing;
        private RadioButton radio_New;

        private readonly List<ConsistencyTestDataset> _datasets;
        private readonly string _lotNumber;
        private readonly List<DefectReviewItem> _lotItems;

        public ConsistencyTestDataset SelectedDataset { get; private set; }

        public DlgAddToDataset(List<ConsistencyTestDataset> datasets, string lotNumber, List<DefectReviewItem> lotItems)
        {
            _datasets = datasets ?? new List<ConsistencyTestDataset>();
            _lotNumber = lotNumber;
            _lotItems = lotItems;
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "添加到一致性测试数据集";
            this.Size = new Size(480, 380);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.FromArgb(216, 219, 188);
            this.Font = new Font("微软雅黑", 9F);

            int y = 15;

            // Lot信息标签
            label_Info = new Label
            {
                Text = $"Lot: {_lotNumber}  ({_lotItems.Count} 条记录)",
                Location = new Point(15, y),
                Size = new Size(440, 25),
                ForeColor = Color.FromArgb(100, 200, 200)
            };
            this.Controls.Add(label_Info);
            y += 35;

            // 选择已有数据集
            radio_Existing = new RadioButton
            {
                Text = "添加到已有数据集",
                Location = new Point(15, y),
                Size = new Size(200, 25),
                Checked = _datasets.Count > 0,
                Enabled = _datasets.Count > 0,
                ForeColor = Color.FromArgb(216, 219, 188)
            };
            this.Controls.Add(radio_Existing);
            y += 30;

            comboBox_Datasets = new ComboBox
            {
                Location = new Point(30, y),
                Size = new Size(410, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.FromArgb(216, 219, 188),
                Enabled = _datasets.Count > 0
            };
            foreach (var ds in _datasets)
            {
                comboBox_Datasets.Items.Add($"{ds.Name} ({ds.LotEntries.Count} Lots)");
            }
            if (_datasets.Count > 0) comboBox_Datasets.SelectedIndex = 0;
            this.Controls.Add(comboBox_Datasets);
            y += 40;

            // 创建新数据集
            radio_New = new RadioButton
            {
                Text = "创建新数据集",
                Location = new Point(15, y),
                Size = new Size(200, 25),
                Checked = _datasets.Count == 0,
                ForeColor = Color.FromArgb(216, 219, 188)
            };
            this.Controls.Add(radio_New);
            y += 30;

            var label_Name = new Label { Text = "数据集名称:", Location = new Point(30, y), Size = new Size(90, 25) };
            this.Controls.Add(label_Name);
            textBox_NewName = new TextBox
            {
                Location = new Point(125, y),
                Size = new Size(315, 28),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.FromArgb(216, 219, 188),
                Enabled = _datasets.Count == 0
            };
            this.Controls.Add(textBox_NewName);
            y += 35;

            var label_Desc = new Label { Text = "描述:", Location = new Point(30, y), Size = new Size(90, 25) };
            this.Controls.Add(label_Desc);
            textBox_Description = new TextBox
            {
                Location = new Point(125, y),
                Size = new Size(315, 50),
                Multiline = true,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.FromArgb(216, 219, 188),
                Enabled = _datasets.Count == 0
            };
            this.Controls.Add(textBox_Description);
            y += 65;

            // 按钮
            btn_OK = new StyledButton { Text = "确定", Size = new Size(90, 35), Location = new Point(250, y) };
            btn_Cancel = new StyledButton { Text = "取消", Size = new Size(90, 35), Location = new Point(350, y) };
            this.Controls.Add(btn_OK);
            this.Controls.Add(btn_Cancel);

            // 事件
            radio_Existing.CheckedChanged += (s, e) => {
                comboBox_Datasets.Enabled = radio_Existing.Checked;
                textBox_NewName.Enabled = !radio_Existing.Checked;
                textBox_Description.Enabled = !radio_Existing.Checked;
            };

            btn_OK.Click += Btn_OK_Click;
            btn_Cancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.AcceptButton = btn_OK;
            this.CancelButton = btn_Cancel;
        }

        private void Btn_OK_Click(object sender, EventArgs e)
        {
            if (radio_New.Checked)
            {
                if (string.IsNullOrWhiteSpace(textBox_NewName.Text))
                {
                    MessageBox.Show("请输入数据集名称。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SelectedDataset = new ConsistencyTestDataset
                {
                    Name = textBox_NewName.Text.Trim(),
                    Description = textBox_Description.Text.Trim()
                };
            }
            else
            {
                if (comboBox_Datasets.SelectedIndex < 0)
                {
                    MessageBox.Show("请选择一个数据集。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SelectedDataset = _datasets[comboBox_Datasets.SelectedIndex];
            }

            // 检查是否已存在该Lot
            if (SelectedDataset.LotEntries.Any(l => l.LotNumber == _lotNumber))
            {
                var result = MessageBox.Show(
                    $"数据集中已存在 Lot: {_lotNumber}，是否覆盖？",
                    "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) return;
                SelectedDataset.LotEntries.RemoveAll(l => l.LotNumber == _lotNumber);
            }

            // 添加Lot条目
            var entry = new DatasetLotEntry
            {
                LotNumber = _lotNumber,
                RecordCount = _lotItems.Count,
                StartDate = _lotItems.Min(i => i.DetectionDate),
                EndDate = _lotItems.Max(i => i.DetectionDate),
                ProductSerial = _lotItems.FirstOrDefault()?.ProductSerial ?? ""
            };
            SelectedDataset.LotEntries.Add(entry);
            SelectedDataset.UpdateTime = DateTime.Now;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}


using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using DeepSightModel;
using System.Threading.Tasks;
using System.Linq;
using DeepSightTool;
using DeepSightDB;

namespace DeepSightAI
{
    /// <summary>
    /// 图像查询控件
    /// </summary>
    public partial class QueryControl : UserControl
    {
        #region Events

        /// <summary>
        /// 查询按钮点击事件
        /// </summary>
        public event EventHandler QueryClicked;

        /// <summary>
        /// 筛选条件变化事件（料号或机台号选择变化时触发）
        /// </summary>
        public event EventHandler FilterChanged;

        /// <summary>
        /// 是否抑制 FilterChanged 事件（在 QueryDataAsync 更新下拉框时使用）
        /// </summary>
        private bool _suppressFilterChanged = false;

        #endregion

        #region Properties

        /// <summary>
        /// 获取或设置Lot号
        /// </summary>
        public string LotNumber
        {
            get => txt_Lot.Text;
            set => txt_Lot.Text = value;
        }

        /// <summary>
        /// 获取日期选择器是否勾选
        /// </summary>
        public bool IsDateChecked
        {
            get => timePicker.Checked;
            set => timePicker.Checked = value;
        }

        /// <summary>
        /// 获取或设置料号（"全部"返回空字符串表示全选）
        /// </summary>
        public string PartNumber
        {
            get => cmb_PartNumber.Text == "全部" ? "" : cmb_PartNumber.Text;
            set => cmb_PartNumber.Text = value;
        }

        /// <summary>
        /// 获取或设置机台号
        /// </summary>
        public string MachineID
        {
            get => cmb_MachineID.Text == "全部" ? "" : cmb_MachineID.Text;
            set => cmb_MachineID.Text = value;
        }

        /// <summary>
        /// 获取机台号列表
        /// </summary>
        public ComboBox.ObjectCollection MachineIDItems => cmb_MachineID.Items;

        /// <summary>
        /// 获取机台号下拉框
        /// </summary>
        public ComboBox MachineIDComboBox => cmb_MachineID;

        /// <summary>
        /// 获取料号列表
        /// </summary>
        public ComboBox.ObjectCollection PartNumberItems => cmb_PartNumber.Items;

        /// <summary>
        /// 获取料号下拉框
        /// </summary>
        public ComboBox PartNumberComboBox => cmb_PartNumber;

        /// <summary>
        /// 获取或设置缺陷名称筛选（"全部"返回空字符串表示全选）
        /// </summary>
        public string SelectedDefectName
        {
            get => cmb_DefectName.Text == "全部" ? "" : cmb_DefectName.Text;
            set => cmb_DefectName.Text = value;
        }

        /// <summary>
        /// 获取缺陷名称下拉框
        /// </summary>
        public ComboBox DefectNameComboBox => cmb_DefectName;

        /// <summary>
        /// ����ѡ����棨A�����棬B�����棬空字符串为全选）
        /// </summary>
        public string SelectedSide
        {
            get => rbn_Front.Checked ? "A" : (rbn_Back.Checked ? "B" : "");
            set
            {
                if (value == "A")
                {
                    rbn_Front.Checked = true;
                }
                else if (value == "B")
                {
                    rbn_Back.Checked = true;
                }
                else
                {
                    rbn_All.Checked = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置查询结果
        /// </summary>
        private List<PanelDataRecord> QueryResult { get;  set; }

        public List<PanelDataRecord> GetQueryResult()
        {
            if (QueryResult == null)
            {
                return new List<PanelDataRecord>();
            }

            var query = QueryResult.AsQueryable();

            // 首先根据料号进行筛选
            if (!string.IsNullOrEmpty(PartNumber))
            {
                query = query.Where(t => t.ProductSerial == PartNumber);
            }

            // 根据机台号进行筛选
            if (!string.IsNullOrEmpty(MachineID))
            {
                query = query.Where(t => t.MachineId == MachineID);
            }

            // 然后根据选择的面和缺陷名称筛选每个记录的Sides列表
            var selectedSide = this.SelectedSide;
            var selectedDefect = this.SelectedDefectName;
            return query.Select(record => new PanelDataRecord
            {
                Id = record.Id,
                MachineId = record.MachineId,
                DetectionDate = record.DetectionDate,
                SerialNumber = record.SerialNumber,
                LotNumber = record.LotNumber,
                ProductSerial = record.ProductSerial,
                AviCreationTime = record.AviCreationTime,
                Sides = record.Sides
                    // 根据UI选择的面来筛选Sides（空字符串表示全选，不筛选）
                    .Where(s => string.IsNullOrEmpty(selectedSide) || s.Side == selectedSide)
                    // 根据缺陷名称筛选：下沉到DetectPoints级别，只保留匹配的缺陷点
                    .Select(s => string.IsNullOrEmpty(selectedDefect) ? s : new SideData
                    {
                        Side = s.Side,
                        AviState = s.AviState,
                        AiState = s.AiState,
                        VvsState = s.VvsState,
                        VrsState = s.VrsState,
                        FinalState = s.FinalState,
                        TestState = s.TestState,
                        LastTestTime = s.LastTestTime,
                        DetectPoints = s.DetectPoints != null
                            ? s.DetectPoints.Where(dp => dp.DefectName == selectedDefect).ToList()
                            : new List<DetectInfo>()
                    })
                    // 过滤掉没有匹配缺陷点的面
                    .Where(s => string.IsNullOrEmpty(selectedDefect)
                        || (s.DetectPoints != null && s.DetectPoints.Count > 0))
                    .ToList()
            })
            // 过滤掉没有任何匹配面的记录
            .Where(record => record.Sides.Count > 0)
            .ToList();
        }

        #endregion

        #region Constructor


        public QueryControl()
        {
            InitializeComponent();

            // 设置起止日期默认为今天
            timePicker.Value = DateTime.Today;
            timePickerEnd.Value = DateTime.Today;
            timePicker.Checked = true;

            // 绑定料号、机台号、缺陷名称下拉框的选择变化事件
            cmb_PartNumber.SelectedIndexChanged += Cmb_PartNumber_SelectedIndexChanged;
            cmb_MachineID.SelectedIndexChanged += Cmb_MachineID_SelectedIndexChanged;
            cmb_DefectName.SelectedIndexChanged += Cmb_DefectName_SelectedIndexChanged;

            // 绑定正反面选择变化事件
            rbn_Front.CheckedChanged += Rbn_Side_CheckedChanged;
            rbn_Back.CheckedChanged += Rbn_Side_CheckedChanged;
            rbn_All.CheckedChanged += Rbn_Side_CheckedChanged;
        }


        #endregion

        #region Event Handlers

        private async void Btn_queryHeatPoint_Click(object sender, EventArgs e)
        {
            await QueryDataAsync();
            QueryClicked?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 料号下拉框选择变化事件处理
        /// </summary>
        private void Cmb_PartNumber_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 当选中非"全部"选项时，触发筛选变化事件
            if (!_suppressFilterChanged && QueryResult != null && QueryResult.Count > 0)
            {
                FilterChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 机台号下拉框选择变化事件处理
        /// </summary>
        private void Cmb_MachineID_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 当选中非"全部"选项时，触发筛选变化事件
            if (!_suppressFilterChanged && QueryResult != null && QueryResult.Count > 0)
            {
                FilterChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 缺陷名称下拉框选择变化事件处理
        /// </summary>
        private void Cmb_DefectName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_suppressFilterChanged && QueryResult != null && QueryResult.Count > 0)
            {
                FilterChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 正反面选择变化事件处理
        /// </summary>
        private void Rbn_Side_CheckedChanged(object sender, EventArgs e)
        {
            // 只在RadioButton被选中时触发，避免重复触发
            if (sender is RadioButton rbn && rbn.Checked)
            {
                if (!_suppressFilterChanged && QueryResult != null && QueryResult.Count > 0)
                {
                    FilterChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 异步查询数据
        /// </summary>
        public async Task QueryDataAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txt_Lot.Text) && !timePicker.Checked)
                {
                    // 输入验证逻辑，如有必要可调整
                    QueryResult = new List<PanelDataRecord>();
                    return;
                }

                if (!string.IsNullOrWhiteSpace(txt_Lot.Text))
                {
                    // 优先使用 Lot 号查询
                    QueryResult = await Machine.master.GetPanelsDataByMachineAndLot(null, txt_Lot.Text);
                }
                else if (timePicker.Checked)
                {
                    // 使用起止日期范围查询
                    DateTime startDate = timePicker.Value.Date;
                    DateTime endDate = timePickerEnd.Value.Date.AddDays(1).AddTicks(-1);

                    // 验证日期范围
                    if (endDate < startDate)
                    {
                        MessageBox.Show("结束日期不能早于起始日期！");
                        QueryResult = new List<PanelDataRecord>();
                        return;
                    }

                    // 如果已经选择了料号，则直接按日期和料号查询
                    if (!string.IsNullOrEmpty(PartNumber))
                    {
                        QueryResult = await Machine.master.GetPanelsData(startDate, endDate, PartNumber);
                    }
                    // 如果未选择料号，则返回当天料号列表供用户选择
                    else
                    {
                        QueryResult = await Machine.master.GetPanelsData(startDate, endDate);
                        _suppressFilterChanged = true;
                        try
                        {
                            PartNumberItems.Clear();
                            PartNumberItems.Add("全部");
                            // 从查询结果中提取唯一的料号
                            var partNumbers = QueryResult.Select(pn => pn.ProductSerial).Distinct();
                            foreach (var pn in partNumbers)
                            {
                                PartNumberItems.Add(pn);
                            }
                            if (PartNumberItems.Count > 0)
                            {
                                // 默认选择"全部"
                                PartNumberComboBox.SelectedIndex = 0;
                            }
                        }
                        finally
                        {
                            _suppressFilterChanged = false;
                        }
                    }
                }
                else
                {
                    QueryResult = new List<PanelDataRecord>();
                }

                // 填充机台号和缺陷名称列表（抑制 FilterChanged 事件，避免重复刷新）
                _suppressFilterChanged = true;
                try
                {
                    UpdateMachineIDList();
                    UpdateDefectNameList();
                }
                finally
                {
                    _suppressFilterChanged = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("请输入Lot号，或选择日期并选择一个料号。");
                LogTextHelper.Error($"查询数据时发生异常：{ex.Message}");
                QueryResult = new List<PanelDataRecord>();
            }
        }

        /// <summary>
        /// 更新机台号下拉列表
        /// </summary>
        private void UpdateMachineIDList()
        {
            MachineIDItems.Clear();
            MachineIDItems.Add("全部");

            if (QueryResult != null && QueryResult.Count > 0)
            {
                // 从查询结果中提取唯一的机台号
                var machineIds = QueryResult
                    .Where(r => !string.IsNullOrEmpty(r.MachineId))
                    .Select(r => r.MachineId)
                    .Distinct()
                    .OrderBy(m => m);

                foreach (var machineId in machineIds)
                {
                    MachineIDItems.Add(machineId);
                }
            }

            // 默认选择"全部"
            MachineIDComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// 更新缺陷名称下拉列表
        /// </summary>
        private void UpdateDefectNameList()
        {
            cmb_DefectName.Items.Clear();
            cmb_DefectName.Items.Add("全部");

            if (QueryResult != null && QueryResult.Count > 0)
            {
                // 从查询结果中提取所有唯一的缺陷名称
                var defectNames = QueryResult
                    .Where(r => r.Sides != null)
                    .SelectMany(r => r.Sides)
                    .Where(s => s.DetectPoints != null)
                    .SelectMany(s => s.DetectPoints)
                    .Where(dp => !string.IsNullOrEmpty(dp.DefectName))
                    .Select(dp => dp.DefectName)
                    .Distinct()
                    .OrderBy(n => n);

                foreach (var defectName in defectNames)
                {
                    cmb_DefectName.Items.Add(defectName);
                }
            }

            // 默认选择"全部"
            cmb_DefectName.SelectedIndex = 0;
        }

        /// <summary>
        /// 清空输入内容
        /// </summary>
        public void ClearInputs()
        {
            txt_Lot.Clear();
            cmb_MachineID.Items.Clear();
            cmb_MachineID.Text = string.Empty;
            cmb_PartNumber.Items.Clear();
            cmb_PartNumber.Text = string.Empty;
            cmb_DefectName.Items.Clear();
            cmb_DefectName.Text = string.Empty;
            timePicker.Checked = false;
            timePickerEnd.Value = DateTime.Now;
            timePicker.Value = DateTime.Now;
            rbn_Front.Checked = true;
        }

        /// <summary>
        /// 验证输入
        /// </summary>
        public bool ValidateInputs(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(txt_Lot.Text) && !timePicker.Checked)
            {
                errorMessage = "请输入Lot号，或选择日期并选择一个料号。";
                return false;
            }

            if (timePicker.Checked && string.IsNullOrEmpty(cmb_PartNumber.Text))
            {
                errorMessage = "请选择至少一个料号。";
                return false;
            }

            if (timePicker.Checked && timePickerEnd.Value.Date < timePicker.Value.Date)
            {
                errorMessage = "结束日期不能早于起始日期！";
                return false;
            }

            return true;
        }

        #endregion
    }
}

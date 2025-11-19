using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using DeepSightModel;
using DeepsightSqlite;
using System.Threading.Tasks;
using System.Linq;
using DeepSightTool;

namespace DeepSightAI
{
    /// <summary>
    /// 热力图查询条件控件
    /// </summary>
    public partial class HeatMapQueryControl : UserControl
    {
        #region Events

        /// <summary>
        /// 查询按钮点击事件
        /// </summary>
        public event EventHandler QueryClicked;

        /// <summary>
        /// 正反面选择改变事件
        /// </summary>
        public event EventHandler SideSelectionChanged;

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
        /// 获取或设置选择的日期
        /// </summary>
        public DateTime SelectedDate
        {
            get => timePicker.Value;
            set => timePicker.Value = value;
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
        /// 获取或设置料号
        /// </summary>
        public string PartNumber
        {
            get => cmb_PartNumber.Text;
            set => cmb_PartNumber.Text = value;
        }

        /// <summary>
        /// 获取料号列表
        /// </summary>
        public ComboBox.ObjectCollection PartNumberItems => cmb_PartNumber.Items;

        /// <summary>
        /// 获取料号ComboBox
        /// </summary>
        public ComboBox PartNumberComboBox => cmb_PartNumber;

        /// <summary>
        /// 获取是否选择正面
        /// </summary>
        public bool IsFrontSideSelected => rbn_Front.Checked;

        /// <summary>
        /// 获取是否选择反面
        /// </summary>
        public bool IsBackSideSelected => rbn_Back.Checked;

        /// <summary>
        /// 设置选择的面（A：正面，B：反面）
        /// </summary>
        public string SelectedSide
        {
            get => rbn_Front.Checked ? "A" : "B";
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
            }
        }

        /// <summary>
        /// 获取或设置查询结果
        /// </summary>
        private List<PanelDataRecord> QueryResult { get;  set; }

        public List<PanelDataRecord> GetQueryResult()
        {
            List<PanelDataRecord> filteredByPartNumber = QueryResult;

            // 首先根据料号进行筛选
            if (!string.IsNullOrEmpty(PartNumber))
            {
                filteredByPartNumber = QueryResult.Where(t => t.ProductSerial == PartNumber).ToList();
            }

            if (filteredByPartNumber == null)
            {
                return new List<PanelDataRecord>();
            }

            // 然后根据选择的面筛选每个记录的Sides列表
            var finalResult = new List<PanelDataRecord>();
            foreach (var record in filteredByPartNumber)
            {
                var newRecord = new PanelDataRecord
                {
                    Id = record.Id,
                    MachineId = record.MachineId,
                    DetectionDate = record.DetectionDate,
                    SerialNumber = record.SerialNumber,
                    LotNumber = record.LotNumber,
                    ProductSerial = record.ProductSerial,
                    IsAIOk = record.IsAIOk,
                    PathIndex = record.PathIndex,
                    AviCreationTime = record.AviCreationTime,
                    // 根据UI选择的面来筛选Sides
                    Sides = record.Sides.Where(s => s.Side == this.SelectedSide).ToList()
                };
                finalResult.Add(newRecord);
            }

            return finalResult;
        }

        #endregion

        #region Constructor


        public HeatMapQueryControl()
        {
            InitializeComponent();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            btn_queryHeatPoint.Click += Btn_queryHeatPoint_Click;
            rbn_Front.CheckedChanged += Rbn_Front_CheckedChanged;
            rbn_Back.CheckedChanged += Rbn_Back_CheckedChanged;
        }

        #endregion

        #region Event Handlers

        private async void Btn_queryHeatPoint_Click(object sender, EventArgs e)
        {
            await QueryDataAsync();
            QueryClicked?.Invoke(this, EventArgs.Empty);
        }

        private void Rbn_Front_CheckedChanged(object sender, EventArgs e)
        {
            if (rbn_Front.Checked)
            {
                SideSelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Rbn_Back_CheckedChanged(object sender, EventArgs e)
        {
            if (rbn_Back.Checked)
            {
                SideSelectionChanged?.Invoke(this, EventArgs.Empty);
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
                    // 根据验证逻辑，两者至少要有一个
                    QueryResult = new List<PanelDataRecord>();
                    return;
                }

                if (!string.IsNullOrWhiteSpace(txt_Lot.Text))
                {
                    // 优先使用 Lot 号查询
                    QueryResult = await Machine.master.workClass.GetPanelsDataByMachineAndLot(null, txt_Lot.Text);
                }

                else if (timePicker.Checked)
                {
                    if (PartNumberItems.Count == 0)
                    {
                        // 使用日期查询
                        DateTime selectedDate = timePicker.Value.Date;
                        DateTime startDate = selectedDate;
                        DateTime endDate = selectedDate.AddDays(1).AddTicks(-1);
                        QueryResult = await Machine.master.workClass.GetPanelsData(startDate, endDate);
                        PartNumberItems.Clear();
                        foreach (var pn in QueryResult)
                        {
                            PartNumberItems.Add(pn.ProductSerial);
                        }
                        if (PartNumberItems.Count > 0)
                        {
                            PartNumberComboBox.SelectedIndex = 0;
                        }
                        MessageBox.Show($"已加载当天料号列表，请选择或输入一个料号后再次查询。");
                    }
                }
                else
                {
                    QueryResult = new List<PanelDataRecord>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("请输入Lot号，或勾选日期并选择一个料号。");
                LogTextHelper.Error($"查询数据时发生异常：{ex.Message}");
                QueryResult = new List<PanelDataRecord>();
            }
        }

        /// <summary>
        /// 清空所有输入
        /// </summary>
        public void ClearInputs()
        {
            txt_Lot.Clear();
            cmb_PartNumber.Items.Clear();
            cmb_PartNumber.Text = string.Empty;
            timePicker.Checked = false;
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
                errorMessage = "请输入Lot号，或勾选日期并选择一个料号。";
                return false;
            }

            if (timePicker.Checked && string.IsNullOrEmpty(cmb_PartNumber.Text))
            {
                errorMessage = "请先选择或输入一个料号。";
                return false;
            }

            return true;
        }

        #endregion
    }
}

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
        /// 获取或设置起始日期
        /// </summary>
        public DateTime StartDate
        {
            get => timePicker.Value;
            set => timePicker.Value = value;
        }

        /// <summary>
        /// 获取或设置结束日期
        /// </summary>
        public DateTime EndDate
        {
            get => timePickerEnd.Value;
            set => timePickerEnd.Value = value;
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
        /// 获取料号下拉框
        /// </summary>
        public ComboBox PartNumberComboBox => cmb_PartNumber;


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

            // 然后根据选择的面筛选每个记录的Sides列表
            var selectedSide = this.SelectedSide;
            return query.Select(record => new PanelDataRecord
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
                // 根据UI选择的面来筛选Sides（空字符串表示全选，不筛选）
                Sides = string.IsNullOrEmpty(selectedSide)
                    ? record.Sides.ToList()
                    : record.Sides.Where(s => s.Side == selectedSide).ToList()
            }).ToList();
        }

        #endregion

        #region Constructor


        public QueryControl()
        {
            InitializeComponent();
        }


        #endregion

        #region Event Handlers

        private async void Btn_queryHeatPoint_Click(object sender, EventArgs e)
        {
            await QueryDataAsync();
            QueryClicked?.Invoke(this, EventArgs.Empty);
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
                    QueryResult = await Machine.master.workClass.GetPanelsDataByMachineAndLot(null, txt_Lot.Text);
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
                        QueryResult = await Machine.master.workClass.GetPanelsData(startDate, endDate, PartNumber);
                    }
                    // 如果未选择料号，则返回当天料号列表供用户选择
                    else
                    {
                        QueryResult = await Machine.master.workClass.GetPanelsData(startDate, endDate);
                        PartNumberItems.Clear();
                        // 从查询结果中提取唯一的料号
                        var partNumbers = QueryResult.Select(pn => pn.ProductSerial).Distinct();
                        foreach (var pn in partNumbers)
                        {
                            PartNumberItems.Add(pn);
                        }
                        if (PartNumberItems.Count > 0)
                        {
                            PartNumberComboBox.SelectedIndex = 0;
                            MessageBox.Show($"已加载当天料号列表，请选择至少一个料号后再次查询！");
                        }
                    }
                }
                else
                {
                    QueryResult = new List<PanelDataRecord>();
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
        /// 清空输入内容
        /// </summary>
        public void ClearInputs()
        {
            txt_Lot.Clear();
            cmb_PartNumber.Items.Clear();
            cmb_PartNumber.Text = string.Empty;
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

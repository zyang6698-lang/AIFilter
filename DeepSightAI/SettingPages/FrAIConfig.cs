using DeepSightTool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DeepSightModel;
using System.Collections.Generic;
using Sunny.UI;

namespace DeepSightAI.SettingPages
{
    /// <summary>
    /// 基础参数
    /// </summary>
    public partial class FrAIConfig : Form
    {
        public FrAIConfig()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
            dataPost.CellValueChanged -= dataPost_CellValueChanged;
            GetSolutionFlow();
            dataPost.CellValueChanged += dataPost_CellValueChanged;

        }
        private Dictionary<string, List<string>> dic_solutionAndFlow = new Dictionary<string, List<string>>();
        /// <summary>
        /// 窗体实例对象
        /// </summary>
        private static FrAIConfig _instance;
        public static FrAIConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrAIConfig();
                }

                return _instance;
            }
        }
        /// <summary>
        /// 获取方案流程
        /// </summary>
        /// <param name="isInface">是否从接口获取 true:接口获取 false：文件获取</param>
        public void GetSolutionFlow(bool isInface = false)
        {
            dataPost.Rows.Clear();
            var json = JsonConvert.SerializeObject(new
            {
                message_type = "visionbuilder_solution_flow_list"
            });
            //JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };//去掉空值NULL
            //string res = JsonConvert.SerializeObject(json, Formatting.None, jsonSetting);
            IntPtr input = Marshal.StringToHGlobalAnsi(json);//JsonConvert.SerializeObject(json, Formatting.None, jsonSetting));
            Machine.master.workClass.defect.ai_Defect.Vision_runMethod(input, out IntPtr intPtr);
            string solutionandflow_List = Marshal.PtrToStringAnsi(intPtr);

            var jsonObj = JObject.Parse(solutionandflow_List);
            var solutions = jsonObj["solution_flow_list"]
                .Select(item => new
                {
                    Solution = item["solution"].ToString(),
                    FlowList = item["flow_list"].Select(flow => flow.ToString()).ToList()
                });
            //if (isInface)
            {
                dic_solutionAndFlow.Clear();
                foreach (var sol in solutions)
                {
                    //如果是空flow
                    if (sol.FlowList.Count == 0)
                    {
                        dataPost.Rows.Add("A", sol.Solution, "(空流程)", false);
                    }

                    if (!dic_solutionAndFlow.ContainsKey(sol.Solution))
                    {
                        dic_solutionAndFlow.Add(sol.Solution, sol.FlowList);
                    }

                    //foreach (var flow in sol.FlowList)
                    //{
                    //    SolutionAndFlow item = new SolutionAndFlow()
                    //    {
                    //        ProductSerial= "A123",
                    //        //ASide= "A",
                    //        Asolution = sol.Solution,
                    //        Aflow = flow,
                    //        Bsolution = sol.Solution,
                    //        Bflow = flow,
                    //    };
                    //    //这个item里面有个默认的false

                    //    if (!Machine.solconfig.solus.Contains(item))
                    //    //if (!Machine.solconfig.solus.Exists(O=>O.solution==item.solution))
                    //    {
                    //        dataPost.Rows.Add("A123", sol.Solution, flow, sol.Solution, flow, false);
                    //    }
                    //}
                }
                ((DataGridViewComboBoxColumn)dataPost.Columns["A_solution"]).DataSource = dic_solutionAndFlow.Keys.ToList();
                ((DataGridViewComboBoxColumn)dataPost.Columns["A_flow"]).DataSource = dic_solutionAndFlow.Values.SelectMany(list => list).Distinct().ToList();
                ((DataGridViewComboBoxColumn)dataPost.Columns["B_solution"]).DataSource = dic_solutionAndFlow.Keys.ToList();
                ((DataGridViewComboBoxColumn)dataPost.Columns["B_flow"]).DataSource = dic_solutionAndFlow.Values.SelectMany(list => list).Distinct().ToList();
            }
            //默认文件获取
            InitMethod();
        }
        /// <summary>
        /// 绑定数据
        /// </summary>
        private void InitMethod()
        {
            try
            {
                if (Machine.solconfig != null)
                {
                    for (int i = 0; i < Machine.solconfig.solus.Count; i++)
                    {
                        dataPost.Rows.Add();
                        dataPost.Rows[i].Cells[0].Value = Machine.solconfig.solus[i].ProductSerial;
                        dataPost.Rows[i].Cells[1].Value = Machine.solconfig.solus[i].Asolution;
                        dataPost.Rows[i].Cells[2].Value = Machine.solconfig.solus[i].Aflow;
                        dataPost.Rows[i].Cells[3].Value = Machine.solconfig.solus[i].Bsolution;
                        dataPost.Rows[i].Cells[4].Value = Machine.solconfig.solus[i].Bflow;
                        dataPost.Rows[i].Cells[5].Value = Machine.solconfig.solus[i].IsSwitch;


                        // 获取设计器创建的列
                        //var sideColumn1 = (DataGridViewComboBoxColumn)dataPost.Columns["A_solution"];
                        //var sideColumn2 = (DataGridViewComboBoxColumn)dataPost.Columns["A_flow"];
                        //var sideColumn3 = (DataGridViewComboBoxColumn)dataPost.Columns["B_solution"];
                        //var sideColumn4 = (DataGridViewComboBoxColumn)dataPost.Columns["B_flow"];

                        //// 修改数据源
                        //sideColumn1.DataSource = new List<string> { "X", "Y", "Z" };
                        //sideColumn2.DataSource = new List<string> { "X", "Y", "Z" };
                        //sideColumn3.DataSource = new List<string> { "X", "Y", "Z" };
                        //sideColumn4.DataSource = new List<string> { "X", "Y", "Z" };

                        //// 刷新显示
                        //dataPost.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常" + ex.ToString());
            }
        }

        private void btn_GetAgain_Click(object sender, EventArgs e)
        {
            GetSolutionFlow(true);
        }
        string solutionName = string.Empty;
        string flowName = string.Empty;
        string productSerial = string.Empty;
        bool isSCH = false;

        private void dataPost_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            //判断是否是第二列
            if (e.ColumnIndex == 2 && dataPost.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
            {
                var cell = dataPost.Rows[e.RowIndex].Cells[e.ColumnIndex];
                cell.Value = !(cell.Value is bool isChecked && isChecked);
                dataPost.CommitEdit(DataGridViewDataErrorContexts.Commit);
                isSCH = Convert.ToBoolean(cell.Value);
            }
            int rowIndex = e.RowIndex;
            //int columnIndex = e.ColumnIndex;
            productSerial = dataPost.Rows[rowIndex].Cells[0].Value?.ToString() ?? "空值";
            solutionName = dataPost.Rows[rowIndex].Cells[1].Value?.ToString() ?? "空值";
            flowName = dataPost.Rows[rowIndex].Cells[2].Value?.ToString() ?? "空值";

            //isSCH = Convert.ToBoolean(dataPost.Rows[rowIndex].Cells[2].Value);
            // isSCH = false;
            this.lbl_solution.Text = solutionName;
            this.lbl_flow.Text = flowName;
            this.lbl_ProductSerial.Text = productSerial;
            this.lbl_Bsolution.Text = dataPost.Rows[rowIndex].Cells[3].Value?.ToString() ?? "空值"; ;
            this.lbl_Bflow.Text = dataPost.Rows[rowIndex].Cells[4].Value?.ToString() ?? "空值"; ;
        }

        private void btn_setSolution_Click(object sender, EventArgs e)
        {
            Machine.master.workClass.solution = Machine.solution = solutionName;
            Machine.master.workClass.flow = Machine.flow = flowName;
            Machine.master.workClass.isSwitch = Machine.isSwitch = isSCH;
            Machine.master.workClass.ProductSerial = Machine.productSerial = productSerial;
            FrmMain.Instance.solutionAndflow.Text = $"当前方案:{solutionName}_当前流程:{flowName}_当前Switch:{isSCH}";
            btn_SavePam_Click(null, null);
            MessageBox.Show("方案及流程设置成功", "设置成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_SavePam_Click(object sender, EventArgs e)
        {
            try
            {
                SolutionConfig solConfig = new SolutionConfig()
                {
                    solus = new List<SolutionAndFlow>(),
                };
                for (int i = 0; i < dataPost.Rows.Count; i++)
                {
                    SolutionAndFlow item = new SolutionAndFlow();
                    if (dataPost.Rows[i].Cells[0].Value != null)
                    {
                        item.ProductSerial = dataPost.Rows[i].Cells[0].Value.ToString();
                        //item.Side= dataPost.Rows[i].Cells[1].Value.ToString();
                        item.Asolution = dataPost.Rows[i].Cells[1].Value.ToString();
                        item.Aflow = dataPost.Rows[i].Cells[2].Value.ToString();
                        item.Bsolution = dataPost.Rows[i].Cells[3].Value.ToString();
                        item.Bflow = dataPost.Rows[i].Cells[4].Value.ToString();
                        item.IsSwitch = Convert.ToBoolean(dataPost.Rows[i].Cells[5].Value);
                    }
                    solConfig.solus.Add(item);
                }
                solConfig.CurrentProductSerial = this.lbl_ProductSerial.Text.ToString();
                solConfig.CurrentSolution = this.lbl_solution.Text.ToString();
                solConfig.CurrentFlow = this.lbl_flow.Text.ToString();
                solConfig.CurrentisSwitch = isSCH;
                Machine.solconfig = solConfig;
                //保存到文件夹
                if (Machine.sol_class.Save(solConfig))
                {
                    MessageBox.Show("方案及流程配置保存成功", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("方案及流程配置保存失败", "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                dataPost.Refresh();
                Machine.master.workClass.solconfig = Machine.solconfig;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常" + ex.ToString());
            }
        }
        private void btn_Add_Click(object sender, EventArgs e)
        {
            int index = this.dataPost.Rows.Add();
            dataPost.Rows[index].Cells[0].Value = "A";
            dataPost.Rows[index].Cells[1].Value = dic_solutionAndFlow.FirstOrDefault().Key;
            dataPost.Rows[index].Cells[2].Value = dic_solutionAndFlow.FirstOrDefault().Value[0];
            dataPost.Rows[index].Cells[3].Value = dic_solutionAndFlow.FirstOrDefault().Key;
            dataPost.Rows[index].Cells[4].Value = dic_solutionAndFlow.FirstOrDefault().Value[0];
            dataPost.Rows[index].Cells[5].Value = false;
            dataPost.Refresh();
        }

        private void btn_Delete_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show(" 你 真 的 要 删 了 我 吗？\r\n", "删除提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (res == DialogResult.OK)
            {
                if (dataPost.Rows.Count >= 1)
                {
                    dataPost.Rows.Remove(dataPost.SelectedRows[0]);
                    dataPost.Refresh(); //刷新显示
                }
            }
        }

        private void dataPost_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataPost.Columns["A_solution"].Index  && e.RowIndex >= 0)
            {
                string selectedValue = dataPost.Rows[e.RowIndex].Cells["A_solution"].Value?.ToString();
                if (dic_solutionAndFlow.TryGetValue(selectedValue, out List<string> flowList))
                {
                    //(DataGridViewComboBoxColumn)dataPost.Columns["A_flow"]). DataSource = flowList;
                    ((DataGridViewComboBoxCell)dataPost.Rows[e.RowIndex].Cells["A_flow"]).DataSource = flowList;
                    ((DataGridViewComboBoxCell)dataPost.Rows[e.RowIndex].Cells["A_flow"]).Value = flowList[0];
                }
                else
                {
                    MessageBox.Show("所选方案不存在");
                    return;
                }
            }
            if (e.ColumnIndex == dataPost.Columns["B_solution"].Index&& e.RowIndex >= 0)
            {
                // 获取选择的值将flow数据重新绑定
                string selectedValue = dataPost.Rows[e.RowIndex].Cells["B_solution"].Value?.ToString();
                if (dic_solutionAndFlow.TryGetValue(selectedValue, out List<string> flowList))
                {
                    //((DataGridViewComboBoxColumn)dataPost.Columns["B_flow"]).DataSource = flowList;
                    ((DataGridViewComboBoxCell)dataPost.Rows[e.RowIndex].Cells["B_flow"]).DataSource = flowList;
                    ((DataGridViewComboBoxCell)dataPost.Rows[e.RowIndex].Cells["B_flow"]).Value = flowList[0];
                }
                else
                {
                    MessageBox.Show("所选方案不存在");
                    return;
                }

            }
        }
    }
}
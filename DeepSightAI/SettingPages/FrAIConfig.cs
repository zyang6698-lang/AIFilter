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
        // 可下拉
        public enum PicOptMode
        {
            by_machine = 0,
            copy =1,
            cut=2,
        }
        DeepSight_ProductMode_class ProductModeConfig ;

        public FrAIConfig()
        {
            InitializeComponent();
            InitParNumberConfig();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
            dataPost.CellValueChanged -= dataPost_CellValueChanged;
            GetSolutionFlow();
            dataPost.CellValueChanged += dataPost_CellValueChanged;
            dataPost.RowsAdded += (s, e) => UpdateRowIndices();
            dataPost.RowsRemoved += (s, e) => UpdateRowIndices();
        }
        private void UpdateRowIndices()
        {
            for (int i = 0; i < dataPost.Rows.Count; i++)
            {
                if (dataPost.Rows[i].IsNewRow) continue;
                dataPost.Rows[i].Cells["Index"].Value = i + 1;
            }
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
            try
            {
                var json = JsonConvert.SerializeObject(new
                {
                    message_type = "visionbuilder_solution_flow_list"
                });
                IntPtr input = Marshal.StringToHGlobalAnsi(json);
                Machine.master.workClass.defect.ai_Defect.Vision_runMethod(input, out IntPtr intPtr);
                string solutionandflow_List = Marshal.PtrToStringAnsi(intPtr);

                if (string.IsNullOrEmpty(solutionandflow_List))
                {
                    throw new Exception("获取的方案流程列表为空。");
                }

                var jsonObj = JObject.Parse(solutionandflow_List);
                var solutions = jsonObj["solution_flow_list"]
                    .Select(item => new
                    {
                        Solution = item["solution"].ToString(),
                        FlowList = item["flow_list"].Select(flow => flow.ToString()).ToList()
                    });
                
                dic_solutionAndFlow.Clear();
                foreach (var sol in solutions)
                {
                    if (sol.FlowList.Count == 0)
                    {
                        dataPost.Rows.Add(
                            dataPost.Rows.Count, // Index placeholder
                            "A",                 // liaohao
                            sol.Solution,        // A_solution
                            "(空流程)",           // A_flow
                            sol.Solution,        // B_solution(可按需求决定是否同 A)
                            "(空流程)",           // B_flow
                            false,               // isSwitch
                            PicOptMode.by_machine.ToString() // Mode
                        );
                    }

                    if (!dic_solutionAndFlow.ContainsKey(sol.Solution))
                    {
                        dic_solutionAndFlow.Add(sol.Solution, sol.FlowList);
                    }
                }
                ((DataGridViewComboBoxColumn)dataPost.Columns["A_solution"]).DataSource = dic_solutionAndFlow.Keys.ToList();
                ((DataGridViewComboBoxColumn)dataPost.Columns["A_flow"]).DataSource = dic_solutionAndFlow.Values.SelectMany(list => list).Distinct().ToList();
                ((DataGridViewComboBoxColumn)dataPost.Columns["B_solution"]).DataSource = dic_solutionAndFlow.Keys.ToList();
                ((DataGridViewComboBoxColumn)dataPost.Columns["B_flow"]).DataSource = dic_solutionAndFlow.Values.SelectMany(list => list).Distinct().ToList();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("获取方案流程失败: " + ex.ToString());
                dic_solutionAndFlow.Clear();
                // 添加默认空行
                dataPost.Rows.Add(
                    1, // Index placeholder
                    "Default",
                    "DefaultSolution",
                    "(空流程)",
                    "DefaultSolution",
                    "(空流程)",
                    false,
                    PicOptMode.by_machine.ToString()
                );
                // 更新ComboBox数据源
                var defaultKeys = new List<string> { "DefaultSolution" };
                var defaultValues = new List<string> { "(空流程)" };
                ((DataGridViewComboBoxColumn)dataPost.Columns["A_solution"]).DataSource = defaultKeys;
                ((DataGridViewComboBoxColumn)dataPost.Columns["A_flow"]).DataSource = defaultValues;
                ((DataGridViewComboBoxColumn)dataPost.Columns["B_solution"]).DataSource = defaultKeys;
                ((DataGridViewComboBoxColumn)dataPost.Columns["B_flow"]).DataSource = defaultValues;
            }
            finally
            {
                //默认文件获取
                InitMethod();
                UpdateRowIndices();
            }
        }

        private void InitParNumberConfig()
        {
            ProductModeConfig = new DeepSight_ProductMode_class();

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
                    // 获取 ComboBox 列的数据源以供后续验证
                    var aSolutionItems = ((DataGridViewComboBoxColumn)dataPost.Columns["A_solution"]).DataSource as List<string> ?? new List<string>();
                    var bSolutionItems = ((DataGridViewComboBoxColumn)dataPost.Columns["B_solution"]).DataSource as List<string> ?? new List<string>();
                    // 注意：Flow 的数据源是动态的，这里先获取全局列表
                    var allFlowItems = ((DataGridViewComboBoxColumn)dataPost.Columns["A_flow"]).DataSource as List<string> ?? new List<string>();

                    for (int i = 0; i < Machine.solconfig.solus.Count; i++)
                    {
                        dataPost.Rows.Add();
                        var row = dataPost.Rows[i];
                        var savedSolu = Machine.solconfig.solus[i];

                        row.Cells["Index"].Value = i + 1;
                        row.Cells[1].Value = savedSolu.ProductSerial;

                        // --- START: 修改部分 ---

                        // 验证并设置 A_solution
                        if (aSolutionItems.Contains(savedSolu.Asolution))
                        {
                            row.Cells["A_solution"].Value = savedSolu.Asolution;
                        }
                        else if (aSolutionItems.Count > 0)
                        {
                            row.Cells["A_solution"].Value = aSolutionItems[0]; // 使用第一个可用的方案作为默认值
                        }

                        // 验证并设置 A_flow
                        if (dic_solutionAndFlow.TryGetValue(row.Cells["A_solution"].Value.ToString(), out var aFlowList) && aFlowList.Contains(savedSolu.Aflow))
                        {
                            ((DataGridViewComboBoxCell)row.Cells["A_flow"]).DataSource = aFlowList;
                            row.Cells["A_flow"].Value = savedSolu.Aflow;
                        }
                        else if (aFlowList != null && aFlowList.Count > 0)
                        {
                            ((DataGridViewComboBoxCell)row.Cells["A_flow"]).DataSource = aFlowList;
                            row.Cells["A_flow"].Value = aFlowList[0]; // 使用方案下的第一个流程
                        }

                        // 验证并设置 B_solution
                        if (bSolutionItems.Contains(savedSolu.Bsolution))
                        {
                            row.Cells["B_solution"].Value = savedSolu.Bsolution;
                        }
                        else if (bSolutionItems.Count > 0)
                        {
                            row.Cells["B_solution"].Value = bSolutionItems[0]; // 使用第一个可用的方案作为默认值
                        }

                        // 验证并设置 B_flow
                        if (dic_solutionAndFlow.TryGetValue(row.Cells["B_solution"].Value.ToString(), out var bFlowList) && bFlowList.Contains(savedSolu.Bflow))
                        {
                            ((DataGridViewComboBoxCell)row.Cells["B_flow"]).DataSource = bFlowList;
                            row.Cells["B_flow"].Value = savedSolu.Bflow;
                        }
                        else if (bFlowList != null && bFlowList.Count > 0)
                        {
                            ((DataGridViewComboBoxCell)row.Cells["B_flow"]).DataSource = bFlowList;
                            row.Cells["B_flow"].Value = bFlowList[0]; // 使用方案下的第一个流程
                        }

                        // --- END: 修改部分 ---

                        row.Cells[6].Value = savedSolu.IsSwitch;
                        if (dataPost.Columns.Contains("Mode"))
                        {
                            // 默认值
                            row.Cells["Mode"].Value = PicOptMode.by_machine.ToString();
                        }
                    }
                }
                // 从配置文件加载Mode
                LoadProductModes();
                UpdateRowIndices();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常" + ex.ToString());
            }
        }

        private void LoadProductModes()
        {
            if (ProductModeConfig.Read(out ProductModeConfig config))
            {
                foreach (DataGridViewRow row in dataPost.Rows)
                {
                    if (row.IsNewRow || row.Cells[1].Value == null) continue;

                    string materialCode = row.Cells[1].Value.ToString();
                    var productItem = ProductModeConfig.GetProduct(materialCode, config);

                    if (productItem != null && !string.IsNullOrEmpty(productItem.CopyCutMode))
                    {
                        // 检查值是否在ComboBox的项中
                        var comboBoxCell = row.Cells["Mode"] as DataGridViewComboBoxCell;
                        if (comboBoxCell != null)
                        {
                            if (comboBoxCell.Items.Contains(productItem.CopyCutMode))
                            {
                                row.Cells["Mode"].Value = productItem.CopyCutMode;
                            }
                            else
                            {
                                // 如果需要，可以记录一个警告，说明该值无效
                            }
                        }
                    }
                }
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
            if (e.ColumnIndex == 3 && dataPost.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
            {
                var cell = dataPost.Rows[e.RowIndex].Cells[e.ColumnIndex];
                cell.Value = !(cell.Value is bool isChecked && isChecked);
                dataPost.CommitEdit(DataGridViewDataErrorContexts.Commit);
                isSCH = Convert.ToBoolean(cell.Value);
            }
            int rowIndex = e.RowIndex;
            //int columnIndex = e.ColumnIndex;
            productSerial = dataPost.Rows[rowIndex].Cells[1].Value?.ToString() ?? "空值";
            solutionName = dataPost.Rows[rowIndex].Cells[2].Value?.ToString() ?? "空值";
            flowName = dataPost.Rows[rowIndex].Cells[3].Value?.ToString() ?? "空值";

            //isSCH = Convert.ToBoolean(dataPost.Rows[rowIndex].Cells[2].Value);
            // isSCH = false;
            this.lbl_solution.Text = solutionName;
            this.lbl_flow.Text = flowName;
            this.lbl_ProductSerial.Text = productSerial;
            this.lbl_Bsolution.Text = dataPost.Rows[rowIndex].Cells[4].Value?.ToString() ?? "空值"; ;
            this.lbl_Bflow.Text = dataPost.Rows[rowIndex].Cells[5].Value?.ToString() ?? "空值"; ;
        }

        private void btn_setSolution_Click(object sender, EventArgs e)
        {
            Machine.master.workClass.Solution = Machine.solution = solutionName;
            Machine.master.workClass.Flow = Machine.flow = flowName;
            Machine.master.workClass.IsSwitch = Machine.isSwitch = isSCH;
            Machine.master.workClass.ProductSerial = Machine.productSerial = productSerial;
            FrmMain.Instance.solutionAndflow.Text = $"当前方案:{solutionName}_当前流程:{flowName}_当前Switch:{isSCH}";
            SaveParam();
            MessageBox.Show("方案及流程设置成功", "设置成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public bool SaveParam()
        {
            bool result = true;
            try
            {
                // 保存 SolutionConfig
                SolutionConfig solConfig = new SolutionConfig()
                {
                    solus = new List<SolutionAndFlow>(),
                };
                for (int i = 0; i < dataPost.Rows.Count; i++)
                {
                    if (dataPost.Rows[i].IsNewRow) continue;
                    SolutionAndFlow item = new SolutionAndFlow();
                    if (dataPost.Rows[i].Cells[1].Value != null)
                    {
                        item.ProductSerial = dataPost.Rows[i].Cells[1].Value.ToString();
                        item.Asolution = dataPost.Rows[i].Cells[2].Value.ToString();
                        item.Aflow = dataPost.Rows[i].Cells[3].Value.ToString();
                        item.Bsolution = dataPost.Rows[i].Cells[4].Value.ToString();
                        item.Bflow = dataPost.Rows[i].Cells[5].Value.ToString();
                        item.IsSwitch = Convert.ToBoolean(dataPost.Rows[i].Cells[6].Value);
                    }
                    solConfig.solus.Add(item);
                }
                solConfig.CurrentProductSerial = this.lbl_ProductSerial.Text.ToString();
                solConfig.CurrentSolution = this.lbl_solution.Text.ToString();
                solConfig.CurrentFlow = this.lbl_flow.Text.ToString();
                solConfig.CurrentisSwitch = isSCH;
                Machine.solconfig = solConfig;
                result=Machine.sol_class.Save(solConfig);


                // 保存 ProductModeConfig
                try
                {
                    ProductModeConfig newProductConfig = new ProductModeConfig { Products = new List<ProductModeItem>() };
                    for (int i = 0; i < dataPost.Rows.Count; i++)
                    {
                        if (dataPost.Rows[i].IsNewRow || dataPost.Rows[i].Cells[1].Value == null) continue;

                        var productSerial = dataPost.Rows[i].Cells[1].Value.ToString();
                        var modeValue = dataPost.Columns.Contains("Mode") ? dataPost.Rows[i].Cells["Mode"].Value?.ToString() : PicOptMode.by_machine.ToString();

                        newProductConfig.Products.Add(new ProductModeItem
                        {
                            Name = productSerial,
                            CopyCutMode = modeValue
                        });
                    }

                    if (ProductModeConfig.Save(newProductConfig))
                    {
                        // 可以选择性地显示成功消息，但为避免过多弹窗，此处省略
                    }
                    else
                    {
                        result = false;
                        MessageBox.Show("料号模式配置保存失败", "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                    LogTextHelper.Error("保存料号模式配置异常: " + ex.ToString());
                    MessageBox.Show("保存料号模式配置时发生错误。", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                dataPost.Refresh();
               
            }
            catch (Exception ex)
            {
                result = false;
                LogTextHelper.Error("异常" + ex.ToString());
            }
            return result;
        }
        private void btn_Add_Click(object sender, EventArgs e)
        {
            if (dic_solutionAndFlow.Count == 0)
            {
                MessageBox.Show("当前无可用方案/流程数据，无法添加。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var existingCodes = new HashSet<string>(
                dataPost.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => !r.IsNewRow && r.Cells[1].Value != null)
                        .Select(r => r.Cells[1].Value.ToString()),
                StringComparer.OrdinalIgnoreCase);

            string newMaterialCode = "NewItem1";
            int counter = 2;
            while (existingCodes.Contains(newMaterialCode))
            {
                newMaterialCode = $"NewItem{counter++}";
            }

            int index = this.dataPost.Rows.Add();
            dataPost.Rows[index].Cells["Index"].Value = index + 1;
            dataPost.Rows[index].Cells[1].Value = newMaterialCode;
            
            // 优先使用名为 "default" 的方案，找不到再用第一个
            KeyValuePair<string, List<string>> defaultSolution;
            if (dic_solutionAndFlow.TryGetValue("default", out var defaultFlows))
            {
                defaultSolution = new KeyValuePair<string, List<string>>("default", defaultFlows);
            }
            else
            {
                defaultSolution = dic_solutionAndFlow.First();
            }
            
            dataPost.Rows[index].Cells[2].Value = defaultSolution.Key;
            dataPost.Rows[index].Cells[3].Value = defaultSolution.Value.FirstOrDefault() ?? "(空流程)";
            dataPost.Rows[index].Cells[4].Value = defaultSolution.Key;
            dataPost.Rows[index].Cells[5].Value = defaultSolution.Value.FirstOrDefault() ?? "(空流程)";
            dataPost.Rows[index].Cells[6].Value = false;
            if (dataPost.Columns.Contains("Mode"))
            {
                dataPost.Rows[index].Cells["Mode"].Value = PicOptMode.by_machine.ToString();
            }
            dataPost.Refresh();
        }

        private void btn_Delete_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show(" 你 真 的 要 删 了 我 吗？\r\n", "删除提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (res == DialogResult.OK)
            {
                if (dataPost.CurrentRow != null && !dataPost.CurrentRow.IsNewRow)
                {
                    dataPost.Rows.Remove(dataPost.CurrentRow);
                    dataPost.Refresh(); //刷新显示
                }
            }
        }

        private void dataPost_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (   e.RowIndex >= 0&& e.ColumnIndex == dataPost.Columns["A_solution"].Index)
            {
                string selectedValue = dataPost.Rows[e.RowIndex].Cells["A_solution"].Value?.ToString();
                if (dic_solutionAndFlow.TryGetValue(selectedValue, out List<string> flowList))
                {
                    ((DataGridViewComboBoxCell)dataPost.Rows[e.RowIndex].Cells["A_flow"]).DataSource = flowList;
                    ((DataGridViewComboBoxCell)dataPost.Rows[e.RowIndex].Cells["A_flow"]).Value = flowList[0];
                }
                else
                {
                    MessageBox.Show("所选方案不存在");
                    return;
                }
            }
            if (e.RowIndex >= 0&&e.ColumnIndex == dataPost.Columns["B_solution"].Index)
            {
                string selectedValue = dataPost.Rows[e.RowIndex].Cells["B_solution"].Value?.ToString();
                if (dic_solutionAndFlow.TryGetValue(selectedValue, out List<string> flowList))
                {
                    ((DataGridViewComboBoxCell)dataPost.Rows[e.RowIndex].Cells["B_flow"]).DataSource = flowList;
                    ((DataGridViewComboBoxCell)dataPost.Rows[e.RowIndex].Cells["B_flow"]).Value = flowList[0];
                }
                else
                {
                    MessageBox.Show("所选方案不存在");
                    return;
                }

            }
            // Mode 列变更暂不处理，如需事件可在此扩展
        }
        // 新增：根据料号位置自动读取料号（目录或文件名），用于 AutoAdd
        private List<string> GetMaterialCodes(string rootPath)
        {
            var list = new List<string>();
            try
            {
                if (string.IsNullOrEmpty(rootPath) || !System.IO.Directory.Exists(rootPath))
                    return list;

                // 例：每个子目录即一个料号
                foreach (var dir in System.IO.Directory.GetDirectories(rootPath))
                {
                    list.Add(System.IO.Path.GetFileName(dir));
                }
                // 如果希望从文件获取：可以再加文件名逻辑
                // foreach (var file in System.IO.Directory.GetFiles(rootPath, "*.json")) { ... }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("读取料号目录失败:" + ex.Message);
            }
            return list;
        }
        private void btnAutoAdd_Click(object sender, EventArgs e)
        {
            // 原逻辑改造：按料号位置批量生成，跳过已存在的料号
            string loc = Machine.solconfig?.PartNumberImagesLoc;
            if (string.IsNullOrEmpty(loc))
            {
                MessageBox.Show("料号位置未配置，请先设置并保存。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var materials = GetMaterialCodes(loc);
            if (materials.Count == 0)
            {
                MessageBox.Show("料号位置无有效料号目录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 当前表格已有的料号集合（首列 liaohao）
            var existingCodes = new HashSet<string>(
                dataPost.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => !r.IsNewRow && r.Cells[1].Value != null)
                        .Select(r => r.Cells[1].Value.ToString()),
                StringComparer.OrdinalIgnoreCase);

            if (dic_solutionAndFlow.Count == 0)
            {
                MessageBox.Show("当前无可用方案/流程数据，无法添加。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 优先使用名为 "default" 的方案，找不到再用第一个
            KeyValuePair<string, List<string>> defaultSolutionPair;
            if (dic_solutionAndFlow.TryGetValue("default", out var defaultFlows))
            {
                defaultSolutionPair = new KeyValuePair<string, List<string>>("default", defaultFlows);
            }
            else
            {
                defaultSolutionPair = dic_solutionAndFlow.First();
            }
            string solKey = defaultSolutionPair.Key;
            string firstFlow = defaultSolutionPair.Value.FirstOrDefault() ?? "(空流程)";

            int added = 0;
            int skipped = 0;

            foreach (var code in materials)
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    skipped++;
                    continue;
                }

                if (existingCodes.Contains(code))
                {
                    // 已存在，跳过
                    skipped++;
                    continue;
                }

                int index = dataPost.Rows.Add();
                dataPost.Rows[index].Cells["Index"].Value = index + 1;
                dataPost.Rows[index].Cells[1].Value = code;      // 料号
                dataPost.Rows[index].Cells[2].Value = solKey;
                dataPost.Rows[index].Cells[3].Value = firstFlow;
                dataPost.Rows[index].Cells[4].Value = solKey;
                dataPost.Rows[index].Cells[5].Value = firstFlow;
                dataPost.Rows[index].Cells[6].Value = false;
                if (dataPost.Columns.Contains("Mode"))
                {
                    dataPost.Rows[index].Cells["Mode"].Value = PicOptMode.by_machine.ToString();
                }

                existingCodes.Add(code);
                added++;
            }

            dataPost.Refresh();

            MessageBox.Show($"批量添加完成，新增: {added}，跳过重复/空值: {skipped}", "结果", MessageBoxButtons.OK,
                added > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
    }
}
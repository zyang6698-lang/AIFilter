using DeepSightTool;
using DeepSightWorkLib.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DeepSightModel;
using DeepSightModel.Configuration;
using System.Collections.Generic;
using Sunny.UI;

namespace DeepSightAI.SettingPages
{
    /// <summary>
    /// 算法配置界面 - 以算法流程为核心，料号为从属
    /// </summary>
    public partial class FrAIConfig : Form
    {
        // Mode 枚举
        public enum PicOptMode
        {
            by_machine = 0,
            copy = 1,
            cut = 2,
        }

        // 内部数据结构：一个算法流程配置对应多个料号
        private class PipelineConfig
        {
            public string ConfigName { get; set; }
            public string ASolution { get; set; }
            public string AFlow { get; set; }
            public string BSolution { get; set; }
            public string BFlow { get; set; }
            public bool IsSwitch { get; set; }
            public List<ProductEntry> Products { get; set; } = new List<ProductEntry>();
        }

        private class ProductEntry
        {
            public string ProductSerial { get; set; }
            public string Mode { get; set; } = "by_machine";
            public string KeyDefectProfile { get; set; } = KeyDefectConfigManager.DefaultProfileName;
        }

        // 内部数据
        private List<PipelineConfig> _pipelineConfigs = new List<PipelineConfig>();
        private Dictionary<string, List<string>> dic_solutionAndFlow = new Dictionary<string, List<string>>();
        private DeepSight_ProductMode_class ProductModeConfig;

        private bool _isUpdating = false; // 防止递归触发事件

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

        public FrAIConfig()
        {
            InitializeComponent();
            InitParNumberConfig();
            CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            MaximizedBounds = Screen.PrimaryScreen.WorkingArea;

            // 初始化并加载数据
            GetSolutionFlow();

            // 绑定右侧料号列表行号更新事件
            dgvProducts.RowsAdded += (s, e) => UpdateProductRowIndices();
            dgvProducts.RowsRemoved += (s, e) => UpdateProductRowIndices();
        }

        private void UpdateProductRowIndices()
        {
            for (int i = 0; i < dgvProducts.Rows.Count; i++)
            {
                if (dgvProducts.Rows[i].IsNewRow) continue;
                dgvProducts.Rows[i].Cells["colIndex"].Value = i + 1;
            }
        }

        private void UpdatePipelineProductCount()
        {
            for (int i = 0; i < dgvPipeline.Rows.Count; i++)
            {
                if (dgvPipeline.Rows[i].IsNewRow) continue;
                if (i < _pipelineConfigs.Count)
                {
                    dgvPipeline.Rows[i].Cells["colProductCount"].Value = _pipelineConfigs[i].Products.Count;
                }
            }
        }

        /// <summary>
        /// 获取方案流程列表并初始化界面
        /// </summary>
        public void GetSolutionFlow(bool isInface = false)
        {
            dgvPipeline.Rows.Clear();
            dgvProducts.Rows.Clear();
            _pipelineConfigs.Clear();
            try
            {
                var json = JsonConvert.SerializeObject(new
                {
                    message_type = "visionbuilder_solution_flow_list"
                });
                IntPtr input = Marshal.StringToHGlobalAnsi(json);
                Machine.master.DefectService.AiDefect.Vision_runMethod(input, out IntPtr intPtr);
                string solutionandflow_List = Marshal.PtrToStringAnsi(intPtr);

                if (string.IsNullOrEmpty(solutionandflow_List))
                {
                    AiEngineAlarm.ReportSolutionListEmpty("Vision_runMethod 返回为空字符串");
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
                    if (!dic_solutionAndFlow.ContainsKey(sol.Solution))
                    {
                        dic_solutionAndFlow.Add(sol.Solution, sol.FlowList.Count > 0 ? sol.FlowList : new List<string> { "(空流程)" });
                    }
                }

                // 更新左侧表格的ComboBox数据源
                var solutionList = dic_solutionAndFlow.Keys.ToList();
                var allFlows = dic_solutionAndFlow.Values.SelectMany(list => list).Distinct().ToList();
                colASolution.DataSource = solutionList;
                colAFlow.DataSource = allFlows;
                colBSolution.DataSource = solutionList;
                colBFlow.DataSource = allFlows;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("获取方案流程失败: " + ex.ToString());
                // 避免与上面"空字符串"分支重复触发：仅在异常消息不含"为空"关键字时再次上报
                if (ex.Message == null || !ex.Message.Contains("为空"))
                {
                    AiEngineAlarm.ReportSolutionListEmpty("Vision_runMethod 调用异常: " + ex.Message);
                }
                dic_solutionAndFlow.Clear();
                dic_solutionAndFlow.Add("DefaultSolution", new List<string> { "(空流程)" });

                var defaultKeys = new List<string> { "DefaultSolution" };
                var defaultValues = new List<string> { "(空流程)" };
                colASolution.DataSource = defaultKeys;
                colAFlow.DataSource = defaultValues;
                colBSolution.DataSource = defaultKeys;
                colBFlow.DataSource = defaultValues;
            }
            finally
            {
                // 加载已保存配置
                InitMethod();
            }
        }

        private void InitParNumberConfig()
        {
            ProductModeConfig = new DeepSight_ProductMode_class();

        }
        /// <summary>
        /// 绑定数据 - 直接从 Pipelines 层级模型读取
        /// </summary>
        private void InitMethod()
        {
            try
            {
                _pipelineConfigs.Clear();

                // 读取ProductMode配置
                ProductModeConfig.Read(out ProductModeConfig modeConfig);
                // 去重：如果有重复Name，取第一个
                var modeDict = modeConfig?.Products?
                    .Where(p => !string.IsNullOrEmpty(p.Name))
                    .GroupBy(p => p.Name)
                    .ToDictionary(g => g.Key, g => g.First().CopyCutMode)
                    ?? new Dictionary<string, string>();

                if (Machine.solconfig?.Pipelines != null)
                {
                    foreach (var pipeline in Machine.solconfig.Pipelines)
                    {
                        var config = new PipelineConfig
                        {
                            ConfigName = pipeline.Name ?? PipelineFlowConfig.DefaultName,
                            ASolution = pipeline.Asolution ?? "",
                            AFlow = pipeline.Aflow ?? "",
                            BSolution = pipeline.Bsolution ?? "",
                            BFlow = pipeline.Bflow ?? "",
                            IsSwitch = pipeline.IsSwitch,
                            Products = (pipeline.ProductSerials ?? new List<string>())
                                .Select(ps => new ProductEntry
                                {
                                    ProductSerial = ps ?? "",
                                    Mode = modeDict.TryGetValue(ps ?? "", out var m) ? m : PicOptMode.by_machine.ToString(),
                                    KeyDefectProfile = KeyDefectConfigManager.Instance.GetProfileNameForProduct(ps ?? "")
                                }).ToList()
                        };
                        _pipelineConfigs.Add(config);
                    }
                }

                // 如果没有任何配置，添加一个 DEFAULT 配置
                if (_pipelineConfigs.Count == 0)
                {
                    var defaultSolution = dic_solutionAndFlow.Keys.FirstOrDefault() ?? "DefaultSolution";
                    var defaultFlow = dic_solutionAndFlow.TryGetValue(defaultSolution, out var flows) && flows.Count > 0
                        ? flows[0] : "(空流程)";

                    _pipelineConfigs.Add(new PipelineConfig
                    {
                        ConfigName = PipelineFlowConfig.DefaultName,
                        ASolution = defaultSolution,
                        AFlow = defaultFlow,
                        BSolution = defaultSolution,
                        BFlow = defaultFlow,
                        IsSwitch = false,
                        Products = new List<ProductEntry>()
                    });
                }

                // 填充左侧算法配置表格
                _isUpdating = true;
                foreach (var config in _pipelineConfigs)
                {
                    int rowIdx = dgvPipeline.Rows.Add();
                    var row = dgvPipeline.Rows[rowIdx];
                    row.Cells["colConfigName"].Value = config.ConfigName;
                    row.Cells["colASolution"].Value = config.ASolution;
                    row.Cells["colAFlow"].Value = config.AFlow;
                    row.Cells["colBSolution"].Value = config.BSolution;
                    row.Cells["colBFlow"].Value = config.BFlow;
                    row.Cells["colIsSwitch"].Value = config.IsSwitch;
                    row.Cells["colProductCount"].Value = config.Products.Count;
                }
                _isUpdating = false;

                // 自动选中第一行
                if (dgvPipeline.Rows.Count > 0)
                {
                    dgvPipeline.ClearSelection();
                    dgvPipeline.Rows[0].Selected = true;
                    dgvPipeline.CurrentCell = dgvPipeline.Rows[0].Cells[0];
                    RefreshProductsGrid(0);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("InitMethod异常: " + ex.ToString());
            }
        }

        /// <summary>
        /// 刷新右侧料号表格
        /// </summary>
        private void RefreshProductsGrid(int pipelineIndex)
        {
            dgvProducts.Rows.Clear();
            if (pipelineIndex < 0 || pipelineIndex >= _pipelineConfigs.Count) return;

            // 刷新缺陷配置 ComboBox 选项
            RefreshKeyDefectProfileColumn();

            var products = _pipelineConfigs[pipelineIndex].Products;
            for (int i = 0; i < products.Count; i++)
            {
                int rowIdx = dgvProducts.Rows.Add();
                dgvProducts.Rows[rowIdx].Cells["colIndex"].Value = i + 1;
                dgvProducts.Rows[rowIdx].Cells["colProductSerial"].Value = products[i].ProductSerial;
                dgvProducts.Rows[rowIdx].Cells["colMode"].Value = products[i].Mode;
                dgvProducts.Rows[rowIdx].Cells["colKeyDefectProfile"].Value = products[i].KeyDefectProfile;
            }
        }

        /// <summary>
        /// 刷新缺陷配置 Profile 下拉列表
        /// </summary>
        private void RefreshKeyDefectProfileColumn()
        {
            colKeyDefectProfile.Items.Clear();
            var profileNames = KeyDefectConfigManager.Instance.GetProfileNames();
            foreach (var name in profileNames)
                colKeyDefectProfile.Items.Add(name);
        }

        #region 事件处理

        /// <summary>
        /// 左侧算法配置选中变化 - 刷新右侧料号列表
        /// </summary>
        private void dgvPipeline_SelectionChanged(object sender, EventArgs e)
        {
            if (_isUpdating) return;
            if (dgvPipeline.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvPipeline.SelectedRows[0].Index;
                RefreshProductsGrid(selectedIndex);
            }
        }

        /// <summary>
        /// 左侧算法配置单元格值变化
        /// </summary>
        private void dgvPipeline_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_isUpdating || e.RowIndex < 0 || e.RowIndex >= _pipelineConfigs.Count) return;

            var config = _pipelineConfigs[e.RowIndex];
            var colName = dgvPipeline.Columns[e.ColumnIndex].Name;
            var cellValue = dgvPipeline.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            switch (colName)
            {
                case "colConfigName":
                    config.ConfigName = cellValue?.ToString() ?? "";
                    break;
                case "colASolution":
                    config.ASolution = cellValue?.ToString() ?? "";
                    // 当Solution改变时，更新对应的Flow下拉选项并同步单元格值
                    if (dic_solutionAndFlow.TryGetValue(config.ASolution, out var aFlows))
                    {
                        _isUpdating = true;
                        var aFlowCell = (DataGridViewComboBoxCell)dgvPipeline.Rows[e.RowIndex].Cells["colAFlow"];
                        aFlowCell.DataSource = aFlows;
                        if (aFlows.Count > 0)
                        {
                            config.AFlow = aFlows[0];
                            aFlowCell.Value = aFlows[0];
                        }
                        _isUpdating = false;
                    }
                    break;
                case "colAFlow":
                    config.AFlow = cellValue?.ToString() ?? "";
                    break;
                case "colBSolution":
                    config.BSolution = cellValue?.ToString() ?? "";
                    if (dic_solutionAndFlow.TryGetValue(config.BSolution, out var bFlows))
                    {
                        _isUpdating = true;
                        var bFlowCell = (DataGridViewComboBoxCell)dgvPipeline.Rows[e.RowIndex].Cells["colBFlow"];
                        bFlowCell.DataSource = bFlows;
                        if (bFlows.Count > 0)
                        {
                            config.BFlow = bFlows[0];
                            bFlowCell.Value = bFlows[0];
                        }
                        _isUpdating = false;
                    }
                    break;
                case "colBFlow":
                    config.BFlow = cellValue?.ToString() ?? "";
                    break;
                case "colIsSwitch":
                    config.IsSwitch = cellValue is bool b && b;
                    break;
            }
        }

        /// <summary>
        /// 处理DataGridView数据错误，防止弹出错误对话框
        /// </summary>
        private void dgvPipeline_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // 抑制ComboBox值无效等错误的默认弹窗
            e.ThrowException = false;
        }

        private void btn_GetAgain_Click(object sender, EventArgs e)
        {
            GetSolutionFlow(true);
        }

        private void btn_setSolution_Click(object sender, EventArgs e)
        {
            SaveParam();
            MessageBox.Show("方案及流程设置成功", "设置成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region 保存功能

        /// <summary>
        /// 保存配置 - 直接保存为 Pipelines 层级格式
        /// </summary>
        public bool SaveParam()
        {
            bool result = true;
            try
            {
                // 先同步右侧表格数据到当前选中的配置
                SyncProductsToCurrentConfig();

                // 构建新格式 SolutionConfig
                SolutionConfig solConfig = new SolutionConfig
                {
                    Pipelines = new List<PipelineFlowConfig>(),
                    PartNumberImagesLoc = Machine.solconfig?.PartNumberImagesLoc
                };

                ProductModeConfig newProductConfig = new ProductModeConfig { Products = new List<ProductModeItem>() };

                foreach (var config in _pipelineConfigs)
                {
                    solConfig.Pipelines.Add(new PipelineFlowConfig
                    {
                        Name = config.ConfigName,
                        Asolution = config.ASolution,
                        Aflow = config.AFlow,
                        Bsolution = config.BSolution,
                        Bflow = config.BFlow,
                        IsSwitch = config.IsSwitch,
                        ProductSerials = config.Products.Select(p => p.ProductSerial).ToList()
                    });

                    foreach (var product in config.Products)
                    {
                        newProductConfig.Products.Add(new ProductModeItem
                        {
                            Name = product.ProductSerial,
                            CopyCutMode = product.Mode
                        });
                    }
                }

                Machine.solconfig = solConfig;
                result = Machine.sol_class.Save(solConfig);

                if (!ProductModeConfig.Save(newProductConfig))
                {
                    result = false;
                    MessageBox.Show("料号模式配置保存失败", "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // 保存料号与缺陷配置的映射
                var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var config in _pipelineConfigs)
                {
                    foreach (var product in config.Products)
                    {
                        if (!string.IsNullOrWhiteSpace(product.ProductSerial) &&
                            !string.IsNullOrWhiteSpace(product.KeyDefectProfile) &&
                            !product.KeyDefectProfile.Equals(KeyDefectConfigManager.DefaultProfileName, StringComparison.OrdinalIgnoreCase))
                        {
                            mappings[product.ProductSerial] = product.KeyDefectProfile;
                        }
                    }
                }
                KeyDefectConfigManager.Instance.SaveMappings(mappings);
            }
            catch (Exception ex)
            {
                result = false;
                LogTextHelper.Error("SaveParam异常: " + ex.ToString());
                MessageBox.Show("保存配置时发生错误。", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return result;
        }

        /// <summary>
        /// 同步右侧表格数据到当前选中的配置
        /// </summary>
        private void SyncProductsToCurrentConfig()
        {
            if (dgvPipeline.SelectedRows.Count == 0) return;
            int selectedIndex = dgvPipeline.SelectedRows[0].Index;
            if (selectedIndex < 0 || selectedIndex >= _pipelineConfigs.Count) return;

            var config = _pipelineConfigs[selectedIndex];
            config.Products.Clear();

            foreach (DataGridViewRow row in dgvProducts.Rows)
            {
                if (row.IsNewRow) continue;
                var serial = row.Cells["colProductSerial"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(serial)) continue;

                config.Products.Add(new ProductEntry
                {
                    ProductSerial = serial,
                    Mode = row.Cells["colMode"].Value?.ToString() ?? PicOptMode.by_machine.ToString(),
                    KeyDefectProfile = row.Cells["colKeyDefectProfile"].Value?.ToString() ?? KeyDefectConfigManager.DefaultProfileName
                });
            }

            UpdatePipelineProductCount();
        }

        #endregion

        #region 添加/删除配置

        private void btnAddConfig_Click(object sender, EventArgs e)
        {
            if (dic_solutionAndFlow.Count == 0)
            {
                MessageBox.Show("当前无可用方案/流程数据，无法添加配置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 同步当前选中配置的料号数据
            SyncProductsToCurrentConfig();

            var defaultSolution = dic_solutionAndFlow.Keys.FirstOrDefault() ?? "DefaultSolution";
            var defaultFlow = dic_solutionAndFlow.TryGetValue(defaultSolution, out var flows) && flows.Count > 0
                ? flows[0] : "(空流程)";

            var newConfig = new PipelineConfig
            {
                ConfigName = $"配置{_pipelineConfigs.Count + 1}",
                ASolution = defaultSolution,
                AFlow = defaultFlow,
                BSolution = defaultSolution,
                BFlow = defaultFlow,
                IsSwitch = false,
                Products = new List<ProductEntry>()
            };
            _pipelineConfigs.Add(newConfig);

            // 添加到表格
            _isUpdating = true;
            int rowIdx = dgvPipeline.Rows.Add();
            var row = dgvPipeline.Rows[rowIdx];
            row.Cells["colConfigName"].Value = newConfig.ConfigName;
            row.Cells["colASolution"].Value = newConfig.ASolution;
            row.Cells["colAFlow"].Value = newConfig.AFlow;
            row.Cells["colBSolution"].Value = newConfig.BSolution;
            row.Cells["colBFlow"].Value = newConfig.BFlow;
            row.Cells["colIsSwitch"].Value = newConfig.IsSwitch;
            row.Cells["colProductCount"].Value = 0;
            _isUpdating = false;

            // 选中新行
            dgvPipeline.ClearSelection();
            dgvPipeline.Rows[rowIdx].Selected = true;
            dgvPipeline.CurrentCell = dgvPipeline.Rows[rowIdx].Cells[0];
            RefreshProductsGrid(rowIdx);
        }

        private void btnDeleteConfig_Click(object sender, EventArgs e)
        {
            if (dgvPipeline.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要删除的配置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_pipelineConfigs.Count <= 1)
            {
                MessageBox.Show("至少需要保留一个配置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult res = MessageBox.Show("确定要删除选中的配置吗？其下所有料号将被移除。", "删除确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (res == DialogResult.OK)
            {
                int selectedIndex = dgvPipeline.SelectedRows[0].Index;
                _pipelineConfigs.RemoveAt(selectedIndex);
                dgvPipeline.Rows.RemoveAt(selectedIndex);

                // 选中第一行
                if (dgvPipeline.Rows.Count > 0)
                {
                    dgvPipeline.ClearSelection();
                    dgvPipeline.Rows[0].Selected = true;
                    RefreshProductsGrid(0);
                }
            }
        }

        #endregion

        #region 添加/删除料号

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            if (dgvPipeline.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择左侧的算法配置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int selectedIndex = dgvPipeline.SelectedRows[0].Index;
            if (selectedIndex < 0 || selectedIndex >= _pipelineConfigs.Count) return;

            // 检查是否已存在同名料号（全局）
            var existingCodes = new HashSet<string>(
                _pipelineConfigs.SelectMany(c => c.Products).Select(p => p.ProductSerial),
                StringComparer.OrdinalIgnoreCase);

            string newCode = "NewItem1";
            int counter = 2;
            while (existingCodes.Contains(newCode))
            {
                newCode = $"NewItem{counter++}";
            }

            var newProduct = new ProductEntry
            {
                ProductSerial = newCode,
                Mode = PicOptMode.by_machine.ToString(),
                KeyDefectProfile = KeyDefectConfigManager.DefaultProfileName
            };
            _pipelineConfigs[selectedIndex].Products.Add(newProduct);

            int rowIdx = dgvProducts.Rows.Add();
            dgvProducts.Rows[rowIdx].Cells["colIndex"].Value = rowIdx + 1;
            dgvProducts.Rows[rowIdx].Cells["colProductSerial"].Value = newProduct.ProductSerial;
            dgvProducts.Rows[rowIdx].Cells["colMode"].Value = newProduct.Mode;
            dgvProducts.Rows[rowIdx].Cells["colKeyDefectProfile"].Value = newProduct.KeyDefectProfile;

            UpdatePipelineProductCount();
        }

        private void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要删除的料号。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dgvPipeline.SelectedRows.Count == 0) return;
            int pipelineIndex = dgvPipeline.SelectedRows[0].Index;
            if (pipelineIndex < 0 || pipelineIndex >= _pipelineConfigs.Count) return;

            DialogResult res = MessageBox.Show("确定要删除选中的料号吗？", "删除确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (res == DialogResult.OK)
            {
                int productIndex = dgvProducts.SelectedRows[0].Index;
                if (productIndex >= 0 && productIndex < _pipelineConfigs[pipelineIndex].Products.Count)
                {
                    _pipelineConfigs[pipelineIndex].Products.RemoveAt(productIndex);
                    dgvProducts.Rows.RemoveAt(productIndex);
                    UpdateProductRowIndices();
                    UpdatePipelineProductCount();
                }
            }
        }

        #endregion

        #region 批量导入

        /// <summary>
        /// 根据料号位置自动读取料号目录
        /// </summary>
        private List<string> GetMaterialCodes(string rootPath)
        {
            var list = new List<string>();
            try
            {
                if (string.IsNullOrEmpty(rootPath) || !System.IO.Directory.Exists(rootPath))
                    return list;

                foreach (var dir in System.IO.Directory.GetDirectories(rootPath))
                {
                    list.Add(System.IO.Path.GetFileName(dir));
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("读取料号目录失败:" + ex.Message);
            }
            return list;
        }

        private void btnAutoAdd_Click(object sender, EventArgs e)
        {
            if (dgvPipeline.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择左侧的算法配置，批量导入的料号将添加到该配置下。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int selectedIndex = dgvPipeline.SelectedRows[0].Index;
            if (selectedIndex < 0 || selectedIndex >= _pipelineConfigs.Count) return;

            string loc = Machine.solconfig?.PartNumberImagesLoc;
            if (string.IsNullOrEmpty(loc))
            {
                MessageBox.Show("料号位置未配置，请先在设置中配置料号图片位置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var materials = GetMaterialCodes(loc);
            if (materials.Count == 0)
            {
                MessageBox.Show("料号位置无有效料号目录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 全局已有的料号集合
            var existingCodes = new HashSet<string>(
                _pipelineConfigs.SelectMany(c => c.Products).Select(p => p.ProductSerial),
                StringComparer.OrdinalIgnoreCase);

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
                    skipped++;
                    continue;
                }

                var newProduct = new ProductEntry
                {
                    ProductSerial = code,
                    Mode = PicOptMode.by_machine.ToString(),
                    KeyDefectProfile = KeyDefectConfigManager.DefaultProfileName
                };
                _pipelineConfigs[selectedIndex].Products.Add(newProduct);

                int rowIdx = dgvProducts.Rows.Add();
                dgvProducts.Rows[rowIdx].Cells["colIndex"].Value = rowIdx + 1;
                dgvProducts.Rows[rowIdx].Cells["colProductSerial"].Value = newProduct.ProductSerial;
                dgvProducts.Rows[rowIdx].Cells["colMode"].Value = newProduct.Mode;
                dgvProducts.Rows[rowIdx].Cells["colKeyDefectProfile"].Value = newProduct.KeyDefectProfile;

                existingCodes.Add(code);
                added++;
            }

            UpdatePipelineProductCount();

            MessageBox.Show($"批量添加完成，新增: {added}，跳过重复/空值: {skipped}", "结果", MessageBoxButtons.OK,
                added > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        #endregion
    }
}
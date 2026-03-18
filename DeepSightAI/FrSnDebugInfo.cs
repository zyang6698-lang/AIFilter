using DeepSightModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// SN调试信息显示窗口 - 双击任务队列时弹出
    /// </summary>
    public partial class FrSnDebugInfo : Form
    {
        private int _lastSearchIndex = 0;
        private TreeNode _lastSearchNode = null;

        /// <summary>
        /// 存储每个Tab对应的原始JSON字符串，用于复制功能
        /// </summary>
        private readonly Dictionary<TabPage, string> _tabRawJsonMap = new Dictionary<TabPage, string>();

        public FrSnDebugInfo(string sn, SnDebugInfo[] debugInfos)
        {
            InitializeComponent();
            Text = $"SN调试信息 - {sn}";
            LoadData(sn, debugInfos);
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            _lastSearchIndex = 0;
            _lastSearchNode = null;
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnFind_Click(object sender, EventArgs e)
        {
            FindNext();
        }

        private void BtnCloseSearch_Click(object sender, EventArgs e)
        {
            searchPanel.Visible = false;
        }

        #region LoadData

        private void LoadData(string sn, SnDebugInfo[] debugInfos)
        {
            if (debugInfos == null || debugInfos.Length == 0)
            {
                SetTreeViewPlaceholder(tvLevelDbJson, "暂无数据 - 该SN尚未完成数据读取");
                SetTreeViewPlaceholder(tvPanelInfoJson, "暂无数据");
                SetTreeViewPlaceholder(tvVbInferenceJson, "暂无数据");
                SetTreeViewPlaceholder(tvInferenceReturnJson, "暂无数据");
                txtOtherInfo.Text = "暂无数据";
                return;
            }

            // LevelDB请求AB侧相同，只显示一份
            string rawLevelDb = null;
            foreach (var info in debugInfos)
            {
                if (!string.IsNullOrEmpty(info.RawLevelDbJson))
                {
                    rawLevelDb = info.RawLevelDbJson;
                    break;
                }
            }
            LoadJsonToTreeView(tvLevelDbJson, tabLevelDb, rawLevelDb);

            // 其余按AB侧分开显示（已排序A在前）
            var sbOther = new System.Text.StringBuilder();

            foreach (var info in debugInfos)
            {
                string sideLabel = $"===== {info.Side}面 =====";

                // PanelInfo JSON - 按面添加到TreeView
                LoadSideJsonToTreeView(tvPanelInfoJson, info.Side, info.PanelInfoJson);

                // VB Inference JSON
                LoadSideJsonToTreeView(tvVbInferenceJson, info.Side, info.VbInferenceJson);

                // 推理返回JSON
                LoadSideJsonToTreeView(tvInferenceReturnJson, info.Side, info.InferenceReturnJson);

                // Other info
                sbOther.AppendLine(sideLabel);
                sbOther.AppendLine($"SN:              {info.SerialNumber}");
                sbOther.AppendLine($"面别:            {info.Side}");
                sbOther.AppendLine($"缺陷数量:        {info.DefectCount}");
                sbOther.AppendLine($"PCS数量:         {info.PcsCount}");
                sbOther.AppendLine($"图片数量:        {info.ImageCount}");
                sbOther.AppendLine($"是否ByPass:      {(info.IsByPass ? "是" : "否")}");
                sbOther.AppendLine($"数据源DB:        {info.SourceDbName}");
                sbOther.AppendLine($"数据源URL:       {info.SourceDbUrl}");
                sbOther.AppendLine($"Minio路径:       {info.MinioPath}");
                sbOther.AppendLine($"数据获取时间:    {info.CreateTime:yyyy-MM-dd HH:mm:ss.fff}");

                if (info.HasError)
                {
                    sbOther.AppendLine();
                    sbOther.AppendLine("---------- 错误信息 ----------");
                    sbOther.AppendLine($"出错步骤:        {info.ErrorStep}");
                    sbOther.AppendLine($"错误原因:        {info.ErrorMessage}");
                    sbOther.AppendLine($"错误时间:        {info.ErrorTime:yyyy-MM-dd HH:mm:ss.fff}");
                }
                sbOther.AppendLine();
            }

            // 存储AB侧的原始JSON用于复制
            StoreSideRawJson(tabPanelInfo, debugInfos, d => d.PanelInfoJson);
            StoreSideRawJson(tabVbJson, debugInfos, d => d.VbInferenceJson);
            StoreSideRawJson(tabInferReturn, debugInfos, d => d.InferenceReturnJson);

            // 展开TreeView第一层节点
            ExpandFirstLevel(tvPanelInfoJson);
            ExpandFirstLevel(tvVbInferenceJson);
            ExpandFirstLevel(tvInferenceReturnJson);

            txtOtherInfo.Text = sbOther.ToString();
        }

        #endregion

        #region JSON → TreeView 辅助方法

        private void SetTreeViewPlaceholder(TreeView tv, string text)
        {
            tv.Nodes.Clear();
            tv.Nodes.Add(new TreeNode(text));
        }

        /// <summary>
        /// 将单个JSON字符串加载到TreeView（用于LevelDB等不分面的场景）
        /// </summary>
        private void LoadJsonToTreeView(TreeView tv, TabPage tab, string json)
        {
            tv.Nodes.Clear();
            if (string.IsNullOrEmpty(json))
            {
                tv.Nodes.Add(new TreeNode("暂无数据"));
                return;
            }

            try
            {
                var token = JToken.Parse(json);
                var rootNode = new TreeNode("JSON");
                AddJTokenNodes(rootNode, token);
                tv.Nodes.Add(rootNode);
                rootNode.Expand();
                _tabRawJsonMap[tab] = token.ToString(Newtonsoft.Json.Formatting.Indented);
            }
            catch
            {
                tv.Nodes.Add(new TreeNode(json));
                _tabRawJsonMap[tab] = json;
            }
        }

        /// <summary>
        /// 按面别将JSON添加到TreeView（用于AB侧分开显示的场景）
        /// </summary>
        private void LoadSideJsonToTreeView(TreeView tv, string side, string json)
        {
            var sideNode = new TreeNode($"===== {side}面 =====");
            sideNode.ForeColor = Color.FromArgb(100, 200, 255);

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var token = JToken.Parse(json);
                    AddJTokenNodes(sideNode, token);
                }
                catch
                {
                    sideNode.Nodes.Add(new TreeNode(json));
                }
            }
            else
            {
                sideNode.Nodes.Add(new TreeNode("暂无数据"));
            }

            tv.Nodes.Add(sideNode);
            tv.ExpandAll();
        }

        /// <summary>
        /// 递归将JToken转换为TreeNode
        /// </summary>
        private void AddJTokenNodes(TreeNode parentNode, JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var prop in ((JObject)token).Properties())
                    {
                        if (prop.Value.Type == JTokenType.Object)
                        {
                            var childCount = ((JObject)prop.Value).Count;
                            var node = new TreeNode($"{prop.Name}  {{ {childCount} }}");
                            node.ForeColor = Color.FromArgb(150, 200, 255);
                            AddJTokenNodes(node, prop.Value);
                            parentNode.Nodes.Add(node);
                        }
                        else if (prop.Value.Type == JTokenType.Array)
                        {
                            var arr = (JArray)prop.Value;
                            var node = new TreeNode($"{prop.Name}  [ {arr.Count} ]");
                            node.ForeColor = Color.FromArgb(200, 180, 100);
                            AddJTokenNodes(node, prop.Value);
                            parentNode.Nodes.Add(node);
                        }
                        else
                        {
                            var valStr = prop.Value.Type == JTokenType.Null ? "null" : prop.Value.ToString();
                            var node = new TreeNode($"{prop.Name}: {valStr}");
                            node.ForeColor = GetValueColor(prop.Value);
                            parentNode.Nodes.Add(node);
                        }
                    }
                    break;

                case JTokenType.Array:
                    var array = (JArray)token;
                    for (int i = 0; i < array.Count; i++)
                    {
                        var item = array[i];
                        if (item.Type == JTokenType.Object)
                        {
                            var childCount = ((JObject)item).Count;
                            var node = new TreeNode($"[{i}]  {{ {childCount} }}");
                            node.ForeColor = Color.FromArgb(150, 200, 255);
                            AddJTokenNodes(node, item);
                            parentNode.Nodes.Add(node);
                        }
                        else if (item.Type == JTokenType.Array)
                        {
                            var innerArr = (JArray)item;
                            var node = new TreeNode($"[{i}]  [ {innerArr.Count} ]");
                            node.ForeColor = Color.FromArgb(200, 180, 100);
                            AddJTokenNodes(node, item);
                            parentNode.Nodes.Add(node);
                        }
                        else
                        {
                            var valStr = item.Type == JTokenType.Null ? "null" : item.ToString();
                            var node = new TreeNode($"[{i}]: {valStr}");
                            node.ForeColor = GetValueColor(item);
                            parentNode.Nodes.Add(node);
                        }
                    }
                    break;

                default:
                    parentNode.Nodes.Add(new TreeNode(token.ToString()));
                    break;
            }
        }

        private Color GetValueColor(JToken value)
        {
            switch (value.Type)
            {
                case JTokenType.String:
                    return Color.FromArgb(180, 230, 150); // 绿色 - 字符串
                case JTokenType.Integer:
                case JTokenType.Float:
                    return Color.FromArgb(200, 160, 255); // 紫色 - 数字
                case JTokenType.Boolean:
                    return Color.FromArgb(255, 150, 100); // 橙色 - 布尔
                case JTokenType.Null:
                    return Color.FromArgb(128, 128, 128); // 灰色 - null
                default:
                    return Color.FromArgb(200, 220, 240);
            }
        }

        private void ExpandFirstLevel(TreeView tv)
        {
            foreach (TreeNode node in tv.Nodes)
            {
                node.Expand();
            }
        }

        private void StoreSideRawJson(TabPage tab, SnDebugInfo[] debugInfos, Func<SnDebugInfo, string> jsonSelector)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var info in debugInfos)
            {
                sb.AppendLine($"===== {info.Side}面 =====");
                var json = jsonSelector(info);
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        sb.AppendLine(JToken.Parse(json).ToString(Newtonsoft.Json.Formatting.Indented));
                    }
                    catch
                    {
                        sb.AppendLine(json);
                    }
                }
                else
                {
                    sb.AppendLine("暂无数据");
                }
                sb.AppendLine();
            }
            _tabRawJsonMap[tab] = sb.ToString();
        }

        #endregion

        #region 复制 / 搜索

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                var currentTab = tabControl.SelectedTab;

                // 优先从存储的原始JSON获取
                if (_tabRawJsonMap.TryGetValue(currentTab, out var rawJson) && !string.IsNullOrEmpty(rawJson))
                {
                    Clipboard.SetText(rawJson);
                    MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 其他信息Tab仍使用RichTextBox
                if (currentTab?.Controls.Count > 0 && currentTab.Controls[0] is RichTextBox rtb)
                {
                    if (!string.IsNullOrEmpty(rtb.Text))
                    {
                        Clipboard.SetText(rtb.Text);
                        MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrSnDebugInfo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                e.SuppressKeyPress = true;
                searchPanel.Visible = true;
                txtSearch.Focus();
                txtSearch.SelectAll();
            }
            else if (e.KeyCode == Keys.Escape && searchPanel.Visible)
            {
                searchPanel.Visible = false;
                e.SuppressKeyPress = true;
            }
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                FindNext();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                searchPanel.Visible = false;
                e.SuppressKeyPress = true;
            }
        }

        private void FindNext()
        {
            string keyword = txtSearch.Text;
            if (string.IsNullOrEmpty(keyword)) return;

            var currentTab = tabControl.SelectedTab;
            if (currentTab?.Controls.Count > 0)
            {
                if (currentTab.Controls[0] is TreeView tv)
                {
                    FindNextInTreeView(tv, keyword);
                }
                else if (currentTab.Controls[0] is RichTextBox rtb)
                {
                    FindNextInRichTextBox(rtb, keyword);
                }
            }
        }

        private void FindNextInTreeView(TreeView tv, string keyword)
        {
            // 收集所有节点为平铺列表
            var allNodes = new List<TreeNode>();
            CollectAllNodes(tv.Nodes, allNodes);

            if (allNodes.Count == 0) return;

            // 找到上次搜索节点的位置
            int startIdx = 0;
            if (_lastSearchNode != null)
            {
                int lastIdx = allNodes.IndexOf(_lastSearchNode);
                if (lastIdx >= 0) startIdx = lastIdx + 1;
            }

            // 从上次位置开始搜索
            for (int i = 0; i < allNodes.Count; i++)
            {
                int idx = (startIdx + i) % allNodes.Count;
                var node = allNodes[idx];
                if (node.Text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // 如果回到了起始点之前说明已经绕了一圈
                    if (i > 0 && idx < startIdx && _lastSearchNode != null)
                    {
                        // 继续，回绕搜索
                    }

                    tv.SelectedNode = node;
                    node.EnsureVisible();
                    _lastSearchNode = node;
                    tv.Focus();
                    return;
                }
            }

            MessageBox.Show("未找到匹配内容", "搜索", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _lastSearchNode = null;
        }

        private void CollectAllNodes(TreeNodeCollection nodes, List<TreeNode> list)
        {
            foreach (TreeNode node in nodes)
            {
                list.Add(node);
                if (node.Nodes.Count > 0)
                    CollectAllNodes(node.Nodes, list);
            }
        }

        private void FindNextInRichTextBox(RichTextBox rtb, string keyword)
        {
            int startIndex = _lastSearchIndex;
            if (startIndex >= rtb.TextLength) startIndex = 0;

            int index = rtb.Find(keyword, startIndex, RichTextBoxFinds.None);
            if (index >= 0)
            {
                rtb.Select(index, keyword.Length);
                rtb.SelectionBackColor = Color.FromArgb(255, 200, 50);
                rtb.SelectionColor = Color.Black;
                rtb.ScrollToCaret();
                _lastSearchIndex = index + keyword.Length;
            }
            else
            {
                if (startIndex > 0)
                {
                    index = rtb.Find(keyword, 0, RichTextBoxFinds.None);
                    if (index >= 0)
                    {
                        rtb.Select(index, keyword.Length);
                        rtb.SelectionBackColor = Color.FromArgb(255, 200, 50);
                        rtb.SelectionColor = Color.Black;
                        rtb.ScrollToCaret();
                        _lastSearchIndex = index + keyword.Length;
                        return;
                    }
                }
                MessageBox.Show("未找到匹配内容", "搜索", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _lastSearchIndex = 0;
            }
        }

        #endregion
    }
}


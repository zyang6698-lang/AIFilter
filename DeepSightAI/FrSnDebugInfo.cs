using DeepSightModel;
using System;
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

        public FrSnDebugInfo(string sn, SnDebugInfo[] debugInfos)
        {
            InitializeComponent();
            Text = $"SN调试信息 - {sn}";
            LoadData(sn, debugInfos);
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            _lastSearchIndex = 0;
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

        private void LoadData(string sn, SnDebugInfo[] debugInfos)
        {
            if (debugInfos == null || debugInfos.Length == 0)
            {
                txtLevelDbJson.Text = "暂无数据 - 该SN尚未完成数据读取";
                txtPanelInfoJson.Text = "暂无数据";
                txtVbInferenceJson.Text = "暂无数据";
                txtInferenceReturnJson.Text = "暂无数据";
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
            if (!string.IsNullOrEmpty(rawLevelDb))
            {
                try
                {
                    txtLevelDbJson.Text = Newtonsoft.Json.Linq.JToken.Parse(rawLevelDb).ToString(Newtonsoft.Json.Formatting.Indented);
                }
                catch
                {
                    txtLevelDbJson.Text = rawLevelDb;
                }
            }
            else
            {
                txtLevelDbJson.Text = "暂无数据";
            }

            // 其余按AB侧分开显示（已排序A在前）
            var sbPanelInfo = new System.Text.StringBuilder();
            var sbVbJson = new System.Text.StringBuilder();
            var sbInferReturn = new System.Text.StringBuilder();
            var sbOther = new System.Text.StringBuilder();

            foreach (var info in debugInfos)
            {
                string sideLabel = $"===== {info.Side}面 =====";

                // PanelInfo JSON
                sbPanelInfo.AppendLine(sideLabel);
                if (!string.IsNullOrEmpty(info.PanelInfoJson))
                {
                    try
                    {
                        var formatted = Newtonsoft.Json.Linq.JToken.Parse(info.PanelInfoJson).ToString(Newtonsoft.Json.Formatting.Indented);
                        sbPanelInfo.AppendLine(formatted);
                    }
                    catch
                    {
                        sbPanelInfo.AppendLine(info.PanelInfoJson);
                    }
                }
                else
                {
                    sbPanelInfo.AppendLine("暂无数据");
                }
                sbPanelInfo.AppendLine();

                // VB Inference JSON
                sbVbJson.AppendLine(sideLabel);
                sbVbJson.AppendLine(!string.IsNullOrEmpty(info.VbInferenceJson) ? info.VbInferenceJson : "暂无数据");
                sbVbJson.AppendLine();

                // 推理返回JSON
                sbInferReturn.AppendLine(sideLabel);
                if (!string.IsNullOrEmpty(info.InferenceReturnJson))
                {
                    try
                    {
                        var formatted = Newtonsoft.Json.Linq.JToken.Parse(info.InferenceReturnJson).ToString(Newtonsoft.Json.Formatting.Indented);
                        sbInferReturn.AppendLine(formatted);
                    }
                    catch
                    {
                        sbInferReturn.AppendLine(info.InferenceReturnJson);
                    }
                }
                else
                {
                    sbInferReturn.AppendLine("暂无数据");
                }
                sbInferReturn.AppendLine();

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

                // 错误信息
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

            txtPanelInfoJson.Text = sbPanelInfo.ToString();
            txtVbInferenceJson.Text = sbVbJson.ToString();
            txtInferenceReturnJson.Text = sbInferReturn.ToString();
            txtOtherInfo.Text = sbOther.ToString();
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                var currentTab = tabControl.SelectedTab;
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
            if (currentTab?.Controls.Count > 0 && currentTab.Controls[0] is RichTextBox rtb)
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
                    // 从头再找一次
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
        }
    }
}


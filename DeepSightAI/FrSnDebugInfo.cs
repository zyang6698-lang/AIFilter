using DeepSightModel;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// SN调试信息显示窗口 - 双击任务队列时弹出
    /// </summary>
    public class FrSnDebugInfo : Form
    {
        private TabControl tabControl;
        private RichTextBox txtLevelDbJson;
        private RichTextBox txtPanelInfoJson;
        private RichTextBox txtVbInferenceJson;
        private RichTextBox txtInferenceReturnJson;
        private RichTextBox txtOtherInfo;

        // 搜索相关控件
        private Panel searchPanel;
        private TextBox txtSearch;
        private int _lastSearchIndex = 0;

        public FrSnDebugInfo(string sn, SnDebugInfo[] debugInfos)
        {
            InitializeComponents();
            Text = $"SN调试信息 - {sn}";
            LoadData(sn, debugInfos);
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.MaximizeBox = true;
            this.BackColor = Color.FromArgb(29, 48, 60);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Font = new Font("微软雅黑", 11F);
            this.KeyPreview = true;
            this.KeyDown += FrSnDebugInfo_KeyDown;

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 12F)
            };
            tabControl.SelectedIndexChanged += (s, ev) => _lastSearchIndex = 0;

            // Tab1: LevelDB JSON
            var tabLevelDb = new TabPage("LevelDB推理请求");
            txtLevelDbJson = CreateRichTextBox();
            tabLevelDb.Controls.Add(txtLevelDbJson);

            // Tab2: PanelInfo JSON
            var tabPanelInfo = new TabPage("PanelInfo JSON");
            txtPanelInfoJson = CreateRichTextBox();
            tabPanelInfo.Controls.Add(txtPanelInfoJson);

            // Tab3: VB Inference JSON
            var tabVbJson = new TabPage("VB推理JSON");
            txtVbInferenceJson = CreateRichTextBox();
            tabVbJson.Controls.Add(txtVbInferenceJson);

            // Tab4: Inference Return JSON
            var tabInferReturn = new TabPage("推理返回JSON");
            txtInferenceReturnJson = CreateRichTextBox();
            tabInferReturn.Controls.Add(txtInferenceReturnJson);

            // Tab5: Other Info
            var tabOther = new TabPage("其他信息");
            txtOtherInfo = CreateRichTextBox();
            tabOther.Controls.Add(txtOtherInfo);

            tabControl.TabPages.AddRange(new TabPage[] { tabLevelDb, tabPanelInfo, tabVbJson, tabInferReturn, tabOther });
            this.Controls.Add(tabControl);

            // 添加底部复制按钮
            var btnPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = Color.FromArgb(29, 48, 60)
            };

            var btnCopy = new Button
            {
                Text = "复制当前页内容",
                Size = new Size(180, 35),
                Location = new Point(10, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 80, 100),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 11F)
            };
            btnCopy.Click += BtnCopy_Click;

            var btnClose = new Button
            {
                Text = "关闭",
                Size = new Size(100, 35),
                Location = new Point(200, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 80, 100),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 11F)
            };
            btnClose.Click += (s, e) => this.Close();

            btnPanel.Controls.Add(btnCopy);
            btnPanel.Controls.Add(btnClose);
            this.Controls.Add(btnPanel);

            // 搜索面板（默认隐藏，Ctrl+F显示）
            searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(40, 60, 75),
                Visible = false
            };

            var lblSearch = new Label
            {
                Text = "搜索:",
                Location = new Point(10, 10),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 11F)
            };

            txtSearch = new TextBox
            {
                Location = new Point(70, 7),
                Size = new Size(350, 28),
                Font = new Font("微软雅黑", 11F),
                BackColor = Color.FromArgb(20, 35, 45),
                ForeColor = Color.White
            };
            txtSearch.KeyDown += TxtSearch_KeyDown;

            var btnFind = new Button
            {
                Text = "查找下一个",
                Location = new Point(430, 5),
                Size = new Size(120, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 80, 100),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10F)
            };
            btnFind.Click += (s, ev) => FindNext();

            var btnCloseSearch = new Button
            {
                Text = "✕",
                Location = new Point(560, 5),
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 80, 100),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10F)
            };
            btnCloseSearch.Click += (s, ev) => { searchPanel.Visible = false; };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(btnFind);
            searchPanel.Controls.Add(btnCloseSearch);
            this.Controls.Add(searchPanel);
        }

        private RichTextBox CreateRichTextBox()
        {
            return new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(20, 35, 45),
                ForeColor = Color.FromArgb(200, 220, 240),
                Font = new Font("Consolas", 12F),
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.Both
            };
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


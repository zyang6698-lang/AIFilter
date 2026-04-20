using System;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    /// <summary>
    /// 缺陷复判快捷键配置页面
    /// </summary>
    public partial class FrShortcutConfig : Sunny.UI.UIPage
    {
        public FrShortcutConfig()
        {
            InitializeComponent();
            PageIndex = 5;
            ShowTitle = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            // 为每个TextBox绑定按键捕获事件
            BindKeyCapture(txtVvsOk);
            BindKeyCapture(txtVvsNg);
            BindKeyCapture(txtVvsNotSet);
            BindKeyCapture(txtNextRow);
            BindKeyCapture(txtNextImage);
            BindKeyCapture(txtPrevImage);
            BindKeyCapture(txtNextPage);
            BindKeyCapture(txtPrevPage);
        }

        private static FrShortcutConfig _instance;

        public static FrShortcutConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrShortcutConfig();
                }
                return _instance;
            }
        }

        private void FrShortcutConfig_Load(object sender, EventArgs e)
        {
            LoadFromConfig();
        }

        /// <summary>
        /// 从配置加载快捷键到界面
        /// </summary>
        public void LoadFromConfig()
        {
            var cfg = Machine.sysConfig;
            if (cfg == null) return;

            txtVvsOk.Text = GetDisplayName(cfg.ShortcutVvsOk);
            txtVvsNg.Text = GetDisplayName(cfg.ShortcutVvsNg);
            txtVvsNotSet.Text = GetDisplayName(cfg.ShortcutVvsNotSet);
            txtNextRow.Text = GetDisplayName(cfg.ShortcutNextRow);
            txtNextImage.Text = GetDisplayName(cfg.ShortcutNextImage);
            txtPrevImage.Text = GetDisplayName(cfg.ShortcutPrevImage);
            txtNextPage.Text = GetDisplayName(cfg.ShortcutNextPage);
            txtPrevPage.Text = GetDisplayName(cfg.ShortcutPrevPage);
        }

        /// <summary>
        /// 将界面设置保存到配置对象
        /// </summary>
        public void SaveToConfig()
        {
            var cfg = Machine.sysConfig;
            if (cfg == null) return;

            cfg.ShortcutVvsOk = GetKeysName(txtVvsOk.Text);
            cfg.ShortcutVvsNg = GetKeysName(txtVvsNg.Text);
            cfg.ShortcutVvsNotSet = GetKeysName(txtVvsNotSet.Text);
            cfg.ShortcutNextRow = GetKeysName(txtNextRow.Text);
            cfg.ShortcutNextImage = GetKeysName(txtNextImage.Text);
            cfg.ShortcutPrevImage = GetKeysName(txtPrevImage.Text);
            cfg.ShortcutNextPage = GetKeysName(txtNextPage.Text);
            cfg.ShortcutPrevPage = GetKeysName(txtPrevPage.Text);
        }

        /// <summary>
        /// 为TextBox绑定按键捕获：用户按下按键后显示按键名
        /// </summary>
        private void BindKeyCapture(TextBox txt)
        {
            txt.KeyDown += (s, e) =>
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                Keys key = e.KeyCode;
                if (key == Keys.ShiftKey || key == Keys.ControlKey || key == Keys.Menu)
                    return;
                txt.Text = GetDisplayName(key.ToString());
            };

            // 阻止Tab键跳转焦点
            txt.PreviewKeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Tab)
                {
                    e.IsInputKey = true;
                }
            };
        }

        /// <summary>
        /// 将Keys枚举名转换为友好显示名
        /// </summary>
        private static string GetDisplayName(string keysName)
        {
            if (string.IsNullOrEmpty(keysName)) return "";
            // D0-D9 显示为 数字 0-9
            if (keysName.Length == 2 && keysName.StartsWith("D") && char.IsDigit(keysName[1]))
                return keysName[1].ToString();
            if (keysName.StartsWith("NumPad"))
                return "小键盘 " + keysName.Substring(6);
            switch (keysName)
            {
                case "Tab": return "Tab";
                case "Up": return "↑";
                case "Down": return "↓";
                case "Left": return "←";
                case "Right": return "→";
                default: return keysName;
            }
        }

        /// <summary>
        /// 将显示名转换回Keys枚举名
        /// </summary>
        private static string GetKeysName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return "";
            // 单个数字 → D+数字
            if (displayName.Length == 1 && char.IsDigit(displayName[0]))
                return "D" + displayName;
            if (displayName.StartsWith("小键盘 "))
                return "NumPad" + displayName.Substring(4);
            switch (displayName)
            {
                case "↑": return "Up";
                case "↓": return "Down";
                case "←": return "Left";
                case "→": return "Right";
                default: return displayName;
            }
        }
    }
}


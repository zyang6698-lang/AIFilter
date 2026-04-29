using System.Drawing;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    /// <summary>
    /// 工具箱配置页
    /// </summary>
    public class PgSettingToolbox : Sunny.UI.UIPage
    {
        private readonly UcStatisticsToolbox _toolbox;

        public PgSettingToolbox()
        {
            PageIndex = 6;
            ShowTitle = false;
            BackColor = Color.FromArgb(29, 48, 60);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            _toolbox = new UcStatisticsToolbox
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(_toolbox);
        }

        /// <summary>
        /// 窗体实例对象
        /// </summary>
        private static PgSettingToolbox _instance;

        public static PgSettingToolbox Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PgSettingToolbox();
                }

                return _instance;
            }
        }

        /// <summary>
        /// 触发工具箱中的"生成推理请求"功能（供菜单栏调用）
        /// </summary>
        public void TriggerGenerateInference()
        {
            _toolbox.TriggerGenerateInference();
        }
    }
}

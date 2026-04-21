using System.Drawing;
using System.Windows.Forms;

namespace DeepSightDisplay
{
    public partial class FrmFullScreen : Form
    {
        public FrmFullScreen()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲

            Rectangle rect = System.Windows.Forms.SystemInformation.VirtualScreen;
            Width = rect.Width;
            Height = rect.Height;
            cvDisplay1.ContextMenuStrip = null;
        }

        /// <summary>
        /// 窗体对象实例
        /// </summary>
        private static FrmFullScreen _instance;

        public static FrmFullScreen Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrmFullScreen();
                }

                return _instance;
            }
        }

        private void frFullScreen_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                cvDisplay1.Clear();
                Hide();
            }
        }
    }
}
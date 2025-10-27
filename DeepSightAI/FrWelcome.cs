using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DeepSightAI
{
    internal partial class FrWelcome : Form
    {
        internal FrWelcome()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            Application.DoEvents();
        }

        #region 窗体拖动

        private static bool IsDrag = false;
        private int enterX;
        private int enterY;

        private void setForm_MouseDown(object sender, MouseEventArgs e)
        {
            IsDrag = true;
            enterX = e.Location.X;
            enterY = e.Location.Y;
        }

        private void setForm_MouseUp(object sender, MouseEventArgs e)
        {
            IsDrag = false;
            enterX = 0;
            enterY = 0;
        }

        private void setForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDrag)
            {
                Left += e.Location.X - enterX;
                Top += e.Location.Y - enterY;
            }
        }

        #endregion 窗体拖动

        /// <summary>
        /// 窗体对象实例
        /// </summary>
        private static FrWelcome _instance;

        internal static FrWelcome Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrWelcome();
                }

                return _instance;
            }
        }

        private void Frm_Welcome_Load(object sender, EventArgs e)
        {
            lbl_version.Text = "V " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            bar_step.Maximum = 100;
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void FrWelcome_Shown(object sender, EventArgs e)
        {
            Machine.Init();
        }
    }
}
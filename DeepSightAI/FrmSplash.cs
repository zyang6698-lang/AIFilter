using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace DeepSightAI
{
    internal partial class FrmSplash : Form
    {
        internal FrmSplash()
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
        private static FrmSplash _instance;

        internal static FrmSplash Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrmSplash();
                }

                return _instance;
            }
        }

        private void Frm_Welcome_Load(object sender, EventArgs e)
        {
            lbl_version.Text = "V " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            bar_step.Maximum = 100;

            // 从配置加载欢迎页标题和字体大小
            if (Machine.sysConfig != null)
            {
                if (!string.IsNullOrEmpty(Machine.sysConfig.WelcomeTitle))
                {
                    lbl_title.Text = Machine.sysConfig.WelcomeTitle;
                }
                if (Machine.sysConfig.WelcomeFontSize > 0)
                {
                    lbl_title.Font = new Font("微软雅黑", Machine.sysConfig.WelcomeFontSize, FontStyle.Bold);
                }
            }
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void FrWelcome_Shown(object sender, EventArgs e)
        {
            Machine.Init();
        }

        /// <summary>
        /// 在欢迎界面上展示启动错误详情，展开窗体并保持界面可交互，不弹窗、不强制结束进程。
        /// </summary>
        /// <param name="errorDetail">需要展示的错误描述，支持多行</param>
        internal void ShowError(string errorDetail)
        {
            lbl_step.Text = "  ✖  启动出错，请查看以下错误信息";
            lbl_step.ForeColor = Color.OrangeRed;
            bar_step.Value = 0;
            lbl_error.Text = errorDetail;
            lbl_error.Visible = true;
            Height = 470;
            Application.DoEvents();
        }
    }
}
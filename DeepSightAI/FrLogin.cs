using DeepSightTool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class FrLogin : Form
    {
        public FrLogin()
        {
            InitializeComponent();
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
        private void FrLogin_Load(object sender, EventArgs e)
        {
            this.cmb_username.Items.AddRange(new List<string> { "操作员", "工程师", "供应商" }.ToArray());
            this.cmb_username.SelectedIndex = 0;
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            try
            {
                //注销登录的权限
                FrmMain.Instance.lbl_username.Text = "当前用户：未登录";
                Close();
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("异常", ex);
            }
        }
        private void btn_Login_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.tbx_password.Text.ToString()))
            {
                MessageBox.Show("登录密码不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (this.cmb_username.SelectedIndex == 0)
            {
                if (this.tbx_password.Text.ToString().Trim() != "123456")
                {
                    goto Label;
                }
                FrmMain.Instance.lbl_username.Text = "当前用户：操作员";
            }
            else if (this.cmb_username.SelectedIndex == 1)
            {
                if (this.tbx_password.Text.ToString().Trim() != "123456")
                {
                    goto Label;
                }
                FrmMain.Instance.lbl_username.Text = "当前用户：工程师";
            }
            else
            {
                //供应商
                if (this.tbx_password.Text.ToString().Trim() != "123456")
                {
                    goto Label;
                }
                FrmMain.Instance.lbl_username.Text = "当前用户：供应商";
            }
            MessageBox.Show("登录成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
            return;

        Label:
            this.tbx_password.Text = "";
            MessageBox.Show("密码错误!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

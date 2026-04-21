using DeepSightModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI.SettingPages
{
    public partial class FrmUserManagement : Form
    {

        //权限列表对象
        public List<SysAdmins> AdminsList = new List<SysAdmins>();
        //数据库对象
        //private DbClass dbClass = new DbClass();
        internal FrmUserManagement()
        {
            InitializeComponent();
            this.Load += FrUserManagement_Load;
            this.dgv_User.AutoGenerateColumns = false;
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
        }

        /// <summary>
        /// 窗体实例对象
        /// </summary>
        private static FrmUserManagement _instance;

        public static FrmUserManagement Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrmUserManagement();
                }
                return _instance;
            }
        }

        private void FrUserManagement_Load(object sender, EventArgs e)
        {
            BindAdminList();
            if (AdminsList.Count > 0)
            {
                SysAdmins objAdmin = AdminsList[0];
                this.txt_User.Text = objAdmin.LoginName;
                this.txt_Pwd.Text = objAdmin.LoginPwd;
                this.chk_AlarmCtrl.Checked = objAdmin.AlarmCtrl == 1 ? true : false;
                this.chk_DefectReportCtrl.Checked = objAdmin.DefectReportCtrl == 1 ? true : false;
                this.chk_AISet.Checked = objAdmin.DefectSet == 1 ? true : false;
                this.chk_FunctionSet.Checked = objAdmin.FunctionSet == 1 ? true : false;
                this.chk_GeneralSet.Checked = objAdmin.GeneralSet == 1 ? true : false;
                this.chk_ProSet.Checked = objAdmin.ProSet == 1 ? true : false;
                this.chk_UserManage.Checked = objAdmin.UserManage == 1 ? true : false;

            }
            this.txt_CurrentUser.Text = Machine.objAdmin.LoginName;
        }
        /// <summary>
        /// 绑定列表
        /// </summary>
        private void BindAdminList()
        {
            AdminsList = GetAdminList();
            this.dgv_User.DataSource = null;
            this.dgv_User.DataSource = AdminsList;

        }
        /// <summary>
        /// 获取列表对象
        /// </summary>
        /// <returns></returns>
        private List<SysAdmins> GetAdminList()
        {
            var list = new List<SysAdmins>(); //dbClass.dbSession.From<SysAdmins>().Where(d => d.LoginName != null).ToList();
            List<SysAdmins> AdminsList = new List<SysAdmins>();
            for (int i = 0; i < list.Count(); i++)
            {
                AdminsList.Add(new SysAdmins()
                {
                    LoginName = list[i].LoginName,
                    LoginPwd = list[i].LoginPwd,
                    AlarmCtrl = list[i].AlarmCtrl,
                    DefectReportCtrl = list[i].DefectReportCtrl,
                    DefectSet = list[i].DefectSet,
                    FunctionSet = list[i].FunctionSet,
                    GeneralSet = list[i].GeneralSet,
                    ProSet = list[i].ProSet,
                    UserManage = list[i].UserManage
                });
            }
            return AdminsList;

        }
        private void btn_Select_Click(object sender, EventArgs e)
        {

            if (this.btn_Select.Text == "全选")
            {
                foreach (Control c in groupBox2.Controls)
                {
                    if (c is CheckBox chk)
                    {
                        chk.Checked = true;
                    }
                }
                this.btn_Select.Text = "取消全选";
            }
            else
            {
                foreach (Control c in groupBox2.Controls)
                {
                    if (c is CheckBox chk)
                    {
                        chk.Checked = false;
                    }
                }
                this.btn_Select.Text = "全选";
            }
        }

        #region 点击用户列表事件
        private void dgv_User_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (this.dgv_User.SelectedRows.Count > 0)
            {
                int index = e == null ? 0 : e.RowIndex;
                if (index >= 0)
                {
                    SysAdmins objAdmin = AdminsList[index];
                    this.txt_User.Text = objAdmin.LoginName;
                    this.txt_Pwd.Text = objAdmin.LoginPwd;
                    this.chk_AlarmCtrl.Checked = objAdmin.AlarmCtrl == 1 ? true : false;
                    this.chk_DefectReportCtrl.Checked = objAdmin.DefectReportCtrl == 1 ? true : false;
                    this.chk_AISet.Checked = objAdmin.DefectSet == 1 ? true : false;
                    this.chk_FunctionSet.Checked = objAdmin.FunctionSet == 1 ? true : false;
                    this.chk_GeneralSet.Checked = objAdmin.GeneralSet == 1 ? true : false;
                    this.chk_ProSet.Checked = objAdmin.ProSet == 1 ? true : false;
                    this.chk_UserManage.Checked = objAdmin.UserManage == 1 ? true : false;
                }
            }
        }
        #endregion
        #region 窗体按钮
        private void btn_Add_Click(object sender, EventArgs e)
        {
            //先检查添加的用户是否存在
            if (LoginCheckNameExit(this.txt_User.Text.Trim()))
            {
                MessageBox.Show("用户添加提示\r\n用户已存在");
                return;
            }
            else
            {
                if (Machine.objAdmin.UserManage != 1)
                {
                    MessageBox.Show("用户添加提示\r\n当前登录用户无人员管理权限");
                    return;
                }
                else//判断当前用户是否有人员管理权限
                {
                    //封装用户对象

                    SysAdmins objadmin = new SysAdmins()
                    {
                        //构造
                        LoginName = this.txt_User.Text.Trim(),
                        LoginPwd = this.txt_Pwd.Text.Trim(),
                        AlarmCtrl = this.chk_AlarmCtrl.Checked ? 1 : 0,
                        DefectReportCtrl = this.chk_DefectReportCtrl.Checked ? 1 : 0,
                        DefectSet = this.chk_AISet.Checked ? 1 : 0,
                        FunctionSet = this.chk_FunctionSet.Checked ? 1 : 0,
                        GeneralSet = this.chk_GeneralSet.Checked ? 1 : 0,
                        ProSet = this.chk_ProSet.Checked ? 1 : 0,
                        UserManage = this.chk_UserManage.Checked ? 1 : 0,
                    };
                    if (AddUser(objadmin))
                    {
                        //MessageBox.Show("添加用户成功");
                        MessageBox.Show("添加用户成功");
                    }
                    else
                    {
                        //MessageBox.Show("添加用户失败");
                        MessageBox.Show("添加用户失败");
                        return;
                    }
                    BindAdminList();
                }

            }
        }


        private void btn_Delete_Click(object sender, EventArgs e)
        {
            //判断权限级别低不可以删
            if (this.txt_User.Text.Trim() == "管理员")
            {
                MessageBox.Show("管理员用户不能删除");
                return;
            }
            if (Machine.objAdmin.UserManage != 1)
            {
                MessageBox.Show("用户删除提示\r\n当前登录用户无人员管理权限");
                return;
            }
            
            SysAdmins objAdmins = new SysAdmins()
            {
                LoginName = this.txt_User.Text.Trim()
            };
            //DialogResult dr = MessageBox.Show("确认删除", "提示", MessageBoxButtons.OKCancel);
            //if (dr == DialogResult.Yes)
            //{
            if (DeleteUser(objAdmins))
            {
                MessageBox.Show("删除成功");
            }
            else
            {
                //MessageBox.Show("删除失败", "提示");
                MessageBox.Show("删除失败");
                return;
            }
            //}
            BindAdminList();
        }


        private void btn_Save_Click(object sender, EventArgs e)
        {
            if (this.txt_CurrentUser.Text != "管理员" && this.txt_User.Text == "管理员")
            {
                MessageBox.Show("权限等级不够\r\n不允许修改管理员权限");
                BindAdminList();
                return;
            }
            if (Machine.objAdmin.UserManage != 1)
            {
                MessageBox.Show("用户信息修改提示\r\n当前登录用户无人员管理权限");
                BindAdminList();
                return;
            }
            //修改并且保存权限
            //除了工程师以外的任意用户 首先获取当前点击的用户判断用户 是否存在
            if (!LoginCheckNameExit(this.txt_User.Text.Trim()))
            {
                MessageBox.Show("当前修改的用户名不存在\r\n请添加用户后再修改");
                return;
            }
            else
            {
                SysAdmins sysAdmins = new SysAdmins()
                {
                    LoginName = this.txt_User.Text.Trim(),
                    LoginPwd = this.txt_Pwd.Text.Trim(),
                    AlarmCtrl = this.chk_AlarmCtrl.Checked ? 1 : 0,
                    DefectReportCtrl = this.chk_DefectReportCtrl.Checked ? 1 : 0,
                    DefectSet = this.chk_AISet.Checked ? 1 : 0,
                    FunctionSet = this.chk_FunctionSet.Checked ? 1 : 0,
                    GeneralSet = this.chk_GeneralSet.Checked ? 1 : 0,
                    ProSet = this.chk_ProSet.Checked ? 1 : 0,
                    UserManage = this.chk_UserManage.Checked ? 1 : 0,
                };
                if (ChangeUserPower(sysAdmins))
                {
                    MessageBox.Show("权限修改成功");
                }
                else
                {
                    MessageBox.Show("权限修改失败");
                }
                BindAdminList();
            }
        }

        #region 判断要添加的用户是否存在
        private bool LoginCheckNameExit(string LoginName)
        {
            foreach (var item in AdminsList)
            {
                if (item.LoginName == LoginName)
                {
                    return true;//用户存在
                }
            }
            //不存在 
            return false;
        }
        #endregion
        #region 根据用户对象添加用户
        private bool AddUser(SysAdmins objAdmin)
        {
            return true;
            //DbTrans trans = dbClass.dbSession.BeginTransaction();
            //if (dbClass.dbSession.Insert<SysAdmins>(trans, objAdmin) > 0)
            //{
            //    trans.Commit();
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }
        #endregion
        #region 删除用户
        private bool DeleteUser(SysAdmins sysAdmins)
        {
            return true;
            //根据用户对象进行删除
            //DbTrans trans = dbClass.dbSession.BeginTransaction();
            //if (dbClass.dbSession.Delete<SysAdmins>(trans, sysAdmins) > 0)
            //{
            //    trans.Commit();
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }
        #endregion
        #region 用户修改权限
        private bool ChangeUserPower(SysAdmins sysAdmins)
        {
            return true;

            //DbTrans trans = dbClass.dbSession.BeginTransaction();
            //if (dbClass.dbSession.Update(trans, sysAdmins, d => d.LoginName == txt_User.Text.Trim()) == 1)
            //{
            //    trans.Commit();
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }
        #endregion

        #endregion

        
    }
}

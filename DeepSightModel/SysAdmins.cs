using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightModel
{
    //此类为用户权限管控实体类
    [Serializable]
    public class SysAdmins
    {
        #region Model
        /// <summary>
        /// 登录用户
        /// </summary>
        public string LoginName { get; set; }
        /// <summary>
        /// 登录密码
        /// </summary>
        public string LoginPwd { get; set; }
        /// <summary>
        /// 报警报表
        /// </summary>
        public int AlarmCtrl { get; set; }
        /// <summary>
        /// 缺陷报表
        /// </summary>
        public int DefectReportCtrl { get; set; }
        /// <summary>
        /// 常规设置
        /// </summary>
        public int GeneralSet { get; set; }
        /// <summary>
        /// 项目设置
        /// </summary>
        public int ProSet { get; set; }
        /// <summary>
        /// 功能设置
        /// </summary>
        public int FunctionSet { get; set; }
        /// <summary>
        /// 缺陷设定
        /// </summary>
        public int DefectSet;
        /// <summary>
        ///用户设定权限
        /// </summary>
        public int UserManage;
        #endregion
    }
}

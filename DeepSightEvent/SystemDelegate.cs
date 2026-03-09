using DeepSightModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightEvent
{
    /// <summary>
    /// 发送报警信息
    /// </summary>
    /// <param name="massage">报警信息param>
    public delegate void SendAlarm(string massage);
    /// <summary>
    /// 发送异常信息
    /// </summary>
    /// <param name="massage"></param>
    public delegate void SendException(string massage);
    /// <summary>
    /// 发送进度信息
    /// </summary>
    /// <param name="id">任务ID</param>
    /// <param name="isOK">任务当前进度  0:</param>
    public delegate void SendProcess(string id, int isOK);
    /// <summary>
    /// 发送任务状态信息（新版本，使用结构化模型）
    /// </summary>
    /// <param name="statusInfo">任务状态信息</param>
    public delegate void SendTaskStatus(TaskStatusInfo statusInfo);
    /// <summary>
    /// 发送任务信息
    /// </summary>
    /// <param name="task">具体任务</param>
    /// <param name="msg">消息</param>
    /// <param name="timeMs">AI处理时间(毫秒)</param>
    public delegate void SendTask(object task, string msg = "", long timeMs = 0);
    /// <summary>
    /// 发送缺陷小图个数
    /// </summary>
    /// <param name="num"></param>
    public delegate void SendDefectNum(int num);
    /// <summary>
    /// 发送缺陷信息
    /// </summary>
    /// <param name="sn">产品码</param>
    /// <param name="info">产品缺陷信息</param>
    public delegate void SendDefectPanelInfo(string sn, RootPanelInfoWithIP info);
    /// <summary>
    /// 发送缺陷结果
    /// </summary>
    /// <param name="sn">产品码</param>
    /// <param name="info">产品缺陷信息</param>
    public delegate void SendDefectResultInfo(string sn, List<string> msg);
    /// <summary>
    /// 发送推理后的缺陷ROI信息
    /// </summary>
    /// <param name="sn">产品码</param>
    /// <param name="rois">推理后的缺陷ROI列表</param>
    public delegate void SendDefectRoiInfo(string sn, List<Roi> rois);

}

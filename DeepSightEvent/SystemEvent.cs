using DeepSightModel;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightEvent
{
    //事件定义
    public class SystemEvent
    {
        public static event SendAlarm EventSendAlarmToUI;
        /// <summary>
        /// 订阅报警
        /// </summary>
        /// <param name="msg"></param>
        public static void SendAlarmMsg(string msg)
        {
            EventSendAlarmToUI?.Invoke(msg);
        }

        public static event SendProcess EventSendProcessToUI;
        /// <summary>
        /// 订阅进度
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isOk"></param>
        public static void SendProcessMsg(string id, int isOk)
        {
            EventSendProcessToUI?.Invoke(id, isOk);
        }

        public static event SendTask EventSendTaskToUI;
        /// <summary>
        /// 订阅任务
        /// </summary>
        /// <param name="task"></param>
        public static void SendTaskMsg(object task, string msg = "")
        {
            EventSendTaskToUI?.Invoke(task, msg);  
            LogTextHelper.Info($"{task} {msg}");
        }

        public static event SendException EventSendExceptionToUI;
        /// <summary>
        /// 订阅异常
        /// </summary>
        /// <param name="msg"></param>
        public static void SendException(string msg)
        {
            EventSendExceptionToUI?.Invoke(msg);
        }

        public static event SendDefectNum EventSendDefectNumToUI;
        /// <summary>
        ///订阅缺陷数
        /// </summary>
        /// <param name="num"></param>
        public static void SendDefectNum(int num)
        {
            EventSendDefectNumToUI?.Invoke(num);
        }

        public static event SendDefectPanelInfo EventSendDefectPanelInfoToUI;

        /// <summary>
        ///
        /// </summary>
        /// <param name="num"></param>
        public static void SendPanelInfo(string Sn, RootPanelInfoWithIP info)
        {
            if (EventSendDefectPanelInfoToUI != null)
            {
                EventSendDefectPanelInfoToUI(Sn, info);
            }
        }


        public static event SendDefectResultInfo EventSendDefectResultInfoToUI;
        /// <summary>
        ///
        /// </summary>
        /// <param name="num"></param>
        public static void SendResultInfo(string Sn, List<string>msg, List<string> details, PcsResult pcsResult)
        {
            if (EventSendDefectResultInfoToUI != null)
            {
                EventSendDefectResultInfoToUI(Sn, msg, details, pcsResult);
            }
        }
    }
}

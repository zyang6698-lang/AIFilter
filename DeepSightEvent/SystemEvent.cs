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

        public static event SendTaskStatus EventSendTaskStatusToUI;
        public static event SendTask EventSendTaskToUI;
        
        /// <summary>
        /// 发送任务状态（推荐使用，结构化方式）
        /// </summary>
        /// <param name="statusInfo">任务状态信息</param>
        public static void SendTaskStatus(TaskStatusInfo statusInfo)
        {
            var handler = EventSendTaskStatusToUI;
            if (handler != null)
            {
                try 
                { 
                    handler(statusInfo); 
                }
                catch (Exception ex) 
                { 
                    LogTextHelper.Error($"EventSendTaskStatusToUI handler error: {ex}"); 
                }
            }

            Task.Run(() =>
            {
                try { LogTextHelper.Info(statusInfo.ToString()); }
                catch { }
            });
        }
        
        /// <summary>
        /// 订阅任务（已过时，建议使用 SendTaskStatus）
        /// </summary>
        /// <param name="task"></param>
        /// <param name="msg"></param>
        /// <param name="timeMs">AI处理时间(毫秒)</param>
        public static void SendTaskMsg(object task, string msg = "", long timeMs = 0)
        {
            var handler = EventSendTaskToUI;
            if (handler != null)
            {
                Task.Run(() =>
                {
                    try { handler(task, msg, timeMs); }
                    catch (Exception ex) { LogTextHelper.Error($"EventSendTaskToUI handler error: {ex}"); }
                });
            }

            Task.Run(() =>
            {
                try { LogTextHelper.Info($"{task} {msg}"); }
                catch { }
            });
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

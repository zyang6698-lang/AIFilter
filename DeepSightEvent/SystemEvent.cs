using DeepSightDB;
using DeepSightModel;
using DeepSightModel.Alarm;
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
        /// 订阅报警（旧接口，自动桥接到 AlarmService）
        /// </summary>
        /// <param name="msg"></param>
        public static void SendAlarmMsg(string msg)
        {
            // 保留旧事件通知（向后兼容）
            EventSendAlarmToUI?.Invoke(msg);

            // 桥接到新告警系统
            AlarmService.Instance.RaiseAlarmFromLegacy(msg);
        }

        /// <summary>
        /// 发送结构化告警（推荐使用）
        /// </summary>
        public static void SendAlarm(AlarmLevel level, AlarmCategory category,
            string source, string message, string detail = null, string relatedSN = null)
        {
            AlarmService.Instance.RaiseAlarm(level, category, source, message, detail, relatedSN);
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
        /// 发送缺陷Panel信息到UI（以SN+Side为单位）
        /// </summary>
        /// <param name="sn">产品码</param>
        /// <param name="side">面别（A/B）</param>
        /// <param name="info">Panel信息</param>
        public static void SendPanelInfo(string sn, string side, RootPanelInfoWithIP info)
        {
            if (EventSendDefectPanelInfoToUI != null)
            {
                EventSendDefectPanelInfoToUI(sn, side, info);
            }
        }


        public static event SendDefectResultInfo EventSendDefectResultInfoToUI;
        /// <summary>
        /// 发送缺陷结果信息到UI（以SN+Side为单位）
        /// </summary>
        /// <param name="sn">产品码</param>
        /// <param name="side">面别（A/B）</param>
        /// <param name="msg">结果信息</param>
        public static void SendResultInfo(string sn, string side, List<string> msg)
        {
            if (EventSendDefectResultInfoToUI != null)
            {
                EventSendDefectResultInfoToUI(sn, side, msg);
            }
        }

        public static event SendDefectRoiInfo EventSendDefectRoiInfoToUI;
        /// <summary>
        /// 发送推理后的缺陷ROI信息到UI（以SN+Side为单位）
        /// </summary>
        /// <param name="sn">产品码</param>
        /// <param name="side">面别（A/B）</param>
        /// <param name="rois">推理后的缺陷ROI列表</param>
        public static void SendRoiInfo(string sn, string side, List<Roi> rois)
        {
            EventSendDefectRoiInfoToUI?.Invoke(sn, side, rois);
        }

        public static event SendDefectDetectInfo EventSendDefectDetectInfoToUI;
        /// <summary>
        /// 发送推理后的缺陷DetectInfo信息到UI（用于图片放大和单图测试）
        /// </summary>
        /// <param name="sn">产品码</param>
        /// <param name="side">面别（A/B）</param>
        /// <param name="detectInfos">推理后的DetectInfo列表</param>
        public static void SendDetectInfo(string sn, string side, List<DetectInfo> detectInfos)
        {
            EventSendDefectDetectInfoToUI?.Invoke(sn, side, detectInfos);
        }
    }
}

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
        public static void SendPanelInfo(string sn, string side, PanelInfoView info)
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

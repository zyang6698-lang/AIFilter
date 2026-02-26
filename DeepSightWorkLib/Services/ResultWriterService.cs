using DeepSightCommunication;
using DeepSightDB;
using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using System;
using System.Collections.Concurrent;

namespace DeepSightWorkLib.Services
{
    public class ResultWriterService
    {
        private readonly HttpClass _httpDb;
        private readonly ConcurrentDictionary<string, DateTime> _processingSnSet;

        public ResultWriterService(HttpClass httpDb, ConcurrentDictionary<string, DateTime> processingSnSet)
        {
            _httpDb = httpDb ?? throw new ArgumentNullException(nameof(httpDb));
            _processingSnSet = processingSnSet ?? throw new ArgumentNullException(nameof(processingSnSet));
        }

        public bool ReturnAVI(Tuple<string, string, string, RootAIResult> info)
        {
            try
            {
                // 优先使用 RootAIResult 中携带的目标 URL，退回到默认 _url
                var targetUrl = !string.IsNullOrEmpty(info.Item4?.TargetUrl) ? info.Item4.TargetUrl :"";
                TaskStatusSender.SendWritingResults(info.Item2, info.Item3);
                LogTextHelper.Info($"SN:{info.Item2} Side:{info.Item3} 回写到 URL:{targetUrl}, DB:{info.Item4?.DbName}");
                if (_httpDb.HttpPostMethod(targetUrl, info.Item4, 1, out string result))
                {
                    TaskStatusSender.SendCompleted(info.Item2, info.Item3);

                    string snKey = $"{info.Item2}_{info.Item3}";
                    if (_processingSnSet.TryRemove(snKey, out DateTime addTime))
                    {
                        var duration = DateTime.Now - addTime;
                        LogTextHelper.Info($"SN:{info.Item2} Side:{info.Item3} 处理完成，耗时：{duration.TotalSeconds:F2}秒，已从处理集合中移除");
                    }
                    else
                    {
                        LogTextHelper.Warn($"SN:{info.Item2} Side:{info.Item3} 未在处理集合中找到，可能已被清理或未正确添加");
                    }

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex.ToString());
                return false;
            }
        }
    }
}

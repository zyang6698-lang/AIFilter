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
        private readonly string _url;
        private readonly string _dsCenterUrl;

        public ResultWriterService(HttpClass httpDb, ConcurrentDictionary<string, DateTime> processingSnSet, string url, string dsCenterUrl)
        {
            _httpDb = httpDb ?? throw new ArgumentNullException(nameof(httpDb));
            _processingSnSet = processingSnSet ?? throw new ArgumentNullException(nameof(processingSnSet));
            _url = url;
            _dsCenterUrl = dsCenterUrl;
        }

        public bool ReturnAVI(Tuple<string, string, string, RootAIResult, DsCenterInfo> info)
        {
            try
            {
                TaskStatusSender.SendWritingResults(info.Item2, info.Item3);
                if (_httpDb.HttpPostMethod(_url, info.Item4, 1, out string result))
                {
                    TaskStatusSender.SendCompleted(info.Item2, info.Item3);

                    string snKey = $"{info.Item2}_{info.Item3}";
                    if (_processingSnSet.TryRemove(snKey, out DateTime addTime))
                    {
                        var duration = DateTime.Now - addTime;
                        LogTextHelper.Info($"SN:{info.Item2} Side:{info.Item3} 处理完成，用时：{duration.TotalSeconds:F2}秒，已从处理集合中移除");
                    }
                    else
                    {
                        LogTextHelper.Warn($"SN:{info.Item2} Side:{info.Item3} 未在处理集合中找到，可能已被清理或未正确添加");
                    }

                    if (info.Item5 != null)
                    {
                        _httpDb.HttpPostMethod2(_dsCenterUrl, info.Item5, 0, out string outInfo);
                        LogTextHelper.Info($"sn:{info.Item2}_中台数据发送，信息:{outInfo}");
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

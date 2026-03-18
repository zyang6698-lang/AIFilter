using DeepSightCommunication;
using DeepSightDB;
using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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

        public bool ReturnAVI(Tuple<List<AIDetailResultItem>, RootAIResult> info)
        {
            try
            {
                var aiDetailResults= info.Item1;
                var aiResult = info.Item2;

                // 优先使用 RootAIResult 中携带的目标 URL，退回到默认 _url
                var targetUrl = !string.IsNullOrEmpty(aiResult?.TargetUrl) ? aiResult.TargetUrl :"";
                TaskStatusSender.SendWritingResults(aiResult.SN, aiResult.Side);
                LogTextHelper.Info($"SN:{aiResult.SN} Side:{aiResult.Side} 回写到 URL:{targetUrl}, DB:{aiResult?.DbName}");
                if (_httpDb.HttpPostMethod(targetUrl, aiResult, 1, out string result))
                {
                    TaskStatusSender.SendCompleted(aiResult.SN, aiResult.Side);

                    string snKey = $"{aiResult.SN}_{aiResult.Side}";
                    if (_processingSnSet.TryRemove(snKey, out DateTime addTime))
                    {
                        var duration = DateTime.Now - addTime;
                        LogTextHelper.Info($"SN:{aiResult.SN} Side:{aiResult.Side} 处理完成，耗时：{duration.TotalSeconds:F2}秒，已从处理集合中移除");
                    }
                    else
                    {
                        LogTextHelper.Warn($"SN:{aiResult.SN} Side:{aiResult.Side} 未在处理集合中找到，可能已被清理或未正确添加");
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
        // TODO 完善VRS接口
        public bool ReturnVRS(List<AIDetailResultItem> aIDetailResultItems)
        {
            var targetUrl = "";
            var data = JsonConvert.SerializeObject(aIDetailResultItems);
            //var data = new RootAIResult
            //{
            //    DbName = "ai_detail_results_tovrs",
            //    Operation = "put",
            //    OpMode = "all_ow",
            //    Key = $"{sn}_{side}",
            //    Value = JsonConvert.SerializeObject(aIDetailResultItems),
            //};
            if (_httpDb.HttpPostMethod(targetUrl, data, 1, out string result))
            {

                return true;
            }
            return false;
        }
    }
}

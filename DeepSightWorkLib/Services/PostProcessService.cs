using DeepSightDB;
using DeepSightEvent;
using DeepSightModel;
using DeepSightModel.Alarm;
using DeepSightModel.Configuration;
using DeepSightTool;
using DeepSightWorkLib.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 负责推理结果的后处理逻辑（保存板面数据到数据库）
    /// 通过外部注入的 delegate 将 PanelSide 保存到 BusinessClass 的数据库操作逻辑，避免直接依赖 BusinessClass。
    /// </summary>
    public class PostProcessService
    {
        private readonly ConfigurationClass _sysConfig;
        private readonly Action<VBModel, List<DetectInfo>, int, int> _savePanelSideAction;
        private readonly OfflineInferenceResultSinkService _offlineResultSinkService;

        // 重点缺陷报警冷却时间记录
        private static DateTime _lastKeyDefectAlarmTime = DateTime.MinValue;
        private static readonly object _alarmLock = new object();

        public PostProcessService(
            ConfigurationClass sysConfig,
            Action<VBModel, List<DetectInfo>, int, int> savePanelSideAction)
        {
            _sysConfig = sysConfig ?? throw new ArgumentNullException(nameof(sysConfig));
            _savePanelSideAction = savePanelSideAction ?? throw new ArgumentNullException(nameof(savePanelSideAction));
            _offlineResultSinkService = new OfflineInferenceResultSinkService();
        }

        /// <summary>
        /// 设置验证测试服务（用于处理测试推理结果）
        /// </summary>
        public void SetValidationTestService(ModelValidationTestService service)
        {
            _offlineResultSinkService.SetValidationTestService(service);
        }

        public void ProcessInferenceResult(InferenceResultModel resultModel)
        {
            VBModel vBModel = resultModel.VBModel;
            string msg = resultModel.RawJsonResult;

            try
            {
                // 如果是验证测试任务，交给验证测试服务处理
                if (vBModel.IsValidationTest)
                {
                    _offlineResultSinkService.Process(vBModel, msg);
                    return;
                }

                // 检查是否需要处理（图片数量为0或超过最大值）
                if (!resultModel.NeedsProcessing)
                {
                    if (vBModel.Mats == null || vBModel.Mats.Count == 0)
                    {
                        // 即使无图片也保存所有缺陷的图片路径（包含直报缺陷）
                        var allDefects = BuildAllDefectsWithPaths(vBModel);
                        foreach (var defect in allDefects)
                        {
                            if (IsAiStatusDirectReport(defect))
                                defect.AIStatus = 4;
                            else
                                defect.AIStatus = 3;
                        }

                        int skipAviState;
                        int skipAiState;
                        if (allDefects.Count == 0)
                        {
                            // 真的没有缺陷
                            skipAviState = 1;
                            skipAiState = 1;
                        }
                        else
                        {
                            skipAviState = 2;
                            skipAiState = CalculateAiState(allDefects);
                        }

                        _savePanelSideAction(vBModel, allDefects, skipAviState, skipAiState);
                        LogTextHelper.Info($"跳过处理(无图片): SN={vBModel.SN}, 保存缺陷路径数={allDefects.Count}, AviState={skipAviState}, AiState={skipAiState}");
                    }
                    else if (vBModel.Mats.Count > _sysConfig.MaxDefectCount)
                    {
                        var allDefects = BuildAllDefectsWithPaths(vBModel);
                        foreach (var d in allDefects) d.AIStatus = IsAiStatusDirectReport(d) ? 4 : 3;
                        _savePanelSideAction(vBModel, allDefects, 2, CalculateAiState(allDefects));
                        LogTextHelper.Info($"跳过处理(图片超限): SN={vBModel.SN}, 保存缺陷路径数={allDefects.Count}");
                    }
                    return;
                }

                LogTextHelper.Info($"开始处理: SN={vBModel.SN}, Side={vBModel.Side}");

                // 从 VBModel 中已构建好的全量缺陷列表读取完整缺陷信息（包含直报缺陷）
                List<DetectInfo> defects = BuildAllDefectsWithPaths(vBModel);

                // 自动发现 AVI 上报的 DefectCode
                if (defects.Count > 0)
                {
                    var profileName = KeyDefectConfigManager.Instance.GetProfileNameForProduct(vBModel.ProductSerial);
                    foreach (var defectCode in defects.Select(d => !string.IsNullOrWhiteSpace(d.OriginDefectName) ? d.OriginDefectName : d.DefectName))
                    {
                        if (!string.IsNullOrEmpty(defectCode))
                            KeyDefectConfigManager.Instance.AutoDiscoverDefect(defectCode, profileName);
                    }
                }

                var obj = JsonConvert.DeserializeObject<RootVBOutInfo>(msg);
                if (obj == null)
                {
                    LogTextHelper.Error($"算法返回结果反序列化失败 for Side {vBModel.Side}，原始消息: {msg}");
                    return;
                }

                string code = obj.Code.ToString();
                string message = obj.Message.ToString();

                int directReportCount = defects.Count(IsAiStatusDirectReport);
                int inferableCount = defects.Count - directReportCount;

                int aviState = defects.Count == 0 ? 1 : 2;
                int aiState = 3;

                if (code == "200")
                {
                    var inferResults = obj.Data.InferWholeData.InferResults;
                    int inferIdx = 0; // AI推理结果的索引（仅对应非直报缺陷）

                    for (int i = 0; i < defects.Count; i++)
                    {
                        var defect = defects[i];

                        // 直报缺陷：AIStatus=4，跳过AI结果映射
                        bool isDirectReport = IsAiStatusDirectReport(defect);
                        if (isDirectReport) continue;

                        // 非直报缺陷：映射 AI 推理结果
                        if (inferIdx >= inferResults.Count)
                        {
                            // AI返回结果不够，剩余非直报缺陷标记为未知
                            defect.AIStatus = 3;
                            inferIdx++;
                            continue;
                        }

                        var inferResult = inferResults[inferIdx];

                        var imgRoi = inferResult.ImgRoi;
                        if (imgRoi != null && imgRoi.Count >= 4)
                        {
                            defect.OriginRoiX = imgRoi[0];
                            defect.OriginRoiY = imgRoi[1];
                            defect.OriginWidth = imgRoi[2];
                            defect.OriginHeight = imgRoi[3];
                        }

                        if (inferResult.Infer_Result == "OK")
                        {
                            defect.AIStatus = 1;
                        }
                        else
                        {
                            defect.AIStatus = 2;
                            //todo需要判空
                            for (int j = 0; j < inferResult.InferDetails.Location.Count; j++)
                            {
                                string sub_defectName = inferResult.Defect_name;
                                int subX = Convert.ToInt32(inferResult.InferDetails.Location[j].X);
                                int subY = Convert.ToInt32(inferResult.InferDetails.Location[j].Y);
                                int subH = Convert.ToInt32(inferResult.InferDetails.Location[j].Height);
                                int subW = Convert.ToInt32(inferResult.InferDetails.Location[j].Width);

                                defect.DefectName = sub_defectName;
                                defect.RoiX = subX;
                                defect.RoiY = subY;
                                defect.Width = subW;
                                defect.Height = subH;

                                defect.DrawInfo = JsonConvert.SerializeObject(inferResult.InferDetails.DrawInfoList);

                                if (sub_defectName == "AU10" || sub_defectName == "CU10" || sub_defectName == "CU41"
                                    || sub_defectName == "HO01" || sub_defectName == "SM10")
                                {
                                    defect.DefectShape = "dot";
                                }
                                else
                                {
                                    defect.DefectShape = "line";
                                }
                            }
                        }

                        // 重点缺陷标记 & 自动发现（根据料号对应的 profile）
                        if (defect.AIStatus == 2 && !string.IsNullOrEmpty(defect.DefectName))
                        {
                            var profileName = KeyDefectConfigManager.Instance.GetProfileNameForProduct(vBModel.ProductSerial);
                            KeyDefectConfigManager.Instance.AutoDiscoverDefect(defect.DefectName, profileName);
                            if (KeyDefectConfigManager.Instance.IsKeyDefect(defect.DefectName, profileName))
                            {
                                defect.IsKeyDefect = true;
                            }
                        }

                        inferIdx++;
                    }

                    aiState = CalculateAiState(defects);

                    int keyDefectInThisSide = defects.Count(h => h.IsKeyDefect);
                    if (keyDefectInThisSide > 0)
                    {
                        CheckKeyDefectAlarm(vBModel.SN);
                    }

                    LogTextHelper.Info($"处理完成: SN={vBModel.SN}, Side={vBModel.Side}, " +
                        $"总缺陷={defects.Count}, 直报={directReportCount}, AI推理={inferableCount}, " +
                        $"AviState={aviState}, AiState={aiState}, KeyDefects={keyDefectInThisSide}");
                }
                else if (code == "600")
                {
                    // AI无缺陷（code=600），但直报缺陷依然需要保存
                    for (int i = 0; i < defects.Count; i++)
                    {
                        var defect = defects[i];
                        bool isDirectReport = IsAiStatusDirectReport(defect);
                        if (!isDirectReport)
                        {
                            defect.AIStatus = 1;
                        }
                        else
                        {
                            defect.AIStatus = 4;
                        }
                    }

                    if (directReportCount > 0)
                    {
                        aviState = 2;
                        aiState = CalculateAiState(defects);
                        LogTextHelper.Info($"处理完成(AI无缺陷,有{directReportCount}个直报): SN={vBModel.SN}, Side={vBModel.Side}");
                    }
                    else
                    {
                        aviState = 1;
                        aiState = 1;
                        LogTextHelper.Info($"处理完成(无缺陷): SN={vBModel.SN}, Side={vBModel.Side}");
                    }
                }
                else
                {
                    LogTextHelper.Warn($"算法处理失败 for Side {vBModel.Side}，错误码: {code}，错误信息：{message}");
                }

                // 不论什么情况都保存（所有缺陷包含完整图片路径）
                _savePanelSideAction(vBModel, defects, aviState, aiState);

            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"处理异常: SN={vBModel.SN}, 错误={ex}");
            }
        }

        /// <summary>
        /// 从 VBModel 中获取包含所有缺陷（含直报）的 DetectInfo 列表。
        /// 直报缺陷统一为 AIStatus=4，非直报缺陷 AIStatus 默认为 0（后续由推理结果填充）。
        /// </summary>
        private List<DetectInfo> BuildAllDefectsWithPaths(VBModel vBModel)
        {
            var defects = vBModel.AllDefectInfos?
                .Select(d => d?.Clone() ?? new DetectInfo())
                .ToList() ?? new List<DetectInfo>();

            return defects;
        }

        private static bool IsAiStatusDirectReport(DetectInfo defect)
        {
            return defect != null && defect.AIStatus == 4;
        }

        private static int CalculateAiState(List<DetectInfo> defects)
        {
            if (defects == null || defects.Count == 0) return 1;
            if (defects.Any(d => d.AIStatus == 3)) return 3;
            if (defects.Any(d => d.AIStatus == 2 || IsAiStatusDirectReport(d))) return 2;
            if (defects.All(d => d.AIStatus == 1)) return 1;
            return 3;
        }

        /// <summary>
        /// 检查重点缺陷报警条件（基于当日统计）
        /// </summary>
        private void CheckKeyDefectAlarm(string currentSN)
        {
            try
            {
                var alarmConfig = KeyDefectConfigManager.Instance.GetAlarmConfig();
                if (!alarmConfig.Enabled) return;

                var todayStat = BoardStatCache.GetTodayStat();
                int totalDefects = todayStat.AiFilterCount - todayStat.AiFilterOKCount; // NG缺陷总数
                int keyDefects = todayStat.KeyDefectCount;

                if (totalDefects <= 0 || keyDefects <= 0) return;

                bool shouldAlarm = false;
                string reason = "";

                // 比例报警
                double ratio = (double)keyDefects / totalDefects;
                if (alarmConfig.AlarmRatioThreshold > 0 && ratio >= alarmConfig.AlarmRatioThreshold)
                {
                    shouldAlarm = true;
                    reason = $"重点缺陷占比 {ratio:P1} 超过阈值 {alarmConfig.AlarmRatioThreshold:P1}";
                }

                // 绝对数量报警
                if (alarmConfig.AlarmCountThreshold > 0 && keyDefects >= alarmConfig.AlarmCountThreshold)
                {
                    shouldAlarm = true;
                    reason = $"重点缺陷数量 {keyDefects} 超过阈值 {alarmConfig.AlarmCountThreshold}";
                }

                if (!shouldAlarm) return;

                // 冷却检查
                lock (_alarmLock)
                {
                    if ((DateTime.Now - _lastKeyDefectAlarmTime).TotalSeconds < alarmConfig.AlarmCooldownSeconds)
                        return;
                    _lastKeyDefectAlarmTime = DateTime.Now;
                }

                AlarmService.Instance.RaiseAlarm(
                    AlarmLevel.Warning,
                    AlarmCategory.Defect,
                    "PostProcessService.KeyDefect",
                    $"[重点缺陷报警] {reason}",
                    $"当前SN={currentSN}, 今日重点缺陷={keyDefects}, NG总数={totalDefects}",
                    currentSN,
                    code: "KEY_DEFECT_THRESHOLD_TRIGGERED");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"重点缺陷报警检查异常: {ex.Message}");
            }
        }
    }
}

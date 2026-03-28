using DeepSightDB;
using DeepSightEvent;
using DeepSightModel;
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
        private readonly Action<RootPanelInfo, List<DetectInfo>, int, int> _savePanelSideAction;

        // 模型验证测试服务（可选注入）
        private ModelValidationTestService _validationTestService;

        // 重点缺陷报警冷却时间记录
        private static DateTime _lastKeyDefectAlarmTime = DateTime.MinValue;
        private static readonly object _alarmLock = new object();

        public PostProcessService(
            ConfigurationClass sysConfig,
            Action<RootPanelInfo, List<DetectInfo>, int, int> savePanelSideAction)
        {
            _sysConfig = sysConfig ?? throw new ArgumentNullException(nameof(sysConfig));
            _savePanelSideAction = savePanelSideAction ?? throw new ArgumentNullException(nameof(savePanelSideAction));
        }

        /// <summary>
        /// 设置验证测试服务（用于处理测试推理结果）
        /// </summary>
        public void SetValidationTestService(ModelValidationTestService service)
        {
            _validationTestService = service;
        }

        public void ProcessInferenceResult(InferenceResultModel resultModel)
        {
            VBModel vBModel = resultModel.VBModel;
            string msg = resultModel.RawJsonResult;
            RootPanelInfo panelInfo = vBModel.panelInfo;

            try
            {
                // 如果是验证测试任务，交给验证测试服务处理
                if (vBModel.IsValidationTest)
                {
                    ProcessValidationTestResult(vBModel, msg);
                    return;
                }

                // 检查是否需要处理（图片数量为0或超过最大值）
                if (!resultModel.NeedsProcessing)
                {
                    if (vBModel.Mats == null || vBModel.Mats.Count == 0)
                    {
                        _savePanelSideAction(panelInfo, new List<DetectInfo>(), 1, 1);
                        LogTextHelper.Info($"跳过处理(无图片): SN={vBModel.SN}");
                    }
                    else if (vBModel.Mats.Count > _sysConfig.MaxDefectCount)
                    {
                        var imageKeys = vBModel.ImageKeys.Select(t => new DetectInfo() { ImagePath = t, AIStatus = 3 }).ToList();
                        _savePanelSideAction(panelInfo, imageKeys, 2, 3);
                        LogTextHelper.Info($"跳过处理(图片超限): SN={vBModel.SN}");
                    }
                    return;
                }

                LogTextHelper.Info($"开始处理: SN={vBModel.SN}, Side={panelInfo.SideIndex}");

                // 从 PanelInfo 中自动发现缺陷名称（AVI 上报的 DefectCode）
                if (panelInfo.PcsInfo != null)
                {
                    foreach (var pcsEntry in panelInfo.PcsInfo.Values)
                    {
                        if (pcsEntry?.DefectInfo == null) continue;
                        foreach (var defect in pcsEntry.DefectInfo)
                        {
                            if (!string.IsNullOrEmpty(defect.DefectCode))
                                KeyDefectConfigManager.Instance.AutoDiscoverDefect(defect.DefectCode, KeyDefectConfigManager.Instance.GetProfileNameForProduct(panelInfo.ProductSerial));
                        }
                    }
                }

                var obj = JsonConvert.DeserializeObject<RootVBOutInfo>(msg);
                if (obj == null)
                {
                    LogTextHelper.Error($"算法返回结果反序列化失败 for Side {panelInfo.SideIndex}，原始消息: {msg}");
                    return;
                }

                string code = obj.Code.ToString();
                string message = obj.Message.ToString();

                if (code == "200")
                {
                    List<DetectInfo> defects = new List<DetectInfo>();

                    // 构建 HeatPoint 点信息
                    for (int i = 0; i < obj.Data.InferWholeData.InferResults.Count; i++)
                    {
                        DetectInfo defect = new DetectInfo();
                        if (vBModel.ImageKeys[i]!=null)
                        {
                            defect.ImagePath = vBModel.ImageKeys[i];
                        }
                        if (vBModel.ImageKeys_Temp[i]!=null)
                        {
                            defect.TempImagePath=vBModel.ImageKeys_Temp[i]; 
                        }
                        if (vBModel.ImageKeys_Gerber[i]!=null)
                        {
                            defect.GerberImagePath = vBModel.ImageKeys_Gerber[i];
                        }
                        var imgRoi = obj.Data.InferWholeData.InferResults[i].ImgRoi;
                        if (imgRoi != null && imgRoi.Count >= 4)
                        {
                            defect.OriginRoiX = imgRoi[0];
                            defect.OriginRoiY = imgRoi[1];
                            defect.OriginWidth = imgRoi[2];
                            defect.OriginHeight = imgRoi[3];
                        }

                        if (obj.Data.InferWholeData.InferResults[i].Infer_Result == "OK")
                        {
                            defect.AIStatus = 1;
                        }
                        else
                        {
                            defect.AIStatus = 2;

                            for (int j = 0; j < obj.Data.InferWholeData.InferResults[i].InferDetails.Location.Count; j++)
                            {
                                string sub_defectName = obj.Data.InferWholeData.InferResults[i].Defect_name;
                                int subX = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].InferDetails.Location[j].X);
                                int subY = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].InferDetails.Location[j].Y);
                                int subH = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].InferDetails.Location[j].Height);
                                int subW = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].InferDetails.Location[j].Width);

                                defect.DefectName = sub_defectName;
                                defect.RoiX = subX;
                                defect.RoiY = subY;
                                defect.Width = subW;
                                defect.Height = subH;

                                defect.DrawInfo =JsonConvert.SerializeObject( obj.Data.InferWholeData.InferResults[i].InferDetails.DrawInfoList);
                                
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
                            var profileName = KeyDefectConfigManager.Instance.GetProfileNameForProduct(panelInfo.ProductSerial);
                            KeyDefectConfigManager.Instance.AutoDiscoverDefect(defect.DefectName, profileName);
                            if (KeyDefectConfigManager.Instance.IsKeyDefect(defect.DefectName, profileName))
                            {
                                defect.IsKeyDefect = true;
                            }
                        }

                        defects.Add(defect);
                    }

                    // 追加直报缺陷（AIStatus=3，跳过了AI推理）
                    if (vBModel.DirectReportDefectIndices != null && vBModel.DirectReportDefectIndices.Count > 0)
                    {
                        for (int i = 0; i < vBModel.DirectReportDefectIndices.Count; i++)
                        {
                            var directReportInfo = new DetectInfo
                            {
                                AIStatus = 3
                            };
                            defects.Add(directReportInfo);
                        }
                        LogTextHelper.Info($"SN:{vBModel.SN} 追加 {vBModel.DirectReportDefectIndices.Count} 个直报缺陷到后处理结果");
                    }

                    // 保存数据到数据库
                    int aviState = defects.Count == 0 ? 1 : 2;
                    int aiState = defects.Any(h => h.AIStatus == 3) ? 3 : defects.Any(h => h.AIStatus == 2) ? 2 : 1;
                    _savePanelSideAction(panelInfo, defects, aviState, aiState);

                    // 重点缺陷报警检查
                    int keyDefectInThisSide = defects.Count(h => h.IsKeyDefect);
                    if (keyDefectInThisSide > 0)
                    {
                        CheckKeyDefectAlarm(vBModel.SN);
                    }

                    LogTextHelper.Info($"处理完成: SN={vBModel.SN}, Side={panelInfo.SideIndex}, AviState={aviState}, AiState={aiState}, KeyDefects={keyDefectInThisSide}");
                }
                else if (code == "600")
                {
                    // 无缺陷但可能有直报缺陷
                    var directReportList = new List<DetectInfo>();
                    if (vBModel.DirectReportDefectIndices != null && vBModel.DirectReportDefectIndices.Count > 0)
                    {
                        for (int i = 0; i < vBModel.DirectReportDefectIndices.Count; i++)
                        {
                            directReportList.Add(new DetectInfo { AIStatus = 3 });
                        }
                        int aviState600 = 2;
                        int aiState600 = 3;
                        _savePanelSideAction(panelInfo, directReportList, aviState600, aiState600);
                        LogTextHelper.Info($"处理完成(AI无缺陷,有{directReportList.Count}个直报): SN={vBModel.SN}, Side={panelInfo.SideIndex}");
                    }
                    else
                    {
                        _savePanelSideAction(panelInfo, new List<DetectInfo>(), 1, 1);
                        LogTextHelper.Info($"处理完成(无缺陷): SN={vBModel.SN}, Side={panelInfo.SideIndex}");
                    }
                }
                else
                {
                    LogTextHelper.Warn($"算法处理失败 for Side {panelInfo.SideIndex}，错误码: {code}，错误信息：{message}");
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"处理异常: SN={vBModel.SN}, 错误={ex}");
            }
        }

        /// <summary>
        /// 处理验证测试的推理结果
        /// </summary>
        private void ProcessValidationTestResult(VBModel vBModel, string rawJsonResult)
        {
            if (_validationTestService == null)
            {
                LogTextHelper.Warn($"验证测试服务未注入，跳过测试结果处理: {vBModel.SN}_{vBModel.Side}");
                return;
            }

            try
            {
                // 解析推理结果
                var inferResults = ExtractInferResults(rawJsonResult);

                // 根据任务类型调用不同的处理方法
                if (vBModel.IsSingleImageTest)
                {
                    _validationTestService.ProcessSingleImageTestResult(vBModel, inferResults);
                    LogTextHelper.Info($"单图测试结果处理完成: {vBModel.SN}");
                }
                else if (vBModel.IsSecondaryInference)
                {
                    // 二次推理：异步处理并更新数据库
                    _ = _validationTestService.ProcessSecondaryInferenceResultAsync(vBModel, inferResults);
                    LogTextHelper.Info($"二次推理结果处理已启动: {vBModel.SN}_{vBModel.Side}");
                }
                else
                {
                    _validationTestService.ProcessValidationTestResult(vBModel, inferResults);
                    LogTextHelper.Info($"验证测试结果处理完成: {vBModel.SN}_{vBModel.Side}");
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"处理验证测试结果异常: {vBModel.SN}_{vBModel.Side}, {ex}");
            }
        }

        /// <summary>
        /// 从原始JSON结果中提取推理结果列表
        /// </summary>
        private List<string> ExtractInferResults(string rawJsonResult)
        {
            var results = new List<string>();

            if (string.IsNullOrEmpty(rawJsonResult))
                return results;

            try
            {
                var obj = JsonConvert.DeserializeObject<RootVBOutInfo>(rawJsonResult);
                if (obj?.Code?.ToString() == "200" && obj.Data?.InferWholeData?.InferResults != null)
                {
                    foreach (var inferResult in obj.Data.InferWholeData.InferResults)
                    {
                        // "OK" -> "0", "NG" -> "1"
                        results.Add(inferResult.Infer_Result == "NG" ? "1" : "0");
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"解析推理结果失败: {ex.Message}");
            }

            return results;
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

                SystemEvent.SendAlarmMsg($"[重点缺陷报警] {reason} (当前SN: {currentSN}, 今日重点缺陷: {keyDefects}, NG总数: {totalDefects})");
                LogTextHelper.Warn($"[重点缺陷报警] {reason}, SN={currentSN}");
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"重点缺陷报警检查异常: {ex.Message}");
            }
        }
    }
}

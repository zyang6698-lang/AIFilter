using DeepSightDB;
using DeepSightModel;
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
                    List<DetectInfo> avi_HeatInfo = new List<DetectInfo>();

                    // 构建 HeatPoint 点信息
                    for (int i = 0; i < obj.Data.InferWholeData.InferResults.Count; i++)
                    {
                        DetectInfo heatInfo = new DetectInfo();
                        if (vBModel.ImageKeys[i]!=null)
                        {
                            heatInfo.ImagePath = vBModel.ImageKeys[i];
                        }
                        var imgRoi = obj.Data.InferWholeData.InferResults[i].ImgRoi;
                        if (imgRoi != null && imgRoi.Count >= 4)
                        {
                            heatInfo.OriginRoiX = imgRoi[0];
                            heatInfo.OriginRoiY = imgRoi[1];
                            heatInfo.OriginWidth = imgRoi[2];
                            heatInfo.OriginHeight = imgRoi[3];
                        }

                        if (obj.Data.InferWholeData.InferResults[i].Infer_Result == "OK")
                        {
                            heatInfo.AIStatus = 1;
                        }
                        else
                        {
                            heatInfo.AIStatus = 2;

                            for (int j = 0; j < obj.Data.InferWholeData.InferResults[i].InferDetails.Location.Count; j++)
                            {
                                string sub_defectName = obj.Data.InferWholeData.InferResults[i].Defect_name;
                                int subX = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].InferDetails.Location[j].X);
                                int subY = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].InferDetails.Location[j].Y);
                                int subH = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].InferDetails.Location[j].Height);
                                int subW = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].InferDetails.Location[j].Width);

                                heatInfo.DefectName = sub_defectName;
                                heatInfo.RoiX = subX;
                                heatInfo.RoiY = subY;
                                heatInfo.Width = subW;
                                heatInfo.Height = subH;

                                heatInfo.DrawInfo =JsonConvert.SerializeObject( obj.Data.InferWholeData.InferResults[i].InferDetails.DrawInfoList);
                                
                                if (sub_defectName == "AU10" || sub_defectName == "CU10" || sub_defectName == "CU41"
                                    || sub_defectName == "HO01" || sub_defectName == "SM10")
                                {
                                    heatInfo.DefectShape = "dot";
                                }
                                else
                                {
                                    heatInfo.DefectShape = "line";
                                }

                            }
                        }

                        if (vBModel.isByPass)
                        {
                            heatInfo.AIStatus = 3;
                        }

                        avi_HeatInfo.Add(heatInfo);
                    }

                    // 保存数据到数据库
                    int aviState = avi_HeatInfo.Count == 0 ? 1 : 2;
                    int aiState = avi_HeatInfo.Any(h => h.AIStatus == 3) ? 3 : avi_HeatInfo.Any(h => h.AIStatus == 2) ? 2 : 1;
                    _savePanelSideAction(panelInfo, avi_HeatInfo, aviState, aiState);

                    LogTextHelper.Info($"处理完成: SN={vBModel.SN}, Side={panelInfo.SideIndex}, AviState={aviState}, AiState={aiState}");
                }
                else if (code == "600")
                {
                    // 无缺陷：AVI OK, AI OK
                    _savePanelSideAction(panelInfo, new List<DetectInfo>(), 1, 1);
                    LogTextHelper.Info($"处理完成(无缺陷): SN={vBModel.SN}, Side={panelInfo.SideIndex}");
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
    }
}

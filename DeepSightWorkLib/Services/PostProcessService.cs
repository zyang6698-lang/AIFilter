using DeepSightDB;
using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 负责处理推理后的后处理逻辑（包含更新中台数据与保存面次数据到数据库）
    /// 通过外部注入的 delegate 将 PanelSide 保存回 BusinessClass 的数据库批处理逻辑，避免直接依赖 BusinessClass。
    /// </summary>
    public class PostProcessService
    {
        private readonly ConcurrentDictionary<string, DsCenterInfo> _dsCenterInfoDict;
        private readonly ConfigurationClass _sysConfig;
        private readonly Action<RootPanelInfo, List<DetectInfo>, int, int> _savePanelSideAction;

        public PostProcessService(ConcurrentDictionary<string, DsCenterInfo> dsCenterInfoDict,
            ConfigurationClass sysConfig,
            Action<RootPanelInfo, List<DetectInfo>, int, int> savePanelSideAction)
        {
            _dsCenterInfoDict = dsCenterInfoDict ?? throw new ArgumentNullException(nameof(dsCenterInfoDict));
            _sysConfig = sysConfig ?? throw new ArgumentNullException(nameof(sysConfig));
            _savePanelSideAction = savePanelSideAction ?? throw new ArgumentNullException(nameof(savePanelSideAction));
        }

        public void ProcessInferenceResult(InferenceResultModel resultModel)
        {
            VBModel vBModel = resultModel.VBModel;
            string msg = resultModel.RawJsonResult;
            RootPanelInfo panelInfo = vBModel.panelInfo;

            try
            {
                // 如果不需要处理（例如图片数量为0或超过最大值）
                if (!resultModel.NeedsProcessing)
                {
                    if (vBModel.Mats == null || vBModel.Mats.Count == 0)
                    {
                        _savePanelSideAction(panelInfo, new List<DetectInfo>(), 1, 1);
                        LogTextHelper.Info($"后处理完成(无图片): SN={vBModel.SN}");
                    }
                    else if (vBModel.Mats.Count > _sysConfig.MaxDefectCount)
                    {
                        var imageKeys = vBModel.ImageKeys.Select(t => new DetectInfo() { ImagePath = t, AIStatus = 3 }).ToList();
                        _savePanelSideAction(panelInfo, imageKeys, 2, 3);
                        LogTextHelper.Info($"后处理完成(图片超限): SN={vBModel.SN}");
                    }
                    return;
                }

                LogTextHelper.Info($"开始后处理: SN={vBModel.SN}, Side={panelInfo.SideIndex}");

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

                    // 更新中台数据
                    UpdateDsCenterInfo(panelInfo, obj);

                    // 处理 HeatPoint 等信息
                    for (int i = 0; i < obj.Data.InferWholeData.InferResults.Count; i++)
                    {
                        DetectInfo heatInfo = new DetectInfo();
                        int index = panelInfo.LocalDescribeDir.IndexOf("deepiresults", StringComparison.OrdinalIgnoreCase);
                        if (index != -1)
                        {
                            string basePath = panelInfo.LocalDescribeDir.Substring(0, index + "deepiresults".Length);
                            string relativePath = obj.Data.InferWholeData.InferResults[i].GroupInfos[0].ImagePath.Replace('/', '\\');
                            string mergedPath = Path.Combine(basePath, relativePath);
                            heatInfo.ImagePath = mergedPath;
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

                                if (j == 0)
                                {
                                    heatInfo.DefectName = sub_defectName;
                                    heatInfo.RoiX = subX ;
                                    heatInfo.RoiY = subY ;
                                    heatInfo.Width = subW ;
                                    heatInfo.Height = subH ;
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
                        }

                        if (vBModel.isByPass)
                        {
                            heatInfo.AIStatus = 3;
                        }

                        avi_HeatInfo.Add(heatInfo);
                    }

                    // 存数据到数据库
                    int aviState = avi_HeatInfo.Count == 0 ? 1 : 2;
                    int aiState = avi_HeatInfo.Any(h => h.AIStatus == 3) ? 3 : avi_HeatInfo.Any(h => h.AIStatus == 2) ? 2 : 1;
                    _savePanelSideAction(panelInfo, avi_HeatInfo, aviState, aiState);

                    LogTextHelper.Info($"后处理完成: SN={vBModel.SN}, Side={panelInfo.SideIndex}, AviState={aviState}, AiState={aiState}");
                }
                else if (code == "600")
                {
                    // 无缺陷，AVI OK, AI OK
                    _savePanelSideAction(panelInfo, new List<DetectInfo>(), 1, 1);
                    LogTextHelper.Info($"后处理完成(无缺陷): SN={vBModel.SN}, Side={panelInfo.SideIndex}");
                }
                else
                {
                    LogTextHelper.Warn($"算法调用失败 for Side {panelInfo.SideIndex}，返回码: {code}，返回信息：{message}");
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"后处理异常: SN={vBModel.SN}, 错误={ex}");
            }
        }

        private void UpdateDsCenterInfo(RootPanelInfo panelInfo, RootVBOutInfo obj)
        {
            if (!_dsCenterInfoDict.TryGetValue($"{panelInfo.LotId}_{panelInfo.SerialNumber}", out DsCenterInfo dsCenterInfo))
            {
                return;
            }
            if (dsCenterInfo == null)
            {
                return;
            }
            LogTextHelper.Info($"取出{panelInfo.LotId}_{panelInfo.SerialNumber}的中台数据，准备更新...");

            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                for (int i = 0; i < obj.Data.InferWholeData.InferResults.Count; i++)
                {
                    dsCenterInfo.Data[0].Content["1"].DefectsCount++;

                    try
                    {
                        //更新中台数据
                        if (panelInfo.SideIndex == "A")
                        {
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].AiResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].ManualResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].ManualDefectCode = obj.Data.InferWholeData.InferResults[i].Defect_name;
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].DefectCode = obj.Data.InferWholeData.InferResults[i].Defect_name;
                        }
                        else
                        {
                            dsCenterInfo.Data[0].Content["1"].EndTime = time;
                            panelInfo.EndTime = time;
                            int Bcount = panelInfo.PcsInfo["1"].DefectInfo.Count;
                            int ALLcount = dsCenterInfo.Data[0].Content["1"].DefectsInfo.Count;
                            int index = ALLcount - Bcount;
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].AiResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].ManualResult = obj.Data.InferWholeData.InferResults[i].Infer_Result.ToLower();
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].ManualDefectCode = obj.Data.InferWholeData.InferResults[i].Defect_name;
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].DefectCode = obj.Data.InferWholeData.InferResults[i].Defect_name;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error("更新中台数据异常" + ex.ToString());
                    }

                    // 更新子缺陷信息到中台数据
                    for (int j = 0; j < obj.Data.InferWholeData.InferResults[i].InferDetails.Location.Count; j++)
                    {
                        DsCenterSubDefectInfo subDefectInfo = new DsCenterSubDefectInfo();
                        subDefectInfo.SubDefectArea = Convert.ToDouble(obj.Data.InferWholeData.InferResults[i].InferDetails.DefectArea);
                        string sub_defectName = obj.Data.InferWholeData.InferResults[i].Defect_name;
                        subDefectInfo.SubDefectCode = sub_defectName;

                        int defectX = 0;
                        int defectY = 0;
                        if (panelInfo.SideIndex == "A")
                        {
                            defectX = dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].DefectRoi.X;
                            defectY = dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].DefectRoi.Y;
                        }
                        else
                        {
                            int Bcount = panelInfo.PcsInfo["1"].DefectInfo.Count;
                            int ALLcount = dsCenterInfo.Data[0].Content["1"].DefectsInfo.Count;
                            int index = ALLcount - Bcount;
                            defectX = dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].DefectRoi.X;
                            defectY = dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].DefectRoi.Y;
                        }

                        int subX = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].InferDetails.Location[j].X);
                        int subY = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].InferDetails.Location[j].Y);
                        int subH = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].InferDetails.Location[j].Height);
                        int subW = Convert.ToInt32(obj.Data.InferWholeData.InferResults[i].InferDetails.Location[j].Width);

                        subDefectInfo.SubDefectHeight = subH;
                        subDefectInfo.SubDefectWidth = subW;
                        subDefectInfo.SubDefectIndex = j;
                        subDefectInfo.SubDefectRoi.Add(subX);
                        subDefectInfo.SubDefectRoi.Add(subY);
                        subDefectInfo.SubDefectRoi.Add(subW);
                        subDefectInfo.SubDefectRoi.Add(subH);
                        //中心点参数
                        int CenterPointX = defectX + subX / 2 + subW / 4;
                        int CenterPointY = defectY + subY / 2 + subH / 4;
                        subDefectInfo.CenterPoint.Add(CenterPointX);
                        subDefectInfo.CenterPoint.Add(CenterPointY);

                        //更新中台数据
                        if (panelInfo.SideIndex == "A")
                        {
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[i].SubDefectsInfo.Add(subDefectInfo);
                        }
                        else
                        {
                            int Bcount = panelInfo.PcsInfo["1"].DefectInfo.Count;
                            int ALLcount = dsCenterInfo.Data[0].Content["1"].DefectsInfo.Count;
                            int index = ALLcount - Bcount;
                            dsCenterInfo.Data[0].Content["1"].DefectsInfo[index + i].SubDefectsInfo.Add(subDefectInfo);
                        }
                    }
                }
                //B面做完判断总结果
                if (panelInfo.SideIndex == "B")
                {
                    if (dsCenterInfo.Data[0].Content["1"].DefectsInfo.Exists(o => o.AiResult.ToLower() == "ng"))
                    {
                        dsCenterInfo.Data[0].Content["1"].ConfirmResult = "ng";
                    }
                    else
                    {
                        dsCenterInfo.Data[0].Content["1"].ConfirmResult = "ok";
                    }
                    dsCenterInfo.Data[0].EndTime = time;
                    dsCenterInfo.Data[0].Content["1"].EndTime = time;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("中台数据处理异常" + ex.ToString());
            }
        }
    }
}

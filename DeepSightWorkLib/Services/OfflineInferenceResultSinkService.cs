using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DeepSightWorkLib.Services
{
    public class OfflineInferenceResultSinkService
    {
        private ModelValidationTestService _validationTestService;

        public void SetValidationTestService(ModelValidationTestService service)
        {
            _validationTestService = service;
        }

        public void Process(VBModel vbModel, string rawJsonResult)
        {
            if (vbModel == null || !vbModel.IsValidationTest)
                return;

            if (_validationTestService == null)
            {
                LogTextHelper.Warn($"验证测试服务未注入，跳过测试结果处理: {vbModel.SN}_{vbModel.Side}");
                return;
            }

            try
            {
                var inferResults = ExtractInferResults(rawJsonResult);

                if (vbModel.IsSingleImageTest)
                {
                    _validationTestService.ProcessSingleImageTestResult(vbModel, inferResults, rawJsonResult);
                    LogTextHelper.Info($"单图测试结果处理完成: {vbModel.SN}");
                }
                else if (vbModel.IsSecondaryInference)
                {
                    _ = _validationTestService.ProcessSecondaryInferenceResultAsync(vbModel, inferResults);
                    LogTextHelper.Info($"二次推理结果处理已启动: {vbModel.SN}_{vbModel.Side}");
                }
                else
                {
                    _validationTestService.ProcessValidationTestResult(vbModel, inferResults);
                    LogTextHelper.Info($"验证测试结果处理完成: {vbModel.SN}_{vbModel.Side}");
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"处理离线推理结果异常: {vbModel.SN}_{vbModel.Side}, {ex}");
                _validationTestService?.RecordPipelineFailure(vbModel, "后处理", ex.Message);
            }
        }

        private static List<string> ExtractInferResults(string rawJsonResult)
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

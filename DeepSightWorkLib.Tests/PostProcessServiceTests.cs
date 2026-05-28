using DeepSightModel;
using DeepSightWorkLib.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DeepSightWorkLib.Tests
{
    [TestClass]
    public class PostProcessServiceTests
    {
        [TestMethod]
        public void ProcessInferenceResult_使用VBModel全量缺陷集合并保留原始缺陷名()
        {
            List<DetectInfo> savedDefects = null;
            int savedAviState = 0;
            int savedAiState = 0;

            var service = new PostProcessService(new ConfigurationClass(), (model, defects, aviState, aiState) =>
            {
                savedDefects = defects;
                savedAviState = aviState;
                savedAiState = aiState;
            });

            var vbModel = new VBModel
            {
                SN = "SN001",
                Side = "A",
                ProductSerial = "P001",
                AllDefectInfos = new List<DetectInfo>
                {
                    new DetectInfo { OriginDefectName = "AVI01", DefectName = "AVI01", DefectIndex = 7, PcsIndex = 2 },
                    new DetectInfo { OriginDefectName = "AVI02", DefectName = "AVI02", DefectIndex = 8, PcsIndex = 3, AIStatus = 4 }
                }
            };

            service.ProcessInferenceResult(new InferenceResultModel
            {
                VBModel = vbModel,
                RawJsonResult = "{\"code\":\"600\",\"msg\":\"ok\"}",
                NeedsProcessing = true
            });

            Assert.IsNotNull(savedDefects);
            Assert.AreEqual(2, savedDefects.Count);
            Assert.AreEqual("AVI01", savedDefects[0].OriginDefectName);
            Assert.AreEqual("AVI02", savedDefects[1].OriginDefectName);
            Assert.AreEqual(1, savedDefects[0].AIStatus);
            Assert.AreEqual(4, savedDefects[1].AIStatus);
            Assert.AreEqual(7, savedDefects[0].DefectIndex);
            Assert.AreEqual(3, savedDefects[1].PcsIndex);
            Assert.AreEqual(2, savedAviState);
            Assert.AreEqual(2, savedAiState);
        }
    }
}
using DeepSightDB;
using DeepSightModel;
using DeepSightWorkLib.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace DeepSightWorkLib.Tests
{
    [TestClass]
    public class DefectProcessorTests
    {
        #region BuildPostProcessModel 测试

        [TestMethod]
        public void BuildPostProcessModel_正确构建模型()
        {
            var model = new VBModel { SN = "SN001", Side = "A" };

            var result = DefectProcessor.BuildPostProcessModel(model, "{\"Code\":200}", true);

            Assert.IsNotNull(result);
            Assert.AreEqual("{\"Code\":200}", result.RawJsonResult);
            Assert.AreSame(model, result.VBModel);
            Assert.IsTrue(result.NeedsProcessing);
            Assert.IsNotNull(result.InferenceCompletedTime);
        }

        [TestMethod]
        public void BuildPostProcessModel_无需处理()
        {
            var model = new VBModel { SN = "SN002", Side = "B" };

            var result = DefectProcessor.BuildPostProcessModel(model, "", false);

            Assert.AreEqual("", result.RawJsonResult);
            Assert.IsFalse(result.NeedsProcessing);
        }

        #endregion

        #region BuildAIResult 测试

        private VBModel CreateTestVBModel()
        {
            return new VBModel
            {
                Key = "test_key",
                SN = "SN_BUILD",
                Side = "A",
                DefectIndex = new List<int> { 0, 1, 2 },
                PcsIndex = new List<int> { 1, 1, 2 },
                SourceDbUrl = "http://localhost:8080",
                SourceWriteBackDbName = "my_writeback_db",
                SourceVRSWriteBackDbName = "my_vrs_db",
                LocalDescribePath = "/path/to/panel.json"
            };
        }

        [TestMethod]
        public void BuildAIResult_正确填充基本字段()
        {
            var model = CreateTestVBModel();
            var msgs = new List<string> { "0", "1", "0" };

            var result = DefectProcessor.BuildAIResult(model, msgs);

            Assert.AreEqual("SN_BUILD", result.SN);
            Assert.AreEqual("A", result.Side);
            Assert.AreEqual("my_writeback_db", result.AVIDbName);
            Assert.AreEqual("my_vrs_db", result.VRSDbName);
            Assert.AreEqual("http://localhost:8080", result.TargetUrl);
            Assert.AreEqual("put", result.Operation);
            Assert.AreEqual("all_ow", result.OpMode); // Side A -> all_ow
            Assert.AreEqual("test_key", result.Key);
        }

        [TestMethod]
        public void BuildAIResult_SideB_OpMode为ap()
        {
            var model = CreateTestVBModel();
            model.Side = "B";
            var msgs = new List<string> { "0", "1", "0" };

            var result = DefectProcessor.BuildAIResult(model, msgs);

            Assert.AreEqual("ap", result.OpMode);
        }

        [TestMethod]
        public void BuildAIResult_正确构建ResultInfos()
        {
            var model = CreateTestVBModel();
            var msgs = new List<string> { "0", "1", "0" };

            var result = DefectProcessor.BuildAIResult(model, msgs);

            Assert.IsNotNull(result.Value); // JSON 字符串
            Assert.AreEqual(3, result.AIDetailResultItems.Count);

            // 验证 AIDetailResultItems 的 AiLabel
            Assert.AreEqual("OK", result.AIDetailResultItems[0].AiLabel);  // msg "0" -> OK
            Assert.AreEqual("NG", result.AIDetailResultItems[1].AiLabel);  // msg "1" -> NG
            Assert.AreEqual("OK", result.AIDetailResultItems[2].AiLabel);  // msg "0" -> OK
        }

        [TestMethod]
        public void BuildAIResult_msg为空时默认全部NG()
        {
            var model = CreateTestVBModel();

            var result = DefectProcessor.BuildAIResult(model, null);

            Assert.AreEqual(3, result.AIDetailResultItems.Count);
            Assert.IsTrue(result.AIDetailResultItems.All(x => x.AiLabel == "NG")); // 默认 "1" -> NG
        }

        [TestMethod]
        public void BuildAIResult_使用默认数据库名称()
        {
            var model = CreateTestVBModel();
            model.SourceWriteBackDbName = null;
            model.SourceVRSWriteBackDbName = "";

            var result = DefectProcessor.BuildAIResult(model, new List<string> { "0", "0", "0" });

            Assert.AreEqual("filter_time_to_airesults", result.AVIDbName);
            Assert.AreEqual("ai_detail_results_tovrs", result.VRSDbName);
        }

        [TestMethod]
        public void BuildAIResult_包含直报缺陷()
        {
            var model = CreateTestVBModel();
            model.AllDefectInfos = new List<DetectInfo>
            {
                new DetectInfo { DefectIndex = 3, PcsIndex = 2, AIStatus = 4, OriginDefectName = "DR01" }
            };

            var msgs = new List<string> { "0", "1", "0" };

            var result = DefectProcessor.BuildAIResult(model, msgs);

            // 3个正常 + 1个直报 = 4 条
            Assert.AreEqual(4, result.AIDetailResultItems.Count);
            Assert.AreEqual("DirectReport", result.AIDetailResultItems[3].AiFlag);
            Assert.AreEqual("NG", result.AIDetailResultItems[3].AiLabel);
        }

        #endregion
    }
}

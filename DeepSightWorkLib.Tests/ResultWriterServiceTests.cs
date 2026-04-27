using DeepSightModel;
using DeepSightWorkLib.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Reflection;

namespace DeepSightWorkLib.Tests
{
    /// <summary>
    /// ResultWriterService 内部辅助方法的单元测试（覆盖 VRS V1.0 分支核心逻辑）
    /// 注意：本类通过反射替换 LevelDbConfigManager._instance 静态字段，不能与其他测试方法并行执行
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class ResultWriterServiceTests
    {
        private static FieldInfo _instanceField;
        private LevelDbConfigList _backup;

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            _instanceField = typeof(LevelDbConfigManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(_instanceField, "未能反射到 LevelDbConfigManager._instance 字段");
        }

        [TestInitialize]
        public void Init()
        {
            // 备份当前单例，用反射临时替换为测试配置，避免触发文件 IO
            _backup = (LevelDbConfigList)_instanceField.GetValue(null);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _instanceField.SetValue(null, _backup);
        }

        private static void SetInstance(params LevelDbConfig[] dbs)
        {
            var list = new LevelDbConfigList { Databases = new List<LevelDbConfig>(dbs) };
            _instanceField.SetValue(null, list);
        }

        #region MapAiLabelToStatusCode

        [TestMethod]
        public void MapAiLabelToStatusCode_OK返回0()
        {
            Assert.AreEqual("0", ResultWriterService.MapAiLabelToStatusCode("OK"));
        }

        [TestMethod]
        public void MapAiLabelToStatusCode_NG返回1()
        {
            Assert.AreEqual("1", ResultWriterService.MapAiLabelToStatusCode("NG"));
        }

        [TestMethod]
        public void MapAiLabelToStatusCode_大小写不敏感()
        {
            Assert.AreEqual("0", ResultWriterService.MapAiLabelToStatusCode("ok"));
            Assert.AreEqual("1", ResultWriterService.MapAiLabelToStatusCode("ng"));
            Assert.AreEqual("0", ResultWriterService.MapAiLabelToStatusCode("Ok"));
        }

        [TestMethod]
        public void MapAiLabelToStatusCode_其他值返回2()
        {
            Assert.AreEqual("2", ResultWriterService.MapAiLabelToStatusCode("UNKNOWN"));
            Assert.AreEqual("2", ResultWriterService.MapAiLabelToStatusCode(""));
            Assert.AreEqual("2", ResultWriterService.MapAiLabelToStatusCode(null));
        }

        #endregion

        #region FindConfigByUrl

        [TestMethod]
        public void FindConfigByUrl_精确匹配()
        {
            var dbA = new LevelDbConfig { IP = "10.0.0.1", Port = "9877", DbName = "A", IsEnabled = true };
            var dbB = new LevelDbConfig { IP = "10.0.0.2", Port = "9877", DbName = "B", IsEnabled = true };
            SetInstance(dbA, dbB);

            var matched = ResultWriterService.FindConfigByUrl("http://10.0.0.2:9877");

            Assert.IsNotNull(matched);
            Assert.AreEqual("B", matched.DbName);
        }

        [TestMethod]
        public void FindConfigByUrl_大小写不敏感()
        {
            var dbA = new LevelDbConfig { IP = "Host-A", Port = "9877", DbName = "A", IsEnabled = true };
            SetInstance(dbA);

            var matched = ResultWriterService.FindConfigByUrl("HTTP://HOST-A:9877");

            Assert.IsNotNull(matched);
            Assert.AreEqual("A", matched.DbName);
        }

        [TestMethod]
        public void FindConfigByUrl_匹配不到时回退第一个启用项()
        {
            var dbA = new LevelDbConfig { IP = "10.0.0.1", Port = "9877", DbName = "A", IsEnabled = false };
            var dbB = new LevelDbConfig { IP = "10.0.0.2", Port = "9877", DbName = "B", IsEnabled = true };
            var dbC = new LevelDbConfig { IP = "10.0.0.3", Port = "9877", DbName = "C", IsEnabled = true };
            SetInstance(dbA, dbB, dbC);

            var matched = ResultWriterService.FindConfigByUrl("http://10.0.0.99:9877");

            Assert.IsNotNull(matched);
            Assert.AreEqual("B", matched.DbName);
        }

        [TestMethod]
        public void FindConfigByUrl_列表为空返回null()
        {
            SetInstance();

            var matched = ResultWriterService.FindConfigByUrl("http://10.0.0.1:9877");

            Assert.IsNull(matched);
        }

        [TestMethod]
        public void FindConfigByUrl_读取V1配置字段()
        {
            var dbA = new LevelDbConfig
            {
                IP = "10.0.0.1",
                Port = "9877",
                DbName = "A",
                IsEnabled = true,
                EnableVRSWriteBackV1 = true,
                VRSWriteBackDbNameV1 = "custom_v1_db"
            };
            SetInstance(dbA);

            var matched = ResultWriterService.FindConfigByUrl("http://10.0.0.1:9877");

            Assert.IsNotNull(matched);
            Assert.IsTrue(matched.EnableVRSWriteBackV1);
            Assert.AreEqual("custom_v1_db", matched.VRSWriteBackDbNameV1);
        }

        #endregion
    }
}

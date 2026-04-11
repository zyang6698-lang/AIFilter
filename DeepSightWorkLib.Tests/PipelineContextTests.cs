using DeepSightModel;
using DeepSightWorkLib.Services;
using DeepSightWorkLib.Services.Pipeline;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace DeepSightWorkLib.Tests
{
    [TestClass]
    public class PipelineContextTests
    {
        [TestMethod]
        public void SetError_标记错误状态()
        {
            var ctx = new PipelineContext();

            ctx.SetError("JSON解析", "JSON为空");

            Assert.IsTrue(ctx.HasError);
            Assert.AreEqual("JSON解析", ctx.ErrorStage);
            Assert.AreEqual("JSON为空", ctx.ErrorMessage);
        }

        [TestMethod]
        public void SN和Side_从AviContext读取()
        {
            var ctx = new PipelineContext
            {
                AviContext = new AviProcessingContext
                {
                    SerialNumber = "SN001",
                    Side = "A"
                }
            };

            Assert.AreEqual("SN001", ctx.SN);
            Assert.AreEqual("A", ctx.Side);
        }

        [TestMethod]
        public void SN和Side_从LoadModel回退读取()
        {
            var ctx = new PipelineContext
            {
                LoadModel = new ImageLoadModel
                {
                    Model = new VBModel { SN = "SN002", Side = "B" }
                }
            };

            Assert.AreEqual("SN002", ctx.SN);
            Assert.AreEqual("B", ctx.Side);
        }

        [TestMethod]
        public void IsValidationTest_默认为false()
        {
            var ctx = new PipelineContext();
            Assert.IsFalse(ctx.IsValidationTest);
        }

        [TestMethod]
        public void Stopwatch_创建时自动启动()
        {
            var ctx = new PipelineContext();
            Assert.IsTrue(ctx.Stopwatch.IsRunning);
        }

        [TestMethod]
        public void ReleaseDownstreamRef_两次调用后触发Dispose()
        {
            // 创建一个简单的 VBModel（不含 Mat，Dispose 安全）
            var model = new VBModel { SN = "SN_DISPOSE", Side = "A" };
            var ctx = new PipelineContext
            {
                LoadModel = new ImageLoadModel { Model = model }
            };

            // 第一次释放，不应触发 Dispose
            ctx.ReleaseDownstreamRef();
            // model 仍然存在（VBModel.Dispose 只是清空 Mats 列表，不会抛异常）

            // 第二次释放，应触发 Dispose
            ctx.ReleaseDownstreamRef();
            // 验证 VBModel.Dispose 已被调用（Mats 被置为 null）
            Assert.IsNull(model.Mats);
        }

        [TestMethod]
        public void ReleaseDownstreamRef_多次额外调用不会抛异常()
        {
            var model = new VBModel { SN = "SN_EXTRA", Side = "A" };
            var ctx = new PipelineContext
            {
                LoadModel = new ImageLoadModel { Model = model }
            };

            // 调用3次（超过默认的2次引用计数）不应抛异常
            ctx.ReleaseDownstreamRef();
            ctx.ReleaseDownstreamRef();
            ctx.ReleaseDownstreamRef();
        }

        [TestMethod]
        public void ReleaseDownstreamRef_LoadModel为null时不抛异常()
        {
            var ctx = new PipelineContext();

            // LoadModel 为 null，Dispose 不应抛异常
            ctx.ReleaseDownstreamRef();
            ctx.ReleaseDownstreamRef();
        }

        [TestMethod]
        public void ReleaseDownstreamRef_线程安全()
        {
            var model = new VBModel { SN = "SN_THREAD", Side = "A" };
            var ctx = new PipelineContext
            {
                LoadModel = new ImageLoadModel { Model = model }
            };

            // 并发从两个线程调用
            var t1 = new Thread(() => ctx.ReleaseDownstreamRef());
            var t2 = new Thread(() => ctx.ReleaseDownstreamRef());
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            // 不抛异常即通过，VBModel.Dispose 应被调用恰好一次
            Assert.IsNull(model.Mats);
        }
    }
}

using DeepSightModel;
using DeepSightWorkLib.Services;
using DeepSightWorkLib.Services.Pipeline;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DeepSightWorkLib.Tests
{
    [TestClass]
    public class ProcessingPipelineTests
    {
        private ProcessingPipeline CreatePipeline(
            Func<PipelineContext, PipelineContext> jsonParse = null,
            Func<PipelineContext, PipelineContext> imageLoad = null,
            Func<PipelineContext, PipelineContext> inference = null,
            Action<PipelineContext> resultWrite = null,
            Action<PipelineContext> postProcess = null,
            Action<PipelineContext> errorHandler = null)
        {
            var pipeline = new ProcessingPipeline()
                .WithJsonParseStage(jsonParse ?? (ctx => ctx))
                .WithImageLoadStage(imageLoad ?? (ctx => ctx))
                .WithInferenceStage(inference ?? (ctx => ctx))
                .WithResultWriteStage(resultWrite ?? (ctx => { }))
                .WithPostProcessStage(postProcess ?? (ctx => { }));

            if (errorHandler != null)
                pipeline.WithErrorHandler(errorHandler);

            return pipeline;
        }

        private PipelineContext CreateTestContext(string sn = "TEST_SN", string side = "A")
        {
            return new PipelineContext
            {
                AviContext = new AviProcessingContext
                {
                    SerialNumber = sn,
                    Side = side,
                    MinioIp = "127.0.0.1",
                    Key = "test_key"
                }
            };
        }

        [TestMethod]
        public void Start_未配置阶段_应抛出异常()
        {
            var pipeline = new ProcessingPipeline();
            bool thrown = false;
            try
            {
                pipeline.Start();
            }
            catch (InvalidOperationException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "应抛出 InvalidOperationException");
        }

        [TestMethod]
        public void Start_重复启动_不应抛异常()
        {
            using (var pipeline = CreatePipeline())
            {
                pipeline.Start();
                pipeline.Start(); // 第二次应被忽略
                Assert.IsTrue(pipeline.IsRunning);
            }
        }

        [TestMethod]
        public void Post_未启动_应返回false()
        {
            using (var pipeline = CreatePipeline())
            {
                var result = pipeline.Post(CreateTestContext());
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void 完整流转_所有阶段按顺序执行()
        {
            var stageOrder = new List<string>();
            var completed = new ManualResetEventSlim(false);

            using (var pipeline = CreatePipeline(
                jsonParse: ctx => { stageOrder.Add("JSON"); return ctx; },
                imageLoad: ctx => { stageOrder.Add("Image"); return ctx; },
                inference: ctx => { stageOrder.Add("Infer"); return ctx; },
                resultWrite: ctx => { stageOrder.Add("Write"); CheckComplete(); },
                postProcess: ctx => { stageOrder.Add("Post"); CheckComplete(); }
            ))
            {
                pipeline.Start();
                pipeline.Post(CreateTestContext());

                Assert.IsTrue(completed.Wait(5000), "Pipeline 未在超时内完成");
                Assert.IsTrue(stageOrder.IndexOf("JSON") < stageOrder.IndexOf("Image"));
                Assert.IsTrue(stageOrder.IndexOf("Image") < stageOrder.IndexOf("Infer"));
            }

            void CheckComplete()
            {
                // 当 Write 和 Post 都执行过后标记完成
                if (stageOrder.Contains("Write") && stageOrder.Contains("Post"))
                    completed.Set();
            }
        }

        [TestMethod]
        public void BroadcastBlock_推理后同时触发两个下游()
        {
            int writeCount = 0;
            int postCount = 0;
            var allDone = new CountdownEvent(2);

            using (var pipeline = CreatePipeline(
                resultWrite: ctx => { Interlocked.Increment(ref writeCount); allDone.Signal(); },
                postProcess: ctx => { Interlocked.Increment(ref postCount); allDone.Signal(); }
            ))
            {
                pipeline.Start();
                pipeline.Post(CreateTestContext());

                Assert.IsTrue(allDone.Wait(5000), "下游未在超时内完成");
                Assert.AreEqual(1, writeCount);
                Assert.AreEqual(1, postCount);
            }
        }

        [TestMethod]
        public void 错误透传_HasError后跳过后续阶段()
        {
            bool imageLoadCalled = false;
            bool resultWriteCalled = false;
            bool postProcessCalled = false;
            var allDone = new CountdownEvent(2);

            using (var pipeline = CreatePipeline(
                jsonParse: ctx => { ctx.SetError("JSON解析", "测试错误"); return ctx; },
                imageLoad: ctx => { imageLoadCalled = true; return ctx; },
                inference: ctx => { Assert.Fail("推理阶段不应被执行"); return ctx; },
                resultWrite: ctx => { resultWriteCalled = true; allDone.Signal(); },
                postProcess: ctx => { postProcessCalled = true; allDone.Signal(); }
            ))
            {
                pipeline.Start();
                pipeline.Post(CreateTestContext());

                // 等待数据流过所有 block
                allDone.Wait(TimeSpan.FromSeconds(5));
            }

            // 有错误时，SafeExecuteTransform 会跳过后续 Transform 阶段
            Assert.IsFalse(imageLoadCalled, "图片加载不应被执行");
            // SafeExecuteAction 仍会调用 action（确保 finally 清理逻辑执行），
            // 由各 Stage 内部检查 HasError 跳过业务逻辑
            Assert.IsTrue(resultWriteCalled, "结果回写 action 应被调用（Stage 内部检查 HasError 跳过业务逻辑）");
            Assert.IsTrue(postProcessCalled, "后处理 action 应被调用（Stage 内部检查 HasError 跳过业务逻辑）");
        }
    }
}

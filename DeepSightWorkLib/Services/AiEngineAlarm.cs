using DeepSightEvent;
using DeepSightModel.Alarm;
using DeepSightTool;
using System;
using System.Threading;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// AI 引擎相关告警的语义化封装
    /// 统一 Source / Category / Level，避免调用点各自拼字符串导致冷却 Key 不稳定
    /// </summary>
    public static class AiEngineAlarm
    {
        #region 告警编码（稳定标识，用于日志检索 / 后续规则化）

        public const string CodeInitFailed        = "AI_ENGINE_INIT_FAILED";
        public const string CodeShowViewFailed    = "AI_ENGINE_SHOW_VIEW_FAILED";
        public const string CodeSolutionListEmpty = "AI_ENGINE_SOLUTION_LIST_EMPTY";

        private const string SourceInit         = "AI_Engine.Init";
        private const string SourceShowView    = "AI_Engine.ShowView";
        private const string SourceSolutionList = "AI_Engine.SolutionList";

        #endregion

        /// <summary>
        /// create_basehandler 初始化失败（通常是 ProxyServer.dll 缺失或加载失败）
        /// </summary>
        public static void ReportInitFailed(Exception ex)
        {
            string detail = ex?.ToString() ?? "(无异常详情)";
            Raise(SourceInit, CodeInitFailed,
                  "AI 引擎初始化失败 (create_basehandler)，推理功能不可用",
                  detail);
        }

        /// <summary>
        /// create_basehandler 初始化失败（传入错误字符串版本）
        /// </summary>
        public static void ReportInitFailed(string errorMessage)
        {
            Raise(SourceInit, CodeInitFailed,
                  "AI 引擎初始化失败 (create_basehandler)，推理功能不可用",
                  errorMessage ?? "(无错误信息)");
        }

        /// <summary>
        /// Vision_Show_View 打开 VB 界面失败
        /// </summary>
        public static void ReportShowViewFailed(Exception ex)
        {
            string detail = ex?.ToString() ?? "(无异常详情)";
            Raise(SourceShowView, CodeShowViewFailed,
                  "打开 VB 算法界面失败 (Vision_Show_View)",
                  detail);
        }

        /// <summary>
        /// Vision_runMethod 获取方案流程列表为空或异常
        /// </summary>
        /// <param name="reason">具体原因（如"返回字符串为空" / 异常信息）</param>
        public static void ReportSolutionListEmpty(string reason)
        {
            Raise(SourceSolutionList, CodeSolutionListEmpty,
                  "获取 AI 方案流程列表为空或失败 (Vision_runMethod)",
                  reason ?? "(无原因)");
        }

        #region 测试入口

        // 测试计数器：保证多次点击"测试告警"时 message 唯一，绕过 AlarmService 冷却
        private static int _testSeq;

        /// <summary>
        /// 测试用：依次触发三种 AI 引擎告警，用于验证告警链路是否畅通
        /// （Toast / FrmAlarm 表格 / 文件持久化）
        /// 冷却 Key = Category + Source + Message 前50字符，故把递增序号和时间戳放入 message
        /// 以保证每次调用都能突破冷却、真正弹窗
        /// </summary>
        public static void TestFireAll()
        {
            int seq = Interlocked.Increment(ref _testSeq);
            string ts = DateTime.Now.ToString("HH:mm:ss.fff");
            string tag = $"[测试#{seq} {ts}]";

            LogTextHelper.Info($"[AiEngineAlarm] TestFireAll 开始 {tag}");

            Raise(SourceInit, CodeInitFailed,
                  $"{tag} AI 引擎初始化失败 (create_basehandler)，推理功能不可用",
                  "模拟 create_basehandler 失败");
            Raise(SourceShowView, CodeShowViewFailed,
                  $"{tag} 打开 VB 算法界面失败 (Vision_Show_View)",
                  "模拟 Vision_Show_View 失败");
            Raise(SourceSolutionList, CodeSolutionListEmpty,
                  $"{tag} 获取 AI 方案流程列表为空或失败 (Vision_runMethod)",
                  "模拟方案列表返回为空");

            LogTextHelper.Info("[AiEngineAlarm] TestFireAll 完成");
        }

        #endregion

        #region 内部

        private static void Raise(string source, string code, string message, string detail)
        {
            try
            {
                // 在 detail 前缀里带上 code，便于日志/文件检索
                string fullDetail = $"[Code={code}] {detail}";
                AlarmService.Instance.RaiseAlarm(
                    AlarmLevel.Error,
                    AlarmCategory.AI,
                    source,
                    message,
                    fullDetail);
            }
            catch (Exception raiseEx)
            {
                LogTextHelper.Error($"[AiEngineAlarm] RaiseAlarm 自身异常: {raiseEx.Message}");
            }
        }

        #endregion
    }
}

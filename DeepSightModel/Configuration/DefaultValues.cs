using System;

namespace DeepSightModel.Configuration
{
    /// <summary>
    /// 默认值常量类
    /// 集中管理项目中所有的默认配置值，方便统一修改
    /// </summary>
    public static class DefaultValues
    {
        #region 项目基础配置

        /// <summary>
        /// 默认项目名称
        /// </summary>
        public const string ProjectName = "DeepSight_AI";

        /// <summary>
        /// 默认线体名称
        /// </summary>
        public const string Line = "Line1";

        /// <summary>
        /// 默认日志保留天数
        /// </summary>
        public const int LogDay = 7;

        /// <summary>
        /// 默认欢迎页标题文字
        /// </summary>
        public const string WelcomeTitle = "Deepsight AI";

        /// <summary>
        /// 默认欢迎页标题字体大小
        /// </summary>
        public const float WelcomeFontSize = 36f;

        #endregion

        #region 快捷键默认配置

        /// <summary>标记VVS_OK的默认快捷键</summary>
        public const string ShortcutVvsOk = "D1";
        /// <summary>标记VVS_NG的默认快捷键</summary>
        public const string ShortcutVvsNg = "D2";
        /// <summary>标记VVS未设置的默认快捷键</summary>
        public const string ShortcutVvsNotSet = "D3";
        /// <summary>切换到下一行的默认快捷键</summary>
        public const string ShortcutNextRow = "Tab";
        /// <summary>下一张图片的默认快捷键</summary>
        public const string ShortcutNextImage = "Down";
        /// <summary>上一张图片的默认快捷键</summary>
        public const string ShortcutPrevImage = "Up";
        /// <summary>下一页的默认快捷键</summary>
        public const string ShortcutNextPage = "Right";
        /// <summary>上一页的默认快捷键</summary>
        public const string ShortcutPrevPage = "Left";

        #endregion

        #region Minio 配置

        /// <summary>
        /// 默认 Minio 配置（IP:Port 格式）
        /// </summary>
        public const string MinioConfig = "127.0.0.1:9102";

        #endregion

        #region 缺陷与超时配置

        /// <summary>
        /// 默认最大缺陷数
        /// </summary>
        public const int MaxDefectCount = 200;

        /// <summary>
        /// Agent 关闭超时时间（毫秒）
        /// </summary>
        public const int AgentShutdownTimeout = 2000;

        /// <summary>
        /// 推理请求超时时间（秒）
        /// </summary>
        public const int InferRequestTimeout = 30;

        /// <summary>
        /// 获取推理结果间隔（秒）
        /// </summary>
        public const int GetInferResultInterval = 5;

        /// <summary>
        /// 获取推理结果超时时间（秒）
        /// </summary>
        public const int GetInferResultTimeout = 300;

        /// <summary>
        /// 最大等待时间（秒）
        /// </summary>
        public const int MaxWaitTime = 10;

        /// <summary>
        /// 是否使用 Gerber 图（默认 false，使用 Template 图）
        /// </summary>
        public const bool UseGerberImage = false;

        #endregion

        #region AVI 配置

        /// <summary>
        /// 默认 AVI 名称
        /// </summary>
        public const string AviName = "AVI";

        /// <summary>
        /// 等待标志文件名
        /// </summary>
        public const string WaitFlag = "wait_format.flag";

        /// <summary>
        /// 完成标志文件名
        /// </summary>
        public const string FinishFlag = "finish_format.flag";

        /// <summary>
        /// 默认 LDB 端点
        /// </summary>
        public const string LDBEndpoint = "127.0.0.1:9877";

        /// <summary>
        /// 默认机台名称
        /// </summary>
        public const string DefaultMachineName = "AVI";

        /// <summary>
        /// 默认数据源类型
        /// </summary>
        public const DataSourceType DefaultDataSourceType = DataSourceType.LevelDb;

        #endregion

        #region 数据库配置

        /// <summary>
        /// 默认数据库主机
        /// </summary>
        public const string DbHost = "localhost";

        /// <summary>
        /// 默认数据库端口
        /// </summary>
        public const int DbPort = 5432;

        /// <summary>
        /// 默认数据库名
        /// </summary>
        public const string DbName = "deepsight";

        /// <summary>
        /// 默认数据库用户名
        /// </summary>
        public const string DbUsername = "postgres";

        /// <summary>
        /// 默认数据库密码
        /// </summary>
        public const string DbPassword = "deepsightai";

        #endregion

        #region 业务处理配置

        /// <summary>
        /// Panel 批量写入大小
        /// </summary>
        public const int PanelSideBatchSize = 100;

        /// <summary>
        /// 缓存过期时间（分钟）
        /// </summary>
        public const int CacheExpireMinutes = 30;

        #endregion

        #region 工作线程轮询间隔（毫秒）

        /// <summary>
        /// 读取 AVI 轮询间隔
        /// </summary>
        public const int ReadAviPollIntervalMs = 1000;

        /// <summary>
        /// 图像加载轮询间隔
        /// </summary>
        public const int ImageLoadPollIntervalMs = 15;

        /// <summary>
        /// 缺陷检测轮询间隔
        /// </summary>
        public const int DefectPollIntervalMs = 15;

        /// <summary>
        /// 返回 AVI 轮询间隔
        /// </summary>
        public const int ReturnAviPollIntervalMs = 15;

        /// <summary>
        /// 后处理轮询间隔
        /// </summary>
        public const int PostProcessPollIntervalMs = 10;

        /// <summary>
        /// 缓存清理轮询间隔
        /// </summary>
        public const int CleanupCachePollIntervalMs = 60000;

        #endregion
    }
}


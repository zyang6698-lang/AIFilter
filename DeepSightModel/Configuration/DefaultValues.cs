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

        #endregion

        #region 服务器配置

        /// <summary>
        /// 默认服务器 IP
        /// </summary>
        public const string ServerIP = "http://";

        /// <summary>
        /// 默认服务器端口
        /// </summary>
        public const string ServerPort = "2000";

        #endregion

        #region Minio 配置

        /// <summary>
        /// 默认 Minio IP
        /// </summary>
        public const string MinioIP = "127.0.0.1";

        /// <summary>
        /// 默认 Minio 端口
        /// </summary>
        public const string MinioPort = "9102";

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
        public const int MaxWaitTime = 5;

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
        public const int ReadAviPollIntervalMs = 100;

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


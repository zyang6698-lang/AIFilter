using System.Threading.Tasks;

namespace DeepSightCommunication.Interfaces
{
    /// <summary>
    /// LevelDB HTTP 客户端接口：通过 HTTP 与 LevelDB 后端服务通信
    /// </summary>
    public interface ILevelDbHttpClient
    {
        /// <summary>
        /// 同步发送 LevelDB 请求
        /// </summary>
        /// <param name="url">请求 URL</param>
        /// <param name="payload">请求负载对象（将被序列化为 JSON）</param>
        /// <param name="operation">操作类型（影响日志与告警内容）</param>
        /// <param name="response">响应内容</param>
        /// <param name="scene">业务场景描述（可选），用于日志标记（如"AVI回写"、"VRS查询"）</param>
        /// <returns>是否成功</returns>
        bool PostJson(string url, object payload, LevelDbOperation operation, out string response, string scene = null);

        /// <summary>
        /// 异步发送 LevelDB 请求
        /// </summary>
        /// <param name="url">请求 URL</param>
        /// <param name="payload">请求负载对象（将被序列化为 JSON）</param>
        /// <param name="operation">操作类型（影响日志内容）</param>
        /// <param name="scene">业务场景描述（可选）</param>
        /// <returns>HTTP 结果</returns>
        Task<HttpResult> PostJsonAsync(string url, object payload, LevelDbOperation operation, string scene = null);
    }
}

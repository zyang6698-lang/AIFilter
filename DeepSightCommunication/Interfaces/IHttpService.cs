using System.Threading.Tasks;

namespace DeepSightCommunication.Interfaces
{
    /// <summary>
    /// HTTP 服务接口，用于与 LevelDB 等后端服务通信
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// HTTP POST 请求（同步）
        /// </summary>
        /// <param name="url">请求 URL</param>
        /// <param name="info">请求参数对象</param>
        /// <param name="type">请求类型 0：请求数据 1：回写数据</param>
        /// <param name="outInfo">响应内容</param>
        /// <returns>是否成功</returns>
        bool HttpPostMethod(string url, object info, int type, out string outInfo);

        /// <summary>
        /// HTTP POST 请求（异步）
        /// </summary>
        /// <param name="url">请求 URL</param>
        /// <param name="info">请求参数对象</param>
        /// <param name="type">请求类型</param>
        /// <returns>HTTP 结果</returns>
        Task<HttpResult> HttpPostAsync(string url, object info, int type);
    }
}


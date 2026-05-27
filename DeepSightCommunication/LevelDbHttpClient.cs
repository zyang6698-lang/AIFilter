using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DeepSightCommunication.Interfaces;

using DeepSightTool;
using Newtonsoft.Json;

namespace DeepSightCommunication
{
    /// <summary>
    /// LevelDB 操作类型，用于日志与告警内容区分
    /// </summary>
    public enum LevelDbOperation
    {
        /// <summary>读取数据（get / list 等）</summary>
        Read,
        /// <summary>写入数据（put / 回写等）</summary>
        Write
    }

    /// <summary>
    /// LevelDB HTTP 客户端：通过 HTTP POST 与 LevelDB 后端服务通信
    /// </summary>
    public class LevelDbHttpClient : ILevelDbHttpClient
    {
        /// <summary>
        /// HTTP 请求超时时间（毫秒）
        /// </summary>
        private const int HttpTimeoutMs = 8000;

        /// <summary>
        /// 共享的 HttpClient 实例（线程安全，推荐复用）
        /// </summary>
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(HttpTimeoutMs)
        };

        /// <summary>
        /// JSON 序列化设置（忽略空值）
        /// </summary>
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// 同步发送 LevelDB 请求
        /// </summary>
        public bool PostJson(string url, object payload, LevelDbOperation operation, out string response, string scene = null)
        {
            response = string.Empty;
            string tag = BuildLogTag(operation, scene);

            try
            {
                string payloadJson = JsonConvert.SerializeObject(payload, Formatting.None, _jsonSettings);
                LogTextHelper.Info($"AI-->DB [{tag}]:{payloadJson}");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Timeout = HttpTimeoutMs;
                request.ContentType = "application/json";

                byte[] data = Encoding.UTF8.GetBytes(payloadJson);
                request.ContentLength = data.Length;

                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                }

                using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
                using (Stream responseStream = webResponse.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    response = reader.ReadToEnd();
                }

                LogTextHelper.Info($"DB-->AI [{tag}]:{response}");
                return true;
            }
            catch (Exception ex)
            {
                response = string.Empty;
                LogTextHelper.ErrorFormat("LevelDB {0} 异常: {1}", tag, ex);
                return false;
            }
        }

        /// <summary>
        /// 异步发送 LevelDB 请求
        /// </summary>
        public async Task<HttpResult> PostJsonAsync(string url, object payload, LevelDbOperation operation, string scene = null)
        {
            var result = new HttpResult();
            string tag = BuildLogTag(operation, scene);

            try
            {
                string payloadJson = JsonConvert.SerializeObject(payload, Formatting.None, _jsonSettings);

                using (var content = new StringContent(payloadJson, Encoding.UTF8, "application/json"))
                using (var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false))
                {
                    result.Response = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    result.Success = response.IsSuccessStatusCode;

                    if (!response.IsSuccessStatusCode)
                    {
                        LogTextHelper.Warn($"LevelDB {tag} 返回非成功状态码: {response.StatusCode}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                result.Success = false;
                result.Response = "请求超时";
                LogTextHelper.Error($"LevelDB {tag} 请求超时: {url}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Response = ex.Message;
                LogTextHelper.Error($"LevelDB {tag} 请求失败: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 构造日志标签：业务场景优先，其次根据操作类型给出默认描述
        /// </summary>
        private static string BuildLogTag(LevelDbOperation operation, string scene)
        {
            if (!string.IsNullOrWhiteSpace(scene)) return scene;
            return operation == LevelDbOperation.Write ? "回写数据" : "请求数据";
        }
    }

    /// <summary>
    /// HTTP 返回结果对象
    /// </summary>
    public class HttpResult
    {
        /// <summary>是否成功</summary>
        public bool Success { get; set; }

        /// <summary>响应内容</summary>
        public string Response { get; set; }
    }
}

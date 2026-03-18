using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DeepSightCommunication.Interfaces;
using DeepSightEvent;
using DeepSightTool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeepSightCommunication
{
    /// <summary>
    /// HTTP 服务实现类，用于与 LevelDB 等后端服务通信
    /// </summary>
    public class HttpClass : IHttpService
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
        /// http操作LevelDB（同步方法）
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="info">请求参数</param>
        /// <param name="type">请求类型 0：请求数据  1：回写数据</param>
        /// <param name="outInfo">输出响应内容</param>
        /// <returns>是否成功</returns>
        public bool HttpPostMethod(string url, object info, int type, out string outInfo)
        {
            bool result = false;
            outInfo = string.Empty;

            try
            {
                string infoJson = JsonConvert.SerializeObject(info, Formatting.None, _jsonSettings);
                LogTextHelper.Info(string.Format("AI-->DB {1}:{0}", infoJson, type == 0 ? "请求数据" : "回写数据"));

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Timeout = HttpTimeoutMs;
                request.ContentType = "application/json";

                byte[] data = Encoding.UTF8.GetBytes(infoJson);
                request.ContentLength = data.Length;

                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                }

                // 修复：使用 using 确保 response 和 stream 正确释放
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    outInfo = reader.ReadToEnd();
                }

                LogTextHelper.Info($"DB-->AI:{outInfo}");
                result = true;
            }
            catch (Exception ex)
            {
                outInfo = string.Empty;
                result = false;
                LogTextHelper.Error("LevelDB拉取数据异常" + ex.ToString());
                SystemEvent.SendAlarmMsg("LevelDB拉取数据异常,详情请见LOG");
            }

            return result;
        }

        /// <summary>
        /// http操作LevelDB（真正的异步方法）
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="info">请求参数</param>
        /// <param name="type">请求类型 0：请求数据  1：回写数据</param>
        /// <returns>HTTP 请求结果</returns>
        public async Task<HttpResult> HttpPostAsync(string url, object info, int type)
        {
            var result = new HttpResult();

            try
            {
                string infoJson = JsonConvert.SerializeObject(info, Formatting.None, _jsonSettings);

                using (var content = new StringContent(infoJson, Encoding.UTF8, "application/json"))
                using (var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false))
                {
                    result.Response = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    result.Success = response.IsSuccessStatusCode;

                    if (!response.IsSuccessStatusCode)
                    {
                        LogTextHelper.Warn($"HTTP请求返回非成功状态码: {response.StatusCode}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                result.Success = false;
                result.Response = "请求超时";
                LogTextHelper.Error($"HTTP请求超时: {url}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Response = ex.Message;
                LogTextHelper.Error($"HTTP请求失败: {ex.Message}");
            }

            return result;
        }
    }

   
    //Http返回结果对象
    public class HttpResult
    {
        public bool Success { get; set; }
        public string Response { get; set; }
    }
}

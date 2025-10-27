using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DeepSightEvent;
using DeepSightTool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeepSightCommunication
{
    public class HttpClass
    {
        /// <summary>
        /// http操作LevelDB
        /// </summary>
        /// <param name="info">请求参数</param>
        /// <param name="type">请求类型 0：请求数据  1：回写数据</param>
        /// <returns></returns>
        public bool HttpPostMethod(string url, object info, int type, out string outInfo)
        {
            bool result = false;
            try
            {
                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };//去掉空值NULL
                string infoJson = JsonConvert.SerializeObject(info, Formatting.None, jsonSetting);
                LogTextHelper.Info(string.Format("AI-->DB {1}:{0}", infoJson, type == 0 ? "请求数据" : "回写数据"));

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Timeout = 8000;
                request.ContentType = "application/json";

                byte[] data = Encoding.UTF8.GetBytes(infoJson);
                request.ContentLength = data.Length;
                using (Stream reqstream = request.GetRequestStream())
                {
                    reqstream.Write(data, 0, data.Length);
                    reqstream.Close();
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();

                //获取响应内容
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    outInfo = reader.ReadToEnd();
                }

                LogTextHelper.Info($"DB-->AI:{outInfo}");
                result = true;
            }
            catch (Exception ex)
            {
                outInfo = "";
                result = false;
                LogTextHelper.Error("HTTP流程异常" + ex.ToString());
                SystemEvent.SendAlarmMsg("HTTP流程异常,详情请见LOG");
            }
            return result;
        }
        public bool HttpPostMethod2(string url, object info, int type, out string outInfo)
        {
            var task = HttpPostAsync(url, info, type);
            task.Wait();
            outInfo = task.Result.Response;
            return task.Result.Success;
        }
        public async Task<HttpResult> HttpPostAsync(string url, object info, int type)
        {
            var result = new HttpResult();

            try
            {
                // 1. 序列化数据
                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };//去掉空值NULL
                string infoJson = JsonConvert.SerializeObject(info, Formatting.None, jsonSetting);
                //if (url.Contains("zmq"))
                //{
                //    LogTextHelper.Info(string.Format("AI-->中台 {1}:{0}", infoJson, type == 0 ? "请求数据" : "回写数据"));
                //}
                //else
                //{
                //    LogTextHelper.Info(string.Format("AI-->DB {1}:{0}", infoJson, type == 0 ? "请求数据" : "回写数据"));
                //}
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Timeout = 8000;
                request.ContentType = "application/json";
                request.KeepAlive = true;
                byte[] data = Encoding.UTF8.GetBytes(infoJson);
                request.ContentLength = data.Length;

                using (Stream reqstream = request.GetRequestStream())
                {
                    reqstream.Write(data, 0, data.Length);
                    reqstream.Close();
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();

                //获取响应内容
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result.Response = Task.Run(async () => await reader.ReadToEndAsync()).GetAwaiter().GetResult();
                    result.Success = true;
                }
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

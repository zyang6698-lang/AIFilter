using Newtonsoft.Json;
using System.Collections.Generic;

namespace DeepSightModel
{
    /// <summary>
    /// AVI响应根对象
    /// </summary>
    public class AviResponse
    {
        [JsonProperty("data_list")]
        public List<object> DataList { get; set; }

        [JsonProperty("db_name")]
        public string DbName { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }

        [JsonProperty("operation")]
        public string Operation { get; set; }

        [JsonProperty("request_time")]
        public string RequestTime { get; set; }

        [JsonProperty("response_time")]
        public string ResponseTime { get; set; }

        [JsonProperty("result")]
        public bool Result { get; set; }

        [JsonProperty("resultCode")]
        public int ResultCode { get; set; }

        [JsonProperty("uniqueKey")]
        public string UniqueKey { get; set; }
    }

    /// <summary>
    /// data_list中的项对象
    /// </summary>
    public class AviDataItem
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// value字段中的对象（需要从JSON字符串反序列化）
    /// </summary>
    public class AviValueData
    {
        [JsonProperty("serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty("results_info")]
        public List<AviResultInfo> ResultsInfo { get; set; }
    }

    /// <summary>
    /// results_info中的项对象
    /// </summary>
    public class AviResultInfo
    {
        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("minio_ip")]
        public string MinioIp { get; set; }

        [JsonProperty("minio_port")]
        public int MinioPort { get; set; }

        [JsonProperty("result_path")]
        public string ResultPath { get; set; }
    }
}


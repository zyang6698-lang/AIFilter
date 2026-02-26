using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DeepSightDB
{
    //AI回写数据类
    public class RootAIResult
    {
        [JsonProperty("db_name")]
        public string DbName { get; set; }

        [JsonProperty("operation")]
        public string Operation { get; set; }

        [JsonProperty("op_mode")]
        public string OpMode { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// 回写目标 LevelDB 服务器 URL（不参与JSON序列化，仅用于路由回写请求）
        /// </summary>
        [JsonIgnore]
        public string TargetUrl { get; set; }
    }

    public class WriteBackData
    {
        [JsonProperty("result_infos")]
        public List<ResultInfo> ResultInfos { get; set; }
        [JsonProperty("serial_number")]
        public string SerialNumber { get; set; }
        [JsonProperty("panel_json_path")]
        public string PanelJsonPath { get; set; }
    }


    public class ResultInfo
    {
        [JsonProperty("result_infos")]
        public string ResultInfos { get; set; }

        [JsonProperty("details")]
        public Details Details { get; set; }
    }

    public class Details
    {

    }
}

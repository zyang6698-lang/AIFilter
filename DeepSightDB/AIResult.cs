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
        public string SN { get; set; }
        public string Side {  get; set; }
        public string DbName { get; set; }

        public string Operation { get; set; }

        public string OpMode { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// 回写目标 LevelDB 服务器 URL（不参与JSON序列化，仅用于路由回写请求）
        /// </summary>
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

    /// <summary>
    /// ai_detail_results_tovrs 表的 Value 项
    /// Key = SN + 面次
    /// </summary>
    public class AIDetailResultItem
    {
        /// <summary>pcs_id</summary>
        [JsonProperty("pcs_index")]
        public int PcsIndex { get; set; }

        /// <summary>缺陷_index</summary>
        [JsonProperty("index")]
        public int Index { get; set; }

        /// <summary>AI结果: OK / NG</summary>
        [JsonProperty("ai_label")]
        public string AiLabel { get; set; }

        /// <summary>AI分类类别</summary>
        [JsonProperty("ai_cls_type")]
        public string AiClsType { get; set; }

        /// <summary>AI判定依据（详情 JsonObject）</summary>
        [JsonProperty("infer_detail")]
        public Dictionary<string, object> InferDetail { get; set; }

        /// <summary>AI当前版本标记: experiment / stable</summary>
        [JsonProperty("ai_flag")]
        public string AiFlag { get; set; }
    }
}

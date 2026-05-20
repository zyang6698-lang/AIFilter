using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DeepSightDB
{
    //请求levelDB字段
    public class RootDbInfo
    {
        //uuid  唯⼀标识
        public string uniqueKey { get; set; }
        // db_name
        public string db_name { get; set; }
        //操作
        public string operation { get; set; }
        //
        public string is_select_range { get; set; }

        // ap --  相同 key的value后⾯附加
        // last_ow -- 替换最新数据
        // all_ow --   覆盖所有数据
        public string op_mode { get; set; }
        //kv列表
        public string is_batch { get; set; }
        //sn
        public string key { get; set; }
        public string value { get; set; }
        //时间戳
        public string range_start { get; set; }
        public string range_end { get; set; }
        //put batch时用，数组内时key value的obj 需要转成字符串
        public List<object> kv_list { get; set; }

    }

    /// <summary>
    /// LevelDB 心跳响应中的单个 DB 状态
    /// </summary>
    public class DbStatusInfo
    {
        [JsonProperty("db_name")]
        public string DbName { get; set; }

        [JsonProperty("get_status")]
        public bool GetStatus { get; set; }

        [JsonProperty("get_status_err")]
        public string GetStatusErr { get; set; }

        [JsonProperty("put_status")]
        public bool PutStatus { get; set; }

        [JsonProperty("put_status_err")]
        public string PutStatusErr { get; set; }

        [JsonProperty("threads_count")]
        public int ThreadsCount { get; set; }
    }

    /// <summary>
    /// LevelDB 心跳（heartbeat）响应
    /// </summary>
    public class HeartbeatResponse
    {
        [JsonProperty("db_count")]
        public int DbCount { get; set; }

        [JsonProperty("db_status")]
        public List<DbStatusInfo> DbStatus { get; set; }

        [JsonProperty("request_time")]
        public string RequestTime { get; set; }

        [JsonProperty("response_time")]
        public string ResponseTime { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("uniqueKey")]
        public string UniqueKey { get; set; }
    }

}

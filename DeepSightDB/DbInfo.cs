using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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



}

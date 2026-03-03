using Newtonsoft.Json;
using System.Collections.Generic;

namespace DeepSightDB
{
    /// <summary>
    /// .dpst 文件的数据结构
    /// </summary>
    public class DpstInfo
    {
        [JsonProperty("channel")]
        public List<int> Channel { get; set; } = new List<int>();

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("image_label")]
        public Dictionary<string, object> ImageLabel { get; set; } = new Dictionary<string, object>();

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("local")]
        public List<string> Local { get; set; } = new List<string>();

        [JsonProperty("source")]
        public List<string> Source { get; set; } = new List<string>();

        [JsonProperty("shapes")]
        public Dictionary<string, object> Shapes { get; set; } = new Dictionary<string, object>();

        [JsonProperty("imageMetaData")]
        public Dictionary<string, object> ImageMetaData { get; set; } = new Dictionary<string, object>();
    }
}


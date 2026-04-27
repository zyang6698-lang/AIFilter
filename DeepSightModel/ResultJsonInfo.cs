using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DeepSightModel
{
    /// <summary>
    /// 推理结果响应模型 - 对应外层 JSON 结构
    /// 
    /// 示例 JSON:
    /// {
    ///     "key": "20260326143651854216",
    ///     "msg": "",
    ///     "request_time": "20260425163021313896",
    ///     "response_time": "20260425163021314014",
    ///     "result": "OK",
    ///     "resultCode": 0,
    ///     "uniqueKey": "",
    ///     "value": "{...};{...};{...}"
    /// }
    /// </summary>
    public class VRSResultResponse
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }

        [JsonProperty("request_time")]
        public string RequestTime { get; set; }

        [JsonProperty("response_time")]
        public string ResponseTime { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("resultCode")]
        public int ResultCode { get; set; }

        [JsonProperty("uniqueKey")]
        public string UniqueKey { get; set; }

        /// <summary>
        /// value 原始字符串，包含分号(;)分隔的多个 JSON 对象。
        /// 每个对象格式: {"AsideInfo":[...],"BsideInfo":[...]}
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// 解析 value 字段后得到的 AsideInfo/BsideInfo 数据块列表。
        /// 仅在调用 <see cref="InferenceResultParser.Parse(string)"/> 或
        /// <see cref="InferenceResultParser.ParseValue(VRSResultResponse)"/> 后填充。
        /// </summary>
        [JsonIgnore]
        public List<SideInfoBlock> SideInfoBlocks { get; set; }

        /// <summary>
        /// 所有 AsideInfo 条目合并后的扁平列表（快捷访问）
        /// </summary>
        [JsonIgnore]
        public List<string> AllAsideInfo
        {
            get
            {
                var result = new List<string>();
                if (SideInfoBlocks != null)
                {
                    foreach (var block in SideInfoBlocks)
                    {
                        if (block.AsideInfo != null)
                            result.AddRange(block.AsideInfo);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 所有 BsideInfo 条目合并后的扁平列表（快捷访问）
        /// </summary>
        [JsonIgnore]
        public List<string> AllBsideInfo
        {
            get
            {
                var result = new List<string>();
                if (SideInfoBlocks != null)
                {
                    foreach (var block in SideInfoBlocks)
                    {
                        if (block.BsideInfo != null)
                            result.AddRange(block.BsideInfo);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 所有条目合并后的扁平列表（快捷访问）
        /// </summary>
        [JsonIgnore]
        public List<string> AllEntries
        {
            get
            {
                var result = new List<string>();
                if (SideInfoBlocks != null)
                {
                    foreach (var block in SideInfoBlocks)
                    {
                        if (block.AsideInfo != null)
                            result.AddRange(block.AsideInfo);
                        if (block.BsideInfo != null)
                            result.AddRange(block.BsideInfo);
                    }
                }
                return result;
            }
        }
    }

    /// <summary>
    /// value 字段中每个分号(;)分隔的 JSON 数据块模型。
    /// 例如: {"AsideInfo":["A_0_1_0_ok",...,"A_17_2_0_ok"],"BsideInfo":["B_1_1_0_ok",...]}
    /// </summary>
    public class SideInfoBlock
    {
        /// <summary>A 面信息列表，格式: "面别_工位号_缺陷编号_结果码_扩展信息"</summary>
        [JsonProperty("AsideInfo")]
        public List<string> AsideInfo { get; set; } = new List<string>();

        /// <summary>B 面信息列表，格式: "面别_工位号_缺陷编号_结果码_扩展信息"</summary>
        [JsonProperty("BsideInfo")]
        public List<string> BsideInfo { get; set; } = new List<string>();

        /// <summary>是否包含任意数据</summary>
        [JsonIgnore]
        public bool HasData => (AsideInfo != null && AsideInfo.Count > 0)
                            || (BsideInfo != null && BsideInfo.Count > 0);
    }

    /// <summary>
    /// 推理结果 JSON 解析器
    /// </summary>
    public static class InferenceResultParser
    {
        /// <summary>
        /// 从完整的 JSON 字符串反序列化并解析 value 字段中的多段数据。
        /// </summary>
        /// <param name="json">完整的 JSON 字符串</param>
        /// <returns>解析后的 <see cref="VRSResultResponse"/> 对象</returns>
        public static VRSResultResponse Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var response = JsonConvert.DeserializeObject<VRSResultResponse>(json);
            if (response != null)
            {
                ParseValue(response);
            }
            return response;
        }

        /// <summary>
        /// 解析 <see cref="VRSResultResponse.Value"/> 字段，
        /// 将分号分隔的多段 JSON {"AsideInfo":...,"BsideInfo":...} 解析为 <see cref="SideInfoBlock"/> 列表。
        /// </summary>
        /// <param name="response">待解析的响应对象（会原地修改其 SideInfoBlocks 属性）</param>
        public static void ParseValue(VRSResultResponse response)
        {
            if (response == null || string.IsNullOrWhiteSpace(response.Value))
            {
                response.SideInfoBlocks = new List<SideInfoBlock>(0);
                return;
            }

            var blocks = new List<SideInfoBlock>();
            // value 可能包含多个 JSON 对象，用分号(;)分隔
            var parts = response.Value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmed = part?.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                try
                {
                    var block = JsonConvert.DeserializeObject<SideInfoBlock>(trimmed);
                    if (block != null && block.HasData)
                    {
                        blocks.Add(block);
                    }
                }
                catch (JsonReaderException)
                {
                    // 忽略无法解析的片段，避免解析中断
                }
            }

            response.SideInfoBlocks = blocks;
        }

        /// <summary>
        /// 将原始 JSON 字符串直接解析为 <see cref="SideInfoBlock"/> 列表。
        /// 适用于 value 字段本身就已经是分号分隔的多段 JSON 的情况。
        /// </summary>
        /// <param name="value">value 字段字符串</param>
        /// <returns>解析后的 <see cref="SideInfoBlock"/> 列表</returns>
        public static List<SideInfoBlock> ParseValueBlocks(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<SideInfoBlock>(0);

            var blocks = new List<SideInfoBlock>();
            var parts = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmed = part?.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                try
                {
                    var block = JsonConvert.DeserializeObject<SideInfoBlock>(trimmed);
                    if (block != null && block.HasData)
                    {
                        blocks.Add(block);
                    }
                }
                catch (JsonReaderException)
                {
                    // 忽略无法解析的片段
                }
            }

            return blocks;
        }

        /// <summary>
        /// 判断 JSON 字符串是否为合法的 <see cref="VRSResultResponse"/> 格式
        /// </summary>
        /// <param name="json">待验证的 JSON 字符串</param>
        /// <returns>true 表示合法，false 表示非法</returns>
        public static bool TryValidate(string json)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<VRSResultResponse>(json);
                return response != null;
            }
            catch
            {
                return false;
            }
        }
    }
}

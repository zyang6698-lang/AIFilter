using System.Collections.Generic;

namespace DeepSightModel
{
    /// <summary>
    /// VRS 判定结果码
    /// </summary>
    public enum VrsResultCode
    {
        Ok = 0,
        Ng = 1,
        Ignore = 2,
        NoResult = 3,
        NotAcceptNg = 4
    }

    /// <summary>
    /// 单条 VRS 记录（由形如 "A_0_1_0_ok" 的字符串解析而来）
    /// 格式：Side_pcsIndex_defectIndex_vrs结果_扩展信息
    /// </summary>
    public class VrsEntry
    {
        /// <summary>面别：A 或 B</summary>
        public string Side { get; set; }

        /// <summary>pcs 编号</summary>
        public int PcsIndex { get; set; }

        /// <summary>缺陷编号</summary>
        public int DefectIndex { get; set; }

        /// <summary>VRS 结果码原始整数（0~4）</summary>
        public int ResultCode { get; set; }

        /// <summary>VRS 结果枚举（与 ResultCode 对应）</summary>
        public VrsResultCode Result { get; set; }

        /// <summary>第 4 段及之后的扩展信息（如 "ok" / 数字代码 / "null"）</summary>
        public string Extra { get; set; }

        /// <summary>原始字符串 token</summary>
        public string Raw { get; set; }
    }

    /// <summary>
    /// vrs_history_result 表中某个 SN 对应的完整 VRS 数据
    /// </summary>
    public class VrsHistoryResult
    {
        /// <summary>查询使用的 SN</summary>
        public string Sn { get; set; }

        /// <summary>A 面原始 token 列表</summary>
        public List<string> AsideInfo { get; set; } = new List<string>();

        /// <summary>B 面原始 token 列表</summary>
        public List<string> BsideInfo { get; set; } = new List<string>();

        /// <summary>解析后的结构化条目（A/B 全部，顺序与 AsideInfo + BsideInfo 一致）</summary>
        public List<VrsEntry> Entries { get; set; } = new List<VrsEntry>();
    }
}

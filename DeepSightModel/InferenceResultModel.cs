using System;
using System.Collections.Generic;

namespace DeepSightModel
{
    /// <summary>
    /// 推理结果模型（用于异步后处理）
    /// </summary>
    public class InferenceResultModel
    {
        /// <summary>
        /// 推理返回的原始 JSON 字符串
        /// </summary>
        public string RawJsonResult { get; set; }

        /// <summary>
        /// VBModel 信息
        /// </summary>
        public VBModel VBModel { get; set; }

        /// <summary>
        /// 推理完成时间
        /// </summary>
        public DateTime InferenceCompletedTime { get; set; }

        /// <summary>
        /// 是否需要处理（用于标记是否跳过后处理）
        /// </summary>
        public bool NeedsProcessing { get; set; } = true;
    }
}


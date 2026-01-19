using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightModel
{
    public class VBModel
    {
        public string Key { get; set; }
        public string SN { get; set; }
        public string Side { get; set; }
        public List<int> DefectIndex { get; set; }
        public List<int> PcsIndex { get; set; }
        public List<Mat> Mats { get;set; }
        public List<string > ImageKeys { get; set; }

        public RootVBInfo VbInfo;
        //2025/08/21/增加minio路径信息
        public string  minioPath;
        public RootPanelInfo panelInfo { get; set; }
        //用于判断ai是否部署该料号，若未部署则为true
        public bool isByPass { get; set; } = false;

        #region 模型验证测试相关属性
        /// <summary>
        /// 是否为模型验证测试任务（用于区分正常推理和测试推理）
        /// </summary>
        public bool IsValidationTest { get; set; } = false;

        /// <summary>
        /// 原始推理结果（用于比对）格式: defectIndex -> AIStatus
        /// </summary>
        public Dictionary<int, int> OriginalAIResults { get; set; }

        /// <summary>
        /// 测试任务 ID
        /// </summary>
        public string TestTaskId { get; set; }
        #endregion
    }

    /// <summary>
    /// 图片加载模型，用于解耦图片读取和推理
    /// </summary>
    public class ImageLoadModel
    {
        /// <summary>
        /// VB模型数据
        /// </summary>
        public VBModel Model { get; set; }


        /// <summary>
        /// 用于发送PanelInfo的对象
        /// </summary>
        public RootPanelInfoWithIP RootPanelInfo { get; set; }
    }
}

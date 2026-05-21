using DeepSightModel;
using OpenCvSharp;
using System.Collections.Generic;

namespace DeepSightWorkLib.Interfaces
{
    /// <summary>
    /// 缺陷检测服务接口
    /// </summary>
    public interface IDefectService
    {
        /// <summary>
        /// AI 引擎是否初始化成功（ProxyServer.dll 加载成功）
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 初始化失败时的错误信息（成功时为 null）
        /// </summary>
        string InitError { get; }

        /// <summary>
        /// 获取底层 AI 检测类实例（用于需要直接访问的场景）
        /// </summary>
        AI_DefectClass AiDefect { get; }

        /// <summary>
        /// 原有推理方法（C++ 端从 Minio 读取图片）
        /// </summary>
        /// <param name="info">推理参数信息</param>
        /// <param name="vb_outStr">推理结果输出</param>
        void DefectMethod(RootVBInfo info, out string vb_outStr);

        /// <summary>
        /// 带图片数据的推理方法（直接传递图片指针给 C++，避免 C++ 读取 Minio）
        /// </summary>
        /// <param name="info">推理参数信息</param>
        /// <param name="mats">图片数据列表（已解码的 Mat）</param>
        /// <param name="vb_outStr">推理结果输出</param>
        void DefectMethodWithImages(RootVBInfo info, List<Mat> mats, out string vb_outStr);

        /// <summary>
        /// 带图片数据的推理方法（原图+模板图，直接传递图片指针给 C++）
        /// </summary>
        /// <param name="info">推理参数信息</param>
        /// <param name="mats">原图数据列表（已解码的 Mat）</param>
        /// <param name="mats_Tmp">模板图数据列表（已解码的 Mat）</param>
        /// <param name="vb_outStr">推理结果输出</param>
        void DefectMethodWithImages2(RootVBInfo info, List<Mat> mats, List<Mat> mats_Tmp, out string vb_outStr);

        /// <summary>
        /// 多批图像推理方法（将所有图片一次性传递给 C++，由 C++ 端按 defect_count × img_count_each_defect 分批推理）
        /// </summary>
        /// <param name="info">推理参数信息</param>
        /// <param name="mats">所有待推理图片列表（已解码的 Mat），总数量 = defect_count × img_count_each_defect</param>
        /// <param name="defectCount">待推理的缺陷数量</param>
        /// <param name="imgCountEachDefect">单个缺陷所需的图片数量</param>
        /// <param name="vb_outStr">推理结果输出</param>
        void DefectMethodWithAllImages(RootVBInfo info, List<Mat> mats, int defectCount, int imgCountEachDefect, out string vb_outStr);
    }
}


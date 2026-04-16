using DeepSightCommunication.Interfaces;
using DeepSightDB;
using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using DeepSightWorkLib.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepSightWorkLib.Services.Pipeline.Stages
{
    /// <summary>
    /// 阶段1: JSON 解析 — 从 MinIO 读取 JSON 并转换为 VBModel
    /// 对应原 BusinessClass.ReadJsonByMinio
    /// </summary>
    public class JsonParseStage
    {
        private readonly IMinioService _minioService;
        private readonly ImageLoaderService _imageLoaderService;
        private readonly IPanelDataConverter _panelDataConverter;
        private readonly Func<PanelConvertContext> _convertContextFactory;

        /// <summary>
        /// 创建 JSON 解析阶段
        /// </summary>
        /// <param name="minioService">MinIO 服务</param>
        /// <param name="imageLoaderService">图片加载服务（用于获取图片路径列表）</param>
        /// <param name="panelDataConverter">面板数据转换器</param>
        /// <param name="convertContextFactory">PanelConvertContext 工厂委托，每次调用获取最新配置快照</param>
        public JsonParseStage(
            IMinioService minioService,
            ImageLoaderService imageLoaderService,
            IPanelDataConverter panelDataConverter,
            Func<PanelConvertContext> convertContextFactory)
        {
            _minioService = minioService ?? throw new ArgumentNullException(nameof(minioService));
            _imageLoaderService = imageLoaderService ?? throw new ArgumentNullException(nameof(imageLoaderService));
            _panelDataConverter = panelDataConverter ?? throw new ArgumentNullException(nameof(panelDataConverter));
            _convertContextFactory = convertContextFactory ?? throw new ArgumentNullException(nameof(convertContextFactory));
        }

        /// <summary>
        /// 执行 JSON 解析阶段（供 Pipeline TransformBlock 使用）
        /// </summary>
        public PipelineContext Execute(PipelineContext ctx)
        {
            // 如果 LoadModel 已就绪（离线/验证测试任务已预构建 VBModel），直接跳过 JSON 解析阶段
            if (ctx.LoadModel != null)
            {
                LogTextHelper.Info($"{ctx.SN} {ctx.Side} LoadModel已预设，跳过JSON解析阶段（离线任务）");
                return ctx;
            }

            var aviCtx = ctx.AviContext;
            var ip = aviCtx.MinioIp;
            var port = aviCtx.MinioPort;
            var key = aviCtx.Key;
            var head = aviCtx.Head;
            var sn = aviCtx.SerialNumber;
            var side = aviCtx.Side;
            var path = aviCtx.MinioPath;
            var writeBackDbName = aviCtx.WriteBackDbName;
            var dbUrl = aviCtx.DbUrl;
            var vrsWriteBackDbName = aviCtx.VrsWriteBackDbName;

            TaskStatusSender.SendQueued(sn, side);
            string json = _minioService.ReadJsonSync("deepiresults", path, ip);

            if (string.IsNullOrWhiteSpace(json))
            {
                ctx.SetError("JSON解析", $"从Minio读取的JSON为空，路径={path}, IP={ip}");
                return ctx;
            }

            var obj = JsonConvert.DeserializeObject<RootPanelInfo>(json);
            if (obj == null)
            {
                ctx.SetError("JSON解析", $"JSON反序列化为RootPanelInfo失败，路径={path}");
                return ctx;
            }

            LogTextHelper.Info($"{sn} {side} 开始将json转为vbinfo");

            // 通过工厂获取最新配置快照，避免 SolConfig/AviConfig 并发修改问题
            var convertContext = _convertContextFactory();
            convertContext.SN= sn;
            convertContext.MinioIP = ip;
            convertContext.MinioPort = port;
            convertContext.Head = head;

            var convertResult = _panelDataConverter.Convert(obj, convertContext);

            RootPanelInfoWithIP rootobj = new RootPanelInfoWithIP()
            {
                IP = ip,
                Head = head,
                RootInfo = obj,
            };

            var (AllImageKeys, AllGerberKeys, AllTempKeys, DirectReportFlags, AllDefectCodes)
                = _imageLoaderService.GetAllImageKeysWithDirectReportFlags(rootobj);
            var (ImageKeys, GerberKeys, TempKeys)
                = ImageLoaderService.DeriveFilteredKeys(AllImageKeys, AllGerberKeys, AllTempKeys, DirectReportFlags);

            VBModel model = new VBModel
            {
                Key = key,
                SN = sn,
                Side = side,
                DefectIndex = convertResult.DefectIndexList,
                PcsIndex = convertResult.PcsIndexList,
                VbInfo = convertResult.VBInfo,
                minioPath = head,
                panelInfo = obj,
                ImageKeys = ImageKeys,
                ImageKeys_Gerber = GerberKeys,
                ImageKeys_Temp = TempKeys,
                AllDefectImageKeys = AllImageKeys,
                AllDefectGerberKeys = AllGerberKeys,
                AllDefectTempKeys = AllTempKeys,
                DirectReportFlags = DirectReportFlags,
                AllDefectCodes = AllDefectCodes,
                SourceDbUrl = dbUrl,
                SourceWriteBackDbName = writeBackDbName,
                SourceVRSWriteBackDbName = vrsWriteBackDbName,
                DirectReportDefectIndices = convertResult.DirectReportDefectIndices,
                DirectReportPcsIndices = convertResult.DirectReportPcsIndices
            };

            // 存储调试信息到缓存
            StoreDebugInfo(sn, side, json, convertResult, ImageKeys, head, obj);

            ctx.LoadModel = new ImageLoadModel
            {
                Model = model,
                RootPanelInfo = rootobj
            };

            LogTextHelper.Info($"{sn} {side} JSON解析完成，待加载图片数量:{ImageKeys.Count}");
            return ctx;
        }

        private void StoreDebugInfo(string sn, string side, string json,
            PanelConvertResult convertResult, List<string> imageKeys, string head, RootPanelInfo obj)
        {
            try
            {
                var debugInfo = SnDebugInfoCache.GetOrCreate(sn, side);
                debugInfo.PanelInfoJson = json;
                debugInfo.VbInferenceJson = JsonConvert.SerializeObject(convertResult.VBInfo, Formatting.Indented);
                debugInfo.DefectCount = convertResult.DefectIndexList?.Count ?? 0;
                debugInfo.PcsCount = convertResult.PcsIndexList?.Count ?? 0;
                debugInfo.ImageCount = imageKeys?.Count ?? 0;
                debugInfo.MinioPath = head;
                debugInfo.ProductSerial = obj.ProductSerial;
                debugInfo.LotNumber = obj.LotId ?? obj.LotBatch;

                var defectCodes = new List<string>();
                if (obj.PcsInfo != null)
                {
                    foreach (var pcs in obj.PcsInfo.Values)
                    {
                        if (pcs?.DefectInfo != null)
                        {
                            foreach (var d in pcs.DefectInfo)
                            {
                                if (!string.IsNullOrEmpty(d.DefectCode))
                                    defectCodes.Add(d.DefectCode);
                            }
                        }
                    }
                }
                var summary = new System.Text.StringBuilder();
                if (defectCodes.Count > 0)
                    summary.Append($"AVI报点{defectCodes.Count}个: {string.Join(",", defectCodes.Distinct())}");
                else
                    summary.Append("AVI无报点");
                if (convertResult.DirectReportDefectIndices?.Count > 0)
                    summary.Append($" | 直报{convertResult.DirectReportDefectIndices.Count}个");
                debugInfo.JudgmentSummary = summary.ToString();

                SnDebugInfoCache.Cleanup();
            }
            catch (Exception debugEx)
            {
                LogTextHelper.Warn($"存储SN调试信息异常: {debugEx.Message}");
            }
        }
    }
}

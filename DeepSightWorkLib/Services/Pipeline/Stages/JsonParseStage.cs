using DeepSightCommunication.Interfaces;
using DeepSightDB;
using DeepSightEvent;
using DeepSightModel;
using DeepSightModel.Alarm;
using DeepSightModel.Configuration;
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
            var vrsDbUrl = aviCtx.VrsDbUrl;

            TaskStatusSender.SendQueued(sn, side);
            string json = _minioService.ReadJsonSync("deepiresults", path, ip);

            if (string.IsNullOrWhiteSpace(json))
            {
                ctx.SetError("JSON解析", $"从Minio读取的JSON为空，路径={path}, IP={ip}");
                return ctx;
            }

            var panelInfo = JsonConvert.DeserializeObject<RootPanelInfo>(json);
            if (panelInfo == null)
            {
                ctx.SetError("JSON解析", $"JSON反序列化为RootPanelInfo失败，路径={path}");
                return ctx;
            }

            // process_status 非 normal 时告警（如 over_max_count 会导致 pcs_info 为空数组，此时 PcsInfo 为 null）
            if (!string.IsNullOrEmpty(panelInfo.ProcessStatus) &&
                !string.Equals(panelInfo.ProcessStatus, "normal", StringComparison.OrdinalIgnoreCase))
            {
                RaiseProcessStatusAbnormalAlarm(sn, side, panelInfo.ProcessStatus, path);
            }

            // line_name 正常情况下不应为空；为空时降级为默认值并告警，提示运维确认 AVI 机台配置
            if (string.IsNullOrEmpty(panelInfo.LineName))
            {
                RaiseLineNameMissingAlarm(sn, side, path);
            }

            LogTextHelper.Info($"{sn} {side} 开始将json转为vbinfo");

            // 通过工厂获取最新配置快照，避免 SolConfig/AviConfig 并发修改问题
            var convertContext = _convertContextFactory();
            convertContext.SN= sn;
            convertContext.MinioIP = ip;
            convertContext.MinioPort = port;
            convertContext.Head = head;

            var convertResult = _panelDataConverter.Convert(panelInfo, convertContext);

            var (AllImageKeys, AllGerberKeys, AllTempKeys, DirectReportFlags, AllDefectCodes, GlobalFlags)
                = _imageLoaderService.GetAllImageKeysWithDirectReportFlags(panelInfo, ip, head);

            // 构造 UI 投影模型（缺陷展平顺序与 GetAllImageKeysWithDirectReportFlags 一致：先 PcsInfo 后 PanelInfo）
            var panelView = BuildPanelView(sn, side, ip, head, panelInfo);
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
                ProductSerial = panelInfo.ProductSerial,
                AviCreateTime = panelInfo.AviCreateTime,
                LotId = panelInfo.LotId,
                LineName = string.IsNullOrEmpty(panelInfo.LineName) ? DefaultValues.LineName : panelInfo.LineName,
                LocalDescribePath = panelInfo.LocalDescribePath,
                PanelSerialNumber = panelInfo.SerialNumber,
                ImageKeys = ImageKeys,
                ImageKeys_Gerber = GerberKeys,
                ImageKeys_Temp = TempKeys,
                AllDefectImageKeys = AllImageKeys,
                AllDefectGerberKeys = AllGerberKeys,
                AllDefectTempKeys = AllTempKeys,
                DirectReportFlags = DirectReportFlags,
                AllDefectCodes = AllDefectCodes,
                GlobalFlags = GlobalFlags,
                SourceDbUrl = dbUrl,
                SourceVRSDbUrl = vrsDbUrl,
                SourceWriteBackDbName = writeBackDbName,
                SourceVRSWriteBackDbName = vrsWriteBackDbName,
                DirectReportDefectIndices = convertResult.DirectReportDefectIndices,
                DirectReportPcsIndices = convertResult.DirectReportPcsIndices
            };

            // 存储调试信息到缓存
            StoreDebugInfo(sn, side, json, convertResult, ImageKeys, head, panelInfo);

            ctx.LoadModel = new ImageLoadModel
            {
                Model = model,
                PanelView = panelView
            };

            LogTextHelper.Info($"{sn} {side} JSON解析完成，待加载图片数量:{ImageKeys.Count}");
            return ctx;
        }

        /// <summary>
        /// 将 RootPanelInfo + IP/Head 投影为 PanelInfoView，缺陷展平顺序：先 PcsInfo，后 PanelInfo（全局点）
        /// </summary>
        private static PanelInfoView BuildPanelView(string sn, string side, string ip, string head, RootPanelInfo panelInfo)
        {
            var view = new PanelInfoView
            {
                SN = sn,
                Side = side,
                ProductSerial = panelInfo.ProductSerial,
                LineName = string.IsNullOrEmpty(panelInfo.LineName) ? DefaultValues.LineName : panelInfo.LineName,
                StationName = panelInfo.StationName,
                LotId = panelInfo.LotId,
                LotBatch = panelInfo.LotBatch,
                SideIndex = panelInfo.SideIndex,
                EndTime = panelInfo.EndTime,
                Head = head,
                IP = ip,
                Defects = new List<DefectInfoView>()
            };

            AppendDefectViews(view.Defects, panelInfo.PcsInfo?.Values);
            AppendDefectViews(view.Defects, panelInfo.PanelInfo != null ? new[] { panelInfo.PanelInfo } : null);
            return view;
        }

        private static void AppendDefectViews(List<DefectInfoView> dest, IEnumerable<PcsInfo> source)
        {
            if (source == null) return;
            foreach (var pcs in source)
            {
                if (pcs?.DefectInfo == null) continue;
                foreach (var d in pcs.DefectInfo)
                {
                    if (d == null) continue;
                    dest.Add(new DefectInfoView
                    {
                        DefectCode = d.DefectCode,
                        DefectIndex = d.DefectIndex,
                        PcsIndex = d.PcsIndex,
                        DefectLocation = d.DefectLocation,
                        AiInferResult = d.AiInferResult,
                        DefectRoi = d.DefectRoi,
                        DefectVrsImage = d.DefectVrsImages != null && d.DefectVrsImages.Count > 0 ? d.DefectVrsImages[0] : null,
                        DefectVrsGerberImage = d.DefectVrsGerberImages != null && d.DefectVrsGerberImages.Count > 0 ? d.DefectVrsGerberImages[0] : null,
                        DefectVrsOkImage = d.DefectVrsOkImages != null && d.DefectVrsOkImages.Count > 0 ? d.DefectVrsOkImages[0] : null,
                    });
                }
            }
        }

        /// <summary>
        /// process_status 非 normal 告警（如 over_max_count 等异常状态）
        /// </summary>
        private static void RaiseProcessStatusAbnormalAlarm(string sn, string side, string processStatus, string path)
        {
            string warnMsg = $"AVI处理状态异常: process_status={processStatus}，pcs_info可能为空，请确认AVI结果是否正常";
            LogTextHelper.Warn($"{sn} {side} {warnMsg}");
            try
            {
                AlarmService.Instance.RaiseAlarm(
                    AlarmLevel.Warning,
                    AlarmCategory.System,
                    "JsonParseStage.ProcessStatus",
                    warnMsg,
                    $"SN={sn}, Side={side}, Path={path}, ProcessStatus={processStatus}",
                    sn);
            }
            catch (Exception alarmEx)
            {
                LogTextHelper.Warn($"ProcessStatus 告警发起异常: {alarmEx.Message}");
            }
        }

        /// <summary>
        /// line_name 缺失告警：日志 + 结构化告警（30s 冷却由 AlarmService 内置处理）
        /// </summary>
        private static void RaiseLineNameMissingAlarm(string sn, string side, string path)
        {
            string warnMsg = $"linename为空，使用默认值{DefaultValues.LineName}，请确认AVI机台配置";
            LogTextHelper.Warn($"{sn} {side} {warnMsg}");
            try
            {
                AlarmService.Instance.RaiseAlarm(
                    AlarmLevel.Warning,
                    AlarmCategory.System,
                    "JsonParseStage.LineName",
                    warnMsg,
                    $"SN={sn}, Side={side}, Path={path}",
                    sn);
            }
            catch (Exception alarmEx)
            {
                LogTextHelper.Warn($"LineName 告警发起异常: {alarmEx.Message}");
            }
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

                debugInfo.ProcessStatus = obj.ProcessStatus;

                var defectCodes = new List<string>();
                CollectDefectCodes(obj.PcsInfo?.Values, defectCodes);
                CollectDefectCodes(obj.PanelInfo != null ? new[] { obj.PanelInfo } : null, defectCodes);
                var summary = new System.Text.StringBuilder();
                if (defectCodes.Count > 0)
                    summary.Append($"AVI报点{defectCodes.Count}个: {string.Join(",", defectCodes.Distinct())}");
                else
                    summary.Append("AVI无报点");
                if (convertResult.DirectReportDefectIndices?.Count > 0)
                    summary.Append($" | 直报{convertResult.DirectReportDefectIndices.Count}个");
                if (!string.IsNullOrEmpty(obj.ProcessStatus) &&
                    !string.Equals(obj.ProcessStatus, "normal", StringComparison.OrdinalIgnoreCase))
                {
                    summary.Append($" | ⚠ process_status={obj.ProcessStatus}(异常)");
                }
                debugInfo.JudgmentSummary = summary.ToString();

                SnDebugInfoCache.Cleanup();
            }
            catch (Exception debugEx)
            {
                LogTextHelper.Warn($"存储SN调试信息异常: {debugEx.Message}");
            }
        }

        private static void CollectDefectCodes(IEnumerable<PcsInfo> source, List<string> defectCodes)
        {
            if (source == null) return;
            foreach (var pcs in source)
            {
                if (pcs?.DefectInfo == null) continue;
                foreach (var d in pcs.DefectInfo)
                {
                    if (!string.IsNullOrEmpty(d.DefectCode))
                        defectCodes.Add(d.DefectCode);
                }
            }
        }
    }
}

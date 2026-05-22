using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DeepSightModel
{
    //定义panelInfo类
    /// <summary>
    /// PcsInfo 字典自定义 JsonConverter：当 JSON 中 pcs_info 为 []（空数组）时返回 null，
    /// 避免 Dictionary 反序列化类型不匹配异常（如 process_status=over_max_count 场景）
    /// </summary>
    public class PcsInfoDictionaryConverter : JsonConverter<Dictionary<string, PcsInfo>>
    {
        public override Dictionary<string, PcsInfo> ReadJson(
            JsonReader reader, Type objectType, Dictionary<string, PcsInfo> existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            // pcs_info = [] 空数组 -> 直接跳过并返回 null
            if (reader.TokenType == JsonToken.StartArray)
            {
                reader.Skip();
                return null;
            }

            // 正常 object 走标准 Dictionary 反序列化
            return serializer.Deserialize<Dictionary<string, PcsInfo>>(reader);
        }

        public override void WriteJson(JsonWriter writer, Dictionary<string, PcsInfo> value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    //定义panelInfo类
    public class RootPanelInfo
    {
        [JsonProperty("archive_path")]
        public string ArchivePath { get; set; }

        [JsonProperty("describe_path")]
        public string DescribePath { get; set; }

        [JsonProperty("disk_serial_number")]
        public string DiskSerialNumber { get; set; }

        [JsonProperty("duration_time")]
        public int DurationTime { get; set; }

        [JsonProperty("end_time")]
        public string EndTime { get; set; }

        [JsonProperty("indication_id")]
        public string IndicationId { get; set; }

        [JsonProperty("line_name")]
        public string LineName { get; set; }

        [JsonProperty("local_describe_dir"),Description("由于数据可能被移动过，会导致json内的路径和实际路径对不上的问题，以实际路径为准，这个尽量不使用")]
        public string LocalDescribeDir { get; set; }

        [JsonProperty("local_describe_path")]
        public string LocalDescribePath { get; set; }

        [JsonProperty("lot_id")]
        public string LotId { get; set; }

        [JsonProperty("lot_batch")]
        public string LotBatch { get; set; }

        [JsonProperty("machine_name")]
        public string MachineName { get; set; }

        [JsonProperty("panel_id")]
        public int PanelId { get; set; }

        /// <summary>
        /// 全局点信息（panel_info），结构与单个 PcsInfo 一致；可能为空
        /// </summary>
        [JsonProperty("panel_info")]
        public PcsInfo PanelInfo { get; set; }

        [JsonProperty("panel_source_image")]
        public PanelSourceImage PanelSourceImage { get; set; }

        [JsonProperty("panel_storage_info")]
        public List<PanelStorageInfo> PanelStorageInfo { get; set; }

        [JsonProperty("pcs_info")]
        [JsonConverter(typeof(PcsInfoDictionaryConverter))]
        public Dictionary<string, PcsInfo> PcsInfo { get; set; }

        [JsonProperty("process_status")]
        public string ProcessStatus { get; set; }

        [JsonProperty("process_time")]
        public string ProcessTime { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("product_serial")]
        public string ProductSerial { get; set; }

        [JsonProperty("serial_level")]
        public string SerialLevel { get; set; }
        /// <summary>
        /// 这个属性目前只有生成推理请求和VRS回写的key会用到，由于AB面SN有差异认为这个参数是不可信的
        /// </summary>
        [JsonProperty("serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty("side_index")]
        public string SideIndex { get; set; }

        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("station_name")]
        public string StationName { get; set; }
        [JsonProperty("result_ini_create_time")]
        public string AviCreateTime { get; set; }
        [JsonProperty("path_index")]
        public string PathIndex { get; set; }
        [JsonProperty("template_img_path")] 
        public string TemplateImgPath { get; set; }
    }

    public class PanelSourceImage
    {
        [JsonProperty("path")]
        public List<string> Path { get; set; }

        [JsonProperty("scale")]
        public float Scale { get; set; }
    }

    public class PanelStorageInfo
    {
        [JsonProperty("new_storage_panel_path")]
        public List<string> NewStoragePanelPath { get; set; }

        [JsonProperty("storage_panel_channels")]
        public List<int> StoragePanelChannels { get; set; }

        [JsonProperty("storage_panel_path")]
        public string StoragePanelPath { get; set; }

        [JsonProperty("storage_panel_resize")]
        public float StoragePanelResize { get; set; }
    }

    public class PcsInfo
    {
        [JsonProperty("defect_info")]
        public List<DefectInfo> DefectInfo { get; set; }

        [JsonProperty("defect_label")]
        public string DefectLabel { get; set; }

        [JsonProperty("defect_zip_uuid")]
        public string DefectZipUuid { get; set; }

        [JsonProperty("display_index")]
        public string DisplayIndex { get; set; }

        [JsonProperty("pcs_serial_level")]
        public string PcsSerialLevel { get; set; }

        [JsonProperty("pcs_serial_number")]
        public string PcsSerialNumber { get; set; }

        [JsonProperty("piece_index")]
        public string PieceIndex { get; set; }

        [JsonProperty("storage_img_info")]
        public List<object> StorageImgInfo { get; set; }

        [JsonProperty("storage_pcs_info")]
        public List<StoragePcsInfo> StoragePcsInfo { get; set; }
    }

    public class DefectInfo
    {
        [JsonProperty("ai_infer_result")]
        public string AiInferResult { get; set; }

        [JsonProperty("defect_channels")]
        public List<int> DefectChannels { get; set; }

        [JsonProperty("defect_code")]
        public string DefectCode { get; set; }

        [JsonProperty("defect_contours")]
        public List<object> DefectContours { get; set; }

        [JsonProperty("defect_describe")]
        public string DefectDescribe { get; set; }

        [JsonProperty("defect_detail")]
        public string DefectDetail { get; set; }

        [JsonProperty("defect_index")]
        public int DefectIndex { get; set; }

        [JsonProperty("defect_label")]
        public string DefectLabel { get; set; }

        [JsonProperty("defect_location")]
        public string DefectLocation { get; set; }

        [JsonProperty("defect_origin_images")]
        public List<object> DefectOriginImages { get; set; }

        [JsonProperty("defect_origin_roi")]
        public Roi DefectOriginRoi { get; set; }

        [JsonProperty("defect_roi")]
        public Roi DefectRoi { get; set; }

        [JsonProperty("defect_scale")]
        public float DefectScale { get; set; }

        [JsonProperty("defect_vrs_gerber_images")]
        public List<string> DefectVrsGerberImages { get; set; }

        [JsonProperty("defect_vrs_images")]
        public List<string> DefectVrsImages { get; set; }

        [JsonProperty("defect_avi_images")]
        public List<string> DefectAviImages { get; set; }

        [JsonProperty("defect_vrs_ok_images")]
        public List<string> DefectVrsOkImages { get; set; }

        [JsonProperty("image_scale")]
        public float ImageScale { get; set; }

        [JsonProperty("pcs_index")]
        public int PcsIndex { get; set; }

        [JsonProperty("sub_defects_info")]
        public List<SubDefectInfo> SubDefectsInfo { get; set; }
    }

    public class Roi
    {
        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }
    }

    public class SubDefectInfo
    {
        [JsonProperty("sub_ai_infer_result")]
        public string SubAiInferResult { get; set; }

        [JsonProperty("sub_defect_algo_path")]
        public string SubDefectAlgoPath { get; set; }

        [JsonProperty("sub_defect_area")]
        public float SubDefectArea { get; set; }

        [JsonProperty("sub_defect_code")]
        public string SubDefectCode { get; set; }

        [JsonProperty("sub_defect_contours")]
        public List<string> SubDefectContours { get; set; }

        [JsonProperty("sub_defect_details")]
        public Dictionary<string, object> SubDefectDetails { get; set; }

        [JsonProperty("sub_defect_height")]
        public double SubDefectHeight { get; set; }

        [JsonProperty("sub_defect_index")]
        public int SubDefectIndex { get; set; }

        [JsonProperty("sub_defect_roi")]
        public List<SubDefectRoi> SubDefectRoi { get; set; }

        [JsonProperty("sub_defect_width")]
        public double SubDefectWidth { get; set; }
    }

    public class SubDefectRoi
    {
        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }
    }

    public class StoragePcsInfo
    {
        [JsonProperty("pcs_angle")]
        public int PcsAngle { get; set; }

        [JsonProperty("pcs_image_path")]
        public string PcsImagePath { get; set; }

        [JsonProperty("pcs_location")]
        public Roi PcsLocation { get; set; }

        [JsonProperty("pcs_scale")]
        public float PcsScale { get; set; }
    }

}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightModel
{
    /// <summary>
    /// 上传数据中台类
    /// </summary>
    public class DsCenterInfo
    {

        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("projectType")]
        public string ProjectType { get; set; } = "panel";

        [JsonProperty("data")]
        public List<PanelData> Data { get; set; } = new List<PanelData>();
    }
    //主节点
    public class PanelData
    {
        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("projectType")]
        public string ProjectType { get; set; } = "panel";

        [JsonProperty("dataVersion")]
        public string DataVersion { get; set; } = "fpcV2";

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("boxId")]
        public string BoxId { get; set; } = "";

        [JsonProperty("lot")]
        public string Lot { get; set; }

        [JsonProperty("sn")]
        public string Sn { get; set; }

        [JsonProperty("avi")]
        public string Avi { get; set; }

        [JsonProperty("vrs")]
        public string Vrs { get; set; } = "stone";

        [JsonProperty("operator")]
        public string Operator { get; set; } = "1";

        [JsonProperty("start_time")]
        public string StartTime { get; set; } = DateTime.Now.ToString("yyyyMMdd HH:mm:ss");

        [JsonProperty("end_time")]
        public string EndTime { get; set; }

        [JsonProperty("content")]
        public Dictionary<string, ContentItem> Content { get; set; } = new Dictionary<string, ContentItem>();

        [JsonProperty("custom_tags")]
        public string CustomTags { get; set; } = "";
    }
    //Content 内容项类
    public class ContentItem
    {
        [JsonProperty("defects_cnt")]
        public int DefectsCount { get; set; }

        [JsonProperty("piece_ves_index")]
        public string PieceVesIndex { get; set; }

        [JsonProperty("defects_info")]
        public List<DsCenterDefectInfo> DefectsInfo { get; set; } = new List<DsCenterDefectInfo>();

        [JsonProperty("disk_serial_number")]
        public string DiskSerialNumber { get; set; } = "noDisk";

        [JsonProperty("end_time")]
        public string EndTime { get; set; }

        [JsonProperty("line_name")]
        public string LineName { get; set; } = "line1";

        [JsonProperty("machine_name")]
        public string MachineName { get; set; }

        [JsonProperty("piece_index")]
        public string PieceIndex { get; set; }

        [JsonProperty("product_serial")]
        public string ProductSerial { get; set; }

        [JsonProperty("serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty("side_index")]
        public string SideIndex { get; set; } = "F";

        [JsonProperty("dataType")]
        public string DataType { get; set; } = "panel";

        [JsonProperty("start_time")]
        public string StartTime { get; set; } = DateTime.Now.ToString("yyyyMMdd HH:mm:ss");

        [JsonProperty("process_time_A")]
        public string ProcessTimeA { get; set; }// = DateTime.Now.ToString("yyyyMMddHHmmssffffff");处理的时候再赋值

        [JsonProperty("expansionAndContraction")]
        public object ExpansionAndContraction { get; set; } = null;

        [JsonProperty("lot_id")]
        public string LotId { get; set; }

        [JsonProperty("complete_mark")]
        public string CompleteMark { get; set; } = "done";

        [JsonProperty("disk_serial_number_A")]
        public string DiskSerialNumberA { get; set; } = "noDisk";

        [JsonProperty("lot")]
        public string Lot { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; } = "1";

        [JsonProperty("operateTime")]
        public string OperateTime { get; set; }

        [JsonProperty("outSideNgMark")]
        public string OutSideNgMark { get; set; } = "";

        [JsonProperty("uploadMesMark")]
        public bool UploadMesMark { get; set; } = false;

        [JsonProperty("status_color")]
        public string StatusColor { get; set; } = "yellow";
        /// <summary>
        /// 总结果：有NG报点则为NG
        /// </summary>
        [JsonProperty("confirm_result")]
        public string ConfirmResult { get; set; }

        [JsonProperty("etDefectCode")]
        public string EtDefectCode { get; set; } = "";

        [JsonProperty("process_time_B")]
        public string ProcessTimeB { get; set; }//; 处理的时候再赋值

        [JsonProperty("disk_serial_number_B")]
        public string DiskSerialNumberB { get; set; } = "noDisk";

        [JsonProperty("conlusion_info")]
        public List<ConclusionInfo> ConclusionInfo { get; set; } = new List<ConclusionInfo>();
    }

    public class DsCenterDefectInfo
    {
        [JsonProperty("ai_infer_result")]
        public string AiInferResult { get; set; } = "IGNORE";

        [JsonProperty("airesult")]
        public string AiResult { get; set; } = "OK"; //ai推理结果

        [JsonProperty("defect_channels")]
        public List<int> DefectChannels { get; set; } = new List<int> { 0, 1, 2, 3 };

        [JsonProperty("defect_code")]
        public string DefectCode { get; set; }//取VB的第一个缺陷名

        [JsonProperty("defect_contours")]
        public List<object> DefectContours { get; set; } = new List<object>();

        [JsonProperty("defect_describe")]
        public string DefectDescribe { get; set; } = "";

        [JsonProperty("defect_detail")]
        public string DefectDetail { get; set; } = "";

        [JsonProperty("defect_index")]
        public int DefectIndex { get; set; }

        [JsonProperty("defect_label")]
        public string DefectLabel { get; set; } = "NG";

        [JsonProperty("defect_location")]
        public string DefectLocation { get; set; } = "Hotbar_rect";

        [JsonProperty("defect_origin_images")]
        public List<object> DefectOriginImages { get; set; } = new List<object>();

        [JsonProperty("defect_origin_roi")]
        public Roi DefectOriginRoi { get; set; } = new Roi();//赋值原缺陷大小

        [JsonProperty("defect_roi")]
        public Roi DefectRoi { get; set; } = new Roi();//赋值原缺陷大小

        [JsonProperty("defect_scale")]
        public double DefectScale { get; set; } = 1;

        [JsonProperty("defect_vrs_gerber_images")]
        public List<string> DefectVrsGerberImages { get; set; } = new List<string>();

        [JsonProperty("defect_vrs_images")]
        public List<string> DefectVrsImages { get; set; } = new List<string>();

        [JsonProperty("image_scale")]
        public double ImageScale { get; set; } = 1;

        [JsonProperty("pcs_index")]
        public int PcsIndex { get; set; } = 1;

        [JsonProperty("sub_defects_info")]
        public List<DsCenterSubDefectInfo> SubDefectsInfo { get; set; } = new List<DsCenterSubDefectInfo>();

        [JsonProperty("defects_roi")]
        public List<int> DefectsRoi { get; set; } = new List<int>();

        [JsonProperty("defect_images")]
        public List<string> DefectImages { get; set; } = new List<string>();

        [JsonProperty("defect_gerber_images")]
        public List<string> DefectGerberImages { get; set; } = new List<string>();

        [JsonProperty("side_type")]
        public string SideType { get; set; }

        [JsonProperty("panelIndex")]
        public int PanelIndex { get; set; } = 0;

        [JsonProperty("subIndex")]
        public int SubIndex { get; set; } = 0;

        [JsonProperty("parent")]
        public string Parent { get; set; } = "1";

        [JsonProperty("aienable")]
        public bool AiEnable { get; set; } = true;

        [JsonProperty("serialMark")]
        public bool SerialMark { get; set; } = false;

        [JsonProperty("pcsVesIndex")]
        public string PcsVesIndex { get; set; }

        [JsonProperty("manual_defectcode")]
        public string ManualDefectCode { get; set; } = "null";

        [JsonProperty("customerTranslation")]
        public string CustomerTranslation { get; set; } = "";

        [JsonProperty("subDefectCount")]
        public int SubDefectCount { get; set; } = 0;

        [JsonProperty("sideType")]
        public string SideType2 { get; set; }

        [JsonProperty("mesCoode")]
        public string MesCode { get; set; } = "";

        [JsonProperty("status_color")]
        public string StatusColor { get; set; } = "yellow";

        [JsonProperty("aiIgnoreMode")]
        public bool AiIgnoreMode { get; set; } = false;

        [JsonProperty("aiOperationSwitch")]
        public bool AiOperationSwitch { get; set; } = true;

        [JsonProperty("aiProductSwitch")]
        public bool AiProductSwitch { get; set; } = true;

        [JsonProperty("manul_result")]
        public string ManualResult { get; set; }

        [JsonProperty("auto_mark")]
        public bool AutoMark { get; set; } = false;

        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; } = 0;

        [JsonProperty("subPageIndex")]
        public int SubPageIndex { get; set; } = 0;

        [JsonProperty("pagedIndex")]
        public int PagedIndex { get; set; } = 0;
    }

    public class DsCenterSubDefectInfo
    {
        [JsonProperty("sub_ai_infer_result")]
        public string SubAiInferResult { get; set; } = "";

        [JsonProperty("sub_defect_algo_path")]
        public string SubDefectAlgoPath { get; set; } = "";

        [JsonProperty("sub_defect_area")]
        public double SubDefectArea { get; set; }

        [JsonProperty("sub_defect_code")]
        public string SubDefectCode { get; set; }

        [JsonProperty("sub_defect_contours")]
        public List<string> SubDefectContours { get; set; } = new List<string>();

        [JsonProperty("sub_defect_details")]
        public SubDefectDetails SubDefectDetails { get; set; } = new SubDefectDetails();

        [JsonProperty("sub_defect_height")]
        public int SubDefectHeight { get; set; }

        [JsonProperty("sub_defect_index")]
        public int SubDefectIndex { get; set; }

        [JsonProperty("sub_defect_roi")]
        public List<int> SubDefectRoi { get; set; } = new List<int>();

        [JsonProperty("sub_defect_width")]
        public int SubDefectWidth { get; set; }

        [JsonProperty("sub_defect_roi_cm")]
        public object SubDefectRoiCm { get; set; }

        [JsonProperty("centerPoint")]
        public List<int> CenterPoint { get; set; } = new List<int>();
    }

    public class SubDefectDetails
    {
        [JsonProperty("algo_path")]
        public string AlgoPath { get; set; }

        [JsonProperty("area")]
        public double Area { get; set; }

        [JsonProperty("sub_defect_area")]
        public double SubDefectArea { get; set; }

        [JsonProperty("sub_defect_height")]
        public int SubDefectHeight { get; set; }

        [JsonProperty("sub_defect_location")]
        public string SubDefectLocation { get; set; }

        [JsonProperty("sub_defect_width")]
        public int SubDefectWidth { get; set; }
    }

    public class ConclusionInfo
    {
        [JsonProperty("defect_code")]
        public string DefectCode { get; set; }

        [JsonProperty("defect_images")]
        public List<string> DefectImages { get; set; }

        [JsonProperty("defect_gerber_images")]
        public List<string> DefectGerberImages { get; set; }

        [JsonProperty("defects_roi")]
        public List<int> DefectsRoi { get; set; }

        [JsonProperty("sub_defects_info")]
        public List<ConclusionSubDefect> SubDefectsInfo { get; set; }

        [JsonProperty("side_type")]
        public string SideType { get; set; }

        [JsonProperty("sideType")]
        public string SideType2 { get; set; }

        [JsonProperty("panelIndex")]
        public int PanelIndex { get; set; }

        [JsonProperty("pcsVesIndex")]
        public string PcsVesIndex { get; set; }

        [JsonProperty("subIndex")]
        public string SubIndex { get; set; }

        [JsonProperty("parent")]
        public string Parent { get; set; }

        [JsonProperty("manual_defectcode")]
        public string ManualDefectCode { get; set; }

        [JsonProperty("status_color")]
        public string StatusColor { get; set; }

        [JsonProperty("manul_result")]
        public string ManualResult { get; set; }

        [JsonProperty("auto_mark")]
        public bool AutoMark { get; set; }

        [JsonProperty("hasScaned")]
        public string HasScaned { get; set; }

        [JsonProperty("needScan")]
        public bool NeedScan { get; set; }
    }

    public class ConclusionSubDefect
    {
        [JsonProperty("sub_defect_roi")]
        public Roi SubDefectRoi { get; set; }
    }

}

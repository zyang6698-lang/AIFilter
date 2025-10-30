using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightModel
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class RootVBOutInfo
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("data")]
        public VBOutData Data { get; set; }
    }

    public class VBOutData
    {
        [JsonProperty("infer_res_uuid")]
        public string InferResUuid { get; set; }

        [JsonProperty("infer_whole_data")]
        public InferWholeData InferWholeData { get; set; }
    }

    public class InferResult
    {
        [JsonProperty("group_uuid")]
        public string GroupUuid { get; set; }

        [JsonProperty("infer_result")]
        public string Infer_Result { get; set; }

        [JsonProperty("infer_details")]
        public InferDetails inferDetails { get; set; }

        [JsonProperty("inspect_details")]
        public InspectDetails inspectDetails { get; set; }

        [JsonProperty("group_infos")]
        public List<GroupInfo> GroupInfos { get; set; }

        [JsonProperty("defect_name")]
        public string Defect_name { get; set; }

        [JsonProperty("defect_code")]
        public string Defect_code { get; set; }

        [JsonProperty("img_roi")]
        public List<int> ImgRoi { get; set; }
    }

    public class InferDetails
    {
        [JsonProperty("defect_area")]
        public string DefectArea { get; set; }
        [JsonProperty("details")]
        public List<string> Details { get; set; }

        //这个对象OUT时用
        [JsonProperty("location")]
        public List<Location> Location { get; set; }

        //0613返回参数添加新节点  此节点字段不固定 ，改为动态字段解析

        [JsonProperty("node_details")]
        // public NodeDetails NodeDetails { get; set; }
        public JObject NodeDetails { get; set; }
    }

    public class Location
    {
        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }
        [JsonProperty("x")]
        public double X { get; set; }
        [JsonProperty("y")]
        public double Y { get; set; }

    }

    public class NodeDetails
    {
        //[JsonProperty("flow_name")]
        //public string FlowName { get; set; }

        //[JsonProperty("input_argument")]
        //public NodeArguments InputArgument { get; set; }

        //[JsonProperty("inspect_info")]
        //public List<object> InspectInfo { get; set; }

        //[JsonProperty("operator_name")]
        //public string OperatorName { get; set; }

        //[JsonProperty("output_argument")]
        //public NodeArguments OutputArgument { get; set; }
    }

    public class NodeArguments
    {
        [JsonProperty("change_detecion_templates_path")]
        public TypedValue<string[]> ChangeDetectionTemplatesPath { get; set; }

        [JsonProperty("enable_change_detecion_templates_path")]
        public TypedValue<bool> EnableChangeDetectionTemplatesPath { get; set; }

        [JsonProperty("enable_inspect_names_in")]
        public TypedValue<bool> EnableInspectNamesIn { get; set; }

        [JsonProperty("from_out")]
        public TypedValue<bool> FromOut { get; set; }

        [JsonProperty("group_index")]
        public TypedValue<int> GroupIndex { get; set; }

        [JsonProperty("image_path")]
        public TypedValue<string> ImagePath { get; set; }

        [JsonProperty("inspect_names_in")]
        public TypedValue<string[]> InspectNamesIn { get; set; }

        [JsonProperty("show_group_index")]
        public TypedValue<int> ShowGroupIndex { get; set; }
    }

    public class TypedValue<T>
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public T Value { get; set; }
    }
}

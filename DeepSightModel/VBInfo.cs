using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DeepSightModel
{
    //调用VB时传入的参数
    public class RootVBInfo
    {

        [JsonProperty("message_type")]
        public string MessageType { get; set; }

        [JsonProperty("params")]
        public ParamsData paramsData { get; set; }

    }

    public class ParamsData
    {
        [JsonProperty("infer_res_uuid")]
        public string InferResUuid { get; set; }

        [JsonProperty("infer_whole_data")]
        public InferWholeData InferWholeData { get; set; }
    }

    public class InferWholeData
    {
        [JsonProperty("image_infer_params")]
        public ImageInferParams ImageInferParams { get; set; }

        [JsonProperty("image_data")]
        public ImageData ImageData { get; set; }

        [JsonProperty("other_infos")]
        public Others OtherInfos { get; set; }

        //这个对象OUT时用
        [JsonProperty("infer_results")]
        public List<InferResult> InferResults { get; set; }
    }
    public class Others
    {
        [JsonProperty("image_minio")]
        public ImageMminio imageminio { get; set; }
    }
    public class ImageMminio
    {
        [JsonProperty("access_key")]
        public string access_key_id { get; set; }

        [JsonProperty("bucket")]
        public string bucket { get; set; }
        [JsonProperty("endpoint_address")]
        public string endpoint_url { get; set; }
        [JsonProperty("access_secret")]
        public string secret_key { get; set; }

        [JsonProperty("port")]
        public string secret_port { get; set; }

    }


    public class ImageInferParams
    {
        [JsonProperty("pipeline_name")]
        public string PipelineName { get; set; }

        [JsonProperty("node_params")]
        public List<NodeParam> NodeParams { get; set; }
    }

    public class NodeParam
    {
        [JsonProperty("node_name")]
        public string NodeName { get; set; }
        [JsonProperty("height")]
        public int height { get; set; }
        [JsonProperty("width")]
        public int width { get; set; }


    }

    public class ImageData
    {
        [JsonProperty("data_type")]
        public string DataType { get; set; }

        [JsonProperty("data_value")]
        public DataValue DataValue { get; set; }
    }

    public class DataValue
    {
        [JsonProperty("infer_image_group")]
        public List<InferImageGroup> InferImageGroup { get; set; }
    }

    public class InferImageGroup
    {
        [JsonProperty("group_uuid")]
        public string GroupUuid { get; set; }

        [JsonProperty("group_infos")]
        public List<GroupInfo> GroupInfos { get; set; }
        [JsonProperty("defect_code")]
        public string DefectCode { get; set; }
        [JsonProperty("tempImgPath")]
        public string TempImgPath { get; set; }
        [JsonProperty("img_roi")]
        public List<int> ImgROI { get; set; }

        [JsonProperty("inspect_details")]
        public  InspectDetails  inspectDetails{ get; set; }
    }
    public class InspectDetails
    {
        [JsonProperty("infer_roi")]
        public List< InferRoi> InferRois { get; set; }
    }
    public class InferRoi
    {
      
    }

    public class GroupInfo
    {
        [JsonProperty("image_path")]
        public string ImagePath { get; set; }

        [JsonProperty("image_uuid")]
        public string ImageUuid { get; set; }

        [JsonProperty("image_type")]
        public string ImageType { get; set; }
    }
}

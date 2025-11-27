using DeepSightEvent;
using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeepSightWorkLib
{

    /// <summary>
    /// 算法检测类 
    /// </summary>
    public class DefectClass
    {
        public AI_DefectClass ai_Defect = null;
        public DefectClass()
        {
            ai_Defect = new AI_DefectClass();
        }
        public void DefectMethod(RootVBInfo info, out string vb_outStr)
        {
            try
            {
                #region json
                string json = @"
{
    ""message_type"": ""visionbuilder_inference"",
    ""params"": {
        ""infer_res_uuid"": """",
        ""infer_whole_data"": {
            ""image_data"": {
                ""data_type"": ""minio"",
                ""data_value"": {
                    ""infer_image_group"": [
                        {
                            ""group_infos"": [
                                {
                                    ""image_path"": ""20250403155016741981/SM PAD FM DL/1502170010001A12502-A-SM PAD FM DL-pcs-X1Y1-vrs0-0.jpg"",
                                    ""image_type"": ""defect"",
                                    ""image_uuid"": """"
                                },
                                {
                                    ""image_path"": ""20250403155016741981/SM PAD FM DL/1502170010001A12502-A-SM PAD FM DL-pcs-X1Y1-vrs0-0-gerber-1.jpg"",
                                    ""image_type"": ""gerber"",
                                    ""image_uuid"": """"
                                },
                                {
                                    ""image_path"": ""20250403155016741981/SM PAD FM DL/1502170010001A12502-A-SM PAD FM DL-pcs-X1Y1-vrs0-0-template-1.jpg"",
                                    ""image_type"": ""template"",
                                    ""image_uuid"": """"
                                }
                            ],
                            ""group_uuid"": ""65e84dc0-256e-11f0-bef0-189341101a89"",
                            ""inspect_details"": {
                                ""infer_roi"": []
                            }
                        },
                        {
                            ""group_infos"": [
                                {
                                    ""image_path"": ""20250403155016741981/BIG RING PAD FM DL/1502170010001A12502-A-BIG RING PAD FM DL-pcs-X2Y1-vrs0-0.jpg"",
                                    ""image_type"": ""defect"",
                                    ""image_uuid"": """"
                                },
                                {
                                    ""image_path"": ""20250403155016741981/BIG RING PAD FM DL/1502170010001A12502-A-BIG RING PAD FM DL-pcs-X2Y1-vrs0-0-gerber-1.jpg"",
                                    ""image_type"": ""gerber"",
                                    ""image_uuid"": """"
                                },
                                {
                                    ""image_path"": ""20250403155016741981/BIG RING PAD FM DL/1502170010001A12502-A-BIG RING PAD FM DL-pcs-X2Y1-vrs0-0-template-1.jpg"",
                                    ""image_type"": ""template"",
                                    ""image_uuid"": """"
                                }
                            ],
                            ""group_uuid"": ""65e874bc-256e-11f0-a2e3-189341101a89"",
                            ""inspect_details"": {
                                ""infer_roi"": []
                            }
                        }
                    ]
                }
            },
            ""image_infer_params"": {
                ""node_params"": [
                    {
                        ""height"": 200,
                        ""node_name"": ""flow1"",
                        ""width"": 200
                    }
                ],
                ""pipeline_name"": ""0311""
            },
            ""other_infos"": {
                ""image_minio"": {
                    ""access_key"": ""deepiobjectdata"",
                    ""bucket"": ""deepiresults"",
                    ""endpoint_address"": ""127.0.0.1"",
                    ""access_secret"": ""deepiobject2019"",
                    ""port"": ""9102""
                }
            }
        }
    }
}";
                #endregion
                JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };//去掉空值NULL
                //string res = JsonConvert.SerializeObject(info, Formatting.None, jsonSetting);

                IntPtr input = Marshal.StringToHGlobalAnsi(JsonConvert.SerializeObject(info, Formatting.None, jsonSetting));
                IntPtr result = IntPtr.Zero;
                ai_Defect.Vision_runMethod(input, out result);
                vb_outStr = Marshal.PtrToStringAnsi(result);
            }
            catch (Exception ex)
            {
                vb_outStr = "";
                SystemEvent.SendAlarmMsg($"VB算法调用异常:{ex.ToString()}");
            }
        }
    }

    //C++接口实现
    public class AI_DefectClass
    {
        public static IntPtr handler = IntPtr.Zero;

        private const string strName = @"ProxyServer.dll";
        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr create_basehandler();

        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int vision_init(int p);

        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int vision_run();

        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int basehandler_handle_message(IntPtr handler, IntPtr input, out IntPtr output);

        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int basehandler_init(IntPtr handler);

        [DllImport(strName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void vision_show_view(int isShow);


        public AI_DefectClass()
        {
            try
            {
                handler = create_basehandler();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public  void Vision_Show_View(int isShow)
        {
           vision_show_view(isShow);
        }

        public int Vision_runMethod(IntPtr input, out IntPtr output)
        {
            return basehandler_handle_message(handler, input, out output);
        }
    }
}

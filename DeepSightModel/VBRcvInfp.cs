using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightModel
{
    //vb推理返回
    public class VBRcvInfp
    {
        //***************AI算法输出***************

        /// <summary>
        ///  模型输出的分类 id
        /// </summary>
        public int category_id { get; set; }

        /// <summary>
        /// 模型输出的分类名称
        /// </summary>
        public string category_name { get; set; }
        /// <summary>
        /// 缺陷所在产品序号 0/1/2/3
        /// </summary>
        public int index { get; set; }

        /// <summary>
        /// 模型输出的矩形框，四个数字分别代表：x, y, w, h，（可能会被算法扩⼤）
        /// </summary>
        public List<List<double>> bbox { get; set; }

        public List<string> sub_DefectNames { get; set; } = new List<string>();

        /// <summary>
        /// 原始矩形框，如果这个框的⾯积⼩于 threshold_area ，就会有这个字段，⽤于可视化
        /// </summary>
        public List<double> raw_bbox { get; set; }

        /// <summary>
        /// 模型输出的框置信度
        /// </summary>
        public double score { get; set; }

        //**************传统算法输出*********************

        /// <summary>
        /// 背景颜⾊均值（背景）
        /// </summary>
        public double mean_background { get; set; }

        /// <summary>
        ///  前景颜⾊均值（瑕疵）
        /// </summary>
        public double mean_foreground { get; set; }

        /// <summary>
        /// ⼤图背景均值
        /// </summary>
        public double mean { get; set; }

        /// <summary>
        /// 瑕疵⾯积
        /// </summary>
        public double defect_area { get; set; }

        public float variance { get; set; }
        /// <summary>
        ///  瑕疵的外接矩形宽度
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// 瑕疵的外接矩形⾼度
        /// </summary>
        public int height { get; set; }

        /// <summary>
        ///  瑕疵的最⼩外接矩形宽度
        /// </summary>
        public double min_bbox_width { get; set; }

        /// <summary>
        /// 瑕疵的最⼩外接矩形⾼度
        /// </summary>
        public double min_bbox_height { get; set; }

        /// <summary>
        /// 信息，⽬前仅在扩展⼩框的时候会有这个字段，存储的内容是f'expand_{expand_ratio:.1f}' ，⽤于可视化
        /// </summary>
        public string message { get; set; }

        public bool raw_mask { get; set; }

        public double max_connection_area { get; set; }

        public string id { get; set; }

        public string Name { get; set; }
        /// <summary>
        /// 前景于背景灰度差
        /// </summary>
        public double delta_back_fore { get; set; }
        /// <summary>
        /// 骨架长度
        /// </summary>
        public double skeleton_length { get; set; }

        /// <summary>
        /// 骨架宽度
        /// </summary>
        public double skeleton_width { get; set; }
    }


    public class PcsResult
    {
        //每个缺陷图上面的缺陷坐标集合
        public List<VBRcvInfp> vb_List { get; set; }
    }
}

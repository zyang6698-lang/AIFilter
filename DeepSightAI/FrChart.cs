using DeepSightDB;
using DeepSightTool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using DeepSightModel;
using Aspose.Cells;
using DeepSightDisplay;
using OpenCvSharp;
using System.Drawing;
using System.Text.RegularExpressions;
using OpenCvSharp.Extensions;
using System.Diagnostics;

namespace DeepSightAI
{
    public partial class FrChart : Form
    {
        public FrChart()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
        }
        /// <summary>
        /// 窗体对象实例
        /// </summary>
        private static FrChart _instance;

        public static FrChart Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrChart();
                }

                return _instance;
            }
        }


    }
}

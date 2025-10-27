
using DeepSightTool;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class FrFullImage : Form
    {
        public FrFullImage()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲

            Rectangle rect = System.Windows.Forms.SystemInformation.VirtualScreen;
            this.Width = rect.Width;
            this.Height = rect.Height;
        }
        /// <summary>
        /// 窗体对象实例
        /// </summary>
        private static FrFullImage _instance;
        public static FrFullImage Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FrFullImage();
                return _instance;
            }
        }

     

        private void FrFullImage_Load(object sender, EventArgs e)
        {
            try
            {
                Rectangle rect = System.Windows.Forms.SystemInformation.VirtualScreen;
                this.Width = rect.Width;
                this.Height = rect.Height;
                this.Location = new System.Drawing.Point(0, 0);
                cvDisplay1.ContextMenuStrip.Visible = false;
                cvDisplay1.DisVisibleContextMenuStrip();
                cvDisplay1.Fit();

             
            }
            catch (System.Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }
        

        private void FrFullImage_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Escape)
                {
                    cvDisplay1.Clear();
                    this.Hide();
                }
            }
            catch (System.Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }
        public void LoadShow(Mat srcImg)
        {
            try
            {
                cvDisplay1.Clear();
                if (srcImg != null)
                {
                    cvDisplay1.Image = srcImg;
                    cvDisplay1.Fit();
                    srcImg.Dispose();
                    srcImg = null;
                }
            }
            catch (System.Exception ex)
            {
                LogTextHelper.Error("Error", ex);
            }
        }
       

      
    }
}

using DeepSightTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 将 VisionBuilder 文件夹加入 DLL 搜索路径
            string vbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VisionBuilder");
            SetDllDirectory(vbPath);

            GlobalMutex();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            #region Error捕抓全局

            //设置应用程序处理Error方式：ThreadException处理
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            //处理UI线程Error
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            //处理非UI线程Error
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            #endregion Error捕抓全局


            Application.Run(FrmSplash.Instance);
        }



        #region Error捕抓全局

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs ex)
        {
            LogTextHelper.Error("Error", ex.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogTextHelper.Error(string.Format("Error {0}", e.ExceptionObject.ToString()));
        }

        #endregion Error捕抓全局

        #region 单例模式

        private static Mutex mutex = null;

        private static void GlobalMutex()
        {
            // 是否第一次创建mutex
            bool newMutexCreated = false;
            string ProgramName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            string mutexName = "Global\\" + ProgramName;
            try
            {
                mutex = new Mutex(true, mutexName, out newMutexCreated);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                System.Threading.Thread.Sleep(100);
                Environment.Exit(1);
            }

            // 第一次创建mutex
            if (newMutexCreated)
            {
                Console.WriteLine("程序已启动");
                //todo:此处为要执行的任务
            }
            else
            {
                MessageBox.Show("\r\n另一个窗口已在运行，不能重复运行！","异常信息",MessageBoxButtons.OK, MessageBoxIcon.Error);

                System.Threading.Thread.Sleep(100);
                Environment.Exit(1);//退出程序
            }
        }

        #endregion 单例模式
    }
}

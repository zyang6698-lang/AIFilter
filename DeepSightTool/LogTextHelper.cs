using System;
using System.Drawing;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace DeepSightTool
{
    /// <summary>
    /// 文本日志记录辅助类
    /// </summary>
    public class LogTextHelper
    {
        public string NewLogFolder = string.Empty;

        private static string LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
        private static string VBresult = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VB结果统计");
        private static string LogFolderConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "人员操作配置记录");

        public static bool RecordLog = true;
        public static bool DebugLog = false;

        public delegate void CallBackLogProc(string msg, Color color);

        public static event CallBackLogProc OnCallBackLogProc;

        /// <summary>
        /// 是否保存日志
        /// </summary>
        public static bool Enable = true;

        static LogTextHelper()
        {
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }
            if (!Directory.Exists(VBresult))
            {
                Directory.CreateDirectory(VBresult);
            }
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="message">错误信息</param>
        public static void WriteLine(string message)
        {
            if (!Enable)
            {
                return;
            }
            string temp = "";
            if (message.Contains("---->"))
            {
                temp = string.Format("{0} \r\n\r\n", message);
            }
            else
            {
                temp = string.Format("[{0}]---->  {1} \r\n\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message);
            }
            //string temp = string.Format("[{0}]---->  {1} \r\n\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message);

            //string temp = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss,fff]---->  ") + message + "\r\n\r\n";
            string fileName = DateTime.Now.ToString("yyyyMMddHH") + ".log";
            try
            {
                if (RecordLog)
                {
                    File.AppendAllText(Path.Combine(LogFolder, fileName), temp, Encoding.GetEncoding("GB2312"));
                }
                if (DebugLog)
                {
                    Console.WriteLine(temp);
                }
            }
            catch
            {
            }
            finally
            {
                temp = null;
                fileName = null;
            }
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="message">错误信息</param>
        public static void WriteLine2(string message)
        {
            if (!Enable)
            {
                return;
            }
            string temp = "";
            if (message.Contains("---->"))
            {
                temp = string.Format("{0} \r\n\r\n", message);
            }
            else
            {
                temp = string.Format("[{0}]---->  {1} \r\n\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message);
            }
            //string temp = string.Format("[{0}]---->  {1} \r\n\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message);

            //string temp = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss,fff]---->  ") + message + "\r\n\r\n";
            string fileName = DateTime.Now.ToString("yyyyMMddHH") + "_Warn.log";
            try
            {
                if (RecordLog)
                {
                    File.AppendAllText(Path.Combine(LogFolder, fileName), temp, Encoding.GetEncoding("GB2312"));
                }
                if (DebugLog)
                {
                    Console.WriteLine(temp);
                }
            }
            catch
            {
            }
            finally
            {
                temp = null;
                fileName = null;
            }
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="ex">异常信息</param>
        public static void WriteLine(string message, Exception ex)
        {
            if (!Enable)
            {
                return;
            }
            string temp = string.Format("[{0}]---->  {1} \r\n {2} \r\n\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message, ex.ToString());
            //string temp = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss,fff]---->  ") + message + "\r\n"
            //    + ex.ToString() + "\r\n\r\n";
            string fileName = DateTime.Now.ToString("yyyyMMddHH") + "_Err.log";
            try
            {
                if (RecordLog)
                {
                    File.AppendAllText(Path.Combine(LogFolder, fileName), temp, Encoding.GetEncoding("GB2312"));
                }
                if (DebugLog)
                {
                    Console.WriteLine(temp);
                }

                if (OnCallBackLogProc != null)
                {
                    OnCallBackLogProc(string.Format("{0}:{1}", message, ex.ToString()), Color.Red);
                }
            }
            catch
            {
            }
            finally
            {
                temp = null;
                fileName = null;
            }
        }

        /// <summary>
        /// 记录类名、消息等信息到日志文件
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="funName">全名</param>
        /// <param name="message">错误信息</param>
        public static void WriteLine(string className, string funName, string message)
        {
            if (!Enable)
            {
                return;
            }
            WriteLine(string.Format("{0}：{1}\r\n{2}", className, funName, message));
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="ex">错误信息</param>
        public static void Debug(object ex)
        {
            if (!Enable)
            {
                return;
            }
            WriteLine(ex.ToString());
            if (OnCallBackLogProc != null)
            {
                OnCallBackLogProc(ex.ToString(), Color.Green);
            }
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="ex">错误信息</param>
        public static void Warn(object ex)
        {
            if (!Enable)
            {
                return;
            }
            WriteLine2(ex.ToString());
            if (OnCallBackLogProc != null)
            {
                OnCallBackLogProc(ex.ToString(), Color.Red);
            }
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="ex">错误信息</param>
        public static void Error(object ex)
        {
            if (!Enable)
            {
                return;
            }
            WriteLine2(ex.ToString());
            if (OnCallBackLogProc != null)
            {
                OnCallBackLogProc(ex.ToString(), Color.Red);
            }
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="ex">错误信息</param>
        public static void Info(object ex)
        {
            if (!Enable)
            {
                return;
            }
            WriteLine(ex.ToString());
            if (OnCallBackLogProc != null)
            {
                OnCallBackLogProc(ex.ToString(), Color.Green);
            }
        }

        /// <summary>
        /// 记录信息和异常信息
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="ex">异常对象</param>
        public static void Debug(object message, Exception ex)
        {
            if (!Enable)
            {
                return;
            }
            WriteLine(message.ToString(), ex);
        }

        /// <summary>
        /// 记录信息和异常信息
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="ex">异常对象</param>
        public static void Warn(object message, Exception ex)
        {
            if (!Enable)
            {
                return;
            }
            WriteLine(message.ToString(), ex);
        }

        /// <summary>
        /// 记录信息和异常信息
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="ex">异常对象</param>
        public static void Error(object message, Exception ex)
        {
            if (!Enable)
            {
                return;
            }
            WriteLine(message.ToString(), ex);
        }

        /// <summary>
        /// 记录信息和异常信息
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="ex">异常对象</param>
        public static void Info(object message, Exception ex)
        {
            if (!Enable)
            {
                return;
            }
            WriteLine(message.ToString(), ex);
        }


        //0915记录人员操作配置日志
        public static void SaveChangeConfigInfo(string str)
        {
            try
            {
                string yearStr = DateTime.Now.Year.ToString();
                string monthStr = DateTime.Now.Month.ToString();
                string dayStr = DateTime.Now.Day.ToString();

                string secStr = DateTime.Now.Second.ToString("00");
                string minStr = DateTime.Now.Minute.ToString("00");
                string hourStr = DateTime.Now.Hour.ToString("00");

                string dateStr = yearStr + monthStr + dayStr + "人员操作";
                string timeStr = hourStr + ":" + minStr + ":" + secStr + "  " + str;

                //如果没有文件夹则创建
                if (!Directory.Exists(LogFolderConfig))
                {
                    Directory.CreateDirectory(LogFolderConfig);
                }

                StreamWriter sw = File.AppendText(LogFolderConfig + "\\" + dateStr + ".txt");
                sw.WriteLine(timeStr);
                sw.Flush();
                sw.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void SaveVBResultInfo(string str)
        {
            try
            {
                string yearStr = DateTime.Now.Year.ToString();
                string monthStr = DateTime.Now.Month.ToString();
                string dayStr = DateTime.Now.Day.ToString();

                string secStr = DateTime.Now.Second.ToString("00");
                string minStr = DateTime.Now.Minute.ToString("00");
                string hourStr = DateTime.Now.Hour.ToString("00");

                string dateStr = yearStr + monthStr + dayStr + "结果统计";
                string timeStr = hourStr + ":" + minStr + ":" + secStr + "  " + str;

                //如果没有文件夹则创建
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"VB结果统计")))
                {
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VB结果统计"));
                }

                StreamWriter sw = File.AppendText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VB结果统计")+ "\\" +dateStr + ".txt");
                sw.WriteLine(timeStr);
                sw.Flush();
                sw.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void WriteJsonFile(string path, object jsonInfo)
        {
            try
            {
                var jsonObject = JsonConvert.DeserializeObject(jsonInfo.ToString());
                string json = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"写Json文件异常：{ex.ToString()}");
            }
        }

    }
}
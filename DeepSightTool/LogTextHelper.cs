using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;

namespace DeepSightTool
{
    /// <summary>
    /// 文本日志记录辅助类 - 集成 Serilog 优化日志性能
    /// </summary>
    public class LogTextHelper
    {
        private static string LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
        private static string LogFolderConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "人员操作配置记录");

        public static bool RecordLog = true;
        public static bool DebugLog = false;

        public enum LogTextLevel
        {
            Debug,
            Info,
            Warn,
            Error
        }

        public delegate void LogWrittenProc(LogTextLevel level, string message, Exception exception, string cooldownKey, string source);

        public static event LogWrittenProc OnLogWritten;

        /// <summary>
        /// 是否保存日志
        /// </summary>
        public static bool Enable = true;

        private static void FireLogWritten(LogTextLevel level, string msg, Exception exception = null, string cooldownKey = null)
        {
            try
            {
                string source = level >= LogTextLevel.Warn ? GetCallerClassName() : null;
                OnLogWritten?.Invoke(level, msg, exception, cooldownKey, source);
            }
            catch
            {
            }
        }

        private static string GetCallerClassName()
        {
            try
            {
                var trace = new StackTrace(false);
                for (int i = 2; i < trace.FrameCount; i++)
                {
                    var type = trace.GetFrame(i)?.GetMethod()?.DeclaringType;
                    if (type != null && type != typeof(LogTextHelper))
                        return type.Name;
                }
            }
            catch { }
            return null;
        }

        // Serilog 日志记录器
        private static Logger _infoLogger;
        private static Logger _warnLogger;
        private static Logger _errorLogger;

        static LogTextHelper()
        {
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }


            // 初始化 Serilog 日志记录器，使用异步写入提升性能
            InitializeSerilog();
        }

        /// <summary>
        /// 初始化 Serilog 日志记录器 - 高性能配置
        /// </summary>
        private static void InitializeSerilog()
        {
            var outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss,fff}]---->  {Message:lj}{NewLine}{NewLine}";
            var encoding = Encoding.GetEncoding("UTF-8");

            // 高性能配置参数
            const int asyncBufferSize = 10000;          // 异步队列大小，默认10000
            const int fileSizeLimitBytes = 100 * 1024 * 1024; // 单个文件100MB
            const bool blockWhenFull = false;           // 队列满时不阻塞，直接丢弃（高性能模式）

            // Info 日志 - 普通日志 (高性能异步写入)
            _infoLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Async(a => a.File(
                    Path.Combine(LogFolder, ".log"),
                    outputTemplate: outputTemplate,
                    encoding: encoding,
                    rollingInterval: RollingInterval.Hour,
                    retainedFileCountLimit: null,
                    fileSizeLimitBytes: fileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    buffered: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(2)),
                    bufferSize: asyncBufferSize,
                    blockWhenFull: blockWhenFull)
                .CreateLogger();

            // Warn 日志 (高性能异步写入)
            _warnLogger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.Async(a => a.File(
                    Path.Combine(LogFolder, "_Warn.log"),
                    outputTemplate: outputTemplate,
                    encoding: encoding,
                    rollingInterval: RollingInterval.Hour,
                    retainedFileCountLimit: null,
                    fileSizeLimitBytes: fileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    buffered: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(2)),
                    bufferSize: asyncBufferSize,
                    blockWhenFull: blockWhenFull)
                .CreateLogger();

            // Error 日志 (高性能异步写入)
            _errorLogger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.Async(a => a.File(
                    Path.Combine(LogFolder, "_Err.log"),
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss,fff}]---->  {Message:lj}{NewLine}{Exception}{NewLine}",
                    encoding: encoding,
                    rollingInterval: RollingInterval.Hour,
                    retainedFileCountLimit: null,
                    fileSizeLimitBytes: fileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    buffered: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(2)),
                    bufferSize: asyncBufferSize,
                    blockWhenFull: blockWhenFull)
                .CreateLogger();
        }

        /// <summary>
        /// 关闭并刷新日志 (应用退出时调用)
        /// </summary>
        public static void CloseAndFlush()
        {
            _infoLogger?.Dispose();
            _warnLogger?.Dispose();
            _errorLogger?.Dispose();
        }

        public static void WriteFromAlarmService(LogTextLevel level, string message, Exception ex = null)
        {
            if (!Enable)
            {
                return;
            }

            try
            {
                WriteToSerilog(level, message, ex);
                if (DebugLog)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss,fff}]---->  {message}{(ex == null ? string.Empty : "\r\n" + ex)}");
                }
            }
            catch
            {
            }
        }

        private static void WriteToSerilog(LogTextLevel level, string message, Exception ex = null)
        {
            switch (level)
            {
                case LogTextLevel.Debug:
                    _infoLogger?.Debug(message);
                    break;
                case LogTextLevel.Info:
                    _infoLogger?.Information(message);
                    break;
                case LogTextLevel.Warn:
                    _warnLogger?.Warning(message);
                    break;
                case LogTextLevel.Error:
                    if (ex == null)
                    {
                        _errorLogger?.Error(message);
                    }
                    else
                    {
                        _errorLogger?.Error(ex, message);
                    }
                    break;
            }
        }

        private static string FormatMessage(string messageTemplate, object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return messageTemplate ?? string.Empty;
            }

            try
            {
                return string.Format(messageTemplate ?? string.Empty, args);
            }
            catch
            {
                return messageTemplate ?? string.Empty;
            }
        }

        private static void WriteParameterized(LogTextLevel level, string messageTemplate, object[] args)
        {
            if (!Enable)
            {
                return;
            }

            var msg = FormatMessage(messageTemplate, args);
            WriteToSerilog(level, msg);
            FireLogWritten(level, msg, null, messageTemplate);
        }


        /// <summary>
        /// 记录错误信息和异常 (使用 Serilog 异步写入)
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="ex">异常信息</param>
        public static void WriteLine(string message, Exception ex)
        {
            if (!Enable)
            {
                return;
            }

            try
            {
                if (RecordLog)
                {
                    // 使用 Serilog 异步写入错误日志
                    _errorLogger?.Error(ex, message);
                }
                if (DebugLog)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss,fff}]---->  {message}\r\n{ex}");
                }

                FireLogWritten(LogTextLevel.Error, message, ex);
            }
            catch
            {
                // 忽略日志写入异常
            }
        }


        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="ex">错误信息</param>
        public static void Debug(object ex)
        {
            if (!Enable)
            {
                return;
            }
            var msg = ex?.ToString() ?? string.Empty;
            _infoLogger?.Debug(msg);
            FireLogWritten(LogTextLevel.Debug, msg);
        }

        public static void DebugFormat(string messageTemplate, params object[] args)
        {
            WriteParameterized(LogTextLevel.Debug, messageTemplate, args);
        }

        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="ex">错误信息</param>
        public static void Warn(object ex)
        {
            if (!Enable)
            {
                return;
            }
            var msg = ex?.ToString() ?? string.Empty;
            _warnLogger?.Warning(msg);
            FireLogWritten(LogTextLevel.Warn, msg);
        }

        public static void WarnFormat(string messageTemplate, params object[] args)
        {
            WriteParameterized(LogTextLevel.Warn, messageTemplate, args);
        }

        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="ex">错误信息</param>
        public static void Error(object ex)
        {
            if (!Enable)
            {
                return;
            }
            var msg = ex?.ToString() ?? string.Empty;
            _errorLogger?.Error(msg);
            FireLogWritten(LogTextLevel.Error, msg);
        }

        public static void ErrorFormat(string messageTemplate, params object[] args)
        {
            WriteParameterized(LogTextLevel.Error, messageTemplate, args);
        }

        /// <summary>
        /// 记录普通信息，仅写入文件日志，不推送到首页运行消息。
        /// 高频流程日志优先使用此方法，避免 UI 刷屏。
        /// </summary>
        /// <param name="ex">日志内容</param>
        public static void Info(object ex)
        {
            if (!Enable)
            {
                return;
            }
            var msg = ex?.ToString() ?? string.Empty;
            _infoLogger?.Information(msg);
            // FireLogWritten(LogTextLevel.Info, msg);
        }

        /// <summary>
        /// 记录需要展示给运行界面的普通信息：写入文件日志，并触发 OnLogWritten。
        /// </summary>
        public static void InfoFormat(string messageTemplate, params object[] args)
        {
            WriteParameterized(LogTextLevel.Info, messageTemplate, args);
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
                LogTextHelper.Error($"写Json文件异常：{ex}");
            }
        }

    }
}
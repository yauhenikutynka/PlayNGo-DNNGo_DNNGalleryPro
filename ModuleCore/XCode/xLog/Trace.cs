using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Threading;
using System.Diagnostics;

using System.Web;
using System.Configuration;

namespace DNNGo.Modules.DNNGalleryPro
{
    /// <summary>
    /// 日志类
    /// </summary>
    public static class Trace
    {
        #region 初始化处理
        private static StreamWriter LogWriter;
        private static String _LogDir;
        private static Object _LogDir_Lock = new object();
        /// <summary>
        /// 日志目录
        /// </summary>
        public static String LogDir
        {
            get
            {
                if (!String.IsNullOrEmpty(_LogDir)) return _LogDir;
                lock (_LogDir_Lock)
                {
                    if (!String.IsNullOrEmpty(_LogDir)) return _LogDir;
                    
                    if (ConfigurationManager.AppSettings["XLogDir"] != null) //读取配置
                    {
                        _LogDir = ConfigurationManager.AppSettings["XLogDir"].ToString();
                        if (HttpContext.Current != null && _LogDir.Substring(1, 1) != @":")
                            _LogDir = HttpContext.Current.Server.MapPath(_LogDir);
                    }
                    else if (HttpContext.Current != null) //网站使用默认日志目录
                    {
                        _LogDir = HttpContext.Current.Server.MapPath("~/Log/");
                    }
                    else //使用应用程序目录
                    {
                        _LogDir = AppDomain.CurrentDomain.BaseDirectory;
                        if (!String.IsNullOrEmpty(AppDomain.CurrentDomain.RelativeSearchPath))
                            _LogDir = Path.Combine(_LogDir, AppDomain.CurrentDomain.RelativeSearchPath);
                        //检查是否在网站中的Bin目录下，多线程的时候，就无法取得HttpContext.Current
                        //从而不知道当前是WinForm还是网站
                        if (_LogDir.ToLower().EndsWith(@"\bin"))
                        {
                            String str = _LogDir.Substring(0, _LogDir.Length - @"bin".Length);
                            if (File.Exists(Path.Combine(str, "web.config"))) _LogDir = str;
                        }
                        _LogDir = Path.Combine(_LogDir, @"Log\");
                    }

                    //保证\结尾
                    if (!String.IsNullOrEmpty(_LogDir) && LogDir.Substring(_LogDir.Length - 1, 1) != @"\") _LogDir += @"\";

                    return _LogDir;
                }
            }
        }

        /// <summary>
        /// 初始化日志记录文件
        /// </summary>
        private static void InitLog()
        {
            String path = LogDir;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            if (path.Substring(path.Length - 2) != @"\") path += @"\";
            String logfile = path + DateTime.Now.ToString("yyyy_MM_dd") + ".log";
            int i = 0;
            while (i < 10)
            {
                try
                {
                    LogWriter = new StreamWriter(logfile, true, Encoding.UTF8);
                    LogWriter.AutoFlush = true;
                    break;
                }
                catch
                {
                    if (logfile.EndsWith("_" + i + ".log"))
                        logfile = logfile.Replace("_" + i + ".log", "_" + (++i) + ".log");
                    else
                        logfile = logfile.Replace(@".log", @"_0.log");
                }
            }
            if (i >= 10) throw new Exception("无法写入日志！");
            LogWriter.WriteLine("\r\n\r\n");
            LogWriter.WriteLine(new String('*', 80));

            String str = DateTime.Now.ToString("HH:mm:ss");
            str += " 进程：" + Process.GetCurrentProcess().Id;
            str += " 线程：" + Thread.CurrentThread.ManagedThreadId;
            if (Thread.CurrentThread.IsThreadPoolThread) str += "(线程池)";
            LogWriter.WriteLine(str + "\t开始记录");
        }

        /// <summary>
        /// 停止日志
        /// </summary>
        private static void CloseWriter(Object obj)
        {
            if (LogWriter == null) return;
            lock (Log_Lock)
            {
                try
                {
                    if (LogWriter == null) return;
                    LogWriter.Close();
                    LogWriter.Dispose();
                    LogWriter = null;
                }
                catch { }
            }
        }
        #endregion

        #region 异步写日志
        private static Timer AutoCloseWriterTimer;
        private static object Log_Lock = new object();
        /// <summary>
        /// 使用线程池线程异步执行日志写入动作
        /// </summary>
        /// <param name="obj"></param>
        private static void PerformWriteLog(Object obj)
        {
            lock (Log_Lock)
            {
                try
                {
                    // 初始化日志读写器
                    if (LogWriter == null) InitLog();
                    // 写日志
                    LogWriter.WriteLine((String)obj);
                    // 声明自动关闭日志读写器的定时器。无限延长时间，实际上不工作
                    if (AutoCloseWriterTimer == null) AutoCloseWriterTimer = new Timer(new TimerCallback(CloseWriter), null, Timeout.Infinite, Timeout.Infinite);
                    // 改变定时器为5秒后触发一次。如果5秒内有多次写日志操作，估计定时器不会触发，直到空闲五秒为止
                    AutoCloseWriterTimer.Change(5000, Timeout.Infinite);
                }
                catch { }
            }
        }
        #endregion

        #region 写日志
        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="msg">信息</param>
        public static void WriteLine(String msg)
        {
            String str = DateTime.Now.ToString("HH:mm:ss.fff");
            str += " 线程：" + Thread.CurrentThread.ManagedThreadId;
            if (Thread.CurrentThread.IsThreadPoolThread) str += "(线程池)";

            // 此时还在异常线程中，可以使用HttpContext，强制初始化日志目录
            String s = LogDir;

            // 使用线程池线程写入日志
            ThreadPool.QueueUserWorkItem(new WaitCallback(PerformWriteLog), str + "\t" + msg);
        }

        /// <summary>
        /// 调试。
        /// 输出堆栈信息，用于调试时处理调用上下文。
        /// 本方法会造成大量日志，请慎用。
        /// </summary>
        public static void Debug()
        {
            Debug(int.MaxValue);
        }

        /// <summary>
        /// 调试。
        /// </summary>
        /// <param name="maxNum">最大捕获堆栈方法数</param>
        public static void Debug(int maxNum)
        {
            int skipFrames = 1;
            if (maxNum == int.MaxValue) skipFrames = 2;
            StackTrace st = new StackTrace(skipFrames, true);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("调用堆栈：");
            int count = Math.Min(maxNum, st.FrameCount);
            for (int i = 0; i < count; i++)
            {
                StackFrame sf = st.GetFrame(i);
                sb.AppendFormat("{0}->{1}", sf.GetMethod().DeclaringType.FullName, sf.GetMethod().ToString());
                if (i < count - 1) sb.AppendLine();
                //sb.AppendLine(sf.ToString());
            }
            WriteLine(sb.ToString());
        }
        #endregion
    }
}
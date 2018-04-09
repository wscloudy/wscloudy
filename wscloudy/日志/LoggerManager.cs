using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;

/// <summary>
/// LOG格式化、初始化分方法
/// </summary>
namespace wscloudy.Log
{
    public class LoggerManager
    {
        /// <summary>
        /// 日志队列
        /// </summary>
        private static IList<string> s_loggerList = new List<string>();
        /// <summary>
        /// 日志写入锁
        /// </summary>
        private static object writeLock = new object();
        /// <summary>
        /// 日志目录路径
        /// </summary>
        public static string m_logPath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";

        private static LoggerManager Constant = new LoggerManager();
        private LoggerManager()
        {
            ThreadPool.QueueUserWorkItem(w =>
            {
                WriteToFile();
            });
        }

        public static void AddLogger(string msg)
        {
            lock (Constant)
            {
                s_loggerList.Add(msg);
            }
        }

        private void WriteToFile()
        {
            string filePath = string.Empty;
            IList<string> logList = null;
            // 如果目录不存在，则创建目录
            if (Directory.Exists(m_logPath) == false)
            {
                Directory.CreateDirectory(m_logPath);
            }
            while (true)
            {
                filePath = Path.Combine(m_logPath,
                string.Format("{0}.{1}", DateTime.Now.ToString("yyyy-MM-dd"), "log"));
                if (s_loggerList.Count <= 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                lock (Constant)
                {
                    logList = new List<string>(s_loggerList);
                    s_loggerList.Clear();
                }

                lock (writeLock)
                {
                    // 将日志消息写入日志文件
                    using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                    {
                        foreach (var log in logList)
                        {
                            byte[] bytes = Encoding.Default.GetBytes(log);
                            fs.Write(bytes, 0, bytes.Length);  
                        }
                        fs.Close();
                    }
                }
                Thread.Sleep(1000 / (s_loggerList.Count + 1));
            }
        }
    }
}

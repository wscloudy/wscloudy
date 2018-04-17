using System;
using System.Collections.Generic;

/// <summary>
/// LOG底层实现方法
/// </summary>
namespace wscloudy.Log
{
    public class Logger                                                             // : ILogger
    {
        /// <summary>
        /// 枚举注释的自定义属性类
        /// </summary>
        public class EnumDescriptionAttribute : Attribute
        {
            private string m_strDescription;
            public EnumDescriptionAttribute(string strPrinterName)
            {
                m_strDescription = strPrinterName;
            }

            public string Description
            {
                get { return m_strDescription; }
            }
        }

        /// <summary>
        /// 执行类型
        /// </summary>
        enum LogType
        {
            /// <summary>
            /// 成功
            /// </summary>
            [EnumDescription("成功")]
            Success = 0x01,
            /// <summary>
            /// 调试
            /// </summary>
            [EnumDescription("调试")]
            Debug = 0x02,
            /// <summary>
            /// 错误
            /// </summary>
            [EnumDescription("错误")]
            Error = 0x03,
            /// <summary>
            /// 故障
            /// </summary>
            [EnumDescription("故障")]
            Fatal = 0x04,
            /// <summary>
            /// 信息
            /// </summary>
            [EnumDescription("信息")]
            Info = 0x05,
            /// <summary>
            /// 警告
            /// </summary>
            [EnumDescription("警告")]
            Warn = 0x06
        }
        /// <summary>
        /// 成功信息
        /// </summary>
        /// <param name="message"></param>
        public void Success(object message)
        {
            WriteToFile(LogType.Success, message);
        }
        /// <summary>
        /// 调式信息
        /// </summary>
        /// <param name="message"></param>
        public void Debug(object message)
        {
#if DEBUG
            WriteToFile(LogType.Debug, message);
#endif
        }
        /// <summary>
        /// 调式信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Debug(object message, Exception exception)
        {
#if DEBUG
            WriteToFile(LogType.Debug, message, exception);
#endif
        }
        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="message"></param>
        public void Error(object message)
        {
            WriteToFile(LogType.Error, message);
        }
        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Error(object message, Exception exception)
        {
            WriteToFile(LogType.Error, message, exception);
        }
        /// <summary>
        /// 故障信息
        /// </summary>
        /// <param name="message"></param>
        public void Fatal(object message)
        {
            WriteToFile(LogType.Fatal, message);
        }
        /// <summary>
        /// 故障信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="excepiton"></param>
        public void Fatal(object message, Exception exception)
        {
            WriteToFile(LogType.Fatal, message, exception);
        }
        /// <summary>
        /// 信息
        /// </summary>
        /// <param name="message"></param>
        public void Info(object message)
        {
            WriteToFile(LogType.Info, message);
        }
        /// <summary>
        /// 信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Info(object message, Exception exception)
        {
            WriteToFile(LogType.Info, message, exception);
        }
        /// <summary>
        /// 警告信息
        /// </summary>
        /// <param name="message"></param>
        public void Warn(object message)
        {
            WriteToFile(LogType.Warn, message);
        }
        /// <summary>
        /// 警告信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Warn(object message, Exception exception)
        {
            WriteToFile(LogType.Warn, message, exception);
        }
        /// <summary>
        /// 写入日志队列中，等待系统统一处理
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        private void WriteToFile(LogType type, object message, Exception exception = null)
        {

            // 获取枚举的描述信息
            var objType = type.GetType().GetField(type.ToString()).GetCustomAttributes(false);
            string strType = objType.Length > 0 ? ((EnumDescriptionAttribute)objType[0]).Description: "Log";
            // 得到日志数据
            string log = string.Empty;
            if (exception == null)
            {
                log = string.Format("[{0}]{1}:{2}{3}",
                   DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), strType,
                   (message == null ? "null" : message.ToString()), Environment.NewLine);
            }
            else
            {
#if DEBUG
                log = string.Format("[{0}]{1}:{2}|Exception:{3}{4}",
                   DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), strType,
                   (message == null ? "null" : message.ToString()), exception.ToString(), Environment.NewLine);
#else
                log = string.Format("[{0}]{1}:{2}|Exception:{3}{4}",
                   DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), strType,
                   (message == null ? "null" : message.ToString()), exception.Message, Environment.NewLine);
#endif
            }

            LoggerManager.AddLogger(log);
        }



        public void InitConfig(IDictionary<string, object> cfg)
        {
            //throw new NotImplementedException();
        }


        public void Success(string format, params object[] args)
        {
            WriteToFile(LogType.Success, string.Format(format, args));
        }

        public void Debug(string format, params object[] args)
        {
            WriteToFile(LogType.Debug, string.Format(format, args));
        }

        public void Debug(Exception exception, string format, params object[] args)
        {
            WriteToFile(LogType.Debug, string.Format(format, args), exception);
        }

        public void Error(string format, params object[] args)
        {
            WriteToFile(LogType.Error, string.Format(format, args));
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            WriteToFile(LogType.Error, string.Format(format, args), exception);
        }

        public void Fatal(string format, params object[] args)
        {
            WriteToFile(LogType.Fatal, string.Format(format, args));
        }

        public void Fatal(Exception exception, string format, params object[] args)
        {
            WriteToFile(LogType.Fatal, string.Format(format, args), exception);
        }

        public void Info(string format, params object[] args)
        {
            WriteToFile(LogType.Fatal, string.Format(format, args));
        }

        public void Info(Exception exception, string format, params object[] args)
        {
            WriteToFile(LogType.Info, string.Format(format, args), exception);
        }

        public void Warn(string format, params object[] args)
        {
            WriteToFile(LogType.Info, string.Format(format, args));
        }

        public void Warn(Exception exception, string format, params object[] args)
        {
            WriteToFile(LogType.Info, string.Format(format, args), exception);
        }
    }
}

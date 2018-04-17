/********************************************************************************
** auth： 邹云
** date： 2016-7-20
** desc：日志写入
** Ver.:  V1.0.0
** 静态调用方式范例 Log.logger.Info(string);
*********************************************************************************/

/// <summary>
/// LOG应用层直接调用方法，静态类
/// </summary>
namespace wscloudy.Log
{
    /// <summary>
    /// 简易日志接口，默认存放于当前目录下logs文件夹
    /// 调用范例：Log.logger.XXX(...)
    /// 文件夹路径读取：LoggerManager.m_logPath
    /// 日志固定文件名：DateTime.Now.ToString("yyyy-MM-dd") + ".log"
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// 调试接口列表
        /// </summary>
        public static Logger logger = new Logger();

    }
}

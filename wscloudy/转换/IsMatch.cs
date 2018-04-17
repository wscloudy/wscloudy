using System.Text.RegularExpressions;

/// <summary>
/// 判断字符串等是否符合要求或者规则
/// </summary>
namespace wscloudy.Match
{
    /// <summary>
    /// 判断字符串等是否符合要求或者规则
    /// </summary>
    public static class IsMatch
    {
        /// <summary>
        /// 判断是否包含中文
        /// </summary>
        /// <param name="str">需要判断的字符串</param>
        /// <returns>符合返回true</returns>
        public static bool IsChinese(string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]"); ;
        }

        /// <summary>
        /// 判断是否是否只包含数字和英文字母
        /// </summary>
        /// <param name="str">需要判断的字符串</param>
        /// <returns>符合返回true</returns>
        public static bool IsNumAndCharacter(string str)
        {
            Regex regex = new Regex(@"^[A-Za-z0-9]+$");
            return regex.IsMatch(str); ;
        }

        /// <summary>
        /// 判断是否是否只包含邮件地址
        /// </summary>
        /// <param name="str">需要判断的字符串</param>
        /// <returns>符合返回true</returns>
        public static bool IsEmail(string str)
        {
            return Regex.IsMatch(str, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"); ;
        }

        /// <summary>
        /// 判断是否是否包含IPv6地址
        /// </summary>
        /// <param name="str">需要判断的字符串</param>
        /// <returns>符合返回true</returns>
        public static bool IsIPv6(string str)
        {
            return Regex.IsMatch(str, "^((([0-9A-Fa-f]{1,4}:){7}[0-9A-Fa-f]{1,4})|(([0-9A-Fa-f]{1,4}:){1,7}:)|(([0-9A-Fa-f]{1,4}:){6}:[0-9A-Fa-f]{1,4})|(([0-9A-Fa-f]{1,4}:){5}(:[0-9A-Fa-f]{1,4}){1,2})|(([0-9A-Fa-f]{1,4}:){4}(:[0-9A-Fa-f]{1,4}){1,3})|(([0-9A-Fa-f]{1,4}:){3}(:[0-9A-Fa-f]{1,4}){1,4})|(([0-9A-Fa-f]{1,4}:){2}(:[0-9A-Fa-f]{1,4}){1,5})|([0-9A-Fa-f]{1,4}:(:[0-9A-Fa-f]{1,4}){1,6})|(:(:[0-9A-Fa-f]{1,4}){1,7})|(([0-9A-Fa-f]{1,4}:){6}(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])(\\.(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])){3})|(([0-9A-Fa-f]{1,4}:){5}:(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])(\\.(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])){3})|(([0-9A-Fa-f]{1,4}:){4}(:[0-9A-Fa-f]{1,4}){0,1}:(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])(\\.(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])){3})|(([0-9A-Fa-f]{1,4}:){3}(:[0-9A-Fa-f]{1,4}){0,2}:(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])(\\.(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])){3})|(([0-9A-Fa-f]{1,4}:){2}(:[0-9A-Fa-f]{1,4}){0,3}:(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])(\\.(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])){3})|([0-9A-Fa-f]{1,4}:(:[0-9A-Fa-f]{1,4}){0,4}:(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])(\\.(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])){3})|(:(:[0-9A-Fa-f]{1,4}){0,5}:(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])(\\.(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])){3}))$");
        }

        /// <summary>
        /// 判断是否是否包含IPv4地址
        /// </summary>
        /// <param name="str">需要判断的字符串</param>
        /// <returns>符合返回true</returns>
        public static bool IsIPv4(string str)
        {
            return Regex.IsMatch(str, @"((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)");
        }
    }

    namespace wscloudy.Converts
    {
        public static class ConvertsWithMatch
        {
            /// <summary>
            /// 解析GPS信息
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static System.Text.RegularExpressions.Match ToGpsLocation(string str = @"$GPRMC,121252.000,A,3958.3032,N,11629.6046,E,15.15,359.95,070306,,,A*54")
            {
                Regex reg = new Regex(@"^\$GPRMC,[\d\.]*,[A|V],(-?[0-9]*\.?[0-9]+),([NS]*),(-?[0-9]*\.?[0-9]+),([EW]*),.*");
                return reg.Match(str);
            }
        }
    }
}

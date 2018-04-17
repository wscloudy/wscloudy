using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

/// <summary>
/// 网络相关操作和转换的独立工具集
/// </summary>
namespace wscloudy.NetTool
{
    public class NetTools
    {
        /// <summary>
        /// 根据IP地址获得主机名称
        /// </summary>
        /// <param name="ip">主机的IP地址</param>
        /// <returns>主机名称</returns>
        public static string GetHostNameByIp(string ip)
        {
            ip = ip.Trim();
            if (ip == string.Empty)
                return string.Empty;
            try
            {
                System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(ip);
                return host.HostName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 根据主机名（域名）获得主机的IP地址
        /// </summary>
        /// <param name="hostName">主机名或域名</param>
        /// <example>GetIPByDomain("pc001"); GetIPByDomain("www.google.com");</example>
        /// <returns>主机的IP地址</returns>
        public static string GetIpByHostName(string hostName)
        {
            hostName = hostName.Trim();
            if (hostName == string.Empty)
                return string.Empty;
            try
            {
                System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(hostName);
                return host.AddressList.GetValue(0).ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取本机ip
        /// </summary>
        /// <returns></returns>
        public static IPAddress[] GetLocalIP()
        {
            IPAddress[] IP = Dns.GetHostAddresses(Dns.GetHostName());
            return IP;
        }
    }
}

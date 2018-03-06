using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace AlarmTool
{
    /// <summary>
    /// 构造一个对称算法,使用3Des加密
    ///如果当前的 Key 属性为 NULL，可调用 GenerateKey 方法以创建新的随机 Key。 
    ///如果当前的 IV 属性为 NULL，可调用 GenerateIV 方法以创建新的随机 IV
    /// </summary>
    public class CryptoTripleDes
    {
        /// <summary>
        ///3DES加密
        /// </summary>
        /// <param name="originalValue">加密数据</param>
        /// <param name="key">24位字符的密钥字符串</param>
        /// <param name="IV">8位字符的初始化向量字符串</param>
        /// <returns></returns>
        public static string DESEncrypt(string originalValue, string key, string IV)
        {

            SymmetricAlgorithm sa;
            ICryptoTransform ct;
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;
            sa = new TripleDESCryptoServiceProvider();
            sa.Key = Encoding.UTF8.GetBytes(key);
            sa.IV = Encoding.UTF8.GetBytes(IV);
            ct = sa.CreateEncryptor();
            byt = Encoding.UTF8.GetBytes(originalValue);
            ms = new MemoryStream();
            cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byt, 0, byt.Length);
            cs.FlushFinalBlock();
            cs.Close();
            return Convert.ToBase64String(ms.ToArray());
        }
        /// <summary>
        /// 3DES解密
        /// </summary>
        /// <param name="data">解密数据</param>
        /// <param name="key">24位字符的密钥字符串(需要和加密时相同)</param>
        /// <param name="iv">8位字符的初始化向量字符串(需要和加密时相同)</param>
        /// <returns></returns>
        public static string DESDecrypst(string data, string key, string IV)
        {
            SymmetricAlgorithm mCSP = new TripleDESCryptoServiceProvider();
            mCSP.Key = Encoding.UTF8.GetBytes(key);
            mCSP.IV = Encoding.UTF8.GetBytes(IV);
            ICryptoTransform ct;
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;
            ct = mCSP.CreateDecryptor(mCSP.Key, mCSP.IV);
            byt = Convert.FromBase64String(data);
            ms = new MemoryStream();
            cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byt, 0, byt.Length);
            cs.FlushFinalBlock();
            cs.Close();
            return Encoding.UTF8.GetString(ms.ToArray());

        }
    }
}

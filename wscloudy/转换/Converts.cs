using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// 各种转换
/// </summary>
namespace wscloudy.Converts
{
    public class Converts
    {
        /// <summary>
        /// 将Dataset序列化成byte[]
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static byte[] DataSetToByte(DataSet ds)
        {
            byte[] bArrayResult = null;
            ds.RemotingFormat = SerializationFormat.Binary;
            MemoryStream ms = new MemoryStream();
            IFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, ds);
            bArrayResult = ms.ToArray();
            ms.Close();
            ms.Dispose();
            return bArrayResult;
        }

        /// <summary>
        /// 将byte[]反序列化成Dataset
        /// </summary>
        /// <param name="bArrayResult"></param>
        /// <returns></returns>
        public static DataSet ByteToDataset(byte[] bArrayResult, int index = 0)
        {
            DataSet dsResult = null;
            MemoryStream ms = new MemoryStream(bArrayResult, index, bArrayResult.Length-index);
            IFormatter bf = new BinaryFormatter();
            object obj = bf.Deserialize(ms);
            dsResult = (DataSet)obj;
            ms.Close();
            ms.Dispose();
            return dsResult;
        }

        /// <summary>
        /// 结构体转byte数组
        /// </summary>
        /// <param name="structObj">要转换的结构体数组</param>
        /// <returns>转换后的byte数组</returns>
        public static byte[] StructToBytes<T>(List<T> structObj)
        {
            List<byte> ret = new List<byte>();
            if (structObj == null || structObj.Count == 0)
            {
                return new byte[0];
            }
            //得到结构体的大小
            int size = Marshal.SizeOf(structObj[0]);
            foreach (T obj in structObj)
            {
                //创建byte数组
                byte[] bytes = new byte[size];
                //分配结构体大小的内存空间
                IntPtr structPtr = Marshal.AllocHGlobal(size);
                //将结构体拷到分配好的内存空间
                Marshal.StructureToPtr(obj, structPtr, false);
                //从内存空间拷到byte数组
                Marshal.Copy(structPtr, bytes, 0, size);
                //释放内存空间
                Marshal.FreeHGlobal(structPtr);
                ret.AddRange(bytes);
            }
            //返回byte数组
            return ret.ToArray();
        }

        /// <summary>
        /// byte数组转结构体
        /// </summary>
        /// <param name="bytes">byte数组</param>
        /// <param name="type">结构体类型</param>
        /// <param name="index">设置从0开始的起始转换字节</param>
        /// <returns>转换后的结构体数组</returns>
        public static T[] BytesToStuct<T>(byte[] bytes, Type type, int index = 0)
        {
            List<T> ret = new List<T>();
            int size = Marshal.SizeOf(type);//得到结构体的大小
            int ptr = index;
            int lenth = bytes.Length - index;
            //byte数组长度小于结构体的大小
            if (size > bytes.Length - index)
            {
                //返回空
                return null;
            }
            while (ptr <= lenth - size)
            {
                //分配结构体大小的内存空间
                IntPtr structPtr = Marshal.AllocHGlobal(size);
                //将byte数组拷到分配好的内存空间
                Marshal.Copy(bytes, ptr, structPtr, size);
                //将内存空间转换为目标结构体
                T obj = (T)Marshal.PtrToStructure(structPtr, type);
                //释放内存空间
                Marshal.FreeHGlobal(structPtr);
                //返回结构体
                ptr += size;
                ret.Add(obj);
            }
            return ret.ToArray();
        }

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string BytesToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2") + " ";
                }
            }
            return returnStr;
        }

        public static byte[] LenthToBytes(UInt16 len)
        {
            return new byte[2] {(byte)(len>>8), (byte)len };
        }

        public static UInt16 BytesToLenth(byte[] buf)
        {
            if (buf == null || buf.Length != 2)
            {
                return 0;
            }
            UInt16 ret = 0;
            ret = (UInt16)((buf[1]) | (UInt16)((buf[0]) << 8));
            return ret;
        }

        /// <summary>
        /// C# byte数组合并(（二进制数组合并）
        /// </summary>
        /// <param name="srcArray1">待合并数组1</param>
        /// <param name="srcArray2">待合并数组2</param>
        /// <returns>合并后的数组</returns>
        public static byte[] CombomBinaryArray(byte[] srcArray1, byte[] srcArray2)
        {
            //根据要合并的两个数组元素总数新建一个数组
            byte[] newArray = new byte[srcArray1.Length + srcArray2.Length];

            //把第一个数组复制到新建数组
            Array.Copy(srcArray1, 0, newArray, 0, srcArray1.Length);

            //把第二个数组复制到新建数组
            Array.Copy(srcArray2, 0, newArray, srcArray1.Length, srcArray2.Length);

            return newArray;
        }

        /// <summary>
        /// 获取（如结构体）中的各个成员名
        /// </summary>
        /// <param name="obj">需要分析的数据类型</param>
        /// <returns></returns>
        public static List<string> GetSubNames(Type t)
        {
            List<string> names = new List<string>();
            FieldInfo[] pp = t.GetFields();
            foreach (FieldInfo p in pp)
            {
                names.Add(p.Name);
            }
            return names;
        }
    }
}

using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace Yun.NetClass
{
    /// <summary>
    /// 网络连接类型
    /// </summary>
    public enum NetType
    {
        TCP,
        UDP
    }

    /// <summary>
    /// LogTrace
    /// </summary>
    public class TraceEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    //转发接收到并且合并后的完整数据
    public class RecvBuffEventArgs : EventArgs
    {
        public byte[] buff { get; set; }
        public EndPoint endPoint { get; set; }
    }

    public class NetHelper
    {
        struct RecvBuff
        {
            public long id;
            public int count;
            public DateTime timeout;
            public byte[][] buf;
        }
        public event EventHandler<RecvBuffEventArgs> RecvCallback = (sender, data) => { };

        NetType netType;
        Socket socket = null;
        Socket fd = null;
        TraceEventArgs log = new TraceEventArgs();
        int maxLength = 1000;
        byte topBuff = 0xA5;//分包包头，后跟总包长度
        byte lastBuff = 0x5A;//分包包尾
        List<RecvBuff> recvBuffs = null;
        /// <summary>
        /// 日志
        /// </summary>
        public event EventHandler<TraceEventArgs> Trace = (sender, data) => { };

        /// <summary>
        /// 创建一个socket并初始化
        /// </summary>
        /// <param name="netType"></param>
        public NetHelper(NetType netType, int maxLength = 30000)
        {
            this.maxLength = maxLength;
            this.netType = netType;
            switch (netType)
            {
                case NetType.UDP:
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    log.Message = "初始化完成";
                    Trace(this, log);
                    break;
                case NetType.TCP:
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    log.Message = "初始化完成";
                    Trace(this, log);
                    break;
            }
            if (socket != null)
            {
                socket.ReceiveBufferSize = int.MaxValue-1;
                socket.SendBufferSize = int.MaxValue-1;
            }
        }

        /// <summary>
        /// 开始监听
        /// </summary>
        /// <param name="port">服务端监听端口号，如果是客户端则为0。默认为客户端</param>
        public void Listener(int port = 0)
        {
            switch (netType)
            {
                case NetType.UDP:
                    socket.Bind(new IPEndPoint(IPAddress.Any, port));
                    break;
                case NetType.TCP:
                    socket.Bind(new IPEndPoint(IPAddress.Any, port));
                    if (port != 0) socket.Listen(100);
                    break;
            }
            log.Message = "监听成功";
            Trace(this, log);
        }

        /// <summary>
        /// 接收字节
        /// </summary>
        /// <param name="remote">远端终点</param>
        /// <param name="timeout">超时，0表示永久阻塞param>
        /// <returns></returns>
        public byte[] Receive(ref EndPoint remote, int timeout = 30000)
        {
            byte[] buff = null;
            int ret = -1;

            if (recvBuffs != null) recvBuffs.Clear();
            recvBuffs = null;
            recvBuffs = new List<RecvBuff>();
            socket.Blocking = true;
            switch (netType)
            {
                case NetType.UDP:
                    if (timeout == 0) timeout = 30000;
                    while (socket != null)
                    {
                        buff = new byte[maxLength + 18];//18位预留控制位
                        ret = socket.ReceiveFrom(buff, ref remote);
                        BuffMerge(buff, remote, ret, timeout);
                    }
                    return null;
                case NetType.TCP:
                    socket.ReceiveTimeout = timeout;
                    buff = new byte[maxLength];
                    if (socket.Connected == false)
                    {
                        fd = socket.Accept();
                        fd.Blocking = true;
                        fd.ReceiveTimeout = timeout;
                        ret = fd.Receive(buff);
                    }
                    else
                    {
                        ret = socket.Receive(buff);
                    }
                    break;
            }
            if (ret == -1 || buff == null)
            {
                log.Message = "接收：无数据或发生错误";
                Trace(this, log);
                return null;
            }
            else
            {
                log.Message = "接收：" + ret.ToString();
                Trace(this, log);
                byte[] tbuf = new byte[ret];
                Buffer.BlockCopy(buff, 0, tbuf, 0, ret);
                return tbuf;
            }
        }

        /// <summary>
        /// 合并分组发送的数据
        /// </summary>
        /// <param name="buff">当前接收的其中一组数据</param>
        private void BuffMerge(byte[] buff, EndPoint remote, int len, int timeout = 30000)
        {
            RecvBuff recvBuff = new RecvBuff();
            //top(1) +区分标识符int64(8) + 分包总数int32(4) + 当前分包int32(4) + data  + last(1)
            if (buff[0] != topBuff) return;
            recvBuff.id = BitConverter.ToInt64(buff, 1);
            int maxCount = BitConverter.ToInt32(buff, 9);
            int count = BitConverter.ToInt32(buff, 13);
            if (buff[len - 1] != lastBuff) return;
            int index = -1;//当前操作的recvBuffs中的序列项
            bool isExist = false;
            for (int i=0; i< recvBuffs.Count; i++)
            {
                if (recvBuffs[i].id == recvBuff.id)
                {
                    isExist = true;
                    index = i;//记录即将修改的位置
                    RecvBuff recvBuff_t = recvBuffs[i];
                    ++recvBuff_t.count;
                    recvBuff_t.timeout = DateTime.Now;
                    recvBuff_t.buf[count] = new byte[len - 18];
                    Buffer.BlockCopy(buff, 17, recvBuff_t.buf[count], 0, recvBuff_t.buf[count].Length);
                    recvBuffs[i] = recvBuff_t;
                    break;
                }
                else
                {
                    TimeSpan ts1 = new TimeSpan(recvBuffs[i].timeout.Ticks);
                    TimeSpan ts2 = new TimeSpan(DateTime.Now.Ticks);
                    TimeSpan ts3 = ts1.Subtract(ts2).Duration();
                    //你想转的格式
                    ts3.TotalMilliseconds.ToString();
                    if (ts3.TotalMilliseconds > timeout)
                    {
                        recvBuffs.RemoveAt(i);//超时，删除该id的全部记录
                        --i;
                    }
                }
            }
            if (!isExist)//如果是没有记录的
            {
                log.Message = "接收到一个连接: " + maxCount.ToString();
                Trace(this, log);
                recvBuff.timeout = DateTime.FromBinary(recvBuff.id);
                recvBuff.buf = new byte[maxCount][];
                recvBuff.buf[count] = new byte[len - 18];
                Buffer.BlockCopy(buff, 17, recvBuff.buf[count], 0, recvBuff.buf[count].Length);
                recvBuff.count = 1;
                index = recvBuffs.Count;//记录即将插入的位置
                recvBuffs.Add(recvBuff);
            }
            if (maxCount == recvBuffs[index].count) //数据已经收集全
            {

                List<byte> callbackBuff = new List<byte>();
                foreach (byte[] tbuf in recvBuffs[index].buf)
                {
                    callbackBuff.AddRange(tbuf);
                }
                RecvBuffEventArgs arg = new RecvBuffEventArgs();
                arg.endPoint = remote;
                arg.buff = callbackBuff.ToArray();
                recvBuffs.RemoveAt(index);//完成任务，删除该id的全部记录
                RecvCallback(this, arg);
            }
            else if (maxCount < recvBuffs[index].count)
            {
                recvBuffs.Clear();
            }
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="buff">字节数组</param>
        /// <param name="remote">远程地址</param>
        /// <param name="TcpTimeout">TCP模式下的超时时间</param>
        /// <returns></returns>
        public int Send(byte[] buff, EndPoint remote, uint TcpTimeout = 1500)
        {
            int ret = 0;
            switch (netType)
            {
                case NetType.UDP:
                    byte[][] sendBuff = BuffCut(buff);
                    foreach (byte[] buf in sendBuff)
                    {
                        ret += socket.SendTo(buf, remote);
                        Thread.Sleep(10);
                    }
                    sendBuff = null;
                    break;
                case NetType.TCP:
                    log.Message = "连接中，请等待";
                    Trace(this, log);
                    if (fd != null && fd.Connected == true)
                    {
                        ret += fd.Send(buff);
                    }
                    else
                    {
                        if (socket.Connected == false)
                        {
                            socket.Blocking = true;
                            Thread.Sleep(100);
                            new Thread(new ParameterizedThreadStart(TcpConnectThread)).Start(remote);
                            //socket.Connect(remote);
                            Thread.Sleep((int)TcpTimeout);
                        }
                        if (socket.Connected == true)
                        {
                            ret += socket.Send(buff);
                        }
                        else
                        {
                            log.Message = "TCP发送失败！";
                            Trace(this, log);
                        }
                    }
                    break;
            }
            if (ret == 0)
            {
                log.Message = "发送：无数据或发生错误";
                Trace(this, log);
                return ret;
            }
            else
            {
                if (netType == NetType.TCP)
                {
                    byte[] buf = new byte[ret];
                    Array.Copy(buff, buf, ret);
                }
                log.Message = "发送：" + ret.ToString();
                Trace(this, log);
                return ret;
            }
        }

        /// <summary>
        /// 分割大数据包
        /// </summary>
        /// <param name="buff">待分组发送的数据</param>
        /// <returns>返回分组后的数据</returns>
        private byte[][] BuffCut(byte[] buff)
        {
            int index = 0;
            List<byte[]> buf = new List<byte[]>();
            Int64 id = DateTime.Now.ToBinary();
            int maxCount;
            int count = 0;
            if (buff.Length % maxLength == 0)
            {
                maxCount = buff.Length / maxLength;
            }
            else
            {
                maxCount = buff.Length / maxLength + 1;
            }
            while (index < buff.Length)
            {
                byte[] tbuf = null;
                if (buff.Length - index < maxLength)
                {
                    //top(1) +区分标识符int64(8) + 分包总数int32(4) + 当前分包int32(4) + data  + last(1)
                    tbuf = new byte[buff.Length - index + 18];
                }
                else
                {
                    tbuf = new byte[maxLength + 18];
                }
                tbuf[0] = topBuff;
                Buffer.BlockCopy(BitConverter.GetBytes(id), 0, tbuf, 1, 8);
                Buffer.BlockCopy(BitConverter.GetBytes(maxCount), 0, tbuf, 9, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(count), 0, tbuf, 13, 4);
                Buffer.BlockCopy(buff, index, tbuf, 17, tbuf.Length - 18);
                tbuf[tbuf.Length - 1] = lastBuff;
                buf.Add(tbuf);
                index += maxLength;
                ++count;
            }
            byte[][] ret = buf.ToArray();
            buf.Clear();
            buf = null;
            return ret;
        }

        /// <summary>
        /// tcp连接线程
        /// </summary>
        /// <param name="remote">需要连接的远程终端</param>
        private void TcpConnectThread(object remote)
        {
            try
            {
                socket.Connect((EndPoint)remote);
            }
            catch (Exception ex)
            {
                log.Message = "连接失败：" + ex.Message;
                Trace(this, log);
            }
        }

        /// <summary>
        /// tcp断开当前连接
        /// </summary>
        public void TcpDisconnect()
        {
            try
            {
                if (netType == NetType.TCP)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.DontFragment = true;
                    socket.Disconnect(true);
                    if (fd != null)
                    {
                        if (fd.Connected == true)
                        {
                            fd.Disconnect(false);
                        }
                        fd.Dispose();
                        fd = null;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 释放占用的资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (socket.Connected)
                {
                    socket.Disconnect(false);
                }
                socket.Close();
            }
            catch { }
            socket = null;
            log = null;
            Thread.Sleep(100);
        }
    }
}

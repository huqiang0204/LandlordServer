using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace huqiang
{
    public class EnvelopeType 
    {
        public const byte Mate = 0;
        public const byte AesMate = 1;
        public const byte Json = 2;
        public const byte AesJson = 3;
        public const byte DataBuffer = 4;
        public const byte AesDataBuffer = 5;
        public const byte String = 6;
        public const byte AesString = 7;
    }
    public struct EnvelopeHead
    {
        /// <summary>
        /// 前三个自己为id，第四字节为tag <<=24
        /// </summary>
        public UInt32 Tag;
        public UInt32 Lenth;
        public UInt16 CurPart;
        public UInt16 AllPart;
        public UInt32 PartLen;
    }
    public struct EnvelopeItem
    {
        public EnvelopeHead head;
        public Int32 part;
        public UInt32 rcvLen;
        public byte[] buff;
    }
    class SocData
    {
        public byte tag;
        public byte[] data;
        public object obj;
    }
    public class TcpSocket
    {
        const int bufferSize = 262144;
        EnvelopeBuffer envelope;
        Thread thread;
        private Socket client = null;
        public bool isConnection { get { if (client == null) return false; return client.Connected; } }
        IPEndPoint iep;
        Queue<SocData> queue;
        public TcpSocket(int bs = 262144,PackType type = PackType.All,int es = 262144)
        {
            buffer = new byte[bs];
            if(type!=PackType.None)
            {
                Packaging = true;
                envelope = new EnvelopeBuffer(es);
                envelope.type = type;
            }
            queue = new Queue<SocData>();
        }
        byte[] buffer;
        bool reConnect=false;
        void Run()
        {
            while (true)
            {
                if (close)
                {
                    if (client != null)
                    {
                        if (client.Connected)
                            client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                    break;
                }
                if (client != null)
                {
                    if (client.Connected)
                    {
                        Receive();
                        if(redic)
                        {
                            if (client.Connected)
                                client.Shutdown(SocketShutdown.Both);
                            client.Close();
                            Connect();
                        }
                    }
                    if (reConnect)
                    {
                        try
                        {
                            client.Close();
                        }
                        catch (Exception ex)
                        {
                        }
                        Connect();
                    }
                }
                else Connect();
            }
            thread = null;
            client = null;
        }
        void Connect()
        {
            try
            {
                redic = false;
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.ReceiveTimeout = 2000;
                client.SendTimeout = 100;
                client.Connect(iep);
                reConnect = false;
                if (client.Connected)
                {
                    envelope.Clear();
                    if (Connected != null)
                        Connected();
                }
            }
            catch (Exception ex)
            {
                client.Close();
                if (ConnectFaild != null)
                    ConnectFaild(ex.StackTrace);
            }
        }
        void Receive()
        {
            try
            {
                int len = client.Receive(buffer);
                if(Packaging)
                {
                    var dat = envelope.Unpack(buffer, len);
                    if(dat!=null)
                    {
                        for (int i = 0; i < dat.Count; i++)
                        {
                            var item = dat[i];
                            EnvelopeCallback(item.data,item.tag);
                        }
                    }
                }
                else
                {
                    byte[] tmp = new byte[len];
                    for (int i = 0; i < len; i++)
                        tmp[i] = buffer[i];
                    if(auto)
                    {
                        if (a_Dispatch != null)
                            a_Dispatch(tmp,0,null);
                    }else
                    {
                        SocData soc = new SocData();
                        soc.data = tmp;
                        lock (queue)
                            queue.Enqueue(soc);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Packaging)
                    envelope.Clear();
                //if (ConnectFaild != null)
                //    ConnectFaild(ex.StackTrace);
            }
        }
        void EnvelopeCallback(byte[] data,byte tag)
        {
            object obj = null;
            if(SelfAnalytical!=null)
            {
                try
                {
                   obj = SelfAnalytical(data, tag);
                }catch(Exception ex)
                {
                }
            }
            if (auto)
            {
                if (a_Dispatch != null)
                    a_Dispatch(data, tag, null);
            }
            else {
                SocData soc = new SocData();
                soc.data = data;
                soc.tag =tag;
                soc.obj = obj;
                lock (queue)
                    queue.Enqueue(soc);
            }
        }
        unsafe int GetLenth(byte[] buff, int index)
        {
            fixed (byte* b = &buff[index])
            {
                return *(Int32*)b;
            }
        }
        bool auto = true;
        Action<byte[], UInt32,object> a_Dispatch;
        /// <summary>
        /// 设置消息派发函数
        /// </summary>
        /// <param name="DispatchMessage"></param>
        /// <param name="autodispatch">true由socket本身的线程进行派发，false为手动派发，请使用update函数</param>
        /// <param name="buff_size">手动派发时，缓存消息的队列大小,默认最小为32</param>
        public void SetDispatchMethod(Action<byte[], UInt32,object> DispatchMessage, bool autodispatch = true)
        {
            a_Dispatch = DispatchMessage;
            auto = autodispatch;
            //if (!auto)
            //{
            //    if (buff_size < 16)
            //        buff_size = 16;
            //    //drm = new DataReaderManage(buff_size);
            //}
        }
        public void ConnectServer(IPAddress ip, int _port)
        {
            if (thread != null)
            {
                return;
            }
            close = false;
            iep = new IPEndPoint(ip, _port);
            if (thread == null)
            {
                thread = new Thread(Run);
                thread.Start();
            }
        }
        /// <summary>
        /// 由子线程解析，请勿访问ui,如果为空则有
        /// </summary>
        public Func<byte[], byte, object> SelfAnalytical;
        /// <summary>
        /// 由其它线程进行消息派发，当异步线程终止时，开启线程
        /// </summary>
        public void Dispatch()
        {
            if(queue!=null)
            {
                int c = queue.Count;
                SocData soc;
                for (int i = 0; i < c; i++)
                {
                    lock (queue)
                        soc = queue.Dequeue();
                    if (a_Dispatch != null)
                        a_Dispatch(soc.data, soc.tag, soc.obj);
                }
            }
        }
        /// <summary>
        /// 向服务器发送消息
        /// </summary>
        /// <param name="data"></param>
        public bool SendMessage(byte[] data, byte type)
        {
            if (client == null)
                return false;
            try
            {
                if (Packaging)
                {
                    var buf = envelope.Pack(data, type);
                    if (buf != null)
                        for (int i = 0; i < buf.Length; i++)
                            client.Send(buf[i]);
                }
                else client.Send(data);
            }
            catch (Exception ex)
            {
                reConnect = true;
                return false;
            }
            return true;
        }
        bool close;
        public void Close()
        {
            close = true;
        }
        public Action Connected;
        public Action<string> ConnectFaild;
        public bool Packaging;
        bool redic;
        public void Redirect(IPEndPoint iPEnd)
        {
            if(iep!=null)
            {
                if (iep.Address.Equals(iPEnd.Address))
                    if (iep.Port == iPEnd.Port)
                        return;
            }
            iep = iPEnd;
            redic = true;
        }
    }
}
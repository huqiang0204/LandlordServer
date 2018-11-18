using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace huqiang
{
    public class UdpSocket
    {
        UdpClient soc;
        Thread thread;
        EnvelopeBuffer envelope;
        IPEndPoint endPoint;
        DataReaderManage drm;
        public bool Packaging = false;
        bool running;
        bool auto;
        public UdpSocket(int port, IPEndPoint remote, bool subThread = true, PackType type = PackType.All, int es = 262144)
        {
            drm = new DataReaderManage(128);
            endPoint = remote;
            //Links = new Linker[thread * 1024];
            soc = new UdpClient(port);
            soc.Client.ReceiveTimeout = 1000;

            if (type != PackType.None)
            {
                Packaging = true;
                envelope = new EnvelopeBuffer(es);
                envelope.type = type;
            }
            running = true;
            auto = subThread;
            if (thread == null)
            {
                thread = new Thread(Run);
                thread.Start();
            }
        }
    
        void Run()
        {
            while (running)
            {
                try
                {
                    byte[] data = soc.Receive(ref endPoint);//接收数据报
                    if (Packaging)
                    {
                        var dat= envelope.Unpack(data, data.Length);
                        if (dat != null)
                        {
                            for (int i = 0; i < dat.Count; i++)
                            {
                                var item = dat[i];
                                EnvelopeCallback(item.data, item.tag);
                            }
                        }
                    }
                    else
                    {
                        EnvelopeCallback(data, 0);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        void EnvelopeCallback(byte[] data,uint tag)
        {
            if (auto)
            {
                if (MainDispatch != null)
                    MainDispatch(data, tag, endPoint);
            }
            else
                drm.PushData(data, tag, endPoint);
        }
        public Action<byte[], UInt32, IPEndPoint> MainDispatch;
        public void Dispatch()
        {
            if (drm != null)
            {
                int c = drm.count;
                for (int i = 0; i < c; i++)
                {
                    var dat = drm.GetNextMetaData();
                    if (dat.data == null)
                        break;
                    if (MainDispatch != null)
                        MainDispatch(dat.data, dat.Tag, dat.obj as IPEndPoint);
                }
            }
        }
        public void Close()
        {
            soc.Close();
            running = false;
        }
        public bool Send(byte[] dat, IPEndPoint point, byte tag)
        {
            try
            {
                if (Packaging)
                {
                    var buf = envelope.Pack(dat, tag);
                    if (buf != null)
                        for (int i = 0; i < buf.Length; i++)
                            soc.Send(buf[i], buf[i].Length, point);
                }
                else soc.Send(dat, dat.Length, point);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void Broadcast(byte[] dat, int port,byte tag)
        {
            var ip = new IPEndPoint(IPAddress.Broadcast, port);
            if (Packaging)
            {
                var buf = envelope.Pack(dat, tag);
                if (buf != null)
                    for (int i = 0; i < buf.Length; i++)
                        soc.Send(buf[i], buf[i].Length, ip);
            }
            else soc.Send(dat, dat.Length,ip);
            endPoint.Address = IPAddress.Any;
        }
        public void Redirect(IPAddress address)
        {
            endPoint.Address = address;
        }
    }
}
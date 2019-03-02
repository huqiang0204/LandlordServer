using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using huqiang;

namespace LandlordServer
{
    public class SocServer
    {
        const int SingleCount=2048;
        Socket soc;
        /// <summary>
        /// 所有玩家的连接
        /// </summary>
        Linker[] Links;
        /// <summary>
        /// 创建一个新的默认连接 参数socket
        /// </summary>
        public Func<Socket, Linker> CreateModle = (s) => { return new Linker(s); };
        /// <summary>
        /// 默认的派发消息
        /// </summary>
        public Action<Linker, byte[]> DispatchMessage = (o, e) => { Console.WriteLine("new message"); };
        /// <summary>
        /// 单例服务器实例
        /// </summary>
        public static SocServer Instance;

        SocketAsyncEventArgs rs;
        Thread server;
        Thread[] threads;
        Timer timer;
        PackType packType;
        IPEndPoint endPoint;
        public SocServer(string ip, int port,PackType type = PackType.Part, int thread = 8)
        {
            packType = type;
            Links = new Linker[thread * SingleCount];
            soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            soc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //端点
            endPoint= new IPEndPoint(IPAddress.Parse(ip), port);
            //绑定
            try
            {
                soc.Bind(endPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            soc.Listen(0);
            Instance = this;
            threads = new Thread[thread];
            for (int i = 0; i < thread; i++)
            {
                threads[i] = new Thread(Run);
                threads[i].Start(i);
            }
        }
        public void Start()
        {
            if(server==null)
            {
                server = new Thread(AcceptClient);
                server.Start();
            }
        }
        Int32 id = 100000;
        byte[] nil = { 0 };
        public void Dispose()
        {
            soc.Disconnect(true);
            soc.Dispose();
            server.Abort();
            for (int i = 0; i < threads.Length; i++)
                threads[i].Abort();
            timer.Dispose();
        }
        void AcceptClient()
        {
            while (true)
            {
                try
                {
                    var client = soc.Accept();
                    for (int i = 0; i < Links.Length; i++)
                    {
                        if (Links[i] == null)
                        {
                            Links[i] = CreateModle(client);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        void Run(object index)
        {
            int os = (int)index;
            while (true)
            {
                int a = DateTime.Now.Millisecond;
                int s = os;
                for (int i = 0; i < SingleCount; i++)
                {
                    var c = Links[s];
                    if (c != null)
                    {
                        c.Recive();
                    }
                    s += 8;
                }
                int t = DateTime.Now.Millisecond;
                t -= a;
                if (t < 0)
                    t += 1000;
                t = 10 - t;
                if (t < 0)
                    t = 10;
                else if (t > 10)
                    t = 10;
                Thread.Sleep(t);
            }
        }

        /// <summary>
        /// 统计tcp连接
        /// </summary>
        void StatisticsTcp()
        {
            int c = 0;
            for (int i = 0; i < Links.Length; i++)
            {
                if (Links[i] != null)
                {
                    c++;
                }
            }
        }
        /// <summary>
        /// 给用户发送心跳
        /// </summary>
        public void Heartbeat()
        {
            int max = threads.Length * SingleCount;
            for (int i = 0; i < max; i++)
            {
                if (Links[i] != null)
                {
                    if (Links[i].Send(nil) < 0)
                    {
                        Links[i].Dispose();
                        Links[i] = null;
                        Console.WriteLine("user break");
                    }
                }
            }
        }
    }
}

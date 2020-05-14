﻿using huqiang;
using LandlordServer.Table;
using System;

namespace LandlordServer
{
    class Program
    {
        static void Main(string[] args)
        {
            UserTable.Initial();
            var kcp = new KcpServer<KcpUser>(8899);
            kcp.OpenHeart();
            kcp.Run();
            while (true)
            {
                var cmd = Console.ReadLine();
                if (cmd == "Close" | cmd == "close")
                    break;
            }
        }
    }
}

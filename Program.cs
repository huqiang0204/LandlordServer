using Data;
using huqiang;
using LandlordServer.Table;
using SqlManager.Sql;
using System;

namespace LandlordServer
{
    class Program
    {
        static void Main(string[] args)
        {
            LocalFile.loadConfig();
            SqlControll.Initial();
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

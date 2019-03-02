using LandlordServer.Table;
using System;

namespace LandlordServer
{
    class Program
    {
        static void Main(string[] args)
        {
            UserTable.Initial();
            //new SocServer("192.168.31.34",6666);
            var soc= new SocServer("192.168.0.196", 6666);
            soc.CreateModle = (o) => { return new User(o); };
            soc.Start();
            while (true)
            {
                var cmd = Console.ReadLine();
                if (cmd == "Close" | cmd == "close")
                    break;
            }
        }
    }
}

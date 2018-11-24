using System;

namespace LandlordServer
{
    class Program
    {
        static void Main(string[] args)
        {
        
            //new SocServer("192.168.31.34",6666);
            new SocServer("192.168.0.196", 6666);
            while (true)
            {
                var cmd = Console.ReadLine();
                if (cmd == "Close" | cmd == "close")
                    break;
            }
        }
    }
}

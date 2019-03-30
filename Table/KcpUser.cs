using huqiang;
using LandlordServer.DataControll;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.Table
{
    public class KcpUser:KcpLink
    {
        public UserInfo userInfo;
        public KcpUser(KcpServer server) : base(server)
        {
            Console.WriteLine("new link");
        }
        public override void Dispatch(byte[] dat, byte tag)
        {
            ServerDataControll.Dispatch(this, dat,tag);
        }
        public override void Disconnect()
        {

        }
        public void Send(byte[][] data)
        {
            try
            {
                for (int i = 0; i < data.Length; i++)
                    kcp.soc.Send(data[i],data[i].Length, endpPoint);
            }
            catch 
            {
            }
        }
    }
}

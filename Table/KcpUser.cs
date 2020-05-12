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
        public KcpUser() 
        {
            Console.WriteLine("new link");
        }
        public override void Dispatch(byte[] dat, byte tag)
        {
            ServerDataControll.Dispatch(this, dat,tag);
        }
        public override void Disconnect()
        {
            kcp.RemoveLink(this);
        }
        public void Send(byte[][] data)
        {
  
        }
    }
}

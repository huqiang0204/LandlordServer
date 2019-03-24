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
           
        }
        public override void Dispatch(byte[] dat, byte tag)
        {
            ServerDataControll.Dispatch(this, dat,tag);
        }
        public override void Disconnect()
        {

        }
        public void Send(byte[] data, byte type = EnvelopeType.Mate)
        {
            try
            {
                var ss = envelope.Pack(data, type);
                for (int i = 0; i < ss.Length; i++)
                    Send(ss[i]);
            }
            catch 
            {
            }

        }
        public void Send(byte[][] data)
        {
            try
            {
                for (int i = 0; i < data.Length; i++)
                    Send(data[i]);
            }
            catch 
            {
            }
        }
        public void Send(string data)
        {
            Send(Encoding.UTF8.GetBytes(data));
        }
    }
}

using huqiang;
using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using LandlordServer.DataControll;
using LandlordServer.Table;
using huqiang.Data;

namespace LandlordServer
{
    /// <summary>
    /// 客户端连接
    /// </summary>
    public class Linker
    {
        public UserInfo userInfo;
        //PlayerInfo playerInfo;
        EnvelopeBuffer envelope;
        internal Socket Link;
        //获取机器唯一id
        public String uniId;
        byte[] buff;
        public Linker(Socket soc, PackType pack = PackType.Part, int buffsize = 4096)
        {
            Link = soc;
            envelope = new EnvelopeBuffer();
            envelope.type = pack;
            var obj = soc.RemoteEndPoint.GetType().GetProperty("Address");
            addr = obj.GetValue(soc.RemoteEndPoint) as IPAddress;
            //生成id
            var buf = addr.GetAddressBytes();
            unsafe
            {
                fixed (byte* bp = &buf[0])
                    ip = *(int*)bp;
            }
            buff = new byte[buffsize];
        }
        //根据ip生成的一个值
        //public int id;
        //ip地址的int值
        public int ip;
        public int port;
        //玩家选人顺序 (0为1p,1为2p)
        public int seat;
        //玩家登录ip
        public IPAddress addr;
        public int Send(byte[] data,byte type = EnvelopeType.Mate)
        {
            try
            {
                if (Link != null)
                    lock (Link)
                        if (Link.Connected)
                        {
                            var ss = envelope.Pack(data,type);
                            for (int i = 0; i < ss.Length; i++)
                                Link.Send(ss[i]);
                            return 1;
                        }
                        else return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
            return 0;
        }
        public int Send(byte[][] data)
        {
            try
            {
                if (Link != null)
                    lock (Link)
                        if (Link.Connected)
                        {
                            for (int i = 0; i < data.Length; i++)
                                Link.Send(data[i]);
                            return 1;
                        }
                        else return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
            return 0;
        }
        public int Send(string data)
        {
            return Send(Encoding.UTF8.GetBytes(data));
        }
        ~Linker()
        {
            if (Link != null)
                lock (Link)
                    Link.Close();
        }
        public virtual void Dispose()
        {
            if (Link != null)
                lock (Link)
                    Link.Close();
        }
        public void Recive()
        {
            try
            {
                int len = Link.Receive(buff, SocketFlags.Peek);
                if (len > 0)
                {
                    len = Link.Receive(buff);
                    var list= envelope.Unpack(buff,len);
                    try
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            var dat = list[i];
                            ServerDataControll.Dispatch(this, dat.data, dat.tag);
                        }
                    }catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                envelope.Clear();
            }
        }
        public static void SendEmptyDataBuffer(Linker linker, int cmd, int type)
        {
            DataBuffer db = new DataBuffer();
            var fake = new FakeStruct(db, Req.Length);
            fake[Req.Cmd] = cmd;
            fake[Req.Type] = type;
            db.fakeStruct = fake;
            linker.Send(AES.Instance.Encrypt(db.ToBytes()), EnvelopeType.AesDataBuffer);
        }
    }
}


using huqiang;
using huqiang.Data;
using LandlordServer.DataControll;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LandlordServer
{

    /// <summary>
    /// 接收玩家输入信息，左右移动和下蹲需要持续按键
    /// 跳跃只需要接收一次，服务器自动计算出接下来的位置
    /// </summary>
    public class User : Linker
    {
        public User(Socket soc) : base(soc)
        {
            DataBuffer db = new DataBuffer();
            FakeStruct fake = new FakeStruct(db, Req.Length);
            fake[Req.Cmd] = DefCmd.Version;
            fake[Req.Type] = MessageType.Def;
            fake[Req.Args] = Configuration.version;
            db.fakeStruct = fake;
           Send(AES.Instance.Encrypt(db.ToBytes()), EnvelopeType.AesDataBuffer);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (userInfo != null)
            {
                userInfo._Online = false;
                userInfo._LastExit = DateTime.Now.Ticks;
            }
        }

    }
}

using huqiang;
using huqiang.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.DataControll
{
    public class ErrorCode
    {
        public static void SendErrorCode(Linker linker,Int32 error)
        {
            DataBuffer db = new DataBuffer();
            var fake = new FakeStruct(db, Req.Length);
            fake[Req.Cmd] = RpcCmd.Login;
            fake[Req.Type] = MessageType.Rpc;
            fake[Req.Error] = error;
            db.fakeStruct = fake;
            linker.Send(AES.Instance.Encrypt(db.ToBytes()), EnvelopeType.AesDataBuffer);
        }

        public const Int32 NotLogin = 0x100;//你还未登陆
        public const Int32 CreateError = 0x200;//创建房间失败
        public const Int32 JoinRoom = 0x201;//你已加入房间，无法再次创建
        public const Int32 ServerFull = 0x202;//服务器已经爆满，无法创建更多房间
        public const Int32 NoFreeRoom = 0x203;//暂时没有空闲的房间
    }
}

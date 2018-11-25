using huqiang;
using huqiang.Data;
using LandlordServer.Game;
using LandlordServer.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.DataControll
{
    public class RpcCmd
    {
        public const Int32 Login = 0;
        public const Int32 CreateRoom = 1;
        public const Int32 JoinRoom = 2;
        public const Int32 ExitRoom = 3;
        public const Int32 RoomDetail = 4;
        public const Int32 GamerReady = 5;
        public const Int32 HairCards = 6;
        public const Int32 CountDown = 7;
    }
    public class RpcData
    {
        public static void Dispatch(Linker linker, DataBuffer data)
        {
            int cmd = data.fakeStruct[Req.Cmd];
            switch (cmd)
            {
                case RpcCmd.Login:
                    Login(linker, data);
                    break;
                case RpcCmd.CreateRoom:
                    CreateRoom(linker,data);
                    break;
            }
        }
        static void Login(Linker linker, DataBuffer buffer)
        {
            string uid = buffer.fakeStruct.GetData<string>(Req.Args);
            var user = UserTable.AddNewUser(uid);

            DataBuffer db = new DataBuffer();
            var fake = new FakeStruct(db,Req.Length+1);
            fake[Req.Cmd] = RpcCmd.Login;
            fake[Req.Type] = MessageType.Rpc;
            fake[Req.Args] = user.Id;
            fake[Req.Length] = user.RoomId;
            db.fakeStruct = fake;
            linker.Send(AES.Instance.Encrypt(db.ToBytes()), EnvelopeType.AesDataBuffer);

            linker.userInfo = user;
            if(user.RoomId>0)
            {
                var room = RoomManager.QueryRoom(user.RoomId);
                if(room!=null)
                {
                    room.Reconnect(linker);
                }
            }
        }
        static void CreateRoom(Linker linker,DataBuffer buffer)
        {
           var user = linker.userInfo;
           if(user==null)
            {
                ErrorCode.SendErrorCode(linker,ErrorCode.NotLogin);
                return;
            }
            int rid = user.RoomId;
            var room = RoomManager.QueryRoom(rid);
            if(room!=null)
            {
                if(room.ExistUser(user.RoomId))
                {
                    ErrorCode.SendErrorCode(linker,ErrorCode.JoinRoom);
                    return;
                }
                else
                {
                    user.RoomId = 0;
                }
            }
            room= RoomManager.CreateRoom();
            if(room==null)
            {
                ErrorCode.SendErrorCode(linker, ErrorCode.ServerFull);
                return;
            }
            room.JoinRoom(linker);
        }
    }
}

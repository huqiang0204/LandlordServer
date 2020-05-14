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
        public static void Dispatch(KcpUser linker, DataBuffer data)
        {
            int cmd = data.fakeStruct[Req.Cmd];
            switch (cmd)
            {
                case RpcCmd.CreateRoom:
                    CreateRoom(linker,data);
                    break;
                case RpcCmd.ExitRoom:
                    ExitRoom(linker, data);
                    break;
            }
        }
      
        static void CreateRoom(KcpUser linker,DataBuffer buffer)
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
        static void ExitRoom(KcpUser linker, DataBuffer buffer)
        {
            var user = linker.userInfo;
            if (user == null)
            {
                ErrorCode.SendErrorCode(linker, ErrorCode.NotLogin);
                return;
            }
            int rid = user.RoomId;
            var room = RoomManager.QueryRoom(rid);
            if (room != null)
                room.EixtRoom(linker);
        }
    }
}

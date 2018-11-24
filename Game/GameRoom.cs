using huqiang;
using huqiang.Data;
using LandlordServer.DataControll;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.Game
{
    public class GameRoom
    {
        public int Number = 0;
        public int RoomId;
        public string Name;
        GamerInfo[] gamers;
        FightingLandlord landlord;
        public int RoomState = 0;
        public GameRoom()
        {
            gamers = new GamerInfo[3];
            landlord = new FightingLandlord();
        }
        public bool ExistUser(int uid)
        {
            for(int i=0;i<3;i++)
            {
                var gamer = gamers[i].userInfo;
                if(gamer.Id==uid)
                {
                    return true;
                }
            }
            return false;
        }
        public void JoinRoom(Linker linker)
        {
            if (linker.userInfo == null)
                return;
            for (int i=0;i<3;i++)
            {
                var user = gamers[i].userInfo;
                if(user==null)
                {
                    gamers[i].userInfo = linker.userInfo;
                    gamers[i].linker = linker;
                    DataBuffer db = new DataBuffer();
                    var fake = new FakeStruct(db,Req.Length);
                    fake[Req.Cmd] = RpcCmd.JoinRoom;
                    fake[Req.Type] = MessageType.Rpc;
                
                    FakeStruct gamerInfo = new FakeStruct(db,3);
                    gamerInfo[0] = i;//seat
                    gamerInfo[1] = user.Id;//uid
                    gamerInfo[2] = RoomId;//roomid
                    fake.SetData(Req.Args,gamerInfo);
                    db.fakeStruct = fake;

                    Broadcast(db);
                    GetRoomDetail(linker);
                    break;
                }
            }
        }
        public void GetRoomDetail(Linker linker)
        {
            DataBuffer data = new DataBuffer();
            var fake = new FakeStruct(data, Req.Length);
            fake[Req.Cmd] = RpcCmd.RoomDetail;
            fake[Req.Type] = MessageType.Rpc;

            FakeStructArray gs = new FakeStructArray(data,2,3);
            for(int i=0;i<3;i++)
            {
                var user = gamers[i].userInfo;
                if(user!=null)
                {
                    gs[i, 0] = user.Id;
                    gs[i, 1] = 1000;//金币
                }
            }
            fake.SetData(Req.Args,gs);
            data.fakeStruct = fake;
            linker.Send(AES.Instance.Encrypt(data.ToBytes()),EnvelopeType.AesDataBuffer);
        }
        public void Reconnect(Linker linker)
        {
            if (linker.userInfo == null)
                return;
            var uid = linker.userInfo.Id;
            for (int i = 0; i < 3; i++)
            {
                var user = gamers[i].userInfo;
                if (user == null)
                {
                    if (user.Id == uid)
                    {
                        gamers[i].userInfo = linker.userInfo;
                        gamers[i].linker = linker;
                        DataBuffer db = new DataBuffer();
                        var fake = new FakeStruct(db, Req.Length);
                        fake[Req.Cmd] = RpcCmd.JoinRoom;
                        fake[Req.Type] = MessageType.Rpc;

                        FakeStruct gamerInfo = new FakeStruct(db, 3);
                        gamerInfo[0] = i;//seat
                        gamerInfo[1] = user.Id;//uid
                        gamerInfo[2] = RoomId;//roomid
                        fake.SetData(Req.Args, gamerInfo);
                        db.fakeStruct = fake;

                        linker.Send(AES.Instance.Encrypt(db.ToBytes()), EnvelopeType.AesDataBuffer);
                        GetRoomDetail(linker);
                        break;
                    }
                }
            }
        }
        public void EixtRoom(Linker linker)
        {
            if (linker.userInfo == null)
                return;
            if (RoomState > 0)
                return;
            int uid = linker.userInfo.Id;
            for (int i = 0; i < 3; i++)
            {
                var gamer = gamers[i].userInfo;
                if (gamer.Id == uid)
                {
                    gamers[i].linker = null;
                    gamers[i].userInfo = null;
                    Number--;
                    Linker.SendEmptyDataBuffer(linker,RpcCmd.ExitRoom,MessageType.Rpc);
                }
            }
        }
        public void Broadcast(DataBuffer data)
        {
            if (gamers == null)
                return;
            var buf = AES.Instance.Encrypt(data.ToBytes());
            var dat = EnvelopeEx.Pack(buf,EnvelopeType.AesDataBuffer,PackType.Part);
            for (int i=0;i<3;i++)
            {
                var linker = gamers[i].linker;
                if (linker != null)
                {
                    if (linker.Send(dat)<0)
                    {
                        gamers[i].linker = null;
                    }
                }
            }
        }
        public void StartGame(Linker linker)
        {

        }
    }
}
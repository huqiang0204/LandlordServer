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
        const Int32 State_Unready = 0;
        const Int32 State_Ready = 1;
        const Int32 State_HairCards = 2;//发牌
        const Int32 State_RobLandlord = 3;//抢地主
        const Int32 State_Gaming = 4;
        const Int32 State_GameOver = 5;

        public int Number = 0;
        public int RoomId;
        public string Name;
        int Countdown =0;
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

            FakeStructArray gs = new FakeStructArray(data,3,3);
            for(int i=0;i<3;i++)
            {
                var user = gamers[i].userInfo;
                if(user!=null)
                {
                    gs[i, 0] = user.Id;
                    gs[i, 1] = 1000;//金币
                    gs[i, 2] = gamers[i].ready;
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
            if (RoomState >State_Unready)
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
        public void Ready(Linker linker,Int32 value)
        {
            if (RoomState > State_Unready)
                return;
            int uid = linker.userInfo.Id;
           for(int i=0;i<3;i++)
            {
                var user = gamers[i].userInfo;
                if(user!=null)
                {
                    if(user.Id==uid)
                    {
                        gamers[i].ready = value;
                        DataBuffer db = new DataBuffer();
                        var fake = new FakeStruct(db,Req.Length+1);
                        fake[Req.Cmd]=RpcCmd.GamerReady;
                        fake[Req.Type] = MessageType.Rpc;
                        fake[Req.Args] = uid;
                        fake[Req.Length] = value;
                        db.fakeStruct = fake;

                        Broadcast(db);
                        break;
                    }
                }
            }
            int s = 0;
            for(int i=0;i<3;i++)
            {
                if (gamers[i].ready > 0)
                    s++;
            }
            if (s == 3)
            {
                RoomState = State_Ready;
                Countdown  = 3;
                NextStep = StartCountdown;
            }
        }
        void BroadcastCountdown()
        {
            DataBuffer db = new DataBuffer();
            var fake = new FakeStruct(db,Req.Length+1);
            fake[Req.Cmd] = RpcCmd.CountDown;
            fake[Req.Type] = MessageType.Rpc;
            fake[Req.Args] = RoomState;
            fake[Req.Length] = Countdown;
            db.fakeStruct = fake;
            Broadcast(db);
        }
        void StartCountdown()
        {
            Countdown--;
            if (Countdown <= 0)
            {
                RoomState = State_HairCards;
                Countdown = 10;
                landlord.ReStart();
                for(int i=0;i<3;i++)
                {
                    var cards = landlord.GamerCards[i];
                    gamers[i].Cards = new List<int>(cards);
                    var linker = gamers[i].linker;
                    if(linker!=null)
                    {
                        DataBuffer db = new DataBuffer();
                        var fake = new FakeStruct(db,Req.Length);
                        fake[Req.Cmd] = RpcCmd.HairCards;
                        fake[Req.Type] = MessageType.Rpc;
                        fake.SetData(Req.Args,cards);
                        db.fakeStruct = fake;

                        linker.Send(AES.Instance.Encrypt(db.ToBytes()),EnvelopeType.AesDataBuffer);
                    }
                }
            }
            else
            {
                BroadcastCountdown();
            }
        }
        void HairCardsCountdown()
        {
            Countdown--;
            if (Countdown <= 0)
            {
                RoomState = State_HairCards;
                Countdown = 4;
            }
            else BroadcastCountdown();
        }
        void RobLandlord()
        {
            Countdown--;
            if (Countdown <= 0)
            {

            }
            else BroadcastCountdown();
        }
        void Gaming()
        {
            Countdown--;
            if (Countdown <= 0)
            {

            }
            else BroadcastCountdown();
        }
        Action NextStep;
        public void Update()
        {
            if (NextStep != null)
                NextStep();
        }
    }
}
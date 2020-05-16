using huqiang.Data;
using LandlordServer.DataControll;
using LandlordServer.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.Game
{
    public class FLGameRoom:GameRoom
    {
        const Int32 State_Unready = 0;
        const Int32 State_Ready = 1;
        const Int32 State_HairCards = 2;//发牌
        const Int32 State_RobLandlord = 3;//抢地主
        const Int32 State_Gaming = 4;
        const Int32 State_GameOver = 5;
        static int StartIndex = 10000;
        FightingLandlord landlord;
        public FLGameRoom()
        {
            gamers = new GamerInfo[3];
            landlord = new FightingLandlord();
            RoomId = StartIndex;
            StartIndex++;
        }
        void BroadcastCountdown()
        {
            DataBuffer db = new DataBuffer();
            var fake = new FakeStruct(db, Req.Length + 1);
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
                for (int i = 0; i < 3; i++)
                {
                    var cards = landlord.GamerCards[i];
                    gamers[i].Cards = new List<int>(cards);
                    var linker = gamers[i].linker;
                    if (linker != null)
                    {
                        DataBuffer db = new DataBuffer();
                        var fake = new FakeStruct(db, Req.Length);
                        fake[Req.Cmd] = RpcCmd.HairCards;
                        fake[Req.Type] = MessageType.Rpc;
                        fake.SetData(Req.Args, cards);
                        db.fakeStruct = fake;

                        //linker.Send(AES.Instance.Encrypt(db.ToBytes()), EnvelopeType.AesDataBuffer);
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
        public void Ready(KcpUser linker, Int32 value)
        {
            if (RoomState > State_Unready)
                return;
            long uid = linker.userInfo.id;
            for (int i = 0; i < 3; i++)
            {
                var user = gamers[i].userInfo;
                if (user != null)
                {
                    if (user.id == uid)
                    {
                        gamers[i].ready = value;
                        DataBuffer db = new DataBuffer();
                        //var fake = new FakeStruct(db,Req.Length+1);
                        //fake[Req.Cmd]=RpcCmd.GamerReady;
                        //fake[Req.Type] = MessageType.Rpc;
                        //fake[Req.Args] = uid;
                        //fake[Req.Length] = value;
                        //db.fakeStruct = fake;

                        Broadcast(db);
                        break;
                    }
                }
            }
            int s = 0;
            for (int i = 0; i < 3; i++)
            {
                if (gamers[i].ready > 0)
                    s++;
            }
            if (s == 3)
            {
                RoomState = State_Ready;
                Countdown = 3;
                NextStep = StartCountdown;
            }
        }
    }
}

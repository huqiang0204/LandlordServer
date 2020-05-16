using huqiang;
using huqiang.Data;
using LandlordServer.DataControll;
using LandlordServer.Table;
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
        protected int Countdown =0;
        protected GamerInfo[] gamers;
 
        public int RoomState = 0;
        public bool ExistUser(int uid)
        {
            for(int i=0;i<3;i++)
            {
                var gamer = gamers[i].userInfo;
                if(gamer.id==uid)
                {
                    return true;
                }
            }
            return false;
        }
        public virtual void JoinRoom(KcpUser linker)
        {
            if (linker.userInfo == null)
                return;
            for (int i=0;i<3;i++)
            {
                var user = gamers[i].userInfo;
                if(user==null)
                {
                    user =
                    gamers[i].userInfo = linker.userInfo;
                    gamers[i].linker = linker;
                    DataBuffer db = new DataBuffer();
                    var fake = new FakeStruct(db,Req.Length);
                    fake[Req.Cmd] = RpcCmd.JoinRoom;
                    fake[Req.Type] = MessageType.Rpc;
                
                    FakeStruct gamerInfo = new FakeStruct(db,3);
                    //gamerInfo[0] = i;//seat
                    //gamerInfo[1] = user.id;//uid
                    //gamerInfo[2] = RoomId;//roomid
                    fake.SetData(Req.Args,gamerInfo);
                    db.fakeStruct = fake;

                    Broadcast(db);
                    GetRoomDetail(linker);
                    user.RoomId = RoomId;
                    break;
                }
            }
        }
        public void GetRoomDetail(KcpUser linker)
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
                    //gs[i, 0] = user.id;
                    //gs[i, 1] = 1000;//金币
                    //gs[i, 2] = gamers[i].ready;
                }
            }
            fake.SetData(Req.Args,gs);
            data.fakeStruct = fake;
            linker.Send(AES.Instance.Encrypt(data.ToBytes()),EnvelopeType.AesDataBuffer);
        }
        public void Reconnect(KcpUser linker)
        {
            if (linker.userInfo == null)
                return;
            var uid = linker.userInfo.id;
            for (int i = 0; i < 3; i++)
            {
                var user = gamers[i].userInfo;
                if (user == null)
                {
                    if (user.id == uid)
                    {
                        gamers[i].userInfo = linker.userInfo;
                        gamers[i].linker = linker;
                        DataBuffer db = new DataBuffer();
                        var fake = new FakeStruct(db, Req.Length);
                        fake[Req.Cmd] = RpcCmd.JoinRoom;
                        fake[Req.Type] = MessageType.Rpc;

                        FakeStruct gamerInfo = new FakeStruct(db, 3);
                        //gamerInfo[0] = i;//seat
                        //gamerInfo[1] = user.id;//uid
                        //gamerInfo[2] = RoomId;//roomid
                        fake.SetData(Req.Args, gamerInfo);
                        db.fakeStruct = fake;

                        linker.Send(AES.Instance.Encrypt(db.ToBytes()), EnvelopeType.AesDataBuffer);
                        GetRoomDetail(linker);
                        break;
                    }
                }
            }
        }
        public virtual void EixtRoom(KcpUser linker)
        {
            if (gamers == null)
                return;
            if (linker.userInfo == null)
                return;
            //if (RoomState >State_Unready)
            //    return;
            long uid = linker.userInfo.id;
            for (int i = 0; i < gamers.Length; i++)
            {
                var gamer = gamers[i].userInfo;
                if (gamer.id == uid)
                {
                    gamers[i].linker = null;
                    gamers[i].userInfo = null;
                    Number--;
                    //Linker.SendEmptyDataBuffer(linker,RpcCmd.ExitRoom,MessageType.Rpc);
                }
            }
        }
        public void Broadcast(DataBuffer data)
        {
            if (gamers == null)
                return;
            var buf = AES.Instance.Encrypt(data.ToBytes());
            var dat = Envelope.Pack(buf,EnvelopeType.AesDataBuffer,PackType.Part,10000,1472);
            for (int i=0;i<3;i++)
            {
                var linker = gamers[i].linker;
                if (linker != null)
                {
                    //if (linker.Send(dat)<0)
                    //{
                    //    gamers[i].linker = null;
                    //}
                }
            }
        }
   
       
        protected Action NextStep;
        public void Update()
        {
            if (NextStep != null)
                NextStep();
        }
    }
}
using huqiang;
using huqiang.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.Game
{
    public class GameRoom
    {
        public int Number=0;
        public int RoomId;
        public string Name;
        GamerInfo[] gamers;
        public GameRoom()
        {
            gamers = new GamerInfo[3];
        }
        public void Reconnect(Linker linker)
        {

        }
        public bool ExistUser(int uid)
        {
            return false;
        }
        public void JoinRoom(Linker linker)
        {

        }
        public void EixtRoom(Linker linker)
        {

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
    }
}

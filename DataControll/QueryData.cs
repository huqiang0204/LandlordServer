using huqiang;
using huqiang.Data;
using LandlordServer.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.DataControll
{
    public class QueryCmd
    {
        public const Int32 QueryRoom = 0;
    }
    public class QueryData
    {
        public static void Dispatch(Linker linker, DataBuffer data)
        {
            int cmd = data.fakeStruct[Req.Cmd];
            switch (cmd)
            {
                case QueryCmd.QueryRoom:
                    QueryRoom(linker, data);
                    break;
            }
        }
        static void QueryRoom(Linker linker, DataBuffer buffer)
        {
            var list = RoomManager.QueryFreeRoom();
            if(list.Count==0)
            {
                ErrorCode.SendErrorCode(linker, ErrorCode.NoFreeRoom);
                return;
            }
            DataBuffer db = new DataBuffer();
            FakeStruct fake = new FakeStruct(db,Req.Length);
            fake[Req.Cmd] = QueryCmd.QueryRoom;
            fake[Req.Type] = MessageType.Query;
            db.fakeStruct = fake;
            int c = list.Count;
            FakeStructArray array = new FakeStructArray(db,3,c);
            for(int i=0;i<c;i++)
            {
                var room = list[i];
                array[c, 0] = room.RoomId;
                array[c, 1] = room.Number;
                array.SetData(c,2,room.Name);
            }
            fake.SetData(Req.Args, array);
            linker.Send(AES.Instance.Encrypt(db.ToBytes()), EnvelopeType.AesDataBuffer);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LandlordServer.Game
{
    public class RoomManager
    {
        static GameRoom[] rooms=new GameRoom[4096];
        static int max;
        public static GameRoom CreateRoom()
        {
            for(int i=0;i<4096;i++)
            {
                if(rooms[i]==null)
                {
                    var room = new GameRoom();
                    room.RoomId = 10000 + i;
                    return room;
                }
            }
            return null;
        }
        public static void ReleaseRoom(int rid)
        {
            int id = rid - 10000;
            if (id < 0)
                return;
            rooms[id] = null;
        }
        public static GameRoom QueryRoom(int rid)
        {
            int id = rid - 10000;
            if (id < 0)
                return null;
            return rooms[id];
        }
        public static List<GameRoom> QueryFreeRoom()
        {
            List<GameRoom> list = new List<GameRoom>();
            for(int i=0;i<4096;i++)
            {
                if(rooms[i]!=null)
                {
                    if(rooms[i].Number < 3)
                    {
                        list.Add(rooms[i]);
                        if (list.Count >= 20)
                            return list;
                    }
                }
            }
            return list;
        }
        public static void Update()
        {
            for (int i = 0; i < 4096; i++)
            {
                if (rooms[i] == null)
                {
                    rooms[i].Update();
                }
            }
        }
    }
}

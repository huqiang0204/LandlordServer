using LandlordServer.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.Game
{
    public struct GamerInfo
    {
        public List<int> Cards;
        public Linker linker;
        public UserInfo userInfo;
        public Int32 ready;
    }
}

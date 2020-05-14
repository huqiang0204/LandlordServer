using Google.Protobuf;
using huqiang;
using huqiang.Data;
using LandlordServer.Table;
using SqlManager.Sql;
using System;
using TinyJson;

namespace LandlordServer.DataControll
{
    public class DefCmd
    {
        public const Int32 Login = 0;
    }
    class Login
    {
        public string type;
        public string key;
        public string pass;
        public string extand;
    }
    public class DefData
    {
        public static void Dispatch(KcpUser linker,DataBuffer data)
        {
            int cmd = data.fakeStruct[Req.Cmd];
            switch(cmd)
            {
                case DefCmd.Login:
                    Login(linker,data);
                    break;
            }
        }
        static void Login(KcpUser linker, DataBuffer buffer)
        {
            string args = buffer.fakeStruct.GetData<string>(Req.Args);
            if (args == null)
                return;
            var login =  JSONParser.FromJson<Login>(args);
            switch(login.type)
            {
                case "Tourists":
                    TouristsLogin(linker,login);
                    break;
                case "Account":
                    break;

            }
            //var user = UserTable.AddNewUser(uid);

            //DataBuffer db = new DataBuffer();
            //var fake = new FakeStruct(db, Req.Length + 1);
            //fake[Req.Cmd] = RpcCmd.Login;
            //fake[Req.Type] = MessageType.Rpc;
            //fake[Req.Args] = user.id;
            //fake[Req.Length] = user.RoomId;
            //db.fakeStruct = fake;
            //linker.Send(AES.Instance.Encrypt(db.ToBytes()), EnvelopeType.AesDataBuffer);

            //linker.userInfo = user;
            //if (user.RoomId > 0)
            //{
            //    var room = RoomManager.QueryRoom(user.RoomId);
            //    if (room != null)
            //    {
            //        room.Reconnect(linker);
            //    }
            //}
        }
        static void TouristsLogin(KcpUser user, Login login)
        {
            var query = SqlCmd.Query("userinfo", "deviceId", login.key, true);
            SqlClient.Instance.ExecuteReader(query, (o) => {
                if (o.Read())
                {
                    UserInfo info = new UserInfo();
                    SqlTable.ReadToObject(info, o);
                    o.Close();
                    info.LastLogin = DateTime.Now.Ticks;
                    user.userInfo = info;
                    user.Login();
                }
                else
                {
                    UserInfo info = new UserInfo();
                    info.coins = 1000;
                    info.deviceId = login.key;
                    info.LastLogin = DateTime.Now.Ticks;
                    var cmd =  SqlCmd.InsertNewRow(info,"userinfo");
                    user.userInfo = info;
                    user.Login();
                    o.Close();
                    SqlClient.Instance.ExecuteCmd(cmd);
                }
            });
        }
    }
}

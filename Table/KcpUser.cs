using huqiang;
using huqiang.Data;
using LandlordServer.DataControll;
using SqlManager.Sql;
using System;
using System.Collections.Generic;
using System.Text;
using TinyJson;

namespace LandlordServer.Table
{
    public class KcpUser:KcpLink
    {
        public UserInfo userInfo;
        public KcpUser() 
        {
            Console.WriteLine("new link");
        }
        public override void Dispatch(byte[] dat, byte tag)
        {
            ServerDataControll.Dispatch(this, dat,tag);
        }
        public override void Disconnect()
        {
            kcp.RemoveLink(this);
            var cmd = SqlCmd.UpdateRow(userInfo, userInfo.id, "userinfo");
            SqlClient.Instance.ExecuteCmd(cmd);
        }
        public void Send(byte[][] data)
        {
  
        }
        public void SendString(Int32 cmd, Int32 type, string obj)
        {
            DataBuffer db = new DataBuffer(4);
            var fs = db.fakeStruct = new FakeStruct(db, Req.Length);
            fs[Req.Cmd] = cmd;
            fs[Req.Type] = type;
            fs.SetData(Req.Args, obj);
            var dat = db.ToBytes();
            dat = AES.Instance.Encrypt(dat);
            Send(dat, EnvelopeType.AesDataBuffer);
        }
        public void Login()
        {
            SendString(DefCmd.Login,MessageType.Def,JSONWriter.ToJson(userInfo));
        }
    }
}

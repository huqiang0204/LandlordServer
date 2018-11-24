using huqiang;
using huqiang.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.DataControll
{
    public class DefCmd
    {
        public const Int32 Version = 0;
        public const Int32 Update = 1;
    }
    public class DefData
    {
        public static void Dispatch(Linker linker,DataBuffer data)
        {
            int cmd = data.fakeStruct[Req.Cmd];
            switch(cmd)
            {
                case DefCmd.Update:
                    GetUpdateFile(linker);
                    break;
            }
        }
        static void GetUpdateFile(Linker linker)
        {
            string Dlluri = OSSManager.GetDllUri();
            string UIuri= OSSManager.GetUIUri();
            DataBuffer db = new DataBuffer();
            var fake = new FakeStruct(db,Req.Length+1);
            fake[Req.Type] = MessageType.Def;
            fake[Req.Cmd] = DefCmd.Update;
            fake.SetData(Req.Args, Dlluri);
            fake.SetData(Req.Length,UIuri);
            db.fakeStruct = fake;
            linker.Send(AES.Instance.Encrypt(db.ToBytes()), EnvelopeType.AesDataBuffer);
        }
    }
}

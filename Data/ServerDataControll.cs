﻿using huqiang;
using huqiang.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.Data
{
    public class Req
    {
        public const Int32 Cmd = 0;
        public const Int32 Type = 1;
        public const Int32 Args = 2;
        public const Int32 Length = 3;
    }
    public class MessageType
    {
        public const Int32 Def = 0;
        public const Int32 Rpc = 1;
        public const Int32 Query = 2;
    }
    public class ServerDataControll
    {
        public static void Dispatch(Linker linker,byte[] dat,byte tag)
        {
            switch (tag)
            {
                case EnvelopeType.AesDataBuffer:
                    DispatchDataBuffer(linker,AES.Instance.Decrypt(dat));
                    break;
                case EnvelopeType.DataBuffer:
                    DispatchDataBuffer(linker,dat);
                    break;
            }
        }
        static void DispatchDataBuffer(Linker linker, byte[] dat)
        {
            var buffer = new DataBuffer(dat);
            var fake = buffer.fakeStruct;
            if (fake != null)
            {
                switch (fake[Req.Type])
                {
                    case MessageType.Def:
                        DefData.Dispatch(linker,buffer);
                        break;
                    case MessageType.Rpc:
                        break;
                    case MessageType.Query:
                        break;
                }
            }
        }
    }
}

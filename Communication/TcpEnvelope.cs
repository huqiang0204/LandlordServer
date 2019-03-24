using System;
using System.Collections.Generic;
using System.Text;

namespace huqiang
{
    public struct EnvelopeItem
    {
        public EnvelopeHead head;
        public Int32 part;
        public Int32 rcvLen;
        public byte[] buff;
        public long time;
    }
    public class TcpEnvelope
    {
        public static void CopyToBuff(byte[] buff, byte[] src, int start, EnvelopeHead head, int FragmentSize)
        {
            int index = (head.CurPart - 1) * FragmentSize;
            int len = (int)head.PartLen;
            for (int i = 0; i < len; i++)
            {
                buff[index] = src[start];
                index++;
                start++;
            }
        }
        public PackType type = PackType.All;
        EnvelopeItem[] pool = new EnvelopeItem[128];
        int remain = 0;
        byte[] buffer;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffLen">256kb</param>
        public TcpEnvelope(int buffLen = 262144)
        {
            buffer = new byte[buffLen];
        }
        public byte[][] Pack(byte[] dat, byte tag)
        {
            return EnvelopeEx.Pack(dat, tag, type);
        }
        public List<EnvelopeData> Unpack(byte[] dat, int len)
        {
            ClearTimeout();
            switch (type)
            {
                case PackType.Part:
                    return OrganizeSubVolume(EnvelopeEx.UnpackPart(dat, len, buffer, ref remain));
                case PackType.Total:
                    return EnvelopeEx.UnpackByte(dat, len, buffer, ref remain);
                case PackType.All:
                    var list = EnvelopeEx.UnpackByte(dat, len, buffer, ref remain);
                    return OrganizeSubVolume(EnvelopeEx.EnvlopeDataToPart(list), 1444 / 8 * 7);
            }
            return null;
        }
        List<EnvelopeData> OrganizeSubVolume(List<EnvelopePart> list, int fs = 1444)
        {
            if (list != null)
            {
                List<EnvelopeData> datas = new List<EnvelopeData>();
                for (int j = 0; j < list.Count; j++)
                {
                    var item = list[j];
                    if (item.head.AllPart > 1)
                    {
                        int s = -1;
                        for (int i = 0; i < 128; i++)
                        {
                            if (s < 0)
                            {
                                if (pool[i].head.MsgID == 0)
                                    s = i;
                            }
                            if (item.head.MsgID == pool[i].head.MsgID)
                            {
                                CopyToBuff(pool[i].buff, item.data, 0, item.head, fs);
                                pool[i].part++;
                                pool[i].rcvLen += item.head.PartLen;
                                if (pool[i].rcvLen >= item.head.Lenth)
                                {
                                    EnvelopeData data = new EnvelopeData();
                                    data.data = pool[i].buff;
                                    data.type = (byte)(pool[i].head.Type);
                                    pool[i].head.MsgID = 0;
                                    datas.Add(data);
                                }
                                goto label;
                            }
                        }
                        pool[s].head = item.head;
                        pool[s].part = 1;
                        pool[s].rcvLen = item.head.PartLen;
                        pool[s].buff = new byte[item.head.Lenth];
                        pool[s].time = DateTime.Now.Ticks;
                        CopyToBuff(pool[s].buff, item.data, 0, item.head, fs);
                    }
                    else
                    {
                        EnvelopeData data = new EnvelopeData();
                        data.data = item.data;
                        data.type = (byte)(item.head.Type);
                        datas.Add(data);
                    }
                label:;
                }
                return datas;
            }
            return null;
        }
        void ClearTimeout()
        {
            var now = DateTime.Now.Ticks;
            for (int i = 0; i < 128; i++)
            {
                if (pool[i].head.MsgID > 0)
                    if (now - pool[i].time > 20 * 1000000)//清除超时20秒的消息
                        pool[i].head.MsgID = 0;
            }
        }
        public void Clear()
        {
            remain = 0;
            for (int i = 0; i < 128; i++)
            {
                pool[i].head.MsgID= 0;
            }
        }
    }
}

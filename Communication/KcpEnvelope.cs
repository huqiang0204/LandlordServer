using System;
using System.Collections.Generic;
using System.Text;

namespace huqiang
{
    public class KcpEnvelope
    {
        class DataItem
        {
            public Int16 id;
            public long time;
            public byte[] dat;
        }
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
        EnvelopeItem[] recvPool = new EnvelopeItem[128];
        List<DataItem> sendBuffer = new List<DataItem>();
        public List<byte[]> ValidateData = new List<byte[]>();
        int remain = 0;
        byte[] buffer;
        Int16 id;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffLen">256kb</param>
        public KcpEnvelope(int buffLen = 262144)
        {
            buffer = new byte[buffLen];
        }
        public byte[][] Pack(byte[] dat, byte type)
        {
            var tmp = EnvelopeEx.PackInt(dat, type,id);
            long now = DateTime.Now.Ticks;
            for(int i=0;i<tmp.Length;i++)
            {
                DataItem item = new DataItem();
                item.id = id;
                id++;
                if (id> 10000)
                    id = 0;
                item.dat = tmp[i];
                item.time = now;
                sendBuffer.Add(item);
            }
            return tmp;
        }
        public List<EnvelopeData> Unpack(byte[] dat, int len)
        {
            ClearTimeout();
            var list = EnvelopeEx.UnpackInt(dat, len, buffer, ref remain);
            var dats = EnvelopeEx.EnvlopeDataToPart(list);
            int c = dats.Count - 1;
            for(;c>=0;c--)
            {
                var item = dats[c];
                Int16 tag = item.head.Type;
                byte type = (byte)(tag);
                if (type==EnvelopeType.Success)
                {
                    Success(item.head.PartID);
                    dats.RemoveAt(c);
                }
                else
                {
                    ReciveOk(item.head.PartID);
                }
            }
            return OrganizeSubVolume(dats, 1444 / 8 * 7);
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
                                if (recvPool[i].head.MsgID == 0)
                                    s = i;
                            }
                            if (item.head.MsgID == recvPool[i].head.MsgID)
                            {
                                CopyToBuff(recvPool[i].buff, item.data, 0, item.head, fs);
                                recvPool[i].part++;
                                recvPool[i].rcvLen += item.head.PartLen;
                                if (recvPool[i].rcvLen >= item.head.Lenth)
                                {
                                    EnvelopeData data = new EnvelopeData();
                                    data.data = recvPool[i].buff;
                                    data.type = (byte)recvPool[i].head.Type;
                                    recvPool[i].head.MsgID = 0;
                                    datas.Add(data);
                                }
                                goto label;
                            }
                        }
                        recvPool[s].head = item.head;
                        recvPool[s].part = 1;
                        recvPool[s].rcvLen = item.head.PartLen;
                        recvPool[s].buff = new byte[item.head.Lenth];
                        recvPool[s].time = DateTime.Now.Ticks;
                        CopyToBuff(recvPool[s].buff, item.data, 0, item.head, fs);
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
                if (recvPool[i].head.MsgID > 0)
                    if (now - recvPool[i].time > 20 * 1000000)//清除超时20秒的消息
                        recvPool[i].head.MsgID = 0;
            }
        }
        public void Clear()
        {
            remain = 0;
            for (int i = 0; i < 128; i++)
            {
                recvPool[i].head.MsgID = 0;
            }
            sendBuffer.Clear();
        }
        void Success(Int16 _id)
        {
            for(int i=0;i<sendBuffer.Count;i++)
                if(sendBuffer[i].id==_id)
                {
                    sendBuffer.RemoveAt(i);
                    break;
                }
        }
        /// <summary>
        /// 获取超时数据
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public byte[][] GetFailedData(long timeout = 10000)
        {
            long now = DateTime.Now.Ticks;
            List<byte[]> tmp = new List<byte[]>();
            for(int i=0;i<sendBuffer.Count;i++)
            {
                if(now-sendBuffer[i].time>timeout)
                {
                    sendBuffer[i].time -= timeout;
                    tmp.Add(sendBuffer[i].dat);
                }
            }
            return tmp.ToArray();
        }
        void ReciveOk(Int16 _id)
        {
            byte[] tmp = EnvelopeEx.PackInt(new byte[2], 128,_id)[0];
            ValidateData.Add(tmp);
        }
    }
}

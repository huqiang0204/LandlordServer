using System;
using System.Collections.Generic;

namespace huqiang
{
    public class EnvelopeType
    {
        public const byte Mate = 0;
        public const byte AesMate = 1;
        public const byte Json = 2;
        public const byte AesJson = 3;
        public const byte DataBuffer = 4;
        public const byte AesDataBuffer = 5;
        public const byte String = 6;
        public const byte AesString = 7;
    }
    public struct EnvelopeHead
    {
        /// <summary>
        /// 前三个自己为id，第四字节为tag <<=24
        /// </summary>
        public UInt32 Tag;
        public UInt32 Lenth;
        public UInt16 CurPart;
        public UInt16 AllPart;
        public UInt32 PartLen;
    }
    public struct EnvelopeItem
    {
        public EnvelopeHead head;
        public Int32 part;
        public UInt32 rcvLen;
        public byte[] buff;
    }
    public enum PackType
    {
        None,
        Part,//分卷，速度快，容错低,只有包头
        Total,//整体发送,分包头包尾，效率低，不适合大数据
        All//分卷发送,分包头包尾，效率低
    }
    public class EnvelopeEx
    {
        public static unsafe void SetInt16(byte[] buff, int index, Int16 value)
        {
            fixed (byte* b = &buff[index])
            {
                *(Int16*)b = value;
            }
        }
        public static unsafe void SetInt32(byte[] buff, int index, Int32 value)
        {
            fixed (byte* b = &buff[index])
            {
                *(Int32*)b = value;
            }
        }
        public static unsafe Int32 GetInt32(byte[] buff, int index)
        {
            fixed (byte* b = &buff[index])
            {
                return *(Int32*)b;
            }
        }
        public static unsafe EnvelopeHead ReadHead(byte[] buff, int index)
        {
            fixed (byte* b = &buff[index])
            {
                return *(EnvelopeHead*)b;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buff">源数据</param>
        /// <param name="len">数据长度</param>
        /// <param name="buffer">缓存数据</param>
        /// <param name="remain"></param>
        /// <param name="fs"></param>
        /// <returns></returns>
        public static List<EnvelopePart> UnpackPart(byte[] buff, int len, byte[] buffer, ref int remain, int fs = 1444)
        {
            List<EnvelopePart> list = new List<EnvelopePart>();
            int s = remain;
            for (int i = 0; i < len; i++)
            {
                buffer[s] = buff[i];
                s++;
            }
            len += remain;
            int index = 0;
            for (int i = 0; i < 1024; i++)
            {
                EnvelopeHead head = ReadHead(buffer, index);
                if (head.PartLen > fs)
                {
                    remain = 0;
                    break;
                }
                if (index + 16 + head.PartLen > len)
                {
                    remain = len - index;
                    for (int j = 0; j < len; j++)
                    {
                        buffer[j] = buffer[index];
                        index++;
                    }
                    return list;
                }
                index += 16;
                if(head.Lenth>2)
                {
                    EnvelopePart part = new EnvelopePart();
                    part.head = head;
                    int l = (int)head.PartLen;
                    var buf = new byte[l];
                    int a = index;
                    for (int j = 0; j < l; j++)
                    {
                        buf[j] = buffer[a]; a++;
                    }
                    part.data = buf;
                    list.Add(part);
                }
                index += (int)head.PartLen;
                if (index >= len)
                {
                    remain = 0;
                    break;
                }
            }
            return list;
        }
        /// <summary>
        /// 标头4字节，总长度4字节, 当前分卷2字节，总分卷2字节，当前分卷长度4字节，总计16字节
        /// </summary>
        public static int Tag = 256;//
        /// <summary>
        /// 每个数据包大小1460字节
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="type"></param>
        /// <param name="fs">每个分卷大小</param>
        /// <returns></returns>
        public static byte[][] SubVolume(byte[] buff, byte type, int fs = 1444)
        {
            if (buff.Length < 2)
                buff = new byte[2];
            Tag++;
            if (Tag >= 0xffffff)
                Tag = 256;
            int len = buff.Length;
            int part = len / fs;
            int r = len % fs;
            int allPart = part;
            if (r > 0)
                allPart++;
            byte[][] buf = new byte[allPart][];
            for (int i = 0; i < part; i++)
                buf[i] = EnvelopePart(buff, i, fs, type, len, allPart, fs);
            if (r > 0)
                buf[part] = EnvelopePart(buff, part, r, type, len, allPart, fs);
            return buf;
        }
        static byte[] EnvelopePart(byte[] buff, int part, int partLen, byte type, int len, int allPart, int fs)
        {
            byte[] tmp = new byte[partLen + 16];
            SetInt32(tmp, 0, Tag);
            tmp[3] = type;
            SetInt32(tmp, 4, len);
            SetInt16(tmp, 8, (Int16)(part + 1));
            SetInt16(tmp, 10, (Int16)allPart);
            SetInt32(tmp, 12, partLen);
            int index = part * fs;
            int start = 16;
            for (int j = 0; j < partLen; j++)
            {
                tmp[start] = buff[index];
                start++;
                index++;
            }
            return tmp;
        }

        /// <summary>
        /// 包头=255，包尾=254
        /// </summary>
        /// <param name="dat"></param>
        public static byte[] Packing(byte[] dat, byte tag)
        {
            int len = dat.Length;
            int part = len / 7;
            int r = len % 7;
            int tl = part * 8;
            if (r > 0)
            {
                part++;
                tl++;
                tl += r;
            }
            byte[] buf = new byte[tl + 3];
            buf[0] = 255;
            buf[1] = tag;
            buf[tl + 2] = 254;
            int s0 = 0;
            int s1 = 2;
            for (int i = 0; i < part; i++)
            {
                PackingPart(dat, s0, buf, s1);
                s0 += 7;
                s1 += 8;
            }
            return buf;
        }
        static void PackingPart(byte[] src, int si, byte[] tar, int ti)
        {
            int a = 0;
            int s = ti + 1;
            for (int i = 0; i < 7; i++)
            {
                byte b = src[si];
                if (b > 127)
                {
                    a |= 1 << i;
                    b -= 128;
                }
                tar[s] = b;
                si++;
                if (si >= src.Length)
                    break;
                s++;
            }
            tar[ti] = (byte)a;
        }
        static byte[] Unpacking(byte[] dat, int start, int end)
        {
            int len = end - start;
            int part = len / 8;
            int r = len % 8;
            int tl = part * 7;
            if (r > 0)
            {
                part++;
                tl += r;
                tl--;
            }
            byte[] buf = new byte[tl];
            int s0 = start;
            int s1 = 0;
            for (int i = 0; i < part; i++)
            {
                UnpackPart(dat, s0, buf, s1);
                s0 += 8;
                s1 += 7;
            }
            return buf;
        }
        static void UnpackPart(byte[] src, int si, byte[] tar, int ti)
        {
            int a = src[si];
            si++;
            for (int i = 0; i < 7; i++)
            {
                byte b = src[si];
                if ((a & 1) > 0)
                    b += 128;
                a >>= 1;
                tar[ti] = b;
                ti++;
                if (ti >= tar.Length)
                    break;
                si++;
            }
        }
        public static List<EnvelopeData> Unpack(byte[] dat, int len, byte[] buffer, ref int remain)
        {
            List<EnvelopeData> list = new List<EnvelopeData>();
            int s = remain;
            for (int i = 0; i < len; i++)
            {
                buffer[s] = dat[i];
                s++;
            }
            len += remain;
            s = 0;
            int e = 0;
            for (int i = 0; i < len; i++)
            {
                var b = buffer[i];
                if (b == 255)
                {
                    s = i + 2;
                }
                else if (b == 254)
                {
                    e = i;
                    EnvelopeData data = new EnvelopeData();
                    data.data = Unpacking(buffer, s, e);
                    data.tag = buffer[s - 1];
                    list.Add(data);
                }
            }
            if (e != 0)
            {
                remain = len - e - 1;
                for (int i = 0; i < remain; i++)
                {
                    buffer[i] = buffer[e];
                    e++;
                }
            }
            return list;
        }
        static byte[] ReadPart(byte[] data, out EnvelopeHead head)
        {
            head = ReadHead(data, 0);
            int len = data.Length - 16;
            if (len >= head.PartLen)
            {
                byte[] buf = new byte[len];
                int start = 16;
                for (int i = 0; i < len; i++)
                {
                    buf[i] = data[start];
                    start++;
                }
                return buf;
            }
            return null;
        }
        public static List<EnvelopePart> EnvlopeDataToPart(List<EnvelopeData> datas)
        {
            if (datas == null)
                return null;
            int c = datas.Count;
            List<EnvelopePart> parts = new List<EnvelopePart>();
            for (int i = 0; i < c; i++)
            {
                EnvelopePart part = new EnvelopePart();
                var buf = ReadPart(datas[i].data, out part.head);
                if (buf != null)
                {
                    part.data = buf;
                    parts.Add(part);
                }
            }
            return parts;
        }
        /// <summary>
        /// 当数据量较大，使用分卷
        /// </summary>
        /// <param name="dat"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static byte[][] PackBig(byte[] dat, byte tag)
        {
            var buf = SubVolume(dat, tag, 1444 / 8 * 7);
            if (buf != null)
                for (int i = 0; i < buf.Length; i++)
                    buf[i] = Packing(buf[i], tag);
            return buf;
        }
        public static byte[][] Pack(byte[] dat, byte tag,PackType type)
        {
            switch (type)
            {
                case PackType.Part:
                    return SubVolume(dat, tag);
                case PackType.Total:
                    return new byte[][] { Packing(dat, tag) };
                case PackType.All:
                    return PackBig(dat, tag);
            }
            return null;
        }
    }
    public class EnvelopeData
    {
        public byte[] data;
        public byte tag;
    }
    public class EnvelopePart
    {
        public EnvelopeHead head;
        public byte[] data;
    }
    public class EnvelopeBuffer
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
        public EnvelopeBuffer(int buffLen = 262144)
        {
            buffer = new byte[buffLen];
        }
        public byte[][] Pack(byte[] dat, byte tag)
        {
            return EnvelopeEx.Pack(dat,tag,type);
        }
        public List<EnvelopeData> Unpack(byte[] dat, int len)
        {
            switch (type)
            {
                case PackType.Part:
                    return OrganizeSubVolume(EnvelopeEx.UnpackPart(dat, len, buffer, ref remain));
                case PackType.Total:
                   return EnvelopeEx.Unpack(dat, len, buffer, ref remain);
                case PackType.All:
                    var list = EnvelopeEx.Unpack(dat, len, buffer, ref remain);
                    return OrganizeSubVolume(EnvelopeEx.EnvlopeDataToPart(list),  1444 / 8 * 7);
            }
            return null;
        }
         List<EnvelopeData> OrganizeSubVolume(List<EnvelopePart> list,  int fs = 1444)
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
                                if (pool[i].head.Tag == 0)
                                    s = i;
                            }
                            if (item.head.Tag == pool[i].head.Tag)
                            {
                                CopyToBuff(pool[i].buff, item.data, 0, item.head, fs);
                                pool[i].part++;
                                pool[i].rcvLen += item.head.PartLen;
                                if (pool[i].rcvLen >= item.head.Lenth)
                                {
                                    EnvelopeData data = new EnvelopeData();
                                    data.data = pool[i].buff;
                                    data.tag = (byte)(pool[i].head.Tag>>24);
                                    pool[i].head.Tag = 0;
                                    datas.Add(data);
                                }
                                goto label;
                            }
                        }
                        pool[s].head = item.head;
                        pool[s].part = 1;
                        pool[s].rcvLen = item.head.PartLen;
                        pool[s].buff = new byte[item.head.Lenth];
                        CopyToBuff(pool[s].buff, item.data, 0, item.head, fs);
                    }
                    else
                    {
                        EnvelopeData data = new EnvelopeData();
                        data.data = item.data;
                        data.tag = (byte)(item.head.Tag>>24);
                        datas.Add(data);
                    }
                label:;
                }
                return datas;
            }
            return null;
        }
        public void Clear()
        {
            remain = 0;
            for (int i = 0; i < 128; i++)
            {
                pool[i].head.Tag = 0;
            }
        }
    }
}

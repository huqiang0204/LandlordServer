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
        public const byte Success = 128;
        public const byte Failed = 129;
    }
    public struct EnvelopeHead
    {
        /// <summary>
        /// 数据压缩类型
        /// </summary>
        public Int16 Type;
        /// <summary>
        /// 此消息的id
        /// </summary>
        public Int16 MsgID;
        /// <summary>
        /// 此消息的分卷id
        /// </summary>
        public Int16 PartID;
        /// <summary>
        /// 此消息的某个分卷
        /// </summary>
        public Int16 CurPart;
        /// <summary>
        /// 此消息总计分卷
        /// </summary>
        public Int16 AllPart;
        /// <summary>
        /// 此消息分卷长度
        /// </summary>
        public Int16 PartLen;
        /// <summary>
        /// 此此消息总计长度
        /// </summary>
        public Int32 Lenth;
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
        public static unsafe EnvelopeHead ReadHead(byte[] buff, int index)
        {
            fixed (byte* b = &buff[index])
            {
                return *(EnvelopeHead*)b;
            }
        }
        public static unsafe void WriteHead(byte[] buff, int index, EnvelopeHead head)
        {
            fixed (byte* b = &buff[index])
            {
                *(EnvelopeHead*)b = *&head;
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
                if (head.Lenth > 2)
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
        /// <summary>
        /// 每个数据包大小1460字节
        /// </summary>
        /// <param name="buff">需要打包的数据</param>
        /// <param name="type">数据类型</param>
        /// <param name="id">数据包标志</param>
        /// <param name="fs">每个分卷大小</param>
        /// <returns></returns>
        public static byte[][] SubVolume(byte[] buff, byte type, Int16 id = 0, Int16 fs = 1444)
        {
            if (buff.Length < 2)
                buff = new byte[2];
            int len = buff.Length;
            int part = len / fs;
            int r = len % fs;
            Int16 allPart = (Int16)part;
            if (r > 0)
                allPart++;
            byte[][] buf = new byte[allPart][];
            Int16 msgId = id;
            EnvelopeHead head = new EnvelopeHead();
            for (int i = 0; i < part; i++)
            {
                head.MsgID = msgId;
                head.Type = type;
                head.PartID = id;
                head.CurPart = (Int16)i;
                head.AllPart = allPart;
                head.PartLen = fs;
                head.Lenth = len;
                byte[] tmp = new byte[fs + 16];
                WriteHead(tmp, 0, head);
                buf[i] = EnvelopePart(buff, tmp, i, fs, fs);
                id++;
            }
            if (r > 0)
            {
                head.MsgID = msgId;
                head.Type = type;
                head.PartID = id;
                head.CurPart = (Int16)part;
                head.AllPart = allPart;
                head.PartLen = (Int16)r;
                head.Lenth = len;
                byte[] tmp = new byte[r + 16];
                WriteHead(tmp, 0, head);
                buf[part] = EnvelopePart(buff, tmp, part, r, fs);
            }
            return buf;
        }
        static byte[] EnvelopePart(byte[] buff, byte[] tmp, int part, int partLen, int fs)
        {
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

        #region 以字节为单元进行封包解包,带宽损耗14%
        /// <summary>
        /// 包头=255，包尾=254
        /// </summary>
        /// <param name="dat"></param>
        /// <param name="type">数据类型</param>
        /// <returns></returns>
        public static byte[] PackingByte(byte[] dat, byte type)
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
            byte[] buf = new byte[tl + 3];//包头1字节,类型1字节，包尾1字节
            buf[0] = 255;
            buf[1] = type;
            buf[tl + 2] = 254;
            int s0 = 0;
            int s1 = 2;
            for (int i = 0; i < part; i++)
            {
                PackingByte(dat, s0, buf, s1);
                s0 += 7;
                s1 += 8;
            }
            return buf;
        }
        static void PackingByte(byte[] src, int si, byte[] tar, int ti)
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
        static void UnpackByte(byte[] src, int si, byte[] tar, int ti)
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
        static byte[] UnpackByte(byte[] dat, int start, int end)
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
                UnpackByte(dat, s0, buf, s1);
                s0 += 8;
                s1 += 7;
            }
            return buf;
        }
        public static List<EnvelopeData> UnpackByte(byte[] dat, int len, byte[] buffer, ref int remain)
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
                    data.data = UnpackByte(buffer, s, e);
                    data.type = buffer[s - 1];
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
        #endregion
        #region 以short为单元进行封包解包,带宽损耗6%
        /// <summary>
        /// 包头=255,255，包尾=255,254
        /// </summary>
        /// <param name="dat"></param>
        /// <param name="type">数据类型</param>
        /// <returns></returns>
        public static byte[] PackingShort(byte[] dat, byte type)
        {
            int len = dat.Length;
            int part = len / 30;
            int r = len % 30;
            int tl = part * 32;
            if (r > 0)
            {
                part++;
                tl += r + 2;
            }
            byte[] buf = new byte[tl + 5];//包头2字节,类型1字节，包尾2字节
            buf[0] = 255;
            buf[1] = 255;
            buf[2] = type;
            int s0 = 0;
            int s1 = 3;
            for (int i = 0; i < part; i++)
            {
                PackingShort(dat, s0, buf, s1);
                s0 += 30;
                s1 += 32;
            }
            buf[tl + 3] = 255;
            buf[tl + 4] = 254;
            return buf;
        }
        static void PackingShort(byte[] src, int si, byte[] tar, int ti)
        {
            int st = si + 1;
            int tt = ti + 3;
            for (int i = 0; i < 15; i++)
            {
                tar[tt] = src[st];
                st += 2;
                if (st >= src.Length)
                    break;
                tt += 2;
            }
            int a = 0;
            int s = ti + 2;
            for (int i = 0; i < 15; i++)
            {
                byte b = src[si];
                if (b > 127)
                {
                    a |= 1 << i;
                    b -= 128;
                }
                tar[s] = b;
                si += 2;
                if (si >= src.Length)
                    break;
                s += 2;
            }
            tar[ti] = (byte)(a >> 8);
            tar[ti + 1] = (byte)a;
        }
        static void UnpackShort(byte[] src, int si, byte[] tar, int ti)
        {
            int a = src[si];
            a <<= 8;
            si++;
            a += src[si];
            si++;
            int st = si + 1;
            int tt = ti + 1;
            for (int i = 0; i < 15; i++)
            {
                if (tt >= tar.Length)
                    break;
                tar[tt] = src[st];
                tt += 2;
                st += 2;
            }
            for (int i = 0; i < 15; i++)
            {
                if (ti + 1 >= tar.Length)
                    break;
                byte b = src[si];
                if ((a & 1) > 0)
                    b += 128;
                a >>= 1;
                tar[ti] = b;
                ti += 2;
                si += 2;
            }
        }
        static byte[] UnpackShort(byte[] dat, int start, int end)
        {
            int len = end - start;
            int part = len / 32;
            int r = len % 32;
            int tl = part * 30;
            if (r > 0)
            {
                part++;
                tl += r;
            }
            byte[] buf = new byte[tl];
            int s0 = start;
            int s1 = 0;
            for (int i = 0; i < part; i++)
            {
                UnpackShort(dat, s0, buf, s1);
                s0 += 32;
                s1 += 30;
            }
            return buf;
        }
        public static List<EnvelopeData> UnpackShort(byte[] dat, int len, byte[] buffer, ref int remain)
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
                    if (i + 1 < buffer.Length)
                    {
                        b = buffer[i + 1];
                        if (b == 255)
                        {
                            s = i + 3;
                        }
                        else if (b == 254)
                        {
                            e = i;
                            EnvelopeData data = new EnvelopeData();
                            data.data = UnpackShort(buffer, s, e);
                            data.type = buffer[s - 1];
                            list.Add(data);
                        }
                    }
                }
            }
            if (e != 0)
            {
                remain = len - e - 2;
                for (int i = 0; i < remain; i++)
                {
                    buffer[i] = buffer[e];
                    e++;
                }
            }
            return list;
        }
        #endregion
        #region 以Int为单元进行封包解包,带宽损耗3%
        /// <summary>
        /// 包头=255,255,255,255,包尾=255,255,255,254
        /// </summary>
        /// <param name="dat"></param>
        /// <param name="type">数据类型</param>
        /// <returns></returns>
        public static byte[] PackingInt(byte[] dat, byte type)
        {
            int len = dat.Length;
            int part = len / 124;
            int r = len % 124;
            int tl = part * 128;
            if (r > 0)
            {
                part++;
                tl += r + 4;
            }
            byte[] buf = new byte[tl + 9];//包头4字节,类型1字节，包尾4字节
            buf[0] = 255;
            buf[1] = 255;
            buf[2] = 255;
            buf[3] = 255;
            buf[4] = type;
            int s0 = 0;
            int s1 = 5;
            for (int i = 0; i < part; i++)
            {
                PackingInt(dat, s0, buf, s1);
                s0 += 124;
                s1 += 128;
            }
            buf[tl + 5] = 255;
            buf[tl + 6] = 255;
            buf[tl + 7] = 255;
            buf[tl + 8] = 254;
            return buf;
        }
        static void PackingInt(byte[] src, int si, byte[] tar, int ti)
        {
            int st = si + 1;
            int tt = ti + 5;
            for (int i = 0; i < 124; i++)
            {
                tar[tt] = src[st];
                st++;
                if (st >= src.Length)
                    break;
                tt++;
            }
            int a = 0;
            int s = ti + 4;
            for (int i = 0; i < 31; i++)
            {
                byte b = src[si];
                if (b > 127)
                {
                    a |= 1 << i;
                    b -= 128;
                }
                tar[s] = b;
                si += 4;
                if (si >= src.Length)
                    break;
                s += 4;
            }
            tar[ti] = (byte)a;
            a >>= 8;
            tar[ti + 1] = (byte)a;
            a >>= 8;
            tar[ti + 2] = (byte)a;
            a >>= 8;
            tar[ti + 3] = (byte)a;

        }
        static void UnpackInt(byte[] src, int si, byte[] tar, int ti)
        {
            int a = src.ReadInt32(si);
            int st = si + 4;
            int tt = ti;
            for (int i = 0; i < 124; i++)
            {
                if (tt >= tar.Length)
                    break;
                tar[tt] = src[st];
                st++;
                tt++;
            }
            si += 4;
            for (int i = 0; i < 31; i++)
            {
                if (ti + 3 >= tar.Length)
                    break;
                byte b = src[si];
                if ((a & 1) > 0)
                    b += 128;
                a >>= 1;
                tar[ti] = b;
                ti += 4;
                si += 4;
            }
        }
        static byte[] UnpackInt(byte[] dat, int start, int end)
        {
            int len = end - start;
            int part = len / 128;
            int r = len % 128;
            int tl = part * 124;
            if (r > 0)
            {
                part++;
                tl += r;
            }
            byte[] buf = new byte[tl];
            int s0 = start;
            int s1 = 0;
            for (int i = 0; i < part; i++)
            {
                UnpackInt(dat, s0, buf, s1);
                s0 += 128;
                s1 += 124;
            }
            return buf;
        }
        public static List<EnvelopeData> UnpackInt(byte[] dat, int len, byte[] buffer, ref int remain)
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
                    if (i + 3 < buffer.Length)
                    {
                        if (buffer[i + 1] == 255)
                            if (buffer[i + 2] == 255)
                            {
                                b = buffer[i + 3];
                                if (b == 255)
                                {
                                    s = i + 5;
                                }
                                else if (b == 254)
                                {
                                    e = i;
                                    EnvelopeData data = new EnvelopeData();
                                    data.data = UnpackInt(buffer, s, e);
                                    data.type = buffer[s - 1];
                                    list.Add(data);
                                }
                            }
                    }
                }
            }
            if (e != 0)
            {
                remain = len - e - 4;
                for (int i = 0; i < remain; i++)
                {
                    buffer[i] = buffer[e];
                    e++;
                }
            }
            return list;
        }
        #endregion
        static byte[] ReadPart(byte[] data, out EnvelopeHead head)
        {
            head = ReadHead(data, 0);
            int len = data.Length - 16;
            if (len >= head.PartLen)
            {
                byte[] buf = new byte[head.PartLen];
                int start = 16;
                for (int i = 0; i < head.PartLen; i++)
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
        /// <param name="type"></param>
        /// <returns></returns>
        public static byte[][] PackByte(byte[] dat, byte type, Int16 id = 0)
        {
            var buf = SubVolume(dat, type, id, 1444 * 7 / 8);
            if (buf != null)
                for (int i = 0; i < buf.Length; i++)
                    buf[i] = PackingByte(buf[i], type);
            return buf;
        }
        public static byte[][] PackShort(byte[] dat, byte type, Int16 id = 0)
        {
            var buf = SubVolume(dat, type, id, 1444 * 30 / 32);
            if (buf != null)
                for (int i = 0; i < buf.Length; i++)
                    buf[i] = PackingShort(buf[i], type);
            return buf;
        }
        public static byte[][] PackInt(byte[] dat, byte type, Int16 id = 0)
        {
            var buf = SubVolume(dat, type, id, 1444 * 124 / 128);
            if (buf != null)
                for (int i = 0; i < buf.Length; i++)
                    buf[i] = PackingInt(buf[i], type);
            return buf;
        }
        public static byte[][] Pack(byte[] dat, byte tag, PackType type)
        {
            switch (type)
            {
                case PackType.Part:
                    return SubVolume(dat, tag);
                case PackType.Total:
                    return new byte[][] { PackingByte(dat, tag) };
                case PackType.All:
                    return PackByte(dat, tag);
            }
            return null;
        }
    }
    public class EnvelopeData
    {
        public byte[] data;
        public byte type;
    }
    public class EnvelopePart
    {
        public EnvelopeHead head;
        public byte[] data;
    }

}

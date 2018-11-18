using System;
using System.IO;
using System.Linq;
using System.Text;


namespace huqiang.Data
{
    public class FakeStringArray : ToBytes
    {
        static byte[] zreo = new byte[4];
        string[] buf;
        public FakeStringArray(int size)
        {
            buf = new string[size];
        }
        public string this[int index]
        {
            set
            {
                buf[index] = value;
            }
            get
            {
                return buf[index];
            }
        }
        public unsafe FakeStringArray(Int32* point)
        {
            int len = *point;
            point++;
            buf = new string[len];
            for (int i = 0; i < len; i++)
            {
                int c = *point;
                point++;
                if (c > 0)
                {
                    byte* bp = (byte*)point;
                    buf[i] = Encoding.UTF8.GetString(bp, c);
                    bp += c;
                    point = (Int32*)bp;
                }
            }
        }
        public byte[] ToBytes()
        {
            if (buf == null)
                return zreo;
            int len = buf.Length;
            MemoryStream ms = new MemoryStream();
            var tmp = len.ToBytes();
            ms.Write(tmp, 0, 4);
            for (int i = 0; i < len; i++)
            {
                var str = buf[i];
                if (str == null)
                    ms.Write(zreo, 0, 4);
                else
                {
                    tmp = Encoding.UTF8.GetBytes(str);
                    int c = tmp.Length;
                    ms.Write(c.ToBytes(), 0, 4);
                    ms.Write(tmp, 0, c);
                }
            }
            tmp = ms.ToArray();
            ms.Dispose();
            return tmp;
        }
    }
}

using System;
using System.IO;
using System.Text;

namespace huqiang
{
    public static class Extand
    {
        static byte[] Zreo = new byte[4];
        static string hex = "0123456789abcdef";
        public unsafe static byte[] ToBytes(this Int16 value)
        {
            byte[] buff = new byte[2];
            fixed (byte* bp = &buff[0])
                *(Int16*)bp = value;
            return buff;
        }
        public unsafe static byte[] ToBytes(this Int32 value)
        {
            byte[] buff = new byte[4];
            fixed (byte* bp = &buff[0])
                *(Int32*)bp = value;
            return buff;
        }
        public unsafe static byte[] ToBytes(this Single value)
        {
            byte[] buff = new byte[4];
            fixed (byte* bp = &buff[0])
                *(Single*)bp = value;
            return buff;
        }
        public unsafe static byte[] ToBytes(this Int64 value)
        {
            byte[] buff = new byte[8];
            fixed (byte* bp = &buff[0])
                *(Int64*)bp = value;
            return buff;
        }
        public unsafe static byte[] ToBytes(this Double value)
        {
            byte[] buff = new byte[8];
            fixed (byte* bp = &buff[0])
                *(Double*)bp = value;
            return buff;
        }
        public static T Clone<T>(this T obj) where T : class, new()
        {
            if (obj != null)
            {
                try
                {
                    Type type = obj.GetType();
                    var tmp = Activator.CreateInstance(type);
                    var fields = type.GetFields();
                    if (fields != null)
                    {
                        for (int i = 0; i < fields.Length; i++)
                        {
                            var f = fields[i];
                            var ft = f.FieldType;
                            if (ft.IsClass)
                            {
                                if (ft == typeof(string))
                                {
                                    f.SetValue(tmp, f.GetValue(obj));
                                }
                                else
                                {
                                    f.SetValue(tmp, f.GetValue(obj).Clone());
                                }
                            }
                            else
                            {
                                f.SetValue(tmp, f.GetValue(obj));
                            }
                        }
                    }
                    return tmp as T;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
            return null;
        }
        public static void WriteString(this Stream stream, string str)
        {
            if (str == null)
            {
                stream.Write(Zreo, 0, 4);
            }
            else if (str.Length == 0)
            {
                stream.Write(Zreo, 0, 4);
            }
            else
            {
                var buf = Encoding.UTF8.GetBytes(str);
                stream.Write(buf.Length.ToBytes(), 0, 4);
                stream.Write(buf, 0, buf.Length);
            }
        }
        public unsafe static void Write(this Stream stream, byte* p, int size)
        {
            for (int i = 0; i < size; i++)
            { stream.WriteByte(*p); p++; }
        }
        public unsafe static Int32 ReadInt32(this byte[] buff, Int32 offset)
        {
            fixed (byte* bp = &buff[0])
                return *(Int32*)(bp + offset);
        }
        public unsafe static void Read(this byte[] buff, void* p, int offset, int size)
        {
            byte* bp = (byte*)p;
            for (int i = 0; i < size; i++)
            {
                *bp = buff[offset];
                bp++;
                offset++;
            }
        }
    }
}

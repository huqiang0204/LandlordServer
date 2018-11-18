using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace huqiang.Data
{
    public interface ToBytes
    {
        byte[] ToBytes();
    }
    public interface FakeArray
    {
         Int32 Length { get; }
    }
    public enum DataType
    {
        String=0,
        FakeStruct=1,
        FakeStructArray=2,
        ByteArray=3,
        Int32Array=4,
        FloatArray=5,
        Int64Array = 6,
        DoubleArray =7,
        FakeStringArray=8
    }
    //C#的这些类型不能被继承：System.ValueType, System.Enum, System.Delegate, System.Array, etc.
    public class DataBuffer
    {
        static DataType GetType(object obj)
        {
            if (obj is string)
                return DataType.String;
            else if (obj is FakeStruct)
                return DataType.FakeStruct;
            else if (obj is FakeStructArray)
                return DataType.FakeStructArray;
            else if (obj is byte[])
                return DataType.ByteArray;
            else if (obj is Int32[])
                return DataType.Int32Array;
            else if (obj is Single[])
                return DataType.FloatArray;
            else if (obj is Int64[])
                return DataType.Int64Array;
            else if (obj is Double[])
                return DataType.DoubleArray;
            else if (obj is FakeStringArray)
                return DataType.FakeStringArray;
            return (DataType)(-1);
        }
        public FakeStruct fakeStruct;
        static byte[] Zreo = new byte[4];
        struct ReferenceCount
        {
            /// <summary>
            /// Reference count
            /// </summary>
            public Int16 rc;
            /// <summary>
            ///byte[], String,FakeStruct,FakeStructArray,int[],float[],double[]
            /// </summary>
            public Int16 type;
            public Int32 size;
            public object obj;
        }
        /// <summary>
        /// 添加一个引用类型的数据
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="size">FakeStructArray 成员大小</param>
        /// <returns></returns>
        internal int AddData(object obj)
        {
            return AddData(obj, GetType(obj));
        }
        internal int AddData(object obj, DataType type)
        {
            if (obj is string[])
                return 0;
            int min = max;
            for (int a = max - 1; a >= 0; a--)
            {
                if (obj == buff[a].obj)
                {
                    buff[a].rc++;
                    return a;
                }
                else if (buff[a].rc == 0)
                {
                    min = a;
                }
            }
            buff[min].rc = 1;
            buff[min].type = (Int16)type;
            buff[min].obj = obj;
            if (type == DataType.FakeStructArray)
                buff[min].size = (obj as FakeStructArray).m_size;
            if (min == max)
            {
                max++;
                if (max >= Count)
                {
                    Count *= 2;
                    ReferenceCount[] tmp = new ReferenceCount[Count];
                    Array.Copy(buff, tmp, min+1);
                    buff = tmp;
                }
            }
            return min;
        }
        public object GetData(int index)
        {
            return buff[index].obj;
        }
        public void RemoveData(int index)
        {
            if (index == 0)
                return;
            buff[index].rc--;
        }
        ReferenceCount[] buff;
        int max = 1;
        int Count = 256;
        /// <summary>
        /// 创建一个空白的缓存
        /// </summary>
        /// <param name="buffCount"></param>
        public DataBuffer(int buffCount = 256)
        {
            buff = new ReferenceCount[buffCount];
            buff[0].rc = 256;
            Count = buffCount;
        }
        byte[] temp;
        unsafe byte* tempStart;
        /// <summary>
        /// 从已有的数据进行恢复
        /// </summary>
        /// <param name="dat"></param>
        public unsafe DataBuffer(byte[] dat)
        {
            temp = dat;
            var src = Marshal.UnsafeAddrOfPinnedArrayElement(dat, 0);
            tempStart = (byte*)src;
            Int32* ip = (Int32*)src;
            ip = ReadHead(ip);
            Int32 len = *ip;
            ip++;
            Int32* rs = ip + len * 3;
            int a = len;
            for (int i = 0; i < 32; i++)
            {
                a >>= 1;
                if (a == 0)
                {
                    a = 2 << i;
                    break;
                }
            }
            max = len;
            Count = a;
            buff = new ReferenceCount[a];
            byte* bp = (byte*)rs;
            for (int i = 0; i < len; i++)
            {
                GetTable(ip, i, bp);
                ip += 3;
            }
            temp = null;
        }
        unsafe Int32* ReadHead(Int32* p)
        {
            int len = *p;
            if (len > 0)
            {
                len /= 4;
                p++;
                fakeStruct = new FakeStruct(this, len, p);
                p += len;
            }
            else
            {
                p++;
            }
            return p;
        }
        unsafe void GetTable(Int32* p, int index, byte* rs)
        {
            Int16* sp = (Int16*)p;
            buff[index].rc = *sp;
            sp++;
            buff[index].type = *sp;
            p++;
            buff[index].size = *p;
            p++;
            Int32 offset = *p;
            rs += offset;
            buff[index].obj = GetObject(rs, (DataType)buff[index].type, buff[index].size);
        }
        unsafe object GetObject(byte* bp, DataType type, int size)
        {
            Int32* p = (Int32*)bp;
            int len = *p;
            if (len == 0)
                return null;
            p++;
            if (type == DataType.String)
            {
                int offset =(int) ((byte*)p - tempStart);
                return Encoding.UTF8.GetString(temp,offset, len);
            }
            else if (type == DataType.FakeStruct)
            {
                len /= 4;
                return new FakeStruct(this, len, p);
            }
            else if (type == DataType.FakeStructArray)
            {
                len /= size;
                len /= 4;
                return new FakeStructArray(this, size, len, p);
            }
            else if (type == DataType.ByteArray)
            {
                byte[] buf = new byte[len];
                bp += 4;
                for (int i = 0; i < len; i++)
                { buf[i] = *bp; bp++; }
                return buf;
            }
            else if (type == DataType.Int32Array)
            {
                len /= 4;
                Int32[] buf = new Int32[len];
                for (int i = 0; i < len; i++)
                { buf[i] = *p; p++; }
                return buf;
            }
            else if (type == DataType.FloatArray)
            {
                len /= 4;
                Single[] buf = new Single[len];
                for (int i = 0; i < len; i++)
                { buf[i] = *p; p++; }
                return buf;
            }
            else if (type == DataType.Int64Array)
            {
                len /= 8;
                Int64[] buf = new long[len];
                for (int i = 0; i < len; i++)
                { buf[i] = *p; p++; }
                return buf;
            }
            else if (type == DataType.DoubleArray)
            {
                len /= 8;
                Double[] buf = new Double[len];
                for (int i = 0; i < len; i++)
                { buf[i] = *p; p++; }
                return buf;
            }else if (type == DataType.FakeStringArray)
            {
                return new FakeStringArray(p);
            }
            return null;
        }
        public byte[] GetBytes(int index)
        {
            var array = buff[index].obj as Array;
            if (array != null)
            {
                int len = array.Length;
                byte[] buf = new byte[len * buff[index].size];
                var src = Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
                Marshal.Copy(src, buf, 0, buf.Length);
                return buf;
            }
            else
            {
                var str = buff[index].obj as string;
                if (str == null)
                {
                    var to = buff[index].obj as ToBytes;
                    if (to != null)
                        return to.ToBytes();
                    return null;
                }
                return Encoding.UTF8.GetBytes(str);
            }
        }
        public byte[] ToBytes()
        {
            MemoryStream table = new MemoryStream();
            if (fakeStruct == null)
            {
                table.Write(Zreo, 0, 4);
            }
            else
            {
                byte[] buf = fakeStruct.ToBytes();
                Int32 len = buf.Length;
                table.Write(len.ToBytes(), 0, 4);
                table.Write(buf, 0, len);
            }
            table.Write(max.ToBytes(), 0, 4);
            MemoryStream ms = new MemoryStream();
            Int32 offset = 0;
            for (int i = 0; i < max; i++)
            {
                var buf = GetBytes(i);
                table.Write(buff[i].rc.ToBytes(), 0, 2);//引用计数
                table.Write(buff[i].type.ToBytes(), 0, 2);//数据类型
                table.Write(buff[i].size.ToBytes(), 0, 4);//数据结构体长度
                table.Write(offset.ToBytes(), 0, 4);//数据偏移地址
                if (buf == null)
                {
                    ms.Write(Zreo, 0, 4);
                    offset += 4;
                }
                else
                {
                    Int32 len = buf.Length;
                    ms.Write(len.ToBytes(), 0, 4);
                    ms.Write(buf, 0, len);
                    offset += len + 4;
                }
            }
            byte[] tmp = ms.ToArray();
            table.Write(tmp, 0, tmp.Length);
            tmp = table.ToArray();
            ms.Dispose();
            table.Dispose();
            return tmp;
        }
    }

}
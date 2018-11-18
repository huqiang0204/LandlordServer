using System;
using System.Runtime.InteropServices;


namespace huqiang.Data
{
    public class FakeStruct : ToBytes
    {
        public unsafe void ReadFromStruct(void* tar)
        {
            Int32* t = (Int32*)tar;
            Int32* p = (Int32*)ptr;
            for (int i = 0; i < element; i++)
            {
                *p = *t;
                t++;
                p++;
            }
        }
        public unsafe void WitreToStruct(void* tar)
        {
            Int32* t = (Int32*)tar;
            Int32* p = (Int32*)ptr;
            for (int i = 0; i < element; i++)
            {
                *t = *p;
                t++;
                p++;
            }
        }
        IntPtr ptr;
        unsafe byte* ip;
        int msize;
        int element;
        DataBuffer buffer;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="size">元素个数</param>
        public FakeStruct(DataBuffer db, int size)
        {
            element = size;
            msize = size * 4;
            ptr = Marshal.AllocHGlobal(msize);
            unsafe
            {
                ip = (byte*)ptr;
                Int32* p = (Int32*)ptr;
                for (int i = 0; i < size; i++)
                {
                    *p = 0;
                    p++;
                }
            }
            buffer = db;
        }
        public unsafe FakeStruct(DataBuffer db, int size, Int32* point)
        {
            element = size;
            msize = size * 4;
            ptr = Marshal.AllocHGlobal(msize);
            unsafe
            {
                ip = (byte*)ptr;
                Int32* p = (Int32*)ptr;
                for (int i = 0; i < size; i++)
                {
                    *p = *point;
                    point++;
                    p++;
                }
            }
            buffer = db;
        }
        ~FakeStruct()
        {
            Marshal.FreeHGlobal(ptr);
        }
        public unsafe Int32 this[int index]
        {
            set
            {
                int o = index * 4;
                *(Int32*)(ip + o) = value;
            }
            get
            {
                int o = index * 4;
                return *(Int32*)(ip + o);
            }
        }
        public unsafe void SetData(int index, object dat)
        {
            int o = index * 4;
            Int32* a = (Int32*)(ip + o);
            buffer.RemoveData(*a);
            *a = buffer.AddData(dat);
        }
        public T GetData<T>(int index) where T : class
        {
            return GetData(index) as T;
        }
        public unsafe object GetData(int index)
        {
            int o = index * 4;
            Int32* a = (Int32*)(ip + o);
            return buffer.GetData(*a);
        }
        public unsafe void SetInt64(int index, Int64 value)
        {
            int o = index * 4;
            *(Int64*)(ip + o) = value;
        }
        public unsafe Int64 GetInt64(int index)
        {
            int o = index * 4;
            return *(Int64*)(ip + o);
        }
        public unsafe void SetFloat(int index, float value)
        {
            int o = index * 4;
            *(float*)(ip + o) = value;
        }
        public unsafe float GetFloat(int index)
        {
            int o = index * 4;
            return *(float*)(ip + o);
        }
        public unsafe void SetDouble(int index, double value)
        {
            int o = index * 4;
            *(double*)(ip + o) = value;
        }
        public unsafe double GetDouble(int index)
        {
            int o = index * 4;
            return *(double*)(ip + o);
        }
        public byte[] ToBytes()
        {
            byte[] tmp = new byte[msize];
            Marshal.Copy(ptr, tmp, 0, msize);
            return tmp;
        }
    }
}

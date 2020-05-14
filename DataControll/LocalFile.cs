using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SqlManager.Sql;
using huqiang.Data;

namespace Data
{
    public class LocalFile
    {
        /// <summary>
        /// 本地产品的所有数据库表属性
        /// </summary>
        public static Dictionary<string, MSDBAttribute[]> dic;
        /// <summary>
        /// 数据校验
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ms"></param>
        public static MSDBAttribute[] CheckData(string type,MSDBAttribute[] ms)
        {
            if (type == null)
                return null;
            if (ms == null)
                return null;
            if(dic.ContainsKey(type))
            {
                var buf = dic[type];
                List<MSDBAttribute> list = new List<MSDBAttribute>();
                for(int i=0;i<ms.Length;i++)
                {
                    var t = ms[i];
                    if(t!=null)
                    {
                        for(int j=0;j<buf.Length;j++)
                        {
                            if(buf[j].FieldName==t.FieldName)
                            {
                                t.DbType = buf[j].DbType;
                                for (int s = 0; s < list.Count; s++)
                                    if (list[s].FieldName == t.FieldName)
                                        goto label;
                                t.CheckValue();
                                list.Add(t);
                                break;
                            }
                        }
                    }
                    label:;
                }
                return list.ToArray();
            }
            return null;
        }
        /// <summary>
        /// 数据校验
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static MSDBAttribute[] CheckData(MSDBAttribute[] ms)
        {
            if (ms == null)
                return null;
            var keys = dic.Keys;
            foreach (var k in keys)
            {
                var buf = dic[k];
                List<MSDBAttribute> list = new List<MSDBAttribute>();
                for (int i = 0; i < ms.Length; i++)
                {
                    var t = ms[i];
                    if (t != null)
                    {
                        for (int j = 0; j < buf.Length; j++)
                        {
                            if (buf[j].FieldName == t.FieldName)
                            {
                                t.DbType = buf[j].DbType;
                                for (int s = 0; s < list.Count; s++)
                                    if (list[s].FieldName == t.FieldName)
                                        goto label;
                                t.CheckValue();
                                list.Add(t);
                                break;
                            }
                        }
                    }
                    label:;
                }
                return list.ToArray();
            }
            return null;
        }
        public static MyIP myIP;
        /// <summary>
        /// 载入本地的ip配置
        /// </summary>
        public static void loadConfig()
        {
            string path = Environment.CurrentDirectory + "\\setting.ini";
            if(File.Exists(path))
            {
                INIReader reader = new INIReader();
                reader.LoadFromFile(path);
                myIP =  reader.Serializal<MyIP>("sql");
            }
            else
            {
                myIP = new MyIP();
            }
        }
    }
    [Serializable]
    public class MyIP
    {
        public string ip;
        public string user;
        public string pass;
    }
}

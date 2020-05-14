using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlManager.Sql
{
    public class SqlTable
    {
        public static void ReadToObject(object tar, MySqlDataReader read)
        {
            Type obj = tar.GetType();
            var fields = obj.GetFields();
            string[] names = new string[fields.Length];
            for (int i = 0; i < names.Length; i++)
                names[i] = fields[i].Name.ToLower();
            int len = read.FieldCount;
            for (int i = 0; i < len; i++)
            {
                try
                {
                    string name = read.GetName(i);
                    for (int j = 0; j < names.Length; j++)
                        if (names[j] == name)
                        {
                            fields[j].SetValue(tar, read.TryParse(i));
                            break;
                        }
                           
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.StackTrace);
                }
            }
        }

    }
}

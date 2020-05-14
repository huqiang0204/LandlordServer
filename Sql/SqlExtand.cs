using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlManager.Sql
{
    public  static class SqlExtand
    {
        public static object TryParse(this MySqlDataReader read, int index)
        {
            try
            {
                var str = read.GetDataTypeName(index);
                switch (str)
                {
                    case "TINYINT":
                        return read.GetByte(index);
                    case "SMALLINT":
                        return read.GetInt16(index);
                    case "INT":
                    case "MEDIUMINT":
                        return read.GetInt32(index);
                    case "BIGINT":
                        return read.GetInt64(index);
                    case "FLOAT":
                        return read.GetFloat(index);
                    case "DOUBLE":
                        return read.GetDouble(index);
                    case "DECIMAL":
                        return read.GetDecimal(index);
                    case "TIMESTAMP":
                        return read.GetTimeSpan(index);
                    case "DATETIME":
                        return read.GetMySqlDateTime(index);
                    default:
                        return read.GetValue(index) as string;
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}

using LandlordServer.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlManager.Sql
{
    public class SqlControll
    {
        const string database = "landlord";
        public static void Initial()
        {
            var ts = SqlClient.Instance.ShowDatabase();
            if (!ts.Contains(database))
                SqlClient.Instance.CreateDatabase(database);
            SqlClient.Instance.ChangeDatabase(database);
            CheckTables(SqlClient.Instance.ShowTables());
        }
        static void CheckTables(List<string> tables)
        {
            if (!tables.Contains("userinfo"))
                SqlClient.Instance.CreateTable(typeof(UserInfo));
        }
    }
}
using SqlManager.Data;
using SqlManager.MySqlTable;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlManager.Sql
{
    public class SqlControll
    {
        public static void Initial()
        {
            var ts = SqlClient.Instance.ShowDatabase();
            if (!ts.Contains("editor"))
                SqlClient.Instance.CreateDatabase("editor");
            SqlClient.Instance.ChangeDatabase("editor");
            CheckTables(SqlClient.Instance.ShowTables());
        }
        static void CheckTables(List<string> tables)
        {
            if (!tables.Contains("works"))
                SqlClient.Instance.CreateTable(typeof(Works));
            if (!tables.Contains("updatelog"))
                SqlClient.Instance.CreateTable(typeof(UpdateLog));
            AddModelTable("sys_user", SqlCmd.TypeToAttrs(typeof(sys_user)));
        }
        /// <summary>
        /// 添加一条模型数据，并为此模型创建表
        /// </summary>
        /// <param name="product"></param>
        public static void AddModelConfig(string name, string json)
        {
            var query = SqlCmd.Query("modelconfig", "name",name,true);
            SqlClient.Instance.ExecuteReader(query, (o) =>
            {
                if (o.Read())
                    return;
                o.Close();
                ModelConfig mc = new ModelConfig();
                mc.name = name;
                mc.data = json;
                var cmd = SqlCmd.InsertNewRow(mc);
                SqlClient.Instance.ExecuteCmd(cmd);
            });
        }
        public static void AddModelTable(string tablename, MSDBAttribute[] ms)
        {
            if (tablename == null | tablename == "")
                return;
            var query = SqlCmd.QueryColumns(tablename, "editor");
            SqlClient.Instance.ExecuteReader(query, (o) =>
            {
                int a = 0;
                List<string> fs = new List<string>();
                while (o.Read())
                {
                    fs.Add(o.GetValue(0) as string);
                    a++;
                }
                o.Close();
                if (a > 0)
                {
                    List<MSDBAttribute> tmp = new List<MSDBAttribute>();
                    for(int i=0;i<ms.Length;i++)
                    {
                        var name = ms[i].FieldName;
                        if(!fs.Contains(name))
                        {
                            tmp.Add(ms[i]);
                        }
                    }
                    if(tmp.Count>0)
                    {
                        string str =  SqlCmd.AddNewFeilds(tmp.ToArray(), tablename);
                        SqlClient.Instance.ExecuteCmd(str);
                    }
                    return;
                }
                SqlClient.Instance.CreateTable(ms, tablename);
            });
        }
        /// <summary>
        /// 查询模型
        /// </summary>
        /// <param name="name"></param>
        public static void QueryModelConfig(string name,Action<ProductProperties> callback)
        {
            if (name == null | name == "")
                return;
            var query = SqlCmd.Query("modelconfig","name",name,true);
            SqlClient.Instance.ExecuteReader(query, (o) => {
                if(o.Read())
                {
                    ModelConfig config = new ModelConfig();
                    SqlTable.ReadToObject(config,o);
                    o.Close();
                    ProductProperties product = new ProductProperties();
                    if (callback != null)
                        callback(product);
                }
            });
        }
        /// <summary>
        /// 插入一条模型数据
        /// </summary>
        /// <param name="product"></param>
        public static void InsertModel(ProductProperties product)
        {
            var type = product.type;
            if(type==null| type=="")
                return;
            var code = product.code;
            if (code == null | code == "")
                return;
            var query = SqlCmd.Query(type, "code", product.code, true);
            SqlClient.Instance.ExecuteReader(query, (o) => {
                if(o.Read())
                {
                    Int64 id = (Int64)o.TryParse(0);
                    o.Close();
                    var pps = product.properties;
                    int len = pps.Length;
                    MSDBAttribute[] ms = new MSDBAttribute[len];
                    for (int i = 0; i < len; i++)
                    {
                        ms[i] = new MSDBAttribute();
                        var name = pps[i].namee;
                        ms[i].FieldName = name;
                        ms[i].DbType = MySqlDataType.VARCHAR;
                        ms[i].len = 32;
                        ms[i].Value = pps[i].result;
                    }
                    var cmd = SqlCmd.UpdateRow(ms, id, type);
                    SqlClient.Instance.ExecuteCmd(cmd);
                }
                else
                {
                    o.Close();
                    var pps = product.properties;
                    int len = pps.Length;
                    MSDBAttribute[] ms = new MSDBAttribute[len + 1];
                    for (int i = 0; i < len; i++)
                    {
                        ms[i] = new MSDBAttribute();
                        var name = pps[i].namee;
                        ms[i].FieldName = name;
                        ms[i].DbType = MySqlDataType.VARCHAR;
                        ms[i].len = 32;
                        ms[i].Value = pps[i].result;
                    }
                    ms[len - 1] = new MSDBAttribute();
                    ms[len - 1].FieldName = "code";
                    ms[len - 1].DbType = MySqlDataType.VARCHAR;
                    ms[len - 1].len = 64;
                    ms[len - 1].Value = product.code;
                    var cmd = SqlCmd.InsertNewRow(ms);
                    SqlClient.Instance.ExecuteCmd(cmd);
                }
            });
        }
    }
}
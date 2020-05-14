using System;
using System.Collections.Generic;
using System.Text;

namespace SqlManager.Sql
{
    public class SqlCmd
    {
        static Type msdbType= typeof(MSDBAttribute);
        public static MSDBAttribute[] TypeToAttrs(Type obj)
        {
            var fields = obj.GetFields();
            MSDBAttribute[] ms = new MSDBAttribute[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                var atts = f.GetCustomAttributes(msdbType, false);
                if (atts.Length > 0)
                {
                    ms[i] = atts[0] as MSDBAttribute;
                    ms[i].FieldName = f.Name.ToLower();
                }
            }
            return ms;
        }
        public static MSDBAttribute[] ObjectToAttrs(object tar)
        {
            Type obj = tar.GetType();
            var fields = obj.GetFields();
            MSDBAttribute[] ms = new MSDBAttribute[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                var atts = f.GetCustomAttributes(msdbType, false);
                if (atts.Length > 0)
                {
                    ms[i] = atts[0] as MSDBAttribute;
                    ms[i].FieldName = f.Name;
                    if (ms[i].DbType >= MySqlDataType.CHAR)
                        ms[i].Value = f.GetValue(tar) as string;
                    else ms[i].Value = f.GetValue(tar).ToString();
                }
            }
            return ms;
        }
        /// <summary>
        /// 返回创建表的命令
        /// </summary>
        /// <param name="obj">表类型</param>
        /// <param name="tablename">指定表名，不指定则使用类名</param>
        /// <returns></returns>
        public static string CreateNewTable(Type obj, string tablename = null)
        {
            MSDBAttribute[] ms = TypeToAttrs(obj);
            if(tablename==null)
                return CreateNewTable(ms,obj.Name.ToLower());
            else
                return CreateNewTable(ms,tablename.ToLower());
        }
        public static string CreateNewTable(MSDBAttribute[] atts, string tablename)
        {
            StringBuilder str = new StringBuilder();
            str.Append("CREATE TABLE ");
            str.Append(tablename.ToLower());
            str.Append("(id BIGINT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT");
            for (int i = 0; i < atts.Length; i++)
            {
                var att = atts[i];
                if(att!=null)
                {
                    if(att.FieldName!="id")
                    {
                        str.Append(",`");
                        str.Append(att.FieldName);
                        str.Append("` ");
                        str.Append(att.DbType.ToString());
                        if (att.len > 0)
                        {
                            str.Append("(");
                            str.Append(att.len.ToString());
                            str.Append(")");
                        }
                    }
                }
            }
            for(int i=0;i<atts.Length;i++)
            {
                var att = atts[i];
                if(att.unique)
                {
                    str.Append(",UNIQUE  (`");
                    str.Append(att.FieldName);// UNIQUE INDEX `code_UNIQUE` (`yt` ASC) VISIBLE);
                    str.Append("`)");
                }
            }
            str.Append(") ENGINE=InnoDB DEFAULT CHARSET=utf8 auto_increment=100000");
            return str.ToString();
        }
        public static string AddNewFeilds(MSDBAttribute[] atts,string tablename)
        {
            StringBuilder str = new StringBuilder();
            str.Append("ALTER TABLE ");
            str.Append(tablename.ToLower());
            bool multi = false;
            for(int i=0;i<atts.Length;i++)
            {
                var att = atts[i];
                if (att != null)
                {
                    if (multi)
                        str.Append(", ADD COLUMN `");
                    else str.Append(" ADD COLUMN `");
                    str.Append(att.FieldName);
                    str.Append("` ");
                    str.Append(att.DbType.ToString());
                    if (att.len > 0)
                    {
                        str.Append("(");
                        str.Append(att.len.ToString());
                        str.Append(")");
                    }
                    multi = true;
                }
            }
            str.Append(";");
            return str.ToString();
        }
        public static string InsertNewRow(object tar, string tablename = null)
        {
            var ms = ObjectToAttrs(tar);
            if (tablename == null)
                return InsertNewRow(ms, tar.GetType().Name);
            return InsertNewRow(ms,tablename);
        }
        public static string InsertNewRow(MSDBAttribute[] atts, string tablename)
        {
            StringBuilder str = new StringBuilder();
            str.Append("insert into ");
            str.Append(tablename.ToLower());
            str.Append("(");
            StringBuilder sv = new StringBuilder();
            sv.Append(" values(");
            bool multi = false;
            for (int i = 0; i < atts.Length; i++)
            {
                var att = atts[i];
              
                if(att!=null)
                {
                    if (att.FieldName != "id")
                    {
                        if (multi)
                        {
                            str.Append(",");
                            sv.Append(",");
                        }
                        if (att.DbType >= MySqlDataType.CHAR)
                            sv.Append("'" + att.Value + "'");
                        else sv.Append(att.Value);
                        str.Append('`');
                        str.Append(att.FieldName);
                        str.Append('`');
                        multi = true;
                    }
                }
            }
            str.Append(")");
            str.Append(sv);
            str.Append(")");
            return str.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="isString">是否是string类型</param>
        /// <returns></returns>
        public static string Query(string table,string key,string value,bool isString=false,string ret="*",int start=0,int end=0)
        {
            StringBuilder str = new StringBuilder();
            str.Append("select ");
            str.Append(ret);
            str.Append(" from ");
            str.Append(table);
            if(key!=null)
            {
                str.Append(" where ");
                str.Append(key);
                str.Append(" = ");
                if(isString)
                {
                    str.Append("'");
                    str.Append(value);
                    str.Append("'");
                }
                else str.Append(value);
            }
            if (start >= 0)
            {
                if (end > start)
                {
                    str.Append(" limit ");
                    str.Append(start);
                    str.Append(",");
                    str.Append(end);
                }
            }
            return str.ToString();
        }
        public static string Query2(string table, string key, string value, bool isString = false, string ret = "*", int start = 0, int end = 0)
        {
            StringBuilder str = new StringBuilder();
            str.Append("select ");
            str.Append(ret);
            str.Append(" from ");
            str.Append(table);
            if (key != null)
            {
                str.Append(" where ");
                str.Append(key);
                str.Append(" = ");
                if (isString)
                {
                    str.Append("'");
                    str.Append(value);
                    str.Append("'");
                }
                else str.Append(value);
            }
            if (start >= 0)
            {
                if (end > start)
                {
                    str.Append(" limit ");
                    str.Append(start);
                    str.Append(",");
                    str.Append(end);
                }
            }
            return str.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="keys"></param>
        /// <param name="values"></param>
        /// <param name="isString">是否是string类型</param>
        /// <returns></returns>
        public static string Query(string table, string[] keys, string[] values, bool isString = false,string ret="*", int start = 0, int end = 0)
        {
            StringBuilder str = new StringBuilder();
            str.Append("select ");
            str.Append(ret);
            str.Append(" from ");
            str.Append(table);
            bool multi = false;
            for(int i=0;i<keys.Length;i++)
            {
                if (multi)
                    str.Append(" and `");
                else str.Append(" where `");
                str.Append(keys[i]);
                str.Append("`=" );
                if (isString)
                {
                    str.Append("'");
                    str.Append(values[i]);
                    str.Append("'");
                }
                else str.Append(values[i]);
                multi = true;
            }
            if (start >= 0)
            {
                if (end > start)
                {
                    str.Append(" limit ");
                    str.Append(start);
                    str.Append(",");
                    str.Append(end);
                }
            }
            return str.ToString();
        }
        public static string Query(string table, MSDBAttribute[] atts,int start=0,int end=0,string ret="*")
        {
            StringBuilder str = new StringBuilder();
            str.Append("select ");
            str.Append(ret);
            str.Append(" from ");
            str.Append(table);
            bool multi = false;
            if(atts!=null)
            for(int i=0;i<atts.Length;i++)
            {
                var att= atts[i];
                if(att!=null)
                {
                        if (att.DbType >= MySqlDataType.CHAR)
                        {
                            if(att.Value!=null)
                            {
                                if (multi)
                                    str.Append(" and `");
                                else str.Append(" where `");
                                str.Append(att.FieldName.ToLower());
                                str.Append("`='" + att.Value + "'");
                                multi = true;
                            }
                        }
                        else
                        {
                            if(att.Value!=null)
                            {
                                if (multi)
                                    str.Append(" and `");
                                else str.Append(" where `");
                                multi = true;
                                str.Append(att.FieldName);
                                str.Append("`");
                                if(att.compar==null)
                                {
                                    str.Append("=");
                                    str.Append(att.Value);
                                }
                                else
                                {
                                    str.Append(att.compar);
                                    str.Append(att.Value);
                                    if(att.compar2!=null)
                                    {
                                        str.Append(" and `");
                                        str.Append(att.FieldName);
                                        str.Append("`");
                                        str.Append(att.compar2);
                                        str.Append(att.Value);
                                    }
                                }
                            }
                        }
                }
            }
            if(start>=0)
            {
                if(end>start)
                {
                    str.Append(" limit ");
                    str.Append(start);
                    str.Append(",");
                    str.Append(end);
                }
            }
            return str.ToString();
        }
        public static string ExistTable(string table,string database=null)
        {
            StringBuilder str = new StringBuilder();
            str.Append("select tables_name from information_schema.tables  where table_name ='");
            str.Append(table.ToLower());
            str.Append("'");
            if(database!=null)
            {
                str.Append(" and table_schema ='");
                str.Append(database);
                str.Append("'");
            }
            return str.ToString();
        }
        public static string QueryColumns(string table, string database = null)
        {
            StringBuilder str = new StringBuilder();
            str.Append("select column_name from information_schema.Columns where table_name ='");
            str.Append(table.ToLower());
            str.Append("'");
            if (database != null)
            {
                str.Append(" and table_schema ='");
                str.Append(database);
                str.Append("'");
            }
            return str.ToString();
        }
        public static string UpdateRow(object tar, Int64 id, string tablename = null)
        {
            var ms = ObjectToAttrs(tar);
            if (tablename == null)
                return UpdateRow(ms, id, tar.GetType().Name);
            return UpdateRow(ms, id, tablename);
        }
        public static string UpdateRow(MSDBAttribute[] atts, Int64 id, string tablename)
        {
            StringBuilder str = new StringBuilder();
            str.Append("update ");
            str.Append(tablename);
            str.Append(" set ");
            bool multi = false;
            for (int i = 0; i < atts.Length; i++)
            {
                var att = atts[i];
                if (att != null)
                {
                    if (multi)
                        str.Append(",");
                    str.Append('`');
                    str.Append(att.FieldName);
                    str.Append("`=");
                    if (att.DbType >= MySqlDataType.CHAR)
                        str.Append("'" + att.Value + "'");
                    else str.Append(att.Value);
                    multi = true;
                }
            }
            str.Append(" where (`id`='");
            str.Append(id);
            str.Append("');");
            return str.ToString();
        }
        public static string UpdateOrInsertRow(object tar,string tablename = null)
        {
            var ms = ObjectToAttrs(tar);
            if (tablename == null)
                return UpdateOrInsertRow(ms, tar.GetType().Name);
            return UpdateOrInsertRow(ms, tablename);
        }
        public static string UpdateOrInsertRow(MSDBAttribute[] atts, string tablename)
        {
            StringBuilder str = new StringBuilder();
            str.Append(InsertNewRow(atts,tablename));
            str.Append("ON DUPLICATE KEY UPDATE ");
            bool multi = false;
            for (int i = 0; i < atts.Length; i++)
            {
                var att = atts[i];
                if (!att.unique)
                {
                    if (multi)
                        str.Append(" and ");
                    else multi = true;
                    str.Append("`");
                    str.Append(att.FieldName);
                    str.Append("`=");
                    if (att.DbType >= MySqlDataType.CHAR)
                        str.Append("'" + att.Value + "'");
                    else str.Append(att.Value);
                    multi = true;
                }
            }
            return str.ToString();
        }
    }
}
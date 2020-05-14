using Data;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlManager.Sql
{
    public class SqlClient
    {
        static SqlClient ins;
        public static SqlClient Instance { get { if (ins == null) ins = new SqlClient(); return ins; } }
        MySqlConnection conn;
        MySqlCommand cmd;

        private SqlClient()
        {

            conn = new MySqlConnection();

            // string connStr = String.Format("server={0};user={1}; password={2}; charset='utf8';database=;pooling=false;SslMode=required",//none
            //"192.168.31.34", "huqiang", "Vj%KMsmde9Mv");//;Database='Cthulhu'
            Connect();
        }
        void Connect()
        {
            string connStr = String.Format("server={0};user={1}; password={2}; charset='utf8';database=;pooling=false;SslMode=none",
                LocalFile.myIP.ip, LocalFile.myIP.user, LocalFile.myIP.pass);

            conn.ConnectionString = connStr;
            try
            {
                conn.Open();
                cmd = conn.CreateCommand();
                Console.WriteLine("数据库连接成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        void ReConnect()
        {
            string connStr = String.Format("server={0};user={1}; password={2}; charset='utf8';database=;pooling=false;SslMode=none",
                LocalFile.myIP.ip, LocalFile.myIP.user, LocalFile.myIP.pass);

            conn.ConnectionString = connStr;
            try
            {
                conn.Open();
                conn.ChangeDatabase("editor");
                cmd = conn.CreateCommand();
                Console.WriteLine("数据库连接成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        /// <summary>
        /// 查询所有数据库
        /// </summary>
        /// <returns></returns>
        public List<string> ShowDatabase()
        {
            cmd.CommandText = "SHOW DATABASES";
            var read = cmd.ExecuteReader();
            List<string> tables = new List<string>();
            while (read.Read())
            {
                string str = read.GetString(0);
                tables.Add(str);
            }
            read.Close();
            return tables;
        }
        /// <summary>
        /// 创建一个数据库
        /// </summary>
        /// <param name="name"></param>
        public void CreateDatabase(string name)
        {
            cmd.CommandText = "CREATE DATABASE `"+name+"` CHARACTER SET 'utf8' COLLATE 'utf8_general_ci';";
            cmd.ExecuteNonQuery();
        }

        public void ChangeDatabase(string name)
        {
            conn.ChangeDatabase(name);
        }
        public List<string> ShowTables()
        {
            cmd.CommandText = "SHOW TABLES";
            var read = cmd.ExecuteReader();
            List<string> tables = new List<string>();
            while (read.Read())
            {
                string str = read.GetString(0);
                tables.Add(str.ToLower());
            }
            read.Close();
            return tables;
        }
        public void CreateTable(Type type)
        {
            cmd.CommandText = SqlCmd.CreateNewTable(type);
            cmd.ExecuteNonQuery();
        }
        public void CreateTable(MSDBAttribute[] atts,string tableName)
        {
            cmd.CommandText = SqlCmd.CreateNewTable(atts,tableName);
            cmd.ExecuteNonQuery();
        }
        public void ExecuteCmd(string cmdText)
        {
            if (conn.State == System.Data.ConnectionState.Closed | conn.State == System.Data.ConnectionState.Broken)//连接中断
                ReConnect();
            cmd.CommandText = cmdText;
            cmd.ExecuteNonQuery();
        }
        public void ExecuteCmd(string cmdText,Action WaitDo)
        {
            if (conn.State == System.Data.ConnectionState.Closed | conn.State == System.Data.ConnectionState.Broken)//连接中断
                ReConnect();
            cmd.CommandText = cmdText;
            cmd.ExecuteNonQuery();
            if (WaitDo != null)
                WaitDo();
        }
        public void ExecuteReader(string cmdText,Action<MySqlDataReader> callback)
        {
            if (conn.State == System.Data.ConnectionState.Closed | conn.State == System.Data.ConnectionState.Broken)//连接中断
                ReConnect();
            cmd.CommandText = cmdText;
            try
            {
                var read = cmd.ExecuteReader();
                try
                {
                    if (callback != null)
                        callback(read);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                if(!read.IsClosed)
                read.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace SqlManager.Sql
{
    public class MSDBAttribute : Attribute
    {
        static String Nums = "0123456789";
        public MySqlDataType DbType;
        /// <summary>
        /// 不能使用关键字 value valuse index
        /// </summary>
        public string FieldName;
        public string Value;
        public int len;
        /// <summary>
        /// "=",">","<",">=","<=","!="
        /// </summary>
        public string compar;
        /// <summary>
        /// "=",">","<",">=","<=","!="
        /// </summary>
        public string compar2;
        public bool unique;
        static void CheckValue(MSDBAttribute ms)
        {
            if (ms.Value == null)
                return;
            if (ms.DbType < MySqlDataType.CHAR)
            {
                if (ms.DbType < MySqlDataType.FLOAT)//整数型
                {
                    var v = ms.Value;
                    for (int i = 0; i < v.Length; i++)
                        if (Nums.IndexOf(v[i]) < 0)
                        {
                            if (i == 0)
                                ms.Value = null;
                            else ms.Value = v.Substring(0, i);
                            return;
                        }
                }
                else if (ms.DbType < MySqlDataType.DATE)//小数型
                {
                    var v = ms.Value;
                    int dc = 0;
                    for (int i = 0; i < v.Length; i++)
                    {
                        if (v[i] == '.')
                        {
                            dc++;
                            if (dc > 1)
                            {
                                if (i == 1)
                                    ms.Value = null;
                                else
                                {
                                    v = v.Substring(0, i);
                                    if (v[0] == '.')
                                        v = "0" + v;
                                    ms.Value = v;
                                }
                                return;
                            }
                        }
                        else
                        if (Nums.IndexOf(v[i]) < 0)
                        {
                            if (i == 0)
                                ms.Value = null;
                            else
                            {
                                v = v.Substring(0, i);
                                if (v[0] == '.')
                                    v = "0" + v;
                                ms.Value = v;
                            }
                            return;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 校验值的合法性
        /// </summary>
        public void CheckValue()
        {
            CheckValue(this);
        }
    }
    public enum MySqlDataType
    {
        /// <summary>
        /// 1字节 (-128，127)
        /// </summary>
        TINYINT,
        /// <summary>
        /// 2字节 (-32 768，32 767) 
        /// </summary>
        SMALLINT,
        /// <summary>
        /// 3字节 (-8 388 608，8 388 607) 
        /// </summary>
        MEDIUMINT,
        /// <summary>
        /// 4 字节  (-2 147 483 648，2 147 483 647) 
        /// </summary>
        INT,
        /// <summary>
        /// 8 字节 (-9 233 372 036 854 775 808，9 223 372 036 854 775 807) 
        /// </summary>
        BIGINT,
        /// <summary>
        /// 4 字节 (-3.402 823 466 E+38，-1.175 494 351 E-38)，0，(1.175 494 351 E-38，3.402 823 466 351 E+38)
        /// </summary>
        FLOAT,
        /// <summary>
        /// 8 字节 (-1.797 693 134 862 315 7 E+308，-2.225 073 858 507 201 4 E-308)，0，(2.225 073 858 507 201 4 E-308，1.797 693 134 862 315 7 E+308) 
        /// </summary>
        DOUBLE,
        /// <summary>
        /// 对DECIMAL(M,D) ，如果M>D，为M+2否则为D+2 
        /// </summary>
        DECIMAL,
        /// <summary>
        /// 3 字节 1000-01-01/9999-12-31 
        /// </summary>
        DATE,
        /// <summary>
        /// 3字节 '-838:59:59'/'838:59:59' 
        /// </summary>
        TIME,
        /// <summary>
        /// 1字节 1901/2155 
        /// </summary>
        YEAR,
        /// <summary>
        /// 8字节 1000-01-01 00:00:00/9999-12-31 23:59:59 
        /// </summary>
        DATETIME,
        /// <summary>
        /// 4字节 1970-01-01 00:00:00/2038 结束时间是第 2147483647 秒，北京时间 2038-1-19 11:14:07，格林尼治时间 2038年1月19日 凌晨 03:14:07
        /// </summary>
        TIMESTAMP,
        /// <summary>
        /// 0-255字符  定长字符串 在括号中指定字符串的长度
        /// </summary>
        CHAR,
        /// <summary>
        /// 0-255 字符  变长字符串 在括号中指定字符串的最大长度，如果值的长度大于 255，则被转换为 TEXT 类型。
        /// </summary>
        VARCHAR,
        /// <summary>
        /// 0-255字节  不超过 255 个字符的二进制字符串
        /// </summary>
        TINYBLOB,
        /// <summary>
        /// 0-255字节 短文本字符串
        /// </summary>
        TINYTEXT,
        /// <summary>
        /// 0-65 535字节   二进制形式的长文本数据
        /// </summary>
        BLOB,
        /// <summary>
        /// 0-65 535字节  长文本数据
        /// </summary>
        TEXT,
        /// <summary>
        /// 0-16 777 215字节 二进制形式的中等长度文本数据
        /// </summary>
        MEDIUMBLOB,
        /// <summary>
        /// 0-16 777 215字节 中等长度文本数据
        /// </summary>
        MEDIUMTEXT,
        /// <summary>
        /// 0-4 294 967 295字节  二进制形式的极大文本数据
        /// </summary>
        LONGBLOB,
        /// <summary>
        /// 0-4 294 967 295字节  极大文本数据
        /// </summary>
        LONGTEXT
    }
}

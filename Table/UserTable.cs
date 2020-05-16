using SqlManager.Sql;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.Table
{
    //用户信息
    public class UserInfo
    {
        [MSDBAttribute(DbType = MySqlDataType.BIGINT)]
        public long id;//id
        [MSDBAttribute(DbType = MySqlDataType.VARCHAR, len = 255)]
        public string deviceId;//设备id
        [MSDBAttribute(DbType = MySqlDataType.VARCHAR, len = 32)]
        public string key;
        [MSDBAttribute(DbType = MySqlDataType.VARCHAR, len = 32)]
        public string pass;
        [MSDBAttribute(DbType = MySqlDataType.VARCHAR, len = 32)]
        public string name;
        [MSDBAttribute(DbType = MySqlDataType.INT)]
        public int sex;
        [MSDBAttribute(DbType = MySqlDataType.VARCHAR, len = 255)]
        public string roleid;//图片名称
        [MSDBAttribute(DbType = MySqlDataType.INT)]
        public int RoomId;//房间id
        [MSDBAttribute(DbType = MySqlDataType.BIGINT)]
        public long LastLogin;//最后登录时间
        [MSDBAttribute(DbType = MySqlDataType.BIGINT)]
        public long LastExit;//最后退出时间
        [MSDBAttribute(DbType = MySqlDataType.BIGINT)]
        public long coins;
        [MSDBAttribute(DbType = MySqlDataType.BIGINT)]
        public long diamond;
        [MSDBAttribute(DbType = MySqlDataType.INT)]
        public int level;
        [MSDBAttribute(DbType = MySqlDataType.INT)]
        public int roomType;
        [MSDBAttribute(DbType = MySqlDataType.INT)]
        public int roomID;
    }
}
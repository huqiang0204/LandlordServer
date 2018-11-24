﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.Table
{
    //用户信息
    public class UserInfo
    {
        public int Id;//id
        public string DeviceId;//设备id
        public int RoomId;//房间id
        public long _LastLogin;//最后登录时间
        public bool _Online;//在线状态
        public long _LastExit;//最后退出时间
        public int _index;
    }
    public class UserTable
    {
        //用户集合
        static UserInfo[] users;
        //当前人数
        static int max;
        static int StartId = 10000;
        public static void Initial()
        {
            users = new UserInfo[65536];
        }
        /// <summary>
        /// 创建一个用户,如果该设备已存在用户则返回该用户
        /// </summary>
        /// <param name="DeviceId"></param>
        /// <returns></returns>
        public static UserInfo AddNewUser(string DeviceId)
        {
            for(int i=0;i<max;i++)
            {
                if(users[i]!=null)
                {
                    if(users[i].DeviceId==DeviceId)
                    {
                        users[i]._LastLogin = DateTime.Now.Ticks;
                        users[i]._Online = true;
                        return users[i];
                    }
                }
            }
            UserInfo info = new UserInfo();
            info.Id = StartId;
            info.DeviceId = DeviceId;
            info._LastLogin = DateTime.Now.Ticks;
            info._Online = true;
            StartId++;
            info._index = max;
            users[max] = info;
            max++;
            return info;
        }
        public static void RemoveUserAt(int index)
        {
            max--;
            users[index] = users[max];
        }
        public static void RemoveUser(UserInfo user)
        {
            if (user == null)
                return;
            RemoveUserAt(user._index);
        }

        const long OverTime = 3600000000;//1*60*60*1000000
        /// <summary>
        /// 清除1小时未登录的用户
        /// </summary>
        static void ReleaseExpiredUsers()
        {
            lock (users)
            {
                long now = DateTime.Now.Ticks;
                int i = max - 1;
                for (; i >= 0; i--)
                {
                    if (users[i] == null)
                    {
                        max--;
                        users[i] = users[max];
                        continue;
                    }
                    if (!users[i]._Online)
                        if (now - users[i]._LastExit > OverTime)
                        {
                            max--;
                            users[i] = users[max];
                        }
                }
            }
        }
        static long ClearTime;
        static int ls;
        public static void Update()
        {
            var s= DateTime.Now.Second;
            int i=s - ls;
            if (i < 0)
                i += 60;
            ls = s;
            ClearTime += i;
            if(ClearTime>3600)
            {
                ClearTime = 0;
                ReleaseExpiredUsers();
            }
        }
    }
}

using huqiang;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LandlordServer
{
    public enum UserAction
    {
        None = 0,       //无状态
        Jump = 1,       //直上跳
        L_Jump = 2,     //左跳
        R_Jump = 3,     //右跳
        Beatk = 4,      //被击中,僵直
        Def_up = 5,     //上段防御
        Def_down = 6,   //下段防御
        Atk = 7,        //攻击
        Atk_1 = 8,  //出拳
        Atk_2 = 9,      //膝撞
        Atk_3 = 10,      //肩冲
        Jump_atk = 11,   //跃击
        fell = 12,       //倒地
        crouch = 13,     //蹲伏
        diaup = 14,      //击飞
        GetUp = 15,      //起身
        collide = 16,   //普通的碰撞
        move = 17,      //移动
    }
    //摇杆操作
    public enum RockerType
    {
        None,
        up = 1,
        r_up = 2,//右跳
        right = 3,
        r_down = 4,//右下
        down = 5,
        l_down = 6,//左下
        left = 7,
        l_up = 8,//左跳
    }

    public enum Skill_ID
    {
        None = 0,   //无状态
        Skill_n = 1,    //A
        Skill_a = 2,
        Skill_b = 3,
        Skill_c = 4,
    }

    /// <summary>
    /// 接收玩家输入信息，左右移动和下蹲需要持续按键
    /// 跳跃只需要接收一次，服务器自动计算出接下来的位置
    /// </summary>
    public class User : Linker
    {

        public IPEndPoint romoteUdp;
        //用于UDP传输数据对象
        public UdpLink udpLink;
        public User(Socket soc) : base(soc)
        {
           
        }

     

        public override void Dispose()
        {
            base.Dispose();
            if (userInfo != null)
            {
                userInfo._Online = false;
                userInfo._LastExit = DateTime.Now.Ticks;
            }
        }

    }
}

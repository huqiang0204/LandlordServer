using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.Game
{
    public enum CardType
    {
        Single=0,
        Double=1,
        Three=2,
        ThreeBandOne=3,
        ThreeBandTow = 4,
        FourStrapsA=5,
        FourBandTwo=6,
        Dragon=7,
        Train =8,
        Plane=9,
        Bomb =10,
    }
    public class CardInfo
    {
        public CardType type;
        public int num;
        public int len;
    }
    public class FightingLandlord
    {
        Random random;
        int[] poker;
        byte[] rtp;
        public int[] CardsA, CardsB, CardsC, LordCrads;
        public FightingLandlord()
        {
            random = new Random();
            poker = new int[54];
            for (int i = 0; i < 54; i++)
                poker[i] = i;
            rtp = new byte[54];
            CardsA = new int[17];
            CardsB = new int[17];
            CardsC = new int[17];
            LordCrads = new int[3];
        }
        /// <summary>
        /// 洗牌
        /// </summary>
        public void ReStart()
        {
            random.NextBytes(rtp);
            for (int i = 0; i < 54; i++)
            {
                byte b = rtp[i];
                int c = poker[i];
                int r = b % 54;
                poker[i] = poker[r];
                poker[r] = c;
            }
            for(int i=0;i<17;i++)
            {
                int index = i * 3;
                CardsA[i] = poker[index];
                CardsB[i] = poker[index+1];
                CardsC[i] = poker[index+2];
            }
            LordCrads[0] = poker[51];
            LordCrads[1] = poker[52];
            LordCrads[2] = poker[53];
        }

        public static CardInfo DeckAnalysis(int[] cards)
        {
            int len = cards.Length;
            int[] nums = new int[len];
            for (int i = 0; i < len; i++)
            {
                if (cards[i] < 52)
                    nums[i] = cards[i] % 13;
                else nums[i] = cards[i];
            }
            Array.Sort(nums);
            switch (len)
            {
                case 1:
                    CardInfo inf = new CardInfo();
                    inf.type = CardType.Single;
                    inf.num = nums[0];
                    return inf;
                case 2:
                    if (cards[0] == 52 && cards[1] == 53)//王炸
                    {
                        inf = new CardInfo();
                        inf.type = CardType.Bomb;
                        inf.num = 14;
                        return inf;
                    }
                    else return CheckDouble(nums);
                case 3:
                    return CheckThree(nums);
                case 4:
                    return CheckFour(nums);
                case 5:
                    return CheckFive(nums);
                case 6:
                    return CheckSix(nums);
                default:
                    if(len%2==1)
                        return CheckOdd(cards);
                    else
                        return CheckEven(cards);
            }
        }
        static CardInfo CheckDouble(int[] card)
        {
            if(card[0]==card[1])
            {
                CardInfo inf = new CardInfo();
                inf.type = CardType.Double;
                inf.num = card[0];
                return inf;
            }
            return null;
        }
        static CardInfo CheckThree(int[] card)
        {
            if(card[0]==card[1]&&card[0]==card[2])
            {
                CardInfo inf = new CardInfo();
                inf.type = CardType.Three;
                inf.num = card[0];
                return inf;
            }
            return null;
        }
        static CardInfo CheckFour(int [] card)
        {
            if(card[0]!=card[3])
            {
                if(card[0]==card[1])
                {
                    if(card[0]==card[2])
                    {
                        CardInfo info = new CardInfo();
                        info.type = CardType.ThreeBandOne;
                        info.num = card[0];
                        return info;
                    }
                }else if(card[1]==card[3])
                {
                    if(card[2]==card[3])
                    {
                        CardInfo info = new CardInfo();
                        info.type = CardType.ThreeBandOne;
                        info.num = card[1];
                        return info;
                    }
                }
            }
            int a = card[0];
            for(int i=1;i<4;i++)
            {
                if (a != card[i])
                    return null;
            }
            CardInfo inf = new CardInfo();
            inf.type = CardType.Bomb;
            inf.num = card[0];
            return inf;
        }
        static CardInfo CheckFive(int[] card)
        {
            var info = CheckDragon(card);
            if (info != null)
                return info;
            int a = card[1];
            if(card[0]==a)
            {
                if(a==card[2] &&a==card[3])
                {
                    int c = card.Length;
                    info = new CardInfo();
                    info.type = CardType.FourStrapsA;
                    info.num = a;
                    info.len = c;
                    return info;
                }
            }else if(a==card[2])
            {
                if (a == card[3] && a == card[4])
                {
                    int c = card.Length;
                    info = new CardInfo();
                    info.type = CardType.FourStrapsA;
                    info.num = card[c - 1];
                    info.len = c;
                    return info;
                }
            }
            return null;
        }
        static CardInfo CheckSix(int[] card)
        {
            var info = CheckDragon(card);
            if (info != null)
                return info;
            info = CheckTrain(card);
            if (info != null)
                return info;
            info= CheckPlane(card);
            if (info != null)
                return info;
            int a = card[0];
            int l = 0;
            for(int i=1;i<card.Length;i++)
            {
                if(a==card[i])
                {
                    l++;
                    if(l>=3)
                    {
                        info = new CardInfo();
                        info.type = CardType.FourBandTwo;
                        info.num = a;
                        return info;
                    }
                }
                else
                {
                    l = 0;
                }
                a = card[i];
            }
            return null;
        }
        static CardInfo CheckPlane(int[] card)
        {
            int c = card.Length;
            if (card[c - 1] == 13)
                return null;
            bool start = false;
            int len = 0;
            int end = 0;
            for (int i = 0; i < c - 3; i++)
            {
                int a = card[i];
                if (a == card[i + 1] && a == card[i + 2])
                {
                    start = true;
                    len++;
                    i += 2;
                }
                else if (start)
                {
                    end = i;
                    break;
                }
            }
            int r = c - len * 3;
            if (r == 0)
            {
                CardInfo info = new CardInfo();
                info.type = CardType.Plane;
                info.num = card[end - 1];
                info.len = c;
                return info;
            }
            if (r == len)
            {
                CardInfo info = new CardInfo();
                info.type = CardType.ThreeBandOne;
                info.num = card[end - 1];
                info.len = c;
                return info;
            }
            if (r == len * 2)
            {
                int s = end - len * 3;
                int l = s / 2;
                for (int i = 0; i < l; i++)
                {
                    int t = i * 2;
                    if (card[t] != card[t + 1])
                        return null;
                }
                s = c - end;
                l = s / 2;
                for (int i = 0; i < l; i++)
                {
                    int t = end + i * 2;
                    if (card[t] != card[t + 1])
                        return null;
                }
                CardInfo info = new CardInfo();
                info.type = CardType.ThreeBandTow;
                info.num = card[end - 1];
                info.len = c;
                return info;
            }
            return null;
        }
        static CardInfo CheckDragon(int[] card)
        {
            int c = card.Length;
            if (card[c - 1] == 13)
                return null;
            int a = card[0];
            for(int i=1;i<c;i++)
            {
                if (card[i] - a != 1)
                    return null;
                a = card[i];
            }
            CardInfo info = new CardInfo();
            info.type = CardType.Dragon;
            info.num = card[c - 1];
            info.len = c;
            return info;
        }
        static CardInfo CheckTrain(int[] card)
        {
            int c = card.Length;
            if (card[c - 1] == 13)
                return null;
            c /= 2;
            int a = card[0];
            if (a != card[1])
                return null;
            for(int i=1;i<c;i++)
            {
                int s = i * 2;
                if (card[s] != card[s + 1])
                    return null;
                if (card[s] - a != 1)
                    return null;
                a = card[s];
            }
            CardInfo info = new CardInfo();
            info.type = CardType.Train;
            info.num = card[c - 1];
            info.len = c;
            return info;
        }
        static CardInfo CheckEven(int[] card)//偶数
        {
            var info = CheckDragon(card);
            if (info != null)
                return info;
            info = CheckTrain(card);
            if (info != null)
                return info;
            return CheckPlane(card);
        }
        static CardInfo CheckOdd(int[] card)//奇数
        {
            var info = CheckDragon(card);
            if (info != null)
                return info;
            return CheckPlane(card);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.Game
{
     public class FLGameRule
    {
        public FLGameRule()
        {
            Cards = new List<int>[3];
        }
        public List<int>[] Cards;
        int CurrentSeat=0;
        CardInfo CurrentCard;
        int passCount;
        bool OutCard(int [] cards,int seat)
        {
            if (seat != CurrentSeat)
                return false;
            Array.Sort(cards);
            int a = cards[0];
            for (int i = 1; i < cards.Length; i++)
            {
                if (a == cards[i])
                    return false;
                a = cards[i];
            } 
            List<int> ca=Cards[CurrentSeat];
            for(int i=0;i<cards.Length;i++)
            {
                if (!ca.Contains(cards[i]))
                    return false;
            }
           var card =  FightingLandlord.DeckAnalysis(cards);
            if (card == null)
                return false;
            if(CurrentCard==null)
            {
                CurrentCard = card;
                CurrentSeat++;
                if (CurrentSeat >= 3)
                    CurrentSeat = 0;
                passCount = 0;
                return true;
            }
            else
            {
                if (card.type == CardType.Bomb)
                {
                    if(card.num>CurrentCard.num)
                    {
                        CurrentCard = card;
                        CurrentSeat++;
                        if (CurrentSeat >= 3)
                            CurrentSeat = 0;
                        passCount = 0;
                        return true;
                    }
                    return false;
                }
                else
                {
                    if (card.type != CurrentCard.type)
                    {
                        return false;
                    }
                    if(card.num>CurrentCard.num)
                    {
                        CurrentCard = card;
                        CurrentSeat++;
                        if (CurrentSeat >= 3)
                            CurrentSeat = 0;
                        passCount = 0;
                        return true;
                    }
                    return false;
                }
            }
        }
        public int OutCards(int[] cards,int seat)
        {
            if(OutCard(cards,seat))
            {
                List<int> ca = Cards[CurrentSeat];
                for (int i = 0; i < cards.Length; i++)
                {
                    ca.Remove(cards[i]);
                }
                return ca.Count;
            }
            return -1;
        }
        public void Pass(int seat)
        {
            if (seat != CurrentSeat)
                return ;
            if (CurrentCard == null)
                return;
            CurrentSeat++;
            if (CurrentSeat >= 3)
                CurrentSeat = 0;
            passCount ++;
            if(passCount>=2)
            {
                passCount = 0;
                CurrentCard = null;
            }
        }
        public void SeatLord(int seat)
        {
            CurrentSeat = seat;
            CurrentCard = null;
            passCount = 0;
        }
    }
}
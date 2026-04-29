using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Core.GameLogic
{
    public static class DeckFactory
    {
        public static List<Card> CreateStandardDeck()
        {
            var cards = new List<Card>();

            for (int i = -2; i <= 12; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    cards.Add(new Card(i));
                }
            }

            var rng = new Random();
            return cards.OrderBy(x => rng.Next()).ToList();
        }
    }
}

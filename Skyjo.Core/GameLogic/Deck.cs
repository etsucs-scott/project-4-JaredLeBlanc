using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Core.GameLogic
{
    public  class Deck
    {
        private Stack<Card> _cards;

        public Deck(IEnumerable<Card> cards)
        {
            _cards = new Stack<Card>(cards.Reverse());
        }

        public List<Card> ToList()
        {
            return _cards.ToList();
        }

        public Card Draw()
        {
            if (_cards.Count == 0)
            {
                throw new InvalidOperationException("Deck is Empty");
            }

            return _cards.Pop();
        }

    }
}

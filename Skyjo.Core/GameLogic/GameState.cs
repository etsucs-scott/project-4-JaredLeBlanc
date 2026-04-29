using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Core.GameLogic
{
    public class GameState
    {
        public List<PlayerState> Players { get; set; }
        public List<Card> Deck { get; set; }
        public int CurrentPlayerIndex { get; set; }
        public Card? CurrentDrawnCard { get; set; }

    }
}

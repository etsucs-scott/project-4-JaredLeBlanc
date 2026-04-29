using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Core.GameLogic
{
    public class Player
    {
        public string Name { get; }

        public Dictionary<(int row, int col), Card> Board { get; }

        public Player(string name)
        {
            Name = name;
            Board = new Dictionary<(int row, int col), Card> ();
        }

    }
}

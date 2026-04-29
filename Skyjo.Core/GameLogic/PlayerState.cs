using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Core.GameLogic
{
    public class PlayerState
    {
        public string Name { get; set; }
        public Dictionary<string, Card> Board { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Tests
{
    public class EndTurnTest
    {
        [Fact]
        public void ChangesPlayer()
        {
            var game = TestHelpers.CreateGame();

            var first = game.CurrentPlayer;

            game.EndTurn();

            Assert.NotEqual(first, game.CurrentPlayer);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Tests
{
    public class FlipLimitTest
    {
        [Fact]
        public void CannotFlipMoreThanTwo()
        {
            var game = TestHelpers.CreateGame();

            var player = game.CurrentPlayer;

            game.FlipCard(0, 0);
            game.FlipCard(0, 1);

            // After 2 flips, turn auto-ends and player changes 
            // if changes to player2, then player1 hit flip limit
            Assert.NotEqual(player, game.CurrentPlayer);
        }
    }
}

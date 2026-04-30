using Xunit;
using System.Collections.Generic;

namespace Skyjo.Tests
{
    public class DealInitialCardsTest
    {
        [Fact]
        public void GiveEachPlayer12Cards()
        {
            var game = TestHelpers.CreateGame();

            Assert.Equal(12, game.CurrentPlayer.Board.Count);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Tests
{
    public class ReplaceCardTest
    {
        [Fact]
        public void ReplaceClearsDrawnCard()
        {
            var game = TestHelpers.CreateGameWithFinishedSetup();

            game.DrawCard();
            game.ReplaceCard(0, 0);

            Assert.False(game.HasDrawnCard);
        }
    }
}

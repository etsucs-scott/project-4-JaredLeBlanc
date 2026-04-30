using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Tests
{
    public class DrawCardTest
    {
        [Fact]
        public void DrawSetsCard()
        {
            var game = TestHelpers.CreateGameWithFinishedSetup();

            var card = game.DrawCard();

            Assert.NotNull(card);
            Assert.True(game.HasDrawnCard);
        }
    }
}

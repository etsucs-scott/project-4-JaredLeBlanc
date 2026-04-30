using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Tests
{
    public class TakeDiscardTest
    {
        [Fact]
        public void ReturnCards()
        {
            var game = TestHelpers.CreateGameWithFinishedSetup();

            var card = game.TakeFromDiscard();

            Assert.NotNull(card);
        }
    }
}

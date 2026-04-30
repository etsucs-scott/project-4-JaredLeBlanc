using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Tests
{
    public class DrawLimitTest
    {
        [Fact]
        public void CannotDrawTwice()
        {
            var game = TestHelpers.CreateGameWithFinishedSetup();

            game.DrawCard();

            Assert.Throws<InvalidOperationException>(() => game.DrawCard());
        }
    }
}

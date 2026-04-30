using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Tests
{
    public class FlipCardSetupTest
    {
        [Fact]
        public void FlipWorks()
        {
            var game = TestHelpers.CreateGame();

            Assert.True(game.FlipCard(0, 0));
        }
    }
}

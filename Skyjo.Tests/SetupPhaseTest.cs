using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Tests
{
    public class SetupPhaseTest
    {
        [Fact]
        public void StartsInSetupPhase()
        {
            var game = TestHelpers.CreateGame();

            Assert.True(game.IsSetupPhase);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Tests
{
    public class GameEndTest
    {
        [Fact]
        public void EndWhenAllCardsRevealed()
        {
            var game = TestHelpers.CreateGameWithFinishedSetup();

            var player = game.CurrentPlayer;

            // Reveal all but one card
            var positions = player.Board.Keys.ToList();
            for (int i = 0; i < positions.Count - 1; i++)
            {
                player.Board[positions[i]].IsFaceUp = true;
            }

            // simulate last card being revealed
            game.DrawCard();
            game.ReplaceCard(positions.Last().row, positions.Last().col);

            // First EndTurn calls for the end of the game
            // and gives other player one more turn
            game.EndTurn();

            // Second EndTurn answers call to end the game
            game.EndTurn();

            Assert.True(game.IsGameOver);
        }
    }
}

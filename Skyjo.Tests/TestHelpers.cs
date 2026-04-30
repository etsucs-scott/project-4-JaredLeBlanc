using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skyjo.Core.GameEngine;
using Skyjo.Core.GameLogic;

namespace Skyjo.Tests
{
    public class TestHelpers
    {
        // Helper to create a fresh game
        public static GameManager CreateGame()
        {
            var players = new List<Player>
            {
                new Player("P1"),
                new Player("P2")
            };

            var deck = new Deck(DeckFactory.CreateStandardDeck());
            var game = new GameManager(players, deck);

            game.DealInitialCards();

            return game;
        }
            
        // Helper to finish the setup part of the game
        public static GameManager CreateGameWithFinishedSetup()
        {
            var game = CreateGame();

            // Finish setup for both players
            game.FlipCard(0, 0);
            game.FlipCard(0, 1);

            game.FlipCard(0, 0);
            game.FlipCard(0, 1);

            return game;
        }
    }
}

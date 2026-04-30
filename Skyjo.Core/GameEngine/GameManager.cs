using Skyjo.Core.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyjo.Core.GameEngine
{
    public class GameManager
    {
        // Core game state
        private Queue<Player> _players;          // Turn order
        private Deck _deck;                      // Draw pile
        private Stack<Card> _discardPile = new();// Discard pile

        private Card? _currentDrawnCard;         // Card currently held by player

        public Player CurrentPlayer => _players.Peek();
        public Card? CurrentDrawnCard => _currentDrawnCard;
        public Card? TopDiscard => _discardPile.Count > 0 ? _discardPile.Peek() : null;

        // Turn state flags
        private bool _hasPlayedThisTurn = false;     // Prevents multiple actions per turn
        private bool _hasDrawnFromDeck = false;      // Player chose deck
        private bool _hasTakenDiscard = false;       // Player chose discard
        private bool _canFlipAfterDiscard = false;   // Allow flip after discarding

        //public bool HasPlayedThisTurn => _hasPlayedThisTurn;
        //public bool HasDrawnFromDeck => _hasDrawnFromDeck;
        //public bool HasTakenDiscard => _hasTakenDiscard;
        public bool HasDrawnCard => _currentDrawnCard != null;

        // Setup phase
        private bool _isSetupPhase = true;
        private Dictionary<Player, int> _flippedCount = new(); // Track 2 flips per player

        public bool IsSetupPhase => _isSetupPhase;

        // End game state
        private bool _isFinalRound = false;
        private Player? _playerWhoEndedRound = null;

        private bool _gameOver = false;
        public bool IsGameOver => _gameOver;

        public Dictionary<string, int>? FinalScores { get; private set; }

        public string? Winner =>
            FinalScores == null
                ? null
                : FinalScores.OrderBy(x => x.Value).First().Key;

        
        // Constructor
        public GameManager(List<Player> players, Deck deck)
        {
            _players = new Queue<Player>(players);
            _deck = deck;

            // Initialize setup tracking
            foreach (var player in players)
                _flippedCount[player] = 0;

            // Start discard pile with one face-up card
            var firstCard = _deck.Draw();
            firstCard.IsFaceUp = true;
            _discardPile.Push(firstCard);

            _isSetupPhase = true;

            ResetTurnState();
        }

        // Turn management
        public void EndTurn()
        {
            // Rotate players
            var player = _players.Dequeue();
            _players.Enqueue(player);

            // Check if setup phase is complete
            if (_isSetupPhase)
            {
                bool allDone = _flippedCount.Values.All(count => count >= 2);
                if (allDone)
                    _isSetupPhase = false;
            }

            var nextPlayer = CurrentPlayer;

            // Final round ends when turn comes back to triggering player
            if (_isFinalRound && nextPlayer == _playerWhoEndedRound)
            {
                EndGame();
            }

            ResetTurnState();
        }

        private void ResetTurnState()
        {
            // Reset aLL turn-related flags
            _hasPlayedThisTurn = false;
            _hasDrawnFromDeck = false;
            _hasTakenDiscard = false;
            _canFlipAfterDiscard = false;
            _currentDrawnCard = null;
        }

        // Draw and discard actions
        public Card DrawCard()
        {
            ValidateTurnStart();

            _currentDrawnCard = _deck.Draw();
            _hasDrawnFromDeck = true;

            return _currentDrawnCard;
        }

        public Card TakeFromDiscard()
        {
            ValidateTurnStart();

            var card = _discardPile.Pop();
            _currentDrawnCard = card;
            _hasTakenDiscard = true;

            return _currentDrawnCard;
        }

        public void DiscardDrawnCard()
        {
            if (_currentDrawnCard == null)
                throw new InvalidOperationException("No card to discard.");

            _currentDrawnCard.IsFaceUp = true;
            _discardPile.Push(_currentDrawnCard);

            _currentDrawnCard = null;

            // Now player is allowed to flip one card
            _canFlipAfterDiscard = true;
        }

        // Card Actions(replace/flip card)
        public void ReplaceCard(int row, int col)
        {
            ValidateAction();

            if (_currentDrawnCard == null)
                throw new InvalidOperationException("No card selected.");

            var player = CurrentPlayer;

            var oldCard = player.Board[(row, col)];
            oldCard.IsFaceUp = true;

            _discardPile.Push(oldCard);

            player.Board[(row, col)] = _currentDrawnCard;
            _currentDrawnCard.IsFaceUp = true;

            _currentDrawnCard = null;

            _hasPlayedThisTurn = true;

            CheckFinalRoundTrigger();
        }

        public bool FlipCard(int row, int col)
        {
            var player = CurrentPlayer;

            if (_gameOver)
                throw new InvalidOperationException("Game is over.");

            // Setup game phase
            if (_isSetupPhase)
            {
                if (_flippedCount[player] >= 2)
                    return false;

                if (player.Board.TryGetValue((row, col), out var card) && !card.IsFaceUp)
                {
                    card.IsFaceUp = true;
                    _flippedCount[player]++;

                    // Move to next player after 2 flips
                    if (_flippedCount[player] == 2)
                        EndTurn();

                    return true;
                }

                return false;
            }

            // Normal game phase
            ValidateAction();

            if (_hasPlayedThisTurn)
                throw new InvalidOperationException("End your turn.");

            if (_currentDrawnCard != null)
                throw new InvalidOperationException("Place or discard first.");

            if (!_canFlipAfterDiscard)
                throw new InvalidOperationException("Must draw a card before flipping.");

            if (player.Board.TryGetValue((row, col), out var normalCard) && !normalCard.IsFaceUp)
            {
                normalCard.IsFaceUp = true;

                _canFlipAfterDiscard = false;
                _hasPlayedThisTurn = true;

                CheckFinalRoundTrigger();

                return true;
            }

            return false;
        }

        // help with game rules
        private void ValidateTurnStart()
        {
            if (_gameOver)
                throw new InvalidOperationException("Game is over.");

            if (_hasPlayedThisTurn)
                throw new InvalidOperationException("End your turn.");

            if (_isSetupPhase)
                throw new InvalidOperationException("Finish setup first.");

            if (_hasDrawnFromDeck || _hasTakenDiscard)
                throw new InvalidOperationException("Already chose a card.");
        }

        private void ValidateAction()
        {
            if (_gameOver)
                throw new InvalidOperationException("Game is over.");

            if (_hasPlayedThisTurn)
                throw new InvalidOperationException("End your turn.");
        }

        private bool IsBoardFullyRevealed(Player player)
        {
            return player.Board.Values.All(card => card.IsFaceUp);
        }

        private void CheckFinalRoundTrigger()
        {
            if (!_isFinalRound && IsBoardFullyRevealed(CurrentPlayer))
            {
                _isFinalRound = true;
                _playerWhoEndedRound = CurrentPlayer;
            }
        }

        // End game logic
        private void EndGame()
        {
            RevealAllCards();
            FinalScores = CalculateScores();
            _gameOver = true;
        }

        private void RevealAllCards()
        {
            foreach (var player in _players)
            {
                foreach (var card in player.Board.Values)
                {
                    card.IsFaceUp = true;
                }
            }
        }

        public Dictionary<string, int> CalculateScores()
        {
            return _players.ToDictionary(
                p => p.Name,
                p => p.Board.Values.Sum(c => c.Value)
            );
        }

        // Save and Load
        public GameState ToGameState()
        {
            var playerList = _players.ToList();

            return new GameState
            {
                Players = playerList.Select(p => new PlayerState
                {
                    Name = p.Name,
                    Board = p.Board.ToDictionary(
                        kvp => $"{kvp.Key.row},{kvp.Key.col}",
                        kvp => kvp.Value
                    )
                }).ToList(),

                Deck = _deck.ToList(),
                CurrentPlayerIndex = playerList.FindIndex(p => p == CurrentPlayer),
                CurrentDrawnCard = _currentDrawnCard
            };
        }

        public static GameManager FromGameState(GameState state)
        {
            var players = state.Players.Select(ps =>
            {
                var player = new Player(ps.Name);

                foreach (var kvp in ps.Board)
                {
                    var parts = kvp.Key.Split(',');
                    int row = int.Parse(parts[0]);
                    int col = int.Parse(parts[1]);

                    player.Board[(row, col)] = kvp.Value;
                }

                return player;
            }).ToList();

            var deck = new Deck(state.Deck);
            var game = new GameManager(players, deck);

            for (int i = 0; i < state.CurrentPlayerIndex; i++)
                game.EndTurn();

            game._currentDrawnCard = state.CurrentDrawnCard;

            return game;
        }

        // Initial Setup
        public void DealInitialCards()
        {
            foreach (var player in _players)
            {
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 4; col++)
                    {
                        var card = _deck.Draw();
                        card.IsFaceUp = false;
                        player.Board[(row, col)] = card;
                    }
                }
            }
        }
    }
}

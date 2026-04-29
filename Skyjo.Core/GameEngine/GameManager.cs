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
        private Queue<Player> _players;

        private Deck _deck;

        private Card? _currentDrawnCard;

        public Card? CurrentDrawnCard => _currentDrawnCard;

        public Player CurrentPlayer => _players.Peek();

        private Dictionary<Player, int> _flippedCount = new();

        public bool HasDrawnCard => _currentDrawnCard != null;


        private bool _isSetupPhase = true;
        public bool IsSetupPhase => _isSetupPhase;

        // help solve drawing cards over and over again
        private bool _hasPlayedThisTurn = false;
        public bool HasPlayedThisTurn => _hasPlayedThisTurn;

        // discard pile
        private Stack<Card> _discardPile = new Stack<Card>();
        public Card? TopDiscard => _discardPile.Count > 0 ? _discardPile.Peek() : null;

        private bool _hasDrawnFromDeck = false;
        private bool _hasTakenDiscard = false;

        public bool HasDrawnFromDeck => _hasDrawnFromDeck;
        public bool HasTakenDiscard => _hasTakenDiscard;


        private TurnPhase _phase = TurnPhase.MustDraw;
        public TurnPhase Phase => _phase;

        private bool _canFlipAfterDiscard = false;


        public GameManager(List<Player> players, Deck deck)
        {
            _players = new Queue<Player>(players);

            _deck = deck;

            _flippedCount = new Dictionary<Player, int>();

            foreach (var player in players)
            {
                _flippedCount[player] = 0;
            }

            var firstCard = _deck.Draw();
            firstCard.IsFaceUp = true;
            _discardPile.Push(firstCard);
        }

        // for final round(once last card flipped)
        private bool _isFinalRound = false;
        private Player? _playerWhoEndedRound = null;

        private bool IsBoardFullyRevealed(Player player)
        {
            return player.Board.Values.All(card => card.IsFaceUp);
        }

        public void EndTurn()
        {
            var player = _players.Dequeue();
            _players.Enqueue(player);


            if (_isSetupPhase)
            {
                bool allDone = _flippedCount.Values.All(count => count >= 2);

                if (allDone)
                {
                    _isSetupPhase = false;
                }
            }

            var nextPlayer = CurrentPlayer;

            // if final round is active AND we returned to the player who ended it
            if (_isFinalRound && nextPlayer == _playerWhoEndedRound)
            {
                EndGame();
            }

            ResetTurnState();
        }

        private void ResetTurnState()
        {
            _hasPlayedThisTurn = false;
            _hasDrawnFromDeck = false;
            _hasTakenDiscard = false;
            _canFlipAfterDiscard = false;
            _currentDrawnCard = null;
        }

        public Dictionary<string, int> CalculateScores()
        {
            var scores = new Dictionary<string, int>();

            foreach (var player in _players)
            {
                int total = player.Board.Values.Sum(card => card.Value);
                scores[player.Name] = total;
            }

            return scores;
        }

        private bool _gameOver = false;
        public bool IsGameOver => _gameOver;

        public Dictionary<string, int>? FinalScores { get; private set; }

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

        public string? Winner
        {
            get
            {
                if (FinalScores == null) return null;

                return FinalScores.OrderBy(x => x.Value).First().Key;
            }
        }

        public Card DrawCard()
        {
            if (_gameOver)
                throw new InvalidOperationException("Game is over.");

            if (_hasPlayedThisTurn)
                throw new InvalidOperationException("End your turn.");

            if (_isSetupPhase)
                throw new InvalidOperationException("Finish setup first.");

            if (_currentDrawnCard == null && (_hasDrawnFromDeck || _hasTakenDiscard))
            {
                _hasDrawnFromDeck = false;
                _hasTakenDiscard = false;
            }

            if (_hasDrawnFromDeck || _hasTakenDiscard)
                throw new InvalidOperationException("Already chose a card.");

            _currentDrawnCard = _deck.Draw();
            _hasDrawnFromDeck = true;

            Console.WriteLine($"DrawState -> Drawn:{_hasDrawnFromDeck}, Taken:{_hasTakenDiscard}, CardNull:{_currentDrawnCard == null}");

            return _currentDrawnCard;
        }
        public Card TakeFromDiscard()
        {
            if (_gameOver)
                throw new InvalidOperationException("Game is over.");

            if (_hasPlayedThisTurn)
                throw new InvalidOperationException("End your turn.");

            if (_isSetupPhase)
                throw new InvalidOperationException("Finish setup first.");

            if (_currentDrawnCard == null && (_hasDrawnFromDeck || _hasTakenDiscard))
            {
                _hasDrawnFromDeck = false;
                _hasTakenDiscard = false;
            }

            if (_hasDrawnFromDeck || _hasTakenDiscard)
                throw new InvalidOperationException("Already chose a card.");

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

            _canFlipAfterDiscard = true;
        }

        public void ReplaceCard(int row, int col)
        {
            if (_gameOver)
                throw new InvalidOperationException("Game is over.");

            if (_hasPlayedThisTurn)
                throw new InvalidOperationException("End your turn.");

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

            if (!_isFinalRound && IsBoardFullyRevealed(CurrentPlayer))
            {
                _isFinalRound = true;
                _playerWhoEndedRound = CurrentPlayer;
            }

            //EndTurn();
        }

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

            // restore turn order
            for (int i = 0; i < state.CurrentPlayerIndex; i++)
                game.EndTurn();

            // restore drawn card
            game._currentDrawnCard = state.CurrentDrawnCard;

            return game;
        }

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

        public bool FlipCard(int row, int col)
        {
            var player = CurrentPlayer;

            if (_gameOver)
                throw new InvalidOperationException("Game is over.");

            // setup game phase
            if (_isSetupPhase)
            {
                if (_flippedCount[player] >= 2)
                    return false;

                if (player.Board.TryGetValue((row, col), out var card) && !card.IsFaceUp)
                {
                    card.IsFaceUp = true;
                    _flippedCount[player]++;

                    // auto move to next player
                    if (_flippedCount[player] == 2)
                    {
                        EndTurn();
                    }

                    return true;
                }

                return false;
            }

            // normal game phase
            if (_hasPlayedThisTurn)
                throw new InvalidOperationException("End your turn.");

            if (_currentDrawnCard != null)
                throw new InvalidOperationException("Place or discard first.");

            if (_currentDrawnCard == null && !_canFlipAfterDiscard)
                throw new InvalidOperationException("Must draw or choose discarded card.");

            if (!_canFlipAfterDiscard)
                throw new InvalidOperationException("You must discard before flipping.");

            if (player.Board.TryGetValue((row, col), out var normalCard) && !normalCard.IsFaceUp)
            {
                normalCard.IsFaceUp = true;

                _canFlipAfterDiscard = false;
                _hasPlayedThisTurn = true;

                if (!_isFinalRound && IsBoardFullyRevealed(CurrentPlayer))
                {
                    _isFinalRound = true;
                    _playerWhoEndedRound = CurrentPlayer;
                }

                //EndTurn();

                return true;
            }

            return false;
        }
    }
}

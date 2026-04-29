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

        public void EndTurn()
        {
            var player = _players.Dequeue();
            _players.Enqueue(player);

            _hasPlayedThisTurn = false;

            if (_isSetupPhase)
            {
                bool allDone = _flippedCount.Values.All(count => count >= 2);

                if (allDone)
                {
                    _isSetupPhase = false;
                }
            }
            _hasDrawnFromDeck = false;
            _hasTakenDiscard = false;
            _currentDrawnCard = null;
        }

        public Card DrawCard()
        {
            if (_isSetupPhase)
                throw new InvalidOperationException("Finish setup first.");

            if (_hasDrawnFromDeck || _hasTakenDiscard)
                throw new InvalidOperationException("Already chose a card.");

            _currentDrawnCard = _deck.Draw();
            _hasDrawnFromDeck = true;

            return _currentDrawnCard;
        }
        public Card TakeFromDiscard()
        {
            if (_isSetupPhase)
                throw new InvalidOperationException("Finish setup first.");

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
        }

        public void ReplaceCard(int row, int col)
        {
            if (_currentDrawnCard == null)
                throw new InvalidOperationException("No card selected.");

            var player = CurrentPlayer;

            var oldCard = player.Board[(row, col)];
            oldCard.IsFaceUp = true;

            _discardPile.Push(oldCard);

            player.Board[(row, col)] = _currentDrawnCard;
            _currentDrawnCard.IsFaceUp = true;

            _currentDrawnCard = null;
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

            // SETUP PHASE (always allowed)
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

            // NORMAL GAME (only after setup)
            if (_currentDrawnCard != null)
                throw new InvalidOperationException("Place or discard first.");

            if (!_hasDrawnFromDeck && !_hasTakenDiscard)
                throw new InvalidOperationException("Must draw or take discard first.");

            if (player.Board.TryGetValue((row, col), out var normalCard) && !normalCard.IsFaceUp)
            {
                normalCard.IsFaceUp = true;
                return true;
            }

            return false;
        }
    }
}

using Avalonia;
using Avalonia.Controls;
using Skyjo.Core.GameEngine;
using Skyjo.Core.GameLogic;
using SkyjoAvaloniaApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SkyjoAvaloniaApp.Commands;
using System.ComponentModel;

namespace SkyjoAvaloniaApp.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        // Commands
        public ICommand FlipCardCommand { get; }
        public ICommand DrawFromDeckCommand { get; }
        public ICommand TakeDiscardCommand { get; }
        public ICommand DiscardCardCommand { get; }

        // Core game reference
        private GameManager _gameManager;

        // UI bindings
        public ObservableCollection<CardViewModel> Board { get; set; } = new();

        public string CurrentPlayerName =>
            _gameManager?.CurrentPlayer?.Name ?? "Design Player";

        public int? DrawnCardValue =>
            _gameManager?.HasDrawnCard == true
                ? _gameManager.CurrentDrawnCard?.Value
                : null;

        public int? DiscardTopValue =>
            _gameManager?.TopDiscard?.Value;

        public bool CanDraw =>
            !_gameManager.IsSetupPhase && !_gameManager.HasDrawnCard;

        // End game UI
        public bool IsGameOver => _gameManager.IsGameOver;

        public string WinnerText =>
            _gameManager.Winner != null
                ? $"Winner: {_gameManager.Winner}"
                : "";

        public string ScoreText =>
            _gameManager.FinalScores == null
                ? ""
                : string.Join("\n",
                    _gameManager.FinalScores
                        .OrderBy(s => s.Value)
                        .Select(s =>
                            s.Key == _gameManager.Winner
                                ? $"{s.Key}: {s.Value}"
                                : $"{s.Key}: {s.Value}"));

        // Error messages
        public string? StatusMessage { get; set; }

        private void SetMessage(string msg)
        {
            StatusMessage = msg;
            OnPropertyChanged(nameof(StatusMessage));
        }

        private void ClearMessage()
        {
            StatusMessage = "";
            OnPropertyChanged(nameof(StatusMessage));
        }

        // Propterty changed
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Constructor
        public GameViewModel()
        {
            // Designer preview support (prevents crashes in XAML preview)
            if (Avalonia.Controls.Design.IsDesignMode)
            {
                Board = new ObservableCollection<CardViewModel>
                {
                    new CardViewModel { Value = 1 },
                    new CardViewModel { Value = 2 },
                    new CardViewModel { Value = 3 },
                    new CardViewModel { Value = 4 }
                };
                return;
            }

            // Initialize commands
            FlipCardCommand = new RelayCommand(FlipCard);
            DrawFromDeckCommand = new RelayCommand(_ => ExecuteAction(() => _gameManager.DrawCard()));
            TakeDiscardCommand = new RelayCommand(_ => ExecuteAction(() => _gameManager.TakeFromDiscard()));
            DiscardCardCommand = new RelayCommand(_ => ExecuteAction(() => _gameManager.DiscardDrawnCard()));

            // Initialize game
            InitializeGame();
        }



        // Game setup
        private void InitializeGame()
        {
            try
            {
                var players = new List<Player>
                {
                    new Player("Player 1"),
                    new Player("Player 2")
                };

                var deck = new Deck(DeckFactory.CreateStandardDeck());
                _gameManager = new GameManager(players, deck);

                _gameManager.DealInitialCards();

                RefreshUI();
            }
            catch
            {
                // Prevents designer crash
            }
        }

        // Action Handler
        /// <summary>
        /// Wraps GameManager calls with try/catch + UI refresh
        /// Prevents repeating the same error handling everywhere
        /// </summary>
        private void ExecuteAction(Action action)
        {
            try
            {
                action();
                ClearMessage();
                RefreshUI();
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message);
            }
        }

        // Game actions
        public void EndTurn()
        {
            ExecuteAction(_gameManager.EndTurn);
        }

        public void SaveGame()
        {
            var service = new SaveService();
            service.Save(_gameManager.ToGameState(), "save.json");
        }

        public void LoadGame()
        {
            var service = new SaveService();
            var state = service.Load("save.json");

            _gameManager = GameManager.FromGameState(state);

            RefreshUI();
        }

        // Card interactions
        private void FlipCard(object? param)
        {
            if (param is not CardViewModel cardVm || _gameManager == null)
                return;

            ExecuteAction(() =>
            {
                if (_gameManager.IsSetupPhase)
                {
                    _gameManager.FlipCard(cardVm.Row, cardVm.Col);
                }
                else if (_gameManager.HasDrawnCard)
                {
                    _gameManager.ReplaceCard(cardVm.Row, cardVm.Col);
                }
                else
                {
                    _gameManager.FlipCard(cardVm.Row, cardVm.Col);
                }
            });
        }

        // UI updates
        private void RefreshUI()
        {
            LoadBoard();

            OnPropertyChanged(nameof(CurrentPlayerName));
            OnPropertyChanged(nameof(DrawnCardValue));
            OnPropertyChanged(nameof(DiscardTopValue));

            OnPropertyChanged(nameof(IsGameOver));
            OnPropertyChanged(nameof(WinnerText));
            OnPropertyChanged(nameof(ScoreText));
        }

        private void LoadBoard()
        {
            Board.Clear();

            var player = _gameManager.CurrentPlayer;

            foreach (var kvp in player.Board)
            {
                Board.Add(new CardViewModel
                {
                    Row = kvp.Key.row,
                    Col = kvp.Key.col,
                    Value = kvp.Value.Value,
                    IsFaceUp = kvp.Value.IsFaceUp,
                    FlipCommand = FlipCardCommand
                });
            }
        }
    }
}

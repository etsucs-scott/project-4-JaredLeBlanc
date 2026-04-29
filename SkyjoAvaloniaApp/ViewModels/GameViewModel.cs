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
        public ICommand FlipCardCommand { get; }

        public ICommand DrawCardCommand { get; }

        public ICommand DrawFromDeckCommand { get; }
        public ICommand TakeDiscardCommand { get; }
        public ICommand DiscardCardCommand { get; }

        private GameManager _gameManager;

        public ObservableCollection<CardViewModel> Board { get; set; } = new();

        public string CurrentPlayerName => _gameManager?.CurrentPlayer?.Name ?? "Design Player";

        public int? DrawnCardValue => _gameManager?.HasDrawnCard == true
        ? _gameManager.CurrentDrawnCard?.Value : null;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool CanDraw => !_gameManager.IsSetupPhase && !_gameManager.HasDrawnCard;

        public int? DiscardTopValue => _gameManager.TopDiscard?.Value;

        public string? Winner => _gameManager.Winner;
        public bool IsGameOver => _gameManager.IsGameOver;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public GameViewModel()
        {
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

            FlipCardCommand = new RelayCommand(FlipCard);

            DrawCardCommand = new RelayCommand(_ =>
            {
                try
                {
                    _gameManager.DrawCard();
                    RefreshUI();
                }
                catch (Exception ex)
                {
                    SetMessage(ex.Message);
                }
            });

            DrawFromDeckCommand = new RelayCommand(_ =>
            {
                try
                {
                    _gameManager.DrawCard();
                    RefreshUI();
                }
                catch (Exception ex)
                {
                    SetMessage(ex.Message);
                }
            });

            TakeDiscardCommand = new RelayCommand(_ =>
            {
                try
                {
                    _gameManager.TakeFromDiscard();
                    RefreshUI();
                }
                catch (Exception ex)
                {
                    SetMessage(ex.Message);
                }
            });

            DiscardCardCommand = new RelayCommand(_ =>
            {
                try
                {
                    _gameManager.DiscardDrawnCard();
                    RefreshUI();
                }
                catch (Exception ex)
                {
                    SetMessage(ex.Message);
                }
            });

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
                LoadBoard();
            }
            catch
            {
                // prevents designer crash if anything goes wrong
            }
        }

        // winner and score text for when game ends
        public string WinnerText =>
            _gameManager.Winner != null
                ? $"Winner: {_gameManager.Winner}"
                : "";

        public string ScoreText =>
            _gameManager.FinalScores == null
            ? ""
            : string.Join("\n",
                _gameManager.FinalScores
                    .OrderBy(s => s.Value) // lowest first
                    .Select(s => $"{s.Key}: {s.Value}"));


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

        public void DrawCard()
        {
            _gameManager.DrawCard();
            ClearMessage();
            LoadBoard();
        }

        public void EndTurn()
        {
            try
            {
                _gameManager.EndTurn();

                ClearMessage();

                RefreshUI();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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

        private void FlipCard(object? param)
        {
            if (param is not CardViewModel cardVm || _gameManager == null)
                return;

            try
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

                ClearMessage();
                RefreshUI();
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message);
            }
        }

        private void RefreshUI()
        {
            LoadBoard();
            OnPropertyChanged(nameof(CurrentPlayerName));
            OnPropertyChanged(nameof(DrawnCardValue));
            OnPropertyChanged(nameof(DiscardTopValue));

            OnPropertyChanged(nameof(Winner));
            OnPropertyChanged(nameof(IsGameOver));
            OnPropertyChanged(nameof(WinnerText));
            OnPropertyChanged(nameof(ScoreText));
        }
    }
}

using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyjoAvaloniaApp.ViewModels;

namespace SkyjoAvaloniaApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new GameViewModel();
        }

        /*
        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as GameViewModel)?.DrawCard();
        }
        */

        private void EndTurn_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as GameViewModel)?.EndTurn();
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as GameViewModel)?.SaveGame();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as GameViewModel)?.LoadGame();
        }
    }
}
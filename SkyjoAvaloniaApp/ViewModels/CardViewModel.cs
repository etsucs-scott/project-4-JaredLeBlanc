using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SkyjoAvaloniaApp.ViewModels
{
    public class CardViewModel
    {
        public int Value { get; set; }
        public bool IsFaceUp { get; set; }

        public int Row { get; set; }
        public int Col { get; set; }
        public string DisplayValue => IsFaceUp ? Value.ToString() : "?";
        public ICommand? FlipCommand { get; set; }
    }
}

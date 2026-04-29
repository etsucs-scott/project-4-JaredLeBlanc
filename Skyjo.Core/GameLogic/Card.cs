namespace Skyjo.Core.GameLogic
{
    public class Card
    {
        public int Value { get; }
        public bool IsFaceUp { get; set; }

        public Card(int value)
        {
            Value = value;
            IsFaceUp = false;
        }

        public void Flip()
        {
            IsFaceUp = true;
        }
    }
}

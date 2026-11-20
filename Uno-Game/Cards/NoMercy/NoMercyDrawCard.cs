using Uno_Game.Cards.StackPack;
using Uno_Game.Core;

namespace Uno_Game.Cards.NoMercy
{
    public class NoMercyDrawCard : StackCard
    {
        public NoMercyDrawCard(CardColor color, int drawCount = 2, int points = 20) 
            : base(color, drawCount, points)
        {

        }

        protected override string Name => $"draw {DrawCount}";

        public override bool SharesSymbolWith(Card other) =>
        other is StackCard stackCard;
    }
}

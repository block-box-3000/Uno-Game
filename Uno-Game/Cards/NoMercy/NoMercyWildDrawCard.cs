using Uno_Game.Cards.StackPack;
using Uno_Game.Core;

namespace Uno_Game.Cards.NoMercy
{
    public class NoMercyWildDrawCard : StackCard
    {
        public NoMercyWildDrawCard(int drawCount = 6, int points = 50) : base(CardColor.Wild, drawCount, points) 
        {
            
        }

        protected override string Name => $"Wild draw {DrawCount}";

        public override bool SharesSymbolWith(Card other) =>
        other is StackCard stackCard;
    }
}

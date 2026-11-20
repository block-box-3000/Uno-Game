using Uno_Game.Cards.ReversePack;
using Uno_Game.Cards.StackPack;
using Uno_Game.Core;

namespace Uno_Game.Cards.NoMercy;

public class WildReverseDrawCard : StackCard
{

    public WildReverseDrawCard(int drawCount = 4) : base (CardColor.Wild, drawCount, 50)
    {
        //Falta revisar que cuando se juegue esta carta en el stack se aplique el reversa
    }

    protected override string Name => $"Wild Reverse Draw {DrawCount} Card";

    public override void Apply(Game game)
    {
        game.Reverse();
        base.Apply(game);
    }

    public override bool SharesSymbolWith(Card other) =>
    other is ReverseDrawCard
    || other is ReverseCard
    || other.GetType() == typeof(DrawCard);
}

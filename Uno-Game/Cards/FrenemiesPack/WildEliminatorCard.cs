using Uno_Game.Core;

namespace Uno_Game.Cards.FrenemiesPack;

/// <summary>Every opponent discards their wild cards (you also choose the color).</summary>
public class WildEliminatorCard : Card
{
    public WildEliminatorCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild eliminator";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.EliminateWilds();
    }
}

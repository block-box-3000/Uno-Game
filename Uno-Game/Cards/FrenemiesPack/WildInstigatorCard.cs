using Uno_Game.Core;

namespace Uno_Game.Cards.FrenemiesPack;

/// <summary>Choose as many opponents as you want and how many cards (1-4) each one draws (you also choose the color).</summary>
public class WildInstigatorCard : Card
{
    public WildInstigatorCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild instigator";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.Instigate();
    }
}

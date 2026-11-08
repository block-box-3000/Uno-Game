using Uno_Game.Core;

namespace Uno_Game.Cards.FlexVersion;

/// <summary>Everybody flips their Power Card to its other side, then you choose the color.</summary>
public class WildAllFlipCard : Card
{
    public WildAllFlipCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild all flip";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.FlipAllPowerCards();
    }
}

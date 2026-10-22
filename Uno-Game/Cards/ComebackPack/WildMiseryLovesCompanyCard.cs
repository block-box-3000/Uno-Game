using Uno_Game.Core;

namespace Uno_Game.Cards.ComebackPack;

/// <summary>All the other players draw until their hand has as many cards as the largest hand (and you choose the color).</summary>
public class WildMiseryLovesCompanyCard : Card
{
    public WildMiseryLovesCompanyCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild misery loves company";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.MiseryLovesCompany();
    }
}

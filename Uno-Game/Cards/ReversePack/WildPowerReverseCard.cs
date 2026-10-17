using Uno_Game.Core;

namespace Uno_Game.Cards.ReversePack;

/// <summary>Wild card that reverses the direction and lets the same player go again.</summary>
public class WildPowerReverseCard : Card
{
    public WildPowerReverseCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild power reverse";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.Reverse();
        game.GrantExtraTurn();
    }
}

using Uno_Game.Core;

namespace Uno_Game.Cards.FlipVersion;

/// <summary>Everything flips to the other side. Played on a matching color or on another Flip card.</summary>
public class FlipCard : Card
{
    public FlipCard(CardColor color, int points = 20) : base(color, points)
    {
    }

    protected override string Name => "flip";

    public override void Apply(Game game) => game.Flip();
}

using Uno_Game.Core;

namespace Uno_Game.Cards.FlexVersion;

/// <summary>A number card with the Flip symbol: whoever plays it flips their Power Card to the other side.</summary>
public class NumberFlipCard : NumberCard
{
    public NumberFlipCard(CardColor color, int number) : base(color, number)
    {
    }

    protected override string Name => $"{Number} (flip)";

    public override bool FlipsPowerCard => true;
}

using Uno_Game.Core;

namespace Uno_Game.Cards.ComebackPack;

/// <summary>
/// Choose the color, then discard an additional card on top of this one. That card has its effect
/// (draw, skip...) and the color stays the one chosen here. Has the letter N for the U-N-O rule.
/// </summary>
public class WildDoubleDownCard : Card
{
    public WildDoubleDownCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild double down";

    public override char? UnoLetter => 'N';

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.DoubleDown();
    }
}

using Uno_Game.Core;

namespace Uno_Game.Cards.FlexVersion;

/// <summary>
/// Regular side of the Wild Flex Draw 4: a classic Wild Draw 4. You choose the color and the next player draws 4 and
/// misses their turn, unless they challenge (see <see cref="Game.WildDrawFour"/>).
/// </summary>
public class WildDrawFourCard : Card
{
    public WildDrawFourCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild draw 4";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.WildDrawFour();
    }
}

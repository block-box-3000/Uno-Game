using Uno_Game.Core;

namespace Uno_Game.Cards.FlipVersion;

/// <summary>
/// Light side: choose the color and the next player draws 2 cards and misses their turn.
/// It should only be played without a card of the color in play (the next player may challenge).
/// </summary>
public class WildDrawTwoCard : Card
{
    public WildDrawTwoCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild draw 2";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.WildDrawTwo();
    }
}

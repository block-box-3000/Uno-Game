using Uno_Game.Core;

namespace Uno_Game.Cards.FlipVersion;

/// <summary>
/// Dark side: choose the color and the next player draws until they get a card of that color, then misses their turn.
/// It should only be played without a card of the color in play (the next player may challenge).
/// </summary>
public class WildDrawColorCard : Card
{
    public WildDrawColorCard() : base(CardColor.Wild, points: 60)
    {
    }

    protected override string Name => "Wild draw color";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.WildDrawColor();
    }
}

using Uno_Game.Core;

namespace Uno_Game.Cards.FrenemiesPack;

/// <summary>
/// Choose another player as your enemy (and choose the color): every time you make your enemy draw cards,
/// they draw 2 extra cards.
/// </summary>
public class WildNemesisCard : Card
{
    public WildNemesisCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild nemesis";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.SetNemesis();
    }
}

using Uno_Game.Core;

namespace Uno_Game.Cards;

public class WildCard : Card
{
    public WildCard(int points = 50) : base(CardColor.Wild, points)
    {
    }

    protected override string Name => "Wild";

    public override void Apply(Game game) => game.ChooseColor();
}

using Uno_Game.Core;

namespace Uno_Game.Cards;

public class WildCard : Card
{
    public WildCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild";

    public override void Apply(Game game) => game.ChooseColor();
}

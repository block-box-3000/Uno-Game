using Uno_Game.Core;

namespace Uno_Game.Cards;

public class DrawWildCard : DrawCard
{
    public DrawWildCard(int drawCount = 4, int turnsSkipped = 1)
        : base(CardColor.Wild, drawCount, points: 50, turnsSkipped)
    {
    }

    protected override string Name => $"Wild draw {DrawCount}";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        base.Apply(game);
    }
}

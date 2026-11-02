using Uno_Game.Core;

namespace Uno_Game.Cards;

public class WildDrawCard : Card
{
    int DrawCount { get; init; }
    public WildDrawCard(int drawCount = 4, int turnsSkipped = 1)
        : base(CardColor.Wild, points: 50)
    {
        DrawCount = drawCount;
    }

    protected override string Name => $"Wild draw {DrawCount}";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        if (game.Packs.HasFlag(ExpansionPacks.Stack))
        {
            if (!game.ChallengeWildDrawBeforeStack(DrawCount))
                game.ResolveStack(DrawCount);
        }
        else
            game.WildDrawCards(DrawCount);
    }
}

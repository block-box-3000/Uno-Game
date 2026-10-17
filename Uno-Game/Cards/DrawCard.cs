using Uno_Game.Cards.ReversePack;
using Uno_Game.Core;

namespace Uno_Game.Cards;

/// <summary>The next player draws cards and loses their turn.</summary>
public class DrawCard : SkipCard
{
    public int DrawCount { get; }

    public DrawCard(CardColor color, int drawCount = 2, int points = 20, int turnsSkipped = 1)
        : base(color, points, turnsSkipped)
    {
        DrawCount = drawCount;
    }

    protected override string Name => $"draw {DrawCount}";

    public override void Apply(Game game)
    {
        if (game.Packs.HasFlag(ExpansionPacks.Stack))
            game.ResolveStack(DrawCount);
        else
            game.PunishNextPlayer(DrawCount, TurnsSkipped);
    }

    public override bool SharesSymbolWith(Card other) =>
    other is ReverseDrawCard
    || other.GetType() == typeof(DrawCard);
}

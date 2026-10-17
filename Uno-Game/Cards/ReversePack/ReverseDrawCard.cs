using Uno_Game.Core;

namespace Uno_Game.Cards.ReversePack;

/// <summary>Reverses the direction; the next player (in the new direction) draws and loses their turn.</summary>
public class ReverseDrawCard : Card
{
    public int DrawCount { get; }
    public int TurnsSkipped { get; }

    public ReverseDrawCard(CardColor color, int drawCount = 2, int points = 20, int turnsSkipped = 1)
        : base(color, points)
    {
        DrawCount = drawCount;
        TurnsSkipped = turnsSkipped;
    }

    protected override string Name => $"reverse draw {DrawCount}";

    public override void Apply(Game game)
    {
        game.Reverse();
        game.PunishNextPlayer(DrawCount, TurnsSkipped);
    }

    public override bool SharesSymbolWith(Card other) =>
    other is ReverseDrawCard
    || other is ReverseCard
    || other.GetType() == typeof(DrawCard);
}

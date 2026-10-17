using Uno_Game.Core;

namespace Uno_Game.Cards.ReversePack;

/// <summary>Reverses the direction and skips the next player (in the new direction).</summary>
public class ReverseSkipCard : Card
{
    public int TurnsSkipped { get; }

    public ReverseSkipCard(CardColor color, int points = 20, int turnsSkipped = 1) : base(color, points)
    {
        TurnsSkipped = turnsSkipped;
    }

    protected override string Name => "reverse skip";

    public override void Apply(Game game)
    {
        game.Reverse();
        game.PunishNextPlayer(drawCount: 0, TurnsSkipped);
    }

    public override bool SharesSymbolWith(Card other) =>
    other is ReverseSkipCard
    || other is ReverseCard
    || other.GetType() == typeof(SkipCard);
}

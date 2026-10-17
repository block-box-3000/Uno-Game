using Uno_Game.Cards.ReversePack;
using Uno_Game.Core;

namespace Uno_Game.Cards;

public class SkipCard : Card
{
    public int TurnsSkipped { get; }

    public SkipCard(CardColor color, int points = 20, int turnsSkipped = 1) : base(color, points)
    {
        TurnsSkipped = turnsSkipped;
    }

    protected override string Name => "skip";

    public override void Apply(Game game) => game.PunishNextPlayer(drawCount: 0, TurnsSkipped);

    public override bool SharesSymbolWith(Card other) =>
    other is ReverseSkipCard
    || other.GetType() == typeof(SkipCard);
}

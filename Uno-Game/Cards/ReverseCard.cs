using Uno_Game.Cards.ReversePack;
using Uno_Game.Core;

namespace Uno_Game.Cards;

public class ReverseCard : Card
{
    public ReverseCard(CardColor color, int points = 20) : base(color, points)
    {
    }

    protected override string Name => "reverse";

    public override void Apply(Game game)
    {
        //With two players a reverse works like a skip
        if (game.Players.Count == 2)
            game.SkipNextPlayers(1);
        else
            game.Reverse();
    }

    public override bool SharesSymbolWith(Card other) =>
    other is ReverseDrawCard
    || other is ReverseSkipCard
    || other.GetType() == typeof(ReverseCard);
}

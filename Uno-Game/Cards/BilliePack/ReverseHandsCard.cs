using Uno_Game.Core;

namespace Uno_Game.Cards.BilliePack;

/// <summary>Everybody lays their hand face up until the player who played this card starts their next turn.</summary>
public class ReverseHandsCard : Card
{
    public ReverseHandsCard(CardColor color, int points = 20) : base(color, points)
    {
    }

    protected override string Name => "reverse hands";

    public override void Apply(Game game) => game.RevealHands();
}

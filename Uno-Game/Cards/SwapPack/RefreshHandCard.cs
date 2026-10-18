using Uno_Game.Core;

namespace Uno_Game.Cards.SwapPack;

/// <summary>Put your whole hand in the discard pile and draw the same number of cards.</summary>
public class RefreshHandCard : Card
{
    public RefreshHandCard(CardColor color, int points = 20) : base(color, points)
    {
    }

    protected override string Name => "refresh hand";

    public override void Apply(Game game) => game.RefreshHand();
}

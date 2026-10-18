using Uno_Game.Core;

namespace Uno_Game.Cards.SwapPack;

/// <summary>Take a random card from another player and give them one of yours.</summary>
public class SwapOneCard : Card
{
    public SwapOneCard(CardColor color, int points = 20) : base(color, points)
    {
    }

    protected override string Name => "swap 1";

    public override void Apply(Game game) => game.SwapCardWithPlayer();
}

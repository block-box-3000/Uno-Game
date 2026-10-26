using Uno_Game.Core;

namespace Uno_Game.Cards.FlipVersion;

/// <summary>Dark side: all the other players are skipped, so the player who played it plays again.</summary>
public class SkipEveryoneCard : Card
{
    public SkipEveryoneCard(CardColor color, int points = 30) : base(color, points)
    {
    }

    protected override string Name => "skip everyone";

    public override void Apply(Game game) => game.SkipEveryone();
}

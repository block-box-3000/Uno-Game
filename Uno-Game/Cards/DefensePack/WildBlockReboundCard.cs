using Uno_Game.Core;
using Uno_Game.Players;

namespace Uno_Game.Cards.DefensePack;

/// <summary>Bounces a Draw penalty back to the player who played it, who draws one card more.</summary>
public class WildBlockReboundCard : DefenseCard
{
    private const int ExtraCards = 1;

    protected override string Name => "Wild block & rebound";

    public override bool CanAnswer(int drawCount) => drawCount > 0;

    public override void Answer(Game game, Player victim, Player attacker, int drawCount, int turnsSkipped) =>
        game.PassPenalty(victim, attacker, drawCount + ExtraCards, turnsSkipped);
}

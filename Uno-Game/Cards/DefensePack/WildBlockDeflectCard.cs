using Uno_Game.Core;
using Uno_Game.Players;

namespace Uno_Game.Cards.DefensePack;

/// <summary>Deflects a Draw or Skip penalty to the next player in the current order.</summary>
public class WildBlockDeflectCard : DefenseCard
{
    protected override string Name => "Wild block & deflect";

    public override bool CanAnswer(int drawCount) => true;

    public override void Answer(Game game, Player victim, Player attacker, int drawCount, int turnsSkipped) =>
        game.PassPenalty(victim, game.PlayerAfter(victim), drawCount, turnsSkipped);
}

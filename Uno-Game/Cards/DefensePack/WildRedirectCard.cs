using Uno_Game.Core;
using Uno_Game.Players;

namespace Uno_Game.Cards.DefensePack;

/// <summary>Redirects a Draw penalty to a player of your choice.</summary>
public class WildRedirectCard : DefenseCard
{
    protected override string Name => "Wild redirect";

    public override bool CanAnswer(int drawCount) => drawCount > 0;

    public override void Answer(Game game, Player victim, Player attacker, int drawCount, int turnsSkipped) =>
        game.PassPenalty(victim, game.ChooseRedirectTarget(victim), drawCount, turnsSkipped);
}

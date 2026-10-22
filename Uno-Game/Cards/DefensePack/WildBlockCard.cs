using Uno_Game.Core;
using Uno_Game.Players;

namespace Uno_Game.Cards.DefensePack;

/// <summary>Blocks a Draw or Skip penalty.</summary>
public class WildBlockCard : DefenseCard
{
    protected override string Name => "Wild block";

    public override bool CanAnswer(int drawCount) => true;

    public override void Answer(Game game, Player victim, Player attacker, int drawCount, int turnsSkipped) =>
        game.BlockPenalty(victim);
}

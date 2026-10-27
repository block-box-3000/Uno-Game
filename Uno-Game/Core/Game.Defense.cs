using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Cards.DefensePack;
using Uno_Game.Players;
using Uno_Game.UI;

namespace Uno_Game.Core
{
    // ---------------------------------------------------------------- Defense pack
    public partial class Game
    {
        /// <summary>
        /// A Draw or Skip penalty falls on <paramref name="victim"/>, who may answer with a defense card
        /// (<see cref="DefenseCard"/>). The card is played out of turn: it becomes the top card and the victim chooses the
        /// color, but whose turn it is does not change, so play resumes from the current position (the victim, no longer
        /// penalized, plays next in the normal order). A penalty passed on by a defense card is final (see
        /// <see cref="PassPenalty"/>). Returns true if the penalty was dealt with.
        /// </summary>
        private bool TryDefend(Player attacker, Player victim, int drawCount, int turnsSkipped)
        {
            List<DefenseCard> options = victim.Hand
                .OfType<DefenseCard>()
                .Where(card => card.CanAnswer(drawCount))
                .ToList();
            if (options.Count == 0)
                return false;

            DefenseCard? defense = victim.ChooseDefense(this, options, attacker, drawCount, turnsSkipped);
            if (defense == null)
                return false;
            if (!options.Contains(defense))
                throw new InvalidOperationException($"{victim.Name} chose a defense card that can't answer this penalty: {defense}");

            ui.Show($"{victim.Name} answers with {defense}!", MessageStyle.Event);
            ui.Pause(ShortDelayMs);

            //The victim plays the card, so they are the one who chooses the color (after PlayCard, which resets it)
            int turnOwner = currentIndex;
            currentIndex = victim.Index;
            PlayCard(victim, defense, announce: false);
            if (HasWon(victim))
                return true;

            ChooseColor();
            currentIndex = turnOwner;

            defense.Answer(this, victim, attacker, drawCount, turnsSkipped);
            return true;
        }

        /// <summary>
        /// A defense card moves a penalty from <paramref name="from"/> to <paramref name="to"/>: the new target draws the cards
        /// and loses their next turn. It is final: no defense card and no Wild No U can answer a passed penalty.
        /// </summary>
        public void PassPenalty(Player from, Player to, int drawCount, int turnsSkipped)
        {
            ui.Show($"The penalty goes to {to.Name}", MessageStyle.Event);

            if (drawCount > 0 && !ForceDraw(to, from, drawCount))
                return;

            if (turnsSkipped > 0)
            {
                skippedPlayers.Add(to);
                ui.Show($"{to.Name} will lose their next turn", MessageStyle.Event);
            }
        }

        public void BlockPenalty(Player victim) =>
            ui.Show($"{victim.Name} blocks the penalty", MessageStyle.Event);

        /// <summary>The next player after <paramref name="player"/> in the current order.</summary>
        public Player PlayerAfter(Player player) => players[WrapIndex(player.Index + Direction)];

        /// <summary>Wild Redirect: the victim picks any other player to receive the penalty.</summary>
        public Player ChooseRedirectTarget(Player victim)
        {
            List<Player> candidates = players.Where(player => player != victim).ToList();
            Player target = victim.ChooseRedirectTarget(this, candidates);

            if (!candidates.Contains(target))
                throw new InvalidOperationException($"{victim.Name} chose an invalid target.");

            return target;
        }

    }
}

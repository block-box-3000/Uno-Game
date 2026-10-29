using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Cards.FrenemiesPack;
using Uno_Game.Cards;
using Uno_Game.Players;
using Uno_Game.UI;

namespace Uno_Game.Core
{
    // ---------------------------------------------------------------- Frenemies pack
    public partial class Game
    {
        private const int PettyPlayCards = 3;      //Cards drawn because of a Wild Petty Play
        private const int NemesisExtraCards = 2;   //Extra cards drawn by the enemy of a Wild Nemesis

        /// <summary>Wild Petty Play played on your turn: the chosen player draws 3 cards.</summary>
        public void PettyPlay()
        {
            Player player = CurrentPlayer;
            Player target = player.ChoosePettyPlayTarget(this);

            if (target == player || !players.Contains(target))
                throw new InvalidOperationException($"{player.Name} chose an invalid target.");

            ui.Show($"{player.Name} picks {target.Name} for a petty play", MessageStyle.Event);
            ForceDraw(target, player, PettyPlayCards);
        }

        /// <summary>
        /// Wild Nemesis: the current player names another player as their enemy. Every time they make that
        /// player draw cards, the enemy draws 2 extra cards, until the owner ends the beef
        /// (see <see cref="AskToEndNemeses"/>). A player can have several enemies.
        /// </summary>
        public void SetNemesis()
        {
            Player owner = CurrentPlayer;
            nemeses.TryGetValue(owner, out List<Player>? current);

            List<Player> candidates = players.Where(player => player != owner && (current == null || !current.Contains(player))).ToList();
            if (candidates.Count == 0)
            {
                ui.Show($"{owner.Name} already has everybody as an enemy", MessageStyle.Event);
                return;
            }

            Player enemy = owner.ChooseEnemy(this, candidates);
            if (!candidates.Contains(enemy))
                throw new InvalidOperationException($"{owner.Name} chose an invalid enemy.");

            if (current == null)
                nemeses[owner] = current = new List<Player>();
            current.Add(enemy);

            ui.Show($"{owner.Name} names {enemy.Name} as their nemesis: every time {owner.Name} makes {enemy.Name} draw, " +
                    $"{enemy.Name} draws {NemesisExtraCards} extra cards", MessageStyle.Event);
        }

        /// <summary>At the start of their round, the owner of a Wild Nemesis decides for each enemy whether the beef is over.</summary>
        private void AskToEndNemeses(Player owner)
        {
            if (!nemeses.TryGetValue(owner, out List<Player>? enemies))
                return;

            foreach (Player enemy in enemies.ToList())
            {
                if (!owner.ShouldEndNemesis(this, enemy))
                    continue;

                enemies.Remove(enemy);
                ui.Show($"{owner.Name} ends the beef with {enemy.Name}", MessageStyle.Event);
            }

            if (enemies.Count == 0)
                nemeses.Remove(owner);
        }

        /// <summary>Wild Eliminator: every opponent discards all their wild cards.</summary>
        public void EliminateWilds()
        {
            Player eliminator = CurrentPlayer;
            ui.Show($"{eliminator.Name} plays Wild Eliminator! Every opponent discards their wild cards", MessageStyle.Event);

            for (int k = 1; k < players.Count; k++)
            {
                Player other = players[WrapIndex(currentIndex + k * Direction)];
                List<Card> wilds = other.Hand.Where(card => card.IsWild).ToList();
                if (wilds.Count == 0)
                    continue;

                foreach (Card wild in wilds)
                {
                    other.Hand.Remove(wild);
                    Deck.Discard(wild);
                }

                //Humans see exactly which cards were discarded; for the rest it stays secret
                if (other.IsHuman || eliminator.IsHuman)
                    ui.Show($"{(other.IsHuman ? "You discard" : other.Name + " discards")} {string.Join(", ", wilds)}", MessageStyle.Event);
                else
                    ui.Show($"{other.Name} discards {wilds.Count} wild card(s)", MessageStyle.Event);
            }

            CheckEmptyHands();
        }

        /// <summary>Wild Instigator: chosen opponents draw from 1 to 4 cards each (in turn order).</summary>
        public void Instigate()
        {
            Player instigator = CurrentPlayer;
            IReadOnlyList<(Player Target, int Cards)> choices = instigator.ChooseInstigatorTargets(this);

            bool valid = choices.All(choice => choice.Target != instigator && players.Contains(choice.Target) && choice.Cards >= 1 && choice.Cards <= 4)
                         && choices.Select(choice => choice.Target).Distinct().Count() == choices.Count;
            if (!valid)
                throw new InvalidOperationException($"{instigator.Name} made an invalid instigation.");

            if (choices.Count == 0)
            {
                ui.Show($"{instigator.Name} instigates nobody", MessageStyle.Event);
                return;
            }

            ui.Show($"{instigator.Name} plays Wild Instigator!", MessageStyle.Event);

            for (int k = 1; k < players.Count; k++)
            {
                Player target = players[WrapIndex(currentIndex + k * Direction)];
                foreach (var choice in choices.Where(choice => choice.Target == target))
                {
                    if (!ForceDraw(target, instigator, choice.Cards))
                        return;
                }
            }
        }

        /// <summary>
        /// A player makes ANOTHER player draw cards. Every forced draw goes through here, so the Frenemies cards
        /// work with any penalty (Draw cards, stacks, color challenge...):
        /// - Wild Nemesis: the victim draws 2 extra cards if the source named them as their enemy.
        /// - Wild Petty Play: the victim may answer out of turn, making the source draw 3 cards (the answer can be
        ///   answered back). It goes to the discard pile, so it changes neither the top card, the color nor whose turn it is:
        ///   play resumes from the current position.
        /// Returns false if the game ended on the way (the victim played their last card).
        /// </summary>
        private bool ForceDraw(Player victim, Player source, int count, bool announce = true)
        {
            WildPettyPlayCard? pettyPlay = victim.Hand.OfType<WildPettyPlayCard>().FirstOrDefault();
            if (pettyPlay != null && victim.ShouldPlayPettyPlay(this, source, count))
            {
                victim.Hand.Remove(pettyPlay);
                Deck.Discard(pettyPlay);
                ui.Show($"{victim.Name} plays a Wild Petty Play Card out of turn!", MessageStyle.Event);
                ui.Pause(ShortDelayMs);

                if (victim.Hand.Count == 0)
                {
                    DeclareWinner(victim);
                    return false;
                }

                if (!ForceDraw(source, victim, PettyPlayCards))
                    return false;
            }

            if (announce)
                ui.Show($"{victim.Name} draws {count} card(s)", MessageStyle.Event);

            int extra = nemeses.TryGetValue(source, out List<Player>? enemies) && enemies.Contains(victim) ? NemesisExtraCards : 0;
            if (extra > 0)
                ui.Show($"{source.Name} is {victim.Name}'s nemesis: {extra} extra card(s)", MessageStyle.Event);

            DrawCards(victim, count + extra);
            return true;
        }

    }
}

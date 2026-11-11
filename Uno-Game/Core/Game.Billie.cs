using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Cards;
using Uno_Game.Players;
using Uno_Game.UI;

namespace Uno_Game.Core
{
    // ---------------------------------------------------------------- Billie pack

    public partial class Game
    {
        // Billie pack
        private Player? handsOwner;     //Reverse Hands: hands stay face up until this player starts their next turn
        private Player? trickOwner;     //Trick Wild Card: who played it...
        private Player? trickVictim;    //...and the player who must answer it
        private readonly Dictionary<Player, List<Player>> nemeses = new();   //Frenemies: owner of a Wild Nemesis -> their enemies
        private readonly HashSet<Player> skippedPlayers = new();   //Defense: players who lose their next turn because a penalty was passed to them
        private Player? previousTurnPlayer;     //Who played the previous turn (to detect the start of a new round for a player)
        private Player? forcedNext;     //Who plays right after the current turn, overriding the normal order

        /// <summary>
        /// Wild Pass Color: every player passes all their cards of the chosen color to the player on their left
        /// (the next one in clockwise order, whatever the current direction). Players without that color pass
        /// nothing but still receive the cards passed to them.
        /// </summary>
        public void PassColor()
        {
            CardColor color = CurrentColor;
            int count = players.Count;

            List<List<Card>> passed = players.Select(player => player.Hand.Where(card => card.Color == color).ToList()).ToList();

            foreach (Player player in players)
                player.Hand.RemoveAll(card => card.Color == color);

            for (int i = 0; i < count; i++)
                players[(i + 1) % count].Hand.AddRange(passed[i]);

            for (int i = 0; i < count; i++)
            {
                Player giver = players[i];
                Player receiver = players[(i + 1) % count];
                if (passed[i].Count == 0)
                    continue;

                //Humans see exactly which cards moved; for the rest it stays secret
                if (giver.IsHuman)
                    ui.Show($"You pass {string.Join(", ", passed[i])} to {receiver.Name}", MessageStyle.Event);
                else if (receiver.IsHuman)
                    ui.Show($"{giver.Name} passes you {string.Join(", ", passed[i])}", MessageStyle.Event);
                else
                    ui.Show($"{giver.Name} passes {passed[i].Count} {color} card(s) to {receiver.Name}", MessageStyle.Event);
            }

            CheckEmptyHands();
        }

        /// <summary>A hand that ends up empty wins (starting with the current player).</summary>
        private void CheckEmptyHands()
        {
            for (int k = 0; k < players.Count; k++)
            {
                Player player = players[WrapIndex(currentIndex + k)];
                if (player.Hand.Count == 1)
                    AnnounceUno(player);
                if (player.Hand.Count == 0)
                {
                    DeclareWinner(player);
                    return;
                }
            }
        }

        /// <summary>
        /// Trick Wild Card: every time the next player can't play, the current player plays again,
        /// until that player manages to play (see <see cref="ResolveTrick"/>).
        /// </summary>
        public void SetTrick()
        {
            trickOwner = CurrentPlayer;
            trickVictim = NextPlayer();
            ui.Show($"Trick! {trickOwner.Name} keeps playing until {trickVictim.Name} can play a {CurrentColor} card", MessageStyle.Event);
        }

        /// <summary>
        /// The victim of a Trick Wild Card had nothing to play and already drew. If they drew a single card and
        /// it can be played, they play it and the trick is over. Otherwise the owner plays again and the trick
        /// stays active: it repeats every round until the victim manages to play. Returns true if the game is over.
        /// </summary>
        private bool ResolveTrick(Player victim, Player owner, int drawn)
        {
            if (drawn == 1)
            {
                Card card = victim.Hand[^1];
                if (IsPlayable(card))
                {
                    //Cleared first: the card they play may start a new trick
                    ClearTrick();
                    ui.Show($"{victim.Name} draws {card}, which can be played", MessageStyle.Event);
                    PlayCard(victim, card, announce: true);
                    if (HasWon(victim))
                        return true;

                    card.Apply(this);
                    return Winner != null;
                }
            }

            ui.Show($"{owner.Name} plays again (still waiting for {victim.Name})", MessageStyle.Event);
            forcedNext = owner;
            return false;
        }

        private void ClearTrick()
        {
            trickOwner = null;
            trickVictim = null;
        }

        /// <summary>
        /// Wild Color Challenge: every other player must show a card of the chosen color. Whoever can't draws 2 cards;
        /// if everybody shows one, the current player draws 3.
        /// </summary>
        public void ColorChallenge()
        {
            Player challenger = CurrentPlayer;
            CardColor color = CurrentColor;
            bool everybodyShowed = true;

            ui.Show($"Color challenge! Everybody shows a {color} card", MessageStyle.Event);

            for (int k = 1; k < players.Count; k++)
            {
                Player other = players[WrapIndex(currentIndex + k * Direction)];
                Card? shown = other.Hand.FirstOrDefault(card => card.Color == color);

                if (shown == null)
                {
                    everybodyShowed = false;
                    ui.Show($"{other.Name} has no {color} card and draws 2 cards", MessageStyle.Event);
                    if (!ForceDraw(other, challenger, 2, announce: false))
                        return;
                }
                else if (other.IsHuman)
                {
                    ui.Show($"You show {shown}", MessageStyle.Event);
                }
                else
                {
                    //Only the challenger gets to see the card
                    string text = challenger.IsHuman ? shown.ToString() : $"a {color} card";
                    ui.Show($"{other.Name} shows {text}", MessageStyle.Event);
                }
            }

            if (everybodyShowed)
            {
                ui.Show($"Everybody showed a {color} card: {challenger.Name} draws 3 cards", MessageStyle.Event);
                DrawCards(challenger, 3);
            }
        }

        /// <summary>
        /// Reverse Hands: every hand is shown face up at the start of each turn until the player
        /// who played the card starts their next turn.
        /// </summary>
        public void RevealHands()
        {
            handsOwner = CurrentPlayer;
            ui.Show($"{handsOwner.Name} plays Reverse Hands! Everybody lays their hand face up until {handsOwner.Name}'s next turn",
                MessageStyle.Event);
        }

        private void ShowRevealedHands() =>
            ui.ShowHands(players.Select(player => (player.Name, (IReadOnlyList<Card>)player.Hand.ToList())).ToList());

    }
}

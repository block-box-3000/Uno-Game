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
    // ---------------------------------------------------------------- Comeback pack
    public partial class Game
    {
        /// <summary>Wild Swap Hands: the current player swaps hands with any other player.</summary>
        public void SwapHands()
        {
            Player player = CurrentPlayer;
            Player target = player.ChooseSwapHandsTarget(this);

            if (target == player || !players.Contains(target))
                throw new InvalidOperationException($"{player.Name} chose an invalid player to swap hands with.");

            List<Card> mine = player.Hand.ToList();
            player.Hand.Clear();
            player.Hand.AddRange(target.Hand);
            target.Hand.Clear();
            target.Hand.AddRange(mine);

            //Humans see the cards they get; for the rest it stays secret
            if (player.IsHuman)
                ui.Show($"You swap hands with {target.Name} and get: {string.Join(", ", player.Hand)}", MessageStyle.Event);
            else if (target.IsHuman)
                ui.Show($"{player.Name} swaps hands with you: you get {string.Join(", ", target.Hand)}", MessageStyle.Event);
            else
                ui.Show($"{player.Name} swaps hands with {target.Name}", MessageStyle.Event);
        }

        /// <summary>
        /// Wild Double Down (its color is already chosen): the player discards an additional card on top of it.
        /// That card is played normally on the chosen color and has its effect (draw, skip...), but the game keeps the
        /// color chosen with the Double Down, even if the additional card is wild. If there is no card to play, nothing happens.
        /// </summary>
        public void DoubleDown()
        {
            Player player = CurrentPlayer;
            CardColor chosen = CurrentColor;

            List<Card> playable = player.Hand.Where(IsPlayable).ToList();
            if (playable.Count == 0)
            {
                ui.Show($"{player.Name} has no card to discard on top of it", MessageStyle.Event);
                return;
            }

            Card extra = player.ChooseCard(this, playable);
            if (!playable.Contains(extra))
                throw new InvalidOperationException($"{player.Name} chose a card that can't be played: {extra}");

            PlayCard(player, extra, announce: true);
            CurrentColor = chosen;
            if (HasWon(player))
                return;

            colorLocked = true;
            try
            {
                extra.Apply(this);
            }
            finally
            {
                colorLocked = false;
            }
        }

        /// <summary>Wild Match the Max: the chosen player draws until their hand is as big as the largest hand.</summary>
        public void MatchTheMax()
        {
            Player player = CurrentPlayer;
            Player target = player.ChooseMatchTarget(this);

            if (target == player || !players.Contains(target))
                throw new InvalidOperationException($"{player.Name} chose an invalid player.");

            int missing = LargestHand() - target.Hand.Count;
            if (missing <= 0)
            {
                ui.Show($"{target.Name} already has the largest hand", MessageStyle.Event);
                return;
            }

            ForceDraw(target, player, missing);
        }

        /// <summary>Wild Misery Loves Company: every other player draws until their hand is as big as the largest hand.</summary>
        public void MiseryLovesCompany()
        {
            Player player = CurrentPlayer;
            int largest = LargestHand();   //Computed once, before anyone draws

            for (int k = 1; k < players.Count; k++)
            {
                Player other = players[WrapIndex(currentIndex + k * Direction)];
                int missing = largest - other.Hand.Count;

                if (missing <= 0)
                {
                    ui.Show($"{other.Name} already has the largest hand", MessageStyle.Event);
                    continue;
                }

                if (!ForceDraw(other, player, missing))
                    return;
            }
        }

        private int LargestHand() => players.Max(player => player.Hand.Count);

        /// <summary>
        /// U-N-O rule: a player holding a U, an N and an O card (and at least one more card) may, at the start of their turn,
        /// discard down to one card for an instant UNO. All the other cards go to the discard pile under the three U-N-O cards,
        /// whose actions are not activated, and the player chooses the next color. It takes the whole turn.
        /// Returns true if the rule was used.
        /// </summary>
        private bool TryUnoRule(Player player)
        {
            Card? u = player.Hand.FirstOrDefault(card => card.UnoLetter == 'U');
            Card? n = player.Hand.FirstOrDefault(card => card.UnoLetter == 'N');
            Card? o = player.Hand.FirstOrDefault(card => card.UnoLetter == 'O');
            if (u == null || n == null || o == null)
                return false;

            //The card to keep can't be one of the three that spell U-N-O
            List<Card> candidates = player.Hand.Where(card => card != u && card != n && card != o).ToList();
            if (candidates.Count == 0)
                return false;

            Card? keep = player.ChooseUnoKeep(this, candidates);
            if (keep == null)
                return false;
            if (!candidates.Contains(keep))
                throw new InvalidOperationException($"{player.Name} chose an invalid card to keep: {keep}");

            ui.Show($"{player.Name} spells U-N-O and discards down to one card!", MessageStyle.Event);

            foreach (Card card in player.Hand.Where(card => card != keep && card != u && card != n && card != o).ToList())
            {
                player.Hand.Remove(card);
                Deck.Discard(card);
            }

            //The three U-N-O cards end up on top, so the last one is the top card of the table
            Deck.Discard(TopCard);
            player.Hand.Remove(u);
            player.Hand.Remove(n);
            player.Hand.Remove(o);
            Deck.Discard(u);
            Deck.Discard(n);
            TopCard = o;
            CurrentColor = CardColor.Wild;
            colorChosen = false;
            ChooseColor();

            ui.Show($"{player.Name} says: UNO!", MessageStyle.Event);
            return true;
        }

    }
}

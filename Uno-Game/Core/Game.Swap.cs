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
    // ---------------------------------------------------------------- Swap pack
    public partial class Game
    {
        /// <summary>Swap 1: the current player takes a random card from another player and gives them one of theirs.</summary>
        public void SwapCardWithPlayer()
        {
            Player player = CurrentPlayer;
            SwapChoice choice = player.ChooseSwap(this);
            Player target = choice.Target;
            Card given = choice.CardToGive;

            if (target == player || !players.Contains(target) || !player.Hand.Contains(given))
                throw new InvalidOperationException($"{player.Name} made an invalid swap.");

            Card taken = target.Hand[rng.Next(target.Hand.Count)];

            player.Hand.Remove(given);
            target.Hand.Remove(taken);
            player.Hand.Add(taken);
            target.Hand.Add(given);

            //Humans see which cards moved; for the rest it stays secret
            if (player.IsHuman)
                ui.Show($"You gave {given} to {target.Name} and took {taken}", MessageStyle.Event);
            else if (target.IsHuman)
                ui.Show($"{player.Name} took your {taken} and gave you {given}", MessageStyle.Event);
            else
                ui.Show($"{player.Name} swaps a card with {target.Name}", MessageStyle.Event);
        }

        /// <summary>Refresh Hand: the whole hand goes to the discard pile and the player draws the same number of cards.</summary>
        public void RefreshHand()
        {
            Player player = CurrentPlayer;
            int count = player.Hand.Count;

            foreach (Card card in player.Hand)
                Deck.Discard(card);
            player.Hand.Clear();

            DrawCards(player, count);
            ui.Show($"{player.Name} refreshes their hand ({count} card(s))", MessageStyle.Event);
        }

        /// <summary>Wild Force Trade: the current player picks two different players and they swap hands.</summary>
        public void TradeHands()
        {
            (Player first, Player second) = CurrentPlayer.ChooseTradePlayers(this);

            if (first == second || !players.Contains(first) || !players.Contains(second))
                throw new InvalidOperationException($"{CurrentPlayer.Name} chose an invalid pair of players.");

            List<Card> firstHand = first.Hand.ToList();
            first.Hand.Clear();
            first.Hand.AddRange(second.Hand);
            second.Hand.Clear();
            second.Hand.AddRange(firstHand);

            ui.Show($"{first.Name} and {second.Name} swap hands", MessageStyle.Event);
        }

        /// <summary>Wild Pass Hands: every player passes their hand to the next player in the current direction.</summary>
        public void PassHands()
        {
            List<List<Card>> hands = players.Select(player => player.Hand.ToList()).ToList();

            for (int i = 0; i < players.Count; i++)
            {
                Player receiver = players[WrapIndex(i + Direction)];
                receiver.Hand.Clear();
                receiver.Hand.AddRange(hands[i]);
            }

            string flow = Direction == 1 ? "clockwise" : "anticlockwise";
            ui.Show($"Everybody passes their hand to the next player ({flow})", MessageStyle.Event);
        }

    }
}

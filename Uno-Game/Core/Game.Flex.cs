using System;
using System.Collections.Generic;
using System.Linq;
using Uno_Game.Cards;
using Uno_Game.Cards.FlexVersion;
using Uno_Game.Players;
using Uno_Game.UI;

namespace Uno_Game.Core
{
    // ---------------------------------------------------------------- Uno Flex
    public partial class Game
    {
        private const int WildDrawFourCards = 4;

        //Players whose Power Card shows the red X (the flex side can't be used). Everybody starts with the green check.
        private readonly HashSet<Player> powerOff = new();

        /// <summary>Uno Flex: can this player play the flex side of a card? (Their Power Card shows the green check.)</summary>
        public bool HasPower(Player player) => Version == GameVersions.Flex && !powerOff.Contains(player);

        /// <summary>A flex card can match the color in play with its flex side if the player whose turn it is has their Power Card on.</summary>
        private bool CanMatchWithFlexColor(Card card) =>
            card is FlexCard flex && flex.Regular is NumberCard && HasPower(CurrentPlayer) && flex.Flex.Color == CurrentColor;

        //The normal rule (color, number or symbol), without the flex side
        private bool IsRegularPlayable(Card card) =>
            card.IsWild || card.Color == CurrentColor || (!TopCard.IsWild && card.SharesSymbolWith(TopCard));

        /// <summary>
        /// The player is about to play a flex card: decides which side is played (<see cref="FlexCard.PlayingFlexSide"/>)
        /// and pays the Power Card if it is the flex side:
        /// - number cards use the flex side only when the regular side doesn't match (to match with the flex color);
        /// - action and wild cards use it when the player decides to (<see cref="Player.ShouldUseFlexSide"/>).
        /// </summary>
        private void ChooseFlexSide(Player player, FlexCard card)
        {
            card.PlayingFlexSide = false;
            if (!HasPower(player))
                return;

            if (card.Regular is NumberCard)
            {
                if (!IsRegularPlayable(card))
                    card.PlayingFlexSide = true;
            }
            else
            {
                card.PlayingFlexSide = player.ShouldUseFlexSide(this, card);
            }

            if (card.PlayingFlexSide)
            {
                ui.Show($"{player.Name} plays the FLEX side: {card.FlexDescription}", MessageStyle.Event);
                UsePower(player);
            }
        }

        private void UsePower(Player player)
        {
            powerOff.Add(player);
            ui.Show($"{player.Name} flips their Power Card to red", MessageStyle.Event);
            CheckAllPowersOff();
        }

        /// <summary>A number card with the Flip symbol flips the Power Card of whoever plays it, whatever side is up.</summary>
        private void TogglePower(Player player)
        {
            if (powerOff.Remove(player))
                ui.Show($"{player.Name} flips their Power Card back to green", MessageStyle.Event);
            else
            {
                powerOff.Add(player);
                ui.Show($"{player.Name} flips their Power Card to red", MessageStyle.Event);
            }

            CheckAllPowersOff();
        }

        /// <summary>Wild All Flip: every player flips their Power Card to the other side.</summary>
        public void FlipAllPowerCards()
        {
            foreach (Player player in players)
            {
                if (!powerOff.Remove(player))
                    powerOff.Add(player);
            }

            ui.Show("Everybody flips their Power Card", MessageStyle.Event);
        }

        //When every Power Card is red, everybody flips them back to green
        private void CheckAllPowersOff()
        {
            if (powerOff.Count < players.Count)
                return;

            powerOff.Clear();
            ui.Show("Every Power Card is red: everybody flips them back to green", MessageStyle.Event);
        }

        /// <summary>Flex side of Flex Draw 2 / Wild Flex All Draw: every other player draws cards. Nobody loses their turn.</summary>
        public void AllDraw(int cards)
        {
            Player player = CurrentPlayer;
            ui.Show($"{player.Name} makes everybody else draw {cards} card(s)", MessageStyle.Event);

            for (int k = 1; k < players.Count; k++)
            {
                Player other = players[WrapIndex(currentIndex + k * Direction)];
                if (!ForceDraw(other, player, cards))
                    return;
            }
        }

        /// <summary>Flex side of Wild Flex Draw 2 / Draw 4: the player chooses who draws. Nobody loses their turn.</summary>
        public void TargetDraw(int cards)
        {
            Player player = CurrentPlayer;
            Player target = player.ChooseFlexTarget(this, cards);

            if (target == player || !players.Contains(target))
                throw new InvalidOperationException($"{player.Name} chose an invalid target.");

            ForceDraw(target, player, cards);
        }

        /// <summary>
        /// Regular side of the Wild Flex Draw 4: the next player draws 4 and misses their turn unless they challenge
        /// (see <see cref="ResolveWildDraw"/>). Here any Wild card in the hand counts as a match for the color in play.
        /// </summary>
        public void WildDrawFour() =>
            ResolveWildDraw("Wild Draw 4", player =>
            {
                ui.Show($"{player.Name} draws {WildDrawFourCards} card(s)", MessageStyle.Event);
                DrawCards(player, WildDrawFourCards);
            });
    }
}

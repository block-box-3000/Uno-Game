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
    // ---------------------------------------------------------------- Teams version
    public partial class Game
    {
        private readonly Dictionary<Player, Player> teammates = new();

        /// <summary>Returns the partner assigned to a player in the current Teams match.</summary>
        public Player? TeammateOf(Player player) => teammates.TryGetValue(player, out Player? teammate) ? teammate : null;

        private void AssignTeammates()
        {
            int playersPerTeam = players.Count / 2;
            for (int i = 0; i < playersPerTeam; i++)
            {
                Player player = players[i];
                Player teammate = players[i + playersPerTeam];

                teammates.Add(player, teammate);
                teammates.Add(teammate, player);
            }
        }

        /// <summary>Collects each pair's initial pass before moving any cards between hands.</summary>
        private void PassInitialTeamCards()
        {
            var passes = new List<(Player Giver, Player Receiver, Card Card)>();
            foreach (Player player in players)
            {
                Player teammate = GetTeammate(player);
                if (player.Index < teammate.Index)
                {
                    IReadOnlyList<Card> cards = player.ChooseTeamCardsToGive(this, teammate, 1);
                    if (cards.Count != 1 || !player.Hand.Contains(cards[0]))
                        throw new InvalidOperationException($"{player.Name} chose an invalid card to pass to {teammate.Name}.");
                    passes.Add((player, teammate, cards[0]));
                }
            }

            foreach ((Player giver, Player receiver, Card card) in passes)
            {
                giver.Hand.Remove(card);
                receiver.Hand.Add(card);
                ui.Show($"{giver.Name} passes a card to {receiver.Name}", MessageStyle.Event);
            }

            ui.Show("");
            ui.Pause(ShortDelayMs);
        }

        /// <summary>The current player and their teammate each choose one card to exchange.</summary>
        public void SwapCardWithTeammate()
        {
            if (Version != GameVersions.Teams)
                throw new InvalidOperationException("This card can only be used in Uno Teams.");

            Player player = CurrentPlayer;
            Player teammate = GetTeammate(player);
            if (player.Hand.Count == 0 || teammate.Hand.Count == 0)
            {
                ui.Show("The card cannot be exchanged because one of the teammates has no cards", MessageStyle.Event);
                return;
            }

            IReadOnlyList<Card> playerChoices = player.ChooseTeamCardsToGive(this, teammate, 1);
            IReadOnlyList<Card> teammateChoices = teammate.ChooseTeamCardsToGive(this, player, 1);
            if (playerChoices.Count != 1 || !player.Hand.Contains(playerChoices[0]))
                throw new InvalidOperationException($"{player.Name} chose an invalid card to exchange.");
            if (teammateChoices.Count != 1 || !teammate.Hand.Contains(teammateChoices[0]))
                throw new InvalidOperationException($"{teammate.Name} chose an invalid card to exchange.");

            (Card playerCard, Card teammateCard) = (playerChoices[0], teammateChoices[0]);
            player.Hand.Remove(playerCard);
            teammate.Hand.Remove(teammateCard);
            player.Hand.Add(teammateCard);
            teammate.Hand.Add(playerCard);

            ui.Show($"{player.Name} and {teammate.Name} exchange one card", MessageStyle.Event);
        }

        /// <summary>The next player and their teammate each draw half the card's draw count; the next player is skipped.</summary>
        public void WildTeamDraw(int drawCount)
        {
            if (Version != GameVersions.Teams)
                throw new InvalidOperationException("This card can only be used in Uno Teams.");
            if (drawCount <= 0 || drawCount % 2 != 0)
                throw new ArgumentOutOfRangeException(nameof(drawCount), "The draw count must be a positive even number.");

            Player nextPlayer = NextPlayer();
            Player teammate = GetTeammate(nextPlayer);
            int cardsPerPlayer = drawCount / 2;

            DrawCards(nextPlayer, cardsPerPlayer);
            DrawCards(teammate, cardsPerPlayer);
            SkipNextPlayers(1);

            ui.Show(
                $"{nextPlayer.Name} and {teammate.Name} each draw {cardsPerPlayer} card(s)",
                MessageStyle.Event);
            ui.Show($"{nextPlayer.Name} loses their turn", MessageStyle.Event);
        }

        /// <summary>Chooses a color and sends the next turn directly to the current player's teammate.</summary>
        public void WildTeamPlay()
        {
            if (Version != GameVersions.Teams)
                throw new InvalidOperationException("This card can only be used in Uno Teams.");

            Player player = CurrentPlayer;
            Player teammate = GetTeammate(player);
            ChooseColor();

            if (teammate.Hand.Any(IsPlayable))
            {
                forcedNext = teammate;
                ui.Show($"Play skips directly to {teammate.Name}", MessageStyle.Event);
                return;
            }

            DrawCards(player, 2);
            forcedNext = players[WrapIndex(teammate.Index + Direction)];
            ui.Show(
                $"{teammate.Name} has no playable cards. {player.Name} draws 2 cards and {teammate.Name} loses their turn",
                MessageStyle.Event);
        }


        /// <summary>Gets the assigned partner or throws if the player has no Teams assignment.</summary>
        private Player GetTeammate(Player player) =>
            teammates.TryGetValue(player, out Player? teammate)
                ? teammate
                : throw new InvalidOperationException($"{player.Name} has no teammate.");
    }
}

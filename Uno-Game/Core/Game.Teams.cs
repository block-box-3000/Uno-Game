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
        public Player? TeammateOf(Player player) => teammates.TryGetValue(player, out Player? teammate) ? teammate : null;

        private void AssignTeammates()
        {
            var remaining = players.ToList();
            while (remaining.Count > 0)
            {
                Player player = remaining[0];
                List<Player> candidates = remaining
                    .Skip(1)
                    .Where(candidate => !AreAdjacent(player, candidate))
                    .Where(candidate => CanPairRemaining(remaining
                        .Where(other => other != player && other != candidate)
                        .ToList()))
                    .ToList();

                if (candidates.Count == 0)
                    throw new InvalidOperationException("The remaining players cannot be paired without adjacent teammates.");

                Player teammate = candidates.Count > 1 ? 
                    player.ChooseTeammate(candidates) : candidates[0];
                if (!candidates.Contains(teammate))
                    throw new InvalidOperationException($"{player.Name} chose an invalid teammate.");

                teammates.Add(player, teammate);
                teammates.Add(teammate, player);
                remaining.Remove(player);
                remaining.Remove(teammate);
            }
        }

        private bool AreAdjacent(Player first, Player second)
        {
            int distance = Math.Abs(first.Index - second.Index);
            return distance == 1 || distance == players.Count - 1;
        }

        private bool CanPairRemaining(IReadOnlyList<Player> remaining)
        {
            if (remaining.Count == 0)
                return true;

            Player player = remaining[0];
            for (int i = 1; i < remaining.Count; i++)
            {
                Player candidate = remaining[i];
                if (AreAdjacent(player, candidate))
                    continue;

                List<Player> next = remaining
                    .Where(other => other != player && other != candidate)
                    .ToList();
                if (CanPairRemaining(next))
                    return true;
            }

            return false;
        }

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

        private Player GetTeammate(Player player) =>
            teammates.TryGetValue(player, out Player? teammate)
                ? teammate
                : throw new InvalidOperationException($"{player.Name} has no teammate.");
    }
}

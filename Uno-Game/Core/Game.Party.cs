using Uno_Game.Cards;
using Uno_Game.Players;
using Uno_Game.UI;

namespace Uno_Game.Core
{
    public partial class Game
    {
        private readonly List<(Player First, Player Second)> playerConnections = new();

        public void PointTakenDrawCards()
        {
            ClearPlayerConnections();

            ui.Show("3", MessageStyle.Event);
            ui.Pause(1000);
            ui.Show("2", MessageStyle.Event);
            ui.Pause(1000);
            ui.Show("1", MessageStyle.Event);
            ui.Pause(1000);
            ui.Show("Everyone point at another player!", MessageStyle.Event);
            ui.Pause(1000);

            var votes = players.ToDictionary(player => player, _ => 0);
            foreach (Player pointingPlayer in players)
            {
                List<Player> candidates = players.Where(player => player != pointingPlayer).ToList();
                Player target = pointingPlayer.ChoosePointTakenTarget(this, candidates);

                if (!candidates.Contains(target))
                    throw new InvalidOperationException($"{pointingPlayer.Name} chose an invalid Point Taken target.");

                votes[target]++;
            }

            foreach (Player target in players)
            {
                int cardsToDraw = Math.Min(votes[target], 5);
                if (cardsToDraw == 0)
                    continue;

                ui.Show($"{target.Name} is pointed at by {votes[target]} player(s) and draws {cardsToDraw} card(s).",
                    MessageStyle.Warning);
                DrawCards(target, cardsToDraw);
            }
        }

        public void ConnectPlayers()
        {
            List<Player> available = players
                .Where(player => !playerConnections.Any(connection =>
                    connection.First == player || connection.Second == player))
                .ToList();

            if (available.Count < 2)
            {
                ui.Show("There are not two unconnected players available.", MessageStyle.Event);
                return;
            }

            Player owner = CurrentPlayer;
            (Player first, Player second) = owner.ChoosePlayersToConnect(this, available);
            if (first == second || !available.Contains(first) || !available.Contains(second))
                throw new InvalidOperationException($"{owner.Name} chose invalid players to connect.");

            playerConnections.Add((first, second));
            ui.Show($"{first.Name} and {second.Name} are connected. Whenever one draws, the other draws the same number of cards.",
                MessageStyle.Event);
        }

        private void ClearPlayerConnections()
        {
            if (playerConnections.Count == 0)
                return;

            playerConnections.Clear();
            ui.Show("All player connections end.", MessageStyle.Event);
        }

        private Player? GetConnectedPlayer(Player player)
        {
            foreach ((Player first, Player second) in playerConnections)
            {
                if (first == player)
                    return second;
                if (second == player)
                    return first;
            }

            return null;
        }

        private void MirrorDrawToConnectedPlayer(Player player, int count)
        {
            if (count <= 0 || Version != GameVersions.Party)
                return;

            Player? connected = GetConnectedPlayer(player);
            if (connected == null)
                return;

            ui.Show($"{connected.Name} is connected to {player.Name} and also draws {count} card(s).",
                MessageStyle.Event);
            DrawCardsWithoutConnection(connected, count);
        }

        public void PileUpAction()
        {
            Player initiatingPlayer = CurrentPlayer;
            var miniPile = new List<Card>();
            Card firstCard;

            do
            {
                firstCard = Deck.Draw();
                miniPile.Add(firstCard);
            }
            while (firstCard.IsWild);

            CardColor miniPileColor = firstCard.Color;
            CurrentColor = miniPileColor;
            ui.Show($"{initiatingPlayer.Name} starts a mini pile with {string.Join(", ", miniPile)}. " +
                    $"The pile color is {miniPileColor}.", MessageStyle.Event);

            Player passingPlayer = NextPlayer();
            while (true)
            {
                currentIndex = passingPlayer.Index;
                List<Card> matchingCards = passingPlayer.Hand
                    .Where(card => card.IsWild || card.Color == miniPileColor)
                    .ToList();

                if (matchingCards.Count == 0)
                {
                    passingPlayer.Hand.AddRange(miniPile);
                    ClearUnoAnnouncement(passingPlayer);
                    ui.Show($"{passingPlayer.Name} has no matching card and takes the whole mini pile " +
                            $"({miniPile.Count} card(s)).", MessageStyle.Warning);
                    return;
                }

                Card selectedCard = passingPlayer.ChooseCard(this, matchingCards);
                if (!matchingCards.Contains(selectedCard))
                    throw new InvalidOperationException(
                        $"{passingPlayer.Name} chose a card that does not match the mini pile.");

                passingPlayer.Hand.Remove(selectedCard);
                ClearUnoAnnouncement(passingPlayer);
                miniPile.Add(selectedCard);
                ui.Show($"{passingPlayer.Name} adds {selectedCard} to the mini pile.", MessageStyle.Event);
                ui.Pause(ShortDelayMs);

                if (HasWon(passingPlayer))
                    return;

                if (passingPlayer.Hand.Count == 1)
                {
                    UnoAnnouncedPlayers.Add(passingPlayer);
                    AnnounceUno(passingPlayer);
                }

                passingPlayer = NextPlayer(passingPlayer);
            }
        }
    }
}

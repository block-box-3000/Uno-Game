using Uno_Game.Cards;
using Uno_Game.Cards.LiarsVersion;
using Uno_Game.Players;
using Uno_Game.UI;

namespace Uno_Game.Core;

public partial class Game
{
    private IReadOnlyList<Card>? liarClaimCards;
    private Card? activeLiarCard;

    private IReadOnlyList<Card> LiarClaimCards =>
        liarClaimCards ??= LiarsDeck.CreateClaims().ToList();

    private bool IsPlayableLiarClaim(Card card) =>
        card.IsWild
        || card.Color == CurrentColor
        || (!TopCard.IsWild && card.SharesSymbolWith(TopCard));

    private sealed class WildLiarsChallengeEntry
    {
        public required Player Player { get; init; }
        public required Card Card { get; init; }
        public required Card DeclaredCard { get; init; }
        public bool WasChallenged { get; set; }
        public bool WasCaughtLying { get; set; }
    }

    public void ResolveWildLiarsChallenge()
    {
        Player challenger = CurrentPlayer;
        CardColor claimedColor = CurrentColor;
        var entries = new List<WildLiarsChallengeEntry>();

        foreach (Player player in Players.Where(player => player != challenger))
        {
            IReadOnlyList<Card> matching = player.Hand
                .Where(card => card.Color == claimedColor)
                .ToList();
            Card selected = player.ChooseWildLiarsChallengeCard(this, claimedColor);

            if (!player.Hand.Contains(selected))
                throw new InvalidOperationException($"{player.Name} chose a card that is not in their hand.");

            if (matching.Count > 0 && !matching.Contains(selected))
                throw new InvalidOperationException($"{player.Name} has a card of {claimedColor} and must choose one.");

            player.Hand.Remove(selected);
            ClearUnoAnnouncement(player);
            if (player.Hand.Count == 1)
                AnnounceUno(player);
            selected.IsLiarCard = true;

            Card declaredCard;
            if (matching.Count > 0)
            {
                declaredCard = selected;
            }
            else
            {
                IReadOnlyList<Card> claims = LiarClaimCards
                    .Where(card => card.Color == claimedColor)
                    .ToList();
                declaredCard = player.ChooseWildLiarsChallengeClaim(this, claimedColor, claims);
                if (!claims.Contains(declaredCard))
                    throw new InvalidOperationException($"{player.Name} chose an invalid {claimedColor} claim.");
            }

            entries.Add(new WildLiarsChallengeEntry
            {
                Player = player,
                Card = selected,
                DeclaredCard = declaredCard
            });
            ui.Show($"{player.Name} plays face down and claims {declaredCard.RevealIdentity()}.",
                MessageStyle.Event);
        }

        var challengedPlayers = new HashSet<Player>();

        while (challengedPlayers.Count < entries.Count)
        {
            IReadOnlyList<Player> candidates = entries
                .Where(entry => !challengedPlayers.Contains(entry.Player))
                .Select(entry => entry.Player)
                .ToList();

            Player target = challenger.ChooseWildLiarsChallengeTarget(this, candidates);
            if (!candidates.Contains(target))
                throw new InvalidOperationException($"{challenger.Name} chose an invalid challenge target.");

            challengedPlayers.Add(target);
            WildLiarsChallengeEntry entry = entries.Single(item => item.Player == target);
            entry.WasChallenged = true;
            entry.Card.IsLiarCard = false;

            bool truthful = entry.Card.Color == claimedColor
                && entry.Card.SharesSymbolWith(entry.DeclaredCard);
            ui.Show($"{challenger.Name} challenges {target.Name}. The card is {entry.Card.RevealIdentity()}.",
                truthful ? MessageStyle.Event : MessageStyle.Warning);

            if (!truthful)
            {
                entry.WasCaughtLying = true;
                target.Hand.Add(entry.Card);
                DrawCards(target, 1);
                ui.Show($"{target.Name} was caught lying, takes the card back, and draws 1 card.",
                    MessageStyle.Warning);
                continue;
            }

            ui.Show($"{target.Name} was telling the truth.", MessageStyle.Event);
            break;
        }

        var winnersAtChallenge = new List<Player>();
        foreach (WildLiarsChallengeEntry entry in entries)
        {
            // A player who was caught lying keeps the card. Truthful challenged cards and
            // every card belonging to a player who was not challenged are played face down.
            if (entry.WasCaughtLying)
                continue;

            entry.Card.IsLiarCard = false;
            Deck.Discard(entry.Card);
            ui.Show($"{entry.Player.Name}'s face-down card is added below the Wild Liar's Challenge card.",
                MessageStyle.Event);

            if (entry.Player.Hand.Count == 0)
                winnersAtChallenge.Add(entry.Player);
        }

        if (winnersAtChallenge.Count > 0)
        {
            Winner ??= winnersAtChallenge[0];
            foreach (Player winner in winnersAtChallenge)
            {
                winners.Add(winner);
                ui.Show(winnersAtChallenge.Count == 1
                    ? $"\n{winner.Name} wins!"
                    : $"\n{winner.Name} shares the victory!",
                    MessageStyle.Success);
            }
            return;
        }

        CurrentColor = claimedColor;
    }

    private Card? PlayCardWithLiars(Player player, Card card, bool announce)
    {
        bool mismatchInLiars = Version == GameVersions.Liars
            && !card.IsLiarCard
            && card.Color != CurrentColor
            && (TopCard.IsWild || !card.SharesSymbolWith(TopCard));

        if (Version != GameVersions.Liars || (!card.IsLiarCard && !mismatchInLiars))
        {
            PlayCard(player, card, announce);
            return card;
        }

        List<Card> compatibleClaims = LiarClaimCards
            .Where(IsPlayableLiarClaim)
            .ToList();
        Card declaredCard = player.ChooseLiarClaim(this, compatibleClaims);
        if (!compatibleClaims.Contains(declaredCard) || declaredCard.IsLiarCard)
            throw new InvalidOperationException($"{player.Name} chose an invalid Liar card declaration.");

        int handIndex = player.Hand.IndexOf(card);
        if (handIndex < 0)
            throw new InvalidOperationException($"{player.Name} does not have the selected Liar card.");

        matchedColorInHand = player.Hand.Any(other => other != card && MatchesColorInPlay(other));
        player.Hand.RemoveAt(handIndex);
        ClearUnoAnnouncement(player);
        ui.Show($"{player.Name} plays {(card.IsLiarCard ? "a Liar card" : "a normal card as a Liar card")} " +
            $"and claims {declaredCard}.", MessageStyle.Event);

        Player challenger = NextPlayer();
        bool challenged = challenger.ShouldChallengeLiar(this, player, declaredCard);
        bool truthfulChallenge = false;

        if (challenged)
        {
            truthfulChallenge = card.Color == declaredCard.Color
                && card.SharesSymbolWith(declaredCard);
            ui.Show($"{challenger.Name} challenges the claim. The actual card is " +
                    $"{card.RevealIdentity()}.", MessageStyle.Event);

            if (!truthfulChallenge)
            {
                player.Hand.Insert(handIndex, card);
                int penalty = card.IsLiarCard ? 1 : 6;
                DrawCards(player, penalty);
                ui.Show($"{player.Name} was caught lying. The card returns to their hand, and they draw {penalty} card(s).",
                    MessageStyle.Warning);
                return null;
            }

            DrawCards(challenger, 1);
            ui.Show($"{challenger.Name} challenged a truthful claim and draws 1 card.", MessageStyle.Warning);
        }

        Deck.Discard(activeLiarCard ?? TopCard);
        activeLiarCard = card;
        TopCard = declaredCard;
        CurrentColor = declaredCard.Color;
        colorChosen = false;

        if (!challenged)
            ui.Show($"{challenger.Name} accepts the claim.", MessageStyle.Event);

        return declaredCard;
    }
}

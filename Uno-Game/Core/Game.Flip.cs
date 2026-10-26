using Uno_Game.Cards;
using Uno_Game.Cards.FlipVersion;
using Uno_Game.Players;
using Uno_Game.UI;

namespace Uno_Game.Core;

/// <summary>Uno Flip: the double-sided cards and the rules that change in this version of the game.</summary>
public partial class Game
{
    private const int WildDrawTwoCards = 2;
    private const int FailedChallengeExtraCards = 2;
    private const int MaxColorDraws = 200;   //Safety limit for "draw until you get a color"

    //Did the player who just played a card hold another card of the color that was in play?
    //It decides if a Wild Draw 2 / Wild Draw Color was played legally. Set in PlayCard.
    private bool matchedColorInHand;

    /// <summary>The side of the cards that is up (always Light outside Uno Flip).</summary>
    public CardSide CurrentSide => Deck.Side?.Current ?? CardSide.Light;

    /// <summary>The four colors in play: the dark ones when the dark side is up.</summary>
    public IReadOnlyList<CardColor> PlayableColors =>
        CurrentSide == CardSide.Dark ? CardColors.DarkPlayable : CardColors.Playable;

    /// <summary>
    /// Flip card: everything flips to the other side (every hand and both piles). The discard pile is turned over:
    /// the Flip card goes to the bottom and the card that was at the bottom is the new top card, with its other face up.
    /// </summary>
    public void Flip()
    {
        if (Deck.Side == null)
            throw new InvalidOperationException("Only Uno Flip has double-sided cards.");

        Deck.Side.Flip();
        TopCard = Deck.FlipDiscardPile(TopCard);
        CurrentColor = TopCard.Color;
        colorChosen = false;

        ui.Show($"FLIP! Everything flips to the {CurrentSide} side", MessageStyle.Event);

        //A wild card on top has no color: whoever played the Flip chooses the color that continues play
        if (TopCard.IsWild)
            ChooseColor();
    }

    /// <summary>Skip Everyone (dark side): every other player is skipped, so the current player plays again.</summary>
    public void SkipEveryone()
    {
        repeatTurn = true;
        ui.Show($"Everybody is skipped: {CurrentPlayer.Name} plays again", MessageStyle.Event);
    }

    /// <summary>Wild Draw 2 (light side): the next player draws 2 cards and misses their turn (unless challenged).</summary>
    public void WildDrawTwo() =>
        ResolveWildDraw("Wild Draw 2", player =>
        {
            ui.Show($"{player.Name} draws {WildDrawTwoCards} card(s)", MessageStyle.Event);
            DrawCards(player, WildDrawTwoCards);
        });

    /// <summary>Wild Draw Color (dark side): the next player draws until getting the chosen color and misses their turn.</summary>
    public void WildDrawColor()
    {
        CardColor color = CurrentColor;
        ResolveWildDraw("Wild Draw Color", player => DrawUntilColor(player, color, extraCards: 0));
    }

    /// <summary>
    /// The next player may challenge a Wild Draw card. The challenged player shows their hand:
    /// - If they held a card of the color that was in play, the play was illegal: they take the penalty instead
    ///   and the challenger plays normally.
    /// - If not, the challenger takes the penalty plus 2 more cards and misses their turn.
    /// Without a challenge the next player takes the penalty and misses their turn.
    /// </summary>
    private void ResolveWildDraw(string cardName, Action<Player> penalty)
    {
        Player attacker = CurrentPlayer;
        Player victim = NextPlayer();
        bool illegalPlay = matchedColorInHand;

        if (victim.ShouldChallenge(this, attacker, cardName))
        {
            ui.Show($"{victim.Name} challenges {attacker.Name}! {attacker.Name} shows their hand:", MessageStyle.Event);
            ui.ShowHands(new List<(string PlayerName, IReadOnlyList<Card> Cards)> { (attacker.Name, attacker.Hand.ToList()) });
            ui.Pause(ShortDelayMs);

            if (illegalPlay)
            {
                ui.Show($"{attacker.Name} had a card of the color in play: they take the penalty instead of {victim.Name}", MessageStyle.Event);
                penalty(attacker);
                return;
            }

            ui.Show($"{attacker.Name} played it legally: {victim.Name} takes the penalty plus {FailedChallengeExtraCards} more cards", MessageStyle.Event);
            penalty(victim);
            DrawCards(victim, FailedChallengeExtraCards);
        }
        else
        {
            penalty(victim);
        }

        SkipFrom(victim, 1);
    }

    /// <summary>The player draws cards until one has the given color (plus <paramref name="extraCards"/> more).</summary>
    private void DrawUntilColor(Player player, CardColor color, int extraCards)
    {
        int drawn = 0;
        while (drawn < MaxColorDraws)
        {
            Card card = Deck.Draw();
            player.Hand.Add(card);
            drawn++;

            if (card.Color == color)
                break;
        }

        ui.Show($"{player.Name} draws {drawn} card(s) until getting a {color} card", MessageStyle.Event);

        if (extraCards > 0)
            DrawCards(player, extraCards);
    }

    //Wild Draw cards can be played without a card of the color in play (see ResolveWildDraw), so the NPC never bluffs
    private bool IsIllegalWildDraw(Player player, Card card) =>
        card.Face is WildDrawTwoCard or WildDrawColorCard
        && player.Hand.Any(other => other != card && other.Color == CurrentColor);

    //In Uno Flip the winner of the round scores the cards left in the other hands, valued by the side that is up
    private void ShowRoundScore(Player winner)
    {
        int points = players.Where(player => player != winner).Sum(player => player.Hand.Sum(card => card.Points));
        ui.Show($"{winner.Name} scores {points} points ({CurrentSide} side)", MessageStyle.Success);
    }
}

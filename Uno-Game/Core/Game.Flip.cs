using Uno_Game.Cards;
using Uno_Game.Cards.FlexVersion;
using Uno_Game.Cards.FlipVersion;
using Uno_Game.Players;
using Uno_Game.UI;

namespace Uno_Game.Core;

/// <summary>Uno Flip: the double-sided cards and the rules that change in this version of the game.</summary>
public partial class Game
{
    private const int MaxColorDraws = 200;   //Safety limit for "draw until you get a color"

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

    /// <summary>Wild Draw Color (dark side): the next player draws until getting the chosen color and misses their turn.</summary>
    public void WildDrawColor()
    {
        CardColor color = CurrentColor;
        ResolveWildDraw("Wild Draw Color", player => DrawUntilColor(player, color, extraCards: 0));
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

    //In Uno Flip the winner of the round scores the cards left in the other hands, valued by the side that is up
    private void ShowRoundScore(Player winner)
    {
        int points = players.Where(player => player != winner).Sum(player => player.Hand.Sum(card => card.Points));
        ui.Show($"{winner.Name} scores {points} points ({CurrentSide} side)", MessageStyle.Success);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Cards;
using Uno_Game.Cards.PopVersion;
using Uno_Game.Players;
using Uno_Game.UI;

namespace Uno_Game.Core;

public partial class Game
{
    public int PopStackCount = 0;

    private PopDeck? popDeck;
    public PopDeck PopDeck => popDeck
        ?? throw new InvalidOperationException("The Pop deck is only available in Uno Pop.");

    private void InitializePopDeck()
    {
        popDeck = new PopDeck(rng);
    }

    public void PopAddToStack(int drawCount) => PopStackCount += drawCount;

    public void ResetPopStack() => PopStackCount = 0;

    public void PopPunish(Player victim)
    {
        ui.Show($"{victim.Name} draws {PopStackCount} card(s) from the Pop stack!", MessageStyle.Warning);
        DrawCards(victim, PopStackCount);
        ResetPopStack();
    }

    public void PopUno(Player player)
    {
        //The card to keep
        List<Card> candidates = player.Hand.ToList();
        if (candidates.Count == 1)
            ui.Show($"{player.Name} already has one card", MessageStyle.Event);

        Card keep = player.ChoosePopUnoKeep(this, candidates);
        
        if (!candidates.Contains(keep))
            throw new InvalidOperationException($"{player.Name} chose an invalid card to keep: {keep}");

        ui.Show($"{player.Name} draws the Pop Uno card and discards down to one card!", MessageStyle.Event);

        foreach (Card card in player.Hand.Where(card => card != keep).ToList())
        {
            player.Hand.Remove(card);
            Deck.Discard(card);
        }

        UnoAnnouncedPlayers.Add(player);

        if (player == CurrentPlayer)
        {
            AnnounceUno(player);
        }
        else
        {
            unoPlayer = player;
            unoCalled = true;
            ui.Show($"{player.Name} says: UNO!", MessageStyle.Event);
        }

        ResetPopStack();
    }

    public void PopCardRevealed(Player player, PopCard card)
    {
        PopAddToStack(card.DrawCount);
        ui.Show($"{player.Name} flips {card}. Pop stack: {PopStackCount}", MessageStyle.Event);
    }

    public void ShowPopFlip(Player player, int cardsToFlip)
    {
        ui.Show($"{player.Name} must flip {cardsToFlip} Pop card(s).", MessageStyle.Warning);
    }

    public void DiscardAllCardsOfColor(CardColor color)
    {
        Player player = CurrentPlayer;
        List<Card> matchingCards = player.Hand.Where(card => card.Color == color).ToList();

        foreach (Card card in matchingCards)
        {
            player.Hand.Remove(card);
            Deck.Discard(card);
        }

        ui.Show($"{player.Name} discards {matchingCards.Count} {color} card(s).", MessageStyle.Event);

        if (HasWon(player))
            return;

        if (player.Hand.Count == 1)
            AnnounceUno(player);
    }

    public void AllFlip()
    {
        Player flippingPlayer = NextPlayer();

        while (true)
        {
            if (PopDeck.PopDraw(this, flippingPlayer))
                break;

            flippingPlayer = NextPlayer(flippingPlayer);
        }
    }
}

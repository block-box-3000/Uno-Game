using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Cards.TeamsVersion;
using Uno_Game.Core;
using Uno_Game.Players;

namespace Uno_Game.Cards.PopVersion;

public class PopDeck
{
    private readonly List<PopCard> drawPopPile;
    private readonly List<PopCard> discardPopPile = new();
    private readonly Random rng;

    public int PopDrawPileCount => drawPopPile.Count;
    public int PopDiscardPileCount => discardPopPile.Count;

    public PopDeck(Random? rng = null)
    {
        this.rng = rng ?? Random.Shared;

        drawPopPile = CreatePopCards().ToList();
        PopShuffle(drawPopPile);
    }

    public static IEnumerable<Card> CreateCards()
    {
        foreach (CardColor color in CardColors.Playable)
        {
            //Number cards: two of each 1-8
            for (int number = 1; number <= 8; number++)
            {
                yield return new NumberCard(color, number);
                yield return new NumberCard(color, number);
            }

            //Action cards: two of each per color
            for (int i = 0; i < 2; i++)
            {
                yield return new ReverseCard(color);
                yield return new PopFlipCard(color, 2);
            }

            yield return new SkipCard(color);
            yield return new DiscardAllCard(color);
        }

        for (int i = 0; i < 4; i++)
        {
            yield return new WildAllFlipCard();
            yield return new WildPopFlipCard(4);
            yield return new WildDiscardAllCard();
        }
    }

    public static IEnumerable<PopCard> CreatePopCards()
    {
        yield return new PopCard(0);
        
        for (int i = 0; i < 5; i++)
        {
            yield return new PopCard(1);
        }
        
        for (int i = 0; i < 2; i++)
        {
            yield return new PopCard(2);
        }

        yield return new PopCard(3);
        yield return new PopCard(1, isPopPunish: true);
        yield return new PopCard(2, isPopPunish: true);
        yield return new PopCard(0, isPopUno: true);
    }

    public bool PopDraw(Game game, Player player)
    {
        if (drawPopPile.Count == 0)
            PopRefill();

        PopCard card = drawPopPile[^1];
        drawPopPile.RemoveAt(drawPopPile.Count - 1);
        discardPopPile.Add(card);
        game.PopCardRevealed(player, card);
        card.Apply(game, player);

        if (card.IsPopPunish || card.IsPopUno)
        {
            RecyclePopCards();
            return true;
        }

        return false;
    }

    public void PopDiscard(PopCard card) => discardPopPile.Add(card);

    private void PopRefill()
    {
        drawPopPile.AddRange(discardPopPile);
        discardPopPile.Clear();

        //Rare case: every card is in the players' hands
        if (drawPopPile.Count == 0)
            drawPopPile.AddRange(CreatePopCards());

        PopShuffle(drawPopPile);
    }

    private void RecyclePopCards()
    {
        drawPopPile.AddRange(discardPopPile);
        discardPopPile.Clear();
        PopShuffle(drawPopPile);
    }

    private void PopShuffle(List<PopCard> cards)
    {
        //Fisher-Yates
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }
}

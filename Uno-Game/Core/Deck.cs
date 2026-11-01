using Uno_Game.Cards;
using Uno_Game.Cards.BilliePack;
using Uno_Game.Cards.ComebackPack;
using Uno_Game.Cards.DefensePack;
using Uno_Game.Cards.FlexVersion;
using Uno_Game.Cards.FlipVersion;
using Uno_Game.Cards.FrenemiesPack;
using Uno_Game.Cards.ReversePack;
using Uno_Game.Cards.SpeedPack;
using Uno_Game.Cards.StackPack;
using Uno_Game.Cards.SwapPack;

namespace Uno_Game.Core;

/// <summary>
/// Draw pile + discard pile. When the draw pile runs out, the discard pile is
/// shuffled back in, so the game never creates cards out of thin air.
/// </summary>
public class Deck
{
    private readonly List<Card> drawPile;
    private readonly List<Card> discardPile = new();
    private readonly ExpansionPacks packs;
    private readonly Random rng;
    private readonly GameVersions version;

    /// <summary>Uno Flip: which side of the cards is up (null in the normal version).</summary>
    public SideIndicator? Side { get; }

    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;

    public Deck(ExpansionPacks packs, Random? rng = null, GameVersions version = GameVersions.Normal)
    {
        this.packs = packs;
        this.rng = rng ?? Random.Shared;
        this.version = version;

        if (version == GameVersions.Flip)
            Side = new SideIndicator();

        drawPile = NewCards().ToList();
        Shuffle(drawPile);
    }

    //A full set of cards for this version of the game
    private IEnumerable<Card> NewCards() =>
        Side != null ? FlipDeck.CreateCards(Side, rng)
        : version == GameVersions.Flex ? FlexDeck.CreateCards()
        : CreateCards(packs);

    public Card Draw()
    {
        if (drawPile.Count == 0)
            Refill();

        Card card = drawPile[^1];
        drawPile.RemoveAt(drawPile.Count - 1);
        return card;
    }

    public void Discard(Card card) => discardPile.Add(card);

    /// <summary>
    /// Uno Flip: the whole discard pile is turned over. The card on top (the Flip card that was just played)
    /// goes to the bottom, and the card that was at the bottom becomes the new top card, which is returned.
    /// </summary>
    public Card FlipDiscardPile(Card top)
    {
        discardPile.Add(top);
        discardPile.Reverse();

        Card newTop = discardPile[^1];
        discardPile.RemoveAt(discardPile.Count - 1);
        return newTop;
    }

    /// <summary>The first card of the game is always a number card.</summary>
    public Card TakeStartingCard()
    {
        int index = drawPile.FindIndex(card => card.Face is NumberCard);
        if (index < 0)
            throw new InvalidOperationException("The deck has no number cards.");

        Card card = drawPile[index];
        drawPile.RemoveAt(index);
        return card;
    }

    /// <summary>
    /// Wild Stack Number: flips cards from the draw pile until a number card shows up.
    /// The flipped cards go to the discard pile and the number of the last one is returned.
    /// </summary>
    public (int Number, List<Card> Flipped) RevealUntilNumber()
    {
        //Kept apart until the end: if they were discarded right away, Refill could bring them back
        var flipped = new List<Card>();

        while (true)
        {
            Card card = Draw();
            flipped.Add(card);

            if (card.Face is NumberCard numberCard)
            {
                foreach (Card flippedCard in flipped)
                    Discard(flippedCard);

                return (numberCard.Number, flipped);
            }
        }
    }

    private void Refill()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();

        //Rare case: every card is in the players' hands
        if (drawPile.Count == 0)
            drawPile.AddRange(NewCards());

        Shuffle(drawPile);
    }

    private void Shuffle(List<Card> cards)
    {
        //Fisher-Yates
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }

    public static IEnumerable<Card> CreateCards(ExpansionPacks packs)
    {
        foreach (CardColor color in CardColors.Playable)
        {
            //Number cards: one 0 and two of each 1-9
            yield return new NumberCard(color, 0);
            for (int number = 1; number <= 9; number++)
            {
                yield return new NumberCard(color, number);
                yield return new NumberCard(color, number);
            }

            //Action cards: two of each per color
            for (int i = 0; i < 2; i++)
            {
                yield return new SkipCard(color);
                yield return new ReverseCard(color);
                yield return new DrawCard(color);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            yield return new WildCard();
            yield return new DrawWildCard();
        }

        if (packs.HasFlag(ExpansionPacks.Reverse))
        {
            foreach (CardColor color in CardColors.Playable)
            {
                yield return new ReverseSkipCard(color);
                yield return new ReverseDrawCard(color);
            }

            for (int i = 0; i < 4; i++)
            {
                yield return new WildPowerReverseCard();
                yield return new WildNoUCard();
            }
        }

        if (packs.HasFlag(ExpansionPacks.Stack))
        {
            foreach (CardColor color in CardColors.Playable)
            {
                yield return new StackCard(color, drawCount: 1);
                yield return new StackCard(color, drawCount: 2);
            }

            for (int i = 0; i < 4; i++)
            {
                yield return new WildStackCard();
                yield return new WildStackNumberCard();
            }
        }

        if (packs.HasFlag(ExpansionPacks.Swap))
        {
            foreach (CardColor color in CardColors.Playable)
            {
                yield return new SwapOneCard(color);
                yield return new RefreshHandCard(color);
            }

            for (int i = 0; i < 4; i++)
            {
                yield return new WildForceTradeCard();
                yield return new WildPassHandsCard();
            }
        }

        if (packs.HasFlag(ExpansionPacks.Speed))
        {
            //Three consecutive numbers per color, as in the real pack
            foreach (var (color, first) in new[] { (CardColor.Yellow, 1), (CardColor.Red, 3), (CardColor.Blue, 5), (CardColor.Green, 7) })
            {
                for (int number = first; number < first + 3; number++)
                    yield return new NumberSpeedCard(color, number);
            }

            for (int i = 0; i < 4; i++)
                yield return new WildSpeedCard();
        }

        if (packs.HasFlag(ExpansionPacks.Billie))
        {
            //Reverse Hands is the only colored card of the pack: one per color
            foreach (CardColor color in CardColors.Playable)
                yield return new ReverseHandsCard(color);

            for (int i = 0; i < 4; i++)
            {
                yield return new WildPassColorCard();
                yield return new TrickWildCard();
                yield return new WildColorChallengeCard();
            }
        }

        if (packs.HasFlag(ExpansionPacks.Frenemies))
        {
            //All the cards of the pack are wild
            for (int i = 0; i < 4; i++)
            {
                yield return new WildPettyPlayCard();
                yield return new WildNemesisCard();
                yield return new WildEliminatorCard();
                yield return new WildInstigatorCard();
            }
        }

        if (packs.HasFlag(ExpansionPacks.Defense))
        {
            //All the cards of the pack are wild
            for (int i = 0; i < 4; i++)
            {
                yield return new WildBlockCard();
                yield return new WildBlockReboundCard();
                yield return new WildBlockDeflectCard();
                yield return new WildRedirectCard();
            }
        }

        if (packs.HasFlag(ExpansionPacks.Comeback))
        {
            //All the cards of the pack are wild
            for (int i = 0; i < 4; i++)
            {
                yield return new WildSwapHandsCard();
                yield return new WildDoubleDownCard();
                yield return new WildMatchTheMaxCard();
                yield return new WildMiseryLovesCompanyCard();
            }
        }
    }
}

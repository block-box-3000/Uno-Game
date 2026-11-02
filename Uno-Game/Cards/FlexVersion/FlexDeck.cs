using Uno_Game.Cards.FlipVersion;
using Uno_Game.Cards.ReversePack;
using Uno_Game.Core;

namespace Uno_Game.Cards.FlexVersion;

/// <summary>The 104 cards of Uno Flex (the 8 Power Cards are not part of the deck: every player has one).</summary>
public static class FlexDeck
{
    public static List<Card> CreateCards()
    {
        var cards = new List<Card>();

        foreach (CardColor color in CardColors.Playable)
        {
            List<CardColor> others = CardColors.Playable.Where(other => other != color).ToList();

            //Numbers 1-8: one classic card and one flex card per number (the flex side shows another color)
            for (int number = 1; number <= 8; number++)
            {
                bool flip = HasFlipSymbol(color, number);
                cards.Add(NumberFace(color, number, flip));
                
                CardColor auxColor = new();

                switch (color)
                {
                    case CardColor.Red:
                        auxColor = number switch
                        {
                            >= 4 and <= 6 => CardColor.Green,
                            >= 7 and <= 8 => CardColor.Yellow,
                            _ => CardColor.Blue
                        };
                        break;
                    case CardColor.Yellow:
                        auxColor = number switch
                        {
                            >= 4 and <= 6 => CardColor.Blue,
                            >= 7 and <= 8 => CardColor.Red,
                            _ => CardColor.Green
                        };
                        break;
                    case CardColor.Blue:
                        auxColor = number switch
                        {
                            >= 4 and <= 6 => CardColor.Yellow,
                            >= 7 and <= 8 => CardColor.Green,
                            _ => CardColor.Red
                        };
                        break;
                    case CardColor.Green:
                        auxColor = number switch
                        {
                            >= 4 and <= 6 => CardColor.Red,
                            >= 7 and <= 8 => CardColor.Blue,
                            _ => CardColor.Yellow
                        };
                        break;
                }

                cards.Add(new FlexCard(new NumberCard(color, number), 
                    new NumberCard(auxColor, number)));
            }

            //Action cards: one classic and one flex card of each kind per color (both sides have the same color)
            cards.Add(new DrawCard(color, 2));
            cards.Add(new FlexCard(new DrawCard(color, 2), new AllDrawCard(color, 1), "All draw 1"));
            cards.Add(new SkipCard(color));
            cards.Add(new FlexCard(new SkipCard(color), new SkipEveryoneCard(color, 20), "Skip everyone"));
            cards.Add(new ReverseCard(color));
            cards.Add(new FlexCard(new ReverseCard(color), new ReverseSkipCard(color), "Reverse and skip"));
        }

        //Wild cards
        for (int i = 0; i < 4; i++)
        {
            cards.Add(new FlexCard(new WildCard(), new AllDrawCard(CardColor.Wild, 2, 50), "All draw 2"));
            cards.Add(new FlexCard(new WildCard(), new TargetDrawCard(2), "Target draws 2"));
        }

        for (int i = 0; i < 2; i++)
            cards.Add(new FlexCard(new WildDrawCard(), new TargetDrawCard(4), "Target draws 4"));

        for (int i = 0; i < 6; i++)
            cards.Add(new WildAllFlipCard());

        return cards;
    }

    //The Flip symbol is on the odd Yellow and Blue numbers and on the even Red and Green numbers
    public static bool HasFlipSymbol(CardColor color, int number) =>
        color is CardColor.Yellow or CardColor.Blue ? number % 2 == 1 : number % 2 == 0;

    private static NumberCard NumberFace(CardColor color, int number, bool flip) =>
        flip ? new NumberFlipCard(color, number) : new NumberCard(color, number);
}

using Uno_Game.Core;

namespace Uno_Game.Cards.FlipVersion;

/// <summary>The 112 double-sided cards of Uno Flip.</summary>
public static class FlipDeck
{
    /// <summary>Creates the 112 cards. Each one gets a light face and a (randomly paired) dark face.</summary>
    public static List<Card> CreateCards(SideIndicator side, Random rng)
    {
        List<Card> light = CreateLightFaces();
        List<Card> dark = CreateDarkFaces();

        //Fisher-Yates
        for (int i = dark.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (dark[i], dark[j]) = (dark[j], dark[i]);
        }

        return light.Select((face, i) => (Card) new TwoSidedCard(side, face, dark[i])).ToList();
    }

    public static List<Card> CreateLightFaces()
    {
        var cards = new List<Card>();

        foreach (CardColor color in CardColors.Playable)
        {
            //Two of each number 1-9 per color
            for (int number = 1; number <= 9; number++)
            {
                cards.Add(new NumberCard(color, number));
                cards.Add(new NumberCard(color, number));
            }

            //Two of each action card per color
            for (int i = 0; i < 2; i++)
            {
                cards.Add(new DrawCard(color, drawCount: 1, points: 10));
                cards.Add(new ReverseCard(color));
                cards.Add(new SkipCard(color));
                cards.Add(new FlipCard(color));
            }
        }

        for (int i = 0; i < 4; i++)
        {
            cards.Add(new WildCard(points: 40));
            cards.Add(new WildDrawTwoCard());
        }

        return cards;
    }

    public static List<Card> CreateDarkFaces()
    {
        var cards = new List<Card>();

        foreach (CardColor color in CardColors.DarkPlayable)
        {
            for (int number = 1; number <= 9; number++)
            {
                cards.Add(new NumberCard(color, number));
                cards.Add(new NumberCard(color, number));
            }

            for (int i = 0; i < 2; i++)
            {
                cards.Add(new DrawCard(color, drawCount: 5, points: 20));
                cards.Add(new ReverseCard(color));
                cards.Add(new SkipEveryoneCard(color));
                cards.Add(new FlipCard(color));
            }
        }

        for (int i = 0; i < 4; i++)
        {
            cards.Add(new WildCard(points: 40));
            cards.Add(new WildDrawColorCard());
        }

        return cards;
    }
}

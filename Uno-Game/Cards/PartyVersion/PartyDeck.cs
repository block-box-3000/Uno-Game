using Uno_Game.Core;

namespace Uno_Game.Cards.PartyVersion
{
    public class PartyDeck
    {
        public static IEnumerable<Card> CreateCards()
        {
            foreach (CardColor color in CardColors.Playable)
            {
                //Number cards: one 0 and three of each 1-9
                yield return new NumberCard(color, 0);
                for (int number = 1; number <= 9; number++)
                {
                    yield return new NumberCard(color, number);
                    yield return new NumberCard(color, number);
                    yield return new NumberCard(color, number);
                }

                for (int i = 0; i < 3; i++)
                {
                    yield return new SkipCard(color);
                    yield return new ReverseCard(color);
                }

                for (int i = 0; i < 2; i++)
                    yield return new DrawCard(color);

                yield return new PointTakenCard(color);
            }

            for (int i = 0; i < 2; i++)
                yield return new WildCard();

            for (int i = 0; i < 4; i++)
                yield return new WildDrawCard();

            for (int i = 0; i < 5; i++)
                yield return new WildPileUpCard();

            for (int i = 0; i < 6; i++)
                yield return new WildDrawnTogetherCard();
        }
    }
}

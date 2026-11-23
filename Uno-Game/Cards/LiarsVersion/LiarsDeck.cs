using Uno_Game.Cards;
using Uno_Game.Core;

namespace Uno_Game.Cards.LiarsVersion
{
    public class LiarsDeck
    {
        public static IEnumerable<Card> CreateCards()
        {
            foreach (CardColor color in CardColors.Playable)
            {
                //Each standard card has one normal copy and one hidden Liar copy.
                for (int number = 0; number <= 9; number++)
                {
                    yield return new NumberCard(color, number);
                    yield return MarkAsLiar(new NumberCard(color, number));
                }

                //Action cards: one normal copy and one hidden Liar copy per color.
                yield return new SkipCard(color);
                yield return MarkAsLiar(new SkipCard(color));
                yield return new ReverseCard(color);
                yield return MarkAsLiar(new ReverseCard(color));
                yield return new DrawCard(color);
                yield return MarkAsLiar(new DrawCard(color));
            }

            for (int i = 0; i < 2; i++)
            {
                yield return MarkAsLiar(new WildDrawCard());
            }

            for (int i = 0; i < 6; i++)
                yield return new WildLiarsChallengeCard();
        }

        public static IEnumerable<Card> CreateClaims()
        {
            foreach (CardColor color in CardColors.Playable)
            {
                for (int number = 0; number <= 9; number++)
                    yield return new NumberCard(color, number);

                yield return new SkipCard(color);
                yield return new ReverseCard(color);
                yield return new DrawCard(color);
            }

            yield return new WildDrawCard();
        }

        public static T MarkAsLiar<T>(T card) where T : Card
        {
            card.IsLiarCard = true;
            return card;
        }
    }
}

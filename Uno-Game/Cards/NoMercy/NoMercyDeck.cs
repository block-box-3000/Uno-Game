using Uno_Game.Cards.FlipVersion;
using Uno_Game.Cards.PopVersion;
using Uno_Game.Core;

namespace Uno_Game.Cards.NoMercy
{
    public class NoMercyDeck
    {
        public static IEnumerable<Card> CreateCards(ExpansionPacks packs)
        {
            foreach (CardColor color in CardColors.Playable)
            {
                //Number cards: two of each 1-9
                yield return new NumberCard(color, 0);
                for (int number = 0; number <= 9; number++)
                {
                    yield return new NumberCard(color, number);
                    yield return new NumberCard(color, number);
                }

                for (int i = 0; i < 2; i++)
                {
                    yield return new NoMercyDrawCard(color, drawCount: 4);
                    yield return new SkipEveryoneCard(color);
                }

                for (int i = 0; i < 3; i++)
                {
                    yield return new SkipCard(color);
                    yield return new ReverseCard(color);
                    yield return new NoMercyDrawCard(color);
                    yield return new DiscardAllCard(color);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                yield return new WildReverseDrawCard();
                yield return new NoMercyWildDrawCard(drawCount: 6);
                yield return new NoMercyWildDrawCard(drawCount: 10);
            }

            //if (packs.HasFlag(ExpansionPacks.Stack))
            //{
            //    foreach (CardColor color in CardColors.Playable)
            //    {
            //        yield return new StackCard(color, drawCount: 1);
            //        yield return new StackCard(color, drawCount: 2);
            //    }

            //    for (int i = 0; i < 4; i++)
            //    {
            //        yield return new WildStackCard();
            //        yield return new WildStackNumberCard();
            //    }
            //}
        }
    }
}

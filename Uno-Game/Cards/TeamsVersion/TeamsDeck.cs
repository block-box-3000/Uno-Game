using Uno_Game.Core;

namespace Uno_Game.Cards.TeamsVersion;

/// <summary>The 112 cards of Uno Teams.</summary>
public static class TeamsDeck
{
    public static IEnumerable<Card> CreateCards()
    {
        foreach (CardColor color in CardColors.Playable)
        {
            //Number cards: two of each 1-9
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
            //yield return new WildTeamDrawCard();
        }

        for (int i = 0; i < 8; i++)
        {
            //yield return new WildTeamPlayCard();
        }
    }
}

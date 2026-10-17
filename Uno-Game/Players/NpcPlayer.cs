using Uno_Game.Cards;
using Uno_Game.Core;

namespace Uno_Game.Players;

public class NpcPlayer : Player
{
    private readonly Random rng;

    public NpcPlayer(int index, Random? rng = null) : base(index)
    {
        this.rng = rng ?? Random.Shared;
    }

    public override bool IsHuman => false;

    public override Card ChooseCard(Game game, IReadOnlyList<Card> playableCards)
    {
        //Revenge mode: prevents the next player from winning easily
        bool nextPlayerIsAboutToWin = game.NextPlayer().Hand.Count == 1;

        //Otherwise use the playable card with the lowest value
        Card? best = null;
        foreach (Card card in playableCards)
        {
            if (nextPlayerIsAboutToWin && card is SkipCard)
                return card;

            if (best == null || card.Points < best.Points)
                best = card;
        }

        return best!;
    }

    public override CardColor ChooseColor(Game game)
    {
        //Pick the color the NPC has the most in its hand
        Dictionary<CardColor, int> count = CardColors.Playable.ToDictionary(color => color, _ => 0);
        foreach (Card card in Hand)
        {
            if (!card.IsWild)
                count[card.Color]++;
        }

        CardColor best = CardColors.Playable[0];
        foreach (CardColor color in CardColors.Playable)
        {
            if (count[color] > count[best])
                best = color;
        }

        //In case the NPC doesn't have any color in its hand
        if (count[best] == 0)
            best = CardColors.Playable[rng.Next(CardColors.Playable.Length)];

        return best;
    }

    //The NPC always sends the punishment back
    public override bool ShouldPlayNoU(Game game, int drawCount) => true;
}

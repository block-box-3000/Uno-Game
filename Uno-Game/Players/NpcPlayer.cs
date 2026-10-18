using Uno_Game.Cards;
using Uno_Game.Cards.StackPack;
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

    //The NPC always answers, with the cheapest card (the list comes sorted)
    public override StackCard? ChooseStackAnswer(Game game, IReadOnlyList<StackCard> answers, int pendingPenalty) =>
        answers[0];

    //Swap 1: give the most expensive card to the player with the most cards (the least dangerous one)
    public override SwapChoice ChooseSwap(Game game)
    {
        Player target = game.Players
            .Where(player => player != this)
            .OrderByDescending(player => player.Hand.Count)
            .First();

        Card give = Hand.OrderByDescending(card => card.Points).First();
        return new SwapChoice(target, give);
    }

    //Force trade: take the smallest hand if it is smaller than mine, otherwise hit the leader
    public override (Player First, Player Second) ChooseTradePlayers(Game game)
    {
        List<Player> others = game.Players
            .Where(player => player != this)
            .OrderBy(player => player.Hand.Count)
            .ToList();

        //With two players there is only one possible pair
        if (others.Count == 1)
            return (this, others[0]);

        Player fewest = others[0];
        if (fewest.Hand.Count < Hand.Count)
            return (this, fewest);

        return (fewest, others[^1]);
    }
}

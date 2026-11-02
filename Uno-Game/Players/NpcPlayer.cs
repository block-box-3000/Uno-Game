using Uno_Game.Cards;
using Uno_Game.Cards.DefensePack;
using Uno_Game.Cards.FlexVersion;
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
            if (nextPlayerIsAboutToWin && card.Face is SkipCard)
                return card;

            if (best == null || card.Points < best.Points)
                best = card;
        }

        return best!;
    }

    public override CardColor ChooseColor(Game game)
    {
        //Pick the color the NPC has the most in its hand
        //The colors depend on the side in play (Uno Flip)
        IReadOnlyList<CardColor> colors = game.PlayableColors;
        Dictionary<CardColor, int> count = colors.ToDictionary(color => color, _ => 0);
        foreach (Card card in Hand)
        {
            if (!card.IsWild)
                count[card.Color]++;
        }

        CardColor best = colors[0];
        foreach (CardColor color in colors)
        {
            if (count[color] > count[best])
                best = color;
        }

        //In case the NPC doesn't have any color in its hand
        if (count[best] == 0)
            best = colors[rng.Next(colors.Count)];

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

    //Frenemies pack: the NPC goes after whoever is closest to winning
    public override Player ChooseEnemy(Game game, IReadOnlyList<Player> candidates) =>
        candidates.OrderBy(player => player.Hand.Count).First();

    //Each round the NPC ends the beef with 40% chance: rnd.Next(1, 11) is 1-10 and 7-10 ends it
    public override bool ShouldEndNemesis(Game game, Player enemy) => rng.Next(1, 11) > 6;

    public override Player ChoosePettyPlayTarget(Game game) => ClosestToWinning(game);

    //4 cards for the leader, then 3, 2 and 1 for the rest
    public override IReadOnlyList<(Player Target, int Cards)> ChooseInstigatorTargets(Game game) =>
        game.Players
            .Where(player => player != this)
            .OrderBy(player => player.Hand.Count)
            .Select((player, i) => (player, Math.Max(1, 4 - i)))
            .ToList();

    //The NPC always gets even
    public override bool ShouldPlayPettyPlay(Game game, Player source, int drawCount) => true;

    //Defense pack: the NPC always defends, with the card that hurts the most (rebound, redirect, deflect, block)
    public override DefenseCard? ChooseDefense(Game game, IReadOnlyList<DefenseCard> options, Player attacker, int drawCount, int turnsSkipped) =>
        options
            .OrderBy(card => card switch
            {
                WildBlockReboundCard => 0,
                WildRedirectCard => 1,
                WildBlockDeflectCard => 2,
                _ => 3
            })
            .First();

    //A redirected penalty goes to whoever is closest to winning
    public override Player ChooseRedirectTarget(Game game, IReadOnlyList<Player> candidates) =>
        candidates.OrderBy(player => player.Hand.Count).First();

    //Comeback pack: swap with the smallest hand and make the leader catch up
    public override Player ChooseSwapHandsTarget(Game game) => ClosestToWinning(game);

    public override Player ChooseMatchTarget(Game game) => ClosestToWinning(game);

    //U-N-O rule: always use it, keeping a wild card so the win is guaranteed (a wild card can always be played)
    public override Card? ChooseUnoKeep(Game game, IReadOnlyList<Card> candidates) =>
        candidates.FirstOrDefault(card => card.IsWild) ?? candidates.OrderBy(card => card.Points).First();

    //Uno Flip: the NPC doesn't know if a Wild Draw was played legally, so it challenges 3 times out of 10
    public override bool ShouldChallenge(Game game, Player attacker, string cardName) => rng.Next(1, 11) <= 10;

    //Uno Flex: the NPC always plays the flex side when it can, and targets whoever is closest to winning
    public override bool ShouldUseFlexSide(Game game, FlexCard card) => true;

    public override Player ChooseFlexTarget(Game game, int cards) => ClosestToWinning(game);

    private Player ClosestToWinning(Game game) =>
        game.Players.Where(player => player != this).OrderBy(player => player.Hand.Count).First();

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

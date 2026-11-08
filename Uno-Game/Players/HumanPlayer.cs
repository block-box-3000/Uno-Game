using Uno_Game.Cards;
using Uno_Game.Cards.DefensePack;
using Uno_Game.Cards.FlexVersion;
using Uno_Game.Cards.StackPack;
using Uno_Game.Core;
using Uno_Game.UI;

namespace Uno_Game.Players;

public class HumanPlayer : Player
{
    private readonly IGameUi ui;

    public HumanPlayer(int index, IGameUi ui) : base(index)
    {
        this.ui = ui;
    }

    public override bool IsHuman => true;

    public override Card ChooseCard(Game game, IReadOnlyList<Card> playableCards)
    {
        ui.ShowTable(game.TopCard, game.CurrentColor, Hand, card => playableCards.Contains(card));

        while (true)
        {
            int option = ui.AskNumber("", 1, Hand.Count);
            Card chosen = Hand[option - 1];

            if (playableCards.Contains(chosen))
                return chosen;

            ui.Show("Select a valid card", MessageStyle.Warning);
        }
    }

    public override Card? ChooseCardOrDraw(Game game, IReadOnlyList<Card> playableCards)
    {
        ui.ShowTable(game.TopCard, game.CurrentColor, Hand, card => playableCards.Contains(card));
        ui.Show($"{Hand.Count + 1}) Draw 1 card");

        while (true)
        {
            int option = ui.AskNumber("", 1, Hand.Count + 1);
            if (option == Hand.Count + 1)
                return null;

            Card chosen = Hand[option - 1];
            if (playableCards.Contains(chosen))
                return chosen;

            ui.Show("Select a valid card", MessageStyle.Warning);
        }
    }

    public override IReadOnlyList<Card> ChooseTeamCardsToGive(Game game, Player teammate, int count)
    {
        var choices = new List<Card>(count);
        var remaining = Hand.ToList();

        if (remaining.Count > 1)
        {
            for (int i = 0; i < count; i++)
            {
                string cards = string.Join("\n", remaining.Select((card, index) => $"{index + 1}) {card}"));
                int option = ui.AskNumber(
                    $"Choose a card to give to {teammate.Name} ({i} of {count})\n{cards}",
                    1, remaining.Count);
                choices.Add(remaining[option - 1]);
                remaining.RemoveAt(option - 1);
            }
        }
        else
        {
            choices.Add(remaining[0]);
            remaining.RemoveAt(0);
        }

        return choices;
    }

    public override bool ShouldPlayDrawnCard(Game game, Card card)
    {
        int option = ui.AskNumber(
            $"{Name}, you drew {card}. Play it?\n1) Play\n2) Keep it", 1, 2);
        return option == 1;
    }

    public override CardColor ChooseColor(Game game)
    {
        //The colors depend on the side in play (Uno Flip)
        IReadOnlyList<CardColor> colors = game.PlayableColors;
        string options = string.Join("\n", colors.Select((color, i) => $"{i + 1}) {color}"));

        int option = ui.AskNumber($"Select a color:\n{options}", 1, colors.Count);
        return colors[option - 1];
    }

    public override bool ShouldPlayNoU(Game game, int drawCount)
    {
        int option = ui.AskNumber(
            $"{Name}, you have to draw {drawCount} card(s) and you have a Wild No U card. Play it?\n1) Yes\n2) No", 1, 2);
        return option == 1;
    }

    public override StackCard? ChooseStackAnswer(Game game, IReadOnlyList<StackCard> answers, int pendingPenalty)
    {
        string options = string.Join("\n", answers.Select((card, i) => $"{i + 1}) Add {card}"));
        int option = ui.AskNumber(
            $"{Name}, the stack is at {pendingPenalty} card(s) (top card: {game.TopCard}). Answer?\n" +
            $"0) Don't answer: draw {pendingPenalty} card(s) and lose your turn\n{options}",
            0, answers.Count);

        return option == 0 ? null : answers[option - 1];
    }

    public override SwapChoice ChooseSwap(Game game)
    {
        List<Player> others = game.Players.Where(player => player != this).ToList();

        Player target = others[0];
        if (others.Count > 1)
        {
            int option = ui.AskNumber(
                $"Swap 1: choose the player you take a random card from\n{DescribePlayers(others)}", 1, others.Count);
            target = others[option - 1];
        }

        string cards = string.Join("\n", Hand.Select((card, i) => $"{i + 1}) {card}"));
        int give = ui.AskNumber($"Choose the card you give to {target.Name}\n{cards}", 1, Hand.Count);

        return new SwapChoice(target, Hand[give - 1]);
    }

    public override (Player First, Player Second) ChooseTradePlayers(Game game)
    {
        List<Player> all = game.Players.ToList();

        //With two players there is only one possible pair
        if (all.Count == 2)
            return (all[0], all[1]);

        int first = ui.AskNumber($"Force trade: choose the first player\n{DescribePlayers(all)}", 1, all.Count);

        List<Player> rest = all.Where(player => player != all[first - 1]).ToList();
        int second = ui.AskNumber($"Choose the second player\n{DescribePlayers(rest)}", 1, rest.Count);

        return (all[first - 1], rest[second - 1]);
    }

    public override Player ChooseEnemy(Game game, IReadOnlyList<Player> candidates) =>
        ChoosePlayer(candidates, "Wild Nemesis: choose your enemy");

    public override bool ShouldEndNemesis(Game game, Player enemy)
    {
        int option = ui.AskNumber(
            $"{Name}, {enemy.Name} is still your enemy (Wild Nemesis). End the beef?\n1) Yes, end it\n2) No, keep it", 1, 2);
        return option == 1;
    }

    public override Player ChoosePettyPlayTarget(Game game) =>
        ChoosePlayer(game.Players.Where(player => player != this).ToList(), "Wild Petty Play: choose the player who draws 3 cards");

    private Player ChoosePlayer(IReadOnlyList<Player> candidates, string question)
    {
        //With a single candidate there is nothing to choose
        if (candidates.Count == 1)
            return candidates[0];

        int option = ui.AskNumber($"{question}\n{DescribePlayers(candidates)}", 1, candidates.Count);
        return candidates[option - 1];
    }

    public override IReadOnlyList<(Player Target, int Cards)> ChooseInstigatorTargets(Game game)
    {
        var targets = new List<(Player Target, int Cards)>();
        List<Player> remaining = game.Players.Where(player => player != this).ToList();

        //Pick a player, then how many cards (1-4); that player leaves the list. 0 stops choosing.
        while (remaining.Count > 0)
        {
            int option = ui.AskNumber(
                $"Wild Instigator: choose a player to make draw cards\n{DescribePlayers(remaining)}\n0) Done", 0, remaining.Count);
            if (option == 0)
                break;

            Player target = remaining[option - 1];
            int cards = ui.AskNumber($"How many cards does {target.Name} draw? (1-4)", 1, 4);

            targets.Add((target, cards));
            remaining.Remove(target);
        }

        return targets;
    }

    public override bool ShouldPlayPettyPlay(Game game, Player source, int drawCount)
    {
        int option = ui.AskNumber(
            $"{Name}, {source.Name} makes you draw {drawCount} card(s) and you have a Wild Petty Play card. " +
            $"Play it so {source.Name} draws 3 cards?\n1) Yes\n2) No", 1, 2);
        return option == 1;
    }

    public override DefenseCard? ChooseDefense(Game game, IReadOnlyList<DefenseCard> options, Player attacker, int drawCount, int turnsSkipped)
    {
        string penalty = drawCount > 0 ? $"a draw {drawCount} penalty" : "a skip penalty";
        string choices = string.Join("\n", options.Select((card, i) => $"{i + 1}) {card}"));

        int option = ui.AskNumber(
            $"{Name}, {attacker.Name} plays {penalty} on you. Defend yourself?\n0) Take the penalty\n{choices}",
            0, options.Count);

        return option == 0 ? null : options[option - 1];
    }

    public override Player ChooseRedirectTarget(Game game, IReadOnlyList<Player> candidates) =>
        ChoosePlayer(candidates, "Wild Redirect: choose the player who gets the penalty");

    public override Player ChooseSwapHandsTarget(Game game) =>
        ChoosePlayer(game.Players.Where(player => player != this).ToList(), "Wild Swap Hands: choose the player you swap hands with");

    public override Player ChooseMatchTarget(Game game) =>
        ChoosePlayer(game.Players.Where(player => player != this).ToList(), "Wild Match the Max: choose the player who draws up to the largest hand");

    public override Card? ChooseUnoKeep(Game game, IReadOnlyList<Card> candidates)
    {
        string cards = string.Join("\n", candidates.Select((card, i) => $"{i + 1}) Keep {card}"));

        int option = ui.AskNumber(
            $"{Name}, you have U-N-O! You can discard down to one card for an instant UNO. Which card do you keep?\n" +
            $"0) Don't use the U-N-O rule\n{cards}",
            0, candidates.Count);

        return option == 0 ? null : candidates[option - 1];
    }

    public override bool ShouldChallenge(Game game, Player attacker, string cardName)
    {
        int option = ui.AskNumber(
            $"{Name}, {attacker.Name} plays {cardName} on you. Challenge it? (" +
            $"{attacker.Hand.Count} card(s) left)\n1) Yes, challenge\n2) No, take the penalty", 1, 2);
        return option == 1;
    }

    public override bool ShouldUseFlexSide(Game game, FlexCard card)
    {
        int option = ui.AskNumber(
            $"{Name}, you can play either side of this card (the flex side flips your Power Card to red):\n" +
            $"1) Regular side: {card.Regular}\n2) Flex side: {card.FlexDescription}", 1, 2);
        return option == 2;
    }

    public override Player ChooseFlexTarget(Game game, int cards) =>
        ChoosePlayer(game.Players.Where(player => player != this).ToList(), $"Choose the player who draws {cards} cards");

    private string DescribePlayers(IEnumerable<Player> players) =>
        string.Join("\n", players.Select((player, i) =>
            $"{i + 1}) {player.Name}{(player == this ? " (you)" : "")} ({player.Hand.Count} cards)"));
}

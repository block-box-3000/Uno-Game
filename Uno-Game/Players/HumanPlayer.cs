using Uno_Game.Cards;
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

    public override CardColor ChooseColor(Game game)
    {
        int option = ui.AskNumber("Select a color:\n1) Red\n2) Yellow\n3) Blue\n4) Green", 1, 4);
        return CardColors.Playable[option - 1];
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
}

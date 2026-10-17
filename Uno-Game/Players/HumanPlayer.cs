using Uno_Game.Cards;
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
}

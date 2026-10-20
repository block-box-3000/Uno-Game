using Uno_Game.Core;

namespace Uno_Game.UI;

public static class PackMenu
{
    //Packs the player can turn on. To unlock a new pack, add it to this list.
    private static readonly (string Name, ExpansionPacks Pack)[] Available =
    {
        ("Reverse pack", ExpansionPacks.Reverse),
        ("Stack pack", ExpansionPacks.Stack),
        ("Swap pack", ExpansionPacks.Swap),
        ("Speed pack", ExpansionPacks.Speed),
        ("Billie Eilish pack", ExpansionPacks.Billie),
    };

    public static ExpansionPacks Select(IGameUi ui)
    {
        ExpansionPacks packs = ExpansionPacks.None;
        int startOption = Available.Length + 1;
        int option;

        do
        {
            string lines = string.Join("\n", Available.Select((available, i) =>
                $"{i + 1}) {available.Name} [{(packs.HasFlag(available.Pack) ? "ON" : "OFF")}]"));

            option = ui.AskNumber($"Select an expansion pack\n{lines}\n{startOption}) Start Game", 1, startOption);

            if (option < startOption)
                packs ^= Available[option - 1].Pack;
        } while (option != startOption);

        return packs;
    }
}

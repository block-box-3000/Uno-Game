using Uno_Game.Core;

namespace Uno_Game.UI;

public static class PackMenu
{
    private const int StartOption = 3;

    public static ExpansionPacks Select(IGameUi ui)
    {
        ExpansionPacks packs = ExpansionPacks.None;
        int option;

        do
        {
            option = ui.AskNumber(
                "Select an expansion pack\n" +
                $"1) Reverse pack [{OnOff(packs, ExpansionPacks.Reverse)}]\n" +
                $"2) Stack pack [{OnOff(packs, ExpansionPacks.Stack)}]\n" +
                $"{StartOption}) Start Game", 1, StartOption);

            switch (option)
            {
                case 1:
                    packs ^= ExpansionPacks.Reverse;
                    break;
                case 2:
                    packs ^= ExpansionPacks.Stack;
                    break;
            }
        } while (option != StartOption);

        return packs;
    }

    private static string OnOff(ExpansionPacks packs, ExpansionPacks pack) => packs.HasFlag(pack) ? "ON" : "OFF";
}

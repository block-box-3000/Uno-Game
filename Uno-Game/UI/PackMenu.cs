using Uno_Game.Core;

namespace Uno_Game.UI;

public static class PackMenu
{
    //Packs the player can turn on. To unlock a new pack, add it to this list.
    private static readonly (string Name, ExpansionPacks Pack)[] NormalPacks =
    {
        ("Reverse pack", ExpansionPacks.Reverse),
        ("Stack pack", ExpansionPacks.Stack),
        ("Swap pack", ExpansionPacks.Swap),
        ("Speed pack", ExpansionPacks.Speed),
        ("Billie Eilish pack", ExpansionPacks.Billie),
        ("Frenemies pack", ExpansionPacks.Frenemies),
        ("Defense pack", ExpansionPacks.Defense),
        ("Comeback pack", ExpansionPacks.Comeback),
    };

    private static readonly (string Name, ExpansionPacks Pack)[] NoMercyPacks =
    {
        ("No Mercy expansion pack", ExpansionPacks.NoMercy),
    };

    public static ExpansionPacks Select(IGameUi ui, GameVersions version)
    {
        (string Name, ExpansionPacks Pack)[] available =
            version == GameVersions.NoMercy ? NoMercyPacks : NormalPacks;
        ExpansionPacks packs = ExpansionPacks.None;
        int startOption = available.Length + 1;
        int option;

        do
        {
            string lines = string.Join("\n", available.Select((pack, i) =>
                $"{i + 1}) {pack.Name} [{(packs.HasFlag(pack.Pack) ? "ON" : "OFF")}]"));

            option = ui.AskNumber($"Select an expansion pack\n{lines}\n{startOption}) Start Game", 1, startOption);

            if (option < startOption)
                packs ^= available[option - 1].Pack;
        } while (option != startOption);

        return packs;
    }
}

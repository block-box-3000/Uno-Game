using Uno_Game.Core;
using Uno_Game.UI;

var ui = new ConsoleUi();

GameVersions version = GameVersionMenu.Select(ui);

//Uno and Uno No Mercy are the only versions with expansion packs
ExpansionPacks packs = version == GameVersions.Normal || version == GameVersions.NoMercy 
    ? PackMenu.Select(ui, version) : ExpansionPacks.None;
Console.Clear();

Game game = Game.CreateStandard(packs, ui, version: version);
game.Run();

using Uno_Game.Core;
using Uno_Game.UI;

var ui = new ConsoleUi();

GameVersions version = GameVersionMenu.Select(ui);

//Uno Flip has its own cards and no expansion packs
ExpansionPacks packs = version == GameVersions.Normal ? PackMenu.Select(ui) : ExpansionPacks.None;
Console.Clear();

Game game = Game.CreateStandard(packs, ui, version: version);
game.Run();

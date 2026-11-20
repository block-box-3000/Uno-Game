using Uno_Game.Cards;
using Uno_Game.Core;
using Uno_Game.Cards.LiarsVersion;
using Uno_Game.UI;
using Uno_Game.Cards.NoMercy;

var ui = new ConsoleUi();

GameVersions version = GameVersionMenu.Select(ui);

//Uno and Uno No Mercy are the only versions with expansion packs
ExpansionPacks packs = version == GameVersions.Normal ? PackMenu.Select(ui) : ExpansionPacks.None;
Console.Clear();

Game game = Game.CreateStandard(packs, ui, version: version);
var hand = game.Players[2].Hand;
hand.Clear();
hand.Add(new WildReverseDrawCard(drawCount: 4));
hand.Add(new WildReverseDrawCard(drawCount: 4));
hand = game.Players[1].Hand;
hand.Clear();
hand.Add(new WildReverseDrawCard(drawCount: 4));
hand.Add(new WildReverseDrawCard(drawCount: 4));
game.ForceIndex(2);
game.Run();

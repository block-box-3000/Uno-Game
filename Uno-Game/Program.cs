using Uno_Game.Cards;
using Uno_Game.Cards.NoMercy;
using Uno_Game.Cards.NoMercyExpansion;
using Uno_Game.Core;
using Uno_Game.UI;

var ui = new ConsoleUi();

GameVersions version = GameVersionMenu.Select(ui);

//Uno and Uno No Mercy are the only versions with expansion packs
ExpansionPacks packs = version == GameVersions.Normal || version == GameVersions.NoMercy 
    ? PackMenu.Select(ui, version) : ExpansionPacks.None;
Console.Clear();

Game game = Game.CreateStandard(packs, ui, version: version, playerCount: 6);
//var hand = game.Players[3].Hand;
//hand.Clear();
//hand.Add(new NoMercyWildDrawCard(drawCount: 10));
//hand.Add(new ReverseCard(CardColor.Blue));
//game.ForceIndex(3);
game.Run();

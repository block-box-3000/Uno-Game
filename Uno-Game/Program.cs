using Uno_Game.Cards.ReversePack;
using Uno_Game.Cards.StackPack;
using Uno_Game.Cards;
using Uno_Game.Core;
using Uno_Game.UI;

var ui = new ConsoleUi();

ExpansionPacks packs = PackMenu.Select(ui);
Console.Clear();

Game game = Game.CreateStandard(packs, ui);
#if DEBUG
game.ForceTopCard(new StackCard(CardColor.Green, 1));

var hand = game.Players[0].Hand;
hand.Clear();
hand.Add(new DrawCard(CardColor.Blue));
hand.Add(new StackCard(CardColor.Blue, 2));
#endif
game.Run();
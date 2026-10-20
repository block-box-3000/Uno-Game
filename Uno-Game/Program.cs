using Uno_Game.Cards.ReversePack;
using Uno_Game.Cards.StackPack;
using Uno_Game.Cards;
using Uno_Game.Core;
using Uno_Game.UI;
using Uno_Game.Cards.SwapPack;
using Uno_Game.Cards.SpeedPack;
using Uno_Game.Cards.BilliePack;

var ui = new ConsoleUi();

ExpansionPacks packs = PackMenu.Select(ui);
Console.Clear();

Game game = Game.CreateStandard(packs, ui);
/*#if DEBUG
game.ForceTopCard(new NumberCard(CardColor.Blue, 1));

var hand = game.Players[0].Hand;
hand.Add(new TrickWildCard());
hand = game.Players[1].Hand;
hand.Clear();
hand.Add(new NumberCard(CardColor.Blue,1));
hand.Add(new NumberCard(CardColor.Blue,1));
hand.Add(new NumberCard(CardColor.Blue,1));

#endif*/
game.Run();
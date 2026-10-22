using Uno_Game.Cards.ReversePack;
using Uno_Game.Cards.StackPack;
using Uno_Game.Cards;
using Uno_Game.Core;
using Uno_Game.UI;
using Uno_Game.Cards.SwapPack;
using Uno_Game.Cards.SpeedPack;
using Uno_Game.Cards.BilliePack;
using Uno_Game.Cards.FrenemiesPack;
using Uno_Game.Cards.DefensePack;

var ui = new ConsoleUi();

ExpansionPacks packs = PackMenu.Select(ui);
Console.Clear();

Game game = Game.CreateStandard(packs, ui,10);
/*#if DEBUG
game.ForceTopCard(new NumberCard(CardColor.Blue, 1));

var hand = game.Players[0].Hand;
hand.Add(new SkipCard(CardColor.Blue));
hand = game.Players[1].Hand;
hand.Add(new WildBlockReboundCard());
hand = game.Players[3].Hand;
hand.Clear();
hand.Add(new WildRedirectCard());
hand.Add(new WildRedirectCard());
hand.Add(new WildRedirectCard());

#endif*/
game.Run();
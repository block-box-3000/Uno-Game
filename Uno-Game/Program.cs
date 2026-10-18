using Uno_Game.Cards.ReversePack;
using Uno_Game.Cards.StackPack;
using Uno_Game.Cards;
using Uno_Game.Core;
using Uno_Game.UI;

var ui = new ConsoleUi();

ExpansionPacks packs = PackMenu.Select(ui);
Console.Clear();

Game game = Game.CreateStandard(packs, ui, playerCount: 10);
/*#if DEBUG
game.ForceTopCard(new NumberCard(CardColor.Green, 1));

var hand = game.Players[0].Hand;
hand.Clear();
hand.Add(new StackCard(CardColor.Green, 2));
hand.Add(new StackCard(CardColor.Green, 2));
hand.Add(new WildNoUCard());
hand = game.Players[1].Hand;
hand.Clear();
hand.Add(new DrawCard(CardColor.Blue));
hand.Add(new StackCard(CardColor.Blue, 2));
hand = game.Players[2].Hand;
hand.Clear();
hand.Add(new DrawCard(CardColor.Blue));
hand.Add(new StackCard(CardColor.Blue, 2));
hand = game.Players[3].Hand;
hand.Clear();
hand.Add(new DrawCard(CardColor.Blue));
hand.Add(new StackCard(CardColor.Blue, 2));
#endif*/
game.Run();
using Uno_Game.Core;

namespace Uno_Game.Cards.SwapPack;

/// <summary>Choose any two players (you can be one of them) and make them swap hands, then choose the color.</summary>
public class WildForceTradeCard : Card
{
    public WildForceTradeCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild force trade";

    public override void Apply(Game game)
    {
        game.TradeHands();
        game.ChooseColor();
    }
}

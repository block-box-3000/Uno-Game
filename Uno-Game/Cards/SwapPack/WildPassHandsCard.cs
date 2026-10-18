using Uno_Game.Core;

namespace Uno_Game.Cards.SwapPack;

/// <summary>Everybody passes their hand to the next player in the current direction, then choose the color.</summary>
public class WildPassHandsCard : Card
{
    public WildPassHandsCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild pass hands";

    public override void Apply(Game game)
    {
        game.PassHands();
        game.ChooseColor();
    }
}

using Uno_Game.Core;

namespace Uno_Game.Cards.SpeedPack;

/// <summary>Wild "Double Speed Up" card: raises the speed level by two and you choose the color.</summary>
public class WildSpeedCard : Card
{
    public WildSpeedCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild double speed up";

    public override int SpeedUp => 2;

    public override void Apply(Game game) => game.ChooseColor();
}

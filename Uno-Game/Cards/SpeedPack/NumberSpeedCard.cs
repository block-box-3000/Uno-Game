using Uno_Game.Core;

namespace Uno_Game.Cards.SpeedPack;

/// <summary>A normal number card that also has the "Speed Up" icon: playing it raises the speed level by one.</summary>
public class NumberSpeedCard : NumberCard
{
    public NumberSpeedCard(CardColor color, int number) : base(color, number)
    {
    }

    protected override string Name => $"{Number} (speed up)";

    public override int SpeedUp => 1;
}

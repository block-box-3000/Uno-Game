namespace Uno_Game.Core;

public enum CardColor
{
    Red = 0,
    Yellow = 1,
    Blue = 2,
    Green = 3,
    Wild = 4
}

public static class CardColors
{
    /// <summary>The four colors a player can choose when playing a wild card.</summary>
    public static readonly CardColor[] Playable =
    {
        CardColor.Red, CardColor.Yellow, CardColor.Blue, CardColor.Green
    };
}

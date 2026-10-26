namespace Uno_Game.Core;

public enum CardColor
{
    Red = 0,
    Yellow = 1,
    Blue = 2,
    Green = 3,
    Wild = 4,

    //Dark side colors (Uno Flip)
    Pink = 5,
    Teal = 6,
    Orange = 7,
    Purple = 8
}

public static class CardColors
{
    /// <summary>The four colors a player can choose when playing a wild card (light side).</summary>
    public static readonly CardColor[] Playable =
    {
        CardColor.Red, CardColor.Yellow, CardColor.Blue, CardColor.Green
    };

    /// <summary>The four colors of the dark side (Uno Flip).</summary>
    public static readonly CardColor[] DarkPlayable =
    {
        CardColor.Pink, CardColor.Teal, CardColor.Orange, CardColor.Purple
    };
}

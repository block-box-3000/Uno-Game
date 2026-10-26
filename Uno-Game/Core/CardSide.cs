namespace Uno_Game.Core;

public enum CardSide
{
    Light,
    Dark
}

/// <summary>
/// Uno Flip: which side of the double-sided cards is up. All the cards of a game share one instance, so
/// flipping it flips every card at once (hands, draw pile and discard pile).
/// </summary>
public class SideIndicator
{
    public CardSide Current { get; private set; } = CardSide.Light;

    public void Flip() => Current = Current == CardSide.Light ? CardSide.Dark : CardSide.Light;
}

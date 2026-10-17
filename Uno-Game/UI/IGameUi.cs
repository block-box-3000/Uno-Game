using Uno_Game.Cards;
using Uno_Game.Core;

namespace Uno_Game.UI;

public enum MessageStyle
{
    Normal,
    Event,
    Npc,
    Warning,
    Success
}

/// <summary>
/// Everything the game needs from the outside world. The game logic never touches
/// Console directly, so it can be tested with a fake implementation.
/// </summary>
public interface IGameUi
{
    void Show(string text, MessageStyle style = MessageStyle.Normal);

    /// <summary>Shows the top card and the player's hand (playable cards highlighted).</summary>
    void ShowTable(Card topCard, CardColor currentColor, IReadOnlyList<Card> hand, Func<Card, bool> isPlayable);

    /// <summary>Asks for a number between min and max (both included).</summary>
    int AskNumber(string prompt, int min, int max);

    void Pause(int milliseconds);
}

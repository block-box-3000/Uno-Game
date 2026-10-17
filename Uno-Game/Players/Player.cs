using Uno_Game.Cards;
using Uno_Game.Core;

namespace Uno_Game.Players;

public abstract class Player
{
    public int Index { get; }
    public string Name => $"Player {Index + 1}";
    public List<Card> Hand { get; } = new();
    public abstract bool IsHuman { get; }

    protected Player(int index)
    {
        Index = index;
    }

    /// <summary>Picks one of the playable cards.</summary>
    public abstract Card ChooseCard(Game game, IReadOnlyList<Card> playableCards);

    /// <summary>Picks the color after playing a wild card.</summary>
    public abstract CardColor ChooseColor(Game game);
}

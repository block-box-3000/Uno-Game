using Uno_Game.Cards;
using Uno_Game.Cards.StackPack;
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

    /// <summary>
    /// Asked when this player is about to draw cards and holds a Wild No U card.
    /// Returns true to play it and send the punishment back.
    /// </summary>
    public abstract bool ShouldPlayNoU(Game game, int drawCount);

    /// <summary>
    /// Stack pack: asked when this player can answer a stack. Returns one of <paramref name="answers"/>
    /// (cheapest first) to add it to the stack, or null to refuse and draw the whole stack.
    /// </summary>
    public abstract StackCard? ChooseStackAnswer(Game game, IReadOnlyList<StackCard> answers, int pendingPenalty);

    /// <summary>Swap pack, Swap 1: who to take a random card from and which card to give them.</summary>
    public abstract SwapChoice ChooseSwap(Game game);

    /// <summary>Swap pack, Wild Force Trade: the two different players that swap hands (can include this player).</summary>
    public abstract (Player First, Player Second) ChooseTradePlayers(Game game);
}

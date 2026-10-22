using Uno_Game.Cards;
using Uno_Game.Cards.DefensePack;
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

    /// <summary>Frenemies pack, Wild Nemesis: the player (among the <paramref name="candidates"/>) this player names as their enemy.</summary>
    public abstract Player ChooseEnemy(Game game, IReadOnlyList<Player> candidates);

    /// <summary>
    /// Frenemies pack, Wild Nemesis: asked at the start of each of this player's turns for every enemy they have.
    /// Returns true to end the beef with <paramref name="enemy"/>.
    /// </summary>
    public abstract bool ShouldEndNemesis(Game game, Player enemy);

    /// <summary>Frenemies pack, Wild Petty Play played on your turn: the player who draws 3 cards.</summary>
    public abstract Player ChoosePettyPlayTarget(Game game);

    /// <summary>Frenemies pack, Wild Instigator: the opponents who draw, and how many cards (1-4) each one draws.</summary>
    public abstract IReadOnlyList<(Player Target, int Cards)> ChooseInstigatorTargets(Game game);

    /// <summary>
    /// Frenemies pack: asked when <paramref name="source"/> makes this player draw cards and they hold a
    /// Wild Petty Play. Returns true to play it out of turn so the source draws 3 cards.
    /// </summary>
    public abstract bool ShouldPlayPettyPlay(Game game, Player source, int drawCount);

    /// <summary>
    /// Defense pack: asked when a Draw or Skip penalty falls on this player and they hold defense cards
    /// that can answer it. Returns one of <paramref name="options"/>, or null to take the penalty.
    /// </summary>
    public abstract DefenseCard? ChooseDefense(Game game, IReadOnlyList<DefenseCard> options, Player attacker, int drawCount, int turnsSkipped);

    /// <summary>Defense pack, Wild Redirect: the player who receives the redirected penalty.</summary>
    public abstract Player ChooseRedirectTarget(Game game, IReadOnlyList<Player> candidates);

    /// <summary>Comeback pack, Wild Swap Hands: the other player this player swaps hands with.</summary>
    public abstract Player ChooseSwapHandsTarget(Game game);

    /// <summary>Comeback pack, Wild Match the Max: the player who draws up to the largest hand.</summary>
    public abstract Player ChooseMatchTarget(Game game);

    /// <summary>
    /// Comeback pack, U-N-O rule: asked at the start of the turn when this player holds a U, an N and an O card
    /// (and at least one more card). Returns the card to keep among <paramref name="candidates"/> to discard down to
    /// one card, or null to play normally.
    /// </summary>
    public abstract Card? ChooseUnoKeep(Game game, IReadOnlyList<Card> candidates);
}

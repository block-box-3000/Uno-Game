using Uno_Game.Cards.ReversePack;
using Uno_Game.Core;

namespace Uno_Game.Cards.StackPack;

/// <summary>
/// Adds cards to a stack. The next player can answer with another stack card;
/// whoever can't answer draws the whole stack.
/// </summary>
public class StackCard : Card
{
    public int DrawCount { get; }

    public StackCard(CardColor color, int drawCount, int points = 20) : base(color, points)
    {
        DrawCount = drawCount;
    }

    protected override string Name => $"stack {DrawCount}";

    /// <summary>How many cards this card adds to the stack. Wild Stack Number decides it when played.</summary>
    public virtual int GetPenalty(Game game) => DrawCount;

    public override void Apply(Game game)
    {
        if (IsWild)
            game.ChooseColor();

        game.ResolveStack(GetPenalty(game));
    }

    //A Stack card goes on another Stack card of any color. On a Draw card it needs the same color,
    //which the normal color rule already covers
    public override bool SharesSymbolWith(Card other) =>
        other is StackCard
        || other is ReverseDrawCard;
}

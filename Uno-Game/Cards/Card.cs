using Uno_Game.Core;

namespace Uno_Game.Cards;

public abstract class Card
{
    public CardColor Color { get; }
    public int Points { get; }
    public bool IsWild => Color == CardColor.Wild;

    protected Card(CardColor color, int points)
    {
        Color = color;
        Points = points;
    }

    /// <summary>Card name without its color, e.g. "skip" or "5".</summary>
    protected abstract string Name { get; }

    /// <summary>
    /// True when this card can be played on <paramref name="other"/> even with a different color
    /// (same number, or same kind of action card).
    /// </summary>
    public virtual bool SharesSymbolWith(Card other) => GetType() == other.GetType();

    /// <summary>The effect of the card. Called right after the card is played.</summary>
    public abstract void Apply(Game game);

    public override string ToString() => IsWild ? Name : $"{Color} {Name}";
}

using Uno_Game.Core;

namespace Uno_Game.Cards;

public abstract class Card
{
    //Virtual so a double-sided card (Uno Flip) can answer with the face that is up
    public virtual CardColor Color { get; }
    public virtual int Points { get; }
    public bool IsWild => Color == CardColor.Wild;

    protected Card(CardColor color, int points)
    {
        Color = color;
        Points = points;
    }

    /// <summary>The card itself, or for a double-sided card (Uno Flip) the face that is up.</summary>
    public virtual Card Face => this;

    /// <summary>Card name without its color, e.g. "skip" or "5".</summary>
    protected abstract string Name { get; }

    /// <summary>
    /// True when this card can be played on <paramref name="other"/> even with a different color
    /// (same number, or same kind of action card).
    /// </summary>
    public virtual bool SharesSymbolWith(Card other) => GetType() == other.GetType();

    /// <summary>Comeback pack: the letter (U, N or O) printed in the corner of the card, for the U-N-O rule.</summary>
    public virtual char? UnoLetter => null;

    /// <summary>Uno Flex: true for the number cards with the Flip symbol (whoever plays one flips their Power Card).</summary>
    public virtual bool FlipsPowerCard => false;

    /// <summary>Speed pack: how many speed levels this card adds when played (its "Speed Up" icons).</summary>
    public virtual int SpeedUp => 0;

    /// <summary>The effect of the card. Called right after the card is played.</summary>
    public abstract void Apply(Game game);

    public override string ToString() => IsWild ? Name : $"{Color} {Name}";
}

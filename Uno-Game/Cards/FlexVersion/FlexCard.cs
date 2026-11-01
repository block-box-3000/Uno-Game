using Uno_Game.Core;

namespace Uno_Game.Cards.FlexVersion;

/// <summary>
/// A Uno Flex card with two sides: the regular side and the flex side. The card is identified (color, number,
/// symbol, points) by its regular side, which is what stays on the table. The flex side can only be played while the
/// player's Power Card is on (see <see cref="Game.HasPower"/>): number cards use it to match another color,
/// action and wild cards use it for a stronger effect.
/// </summary>
public class FlexCard : Card
{
    public Card Regular { get; }
    public Card Flex { get; }

    /// <summary>Short text shown in the hand, e.g. "skip everyone" or "Yellow" (the color of a flex number card).</summary>
    public string FlexDescription { get; }

    /// <summary>Set by the game right before the card is played: true when the player plays the flex side.</summary>
    public bool PlayingFlexSide { get; set; }

    public FlexCard(Card regular, Card flex, string? flexDescription = null) : base(regular.Color, regular.Points)
    {
        Regular = regular;
        Flex = flex;
        FlexDescription = flexDescription ?? (regular is NumberCard ? flex.Color.ToString() : flex.ToString());
    }

    public override CardColor Color => Regular.Color;

    public override int Points => Regular.Points;

    public override Card Face => Regular;

    public override bool FlipsPowerCard => Regular.FlipsPowerCard;

    protected override string Name => Regular.ToString();

    public override string ToString() => $"{Regular} (flex: {FlexDescription})";

    public override bool SharesSymbolWith(Card other) => Regular.SharesSymbolWith(other.Face);

    public override void Apply(Game game) => (PlayingFlexSide ? Flex : Regular).Apply(game);
}

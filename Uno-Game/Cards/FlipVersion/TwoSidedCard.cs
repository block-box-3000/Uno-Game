using Uno_Game.Core;

namespace Uno_Game.Cards.FlipVersion;

/// <summary>
/// A physical Uno Flip card: it has a light face and a dark face, and every property (color, points, effect...)
/// is the one of the face that is up. Flipping the shared <see cref="SideIndicator"/> flips all the cards.
/// </summary>
public class TwoSidedCard : Card
{
    private readonly SideIndicator side;

    public Card Light { get; }
    public Card Dark { get; }

    private Card Active => side.Current == CardSide.Light ? Light : Dark;

    public TwoSidedCard(SideIndicator side, Card light, Card dark) : base(light.Color, light.Points)
    {
        this.side = side;
        Light = light;
        Dark = dark;
    }

    public override CardColor Color => Active.Color;

    public override int Points => Active.Points;

    public override Card Face => Active;

    protected override string Name => Active.ToString();

    public override string ToString() => Active.ToString();

    //Faces compare themselves with the face that is up on the other card
    public override bool SharesSymbolWith(Card other) => Active.SharesSymbolWith(other.Face);

    public override void Apply(Game game) => Active.Apply(game);
}

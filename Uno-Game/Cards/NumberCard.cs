using Uno_Game.Core;

namespace Uno_Game.Cards;

public class NumberCard : Card
{
    public int Number { get; }

    public NumberCard(CardColor color, int number) : base(color, points: number)
    {
        Number = number;
    }

    protected override string Name => Number.ToString();

    public override bool SharesSymbolWith(Card other) => other is NumberCard n && n.Number == Number;

    public override void Apply(Game game)
    {
        //Number cards have no effect
    }
}

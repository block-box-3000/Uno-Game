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
        //Only Number cards 0-7,10 have effects in special versions
        if (game.Version == GameVersions.Liars || game.Version == GameVersions.NoMercy)
        {
            if (Number == 0)
                game.PassHands();
            else if (Number == 7)
                game.SwapHands();
            else if (Number == 10)
                game.SkipEveryone();
        }
    }
}

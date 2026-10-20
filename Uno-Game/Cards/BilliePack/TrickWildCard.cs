using Uno_Game.Core;

namespace Uno_Game.Cards.BilliePack;

/// <summary>Choose a color; if the next player can't play a card of that color, you play again.</summary>
public class TrickWildCard : Card
{
    public TrickWildCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Trick wild";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.SetTrick();
    }
}

using Uno_Game.Core;

namespace Uno_Game.Cards.BilliePack;

/// <summary>Choose a color; every player passes their cards of that color to the player on their left.</summary>
public class WildPassColorCard : Card
{
    public WildPassColorCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild pass color";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.PassColor();
    }
}

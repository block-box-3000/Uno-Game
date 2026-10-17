using Uno_Game.Core;

namespace Uno_Game.Cards.ReversePack;

/// <summary>
/// Wild card that reverses the direction when played. If it is still in a player's hand when
/// they receive a punishment, it sends the punishment back (see <see cref="Game.PunishNextPlayer"/>).
/// </summary>
public class WildNoUCard : Card
{
    public WildNoUCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild No U";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.Reverse();
    }
}

using Uno_Game.Core;

namespace Uno_Game.Cards.FrenemiesPack;

/// <summary>
/// Played on your turn: choose a player who draws 3 cards (and choose the color).
/// It can also be played out of turn when another player makes you draw cards, so that player draws 3
/// (see <see cref="Game"/>.ForceDraw). Played that way it does not change whose turn it is.
/// </summary>
public class WildPettyPlayCard : Card
{
    public WildPettyPlayCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild petty play";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.PettyPlay();
    }
}

using Uno_Game.Core;
using Uno_Game.Players;

namespace Uno_Game.Cards.DefensePack;

/// <summary>
/// Base of the Defense pack cards. All of them are wild: on your turn they are a regular Wild card
/// (choose a color); out of turn they answer a Draw or Skip penalty played on you.
/// </summary>
public abstract class DefenseCard : Card
{
    protected DefenseCard() : base(CardColor.Wild, points: 50)
    {
    }

    /// <summary>
    /// Can this card answer the penalty? <paramref name="drawCount"/> is 0 for a Skip penalty
    /// and the number of cards to draw for a Draw penalty.
    /// </summary>
    public abstract bool CanAnswer(int drawCount);

    /// <summary>
    /// What happens to the penalty once the victim has played this card. The card is already on the table
    /// and its color is already chosen. The victim is no longer penalized.
    /// </summary>
    public abstract void Answer(Game game, Player victim, Player attacker, int drawCount, int turnsSkipped);

    //Played on your turn it is a regular Wild card
    public override void Apply(Game game) => game.ChooseColor();
}

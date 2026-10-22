using Uno_Game.Core;

namespace Uno_Game.Cards.ComebackPack;

/// <summary>
/// Choose a player: they draw until their hand has as many cards as the largest hand (and you choose the color).
/// Has the letter O for the U-N-O rule.
/// </summary>
public class WildMatchTheMaxCard : Card
{
    public WildMatchTheMaxCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild match the max";

    public override char? UnoLetter => 'O';

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.MatchTheMax();
    }
}

using Uno_Game.Core;

namespace Uno_Game.Cards.BilliePack;

/// <summary>
/// Choose a color; everybody else must show you a card of that color. Whoever can't draws 2 cards,
/// and if everybody shows one, you draw 3.
/// </summary>
public class WildColorChallengeCard : Card
{
    public WildColorChallengeCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild color challenge";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.ColorChallenge();
    }
}

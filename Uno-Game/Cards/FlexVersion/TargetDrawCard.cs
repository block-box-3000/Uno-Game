using Uno_Game.Core;

namespace Uno_Game.Cards.FlexVersion;

/// <summary>
/// Flex side of the Wild Flex Draw 2 and of the Wild Flex Draw 4: you choose the color and one player who draws
/// 2 or 4 cards. Play continues with the next player: nobody loses their turn.
/// </summary>
public class TargetDrawCard : Card
{
    public int DrawCount { get; }

    public TargetDrawCard(int drawCount) : base(CardColor.Wild, points: 50)
    {
        DrawCount = drawCount;
    }

    protected override string Name => $"Wild target draw {DrawCount}";

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.TargetDraw(DrawCount);
    }
}

using Uno_Game.Core;

namespace Uno_Game.Cards.FlexVersion;

/// <summary>
/// Flex side of the Flex Draw 2 (all the other players draw 1) and of the Wild Flex All Draw (all the other
/// players draw 2). The next player does not lose their turn. If the card is wild, you also choose the color.
/// </summary>
public class AllDrawCard : Card
{
    public int DrawCount { get; }

    public AllDrawCard(CardColor color, int drawCount, int points = 20) : base(color, points)
    {
        DrawCount = drawCount;
    }

    protected override string Name => $"{(IsWild ? "Wild " : "")}all draw {DrawCount}";

    public override void Apply(Game game)
    {
        if (IsWild)
            game.ChooseColor();

        game.AllDraw(DrawCount);
    }
}

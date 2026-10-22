using Uno_Game.Core;

namespace Uno_Game.Cards.ComebackPack;

/// <summary>Swap hands with any player (and choose the color). Has the letter U for the U-N-O rule.</summary>
public class WildSwapHandsCard : Card
{
    public WildSwapHandsCard() : base(CardColor.Wild, points: 50)
    {
    }

    protected override string Name => "Wild swap hands";

    public override char? UnoLetter => 'U';

    public override void Apply(Game game)
    {
        game.ChooseColor();
        game.SwapHands();
    }
}

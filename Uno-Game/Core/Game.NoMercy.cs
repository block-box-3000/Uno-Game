using System.Numerics;
using Uno_Game.Players;

namespace Uno_Game.Core;

public partial class Game
{
    public void ColorRoulette()
    {
        AdvanceTurn();
        Player victim = CurrentPlayer;
        ChooseColor();
        CardColor color = CurrentColor;
        DrawUntilColor(victim, color, extraCards: 0);
        SkipFrom(victim, 1);
    }
}

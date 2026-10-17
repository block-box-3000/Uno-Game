using Uno_Game.Core;

namespace Uno_Game.Cards.StackPack
{
    /// <summary>
    /// Like Wild Stack 3, but the penalty is the number on the first number card
    /// revealed from the draw pile when it is played.
    /// </summary>
    public class WildStackNumberCard : StackCard
    {
        public WildStackNumberCard(int points = 50) : base(CardColor.Wild, drawCount: 0, points)
        {
        }

        protected override string Name => "Wild stack number";

        public override int GetPenalty(Game game) => game.RevealPenaltyNumber();
    }
}

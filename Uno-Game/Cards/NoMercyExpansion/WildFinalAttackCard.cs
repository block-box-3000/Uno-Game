using Uno_Game.Core;

namespace Uno_Game.Cards.NoMercyExpansion
{
    public class WildFinalAttackCard : Card
    {
        public WildFinalAttackCard() : base (CardColor.Wild, 50)
        {

        }

        protected override string Name => "Wild Final Attack";

        public override void Apply(Game game)
        {
            game.FinalAttack();
            game.ChooseColor();
        }
    }
}

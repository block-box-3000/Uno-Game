using Uno_Game.Core;

namespace Uno_Game.Cards.PartyVersion
{
    public class WildPileUpCard : Card
    {
        public WildPileUpCard() : base(CardColor.Wild, 50)
        {

        }

        protected override string Name => "Wild Pile Up Card";

        public override void Apply(Game game)
        {
            game.PileUpAction();
        }
    }
}

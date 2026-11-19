using Uno_Game.Core;

namespace Uno_Game.Cards.LiarsVersion
{
    public class WildLiarsChallengeCard : Card
    {
        public WildLiarsChallengeCard() : base(CardColor.Wild, 50)
        {

        }

        protected override string Name => "Wild Liar's Challenge";

        public override void Apply(Game game)
        {
            game.ChooseColor();
            game.ResolveWildLiarsChallenge();
        }
    }
}

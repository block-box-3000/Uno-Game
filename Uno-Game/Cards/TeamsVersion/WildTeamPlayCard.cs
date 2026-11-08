using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;

namespace Uno_Game.Cards.TeamsVersion
{
    public class WildTeamPlayCard : Card
    {
        public WildTeamPlayCard() : base(CardColor.Wild, 50)
        {
        }

        protected override string Name => "Wild team play";

        public override void Apply(Game game)
        {
            game.WildTeamPlay();
        }
    }
}

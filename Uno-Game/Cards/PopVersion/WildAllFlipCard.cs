using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;
using Uno_Game.Players;

namespace Uno_Game.Cards.PopVersion
{
    public class WildAllFlipCard : Card
    {
        public WildAllFlipCard() : base(CardColor.Wild, 50)
        {

        }

        protected override string Name => "Wild All Flip Card";

        public override void Apply(Game game)
        {
            game.AllFlip();
            game.ChooseColor();
        }
    }
}

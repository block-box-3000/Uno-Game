using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;

namespace Uno_Game.Cards.NoMercy
{
    public class WildColorRouletteCard : Card
    {
        public WildColorRouletteCard() : base(CardColor.Wild, 50)
        {

        }

        protected override string Name => "Wild Color Roulette";

        public override void Apply(Game game)
        {
            game.ColorRoulette();
        }
    }
}
